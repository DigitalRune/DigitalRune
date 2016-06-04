//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GodRayFilter.fx
/// Adds a god ray effect to the scene.
//
// See GodRayFilter.cs for more information.
//
// Notes:
// When the background rather dark (e.g. no sun disk), color information might
// get lost during the blur passes. In this case we could boost the colors in
// PSCreateMask() or we could apply the Intensity in PSCreateMask() instead of
// PSCombine().
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Noise.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The size of the viewport in pixels.
float2 ViewportSize;

float4 Parameters0 = float4(0.5, 0.5, 0.2, 100);
#define LightPosition Parameters0.xy      // The position of the light in texture space.
#define LightRadiusSquared Parameters0.z  // The radius² of the mask.
#define MaxRayLength Parameters0.w        // The max sample distance.

float2 Parameters1 = float2(1, 1);
#define Softness Parameters1.x            // 0 = additive blend, 1 = screen blend
#define AspectRatio Parameters1.y         // The aspect ratio of the viewport.

float3 Intensity = float3(1, 1, 1);

// The number of samples per pixel.
int NumberOfSamples = 8;

// The scene.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  // Samples may lie outside the source texture.
  AddressU = CLAMP;
  AddressV = CLAMP;
  
  Texture = <SourceTexture>;
};

// The texture containing the light shafts.
texture RayTexture;
sampler RaySampler : register(s1) = sampler_state
{
  Texture = <RayTexture>;
};

// The depth texture.
texture GBuffer0;
sampler GBuffer0Sampler : register(s1) = sampler_state
{
  Texture = <GBuffer0>;
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

float lengthSquared(float2 v)
{
  return v.x * v.x + v.y * v.y;
}


VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


// Masks out pixels where the depth is < 1.
float4 PSCreateMask(float2 texCoord : TEXCOORD0) : COLOR
{
  float4 color = tex2Dlod(SourceSampler, float4(texCoord, 0, 0));
  
  // ---- Sun radius mask. (Use falloff: 1-x^4)
  float2 blurVector = LightPosition - texCoord;
  blurVector.x *= AspectRatio;
  float radiusSquared = lengthSquared(blurVector) / LightRadiusSquared;
  float isSun = saturate(1 - radiusSquared * radiusSquared);
  
  // ----- Depth buffer mask.
  float depth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  float isSky = depth > 0.9999f;
  
  color.rgb *= isSun * isSky;
  return float4(color.rgb, 1);
}


// Blur light shafts.
float4 PSBlur(float2 texCoord : TEXCOORD0) : COLOR
{
  //return tex2Dlod(SourceSampler, float4(texCoord, 0, 0));
  
  float2 blurVector = LightPosition - texCoord;
  
  // Limit sample distance to prevent undersampling.
  float blurLength = length(blurVector);
  blurVector = blurVector / (blurLength + 0.0001f) * min(MaxRayLength, blurLength);
  
  float2 uvDelta = blurVector / NumberOfSamples;
  
  // Optional: Use random offset per pixel.
  // Randomizing the sample offsets reduces banding but introduces visible noise.
  //float random = Noise2(texCoord);   // Random value in [0, 1]
  //texCoord += random * uvDelta;
  
  // Use linearly decreasing weights:
  //    n/n, (n-1)/n, ..., 1/n
  // The sum of all weights is
  //    sum(n/n + (n-1)/n + ... + 1/n) = (n+1)/2
  float weight = 1;
  float weightDelta = -1.0 / NumberOfSamples;
  float weightSum = (NumberOfSamples + 1) * 0.5;
  
  float3 color = 0;
  for (int i = 0; i < NumberOfSamples; i++)
  {
    color += tex2D(SourceSampler, texCoord).rgb * weight;
    texCoord += uvDelta;
    weight += weightDelta;
  }
  color /= weightSum;
  
  return float4(color, 1);
}


// Combines the original scene with the light shaft image.
float4 PSCombine(float2 texCoord : TEXCOORD0) : COLOR
{
  //return tex2Dlod(RaySampler, float4(texCoord, 0, 0));
  
  float3 sceneColor = tex2Dlod(SourceSampler, float4(texCoord, 0, 0)).rgb;
  float3 rayColor = tex2Dlod(RaySampler, float4(texCoord, 0, 0)).rgb;
  
  rayColor *= Intensity;
  
  // Softness = 0 --> Additive blending.
  // Softness = 1 --> "Screen" blending (which is softer).
  return float4(sceneColor + rayColor * saturate(1 - sceneColor * Softness), 1);
}


//--------------------------------------------------------
// Techniques
//--------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

technique
{
  pass CreateMask
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCreateMask();
  }
  
  pass Blur
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlur();
  }
  
  pass Combine
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombine();
  }
}
