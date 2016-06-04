// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Defines the texture format.
  /// </summary>
  public enum DRTextureFormat
  {
    /// <summary>
    /// The texture format of the input texture is not changed by the content processor.
    /// </summary>
    NoChange,

    /// <summary>
    /// The texture format of the input texture is converted to <strong>SurfaceFormat.Color</strong>
    /// (32-bit ARGB format with alpha, 8 bits per channel) by the content processor.
    /// </summary>
    Color,

    /// <summary>
    /// The texture format of the input texture is converted to an appropriate DXT compression by 
    /// the content processor. (If the input texture contains fractional alpha values, it is 
    /// converted to DXT5 format; otherwise it is converted to DXT1.)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    Dxt, // TODO: Rename to "Compressed"?

    /// <summary>
    /// The texture format of the input texture is converted to DXT5nm by the content processor.
    /// (This format reduces compression artifacts when storing normal maps. The x-component of the
    /// normal is stored in the Alpha channel and the y-component is stored in the Green channel.
    /// The z-component needs to be reconstructed in the pixel shader.)
    /// </summary>
    Normal,

    /// <summary>
    /// The texture format of the input texture is converted to DXT5nm by the content processor.
    /// (This format reduces compression artifacts when storing normal maps. The x-component of the
    /// normal is stored in the Alpha channel and the <strong>inverted</strong> y-component is 
    /// stored in the Green channel. The z-component needs to be reconstructed in the pixel shader.)
    /// </summary>
    NormalInvertY,
  }
}
