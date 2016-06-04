//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MaterialPm.fx
/// Combines the material of a model (e.g. textures) with the light buffer data.
/// Uses parallax mapping.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "Parallax.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;
float3 CameraPosition : CAMERAPOSITION;

DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer0, 0);
DECLARE_UNIFORM_LIGHTBUFFER(LightBuffer1, 1);

float3 DiffuseColor : DIFFUSECOLOR;
float3 SpecularColor : SPECULARCOLOR;
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB)

// ----- Parallax Mapping
float HeightScale = 0.03;
float HeightBias = 0;
texture HeightTexture;
sampler HeightSampler = sampler_state
{
  Texture = <HeightTexture>;
  MinFilter = LINEAR;
  MagFilter = LINEAR;
  MipFilter = LINEAR;
  AddressU = Wrap;
  AddressV = Wrap;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
  float3 Normal : NORMAL;
  float3 Tangent : TANGENT;
  float3 Binormal : BINORMAL;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float2 ViewDirectionTangent : TEXCOORD2;  // View direction vector in tangent space.
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
  float2 ViewDirectionTangent : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float4 instanceColorAndAlpha)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;

  float4 positionWorld = mul(input.Position, world);
  float4 positionView = mul(positionWorld, View);
  output.Position = mul(positionView, Projection);
  output.PositionProj = output.Position;
  
  float3 normal =  mul(input.Normal, (float3x3)world);
  float3 tangent = mul(input.Tangent, (float3x3)world);
  float3 binormal = mul(input.Binormal, (float3x3)world);
  
  // Matrix that transforms column(!) vectors from world space to tangent space.
  float3x3 worldToTangent = float3x3(tangent, binormal, normal);
  
  float3 viewDirectionWorld = positionWorld.xyz - CameraPosition;
  output.ViewDirectionTangent = mul(worldToTangent, viewDirectionWorld).xy;
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World, 0);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 colorAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);

  return VS(input, world, colorAndAlpha);
}


float4 PS(PSInput input) : COLOR0
{
  float2 texCoord = ParallaxMapping(input.TexCoord, HeightSampler,
    HeightScale, HeightBias, normalize(input.ViewDirectionTangent)).xy;
  
  float4 diffuseMap = tex2D(DiffuseSampler, texCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float4 specularMap = tex2D(SpecularSampler, texCoord);
  float3 specular = FromGamma(specularMap.rgb);
  
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);

  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 1);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX           // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
#if !SM4
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#else
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}

technique DefaultInstancing
{
  pass
  {
#if !SM4
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
