//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file CameraMotionBlur.fx
/// Blurs the image when the camera moves.
//
// see "GPU Gems 3: Motion Blur as a Post-Processing Effect"
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

float3 FrustumCorners[4];

// The inverse view matrix.
float4x4 ViewInverse;

// The view projection matrix of the last frame.
float4x4 ViewProjOld;

// The blur strength in the range [0, infinity[.
float Strength = 0.6f;

// The number of samples for the blur.
int NumberOfSamples = 8;

// The input texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// The normalized linear planar depth (range [0, 1]).
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


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


float4 PS(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR0
{
  // Get depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Reconstruct the view space position.
  float4 positionView = float4(frustumRay * depth, 1);
  
  // Get the xy position on the near plane in projection space. This is in the range [-1, 1].
  float2 positionProj = float2(texCoord.x * 2 - 1, (1 - texCoord.y) * 2 - 1);
  
  // Get world space position.
  float4 positionWorld = mul(positionView, ViewInverse);
  
  // Compute positionProj of the last frame.
  float4 positionProjOld = mul(positionWorld, ViewProjOld);
  
  // Perspective divide.
  positionProjOld /= positionProjOld.w;
  
  // Get screen space velocity.
  // Divide by 2 to convert from homogenous clip space [-1, 1] to texture space [0, 1].
  float2 velocity = -(positionProj - positionProjOld.xy) / 2 / NumberOfSamples * Strength;
  velocity.y = -velocity.y;
  texCoord -= velocity * NumberOfSamples / 2;
  
  // Blur in velocity direction.
  float4 color = float4(0, 0, 0, 0);
  float weightSum = 0;
  float weightDelta = 1 / (float)NumberOfSamples * 0.5;
  for (int i = 0; i < NumberOfSamples; i++)
  {
    float weight = 1 - i * weightDelta;
    color += tex2D(SourceSampler, texCoord) * weight;
    texCoord += velocity;
    weightSum += weight;
  }
  
  color /= weightSum;
  return color;
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
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
