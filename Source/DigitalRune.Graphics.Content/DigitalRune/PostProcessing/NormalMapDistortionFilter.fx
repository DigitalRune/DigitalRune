//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file NormalMapDistortionFilter.fx
/// Samples a texture; a normal map is used to offset the texture coordinates
/// to create a distortion.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// Scale applied to the texture coordinates for the normal map lookup.
float2 Scale;

// Offset applied to the texture coordinates for the normal map lookup.
// (Can be varied to create an animation).
float2 Offset;

// The strength of the effect.
float2 Strength;

// The input texture (usually the 3D scene that should be distorted).
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
{
  Texture = <SourceTexture>;
};

// Normal map for distorting the texture coordinates.
texture NormalMap;
sampler NormalMapSampler : register(s1) = sampler_state
{
  Texture = <NormalMap>;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float2 TexCoord : TEXCOORD;
};

struct VSOutput
{
  float2 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSBasic(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Sample normal map and use xy as an offset when the scene is sampled.
  float2 offset = tex2D(NormalMapSampler, texCoord * Scale + Offset).xy * 2 - 1;
  return tex2D(SourceSampler, texCoord + offset * Strength);
}


float4 PSBlur4(float2 texCoord : TEXCOORD0) : COLOR0
{
  float2 offset = tex2D(NormalMapSampler, texCoord * Scale + Offset).xy * 2 - 1;
  texCoord += offset * Strength;
  
  // Take 5 samples around the displaced sample location to create a filtered result.
  offset = 1 / ViewportSize;
  float4 color0 = tex2D(SourceSampler, texCoord);
  float4 color1 = tex2D(SourceSampler, texCoord + float2(-1.5f * offset.x, -0.5f * offset.y)) * 4;
  float4 color2 = tex2D(SourceSampler, texCoord + float2(-0.5f * offset.x, +1.5f * offset.y)) * 4;
  float4 color3 = tex2D(SourceSampler, texCoord + float2(+1.5f * offset.x, +0.5f * offset.y)) * 4;
  float4 color4 = tex2D(SourceSampler, texCoord + float2(+0.5f * offset.x, -1.5f * offset.y)) * 4;
  return (color0 + color1 + color2 + color3 + color4) / 17;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_2_0
#define PSTARGET ps_2_0
#else
#define VSTARGET vs_4_0_level_9_1
#define PSTARGET ps_4_0_level_9_1
#endif

technique
{
  pass Basic
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBasic();
  }
  
  pass Blur4
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSBlur4();
  }
}
