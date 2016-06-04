// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Defines the upsampling filter that is used when combining the low-resolution, off-screen
  /// buffer with the scene.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [Obsolete("The enum UpsamplingFilter has been renamed to avoid confusion with the new UpsampleFilter. Use the new enum DigitalRune.Graphics.PostProcessing.UpsamplingMode instead.")]
  public enum UpsamplingFilter
  {
    /// <summary>
    /// Point upsampling. (Fastest, lowest quality)
    /// </summary>
    Point,

    /// <summary>
    /// Bilinear upsampling. (Fast, low quality)
    /// </summary>
    Linear,

    /// <summary>
    /// Joint (cross) bilateral upsampling. (Slow, best quality for surfaces)
    /// </summary>
    Bilateral,

    /// <summary>
    /// Nearest-depth upsampling. (Slow, best quality for particles and volumetric effects)
    /// </summary>
    NearestDepth
  }
}
