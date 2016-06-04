//-----------------------------------------------------------------------------
// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.
//-----------------------------------------------------------------------------
//
/// \file Optimization.fxh
/// Fast optimized approximations for common functions and other optimization
/// tricks.
//
//-----------------------------------------------------------------------------

#ifndef DIGITALRUNE_OPTIMIZATION_FXH
#define DIGITALRUNE_OPTIMIZATION_FXH

//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

/// Computes 2^x using a fast approximation.
/// \param[in] x    The exponent.
/// \return The number 2 raised to the power x.
/// \remarks
/// Accurate to 12 bits.
/// From "An Approximation to the Chapman Grazing-Incidence Function for Atmospheric
/// Scattering", GPU Pro3, pp. 105
float Exp2Fast(float x)
{
  const float3 c[3] = { 5.79525, 12.52461, -2.88611 };
  int e = round(x);
  float t = x - e;
  float m = (t * t +c[0] * t + c[1]) / (c[2] * t + c[1]);
  return ldexp(m, e);
}


/// Computes sin(x) using a fast approximation.
/// \param[in] x    The angle in the range [-Pi/x, +Pi/2]
/// \return The sine of x.
float SinFast(float x)
{
  float3 c = float3(1, -0.1666666, 0.0083333);
  float xSquared = x * x;
  
  float3 p;
  p.x = x;
  p.y = x * xSquared;
  p.z = p.y * xSquared;
  return dot(p, c);
}

#endif
