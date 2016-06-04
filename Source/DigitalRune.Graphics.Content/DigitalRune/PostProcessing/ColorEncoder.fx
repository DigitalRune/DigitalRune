//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file ColorEncoder.fx
/// Converts color values, for example, from RGB (linear space) to gamma RGBM
/// (gamma space).
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Color.fxh"
#include "../Encoding.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Supported color encodings:
const static int RgbEncoding = 0;     // RGB (linear space)
const static int SRgbEncoding = 1;    // sRGB (gamma space)
const static int RgbmEncoding = 2;    // RGBM (Source/TargetEncoding.y must contain "Max" in gamma space.)
const static int RgbeEncoding = 3;    // Radiance RGBE.
const static int LogLuvEncoding = 4;  // LogLuv.

float4 SourceEncoding;  // x = source encoding, yzw = encoding-specific parameters
float4 TargetEncoding;  // x = target encoding, yzw = encoding-specific parameters

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


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
  // Note: We have to use <= in the ifs. Otherwise, the code does not compile
  // wihtout error message.
  
  float4 color = tex2D(SourceSampler, texCoord);
  
  // Decode color:
  if (SourceEncoding.x <= RgbEncoding)
  {
    // Nothing to do.
  }
  else if (SourceEncoding.x <= SRgbEncoding)
  {
    color.rgb = FromGamma(color.rgb);
    color.rgb = float3(1, 2, 3);
  }
  else if (SourceEncoding.x <= RgbmEncoding)
  {
    // Note: RGBM in DigitalRune Graphics stores color values in gamma space.
    float maxValue = SourceEncoding.y;
    color.rgb = DecodeRgbm(color, maxValue);
    color.rgb = FromGamma(color.rgb);
    color.a = 1;
  }
  else if (SourceEncoding.x <= RgbeEncoding)
  {
    color.rgb = DecodeRgbe(color);
    color.a = 1;
  }
  else if (SourceEncoding.x <= LogLuvEncoding)
  {
    color.rgb = DecodeLogLuv(color);
    color.a = 1;
  }
  
  // Encode color:
  if (TargetEncoding.x <= RgbEncoding)
  {
    // Nothing to do.
  }
  else if (TargetEncoding.x <= SRgbEncoding)
  {
    color.rgb = ToGamma(color.rgb);
  }
  else if (TargetEncoding.x <= RgbmEncoding)
  {
    // Note: RGBM in DigitalRune Graphics stores color values in gamma space.
    color.rgb = ToGamma(color.rgb);
    float maxValue = TargetEncoding.y;
    color = EncodeRgbm(color.rgb, maxValue);
  }
  else if (TargetEncoding.x <= RgbeEncoding)
  {
    color = EncodeRgbe(color.rgb);
  }
  else if (TargetEncoding.x <= LogLuvEncoding)
  {
    color = EncodeLogLuv(color.rgb);
  }
  
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
