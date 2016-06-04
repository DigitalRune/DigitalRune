//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file CieSkyFilter.fx
/// Applies an attenuation to the source texture using the CIE sky luminance
/// distribution.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize;

// The frustum corners in WORLD SPACE!!!
float3 FrustumCorners[4];

// The direction to the sun.
float3 SunDirection;
float Exposure;

float4 Abcd = float4(-1.0, -0.32, 10.0, -3.0);
float2 EAndStrength = float2(0.45, 1);
#define A Abcd.x
#define B Abcd.y
#define C Abcd.z
#define D Abcd.w
#define E EAndStrength.x
#define Strength EAndStrength.y

// The input texture.
texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
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
  float4 color = tex2D(SourceSampler, texCoord);
  float3 toPosition = normalize(frustumRay);
  
  // Angle between sun and pixel.
  float cosTheta = dot(SunDirection, toPosition);
  float theta = acos(cosTheta);
  
  // Angle between pixel and zenith (which is (0, 1, 0)).
  float cosPhi = toPosition.y;
  
  // Angle between sun and zenith.
  float cosThetaZ = SunDirection.y;
  
  // Compute relative luminance.
  float luminanceAttenuation =
    (1 + A * exp(B / cosPhi)) * (1 + C * (exp(D * theta) - exp(D * Pi / 2)) + E * cosTheta *cosTheta)
    / ((1 + A * exp(B / cosThetaZ)) * (1 + C * (1 - exp(D * Pi / 2)) + E));
  
  luminanceAttenuation = lerp(1, luminanceAttenuation, Strength);
  
  return float4(color.rgb * Exposure * luminanceAttenuation, color.a);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
