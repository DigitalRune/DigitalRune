// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents sRGB encoding of color values.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SRgbEncoding : ColorEncoding
  {
    // sRGB in DigitalRune Graphics are color values in gamma space.
    // (Simple gamma curve, not the exact sRGB curve.)
  }
}
