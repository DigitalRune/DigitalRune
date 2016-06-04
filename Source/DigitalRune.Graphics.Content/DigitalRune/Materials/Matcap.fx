//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Matcap.fx
/// Renders an object by sampling a surface material ("material capture") using
/// the normal vector.
/// Supports:
/// - Matcap texture
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Material.fxh"
#if DEPTH_TEST
#include "../Encoding.fxh"
#include "../Deferred.fxh"
#endif


//--------------------------------------------------------
// Defines
//--------------------------------------------------------

// Possible defines
//#define DEPTH_TEST 1    // Perform manual depth test using G-buffer.
//#define TWO_SIDED 1
//#define NORMAL_MAP 1
//#define SKINNING 1


//--------------------------------------------------------
// Constants
//--------------------------------------------------------

float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 CameraPosition : CAMERAPOSITION;

#if DEPTH_TEST
float2 ViewportSize : VIEWPORTSIZE;
DECLARE_UNIFORM_GBUFFER(GBuffer0, 0);
float CameraFar : CAMERAFAR;
#endif


//--------------------------------------------------------
// Material Parameters
//--------------------------------------------------------

texture MatcapTexture;
sampler MatcapSampler = sampler_state
{
  Texture = <MatcapTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MINFILTER = LINEAR;
  MAGFILTER = LINEAR;
  MIPFILTER = LINEAR;
};

#if NORMAL_MAP
DECLARE_UNIFORM_NORMALTEXTURE
#endif

#if SKINNING
float4x3 Bones[72] : BONES;
#endif


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION0;
#if NORMAL_MAP
  float2 TexCoord : TEXCOORD0;
#endif
  float3 Normal : NORMAL0;
#if NORMAL_MAP
  float3 Tangent : TANGENT0;
  float3 Binormal : BINORMAL0;
#endif
#if SKINNING
  uint4 BoneIndices : BLENDINDICES0;
  float4 BoneWeights : BLENDWEIGHT0;
#endif
};

struct VSOutput
{
  float4 Position : SV_Position;
#if NORMAL_MAP
  float2 TexCoord : TEXCOORD0;
#endif
  float3 Normal : TEXCOORD1;
#if NORMAL_MAP
  float3 Tangent : TEXCOORD2;
  float3 Binormal : TEXCOORD3;
#endif
#if DEPTH_TEST
  float4 PositionProj : TEXCOORD4;
  float Depth : TEXCOORD5;
#endif
};


//-----------------------------------------------------------------------------
// Shaders
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input, float4x4 world)
{
  VSOutput output = (VSOutput)0;
#if NORMAL_MAP
  output.TexCoord = input.TexCoord;
#endif
#if SKINNING
  float4x3 skinningMatrix = 0;
  skinningMatrix += Bones[input.BoneIndices.x] * input.BoneWeights.x;
  skinningMatrix += Bones[input.BoneIndices.y] * input.BoneWeights.y;
  skinningMatrix += Bones[input.BoneIndices.z] * input.BoneWeights.z;
  skinningMatrix += Bones[input.BoneIndices.w] * input.BoneWeights.w;
  input.Position.xyz = mul(input.Position, skinningMatrix);
  input.Normal.xyz = mul(input.Normal, (float3x3)skinningMatrix);
#if NORMAL_MAP
  input.Tangent.xyz = mul(input.Tangent, (float3x3)skinningMatrix);
  input.Binormal.xyz = mul(input.Binormal, (float3x3)skinningMatrix);
#endif
#endif
  
  float4 position = mul(input.Position, world);
  float4 positionView = mul(position, View);
  output.Position = mul(positionView, Projection);
  
#if DEPTH_TEST
  output.PositionProj = output.Position;
  output.Depth = -positionView.z/CameraFar;
#endif
  float4x4 worldView = mul(world, View);
  output.Normal =  mul(input.Normal, (float3x3)worldView);
#if NORMAL_MAP
  output.Tangent = mul(input.Tangent, (float3x3)worldView);
  output.Binormal = mul(input.Binormal, (float3x3)worldView);
#endif
  
  return output;
}


VSOutput VSNoInstancing(VSInput input)
{
  return VS(input, World);
}


VSOutput VSInstancing(VSInput input, float4x4 world : BLENDWEIGHT)
{
  return VS(input, transpose(world));
}


float4 PS(VSOutput input) : COLOR
{
#if DEPTH_TEST
  // Get the screen space texture coordinate for this position.
  float2 texCoordScreen = ProjectionToScreen(input.PositionProj, ViewportSize);
  float4 gBuffer0Sample = tex2Dlod(GBuffer0Sampler, float4(texCoordScreen, 0, 0));
  clip(GetGBufferDepth(gBuffer0Sample) - input.Depth);
#endif
  
  // Normalize tangent space vectors.
  float3 normal = normalize(input.Normal);
#if NORMAL_MAP
  float3 tangent = normalize(input.Tangent);
  float3 binormal = normalize(input.Binormal);
  
  // Normals maps are encoded using DXT5nm.
  float3 normalMapSample = GetNormalDxt5nm(NormalSampler, input.TexCoord); // optional: multiply with "Bumpiness" factor.
  normal = normal * normalMapSample.z + tangent * normalMapSample.x - binormal * normalMapSample.y;
#endif
  
  // Convert normal vector to matcap texture coordinate.
  float2 uv = float2(normal.x, -normal.y) * 0.5 + 0.5;
  float4 color = tex2D(MatcapSampler, uv);
  return color;
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
#if TWO_SIDED
  pass BackSides
  {
    CullMode = CW;
    VertexShader = compile VSTARGET VSNoInstancing();
    PixelShader = compile PSTARGET PS();
  }
#endif
  
  pass FrontSides
  {
    CullMode = CCW;
    VertexShader = compile VSTARGET VSNoInstancing();
    PixelShader = compile PSTARGET PS();
  }
}
