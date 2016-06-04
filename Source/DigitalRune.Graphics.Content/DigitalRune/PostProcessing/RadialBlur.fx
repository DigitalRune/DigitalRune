//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file RadialBlur.fx
/// Creates a radial blur for a "high speed effect".
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The viewport size in pixels.
float2 ViewportSize;

// The number of samples to take.
int NumberOfSamples = 8;

// The radius in the range [0, 1] where the maximal blur is reached.
//   0 ... full blur starts in the viewport center.
//   1 ... full blur is reached at viewport border.
float MaxBlurRadius = 1;

// The range of texels blurred at MaxBlurRadius relative to the viewport.
//   0 ... range = 0, no blur
//   1 ... range = viewport extent, max blur
float MaxBlurAmount = 0.04; // 4% of viewport

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
  // Vector from the current pixel to the screen center.
  float2 radiusVector = texCoord - float2(0.5, 0.5);
  
  // The radius (distance from center to current pixel).
  float r = length(radiusVector) + 0.00001f;
  
  // Normalize radiusVector.
  radiusVector /= r;
  
  // Multiply radius by 2, so that 0 = center and 1 = screen border.
  // Then scale it so that r represents the normalized blur intensity.
  // Clamp the result to the range [0, 1].
  //   0 = center (no blur)
  //   1 = MaxBlurRadius (max blur).
  r = 2 * r / MaxBlurRadius;
  r = saturate(r);
  
  // Compute the delta (offset) between the samples.
  float2 delta = radiusVector       // Normalized vector in blur direction
                 * r * r            // A function that is 0 in the center and 1 at the MaxBlurRadius.
                 * MaxBlurAmount    // The range of texels we want to blur at MaxBlurRadius (value is relative to ViewportSize).
                 / NumberOfSamples; // The number of samples to take in the range.
  
  // Positive delta would be correct. But that would try to sample outside
  // the screen :-(. Therefore we sample from within. This is not noticable
  // and avoids artifacts at the screen border.
  delta = -delta;
  
  // Blur in velocity direction.
  float4 color = float4(0, 0, 0, 0);
  for (int i = 0; i < NumberOfSamples; i++)
  {
    color += tex2D(SourceSampler, texCoord);
    texCoord += delta;
  }
  color /= NumberOfSamples;
  return color;
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
