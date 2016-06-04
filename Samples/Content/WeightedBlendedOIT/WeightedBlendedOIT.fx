//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file WeighedBlendedOIT.fx
/// Implements the final pass for Weighted Blended Order-Independent
/// Transparency (WBOIT). The render targets A, B storing the transparent
/// objects are combined with the scene.
//
//-----------------------------------------------------------------------------

#include "../../../Source/DigitalRune.Graphics.Content/DigitalRune/Common.fxh"


// The viewport size in pixels.
float2 ViewportSize;

texture2D TextureA;
sampler SamplerA = sampler_state { Texture = <TextureA>; };

texture2D TextureB;
sampler SamplerB = sampler_state { Texture = <TextureB>; };


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


VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PS(float2 texCoord : TEXCOORD0) : COLOR0
{
  float4 accum = tex2D(SamplerA, texCoord);
  float r = accum.a;
  accum.a = tex2D(SamplerB, texCoord).r;
  
  float4 ci = float4(accum.rgb / clamp(accum.a, 1e-4, 5e4), r); // Original paper
  //float4 ci = float4(accum.rgb / max(accum.a, 1e-5), r);      // NVIDIA example
  
  // Gamma correction.
  ci.rgb = ToGamma(ci.rgb);
  
  return ci;
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
