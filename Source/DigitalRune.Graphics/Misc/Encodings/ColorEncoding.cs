// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines how a color value is encoded in a texel of a texture.
  /// </summary>
  public abstract class ColorEncoding
  {
    /// <summary>
    /// Linear (not encoded) RGB values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RgbEncoding Rgb = new RgbEncoding();


    /// <summary>
    /// sRGB encoding of color values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly SRgbEncoding SRgb = new SRgbEncoding();


    /// <summary>
    /// RGBE encoding of color values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RgbeEncoding Rgbe = new RgbeEncoding();


    /// <summary>
    /// RGBM encoding of color values with a maximum value of 50.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly RgbmEncoding Rgbm = new RgbmEncoding(50);

    // Notes regarding maximum value:
    // According to Naty Hoffman, "Physically Based Shading Models in Film and Game Production", SIGGRAPH 2010 Course
    // with careful management of lighting and exposure HDR requires not more precision
    // than ~25-100X display white. (A factor of ~4-8X in gamma space.)
    // Environment maps goes to ~20-50X display white before saturation. (A factor
    // of ~4-6X in gamma space.)

    // According to Chris Tchou, "HDR The Bungie Way", Gamefest 2006 presentation
    // 8-bit RGBM gives is enough to store 5-9 stops of exposure (= 32-512X in linear space).

    // Brian Karis (http://graphicrants.blogspot.co.at/2009/04/rgbm-color-encoding.html)
    // uses a factor of 6X in gamma space (= 51.5X in linear space).


    /// <summary>
    /// LogLuv encoding of color values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
    public static readonly LogLuvEncoding LogLuv = new LogLuvEncoding();
  }
}
