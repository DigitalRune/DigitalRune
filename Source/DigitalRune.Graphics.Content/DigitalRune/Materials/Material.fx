//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Material.fx
/// Combines the material of a model (e.g. textures) with the light buffer data.
/// Supports:
/// - Diffuse color/texture
/// - Specular color/texture
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#include "../Material.fxh"
#include "../Noise.fxh"


//-----------------------------------------------------------------------------
// Defines
//-----------------------------------------------------------------------------

// Possible defines
//#define ALPHA_TEST 1
//#define TRANSPARENT 1
//#define EMISSIVE 1
//#define MORPHING 1
//#define SKINNING 1

#if ALPHA_TEST
#define CULL_MODE NONE
#else
#define CULL_MODE CCW
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;

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
#if TRANSPARENT
float InstanceAlpha : INSTANCEALPHA = 1;
#endif
DECLARE_UNIFORM_DIFFUSETEXTURE      // Diffuse (RGB) + Alpha (A)
DECLARE_UNIFORM_SPECULARTEXTURE     // Specular (RGB) + Emissive (A)

#if MORPHING
float MorphWeight0 : MORPHWEIGHT0;
float MorphWeight1 : MORPHWEIGHT1;
float MorphWeight2 : MORPHWEIGHT2;
float MorphWeight3 : MORPHWEIGHT3;
float MorphWeight4 : MORPHWEIGHT4;
#endif

#if SKINNING
float4x3 Bones[72];
#endif


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
  float2 TexCoord : TEXCOORD0;
#if MORPHING
  float3 MorphPosition0 : POSITION1;
  float3 MorphPosition1 : POSITION2;
  float3 MorphPosition2: POSITION3;
  float3 MorphPosition3 : POSITION4;
  float3 MorphPosition4 : POSITION5;
#endif
#if SKINNING
  uint4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};


struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
#if TRANSPARENT
  float4 InstanceColorAndAlpha : TEXCOORD2;
#endif
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 PositionProj : TEXCOORD1;
#if TRANSPARENT
  float4 InstanceColorAndAlpha : TEXCOORD2;
#if SM4
  float4 VPos : SV_Position;
#else
  float2 VPos : VPOS;
#endif
#endif
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float4 instanceColorAndAlpha)
{
  float4 position = input.Position;
  
#if MORPHING
  // ----- Apply morph targets.
  position.xyz += MorphWeight0 * input.MorphPosition0;
  position.xyz += MorphWeight1 * input.MorphPosition1;
  position.xyz += MorphWeight2 * input.MorphPosition2;
  position.xyz += MorphWeight3 * input.MorphPosition3;
  position.xyz += MorphWeight4 * input.MorphPosition4;
#endif

#if SKINNING
  // ----- Apply skinning matrix.
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  position.xyz = mul(position, skinningMatrix);
#endif
  
  // ----- Apply world, view, projection transformation.  
  float4 positionWorld = mul(position, world);
  float4 positionView = mul(positionWorld, View);
  float4 positionProj = mul(positionView, Projection);

  // ----- Output
  VSOutput output = (VSOutput)0;
  output.Position = positionProj;
  output.TexCoord = input.TexCoord;
  output.PositionProj = positionProj;
#if TRANSPARENT
  output.InstanceColorAndAlpha = instanceColorAndAlpha;
#endif
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
#if TRANSPARENT
  return VS(input, World, float4(0, 0, 0, InstanceAlpha));
#else
  return VS(input, World, 0);
#endif
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
  float4 diffuseMap = tex2D(DiffuseSampler, input.TexCoord);
  
#if ALPHA_TEST
  clip(diffuseMap.a - ReferenceAlpha);
#endif
#if TRANSPARENT
  // Screen-door transparency
  float c = input.InstanceColorAndAlpha.a - Dither4x4(input.VPos.xy);
  // The alpha can be negative, which means the dither pattern is inverted.
  if (input.InstanceColorAndAlpha.a < 0)
    c = -(c + 1);
  
  clip(c);
#endif
  
  float4 specularMap = tex2D(SpecularSampler, input.TexCoord);
  float3 diffuse = FromGamma(diffuseMap.rgb);
  float3 specular = FromGamma(specularMap.rgb);
  
#if EMISSIVE
  float emissive = specularMap.a;
#endif
  
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  
  float4 lightBuffer0Sample = tex2D(LightBuffer0Sampler, texCoordScreen);
  float4 lightBuffer1Sample = tex2D(LightBuffer1Sampler, texCoordScreen);
  
  float3 diffuseLight = GetLightBufferDiffuse(lightBuffer0Sample, lightBuffer1Sample);
  float3 specularLight = GetLightBufferSpecular(lightBuffer0Sample, lightBuffer1Sample);
  
#if EMISSIVE
  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight + EmissiveColor * diffuse * emissive, 1);
#else
  return float4(DiffuseColor * diffuse * diffuseLight + SpecularColor * specular * specularLight, 1);
#endif
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SKINNING && !MORPHING
#define SUPPORTS_INSTANCING 1
#endif

technique Default
#if !MGFX && SUPPORTS_INSTANCING     // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
    CullMode = CULL_MODE;
    
#if !SM4 && !TRANSPARENT
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#elif !SM4
    VertexShader = compile vs_3_0 VSNoInstancing();
    PixelShader = compile ps_3_0 PS();                // ps_3_0 required for VPOS.
#elif SM4 && !TRANSPARENT
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#elif SM4
    VertexShader = compile vs_4_0 VSNoInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}

#if SUPPORTS_INSTANCING
technique DefaultInstancing
{
  pass
  {
    CullMode = CULL_MODE;
#if !SM4
    VertexShader = compile vs_3_0 VSInstancing();
    PixelShader = compile ps_3_0 PS();
#else
    VertexShader = compile vs_4_0 VSInstancing();
    PixelShader = compile ps_4_0 PS();
#endif
  }
}
#endif
