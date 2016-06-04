// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents RGBE encoding of color values.
  /// </summary>
  /// <remarks>
  /// The Radiance RGBE format stores a high-dynamic range RGB value as RGB with an exponent in the 
  /// alpha channel.
  /// </remarks>
  /// <seealso href="http://en.wikipedia.org/wiki/RGBE_image_format">RGBE image format</seealso>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class RgbeEncoding : ColorEncoding
  {
    // RGBE in DigitalRune Graphics stores color values in linear space.
  }
}
