// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;


namespace DigitalRune.Game.UI.Rendering
{
  /// <summary>
  /// Represents an image of the UI theme.
  /// </summary>
  /// <remarks>
  /// The image is a region in a texture atlas of the theme. Images support 9-grid scaling.
  /// </remarks>
  public class ThemeImage : INamedObject
  {
    /// <summary>
    /// Gets or sets the name of the image.
    /// </summary>
    /// <value>The name of the image.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the texture atlas containing the image.
    /// </summary>
    /// <value>
    /// The texture atlas containing the image. (Can be <see langword="null"/> to use the default 
    /// texture atlas.)
    /// </value>
    public ThemeTexture Texture { get; set; }


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
    /// The tile mode that defines whether the image is repeated and how. The default value is
    /// <see cref="Rendering.TileMode.None"/>.
    /// </value>
    /// <remarks>
    /// Note that, when either the <see cref="HorizontalAlignment"/> or the 
    /// <see cref="VerticalAlignment"/> is set to <see cref="UI.HorizontalAlignment.Stretch"/> then
    /// the image is never tiled.
    /// </remarks>
    public TileMode TileMode { get; set; }


    /// <summary>
    /// Gets or sets the border that defines the 9-grid layout for image stretching.
    /// </summary>
    /// <value>The border that defines the 9-grid layout.</value>
    /// <remarks>
    /// When the alignment is set to <i>Stretch</i>, the image will be stretched to fill the control 
    /// area. A typical <i>9-grid scaling</i> is applied and the <see cref="Border"/> defines the 
    /// left/right/top/bottom margins. The left-top, left-bottom, right-top and right-bottom parts 
    /// of the image will not be stretched. The center-top and center-bottom parts will be stretched 
    /// horizontally. The left-center and right-center parts will be stretched vertically. The 
    /// center part will be stretched horizontally and vertically.
    /// </remarks>
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
