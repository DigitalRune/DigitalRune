//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file SharpeningFilter.fx
/// Applies a sharpening effect using the Laplacian operator (edge detector).
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The sharpness factor in the range [0, +inf[.
//   Sharpness = 0: original image
//   Sharpness > 0: sharpened image.
float Sharpness = 0.5;

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
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float2 TexCoord2 : TEXCOORD2;
  float2 TexCoord3 : TEXCOORD3;
  float2 TexCoord4 : TEXCOORD4;
  float4 Position : SV_Position;
};

struct PSInput
{
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float2 TexCoord2 : TEXCOORD2;
  float2 TexCoord3 : TEXCOORD3;
  float2 TexCoord4 : TEXCOORD4;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

// Transforms the screen space position to standard projection space and
// prepares the texture coordinates for the pixel shader.
VSOutput VS(VSInput input)
{
  VSOutput output = (VSOutput)0;
  
  // Transform screen space position to standard projection space.
  output.Position = ScreenToProjection(input.Position, ViewportSize);
  
  // Compute sample offset (1 texel).
  float2 offset = 1 / ViewportSize;
  
  // Compute texture coordinates and pack them into texCoord0 and texCoord1.
  output.TexCoord0 = input.TexCoord;
  output.TexCoord1 = input.TexCoord + float2(0, -offset.y);
  output.TexCoord2 = input.TexCoord + float2(-offset.x, 0);
  output.TexCoord3 = input.TexCoord + float2(offset.x, 0);
  output.TexCoord4 = input.TexCoord + float2(0,offset.y);
  
  return output;
}


float4 PS(PSInput input) : COLOR
{
  // Sample the image 5 times.
  float4 center = tex2D(SourceSampler, input.TexCoord0);
  float4 top = tex2D(SourceSampler, input.TexCoord1);
  float4 left = tex2D(SourceSampler, input.TexCoord2);
  float4 right = tex2D(SourceSampler, input.TexCoord3);
  float4 bottom = tex2D(SourceSampler, input.TexCoord4);
  
  // Apply the negative Laplacian operator to detect edges.
  float4 edge = 4 * center - top - left - right - bottom;
  
  return center + Sharpness * edge;
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
  pass
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PS();
  }
}
