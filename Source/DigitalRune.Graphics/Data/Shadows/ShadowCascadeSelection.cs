// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines how the cascade of a cascaded shadow map is selected.
  /// </summary>
  public enum ShadowCascadeSelection
  {
    /// <summary>
    /// The shadow cascade is selected using the fastest available method.
    /// </summary>
    Fast,

    /// <summary>
    /// The optimal shadow cascade is selected. (Best visual result, but with seams between
    /// cascades.)
    /// </summary>
    Best,

    /// <summary>
    /// The optimal shadow cascade is selected using dithering to hide transitions between cascades.
    /// (Best visual result, but slower.)
    /// </summary>
    BestDithered,

    ///// <summary>
    ///// The optimal shadow cascade is selected using interpolation to hide transitions between
    ///// cascades. (Best visual result, but slowest.)
    ///// </summary>
    //BestInterpolated,
  }
}
