//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file LuminanceFilter.fx
/// Computes luminance information (min, average and max).
//
// Notes:
// We could also use a luminance history function [Tchou] to even out fast luminance
// changes (e.g. flashes). See HDR the Bungie Way.
//
//-----------------------------------------------------------------------------

#include "../Common.fxh"


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// The source image size in texels.
float2 SourceSize;

// The target image size in pixels.
float2 TargetSize;

// true if the average luminance is computed using the geometric mean;
// otherwise a normal average is used.
bool UseGeometricMean = false;

// true to use the old luminance to model the eye adjustment behavior.
bool UseAdaption = true;

// The time that has passes since the old luminance was computed.
float DeltaTime = 1.0 / 60.0;

// The speed of the eye adaption in the range [0, infinity[. Use
// small values like 0.02.
float AdaptionSpeed = 0.02;

// The input texture.
texture SourceTexture;
sampler SourceSampler = sampler_state
{
  Texture = <SourceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
};

// The luminance texture.
texture LastLuminanceTexture;
sampler LastLuminanceSampler = sampler_state
{
  Texture = <LastLuminanceTexture>;
  AddressU = CLAMP;
  AddressV = CLAMP;
  MagFilter = POINT;
  MinFilter = POINT;
  MipFilter = NONE;
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
  output.Position = ScreenToProjection(input.Position, TargetSize);
  output.TexCoord = input.TexCoord;
  return output;
}


float4 PSCreate(float2 texCoord : TEXCOORD0) : COLOR
{
  // Take 4 samples and compute the luminance (average, minimum and maximum).
  float2 offset = 0.5 / SourceSize;
  
  float4x4 samples;
  samples[0] = tex2D(SourceSampler, texCoord + float2(-offset.x, -offset.y));
  samples[1] = tex2D(SourceSampler, texCoord + float2(-offset.x,  offset.y));
  samples[2] = tex2D(SourceSampler, texCoord + float2( offset.x, -offset.y));
  samples[3] = tex2D(SourceSampler, texCoord + float2( offset.x,  offset.y));
  
  // Convert to luminance.
  samples[0].x = dot(samples[0].rgb, LuminanceWeights);
  samples[1].x = dot(samples[1].rgb, LuminanceWeights);
  samples[2].x = dot(samples[2].rgb, LuminanceWeights);
  samples[3].x = dot(samples[3].rgb, LuminanceWeights);
  
  // Errorneous shaders could create infinite or NaN values. If this happens the
  // whole screen will go black. The game will never recover once NaN or inf
  // is stored in the LastLuminanceTexture. --> Filter these errorneous pixels.
  // For debugging we can manually create invalid color values like this:
  //    x = 1 / (x * 1e-30);
  if (isinf(samples[0].x) || isnan(samples[0].x))    // Note: isfinite(x) does not work.
    samples[0].x = 1;
  if (isinf(samples[1].x) || isnan(samples[1].x))
    samples[1].x = 1;
  if (isinf(samples[2].x) || isnan(samples[2].x))
    samples[2].x = 1;
  if (isinf(samples[3].x) || isnan(samples[3].x))
    samples[3].x = 1;
  
  float minimum = min(samples[0].x, samples[1].x);
  minimum = min(minimum, samples[2].x);
  minimum = min(minimum, samples[3].x);
  
  float maximum = max(samples[0].x, samples[1].x);
  maximum = max(maximum, samples[2].x);
  maximum = max(maximum, samples[3].x);
  
  if (UseGeometricMean)
  {
    // We build the logarithmic sum (for the geometric mean/antilogarithm).
    // e^(...) is comuted in the Final pass.
    // We add a small epsilon because to avoid log(0) = -infinity at black pixels.
    const float epsilon = 0.0001;
    samples[0].x = log(samples[0].x + epsilon);
    samples[1].x = log(samples[1].x + epsilon);
    samples[2].x = log(samples[2].x + epsilon);
    samples[3].x = log(samples[3].x + epsilon);
  }
  
  float average = samples[0].x + samples[1].x + samples[2].x + samples[3].x;
  average /= 4;
  
  return float4(minimum, average, maximum, 1);
}


float4 PSDownsample(float2 texCoord : TEXCOORD0) : COLOR
{
  // Take 4 samples and compute the luminance (average, minimum and maximum).
  float2 offset = 0.5 / SourceSize;
  
  float4x4 samples;
  samples[0] = tex2D(SourceSampler, texCoord + float2(-offset.x, -offset.y));
  samples[1] = tex2D(SourceSampler, texCoord + float2(-offset.x,  offset.y));
  samples[2] = tex2D(SourceSampler, texCoord + float2( offset.x, -offset.y));
  samples[3] = tex2D(SourceSampler, texCoord + float2 (offset.x,  offset.y));
  
  float minimum = min(samples[0].x, samples[1].x);
  minimum = min(minimum, samples[2].x);
  minimum = min(minimum, samples[3].x);
  
  float average = samples[0].y + samples[1].y + samples[2].y + samples[3].y;
  average /= 4;
  
  float maximum = max(samples[0].z, samples[1].z);
  maximum = max(maximum, samples[2].z);
  maximum = max(maximum, samples[3].z);
  
  return float4(minimum, average, maximum, 1);
}


float4 PSFinal(float2 texCoord : TEXCOORD0) : COLOR
{
  // Take 4 samples and compute the luminance (average, minimum and maximum).
  float2 offset = 0.5 / SourceSize;
  
  float4x4 samples;
  samples[0] = tex2D(SourceSampler, texCoord + float2(-offset.x, -offset.y));
  samples[1] = tex2D(SourceSampler, texCoord + float2(-offset.x,  offset.y));
  samples[2] = tex2D(SourceSampler, texCoord + float2( offset.x, -offset.y));
  samples[3] = tex2D(SourceSampler, texCoord + float2 (offset.x,  offset.y));
  
  float minimum = min(samples[0].x, samples[1].x);
  minimum = min(minimum, samples[2].x);
  minimum = min(minimum, samples[3].x);
  
  float average = samples[0].y + samples[1].y + samples[2].y + samples[3].y;
  average /= 4;
  if (UseGeometricMean)
  {
    // Now finish the geometric mean by returning e^(logSum).
    average = exp(average);
  }
  
  float maximum = max(samples[0].z, samples[1].z);
  maximum = max(maximum, samples[2].z);
  maximum = max(maximum, samples[3].z);
  
  float4 luminance = float4(minimum, average, maximum, 1);
  
  if (UseAdaption)
  {
    // Adapt luminance with a delay.
    float4 oldLuminance = tex2D(LastLuminanceSampler, float2(0.5f, 0.5f));
    if (all(!isinf(oldLuminance) && !isnan(oldLuminance)))
      luminance = oldLuminance + (luminance - oldLuminance) * (1 - pow(max(0.000001, 1 - AdaptionSpeed), 30 * DeltaTime));
    
    // Adapt the luminance using Pattanaik's technique (mentioned in MJPs Bokeh sample).
    //luminance = oldLuminance + (luminance - oldLuminance) * (1 - exp(-DeltaTime * Tau));
    // Tau interpolates between adaption rate of rods and cones:
    // Tau = lerp(TauRods, TauCones, p); with TauRods = 0.2, TauCones = 0.4
  }
  
  return luminance;
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
  pass Create
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSCreate();
  }
  
  pass Downsample
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSDownsample();
  }
  
  pass Final
  {
    VertexShader = compile VSTARGET VS();
    PixelShader = compile PSTARGET PSFinal();
  }
}
