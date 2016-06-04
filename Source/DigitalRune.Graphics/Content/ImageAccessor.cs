// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Wraps a floating-point image (format <see cref="DataFormat.R32G32B32A32_FLOAT"/>) for easy
  /// manipulation. (Wrapper needs to be disposed after use!)
  /// </summary>
  internal struct ImageAccessor : IDisposable
  {
    private readonly int _width;
    private readonly int _height;
    private readonly int _size;
    private GCHandle _handle;
    private readonly IntPtr _intPtr;


    /// <summary>
    /// Gets the width of the image in pixels.
    /// </summary>
    /// <value>The width of the image in pixels.</value>
    public int Width { get { return _width; } }


    /// <summary>
    /// Gets the height of the image in pixels.
    /// </summary>
    /// <value>The height of the image in pixels.</value>
    public int Height { get { return _height; } }


    /// <summary>
    /// Initializes a new instance of the <see cref="ImageAccessor"/> struct.
    /// </summary>
    /// <param name="image">
    /// The floating-point image (format <see cref="DataFormat.R32G32B32A32_FLOAT"/>).
    /// </param>
    public ImageAccessor(Image image)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (image.Format != DataFormat.R32G32B32A32_FLOAT)
        throw new ArgumentException("Image format needs to be R32G32B32A32_FLOAT", "image");

      _width = image.Width;
      _height = image.Height;
      _size = image.Width * image.Height;
      _handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
      _intPtr = _handle.AddrOfPinnedObject();
    }


    /// <summary>
    /// Disposes this instance and unlocks the memory.
    /// </summary>
    public void Dispose()
    {
      _handle.Free();
    }


    /// <summary>
    /// Gets the pixel at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The pixel color.</returns>
    public Vector4F GetPixel(int index)
    {
      if ((uint)index >= (uint)_size)
        throw new ArgumentOutOfRangeException();

      unsafe
      {
        float* ptr = (float*)_intPtr.ToPointer() + index * 4;
        return new Vector4F(*ptr++, *ptr++, *ptr++, *ptr);
      }
    }


    /// <summary>
    /// Sets the pixel at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="color">The pixel color.</param>
    public void SetPixel(int index, Vector4F color)
    {
      if ((uint)index >= (uint)_size)
        throw new ArgumentOutOfRangeException();

      unsafe
      {
        float* ptr = (float*)_intPtr.ToPointer() + index * 4;
        *ptr++ = color.X;
        *ptr++ = color.Y;
        *ptr++ = color.Z;
        *ptr = color.W;
      }
    }


    /// <summary>
    /// Gets the pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <returns>The pixel color.</returns>
    public Vector4F GetPixel(int x, int y)
    {
#if DEBUG
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
        throw new ArgumentOutOfRangeException();
#endif

      return GetPixel(y * _width + x);
    }


    /// <summary>
    /// Gets the pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="wrapMode">The wrap mode.</param>
    /// <returns>The pixel color.</returns>
    public Vector4F GetPixel(int x, int y, TextureAddressMode wrapMode)
    {
      switch (wrapMode)
      {
        case TextureAddressMode.Clamp:
          x = TextureHelper.WrapClamp(x, _width);
          y = TextureHelper.WrapClamp(y, _height);
          break;
        case TextureAddressMode.Repeat:
          x = TextureHelper.WrapRepeat(x, _width);
          y = TextureHelper.WrapRepeat(y, _height);
          break;
        case TextureAddressMode.Mirror:
          x = TextureHelper.WrapMirror(x, _width);
          y = TextureHelper.WrapMirror(y, _height);
          break;
      }

      return GetPixel(y * _width + x);
    }


    /// <summary>
    /// Gets the pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="color">The pixel color.</param>
    public void SetPixel(int x, int y, Vector4F color)
    {
#if DEBUG
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height)
        throw new ArgumentOutOfRangeException();
#endif

      SetPixel(y * _width + x, color);
    }
  }


  /// <summary>
  /// Wraps a floating-point volume (format <see cref="DataFormat.R32G32B32A32_FLOAT"/>) for easy
  /// manipulation. (Wrapper needs to be disposed after use!)
  /// </summary>
  internal struct VolumeAccessor : IDisposable
  {
    private readonly int _width;
    private readonly int _height;
    private readonly int _depth;
    private readonly GCHandle[] _handles;
    private readonly IntPtr[] _intPtrs;


    /// <summary>
    /// Gets the width of the volume in pixels.
    /// </summary>
    /// <value>The width of the volume in pixels.</value>
    public int Width { get { return _width; } }


    /// <summary>
    /// Gets the height of the volume in pixels.
    /// </summary>
    /// <value>The height of the volume in pixels.</value>
    public int Height { get { return _height; } }


    /// <summary>
    /// Gets the depth of the volume in pixels.
    /// </summary>
    /// <value>The depth of the volume in pixels.</value>
    public int Depth { get { return _depth; } }


    /// <summary>
    /// Initializes a new instance of the <see cref="VolumeAccessor"/> struct.
    /// </summary>
    /// <param name="texture">The volume texture.</param>
    /// <param name="mipIndex">The mipmap level.</param>
    /// <param name="arrayOrFaceIndex">
    /// The array index for texture arrays, or the face index for cube maps. Must be 0 for volume
    /// textures.
    /// </param>
    public VolumeAccessor(Texture texture, int mipIndex, int arrayOrFaceIndex)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      int index = texture.GetImageIndex(mipIndex, arrayOrFaceIndex, 0);
      var image = texture.Images[index];

      _width = image.Width;
      _height = image.Height;
      _depth = texture.Description.Depth;
      _handles = new GCHandle[_depth];
      _intPtrs = new IntPtr[_depth];

      for (int z = 0; z < _depth; z++)
      {
        image = texture.Images[index + z];
        _handles[z] = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
        _intPtrs[z] = _handles[z].AddrOfPinnedObject();
      }
    }


    /// <summary>
    /// Disposes this instance and unlocks the memory.
    /// </summary>
    public void Dispose()
    {
      for (int z = 0; z < _depth; z++)
        _handles[z].Free();
    }


    /// <summary>
    /// Gets the pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="z">The z position.</param>
    /// <param name="wrapMode">The wrap mode.</param>
    /// <returns>The pixel color.</returns>
    public Vector4F GetPixel(int x, int y, int z, TextureAddressMode wrapMode)
    {
      switch (wrapMode)
      {
        case TextureAddressMode.Clamp:
          x = TextureHelper.WrapClamp(x, _width);
          y = TextureHelper.WrapClamp(y, _height);
          z = TextureHelper.WrapClamp(z, _depth);
          break;
        case TextureAddressMode.Repeat:
          x = TextureHelper.WrapRepeat(x, _width);
          y = TextureHelper.WrapRepeat(y, _height);
          z = TextureHelper.WrapRepeat(z, _depth);
          break;
        case TextureAddressMode.Mirror:
          x = TextureHelper.WrapMirror(x, _width);
          y = TextureHelper.WrapMirror(y, _height);
          z = TextureHelper.WrapMirror(z, _depth);
          break;
      }

      unsafe
      {
        float* ptr = (float*)_intPtrs[z].ToPointer() + (y * _width + x) * 4;
        return new Vector4F(*ptr++, *ptr++, *ptr++, *ptr);
      }
    }


    /// <summary>
    /// Gets the pixel at the specified position.
    /// </summary>
    /// <param name="x">The x position.</param>
    /// <param name="y">The y position.</param>
    /// <param name="z">The z position.</param>
    /// <param name="color">The pixel color.</param>
    public void SetPixel(int x, int y, int z, Vector4F color)
    {
#if DEBUG
      if ((uint)x >= (uint)_width || (uint)y >= (uint)_height || (uint)z >= (uint)_depth)
        throw new ArgumentOutOfRangeException();
#endif

      unsafe
      {
        float* ptr = (float*)_intPtrs[z].ToPointer() + (y * _width + x) * 4;
        *ptr++ = color.X;
        *ptr++ = color.Y;
        *ptr++ = color.Z;
        *ptr = color.W;
      }
    }
  }
}
