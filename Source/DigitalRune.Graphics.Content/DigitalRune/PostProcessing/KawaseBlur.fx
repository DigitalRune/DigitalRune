//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file KawaseBlur.fx
/// Performs a blur using the method of the Kawase bloom filter.
// 
// See "Wolfgang Engel: Programming Vertex and Pixel Shaders, Fake HDR".
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// 0 to not use half-pixel offset. 1 to use a half-pixel offset while sampling.
// Use 1 if the sampler uses bilinear filtering.
// For point filtering, 0 gives better results.
float UseHalfPixelOffset;

// The number of the current iteration.
// If a half-pixel offset is used, the values 0 ... (n - 1) should be set.
// If no half-pixel offset is used, the values 1 ... n should be set.
float Iteration;

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
  float4 Position : SV_Position;
};

struct PSInput
{
  float2 TexCoord0 : TEXCOORD0;
  float2 TexCoord1 : TEXCOORD1;
  float2 TexCoord2 : TEXCOORD2;
  float2 TexCoord3 : TEXCOORD3;
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
  output.TexCoord0 = input.TexCoord;
  
  // Compute offset. We optionally start at (0.5, 0.5) and increase the
  // offset by 1 texel for each iteration.
  float2 offset = UseHalfPixelOffset * 0.5 / ViewportSize + 1 / ViewportSize * Iteration;
  
  // Compute texture coordinates.
  output.TexCoord0 = input.TexCoord + offset;
  output.TexCoord1 = input.TexCoord + float2(-offset.x,  offset.y);
  output.TexCoord2 = input.TexCoord + float2(-offset.x, -offset.y);
  output.TexCoord3 = input.TexCoord + float2( offset.x, -offset.y);
  
  return output;
}


// Takes 4 diagonal samples and averages them.
float4 PS(PSInput input) : COLOR
{
  float4 color = 0.25 * (tex2D(SourceSampler, input.TexCoord0)
                         + tex2D(SourceSampler, input.TexCoord1)
                         + tex2D(SourceSampler, input.TexCoord2)
                         + tex2D(SourceSampler, input.TexCoord3));
  return color;
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
