// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
// The ephemeris model is based on:
//
//    "Physically-Based Outdoor Scene Lighting", by Frank Kane (Founder of Sundog Software, LLC),
//    Game Engine Gems 1.
//
//    Copyright (c) 2004-2008  Sundog Software, LLC. All rights reserved worldwide.
//
// Code is used with permission from Frank Kane.
#endregion

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  partial class Ephemeris
  {
    // A twilight luminance table with measured values.
    private static Dictionary<int, float> TwilightLuminance
    {
      get
      {
        if (_twilightLuminance == null)
        {
          // We convert from asb to cd/m² (1 asb = Pi cd/m²).
          _twilightLuminance = new Dictionary<int, float>();
          _twilightLuminance[5] = 2200 / ConstantsF.Pi;
          _twilightLuminance[4] = 1800 / ConstantsF.Pi;
          _twilightLuminance[3] = 1400 / ConstantsF.Pi;
          _twilightLuminance[2] = 1200 / ConstantsF.Pi;
          _twilightLuminance[1] = 710 / ConstantsF.Pi;
          _twilightLuminance[0] = 400 / ConstantsF.Pi;
          _twilightLuminance[-1] = 190 / ConstantsF.Pi;
          _twilightLuminance[-2] = 77 / ConstantsF.Pi;
          _twilightLuminance[-3] = 28 / ConstantsF.Pi;
          _twilightLuminance[-4] = 9.4f / ConstantsF.Pi;
          _twilightLuminance[-5] = 2.9f / ConstantsF.Pi;
          _twilightLuminance[-6] = 0.9f / ConstantsF.Pi;
          _twilightLuminance[-7] = 0.3f / ConstantsF.Pi;
          _twilightLuminance[-8] = 0.11f / ConstantsF.Pi;
          _twilightLuminance[-9] = 0.047f / ConstantsF.Pi;
          _twilightLuminance[-10] = 0.021f / ConstantsF.Pi;
          _twilightLuminance[-11] = 0.0092f / ConstantsF.Pi;
          _twilightLuminance[-12] = 0.0031f / ConstantsF.Pi;
          _twilightLuminance[-13] = 0.0022f / ConstantsF.Pi;
          _twilightLuminance[-14] = 0.0019f / ConstantsF.Pi;
          _twilightLuminance[-15] = 0.0018f / ConstantsF.Pi;
          _twilightLuminance[-16] = 0.0018f / ConstantsF.Pi;
        }
        return _twilightLuminance;
      }
    }
    private static Dictionary<int, float> _twilightLuminance;


    // To avoid memory allocations, we allocate these spectrums instances only once.
    private static readonly Spectrum _spectrum = new Spectrum();
    private static readonly Spectrum _spectrumDirect = new Spectrum();    // Direct light.
    private static readonly Spectrum _spectrumIndirect = new Spectrum();  // Indirect/scattered/ambient light.


    /// <summary>
    /// Gets the extraterrestrial sunlight intensity based on NASA data.
    /// </summary>
    /// <value>The sunlight intensity outside the earth's atmosphere in [lux].</value>
    public static Vector3F ExtraterrestrialSunlight
    {
      get
      {
        if (!_extraterrestrialSunlight.HasValue)
        {
          _spectrum.SetSolarSpectrum();
          Vector3F sunlightXyz = _spectrum.ToXYZ();
          Vector3F sunlightRgb = GraphicsHelper.XYZToRGB * sunlightXyz;
          _extraterrestrialSunlight = sunlightRgb;
        }

        return _extraterrestrialSunlight.Value;
      }
    }
    private static Vector3F? _extraterrestrialSunlight;


    /// <summary>
    /// Computes the sunlight intensity.
    /// </summary>
    /// <param name="altitude">
    /// The altitude (elevation) of the observer's position in meters above the mean sea level.
    /// </param>
    /// <param name="turbidity">
    /// The turbidity, which measures how polluted the air is. The values should be in the range 
    /// [1.8, 20]. A turbidity of 2 describes a clear day whereas a turbidity of 20 represents thick
    /// haze. A commonly used value is 2.2.
    /// </param>
    /// <param name="sunDirection">The direction from the observer's position to the sun.</param>
    /// <param name="directSunlight">The direct sunlight illuminance in [lux].</param>
    /// <param name="scatteredSunlight">
    /// The scattered sunlight illuminance (= ambient light, indirect light or "skylight"
    /// contribution of the sun) in [lux].
    /// </param>
    /// <remarks>
    /// <para>
    /// The light values are computed for the earth using NASA data, experimental data and a 
    /// physically-based model of the atmosphere.
    /// </para>
    /// <para>
    /// All light values are computed for a cloudless sky. When the sky is cloudy, reduce the direct
    /// light and increase the scattered/ambient light. Additionally you will want to increase the
    /// ambient light at night to model light pollution.
    /// </para>
    /// <para>
    /// <strong>Thread-Safety:</strong> This method is <strong>not</strong> thread-safe, i.e. it 
    /// must not be called simultaneously from multiple threads.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void GetSunlight(float altitude, float turbidity, Vector3D sunDirection,
                                   out Vector3F directSunlight, out Vector3F scatteredSunlight)
    {
      _spectrum.SetSolarSpectrum();

      sunDirection.TryNormalize();
      double cosZenith = sunDirection.Y;

      Vector3F direct, indirect;
      if (cosZenith > 0)
      {
        // Daylight - Sun is above horizon.
        double zenithAngle = Math.Acos(cosZenith);
        _spectrum.ApplyAtmosphericTransmittance(zenithAngle, turbidity, altitude, _spectrumDirect, _spectrumIndirect);
        direct = _spectrumDirect.ToXYZ();
        indirect = _spectrumIndirect.ToXYZ();
      }
      else
      {
        // Twilight - Sun is below horizon.
        // We lookup luminance based on experimental results on cloudless nights.

        // Get sun angle in degrees for table lookup.
        float solarAltitude = (float)MathHelper.ToDegrees(Math.Asin(sunDirection.Y));

        // Get luminance from table (linearly interpolating the next two table entries).
        int lower = (int)Math.Floor(solarAltitude);
        int higher = (int)Math.Ceiling(solarAltitude);
        float a, b;
        TwilightLuminance.TryGetValue(lower, out a);
        TwilightLuminance.TryGetValue(higher, out b);
        float Y = InterpolationHelper.Lerp(a, b, solarAltitude - lower);

        // We use fixed chromacity values.
        float x = 0.2f;
        float y = 0.2f;

        // Convert xyY to XYZ.
        float X = x * (Y / y);
        float Z = (1.0f - x - y) * (Y / y);

        // Get sunlight from slightly above the horizon.
        const float epsilon = 0.001f;
        const double zenithAngle = ConstantsD.PiOver2 - epsilon;
        _spectrum.ApplyAtmosphericTransmittance(zenithAngle, turbidity, altitude, _spectrumDirect, _spectrumIndirect);
        direct = _spectrumDirect.ToXYZ();
        indirect = _spectrumIndirect.ToXYZ();

        // Blend between table values and sunset light.
        float blend = MathHelper.Clamp(-solarAltitude / 5.0f, 0, 1);
        direct =  InterpolationHelper.Lerp(direct, new Vector3F(0, 0, 0), blend);
        indirect = InterpolationHelper.Lerp(indirect, new Vector3F(X, Y, Z), blend);
      }

      // Convert XYZ to RGB.
      directSunlight = GraphicsHelper.XYZToRGB * direct;
      scatteredSunlight = GraphicsHelper.XYZToRGB * indirect;
    }


    /// <summary>
    /// Computes the moonlight intensity.
    /// </summary>
    /// <param name="altitude">
    /// The altitude (elevation) of the observer's position in meters above the mean sea level.
    /// </param>
    /// <param name="turbidity">
    /// The turbidity, which measures how polluted the air is. The values should be in the range 
    /// [1.8, 20]. A turbidity of 2 describes a clear day whereas a turbidity of 20 represents thick
    /// haze. A commonly used value is 2.2.
    /// </param>
    /// <param name="moonPosition">The moon position in world space.</param>
    /// <param name="phaseAngle">
    /// The moon phase angle in radians in the range [0, 2π]. A new moon has a phase angle of 0. A 
    /// full moon has a phase angle of π. 
    /// </param>
    /// <param name="directMoonlight">The direct moonlight illuminance in [lux].</param>
    /// <param name="scatteredMoonlight">
    /// The scattered moonlight illuminance (= ambient light, indirect light or "skylight"
    /// contribution of the moon) in [lux].
    /// </param>
    /// <inheritdoc cref="GetSunlight"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void GetMoonlight(float altitude, float turbidity, Vector3D moonPosition, float phaseAngle,
                                    out Vector3F directMoonlight, out Vector3F scatteredMoonlight)
    {
      Vector3D moonDirection = moonPosition.Normalized;
      double cosZenith = moonDirection.Y;
      double zenith = Math.Acos(cosZenith);

      float moonLuminance = (float)GetMoonLuminance(moonPosition, moonDirection, phaseAngle);
      _spectrum.SetLunarSpectrum(moonLuminance);
      _spectrum.ApplyAtmosphericTransmittance(zenith, turbidity, altitude, _spectrumDirect, _spectrumIndirect);

      // Convert XYZ to RGB.
      directMoonlight = GraphicsHelper.XYZToRGB * _spectrumDirect.ToXYZ();
      scatteredMoonlight = GraphicsHelper.XYZToRGB * _spectrumIndirect.ToXYZ();
    }


    // Computes the luminance of the moon based on the simulated phase and distance from earth.
    private static double GetMoonLuminance(Vector3D moonPosition, Vector3D moonDirection, float phaseAngle)
    {
      double moonAngle = MathHelper.ToDegrees(Math.Asin(moonDirection.Y));

      const double Esm = 1905.0;          // W/m2
      const double C = 0.072;
      const double Rm = 1738.1 * 1000.0;  // m
      double d = moonPosition.Length;     // Moon distance.

      double mPhase = phaseAngle;

      double ePhase = ConstantsD.Pi - mPhase;
      while (ePhase < 0)
        ePhase += 2.0 * ConstantsD.Pi;

      // Earthshine
      double Eem = 0.19 * 0.5 * (1.0 - Math.Sin(ePhase / 2.0) * Math.Tan(ePhase / 2.0) * Math.Log(1.0 / Math.Tan(ePhase / 4.0)));

      // Total moonlight
      double Em = ((2.0 * C * Rm * Rm) / (3.0 * d * d)) 
                  * (Eem + Esm * (1.0 - Math.Sin(mPhase / 2.0) * Math.Tan(mPhase / 2.0) * Math.Log(1.0 / Math.Tan(mPhase / 4.0))));

      // Convert irradiance [W/m²] to illuminance [lux] and illuminance to luminance [cd/m²].
      double luminance = Em * 683.0 / 3.14;

      // Handle twilight effects from moon.
      if (moonAngle < 0)
        luminance = luminance * Math.Exp(1.1247 * moonAngle);

      return luminance;
    }


    // For reference: Here is a simpler method to approximate the sun color.
    //private static Vector3F UpdateSunColor(Vector3F lightDirection, float turbidity)
    //{
    //  // turbidity = 2;

    //  // Code is from Hoffman scattering method in Shader X3/8.3. Also in 
    //  // Game Programming Methods where this source is cited:
    //  // This function taken from source available on 99 sig paper web page.
    //  // http://www.cs.utah.edu/vissim/papers/sunsky/code/RiSunConstants.C

    //  // Ratio of small to large particle sizes. (0:4, usually 1.3)
    //  const float alpha = 1.3f;

    //  //float cosineTheta = Math.Min(1, 0.2f + Vector3F.Dot(Vector3F.UnitY, -lightDirection));
    //  float cosineTheta = -lightDirection.Y;

    //  Vector3F lightColor = new Vector3F();
    //  if (!(cosineTheta < 0))
    //  {
    //    float theta = (float)Math.Acos(cosineTheta);

    //    // Amount of aerosols (water + dust)
    //    float beta = 0.04608365822050f * turbidity - 0.04586025928522f;

    //    // Rayleigh tau.
    //    float tauR;
    //    // Aersol tau.
    //    float tauA;

    //    float[] tau = new float[3];
    //    float m = (float)(1.0f / (cosineTheta + 0.15f * Math.Pow(93.885f - theta / ConstantsF.Pi * 180.0f, -1.253f)));
    //    // Relative Optical Mass

    //    // Wavelengths in µm
    //    float[] lambda = { 0.65f, 0.57f, 0.475f };

    //    for (int i = 0; i < 3; ++i)
    //    {
    //      // Rayleigh Scattering        
    //      tauR = (float)Math.Exp(-m * 0.008735f * Math.Pow(lambda[i], -4.08f));
    //      tauA = (float)Math.Exp(-m * beta * Math.Pow(lambda[i], -alpha));
    //      tau[i] = tauR * tauA;

    //      // TODO: if m < 0 tau[i] == 0?
    //    }

    //    lightColor = new Vector3F(tau[0], tau[1], tau[2]);
    //  }

    //  return lightColor;
    //}
  }
}
