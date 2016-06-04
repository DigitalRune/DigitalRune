// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Describes the size and format of a texture or a render target.
  /// </summary>
  /// <remarks>
  /// <para>
  /// All properties are <see cref="Nullable"/>. A value of <see langword="null"/> means that the
  /// property is undefined. For example, if <see cref="Width"/> is <see langword="null"/>, then
  /// it is undefined and the user may choose any suitable width.
  /// </para>
  /// <para>
  /// A <see cref="RenderTargetFormat"/> can be also be used to describe cube maps. In this case
  /// the <see cref="Width"/> defines the size of the cube map and <see cref="Height"/> should be
  /// ignored because cube map faces are always quadratic.
  /// </para>
  /// </remarks>
  public struct RenderTargetFormat : IEquatable<RenderTargetFormat>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the width (in pixels).
    /// </summary>
    /// <value>
    /// The width (in pixels) or <see langword="null"/> if the width is undefined.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int? Width
    {
      get { return _width; }
      set
      {
        if (value.HasValue && value.Value <= 0)
          throw new ArgumentOutOfRangeException("value", "The width must not be 0 or negative.");

        _width = value;
      }
    }
    private int? _width;


    /// <summary>
    /// Gets or sets the height (in pixels).
    /// </summary>
    /// <value>
    /// The height (in pixels) or <see langword="null"/> if the height is undefined.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative or 0.
    /// </exception>
    public int? Height
    {
      get { return _height; }
      set
      {
        if (value.HasValue && value <= 0)
          throw new ArgumentOutOfRangeException("value", "The height must not be 0 or negative.");

        _height = value;
      }
    }
    private int? _height;


    /// <summary>
    /// Gets or sets a value indicating whether the texture uses mipmapping.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to enable a full mipmap chain to be generated.
    /// <see langword="false"/> to disable mipmapping. 
    /// <see langword="null"/> if the mipmapping behavior is undefined.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public bool? Mipmap
    {
      get { return _mipmap; }
      set { _mipmap = value; }
    }
    private bool? _mipmap;


    /// <summary>
    /// Gets or sets the surface format.
    /// </summary>
    /// <value>
    /// The surface format or <see langword="null"/> if the surface format is undefined.
    /// </value>
    public SurfaceFormat? SurfaceFormat
    {
      get { return _surfaceFormat; }
      set { _surfaceFormat = value; }
    }
    private SurfaceFormat? _surfaceFormat;


    /// <summary>
    /// Gets or sets the depth/stencil buffer format.
    /// </summary>
    /// <value>
    /// The depth/stencil buffer format or <see langword="null"/> if the depth/stencil format is 
    /// undefined.
    /// </value>
    public DepthFormat? DepthStencilFormat
    {
      get { return _depthStencilFormat; }
      set { _depthStencilFormat = value; }
    }
    private DepthFormat? _depthStencilFormat;


    /// <summary>
    /// Gets or sets the number of sample locations during multisampling.
    /// </summary>
    /// <value>
    /// The number of sample locations during multisampling or <see langword="null"/> if the 
    /// number of sample locations is undefined.
    /// </value>
    public int? MultiSampleCount
    {
      get { return _multiSampleCount; }
      set { _multiSampleCount = value; }
    }
    private int? _multiSampleCount;


    /// <summary>
    /// Gets or sets the render target usage.
    /// </summary>
    /// <value>
    /// The render target usage or <see langword="null"/> if the usage is undefined.
    /// </value>
    public RenderTargetUsage? RenderTargetUsage
    {
      get { return _renderTargetUsage; }
      set { _renderTargetUsage = value; }
    }
    private RenderTargetUsage? _renderTargetUsage;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat" /> struct.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="width">
    /// The width or <see langword="null"/> if the width is undefined.
    /// </param>
    /// <param name="height">
    /// The height or <see langword="null"/> if the height is undefined.
    /// </param>
    /// <param name="mipmap">
    /// <see langword="true"/> to enable a full mipmap chain. <see langword="false"/> to disable 
    /// mipmapping. <see langword="null"/> if the mipmapping behavior is undefined.
    /// </param>
    /// <param name="surfaceFormat">
    /// The surface format or <see langword="null"/> if the surface format is undefined.
    /// </param>
    /// <param name="depthStencilFormat">
    /// The depth/stencil format or <see langword="null"/> if the depth/stencil format is undefined.
    /// </param>
    /// <remarks>
    /// The <see cref="MultiSampleCount"/> is initialized with 0. The 
    /// <see cref="RenderTargetUsage"/> is initialized with 
    /// <see cref="Microsoft.Xna.Framework.Graphics.RenderTargetUsage.DiscardContents"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public RenderTargetFormat(int? width, int? height, bool? mipmap, SurfaceFormat? surfaceFormat, 
      DepthFormat? depthStencilFormat)
    {
      if (width.HasValue && width.Value <= 0)
        throw new ArgumentOutOfRangeException("width", "The width must not be 0 or negative.");
      if (height.HasValue && height.Value <= 0)
        throw new ArgumentOutOfRangeException("height", "The height must not be 0 or negative.");

      _width = width;
      _height = height;
      _mipmap = mipmap;
      _surfaceFormat = surfaceFormat;
      _depthStencilFormat = depthStencilFormat;
      _multiSampleCount = 0;
      _renderTargetUsage = Microsoft.Xna.Framework.Graphics.RenderTargetUsage.DiscardContents;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="width">The width or <see langword="null"/> if the width is undefined.</param>
    /// <param name="height">
    /// The height or <see langword="null"/> if the height is undefined.
    /// </param>
    /// <param name="mipmap">
    /// <see langword="true"/> to enable a full mipmap chain. <see langword="false"/> to disable
    /// mipmapping. <see langword="null"/> if the mipmapping behavior is undefined.
    /// </param>
    /// <param name="surfaceFormat">
    /// The surface format or <see langword="null"/> if the surface format is undefined.
    /// </param>
    /// <param name="depthStencilFormat">
    /// The depth/stencil format or <see langword="null"/> if the depth/stencil format is undefined.
    /// </param>
    /// <param name="multiSampleCount">
    /// The number of sample locations during multisampling or <see langword="null"/> if the 
    /// number of sample locations is undefined.
    /// </param>
    /// <param name="usage">
    /// The render target usage or <see langword="null"/> if the usage is undefined.
    /// </param>
    /// <exception cref="System.ArgumentOutOfRangeException">
    /// width;The width must not be 0 or negative. or height;The height must not be 0 or negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is negative or 0.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public RenderTargetFormat(int? width, int? height, bool? mipmap, SurfaceFormat? surfaceFormat, 
      DepthFormat? depthStencilFormat, int? multiSampleCount, RenderTargetUsage? usage)
    {
      if (width.HasValue && width.Value <= 0)
        throw new ArgumentOutOfRangeException("width", "The width must not be 0 or negative.");
      if (height.HasValue && height.Value <= 0)
        throw new ArgumentOutOfRangeException("height", "The height must not be 0 or negative.");

      _width = width;
      _height = height;
      _mipmap = mipmap;
      _surfaceFormat = surfaceFormat;
      _depthStencilFormat = depthStencilFormat;
      _multiSampleCount = multiSampleCount;
      _renderTargetUsage = usage;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="renderTarget2D">
    /// The <see cref="RenderTarget2D"/> from which the settings are copied. Can be 
    /// <see langword="null"/> to set all properties to undefined.
    /// </param>
    public RenderTargetFormat(RenderTarget2D renderTarget2D)
    {
      if (renderTarget2D == null)
      {
        _width = null;
        _height = null;
        _mipmap = null;
        _surfaceFormat = null;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
      else
      {
        _width = renderTarget2D.Width;
        _height = renderTarget2D.Height;
        _mipmap = renderTarget2D.LevelCount > 1;
        _surfaceFormat = renderTarget2D.Format;
        _depthStencilFormat = renderTarget2D.DepthStencilFormat;
        _multiSampleCount = renderTarget2D.MultiSampleCount;
        _renderTargetUsage = renderTarget2D.RenderTargetUsage;
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="renderTargetCube">
    /// The <see cref="RenderTargetCube"/> from which the settings are copied. Can be 
    /// <see langword="null"/> to set all properties to undefined.
    /// </param>
    public RenderTargetFormat(RenderTargetCube renderTargetCube)
    {
      if (renderTargetCube == null)
      {
        _width = null;
        _height = null;
        _mipmap = null;
        _surfaceFormat = null;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
      else
      {
        _width = renderTargetCube.Size;
        _height = renderTargetCube.Size;
        _mipmap = renderTargetCube.LevelCount > 1;
        _surfaceFormat = renderTargetCube.Format;
        _depthStencilFormat = renderTargetCube.DepthStencilFormat;
        _multiSampleCount = renderTargetCube.MultiSampleCount;
        _renderTargetUsage = renderTargetCube.RenderTargetUsage;
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="texture2D">
    /// The <see cref="Texture2D"/> from which the settings are copied. Can be 
    /// <see langword="null"/> to set all properties to undefined.
    /// </param>
    public RenderTargetFormat(Texture2D texture2D)
    {
      if (texture2D == null)
      {
        _width = null;
        _height = null;
        _mipmap = null;
        _surfaceFormat = null;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
      else
      {
        _width = texture2D.Width;
        _height = texture2D.Height;
        _mipmap = texture2D.LevelCount > 1;
        _surfaceFormat = texture2D.Format;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTargetFormat"/> struct.
    /// </summary>
    /// <param name="textureCube">
    /// The <see cref="TextureCube"/> from which the settings are copied. Can be 
    /// <see langword="null"/> to set all properties to undefined.
    /// </param>
    public RenderTargetFormat(TextureCube textureCube)
    {
      if (textureCube == null)
      {
        _width = null;
        _height = null;
        _mipmap = null;
        _surfaceFormat = null;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
      else
      {
        _width = textureCube.Size;
        _height = null;
        _mipmap = textureCube.LevelCount > 1;
        _surfaceFormat = textureCube.Format;
        _depthStencilFormat = null;
        _multiSampleCount = null;
        _renderTargetUsage = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to this instance;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is RenderTargetFormat && Equals((RenderTargetFormat)obj);
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    public bool Equals(RenderTargetFormat other)
    {
      return _width == other._width 
             && _height == other._height 
             && _mipmap == other._mipmap 
             && _surfaceFormat == other._surfaceFormat 
             && _depthStencilFormat == other._depthStencilFormat
             && _multiSampleCount == other._multiSampleCount
             && _renderTargetUsage == other._renderTargetUsage;
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
        var hashCode = _width.GetHashCode();
        hashCode = (hashCode * 397) ^ _height.GetHashCode();
        hashCode = (hashCode * 397) ^ _mipmap.GetHashCode();

        // Note: enum.GetHashCode() causes boxing. Use int.GetHashCode() instead.
        hashCode = (hashCode * 397) ^ (_surfaceFormat.HasValue ? ((int)_surfaceFormat).GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ (_depthStencilFormat.HasValue ? ((int)_depthStencilFormat).GetHashCode() : 0);
        hashCode = (hashCode * 397) ^ _multiSampleCount.GetHashCode();
        hashCode = (hashCode * 397) ^ (_renderTargetUsage.HasValue ? ((int)_renderTargetUsage).GetHashCode() : 0);

        return hashCode;
      }
      // ReSharper restore NonReadonlyFieldInGetHashCode
    }


    /// <summary>
    /// Compares <see cref="RenderTargetFormat"/> to determine whether they are the same.
    /// </summary>
    /// <param name="left">The first <see cref="RenderTargetFormat"/>.</param>
    /// <param name="right">The second <see cref="RenderTargetFormat"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(RenderTargetFormat left, RenderTargetFormat right)
    {
      return left.Equals(right);
    }


    /// <summary>
    /// Compares <see cref="RenderTargetFormat"/> to determine whether they are different.
    /// </summary>
    /// <param name="left">The first <see cref="RenderTargetFormat"/>.</param>
    /// <param name="right">The second <see cref="RenderTargetFormat"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(RenderTargetFormat left, RenderTargetFormat right)
    {
      return !left.Equals(right);
    }


    /// <summary>
    /// Determines whether this instance is compatible with the specified render target format.
    /// </summary>
    /// <param name="format">The format to compare with this instance.</param>
    /// <returns>
    /// <see langword="true" /> if this instance is compatible with the given format; otherwise, 
    /// <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// This instance is compatible with the specified format if all properties are equal or if a
    /// property of this instance is undefined. This method returns <see langword="false"/> if a
    /// property in this instance has a value, but is undefined in the specified format.
    /// </remarks>
    internal bool IsCompatibleWith(RenderTargetFormat format)
    {
      if (_width.HasValue && (!format._width.HasValue || format._width != _width)
          || _height.HasValue && (!format._height.HasValue || format._height != _height)
          || _mipmap.HasValue && (!format._mipmap.HasValue || format._mipmap != _mipmap)
          || _surfaceFormat.HasValue && (!format._surfaceFormat.HasValue || format._surfaceFormat != _surfaceFormat)
          || _depthStencilFormat.HasValue && (!format._depthStencilFormat.HasValue || format._depthStencilFormat != _depthStencilFormat)
          || _multiSampleCount.HasValue && (!format._multiSampleCount.HasValue || format._multiSampleCount!= _multiSampleCount)
          || _renderTargetUsage.HasValue && (!format._renderTargetUsage.HasValue || format._renderTargetUsage != _renderTargetUsage))
      {
        return false;
      }

      return true;
    }


    /// <summary>
    /// Determines whether this instance is compatible with the specified render target format.
    /// </summary>
    /// <param name="texture">The format to compare with this instance.</param>
    /// <returns>
    /// <see langword="true" /> if this instance is compatible with the given format; otherwise, 
    /// <see langword="false" />.
    /// </returns>
    /// <remarks>
    /// This instance is compatible with the specified format if all properties are equal or if a
    /// property of this instance is undefined. This method returns <see langword="false"/> if a
    /// property in this instance has a value, but is undefined in the specified format.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public bool IsCompatibleWith(Texture texture)
    {
      if (texture == null) 
        throw new ArgumentNullException("texture");

      if (_mipmap.HasValue && (texture.LevelCount > 1) != _mipmap)
        return false;

      if (_surfaceFormat.HasValue && texture.Format != _surfaceFormat)
        return false;

      var texture2D = texture as Texture2D;
      if (texture2D != null)
      {
        if (_width.HasValue && texture2D.Width != _width)
          return false;
      
        if (_height.HasValue && texture2D.Height != _height)
          return false;

        var renderTarget2D = texture as RenderTarget2D;
        if (renderTarget2D != null)
        {
          if (_depthStencilFormat.HasValue && renderTarget2D.DepthStencilFormat != _depthStencilFormat)
            return false;
          if (_multiSampleCount.HasValue && renderTarget2D.MultiSampleCount != _multiSampleCount)
            return false;
          if (_renderTargetUsage.HasValue && renderTarget2D.RenderTargetUsage != _renderTargetUsage)
            return false;
        }
        else
        {
          // If a depth stencil buffer is required, we need a render target.
          if (_depthStencilFormat.HasValue && _depthStencilFormat.Value != DepthFormat.None)
            return false;
        }
      }
      else
      {
        var textureCube = texture as TextureCube;
        if (textureCube != null)
        {
          if (_width.HasValue && textureCube.Size != _width)
            return false;

          var renderTargetCube = texture as RenderTargetCube;
          if (renderTargetCube != null)
          {
            if (_depthStencilFormat.HasValue && renderTargetCube.DepthStencilFormat != _depthStencilFormat)
              return false;
            if (_multiSampleCount.HasValue && renderTargetCube.MultiSampleCount != _multiSampleCount)
              return false;
            if (_renderTargetUsage.HasValue && renderTargetCube.RenderTargetUsage != _renderTargetUsage)
              return false;
          }
          else
          {
            // If a depth stencil buffer is required, we need a render target.
            if (_depthStencilFormat.HasValue && _depthStencilFormat.Value != DepthFormat.None)
              return false;
          }
        }
        else
        {
          throw new ArgumentException("Texture must be Texture2D or TextureCube.", "texture");
        }
      }

      return true;
    }
    #endregion
  }
}
