//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file VarianceShadowMask.fx
/// Creates the shadow mask for a VSM shadow of a directional light.
//
//-----------------------------------------------------------------------------

#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Noise.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/ShadowMap.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);

float4x4 ShadowMatrix;

texture ShadowMap;
sampler ShadowMapSampler = sampler_state
{
  Texture = <ShadowMap>;
  AddressU  = CLAMP;
  AddressV  = CLAMP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;        // Mipmaps could be used with VSM - but not with XNA.
                           // No hardware filtering for float textures in XNA. :-(
};

float4 Parameters0;
#define ShadowMapSize Parameters0.xy
#define MaxDistance Parameters0.z
#define DepthBias Parameters0.w

float4 Parameters1;
#define FadeOutRange Parameters1.x
#define MinVariance Parameters1.y
#define ReduceLightBleedingAmount Parameters1.z
#define ShadowFog Parameters1.w


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


/// Determines whether a position is in the shadow using Variance Shadow Mapping (VSM).
/// \param[in] minVariance    The min limit to which the variance is clamped. A min value
///                           helps to reduce aliasing.
/// \param[in] reduceLightBleedingAmount   A value in the range [0, 1] that determines
///                           how much shadows will be darkened to reduce light bleeding
///                           artifacts. 0 means no darkening.
float GetShadowFactorVsm(float currentDepth, float2 texCoord,
                         sampler2D shadowMap, float2 shadowMapSize, float minVariance, float reduceLightBleedingAmount)
{
  // If we have hardware filtering:
  //float2 moments = tex2D(shadowMap, texCoord).rg;
  
  // Manual bilinear filtering:
  float2 a = tex2D(shadowMap, texCoord + float2(-0.5, -0.5) / shadowMapSize).rg;
  float2 b = tex2D(shadowMap, texCoord + float2( 0.5, -0.5) / shadowMapSize).rg;
  float2 c = tex2D(shadowMap, texCoord + float2(-0.5,  0.5) / shadowMapSize).rg;
  float2 d = tex2D(shadowMap, texCoord + float2( 0.5,  0.5) / shadowMapSize).rg;
  
  float2 weight = frac(texCoord * shadowMapSize - float2(0.5, 0.5));
  float2 ab = lerp(a, b, weight.x);
  float2 cd = lerp(c, d, weight.x);
  float2 moments = lerp(ab, cd, weight.y);
  
  // Get probability that this pixel is lit.
  float p = GetChebyshevUpperBound(moments, currentDepth, minVariance);
  
  // To reduce light bleeding cut off values below reduceLightBleedingAmount.
  // Rescale the rest to [0, 1].
  p = LinearStep(reduceLightBleedingAmount, 1, p);
  
  return 1 - p;
}


float4 PS(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1) : COLOR
{
  // Get depth.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Reconstruct the view space position.
  float3 positionView = frustumRay * depth;
  
  // Compute shadow map texture coordinates.
  float3 shadowTexCoord = GetShadowTexCoord(float4(positionView, 1), ShadowMatrix);
  
  // Abort if the texture coordinates are outside [0, 1].
  if (!IsInRange(shadowTexCoord, 0, 1))
    return 1 - ShadowFog.xxxx;
  
  // Compute the shadow factor (0 = no shadow, 1 = shadow).
  float shadow = GetShadowFactorVsm(shadowTexCoord.z, shadowTexCoord.xy, ShadowMapSampler, ShadowMapSize, MinVariance, ReduceLightBleedingAmount);
  
  // Fade out the shadow in the distance:
  float distanceFade = saturate((MaxDistance - (-positionView.z)) / MaxDistance / FadeOutRange);
  // Fade out based on distance to shadow map borders.
  float3 tc = abs(shadowTexCoord * 2 - 1);
  float s = 1 - max(max(tc.x, tc.y), tc.z);
  float borderFade = 1 - saturate(1 - s / (2 * FadeOutRange));
  // Combined fade out factor:
  float fade = max(borderFade, distanceFade);
  shadow = lerp(ShadowFog, shadow, fade);
  
  // The shadow mask stores the inverse.
  return 1 - shadow.xxxx;
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
