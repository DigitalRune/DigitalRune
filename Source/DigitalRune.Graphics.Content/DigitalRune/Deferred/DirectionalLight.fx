//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file DirectionalLight.fx
/// Renders a directional light into the light buffer for deferred lighting.
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

float3 DirectionalLightDiffuse : DIRECTIONALLIGHTDIFFUSE;
float3 DirectionalLightSpecular : DIRECTIONALLIGHTSPECULAR;
float3 DirectionalLightDirection : DIRECTIONALLIGHTDIRECTION;
texture DirectionalLightTexture : DIRECTIONALLIGHTTEXTURE;
sampler DirectionalLightTextureSampler = sampler_state
{
  Texture = <DirectionalLightTexture>;
  AddressU  = WRAP;
  AddressV  = WRAP;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
};
float4x4 DirectionalLightTextureMatrix; // Converts from view to light texture space.

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
  
  // Get normal.
  float4 gBuffer1Sample = tex2D(GBuffer1Sampler, texCoord);
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  
  // Compute N.L and abort if it attenuates the light to 0.
  float nDotL = dot(normal, -DirectionalLightDirection);
  clip(nDotL - 0.0001f);
  
  float3 viewDirection = normalize(frustumRay * depth);
  
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
  
  // Blinn-Phong specular
  float3 h = -normalize(DirectionalLightDirection + viewDirection);
  float3 nDotH = saturate(dot(normal, h));
  float3 blinnPhong = pow(0.000001 + nDotH, specularPower);
  
  // Use a self-shadowing term. (e.g. N.L or saturate(4 * N.L)).
  float selfShadowingTerm = nDotL;
  
  float shadowTerm = 1;
  float3 textureColor = float3(1, 1, 1);
  
  if (hasShadow)
    shadowTerm = dot(tex2D(ShadowMaskSampler, texCoord), ShadowMaskChannel);
  
  if (textureType != TextureNone)
  {
    float4 lightTexCoord = mul(float4(viewDirection, 1), DirectionalLightTextureMatrix);
    
    if (textureType == TextureRgb)
      textureColor = FromGamma(tex2Dproj(DirectionalLightTextureSampler, lightTexCoord).rgb);
    else
      textureColor = FromGamma(tex2Dproj(DirectionalLightTextureSampler, lightTexCoord).aaa);
  }
  
  lightBuffer0.rgb = textureColor * DirectionalLightDiffuse * nDotL * shadowTerm;
  lightBuffer1.rgb = textureColor * DirectionalLightSpecular * blinnPhong * selfShadowingTerm * shadowTerm;
}

void PS(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
        out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, false, TextureNone);
}

void PSShadow(float2 texCoord : TEXCOORD0, float3 frustumRay : TEXCOORD1,
              out float4 lightBuffer0 : COLOR0, out float4 lightBuffer1 : COLOR1)
{
  PS(texCoord, frustumRay, lightBuffer0, lightBuffer1, true, TextureNone);
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
