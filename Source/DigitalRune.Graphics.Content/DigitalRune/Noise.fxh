//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Noise.fxh
/// GPU-based noise functions.
///
/// If the Dither function results are used for screen door transparency, use
/// it like this:
///   clip(Alpha - Dither(pixel pos));
///
/// When using screen door transparency for blending LODs, one LOD needs to use
///   clip(Alpha - Dither(pixel pos));
/// and the other LOD needs to use the inverse pattern
///   clip(Alpha - (1 - Dither(pixel pos)));
///
/// To use the method Dither16x16(), you need to define
///   #define USE_DITHER16X16 1
/// before including Noise.fxh.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_NOISE_FXH
#define DIGITALRUNE_NOISE_FXH

// The DitherNxN methods can be used for "ordered dithering".
// The threshold matrix is also known as "index matrix" or "Bayer matrix".
// Reference: http://en.wikipedia.org/wiki/Ordered_dithering)

/// Returns a value in the range [0, 1] of a regular 2x2 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither2x2(float2 p)
{
  const float2x2 DitherMatrix =
  {
    1.0 / 5.0, 3.0 / 5.0,
    4.0 / 5.0, 2.0 / 5.0
  };
  return DitherMatrix[p.x % 2][p.y % 2];
}


/// Returns a value in the range [0, 1] of a regular 3x3 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither3x3(float2 p)
{
  const float3x3 DitherMatrix =
  {
    3.0 / 10.0, 7.0 / 10.0, 4.0 / 10.0,
    6.0 / 10.0, 1.0 / 10.0, 9.0 / 10.0,
    2.0 / 10.0, 8.0 / 10.0, 5.0 / 10.0,
  };
  return DitherMatrix[p.x % 3][p.y % 3];
}


/// Returns a value in the range [0, 1] of a regular 4x4 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither4x4(float2 p)
{
  static const float4x4 DitherMatrix =
  {
     1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
    13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
     4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
    16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
  };
  return DitherMatrix[p.x % 4][p.y % 4];
}


#if USE_DITHER16X16
texture DitherTexture : DITHERMAP;
sampler DitherSampler = sampler_state
{
  Texture = <DitherTexture>;
  AddressU  = WRAP;
  AddressV  = WRAP;
  MinFilter = POINT;
  MagFilter = POINT;
  MipFilter = NONE;
};


/// Returns a value in the range [0, 1] of a 16x16 dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither16x16(float2 p)
{
  // The dither map contains values 0 - 1 (encoded as byte).
  // The 16x16 dither values need to be in the range 1 / 257 - 256 / 257.
  return tex2D(DitherSampler, p / 16).a * 255 / 257 + 1 / 257;
}
#endif


/// Returns a value in the range [0, 1] of a regular dither pattern.
/// \param[in] p   The (x, y) position in pixels.
/// \return A value in the range [0, 1].
float Dither1(float2 p)
{
  // By changing the constants, we can create different patterns.
  // (The second constant needs to be negative).

  // Just Cause 2 constants (GPU Pro 2).
  float2 r = { 0.782934, -0.627817 }; 
  
  // Torque engine constants.
  //float2 r = { 0.350, -0.916 };     // Vertical gradiants with offsets.
  //float2 r = { 0, -0.916 };         // Horizontal blinds
  //float2 r = { 78.233, -12.9898 };  // Long vertical gradiants with offsets.
  
  return frac(dot(p, r));
  
  // Attention! This function may return 0 - whereas the other Dither function
  // always return a value greater than 0. If you don't want 0, try this:
  //return 0.99 * frac(dot(p, r)) + 0.001;
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Three values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise1(float2 p)
{
  return frac(sin(dot(p.xy, float2(12.9898, 78.233)))* 43758.5453);
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Two values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise2(float2 p)
{
  // The noise is a modification of the noise algorithm of Pat 'Hawthorne' Shearon.
  
  float a = p.x * p.y * 50000.0;
  a = fmod(a, 13);
  a = a * a;
  
  float randomA = fmod(a, 0.01) * 100;
  return randomA;
}


/// Returns a "random" value in the range [0, 1].
/// \param[in] p   Three values in the range [0, 1].
/// \return A value in the range [0, 1].
float Noise2(float3 p)
{
  // The noise is a modification of the noise algorithm of Pat 'Hawthorne' Shearon.
  
  float a = p.x * p.y * 50000.0;
  a = fmod(a, 13);
  a = a * a;
  
  float z = abs(p.z);  // z should be around [0, 1].
  float b = a * z + z;
  float randomB = fmod(b, 0.01) * 100;
  
  return abs(randomB);
}
#endif
