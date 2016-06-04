// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Specifies whether the theme image is a tile that is repeated and how.
  /// </summary>
  public enum TileMode
  {
    // TODO: Add tile modes FlipX, FlipY, FlipXY, TileXFlipY, FlipYTileX.

    /// <summary>
    /// The image is not repeated. Only one copy of the image is drawn.
    /// </summary>
    None,

    /// <summary>
    /// The image is repeated horizontally only.
    /// </summary>
    TileX,

    /// <summary>
    /// The image is repeated vertically only.
    /// </summary>
    TileY,

    /// <summary>
    /// The image is repeated both horizontally and vertically.
    /// </summary>
    TileXY,
  }
}
