//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ProjectedShadowSkinned.fx
/// Renders a planar projected shadow for a mesh. Supports mesh skinning 
/// (skeletal animation).
//
//-----------------------------------------------------------------------------


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

float4x4 World;
float4x4 ViewProjection;

float4x3 Bones[72];

float4x4 ShadowMatrix;
float4 ShadowColor;       // The shadow color and alpha value (using premultiplied alpha).


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
  uint4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
};


struct VSOutput
{
  float4 Position : SV_Position;
};



//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;

  // Mesh skinning.
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  input.Position.xyz = mul(input.Position, skinningMatrix);
  
  // World transform.
  float4x4 worldViewProjection = mul(mul(World, ShadowMatrix), ViewProjection);
  output.Position = mul(input.Position, worldViewProjection);
  
  return output;
}


float4 PS(VSOutput input) : COLOR
{
  return ShadowColor;
}


#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
