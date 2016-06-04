//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Fog.fxh
/// Functions for fog computation.
//
// ----- Usage
// 1. Compute distance (see below).
// 2. Compute fog intensity (in vertex or pixel shader):
//    float fogIntensity = ComputeXxxFogIntensity(...);
// 3. Lerp output color with fog (usually) in pixel shader:
//    color = ApplyFog(color, FogColor, fogIntensity);
//
// ----- Coordinate spaces
// The parameters start, end, dist must all be in the same coordinate
// space. They are usually given in world space.
//
// ----- Distance computation
// An accurate fog distance is computed like this:
//  dist = distance(positionWorld, CameraPosition);  OR
//  dist = length(positionWorld - CameraPosition);   OR
//  dist = length(positionView);
// A simple planar fog distance can be computed like this:
//  dist = -positionView.z;  OR
//  dist = positionProj.w;
//
// ----- Per-vertex vs. per-pixel fog
// The fog intensity can be computed in the vertex shader or in the pixel shader.
// If the fog intensity is computed in the vertex shader, the fog may look bad
// on very large triangles.
//
// ----- References
// Shader X² Introduction & Tutorials with DirectX 9 - Introduction to
//   Different Fog Effects
// ...
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_FOG_FXH
#define DIGITALRUNE_FOG_FXH


//-----------------------------------------------------------------------------
// Fog Constants
//-----------------------------------------------------------------------------

/*
// Distance where fog starts.
float FogStart : FOGSTART;

// Distance where fog reaches full intensity.
float FogEnd : FOGEND;

// Color of fog (RGBA). If alpha is 0, fog is disabled.
float4 FogColor : FOGCOLOR;

// Combined fog parameters.
float4 FogParameters : FOGPARAMETERS;  // (Start, End, Density, HeightFalloff)
#define FogStart FogParameters.x
#define FogEnd FogParameters.y
#define FogDensity FogParameters.z
#define FogHeightFalloff FogParameters.w
 */


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Computes the fog intensity for a given distance and linear fog.
/// \param[in] dist     The distance from the eye position.
/// \param[in] start    The distance where the fog starts.
/// \param[in] end      The distance where the fog reaches its full intensity.
/// \return The fog intensity in the range [0, 1]. 0 means no fog. 1 means 100%
///         fog.
float ComputeLinearFogIntensity(float dist, float start, float end)
{
  return saturate((dist - start) / (end - start));
}


/// Computes the fog intensity for a given distance and a smoothstep fog curve.
/// \param[in] dist     The distance from the eye position.
/// \param[in] start    The distance where the fog starts.
/// \param[in] end      The distance where the fog reaches its full intensity.
/// \return The fog intensity in the range [0, 1]. 0 means no fog. 1 means 100%
///         fog.
float ComputeSmoothFogIntensity(float dist, float start, float end)
{
  return smoothstep(start, end, dist);
}


/// Computes the fog intensity for a given distance and a custom nonlinear fog.
/// \param[in] dist       The distance from the eye position.
/// \param[in] start      The distance where the fog starts.
/// \param[in] end        The distance where the fog reaches its full intensity.
/// \param[in] exponent   The exponent of the fog curve (dist^exponent).
///                       Use 1 to get linear fog.
///                       Use a value < 1 to get fog which is thick near the camera.
///                       Use a value > 1 to get fog which is thicker in the far.
/// \return The fog intensity in the range [0, 1]. 0 means no fog. 1 means 100%
///         fog.
float ComputeNonlinearFogIntensity(float dist, float start, float end, float exponent)
{
  return pow(0.000001 + saturate((dist - start) / (end - start)), exponent);
}


/// Computes the fog intensity for a given distance and exponential fog.
/// \param[in] dist     The distance from the eye position.
/// \param[in] density  The density of the fog.
///                     Use a value > 0 (usually near 0) for a reasonable fog effect.
///                     Use 0 to disable fog.
/// \return The fog intensity in the range [0, 1]. 0 means no fog. 1 means 100% fog.
/// \remarks
/// Exponential fog reaches 100% density at infinite distance.
float ComputeExponentialFogIntensity(float dist, float density)
{
  return 1 - exp2(-density *  dist);
}


/// Computes the fog intensity for a given distance and exponential squared fog.
/// \param[in] dist     The distance from the eye position.
/// \param[in] density  The density of the fog at the camera.1
///                     Use a value > 0 (usually near 0) for a reasonable fog effect.
///                     Use 0 to disable fog.
/// \return The fog intensity in the range [0, 1]. 0 means no fog. 1 means 100% fog.
/// \remarks
/// Exponential fog reaches 100% density at infinite distance.
float ComputeExponentialSquaredFogIntensity(float dist, float density)
{
  return 1 - exp2(-pow(density *  dist, 2));
}


/// Computes the optical length for exponentially decreasing fog density.
/// \param[in] dist              The distance from the eye position.
/// \param[in] cameraDensity     The density of the fog at the camera:
///                              fogDensity * 2^(-HeightFalloff * y) or
///                              1 / (End - Start) * 2^(-HeightFalloff * y) if
///                              fog density is defined using End and Start parameters.
///                              Use a value > 0 (usually near 0) for a reasonable fog effect.
///                              Use 0 to disable fog.
/// \param[in] cameraToPosition  The vector from the camera to the fogged position.
/// \param[in] heightFalloff     The height falloff parameter, see remarks.
/// \remarks
/// We assume the fog density decreases exponentially: density(y) = e^(-HeightFalloff * y)
/// The optical length is like the distance travelled in the fog times the density.
/// To find the optical length we have to compute the line integral (in german "Kurvenintegral").
/// See Crytek presentation "Real-time Atmospheric Effects in Games Revisited", GDC 2007.
float GetOpticalLengthInHeightFog(float dist, float cameraDensity, float3 cameraToPosition, float heightFalloff)
{
  float opticalLength = dist * cameraDensity;
  const float SlopeThreshold = 0.00001;
  if (abs(cameraToPosition.y) > SlopeThreshold && any(heightFalloff))
  {
    // This part is only computed if t cannot be 0 (division by zero).
    float t = heightFalloff * cameraToPosition.y;
    opticalLength *= (1.0 - exp2(-t)) / t;
  }
  // opticalLength can easily become +infinite which would lead to NaN.
  // --> clamp to finite range. (The upper limit is just an abritrary large number.)
  opticalLength = clamp(opticalLength, 0, 1e16);
  return opticalLength;
}


/// Linearly interpolates between the pixel color (from lighting) and the fog color.
/// \param[in] pixelColor     The color without fog.
/// \param[in] fogColor       The color of the fog.
/// \param[in] fogIntensity   The fog intensity. 0 means no fog, 1 means 100% fog.
/// \return The fogged color.
float3 ApplyFog(float3 pixelColor, float3 fogColor, float fogIntensity)
{
  return lerp(pixelColor, fogColor, fogIntensity);
}


/// Computes the phase function for light scattering.
/// \param[in] cosTheta   The cosine of the angle between the direction to the light
///                       and the viewing direction (e.g. observer to sky).
/// \param[in] g          The anisotropy factor. (0 = isotropic scattering,
///                       1 = forward scattering)
/// \return The amount of light scattered in the direction of the observer.
float3 FogPhaseFunction(float cosTheta, float3 g)
{
  // Henyey-Greenstein phase function.
  // This phase function is simpler and it is 1 for g = 0.
  return (1.0 - g * g) / pow(1.0 + g * g - 2.0 * g * cosTheta, 3.0 / 2.0);
  // The original phase function is normalized using a factor 1 / (4 * Pi) so that
  // the integral of the sphere is 1. We omit the normalization factor because we
  // want to keep the overall fog brightness. That means the average value in each
  // direction should be 1.
}
#endif
