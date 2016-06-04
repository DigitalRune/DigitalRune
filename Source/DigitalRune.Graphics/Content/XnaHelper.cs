// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Provides methods for converting types from/to XNA.
  /// </summary>
  internal static class XnaHelper
  {
    //--------------------------------------------------------------
    #region Meshes
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the specified DigitalRune <see cref="DataFormat"/> to the XNA
    /// <see cref="VertexElementFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="DataFormat"/>.</param>
    /// <returns>The <see cref="VertexElementFormat"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The format is not supported in XNA or MonoGame.
    /// </exception>
    internal static VertexElementFormat ToVertexElementFormat(this DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32_FLOAT:
          return VertexElementFormat.Single;
        case DataFormat.R32G32_FLOAT:
          return VertexElementFormat.Vector2;
        case DataFormat.R32G32B32_FLOAT:
          return VertexElementFormat.Vector3;
        case DataFormat.R32G32B32A32_FLOAT:
          return VertexElementFormat.Vector4;
        case DataFormat.R8G8B8A8_UNORM:
          return VertexElementFormat.Color;
        case DataFormat.R8G8B8A8_UINT:
          return VertexElementFormat.Byte4;
        case DataFormat.R16G16_SINT:
          return VertexElementFormat.Short2;
        case DataFormat.R16G16B16A16_SINT:
          return VertexElementFormat.Short4;
        case DataFormat.R16G16_SNORM:
          return VertexElementFormat.NormalizedShort2;
        case DataFormat.R16G16B16A16_SNORM:
          return VertexElementFormat.NormalizedShort4;
        case DataFormat.R16G16_FLOAT:
          return VertexElementFormat.HalfVector2;
        case DataFormat.R16G16B16A16_FLOAT:
          return VertexElementFormat.HalfVector4;
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Vertex element format ({0}) is not supported in XNA.", format);
          throw new NotSupportedException(message);
      }
    }


    /// <summary>
    /// Converts the specified XNA <see cref="VertexElementFormat"/> to a DigitalRune
    /// <see cref="DataFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="VertexElementFormat"/>.</param>
    /// <returns>The <see cref="DataFormat"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The format cannot be converted to <see cref="DataFormat"/>.
    /// </exception>
    internal static DataFormat ToDataFormat(this VertexElementFormat format)
    {
      switch (format)
      {
        case VertexElementFormat.Single:
          return DataFormat.R32_FLOAT;
        case VertexElementFormat.Vector2:
          return DataFormat.R32G32_FLOAT;
        case VertexElementFormat.Vector3:
          return DataFormat.R32G32B32_FLOAT;
        case VertexElementFormat.Vector4:
          return DataFormat.R32G32B32A32_FLOAT;
        case VertexElementFormat.Color:
          return DataFormat.R8G8B8A8_UNORM;
        case VertexElementFormat.Byte4:
          return DataFormat.R8G8B8A8_UINT;
        case VertexElementFormat.Short2:
          return DataFormat.R16G16_SINT;
        case VertexElementFormat.Short4:
          return DataFormat.R16G16B16A16_SINT;
        case VertexElementFormat.NormalizedShort2:
          return DataFormat.R16G16_SNORM;
        case VertexElementFormat.NormalizedShort4:
          return DataFormat.R16G16B16A16_SNORM;
        case VertexElementFormat.HalfVector2:
          return DataFormat.R16G16_FLOAT;
        case VertexElementFormat.HalfVector4:
          return DataFormat.R16G16B16A16_FLOAT;
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Unexpected vertex element format ({0}).", format);
          throw new NotSupportedException(message);
      }
    }


    /// <summary>
    /// Converts the specified DigitalRune <see cref="VertexElementSemantic"/> to the XNA
    /// <see cref="VertexElementUsage"/>.
    /// </summary>
    /// <param name="semantic">The <see cref="VertexElementSemantic"/>.</param>
    /// <returns>The <see cref="VertexElementUsage"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The semantic is not supported in XNA or MonoGame.
    /// </exception>
    internal static VertexElementUsage ToVertexElementUsage(this VertexElementSemantic semantic)
    {
      switch (semantic)
      {
        case VertexElementSemantic.Binormal:
          return VertexElementUsage.Binormal;
        case VertexElementSemantic.BlendIndices:
          return VertexElementUsage.BlendIndices;
        case VertexElementSemantic.BlendWeight:
          return VertexElementUsage.BlendWeight;
        case VertexElementSemantic.Color:
          return VertexElementUsage.Color;
        case VertexElementSemantic.Normal:
          return VertexElementUsage.Normal;
        case VertexElementSemantic.Position:
          return VertexElementUsage.Position;
        case VertexElementSemantic.PointSize:
          return VertexElementUsage.PointSize;
        case VertexElementSemantic.Tangent:
          return VertexElementUsage.Tangent;
        case VertexElementSemantic.TextureCoordinate:
          return VertexElementUsage.TextureCoordinate;
        case VertexElementSemantic.PositionTransformed:
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Vertex element semantic ({0}) is not supported in XNA.", semantic);
          throw new NotSupportedException(message);
      }
    }


    /// <summary>
    /// Converts the specified XNA <see cref="VertexElementUsage"/> to the DigitalRune
    /// <see cref="VertexElementSemantic"/>.
    /// </summary>
    /// <param name="usage">The <see cref="VertexElementUsage"/>.</param>
    /// <returns>The <see cref="VertexElementSemantic"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The usage cannot be converted to <see cref="VertexElementSemantic"/>.
    /// </exception>
    internal static VertexElementSemantic ToVertexElementSemantic(this VertexElementUsage usage)
    {
      switch (usage)
      {
        case VertexElementUsage.Position:
          return VertexElementSemantic.Position;
        case VertexElementUsage.Color:
          return VertexElementSemantic.Color;
        case VertexElementUsage.TextureCoordinate:
          return VertexElementSemantic.TextureCoordinate;
        case VertexElementUsage.Normal:
          return VertexElementSemantic.Normal;
        case VertexElementUsage.Binormal:
          return VertexElementSemantic.Binormal;
        case VertexElementUsage.Tangent:
          return VertexElementSemantic.Tangent;
        case VertexElementUsage.BlendIndices:
          return VertexElementSemantic.BlendIndices;
        case VertexElementUsage.BlendWeight:
          return VertexElementSemantic.BlendWeight;
        case VertexElementUsage.PointSize:
          return VertexElementSemantic.PointSize;
        case VertexElementUsage.Depth:
        case VertexElementUsage.Fog:
        case VertexElementUsage.Sample:
        case VertexElementUsage.TessellateFactor:
        default:
          string message = string.Format(CultureInfo.InvariantCulture, "Vertex element usage ({0}) is not supported in DigitalRune Graphics.", usage);
          throw new NotSupportedException(message);
      }
    }


    /// <summary>
    /// Converts the specified DigitalRune <see cref="VertexElement"/> to the XNA
    /// <see cref="Microsoft.Xna.Framework.Graphics.VertexElement"/>.
    /// </summary>
    /// <param name="element">The <see cref="VertexElement"/>.</param>
    /// <returns>The <see cref="Microsoft.Xna.Framework.Graphics.VertexElement"/>.</returns>
    internal static Microsoft.Xna.Framework.Graphics.VertexElement ToXna(this VertexElement element)
    {
      return new Microsoft.Xna.Framework.Graphics.VertexElement(
        element.AlignedByteOffset,
        element.Format.ToVertexElementFormat(),
        element.Semantic.ToVertexElementUsage(),
        element.SemanticIndex);
    }


    /// <summary>
    /// Converts the specified XNA <see cref="Microsoft.Xna.Framework.Graphics.VertexElement"/> to
    /// the DigitalRune <see cref="VertexElement"/>.
    /// </summary>
    /// <param name="element">
    /// The <see cref="Microsoft.Xna.Framework.Graphics.VertexElement"/>.
    /// </param>
    /// <returns>The <see cref="VertexElement"/>.</returns>
    internal static VertexElement ToDR(this Microsoft.Xna.Framework.Graphics.VertexElement element)
    {
      return new VertexElement(
        element.VertexElementUsage.ToVertexElementSemantic(),
        element.UsageIndex,
        element.VertexElementFormat.ToDataFormat(),
        element.Offset);
    }
    #endregion


    //--------------------------------------------------------------
    #region Textures
    //--------------------------------------------------------------

    /// <summary>
    /// Converts the specified DigitalRune <see cref="DataFormat"/> to the XNA
    /// <see cref="SurfaceFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="DataFormat"/>.</param>
    /// <returns>The <see cref="SurfaceFormat"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The format is not supported in XNA or MonoGame.
    /// </exception>
    public static SurfaceFormat ToSurfaceFormat(this DataFormat format)
    {
      // Use same order as in MonoGame/SurfaceFormat.cs and keep in sync.
      // sRGB formats are treaded as non-sRGB formats.
      switch (format)
      {
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          return SurfaceFormat.Color;
        case DataFormat.B5G6R5_UNORM:
          return SurfaceFormat.Bgr565;
        case DataFormat.B5G5R5A1_UNORM:
          return SurfaceFormat.Bgra5551;
        case DataFormat.B4G4R4A4_UNORM:
          return SurfaceFormat.Bgra4444;
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
          return SurfaceFormat.Dxt1;
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
          return SurfaceFormat.Dxt3;
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
          return SurfaceFormat.Dxt5;
        case DataFormat.R8G8_SNORM:
          return SurfaceFormat.NormalizedByte2;
        case DataFormat.R8G8B8A8_SNORM:
          return SurfaceFormat.NormalizedByte4;
        case DataFormat.R10G10B10A2_UNORM:
          return SurfaceFormat.Rgba1010102;
        case DataFormat.R16G16_UNORM:
          return SurfaceFormat.Rg32;
        case DataFormat.R16G16B16A16_UNORM:
          return SurfaceFormat.Rgba64;
        case DataFormat.A8_UNORM:
        case DataFormat.R8_UNORM:
          return SurfaceFormat.Alpha8;
        case DataFormat.R32_FLOAT:
          return SurfaceFormat.Single;
        case DataFormat.R32G32_FLOAT:
          return SurfaceFormat.Vector2;
        case DataFormat.R32G32B32A32_FLOAT:
          return SurfaceFormat.Vector4;
        case DataFormat.R16_FLOAT:
          return SurfaceFormat.HalfSingle;
        case DataFormat.R16G16_FLOAT:
          return SurfaceFormat.HalfVector2;
        case DataFormat.R16G16B16A16_FLOAT:
          return SurfaceFormat.HalfVector4;
        //return SurfaceFormat.HdrBlendable;  // Only needed as render target format.

#if MONOGAME
        case DataFormat.B8G8R8X8_UNORM:
          return SurfaceFormat.Bgr32;
        case DataFormat.B8G8R8A8_UNORM:
          return SurfaceFormat.Bgra32;

        case DataFormat.PVRTCI_2bpp_RGB:
          return SurfaceFormat.RgbPvrtc2Bpp;
        case DataFormat.PVRTCI_4bpp_RGB:
          return SurfaceFormat.RgbPvrtc4Bpp;
        case DataFormat.PVRTCI_2bpp_RGBA:
          return SurfaceFormat.RgbaPvrtc2Bpp;
        case DataFormat.PVRTCI_4bpp_RGBA:
          return SurfaceFormat.RgbaPvrtc4Bpp;

        case DataFormat.ETC1:
          return SurfaceFormat.RgbEtc1;

        //case DataFormat.ATC_RGB: Not supported in MonoGame.
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
          return SurfaceFormat.RgbaAtcExplicitAlpha;
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          return SurfaceFormat.RgbaAtcInterpolatedAlpha;
#endif

        default:
          string message = string.Format(CultureInfo.InvariantCulture, "The texture format {0} is not supported in MonoGame.", format);
          throw new NotSupportedException(message);

        // Not supported:
        //  SurfaceFormat.Dxt1a = 70
      }
    }


    /// <summary>
    /// Converts the specified XNA <see cref="SurfaceFormat"/> to the DigitalRune
    /// <see cref="DataFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="SurfaceFormat"/>.</param>
    /// <returns>The <see cref="SurfaceFormat"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The format cannot be converted to <see cref="DataFormat"/>.
    /// </exception>
    public static DataFormat ToDataFormat(this SurfaceFormat format)
    {
      // Use same order as in MonoGame/SurfaceFormat.cs and keep in sync.
      switch (format)
      {
        case SurfaceFormat.Color:
          return DataFormat.R8G8B8A8_UNORM;
        case SurfaceFormat.Bgr565:
          return DataFormat.B5G6R5_UNORM;
        case SurfaceFormat.Bgra5551:
          return DataFormat.B5G5R5A1_UNORM;
        case SurfaceFormat.Bgra4444:
          return DataFormat.B4G4R4A4_UNORM;
        case SurfaceFormat.Dxt1:
          return DataFormat.BC1_UNORM;
        case SurfaceFormat.Dxt3:
          return DataFormat.BC2_UNORM;
        case SurfaceFormat.Dxt5:
          return DataFormat.BC3_UNORM;
        case SurfaceFormat.NormalizedByte2:
          return DataFormat.R8G8_SNORM;
        case SurfaceFormat.NormalizedByte4:
          return DataFormat.R8G8B8A8_SNORM;
        case SurfaceFormat.Rgba1010102:
          return DataFormat.R10G10B10A2_UNORM;
        case SurfaceFormat.Rg32:
          return DataFormat.R16G16_UNORM;
        case SurfaceFormat.Rgba64:
          return DataFormat.R16G16B16A16_UNORM;
        case SurfaceFormat.Alpha8:
          return DataFormat.A8_UNORM;
        case SurfaceFormat.Single:
          return DataFormat.R32_FLOAT;
        case SurfaceFormat.Vector2:
          return DataFormat.R32G32_FLOAT;
        case SurfaceFormat.Vector4:
          return DataFormat.R32G32B32A32_FLOAT;
        case SurfaceFormat.HalfSingle:
          return DataFormat.R16_FLOAT;
        case SurfaceFormat.HalfVector2:
          return DataFormat.R16G16_FLOAT;
        case SurfaceFormat.HalfVector4:
          return DataFormat.R16G16B16A16_FLOAT;

#if MONOGAME
        case SurfaceFormat.Bgr32:
          return DataFormat.B8G8R8X8_UNORM;
        case SurfaceFormat.Bgra32:
          return DataFormat.B8G8R8A8_UNORM;

        case SurfaceFormat.RgbPvrtc2Bpp:
          return DataFormat.PVRTCI_2bpp_RGB;
        case SurfaceFormat.RgbPvrtc4Bpp:
          return DataFormat.PVRTCI_4bpp_RGB;
        case SurfaceFormat.RgbaPvrtc2Bpp:
          return DataFormat.PVRTCI_2bpp_RGBA;
        case SurfaceFormat.RgbaPvrtc4Bpp:
          return DataFormat.PVRTCI_4bpp_RGBA;

        case SurfaceFormat.RgbEtc1:
          return DataFormat.ETC1;

        case SurfaceFormat.RgbaAtcExplicitAlpha:
          return DataFormat.ATC_RGBA_EXPLICIT_ALPHA;
        case SurfaceFormat.RgbaAtcInterpolatedAlpha:
          return DataFormat.ATC_RGBA_INTERPOLATED_ALPHA;
#endif

        default:
          string message = string.Format(CultureInfo.InvariantCulture, "The SurfaceFormat {0} cannot be converted to DataFormat.", format);
          throw new NotSupportedException(message);

        // Not supported:
        //  SurfaceFormat.HdrBlendable // Only needed as render target format.
        //  SurfaceFormat.Dxt1a = 70
      }
    }
    #endregion
  }
}
