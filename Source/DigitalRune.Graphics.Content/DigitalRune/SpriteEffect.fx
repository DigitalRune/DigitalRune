//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SpriteEffect.fx
/// Similar to XNA's sprite effect, except that the tint color is not clamped
/// and the pixel shaders uses sRGB reads.
//
//-----------------------------------------------------------------------------

#include "Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

texture2D Texture;
sampler TextureSampler : register(s0) = sampler_state
{
  Texture = <Texture>;
};

float4x4 Transform;


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct VSInput
{
  float4 Position : POSITION;
  float4 Color : COLOR;
  float2 TexCoord : TEXCOORD;
};


// Note: For pixel shader versions ps_1_1 - ps_2_0, diffuse and specular colors are
// saturated (clamped) in the range 0 to 1 before use by the shader.
// --> Color (HDR!) needs to be passed as TEXCOORD instead of COLOR!
struct VSOutput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;
  float4 Position : SV_Position;
};


struct PSInput
{
  float2 TexCoord : TEXCOORD0;
  float4 Color : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  
  output.Position = mul(input.Position, Transform);
  output.Color = input.Color;
  output.TexCoord = input.TexCoord;
  
  return output;
}


float4 PS(PSInput input, uniform const bool gammaCorrection) : SV_Target0
{
  float4 color = tex2D(TextureSampler, input.TexCoord);
  
  // Convert from sRGB to linear space.
  color.rgb = FromGamma(color.rgb);
  
  // Apply tint color.
  color *= input.Color;
  
  if (gammaCorrection)
    color.rgb = ToGamma(color.rgb);
  
  return color;
}


float4 PS(PSInput input) : SV_Target0 { return PS(input, false); }
float4 PSWithGamma(PSInput input) : SV_Target0 { return PS(input, true); }


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

technique Sprite
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}


technique SpriteWithGamma
{
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSWithGamma();
  }
}
