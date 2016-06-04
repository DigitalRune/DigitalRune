//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Skybox.fx
/// Draws a skybox with a cube map for the scene background.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Encoding.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Supported color encodings:
const static int RgbEncoding = 0;     // RGB (linear space)
const static int SRgbEncoding = 1;    // sRGB (gamma space)
const static int RgbmEncoding = 2;    // RGBM (RgbmMax needs to be set.)

// Not yet implemented:
//const static int RgbeEncoding = 3;    // Radiance RGBE.
//const static int LogLuvEncoding = 4;  // LogLuv.

float RgbmMax;   // RGBM max value in gamma space.

float4x4 WorldViewProjection;

// A tint color or scale factor (including alpha).
float4 Color;

int TextureSize;
textureCUBE Texture;
samplerCUBE TextureSampler = sampler_state
{
  Texture = <Texture>;
};


//-----------------------------------------------------------------------------
// Structures
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
};

struct VSOutput
{
  float3 TexCoord : TEXCOORD;
  float4 Position : SV_Position;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  
  // The cube map texture coordinate is equal to the local position because
  // the cube is centered at the local space origin.
  output.TexCoord = input.Position.xyz;
  
  // Transform local position to projection space.
  // And set z coordinate to w value. Thus, after perspective divide z will be 1
  // and the pixels are drawn behind everything else.
  output.Position = mul(float4(input.Position.xyz, 0), WorldViewProjection).xyww;
  
  return output;
}


float4 PS(float3 texCoord : TEXCOORD0,
          uniform const int inputEncoding,
          uniform const int outputEncoding) : COLOR
{
  float4 color = 0;
  if (inputEncoding != RgbEncoding)
  {
    color = texCUBE(TextureSampler, texCoord);
  }
  else
  {
    // Get tex coords from range [-1, 1] to range [-TextureSize, TextureSize].
    texCoord = texCoord * TextureSize;
    
    // ShaderX2 and Engel add a half pixel offset, but creates bad artifacts :-(
    //texCoord += float3(0.5, 0.5, 0.5);
    
    // Clamp to an integer coordinate.
    float3 tc0 = floor(texCoord);
    // Get a second integer coordinate.
    float3 tc1 = tc0 + float3(1.0, 1.0, 1.0);
    
    // Sample at the 8 possible positions made up of tc0 and tc1.
    float4 c000 = texCUBE(TextureSampler, float3(tc0.x, tc0.y, tc0.z));
    float4 c001 = texCUBE(TextureSampler, float3(tc0.x, tc0.y, tc1.z));
    float4 c010 = texCUBE(TextureSampler, float3(tc0.x, tc1.y, tc0.z));
    float4 c011 = texCUBE(TextureSampler, float3(tc0.x, tc1.y, tc1.z));
    float4 c100 = texCUBE(TextureSampler, float3(tc1.x, tc0.y, tc0.z));
    float4 c101 = texCUBE(TextureSampler, float3(tc1.x, tc0.y, tc1.z));
    float4 c110 = texCUBE(TextureSampler, float3(tc1.x, tc1.y, tc0.z));
    float4 c111 = texCUBE(TextureSampler, float3(tc1.x, tc1.y, tc1.z));
    
    // Lerp all results together similar to 2D bilinear filtering.
    float3 p = frac(texCoord);
    color = lerp(lerp(lerp(c000, c010, p.y),
                      lerp(c100, c110, p.y),
                      p.x),
                 lerp(lerp(c001, c011, p.y),
                      lerp(c101, c111, p.y),
                      p.x),
                 p.z);
  }
  
  if (inputEncoding == RgbEncoding)
  {
    // Nothing to do.
  }
  else if (inputEncoding == SRgbEncoding)
  {
    color.rgb = FromGamma(color.rgb);
  }
  else // if (inputEncoding == RgbmEncoding)
  {
    // Note: RGBM in DigitalRune Graphics stores color values in gamma space.
    color.rgb = DecodeRgbm(color, RgbmMax);
    color.rgb = FromGamma(color.rgb);
    color.a = 1;
  }
  
  color *= Color;
  color.rgb *= Color.a;   // Premultiplied alpha!
  
  if (outputEncoding == SRgbEncoding)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}
float4 PS_RgbToRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, RgbEncoding, RgbEncoding); }
float4 PS_SRgbToRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, SRgbEncoding, RgbEncoding); }
float4 PS_RgbmToRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, RgbmEncoding, RgbEncoding); }
float4 PS_RgbToSRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, RgbEncoding, SRgbEncoding); }
float4 PS_SRgbToSRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, SRgbEncoding, SRgbEncoding); }
float4 PS_RgbmToSRgb(float3 texCoord : TEXCOORD0) : COLOR { return PS(texCoord, RgbmEncoding, SRgbEncoding); }


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
  pass RgbToRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_RgbToRgb();
  }
  pass SRgbToRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_SRgbToRgb();
  }
  pass RgbmToRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_RgbmToRgb();
  }
  pass RgbToSRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_RgbToSRgb();
  }
  pass SRgbToSRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_SRgbToSRgb();
  }
  pass RgbmToSRgb
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS_RgbmToSRgb();
  }
}
