// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Represents an image of the UI theme.
  /// </summary>
  public class ThemeImageContent : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the image.
    /// </summary>
    /// <value>The name of the image.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the name of the texture atlas containing the image.
    /// </summary>
    /// <value>The name of the texture atlas containing the image.</value>
    public string Texture { get; set; }


    /// <summary>
    /// Gets or sets the source rectangle of the image in the texture atlas of the theme.
    /// </summary>
    /// <value>The source rectangle.</value>
    public Rectangle SourceRectangle { get; set; }


    /// <summary>
    /// Gets or sets the margin (left, top, right, bottom).
    /// </summary>
    /// <value>
    /// The margin (left, top, right, bottom). Can be negative to draw outside of the control area.
    /// </value>
    public Vector4F Margin { get; set; }


    /// <summary>
    /// Gets or sets the horizontal alignment.
    /// </summary>
    /// <value>The horizontal alignment.</value>
    public HorizontalAlignment HorizontalAlignment { get; set; }


    /// <summary>
    /// Gets or sets the vertical alignment.
    /// </summary>
    /// <value>The vertical alignment.</value>
    public VerticalAlignment VerticalAlignment { get; set; }


    /// <summary>
    /// Gets or sets the tile mode.
    /// </summary>
    /// <value>
    /// The tile mode that defines whether the image is repeated and how.
    /// </value>
    public TileMode TileMode { get; set; }


    /// <summary>
    /// Gets or sets the border that defines the 9-grid layout for image stretching.
    /// </summary>
    /// <value>The border that defines the 9-grid layout.</value>
    public Vector4F Border { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether this image is drawn on top of the control.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this image is drawn on top of the control; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    public bool IsOverlay { get; set; }


    /// <summary>
    /// Gets or sets the tint color.
    /// </summary>
    /// <value>The tint color.</value>
    public Color Color { get; set; }
  }
}
