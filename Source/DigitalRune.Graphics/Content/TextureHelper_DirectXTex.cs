#region ----- Copyright -----
/*
  This is a port of DirectXTex (see http://directxtex.codeplex.com/) which is licensed under the
  MIT license. Extensions to the DDS functionality are marked with [DIGITALRUNE].


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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Additional options for <see cref="TextureHelper.ComputePitch(DataFormat,int,int,out int,out int,ComputePitchFlags)"/>.
  /// </summary>
  [Flags]
  internal enum ComputePitchFlags
  {
    /// <summary>Normal operation.</summary>
    None = 0x0,

    /// <summary>Assume pitch is DWORD aligned instead of BYTE aligned.</summary>
    LegacyDword = 0x1,

    /// <summary>Assume pitch is 16-byte aligned instead of BYTE aligned.</summary>
    Paragraph = 0x2,

    /// <summary>Assume pitch is 32-byte aligned instead of BYTE aligned.</summary>
    Ymm = 0x4,

    /// <summary>Assume pitch is 64-byte aligned instead of BYTE aligned.</summary>
    Zmm = 0x8,

    /// <summary>Assume pitch is 4096-byte aligned instead of BYTE aligned.</summary>
    Page4K = 0x200,

    /// <summary>Override with a legacy 24 bits-per-pixel format size.</summary>
    Bpp24 = 0x10000,

    /// <summary>Override with a legacy 16 bits-per-pixel format size.</summary>
    Bpp16 = 0x20000,

    /// <summary>Override with a legacy 8 bits-per-pixel format size.</summary>
    Bpp8 = 0x40000,
  };


  /// <summary>
  /// Additional options for scanline operations in <see cref="TextureHelper"/>.
  /// </summary>
  [Flags]
  internal enum ScanlineFlags
  {
    /// <summary>Normal operation.</summary>
    None = 0,

    /// <summary>Set alpha channel to known opaque value.</summary>
    SetAlpha = 0x1,

    /// <summary>Enables specific legacy format conversion cases.</summary>
    Legacy = 0x2,
  };



  partial class TextureHelper
  {
    //--------------------------------------------------------------
    #region Format
    //--------------------------------------------------------------

    /// <summary>
    /// Determines whether the specified texture format is valid.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="format"/> is valid; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsValid(DataFormat format)  // [DIGITALRUNE]
    {
      return Enum.IsDefined(typeof(DataFormat), format);
    }


    /// <summary>
    /// Determines whether the specified texture format is a valid DirectDraw surface (DDS) format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="format"/> is a valid DDS format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    internal static bool IsValidDds(DataFormat format)
    {
      return (int)format >= 1 && (int)format <= 190;
    }


    /// <summary>
    /// Determines whether the specified texture format uses a color palette.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> uses a color palette; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsPalettized(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.AI44:
        case DataFormat.IA44:
        case DataFormat.P8:
        case DataFormat.A8P8:
          return true;
        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the specified texture format is a compressed format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is a compressed format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsCompressed(DataFormat format)
    {
      return IsBCn(format) || IsPvrtc(format) || IsEtc(format) || IsAtc(format);
    }


    /// <summary>
    /// Determines whether the specified texture format is a block compressed (BC<i>n</i>) format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is a block compressed (BC<i>n</i>)
    /// format; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsBCn(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is a PowerVR texture compression (PVRTC)
    /// format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is a PowerVR texture compression (PVRTC)
    /// format; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsPvrtc(DataFormat format)  // [DIGITALRUNE]
    {
      switch (format)
      {
        case DataFormat.PVRTCI_2bpp_RGB:
        case DataFormat.PVRTCI_4bpp_RGB:
        case DataFormat.PVRTCI_2bpp_RGBA:
        case DataFormat.PVRTCI_4bpp_RGBA:
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is an Ericcson texture compression (ETC)
    /// format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is an Ericcson texture compression (ETC)
    /// format; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsEtc(DataFormat format)  // [DIGITALRUNE]
    {
      switch (format)
      {
        case DataFormat.ETC1:
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is an AMD texture compression (ATC/ATITC)
    /// format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is an AMD texture compression
    /// (ATC/ATITC) format; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsAtc(DataFormat format)  // [DIGITALRUNE]
    {
      switch (format)
      {
        case DataFormat.ATC_RGB:
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          return true;

        default:
          return false;
      }
    }

    /// <summary>
    /// Determines whether the specified texture format is a packed format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="format"/> is packed; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// YUV formats are divided into packed formats and planar formats. In a packed format, the Y,
    /// U, and V components are stored in a single array. Pixels are organized into groups of
    /// macropixels, whose layout depends on the format. In a planar format, the Y, U, and V
    /// components are stored as three separate planes.
    /// (Source: <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd206750.aspx">Recommended 8-Bit YUV Formats for Video Rendering</see>)
    /// </remarks>
    public static bool IsPacked(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R8G8_B8G8_UNORM:
        case DataFormat.G8R8_G8B8_UNORM:
        case DataFormat.YUY2: // 4:2:2 8-bit
        case DataFormat.Y210: // 4:2:2 10-bit
        case DataFormat.Y216: // 4:2:2 16-bit
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is a planar format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="format"/> is planar; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// YUV formats are divided into packed formats and planar formats. In a packed format, the Y,
    /// U, and V components are stored in a single array. Pixels are organized into groups of
    /// macropixels, whose layout depends on the format. In a planar format, the Y, U, and V
    /// components are stored as three separate planes.
    /// (Source: <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/dd206750.aspx">Recommended 8-Bit YUV Formats for Video Rendering</see>)
    /// </remarks>
    public static bool IsPlanar(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.NV12:             // 4:2:0 8-bit
        case DataFormat.P010:             // 4:2:0 10-bit
        case DataFormat.P016:             // 4:2:0 16-bit
        case DataFormat.Y420_OPAQUE:      // 4:2:0 8-bit
        case DataFormat.NV11:             // 4:1:1 8-bit

        case DataFormat.P208:             // 4:2:2 8-bit
        case DataFormat.V208:             // 4:4:0 8-bit
        case DataFormat.V408:             // 4:4:4 8-bit
                                          // These are JPEG Hardware decode formats (DXGI 1.4)

        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          // These are Xbox One platform specific types
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is a video format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is a video format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsVideo(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.AYUV:
        case DataFormat.Y410:
        case DataFormat.Y416:
        case DataFormat.NV12:
        case DataFormat.P010:
        case DataFormat.P016:
        case DataFormat.YUY2:
        case DataFormat.Y210:
        case DataFormat.Y216:
        case DataFormat.NV11:
        // These video formats can be used with the 3D pipeline through special view mappings.

        case DataFormat.Y420_OPAQUE:
        case DataFormat.AI44:
        case DataFormat.IA44:
        case DataFormat.P8:
        case DataFormat.A8P8:
        // These are limited use video formats not usable in any way by the 3D pipeline.

        case DataFormat.P208:
        case DataFormat.V208:
        case DataFormat.V408:
          // These video formats are for JPEG Hardware decode (DXGI 1.4)
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Gets the bits per pixel for the specified texture format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>The bits per pixel. Returns 0 on failure.</returns>
    public static int BitsPerPixel(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_TYPELESS:
        case DataFormat.R32G32B32A32_FLOAT:
        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
          return 128;

        case DataFormat.R32G32B32_TYPELESS:
        case DataFormat.R32G32B32_FLOAT:
        case DataFormat.R32G32B32_UINT:
        case DataFormat.R32G32B32_SINT:
          return 96;

        case DataFormat.R16G16B16A16_TYPELESS:
        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
        case DataFormat.R32G32_TYPELESS:
        case DataFormat.R32G32_FLOAT:
        case DataFormat.R32G32_UINT:
        case DataFormat.R32G32_SINT:
        case DataFormat.R32G8X24_TYPELESS:
        case DataFormat.D32_FLOAT_S8X24_UINT:
        case DataFormat.R32_FLOAT_X8X24_TYPELESS:
        case DataFormat.X32_TYPELESS_G8X24_UINT:
        case DataFormat.Y416:
        case DataFormat.Y210:
        case DataFormat.Y216:
          return 64;

        case DataFormat.R10G10B10A2_TYPELESS:
        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R11G11B10_FLOAT:
        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.R16G16_TYPELESS:
        case DataFormat.R16G16_FLOAT:
        case DataFormat.R16G16_UNORM:
        case DataFormat.R16G16_UINT:
        case DataFormat.R16G16_SNORM:
        case DataFormat.R16G16_SINT:
        case DataFormat.R32_TYPELESS:
        case DataFormat.D32_FLOAT:
        case DataFormat.R32_FLOAT:
        case DataFormat.R32_UINT:
        case DataFormat.R32_SINT:
        case DataFormat.R24G8_TYPELESS:
        case DataFormat.D24_UNORM_S8_UINT:
        case DataFormat.R24_UNORM_X8_TYPELESS:
        case DataFormat.X24_TYPELESS_G8_UINT:
        case DataFormat.R9G9B9E5_SHAREDEXP:
        case DataFormat.R8G8_B8G8_UNORM:
        case DataFormat.G8R8_G8B8_UNORM:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.B8G8R8X8_TYPELESS:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
        case DataFormat.AYUV:
        case DataFormat.Y410:
        case DataFormat.YUY2:

        case DataFormat.R10G10B10_7E3_A2_FLOAT:
        case DataFormat.R10G10B10_6E4_A2_FLOAT:
        case DataFormat.R10G10B10_SNORM_A2_UNORM:
          // These are Xbox One platform specific types.
          return 32;

        case DataFormat.P010:
        case DataFormat.P016:

        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
        // These are Xbox One platform specific types.

        case DataFormat.V408:
          // These are JPEG Hardware decode formats (DXGI 1.4)
          return 24;

        case DataFormat.R8G8_TYPELESS:
        case DataFormat.R8G8_UNORM:
        case DataFormat.R8G8_UINT:
        case DataFormat.R8G8_SNORM:
        case DataFormat.R8G8_SINT:
        case DataFormat.R16_TYPELESS:
        case DataFormat.R16_FLOAT:
        case DataFormat.D16_UNORM:
        case DataFormat.R16_UNORM:
        case DataFormat.R16_UINT:
        case DataFormat.R16_SNORM:
        case DataFormat.R16_SINT:
        case DataFormat.B5G6R5_UNORM:
        case DataFormat.B5G5R5A1_UNORM:
        case DataFormat.A8P8:
        case DataFormat.B4G4R4A4_UNORM:

        case DataFormat.P208:
        case DataFormat.V208:
          // These are JPEG Hardware decode formats (DXGI 1.4)
          return 16;

        case DataFormat.NV12:
        case DataFormat.Y420_OPAQUE:
        case DataFormat.NV11:
          return 12;

        case DataFormat.R8_TYPELESS:
        case DataFormat.R8_UNORM:
        case DataFormat.R8_UINT:
        case DataFormat.R8_SNORM:
        case DataFormat.R8_SINT:
        case DataFormat.A8_UNORM:
        case DataFormat.AI44:
        case DataFormat.IA44:
        case DataFormat.P8:

        case DataFormat.R4G4_UNORM:
          // These are Xbox One platform specific types.
          return 8;

        case DataFormat.R1_UNORM:
          return 1;

        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
          return 4;

        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
          return 8;

        // [DIGITALRUNE] Non-DirectX formats
        case DataFormat.PVRTCI_2bpp_RGB:
        case DataFormat.PVRTCI_2bpp_RGBA:
          return 2;
        case DataFormat.PVRTCI_4bpp_RGB:
        case DataFormat.PVRTCI_4bpp_RGBA:
        case DataFormat.ETC1:
        case DataFormat.ATC_RGB:
          return 4;
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          return 8;

        default:
          return 0;
      }
    }


    /// <summary>
    /// Gets the bits per color channel for the specified texture format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// The bits per color channel. For mixed formats, it returns the largest color-depth in the
    /// format. Returns 0 on failure.
    /// </returns>
    public static int BitsPerColor(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_TYPELESS:
        case DataFormat.R32G32B32A32_FLOAT:
        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
        case DataFormat.R32G32B32_TYPELESS:
        case DataFormat.R32G32B32_FLOAT:
        case DataFormat.R32G32B32_UINT:
        case DataFormat.R32G32B32_SINT:
        case DataFormat.R32G32_TYPELESS:
        case DataFormat.R32G32_FLOAT:
        case DataFormat.R32G32_UINT:
        case DataFormat.R32G32_SINT:
        case DataFormat.R32G8X24_TYPELESS:
        case DataFormat.D32_FLOAT_S8X24_UINT:
        case DataFormat.R32_FLOAT_X8X24_TYPELESS:
        case DataFormat.X32_TYPELESS_G8X24_UINT:
        case DataFormat.R32_TYPELESS:
        case DataFormat.D32_FLOAT:
        case DataFormat.R32_FLOAT:
        case DataFormat.R32_UINT:
        case DataFormat.R32_SINT:
          return 32;

        case DataFormat.R24G8_TYPELESS:
        case DataFormat.D24_UNORM_S8_UINT:
        case DataFormat.R24_UNORM_X8_TYPELESS:
        case DataFormat.X24_TYPELESS_G8_UINT:
          return 24;

        case DataFormat.R16G16B16A16_TYPELESS:
        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
        case DataFormat.R16G16_TYPELESS:
        case DataFormat.R16G16_FLOAT:
        case DataFormat.R16G16_UNORM:
        case DataFormat.R16G16_UINT:
        case DataFormat.R16G16_SNORM:
        case DataFormat.R16G16_SINT:
        case DataFormat.R16_TYPELESS:
        case DataFormat.R16_FLOAT:
        case DataFormat.D16_UNORM:
        case DataFormat.R16_UNORM:
        case DataFormat.R16_UINT:
        case DataFormat.R16_SNORM:
        case DataFormat.R16_SINT:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
        case DataFormat.Y416:
        case DataFormat.P016:
        case DataFormat.Y216:

        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          // These are Xbox One platform specific types.
          return 16;

        case DataFormat.R9G9B9E5_SHAREDEXP:
          return 14;

        case DataFormat.R11G11B10_FLOAT:
          return 11;

        case DataFormat.R10G10B10A2_TYPELESS:
        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
        case DataFormat.Y410:
        case DataFormat.P010:
        case DataFormat.Y210:

        case DataFormat.R10G10B10_7E3_A2_FLOAT:
        case DataFormat.R10G10B10_6E4_A2_FLOAT:
        case DataFormat.R10G10B10_SNORM_A2_UNORM:
          // These are Xbox One platform specific types.
          return 10;

        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.R8G8_TYPELESS:
        case DataFormat.R8G8_UNORM:
        case DataFormat.R8G8_UINT:
        case DataFormat.R8G8_SNORM:
        case DataFormat.R8G8_SINT:
        case DataFormat.R8_TYPELESS:
        case DataFormat.R8_UNORM:
        case DataFormat.R8_UINT:
        case DataFormat.R8_SNORM:
        case DataFormat.R8_SINT:
        case DataFormat.A8_UNORM:
        case DataFormat.R8G8_B8G8_UNORM:
        case DataFormat.G8R8_G8B8_UNORM:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.B8G8R8X8_TYPELESS:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
        case DataFormat.AYUV:
        case DataFormat.NV12:
        case DataFormat.Y420_OPAQUE:
        case DataFormat.YUY2:
        case DataFormat.NV11:

        case DataFormat.P208:
        case DataFormat.V208:
        case DataFormat.V408:
          // These are JPEG Hardware decode formats (DXGI 1.4).
          return 8;

        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
          return 7;

        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.B5G6R5_UNORM:
          return 6;

        case DataFormat.B5G5R5A1_UNORM:
          return 5;

        case DataFormat.B4G4R4A4_UNORM:

        case DataFormat.R4G4_UNORM:
          // These are Xbox One platform specific types.
          return 4;

        case DataFormat.R1_UNORM:
          return 1;

        // [DIGITALRUNE] Non-DirectX formats
        case DataFormat.PVRTCI_2bpp_RGB:
        case DataFormat.PVRTCI_4bpp_RGB:
        case DataFormat.PVRTCI_2bpp_RGBA:
        case DataFormat.PVRTCI_4bpp_RGBA:
          return 5;   // 5 in opaque color mode, 4 in transparent color mode.

        case DataFormat.ETC1:
          return 5;   // 4 in individual mode, 5 in differential mode.

        case DataFormat.ATC_RGB:
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          return 0;   // Not disclosed by AMD.

        case DataFormat.AI44:
        case DataFormat.IA44:
        case DataFormat.P8:
        case DataFormat.A8P8:
        // Palettized formats return 0 for this function.

        default:
          return 0;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is a depth-stencil format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is a depth-stencil format; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsDepthStencil(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.D32_FLOAT_S8X24_UINT:
        case DataFormat.R32_FLOAT_X8X24_TYPELESS:
        case DataFormat.X32_TYPELESS_G8X24_UINT:
        case DataFormat.D32_FLOAT:
        case DataFormat.D24_UNORM_S8_UINT:
        case DataFormat.R24_UNORM_X8_TYPELESS:
        case DataFormat.X24_TYPELESS_G8_UINT:
        case DataFormat.D16_UNORM:

        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          // These are Xbox One platform specific types.
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is sRGB.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is sRGB; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsSRgb(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
        case DataFormat.BC7_UNORM_SRGB:
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format is typeless.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <param name="partialTypeless">
    /// <see langword="true"/> to check for partial typeless formats.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is typeless; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public static bool IsTypeless(DataFormat format, bool partialTypeless)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_TYPELESS:
        case DataFormat.R32G32B32_TYPELESS:
        case DataFormat.R16G16B16A16_TYPELESS:
        case DataFormat.R32G32_TYPELESS:
        case DataFormat.R32G8X24_TYPELESS:
        case DataFormat.R10G10B10A2_TYPELESS:
        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R16G16_TYPELESS:
        case DataFormat.R32_TYPELESS:
        case DataFormat.R24G8_TYPELESS:
        case DataFormat.R8G8_TYPELESS:
        case DataFormat.R16_TYPELESS:
        case DataFormat.R8_TYPELESS:
        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8X8_TYPELESS:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC7_TYPELESS:
          return true;

        case DataFormat.R32_FLOAT_X8X24_TYPELESS:
        case DataFormat.X32_TYPELESS_G8X24_UINT:
        case DataFormat.R24_UNORM_X8_TYPELESS:
        case DataFormat.X24_TYPELESS_G8_UINT:
          return partialTypeless;

        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          // These are Xbox One platform specific types.
          return partialTypeless;

        default:
          return false;
      }
    }


    /// <summary>
    /// Determines whether the specified texture format stores an alpha channel.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="format"/> contains an alpha channel;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasAlpha(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_TYPELESS:
        case DataFormat.R32G32B32A32_FLOAT:
        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
        case DataFormat.R16G16B16A16_TYPELESS:
        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
        case DataFormat.R10G10B10A2_TYPELESS:
        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.A8_UNORM:
        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.B5G5R5A1_UNORM:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
        case DataFormat.AYUV:
        case DataFormat.Y410:
        case DataFormat.Y416:
        case DataFormat.AI44:
        case DataFormat.IA44:
        case DataFormat.A8P8:
        case DataFormat.B4G4R4A4_UNORM:

        case DataFormat.R10G10B10_7E3_A2_FLOAT:
        case DataFormat.R10G10B10_6E4_A2_FLOAT:
        case DataFormat.R10G10B10_SNORM_A2_UNORM:
        // These are Xbox One platform specific types.

        // [DIGITALRUNE] Non-DirectX formats
        case DataFormat.PVRTCI_2bpp_RGBA:
        case DataFormat.PVRTCI_4bpp_RGBA:
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          return true;

        default:
          return false;
      }
    }


    /// <summary>
    /// Converts the specified texture format to an sRGB equivalent type if available.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>The sRGB equivalent type if available.</returns>
    public static DataFormat MakeSRgb(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R8G8B8A8_UNORM:
          return DataFormat.R8G8B8A8_UNORM_SRGB;

        case DataFormat.BC1_UNORM:
          return DataFormat.BC1_UNORM_SRGB;

        case DataFormat.BC2_UNORM:
          return DataFormat.BC2_UNORM_SRGB;

        case DataFormat.BC3_UNORM:
          return DataFormat.BC3_UNORM_SRGB;

        case DataFormat.B8G8R8A8_UNORM:
          return DataFormat.B8G8R8A8_UNORM_SRGB;

        case DataFormat.B8G8R8X8_UNORM:
          return DataFormat.B8G8R8X8_UNORM_SRGB;

        case DataFormat.BC7_UNORM:
          return DataFormat.BC7_UNORM_SRGB;

        default:
          return format;
      }
    }


    /// <summary>
    /// Converts to a format to an equivalent TYPELESS format if available.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>The equivalent TYPELESS format if available.</returns>
    public static DataFormat MakeTypeless(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_FLOAT:
        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
          return DataFormat.R32G32B32A32_TYPELESS;

        case DataFormat.R32G32B32_FLOAT:
        case DataFormat.R32G32B32_UINT:
        case DataFormat.R32G32B32_SINT:
          return DataFormat.R32G32B32_TYPELESS;

        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
          return DataFormat.R16G16B16A16_TYPELESS;

        case DataFormat.R32G32_FLOAT:
        case DataFormat.R32G32_UINT:
        case DataFormat.R32G32_SINT:
          return DataFormat.R32G32_TYPELESS;

        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R10G10B10_7E3_A2_FLOAT:
        case DataFormat.R10G10B10_6E4_A2_FLOAT:
        case DataFormat.R10G10B10_SNORM_A2_UNORM:
          return DataFormat.R10G10B10A2_TYPELESS;

        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
          return DataFormat.R8G8B8A8_TYPELESS;

        case DataFormat.R16G16_FLOAT:
        case DataFormat.R16G16_UNORM:
        case DataFormat.R16G16_UINT:
        case DataFormat.R16G16_SNORM:
        case DataFormat.R16G16_SINT:
          return DataFormat.R16G16_TYPELESS;

        case DataFormat.D32_FLOAT:
        case DataFormat.R32_FLOAT:
        case DataFormat.R32_UINT:
        case DataFormat.R32_SINT:
          return DataFormat.R32_TYPELESS;

        case DataFormat.R8G8_UNORM:
        case DataFormat.R8G8_UINT:
        case DataFormat.R8G8_SNORM:
        case DataFormat.R8G8_SINT:
          return DataFormat.R8G8_TYPELESS;

        case DataFormat.R16_FLOAT:
        case DataFormat.D16_UNORM:
        case DataFormat.R16_UNORM:
        case DataFormat.R16_UINT:
        case DataFormat.R16_SNORM:
        case DataFormat.R16_SINT:
          return DataFormat.R16_TYPELESS;

        case DataFormat.R8_UNORM:
        case DataFormat.R8_UINT:
        case DataFormat.R8_SNORM:
        case DataFormat.R8_SINT:
        case DataFormat.R4G4_UNORM:
          return DataFormat.R8_TYPELESS;

        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
          return DataFormat.BC1_TYPELESS;

        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
          return DataFormat.BC2_TYPELESS;

        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
          return DataFormat.BC3_TYPELESS;

        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
          return DataFormat.BC4_TYPELESS;

        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
          return DataFormat.BC5_TYPELESS;

        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
          return DataFormat.B8G8R8A8_TYPELESS;

        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
          return DataFormat.B8G8R8X8_TYPELESS;

        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
          return DataFormat.BC6H_TYPELESS;

        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
          return DataFormat.BC7_TYPELESS;

        default:
          return format;
      }
    }


    /// <summary>
    /// Converts to a TYPELESS format to an equivalent UNORM format if available.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>The equivalent UNORM format if available.</returns>
    public static DataFormat MakeTypelessUNorm(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R16G16B16A16_TYPELESS:
          return DataFormat.R16G16B16A16_UNORM;

        case DataFormat.R10G10B10A2_TYPELESS:
          return DataFormat.R10G10B10A2_UNORM;

        case DataFormat.R8G8B8A8_TYPELESS:
          return DataFormat.R8G8B8A8_UNORM;

        case DataFormat.R16G16_TYPELESS:
          return DataFormat.R16G16_UNORM;

        case DataFormat.R8G8_TYPELESS:
          return DataFormat.R8G8_UNORM;

        case DataFormat.R16_TYPELESS:
          return DataFormat.R16_UNORM;

        case DataFormat.R8_TYPELESS:
          return DataFormat.R8_UNORM;

        case DataFormat.BC1_TYPELESS:
          return DataFormat.BC1_UNORM;

        case DataFormat.BC2_TYPELESS:
          return DataFormat.BC2_UNORM;

        case DataFormat.BC3_TYPELESS:
          return DataFormat.BC3_UNORM;

        case DataFormat.BC4_TYPELESS:
          return DataFormat.BC4_UNORM;

        case DataFormat.BC5_TYPELESS:
          return DataFormat.BC5_UNORM;

        case DataFormat.B8G8R8A8_TYPELESS:
          return DataFormat.B8G8R8A8_UNORM;

        case DataFormat.B8G8R8X8_TYPELESS:
          return DataFormat.B8G8R8X8_UNORM;

        case DataFormat.BC7_TYPELESS:
          return DataFormat.BC7_UNORM;

        default:
          return format;
      }
    }


    /// <summary>
    /// Converts to a TYPELESS format to an equivalent FLOAT format if available.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <returns>The equivalent FLOAT format if available.</returns>
    public static DataFormat MakeTypelessFloat(DataFormat format)
    {
      switch (format)
      {
        case DataFormat.R32G32B32A32_TYPELESS:
          return DataFormat.R32G32B32A32_FLOAT;

        case DataFormat.R32G32B32_TYPELESS:
          return DataFormat.R32G32B32_FLOAT;

        case DataFormat.R16G16B16A16_TYPELESS:
          return DataFormat.R16G16B16A16_FLOAT;

        case DataFormat.R32G32_TYPELESS:
          return DataFormat.R32G32_FLOAT;

        case DataFormat.R16G16_TYPELESS:
          return DataFormat.R16G16_FLOAT;

        case DataFormat.R32_TYPELESS:
          return DataFormat.R32_FLOAT;

        case DataFormat.R16_TYPELESS:
          return DataFormat.R16_FLOAT;

        default:
          return format;
      }
    }


    /// <summary>
    /// Gets the number of scanlines used publicly by the specified texture format.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The number of scanlines.</returns>
    public static int ComputeScanlines(DataFormat format, int height)
    {
      switch (format)
      {
        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:

        case DataFormat.PVRTCI_2bpp_RGB:
        case DataFormat.PVRTCI_4bpp_RGB:
        case DataFormat.PVRTCI_2bpp_RGBA:
        case DataFormat.PVRTCI_4bpp_RGBA:
        case DataFormat.ETC1:
        case DataFormat.ATC_RGB:
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          Debug.Assert(IsCompressed(format));
          // Blocks (BCn, PVRTC, ETC, ATC) have a height of 4 pixels.
          return Math.Max(1, (height + 3) / 4);

        case DataFormat.NV11:
        case DataFormat.P208:
          Debug.Assert(IsPlanar(format));
          return height * 2;

        case DataFormat.V208:
          Debug.Assert(IsPlanar(format));
          return height + ((height + 1) >> 1) * 2;

        case DataFormat.V408:
          Debug.Assert(IsPlanar(format));
          return height + (height >> 1) * 4;

        case DataFormat.NV12:
        case DataFormat.P010:
        case DataFormat.P016:
        case DataFormat.Y420_OPAQUE:
        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          Debug.Assert(IsPlanar(format));
          return height + ((height + 1) >> 1);

        default:
          Debug.Assert(IsValid(format));
          Debug.Assert(!IsCompressed(format) && !IsPlanar(format));
          return height;
      }
    }


    /// <overloads>
    /// <summary>
    /// Gets the image row pitch (in bytes) and the slice pitch (size of the image in bytes) based
    /// on format, width, and height.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the image row pitch (in bytes) and the slice pitch (size of the image in bytes) based
    /// on format, width, and height.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="rowPitch">The row pitch in bytes.</param>
    /// <param name="slicePitch">The image size in bytes.</param>
    public static void ComputePitch(DataFormat format, int width, int height, out int rowPitch, out int slicePitch)
    {
      ComputePitch(format, width, height, out rowPitch, out slicePitch, ComputePitchFlags.None);
    }


    /// <summary>
    /// Gets the image row pitch (in bytes) and the slice pitch (size of the image in bytes) based
    /// on format, width, height and additional options.
    /// </summary>
    /// <param name="format">The texture format.</param>
    /// <param name="width">The image width in pixels.</param>
    /// <param name="height">The image height in pixels.</param>
    /// <param name="rowPitch">The row pitch in bytes.</param>
    /// <param name="slicePitch">The image size in bytes.</param>
    /// <param name="flags">Additional options.</param>
    public static void ComputePitch(DataFormat format, int width, int height, out int rowPitch, out int slicePitch, ComputePitchFlags flags)
    {
      switch (format)
      {
        case DataFormat.BC1_TYPELESS:
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
        case DataFormat.BC4_TYPELESS:
        case DataFormat.BC4_UNORM:
        case DataFormat.BC4_SNORM:
          {
            Debug.Assert(IsBCn(format));

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * 8;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.BC2_TYPELESS:
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
        case DataFormat.BC3_TYPELESS:
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
        case DataFormat.BC5_TYPELESS:
        case DataFormat.BC5_UNORM:
        case DataFormat.BC5_SNORM:
        case DataFormat.BC6H_TYPELESS:
        case DataFormat.BC6H_UF16:
        case DataFormat.BC6H_SF16:
        case DataFormat.BC7_TYPELESS:
        case DataFormat.BC7_UNORM:
        case DataFormat.BC7_UNORM_SRGB:
          {
            Debug.Assert(IsBCn(format));

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * 16;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.PVRTCI_2bpp_RGB:
        case DataFormat.PVRTCI_2bpp_RGBA:
          {
            Debug.Assert(IsPvrtc(format));

            // Bytes per block.
            int bpb = 8;

            // Additional limitations set by Apple:
            // (Reference https://developer.apple.com/library/ios/documentation/3DDrawing/Conceptual/OpenGLES_ProgrammingGuide/TextureTool/TextureTool.html)
            //
            // - width and height must be power of two.
            // - width and height must be square.
            // - width and height must be at least 8.
            //   (PVRTexLib returns the same slice pitch for size = 1, 2, 4 and 8!)
            // - Minimum data size is 32 bytes (= 2x2 blocks).
            //   => Minimum size for 2 bpp mode: 16x8 pixels
            //      Minimum size for 4 bpp mode: 8x8 pixels.

            // 2 bpp mode: block = 8x4 pixels
            width = Math.Max(16, width);
            height = Math.Max(8, height);

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 7) / 8);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * bpb;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.PVRTCI_4bpp_RGB:
        case DataFormat.PVRTCI_4bpp_RGBA:
          {
            Debug.Assert(IsPvrtc(format));

            // Bytes per block.
            int bpb = 8;

            // Additional limitations set by Apple:
            // (Reference https://developer.apple.com/library/ios/documentation/3DDrawing/Conceptual/OpenGLES_ProgrammingGuide/TextureTool/TextureTool.html)
            //
            // - width and height must be power of two.
            // - width and height must be square.
            // - width and height must be at least 8.
            //   (PVRTexLib returns the same slice pitch for size = 1, 2, 4 and 8!)
            // - Minimum data size is 32 bytes (= 2x2 blocks).
            //   => Minimum size for 2 bpp mode: 16x8 pixels
            //      Minimum size for 4 bpp mode: 8x8 pixels.

            // 4 bpp mode: block = 4x4 pixels
            width = Math.Max(8, width);
            height = Math.Max(8, height);

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * bpb;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.ETC1:
          {
            Debug.Assert(IsEtc(format));

            // Reference: https://www.khronos.org/registry/gles/extensions/OES/OES_compressed_ETC1_RGB8_texture.txt

            // Bytes per block.
            int bpb = 8;

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * bpb;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.ATC_RGB:
          {
            Debug.Assert(IsAtc(format));

            // Reference: https://www.khronos.org/registry/gles/extensions/AMD/AMD_compressed_ATC_texture.txt

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * 8;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
          {
            Debug.Assert(IsAtc(format));

            // Reference: https://www.khronos.org/registry/gles/extensions/AMD/AMD_compressed_ATC_texture.txt

            // Width and height in blocks.
            int nbw = Math.Max(1, (width + 3) / 4);
            int nbh = Math.Max(1, (height + 3) / 4);

            rowPitch = nbw * 16;
            slicePitch = rowPitch * nbh;
          }
          break;

        case DataFormat.R8G8_B8G8_UNORM:
        case DataFormat.G8R8_G8B8_UNORM:
        case DataFormat.YUY2:
          {
            Debug.Assert(IsPacked(format));

            rowPitch = ((width + 1) >> 1) * 4;
            slicePitch = rowPitch * height;
          }
          break;

        case DataFormat.Y210: // 4:2:2 10-bit
        case DataFormat.Y216: // 4:2:2 16-bit
          {
            Debug.Assert(IsPacked(format));

            rowPitch = ((width + 1) >> 1) * 8;
            slicePitch = rowPitch * height;
          }
          break;

        case DataFormat.NV12:
        case DataFormat.Y420_OPAQUE:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = ((width + 1) >> 1) * 2;
            slicePitch = rowPitch * (height + ((height + 1) >> 1));
          }
          break;

        case DataFormat.P010:
        case DataFormat.P016:
        case DataFormat.D16_UNORM_S8_UINT:
        case DataFormat.R16_UNORM_X8_TYPELESS:
        case DataFormat.X16_TYPELESS_G8_UINT:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = ((width + 1) >> 1) * 4;
            slicePitch = rowPitch * (height + ((height + 1) >> 1));
          }
          break;

        case DataFormat.NV11:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = ((width + 3) >> 2) * 4;

            // Direct3D makes this simplifying assumption, although it is larger than the 4:1:1 data.
            slicePitch = rowPitch * height * 2;
          }
          break;

        case DataFormat.P208:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = ((width + 1) >> 1) * 2;
            slicePitch = rowPitch * height * 2;
          }
          break;

        case DataFormat.V208:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = width;
            slicePitch = rowPitch * (height + (((height + 1) >> 1) * 2));
          }
          break;

        case DataFormat.V408:
          {
            Debug.Assert(IsPlanar(format));

            rowPitch = width;
            slicePitch = rowPitch * (height + ((height >> 1) * 4));
          }
          break;

        default:
          {
            Debug.Assert(IsValid(format));
            Debug.Assert(!IsCompressed(format) && !IsPacked(format) && !IsPlanar(format));

            int bpp;

            if ((flags & ComputePitchFlags.Bpp24) != 0)
              bpp = 24;
            else if ((flags & ComputePitchFlags.Bpp16) != 0)
              bpp = 16;
            else if ((flags & ComputePitchFlags.Bpp8) != 0)
              bpp = 8;
            else
              bpp = BitsPerPixel(format);

            if ((flags & (ComputePitchFlags.LegacyDword | ComputePitchFlags.Paragraph | ComputePitchFlags.Ymm | ComputePitchFlags.Zmm | ComputePitchFlags.Page4K)) != 0)
            {
              if ((flags & ComputePitchFlags.Page4K) != 0)
              {
                rowPitch = ((width * bpp + 32767) / 32768) * 4096;
                slicePitch = rowPitch * height;
              }
              else if ((flags & ComputePitchFlags.Zmm) != 0)
              {
                rowPitch = ((width * bpp + 511) / 512) * 64;
                slicePitch = rowPitch * height;
              }
              else if ((flags & ComputePitchFlags.Ymm) != 0)
              {
                rowPitch = ((width * bpp + 255) / 256) * 32;
                slicePitch = rowPitch * height;
              }
              else if ((flags & ComputePitchFlags.Paragraph) != 0)
              {
                rowPitch = ((width * bpp + 127) / 128) * 16;
                slicePitch = rowPitch * height;
              }
              else // DWORD alignment
              {
                // Special computation for some incorrectly created DDS files based on
                // legacy DirectDraw assumptions about pitch alignment
                rowPitch = ((width * bpp + 31) / 32) * Marshal.SizeOf(typeof(uint));
                slicePitch = rowPitch * height;
              }
            }
            else
            {
              // Default byte alignment
              rowPitch = (width * bpp + 7) / 8;
              slicePitch = rowPitch * height;
            }
          }
          break;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXTexConvert.cpp
    //--------------------------------------------------------------

    /// <summary>
    /// Copies an image row with optional clearing of alpha value to 1.0.
    /// </summary>
    internal static void CopyScanline(BinaryReader reader, int inSize, BinaryWriter writer, int outSize, DataFormat format, ScanlineFlags flags)
    {
      Debug.Assert(reader != null && inSize > 0);
      Debug.Assert(writer != null && outSize > 0);
      Debug.Assert(IsValidDds(format) && !IsPalettized(format));

      if ((flags & ScanlineFlags.SetAlpha) != 0)
      {
        switch (format)
        {
          //-----------------------------------------------------------------------------
          case DataFormat.R32G32B32A32_TYPELESS:
          case DataFormat.R32G32B32A32_FLOAT:
          case DataFormat.R32G32B32A32_UINT:
          case DataFormat.R32G32B32A32_SINT:
            if (inSize >= 16 && outSize >= 16)
            {
              uint alpha;
              if (format == DataFormat.R32G32B32A32_FLOAT)
                alpha = 0x3f800000;
              else if (format == DataFormat.R32G32B32A32_SINT)
                alpha = 0x7fffffff;
              else
                alpha = 0xffffffff;

              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 15); count += 16)
              {
                writer.Write(reader.ReadInt32());
                writer.Write(reader.ReadInt32());
                writer.Write(reader.ReadInt32());
                reader.ReadInt32(); // Ignore
                writer.Write(alpha);
              }
            }
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.R16G16B16A16_TYPELESS:
          case DataFormat.R16G16B16A16_FLOAT:
          case DataFormat.R16G16B16A16_UNORM:
          case DataFormat.R16G16B16A16_UINT:
          case DataFormat.R16G16B16A16_SNORM:
          case DataFormat.R16G16B16A16_SINT:
          case DataFormat.Y416:
            if (inSize >= 8 && outSize >= 8)
            {
              ushort alpha;
              if (format == DataFormat.R16G16B16A16_FLOAT)
                alpha = 0x3c00;
              else if (format == DataFormat.R16G16B16A16_SNORM || format == DataFormat.R16G16B16A16_SINT)
                alpha = 0x7fff;
              else
                alpha = 0xffff;

              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 7); count += 8)
              {
                writer.Write(reader.ReadUInt16());
                writer.Write(reader.ReadUInt16());
                writer.Write(reader.ReadUInt16());
                reader.ReadUInt16(); // Ignore
                writer.Write(alpha);
              }
            }
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.R10G10B10A2_TYPELESS:
          case DataFormat.R10G10B10A2_UNORM:
          case DataFormat.R10G10B10A2_UINT:
          case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
          case DataFormat.Y410:
          case DataFormat.R10G10B10_7E3_A2_FLOAT:
          case DataFormat.R10G10B10_6E4_A2_FLOAT:
          case DataFormat.R10G10B10_SNORM_A2_UNORM:
            if (inSize >= 4 && outSize >= 4)
            {
              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 3); count += 4)
              {
                writer.Write(reader.ReadUInt32() | 0xC0000000);
              }
            }
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.R8G8B8A8_TYPELESS:
          case DataFormat.R8G8B8A8_UNORM:
          case DataFormat.R8G8B8A8_UNORM_SRGB:
          case DataFormat.R8G8B8A8_UINT:
          case DataFormat.R8G8B8A8_SNORM:
          case DataFormat.R8G8B8A8_SINT:
          case DataFormat.B8G8R8A8_UNORM:
          case DataFormat.B8G8R8A8_TYPELESS:
          case DataFormat.B8G8R8A8_UNORM_SRGB:
          case DataFormat.AYUV:
            if (inSize >= 4 && outSize >= 4)
            {
              uint alpha = (format == DataFormat.R8G8B8A8_SNORM || format == DataFormat.R8G8B8A8_SINT) ? 0x7f000000 : 0xff000000;

              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 3); count += 4)
              {
                uint t = reader.ReadUInt32() & 0xFFFFFF;
                t |= alpha;
                writer.Write(t);
              }
            }
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.B5G5R5A1_UNORM:
            if (inSize >= 2 && outSize >= 2)
            {
              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 1); count += 2)
              {
                writer.Write((ushort)(reader.ReadUInt16() | 0x8000));
              }
            }
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.A8_UNORM:
            for (int i = 0; i < outSize; i++)
              writer.Write((byte)0xff);
            return;

          //-----------------------------------------------------------------------------
          case DataFormat.B4G4R4A4_UNORM:
            if (inSize >= 2 && outSize >= 2)
            {
              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 1); count += 2)
              {
                writer.Write((ushort)(reader.ReadUInt16() | 0xF000));
              }
            }
            return;
        }
      }

      // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
      byte[] scanline = reader.ReadBytes(Math.Min(inSize, outSize));
      writer.Write(scanline);
    }


    /// <summary>
    /// Swizzles (RGB &lt;-&gt; BGR) an image row with optional clearing of alpha value to 1.0.
    /// (Can be used in place as well; otherwise copies the image row unmodified.)
    /// </summary>
    internal static void SwizzleScanline(BinaryReader reader, int inSize, BinaryWriter writer, int outSize, DataFormat format, ScanlineFlags flags)
    {
      Debug.Assert(reader != null && inSize > 0);
      Debug.Assert(writer != null && outSize > 0);
      Debug.Assert(IsValidDds(format) && !IsPlanar(format) && !IsPalettized(format));

      switch (format)
      {
        //---------------------------------------------------------------------------------
        case DataFormat.R10G10B10A2_TYPELESS:
        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R10G10B10_XR_BIAS_A2_UNORM:
        case DataFormat.R10G10B10_SNORM_A2_UNORM:
          if (inSize >= 4 && outSize >= 4)
          {
            if ((flags & ScanlineFlags.Legacy) != 0)
            {
              // Swap Red (R) and Blue (B) channel (used for D3DFMT_A2R10G10B10 legacy sources)
              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 3); count += 4)
              {
                uint t = reader.ReadUInt32();

                uint t1 = (t & 0x3ff00000) >> 20;
                uint t2 = (t & 0x000003ff) << 20;
                uint t3 = (t & 0x000ffc00);
                uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xC0000000 : (t & 0xC0000000);

                writer.Write((uint)(t1 | t2 | t3 | ta));
              }
              return;
            }
          }
          break;

        //---------------------------------------------------------------------------------
        case DataFormat.R8G8B8A8_TYPELESS:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8A8_TYPELESS:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
        case DataFormat.B8G8R8X8_TYPELESS:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
          if (inSize >= 4 && outSize >= 4)
          {
            // Swap Red (R) and Blue (B) channels (used to convert from DXGI 1.1 BGR formats to DXGI 1.0 RGB)
            int size = Math.Min(outSize, inSize);
            for (int count = 0; count < (size - 3); count += 4)
            {
              uint t = reader.ReadUInt32();

              uint t1 = (t & 0x00ff0000) >> 16;
              uint t2 = (t & 0x000000ff) << 16;
              uint t3 = (t & 0x0000ff00);
              uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (t & 0xFF000000);

              writer.Write((uint)(t1 | t2 | t3 | ta));
            }
            return;
          }
          break;

        //---------------------------------------------------------------------------------
        case DataFormat.YUY2:
          if (inSize >= 4 && outSize >= 4)
          {
            if ((flags & ScanlineFlags.Legacy) != 0)
            {
              // Reorder YUV components (used to convert legacy UYVY -> YUY2)
              int size = Math.Min(outSize, inSize);
              for (int count = 0; count < (size - 3); count += 4)
              {
                uint t = reader.ReadUInt32();

                uint t1 = (t & 0x000000ff) << 8;
                uint t2 = (t & 0x0000ff00) >> 8;
                uint t3 = (t & 0x00ff0000) << 8;
                uint t4 = (t & 0xff000000) >> 8;

                writer.Write((uint)(t1 | t2 | t3 | t4));
              }
              return;
            }
          }
          break;
      }

      // Fall-through case is to just use memcpy (assuming this is not an in-place operation)
      byte[] scanline = reader.ReadBytes(Math.Min(inSize, outSize));
      writer.Write(scanline);
    }


    /// <summary>
    /// Converts an image row with optional clearing of alpha value to 1.0.
    /// </summary>
    internal static bool ExpandScanline(BinaryReader reader, int inSize, DataFormat inFormat, BinaryWriter writer, int outSize, DataFormat outFormat, ScanlineFlags flags)
    {
      Debug.Assert(reader != null && inSize > 0);
      Debug.Assert(writer != null && outSize > 0);
      Debug.Assert(IsValidDds(inFormat) && !IsPlanar(inFormat) && !IsPalettized(inFormat));
      Debug.Assert(IsValidDds(outFormat) && !IsPlanar(outFormat) && !IsPalettized(outFormat));

      bool setAlpha = (flags & ScanlineFlags.SetAlpha) != 0;
      switch (inFormat)
      {
        case DataFormat.B5G6R5_UNORM:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // DXGI_FORMAT_B5G6R5_UNORM -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              ushort t = reader.ReadUInt16();
              writer.Write(DataFormatHelper.Bgr565ToRgba8888(t));
            }
            return true;
          }
          return false;

        case DataFormat.B5G5R5A1_UNORM:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // DXGI_FORMAT_B5G5R5A1_UNORM -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              ushort color = reader.ReadUInt16();
              writer.Write(DataFormatHelper.Bgra5551ToRgba8888(color, setAlpha));
            }
            return true;
          }
          return false;

        case DataFormat.B4G4R4A4_UNORM:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // DXGI_FORMAT_B4G4R4A4_UNORM -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              ushort color = reader.ReadUInt16();
              writer.Write(DataFormatHelper.Bgra4444ToRgba8888(color, setAlpha));
            }
            return true;
          }
          return false;
      }

      return false;
    }
    #endregion
  }
}
