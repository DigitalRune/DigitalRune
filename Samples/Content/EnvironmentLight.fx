//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file EnvironmentLight.fx
///
//
//-----------------------------------------------------------------------------

#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_FRUSTUMCORNERS(FrustumCorners);
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);

float3 DiffuseColor;
float3 SpecularColor;
int TextureSize;
int MaxMipLevel;

texture EnvironmentMap : ENVIRONMENTMAP;
samplerCUBE EnvironmentSampler = sampler_state
{
  Texture = <EnvironmentMap>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = CLAMP;
  AddressV = CLAMP;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSFrustumRayOutput VS(VSFrustumRayInput input)
{
  return VSFrustumRay(input, ViewportSize, FrustumCorners);
}


void PS(float2 texCoord : TEXCOORD0,
        float3 frustumRay : TEXCOORD1,
        out float4 lightBuffer0 : COLOR0,
        out float4 lightBuffer1 : COLOR1)
{
  lightBuffer0 = 0;
  lightBuffer1 = 0;
  
  // Get depth from G-buffer.
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoord, 0, 0));
  float depth = GetGBufferDepth(gBuffer0Sample);
  
  // Abort for skybox pixels.
  clip(0.9999f - depth);
    
  // Reconstruct view direction (camera to pixel) in world space.
  float3 viewDirection = frustumRay * depth;
 
  // Get normal and specular power from G-buffer.
  float4 gBuffer1Sample = tex2Dlod(GBuffer1Sampler, float4(texCoord, 0, 0));
  float3 normal = GetGBufferNormal(gBuffer1Sample);
  float specularPower = GetGBufferSpecularPower(gBuffer0Sample, gBuffer1Sample);
   
  // Diffuse light: Use normal direction to sample the lowest detail mip level.
  // (Note: This looks good in MonoGame but not in XNA because XNA does not filter
  // the borders of the cube map sides.)
  lightBuffer0.rgb = DiffuseColor * FromGamma(texCUBElod(EnvironmentSampler, float4(normal, MaxMipLevel)).rgb);
  
  // Specular light: Use reflection vector to sample cube map.
  float3 viewDirectionReflected = reflect(viewDirection, normal);
  // Cube maps are left handed --> Sample with inverted z.
  viewDirectionReflected.z *= -1;
  // The specular power (surface roughness) is used to select the mip map level.
  // TODO: The term log2(w * sqrt(3)) can be precomputed.
  float mipLevel = log2(TextureSize * sqrt(3)) - 0.5 * log2(specularPower + 1);
  lightBuffer1.rgb = SpecularColor * FromGamma(texCUBElod(EnvironmentSampler, float4(viewDirectionReflected, mipLevel)).rgb);
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
