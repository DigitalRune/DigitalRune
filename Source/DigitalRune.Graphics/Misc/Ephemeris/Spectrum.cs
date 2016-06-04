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
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents a spectrum of electromagnetic energy.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This class represents a spectrum of the visible light. The array <see cref="Powers"/> contains
  /// <see cref="NumberOfSamples"/> elements. Each element describes the power of a frequency in the
  /// range of visible light (380 to 780 nm). The interval between two samples is 
  /// <see cref="SampleWidth"/>.
  /// </para>
  /// <para>
  /// This class also models the passage of this energy through the earth atmosphere under given
  /// conditions, and converts the spectrum to XYZ color data. See 
  /// <see cref="ApplyAtmosphericTransmittance"/>.
  /// </para>
  /// </remarks>
  internal partial class Spectrum
  {
    // References: 
    // - "Physically-Based Outdoor Scene Lighting", Game Engine Gems 1.
    // - Bird, Richard E., Riordan, Carol, Simple Solar Spectral Model for Direct and Diffuse 
    //   Irradiance on Horizontal and Tilted Planes at the Earth's Surface for Cloudless Atmospheres, 
    //   Journal of Applied Meteorology 1986 25: 87-97


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The number of samples.
    /// </summary>
    public const int NumberOfSamples = 81;

    /// <summary>
    /// The interval between two frequency samples.
    /// </summary>
    public const int SampleWidth = 5;

    // A table used to convert the samples to CIE XYZ values.
    // (Note: The luminance should be equal to the photometric curve - see "Real-Time Rendering", pp. 209)
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    private readonly float[,] SampleToXYZ =
    {
      { 0.0014f, 0.0000f, 0.0065f }, { 0.0022f, 0.0001f, 0.0105f }, { 0.0042f, 0.0001f, 0.0201f },
      { 0.0076f, 0.0002f, 0.0362f }, { 0.0143f, 0.0004f, 0.0679f }, { 0.0232f, 0.0006f, 0.1102f },
      { 0.0435f, 0.0012f, 0.2074f }, { 0.0776f, 0.0022f, 0.3713f }, { 0.1344f, 0.0040f, 0.6456f },
      { 0.2148f, 0.0073f, 1.0391f }, { 0.2839f, 0.0116f, 1.3856f }, { 0.3285f, 0.0168f, 1.6230f },
      { 0.3483f, 0.0230f, 1.7471f }, { 0.3481f, 0.0298f, 1.7826f }, { 0.3362f, 0.0380f, 1.7721f },
      { 0.3187f, 0.0480f, 1.7441f }, { 0.2908f, 0.0600f, 1.6692f }, { 0.2511f, 0.0739f, 1.5281f },
      { 0.1954f, 0.0910f, 1.2876f }, { 0.1421f, 0.1126f, 1.0419f }, { 0.0956f, 0.1390f, 0.8130f },
      { 0.0580f, 0.1693f, 0.6162f }, { 0.0320f, 0.2080f, 0.4652f }, { 0.0147f, 0.2586f, 0.3533f },
      { 0.0049f, 0.3230f, 0.2720f }, { 0.0024f, 0.4073f, 0.2123f }, { 0.0093f, 0.5030f, 0.1582f },
      { 0.0291f, 0.6082f, 0.1117f }, { 0.0633f, 0.7100f, 0.0782f }, { 0.1096f, 0.7932f, 0.0573f },
      { 0.1655f, 0.8620f, 0.0422f }, { 0.2257f, 0.9149f, 0.0298f }, { 0.2904f, 0.9540f, 0.0203f },
      { 0.3597f, 0.9803f, 0.0134f }, { 0.4334f, 0.9950f, 0.0087f }, { 0.5121f, 1.0000f, 0.0057f },
      { 0.5945f, 0.9950f, 0.0039f }, { 0.6784f, 0.9786f, 0.0027f }, { 0.7621f, 0.9520f, 0.0021f },
      { 0.8425f, 0.9154f, 0.0018f }, { 0.9163f, 0.8700f, 0.0017f }, { 0.9786f, 0.8163f, 0.0014f },
      { 1.0263f, 0.7570f, 0.0011f }, { 1.0567f, 0.6949f, 0.0010f }, { 1.0622f, 0.6310f, 0.0008f },
      { 1.0456f, 0.5668f, 0.0006f }, { 1.0026f, 0.5030f, 0.0003f }, { 0.9384f, 0.4412f, 0.0002f },
      { 0.8544f, 0.3810f, 0.0002f }, { 0.7514f, 0.3210f, 0.0001f }, { 0.6424f, 0.2650f, 0.0000f },
      { 0.5419f, 0.2170f, 0.0000f }, { 0.4479f, 0.1750f, 0.0000f }, { 0.3608f, 0.1382f, 0.0000f },
      { 0.2835f, 0.1070f, 0.0000f }, { 0.2187f, 0.0816f, 0.0000f }, { 0.1649f, 0.0610f, 0.0000f },
      { 0.1212f, 0.0446f, 0.0000f }, { 0.0874f, 0.0320f, 0.0000f }, { 0.0636f, 0.0232f, 0.0000f },
      { 0.0468f, 0.0170f, 0.0000f }, { 0.0329f, 0.0119f, 0.0000f }, { 0.0227f, 0.0082f, 0.0000f },
      { 0.0158f, 0.0057f, 0.0000f }, { 0.0114f, 0.0041f, 0.0000f }, { 0.0081f, 0.0029f, 0.0000f },
      { 0.0058f, 0.0021f, 0.0000f }, { 0.0041f, 0.0015f, 0.0000f }, { 0.0029f, 0.0010f, 0.0000f },
      { 0.0020f, 0.0007f, 0.0000f }, { 0.0014f, 0.0005f, 0.0000f }, { 0.0010f, 0.0004f, 0.0000f },
      { 0.0007f, 0.0002f, 0.0000f }, { 0.0005f, 0.0002f, 0.0000f }, { 0.0003f, 0.0001f, 0.0000f },
      { 0.0002f, 0.0001f, 0.0000f }, { 0.0002f, 0.0001f, 0.0000f }, { 0.0001f, 0.0000f, 0.0000f },
      { 0.0001f, 0.0000f, 0.0000f }, { 0.0001f, 0.0000f, 0.0000f }, { 0.0000f, 0.0000f, 0.0000f }
    };
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the array of spectral powers, from 380 - 780 nm sampled at 5 nm intervals.
    /// </summary>
    /// <value>
    /// The array of spectral powers, from 380 - 780 nm sampled at 5 nm intervals.
    /// </value>
    public float[] Powers { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Spectrum"/> class.
    /// </summary>
    public Spectrum()
    {
      Powers = new float[NumberOfSamples];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the spectrum to XYZ color information.
    /// </summary>
    /// <returns>The XYZ color in [lux] (illuminance).</returns>
    public Vector3F ToXYZ()
    {
      Vector3F colorXYZ = new Vector3F(0, 0, 0);
      for (int i = 1; i < NumberOfSamples; i++)
      {
        colorXYZ.X += Powers[i] * SampleToXYZ[i, 0];
        colorXYZ.Y += Powers[i] * SampleToXYZ[i, 1];
        colorXYZ.Z += Powers[i] * SampleToXYZ[i, 2];
      }

      // Convert from irradiance to illuminance. (For green 1 W/m² = 683 lux; for other frequencies
      // it is less.)
      return colorXYZ * 683.0f;
    }


    // Multiplies two Spectrums together, by multiplying the spectral powers at each wavelength sample.
    //static Spectrum Multiply(Spectrum a, Spectrum s)
    //{
    //  Spectrum result = new Spectrum();

    //  for (int i = 0; i < NumberOfSamples; i++)
    //  {
    //    result.Powers[i] = a.Powers[i] * s.Powers[i];
    //  }

    //  return result;
    //}


    ///// <summary>
    ///// Multiplies the <see cref="Spectrum"/> by a scalar.
    ///// </summary>
    ///// <param name="s">The scalar.</param>
    //public void Multiply(float s)
    //{
    //  for (int i = 0; i < NumberOfSamples; i++)
    //    Powers[i] = Powers[i] * s;
    //}


    /// <summary>
    /// Computes direct and indirect light information when this spectrum passes through the earth's
    /// atmosphere.
    /// </summary>
    /// <param name="zenithAngle">
    /// The angle between the zenith and the direction of the light source emitting the simulated 
    /// spectrum in radian.
    /// </param>
    /// <param name="turbidity">
    /// The simulated atmospheric turbidity in the range [1.8, 20.0]. A turbidity of 2 describes a 
    /// clear day whereas a turbidity of 20 represents thick haze. A commonly used value is 2.2.
    /// </param>
    /// <param name="altitude">The simulated altitude in meters above mean sea level.</param>
    /// <param name="directIrradiance">
    /// Output: The spectral energy directly from the light source that survives transmission 
    /// through the atmosphere.
    /// </param>
    /// <param name="scatteredIrradiance">
    /// Output: The spectral energy scattered by the atmosphere, which makes up "skylight" (ambient
    /// light from the sky).
    /// </param>
    /// <remarks>
    /// <para>
    /// This method simulates the passage of this spectrum through earth's atmosphere, employing the
    /// National Renewable Energy Lab's "Bird model". Two new spectra, representing the direct and
    /// scattered irradiance resulting from passage through the atmosphere, are returned.
    /// </para>
    /// <para>
    /// If the light source is below the horizon, this model returns 0. Thus, it cannot be used to
    /// compute twilight.
    /// </para>
    /// </remarks>
    public void ApplyAtmosphericTransmittance(double zenithAngle, double turbidity, double altitude,
                                              Spectrum directIrradiance, Spectrum scatteredIrradiance)
    {
      //  For more info, see the reference in the notes of this class.

      double[] Ao =  
      {
        0, 0, 0, 0,                                                           // 380 - 395
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0.003,                                  // 400 - 450
        0.004, 0.006, 0.007, 0.009, 0.011, 0.014, 0.017, 0.021, 0.025, 0.03,  // 455 - 500
        0.035, 0.04, 0.044, 0.048, 0.055, 0.063, 0.071, 0.075, 0.08, 0.085,   // 505 - 550
        0.091, 0.12, 0.12, 0.12, 0.12, 0.12, 0.12, 0.12, 0.119, 0.12,         // 555 - 600
        0.12, 0.12, 0.10, 0.09, 0.09, 0.085, 0.08, 0.075, 0.07, 0.07,         // 605 - 650
        0.065, 0.06, 0.055, 0.05, 0.045, 0.04, 0.035, 0.028, 0.25, 0.023,     // 655 - 700
        0.02, 0.018, 0.016, 0.012, 0.012, 0.012, 0.012, 0.01, 0.01, 0.01,     // 705 - 750
        0.008, 0.007, 0.006, 0.005, 0.003, 0
      };

      double[] Au = 
      {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 380 - 500
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 505 - 550
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 550 - 600
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 605 - 650
        0, 0, 0, 0, 0, 0, 0, 0.15, 0, 0,                                            // 655 - 700
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 705 - 750
        0, 4.0, 0, 0, 0, 0,                                                         // 755 - 780
      };

      double[] Aw = 
      {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  // 380 - 500
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 505 - 550
        0, 0, 0, 0, 0, 0, 0, 0.075, 0, 0,                                           // 550  -600
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0,                                               // 605 - 650
        0, 0, 0, 0, 0, 0, 0, 0.016, 0.015, 0.014,                                   // 655 - 700
        0.013, 0.0125, 1.8, 2.3, 2.5, 2.3, 1.8, 0.061, 0.003, 0.0008,               // 705 - 750
        0.0001, 0.00001, 0.00001, 0.0001, 0.0003, 0.0006,                           // 755 - 780
      };

      double beta = 0.04608 * turbidity - 0.04586;
      double cosZenith = Math.Cos(zenithAngle);
      double zenithDeg = MathHelper.ToDegrees(zenithAngle);

      const double modelLimit = 90.0; //93.885
      if (zenithDeg < modelLimit)
      {
        // NREL air mass model
        //double m = 1.0 / (cosZenith + 0.15 * pow(93.885 - zenithDeg, -1.253));

        // International Comet Quarterly air mass model
        //double m = 1.0 / (cosZenith + 0.025 * exp(-11 * cosZenith));

        // SPECTRL2 air mass model (also adopted by CIE)
        double m = 1.0 / (cosZenith + 0.50572 * Math.Pow(93.885 - zenithDeg, -1.6364));

        // Account for high altitude. As you lose atmosphere, less scattering occurs.
        const double H = 8435.0; // pressure scale height
        double isothermalEffect = Math.Exp(-(altitude / H));
        m *= isothermalEffect;

        // ozone mass
        const double O3 = 0.35; // ozone amount, atm-cm
        double Mo = 1.003454 / Math.Sqrt(cosZenith * cosZenith + 0.006908);

        const double W = 2.5;         // precipitable water vapor (cm)
        const double omega = 0.945;   // single scattering albedo, 0.4 microns
        const double omegap = 0.095;  // Wavelength variation factor
        const double asym = 0.65;     // aerosol asymmetry factor

        double alg = Math.Log(1.0 - asym);
        double afs = alg * (1.459 + alg * (0.1595 + alg * 0.4129));
        double bfs = alg * (0.0783 + alg * (-0.3824 - alg * 0.5874));
        double fsp = 1.0 - 0.5 * Math.Exp((afs + bfs / 1.8) / 1.8);
        double fs = 1.0 - 0.5 * Math.Exp((afs + bfs * cosZenith) * cosZenith);

        for (int i = 0; i < NumberOfSamples; i++)
        {
          double um = 0.380 + (i * 0.005);

          // Rayleigh scattering
          double Tr = Math.Exp(-m / (Math.Pow(um, 4.0) * (115.6406 - (1.335 / (um * um)))));

          // Aerosols
          double a = um < 0.5 ? 1.0274 : 1.2060;
          double c1 = beta * Math.Pow(2.0 * um, -a);
          double Ta = Math.Exp(-c1 * m);

          // Water vapor
          double aWM = Aw[i] * W * m;
          double Tw = Math.Exp(-0.2385 * aWM / Math.Pow(1.0 + 20.07 * aWM, 0.45));

          // Ozone
          double To = Math.Exp(-Ao[i] * O3 * Mo);

          // Mixed gas is only important in infrared
          double Tm = Math.Exp((-1.41 * Au[i] * m) / Math.Pow(1.0 + 118.3 * Au[i] * m, 0.45));

          // Aerosol scattering
          double logUmOver4 = Math.Log(um / 0.4);
          double omegl = omega * Math.Exp(-omegap * logUmOver4 * logUmOver4);
          double Tas = Math.Exp(-omegl * c1 * m);

          // Aerosol absorptance
          double Taa = Math.Exp((omegl - 1.0) * c1 * m);

          // Primed Rayleigh scattering (m = 1.8)
          double Trp = Math.Exp(-1.8 / (um * um * um * um) * (115.6406 - 1.3366 / (um * um)));

          // Primed water vapor scattering
          double Twp = Math.Exp(-0.4293 * Aw[i] * W / Math.Pow((1.0 + 36.126 * Aw[i] * W), 0.45));

          // Mixed gas
          double Tup = Math.Exp(-2.538 * Au[i] / Math.Pow((1.0 + 212.94 * Au[i]), 0.45));

          // Primed aerosol scattering
          double Tasp = Math.Exp(-omegl * c1 * 1.8);

          // Primed aerosol absorptance
          double Taap = Math.Exp((omegl - 1.0) * c1 * 1.8);

          // Direct energy
          double xmit = Tr * Ta * Tw * To * Tm;

          directIrradiance.Powers[i] = (float)(Powers[i] * xmit);

          // diffuse energy
          double c2 = To * Tw * cosZenith * Taa;
          double c4 = 1.0;
          if (um <= 0.45)
          {
            c4 = Math.Pow((um + 0.55), 1.8);
          }

          double rhoa = Tup * Twp * Taap * (0.5 * (1.0 - Trp) + (1.0 - fsp) * Trp * (1.0 - Tasp));
          const double rho = 0.3; // ground albedo. Set less for ocean, more for snow.
          double dray = c2 * (1.0 - Math.Pow(Tr, 0.95)) / 2.0;
          double daer = c2 * Math.Pow(Tr, 1.5) * (1.0 - Tas) * fs;
          double drgd = (directIrradiance.Powers[i] * cosZenith + dray + daer) * rho * rhoa / (1.0 - rho * rhoa);

          scatteredIrradiance.Powers[i] = (float)(Powers[i] * (dray + daer + drgd) * c4);
          if (scatteredIrradiance.Powers[i] < 0)
            scatteredIrradiance.Powers[i] = 0;
        }
      }
      else
      {
        for (int i = 0; i < NumberOfSamples; i++)
        {
          directIrradiance.Powers[i] = 0;
          scatteredIrradiance.Powers[i] = 0;
        }
      }
    }
    #endregion
  }
}
