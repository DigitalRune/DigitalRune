//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Fxaa.fx
/// Applies Fast approXimate Anti-Aliasing (FXAA).
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"

#define FXAA_PC 1
#define FXAA_HLSL_3 1
#define FXAA_QUALITY__PRESET 12
//#define FXAA_GREEN_AS_LUMA 1   // If we use this we can skip the luma pass.
#include "../Fxaa3_11.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

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


float4 PSLuminanceToAlpha(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  
  // Color should be tonemapped and non-linear.
  //color.rgb = ToneMap(color.rgb);   // linear color output
  //color.rgb = sqrt(color.rgb);      // gamma 2.0 color output
  // According to the FXAA author, results are better if luminance is computed
  // from non-linear RGB - although normally luminance is computed from linear
  // RGB.
  color.a = dot(color.rgb, LuminanceWeightsRec601); // compute luminance
  return color;
}


float4 PSFxaa(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 notUsed = float4(0, 0, 0, 0);
  return FxaaPixelShader(
    texCoord.xy,
    notUsed,
    SourceSampler,
    SourceSampler,
    SourceSampler,
    1 / ViewportSize,
    notUsed, // float4(-0.5 / ViewportSize.x, -0.5 / ViewportSize.y, 0.5 / ViewportSize.x, 0.5 / ViewportSize.y),
    notUsed, // float4(-2 / ViewportSize.x, -2 / ViewportSize.y, 2 / ViewportSize.x, 2 / ViewportSize.y),
    notUsed, // float4(8 / ViewportSize.x, 8 / ViewportSize.y, -4 / ViewportSize.x, -4 / ViewportSize.y),
    0.75,
    0.166,
    0.0625,
    8,
    0.125,
    0.05,
    notUsed);
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

#if !SM4
#define VSTARGET vs_3_0
#define PSTARGET ps_3_0
#else
#define VSTARGET vs_4_0
#define PSTARGET ps_4_0
#endif

technique
{
  pass LuminanceToAlpha
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSLuminanceToAlpha();
  }
  
  pass Fxaa
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFxaa();
  }
}
