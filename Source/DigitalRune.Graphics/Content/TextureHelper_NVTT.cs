// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Nvidia.TextureTools;


namespace DigitalRune.Graphics.Content
{
  partial class TextureHelper
  {
    // Note: nvtt.dll is included in MonoGame.Framework.Content.Pipeline.Windows and
    // copied to the output folder.

    private class OutputHandler
    {
      private readonly Texture _texture;
      private int _imageIndex;
      private int _dataOffset;

      public OutputHandler(Texture texture)
      {
        _texture = texture;
      }

      public void BeginImage(int size, int width, int height, int depth, int face, int miplevel)
      {
        _imageIndex = _texture.GetImageIndex(miplevel, face, 0);
        _dataOffset = 0;

        Debug.Assert(size == _texture.Images[_imageIndex].Data.Length);
        Debug.Assert(width == _texture.Images[_imageIndex].Width);
        Debug.Assert(height == _texture.Images[_imageIndex].Height);
        Debug.Assert(depth == 1, "Volume texture are not yet supported in NVTT.");
      }

      public bool WriteData(IntPtr data, int length)
      {
        var image = _texture.Images[_imageIndex];
        Debug.Assert(_dataOffset + length <= image.Data.Length);
        Marshal.Copy(data, image.Data, _dataOffset, length);
        _dataOffset += length;
        return true;
      }
    }


    /// <summary>
    /// Resizes the specified texture and/or generates mipmaps.
    /// </summary>
    /// <param name="texture">The texture.</param>
    /// <param name="width">The desired width.</param>
    /// <param name="height">The desired height.</param>
    /// <param name="inputGamma">The input gamma.</param>
    /// <param name="outputGamma">The output gamma.</param>
    /// <param name="generateMipmaps">
    /// <see langword="true"/> to generate all mipmap levels; otherwise <see langword="false"/>.
    /// </param>
    /// <param name="hasAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> requires an alpha channel; otherwise,
    /// <see langword="false"/> if <paramref name="texture"/> is opaque.
    /// </param>
    /// <param name="hasFractionalAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> contains fractional alpha values;
    /// otherwise, <see langword="false"/> if <paramref name="texture"/> is opaque or contains only
    /// binary alpha.
    /// </param>
    /// <param name="premultipliedAlpha">
    /// <see langword="true"/> when <paramref name="texture"/> is using premultiplied alpha.;
    /// otherwise, <see langword="false"/>.</param>
    /// <returns>The resized texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    public static Texture ResizeAndGenerateMipmaps(Texture texture, int width, int height, float inputGamma, float outputGamma, bool generateMipmaps, bool hasAlpha, bool hasFractionalAlpha, bool premultipliedAlpha)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");

      // NVIDIA Texture Tools expect BGRA 8:8:8:8.
      if (texture.Description.Format != TextureFormat.B8G8R8A8_UNorm)
        throw new ArgumentException("Texture format needs to be B8G8R8A8_UNORM.", "texture");

      if (texture.Description.Dimension != TextureDimension.TextureCube && texture.Description.ArraySize > 1)
        throw new NotSupportedException("Resizing and mipmap generation for texture arrays is not supported.");
      if (texture.Description.Dimension == TextureDimension.Texture3D)
        throw new NotSupportedException("Resizing and mipmap generation for volume textures is not supported.");

      // ----- InputOptions
      var inputOptions = new InputOptions();
      inputOptions.SetAlphaMode(hasAlpha ? (premultipliedAlpha ? AlphaMode.Premultiplied : AlphaMode.Transparency)
                                         : AlphaMode.None);
      inputOptions.SetFormat(InputFormat.BGRA_8UB);
      inputOptions.SetGamma(inputGamma, outputGamma);
      inputOptions.SetMipmapFilter(MipmapFilter.Box);
      inputOptions.SetMipmapGeneration(generateMipmaps);
      bool roundToPowerOfTwo = (width != texture.Description.Width || height != texture.Description.Height);
      inputOptions.SetRoundMode(roundToPowerOfTwo ? RoundMode.ToNextPowerOfTwo : RoundMode.None);
      inputOptions.SetWrapMode(WrapMode.Mirror);

      var description = texture.Description;
      bool isCube = description.Dimension == TextureDimension.TextureCube;
      var textureType = isCube ? TextureType.TextureCube : TextureType.Texture2D;
      inputOptions.SetTextureLayout(textureType, description.Width, description.Height, 1);

      for (int arrayIndex = 0; arrayIndex < description.ArraySize; arrayIndex++)
      {
        for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
        {
          int index = texture.GetImageIndex(mipIndex, arrayIndex, 0);
          var image = texture.Images[index];
          var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
          inputOptions.SetMipmapData(handle.AddrOfPinnedObject(), image.Width, image.Height, 1, arrayIndex, mipIndex);
          handle.Free();
        }
      }

      // ----- OutputOptions
      var outputOptions = new OutputOptions();
      outputOptions.SetOutputHeader(false);
      outputOptions.Error += OnError;

      description.Format = TextureFormat.R8G8B8A8_UNorm;
      description.Width = width;
      description.Height = height;
      description.MipLevels = generateMipmaps ? CalculateMipLevels(width, height) : 1;
      var resizedTexture = new Texture(description);
      var outputHandler = new OutputHandler(resizedTexture);
      outputOptions.SetOutputHandler(outputHandler.BeginImage, outputHandler.WriteData);

      // ----- CompressionOptions
      var compressionOptions = new CompressionOptions();
      compressionOptions.SetFormat(Format.RGBA);
      compressionOptions.SetPixelFormat(32, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);
      compressionOptions.SetQuality(Quality.Normal);

      // ----- Run NVTT
      try
      {
        var compressor = new Compressor();
        compressor.Compress(inputOptions, compressionOptions, outputOptions);
      }
      catch (NullReferenceException)
      {
        // Resizing and mipmap generation without compression sometimes causes a
        // NullReferenceException in nvttCompress().
        throw new Exception("NullReferenceException in NVIDIA texture tools. Please try again.");
      }

      return resizedTexture;
    }


    /// <summary>
    /// Compresses the specified texture using a Block Compression format (BC<i>n</i>).
    /// </summary>
    /// <param name="texture">The uncompressed texture.</param>
    /// <param name="inputGamma">The input gamma.</param>
    /// <param name="outputGamma">The output gamma.</param>
    /// <param name="generateMipmaps">
    /// <see langword="true"/> to generate all mipmap levels; otherwise <see langword="false"/>.
    /// </param>
    /// <param name="hasAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> requires an alpha channel; otherwise,
    /// <see langword="false"/> if <paramref name="texture"/> is opaque.
    /// </param>
    /// <param name="hasFractionalAlpha">
    /// <see langword="true"/> if <paramref name="texture"/> contains fractional alpha values;
    /// otherwise, <see langword="false"/> if <paramref name="texture"/> is opaque or contains only
    /// binary alpha.
    /// </param>
    /// <param name="premultipliedAlpha">
    /// <see langword="true"/> when <paramref name="texture"/> is using premultiplied alpha.;
    /// otherwise, <see langword="false"/>.</param>
    /// <param name="sharpAlpha">
    /// <see langword="true"/> when the texture contains a sharp alpha mask; otherwise
    /// <see langword="false"/>.
    /// </param>
    /// <returns>The compressed texture.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="texture"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Texture width and height need to be equal (square texture) and  a power of two (POT
    /// texture).
    /// </exception>
    internal static Texture CompressBCn(Texture texture, float inputGamma, float outputGamma, bool generateMipmaps, bool hasAlpha, bool hasFractionalAlpha, bool premultipliedAlpha, bool sharpAlpha = false)
    {
      if (texture == null)
        throw new ArgumentNullException("texture");
      if (texture.Description.Dimension == TextureDimension.Texture3D)
        throw new NotSupportedException("Texture compression for volume textures is not supported.");

      // NVIDIA Texture Tools expect BGRA 8:8:8:8.
      texture = texture.ConvertTo(TextureFormat.B8G8R8A8_UNorm);

      // ----- InputOptions
      var inputOptions = new InputOptions();
      inputOptions.SetAlphaMode(hasAlpha ? (premultipliedAlpha ? AlphaMode.Premultiplied : AlphaMode.Transparency)
                                         : AlphaMode.None);
      inputOptions.SetFormat(InputFormat.BGRA_8UB);
      inputOptions.SetGamma(inputGamma, outputGamma);
      inputOptions.SetMipmapFilter(MipmapFilter.Box);
      inputOptions.SetMipmapGeneration(generateMipmaps);
      inputOptions.SetRoundMode(RoundMode.None);  // Size is set explicitly.
      inputOptions.SetWrapMode(WrapMode.Mirror);

      var description = texture.Description;
      bool isCube = description.Dimension == TextureDimension.TextureCube;
      var textureType = isCube ? TextureType.TextureCube : TextureType.Texture2D;
      inputOptions.SetTextureLayout(textureType, description.Width, description.Height, 1);

      for (int arrayIndex = 0; arrayIndex < description.ArraySize; arrayIndex++)
      {
        for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
        {
          int index = texture.GetImageIndex(mipIndex, arrayIndex, 0);
          var image = texture.Images[index];
          var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
          inputOptions.SetMipmapData(handle.AddrOfPinnedObject(), image.Width, image.Height, 1, arrayIndex, mipIndex);
          handle.Free();
        }
      }

      // ----- OutputOptions
      var outputOptions = new OutputOptions();
      outputOptions.SetOutputHeader(false);
      outputOptions.Error += OnError;

      Format compressedFormat;
      if (hasAlpha)
      {
        if (sharpAlpha)
        {
          compressedFormat = Format.BC2;
          description.Format = TextureFormat.BC2_UNorm;
        }
        else
        {
          compressedFormat = Format.BC3;
          description.Format = TextureFormat.BC3_UNorm;
        }
      }
      else
      {
        compressedFormat = Format.BC1;
        description.Format = TextureFormat.BC1_UNorm;
      }
      var compressedTexture = new Texture(description);
      var outputHandler = new OutputHandler(compressedTexture);
      outputOptions.SetOutputHandler(outputHandler.BeginImage, outputHandler.WriteData);

      // ----- CompressionOptions
      var compressionOptions = new CompressionOptions();
      compressionOptions.SetFormat(compressedFormat);
      compressionOptions.SetQuality(Quality.Normal);

      // ----- Run NVTT
      try
      {
        var compressor = new Compressor();
        compressor.Compress(inputOptions, compressionOptions, outputOptions);
      }
      catch (NullReferenceException)
      {
        throw new Exception("NullReferenceException in NVIDIA texture tools. Please try again.");
      }

      return compressedTexture;
    }


    private static void OnError(Error error)
    {
      throw new Exception(string.Format("NVIDIA texture tools failed: {0}", Compressor.ErrorString(error)));
    }
  }
}
