#region ----- Copyright -----
/*
  This is a port of the DDS loader in DirectXTex (see http://directxtex.codeplex.com/) which is
  licensed under the MIT license.

 
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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace DigitalRune.Graphics.Content
{
  //--------------------------------------------------------------
  #region DirectXTex.h
  //--------------------------------------------------------------

  /// <summary>
  /// Additional options for <see cref="DdsHelper"/>.
  /// </summary>
  [Flags]
  internal enum DdsFlags
  {
    /// <summary>None.</summary>
    None = 0x0,

    /// <summary>Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files).</summary>
    LegacyDword = 0x1,

    /// <summary>Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8).</summary>
    NoLegacyExpansion = 0x2,

    /// <summary>Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks.</summary>
    NoR10B10G10A2Fixup = 0x4,

    /// <summary>Convert DXGI 1.1 BGR formats to R8G8B8A8_UNORM to avoid use of optional WDDM 1.1 formats.</summary>
    ForceRgb = 0x8,

    /// <summary>Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats.</summary>
    No16Bpp = 0x10,

    /// <summary>When loading legacy luminance formats expand replicating the color channels rather than leaving them packed (L8, L16, A8L8).</summary>
    ExpandLuminance = 0x20,

    /// <summary>Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files).</summary>
    ForceDX10Ext = 0x10000,

    /// <summary>DDS_FLAGS_FORCE_DX10_EXT including miscFlags2 information (result may not be compatible with D3DX10 or D3DX11).</summary>
    ForceDX10ExtMisc2 = 0x20000,
  };
  #endregion


  /// <summary>
  /// Provides methods for loading/saving Direct Draw Surfaces (.DDS files).
  /// </summary>
  internal static class DdsHelper
  {
    //--------------------------------------------------------------
    #region Direct3D 11
    //--------------------------------------------------------------

    /// <summary>
    /// Identifies the type of resource being used (D3D11_RESOURCE_DIMENSION).
    /// </summary>
    private enum ResourceDimension   // = SharpDX.Direct3D11.ResourceDimension
    {
      Unknown,
      Buffer,
      Texture1D,
      Texture2D,
      Texture3D,
    }


    /// <summary>
    /// Identifies options for resources (D3D11_RESOURCE_MISC_FLAG).
    /// </summary>
    [Flags]
    private enum ResourceOptionFlags // = SharpDX.Direct3D11.ResourceOptionFlags
    {
      None = 0,
      GenerateMipMaps = 1,
      Shared = 2,
      TextureCube = 4,
      DrawIndirectArguments = 16,
      BufferAllowRawViews = 32,
      BufferStructured = 64,
      ResourceClamp = 128,
      SharedKeyedMutex = 256,
      GdiCompatible = 512,
    }
    #endregion


    //--------------------------------------------------------------
    #region DDS.h
    //--------------------------------------------------------------

    private const uint MagicHeader = 0x20534444; // "DDS "

    /// <summary>
    /// Describe a DDS pixel format.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct PixelFormat
    {
      public PixelFormat(PixelFormatFlags flags, int fourCC, int rgbBitCount, uint rBitMask, uint gBitMask, uint bBitMask, uint aBitMask)
      {
        Size = Marshal.SizeOf(typeof(PixelFormat));
        Flags = flags;
        FourCC = fourCC;
        RGBBitCount = rgbBitCount;
        RBitMask = rBitMask;
        GBitMask = gBitMask;
        BBitMask = bBitMask;
        ABitMask = aBitMask;
      }

      public int Size;
      public PixelFormatFlags Flags;
      public int FourCC;
      public int RGBBitCount;
      public uint RBitMask;
      public uint GBitMask;
      public uint BBitMask;
      public uint ABitMask;

      public static readonly PixelFormat DXT1 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '1'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat DXT2 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '2'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat DXT3 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '3'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat DXT4 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '4'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat DXT5 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', 'T', '5'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat BC4_UNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '4', 'U'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat BC4_SNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '4', 'S'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat BC5_UNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '5', 'U'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat BC5_SNorm = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('B', 'C', '5', 'S'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat R8G8_B8G8 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('R', 'G', 'B', 'G'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat G8R8_G8B8 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('G', 'R', 'G', 'B'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat YUY2 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('Y', 'U', 'Y', '2'), 0, 0, 0, 0, 0);
      public static readonly PixelFormat A8R8G8B8 = new PixelFormat(PixelFormatFlags.Rgba, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);
      public static readonly PixelFormat X8R8G8B8 = new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
      public static readonly PixelFormat A8B8G8R8 = new PixelFormat(PixelFormatFlags.Rgba, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);
      public static readonly PixelFormat X8B8G8R8 = new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000);
      public static readonly PixelFormat G16R16 = new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);
      public static readonly PixelFormat R5G6B5 = new PixelFormat(PixelFormatFlags.Rgb, 0, 16, 0x0000f800, 0x000007e0, 0x0000001f, 0x00000000);
      public static readonly PixelFormat A1R5G5B5 = new PixelFormat(PixelFormatFlags.Rgba, 0, 16, 0x00007c00, 0x000003e0, 0x0000001f, 0x00008000);
      public static readonly PixelFormat A4R4G4B4 = new PixelFormat(PixelFormatFlags.Rgba, 0, 16, 0x00000f00, 0x000000f0, 0x0000000f, 0x0000f000);
      public static readonly PixelFormat R8G8B8 = new PixelFormat(PixelFormatFlags.Rgb, 0, 24, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000);
      public static readonly PixelFormat L8 = new PixelFormat(PixelFormatFlags.Luminance, 0, 8, 0xff, 0x00, 0x00, 0x00);
      public static readonly PixelFormat L16 = new PixelFormat(PixelFormatFlags.Luminance, 0, 16, 0xffff, 0x0000, 0x0000, 0x0000);
      public static readonly PixelFormat A8L8 = new PixelFormat(PixelFormatFlags.LuminanceAlpha, 0, 16, 0x00ff, 0x0000, 0x0000, 0xff00);
      public static readonly PixelFormat A8 = new PixelFormat(PixelFormatFlags.Alpha, 0, 8, 0x00, 0x00, 0x00, 0xff);

      // Legacy bumpmap formats
      public static readonly PixelFormat V8U8 = new PixelFormat(PixelFormatFlags.BumpDuDv, 0, 16, 0x00ff, 0xff00, 0x0000, 0x0000);
      public static readonly PixelFormat Q8W8V8U8 = new PixelFormat(PixelFormatFlags.BumpDuDv, 0, 32, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000);
      public static readonly PixelFormat V16U16 = new PixelFormat(PixelFormatFlags.BumpDuDv, 0, 32, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000);

      // D3DFMT_A2R10G10B10/D3DFMT_A2B10G10R10 should be written using DX10 extension to avoid D3DX 10:10:10:2 reversal issue
      // This indicates the DDS_HEADER_DXT10 extension is present (the format is in dxgiFormat)
      public static readonly PixelFormat DX10 = new PixelFormat(PixelFormatFlags.FourCC, new FourCC('D', 'X', '1', '0'), 0, 0, 0, 0, 0);
    }


    /// <summary>
    /// The pixel format flags.
    /// </summary>
    [Flags]
    private enum PixelFormatFlags
    {
      FourCC = 0x00000004,          // DDPF_FOURCC
      Rgb = 0x00000040,             // DDPF_RGB
      Rgba = 0x00000041,            // DDPF_RGB | DDPF_ALPHAPIXELS
      Luminance = 0x00020000,       // DDPF_LUMINANCE
      LuminanceAlpha = 0x00020001,  // DDPF_LUMINANCE | DDPF_ALPHAPIXELS
      Alpha = 0x00000002,           // DDPF_ALPHA
      Pal8 = 0x00000020,            // DDPF_PALETTEINDEXED8
      BumpDuDv = 0x00080000         // DDPF_BUMPDUDV
    }


    /// <summary>
    /// The DDS header flags.
    /// </summary>
    [Flags]
    private enum HeaderFlags
    {
      Texture = 0x00001007,     // DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT 
      Mipmap = 0x00020000,      // DDSD_MIPMAPCOUNT
      Volume = 0x00800000,      // DDSD_DEPTH
      Pitch = 0x00000008,       // DDSD_PITCH
      LinearSize = 0x00080000,  // DDSD_LINEARSIZE
      Height = 0x00000002,      // DDSD_HEIGHT
      Width = 0x00000004,       // DDSD_WIDTH
    };


    /// <summary>
    /// The DDS surface flags.
    /// </summary>
    [Flags]
    private enum SurfaceFlags
    {
      Texture = 0x00001000, // DDSCAPS_TEXTURE
      Mipmap = 0x00400008,  // DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
      Cubemap = 0x00000008, // DDSCAPS_COMPLEX
    }


    /// <summary>
    /// The DDS cube map flags.
    /// </summary>
    [Flags]
    private enum CubemapFlags
    {
      CubeMap = 0x00000200,   // DDSCAPS2_CUBEMAP
      Volume = 0x00200000,    // DDSCAPS2_VOLUME
      PositiveX = 0x00000600, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
      NegativeX = 0x00000a00, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
      PositiveY = 0x00001200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
      NegativeY = 0x00002200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
      PositiveZ = 0x00004200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
      NegativeZ = 0x00008200, // DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ
      AllFaces = PositiveX | NegativeX | PositiveY | NegativeY | PositiveZ | NegativeZ,
    }


    [Flags]
    private enum MiscFlags
    {
      AlphaModeMask = 0x7,
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Header
    {
      public int Size;
      public HeaderFlags Flags;
      public int Height;
      public int Width;
      public int PitchOrLinearSize;
      public int Depth;                 // only if DDS_HEADER_FLAGS_VOLUME is set in dwFlags
      public int MipMapCount;
      private readonly uint unused1;    // dwReserved1[11]
      private readonly uint unused2;
      private readonly uint unused3;
      private readonly uint unused4;
      private readonly uint unused5;
      private readonly uint unused6;
      private readonly uint unused7;
      private readonly uint unused8;
      private readonly uint unused9;
      private readonly uint unused10;
      private readonly uint unused11;
      public PixelFormat PixelFormat;
      public SurfaceFlags SurfaceFlags; // dwCaps
      public CubemapFlags CubemapFlags; // dwCaps2
      private readonly uint Unused12;   // dwCaps3
      private readonly uint Unused13;   // dwCaps4
      private readonly uint Unused14;   // dwReserved2
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct HeaderDXT10
    {
      public DataFormat DXGIFormat;
      public ResourceDimension ResourceDimension;
      public ResourceOptionFlags MiscFlags;
      public int ArraySize;
      private readonly MiscFlags MiscFlags2;
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXTexDDS.cpp
    //--------------------------------------------------------------

    [Flags]
    private enum ConversionFlags
    {
      None = 0x0,
      Expand = 0x1,           // Conversion requires expanded pixel size
      NoAlpha = 0x2,          // Conversion requires setting alpha to known value
      Swizzle = 0x4,          // BGR/RGB order swizzling required
      Pal8 = 0x8,             // Has an 8-bit palette
      Format888 = 0x10,       // Source is an 8:8:8 (24bpp) format
      Format565 = 0x20,       // Source is a 5:6:5 (16bpp) format
      Format5551 = 0x40,      // Source is a 5:5:5:1 (16bpp) format
      Format4444 = 0x80,      // Source is a 4:4:4:4 (16bpp) format
      Format44 = 0x100,       // Source is a 4:4 (8bpp) format
      Format332 = 0x200,      // Source is a 3:3:2 (8bpp) format
      Format8332 = 0x400,     // Source is a 8:3:3:2 (16bpp) format
      FormatA8P8 = 0x800,     // Has an 8-bit palette with an alpha channel
      DX10 = 0x10000,         // Has the 'DX10' extension header

      // Premultiplied formats are considered deprecated.
      //PremultipliedAlpha = 0x20000,      // Contains premultiplied alpha data

      FormatL8 = 0x40000,     // Source is a 8 luminance format 
      FormatL16 = 0x80000,    // Source is a 16 luminance format 
      FormatA8L8 = 0x100000,  // Source is a 8:8 luminance format 
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct LegacyMap
    {
      /// <summary>
      /// Initializes a new instance of the <see cref="LegacyMap" /> struct.
      /// </summary>
      /// <param name="format">The format.</param>
      /// <param name="conversionFlags">The conversion flags.</param>
      /// <param name="pixelFormat">The pixel format.</param>
      public LegacyMap(DataFormat format, ConversionFlags conversionFlags, PixelFormat pixelFormat)
      {
        Format = format;
        ConversionFlags = conversionFlags;
        PixelFormat = pixelFormat;
      }

      public DataFormat Format;
      public ConversionFlags ConversionFlags;
      public PixelFormat PixelFormat;
    };


    private static readonly LegacyMap[] LegacyMaps = new[]
    {
      new LegacyMap(DataFormat.BC1_UNORM, ConversionFlags.None, PixelFormat.DXT1), // D3DFMT_DXT1
      new LegacyMap(DataFormat.BC2_UNORM, ConversionFlags.None, PixelFormat.DXT3), // D3DFMT_DXT3
      new LegacyMap(DataFormat.BC3_UNORM, ConversionFlags.None, PixelFormat.DXT5), // D3DFMT_DXT5

      new LegacyMap(DataFormat.BC2_UNORM, ConversionFlags.None /* ConversionFlags.PremultipliedAlpha */, PixelFormat.DXT2), // D3DFMT_DXT2
      new LegacyMap(DataFormat.BC3_UNORM, ConversionFlags.None /* ConversionFlags.PremultipliedAlpha */, PixelFormat.DXT4), // D3DFMT_DXT4

      new LegacyMap(DataFormat.BC4_UNORM, ConversionFlags.None, PixelFormat.BC4_UNorm),
      new LegacyMap(DataFormat.BC4_SNORM, ConversionFlags.None, PixelFormat.BC4_SNorm),
      new LegacyMap(DataFormat.BC5_UNORM, ConversionFlags.None, PixelFormat.BC5_UNorm),
      new LegacyMap(DataFormat.BC5_SNORM, ConversionFlags.None, PixelFormat.BC5_SNorm),

      new LegacyMap(DataFormat.BC4_UNORM, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
      new LegacyMap(DataFormat.BC5_UNORM, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),

      new LegacyMap(DataFormat.R8G8_B8G8_UNORM, ConversionFlags.None, PixelFormat.R8G8_B8G8), // D3DFMT_R8G8_B8G8
      new LegacyMap(DataFormat.G8R8_G8B8_UNORM, ConversionFlags.None, PixelFormat.G8R8_G8B8), // D3DFMT_G8R8_G8B8

      new LegacyMap(DataFormat.B8G8R8A8_UNORM, ConversionFlags.None, PixelFormat.A8R8G8B8), // D3DFMT_A8R8G8B8 (uses DXGI 1.1 format)
      new LegacyMap(DataFormat.B8G8R8X8_UNORM, ConversionFlags.None, PixelFormat.X8R8G8B8), // D3DFMT_X8R8G8B8 (uses DXGI 1.1 format)
      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.None, PixelFormat.A8B8G8R8), // D3DFMT_A8B8G8R8
      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.NoAlpha, PixelFormat.X8B8G8R8), // D3DFMT_X8B8G8R8
      new LegacyMap(DataFormat.R16G16_UNORM, ConversionFlags.None, PixelFormat.G16R16), // D3DFMT_G16R16

      new LegacyMap(DataFormat.R10G10B10A2_UNORM, ConversionFlags.Swizzle, new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)), // D3DFMT_A2R10G10B10 (D3DX reversal issue workaround)
      new LegacyMap(DataFormat.R10G10B10A2_UNORM, ConversionFlags.None, new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),    // D3DFMT_A2B10G10R10 (D3DX reversal issue workaround)

      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.Expand
                                               | ConversionFlags.NoAlpha
                                               | ConversionFlags.Format888, PixelFormat.R8G8B8), // D3DFMT_R8G8B8

      new LegacyMap(DataFormat.B5G6R5_UNORM, ConversionFlags.Format565, PixelFormat.R5G6B5), // D3DFMT_R5G6B5
      new LegacyMap(DataFormat.B5G5R5A1_UNORM, ConversionFlags.Format5551, PixelFormat.A1R5G5B5), // D3DFMT_A1R5G5B5
      new LegacyMap(DataFormat.B5G5R5A1_UNORM, ConversionFlags.Format5551
                                               | ConversionFlags.NoAlpha, new PixelFormat(PixelFormatFlags.Rgb, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)), // D3DFMT_X1R5G5B5
     
      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.Expand
                                               | ConversionFlags.Format8332, new PixelFormat(PixelFormatFlags.Rgb, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)), // D3DFMT_A8R3G3B2
      new LegacyMap(DataFormat.B5G6R5_UNORM, ConversionFlags.Expand
                                             | ConversionFlags.Format332, new PixelFormat(PixelFormatFlags.Rgb, 0, 8, 0xe0, 0x1c, 0x03, 0x00)), // D3DFMT_R3G3B2
  
      new LegacyMap(DataFormat.R8_UNORM, ConversionFlags.None, PixelFormat.L8), // D3DFMT_L8
      new LegacyMap(DataFormat.R16_UNORM, ConversionFlags.None, PixelFormat.L16), // D3DFMT_L16
      new LegacyMap(DataFormat.R8G8_UNORM, ConversionFlags.None, PixelFormat.A8L8), // D3DFMT_A8L8

      new LegacyMap(DataFormat.A8_UNORM, ConversionFlags.None, PixelFormat.A8), // D3DFMT_A8

      new LegacyMap(DataFormat.R16G16B16A16_UNORM, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16
      new LegacyMap(DataFormat.R16G16B16A16_SNORM, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)), // D3DFMT_Q16W16V16U16
      new LegacyMap(DataFormat.R16_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)), // D3DFMT_R16F
      new LegacyMap(DataFormat.R16G16_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)), // D3DFMT_G16R16F
      new LegacyMap(DataFormat.R16G16B16A16_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16F
      new LegacyMap(DataFormat.R32_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)), // D3DFMT_R32F
      new LegacyMap(DataFormat.R32G32_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)), // D3DFMT_G32R32F
      new LegacyMap(DataFormat.R32G32B32A32_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)), // D3DFMT_A32B32G32R32F

      new LegacyMap(DataFormat.R32_FLOAT, ConversionFlags.None, new PixelFormat(PixelFormatFlags.Rgb, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)), // D3DFMT_R32F (D3DX uses FourCC 114 instead)

      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.Expand
                                               | ConversionFlags.Pal8
                                               | ConversionFlags.FormatA8P8, new PixelFormat(PixelFormatFlags.Pal8, 0, 16, 0, 0, 0, 0)), // D3DFMT_A8P8
      new LegacyMap(DataFormat.R8G8B8A8_UNORM, ConversionFlags.Expand
                                               | ConversionFlags.Pal8, new PixelFormat(PixelFormatFlags.Pal8, 0, 8, 0, 0, 0, 0)), // D3DFMT_P8
//#if DIRECTX11_1
      new LegacyMap(DataFormat.B4G4R4A4_UNORM, ConversionFlags.Format4444, PixelFormat.A4R4G4B4 ), // D3DFMT_A4R4G4B4 (uses DXGI 1.2 format)
      new LegacyMap(DataFormat.B4G4R4A4_UNORM, ConversionFlags.NoAlpha
                                               | ConversionFlags.Format4444, new PixelFormat(PixelFormatFlags.Rgb, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)), // D3DFMT_X4R4G4B4 (uses DXGI 1.2 format)
      new LegacyMap(DataFormat.B4G4R4A4_UNORM, ConversionFlags.Expand
                                               | ConversionFlags.Format44, new PixelFormat(PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)), // D3DFMT_A4L4 (uses DXGI 1.2 format)
      new LegacyMap(DataFormat.YUY2, ConversionFlags.None, PixelFormat.YUY2),  // D3DFMT_YUY2 (uses DXGI 1.2 format)
      new LegacyMap(DataFormat.YUY2, ConversionFlags.Swizzle, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('U','Y','V','Y'), 0, 0, 0, 0, 0)), // D3DFMT_UYVY (uses DXGI 1.2 format)
//#else
//      // !DXGI_1_2_FORMATS
//      new LegacyMap(DataFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
//                                               | ConversionFlags.Format4444, PixelFormat.A4R4G4B4), // D3DFMT_A4R4G4B4
//      new LegacyMap(DataFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
//                                               | ConversionFlags.NoAlpha
//                                               | ConversionFlags.Format4444, new PixelFormat(PixelFormatFlags.Rgb, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)), // D3DFMT_X4R4G4B4
//      new LegacyMap(DataFormat.R8G8B8A8_UNorm, ConversionFlags.Expand
//                                               | ConversionFlags.Format44, new PixelFormat(PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)), // D3DFMT_A4L4
//      new LegacyMap(DataFormat.R8G8B8A8_UNorm, ConversionFlags.None, PixelFormat.YUY2),  // D3DFMT_YUY2
//      new LegacyMap(DataFormat.R8G8B8A8_UNorm, ConversionFlags.Swizzle, new PixelFormat(PixelFormatFlags.FourCC, new FourCC('U','Y','V','Y'), 0, 0, 0, 0, 0)), // D3DFMT_UYVY
//#endif

      new LegacyMap(DataFormat.R8G8_SNORM, ConversionFlags.None, PixelFormat.V8U8),           // D3DFMT_V8U8
      new LegacyMap(DataFormat.R8G8B8A8_SNORM, ConversionFlags.None, PixelFormat.Q8W8V8U8),   // D3DFMT_Q8W8V8U8
      new LegacyMap(DataFormat.R16G16_SNORM, ConversionFlags.None, PixelFormat.V16U16),       // D3DFMT_V16U16
    };


    // Note that many common DDS reader/writers (including D3DX) swap the
    // the RED/BLUE masks for 10:10:10:2 formats. We assume
    // below that the 'backwards' header mask is being used since it is most
    // likely written by D3DX. The more robust solution is to use the 'DX10'
    // header extension and specify the DXGI_FORMAT_R10G10B10A2_UNORM format directly

    // We do not support the following legacy Direct3D 9 formats:
    //      D3DFMT_A2W10V10U10
    //      BumpLuminance D3DFMT_L6V5U5, D3DFMT_X8L8V8U8
    //      FourCC 117 D3DFMT_CxV8U8
    //      ZBuffer D3DFMT_D16_LOCKABLE
    //      FourCC 82 D3DFMT_D32F_LOCKABLE

    private static DataFormat GetDXGIFormat(ref PixelFormat pixelFormat, DdsFlags flags, out ConversionFlags conversionFlags)
    {
      conversionFlags = ConversionFlags.None;

      int index;
      for (index = 0; index < LegacyMaps.Length; ++index)
      {
        var entry = LegacyMaps[index];

        if ((pixelFormat.Flags & entry.PixelFormat.Flags) != 0)
        {
          if ((entry.PixelFormat.Flags & PixelFormatFlags.FourCC) != 0)
          {
            if (pixelFormat.FourCC == entry.PixelFormat.FourCC)
              break;
          }
          else if ((entry.PixelFormat.Flags & PixelFormatFlags.Pal8) != 0)
          {
            if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
              break;
          }
          else if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
          {
            // RGB, RGBA, ALPHA, LUMINANCE
            if (pixelFormat.RBitMask == entry.PixelFormat.RBitMask
                && pixelFormat.GBitMask == entry.PixelFormat.GBitMask
                && pixelFormat.BBitMask == entry.PixelFormat.BBitMask
                && pixelFormat.ABitMask == entry.PixelFormat.ABitMask)
              break;
          }
        }
      }

      if (index >= LegacyMaps.Length)
        return DataFormat.Unknown;

      conversionFlags = LegacyMaps[index].ConversionFlags;
      var format = LegacyMaps[index].Format;

      if ((conversionFlags & ConversionFlags.Expand) != 0 && (flags & DdsFlags.NoLegacyExpansion) != 0)
        return DataFormat.Unknown;

      if (format == DataFormat.R10G10B10A2_UNORM && (flags & DdsFlags.NoR10B10G10A2Fixup) != 0)
      {
        conversionFlags ^= ConversionFlags.Swizzle;
      }

      return format;
    }


    /// <summary>
    /// Decodes DDS header including optional DX10 extended header
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static TextureDescription DecodeDDSHeader(BinaryReader reader, DdsFlags flags, out ConversionFlags convFlags)
    {
      Debug.Assert(Marshal.SizeOf(typeof(Header)) == 124, "DDS Header size mismatch");
      Debug.Assert(Marshal.SizeOf(typeof(HeaderDXT10)) == 20, "DDS DX10 Extended Header size mismatch");

      var description = new TextureDescription();
      convFlags = ConversionFlags.None;

      long size = reader.BaseStream.Length - reader.BaseStream.Position;
      int sizeOfTGAHeader = Marshal.SizeOf(typeof(Header));
      if (size < sizeOfTGAHeader)
        throw new InvalidDataException("The DDS file is corrupt.");

      // DDS files always start with the same magic number ("DDS ").
      if (reader.ReadUInt32() != MagicHeader)
        throw new InvalidDataException("The file does not appear to be a DDS file.");

      var header = reader.BaseStream.ReadStruct<Header>();

      // Verify header to validate DDS file
      if (header.Size != Marshal.SizeOf(typeof(Header))
          || header.PixelFormat.Size != Marshal.SizeOf(typeof(PixelFormat)))
        throw new InvalidDataException("Incorrect sizes in DDS file.");

      description.MipLevels = header.MipMapCount;
      if (description.MipLevels == 0)
        description.MipLevels = 1;

      // Check for DX10 extension
      if ((header.PixelFormat.Flags & PixelFormatFlags.FourCC) != 0
          && (new FourCC('D', 'X', '1', '0') == header.PixelFormat.FourCC))
      {
        // Buffer must be big enough for both headers and magic value
        if (reader.BaseStream.Length < (Marshal.SizeOf(typeof(Header)) + Marshal.SizeOf(typeof(HeaderDXT10)) + sizeof(uint)))
          throw new InvalidDataException("The DDS files is truncated.");

        var headerDX10 = reader.BaseStream.ReadStruct<HeaderDXT10>();
        convFlags |= ConversionFlags.DX10;

        description.ArraySize = headerDX10.ArraySize;
        if (description.ArraySize == 0)
          throw new InvalidDataException("Invalid array size specified in DDS file.");

        description.Format = headerDX10.DXGIFormat;
        if (!TextureHelper.IsValidDds(description.Format) || TextureHelper.IsPalettized(description.Format))
          throw new InvalidDataException("Invalid format specified in DDS file.");

        switch (headerDX10.ResourceDimension)
        {
          case ResourceDimension.Texture1D:

            // D3DX writes 1D textures with a fixed Height of 1
            if ((header.Flags & HeaderFlags.Height) != 0 && header.Height != 1)
              throw new InvalidDataException("Invalid height for 1D texture specified in DDS file.");

            description.Dimension = TextureDimension.Texture1D;
            description.Width = header.Width;
            description.Height = 1;
            description.Depth = 1;
            break;

          case ResourceDimension.Texture2D:
            if ((headerDX10.MiscFlags & ResourceOptionFlags.TextureCube) != 0)
            {
              description.Dimension = TextureDimension.TextureCube;
              description.ArraySize *= 6;
            }
            else
            {
              description.Dimension = TextureDimension.Texture2D;
            }

            description.Width = header.Width;
            description.Height = header.Height;
            description.Depth = 1;
            break;

          case ResourceDimension.Texture3D:
            if ((header.Flags & HeaderFlags.Volume) == 0)
              throw new InvalidDataException("Volume flag for 3D texture is missing in DDS file.");

            if (description.ArraySize > 1)
              throw new InvalidDataException("Invalid array size for 3D texture specified in DDS file.");

            description.Dimension = TextureDimension.Texture3D;
            description.Width = header.Width;
            description.Height = header.Height;
            description.Depth = header.Depth;
            break;

          default:
            throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Invalid texture dimension specified in DDS file."));
        }
      }
      else
      {
        description.ArraySize = 1;

        if ((header.Flags & HeaderFlags.Volume) != 0)
        {
          description.Dimension = TextureDimension.Texture3D;
          description.Width = header.Width;
          description.Height = header.Height;
          description.Depth = header.Depth;
        }
        else
        {
          if ((header.CubemapFlags & CubemapFlags.CubeMap) != 0)
          {
            // We require all six faces to be defined
            if ((header.CubemapFlags & CubemapFlags.AllFaces) != CubemapFlags.AllFaces)
              throw new InvalidDataException("Cube map faces missing in DDS file. All six faces required.");

            description.Dimension = TextureDimension.TextureCube;
            description.ArraySize = 6;
          }
          else
          {
            description.Dimension = TextureDimension.Texture2D;
          }

          description.Width = header.Width;
          description.Height = header.Height;
          description.Depth = 1;

          // Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
        }

        description.Format = GetDXGIFormat(ref header.PixelFormat, flags, out convFlags);

        if (description.Format == DataFormat.Unknown)
          throw new NotSupportedException("The texture format used in the DDS file is not supported.");

        // Premultiplied formats are considered deprecated.
        //if ((convFlags & ConversionFlags.PremultipliedAlpha) != 0)
        //  Description.AlphaMode = AlphaMode.Premultiplied;

        // Special flag for handling LUMINANCE legacy formats
        if ((flags & DdsFlags.ExpandLuminance) != 0)
        {
          switch (description.Format)
          {
            case DataFormat.R8_UNORM:
              description.Format = DataFormat.R8G8B8A8_UNORM;
              convFlags |= ConversionFlags.FormatL8 | ConversionFlags.Expand;
              break;

            case DataFormat.R8G8_UNORM:
              description.Format = DataFormat.R8G8B8A8_UNORM;
              convFlags |= ConversionFlags.FormatA8L8 | ConversionFlags.Expand;
              break;

            case DataFormat.R16_UNORM:
              description.Format = DataFormat.R16G16B16A16_UNORM;
              convFlags |= ConversionFlags.FormatL16 | ConversionFlags.Expand;
              break;
          }
        }
      }

      // Special flag for handling BGR DXGI 1.1 formats
      if ((flags & DdsFlags.ForceRgb) != 0)
      {
        switch (description.Format)
        {
          case DataFormat.B8G8R8A8_UNORM:
            description.Format = DataFormat.R8G8B8A8_UNORM;
            convFlags |= ConversionFlags.Swizzle;
            break;

          case DataFormat.B8G8R8X8_UNORM:
            description.Format = DataFormat.R8G8B8A8_UNORM;
            convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
            break;

          case DataFormat.B8G8R8A8_TYPELESS:
            description.Format = DataFormat.R8G8B8A8_TYPELESS;
            convFlags |= ConversionFlags.Swizzle;
            break;

          case DataFormat.B8G8R8A8_UNORM_SRGB:
            description.Format = DataFormat.R8G8B8A8_UNORM_SRGB;
            convFlags |= ConversionFlags.Swizzle;
            break;

          case DataFormat.B8G8R8X8_TYPELESS:
            description.Format = DataFormat.R8G8B8A8_TYPELESS;
            convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
            break;

          case DataFormat.B8G8R8X8_UNORM_SRGB:
            description.Format = DataFormat.R8G8B8A8_UNORM_SRGB;
            convFlags |= ConversionFlags.Swizzle | ConversionFlags.NoAlpha;
            break;
        }
      }

      // Special flag for handling 16bpp formats
      if ((flags & DdsFlags.No16Bpp) != 0)
      {
        switch (description.Format)
        {
          case DataFormat.B5G6R5_UNORM:
          case DataFormat.B5G5R5A1_UNORM:
          case DataFormat.B4G4R4A4_UNORM:
            description.Format = DataFormat.R8G8B8A8_UNORM;
            convFlags |= ConversionFlags.Expand;
            if (description.Format == DataFormat.B5G6R5_UNORM)
              convFlags |= ConversionFlags.NoAlpha;
            break;
        }
      }
      return description;
    }


    /// <summary>
    /// Encodes DDS file header (magic value, header, optional DX10 extended header).
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void EncodeDDSHeader(BinaryWriter writer, TextureDescription description, DdsFlags flags)
    {
      if (!TextureHelper.IsValidDds(description.Format))
        throw new ArgumentException("Invalid texture format.", "description");

      if (TextureHelper.IsPalettized(description.Format))
        throw new NotSupportedException("Palettized texture formats are not supported.");

      if (description.ArraySize > 1)
      {
        if (description.ArraySize != 6 || description.Dimension != TextureDimension.Texture2D || description.Dimension != TextureDimension.TextureCube)
        {
          // Texture1D arrays, Texture2D arrays, and TextureCube arrays must be stored using 'DX10' extended header
          flags |= DdsFlags.ForceDX10Ext;
        }
      }

      if ((flags & DdsFlags.ForceDX10ExtMisc2) != 0)
        flags |= DdsFlags.ForceDX10Ext;

      PixelFormat ddpf = default(PixelFormat);
      if ((flags & DdsFlags.ForceDX10Ext) == 0)
      {
        switch (description.Format)
        {
          case DataFormat.R8G8B8A8_UNORM:
            ddpf = PixelFormat.A8B8G8R8;
            break;
          case DataFormat.R16G16_UNORM:
            ddpf = PixelFormat.G16R16;
            break;
          case DataFormat.R8G8_UNORM:
            ddpf = PixelFormat.A8L8;
            break;
          case DataFormat.R16_UNORM:
            ddpf = PixelFormat.L16;
            break;
          case DataFormat.R8_UNORM:
            ddpf = PixelFormat.L8;
            break;
          case DataFormat.A8_UNORM:
            ddpf = PixelFormat.A8;
            break;
          case DataFormat.R8G8_B8G8_UNORM:
            ddpf = PixelFormat.R8G8_B8G8;
            break;
          case DataFormat.G8R8_G8B8_UNORM:
            ddpf = PixelFormat.G8R8_G8B8;
            break;
          case DataFormat.BC1_UNORM:
            ddpf = PixelFormat.DXT1;
            break;
          case DataFormat.BC2_UNORM:
            ddpf = /* (description.AlphaMode == AlphaMode.Premultiplied) ? PixelFormat.DXT2 : */ PixelFormat.DXT3;
            break;
          case DataFormat.BC3_UNORM:
            ddpf = /* (description.AlphaMode == AlphaMode.Premultiplied) ? PixelFormat.DXT2 : */ PixelFormat.DXT5;
            break;
          case DataFormat.BC4_UNORM:
            ddpf = PixelFormat.BC4_UNorm;
            break;
          case DataFormat.BC4_SNORM:
            ddpf = PixelFormat.BC4_SNorm;
            break;
          case DataFormat.BC5_UNORM:
            ddpf = PixelFormat.BC5_UNorm;
            break;
          case DataFormat.BC5_SNORM:
            ddpf = PixelFormat.BC5_SNorm;
            break;
          case DataFormat.B5G6R5_UNORM:
            ddpf = PixelFormat.R5G6B5;
            break;
          case DataFormat.B5G5R5A1_UNORM:
            ddpf = PixelFormat.A1R5G5B5;
            break;
          case DataFormat.R8G8_SNORM:
            ddpf = PixelFormat.V8U8;
            break;
          case DataFormat.R8G8B8A8_SNORM:
            ddpf = PixelFormat.Q8W8V8U8;
            break;
          case DataFormat.R16G16_SNORM:
            ddpf = PixelFormat.V16U16;
            break;
          case DataFormat.B8G8R8A8_UNORM:
            ddpf = PixelFormat.A8R8G8B8;
            break; // DXGI 1.1
          case DataFormat.B8G8R8X8_UNORM:
            ddpf = PixelFormat.X8R8G8B8;
            break; // DXGI 1.1
          //#if DIRECTX11_1
          case DataFormat.B4G4R4A4_UNORM:
            ddpf = PixelFormat.A4R4G4B4;
            break;
          case DataFormat.YUY2:
            ddpf = PixelFormat.YUY2;
            break;
          //#endif
          // Legacy D3DX formats using D3DFMT enum value as FourCC
          case DataFormat.R32G32B32A32_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 116; // D3DFMT_A32B32G32R32F
            break;
          case DataFormat.R16G16B16A16_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 113; // D3DFMT_A16B16G16R16F
            break;
          case DataFormat.R16G16B16A16_UNORM:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 36; // D3DFMT_A16B16G16R16
            break;
          case DataFormat.R16G16B16A16_SNORM:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 110; // D3DFMT_Q16W16V16U16
            break;
          case DataFormat.R32G32_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 115; // D3DFMT_G32R32F
            break;
          case DataFormat.R16G16_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 112; // D3DFMT_G16R16F
            break;
          case DataFormat.R32_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 114; // D3DFMT_R32F
            break;
          case DataFormat.R16_FLOAT:
            ddpf.Size = Marshal.SizeOf(typeof(PixelFormat));
            ddpf.Flags = PixelFormatFlags.FourCC;
            ddpf.FourCC = 111; // D3DFMT_R16F
            break;
        }
      }

      writer.Write(MagicHeader);

      var header = new Header();
      header.Size = Marshal.SizeOf(typeof(Header));
      header.Flags = HeaderFlags.Texture;
      header.SurfaceFlags = SurfaceFlags.Texture;

      if (description.MipLevels > 0)
      {
        header.Flags |= HeaderFlags.Mipmap;
        header.MipMapCount = description.MipLevels;

        if (header.MipMapCount > 1)
          header.SurfaceFlags |= SurfaceFlags.Mipmap;
      }

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
          header.Height = description.Height;
          header.Width = header.Depth = 1;
          break;

        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          header.Height = description.Height;
          header.Width = description.Width;
          header.Depth = 1;

          if (description.Dimension == TextureDimension.TextureCube)
          {
            header.SurfaceFlags |= SurfaceFlags.Cubemap;
            header.CubemapFlags |= CubemapFlags.AllFaces;
          }
          break;

        case TextureDimension.Texture3D:
          header.Flags |= HeaderFlags.Volume;
          header.CubemapFlags |= CubemapFlags.Volume;
          header.Height = description.Height;
          header.Width = description.Width;
          header.Depth = description.Depth;
          break;

        default:
          throw new NotSupportedException("The specified texture dimension is not supported.");
      }

      int rowPitch, slicePitch;
      TextureHelper.ComputePitch(description.Format, description.Width, description.Height, out rowPitch, out slicePitch);

      if (TextureHelper.IsBCn(description.Format))
      {
        header.Flags |= HeaderFlags.LinearSize;
        header.PitchOrLinearSize = slicePitch;
      }
      else
      {
        header.Flags |= HeaderFlags.Pitch;
        header.PitchOrLinearSize = rowPitch;
      }

      if (ddpf.Size != 0)
      {
        header.PixelFormat = ddpf;
        writer.BaseStream.WriteStruct(header);
      }
      else
      {
        header.PixelFormat = PixelFormat.DX10;

        var ext = new HeaderDXT10();
        ext.DXGIFormat = description.Format;
        switch (description.Dimension)
        {
          case TextureDimension.Texture1D:
            ext.ResourceDimension = ResourceDimension.Texture1D;
            break;
          case TextureDimension.Texture2D:
          case TextureDimension.TextureCube:
            ext.ResourceDimension = ResourceDimension.Texture2D;
            break;
          case TextureDimension.Texture3D:
            ext.ResourceDimension = ResourceDimension.Texture3D;
            break;
        }

        if (description.Dimension == TextureDimension.TextureCube)
        {
          ext.MiscFlags |= ResourceOptionFlags.TextureCube;
          ext.ArraySize = description.ArraySize / 6;
        }
        else
        {
          ext.ArraySize = description.ArraySize;
        }

        if ((flags & DdsFlags.ForceDX10ExtMisc2) != 0)
        {
          // This was formerly 'reserved'. D3DX10 and D3DX11 will fail if this value is anything other than 0.
          //ext.MiscFlags2 = description.MiscFlags2;
          throw new NotImplementedException("DdsFlags.ForceDX10ExtMisc2 is not implemented.");
        }

        writer.BaseStream.WriteStruct(header);
        writer.BaseStream.WriteStruct(ext);
      }
    }


    private enum LegacyFormat
    {
      Unknown = 0,
      R8G8B8,
      R3G3B2,
      A8R3G3B2,
      P8,
      A8P8,
      A4L4,
      B4G4R4A4,
      L8,
      L16,
      A8L8
    };


    private static LegacyFormat FindLegacyFormat(ConversionFlags flags)
    {
      var lformat = LegacyFormat.Unknown;

      if ((flags & ConversionFlags.Pal8) != 0)
        lformat = (flags & ConversionFlags.FormatA8P8) != 0 ? LegacyFormat.A8P8 : LegacyFormat.P8;
      else if ((flags & ConversionFlags.Format888) != 0)
        lformat = LegacyFormat.R8G8B8;
      else if ((flags & ConversionFlags.Format332) != 0)
        lformat = LegacyFormat.R3G3B2;
      else if ((flags & ConversionFlags.Format8332) != 0)
        lformat = LegacyFormat.A8R3G3B2;
      else if ((flags & ConversionFlags.Format44) != 0)
        lformat = LegacyFormat.A4L4;
      else if ((flags & ConversionFlags.Format4444) != 0)
        lformat = LegacyFormat.B4G4R4A4;
      else if ((flags & ConversionFlags.FormatL8) != 0)
        lformat = LegacyFormat.L8;
      else if ((flags & ConversionFlags.FormatL16) != 0)
        lformat = LegacyFormat.L16;
      else if ((flags & ConversionFlags.FormatA8L8) != 0)
        lformat = LegacyFormat.A8L8;

      return lformat;
    }


    /// <summary>
    /// Converts an image row with optional clearing of alpha value to 1.0.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if supported; otherwise, <see langword="false"/> if expansion case is not supported.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    private static bool LegacyExpandScanline(BinaryReader reader, int inSize, LegacyFormat inFormat, BinaryWriter writer, int outSize, DataFormat outFormat, uint[] pal8, ScanlineFlags flags)
    {
      Debug.Assert(reader != null && inSize > 0);
      Debug.Assert(writer != null && outSize > 0);
      Debug.Assert(TextureHelper.IsValidDds(outFormat) && !TextureHelper.IsPlanar(outFormat) && !TextureHelper.IsPalettized(outFormat));

      switch (inFormat)
      {
        case LegacyFormat.R8G8B8:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // D3DFMT_R8G8B8 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 3 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 2)) && (ocount < (outSize - 3))); icount += 3, ocount += 4)
            {
              // 24bpp Direct3D 9 files are actually BGR, so need to swizzle as well
              uint t1 = (uint)(reader.ReadByte() << 16);
              uint t2 = (uint)(reader.ReadByte() << 8);
              uint t3 = reader.ReadByte();

              writer.Write((int)(t1 | t2 | t3 | 0xff000000));
            }
            return true;
          }
          return false;

        case LegacyFormat.R3G3B2:
          switch (outFormat)
          {
            case DataFormat.R8G8B8A8_UNORM:
              // D3DFMT_R3G3B2 -> DXGI_FORMAT_R8G8B8A8_UNORM
              if (inSize >= 1 && outSize >= 4)
              {
                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 3))); ++icount, ocount += 4)
                {
                  byte t = reader.ReadByte();

                  uint t1 = (uint)((t & 0xe0) | ((t & 0xe0) >> 3) | ((t & 0xc0) >> 6));
                  uint t2 = (uint)(((t & 0x1c) << 11) | ((t & 0x1c) << 8) | ((t & 0x18) << 5));
                  uint t3 = (uint)(((t & 0x03) << 22) | ((t & 0x03) << 20) | ((t & 0x03) << 18) | ((t & 0x03) << 16));

                  writer.Write((int)(t1 | t2 | t3 | 0xff000000));
                }
                return true;
              }
              return false;

            case DataFormat.B5G6R5_UNORM:
              // D3DFMT_R3G3B2 -> DXGI_FORMAT_B5G6R5_UNORM
              if (inSize >= 1 && outSize >= 2)
              {
                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 1))); ++icount, ocount += 2)
                {
                  byte t = reader.ReadByte();

                  int t1 = ((t & 0xe0) << 8) | ((t & 0xc0) << 5);
                  int t2 = ((t & 0x1c) << 6) | ((t & 0x1c) << 3);
                  int t3 = ((t & 0x03) << 3) | ((t & 0x03) << 1) | ((t & 0x02) >> 1);

                  writer.Write((short)(t1 | t2 | t3));
                }
                return true;
              }
              return false;
          }
          break;

        case LegacyFormat.A8R3G3B2:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // D3DFMT_A8R3G3B2 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              short t = reader.ReadInt16();

              uint t1 = (uint)((t & 0x00e0) | ((t & 0x00e0) >> 3) | ((t & 0x00c0) >> 6));
              uint t2 = (uint)(((t & 0x001c) << 11) | ((t & 0x001c) << 8) | ((t & 0x0018) << 5));
              uint t3 = (uint)(((t & 0x0003) << 22) | ((t & 0x0003) << 20) | ((t & 0x0003) << 18) | ((t & 0x0003) << 16));
              uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)((t & 0xff00) << 16));

              writer.Write((uint)(t1 | t2 | t3 | ta));
            }
            return true;
          }
          return false;

        case LegacyFormat.P8:
          if ((outFormat != DataFormat.R8G8B8A8_UNORM) || pal8 == null)
            return false;

          // D3DFMT_P8 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 1 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 3))); ++icount, ocount += 4)
            {
              byte t = reader.ReadByte();
              writer.Write(pal8[t]);
            }
            return true;
          }
          return false;

        case LegacyFormat.A8P8:
          if ((outFormat != DataFormat.R8G8B8A8_UNORM) || pal8 == null)
            return false;

          // D3DFMT_A8P8 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              short t = reader.ReadInt16();

              uint t1 = pal8[t & 0xff];
              uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)((t & 0xff00) << 16));

              writer.Write((int)(t1 | ta));
            }
            return true;
          }
          return false;

        case LegacyFormat.A4L4:
          switch (outFormat)
          {
            //#if DIRECTX11_1
            case DataFormat.B4G4R4A4_UNORM:
              // D3DFMT_A4L4 -> DXGI_FORMAT_B4G4R4A4_UNORM 
              if (inSize >= 1 && outSize >= 2)
              {
                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 1))); ++icount, ocount += 2)
                {
                  byte t = reader.ReadByte();

                  ushort t1 = (ushort)(t & 0x0f);
                  ushort ta = (flags & ScanlineFlags.SetAlpha) != 0 ? (ushort)0xf000 : (ushort)((t & 0xf0) << 8);

                  writer.Write((ushort)(t1 | (t1 << 4) | (t1 << 8) | ta));
                }
                return true;
              }
              return false;
            //#endif

            case DataFormat.R8G8B8A8_UNORM:
              // D3DFMT_A4L4 -> DXGI_FORMAT_R8G8B8A8_UNORM
              if (inSize >= 1 && outSize >= 4)
              {
                for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 3))); ++icount, ocount += 4)
                {
                  byte t = reader.ReadByte();

                  uint t1 = (uint)(((t & 0x0f) << 4) | (t & 0x0f));
                  uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)(((t & 0xf0) << 24) | ((t & 0xf0) << 20)));

                  writer.Write((uint)(t1 | (t1 << 8) | (t1 << 16) | ta));
                }
                return true;
              }
              return false;
          }
          break;

        //#if !DIRECTX11_1
        case LegacyFormat.B4G4R4A4:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // D3DFMT_A4R4G4B4 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              ushort t = reader.ReadUInt16();

              uint t1 = (uint)(((t & 0x0f00) >> 4) | ((t & 0x0f00) >> 8));
              uint t2 = (uint)(((t & 0x00f0) << 8) | ((t & 0x00f0) << 4));
              uint t3 = (uint)(((t & 0x000f) << 20) | ((t & 0x000f) << 16));
              uint ta = ((flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)(((t & 0xf000) << 16) | ((t & 0xf000) << 12)));

              writer.Write((uint)(t1 | t2 | t3 | ta));
            }
            return true;
          }
          return false;
        //#endif

        case LegacyFormat.L8:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // D3DFMT_L8 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 1 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < inSize) && (ocount < (outSize - 3))); ++icount, ocount += 4)
            {
              uint t1 = reader.ReadByte();
              uint t2 = (t1 << 8);
              uint t3 = (t1 << 16);

              writer.Write((uint)(t1 | t2 | t3 | 0xff000000));
            }
            return true;
          }
          return false;

        case LegacyFormat.L16:
          if (outFormat != DataFormat.R16G16B16A16_UNORM)
            return false;

          // D3DFMT_L16 -> DXGI_FORMAT_R16G16B16A16_UNORM
          if (inSize >= 2 && outSize >= 8)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 7))); icount += 2, ocount += 8)
            {
              ushort t = reader.ReadUInt16();

              ulong t1 = t;
              ulong t2 = (t1 << 16);
              ulong t3 = (t1 << 32);

              writer.Write((ulong)(t1 | t2 | t3 | 0xffff000000000000));
            }
            return true;
          }
          return false;

        case LegacyFormat.A8L8:
          if (outFormat != DataFormat.R8G8B8A8_UNORM)
            return false;

          // D3DFMT_A8L8 -> DXGI_FORMAT_R8G8B8A8_UNORM
          if (inSize >= 2 && outSize >= 4)
          {
            for (int ocount = 0, icount = 0; ((icount < (inSize - 1)) && (ocount < (outSize - 3))); icount += 2, ocount += 4)
            {
              ushort t = reader.ReadUInt16();

              uint t1 = (uint)(t & 0xff);
              uint t2 = (t1 << 8);
              uint t3 = (t1 << 16);
              uint ta = (flags & ScanlineFlags.SetAlpha) != 0 ? 0xff000000 : (uint)((t & 0xff00) << 16);

              writer.Write((int)(t1 | t2 | t3 | ta));
            }
            return true;
          }
          return false;
      }

      return false;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    private static Texture CopyImage(BinaryReader reader, TextureDescription description, ComputePitchFlags cpFlags, ConversionFlags convFlags, uint[] pal8)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      if ((convFlags & ConversionFlags.Expand) != 0)
      {
        if ((convFlags & ConversionFlags.Format888) != 0)
          cpFlags |= ComputePitchFlags.Bpp24;
        else if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444 | ConversionFlags.Format8332 | ConversionFlags.FormatA8P8 | ConversionFlags.FormatL16 | ConversionFlags.FormatA8L8)) != 0)
          cpFlags |= ComputePitchFlags.Bpp16;
        else if ((convFlags & (ConversionFlags.Format44 | ConversionFlags.Format332 | ConversionFlags.Pal8 | ConversionFlags.FormatL8)) != 0)
          cpFlags |= ComputePitchFlags.Bpp8;
      }

      var texture = new Texture(description);
      description = texture.Description;  // MipLevel may have been set.

      ScanlineFlags tflags = (convFlags & ConversionFlags.NoAlpha) != 0 ? ScanlineFlags.SetAlpha : 0;
      if ((convFlags & ConversionFlags.Swizzle) != 0)
        tflags |= ScanlineFlags.Legacy;

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          {
            int index = 0;
            for (int item = 0; item < description.ArraySize; ++item)
            {
              int width = description.Width;
              int height = description.Height;

              for (int level = 0; level < description.MipLevels; ++level, ++index)
              {
                int sRowPitch, sSlicePitch;
                TextureHelper.ComputePitch(description.Format, width, height, out sRowPitch, out sSlicePitch, cpFlags);

                var image = texture.Images[index];
                if (TextureHelper.IsBCn(description.Format) || TextureHelper.IsPlanar(description.Format))
                {
                  reader.Read(image.Data, 0, image.Data.Length);
                }
                else
                {
                  using (var stream = new MemoryStream(image.Data))
                  using (var writer = new BinaryWriter(stream))
                  {
                    for (int h = 0; h < height; ++h)
                    {
                      if ((convFlags & ConversionFlags.Expand) != 0)
                      {
                        if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444)) != 0)
                        {
                          if (!TextureHelper.ExpandScanline(reader, sRowPitch, (convFlags & ConversionFlags.Format565) != 0 ? DataFormat.B5G6R5_UNORM : DataFormat.B5G5R5A1_UNORM, writer, image.RowPitch, DataFormat.R8G8B8A8_UNORM, tflags))
                            throw new InvalidDataException("Unable to expand format.");
                        }
                        else
                        {
                          LegacyFormat lformat = FindLegacyFormat(convFlags);
                          if (!LegacyExpandScanline(reader, sRowPitch, lformat, writer, image.RowPitch, description.Format, pal8, tflags))
                            throw new InvalidDataException("Unable to expand legacy format.");
                        }
                      }
                      else if ((convFlags & ConversionFlags.Swizzle) != 0)
                      {
                        TextureHelper.SwizzleScanline(reader, sRowPitch, writer, image.RowPitch, description.Format, tflags);
                      }
                      else
                      {
                        TextureHelper.CopyScanline(reader, sRowPitch, writer, image.RowPitch, description.Format, tflags);
                      }
                    }
                  }
                }

                if (width > 1)
                  width >>= 1;

                if (height > 1)
                  height >>= 1;
              }
            }
          }
          break;

        case TextureDimension.Texture3D:
          {
            int index = 0;

            int width = description.Width;
            int height = description.Height;
            int depth = description.Depth;

            for (int level = 0; level < description.MipLevels; ++level)
            {
              int sRowPitch, sSlicePitch;
              TextureHelper.ComputePitch(description.Format, width, height, out sRowPitch, out sSlicePitch, cpFlags);

              for (int slice = 0; slice < depth; ++slice, ++index)
              {
                // We use the same memory organization that Direct3D 11 needs for D3D11_SUBRESOURCE_DATA
                // with all slices of a given miplevel being continuous in memory
                var image = texture.Images[index];

                if (TextureHelper.IsBCn(description.Format))
                {
                  reader.Read(image.Data, 0, image.Data.Length);
                }
                else if (TextureHelper.IsPlanar(description.Format))
                {
                  // Direct3D does not support any planar formats for Texture3D
                  throw new NotSupportedException("Planar texture formats are not support for volume textures.");
                }
                else
                {
                  using (var stream = new MemoryStream(image.Data))
                  using (var writer = new BinaryWriter(stream))
                  {
                    for (int h = 0; h < height; ++h)
                    {
                      if ((convFlags & ConversionFlags.Expand) != 0)
                      {
                        if ((convFlags & (ConversionFlags.Format565 | ConversionFlags.Format5551 | ConversionFlags.Format4444)) != 0)
                        {
                          if (!TextureHelper.ExpandScanline(reader, sRowPitch, (convFlags & ConversionFlags.Format565) != 0 ? DataFormat.B5G6R5_UNORM : DataFormat.B5G5R5A1_UNORM, writer, image.RowPitch, DataFormat.R8G8B8A8_UNORM, tflags))
                            throw new InvalidDataException("Unable to expand format.");
                        }
                        else
                        {
                          LegacyFormat lformat = FindLegacyFormat(convFlags);
                          if (!LegacyExpandScanline(reader, sRowPitch, lformat, writer, image.RowPitch, description.Format, pal8, tflags))
                            throw new InvalidDataException("Unable to expand legacy format.");
                        }
                      }
                      else if ((convFlags & ConversionFlags.Swizzle) != 0)
                      {
                        TextureHelper.SwizzleScanline(reader, sRowPitch, writer, image.RowPitch, description.Format, tflags);
                      }
                      else
                      {
                        TextureHelper.CopyScanline(reader, sRowPitch, writer, image.RowPitch, description.Format, tflags);
                      }
                    }
                  }
                }
              }

              if (width > 1)
                width >>= 1;

              if (height > 1)
                height >>= 1;

              if (depth > 1)
                depth >>= 1;
            }
          }
          break;

        default:
          throw new NotSupportedException("The specified texture dimension is not supported.");
      }

      return texture;
    }


    /// <summary>
    /// Loads the specified DDS texture.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="flags">Additional options.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static Texture Load(Stream stream, DdsFlags flags)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      using (var reader = new BinaryReader(stream))
      {
        ConversionFlags conversionFlags;
        var description = DecodeDDSHeader(reader, flags, out conversionFlags);

        uint[] pal8 = null;
        if ((conversionFlags & ConversionFlags.Pal8) != 0)
        {
          pal8 = new uint[256];
          for (int i = 0; i < pal8.Length; i++)
            pal8[i] = reader.ReadUInt32();
        }

        var computePitchFlags = (flags & DdsFlags.LegacyDword) != 0 ? ComputePitchFlags.LegacyDword : ComputePitchFlags.None;
        return CopyImage(reader, description, computePitchFlags, conversionFlags, pal8);
      }
    }


    /// <summary>
    /// Saves the specified texture in DDS format.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="flags">Additional options.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> or <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static void Save(Texture texture, Stream stream, DdsFlags flags)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (stream == null)
        throw new ArgumentNullException("stream");

#if NET45
      using (var writer = new BinaryWriter(stream, Encoding.Default, true))
#else
      using (var writer = new BinaryWriter(stream, Encoding.Default))   // Warning: Closes the stream!
#endif
      {
        var description = texture.Description;
        EncodeDDSHeader(writer, description, flags);

        switch (description.Dimension)
        {
          case TextureDimension.Texture1D:
          case TextureDimension.Texture2D:
          case TextureDimension.TextureCube:
            {
              int index = 0;
              for (int item = 0; item < description.ArraySize; ++item)
              {
                for (int level = 0; level < description.MipLevels; ++level)
                {
                  var image = texture.Images[index];
                  stream.Write(image.Data, 0, image.Data.Length);
                  ++index;
                }
              }
            }
            break;

          case TextureDimension.Texture3D:
            {
              if (description.ArraySize != 1)
                throw new NotSupportedException("Arrays of volume textures are not supported.");

              int d = description.Depth;

              int index = 0;
              for (int level = 0; level < description.MipLevels; ++level)
              {
                for (int slice = 0; slice < d; ++slice)
                {
                  var image = texture.Images[index];
                  stream.Write(image.Data, 0, image.Data.Length);
                  ++index;
                }

                if (d > 1)
                  d >>= 1;
              }
            }
            break;

          default:
            throw new NotSupportedException("The specified texture dimension is not supported.");
        }
      }
    }
    #endregion
  }
}
