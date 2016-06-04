//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file UpsampleFilter.fx
/// Combines an off-screen buffer with a scene render target using point,
/// bilinear, joint bilateral, or nearest-depth upsampling. The z-buffer can
/// optionally be rebuilt.
//
// Notes:
// - Upsampling/combine assumes linear space. No gamma correction implemented.
// - Joint bilateral upsampling works best for surfaces.
// - Nearest-depth upsampling works best for particle/volumetric effects.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 SourceSize;  // Low-resolution image size.
float2 TargetSize;  // Full-resolution image size.

// The low-resolution image.
texture SourceTexture;

// Note: In DirectX 11 we can define two samplers for SourceTexture. One for
// point sampling and one for linear sampling. But XNA does not support linear
// sampling of floating-point textures.
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// Optional: The original full-resolution scene.
texture SceneTexture;
sampler SceneSampler = sampler_state
{
  Texture = <SceneTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// Depth buffer (G-buffer 0).
DECLARE_UNIFORM_GBUFFER(DepthBuffer, 0);

// For z-buffer reconstruction.
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;

// For joint bilateral upsampling.
float DepthSensitivity = 100;

// For nearest-depth upsampling.
float DepthThreshold = 0.005f;  // Depth threshold for edge detection.

// The low-resolution depth buffer (required for bilateral and nearest-depth upsampling).
texture DepthBufferLow;
sampler DepthBufferLowSampler = sampler_state
{
  Texture = <DepthBufferLow>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};

struct VSOutputLinear
{
  float2 TexCoord : TEXCOORD;
  float2 Uv00 : TEXCOORD1;
  float2 Uv10 : TEXCOORD2;
  float2 Uv01 : TEXCOORD3;
  float2 Uv11 : TEXCOORD4;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
  return output;
}


VSOutputLinear VSLinear(VSInput input)
{
  VSOutputLinear output = (VSOutputLinear)0;
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
  
  // 2x2 texture coordinates for sampling low-resolution buffers.
  float2 lowResTexelSize = 1.0 / SourceSize;
  output.Uv00 = input.TexCoord - 0.5 * lowResTexelSize;
  output.Uv10 = output.Uv00 + float2(lowResTexelSize.x, 0);
  output.Uv01 = output.Uv00 + float2(0, lowResTexelSize.y);
  output.Uv11 = output.Uv00 + lowResTexelSize;
  
  return output;
}


// Sample texture with manual bilinear filtering.
float4 SampleLinear(sampler textureSampler, float2 textureSize,
                    float2 uv00, float2 uv10, float2 uv01, float2 uv11)
{
  float4 s00 = tex2Dlod(textureSampler, float4(uv00, 0, 0));
  float4 s10 = tex2Dlod(textureSampler, float4(uv10, 0, 0));
  float4 s01 = tex2Dlod(textureSampler, float4(uv01, 0, 0));
  float4 s11 = tex2Dlod(textureSampler, float4(uv11, 0, 0));
  
  float2 texelPos = textureSize * uv00;
  float2 lerps = frac(texelPos);
  return lerp(lerp(s00, s10, lerps.x), lerp(s01, s11, lerps.x), lerps.y);
}


// Reconstruct z-buffer depth from G-buffer depth.
float RebuildZ(float gBufferDepth, float4x4 projection, float cameraFar)
{
  // Full-resolution linear depth [0, CameraFar].
  float linearZ = gBufferDepth * cameraFar;
  
  // Reconstruct z-buffer depth (see RebuildZBuffer.fx for explanations).
  //depth = saturate((-linearZ * Projection._m22 + Projection._m32) / linearZ);
  // Effect compiler bug: Compiler removes saturate and pixel with a depth slightly above 1
  // fail the depth test (even if DepthBufferFunction is Always)!
  // --> We have to reformulate the equation.
  float depth = (-linearZ * Projection._m22 + Projection._m32) / linearZ;
  depth = max(0, min(1, depth));
  
  return depth;
}


void UpsampleAndRebuildZ(float2 texCoord, uniform bool combineWithScene, out float4 color, out float depth)
{
  // Upsample image.
  color = tex2Dlod(SourceSampler, float4(texCoord, 0, 0));
  if (combineWithScene)
  {
    float4 sceneColor = tex2Dlod(SceneSampler, float4(texCoord, 0, 0));
    color.rgb = color.rgb + sceneColor.rgb * color.a;
    color.a = 1;
  }
  
  // Rebuild z-buffer.
  depth = GetGBufferDepth(tex2Dlod(DepthBufferSampler, float4(texCoord, 0, 0)));
  depth = RebuildZ(depth, Projection, CameraFar);
}


void UpsampleLinearAndRebuildZ(float2 texCoord, float2 uv00, float2 uv10, float2 uv01, float2 uv11,
                               uniform bool combineWithScene,
                               out float4 color, out float depth)
{
  // Upsample image.
  color = SampleLinear(SourceSampler, SourceSize, uv00, uv10, uv01, uv11);
  if (combineWithScene)
  {
    float4 sceneColor = tex2Dlod(SceneSampler, float4(texCoord, 0, 0));
    color.rgb = color.rgb + sceneColor.rgb * color.a;
    color.a = 1;
  }
  
  // Rebuild z-buffer.
  depth = GetGBufferDepth(tex2Dlod(DepthBufferSampler, float4(texCoord, 0, 0)));
  depth = RebuildZ(depth, Projection, CameraFar);
}


void UpsampleBilateralAndRebuildZ(float2 texCoord, float2 uv00, float2 uv10, float2 uv01, float2 uv11,
                                  uniform bool combineWithScene, out float4 color, out float depth)
{
  // 2x2 low-resolution color.
  float4 s00 = tex2Dlod(SourceSampler, float4(uv00, 0, 0));
  float4 s10 = tex2Dlod(SourceSampler, float4(uv10, 0, 0));
  float4 s01 = tex2Dlod(SourceSampler, float4(uv01, 0, 0));
  float4 s11 = tex2Dlod(SourceSampler, float4(uv11, 0, 0));
  
  // ----- Joint bilateral upsampling
  // This implementation is based on
  //  Sloan et al. "Image-Based Proxy Accumulation for Real-Time Soft Global Illumination",
  //  http://www.iro.umontreal.ca/~derek/files/ProxyPG.pdf
  //
  // The weights for the 2x2 block are computed as:
  //
  //  w_normalized = w / sum(w)
  //
  // where
  //
  //  w = wb * wz * wn
  //
  //  wb = bilinear weights
  //  wz = 1 / (epsilon + |z - zFull|)        ... depth similarity
  //  wn = pow(dot(normal, normalFull), 32)   ... normal similarity
  //
  // This implementation considers only depth similarity, but ignores normals.
  // We can change the weights wz for improved control.
  //
  //  wz = 1 / (epsilon + |z - zFull|)
  //     = (1/epsilon) / (1 + 1/epsilon * |z - zFull|)
  //
  // The constant factor 1/epsilon can be ignored.
  // 
  //  wz = 1 / (1 + 1/epsilon * |z - zFull|)
  //     = 1 / (1 + DepthSensitivity * |z - zFull|)
  
  // Calculate bilinear weights.
  float2 texelPos = SourceSize * uv00;
  float2 f = frac(texelPos);
  float2 g = 1 - f;
  float w00 = g.x * g.y;
  float w10 = f.x * g.y;
  float w01 = g.x * f.y;
  float w11 = f.x * f.y;
  float4 weights = float4(w00, w10, w01, w11);
  
  // Full-resolution linear depth [0, 1].
  float zFull = GetGBufferDepth(tex2Dlod(DepthBufferSampler, float4(texCoord, 0, 0)));
  
  // 2x2 low-resolution linear, normalized depth [0, 1].
  float z00 = tex2Dlod(DepthBufferLowSampler, float4(uv00, 0, 0)).x;
  float z10 = tex2Dlod(DepthBufferLowSampler, float4(uv10, 0, 0)).x;
  float z01 = tex2Dlod(DepthBufferLowSampler, float4(uv01, 0, 0)).x;
  float z11 = tex2Dlod(DepthBufferLowSampler, float4(uv11, 0, 0)).x;
  float4 z = float4(z00, z10, z01, z11);
  weights *= 1 / (1 + DepthSensitivity * abs(z - zFull));
  
  float weightSum = dot(weights, 1);
  weights /= weightSum;
  
  color = s00 * weights.x
          + s10 * weights.y
          + s01 * weights.z
          + s11 * weights.w;
  
  if (combineWithScene)
  {
    float4 sceneColor = tex2Dlod(SceneSampler, float4(texCoord, 0, 0));
    color.rgb = color.rgb + sceneColor.rgb * color.a;
    color.a = 1;
  }
  
  // Rebuild z-buffer.
  depth = RebuildZ(zFull, Projection, CameraFar);
}


void UpsampleNearestDepthAndRebuildZ(float2 texCoord : TEXCOORD0, float2 uv00, float2 uv10, float2 uv01, float2 uv11,
                                     uniform bool combineWithScene, out float4 color, out float depth)
{
  // Nearest-depth upsampling:
  // - Jansen & Bavoil, Fast rendering of opacity-mapped particles using DirectX 11
  //   tessellation and mixed resolutions. NVIDIA Whitepaper.
  // - Opacity Mapping Sample, NVIDIA Graphics SDK 11 Direct3D,
  //   https://developer.nvidia.com/nvidia-graphics-sdk-11-direct3d
  
  // TODO: Check NVIDIA's Opacity Mapping Sample for DirectX 11 optimizations.
  // - No need to define ViewportSize and OffscreenBufferSize as parameters.
  //   Use GetDimensions(w, h) instead.
  // - Use GatherRed() in DirectX 11 to fetch 2x2 low-resolution depths.
  // - Use two samplers (one for point sampling and one for linear sampling)
  //   instead of manual bilinear filtering.
  
  // Full-resolution linear depth [0, 1].
  float zFull = GetGBufferDepth(tex2Dlod(DepthBufferSampler, float4(texCoord, 0, 0)));
  
  // 2x2 low-resolution linear depth.
  float z00 = tex2Dlod(DepthBufferLowSampler, float4(uv00, 0, 0)).x;
  float z10 = tex2Dlod(DepthBufferLowSampler, float4(uv10, 0, 0)).x;
  float z01 = tex2Dlod(DepthBufferLowSampler, float4(uv01, 0, 0)).x;
  float z11 = tex2Dlod(DepthBufferLowSampler, float4(uv11, 0, 0)).x;
  float4 z = float4(z00, z10, z01, z11);
  float4 zDelta = abs(z - zFull);
  float dist00 = zDelta.x;
  float dist10 = zDelta.y;
  float dist01 = zDelta.z;
  float dist11 = zDelta.w;
  
  [branch]  // Force branching.
  if (all(zDelta < DepthThreshold))
  {
    // ----- Non-edge detected.
    // Sample color at texCoord using bilinear filtering to avoid blocky artifacts.
    color = SampleLinear(SourceSampler, SourceSize, uv00, uv10, uv01, uv11);
  }
  else
  {
    // ----- Edge detected.
    // Sample color at low-resolution sample with nearest depth using point filtering
    // to avoid jaggies and halos.
    float minDist = dist00;
    float2 nearestUV = uv00;
    if (dist10 < minDist)
    {
      minDist = dist10;
      nearestUV = uv10;
    }
    
    if (dist01 < minDist)
    {
      minDist = dist01;
      nearestUV = uv01;
    }
    
    if (dist11 < minDist)
    {
      nearestUV = uv11;
    }
    
    color = tex2Dlod(SourceSampler, float4(nearestUV, 0, 0));
  }
  
  if (combineWithScene)
  {
    float4 sceneColor = tex2D(SceneSampler, texCoord);
    color.rgb = color.rgb + sceneColor.rgb * color.a;
    color.a = 1;
  }
  
  // Rebuild z-buffer.
  depth = RebuildZ(zFull, Projection, CameraFar);
}


void PSUpsample(float2 texCoord : TEXCOORD0, out float4 color : COLOR0)
{
  float depth;
  UpsampleAndRebuildZ(texCoord, false, color, depth);
}

void PSCombine(float2 texCoord : TEXCOORD0, out float4 color : COLOR0)
{
  float depth;
  UpsampleAndRebuildZ(texCoord, true, color, depth);
}

void PSUpsampleAndRebuildZ(float2 texCoord : TEXCOORD0, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleAndRebuildZ(texCoord, false, color, depth);
}

void PSCombineAndRebuildZ(float2 texCoord : TEXCOORD0, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleAndRebuildZ(texCoord, true, color, depth);
}

void PSUpsampleLinear(float2 texCoord : TEXCOORD0, float2 uv00 : TEXCOORD1, float2 uv10 : TEXCOORD2, float2 uv01 : TEXCOORD3, float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleLinearAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineLinear(float2 texCoord : TEXCOORD0, float2 uv00 : TEXCOORD1, float2 uv10 : TEXCOORD2, float2 uv01 : TEXCOORD3, float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleLinearAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}

void PSUpsampleLinearAndRebuildZ(float2 texCoord : TEXCOORD0, float2 uv00 : TEXCOORD1, float2 uv10 : TEXCOORD2, float2 uv01 : TEXCOORD3, float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleLinearAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineLinearAndRebuildZ(float2 texCoord : TEXCOORD0, float2 uv00 : TEXCOORD1, float2 uv10 : TEXCOORD2, float2 uv01 : TEXCOORD3, float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleLinearAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}

void PSUpsampleBilateral(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleBilateralAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineBilateral(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleBilateralAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}

void PSUpsampleBilateralAndRebuildZ(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleBilateralAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineBilateralAndRebuildZ(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleBilateralAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}

void PSUpsampleNearestDepth(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleNearestDepthAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineNearestDepth(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0)
{
  float depth;
  UpsampleNearestDepthAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}

void PSUpsampleNearestDepthAndRebuildZ(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleNearestDepthAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, false, color, depth);
}

void PSCombineNearestDepthAndRebuildZ(in float2 texCoord : TEXCOORD0, in float2 uv00 : TEXCOORD1, in float2 uv10 : TEXCOORD2, in float2 uv01 : TEXCOORD3, in float2 uv11 : TEXCOORD4, out float4 color : COLOR0, out float depth : DEPTH)
{
  UpsampleNearestDepthAndRebuildZ(texCoord, uv00, uv10, uv01, uv11, true, color, depth);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

// Technique 0: Point upsampling.
// Technique 1: Bilinear upsampling.
// Technique 2: Joint bilateral upsampling.
// Technique 3: Nearest-depth upsampling.
// Pass 0: Upsample
// Pass 1: Upsample and combine with scene.
// Pass 2: Upsample and rebuild z-buffer.
// Pass 3: Upsample, combine with scene and rebuild z-buffer.

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSUpsample();
  }
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombine();
  }
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSUpsampleAndRebuildZ();
  }
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombineAndRebuildZ();
  }
}

technique
{
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleLinear();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineLinear();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleLinearAndRebuildZ();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineLinearAndRebuildZ();
  }
}

technique
{
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleBilateral();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineBilateral();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleBilateralAndRebuildZ();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineBilateralAndRebuildZ();
  }
}

technique
{
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleNearestDepth();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineNearestDepth();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSUpsampleNearestDepthAndRebuildZ();
  }
  pass
  {
    VertexShader = compile VSTARGET VSLinear();
    PixelShader = compile PSTARGET PSCombineNearestDepthAndRebuildZ();
  }
}
