//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file HorizontalBlur.fx
/// Performs a horizontal blur.
// 
// (The used method is similar to Shader X3, A steerable streak filter.)
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"

#pragma warning( disable : 3571 )      // pow(f, e) - pow will not work for negative f.


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The zero-based number of the current iteration.
float Iteration;

// The number of samples per iteration per side.
// (The total number of samples per iteration is twice this value.)
int NumberOfSamples = 4;

//uniform const float Attenuation = 0.95;

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


float4 PS(float2 texCoord : TEXCOORD0) : COLOR
{
  float b = pow(NumberOfSamples, Iteration);
  
  float halfTexelOffset = 0.5 / ViewportSize.x;
  float texelOffset = 1.0 / ViewportSize.x;
  //float weightSum = 0;
  float4 color = 0;
  for (int s = 0; s < NumberOfSamples; s++)
  {
    // ----- Original:
    //float weight = saturate(pow(Attenuation, b * (s + 1)));
    //float offset =  b * (s + 1) / ViewportSize.x;
    //color += saturate(weight) * tex2D(SourceSampler, texCoord + float2(offset, 0));
    //color += saturate(weight) * tex2D(SourceSampler, texCoord - float2(offset, 0));
    //weightSum += weight * 2;
    
    float offset = b * halfTexelOffset + b * s * texelOffset;
    color += tex2D(SourceSampler, texCoord + float2(offset, 0));
    color += tex2D(SourceSampler, texCoord - float2(offset, 0));
  }
  
  return color / color.a;     // The sum of the alpha values is equal to the number of samples!
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
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
