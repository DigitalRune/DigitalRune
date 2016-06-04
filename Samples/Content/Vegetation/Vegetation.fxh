//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Vegetation.fxh
/// The function ComputeSwayOffset computes a vertex offset to create an
/// animation of a plant swaying in the wind.
/// The animation consists of three parts:
/// - trunk sway: The whole plant rocks back and forth.
/// - branch sway: Individual branches or big (e.g. palm) leaves sway up and down.
/// - leaf sway: Leaves or the edges of big leaves flutter horizontally.
/// References:
/// - Vegetation Procedural Animation and Shading in Crysis, GPU Gems 3, pp. 373
///   http://http.developer.nvidia.com/GPUGems3/gpugems3_ch16.html
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_VEGETATION_FXH
#define DIGITALRUNE_VEGETATION_FXH

#ifndef DIGITALRUNE_NOISE_FXH
#error "Noise.fxh required. Please include Noise.fxh before including Vegetation.fxh."
#endif

/*
// Wind and sway parameters.
float Time : TIME;
float3 Wind : WIND;
float2 WindWaveParameters;  // (frequency (= 1 / wave length), randomness)
float3 SwayFrequencies;     // (trunk, branch, unused)
float3 SwayScales;          // (trunk, branch, leaf)
 */


//-----------------------------------------------------------------------------
// Smooth triangle wave
//-----------------------------------------------------------------------------

// Smooth S-curve between 0 and 1.
float4 SmoothCurve(float4 x)
{
  return x * x *( 3.0 - 2.0 * x );
}

// A function where y goes from 0 to 1 and back to 0 in the x range [0, 1].
float4 TriangleWave(float4 x)
{
  return abs( frac( x + 0.5 ) * 2.0 - 1.0 );
}

// A smooth curve which can be used as a replacement for sin(2*Pi*x) in sway animations.
float4 SmoothTriangleWave(float4 x)
{
  return SmoothCurve(TriangleWave(x));
}


//-----------------------------------------------------------------------------
// Sway functions
//-----------------------------------------------------------------------------

float3 ComputeTrunkSwayOffset(
  float3 plantPosition, float3 vertexPosition, float modelScale,
  float3 wind, float2 windWaveParameters, float time, float frequency, float amplitude)
{
#if !MGFX
  // This is a near-0 value which can be added to effect parameters to
  // workaround a DX9 HLSL compiler preshader bug.
  float dummy = vertexPosition.y * 1e-30f;
#else
  float dummy = 0;
#endif
  
  // ----- Phase variation to create waves in grass fields.
  // Without phase variation all meshes sway synchronously. By varying the phase
  // we can make wave ripples flow through grass fields.
  // A) Some implementations make waves in an arbitrary direction with wave length.
  //float phase = dot(plantPosition, 1);
  // B) Better to make waves in wind direction.
  float windWaveFrequency = windWaveParameters.x;
  float windSpeed = length(wind) + 0.00001f; // Add small value to avoid division by 0.
  float phase = dot(plantPosition, wind / windSpeed) * windWaveFrequency;
  // C) Use a band-limited noise (e.g. Perlin noise) to make only local batches sway together.
  // TODO: Use plant position to sample noise texture.
  
  // Optionally, we can add randomness to avoid a very uniform look.
  float random = Noise2(plantPosition + dummy);
  float windWaveRandomness = windWaveParameters.y;
  phase += random * windWaveRandomness;
  
  // ----- Swaying offset
  // Plants will sway forward and backward in the wind like a sine wave. However,
  // because of the complex assembly of branches and leaves, the sway animation
  // can be rather complex.
  // A) Single sine. Good enough for many cases, especially if wind is animated on CPU.
  float3 offset = wind * sin(2 * 3.14 * (frequency * time + phase));
  // B) Combining two sines in forward direction.
  //float x = 2 * Pi * (time * frequency + phase);
  //float3 offset = wind * (sin(x) + 0.25 * cos(x * 2 + 3));
  // C) Forward/backward swing using phyiscally-based observations (from GPU Gems 3 chapter 6).
  //float x = 2 * Pi * (time * frequency + phase);
  //// Flexible tree:
  ////float3 offset = 20 * wind * (cos(x) * cos(x * 3) * cos(x * 5) * cos(x * 7) + sin(x * 25) * 0.1);
  //// Stiff tree:
  //float3 offset = 20 * wind * (cos(x) * cos(x) * cos(x * 3) * cos(x * 5) * 0.5 + sin(x * 25) * 0.02);
  // D) Combines sines in forward and side direction.
  //float x = 2 * Pi * (time * frequency + phase);
  //float3 windForward = float3(wind.x, 0, wind.z);
  //float3 windRight = float3(-wind.z, 0, wind.x);
  //float3 offset = windForward * sin(x) + windRight * sin(1.7 * x) * 0.25;
  // E) Combine sines in forward and side direction. (from LTree open source project)
  //float x = 2 * Pi * (time * frequency * 0.1 + phase);
  //float3 windForward = float3(wind.x, 0, wind.z);
  //float3 windRight = float3(-wind.z, 0, wind.x);
  //float3 offset = 10.0 * windRight * sin(x * 3)
  //            + 1.5  * windRight * sin(x * 11 + 3) * sin(x * 1 + 3)
  //            + 15   * windForward * sin(x * 5 + 1)
  //            + 1.5  * windForward * sin(x * 11 + 3);
  // F) We could also compute an offset vector for each plant on the CPU...
  
  // The offset is scaled with the model size.
  offset *= modelScale;
  
  // ----- Apply displacement.
  // A) Simple displacement.
  //offset = offset * amplitude;
  // B) Smooth bending factor and spherical reascale. (from GPU Gems 3 chapter 16 (Crysis))
  float bendFactor = amplitude;
  bendFactor += 1.0;
  bendFactor *= bendFactor;
  bendFactor = bendFactor * bendFactor - bendFactor;
  // Displace position horizontally.
  float3 newPosition = vertexPosition + bendFactor * float3(offset.x, 0, offset.z);
  // Spherical rescale.
  float radius = length(vertexPosition - plantPosition);
  float3 delta = newPosition - plantPosition;
  newPosition = plantPosition + delta / (length(delta) + 0.00001) * radius;
  offset = newPosition - vertexPosition;
  
  return offset;
}


float3 ComputeBranchSwayOffset(
  float3 plantPosition, float3 vertexPosition, float3 normal,
  float3 wind, float time, float frequency, float branchPhase,
  float branchSwayScale, float leafSwayScale)
{
  // Add a phase variation per plant.
  branchPhase += dot(plantPosition, 1);
  
  // The branch phase is usually painted in the vertex colors. The leaf phase is
  // currently only initialized with a value derived from the position.
  float leafPhase = 0;
  //leafPhase = dot(vertexPosition, leafPhase + branchPhase);
  leafPhase = dot(vertexPosition, 1) + branchPhase;
  
  // Add phase to time.
  float2 times = time + float2(leafPhase, branchPhase);
  
  // Multiply time with frequency to get parameters and compute 4 waves
  // (2 waves for leaf sway, 2 waves for branch sway).
  float4 frequencies = float4(1.975, 0.793, 0.375, 0.193);
  float4 x = (frac(times.xxyy * frequencies) * 2.0 - 1.0 ) * frequency;
  float4 waves = SmoothTriangleWave(x);
  float2 waveSum = waves.xz + waves.yw;
  
  // Make branches swing up and down - not only up.
  waveSum = 2 * waveSum - 1;
  
  float3 offset = waveSum.xyx;
  float windSpeed = length(wind);
  offset.x *= normal.x * leafSwayScale * windSpeed;
  offset.y *= branchSwayScale * windSpeed;
  offset.z *= normal.z * leafSwayScale * windSpeed;
  
  return offset;
}


/// Computes a sway offset vector for wind animation of plants.
/// \param[in] world                The world transform matrix of the plant.
/// \param[in] vertexPositionWorld  The current vertex position in world space.
/// \param[in] normalWorld          The current normal vector in world space.
/// \param[in] wind                 The wind velocity.
/// \param[in] windWaveParameters   The wind wave parameters (frequency, randomness)
///                                 This controls the wave patterns in a field of plants.
/// \param[in] time                 The current absolute time in seconds.
/// \param[in] trunkFrequency       The sway frequency of the trunk.
/// \param[in] branchFrequency      The sway frequency of branches.
/// \param[in] branchPhase          A random phase in [0, 1] per branch. Usually
//                                  encoded in the green vertex channel.
/// \param[in] trunkAmplitude       The sway amplitude of the trunk. This value should
///                                 be proportional to the "normalized" plant height!
/// \param[in] branchAmplitude      The sway amplitude for the branch animation.
/// \param[in] leafAmplitude        The sway amplitude for the leaf animation.
/// \return The vertex offset in world space.
float3 ComputeSwayOffset(
  float4x4 world, float3 vertexPositionWorld, float3 normalWorld,
  float3 wind, float2 windWaveParameters, float time,
  float trunkFrequency, float branchFrequency, float branchPhase,
  float trunkAmplitude, float branchAmplitude, float leafAmplitude)
{
  // Extract useful data from world matrix.
  float3 plantPositionWorld = world._m30_m31_m32; // = mul(float4(0, 0, 0, 1), world);
  float modelScaleX = length(world._m00_m10_m20);
  float modelScaleY = length(world._m01_m11_m21);
  
  float3 trunkSwayOffset = ComputeTrunkSwayOffset(
    plantPositionWorld, vertexPositionWorld, modelScaleY,
    wind, windWaveParameters, time, trunkFrequency, trunkAmplitude);
  
  // Extract x scale from world matrix and apply to amplitudes to make bigger
  // plant instances sway more.
  branchAmplitude *= modelScaleY;
  // (Actually, leaves on a big tree might not sway more than leaves on a small
  // tree. However, since the models are simply scaled, the leaves also get bigger
  // unlike real leaves. At least for scaled palm trees leaves this looks good.)
  leafAmplitude *= modelScaleX;
  float3 branchSwayOffset = ComputeBranchSwayOffset(
    plantPositionWorld, vertexPositionWorld, normalWorld,
    wind, time, branchFrequency, branchPhase,
    branchAmplitude, leafAmplitude);
  
  return trunkSwayOffset + branchSwayOffset;
}
#endif
