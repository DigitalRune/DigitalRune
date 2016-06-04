//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file MadFilter.fx
/// Multiply-and-Add processor. The color (RGB) is scaled and an offset is
/// added.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// Per-component factor and offset.
float3 Scale;
float3 Offset;

// The input texture.
texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
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


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  color.rgb = color.rgb * Scale + Offset;
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

technique
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
