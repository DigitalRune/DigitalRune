#region ----- Copyright -----
/*
  The WicHelper is a port of the WIC loader in DirectXTex (see http://directxtex.codeplex.com/)
  which is licensed under the MIT license.


  Copyright (c) 2015 Microsoft Corp

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
  associated documentation files (the "Software"), to deal in the Software without restriction,
  including without limitation the rights to use, copy, modify, merge, publish, distribute,
  sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or
  substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
  NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
  OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.WIC;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Additional options for <see cref="WicHelper"/>.
  /// </summary>
  [Flags]
  internal enum WicFlags
  {
    /// <summary>None.</summary>
    None = 0x0,

    /// <summary>Loads DXGI 1.1 BGR formats as R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats.</summary>
    ForceRgb = 0x1,

    /// <summary>Loads DXGI 1.1 X2 10:10:10:2 format as R10G10B10A2_UNORM.</summary>
    NoX2Bias = 0x2,

    /// <summary>Loads 565, 5551, and 4444 formats as 8888 to avoid use of optional WDDM 1.2 formats.</summary>
    No16Bpp = 0x4,

    /// <summary>Loads 1-bit monochrome (black &amp; white) as R1_UNORM rather than 8-bit grayscale.</summary>
    FlagsAllowMono = 0x8,

    /// <summary>Loads all images in a multi-frame file, converting/resizing to match the first frame as needed, defaults to 0th frame otherwise.</summary>
    AllFrames = 0x10,

    /// <summary>Ignores sRGB metadata if present in the file.</summary>
    IgnoreSrgb = 0x20,

    /// <summary>Use ordered 4x4 dithering for any required conversions.</summary>
    Dither = 0x10000,

    /// <summary>Use error-diffusion dithering for any required conversions.</summary>
    DitherDiffusion = 0x20000,

    /// <summary>Use point filtering for any required image resizing (only needed when loading arrays of differently sized images).</summary>
    FilterPoint = 0x100000,

    /// <summary>Use linear filtering for any required image resizing (only needed when loading arrays of differently sized images).</summary>
    FilterLinear = 0x200000,

    /// <summary>Use cubic filtering for any required image resizing (only needed when loading arrays of differently sized images).</summary>
    FilterCubic = 0x300000,

    /// <summary>Use Fant filtering (default) for any required image resizing (only needed when loading arrays of differently sized images).</summary>
    FilterFant = 0x400000,
  }


  /// <summary>
  /// Provides methods for loading/saving images using the Windows Imaging Component (WIC).
  /// </summary>
  internal static class WicHelper
  {
    //--------------------------------------------------------------
    #region DirectXTexUtil.cpp
    //--------------------------------------------------------------

    // WIC Pixel Format Translation Data
    private struct WicTranslate
    {
      public WicTranslate(Guid wic, DataFormat format, bool srgb)
      {
        Wic = wic;
        Format = format;
        Srgb = srgb;
      }

      public readonly Guid Wic;
      public readonly DataFormat Format;
      public readonly bool Srgb;
    };


    private static readonly WicTranslate[] WicFormats =
    {
      new WicTranslate(PixelFormat.Format128bppRGBAFloat, DataFormat.R32G32B32A32_FLOAT, false),

      new WicTranslate(PixelFormat.Format64bppRGBAHalf, DataFormat.R16G16B16A16_FLOAT, false),
      new WicTranslate(PixelFormat.Format64bppRGBA, DataFormat.R16G16B16A16_UNORM, true),

      new WicTranslate(PixelFormat.Format32bppRGBA, DataFormat.R8G8B8A8_UNORM, true),
      new WicTranslate(PixelFormat.Format32bppBGRA, DataFormat.B8G8R8A8_UNORM, true), // DXGI 1.1
      new WicTranslate(PixelFormat.Format32bppBGR, DataFormat.B8G8R8X8_UNORM, true), // DXGI 1.1

      new WicTranslate(PixelFormat.Format32bppRGBA1010102XR, DataFormat.R10G10B10_XR_BIAS_A2_UNORM, true), // DXGI 1.1
      new WicTranslate(PixelFormat.Format32bppRGBA1010102, DataFormat.R10G10B10A2_UNORM, true),

      new WicTranslate(PixelFormat.Format16bppBGRA5551, DataFormat.B5G5R5A1_UNORM, true),
      new WicTranslate(PixelFormat.Format16bppBGR565, DataFormat.B5G6R5_UNORM, true),

      new WicTranslate(PixelFormat.Format32bppGrayFloat, DataFormat.R32_FLOAT, false),
      new WicTranslate(PixelFormat.Format16bppGrayHalf, DataFormat.R16_FLOAT, false),
      new WicTranslate(PixelFormat.Format16bppGray, DataFormat.R16_UNORM, true),
      new WicTranslate(PixelFormat.Format8bppGray, DataFormat.R8_UNORM, true),

      new WicTranslate(PixelFormat.Format8bppAlpha, DataFormat.A8_UNORM, false),

      new WicTranslate(PixelFormat.FormatBlackWhite, DataFormat.R1_UNORM, false),

#if DIRECTX11_1
      new WicTranslate(PixelFormat.Format96bppRGBFloat, DataFormat.R32G32B32_Float, false),
#endif
    };


    /// <summary>
    /// Converts a WIC <see cref="PixelFormat"/> to a <see cref="DataFormat"/>.
    /// </summary>
    /// <param name="guid">The WIC <see cref="PixelFormat"/>.</param>
    /// <returns>The <see cref="DataFormat"/>.</returns>
    private static DataFormat ToFormat(Guid guid)
    {
      for (int i = 0; i < WicFormats.Length; i++)
      {
        if (WicFormats[i].Wic == guid)
          return WicFormats[i].Format;
      }

      return DataFormat.Unknown;
    }


    /// <summary>
    /// Converts a <see cref="DataFormat"/> to a a WIC <see cref="PixelFormat"/>.
    /// </summary>
    /// <param name="format">The <see cref="DataFormat"/></param>
    /// <param name="ignoreRgbVsBgr">
    /// <see langword="true"/> to use the canonical WIC 32bppBGRA and ignore any BGR to RGB color
    /// changes. This optimization avoids an extra format conversion when using the WIC scaler.
    /// </param>
    /// <returns>The WIC <see cref="PixelFormat"/>.</returns>
    /// <exception cref="NotSupportedException">
    /// The specified texture format is not supported.
    /// </exception>
    private static Guid ToWic(DataFormat format, bool ignoreRgbVsBgr)
    {
      switch (format)
      {
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          if (ignoreRgbVsBgr)
          {
            // If we are not doing conversion so don't really care about BGR vs RGB color-order, we
            // can use the canonical WIC 32bppBGRA format which avoids an extra format conversion
            // when using the WIC scaler.
            return PixelFormat.Format32bppBGRA;
          }
          else
          {
            return PixelFormat.Format32bppRGBA;
          }

        case DataFormat.D32_FLOAT:
          return PixelFormat.Format32bppGrayFloat;

        case DataFormat.D16_UNORM:
          return PixelFormat.Format16bppGray;

        case DataFormat.B8G8R8A8_UNORM_SRGB:
          return PixelFormat.Format32bppBGRA;

        case DataFormat.B8G8R8X8_UNORM_SRGB:
          return PixelFormat.Format32bppBGR;

#if DIRECTX11_1
      case DataFormat.R32G32B32_Float:
          return PixelFormat.Format96bppRGBFloat;
#endif

        default:
          for (int i = 0; i < WicFormats.Length; i++)
          {
            if (WicFormats[i].Format == format)
            {
              return WicFormats[i].Wic;
            }
          }
          break;
      }

      throw new NotSupportedException("The specified texture format is not supported.");
    }
    #endregion


    // WIC Pixel Format nearest conversion table
    private struct WICConvert
    {
      public WICConvert(Guid source, Guid target)
      {
        Source = source;
        Target = target;
      }

      public Guid Source;
      public Guid Target;
    }


    private static readonly WICConvert[] WICConvertTable =
    {
      // Directly support the formats listed in XnaTexUtil::g_WICFormats, so no conversion required
      // Note target Guid in this conversion table must be one of those directly supported formats.

      new WICConvert(PixelFormat.Format1bppIndexed, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format2bppIndexed, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format4bppIndexed, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format8bppIndexed, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM

      new WICConvert(PixelFormat.Format2bppGray, PixelFormat.Format8bppGray), // DXGI_FORMAT_R8_UNORM
      new WICConvert(PixelFormat.Format4bppGray, PixelFormat.Format8bppGray), // DXGI_FORMAT_R8_UNORM

      new WICConvert(PixelFormat.Format16bppGrayFixedPoint, PixelFormat.Format16bppGrayHalf), // DXGI_FORMAT_R16_FLOAT
      new WICConvert(PixelFormat.Format32bppGrayFixedPoint, PixelFormat.Format32bppGrayFloat), // DXGI_FORMAT_R32_FLOAT

      new WICConvert(PixelFormat.Format16bppBGR555, PixelFormat.Format16bppBGRA5551), // DXGI_FORMAT_B5G5R5A1_UNORM 
      new WICConvert(PixelFormat.Format32bppBGR101010, PixelFormat.Format32bppRGBA1010102), // DXGI_FORMAT_R10G10B10A2_UNORM

      new WICConvert(PixelFormat.Format24bppBGR, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format24bppRGB, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format32bppPBGRA, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format32bppPRGBA, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM

      new WICConvert(PixelFormat.Format48bppRGB, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format48bppBGR, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format64bppBGRA, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format64bppPRGBA, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format64bppPBGRA, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM

      new WICConvert(PixelFormat.Format48bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format48bppBGRFixedPoint, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format64bppRGBAFixedPoint, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format64bppBGRAFixedPoint, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format64bppRGBFixedPoint, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format64bppRGBHalf, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format48bppRGBHalf, PixelFormat.Format64bppRGBAHalf), // DXGI_FORMAT_R16G16B16A16_FLOAT

      new WICConvert(PixelFormat.Format128bppPRGBAFloat, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT
      new WICConvert(PixelFormat.Format128bppRGBFloat, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT
      new WICConvert(PixelFormat.Format128bppRGBAFixedPoint, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT
      new WICConvert(PixelFormat.Format128bppRGBFixedPoint, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT
      new WICConvert(PixelFormat.Format32bppRGBE, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT

      new WICConvert(PixelFormat.Format32bppCMYK, PixelFormat.Format32bppRGBA), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format64bppCMYK, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format40bppCMYKAlpha, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format80bppCMYKAlpha, PixelFormat.Format64bppRGBA), // DXGI_FORMAT_R16G16B16A16_UNORM

#if DIRECTX11_1   // (_WIN32_WINNT >= _WIN32_WINNT_WIN8) || defined(_WIN7_PLATFORM_UPDATE)
                  // WIC2 is available on Windows 8 and Windows 7 SP1 with KB 2670838 installed
                  // SharpDX: ImagingFactory2 is only available in Windows 8 build.
      new WICConvert(PixelFormat.Format32bppRGB, PixelFormat.Format32bppRGBA ), // DXGI_FORMAT_R8G8B8A8_UNORM
      new WICConvert(PixelFormat.Format64bppRGB, PixelFormat.Format64bppRGBA ), // DXGI_FORMAT_R16G16B16A16_UNORM
      new WICConvert(PixelFormat.Format64bppPRGBAHalf, PixelFormat.Format64bppRGBAHalf ), // DXGI_FORMAT_R16G16B16A16_FLOAT
      new WICConvert(PixelFormat.Format96bppRGBFixedPoint, PixelFormat.Format96bppRGBFloat ), // DXGI_FORMAT_R32G32B32_FLOAT
#else
      new WICConvert(PixelFormat.Format96bppRGBFixedPoint, PixelFormat.Format128bppRGBAFloat), // DXGI_FORMAT_R32G32B32A32_FLOAT
#endif

      // We don't support n-channel formats
    };


    // Returns the DXGI format and optionally the WIC pixel GUID to convert to.
    private static DataFormat DetermineFormat(ImagingFactory imagingFactory, Guid pixelFormat, WicFlags flags, out Guid convertGuid)
    {
      DataFormat format = ToFormat(pixelFormat);
      convertGuid = Guid.Empty;

      if (format == DataFormat.Unknown)
      {
        for (int i = 0; i < WICConvertTable.Length; ++i)
        {
          if (pixelFormat == WICConvertTable[i].Source)
          {
            convertGuid = WICConvertTable[i].Target;

            format = ToFormat(WICConvertTable[i].Target);
            Debug.Assert(format != DataFormat.Unknown);
            break;
          }
        }
      }

      // Handle special cases based on flags
      switch (format)
      {
        case DataFormat.B8G8R8A8_UNORM: // BGRA
        case DataFormat.B8G8R8X8_UNORM: // BGRX
          if ((flags & WicFlags.ForceRgb) != 0)
          {
            format = DataFormat.R8G8B8A8_UNORM;
            convertGuid = PixelFormat.Format32bppRGBA;
          }
          break;

        case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
          if ((flags & WicFlags.NoX2Bias) != 0)
          {
            format = DataFormat.R10G10B10A2_UNORM;
            convertGuid = PixelFormat.Format32bppRGBA1010102;
          }
          break;

        case DataFormat.B5G5R5A1_UNORM:
        case DataFormat.B5G6R5_UNORM:
          if ((flags & WicFlags.No16Bpp) != 0)
          {
            format = DataFormat.R8G8B8A8_UNORM;
            convertGuid = PixelFormat.Format32bppRGBA;
          }
          break;

        case DataFormat.R1_UNORM:
          if ((flags & WicFlags.FlagsAllowMono) == 0)
          {
            // By default we want to promote a black & white to grayscale since R1 is not a generally supported D3D format
            format = DataFormat.R8_UNORM;
            convertGuid = PixelFormat.Format8bppGray;
          }
          break;
      }

      return format;
    }


    private static TextureDescription DecodeMetadata(ImagingFactory imagingFactory, WicFlags flags, BitmapDecoder decoder, BitmapFrameDecode frame, out Guid pixelFormat)
    {
      var size = frame.Size;

      var description = new TextureDescription
      {
        Dimension = TextureDimension.Texture2D,
        Width = size.Width,
        Height = size.Height,
        Depth = 1,
        MipLevels = 1,
        ArraySize = (flags & WicFlags.AllFrames) != 0 ? decoder.FrameCount : 1,
        Format = DetermineFormat(imagingFactory, frame.PixelFormat, flags, out pixelFormat)
      };

      if (description.Format == DataFormat.Unknown)
        throw new NotSupportedException("The pixel format is not supported.");

      if ((flags & WicFlags.IgnoreSrgb) == 0)
      {
        // Handle sRGB.
#pragma warning disable 168
        try
        {
          Guid containerFormat = decoder.ContainerFormat;
          var metareader = frame.MetadataQueryReader;
          if (metareader != null)
          {
            // Check for sRGB color space metadata.
            bool sRgb = false;

            if (containerFormat == ContainerFormatGuids.Png)
            {
              // Check for sRGB chunk.
              if (metareader.GetMetadataByName("/sRGB/RenderingIntent") != null)
                sRgb = true;
            }
            else if (containerFormat == ContainerFormatGuids.Jpeg)
            {
              if (Equals(metareader.GetMetadataByName("/app1/ifd/exif/{ushort=40961}"), 1))
                sRgb = true;
            }
            else if (containerFormat == ContainerFormatGuids.Tiff)
            {
              if (Equals(metareader.GetMetadataByName("/ifd/exif/{ushort=40961}"), 1))
                sRgb = true;
            }
            else
            {
              if (Equals(metareader.GetMetadataByName("System.Image.ColorSpace"), 1))
                sRgb = true;
            }

            if (sRgb)
              description.Format = TextureHelper.MakeSRgb(description.Format);
          }
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch (Exception exception)
        {
          // Some formats just don't support metadata (BMP, ICO, etc.).
        }
      }
#pragma warning restore 168

      return description;
    }


    private static BitmapDitherType GetWicDither(WicFlags flags)
    {
      if ((flags & WicFlags.Dither) != 0)
        return BitmapDitherType.Ordered4x4;

      if ((flags & WicFlags.DitherDiffusion) != 0)
        return BitmapDitherType.ErrorDiffusion;

      return BitmapDitherType.None;
    }


    private static BitmapInterpolationMode GetWicInterp(WicFlags flags)
    {
      if ((flags & WicFlags.FilterPoint) != 0)
        return BitmapInterpolationMode.NearestNeighbor;

      if ((flags & WicFlags.FilterLinear) != 0)
        return BitmapInterpolationMode.Linear;

      if ((flags & WicFlags.FilterCubic) != 0)
        return BitmapInterpolationMode.Cubic;

      return BitmapInterpolationMode.Fant;
    }


    private static Texture DecodeSingleframe(ImagingFactory imagingFactory, WicFlags flags, TextureDescription description, Guid convertGuid, BitmapFrameDecode frame)
    {
      var texture = new Texture(description);
      var image = texture.Images[0];

      if (convertGuid == Guid.Empty)
      {
        frame.CopyPixels(image.Data, image.RowPitch);
      }
      else
      {
        using (var converter = new FormatConverter(imagingFactory))
        {
          converter.Initialize(frame, convertGuid, GetWicDither(flags), null, 0, BitmapPaletteType.Custom);
          converter.CopyPixels(image.Data, image.RowPitch);
        }
      }

      return texture;
    }


    private static Texture DecodeMultiframe(ImagingFactory imagingFactory, WicFlags flags, TextureDescription description, BitmapDecoder decoder)
    {
      var texture = new Texture(description);
      Guid dstFormat = ToWic(description.Format, false);

      for (int index = 0; index < description.ArraySize; ++index)
      {
        var image = texture.Images[index];
        using (var frame = decoder.GetFrame(index))
        {
          var pfGuid = frame.PixelFormat;
          var size = frame.Size;

          if (size.Width == description.Width && size.Height == description.Height)
          {
            // This frame does not need resized
            if (pfGuid == dstFormat)
            {
              frame.CopyPixels(image.Data, image.RowPitch);
            }
            else
            {
              using (var converter = new FormatConverter(imagingFactory))
              {
                converter.Initialize(frame, dstFormat, GetWicDither(flags), null, 0, BitmapPaletteType.Custom);
                converter.CopyPixels(image.Data, image.RowPitch);
              }
            }
          }
          else
          {
            // This frame needs resizing
            using (var scaler = new BitmapScaler(imagingFactory))
            {
              scaler.Initialize(frame, description.Width, description.Height, GetWicInterp(flags));

              Guid pfScaler = scaler.PixelFormat;
              if (pfScaler == dstFormat)
              {
                scaler.CopyPixels(image.Data, image.RowPitch);
              }
              else
              {
                // The WIC bitmap scaler is free to return a different pixel format than the source image, so here we
                // convert it to our desired format
                using (var converter = new FormatConverter(imagingFactory))
                {
                  converter.Initialize(scaler, dstFormat, GetWicDither(flags), null, 0, BitmapPaletteType.Custom);
                  converter.CopyPixels(image.Data, image.RowPitch);
                }
              }
            }
          }
        }
      }

      return texture;
    }


    private static void EncodeMetadata(BitmapFrameEncode frame, Guid containerFormat, DataFormat format)
    {
      var metawriter = frame.MetadataQueryWriter;
      if (metawriter != null)
      {
        bool sRgb = TextureHelper.IsSRgb(format);

        if (containerFormat == ContainerFormatGuids.Png)
        {
          metawriter.SetMetadataByName("/tEXt/{str=Software}", "DirectXTex");
          if (sRgb)
            metawriter.SetMetadataByName("/sRGB/RenderingIntent", (byte)0);
        }
        else if (containerFormat == ContainerFormatGuids.Jpeg)
        {
          metawriter.SetMetadataByName("/app1/ifd/{ushort=305}", "DirectXTex");
          if (sRgb)
            metawriter.SetMetadataByName("/app1/ifd/exif/{ushort=40961}", (ushort)1);
        }
        else if (containerFormat == ContainerFormatGuids.Tiff)
        {
          metawriter.SetMetadataByName("/ifd/{ushort=305}", "DirectXTex");
          if (sRgb)
            metawriter.SetMetadataByName("/ifd/exif/{ushort=40961}", (ushort)1);
        }
        else
        {
          metawriter.SetMetadataByName("System.ApplicationName", "DirectXTex");
          if (sRgb)
            metawriter.SetMetadataByName("System.Image.ColorSpace", (ushort)1);
        }
      }
    }


    private static void EncodeImage(ImagingFactory imagingFactory, Image image, WicFlags flags, Guid containerFormat, BitmapFrameEncode frame)
    {
      Guid pfGuid = ToWic(image.Format, false);

      frame.Initialize();
      frame.SetSize(image.Width, image.Height);
      frame.SetResolution(72, 72);
      Guid targetGuid = pfGuid;
      frame.SetPixelFormat(ref targetGuid);

      EncodeMetadata(frame, containerFormat, image.Format);

      if (targetGuid != pfGuid)
      {
        // Conversion required to write.
        GCHandle handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
        using (var source = new Bitmap(imagingFactory, image.Width, image.Height, pfGuid, new DataRectangle(handle.AddrOfPinnedObject(), image.RowPitch), image.Data.Length))
        {
          using (var converter = new FormatConverter(imagingFactory))
          {
            if (!converter.CanConvert(pfGuid, targetGuid))
              throw new NotSupportedException("Format conversion is not supported.");

            converter.Initialize(source, targetGuid, GetWicDither(flags), null, 0, BitmapPaletteType.Custom);
            frame.WriteSource(converter, new Rectangle(0, 0, image.Width, image.Height));
          }
        }

        handle.Free();
      }
      else
      {
        // No conversion required.
        frame.WritePixels(image.Height, image.RowPitch, image.Data);
      }

      frame.Commit();
    }


    private static void EncodeSingleframe(ImagingFactory imagingFactory, Image image, Stream stream, Guid containerFormat, WicFlags flags)
    {
      using (var encoder = new BitmapEncoder(imagingFactory, containerFormat, stream))
      {
        using (var frame = new BitmapFrameEncode(encoder))
        {
          if (containerFormat == ContainerFormatGuids.Bmp)
          {
#pragma warning disable 168
            // ReSharper disable once EmptyGeneralCatchClause
            try
            {
              frame.Options.Set("EnableV5Header32bppBGRA", true);
            }
            catch (Exception exception)
            {
              // WIC2 is available on Windows 8 and Windows 7 SP1 with KB 2670838 installed
              // SharpDX: ImagingFactory2 is only available in Windows 8 build.
            }
#pragma warning restore 168
          }

          EncodeImage(imagingFactory, image, flags, containerFormat, frame);
          encoder.Commit();
        }
      }
    }


    private static void EncodeMultiframe(ImagingFactory imagingFactory, IList<Image> images, Stream stream, Guid containerFormat, WicFlags flags)
    {
      using (var encoder = new BitmapEncoder(imagingFactory, containerFormat))
      {
        using (var encoderInfo = encoder.EncoderInfo)
        {
          if (!encoderInfo.IsMultiframeSupported)
            throw new NotSupportedException("The specified image format does not support multiple frames.");
        }

        encoder.Initialize(stream);
        for (int i = 0; i < images.Count; i++)
        {
          var image = images[i];
          using (var frame = new BitmapFrameEncode(encoder))
            EncodeImage(imagingFactory, image, flags, containerFormat, frame);
        }

        encoder.Commit();
      }
    }


    /// <summary>
    /// Loads the specified image(s).
    /// </summary>
    /// <param name="imagingFactory">
    /// The factory for creating components for the Windows Imaging Component (WIC).
    /// </param>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="flags">Additional options.</param>
    /// <returns>A <see cref="Texture"/> representing the image(s).</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="imagingFactory"/>, <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static Texture Load(ImagingFactory imagingFactory, Stream stream, WicFlags flags)
    {
      if (imagingFactory == null)
        throw new ArgumentNullException("imagingFactory");
      if (stream == null)
        throw new ArgumentNullException("stream");

      // Simple version:
      /* 
      using (var bitmapDecoder = new BitmapDecoder(_imagingFactory, stream, DecodeOptions.CacheOnDemand))
      {
        // Convert the image to pre-multiplied RGBA8.
        using (var formatConverter = new FormatConverter(_imagingFactory))
        {
          formatConverter.Initialize(bitmapDecoder.GetFrame(0), PixelFormat.Format32bppPRGBA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);

          // Return a API-independent texture.
          var description = new TextureDescription
          {
            Dimension = TextureDimension.Texture2D,
            Width = formatConverter.Size.Width,
            Height = formatConverter.Size.Height,
            Depth = 1,
            MipLevels = 1,
            ArraySize = 1,
            Format = DataFormat.R8G8B8A8_UNorm
          };

          var texture = new Texture(description);
          var image = texture.Images[0];
          formatConverter.CopyPixels(image.Data, image.RowPitch);

          return texture;
        }
      }
      //*/

      // DirectXTex version:
      using (var decoder = new BitmapDecoder(imagingFactory, stream, DecodeOptions.CacheOnDemand))
      {
        var frame = decoder.GetFrame(0);

        // Get metadata.
        Guid convertGuid;
        var description = DecodeMetadata(imagingFactory, flags, decoder, frame, out convertGuid);

        if (description.ArraySize > 1 && (flags & WicFlags.AllFrames) != 0)
          return DecodeMultiframe(imagingFactory, flags, description, decoder);
        
        return DecodeSingleframe(imagingFactory, flags, description, convertGuid, frame);
      }
    }


    /// <summary>
    /// Saves the specified images.
    /// </summary>
    /// <param name="imagingFactory">
    /// The factory for creating components for the Windows Imaging Component (WIC).
    /// </param>
    /// <param name="images">The images.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="flags">Additional options.</param>
    /// <param name="containerFormat">
    /// The container format (see <see cref="ContainerFormatGuids"/>).
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="imagingFactory"/>, <paramref name="images"/> or <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="images"/> is empty.
    /// </exception>
    public static void Save(ImagingFactory imagingFactory, IList<Image> images, Stream stream, WicFlags flags, Guid containerFormat)
    {
      if (imagingFactory == null)
        throw new ArgumentNullException("imagingFactory");
      if (images == null)
        throw new ArgumentNullException("images");
      if (images.Count == 0)
        throw new ArgumentException("The list of images must not be empty.", "images");
      if (stream == null)
        throw new ArgumentNullException("stream");

      if (images.Count > 1)
        EncodeMultiframe(imagingFactory, images, stream, containerFormat, flags);
      else
        EncodeSingleframe(imagingFactory, images[0], stream, containerFormat, flags);
    }


    /// <summary>
    /// Saves the specified image.
    /// </summary>
    /// <param name="imagingFactory">
    /// The factory for creating components for the Windows Imaging Component (WIC).
    /// </param>
    /// <param name="image">The image.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="flags">Additional options.</param>
    /// <param name="containerFormat">The container format.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="imagingFactory"/>, <paramref name="image"/> or <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static void Save(ImagingFactory imagingFactory, Image image, Stream stream, WicFlags flags, Guid containerFormat)
    {
      if (imagingFactory == null)
        throw new ArgumentNullException("imagingFactory");
      if (image == null)
        throw new ArgumentNullException("image");
      if (stream == null)
        throw new ArgumentNullException("stream");

      EncodeSingleframe(imagingFactory, image, stream, containerFormat, flags);
    }
  }
}
