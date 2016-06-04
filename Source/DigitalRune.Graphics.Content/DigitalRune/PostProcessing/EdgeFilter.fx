//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file EdgeFilter.fx
/// Draws silhouette outlines and crease edges using edge detection.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

float HalfEdgeWidth = 1.0;
float DepthThreshold = 0.05;
float DepthSensitivity = 10000;
float NormalThreshold = 0.99;
float NormalSensitivity = 1;
float3 CameraBackward;       // The camera backward vector in world space.

// A silhouette edge is a depth discontinuity.
float4 SilhouetteColor = float4(0, 0, 0, 1);

// A crease edge is a normal discontinuity.
float4 CreaseColor = float4(1, 1, 1, 1);


// The input texture.
Texture2D SourceTexture;
sampler2D SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// The depth buffer.
Texture2D GBuffer0;
sampler2D GBuffer0Sampler = sampler_state
{
  Texture = <GBuffer0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// The normal buffer.
Texture2D GBuffer1;
sampler2D GBuffer1Sampler = sampler_state
{
  Texture = <GBuffer1>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
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


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


/// Detects 1-pixel edges using depth and normal information in G-buffer.
/// Silhouette edges are depth discontinuities. Crease edges are normal discontinuities.
/// The 1-pixel edge is drawn on the pixel a that is in front.
/// \param[in]  texCoord    The texture coordinate.
/// \param[out] silhouette  The intensity of the silhouette edge at the current pixel.
/// \param[out] crease      The intensity of the crease edge at the current pixel.
void DetectOnePixelEdge(float2 texCoord, out float silhouette, out float crease)
{
  // ----- Edge detection using horizontal and vertical samples
  //
  //   -|----|----|----|-
  //    |    | s0 |    |
  //   -|----|----|----|-
  //    | s1 | s  | s2 |
  //   -|----|----|----|-
  //    |    | s3 |    |
  //   -|----|----|----|-
  //
  float2 offset = 1 / ViewportSize;
  
  float z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  float4 zs;
  zs.x = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(0, -1) * offset, 0, 0)));
  zs.y = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, 0) * offset, 0, 0)));
  zs.z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, 0) * offset, 0, 0)));
  zs.w = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(0, 1) * offset, 0, 0)));
  
  float3 n = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0)));
  float3 n0 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(0, -1) * offset, 0, 0)));
  float3 n1 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, 0) * offset, 0, 0)));
  float3 n2 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, 0) * offset, 0, 0)));
  float3 n3 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(0, 1) * offset, 0, 0)));
  
  // ----- Depth Threshold
  // Artifacts appear when
  //
  //  n --> 90°  (The normal goes to 90° in view space.)
  //    AND
  //  z --> 1    (The sample depth goes to 1.)
  //
  // The first condition can be modeled as: (1 - n.z)
  // The second condition can be modeled as: depth^i (with i = 2 giving optimal results)
  // AND is modeled as multiplication.
  //
  // Increase the depth threshold to prevent artifacts:
  // Normal is in world space. To get n.z in view space:
  float nz = dot(CameraBackward, n);
  float depthThreshold = lerp(DepthThreshold, 1, (1 - nz) * z * z);
  
  // Optional: Increase normal threshold in the distance.
  float normalThreshold = NormalThreshold;
  //normalThreshold = lerp(NormalThreshold, 2, z * z);
  
  // ----- Silhouette (depth discontinuity)
  float4 dz = saturate(zs - z);
  dz = saturate((dz - depthThreshold) * DepthSensitivity);
  
  // Optional: Use d² instead of d to create a sharper transition.
  //dz = dz * dz;
  
  // Take dz.x OR dz.y OR dz.z OR dz.w.
  dz = 1 - dz;
  silhouette = 1 - dz.x * dz.y * dz.z * dz.w;
  
  // ----- Crease (normal discontinuity)
  float4 dn = float4(dot(n, n0), dot(n, n1), dot(n, n2), dot(n, n3));
  dn = 1 - dn;
  
  // Edge should only be drawn on pixel that is in front.
  float4 isInFront = (z < zs);
  dn *= isInFront;
  
  // Skybox pixels do not have valid normals. Ignore them.
  const float skyBoxLimit = 0.99999;
  float4 isNotSkyBox = (zs <= skyBoxLimit);
  dn *= isNotSkyBox;
  
  dn = saturate((dn - normalThreshold) * NormalSensitivity);
  
  // Take average of normal deltas.
  // crease = (dn.x + dn.y + dn.z + dn.w) / 4;
  crease = dot(dn, 0.25);
}


/// Detects edges using depth and normal information in G-buffer.
/// Silhouette edges are depth discontinuities. Crease edges are normal discontinuities.
/// \param[in]  texCoord    The texture coordinate.
/// \param[out] silhouette  The intensity of the silhouette edge at the current pixel.
/// \param[out] crease      The intensity of the crease edge at the current pixel.
void DetectEdge(float2 texCoord, out float silhouette, out float crease)
{
  // ----- Edge detection using diagonal samples
  //
  //   -|----|----|----|-
  //    | s0 |    | s1 |
  //   -|----|----|----|-
  //    |    | s  |    |
  //   -|----|----|----|-
  //    | s2 |    | s3 |
  //   -|----|----|----|-
  //
  // According to Nienhaus and Döllner: "Edge-Enhancement - An Algorithm for Real-Time Non-Photorealistic Rendering"
  // diagonal samples yield better results than sampling all 8 directions. This coincides with our own findings.
  
  // The sample offset is scaled by edge width.
  float2 offset = HalfEdgeWidth / ViewportSize;
  
  float z = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  float z0 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, -1) * offset, 0, 0)));
  float z1 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, -1) * offset, 0, 0)));
  float z2 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(-1, 1) * offset, 0, 0)));
  float z3 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord + float2(1, 1) * offset, 0, 0)));
  float3 n = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0)));
  float3 n0 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, -1) * offset, 0, 0)));
  float3 n1 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, -1) * offset, 0, 0)));
  float3 n2 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(-1, 1) * offset, 0, 0)));
  float3 n3 = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord + float2(1, 1) * offset, 0, 0)));
  
  // ----- Depth Threshold
  // Artifacts appear when
  //
  //  n --> 90°  (The normal goes to 90° in view space.)
  //    AND
  //  z --> 1    (The sample depth goes to 1.)
  //
  // The first condition can be modeled as: (1 - n.z)
  // The second condition can be modeled as: depth^i (with i = 2 giving optimal results)
  // AND is modeled as multiplication.
  //
  // Increase the depth threshold to prevent artifacts:
  // Normal is in world space. To get n.z in view space:
  float nz = dot(CameraBackward, n);
  float depthThreshold = lerp(DepthThreshold, 1, (1 - nz) * z * z);
  
  // Optional: Increase normal threshold in the distance.
  float normalThreshold = NormalThreshold;
  //normalThreshold = lerp(NormalThreshold, 2, z * z);
  
  // ----- Silhouette (depth discontinuity)
  float dz0 = abs(z3 - z0); // Delta along -45° diagonal.
  float dz1 = abs(z1 - z2); // Delta along +45° diagonal.
  dz0 = saturate((dz0 - depthThreshold) * DepthSensitivity);
  dz1 = saturate((dz1 - depthThreshold) * DepthSensitivity);
  
  // Optional: Use d² instead of d to create a sharper transition.
  //dz0 = dz0 * dz0;
  //dz1 = dz1 * dz1;
  
  // Take dz0 OR dz1.
  silhouette = 1 - (1 - dz0) * (1 - dz1);
  
  // ----- Crease (normal discontinuity)
  float dn0 = 1 - dot(n0, n3); // Delta along -45° diagonal.
  float dn1 = 1 - dot(n1, n2); // Delta along +45° diagonal.
  
  // Skybox pixels do not have valid normals. Ignore them.
  const float skyBoxLimit = 0.99999;
  //if (z0 > skyBoxLimit || z3 > skyBoxLimit)
  //  dn0 = 0;
  //if (z1 > skyBoxLimit || z2 > skyBoxLimit)
  //  dn1 = 0;
  // Optimized:
  float4 isNotSkyBox =  (float4(z0, z1, z2, z3) <= skyBoxLimit);
  dn0 *= isNotSkyBox.x * isNotSkyBox.w;
  dn1 *= isNotSkyBox.y * isNotSkyBox.z;
  
  dn0 = saturate((dn0 - normalThreshold) * NormalSensitivity);
  dn1 = saturate((dn1 - normalThreshold) * NormalSensitivity);
  
  // Take average of normal deltas.
  crease = 0.5 * (dn0 + dn1);
}


float4 PS(in float2 texCoord : TEXCOORD0, bool onePixelEdge) : COLOR0
{
  float silhouette, crease;
  if (onePixelEdge)
    DetectOnePixelEdge(texCoord, silhouette, crease);
  else
    DetectEdge(texCoord, silhouette, crease);
  
  // Debugging: Output silhouette image.
  //return float4(silhouette, silhouette, silhouette, 1);
  
  // Debugging: Output crease image.
  //return float4(crease, crease, crease, 1);
  
  // Debugging: Output combined edge image.
  //float edge = 1 - (1 - silhouette) * (1 - crease);
  //return float4(edge, edge, edge, 1);
  
  float4 color = tex2D(SourceSampler, texCoord);
  float3 edgeColor;
  float edgeFactor;
  if (silhouette)
  {
    edgeColor = SilhouetteColor.rgb;
    edgeFactor = silhouette * SilhouetteColor.a;
  }
  else
  {
    edgeColor = CreaseColor.rgb;
    edgeFactor = crease * CreaseColor.a;
  }
  
  // Option A: Draw silhouette and crease edges.
  color.rgb = lerp(color.rgb, edgeColor, edgeFactor);
  
  // Option B: Modulate source image.
  // Lerp between source color and 2X multiplicative blending.
  //color.rgb = lerp(color.rgb, 2 * color.rgb * edgeColor, edgeFactor);
  
  return color;
}

float4 PSEdge(in float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, false); }
float4 PSOnePixelEdge(in float2 texCoord : TEXCOORD0) : COLOR0 { return PS(texCoord, true); }


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

technique
{
  pass Edge
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSEdge();
  }
  
  pass OnePixelEdge
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSOnePixelEdge();
  }
}
