//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SepiaFilter.fx
/// Recolors the given texture in sepia.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The strength of the effect (expected range [0, 1]).
float Strength;

// The input texture.
texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
};

// Color conversion matrix.
static const float4x4 ColorToSepiaMatrix = float4x4(0.393, 0.769, 0.189, 0,
                                                    0.349, 0.686, 0.168, 0,
                                                    0.272, 0.534, 0.131, 0,
                                                    0,     0,     0,     1);


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


// Fully convert image to sepia.
float4 PSFull(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  
  // Random formula from the interwebs.
  return mul(ColorToSepiaMatrix, color);
  
  // Another random formula from the interwebs.
  //float luminance = dot(color, LuminanceWeightsRec601);
  //return float4(luminance, luminance * 0.8,  luminance * 0.6, 1);
  
  // See "Game Programming Gems 4: Fast Sepia Tone Conversion"
  //float luminance = dot(color, LuminanceWeightsRec601);
  //return float4(luminance + 0.191,
                //luminance - 0.054,
                //luminance - 0.221,
                //1);
}


// Linearly interpolate between source and sepia image.
float4 PSPartial(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  return lerp(color, mul(ColorToSepiaMatrix, color), Strength);
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
  pass Full
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFull();
  }
  
  pass Partial
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSPartial();
  }
}
