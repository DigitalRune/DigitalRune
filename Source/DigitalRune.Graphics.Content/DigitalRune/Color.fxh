//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Color.fxh
/// Color-related functions.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_COLOR_FXH
#define DIGITALRUNE_COLOR_FXH

// Common.fxh is required for the luminance weights.
#ifndef DIGITALRUNE_COMMON_FXH
#error "Common.fxh required. Please include Common.fxh before including Color.fxh."
#endif

/// A matrix which converts colors from the CIE XYZ color space to the
/// sRGB color space; must be used like this: RGB = mul(XYZ, XYZToRGB);
static const float3x3 XYZToRGB = { 3.240479f, -0.969256f,  0.055648f,
                                   -1.53715f,  1.875991f, -0.204043f,
                                   -0.49853f,  0.041556f,  1.057311f };


/// A matrix which converts colors from the sRGB color space to the CIE XYZ
/// color space; must be used like this: XYZ = mul(RGB, RGBToXYZ);
static const float3x3 RGBToXYZ = { 0.412453f, 0.212671f, 0.019333f,
                                   0.357579f, 0.715159f, 0.119193f,
                                   0.180420f, 0.072167f, 0.950226f };


/// Desaturates a given color. The saturation of the current color is assumed
/// to be 1. The color's saturation is changed to the given saturation value.
/// \param[in] color       The color.
/// \param[in] saturation  The target saturation in the range [0, 1].
/// \return  The color with reduced saturation.
float3 Desaturate(float3 color, float saturation)
{
  float gray = dot(color, LuminanceWeights);
  
  // Lerp between gray and the original color.
  return lerp(gray, color, saturation);
}


/// Converts a HSV color value to RGB.
/// \param[in] color       The HSV color (h, s and v in the range [0,1]).
/// \return The RGB color.
float3 FromHsv(float3 color)
{
  float h = color.x;
  float s = color.y;
  float v = color.z;
  
  float r = v;
  float g = v;
  float b = v;
  
  if (s != 0)
  {
    if (h == 1)
      h = 0;
    
    h *= 360.0 / 60.0;
    
    float i = floor(h);
    float f = h - i;
    float p = v * (1 - s);
    float q = v * (1 - s * f);
    float t = v * (1 - s * (1.0 - f));
    
    if (i == 0)
    {
      r = v;
      g = t;
      b = p;
    }
    else if (i == 1)
    {
      r = q;
      g = v;
      b = p;
    }
    else if (i == 2)
    {
      r = p;
      g = v;
      b = t;
    }
    else if (i == 3)
    {
      r = p;
      g = q;
      b = v;
    }
    else if (i == 4)
    {
      r = t;
      g = p;
      b = v;
    }
    else if (i == 5)
    {
      r = v;
      g = p;
      b = q;
    }
  }
  
  return float3(r, g, b);
}


/// Converts a RGB color value to HSV.
/// \param[in] color       The RGB color.
/// \return The HSV color. (h, s, v) in the range [0,1]).
float3 ToHsv(float3 color)
{
  float minimum = min(color.r, min(color.g, color.b));
  float maximum = max(color.r, max(color.g, color.b));
  float delta = maximum - minimum;
  
  float v = maximum;
  
  float s = 0;
  if (maximum != 0)
    s = delta / maximum;
  
  float h = 0;
  if (s != 0)
  {
    if (maximum == color.r)
      h = (color.g - color.b) / delta;
    else if (maximum == color.g)
      h = 2 + (color.b - color.r) / delta;
    else
      h = 4 + (color.r - color.g) / delta;
  }
  
  h *= 60;
  if (h < 0)
    h += 360;
  
  return float3(h / 360, s, v);
}
#endif
