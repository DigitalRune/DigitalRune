//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Tonemapping.fxh
/// Functions to adjust exposure and map HDR colors to LDR.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_TONEMAPPING_FXH
#define DIGITALRUNE_TONEMAPPING_FXH

// Common.fxh is required for the luminance weights.
#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including Tonemapping.fxh."
#endif


//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

// Common shader parameters for tone-mapping.
/*
float Exposure;    // The exposure level.

// The average gray value that corresponds to the average luminance. (The "key"
// of the scene.) Good values according to "Programming Vertex and Pixel Shader"
// are 0.18, 0.36, 0.54.
float MiddleGray = 0.18f;
 */


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Scales the color by the given exposure value.
/// \param[in] color      The color.
/// \param[in] exposure   The exposure (factor).
/// \return The scaled color.
float3 AdjustExposure(float3 color, float exposure)
{
  return color * exposure;
}


/// Scales the color by an automatic computed exposure value which moves the
/// average scene luminance to a predefined level (middle gray).
/// \param[in] color             The color.
/// \param[in] averageLuminance  The actual average luminance of the scene image.
/// \param[in] middleGray        The "key" of the scene (= desired average luminance.)
/// \return The scaled color.
float3 AdjustExposure(float3 color, float averageLuminance, float middleGray)
{
  float exposure = middleGray / (averageLuminance + 0.001f); // Small epsilon to avoid div by zero.
  return color * exposure;
}


/// Scales the color by an automatic computed exposure value which moves the
/// average scene luminance to a predefined level (middle gray).
/// \param[in] color             The color.
/// \param[in] averageLuminance  The actual average luminance of the scene image.
/// \param[in] middleGray        The "key" of the scene (= desired average luminance.)
/// \param[in] black             The minimal luminance. Colors darker than
///                              this value are cut off. If this value is 0
///                              it has no effect.
/// \return The scaled color.
float3 AdjustExposure(float3 color, float averageLuminance, float middleGray, float black)
{
  float exposure = middleGray / (averageLuminance + 0.001f);
  return max(0, color - black) * exposure;
}


/// Scales the color by an automatic computed exposure value which moves the
/// average scene luminance to a predefined level (middle gray).
/// \param[in] color             The color.
/// \param[in] averageLuminance  The actual average luminance of the scene image.
/// \param[in] middleGray        The "key" of the scene (= desired average luminance.)
/// \param[in] black             The minimal luminance. Colors darker than
///                              this value are cut off. If this value is 0
///                              it has no effect.
/// \param[in] minExposure       The lower limit for the allowed exposure value.
/// \param[in] maxExposure       The upper limit for the allowed exposure value.
/// \return The scaled color.
float3 AdjustExposure(float3 color, float averageLuminance, float middleGray, float black,
                      float minExposure, float maxExposure)
{
  float exposure = middleGray / (averageLuminance + 0.001f);
  exposure = clamp(exposure, minExposure, maxExposure);
  return max(0, color - black) * exposure;
}


/// Maps HDR colors to LDR using a simple exponential exposure curve.
/// \param[in] color  The color.
/// \return The tone-mapped color.
float3 TonemapExponential(float3 color)
{
  // See Book "Programming Vertex and Pixel Shaders", Section "Simple Exposure"
  return 1.0 - exp(-color);
  
  // Original: return 1.0 - exp(color * -exposure);
  // But we make AdjustExposure as a separate step.
}


/// Maps HDR colors to LDR using a scale factor and gamma correction for lower
/// values and a simple exponential curve for higher values.
/// \param[in] color  The color.
/// \return The tone-mapped color.
float3 TonemapExponentialWithGamma(float3 color)
{
  // The curve (x * 0.38317)^(1/2.2) is used for values below 1.413.
  // The exponential curve is used for higher values.
  // Both curves meet (almost) at 1.413.
  color.r = color.r < 1.413 ? pow(0.0001 + color.r * 0.38317, 1.0 / 2.2) : 1.0 - exp(-color.r);
  color.g = color.g < 1.413 ? pow(0.0001 + color.g * 0.38317, 1.0 / 2.2) : 1.0 - exp(-color.g);
  color.b = color.b < 1.413 ? pow(0.0001 + color.b * 0.38317, 1.0 / 2.2) : 1.0 - exp(-color.b);
  return color;
}


/// Maps HDR colors to LDR using using Reinhard's operator.
/// \param[in] color  The color.
/// \return The tone-mapped color.
float3 TonemapReinhard(float3 color)
{
  // Variant A:
  return color / (1 + color);
  
  //// Variant B:
  //float pixelLuminance = dot(color, LuminanceWeights);
  //float adjustedLuminance = pixelLuminance / (1 + pixelLuminance);
  //return color / pixelLuminance * adjustedLuminance;
  
  //// Variant C:
  //float pixelLuminance = dot(color, LuminanceWeights);
  //float adjustedLuminance = pixelLuminance / (1 + pixelLuminance);
  //return color * adjustedLuminance;
}


/// Maps HDR colors to LDR using Reinhard's operator.
/// \param[in] color   The input color.
/// \param[in] white   The luminance of the white level, e.g. the max scene
///                    luminance or a lower value to allow brighter pixels to
///                    "burn out".
/// \return The tone-mapped color.
float3 TonemapReinhard(float3 color, float white)
{
  // Using epsilon to avoid division by zero.
  return color * (1 + color / (white * white + 0.0001f))
         / (1 + color);
}


//float3 TonemapDuiker(float3 color, ...)
//{
//  // For Haarm-Peter Duiker’s curve see http://filmicgames.com/archives/75.
//  // Result is gamma corrected.
//}


/// Maps HDR colors to LDR using a filmic curve (including gamma-correction).
/// \param[in] color  The color.
/// \return The tone-mapped color.
float3 TonemapFilmicWithGamma(float3 color)
{
  // Variant A:
  const float blackLevel = 0.004;
  float3 x = max(0, color - blackLevel);
  return (x * (6.2 * x + 0.5)) / (x * (6.2 * x + 1.7) + 0.06);
  
  //// Variant B:
  //float pixelLuminance = dot(color, LuminanceWeights);
  //const float blackLevel = 0.004;
  //float x = max(0, pixelLuminance - blackLevel);
  //x = (x * (6.2 * x + 0.5)) / (x * (6.2 * x + 1.7) + 0.06);
  //return x * color / pixelLuminance;
}


/// Maps HDR colors to LDR using a filmic curve (including gamma-correction)
/// with improved black levels.
/// \param[in] color  The color.
/// \return The tone-mapped color.
float3 TonemapFilmicWithGammaEx(float3 color)
{
  // From http://iwasbeingirony.blogspot.com/2010/04/approximating-film-with-tonemapping.html
  // TonemapFilmic cuts off blacks. Dark blacks can be improved with this:
  const float cutoff = 0.025;
  float3 x = color + (cutoff * 2 - color) * saturate(cutoff * 2 - color) * (0.25f / cutoff) - cutoff;
  return (x * (6.2 * x + 0.5)) / (x * (6.2 * x + 1.7) + 0.06);
}


/// Maps HDR colors to LDR using a configurable filmic curve.
/// \param[in] color  The HDR color.
/// \param[in] A      Shoulder strength, e.g. 0.15.
/// \param[in] B      Linear strength, e.g. 0.50.
/// \param[in] C      Linear angle, e.g. 0.10.
/// \param[in] D      Toe strength, e.g. 0.20.
/// \param[in] E      E / F = Toe Angle, e.g. E = 0.02.
/// \param[in] F      E / F = Toe Angle, e.g. F = 0.30.
/// \returns The tone-mapped color (in linear space).
/// \remarks
/// The parameters A - F define the characteristics of the curve.
float3 TonemapFilmic(float3 color, float A, float B, float C, float D, float E, float F)
{
  return (color * (A * color + C * B) + D * E) / (color * (A * color + B) + D * F) - E / F;
}


/// Maps HDR colors to LDR using a configurable filmic curve and a given white level.
/// \param[in] color  The HDR color.
/// \param[in] white  The luminance of the white level, e.g. the max scene
///                   luminance or a lower value to allow brighter pixels to
///                   "burn out", e.g. 11.2.
/// \param[in] A      Shoulder strength, e.g. 0.15.
/// \param[in] B      Linear strength, e.g. 0.50.
/// \param[in] C      Linear angle, e.g. 0.10.
/// \param[in] D      Toe strength, e.g. 0.20.
/// \param[in] E      E / F = Toe Angle, e.g. E = 0.02.
/// \param[in] F      E / F = Toe Angle, e.g. F = 0.30.
/// \returns The tone-mapped color (in linear space).
/// \remarks
/// The parameters A - F define the characteristics of the curve.
float3 TonemapFilmic(float3 color, float white,
                     float A, float B, float C, float D, float E, float F)
{
  // See http://filmicgames.com/archives/75.
  color = TonemapFilmic(color, A, B, C, D, E, F);
  float3 whiteScale = 1 / TonemapFilmic(white, A, B, C, D, E, F);
  return color * whiteScale;
}


/// Maps HDR colors to LDR using a logarithmic mapping.
/// \param[in] color  The color.
/// \param[in] white  The luminance of the white level.
/// \return The tone-mapped color.
float3 TonemapLogarithmic(float3 color, float white)
{
  return log10(1 + color) / log10(1 + white);
}


/// Maps HDR colors to LDR using a Drago's logarithmic mapping.
/// \param[in] color  The color.
/// \param[in] white  The luminance of the white level.
/// \param[in] bias   The bias.
/// \return The tone-mapped color.
float3 ToneMapDragoLogarithmic(float3 color, float white, float bias)
{
  // see paper http://www.mpi-inf.mpg.de/resources/tmo/logmap/.
  return log10(1 + color)
         / log10(1 + white)
         / log10(2 + 8 * pow(0.000001 + ((color / white)), log10(bias) / log10(0.5f)));
}
#endif
