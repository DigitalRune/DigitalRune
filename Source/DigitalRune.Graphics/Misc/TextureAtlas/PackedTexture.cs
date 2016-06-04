// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Defines a texture which has been packed into a texture atlas.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A texture atlas is a large image containing a collection of textures. It is often more 
  /// efficient to pack multiple textures into a texture atlas which can be treated as a single unit
  /// by the graphics hardware. The <see cref="PackedTexture"/> represents a single texture within 
  /// the texture atlas.
  /// </para>
  /// <para>
  /// <strong>Tile Sets:</strong><br/>
  /// A packed texture can further be divided into tiles (= tile set). A tile set is a collection
  /// of images ("sprites" or "tiles"). All tiles have the same size and are packed in a regular
  /// grid: <see cref="NumberOfColumns"/> defines the number of tiles in x direction and 
  /// <see cref="NumberOfRows"/> defines the number of tiles in y direction. 
  /// </para>
  /// <para>
  /// <strong>2D Animations:</strong><br/>
  /// A tile set can be used for 2D animations ("flipbook animations", "sprite sheet animations"): 
  /// The tile set contains a sequence of images (animation frames). Cycling through the images in 
  /// rapid succession creates the illusion of movement.
  /// </para>
  /// <para>
  /// When a tile set is used for animation, the top-left tile contains the first animation frame, 
  /// the second tile in the upper row contains the second animation frame, and so on. The 
  /// bottom-right tile contains the last animation frame.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> Most renderers require that the animation frames are tightly 
  /// packed and the tile set does not contain empty tiles. For example, an animation consisting of 
  /// 3 frames can be packed as a 1x3 or 3x1 tile set, but not as 2x2!
  /// </para>
  /// </remarks>
  public class PackedTexture : INamedObject
  {
    // Note: XNA textures implement CompareTo(). Unfortunately, the method is internal.
    // As a workaround for sorting: Assign a unique ID to each texture.

    // TODO: PackedTexture only containing name --> unresolved?
    //       PackedTexture with properties set --> resolved?

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the name of the texture.
    /// </summary>
    /// <value>The name of the texture.</value>
    public string Name { get; set; }


    /// <summary>
    /// Gets or sets the texture atlas containing the packed texture.
    /// </summary>
    /// <value>The texture atlas that contains the packed texture.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public Texture2D TextureAtlas
    {
      get { return TextureAtlasEx.Resource; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        TextureAtlasEx = Texture2DEx.From(value);
      }
    }


    /// <summary>
    /// Gets the <see cref="Texture2DEx"/>.
    /// </summary>
    /// <value>The <see cref="Texture2DEx"/>.</value>
    internal Texture2DEx TextureAtlasEx { get; private set; }


    /// <summary>
    /// Gets or sets the offset of the packed texture in the texture atlas in UV coordinates.
    /// </summary>
    /// <value>
    /// The offset of the packed texture in the texture atlas. The offset is given in UV coordinates
    /// where (0, 0) is the upper-left corner and (1, 1) is the lower-right corner of the texture 
    /// atlas.
    /// </value>
    public Vector2F Offset { get; set; }


    /// <summary>
    /// Gets or sets the scale of the packed texture relative to the texture atlas.
    /// </summary>
    /// <value>
    /// The scale of the packed texture in the texture atlas. The scale is relative to the texture
    /// atlas. Example: a value of (0.5, 0.5) indicates that the packed texture is half the width 
    /// and height of the texture atlas.
    /// </value>
    public Vector2F Scale { get; set; }


    #region ----- Tile Set -----

    /// <summary>
    /// Gets or sets the number of columns in the tile set.
    /// </summary>
    /// <value>The number of columns in the tile set. The default value is 1.</value>
    /// <seealso cref="NumberOfRows"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range.
    /// </exception>
    public int NumberOfColumns
    {
      get { return _numberOfColumns; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _numberOfColumns = value;
      }
    }
    private int _numberOfColumns;


    /// <summary>
    /// Gets or sets the number of rows in the tile set.
    /// </summary>
    /// <value>The number of rows in the tile set. The default value is 1.</value>
    /// <seealso cref="NumberOfColumns"/>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range.
    /// </exception>
    public int NumberOfRows
    {
      get { return _numberOfRows; }
      set
      {
        if (value < 0)
          throw new ArgumentOutOfRangeException("value");

        _numberOfRows = value;
      }
    }
    private int _numberOfRows;
    #endregion

    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="PackedTexture" /> class for a single texture.
    /// </summary>
    /// <param name="texture">The texture atlas that contains the packed texture.</param>
    public PackedTexture(Texture2D texture)
      : this((texture != null) ? texture.Name : null, texture, new Vector2F(0, 0), new Vector2F(1, 1), 1, 1)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PackedTexture" /> class.
    /// </summary>
    /// <param name="name">
    /// The original asset name of the packed texture. Can be <see langword="null"/> or empty.
    /// </param>
    /// <param name="texture">The texture atlas that contains the packed texture.</param>
    /// <param name="offset">The UV offset of the packed texture in the texture atlas.</param>
    /// <param name="scale">The scale of the packed texture relative to the texture atlas.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public PackedTexture(string name, Texture2D texture, Vector2F offset, Vector2F scale)
      : this(name, texture, offset, scale, 1, 1)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PackedTexture" /> class.
    /// </summary>
    /// <param name="name">
    /// The original asset name of the packed texture. Can be <see langword="null"/> or empty.
    /// </param>
    /// <param name="texture">The texture atlas that contains the packed texture.</param>
    /// <param name="offset">The UV offset of the packed texture in the texture atlas.</param>
    /// <param name="scale">The scale of the packed texture relative to the texture atlas.</param>
    /// <param name="numberOfColumns">The number of columns in the tile set.</param>
    /// <param name="numberOfRows">The number of rows in the tile set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfColumns"/> or <paramref name="numberOfRows"/> is out of range.
    /// </exception>
    public PackedTexture(string name, Texture2D texture, Vector2F offset, Vector2F scale, int numberOfColumns, int numberOfRows)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (numberOfColumns < 1)
        throw new ArgumentOutOfRangeException("numberOfColumns");
      if (numberOfRows < 1)
        throw new ArgumentOutOfRangeException("numberOfRows");

      Name = name;
      TextureAtlas = texture;
      Offset = offset;
      Scale = scale;
      _numberOfColumns = numberOfColumns;
      _numberOfRows = numberOfRows;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Converts texture coordinates.
    /// </summary>
    /// <param name="texCoord">The texture coordinates of the unpacked texture.</param>
    /// <param name="animationTime">
    /// For tile sets: The normalized animation time. (0 = start of the animation, 1 = end of 
    /// animation)
    /// </param>
    /// <returns>The texture coordinate in the texture atlas.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public Vector2F GetTextureCoordinates(Vector2F texCoord, float animationTime)
    {
      // Same function as used in Billboard.fx.
      float tx = _numberOfColumns;
      float ty = _numberOfRows;
      float itx = 1 / tx;
      float ity = 1 / ty;

      // Texture coordinates relative to first tile:
      texCoord = new Vector2F(texCoord.X * itx, texCoord.Y * ity);

      // Calculate and apply offset of current tile.
      //
      // Wanted:
      //   offsetX ... x-offset of current frame relative to tile sheet.
      //   offsetY ... y-offset of current frame relative to tile sheet.
      //
      // When tile sheet is flat list:
      //   index = floor(time * numberOfTiles)
      // When tile sheet is stacked:
      //   indexY = floor(time * ty)
      //   indexX = ?
      // Conversion from stacked to flat list:
      //   index = indexY * tx + indexX
      //
      // => indexX = index - indexY * tx
      //    Size of a tile = 1/tx = itx
      //    offsetX = indexX * itx 
      //            = index * itx - indexY * tx * itx
      //            = index * itx - indexY
      //            = floor(time * numberOfTiles) / tx - floor(time * ty)
      //            = floor(time * tx * ty) * itx - floor(time * ty)
      float offsetX = (float)Math.Floor(animationTime * tx * ty) * itx - (float)Math.Floor(animationTime * ty);

      // When tile sheet is packed from top to bottom.
      float offsetY = (float)Math.Floor(animationTime * ty) * ity;

      // Or, when tile sheet is packed from bottom to top.
      // float offsetY = 1 - ity - floor(time * ty) * ity;

      texCoord += new Vector2F(offsetX, offsetY);
      return texCoord * Scale + Offset;
    }


    /// <summary>
    /// Gets the bounds of the packed texture in pixel.
    /// </summary>
    /// <param name="animationTime">
    /// For tile sets: The normalized animation time. (0 = start of the animation, 1 = end of 
    /// animation)
    /// </param>
    /// <returns>The bounds of the packed texture in pixel.</returns>
    public Rectangle GetBounds(float animationTime)
    {
      // Same as above in GetTextureCoordinates().
      float tx = _numberOfColumns;
      float ty = _numberOfRows;
      float itx = 1 / tx;
      float ity = 1 / ty;
      float offsetX = (float)Math.Floor(animationTime * tx * ty) * itx - (float)Math.Floor(animationTime * ty);
      float offsetY = (float)Math.Floor(animationTime * ty) * ity;

      // Get pixel bounds.
      float pixelWidth = TextureAtlas.Width;
      float pixelHeight = TextureAtlas.Height;
      int x = (int)((offsetX * Scale.X + Offset.X) * pixelWidth);
      int y = (int)((offsetY * Scale.Y + Offset.Y) * pixelHeight);
      int w = (int)(itx * Scale.X * pixelWidth);
      int h = (int)(ity * Scale.Y * pixelHeight);
      return new Rectangle(x, y, w, h);
    }
    #endregion
  }
}
