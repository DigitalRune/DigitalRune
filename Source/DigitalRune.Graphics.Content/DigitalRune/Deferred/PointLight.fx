//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file PointLight.fx
/// Renders a point light into the light buffer for deferred lighting.
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
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);  // In world space.
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);

float3 PointLightDiffuse : POINTLIGHTDIFFUSE;
float3 PointLightSpecular : POINTLIGHTSPECULAR;
float3 PointLightPosition : POINTLIGHTPOSITION;  // Position in world space relative to camera!
float PointLightRange : POINTLIGHTRANGE;
float PointLightAttenuation : POINTLIGHTATTENUATION;
DECLARE_UNIFORM_LIGHTTEXTURE(PointLightTexture, POINTLIGHTTEXTURE);
float3x3 PointLightTextureMatrix;   // Converts from view to light texture space.

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
        uniform const bool hasShadow,
        uniform const int textureType)
{
  lightBuffer0 = 0;
  lightBuffer1 = 0;
  
  // Get depth.
  float4 gBuffer0Sample = tex2D(GBuffer0Sampler, texCoord);
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
  
  // Reconstruct world space position.
  float3 cameraToPixel = frustumRay * depth;
  
  // Get normal.
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoord);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  // Compute light distance and normalized direction.
  float3 lightDirection = cameraToPixel - PointLightPosition;
  float lightDistance = length(lightDirection);
  lightDirection = lightDirection / lightDistance;
  
  // Compute N.L and distance attenuation. Abort if the light is attenuated to 0.
  float distanceAttenuation = ComputeDistanceAttenuation(lightDistance, PointLightRange, PointLightAttenuation);
  float nDotLAttenuated = dot(normal, -lightDirection) * distanceAttenuation;
  clip(nDotLAttenuated - 0.0001f);
  
  // Blinn-Phong specular term.
  float3 viewDirection = normalize(cameraToPixel);
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
  float3 h = -normalize(lightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.000001 + nDotH, specularPower);
  
  // Shadow map
  float shadowTerm = 1;
  if (hasShadow)
    shadowTerm = dot(tex2D(ShadowMaskSampler, texCoord), ShadowMaskChannel);
  
  // Projected texture
  float3 textureColor = float3(1, 1, 1);
  if (textureType != TextureNone)
  {
    if (textureType == TextureRgb)
      textureColor = texCUBE(PointLightTextureSampler, mul(lightDirection, PointLightTextureMatrix)).rgb;
    else
      textureColor = texCUBE(PointLightTextureSampler, mul(lightDirection, PointLightTextureMatrix)).aaa;
    
    textureColor = FromGamma(textureColor);
  }
  
  lightBuffer0.rgb = textureColor * PointLightDiffuse * nDotLAttenuated * shadowTerm;
  lightBuffer1.rgb = textureColor * PointLightSpecular * blinnPhong * nDotLAttenuated * shadowTerm;
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
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
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
