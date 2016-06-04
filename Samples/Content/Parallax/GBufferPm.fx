//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file GBufferPm.fx
/// Creates the G-buffer for normal models with parallax mapping.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Encoding.fxh"
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Material.fxh"
#include "Parallax.fxh"

// CREATE_GBUFFER creates automatically bound shader constants required for
// encoding data in the G-Buffer.
#define CREATE_GBUFFER 1
#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Deferred.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float CameraFar : CAMERAFAR;
float3 CameraPosition : CAMERAPOSITION;

float SpecularPower : SPECULARPOWER;

DECLARE_UNIFORM_NORMALTEXTURE

float SceneNodeType : SCENENODETYPE;

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
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
  float4 SceneNodeTypeAndAlpha : TEXCOORD5;
  float3 ViewDirectionTangent : TEXCOORD6;  // View direction vector in tangent space.
  float3 PositionWorld : TEXCOORD7;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float Depth : TEXCOORD1;
  float3 Normal : TEXCOORD2;
  float3 Tangent : TEXCOORD3;
  float3 Binormal : TEXCOORD4;
  float4 SceneNodeTypeAndAlpha : TEXCOORD5;
  float3 ViewDirectionTangent : TEXCOORD6;
  float3 PositionWorld : TEXCOORD7;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world, float sceneNodeType, float alpha)
{
  VSOutput output = (VSOutput)0;
  output.TexCoord = input.TexCoord;
  
  float4 positionWorld = mul(input.Position, world);
  float4 positionView = mul(positionWorld, View);
  output.PositionWorld = positionWorld.xyz;
  output.Position = mul(positionView, Projection);
  output.Depth = -positionView.z / CameraFar;
  output.Normal = mul(input.Normal, (float3x3)world);
  output.Tangent = mul(input.Tangent, (float3x3)world);
  output.Binormal = mul(input.Binormal, (float3x3)world);
  output.SceneNodeTypeAndAlpha = float4(sceneNodeType, 0, 0, alpha);
  
  // Matrix that transforms column(!) vectors from world space to tangent space.
  float3x3 worldToTangent = float3x3(output.Tangent, output.Binormal, output.Normal);
  
  float3 viewDirectionWorld = positionWorld.xyz - CameraPosition;
  output.ViewDirectionTangent = mul(worldToTangent, viewDirectionWorld);
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World, SceneNodeType, 1);
}


VSOutput VSInstancing(VSInput input,
                      float4 worldColumn0 : BLENDWEIGHT0,
                      float4 worldColumn1 : BLENDWEIGHT1,
                      float4 worldColumn2 : BLENDWEIGHT2,
                      float4 sceneNodeTypeAndAlpha : BLENDWEIGHT3)
{
  float4x4 worldTransposed =
  {
    worldColumn0,
    worldColumn1,
    worldColumn2,
    float4(0, 0, 0, 1)
  };
  float4x4 world = transpose(worldTransposed);
  
  return VS(input, world, sceneNodeTypeAndAlpha.x, sceneNodeTypeAndAlpha.a);
}


void PS(PSInput input, out float4 depthBuffer : COLOR0, out float4 normalBuffer : COLOR1)
{
  // Normalize input vectors.
  float3 normal = normalize(input.Normal);
  float3 tangent = normalize(input.Tangent);
  float3 binormal = normalize(input.Binormal);
  float3 cameraDirection = -float3(View._m02, View._m12, View._m22);
  float3 viewDirectionWorld = normalize(input.PositionWorld - CameraPosition);
  float3 viewDirectionTangent = normalize(input.ViewDirectionTangent);
  
  // Parallax mapping
  float3 texCoordAndHeight = ParallaxMapping(input.TexCoord, HeightSampler,
    HeightScale, HeightBias, viewDirectionTangent.xy);
  
  // Apply parallax to texture coordinates.
  float2 texCoord = texCoordAndHeight.xy;
  
  // Optional: Apply parallax to G-buffer depth.
  float height = texCoordAndHeight.z;
  input.Depth = input.Depth
    - height / abs(viewDirectionTangent.z)        // Similar triangles: h / v.z = depthDelta / |v|
      * dot(cameraDirection, viewDirectionWorld)  // Convert linear distance to view space depth.
      / CameraFar;                                // Convert to [0, 1] range of G-buffer depth.
  
  // Normals maps are encoded using DXT5nm.
  float3 normalMapSample = GetNormalDxt5nm(NormalSampler, texCoord);
  normal = normal * normalMapSample.z + tangent * normalMapSample.x - binormal * normalMapSample.y;

  depthBuffer = 1;
  normalBuffer = float4(0, 0, 0, 1);
  SetGBufferDepth(input.Depth, input.SceneNodeTypeAndAlpha.x, depthBuffer);
  SetGBufferNormal(normal.xyz, normalBuffer);
  SetGBufferSpecularPower(SpecularPower, depthBuffer, normalBuffer);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !MGFX              // TODO: Add Annotation support to MonoGame.
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
