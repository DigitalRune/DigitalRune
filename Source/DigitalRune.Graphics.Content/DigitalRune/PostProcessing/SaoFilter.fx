//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SaoFilter.fx
/// Scalable Ambient Obscurance
//
// Notes:
// The new algorithm is based on the improved Alchemy AO algorithm which is
// open source under the "BSD" license.
// See http://graphics.cs.williams.edu/papers/SAOHPG12/.
//
// Copyright (c) 2011-2012, NVIDIA
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//
// 1. Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// 
// 2. Redistributions in binary form must reproduce the above copyright notice,
// this list of conditions and the following disclaimer in the documentation
// and/or other materials provided with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"

// Warning: gradient-based operations must be moved out of flow control...
// Is caused by DeriveNormal, which is called before flow control!?
#pragma warning(disable : 4121)


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The log of the maximum pixel offset switching to lower mip level.
// If this value is < 3, then to low mip levels lead to flickering.
// If this value is > 5, then performance drops.
const static int LogMaxOffset = 3;

const static int MaxMipMapLevel = 5;

// The blur radius (in number of samples).
// 4 means that the blur will use 9 samples.
const static int BlurRadius = 4;

// Gaussian coefficients
const static float GaussianWeights[BlurRadius + 1] =
//    { 0.356642, 0.239400, 0.072410, 0.009869 };
//    { 0.398943, 0.241971, 0.053991, 0.004432, 0.000134 };  // stddev = 1.0
      { 0.153170, 0.144893, 0.122649, 0.092902, 0.062970 };  // stddev = 2.0
//    { 0.111220, 0.107798, 0.098151, 0.083953, 0.067458, 0.050920, 0.036108 }; // stddev = 3.0

DECLARE_UNIFORM_FRUSTUMINFO(FrustumInfo);

int NumberOfAOSamples = 11;

float4 AOParameters0;
// The height of a 1 unit object 1 unit relative to the screen.
#define ProjectionScale AOParameters0.x
#define Radius AOParameters0.y
// Intensity divided by radius^6
#define IntensityDivR6 AOParameters0.z
#define Bias AOParameters0.w

float4 AOParameters1;
// The viewport size in pixels.
#define ViewportSize AOParameters1.xy
// The distance of the frustum far plane.
#define Far AOParameters1.z
#define MaxAO AOParameters1.w

float4 AOParameters2 = float4(7, 1, 3, 0);
// If the samples line up, then change this parameter.
#define NumberOfSpiralTurns AOParameters2.x
// EdgeSharpness = 1.0 / EdgeSoftness * CameraFar
#define EdgeSharpness AOParameters2.y
// 2 = step in 2-pixel intervals.
// 3 should be ok too, creates good blur with some dithering.
#define BlurScale AOParameters2.z
#define MinBias AOParameters2.w

// The input texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
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

// The depth texture.
texture GBuffer0;
sampler GBuffer0Sampler = sampler_state
{
  Texture = <GBuffer0>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = POINT;
};

// Only needed if we use G-Buffer normals.
//float4x4 View;
//texture GBuffer1;
//sampler GBuffer1Sampler = sampler_state
//{
//  Texture = <GBuffer1>;
//  AddressU = CLAMP;
//  AddressV = CLAMP;
//  MagFilter = POINT;
//  MinFilter = POINT;
//  MipFilter = POINT;
//};


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

/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Three values in the range [0, 1].
/// \return A value in the range [0, 1].
float GetRandomValue(float2 p)
{
  //return frac(sin(dot(p.xy, float2(12.9898, 78.233)))* 43758.5453);
  
  float a = p.x * p.y * 50000.0;
  a = fmod(a, 13);
  a = a * a;
  float randomA = fmod(a, 0.01) * 100;
  return randomA;
}


// Packs a [0, 1] float value into two float2 channels for storage in a 2 bytes channels.
float2 PackValue(float value)
{
  // Round to the nearest 1/256.0
  float integer = floor(value * 256.0);
  return float2(
    integer * (1.0 / 256.0),      // Integer
    value * 256.0 - integer);     // Fraction
}
float UnpackValue(float2 packedValue)
{
  return packedValue.x * (256.0 / 257.0) + packedValue.y * (1.0 / 257.0);
}


void GetSampleOffset(int index, float rotationOffset, out float2 offset, out float radius)
{
  radius = (index + 0.5) / NumberOfAOSamples;
  float angle = rotationOffset + radius * NumberOfSpiralTurns * 6.28;
  offset = float2(cos(angle), sin(angle));
}


float SampleAO(float2 texCoord, float3 positionView, float3 normalView, float radiusSquared,
               float scaledRadius, float index, float rotationOffset, float bias)
{
  float2 relOffset;
  float relRadius;
  GetSampleOffset(index, rotationOffset, relOffset, relRadius);
  
  scaledRadius *= relRadius;
  
  int mipLevel = clamp(int(floor(log2(scaledRadius * ViewportSize.y))) - LogMaxOffset, 0, MaxMipMapLevel);
  //mipLevel = 0;
  
  float2 unclampedTexCoord = texCoord + relOffset * scaledRadius;
  
  float2 sampleTexCoord = saturate(unclampedTexCoord);
  
  // Ignore samples outside the screen because they can create dark AO blobs.
  // (We can skip this if we use a guard band.)
  if (any(sampleTexCoord - unclampedTexCoord))
    return 0;
  
  // tex2DLod will clamp, but for use in GetPositionView we have to clamp ourselves.
  //sampleTexCoord = saturate(sampleTexCoord);
  
  // Make texCoord relative to mip level: Divide by 2^mipLevel
  //sampleTexCoord = ((int)(sampleTexCoord * ViewportSize) >> mipLevel) / ViewportSize;
  
  float sampleGBufferDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(sampleTexCoord, 0, mipLevel)));
  
  if (sampleGBufferDepth > 0.999)
    return 0;
  
  float sampleDepth = Far * sampleGBufferDepth;
  
  float3 samplePositionView = GetPositionView(sampleTexCoord, sampleDepth, FrustumInfo);
  
  float3 v = samplePositionView - positionView;
  
  float vv = dot(v, v);
  float vn = dot (v, normalView);
  
  const float epsilon = 0.01;
  float f = max(radiusSquared - vv, 0);
  return f * f * f * max((vn - bias) / (epsilon + vv), 0);
}


VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSCreateAO(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 result = float4(0, 0, 0, 0);
  
  // Depth in [0, 1].
  float gBufferDepth = GetGBufferDepth(tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0)));
  
  // Store depth in gb to speed up bilateral blur.
  result.gb = PackValue(gBufferDepth);
  
  // The depth in range [0, Far].
  float depth = Far * gBufferDepth;
  
  float3 positionView = GetPositionView(texCoord, depth, FrustumInfo);
  
  // Use reconstructed normals.
  float3 normalView = DeriveNormal(positionView);
  
  // Abort for skybox pixels. 
  // (This check is made after DeriveNormal() because the method needs ddx/ddy.)
  clip(0.9999f - gBufferDepth);
  
  // Use G-Buffer normals.
  //float3 normalWorld = GetGBufferNormal(tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0)));
  //float3 normalView = mul(normalWorld, View);
  
  // Hash function from AlchemyAO. (Requires SM4+.)
  //float rotationOffset =  (3 * ssC.x ^ ssC.y + ssC.x * ssC.y) * 10;
  float rotationOffset = GetRandomValue(texCoord) * 10;

  // Optimization: Radius² is used in SampleAO().
  float radiusSquared = Radius * Radius;
  
  // Convert world space radius to texture space.
  float scaledRadius = ProjectionScale * Radius / depth;
  
  // A MinBias is used to avoid self-shadowing artifacts in concave edges.
  // We also increase the bias with distance to hide artifacts caused by precision
  // problems.
  float bias = max(MinBias, depth * Bias);
  
  float sum = 0;
  for (int i = 0; i < NumberOfAOSamples; i++)
    sum += SampleAO(texCoord, positionView, normalView, radiusSquared, scaledRadius, i, rotationOffset, bias);
  
  float ao = max(0, 1 - min(MaxAO, sum * IntensityDivR6 * (5.0 / NumberOfAOSamples)));
  
  // Bilateral box-filter over a quad for free, respecting depth edges.
  // (The difference that this makes is subtle.)
  if (abs(ddx(positionView.z)) < 0.02)
  {
    ao -= ddx(ao) * (((int)(texCoord.x * ViewportSize.x) % 2) - 0.5);
  }
  
  if (abs(ddy(positionView.z)) < 0.02)
  {
    ao -= ddy(ao) * (((int)(texCoord.y * ViewportSize.y) % 2) - 0.5);
  }
  
  result.x = ao;
  
  return result;
}


float4 PSBlur(float2 texCoord : TEXCOORD0,
              uniform const float2 offset,
              uniform const bool isLastPass,
              uniform const bool combine) : COLOR0
{
  float4 centerSample = tex2Dlod(OcclusionSampler, float4(texCoord, 0, 0));
  
  float sum = centerSample.r;
  float depth = UnpackValue(centerSample.gb);
  
  float ao = 1;
  if (depth < 0.999)    // No blur needed on skybox pixels.
  {
    float weightSum = GaussianWeights[0];
    sum *= weightSum;
    
    [unroll]
    for (int r = -BlurRadius; r <= BlurRadius; r++)
    {
      if (r != 0)
      {
        float2 sampleTexCoord = texCoord + offset * r * BlurScale;
        float3 sample = tex2Dlod(OcclusionSampler, float4(sampleTexCoord, 0, 0)).rgb;
        float sampleDepth = UnpackValue(sample.gb);
        float sampleAO = sample.r;
        
        // Gaussian blur weight.
        float weight = 0.3 + GaussianWeights[abs(r)];
        
        // Bilateral weight.
        weight *= max(0, 1.0 - EdgeSharpness * abs(sampleDepth - depth));
        
        sum += sampleAO * weight;
        weightSum += weight;
      }
    }
    
    ao = sum / (weightSum + 0.0001);
  }
  
  if (isLastPass)
  {
    if (!combine)
      return float4(ao, ao, ao, ao);
    
    float4 color = tex2Dlod(SourceSampler, float4(texCoord, 0, 0));
    return color * ao;
  }
  
  return float4(ao, centerSample.gb, 1);
}


float4 PSBlurHorizonal(float2 texCoord : TEXCOORD0) : COLOR0
{
  return PSBlur(texCoord, float2(1 / ViewportSize.x, 0), false, false);
}


float4 PSBlurVertical(float2 texCoord : TEXCOORD0) : COLOR0
{
  return PSBlur(texCoord, float2(0, 1 / ViewportSize.y), true, false);
}


float4 PSBlurVerticalAndCombine(float2 texCoord : TEXCOORD0) : COLOR0
{
  return PSBlur(texCoord, float2(0, 1 / ViewportSize.y), true, true);
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
  pass CreateAO
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCreateAO();
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
  
  pass BlurVerticalAndCombine
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlurVerticalAndCombine();
  }
}
