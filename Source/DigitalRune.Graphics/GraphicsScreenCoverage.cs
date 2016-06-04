// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines whether a <see cref="GraphicsScreen"/> covers the screens behind it.
  /// </summary>
  /// <remarks>
  /// When not sure what to set, use <see cref="Partial"/> (safe option).
  /// </remarks>
  public enum GraphicsScreenCoverage
  {
    /// <summary>
    /// The <see cref="GraphicsScreen"/> does not cover the entire view. (The screen draws only to 
    /// a certain regions or some pixels are transparent). Screens in the background are partially 
    /// visible.
    /// </summary>
    Partial,

    /// <summary>
    /// The <see cref="GraphicsScreen"/> is fully opaque and covers the entire view. Screens in the 
    /// background are completely hidden.
    /// </summary>
    Full,
  }
}
