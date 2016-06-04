//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SsaoFilter.fx
/// Screen-Space Ambient-Occlusion
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

// 16 normalized random vector used to create random sample directions.
float3 RandomVectors[9];

// The distance of the frustum far plane.
float Far;

// Inner and outer sample radius.
float2 Radius = float2(0.01f, 0.02f);
float2 MaxDistances = float2(0.5f, 1.5f);
float Strength = 0.5f;

// The softness factor for the edge-preserving blur.
// - Small values near 0 preserve edges.
// - Larger value soften the edges.
// 0 is not allowed.
float EdgeSoftness = 0.1;

// Sample offsets, weights and sphere widths for method A.
static float2 OffsetsA[4] = { float2(0.2f, 0), float2(-0.4f, 0), float2(0, 0.8f), float2(0, -1.6f) };
static float WeightsA[4] = { 1.0f / 4.0f, 1.0f / 4.0f, 1.0f / 4.0f, 1.0f / 4.0f };
static float SphereWidthsA[4] = { 1.0f, 2.0f, 3.0f, 4.0f };

// The same for method B:
static float2 OffsetsB[2] = { float2(0.3f, 0), float2(0.8f, 0) };
static float WeightsB[2] = { 1.0f / 4.0f, 1.0f / 4.0f };
static float SphereWidthsB[2] = { 1.0f, 1.0f };

// The input texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// The depth texture.
texture GBuffer0;
sampler GBuffer0Sampler = sampler_state
{
  Texture = <GBuffer0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// The occlusion texture (used in the blur passes).
texture OcclusionTexture;
sampler OcclusionSampler = sampler_state
{
  Texture = <OcclusionTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = LINEAR;
  MinFilter = LINEAR;
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


float4 PSCreateLinesA(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 currentPixel = texCoord * ViewportSize;
  float invAspectRatio = float(ViewportSize.y) / float(ViewportSize.x);
  
  // Get a random vector. The pattern tiles all 3x3 pixels.
  float2 randomIndices = floor(currentPixel) % 3;
  float3 randomVector = RandomVectors[randomIndices.x * 3 + randomIndices.y];
  
  float depth = GetGBufferDepth(tex2D(GBuffer0Sampler, texCoord)) * Far;
  float occlusionAmount = 0;     // Original: float occlusionAmount = 0.5f * CenterWeight;
  for (int j = 0; j < 4; j++)
  {
    float2 samplePoint = OffsetsA[j].xy;
    samplePoint = reflect(samplePoint, randomVector.xy);
    samplePoint.x *= invAspectRatio;
    float3 sampleOffset = float3(samplePoint, SphereWidthsA[j]);
    sampleOffset *= Radius.x * randomVector.z;
    float sampleWidth = sampleOffset.z;
    float2 uvSample = texCoord + sampleOffset.xy;
    float4 gBuffer0 = tex2Dlod(GBuffer0Sampler, float4(uvSample, 0, 0));
    float depthSample = GetGBufferDepth(gBuffer0) * Far;
    
    float depthDifference = depth - depthSample;
    float occlusionContribution = saturate((depthDifference / sampleWidth.x) + 0.5f);
    float distanceModifier = saturate((MaxDistances.x - depthDifference) / MaxDistances.x);
    float modifiedContribution = lerp(0, occlusionContribution, distanceModifier);
    modifiedContribution *= WeightsA[j] ;
    occlusionAmount += modifiedContribution;
  }
  
  occlusionAmount = saturate((occlusionAmount - 0.5f) * 2);
  occlusionAmount = saturate(1 - (Strength * occlusionAmount));
  
  return occlusionAmount * occlusionAmount;
}


// Create SSAO using ToyStory 3 style line samples.
float4 PSCreateLinesB(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 currentPixel = texCoord * ViewportSize;
  float invAspectRatio = float(ViewportSize.y) / float(ViewportSize.x);
  
  // Get a random vector. The pattern tiles all 3x3 pixels.
  // xy are a random normal for reflection/rotation. z is a random scale factor.
  float2 randomIndices = floor(currentPixel) % 3;
  float3 randomVector = RandomVectors[randomIndices.x * 3 + randomIndices.y];
  
  float depth = GetGBufferDepth(tex2D(GBuffer0Sampler, texCoord)) * Far;
  float occlusionAmount = 0;          // Original: float occlusionAmount = 0.5f * CenterWeight;
  float2 occlusionAmounts = float2(occlusionAmount, occlusionAmount);
  for (int i = 0; i < 2; i++)
  {
    float radius = Radius[i] * randomVector.z; // Original: radius = min(radii[i] / depth, 0.07f);
    float iIs1 = i;
    float iIs0 = 1 - i;
    for (int j = 0; j < 2; j++)
    {
      float2 samplePoint = OffsetsB[j].xy;
      samplePoint = reflect(randomVector.xy, samplePoint);
      samplePoint.x *= invAspectRatio;
      float3 sampleOffset = float3(samplePoint, SphereWidthsB[j]);
      sampleOffset *= radius;
      float sampleWidth = sampleOffset.z;
      float2 uvSample1 = texCoord + sampleOffset.xy;
      float depthSample1 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(uvSample1, 0, 0)));
      float2 uvSample2 = texCoord - sampleOffset.xy;
      float depthSample2 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(uvSample2, 0, 0)));
      
      float2 depthSamples = float2(depthSample1, depthSample2) * Far;
      float2 depthDifferences = depth.xx - depthSamples;
      float2 occlusionContributions = saturate((depthDifferences / sampleWidth.xx) + 0.5f);
      
      // ----- Handling missing samples (to avoid black halos):
      float2 distanceModifiers = saturate((MaxDistances[i] - depthDifferences.xy) / MaxDistances[i]);
      // If one sample is missing, try to use the inverted value of the paired sample.
      float2 modifiedContributions = lerp(lerp(0.5f, 1.0f - occlusionContributions.yx, distanceModifiers.yx), occlusionContributions.xy, distanceModifiers.xy);
      // Use 0 for missing samples.
      //float2 modifiedContributions = lerp(0, occlusionContributions.xy, distanceModifiers.xy);
      // For debugging: Do not handle missing samples.
      //float2 modifiedContributions = occlusionContributions;
      
      modifiedContributions *= WeightsB[j] ;
      float modifiedContribution = modifiedContributions.x + modifiedContributions.y;
      // To avoid array indexing in l-value (occlusionAmounts[i] += ...;)
      occlusionAmounts.x += (modifiedContribution * (1 - i));
      occlusionAmounts.y += (modifiedContribution * (i));
    }
  }
  
  occlusionAmounts = saturate((occlusionAmounts - 0.5f) * 2);
  occlusionAmount = saturate(occlusionAmounts.x + occlusionAmounts.y);
  
  occlusionAmount = saturate(1 - (Strength * occlusionAmount));
  
  return occlusionAmount * occlusionAmount;
}


float4 PSBlur(float2 texCoord : TEXCOORD0, uniform const float2 offset) : COLOR0
{
  // A bilateral blur filter.
  // This is an edge-preserving filter. This is needed because a normal blur
  // would blur to much light areas into shadow areas creating a bright halo
  // around objects.
  
  // Get value and depth of current pixel.
  float current = tex2Dlod(OcclusionSampler, float4(texCoord, 0, 0)).x;
  float depth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  
  // Now, create weighted sum of samples.
  // For the bilateral part: Reduce the weight if the depth differ a lot.
  // (Notes: We could also simply discard bad samples and renormalize.)
  float sum = 0;
  float weightSum = 0;
  for (int i = -1; i <= 1; i++)
  {
    float2 currentOffset = i * offset;
    float sample0 = tex2Dlod(OcclusionSampler, float4(texCoord - currentOffset, 0, 0)).x;
    float sampleDepth0 = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord - currentOffset, 0, 0)));
    float weight0 = 1 - saturate(abs(depth - sampleDepth0) / (EdgeSoftness * depth));
    
    sum += sample0 * weight0;
    weightSum += weight0;
  }
  
  return sum / weightSum;
}


float4 PSBlurHorizonal(float2 texCoord : TEXCOORD0) : COLOR0
{
  return PSBlur(texCoord, float2(1 / ViewportSize.x, 0));
}


float4 PSBlurVertical(float2 texCoord : TEXCOORD0) : COLOR0
{
  return PSBlur(texCoord, float2(0, 1 / ViewportSize.y));
}


float4 PSCombine(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Sample scene and ambient occlusion map.
  float4 color = tex2Dlod(SourceSampler, float4(texCoord, 0, 0));
  float ao = tex2Dlod(OcclusionSampler, float4(texCoord, 0, 0)).x;
  
  ao = saturate(1 - Strength * (1 - ao));
  
  // Use ambient occlusion to make parts of the scene darker.
  return color * ao;
}


float4 PSCopy(float2 texCoord : TEXCOORD0) : COLOR0
{
  float ao = tex2Dlod(OcclusionSampler, float4(texCoord, 0, 0)).x;
  ao = saturate(1 - Strength * (1 - ao));
  return ao;
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

technique
{
  pass CreateLinesA
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCreateLinesA();
  }
  
  pass CreateLinesB
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCreateLinesB();
  }
  
  pass BlurHorizontal
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlurHorizonal();
  }
  
  pass BlurVertical
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlurVertical();
  }
  
  pass Combine
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCombine();
  }
  
  pass Copy
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCopy();
  }
}
