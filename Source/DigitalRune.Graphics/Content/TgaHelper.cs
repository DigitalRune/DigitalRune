#region ----- Copyright -----
/*
  The TgaHelper is a port of the TGA loader in DirectXTex (see http://directxtex.codeplex.com/)
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
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Provides methods for loading/saving Truevision TARGA images (.TGA files).
  /// </summary>
  internal static class TgaHelper
  {
    // The implementation here has the following limitations:
    //      * Does not support files that contain color maps (these are rare in practice)
    //      * Interleaved files are not supported (deprecated aspect of TGA format)
    //      * Only supports 8-bit grayscale; 16-, 24-, and 32-bit true color images
    //      * Always writes uncompressed files (i.e. can read RLE compression, but does not write it)

    private enum ImageType : byte
    {
      NoImage = 0,
      ColorMapped = 1,
      TrueColor = 2,
      BlackAndWhite = 3,
      ColorMappedRLE = 9,
      TrueColorRLE = 10,
      BlackAndWhiteRLE = 11,
    };


    [Flags]
    private enum DescriptorFlags : byte
    {
      InvertX = 0x10,
      InvertY = 0x20,
      Interleaved2Way = 0x40, // Deprecated
      Interleaved4Way = 0x80, // Deprecated
    };


    private const string TGA20_Signature = "TRUEVISION-XFILE.";


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Header
    {
      public byte IDLength;
      public byte ColorMapType;
      public ImageType ImageType;
      public ushort ColorMapFirst;
      public ushort ColorMapLength;
      public byte ColorMapSize;
      public ushort XOrigin;
      public ushort YOrigin;
      public ushort Width;
      public ushort Height;
      public byte BitsPerPixel;
      public DescriptorFlags Descriptor;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct Footer
    {
      public ushort ExtensionOffset;
      public ushort DeveloperOffset;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
      public string Signature;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    private struct Extension
    {
      public ushort Size;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
      public string AuthorName;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 324)]
      public string AuthorComment;
      public ushort StampMonth;
      public ushort StampDay;
      public ushort StampYear;
      public ushort StampHour;
      public ushort StampMinute;
      public ushort StampSecond;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
      public string JobName;
      public ushort JobHour;
      public ushort JobMinute;
      public ushort JobSecond;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 41)]
      public string SoftwareId;
      public ushort VersionNumber;
      public byte VersionLetter;
      public uint KeyColor;
      public ushort PixelNumerator;
      public ushort PixelDenominator;
      public ushort GammaNumerator;
      public ushort GammaDenominator;
      public uint ColorOffset;
      public uint StampOffset;
      public uint ScanOffset;
      public byte AttributesType;
    };


    [Flags]
    private enum ConversionFlags
    {
      None = 0x0,
      Expand = 0x1,         // Conversion requires expanded pixel size
      InvertX = 0x2,        // If set, scanlines are right-to-left
      InvertY = 0x4,        // If set, scanlines are top-to-bottom
      RLE = 0x8,            // Source data is RLE compressed

      Swizzle = 0x10000,    // Swizzle BGR<->RGB data
      Format888 = 0x20000,  // 24bpp format
    };


    /// <summary>
    /// Decodes the TGA file header.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="offset">The offset in the stream at which the data starts.</param>
    /// <param name="convFlags">The conversion flags.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Invalid data.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The specified format is not supported.
    /// </exception>
    private static TextureDescription DecodeTGAHeader(Stream stream, out int offset, out ConversionFlags convFlags)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      TextureDescription description = new TextureDescription
      {
        Dimension = TextureDimension.Texture2D,
        Format = DataFormat.R8G8B8A8_UNORM,
      };

      offset = 0;
      convFlags = ConversionFlags.None;

      long size = stream.Length - stream.Position;
      int sizeOfTGAHeader = Marshal.SizeOf(typeof(Header));
      if (size < sizeOfTGAHeader)
        throw new InvalidDataException("The TGA file is corrupt.");

      var header = stream.ReadStruct<Header>();
      if (header.ColorMapType != 0 || header.ColorMapLength != 0)
        throw new NotSupportedException("TGA files with color maps are not supported.");

      if ((header.Descriptor & (DescriptorFlags.Interleaved2Way | DescriptorFlags.Interleaved4Way)) != 0)
        throw new NotSupportedException("TGA files with interleaved images are not supported.");

      if (header.Width == 0 || header.Height == 0)
        throw new NotSupportedException("The TGA file is corrupt. Width and height are invalid.");

      switch (header.ImageType)
      {
        case ImageType.TrueColor:
        case ImageType.TrueColorRLE:
          switch (header.BitsPerPixel)
          {
            case 16:
              description.Format = DataFormat.B5G5R5A1_UNORM;
              break;
            case 24:
              description.Format = DataFormat.R8G8B8A8_UNORM;
              convFlags |= ConversionFlags.Expand;
              // We could use DXGI_FORMAT_B8G8R8X8_UNORM, but we prefer DXGI 1.0 formats
              break;
            case 32:
              description.Format = DataFormat.R8G8B8A8_UNORM;
              // We could use DXGI.Format.B8G8R8A8_UNORM, but we prefer DXGI 1.0 formats
              break;
          }

          if (header.ImageType == ImageType.TrueColorRLE)
            convFlags |= ConversionFlags.RLE;
          break;

        case ImageType.BlackAndWhite:
        case ImageType.BlackAndWhiteRLE:
          switch (header.BitsPerPixel)
          {
            case 8:
              description.Format = DataFormat.R8_UNORM;
              break;
            default:
              throw new NotSupportedException("The black-and-white format used by the TGA file is not supported. Only 8-bit black-and-white images are supported.");
          }

          if (header.ImageType == ImageType.BlackAndWhiteRLE)
            convFlags |= ConversionFlags.RLE;
          break;

        case ImageType.NoImage:
        case ImageType.ColorMapped:
        case ImageType.ColorMappedRLE:
          throw new NotSupportedException("The image format used by the TGA file is not supported.");
        default:
          throw new InvalidDataException("Unknown image format used by the TGA file.");
      }

      description.Width = header.Width;
      description.Height = header.Height;
      description.Depth = 1;
      description.MipLevels = 1;
      description.ArraySize = 1;

      if ((header.Descriptor & DescriptorFlags.InvertX) != 0)
        convFlags |= ConversionFlags.InvertX;

      if ((header.Descriptor & DescriptorFlags.InvertY) != 0)
        convFlags |= ConversionFlags.InvertY;

      offset = sizeOfTGAHeader;
      if (header.IDLength != 0)
        offset += header.IDLength;
      return description;
    }


    private static ushort ReadBgra5551(BinaryReader reader)
    {
      int low = reader.ReadByte();
      int high = reader.ReadByte();
      return (ushort)((high << 8) | low);
    }


    private static uint ReadColor(BinaryReader reader, bool expand)
    {
      // BGR(A) --> RGBA
      int b = reader.ReadByte();
      int g = reader.ReadByte();
      int r = reader.ReadByte();
      int a = expand ? 255 : reader.ReadByte();

      return (uint)((r & 0xff) | (g & 0xff) << 8 | (b & 0xff) << 16 | (a & 0xff) << 24);
    }


    private static byte GetAlpha(uint color)
    {
      return (byte)(color >> 24);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    private static void SetAlphaToOpaque(Image image)
    {
      using (var inputStream = new MemoryStream(image.Data, false))
      using (var reader = new BinaryReader(inputStream))
      using (var outputStream = new MemoryStream(image.Data, true))
      using (var writer = new BinaryWriter(outputStream))
      {
        for (int y = 0; y < image.Height; y++)
          TextureHelper.CopyScanline(reader, image.RowPitch, writer, image.RowPitch, image.Format, ScanlineFlags.SetAlpha);
      }
    }


    /// <summary>
    /// Uncompresses the pixel data from a TGA file into the target image.
    /// </summary>
    /// <param name="reader">The input reader.</param>
    /// <param name="image">The image.</param>
    /// <param name="format">The texture format.</param>
    /// <param name="convFlags">The conversion flags.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="reader"/> or <paramref name="image"/> is <see langword="null"/>.
    /// </exception>
    private static unsafe void UncompressPixels(BinaryReader reader, Image image, DataFormat format, ConversionFlags convFlags)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (image == null)
        throw new ArgumentNullException("image");

      bool invertX = (convFlags & ConversionFlags.InvertX) != 0;
      bool invertY = (convFlags & ConversionFlags.InvertY) != 0;
      bool expand = (convFlags & ConversionFlags.Expand) != 0;

      byte[] pixelData = image.Data;
      fixed (byte* imagePtr = pixelData)
      {
        switch (format)
        {
          case DataFormat.R8_UNORM: // 8-bit
            {
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                byte* ptr = imagePtr + image.RowPitch * yOffset + xOffset;

                for (int x = 0; x < image.Width; )
                {
                  byte b = reader.ReadByte();
                  int count = (b & 0x7F) + 1;
                  if ((b & 0x80) != 0)
                  {
                    // Repeat
                    b = reader.ReadByte();
                    for (int j = 0; j < count; j++)
                    {
                      *ptr = b;
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }
                  else
                  {
                    // Literal
                    for (int j = 0; j < count; j++)
                    {
                      *ptr = reader.ReadByte();
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }

                  x += count;
                }
              }
            }
            break;

          case DataFormat.B5G5R5A1_UNORM: // 16-bit
            {
              bool nonZeroAlpha = false;
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                ushort* ptr = (ushort*)(imagePtr + image.RowPitch * yOffset) + xOffset;

                for (int x = 0; x < image.Width; )
                {
                  byte b = reader.ReadByte();
                  int count = (b & 0x7F) + 1;
                  if ((b & 0x80) != 0)
                  {
                    // Repeat
                    ushort color = ReadBgra5551(reader);
                    if (GetAlpha(color) != 0)
                      nonZeroAlpha = true;

                    for (int j = count; j > 0; j--)
                    {
                      *ptr = color;
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }
                  else
                  {
                    // Literal
                    for (int j = count; j > 0; j--)
                    {
                      ushort color = ReadBgra5551(reader);
                      if (GetAlpha(color) != 0)
                        nonZeroAlpha = true;

                      *ptr = color;
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }

                  x += count;
                }
              }

              // If there are no non-zero alpha channel entries, we'll assume alpha is not used and force it to opaque.
              if (!nonZeroAlpha)
              {
                SetAlphaToOpaque(image);
              }
            }
            break;

          case DataFormat.R8G8B8A8_UNORM: // 24/32-bit
            {
              bool nonZeroAlpha = false;
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                uint* ptr = (uint*)(imagePtr + image.RowPitch * yOffset) + xOffset;

                for (int x = 0; x < image.Width; )
                {
                  byte b = reader.ReadByte();
                  int count = (b & 0x7F) + 1;
                  if ((b & 0x80) != 0)
                  {
                    // Repeat
                    uint color = ReadColor(reader, expand);
                    if (GetAlpha(color) != 0)
                      nonZeroAlpha = true;

                    for (int j = count; j > 0; j--)
                    {
                      *ptr = color;
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }
                  else
                  {
                    // Literal
                    for (int j = count; j > 0; j--)
                    {
                      uint color = ReadColor(reader, expand);
                      if (GetAlpha(color) != 0)
                        nonZeroAlpha = true;

                      *ptr = color;
                      if (invertX)
                        ptr--;
                      else
                        ptr++;
                    }
                  }

                  x += count;
                }
              }

              // If there are no non-zero alpha channel entries, we'll assume alpha is not used and force it to opaque.
              if (!nonZeroAlpha)
              {
                SetAlphaToOpaque(image);
              }
            }
            break;
        }
      }
    }


    private static unsafe void CopyPixels(BinaryReader reader, Image image, DataFormat format, ConversionFlags convFlags)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");
      if (image == null)
        throw new ArgumentNullException("image");

      bool invertX = (convFlags & ConversionFlags.InvertX) != 0;
      bool invertY = (convFlags & ConversionFlags.InvertY) != 0;
      bool expand = (convFlags & ConversionFlags.Expand) != 0;

      byte[] pixelData = image.Data;
      fixed (byte* imagePtr = pixelData)
      {
        switch (format)
        {
          case DataFormat.R8_UNORM: // 8-bit
            {
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                byte* ptr = imagePtr + image.RowPitch * yOffset + xOffset;

                for (int x = 0; x < image.Width; x++)
                {
                  *ptr = reader.ReadByte();
                  if (invertX)
                    ptr--;
                  else
                    ptr++;
                }
              }
            }
            break;

          case DataFormat.B5G5R5A1_UNORM: // 16-bit
            {
              bool nonZeroAlpha = false;
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                ushort* ptr = (ushort*)(imagePtr + image.RowPitch * yOffset) + xOffset;

                for (int x = 0; x < image.Width; x++)
                {
                  ushort color = ReadBgra5551(reader);
                  if (GetAlpha(color) != 0)
                    nonZeroAlpha = true;

                  *ptr = color;
                  if (invertX)
                    ptr--;
                  else
                    ptr++;
                }
              }

              // If there are no non-zero alpha channel entries, we'll assume alpha is not used and force it to opaque.
              if (!nonZeroAlpha)
              {
                SetAlphaToOpaque(image);
              }
            }
            break;

          case DataFormat.R8G8B8A8_UNORM: // 24/32-bit
            {
              bool nonZeroAlpha = false;
              for (int y = 0; y < image.Height; y++)
              {
                int xOffset = invertX ? image.Width - 1 : 0;
                int yOffset = invertY ? y : image.Height - y - 1;
                uint* ptr = (uint*)(imagePtr + image.RowPitch * yOffset) + xOffset;

                for (int x = 0; x < image.Width; x++)
                {
                  uint color = ReadColor(reader, expand);
                  if (GetAlpha(color) != 0)
                    nonZeroAlpha = true;

                  *ptr = color;
                  if (invertX)
                    ptr--;
                  else
                    ptr++;
                }
              }

              // If there are no non-zero alpha channel entries, we'll assume alpha is not used and force it to opaque.
              if (!nonZeroAlpha)
              {
                SetAlphaToOpaque(image);
              }
            }
            break;
        }
      }
    }


    private static Header EncodeTgaHeader(Image image, ref ConversionFlags convFlags)
    {
      Header header = new Header();

      if (image.Width > 0xFFFF || image.Height > 0xFFFF)
        throw new NotSupportedException("The specified image exceeds the maximum pixel size of a TGA file.");

      header.Width = (ushort)image.Width;
      header.Height = (ushort)image.Height;

      switch (image.Format)
      {
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          header.ImageType = ImageType.TrueColor;
          header.BitsPerPixel = 32;
          header.Descriptor = DescriptorFlags.InvertY | (DescriptorFlags)8; // 8 bit alpha
          convFlags |= ConversionFlags.Swizzle;
          break;

        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8A8_UNORM_SRGB:
          header.ImageType = ImageType.TrueColor;
          header.BitsPerPixel = 32;
          header.Descriptor = DescriptorFlags.InvertY | (DescriptorFlags)8; // 8 bit alpha
          break;

        case DataFormat.B8G8R8X8_UNORM:
        case DataFormat.B8G8R8X8_UNORM_SRGB:
          header.ImageType = ImageType.TrueColor;
          header.BitsPerPixel = 24;
          header.Descriptor = DescriptorFlags.InvertY;
          convFlags |= ConversionFlags.Format888;
          break;

        case DataFormat.R8_UNORM:
        case DataFormat.A8_UNORM:
          header.ImageType = ImageType.BlackAndWhite;
          header.BitsPerPixel = 8;
          header.Descriptor = DescriptorFlags.InvertY;
          break;

        case DataFormat.B5G5R5A1_UNORM:
          header.ImageType = ImageType.TrueColor;
          header.BitsPerPixel = 16;
          header.Descriptor = DescriptorFlags.InvertY | (DescriptorFlags)1; // 1 bit alpha
          break;
      }

      return header;
    }


    // Copies 32-bit BGRX data to 24-bit BGR data.
    private static void Copy24BppScanline(BinaryReader reader, int inSize, BinaryWriter writer)
    {
      Debug.Assert(reader != null && inSize > 0);
      Debug.Assert(writer != null);

      if (inSize >= 4)
      {
        for (int count = 0; count < (inSize - 3); count += 4)
        {
          uint color = reader.ReadUInt32();
          writer.Write((byte)(color & 0xFF));             // Blue
          writer.Write((byte)((color & 0xFF00) >> 8));    // Green
          writer.Write((byte)((color & 0xFF0000) >> 16)); // Red
        }
      }
    }


    /// <summary>
    /// Loads the specified TGA image.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The <see cref="Texture"/> representing the TGA image.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public static Texture Load(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      int offset;
      ConversionFlags convFlags;
      var description = DecodeTGAHeader(stream, out offset, out convFlags);
      var texture = new Texture(description);

      stream.Position = offset;
      using (var reader = new BinaryReader(stream, Encoding.Default))
      {
        if ((convFlags & ConversionFlags.RLE) != 0)
          UncompressPixels(reader, texture.Images[0], description.Format, convFlags);
        else
          CopyPixels(reader, texture.Images[0], description.Format, convFlags);
      }

      return texture;
    }


    /// <summary>
    /// Saves the specified image in TGA format.
    /// </summary>
    /// <param name="image">The image.</param>
    /// <param name="stream">The stream to write to.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="image"/> or <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    public static void Save(Image image, Stream stream)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (stream == null)
        throw new ArgumentNullException("stream");

      var convFlags = ConversionFlags.None;
      var header = EncodeTgaHeader(image, ref convFlags);

      // Write header.
      stream.WriteStruct(header);

      // Determine memory required for image data.
      int rowPitch = (convFlags & ConversionFlags.Format888) != 0 ? image.Width * 3 : image.RowPitch;

      // Write pixels.
      using (var sourceStream = new MemoryStream(image.Data, false))
      using (var reader = new BinaryReader(sourceStream))
#if NET45
      using (var writer = new BinaryWriter(stream, Encoding.Default, true))
#else
      using (var writer = new BinaryWriter(stream, Encoding.Default))   // Warning: Closes the stream!
#endif
      {
        for (int y = 0; y < image.Height; y++)
        {
          if ((convFlags & ConversionFlags.Format888) != 0)
          {
            Copy24BppScanline(reader, image.RowPitch, writer);
          }
          else if ((convFlags & ConversionFlags.Swizzle) != 0)
          {
            TextureHelper.SwizzleScanline(reader, image.RowPitch, writer, rowPitch, image.Format, ScanlineFlags.None);
          }
          else
          {
            TextureHelper.CopyScanline(reader, image.RowPitch, writer, rowPitch, image.Format, ScanlineFlags.None);
          }
        }
      }
    }
  }
}
