// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Provides methods for converting data formats.
  /// </summary>
  internal static class DataFormatHelper
  {
    //--------------------------------------------------------------
    #region Data Conversion (Integer formats)
    //--------------------------------------------------------------

    /// <summary>
    /// Converts a <see cref="DataFormat.B5G6R5_UNORM"/> to <see cref="DataFormat.R8G8B8A8_UNORM"/>.
    /// </summary>
    /// <param name="color">The color as <see cref="DataFormat.B5G6R5_UNORM"/>.</param>
    /// <returns>
    /// The color as <see cref="DataFormat.R8G8B8A8_UNORM"/>. Alpha is set to 1.0 (opaque).
    /// </returns>
    public static uint Bgr565ToRgba8888(ushort color)
    {
      // R[15:11] B[10:0] B[4:0] --> A[31:24] B[23:16] G[15:8] R[7:0]

      // Best mapping from 5-bit value to 8-bit value:
      // - Shift 5 bits to most significant bits.
      //   rrrrr --> rrrrr000
      // - Shift most significant 3 bits to the least significant bits.
      //   rrr00 --> 00000rrr
      uint t1 = (uint)(((color & 0xf800) >> 8) | ((color & 0xe000) >> 13));   // R
      uint t2 = (uint)(((color & 0x07e0) << 5) | ((color & 0x0600) >> 5));    // G << 8
      uint t3 = (uint)(((color & 0x001f) << 19) | ((color & 0x001c) << 14));  // B << 16
      const uint ta = 0xff000000;                                             // A << 24

      return t1 | t2 | t3 | ta;
    }


    /// <summary>
    /// Converts a <see cref="DataFormat.B5G6R5_UNORM"/> to 8-bit RGB components.
    /// </summary>
    /// <param name="color">The color as <see cref="DataFormat.B5G6R5_UNORM"/>.</param>
    /// <param name="r">The red component.</param>
    /// <param name="g">The green component.</param>
    /// <param name="b">The blue component.</param>
    public static void Bgr565ToRgb888(ushort color, out byte r, out byte g, out byte b)
    {
      // R[15:11] B[10:0] B[4:0]
      r = (byte)(((color & 0xf800) >> 8) | ((color & 0xe000) >> 13));
      g = (byte)(((color & 0x07e0) >> 3) | ((color & 0x0600) >> 9));
      b = (byte)(((color & 0x001f) << 3) | ((color & 0x001c) >> 2));  // B << 16
    }


    /// <summary>
    /// Converts a <see cref="DataFormat.B5G5R5A1_UNORM"/> to <see cref="DataFormat.R8G8B8A8_UNORM"/>.
    /// </summary>
    /// <param name="color">The color as <see cref="DataFormat.B5G5R5A1_UNORM"/>.</param>
    /// <param name="setAlpha">
    /// If set to <see langword="true"/> the alpha value is set to 1.0 (opaque).
    /// </param>
    /// <returns>The color as <see cref="DataFormat.R8G8B8A8_UNORM"/>.</returns>
    public static uint Bgra5551ToRgba8888(ushort color, bool setAlpha)
    {
      uint t1 = (uint)(((color & 0x7c00) >> 7) | ((color & 0x7000) >> 12));       // R
      uint t2 = (uint)(((color & 0x03e0) << 6) | ((color & 0x0380) << 1));        // G << 8
      uint t3 = (uint)(((color & 0x001f) << 19) | ((color & 0x001c) << 14));      // B << 16
      uint ta = setAlpha ? 0xff000000 : ((color & 0x8000) != 0 ? 0xff000000 : 0); // A << 24

      return t1 | t2 | t3 | ta;
    }


    /// <summary>
    /// Converts a <see cref="DataFormat.B4G4R4A4_UNORM"/> to <see cref="DataFormat.R8G8B8A8_UNORM"/>.
    /// </summary>
    /// <param name="color">The color as <see cref="DataFormat.B4G4R4A4_UNORM"/>.</param>
    /// <param name="setAlpha">
    /// If set to <see langword="true"/> the alpha value is set to 1.0 (opaque).
    /// </param>
    /// <returns>The color as <see cref="DataFormat.R8G8B8A8_UNORM"/>.</returns>
    public static uint Bgra4444ToRgba8888(ushort color, bool setAlpha)
    {
      uint t1 = (uint)(((color & 0x0f00) >> 4) | ((color & 0x0f00) >> 8));                            // R
      uint t2 = (uint)(((color & 0x00f0) << 8) | ((color & 0x00f0) << 4));                            // G << 8
      uint t3 = (uint)(((color & 0x000f) << 20) | ((color & 0x000f) << 16));                          // B << 16
      uint ta = setAlpha ? 0xff000000 : (uint)(((color & 0xf000) << 16) | ((color & 0xf000) << 12));  // A << 24

      return t1 | t2 | t3 | ta;
    }
    #endregion


    //--------------------------------------------------------------
    #region Data Conversion (Float <-> SInt, UInt, SNorm, UNorm)
    //--------------------------------------------------------------

    // References: 
    // - Data Conversion Rules, http://msdn.microsoft.com/en-us/library/windows/desktop/dd607323(v=vs.85).aspx
    //
    // The bitmask can be calculated as
    //    uint bitmask = (1u << numberOfBits) - 1u;
    //
    // Since the conversion methods are used in tight loops, we specify the bitmask
    // directly instead of specifying the number of bits.


    /// <summary>
    /// Converts a floating point value to a signed integer value.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The signed integer value.</returns>
    [CLSCompliant(false)]
    public static uint FloatToSInt(float value, uint bitmask)
    {
      float max = bitmask >> 1;
      float min = -max - 1;
      return (uint)ClampAndRound(value, min, max, MidpointRounding.ToEven) & bitmask;
    }


    /// <summary>
    /// Converts a signed integer value to a floating point value.
    /// </summary>
    /// <param name="value">The signed integer value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The floating point value.</returns>
    [CLSCompliant(false)]
    public static float SIntToFloat(uint value, uint bitmask)
    {
      uint signmask = (bitmask + 1) >> 1;
      if ((value & signmask) != 0)
        value |= ~bitmask;  // Fill upper bits to get two's complement.
      else
        value &= bitmask;

      return (int)value;
    }


    /// <summary>
    /// Converts a floating point value to an unsigned integer value.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The unsigned integer value.</returns>
    [CLSCompliant(false)]
    public static uint FloatToUInt(float value, uint bitmask)
    {
      return (uint)ClampAndRound(value, 0, bitmask, MidpointRounding.ToEven);
    }


    /// <summary>
    /// Converts an unsigned integer value to a floating point value.
    /// </summary>
    /// <param name="value">The unsigned integer value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The floating point value.</returns>
    [CLSCompliant(false)]
    public static float UIntToFloat(uint value, uint bitmask)
    {
      value &= bitmask;
      return value;
    }


    /// <summary>
    /// Converts a floating point value to a signed normalized integer value.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The signed normalized integer value.</returns>
    [CLSCompliant(false)]
    public static uint FloatToSNorm(float value, uint bitmask)
    {
      float max = bitmask >> 1;
      value *= max;
      return (uint)ClampAndRound(value, -max, max, MidpointRounding.AwayFromZero) & bitmask;
    }


    /// <summary>
    /// Converts an signed normalized integer value to a floating point value.
    /// </summary>
    /// <param name="value">The signed normalized integer value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The floating point value.</returns>
    [CLSCompliant(false)]
    public static float SNormToFloat(uint value, uint bitmask)
    {
      uint signmask = (bitmask + 1) >> 1;
      if ((value & signmask) != 0)
      {
        // Minimum and second-minimum map to -1.0.
        // (Example: The 5-bit values 10000 and 10001 map to -1.0.)
        if ((value & bitmask) == signmask)
          return -1f;

        value |= ~bitmask;  // Fill upper bits to get two's complement.
      }
      else
      {
        value &= bitmask;
      }

      float max = bitmask >> 1;
      return (int)value / max;
    }


    /// <summary>
    /// Converts a floating point value to an unsigned normalized integer value.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The unsigned normalized integer value.</returns>
    [CLSCompliant(false)]
    public static uint FloatToUNorm(float value, uint bitmask)
    {
      value *= bitmask;
      return (uint)ClampAndRound(value, 0, bitmask, MidpointRounding.AwayFromZero);
    }


    /// <summary>
    /// Converts an unsigned normalized integer value to a floating point value.
    /// </summary>
    /// <param name="value">The unsigned normalized integer value.</param>
    /// <param name="bitmask">The bitmask of the integer value.</param>
    /// <returns>The floating point value.</returns>
    [CLSCompliant(false)]
    public static float UNormToFloat(uint value, uint bitmask)
    {
      value &= bitmask;
      return (float)value / bitmask;
    }


    /// <summary>
    /// Converts a floating point value to an integer value.
    /// </summary>
    /// <param name="value">The floating point value.</param>
    /// <param name="min">The min integer value.</param>
    /// <param name="max">The max integer value.</param>
    /// <param name="mode">The rounding mode </param>
    /// <returns>The closest integer representation of <paramref name="value"/>.</returns>
    private static float ClampAndRound(float value, float min, float max, MidpointRounding mode)
    {
      // NaN --> 0.
      if (Numeric.IsNaN(value))
        return 0;

      // -INF and value < min --> min.
      if (value <= min)
        return min;

      // +INF and value > max --> max.
      if (value >= max)
        return max;

      return (float)Math.Round(value, mode);
    }
    #endregion
  }


  /// <summary>
  /// Defines the texture address mode.
  /// </summary>
  public enum TextureAddressMode
  {
    /// <summary>Texture coordinates are clamped to [0.0, 1.0].</summary>
    Clamp,

    /// <summary>Texture coordinates repeat beyond [0.0, 1.0]</summary>
    Repeat,

    /// <summary>Similar to <see cref="Repeat"/>, except that the texture is flipped with each repetition.</summary>
    Mirror
  }


  /// <summary>
  /// Provides helper methods for processing textures.
  /// </summary>
  internal static partial class TextureHelper
  {
    //--------------------------------------------------------------
    #region Texture Extensions
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the depth of the specified mipmap level.
    /// </summary>
    /// <param name="description">The texture description.</param>
    /// <param name="mipLevel">The mipmap level, where 0 is the most detailed level.</param>
    /// <returns>The depth of texture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mipLevel"/> is out of range.
    /// </exception>
    public static int GetDepth(this TextureDescription description, int mipLevel)
    {
      if (mipLevel >= description.MipLevels)
        throw new ArgumentOutOfRangeException("mipLevel");

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          return 1;

        case TextureDimension.Texture3D:
          int d = description.Depth;
          for (int level = 0; level < mipLevel; level++)
          {
            if (d > 1)
              d >>= 1;
          }

          return d;

        default:
          throw new InvalidOperationException("Invalid texture dimension.");
      }
    }


    /// <summary>
    /// Gets the index of a specific image in the <see cref="Texture.Images"/> collection.
    /// </summary>
    /// <param name="description">The texture description.</param>
    /// <param name="mipIndex">The mipmap level, where 0 is the most detailed level.</param>
    /// <param name="arrayOrFaceIndex">
    /// The array index for texture arrays, or the face index for cube maps. Must be 0 for volume
    /// textures.
    /// </param>
    /// <param name="zIndex">The z index for volume textures.</param>
    /// <returns>
    /// The index of the specified image in the <see cref="Texture.Images"/> collection.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mipIndex"/>, <paramref name="arrayOrFaceIndex"/>, or <paramref name="zIndex"/> is
    /// out of range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Invalid texture dimension.
    /// </exception>
    public static int GetImageIndex(this TextureDescription description, int mipIndex, int arrayOrFaceIndex, int zIndex)
    {
      if (mipIndex >= description.MipLevels)
        throw new ArgumentOutOfRangeException("mipIndex");

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          if (zIndex > 0)
            throw new ArgumentOutOfRangeException("zIndex");
          if (arrayOrFaceIndex >= description.ArraySize)
            throw new ArgumentOutOfRangeException("arrayOrFaceIndex");

          return mipIndex + description.MipLevels * arrayOrFaceIndex;

        case TextureDimension.Texture3D:
          if (arrayOrFaceIndex > 0)
            throw new ArgumentOutOfRangeException("arrayOrFaceIndex");

          int index = 0;
          int d = description.Depth;

          for (int level = 0; level < mipIndex; level++)
          {
            index += d;
            if (d > 1)
              d >>= 1;
          }

          if (zIndex >= d)
            throw new ArgumentOutOfRangeException("zIndex");

          index += zIndex;
          return index;

        default:
          throw new InvalidOperationException("Invalid texture dimension.");
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Mipmaps
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Calculates the number of mipmap levels in the full mipmap chain.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Calculates the number of mipmap levels in the full mipmap chain of a 2D texture.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <returns>The number of mipmap levels in the full mipmap chain.</returns>
    public static int CalculateMipLevels(int width, int height)
    {
      int mipLevels = 1;
      while (height > 1 || width > 1)
      {
        if (height > 1)
          height >>= 1;

        if (width > 1)
          width >>= 1;

        mipLevels++;
      }

      return mipLevels;
    }


    /// <summary>
    /// Calculates the number of mipmap levels in the full mipmap chain of a volume texture.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="depth">The depth of the texture.</param>
    /// <returns>The number of mipmap levels in the full mipmap chain.</returns>
    public static int CalculateMipLevels(int width, int height, int depth)
    {
      int mipLevels = 1;
      while (height > 1 || width > 1 || depth > 1)
      {
        if (height > 1)
          height >>= 1;

        if (width > 1)
          width >>= 1;

        if (depth > 1)
          depth >>= 1;

        mipLevels++;
      }

      return mipLevels;
    }


    /// <overloads>
    /// <summary>
    /// Checks whether the specified number of mipmap levels is valid.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Checks whether the specified number of mipmap levels is valid for certain 2D texture.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="mipLevels">The number of mipmap levels.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="mipLevels"/> is a valid number of mipmap levels;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ValidateMipLevels(int width, int height, int mipLevels)
    {
      if (mipLevels < 1)
        return false;

      int maxMips = CalculateMipLevels(width, height);
      if (mipLevels > maxMips)
        return false;

      return true;
    }


    /// <summary>
    /// Checks whether the specified number of mipmap levels is valid for certain volume texture.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="depth">The depth of the texture.</param>
    /// <param name="mipLevels">The number of mipmap levels.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="mipLevels"/> is a valid number of mipmap levels;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ValidateMipLevels(int width, int height, int depth, int mipLevels)
    {
      if (mipLevels < 1)
        return false;

      int maxMips = CalculateMipLevels(width, height, depth);
      if (mipLevels > maxMips)
        return false;

      return true;
    }
    #endregion


    //--------------------------------------------------------------
    #region Texture Address Modes
    //--------------------------------------------------------------

    /// <summary>
    /// Wraps the specified index to [0, width - 1] using CLAMP.
    /// </summary>
    /// <param name="x">The x texture coordinate.</param>
    /// <param name="width">The width.</param>
    /// <returns>The index in [0, width - 1].</returns>
    public static int WrapClamp(int x, int width)
    {
      return MathHelper.Clamp(x, 0, width - 1);
    }


    /// <summary>
    /// Wraps the specified index to [0, width - 1] using REPEAT.
    /// </summary>
    /// <param name="x">The x texture coordinate.</param>
    /// <param name="width">The width.</param>
    /// <returns>The texture coordinate in [0, 1].</returns>
    public static int WrapRepeat(int x, int width)
    {
      return (x >= 0) ? x % width : (x + 1) % width + width - 1;
    }


    /// <summary>
    /// Wraps the specified index to [0, width - 1] using MIRROR.
    /// </summary>
    /// <param name="x">The x texture coordinate.</param>
    /// <param name="width">The width.</param>
    /// <returns>The texture coordinate in [0, 1].</returns>
    public static int WrapMirror(int x, int width)
    {
      // DigitalRune implementation. (Bug in NVTT!)
      if (x < 0)
        x = -(x + 1);

      if ((x / width) % 2 == 1)
      {
        // Mirrored
        return width - x % width - 1;
      }

      return x % width;
    }
    #endregion


    //--------------------------------------------------------------
    #region Texture Conversion
    //--------------------------------------------------------------

    // Note:
    // - Linear and sRGB formats are treated identically. There is no conversion
    //   between linear color space and sRGB color space. (Almost all 8-bit RGB images
    //   are R8G8B8A8_UNORM_SRGB loaded. However, images loaded from JPEG or PNG
    //   will be either R8G8B8A8_UNORM or R8G8B8A8_UNORM_SRGB, depending on whether a
    //   color profile is stored in the file.)
    //
    // - Conversion to/from the following formats are not yet implemented:
    //       R32G32B32A32_TYPELESS
    //       R32G32B32_TYPELESS
    //       R16G16B16A16_TYPELESS
    //       R32G32_TYPELESS
    //       R32G8X24_TYPELESS
    //       D32_FLOAT_S8X24_UINT
    //       R32_FLOAT_X8X24_TYPELESS
    //       X32_TYPELESS_G8X24_UINT
    //       R10G10B10A2_TYPELESS
    //       R11G11B10_FLOAT
    //       R8G8B8A8_TYPELESS
    //       R16G16_TYPELESS
    //       R32_TYPELESS
    //       D32_FLOAT
    //       R24G8_TYPELESS
    //       D24_UNORM_S8_UINT
    //       R24_UNORM_X8_TYPELESS
    //       X24_TYPELESS_G8_UINT
    //       R8G8_TYPELESS
    //       R16_TYPELESS
    //       D16_UNORM
    //       R8_TYPELESS
    //       R1_UNORM
    //       R9G9B9E5_SHAREDEXP
    //       R8G8_B8G8_UNORM
    //       G8R8_G8B8_UNORM
    //       BC1_TYPELESS
    //       BC2_TYPELESS
    //       BC3_TYPELESS
    //       BC4_TYPELESS
    //       BC4_UNORM
    //       BC4_SNORM
    //       BC5_TYPELESS
    //       BC5_UNORM
    //       BC5_SNORM
    //       R10G10B10_XR_BIAS_A2_UNORM
    //       B8G8R8A8_TYPELESS
    //       B8G8R8X8_TYPELESS
    //       BC6H_TYPELESS
    //       BC6H_UF16
    //       BC6H_SF16
    //       BC7_TYPELESS
    //       BC7_UNORM
    //       BC7_UNORM_SRGB
    //       AYUV
    //       Y410
    //       Y416
    //       NV12
    //       P010
    //       P016
    //       Y420_OPAQUE
    //       YUY2
    //       Y210
    //       Y216
    //       NV11
    //       AI44
    //       IA44
    //       P8
    //       A8P8
    //       R10G10B10_7E3_A2_FLOAT
    //       R10G10B10_6E4_A2_FLOAT
    //       D16_UNORM_S8_UINT
    //       R16_UNORM_X8_TYPELESS
    //       X16_TYPELESS_G8_UINT
    //       PVRTCI_2bpp_RGB
    //       PVRTCI_4bpp_RGB
    //       PVRTCI_2bpp_RGBA
    //       PVRTCI_4bpp_RGBA
    //       ETC1
    //       ATC_RGBA_EXPLICIT_ALPHA
    //       ATC_RGBA_INTERPOLATED_ALPHA
    //
    // Add the missing conversions when needed!


    /// <summary>
    /// Determines whether a specific format conversion is supported.
    /// </summary>
    /// <param name="srcFormat">The source format.</param>
    /// <param name="dstFormat">The destination format.</param>
    /// <returns>
    /// <see langword="true"/> if the conversion from <paramref name="srcFormat"/> to
    /// <paramref name="dstFormat"/> is supported; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanConvert(DataFormat srcFormat, DataFormat dstFormat)
    {
      switch (srcFormat)
      {
        case DataFormat.R32G32B32A32_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_UINT:
            case DataFormat.R32G32B32A32_SINT:
            case DataFormat.R32G32B32_FLOAT:
            case DataFormat.R32G32B32_UINT:
            case DataFormat.R32G32B32_SINT:
            case DataFormat.R16G16B16A16_FLOAT:
            case DataFormat.R16G16B16A16_UNORM:
            case DataFormat.R16G16B16A16_UINT:
            case DataFormat.R16G16B16A16_SNORM:
            case DataFormat.R16G16B16A16_SINT:
            case DataFormat.R32G32_FLOAT:
            case DataFormat.R32G32_UINT:
            case DataFormat.R32G32_SINT:
            case DataFormat.R10G10B10A2_UNORM:
            case DataFormat.R10G10B10A2_UINT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
            case DataFormat.R8G8B8A8_UINT:
            case DataFormat.R8G8B8A8_SNORM:
            case DataFormat.R8G8B8A8_SINT:
            case DataFormat.R16G16_FLOAT:
            case DataFormat.R16G16_UNORM:
            case DataFormat.R16G16_UINT:
            case DataFormat.R16G16_SNORM:
            case DataFormat.R16G16_SINT:
            case DataFormat.R32_FLOAT:
            case DataFormat.R32_UINT:
            case DataFormat.R32_SINT:
            case DataFormat.R8G8_UNORM:
            case DataFormat.R8G8_UINT:
            case DataFormat.R8G8_SNORM:
            case DataFormat.R8G8_SINT:
            case DataFormat.R16_FLOAT:
            case DataFormat.R16_UNORM:
            case DataFormat.R16_UINT:
            case DataFormat.R16_SNORM:
            case DataFormat.R16_SINT:
            case DataFormat.R8_UNORM:
            case DataFormat.R8_UINT:
            case DataFormat.R8_SNORM:
            case DataFormat.R8_SINT:
            case DataFormat.A8_UNORM:
            case DataFormat.B5G6R5_UNORM:
            case DataFormat.B5G5R5A1_UNORM:
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8X8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
            case DataFormat.B8G8R8X8_UNORM_SRGB:
            case DataFormat.B4G4R4A4_UNORM:
              return true;
          }
          break;

        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
        case DataFormat.R32G32B32_FLOAT:
        case DataFormat.R32G32B32_UINT:
        case DataFormat.R32G32B32_SINT:
        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
        case DataFormat.R32G32_FLOAT:
        case DataFormat.R32G32_UINT:
        case DataFormat.R32G32_SINT:
        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.R16G16_FLOAT:
        case DataFormat.R16G16_UNORM:
        case DataFormat.R16G16_UINT:
        case DataFormat.R16G16_SNORM:
        case DataFormat.R16G16_SINT:
        case DataFormat.R32_FLOAT:
        case DataFormat.R32_UINT:
        case DataFormat.R32_SINT:
        case DataFormat.R8G8_UNORM:
        case DataFormat.R8G8_UINT:
        case DataFormat.R8G8_SNORM:
        case DataFormat.R8G8_SINT:
        case DataFormat.R16_FLOAT:
        case DataFormat.R16_UNORM:
        case DataFormat.R16_UINT:
        case DataFormat.R16_SNORM:
        case DataFormat.R16_SINT:
        case DataFormat.R8_UINT:
        case DataFormat.R8_SNORM:
        case DataFormat.R8_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              return true;
          }
          break;

        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
            case DataFormat.BC1_UNORM:
            case DataFormat.BC1_UNORM_SRGB:
            case DataFormat.BC2_UNORM:
            case DataFormat.BC2_UNORM_SRGB:
            case DataFormat.BC3_UNORM:
            case DataFormat.BC3_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.R8_UNORM:
        case DataFormat.A8_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.B5G6R5_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.B5G5R5A1_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.B4G4R4A4_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;

        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              return true;
          }
          break;
      }

      return false;
    }


    /// <summary>
    /// Converts the content of an image to another format.
    /// </summary>
    /// <param name="srcImage">The source image.</param>
    /// <param name="dstImage">The destination image.</param>
    /// <exception cref="NotSupportedException">
    /// Format conversion to the specified format is not supported.
    /// </exception>
    public static unsafe void Convert(Image srcImage, Image dstImage)
    {
      if (srcImage == null)
        throw new ArgumentNullException("srcImage");
      if (dstImage == null)
        throw new ArgumentNullException("dstImage");
      if (srcImage.Width != dstImage.Width || srcImage.Height != dstImage.Height)
        throw new NotImplementedException("Image resizing is not implemented.");

      var srcFormat = srcImage.Format;
      var dstFormat = dstImage.Format;
      var srcData = srcImage.Data;
      var dstData = dstImage.Data;
      int width = srcImage.Width;
      int height = srcImage.Height;
      int numberOfPixels = width * height;

      switch (srcFormat)
      {
        case DataFormat.R32G32B32A32_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint b = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint a = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R32G32B32A32_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint b = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint a = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R32G32B32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;
                  float g = *srcPtr++;
                  float b = *srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                }
              }
              return;
            case DataFormat.R32G32B32_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint b = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                }
              }
              return;
            case DataFormat.R32G32B32_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint b = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                }
              }
              return;
            case DataFormat.R16G16B16A16_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = HalfHelper.Pack(*srcPtr++);
                  ushort g = HalfHelper.Pack(*srcPtr++);
                  ushort b = HalfHelper.Pack(*srcPtr++);
                  ushort a = HalfHelper.Pack(*srcPtr++);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R16G16B16A16_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  ushort b = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  ushort a = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R16G16B16A16_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  ushort b = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  ushort a = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R16G16B16A16_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  ushort b = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  ushort a = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R16G16B16A16_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  ushort b = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  ushort a = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R32G32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;
                  float g = *srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R32G32_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R32G32_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  uint g = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R10G10B10A2_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint x = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x3FF);
                  uint y = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x3FF) << 10;
                  uint z = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x3FF) << 20;
                  uint w = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x3) << 30;

                  *dstPtr++ = x | y | z | w;
                }
              }
              return;
            case DataFormat.R10G10B10A2_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint x = DataFormatHelper.FloatToUInt(*srcPtr++, 0x3FF);
                  uint y = DataFormatHelper.FloatToUInt(*srcPtr++, 0x3FF) << 10;
                  uint z = DataFormatHelper.FloatToUInt(*srcPtr++, 0x3FF) << 20;
                  uint w = DataFormatHelper.FloatToUInt(*srcPtr++, 0x3) << 30;

                  *dstPtr++ = x | y | z | w;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte a = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  byte a = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R8G8B8A8_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  byte a = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R8G8B8A8_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  byte a = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R16G16_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = HalfHelper.Pack(*srcPtr++);
                  ushort g = HalfHelper.Pack(*srcPtr++);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R16G16_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R16G16_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R16G16_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R16G16_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  ushort g = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R32_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R32_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFFFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R8G8_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R8G8_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R8G8_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R8G8_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                }
              }
              return;
            case DataFormat.R16_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = HalfHelper.Pack(*srcPtr++);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R16_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R16_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R16_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R16_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  ushort r = (ushort)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFFFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R8_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.A8_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;
                  byte a = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);

                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R8_UINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUInt(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R8_SNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSNorm(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R8_SINT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToSInt(*srcPtr++, 0xFF);
                  srcPtr++;
                  srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.B5G6R5_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1F) << 11;
                  uint g = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x3F) << 5;
                  uint b = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1F);

                  *dstPtr++ = (ushort)(r | g | b);
                }
              }
              return;
            case DataFormat.B5G5R5A1_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1F) << 10;
                  uint g = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1F) << 5;
                  uint b = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1F);
                  uint a = DataFormatHelper.FloatToUNorm(*srcPtr++, 0x1) << 15;

                  *dstPtr++ = (ushort)(r | g | b | a);
                }
              }
              return;
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte a = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);

                  *dstPtr++ = b;
                  *dstPtr++ = g;
                  *dstPtr++ = r;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.B8G8R8X8_UNORM:
            case DataFormat.B8G8R8X8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte g = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  byte b = (byte)DataFormatHelper.FloatToUNorm(*srcPtr++, 0xFF);
                  srcPtr++;

                  *dstPtr++ = b;
                  *dstPtr++ = g;
                  *dstPtr++ = r;
                  *dstPtr++ = 0;  // Unused
                }
              }
              return;
            case DataFormat.B4G4R4A4_UNORM:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                ushort* dstPtr = (ushort*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint r = DataFormatHelper.FloatToUNorm(*srcPtr++, 0xF) << 8;
                  uint g = DataFormatHelper.FloatToUNorm(*srcPtr++, 0xF) << 4;
                  uint b = DataFormatHelper.FloatToUNorm(*srcPtr++, 0xF);
                  uint a = DataFormatHelper.FloatToUNorm(*srcPtr++, 0xF) << 12;

                  *dstPtr++ = (ushort)(r | g | b | a);
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32B32A32_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float b = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float a = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32B32A32_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float b = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float a = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32B32_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;
                  float g = *srcPtr++;
                  float b = *srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32B32_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float b = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32B32_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float b = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16B16A16_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = HalfHelper.Unpack(*srcPtr++);
                  float g = HalfHelper.Unpack(*srcPtr++);
                  float b = HalfHelper.Unpack(*srcPtr++);
                  float a = HalfHelper.Unpack(*srcPtr++);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16B16A16_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);
                  float b = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);
                  float a = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16B16A16_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);
                  float b = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);
                  float a = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16B16A16_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);
                  float b = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);
                  float a = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16B16A16_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);
                  float b = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);
                  float a = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;
                  float g = *srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R32G32_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R10G10B10A2_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint color = *srcPtr++;
                  float r = DataFormatHelper.UNormToFloat(color, 0x3FF);
                  float g = DataFormatHelper.UNormToFloat(color >> 10, 0x3FF);
                  float b = DataFormatHelper.UNormToFloat(color >> 20, 0x3FF);
                  float a = DataFormatHelper.UNormToFloat(color >> 30, 0x3);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R10G10B10A2_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint color = *srcPtr++;
                  float r = DataFormatHelper.UIntToFloat(color, 0x3FF);
                  float g = DataFormatHelper.UIntToFloat(color >> 10, 0x3FF);
                  float b = DataFormatHelper.UIntToFloat(color >> 20, 0x3FF);
                  float a = DataFormatHelper.UIntToFloat(color >> 30, 0x3);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float b = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float a = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = *srcPtr++;
                  byte g = *srcPtr++;
                  byte b = *srcPtr++;
                  byte a = *srcPtr++;

                  *dstPtr++ = b;
                  *dstPtr++ = g;
                  *dstPtr++ = r;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.BC1_UNORM:
            case DataFormat.BC1_UNORM_SRGB:
              {
                var flags = SquishFlags.Dxt1;
                //#if DEBUG
                flags |= SquishFlags.ColourRangeFit;
                //#endif
                Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, flags) == dstData.Length);
                Squish.CompressImage(srcData, srcImage.Width, srcImage.Height, dstData, flags);
                return;
              }
            case DataFormat.BC2_UNORM:
            case DataFormat.BC2_UNORM_SRGB:
              {
                var flags = SquishFlags.Dxt3;
                //#if DEBUG
                flags |= SquishFlags.ColourRangeFit;
                //#endif
                Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, flags) == dstData.Length);
                Squish.CompressImage(srcData, srcImage.Width, srcImage.Height, dstData, flags);
                return;
              }
            case DataFormat.BC3_UNORM:
            case DataFormat.BC3_UNORM_SRGB:
              {
                var flags = SquishFlags.Dxt5;
                //#if DEBUG
                flags |= SquishFlags.ColourRangeFit;
                //#endif
                Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, flags) == dstData.Length);
                Squish.CompressImage(srcData, srcImage.Width, srcImage.Height, dstData, flags);
                return;
              }
          }
          break;

        case DataFormat.R8G8B8A8_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);
                  float b = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);
                  float a = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8B8A8_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);
                  float b = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);
                  float a = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8B8A8_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);
                  float b = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);
                  float a = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = HalfHelper.Unpack(*srcPtr++);
                  float g = HalfHelper.Unpack(*srcPtr++);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16G16_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R32_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                float* srcPtr = (float*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = *srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R32_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R32_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                uint* srcPtr = (uint*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFFFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R8G8_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = 0;
                  *dstPtr++ = 1;
                }
              }
              return;
          }
          break;

        case DataFormat.R16_FLOAT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = HalfHelper.Unpack(*srcPtr++);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R16_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R16_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R16_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R16_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFFFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R8_UNORM:
        case DataFormat.A8_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
            case DataFormat.B8G8R8A8_UNORM:
            case DataFormat.B8G8R8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte r = *srcPtr++;

                  *dstPtr++ = r; // R
                  *dstPtr++ = r; // G
                  *dstPtr++ = r; // B
                  *dstPtr++ = r; // A
                }
              }
              return;
          }
          break;

        case DataFormat.R8_UINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.UIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R8_SNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.R8_SINT:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float r = DataFormatHelper.SIntToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                  *dstPtr++ = r;
                }
              }
              return;
          }
          break;

        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, SquishFlags.Dxt1) == srcData.Length);
              Squish.DecompressImage(srcData, srcImage.Width, srcImage.Height, dstData, SquishFlags.Dxt1);
              //DecompressBc1(srcData, srcImage.Width, srcImage.Height, ref dstData);
              return;
          }
          break;

        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, SquishFlags.Dxt3) == srcData.Length);
              Squish.DecompressImage(srcData, srcImage.Width, srcImage.Height, dstData, SquishFlags.Dxt3);
              //DecompressBc2(srcData, srcImage.Width, srcImage.Height, ref dstData);
              return;
          }
          break;

        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              Debug.Assert(Squish.GetStorageRequirements(srcImage.Width, srcImage.Height, SquishFlags.Dxt5) == srcData.Length);
              Squish.DecompressImage(srcData, srcImage.Width, srcImage.Height, dstData, SquishFlags.Dxt5);
              //DecompressBc3(srcData, srcImage.Width, srcImage.Height, ref dstData);
              return;
          }
          break;

        case DataFormat.B5G6R5_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint color = *srcPtr++;

                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 11, 0x1F);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 5, 0x3F);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color, 0x1F);
                  *dstPtr++ = 1.0f;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                  *dstPtr++ = DataFormatHelper.Bgr565ToRgba8888(*srcPtr++);
              }
              return;
          }
          break;

        case DataFormat.B5G5R5A1_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint color = *srcPtr++;

                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 10, 0x1F);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 5, 0x1F);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color, 0x1F);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 15, 0x1);
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                  *dstPtr++ = DataFormatHelper.Bgra5551ToRgba8888(*srcPtr++, false);
              }
              return;
          }
          break;

        case DataFormat.B4G4R4A4_UNORM:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  uint color = *srcPtr++;

                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 8, 0xF);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 4, 0xF);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color, 0xF);
                  *dstPtr++ = DataFormatHelper.UNormToFloat(color >> 12, 0xF);
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                ushort* srcPtr = (ushort*)srcDataPtr;
                uint* dstPtr = (uint*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                  *dstPtr++ = DataFormatHelper.Bgra4444ToRgba8888(*srcPtr++, false);
              }
              return;
          }
          break;

        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float b = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float a = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte b = *srcPtr++;
                  byte g = *srcPtr++;
                  byte r = *srcPtr++;
                  byte a = *srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = a;
                }
              }
              return;
          }
          break;

        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
          switch (dstFormat)
          {
            case DataFormat.R32G32B32A32_FLOAT:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                float* dstPtr = (float*)dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  float b = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float g = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  float r = DataFormatHelper.UNormToFloat(*srcPtr++, 0xFF);
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = 1;
                }
              }
              return;
            case DataFormat.R8G8B8A8_UNORM:
            case DataFormat.R8G8B8A8_UNORM_SRGB:
              fixed (byte* srcDataPtr = srcData, dstDataPtr = dstData)
              {
                byte* srcPtr = srcDataPtr;
                byte* dstPtr = dstDataPtr;
                for (int i = 0; i < numberOfPixels; i++)
                {
                  byte b = *srcPtr++;
                  byte g = *srcPtr++;
                  byte r = *srcPtr++;
                  srcPtr++;

                  *dstPtr++ = r;
                  *dstPtr++ = g;
                  *dstPtr++ = b;
                  *dstPtr++ = 255;
                }
              }
              return;
          }
          break;
      }

      throw new NotSupportedException(string.Format("Texture conversion from {0} to format {1} is not supported.", srcFormat, dstFormat));
    }
    #endregion


    //--------------------------------------------------------------
    #region Block Compression
    //--------------------------------------------------------------

    // Reference: http://msdn.microsoft.com/en-us/library/bb694531.aspx

    /// <overloads>
    /// <summary>
    /// Decompresses a buffer using BC1 (DXT1) block compression.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Decompresses a buffer using BC1 (DXT1) block compression.
    /// </summary>
    /// <param name="compressedData">The buffer using BC1 (DXT1) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="compressedData" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width" /> or <paramref name="height" /> is out of range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="uncompressedData"/> has wrong size.
    /// </exception>
    public static void DecompressBc1(byte[] compressedData, int width, int height, ref byte[] uncompressedData)
    {
      if (compressedData == null)
        throw new ArgumentNullException("compressedData");

      using (var stream = new MemoryStream(compressedData))
        DecompressBc1(stream, width, height, ref uncompressedData);
    }


    /// <summary>
    /// Decompresses a buffer using BC1 (DXT1) block compression.
    /// </summary>
    /// <param name="stream">The buffer using BC1 (DXT1) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <returns>The image data (uncompressed RGBA 8:8:8:8).</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is out of range.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="uncompressedData"/> has wrong size.
    /// </exception>
    public static void DecompressBc1(Stream stream, int width, int height, ref byte[] uncompressedData)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height");

      int uncompressedSize = width * height * 4;
      if (uncompressedData == null)
        uncompressedData = new byte[uncompressedSize];
      else if (uncompressedData.Length != uncompressedSize)
        throw new ArgumentException("Buffer has wrong size.", "uncompressedData");

      using (var reader = new BinaryReader(stream))
      {
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;

        for (int y = 0; y < blockCountY; y++)
          for (int x = 0; x < blockCountX; x++)
            DecompressBc1Block(reader, x, y, width, height, uncompressedData);
      }
    }


    private static void DecompressBc1Block(BinaryReader reader, int x, int y, int width, int height, byte[] uncompressedData)
    {
      // Block stores two BGR 5:6:5 colors and a 4 x 4 lookup table (16 x 2-bit color indices).
      ushort c0 = reader.ReadUInt16();
      ushort c1 = reader.ReadUInt16();
      uint lookupTable = reader.ReadUInt32();

      byte r0, g0, b0, a0 = 0xFF;
      DataFormatHelper.Bgr565ToRgb888(c0, out r0, out g0, out b0);

      byte r1, g1, b1, a1 = 0xFF;
      DataFormatHelper.Bgr565ToRgb888(c1, out r1, out g1, out b1);

      byte r2, g2, b2, a2;
      byte r3, g3, b3, a3;
      if (c0 > c1)
      {
        // Derive the other two colors.
        r2 = (byte)((2 * r0 + r1) / 3);
        g2 = (byte)((2 * g0 + g1) / 3);
        b2 = (byte)((2 * b0 + b1) / 3);
        a2 = 0xFF;

        r3 = (byte)((r0 + 2 * r1) / 3);
        g3 = (byte)((g0 + 2 * g1) / 3);
        b3 = (byte)((b0 + 2 * b1) / 3);
        a3 = 0xFF;
      }
      else
      {
        // Derive the other two colors.
        r2 = (byte)((r0 + r1) / 2);
        g2 = (byte)((g0 + g1) / 2);
        b2 = (byte)((b0 + b1) / 2);
        a2 = 0xFF;

        r3 = 0;
        g3 = 0;
        b3 = 0;
        a3 = 0;
      }

      for (int by = 0; by < 4; by++)
      {
        for (int bx = 0; bx < 4; bx++)
        {
          byte r = 0, g = 0, b = 0, a = 0;
          uint index = (lookupTable >> 2 * (4 * by + bx)) & 0x03;

          switch (index)
          {
            case 0: r = r0; g = g0; b = b0; a = a0; break;
            case 1: r = r1; g = g1; b = b1; a = a1; break;
            case 2: r = r2; g = g2; b = b2; a = a2; break;
            case 3: r = r3; g = g3; b = b3; a = a3; break;
          }

          int px = (x << 2) + bx;
          int py = (y << 2) + by;
          if ((px < width) && (py < height))
          {
            int offset = ((py * width) + px) << 2;
            uncompressedData[offset] = r;
            uncompressedData[offset + 1] = g;
            uncompressedData[offset + 2] = b;
            uncompressedData[offset + 3] = a;
          }
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Decompresses a buffer using BC2 (DXT3) block compression.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Decompresses a buffer using BC2 (DXT3) block compression.
    /// </summary>
    /// <param name="compressedData">The buffer using BC2 (DXT3) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="compressedData"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is out of range.
    /// </exception>
    public static void DecompressBc2(byte[] compressedData, int width, int height, ref byte[] uncompressedData)
    {
      if (compressedData == null)
        throw new ArgumentNullException("compressedData");

      using (var stream = new MemoryStream(compressedData))
        DecompressBc2(stream, width, height, ref uncompressedData);
    }


    /// <summary>
    /// Decompresses a buffer using BC2 (DXT3) block compression.
    /// </summary>
    /// <param name="stream">The buffer using BC2 (DXT3) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is out of range.
    /// </exception>
    public static void DecompressBc2(Stream stream, int width, int height, ref byte[] uncompressedData)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height");

      int uncompressedSize = width * height * 4;
      if (uncompressedData == null)
        uncompressedData = new byte[uncompressedSize];
      else if (uncompressedData.Length != uncompressedSize)
        throw new ArgumentException("Buffer has wrong size.", "uncompressedData");

      using (var reader = new BinaryReader(stream))
      {
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;

        for (int y = 0; y < blockCountY; y++)
          for (int x = 0; x < blockCountX; x++)
            DecompressBc2Block(reader, x, y, width, height, uncompressedData);
      }
    }

    private static void DecompressBc2Block(BinaryReader reader, int x, int y, int width, int height, byte[] uncompressedData)
    {
      // Alpha stored as 4x4 4-bit values (64 bit).
      byte a0 = reader.ReadByte();
      byte a1 = reader.ReadByte();
      byte a2 = reader.ReadByte();
      byte a3 = reader.ReadByte();
      byte a4 = reader.ReadByte();
      byte a5 = reader.ReadByte();
      byte a6 = reader.ReadByte();
      byte a7 = reader.ReadByte();

      // Block stores two BGR 5:6:5 colors and a 4x4 lookup table (16 x 2-bit color indices).
      ushort c0 = reader.ReadUInt16();
      ushort c1 = reader.ReadUInt16();
      uint lookupTable = reader.ReadUInt32();

      byte r0, g0, b0;
      DataFormatHelper.Bgr565ToRgb888(c0, out r0, out g0, out b0);

      byte r1, g1, b1;
      DataFormatHelper.Bgr565ToRgb888(c1, out r1, out g1, out b1);

      // Derive the other two colors.
      byte r2 = (byte)((2 * r0 + r1) / 3);
      byte g2 = (byte)((2 * g0 + g1) / 3);
      byte b2 = (byte)((2 * b0 + b1) / 3);

      byte r3 = (byte)((r0 + 2 * r1) / 3);
      byte g3 = (byte)((g0 + 2 * g1) / 3);
      byte b3 = (byte)((b0 + 2 * b1) / 3);

      int alphaIndex = 0;
      for (int by = 0; by < 4; by++)
      {
        for (int bx = 0; bx < 4; bx++)
        {
          byte r = 0, g = 0, b = 0, a = 0;
          switch (alphaIndex)
          {
            case 0: a = (byte)((a0 & 0x0F) | ((a0 & 0x0F) << 4)); break;
            case 1: a = (byte)((a0 & 0xF0) | ((a0 & 0xF0) >> 4)); break;
            case 2: a = (byte)((a1 & 0x0F) | ((a1 & 0x0F) << 4)); break;
            case 3: a = (byte)((a1 & 0xF0) | ((a1 & 0xF0) >> 4)); break;
            case 4: a = (byte)((a2 & 0x0F) | ((a2 & 0x0F) << 4)); break;
            case 5: a = (byte)((a2 & 0xF0) | ((a2 & 0xF0) >> 4)); break;
            case 6: a = (byte)((a3 & 0x0F) | ((a3 & 0x0F) << 4)); break;
            case 7: a = (byte)((a3 & 0xF0) | ((a3 & 0xF0) >> 4)); break;
            case 8: a = (byte)((a4 & 0x0F) | ((a4 & 0x0F) << 4)); break;
            case 9: a = (byte)((a4 & 0xF0) | ((a4 & 0xF0) >> 4)); break;
            case 10: a = (byte)((a5 & 0x0F) | ((a5 & 0x0F) << 4)); break;
            case 11: a = (byte)((a5 & 0xF0) | ((a5 & 0xF0) >> 4)); break;
            case 12: a = (byte)((a6 & 0x0F) | ((a6 & 0x0F) << 4)); break;
            case 13: a = (byte)((a6 & 0xF0) | ((a6 & 0xF0) >> 4)); break;
            case 14: a = (byte)((a7 & 0x0F) | ((a7 & 0x0F) << 4)); break;
            case 15: a = (byte)((a7 & 0xF0) | ((a7 & 0xF0) >> 4)); break;
          }
          alphaIndex++;

          uint colorIndex = (lookupTable >> 2 * (4 * by + bx)) & 0x03;
          switch (colorIndex)
          {
            case 0: r = r0; g = g0; b = b0; break;
            case 1: r = r1; g = g1; b = b1; break;
            case 2: r = r2; g = g2; b = b2; break;
            case 3: r = r3; g = g3; b = b3; break;
          }

          int px = (x << 2) + bx;
          int py = (y << 2) + by;
          if ((px < width) && (py < height))
          {
            int offset = ((py * width) + px) << 2;
            uncompressedData[offset] = r;
            uncompressedData[offset + 1] = g;
            uncompressedData[offset + 2] = b;
            uncompressedData[offset + 3] = a;
          }
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Decompresses a buffer using BC3 (DXT5) block compression.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Decompresses a buffer using BC3 (DXT5) block compression.
    /// </summary>
    /// <param name="compressedData">The buffer using BC3 (DXT5) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="compressedData"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is out of range.
    /// </exception>
    public static void DecompressBc3(byte[] compressedData, int width, int height, ref byte[] uncompressedData)
    {
      if (compressedData == null)
        throw new ArgumentNullException("compressedData");

      using (var stream = new MemoryStream(compressedData))
        DecompressBc3(stream, width, height, ref uncompressedData);
    }


    /// <summary>
    /// Decompresses a buffer using BC3 (DXT5) block compression.
    /// </summary>
    /// <param name="stream">The buffer using BC3 (DXT5) block compression.</param>
    /// <param name="width">The width of the uncompressed image in pixels.</param>
    /// <param name="height">The height of the uncompressed image in pixels.</param>
    /// <param name="uncompressedData">The uncompressed image data (RGBA 8:8:8:8).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> or <paramref name="height"/> is out of range.
    /// </exception>
    public static void DecompressBc3(Stream stream, int width, int height, ref byte[] uncompressedData)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");
      if (width <= 0)
        throw new ArgumentOutOfRangeException("width");
      if (height <= 0)
        throw new ArgumentOutOfRangeException("height");

      int uncompressedSize = width * height * 4;
      if (uncompressedData == null)
        uncompressedData = new byte[uncompressedSize];
      else if (uncompressedData.Length != uncompressedSize)
        throw new ArgumentException("Buffer has wrong size.", "uncompressedData");

      using (var reader = new BinaryReader(stream))
      {
        int blockCountX = (width + 3) / 4;
        int blockCountY = (height + 3) / 4;

        for (int y = 0; y < blockCountY; y++)
          for (int x = 0; x < blockCountX; x++)
            DecompressBc3Block(reader, x, y, width, height, uncompressedData);
      }
    }


    private static void DecompressBc3Block(BinaryReader reader, int x, int y, int width, int height, byte[] uncompressedData)
    {
      // Alpha is stored as two 8-bit alpha values.
      byte a0 = reader.ReadByte();
      byte a1 = reader.ReadByte();

      // And 4 x 4 lookup table (16 x 3 bit alpha indices).
      ulong alphaIndices = reader.ReadByte();
      alphaIndices += (ulong)reader.ReadByte() << 8;
      alphaIndices += (ulong)reader.ReadByte() << 16;
      alphaIndices += (ulong)reader.ReadByte() << 24;
      alphaIndices += (ulong)reader.ReadByte() << 32;
      alphaIndices += (ulong)reader.ReadByte() << 40;

      // Derive the other alpha values.
      byte a2, a3, a4, a5, a6, a7;
      if (a0 > a1)
      {
        // 6 interpolated alpha values.
        a2 = (byte)(6 / 7 * a0 + 1 / 7 * a1);
        a3 = (byte)(5 / 7 * a0 + 2 / 7 * a1);
        a4 = (byte)(4 / 7 * a0 + 3 / 7 * a1);
        a5 = (byte)(3 / 7 * a0 + 4 / 7 * a1);
        a6 = (byte)(2 / 7 * a0 + 5 / 7 * a1);
        a7 = (byte)(1 / 7 * a0 + 6 / 7 * a1);
      }
      else
      {
        // 4 interpolated alpha values.
        a2 = (byte)(4 / 5 * a0 + 1 / 5 * a1);
        a3 = (byte)(3 / 5 * a0 + 2 / 5 * a1);
        a4 = (byte)(2 / 5 * a0 + 3 / 5 * a1);
        a5 = (byte)(1 / 5 * a0 + 4 / 5 * a1);
        a6 = 0;
        a7 = 0xFF;
      }

      // Block stores two BGR 5:6:5 colors and a 4x4 lookup table (16 x 2-bit color indices).
      ushort c0 = reader.ReadUInt16();
      ushort c1 = reader.ReadUInt16();
      uint lookupTable = reader.ReadUInt32();

      byte r0, g0, b0;
      DataFormatHelper.Bgr565ToRgb888(c0, out r0, out g0, out b0);

      byte r1, g1, b1;
      DataFormatHelper.Bgr565ToRgb888(c1, out r1, out g1, out b1);

      // Derive the other two colors.
      byte r2 = (byte)((2 * r0 + r1) / 3);
      byte g2 = (byte)((2 * g0 + g1) / 3);
      byte b2 = (byte)((2 * b0 + b1) / 3);

      byte r3 = (byte)((r0 + 2 * r1) / 3);
      byte g3 = (byte)((g0 + 2 * g1) / 3);
      byte b3 = (byte)((b0 + 2 * b1) / 3);

      for (int by = 0; by < 4; by++)
      {
        for (int bx = 0; bx < 4; bx++)
        {
          byte r = 0, g = 0, b = 0, a = 0xff;

          uint alphaIndex = (uint)((alphaIndices >> 3 * (4 * by + bx)) & 0x07);
          switch (alphaIndex)
          {
            case 0: a = a0; break;
            case 1: a = a1; break;
            case 2: a = a2; break;
            case 3: a = a3; break;
            case 4: a = a4; break;
            case 5: a = a5; break;
            case 6: a = a6; break;
            case 7: a = a7; break;
          }

          uint colorIndex = (lookupTable >> 2 * (4 * by + bx)) & 0x03;
          switch (colorIndex)
          {
            case 0: r = r0; g = g0; b = b0; break;
            case 1: r = r1; g = g1; b = b1; break;
            case 2: r = r2; g = g2; b = b2; break;
            case 3: r = r3; g = g3; b = b3; break;
          }

          int px = (x << 2) + bx;
          int py = (y << 2) + by;
          if ((px < width) && (py < height))
          {
            int offset = ((py * width) + px) << 2;
            uncompressedData[offset] = r;
            uncompressedData[offset + 1] = g;
            uncompressedData[offset + 2] = b;
            uncompressedData[offset + 3] = a;
          }
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Mirror, Rotate
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Mirrors the texture/image horizontally.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Mirrors the texture horizontally.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void FlipX(Texture texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        FlipX(image);
#else
      Parallel.ForEach(texture.Images, FlipX);
#endif
    }


    /// <summary>
    /// Mirrors the image horizontally.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void FlipX(Image image)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      using (var image4F = new ImageAccessor(image))
      {
        int width = image4F.Width;
        int height = image4F.Height;
        int halfWidth = width / 2;
        for (int y = 0; y < height; y++)
        {
          for (int x0 = 0; x0 < halfWidth; x0++)
          {
            int x1 = width - x0 - 1;
            Vector4F color0 = image4F.GetPixel(x0, y);
            Vector4F color1 = image4F.GetPixel(x1, y);
            image4F.SetPixel(x0, y, color1);
            image4F.SetPixel(x1, y, color0);
          }
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Mirrors the texture/image vertically.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Mirrors the texture vertically.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void FlipY(Texture texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        FlipY(image);
#else
      Parallel.ForEach(texture.Images, FlipY);
#endif
    }


    /// <summary>
    /// Mirrors the image vertically.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void FlipY(Image image)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      using (var image4F = new ImageAccessor(image))
      {
        int width = image4F.Width;
        int height = image4F.Height;
        int halfHeight = height / 2;
        for (int y0 = 0; y0 < halfHeight; y0++)
        {
          for (int x = 0; x < width; x++)
          {
            int y1 = height - y0 - 1;
            Vector4F color0 = image4F.GetPixel(x, y0);
            Vector4F color1 = image4F.GetPixel(x, y1);
            image4F.SetPixel(x, y0, color1);
            image4F.SetPixel(x, y1, color0);
          }
        }
      }
    }


    /// <summary>
    /// Mirrors the volume texture along the z-axis.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <remarks>
    /// The method does nothing if <paramref name="texture"/> is not a volume texture.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void FlipZ(Texture texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      var description = texture.Description;
      if (description.Dimension != TextureDimension.Texture3D)
        return;

      for (int level = 0; level < description.MipLevels; level++)
      {
        int baseIndex = texture.GetImageIndex(level, 0, 0);
        for (int z0 = 0; z0 < description.Depth / 2; z0++)
        {
          int z1 = description.Depth - z0 - 1;
          int index0 = baseIndex + z0;
          int index1 = baseIndex + z1;
          var image0 = texture.Images[index0];
          var image1 = texture.Images[index1];
          texture.Images[index0] = image1;
          texture.Images[index1] = image0;
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Rotates the texture/image counter-clockwise 0°, 90°, 180°, or 270°.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Rotates the specified texture counter-clockwise 0°, 90°, 180°, or 270°.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="degrees">
    /// The rotation angle in degrees. Allowed values: -360, -270, -180, -90, 0, 90, 180, 270, 360
    /// </param>
    /// <returns>The rotated texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Invalid rotation angle. Allowed values are -360, -270, -180, -90, 0, 90, 180, 270, 360.
    /// </exception>
    public static Texture Rotate(Texture texture, int degrees)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      var description = texture.Description;
      switch (degrees)
      {
        case -360:
        case -180:
        case 0:
        case 180:
        case 360:
          break;

        case -270:
        case -90:
        case 90:
        case 270:
          int width = description.Width;
          int height = description.Height;
          description.Width = height;
          description.Height = width;
          break;

        default:
          throw new ArgumentException("Allowed rotation angles are -360, -270, -180, -90, 0, 90, 180, 270, 360", "degrees");
      }

      var rotatedTexture = new Texture(description);

#if SINGLE_THREADED
      for (int i = 0; i < texture.Images.Count; i++)
        Rotate(texture.Images[i], rotatedTexture.Images[i], degrees);
#else
      Parallel.For(0, texture.Images.Count, i => Rotate(texture.Images[i], rotatedTexture.Images[i], degrees));
#endif

      return rotatedTexture;
    }


    /// <summary>
    /// Rotates the image counter-clockwise 0°, 90°, 180°, or 270°.
    /// </summary>
    /// <param name="srcImage">The unrotated image.</param>
    /// <param name="dstImage">The rotated image.</param>
    /// <param name="degrees">
    /// The angle in degrees. Allowed values: -270, -180, -90, 0, 90, 180, 270
    /// </param>
    private static void Rotate(Image srcImage, Image dstImage, int degrees)
    {
      Debug.Assert(srcImage != null);
      Debug.Assert(dstImage != null);
      Debug.Assert(srcImage.Format == dstImage.Format);

      using (var src = new ImageAccessor(srcImage))
      using (var dst = new ImageAccessor(dstImage))
      {
        int width = srcImage.Width;
        int height = srcImage.Height;
        switch (degrees)
        {
          case -360:
          case 0:
          case 360:
            Debug.Assert(srcImage.Data.Length == dstImage.Data.Length);
            Buffer.BlockCopy(srcImage.Data, 0, dstImage.Data, 0, srcImage.Data.Length);
            break;

          case -180:
          case 180:
            Debug.Assert(srcImage.Width == dstImage.Width || srcImage.Height == dstImage.Height);
            for (int y = 0; y < height; y++)
              for (int x = 0; x < width; x++)
                dst.SetPixel(width - x - 1, height - y - 1, src.GetPixel(x, y));
            break;

          case -270:
          case 90:
            Debug.Assert(srcImage.Width == dstImage.Height || srcImage.Height == dstImage.Width);
            for (int y = 0; y < height; y++)
              for (int x = 0; x < width; x++)
                dst.SetPixel(y, width - x - 1, src.GetPixel(x, y));
            break;

          case -90:
          case 270:
            Debug.Assert(srcImage.Width == dstImage.Height || srcImage.Height == dstImage.Width);
            for (int y = 0; y < height; y++)
              for (int x = 0; x < width; x++)
                dst.SetPixel(height - y - 1, x, src.GetPixel(x, y));
            break;

          default:
            Debug.Fail("Invalid rotation angle.");
            break;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Processing
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Converts the specified texture/image from gamma space to linear space.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the specified texture from gamma space to linear space.
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="gamma">The gamma value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void GammaToLinear(Texture texture, float gamma)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      if (Numeric.AreEqual(gamma, 1.0f))
        return;

      if (gamma <= 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid gamma value {0}. The gamma correction value must be greater than 0.",
          gamma);
        throw new ArgumentException(message, "gamma");
      }

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        GammaToLinear(image, gamma);
#else
      Parallel.ForEach(texture.Images, image => GammaToLinear(image, gamma));
#endif
    }


    /// <summary>
    /// Converts the specified image from gamma space to linear space.
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="gamma">The gamma value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void GammaToLinear(Image image, float gamma)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      if (Numeric.AreEqual(gamma, 1.0f))
        return;

      if (gamma <= 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid gamma value {0}. The gamma correction value must be greater than 0.",
          gamma);
        throw new ArgumentException(message, "gamma");
      }

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            Vector4F color = image4F.GetPixel(x, y);
            color.X = (float)Math.Pow(color.X, gamma);
            color.Y = (float)Math.Pow(color.Y, gamma);
            color.Z = (float)Math.Pow(color.Z, gamma);
            image4F.SetPixel(x, y, color);
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }


    /// <overloads>
    /// <summary>
    /// Converts the specified texture/image from linear space to gamma space.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Converts the specified texture from linear space to gamma space.
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="gamma">The gamma value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void LinearToGamma(Texture texture, float gamma)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      if (Numeric.AreEqual(gamma, 1.0f))
        return;

      if (gamma <= 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid gamma value {0}. The gamma correction value must be greater than 0.",
          gamma);
        throw new ArgumentException(message, "gamma");
      }

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        LinearToGamma(image, gamma);
#else
      Parallel.ForEach(texture.Images, image => LinearToGamma(image, gamma));
#endif
    }


    /// <summary>
    /// Converts the specified image from linear space to gamma space.
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="gamma">The gamma value.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void LinearToGamma(Image image, float gamma)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      if (Numeric.AreEqual(gamma, 1.0f))
        return;

      if (gamma <= 0)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid gamma value {0}. The gamma correction value must be greater than 0.",
          gamma);
        throw new ArgumentException(message, "gamma");
      }

      float gammaCorrection = 1.0f / gamma;

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            Vector4F color = image4F.GetPixel(x, y);
            color.X = (float)Math.Pow(color.X, gammaCorrection);
            color.Y = (float)Math.Pow(color.Y, gammaCorrection);
            color.Z = (float)Math.Pow(color.Z, gammaCorrection);
            image4F.SetPixel(x, y, color);
          }
        }
#if !SINGLE_THREADED
);
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }


    /// <overloads>
    /// <summary>
    /// Applies color keying to the specified texture/image.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Applies color keying to the specified texture (<see cref="DataFormat.R8G8B8A8_UNORM"/>).
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R8G8B8A8_UNORM"/>).</param>
    /// <param name="r">The red component of the color key.</param>
    /// <param name="g">The green component of the color key.</param>
    /// <param name="b">The blue component of the color key.</param>
    /// <param name="a">The alpha component of the color key.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void ApplyColorKey(Texture texture, byte r, byte g, byte b, byte a)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (BitsPerPixel(texture.Description.Format) != 32)
        throw new NotSupportedException(string.Format("The texture format ({0}) is not supported.", texture.Description.Format));

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        ApplyColorKey(image, r, g, b, a);
#else
      Parallel.ForEach(texture.Images, image => ApplyColorKey(image, r, g, b, a));
#endif
    }


    /// <summary>
    /// Applies color keying to the specified image (<see cref="DataFormat.R8G8B8A8_UNORM"/>).
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R8G8B8A8_UNORM"/>).</param>
    /// <param name="r">The red component of the color key.</param>
    /// <param name="g">The green component of the color key.</param>
    /// <param name="b">The blue component of the color key.</param>
    /// <param name="a">The alpha component of the color key.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static unsafe void ApplyColorKey(Image image, byte r, byte g, byte b, byte a)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      // ReSharper disable AccessToDisposedClosure

      uint colorKey;
      switch (image.Format)
      {
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
          colorKey = (r | ((uint)g << 8) | ((uint)b << 16) | ((uint)a << 24));
          break;
        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
          colorKey = (b | ((uint)g << 8) | ((uint)r << 16) | ((uint)a << 24));
          break;
        default:
          throw new NotSupportedException(string.Format("The texture format ({0}) is not supported.", image.Format));
      }

      int size = image.Width * image.Height;
      fixed (byte* dataPtr = image.Data)
      {
        uint* ptr = (uint*)dataPtr;
        for (int i = 0; i < size; i++)
        {
          if (*ptr == colorKey)
            *ptr = 0;

          ptr++;
        }
      }
    }


    /// <summary>
    /// Applies color keying to the specified texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="colorKey">Color of the color key.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void ApplyColorKey(Texture texture, Vector4F colorKey)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        ApplyColorKey(image, colorKey);
#else
      Parallel.ForEach(texture.Images, image => ApplyColorKey(image, colorKey));
#endif
    }


    /// <summary>
    /// Applies color keying to the specified image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="colorKey">Color of the color key.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void ApplyColorKey(Image image, Vector4F colorKey)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            Vector4F color = image4F.GetPixel(x, y);
            if (Vector4F.AreNumericallyEqual(color, colorKey))
              image4F.SetPixel(x, y, new Vector4F(color.X, color.Y, color.Z, 0));
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
    }


    /// <overloads>
    /// <summary>
    /// Premultiplies the alpha value of the specified texture/image.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Premultiplies the alpha value of the specified texture.
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void PremultiplyAlpha(Texture texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        PremultiplyAlpha(image);
#else
      Parallel.ForEach(texture.Images, PremultiplyAlpha);
#endif
    }


    /// <summary>
    /// Premultiplies the alpha value of the specified image.
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void PremultiplyAlpha(Image image)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            Vector4F color = image4F.GetPixel(x, y);
            if (color.W < 1.0f)
            {
              color.X *= color.W;
              color.Y *= color.W;
              color.Z *= color.W;
              image4F.SetPixel(x, y, color);
            }
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }


    /// <overloads>
    /// <summary>
    /// Determines whether the specified texture/image uses the alpha channel.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified texture uses the alpha channel.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="hasAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> requires an alpha channel; otherwise,
    /// <see langword="false"/> if <paramref name="texture"/> is opaque.
    /// </param>
    /// <param name="hasFractionalAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> contains fractional alpha values;
    /// otherwise, <see langword="false"/> if <paramref name="texture"/> is opaque or contains only
    /// binary alpha.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void HasAlpha(Texture texture, out bool hasAlpha, out bool hasFractionalAlpha)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      // Test mip level 0.
      Image[] images;
      switch (texture.Description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          {
            images = new Image[texture.Description.ArraySize];
            for (int arrayIndex = 0; arrayIndex < images.Length; arrayIndex++)
              images[arrayIndex] = texture.Images[texture.GetImageIndex(0, arrayIndex, 0)];
          }
          break;
        case TextureDimension.Texture3D:
          {
            images = new Image[texture.Description.Depth];
            for (int zIndex = 0; zIndex < images.Length; zIndex++)
              images[zIndex] = texture.Images[texture.GetImageIndex(0, 0, zIndex)];
          }
          break;
        default:
          throw new NotSupportedException("The specified texture dimension is not supported.");
      }

      hasAlpha = false;
      hasFractionalAlpha = false;
      foreach (var image in images)
      {
        bool currentHasAlpha, currentHasFractionalAlpha;
        HasAlpha(image, out currentHasAlpha, out currentHasFractionalAlpha);

        hasAlpha = hasAlpha || currentHasAlpha;
        hasFractionalAlpha = currentHasFractionalAlpha;
        if (hasFractionalAlpha)
          return;
      }
    }


    /// <summary>
    /// Determines whether the specified image uses the alpha channel.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="hasAlpha">
    /// <see langword="true"/> if <paramref name="image"/> requires an alpha channel; otherwise,
    /// <see langword="false"/> if <paramref name="image"/> is opaque.
    /// </param>
    /// <param name="hasFractionalAlpha">
    /// <see langword="true"/> if <paramref name="image"/> contains fractional alpha values;
    /// otherwise, <see langword="false"/> if <paramref name="image"/> is opaque or contains only
    /// binary alpha.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public unsafe static void HasAlpha(Image image, out bool hasAlpha, out bool hasFractionalAlpha)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      hasAlpha = false;
      hasFractionalAlpha = false;
      int numberOfPixels = image.Width * image.Height;

      switch (image.Format)
      {
        case DataFormat.R32G32B32A32_FLOAT:
          using (var image4F = new ImageAccessor(image))
          {
            for (int i = 0; i < numberOfPixels; i++)
            {
              Vector4F color = image4F.GetPixel(i);
              if (Numeric.IsLess(color.W, 1.0f))
              {
                hasAlpha = true;
                if (Numeric.IsGreater(color.W, 0.0f))
                {
                  hasFractionalAlpha = true;
                  return;
                }
              }
            }
          }
          break;

        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
          {
            fixed (byte* dataPtr = image.Data)
            {
              byte* ptr = dataPtr + 3;
              for (int i = 0; i < numberOfPixels; i++)
              {
                byte a = *ptr;
                if (a < 0xFF)
                {
                  hasAlpha = true;
                  if (a > 0)
                  {
                    hasFractionalAlpha = true;
                    return;
                  }
                }

                ptr += 4;
              }
            }
          }
          break;

        default:
          throw new NotSupportedException(string.Format("The texture format ({0}) is not supported.", image.Format));
      }
    }


    /// <summary>
    /// Determines the alpha test coverage of the specified image.
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="referenceAlpha">The reference value used in the alpha test.</param>
    /// <param name="alphaScale">A scale factor applied to the alpha value.</param>
    /// <returns>The alpha test coverage of the highest mipmap level.</returns>
    /// <remarks>
    /// The alpha test coverage is the relative amount of pixel that pass the alpha test. This
    /// method assumes that pixels with an alpha value greater or equal to
    /// <paramref name="referenceAlpha" /> pass the alpha test.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    private static float GetAlphaTestCoverage(Image image, float referenceAlpha, float alphaScale = 1)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      float coverage = 0;
      int size = image.Width * image.Height;
      using (var image4F = new ImageAccessor(image))
      {
        for (int i = 0; i < size; i++)
        {
          Vector4F color = image4F.GetPixel(i);
          float alpha = MathHelper.Clamp(color.W * alphaScale, 0, 1);
          if (alpha >= referenceAlpha)
            coverage++;
        }
      }

      return coverage / size;
    }


    /// <overloads>
    /// <summary>
    /// Scales the alpha values to create an equal alpha test coverage across all mipmap levels.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Scales the alpha values to create an equal alpha test coverage across all mipmap levels.
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="referenceAlpha">The reference alpha.</param>
    /// <param name="premultipliedAlpha">
    /// <see langword="true"/> if texture uses premultiplied alpha.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void ScaleAlphaToCoverage(Texture texture, float referenceAlpha, bool premultipliedAlpha)
    {
      // See http://the-witness.net/news/2010/09/computing-alpha-mipmaps/.
      // Reference implementation:
      //  NVIDIA Texture Tools - http://code.google.com/p/nvidia-texture-tools/)
      //  file nvidia-texture-tools\src\nvimage\FloatImage.cpp, method scaleAlphaToCoverage()
      //  (Original code marked as "public domain".)

      if (texture == null)
        throw new ArgumentNullException("texture");
      if (texture.Description.Dimension == TextureDimension.Texture3D)
        throw new ArgumentException("Scaling alpha-to-coverage is not supported for volume textures.");

      int mipLevels = texture.Description.MipLevels;
      int arraySize = texture.Description.ArraySize;
#if SINGLE_THREADED
      for (int arrayIndex = 0; arrayIndex < arraySize; arrayIndex++)
#else
      Parallel.For(0, arraySize, arrayIndex =>
#endif
      {
        int index0 = texture.GetImageIndex(0, arrayIndex, 0);
        float coverage = GetAlphaTestCoverage(texture.Images[index0], referenceAlpha);

#if SINGLE_THREADED
        for (int mipIndex = 0; mipIndex < mipLevels; mipIndex++)
#else
        Parallel.For(1, mipLevels, mipIndex =>
#endif
        {
          int index = texture.GetImageIndex(mipIndex, arrayIndex, 0);
          ScaleAlphaToCoverage(texture.Images[index], referenceAlpha, coverage, premultipliedAlpha);
        }
#if !SINGLE_THREADED
        );
#endif
      }
#if !SINGLE_THREADED
      );
#endif
    }


    /// <summary>
    /// Scales the alpha values of the image to create the desired alpha test coverage.
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="referenceAlpha">The reference alpha.</param>
    /// <param name="desiredCoverage">The desired alpha test coverage.</param>
    /// <param name="premultipliedAlpha">
    /// <see langword="true"/> if texture uses premultiplied alpha.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void ScaleAlphaToCoverage(Image image, float referenceAlpha, float desiredCoverage, bool premultipliedAlpha)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      // To reach the desired alpha test coverage, all alpha values need to be scaled
      // by a certain factor. One solution to find the scale factor is by doing a binary
      // search. 
      float minAlphaScale = 0.0f;
      float maxAlphaScale = 8.0f;
      float alphaScale = 4.0f;

      // In the NVIDIA Texture Tools the binary search is limited to a hardcoded number
      // of steps.
      const int numberOfSteps = 10;
      for (int i = 0; i < numberOfSteps; i++)
      {
        float currentCoverage = GetAlphaTestCoverage(image, referenceAlpha, alphaScale);

        if (currentCoverage < desiredCoverage)
          minAlphaScale = alphaScale;
        else if (currentCoverage > desiredCoverage)
          maxAlphaScale = alphaScale;
        else
          break;

        alphaScale = (minAlphaScale + maxAlphaScale) / 2.0f;
      }

      if (premultipliedAlpha)
      {
        // ReSharper disable AccessToDisposedClosure
        using (var image4F = new ImageAccessor(image))
        {
#if SINGLE_THREADED
          for (int y = 0; y < image4F.Height; y++)
#else
          Parallel.For(0, image4F.Height, y =>
#endif
          {
            for (int x = 0; x < image4F.Width; x++)
            {
              Vector4F color = image4F.GetPixel(x, y);
              float alpha = color.W;
              if (alpha > Numeric.EpsilonF)
              {
                // Undo premultiplication.
                float oneOverAlpha = 1 / alpha;
                color.X *= oneOverAlpha;
                color.Y *= oneOverAlpha;
                color.Z *= oneOverAlpha;

                // Scale alpha.
                alpha = MathHelper.Clamp(alpha * alphaScale, 0, 1);

                // Premultiply alpha.
                color.X *= alpha;
                color.Y *= alpha;
                color.Z *= alpha;
                color.W = alpha;
              }
              image4F.SetPixel(x, y, color);
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }
        // ReSharper restore AccessToDisposedClosure
      }
      else
      {
        // ReSharper disable AccessToDisposedClosure
        using (var image4F = new ImageAccessor(image))
        {
#if SINGLE_THREADED
          for (int y = 0; y < image4F.Height; y++)
#else
          Parallel.For(0, image4F.Height, y =>
#endif
          {
            for (int x = 0; x < image4F.Width; x++)
            {
              Vector4F color = image4F.GetPixel(x, y);
              color.W = MathHelper.Clamp(color.W * alphaScale, 0, 1);
              image4F.SetPixel(x, y, color);
            }
          }
#if !SINGLE_THREADED
          );
#endif
        }
        // ReSharper restore AccessToDisposedClosure
      }
    }


    /// <overloads>
    /// <summary>
    /// Expands the texture/image from unsigned normalized values [0, 1] to signed normalized values
    /// [-1, 1]. (Assumes input data is normal map!)
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Expands the texture from unsigned normalized values [0, 1] to signed normalized values
    /// [-1, 1]. (Assumes input data is normal map!)
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void UnpackNormals(Texture texture)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        UnpackNormals(image);
#else
      Parallel.ForEach(texture.Images, UnpackNormals);
#endif
    }


    /// <summary>
    /// Expands the image from unsigned normalized values [0, 1] to signed normalized values
    /// [-1, 1]. (Assumes input data is normal map!)
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void UnpackNormals(Image image)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            // Only for normal map: (byte)128 maps to (float)0.
            Vector4F color = image4F.GetPixel(x, y);
            color.X = color.X * 255 / 128 - 1;
            color.Y = color.Y * 255 / 128 - 1;
            color.Z = color.Z * 255 / 128 - 1;
            image4F.SetPixel(x, y, color);
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }


    /// <overloads>
    /// <summary>
    /// Prepares a normal map for compression using DXT5 (a.k.a. DXT5nm).
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Prepares a normal map for compression using DXT5 (a.k.a. DXT5nm).
    /// </summary>
    /// <param name="texture">The texture (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="invertY"><see langword="true"/> to invert the y component.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static void ProcessNormals(Texture texture, bool invertY)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

#if SINGLE_THREADED
      foreach (var image in texture.Images)
        ProcessNormals(image, invertY);
#else
      Parallel.ForEach(texture.Images, image => ProcessNormals(image, invertY));
#endif
    }


    /// <summary>
    /// Prepares a normal map for compression using DXT5 (a.k.a. DXT5nm).
    /// </summary>
    /// <param name="image">The image (<see cref="DataFormat.R32G32B32A32_FLOAT"/>).</param>
    /// <param name="invertY"><see langword="true"/> to invert the y component.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    public static void ProcessNormals(Image image, bool invertY)
    {
      if (image == null)
        throw new ArgumentNullException("image");

      float sign = invertY ? -1 : 1;

      // ReSharper disable AccessToDisposedClosure
      using (var image4F = new ImageAccessor(image))
      {
#if SINGLE_THREADED
        for (int y = 0; y < image4F.Height; y++)
#else
        Parallel.For(0, image4F.Height, y =>
#endif
        {
          for (int x = 0; x < image4F.Width; x++)
          {
            Vector4F v = image4F.GetPixel(x, y);
            Vector3F normal = new Vector3F(v.X, v.Y, v.Z);

            // Renormalize normals. (Important for higher mipmap levels.)
            if (!normal.TryNormalize())
              normal = new Vector3F(0, 0, 1);

            // Convert to DXT5nm (xGxR).
            v.X = 0.0f;
            v.Y = sign * normal.Y * 0.5f + 0.5f;
            v.Z = 0.0f;
            v.W = normal.X * 0.5f + 0.5f;

            image4F.SetPixel(x, y, v);
          }
        }
#if !SINGLE_THREADED
        );
#endif
      }
      // ReSharper restore AccessToDisposedClosure
    }
    #endregion
  }
}
