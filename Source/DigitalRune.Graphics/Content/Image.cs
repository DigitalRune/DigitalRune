// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Represents a bitmap image.
  /// </summary>
  /// <seealso cref="Texture"/>
  internal class Image
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    /// <value>The width of the image in pixels.</value>
    public int Width { get; private set; }


    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    /// <value>The height of the image in pixels.</value>
    public int Height { get; private set; }


    /// <summary>
    /// Gets the format the pixels are stored in.
    /// </summary>
    /// <value>The format the pixels are stored in.</value>
    public DataFormat Format { get; private set; }


    /// <summary>
    /// Gets the row pitch (= size of a row in bytes).
    /// </summary>
    /// <value>The row pitch (= size of a row in bytes).</value>
    public int RowPitch { get; private set; }


    /// <summary>
    /// Gets or sets the image data.
    /// </summary>
    /// <value>The image data.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The size of the specified buffer does not match.
    /// </exception>
    public byte[] Data
    {
      get { return _data; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");
        if (value.Length != _data.Length)
          throw new ArgumentException(string.Format("Buffer has invalid size. Expected size: {0} bytes; Actual size: {1}", _data.Length, value.Length));

        _data = value;
      }
    }
    private byte[] _data;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new, empty instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="format">The texture format.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is 0 or negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="format"/> is invalid.
    /// </exception>
    public Image(int width, int height, DataFormat format)
    {
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width", "Image size must not be negative or 0.");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height", "Image size must not be negative or 0.");
      if (!TextureHelper.IsValid(format))
        throw new ArgumentException("Invalid texture format.", "format");

      int rowPitch, slicePitch;
      TextureHelper.ComputePitch(format, width, height, out rowPitch, out slicePitch);

      Width = width;
      Height = height;
      Format = format;
      RowPitch = rowPitch;
      _data = new byte[slicePitch];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class with the specified data.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="format">The texture format.</param>
    /// <param name="data">The contents of the image.</param>
    /// <exception cref="ArgumentException">
    /// <paramref name="format"/> is invalid, or <paramref name="data"/> has wrong size.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="data"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/>is 0 or negative.
    /// </exception>
    public Image(int width, int height, DataFormat format, byte[] data)
    {
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width", "Image size must not be negative or 0.");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height", "Image size must not be negative or 0.");
      if (!TextureHelper.IsValid(format))
        throw new ArgumentException("Invalid texture format.", "format");
      if (data == null)
        throw new ArgumentNullException("data");

      int rowPitch, slicePitch;
      TextureHelper.ComputePitch(format, width, height, out rowPitch, out slicePitch);
      if (data.Length != slicePitch)
        throw new ArgumentException(string.Format("Buffer has invalid size. Expected size: {0} bytes; Actual size: {1}", slicePitch, data.Length));

      Width = width;
      Height = height;
      Format = format;
      RowPitch = rowPitch;
      _data = data;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
