//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Spotlight.fx
/// Renders a spotlight into the light buffer for deferred lighting.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Lighting.fxh"
#include "../Noise.fxh"
#include "../ShadowMap.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Type of light texture.
static const int TextureNone = 0;    // No texture.
static const int TextureRgb = 1;     // RGB texture.
static const int TextureAlpha = 2;   // Alpha-only texture.


float4x4 WorldViewProjection : WORLDVIEWPROJECTION;  // (Only for clip geometry.)
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);

float3 SpotlightDiffuse : SPOTLIGHTDIFFUSE;
float3 SpotlightSpecular : SPOTLIGHTSPECULAR;
float3 SpotlightDirection : SPOTLIGHTDIRECTION;
float3 SpotlightPosition : SPOTLIGHTPOSITION;     // Position in world space relative to camera!
float SpotlightRange : SPOTLIGHTRANGE;
float SpotlightAttenuation : SPOTLIGHTATTENUATION;
float SpotlightCutoffAngle : SPOTLIGHTCUTOFFANGLE;
float SpotlightFalloffAngle : SPOTLIGHTFALLOFFANGLE;
DECLARE_UNIFORM_LIGHTTEXTURE(SpotlightTexture, SPOTLIGHTTEXTURE);  // Matrix is also relative to camera!
float4x4 SpotlightTextureMatrix;   // Converts from view to light texture space.

float4 ShadowMaskChannel;
DECLARE_UNIFORM_SHADOWMASK(ShadowMask);


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

float4 VSClip(float4 position : POSITION) : SV_Position
{
  return mul(position, WorldViewProjection);
}

float4 PSClip() : COLOR0
{
  return 0;
}


VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


void PS(float2 texCoord : TEXCOORD0,
        float3 frustumRay : TEXCOORD1,
        out float4 lightBuffer0 : COLOR0,
        out float4 lightBuffer1 : COLOR1,
        uniform const float hasShadow,
        uniform const int textureType)
{
  lightBuffer0 = 0;
  lightBuffer1 = 0;
  
  float4 gBuffer0Sample = tex2D(GBuffer0Sampler, texCoord);
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  clip(0.9999f - depth);
  
  float3 cameraToPixel = frustumRay * depth;
  
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoord);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  float3 lightDirection = cameraToPixel - SpotlightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  // Compute N.L and distance attenuation. Abort if the light is attenuated to 0.
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, SpotlightRange, SpotlightAttenuation);
  float coneAttenuation = ComputeConeAttenuation(lightDirection, SpotlightDirection, SpotlightFalloffAngle, SpotlightCutoffAngle);
  float nDotLAttenuated = dot(normal, -lightDirection) * distanceAttenuation * coneAttenuation;
  clip(nDotLAttenuated - 0.0001f);
  
  //// Blinn-Phong
  float3 viewDirection = normalize(cameraToPixel);
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
  float3 h = -normalize(lightDirection + viewDirection);
  float blinnPhong = pow(0.000001 + saturate(dot(normal, h)), specularPower);
  
  // Shadow map
  float shadowTerm = 1;
  if (hasShadow)
    shadowTerm = dot(tex2D(ShadowMaskSampler, texCoord), ShadowMaskChannel);
  
  // Projected texture
  float3 textureColor = float3(1, 1, 1);
  if (textureType != TextureNone)
  {
    float4 lightTexCoord = mul(float4(cameraToPixel, 1), SpotlightTextureMatrix);
    
    if (textureType == TextureRgb)
      textureColor = FromGamma(tex2Dproj(SpotlightTextureSampler, lightTexCoord).rgb);
    else
      textureColor = FromGamma(tex2Dproj(SpotlightTextureSampler, lightTexCoord).aaa);
  }
  
  lightBuffer0.rgb = textureColor * SpotlightDiffuse * nDotLAttenuated * shadowTerm;
  lightBuffer1.rgb = textureColor * SpotlightSpecular * blinnPhong * nDotLAttenuated * shadowTerm;
}

void PS(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
        out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, false);
}

void PSShadow(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
              out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, false);
}

void PSTextureRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                  out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, TextureRgb);
}

void PSTextureAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                    out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, TextureAlpha);
}

void PSShadowTextureRgb(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                        out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, TextureRgb);
}

void PSShadowTextureAlpha(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
                          out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, TextureAlpha);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0_level_9_3
#define PSTARGET ps_4_0_level_9_3
#endif

technique
{
  pass Clip
  {
    VertexShader = compile VSTARGET VSClip();
    PixelShader = compile PSTARGET PSClip();
  }
  pass Default
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
  pass Shadowed
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSShadow();
  }
  pass TexturedRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSTextureRgb();
  }
  pass TexturedAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSTextureAlpha();
  }
  pass ShadowedTexturedRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSShadowTextureRgb();
  }
  pass ShadowedTexturedAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSShadowTextureAlpha();
  }
}
