//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Material.fx
/// Combines the decal material (colors, textures) with the light buffer data.
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Material.fxh"
#include "../Deferred.fxh"
#include "../Decal.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines
//#define ALPHA_TEST 1
//#define EMISSIVE 1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 WorldView : WORLDVIEW;
float4x4 WorldViewInverse : WORLDVIEWINVERSE;
float4x4 Projection : PROJECTION;

// 3D position and normal reconstruction from G-buffer.
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
DECLARE_UNIFORM_GBUFFER(GBuffer1, 1);
float CameraFar : CAMERAFAR;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
#if EMISSIVE
float3 EmissiveColor : EMISSIVECOLOR;
#endif
#if ALPHA_TEST
float ReferenceAlpha : REFERENCEALPHA = 0.9f;
#endif
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
};

struct VSOutput
{
  float4 PositionView : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float Depth : TEXCOORD2;
  float4 Position : SV_Position;
};

struct PSInput
{
  float4 PositionView : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float Depth : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.PositionView = mul(input.Position, WorldView);
  output.Position = mul(output.PositionView, Projection);
  output.PositionProj = output.Position;
  output.Depth = -output.PositionView.z / CameraFar;
  
  return output;
}


float4 PS(PSInput input) : COLOR
{
  float3 normal;     // Not used.
  float2 uvDecal;    // UV texture coordinates in decal space.
  float2 uvScreen;   // UV texture coordinates in screen space.
  DeferredDecal(input.PositionView, input.PositionProj, input.Depth,
                WorldViewInverse, ViewportSize,
                GBuffer0Sampler, GBuffer1Sampler,
                normal, uvDecal, uvScreen);
  
  // Diffuse map: premultiplied diffuse color + alpha
  float4 diffuseMap = tex2D(DiffuseSampler, uvDecal);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float alpha = diffuseMap.a;
#if ALPHA_TEST
  clip(alpha * DecalAlpha - ReferenceAlpha);
#endif
  
  // Specular map: non-premultiplied specular color + emissive
  float4 specularMap = tex2D(SpecularSampler, uvDecal);
  specularMap *= alpha;  // Apply alpha, which is already premultiplied in the diffuse map.
  float3 specular = FromGamma(specularMap.rgb);
#if EMISSIVE
  float emissive = specularMap.a;
#endif
  
  // Sample diffuse and specular light intensities.
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, uvScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, uvScreen);
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
  // Combine material colors with lights.
  
  // Diffuse
  float3 result = DiffuseColor * diffuse * diffuseLight;
  
  // Specular
  result += SpecularColor * specular * specularLight;
  
#if EMISSIVE
  // Emissive
  result += EmissiveColor * diffuse * emissive;
#endif
  
  // Premultiply alpha.
  result *= DecalAlpha;
  
  return float4(result, alpha * DecalAlpha);
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

technique Default
{
  pass
  {
#if ALPHA_TEST
    AlphaBlendEnable = false;
#endif
    
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
