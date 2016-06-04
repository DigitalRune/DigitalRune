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


namespace DigitalRune.Graphics
{
  partial class Spectrum
  {
    /// <summary>
    /// Sets this instance to the spectrum of light emanating from the moon using an approximation.
    /// </summary>
    /// <param name="moonLuminance">The moon luminance.</param>
    /// <remarks>
    /// This lunar spectrum is a simple approximation that just ramps up linearly from 380-780 nm
    /// from 0.7 to 1.35. This spectrum is multiplied by the average luminance of the moon for a
    /// specific phase and distance.
    /// </remarks>
    public void SetLunarSpectrum(float moonLuminance)
    {
      const double minLuminance = 0.7;
      const double maxLuminance = 1.35;

      float total = 0;
      for (int i = 0; i < NumberOfSamples; i++)
      {
        double a = i / (double)NumberOfSamples;
        Powers[i] = (float)(minLuminance * (1.0 - a) + maxLuminance * a);
        total += Powers[i];
      }

      // Apply moon luminance and normalize.
      for (int i = 0; i < NumberOfSamples; i++)
        Powers[i] *= moonLuminance / total;
    }
  }
}
