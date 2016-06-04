//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Occluder.fx
/// Renders the model as an occluder into the occlusion buffer.
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
//#define SKINNING 1


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float2 ViewportSize : VIEWPORTSIZE;

#if SKINNING
float4x3 Bones[72];
#endif


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
#if SKINNING
  uint4 BoneIndices : BLENDINDICES0;  // uint4 instead of int4 because XNA content pipeline only has Byte4 (R8G8B8A_UInt), but no R8G8B8A_SInt.
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};


struct VSOutput
{
  float2 DepthProj : TEXCOORD0;   // The clip space depth (z, w)
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 DepthProj : TEXCOORD0;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world)
{
  VSOutput output = (VSOutput)0;
#if SKINNING
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  input.Position.xyz = mul(input.Position, skinningMatrix);
#endif
  
  float4 positionView = mul(input.Position, mul(world, View));
  output.Position = mul(positionView, Projection);
  output.DepthProj = output.Position.zw;
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World);
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
  
  return VS(input, world);
}


float4 PS(PSInput input) : COLOR0
{
  // DepthProj stores z and w in clip space.
  // Homogeneous divide gives non-linear depth in z-buffer.
  float depth = input.DepthProj.x / input.DepthProj.y;
  
  return float4(depth, depth, depth, 1);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique Default
#if !SKINNING && !MGFX           // TODO: Add Annotation support to MonoGame.
< string InstancingTechnique = "DefaultInstancing"; >
#endif
{
  pass
  {
    CullMode = NONE;
    
#if !SM4
    VertexShader = compile vs_2_0 VSNoInstancing();
    PixelShader = compile ps_2_0 PS();
#elif SM4
    VertexShader = compile vs_4_0_level_9_1 VSNoInstancing();
    PixelShader = compile ps_4_0_level_9_1 PS();
#endif
  }
}

#if !SKINNING
technique DefaultInstancing
{
  pass
  {
    CullMode = NONE;
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
