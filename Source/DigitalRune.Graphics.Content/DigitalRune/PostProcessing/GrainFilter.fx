//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Grain.fx
/// Adds film grain.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"
#include "../Noise.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The size of the viewport in pixels.
float2 ViewportSize;

// A time dependent value used to animate grain.
float Time = 0;

// The strength of the grain effect.
float Strength = 0.1f;

float GrainScale = 1;

// The luminance threshold.
// (Pixels brighter than this value are not affected by noise.)
float LuminanceThreshold;

// The scene texture.
texture SourceTexture;
sampler SourceSampler : register(s0) = sampler_state
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


float4 PSNoise(float2 texCoord : TEXCOORD0, bool scaleWithLuminance) : COLOR0
{
  float4 color = tex2D(SourceSampler, texCoord);
  
  // Apply GrainScale.
  texCoord.x = (int)((texCoord.x * ViewportSize.x) / GrainScale) / ViewportSize.x;
  texCoord.y = (int)((texCoord.y * ViewportSize.y) / GrainScale) / ViewportSize.y;
  
  // Note: We need to use saturate(tc) because otherwise some pixels might flicker -
  // which must be a graphics card/driver bug...
  float grain = Noise1(saturate(texCoord) + frac(Time));
  
  float scale = Strength;
  
  if (scaleWithLuminance)
  {
    float luminance = dot(color.rgb, LuminanceWeights);
    
    // Dark pixels should get more noise.
    float luminanceFactor = saturate((LuminanceThreshold - luminance) / LuminanceThreshold);
    
    //luminanceFactor = pow(0.000001 + luminanceFactor, Power);
    //luminanceFactor = luminanceFactor * luminanceFactor;
    
    scale *= luminanceFactor;
  }
  
  // ----- Method 1: Lerp between scene and noise.
  // If we use 2 * grain, then the average scene brightness stays the same.
  //return lerp(color, color * 2 * grain, Strength);
  
  // ----- Optimized method 1:
  //grain = 2 * grain - 1;  // Scale from [0, 1] to [-1, 1]
  //return color * (1 - grain * scale);
  
  // Method 2: Add noise.
  grain = 2 * grain - 1;  // Scale from [0, 1] to [-1, 1]
  return color + grain * scale;
}
float4 PSEqualNoise(float2 texCoord : TEXCOORD0) : COLOR0 { return PSNoise(texCoord, false); }
float4 PSScaledNoise(float2 texCoord : TEXCOORD0) : COLOR0 { return PSNoise(texCoord, true); }


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
  pass EqualNoise
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSEqualNoise();
  }
  
  pass ScaledNoise
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSScaledNoise();
  }
}
