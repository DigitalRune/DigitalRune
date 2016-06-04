// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents a font of the UI theme.
  /// </summary>
  public class ThemeFont : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the font.
    /// </summary>
    /// <value>The name of the font.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this font is the default font.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this font is the default font; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsDefault { get; set; }


    /// <summary>
    /// Gets or sets the font.
    /// </summary>
    /// <value>The font.</value>
    public SpriteFont Font { get; set; }
  }
}
