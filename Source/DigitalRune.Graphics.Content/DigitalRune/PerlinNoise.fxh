//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file PerlinNoise.fxh
/// GPU-based Perlin noise.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_PERLINNOISE_FXH
#define DIGITALRUNE_PERLINNOISE_FXH


//-----------------------------------------------------------------------------
// Pre-Computed Lookup Textures
//-----------------------------------------------------------------------------

// The optimized permutation texture for 3D noise.
texture PerlinPermutation3DTexture;
sampler PerlinPermutation3DSampler = sampler_state
{
  texture = <PerlinPermutation3DTexture>;
  AddressU  = WRAP;
  AddressV  = WRAP;
  MAGFILTER = POINT;
  MINFILTER = POINT;
  MIPFILTER = NONE;
};


// The optimized gradient texture for 3D noise
texture PerlinGradient3DTexture;
sampler PerlinGradient3DSampler = sampler_state
{
  texture = <PerlinGradient3DTexture>;
  AddressU  = WRAP;
  AddressV  = CLAMP;
  MAGFILTER = POINT;
  MINFILTER = POINT;
  MIPFILTER = NONE;
};


// The permutation texture for 4D noise.
texture PerlinPermutationTexture;
sampler PerlinPermutationSampler = sampler_state
{
  texture = <PerlinPermutationTexture>;
  AddressU  = WRAP;
  AddressV  = CLAMP;
  MAGFILTER = POINT;
  MINFILTER = POINT;
  MIPFILTER = NONE;
};


// The gradient texture for 4D noise.
texture PerlinGradient4DTexture;
sampler PerlinGradient4DSampler = sampler_state
{
  texture = <PerlinGradient4DTexture>;
  AddressU  = WRAP;
  AddressV  = CLAMP;
  MAGFILTER = POINT;
  MINFILTER = POINT;
  MIPFILTER = NONE;
};


//-----------------------------------------------------------------------------
// Improved Perlin 3D Noise
//-----------------------------------------------------------------------------

float4 PerlinPermutation3D(float2 p)
{
  return tex2D(PerlinPermutation3DSampler, p);
}


float PerlinGradient3D(float x, float3 p)
{
  return dot(tex1D(PerlinGradient3DSampler, x), p);
}


float3 PerlinFade(float3 t)
{
  return t * t * t * (t * (t * 6 - 15) + 10);
}


/// Computes Improved Perlin Noise (3D).
/// \param[in] p  A 3D vector.
/// \return The noise value.
/// \remarks
/// This method requires following lookup textures:
///   PerlinPermutation3DTexture, PerlinGradient3DTexture
float PerlinNoise3D(float3 p)
{
  float3 P = fmod(floor(p), 256.0);	  // Find unit cube that contains point.
  p -= floor(p);                      // Find relative x, y, z of point in cube.
  float3 f = PerlinFade(p);           // Compute PerlinFade curves for each of x, y, z.
  
  P = P / 256.0;
  const float one = 1.0 / 256.0;
  
  // Hash coordinates of the 8 cube corners
  float4 AA = PerlinPermutation3D(P.xy) + P.z;
  
  // and add blended results from 8 corners of cube.
  return
    lerp(
      lerp(
        lerp(
          PerlinGradient3D(AA.x, p),
          PerlinGradient3D(AA.z, p + float3(-1, 0, 0)),
          f.x),
        lerp(
          PerlinGradient3D(AA.y, p + float3(0, -1, 0)),
          PerlinGradient3D(AA.w, p + float3(-1, -1, 0)),
          f.x),
        f.y),
      lerp(
        lerp(
          PerlinGradient3D(AA.x+one, p + float3(0, 0, -1)),
          PerlinGradient3D(AA.z+one, p + float3(-1, 0, -1)),
          f.x),
        lerp(
          PerlinGradient3D(AA.y+one, p + float3(0, -1, -1)),
          PerlinGradient3D(AA.w+one, p + float3(-1, -1, -1)),
          f.x),
        f.y),
      f.z);
}


/// Computes Fractal Brownian Motion (fBm)
/// Combines several octaves of noise.
float FractalBrownianMotion(float3 p, int numberOfOctaves, float4 offsets)
{
  float lacunarity = 2.0;
  float gain = 0.5;
  
  float f = 1;      // Frequency
  float a = 0.5;    // Amplitude
  
  float result = 0;
  for (int i = 0; i < octaves; i++)
  {
    result += a * PerlinNoise3D(f * p + offsets[i%4]);
    f *= lacunarity;
    a *= gain;
  }
  return result;
}


/// Computes Fractal Brownian Motion (fBm)
/// Combines several octaves of the abs() of noise.
float Turbulence(float3 p, int numberOfOctaves, float4 offsets)
{
  float lacunarity = 2.0;
  float gain = 0.5;
  
  float f = 1;    // Frequency
  float a = 1;    // Amplitude
  
  float result = 0;
  for (int i = 0; i < numberOfOctaves; i++)
  {
    result += a * abs(PerlinNoise3D(f * p + offsets[i%4]));
    f *= lacunarity;
    a *= gain;
  }
  return result;
}


static float Ridge(float h, float offset)
{
  h = abs(h);
  h = offset - h;
  h = h * h;
  return h;
}


float RidgedMultifractal(float3 p, int numberOfOctaves, float offset = 1.0)
{
  float lacunarity = 2.0;
  float gain = 0.5;
  
  float f = 1;    // Frequency
  float a = 0.5;    // Amplitude
  
  float result = 0;
  float prev = 1.0;
  for (int i = 0; i < numberOfOctaves; i++)
  {
    float n = Ridge(PerlinNoise3D(f * p), offset);
    result += a * n * prev;
    prev = n;
    f *= lacunarity;
    a *= gain;
  }
  return result;
}


//-----------------------------------------------------------------------------
// Improved Perlin 4D Noise
//-----------------------------------------------------------------------------

// TODO: 4D noise is not tested yet.

float PerlinPermutation(float x)
{
  return tex1D(PerlinPermutationSampler, x).x;
}


float PerlinGradient4D(float x, float4 p)
{
  return dot(tex1D(PerlinGradient4DSampler, x), p);
}


float4 PerlinFade(float4 t)
{
  return t * t * t * (t * (t * 6 - 15) + 10);
}


/// Computes Improved Perlin Noise (4D).
/// \param[in] p  A 4D vector.
/// \return The noise value.
/// \remarks
/// This method requires following lookup textures:
///   PerlinPermutationTexture, PerlinGradient4DTexture
float PerlinNoise4D(float4 p)
{
  float4 P = fmod(floor(p), 256.0);	  // Find unit hypercube that contains point.
  p -= floor(p);                      // Find relative x, y, z of point in cube.
  float4 f = PerlinFade(p);           // Compute fade curves for each of x, y, z, w.
  P = P / 256.0;
  const float one = 1.0 / 256.0;
  
  // Hash coordinates of the 16 corners of the hypercube.
  float A = PerlinPermutation(P.x) + P.y;
  float AA = PerlinPermutation(A) + P.z;
  float AB = PerlinPermutation(A + one) + P.z;
  float B =  PerlinPermutation(P.x + one) + P.y;
  float BA = PerlinPermutation(B) + P.z;
  float BB = PerlinPermutation(B + one) + P.z;
  
  float AAA = PerlinPermutation(AA)+P.w, AAB = PerlinPermutation(AA+one)+P.w;
  float ABA = PerlinPermutation(AB)+P.w, ABB = PerlinPermutation(AB+one)+P.w;
  float BAA = PerlinPermutation(BA)+P.w, BAB = PerlinPermutation(BA+one)+P.w;
  float BBA = PerlinPermutation(BB)+P.w, BBB = PerlinPermutation(BB+one)+P.w;
  
  // INTERPOLATE DOWN
  return
    lerp(
      lerp(
        lerp(
          lerp(
            PerlinGradient4D(PerlinPermutation(AAA), p),
            PerlinGradient4D(PerlinPermutation(BAA), p + float4(-1, 0, 0, 0)),
            f.x),
          lerp(
            PerlinGradient4D(PerlinPermutation(ABA), p + float4(0, -1, 0, 0)),
            PerlinGradient4D(PerlinPermutation(BBA), p + float4(-1, -1, 0, 0)),
            f.x),
          f.y),
        lerp(
          lerp(
            PerlinGradient4D(PerlinPermutation(AAB), p + float4(0, 0, -1, 0)),
            PerlinGradient4D(PerlinPermutation(BAB), p + float4(-1, 0, -1, 0)),
            f.x),
          lerp(
            PerlinGradient4D(PerlinPermutation(ABB), p + float4(0, -1, -1, 0)),
            PerlinGradient4D(PerlinPermutation(BBB), p + float4(-1, -1, -1, 0)),
            f.x),
          f.y),
        f.z),
      lerp(
        lerp(
          lerp(
            PerlinGradient4D(PerlinPermutation(AAA+one), p + float4(0, 0, 0, -1)),
            PerlinGradient4D(PerlinPermutation(BAA+one), p + float4(-1, 0, 0, -1)),
            f.x),
          lerp(
            PerlinGradient4D(PerlinPermutation(ABA+one), p + float4(0, -1, 0, -1)),
            PerlinGradient4D(PerlinPermutation(BBA+one), p + float4(-1, -1, 0, -1)),
            f.x),
          f.y),
        lerp(
          lerp(
            PerlinGradient4D(PerlinPermutation(AAB+one), p + float4(0, 0, -1, -1)),
            PerlinGradient4D(PerlinPermutation(BAB+one), p + float4(-1, 0, -1, -1)),
            f.x),
          lerp(
            PerlinGradient4D(PerlinPermutation(ABB+one), p + float4(0, -1, -1, -1)),
            PerlinGradient4D(PerlinPermutation(BBB+one), p + float4(-1, -1, -1, -1)),
            f.x),
          f.y),
        f.z),
      f.w);
}
#endif
