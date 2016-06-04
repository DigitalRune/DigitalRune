// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Describes a texture resource.
  /// </summary>
  /// <seealso cref="Texture"/>
  internal struct TextureDescription : IEquatable<TextureDescription>
  {
    private TextureDimension _dimension;
    private int _width;
    private int _height;
    private int _depth;
    private int _mipLevels;
    private int _arraySize;
    private DataFormat _format;


    /// <summary>
    /// Gets or sets the dimension of the texture.
    /// </summary>
    /// <value>The dimension of the texture.</value>
    public TextureDimension Dimension
    {
      get { return _dimension; }
      set { _dimension = value; }
    }


    /// <summary>
    /// Gets or sets the width of the texture.
    /// </summary>
    /// <value>The width of the texture.</value>
    public int Width
    {
      get { return _width; }
      set { _width = value; }
    }


    /// <summary>
    /// Gets or sets the height of the texture.
    /// </summary>
    /// <value>The height of the texture.</value>
    public int Height
    {
      get { return _height; }
      set { _height = value; }
    }


    /// <summary>
    /// Gets or sets the depth of the texture.
    /// </summary>
    /// <value>The depth of the texture.</value>
    public int Depth
    {
      get { return _depth; }
      set { _depth = value; }
    }


    /// <summary>
    /// Gets or sets the number of mipmap levels in the texture.
    /// </summary>
    /// <value>The number of mipmap levels in the texture.</value>
    public int MipLevels
    {
      get { return _mipLevels; }
      set { _mipLevels = value; }
    }


    /// <summary>
    /// Gets or sets the number of textures in the texture array.
    /// </summary>
    /// <value>The number of texture in the texture array.</value>
    public int ArraySize
    {
      get { return _arraySize; }
      set { _arraySize = value; }
    }


    /// <summary>
    /// Gets or sets the texture format.
    /// </summary>
    /// <value>The texture format.</value>
    public DataFormat Format
    {
      get { return _format; }
      set { _format = value; }
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" />
    /// parameter; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(TextureDescription other)
    {
      return _dimension == other._dimension
             && _width == other._width
             && _height == other._height
             && _depth == other._depth
             && _mipLevels == other._mipLevels
             && _arraySize == other._arraySize
             && _format == other._format;
    }


    /// <summary>
    /// Determines whether the specified <see cref="Object" />, is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object" /> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object" /> is equal to this
    /// instance; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is TextureDescription && Equals((TextureDescription)obj);
    }


    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures
    /// like a hash table.
    /// </returns>
    public override int GetHashCode()
    {
      // ReSharper disable NonReadonlyFieldInGetHashCode
      unchecked
      {
        var hashCode = (int)_dimension;
        hashCode = (hashCode * 397) ^ _width;
        hashCode = (hashCode * 397) ^ _height;
        hashCode = (hashCode * 397) ^ _depth;
        hashCode = (hashCode * 397) ^ _mipLevels;
        hashCode = (hashCode * 397) ^ _arraySize;
        hashCode = (hashCode * 397) ^ (int)_format;
        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Compares two <see cref="TextureDescription"/> objects to determine whether they are the 
    /// same.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(TextureDescription left, TextureDescription right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares two <see cref="TextureDescription"/> objects to determine whether they are the 
    /// different.
    /// </summary>
    /// <param name="left">The first instance.</param>
    /// <param name="right">The second instance.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(TextureDescription left, TextureDescription right)
    {
      return !left.Equals(right);
    }


    /// <summary>
    /// Returns a <see cref="String" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="String" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return string.Format(
        "Dimension: {0}, Width: {1}, Height: {2}, Depth: {3}, MipLevels: {4}, ArraySize: {5}, Format: {6}",
        Dimension, Width, Height, Depth, MipLevels, ArraySize, Format);
    }
  }
}
