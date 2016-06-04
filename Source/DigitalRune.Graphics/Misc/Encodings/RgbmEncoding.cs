// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Represents RGBM encoding of color values.
  /// </summary>
  /// <remarks>
  /// The RGBM format stores a high-dynamic range RGB value as RGB with a multiplier in the alpha 
  /// channel.
  /// </remarks>
  /// <seealso href="http://graphicrants.blogspot.com/2009/04/rgbm-color-encoding.html">RGBM color encoding</seealso>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class RgbmEncoding : ColorEncoding
  {
    // RGBM in DigitalRune Graphics stores color values in gamma space.


    /// <summary>
    /// Gets the maximum value for R, G, and B in linear color space.
    /// </summary>
    /// <value>The maximum value for R, G, and B in linear color space.</value>
    public float Max { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="RgbmEncoding" /> class.
    /// </summary>
    /// <param name="max">The maximum value for R, G and B in linear color space.</param>
    public RgbmEncoding(float max)
    {
      Max = max;
    }
  }
}
