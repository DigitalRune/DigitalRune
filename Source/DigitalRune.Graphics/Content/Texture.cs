// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Represents a texture resource. (For use in content pipeline. Not intended to be used at
  /// runtime.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="Texture"/> can represent 1D, 2D, 3D (volume) textures, cube maps, and texture
  /// arrays. The implementation is API-independent and is used for processing assets in the content
  /// pipeline.
  /// </para>
  /// <para>
  /// A texture consists of one or more images (see <see cref="Images"/>). The order of the images
  /// is the same as Direct3D uses for texture subresources. Use <see cref="GetImageIndex"/> to
  /// find the index of a specific image.
  /// </para>
  /// </remarks>
  internal class Texture
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the texture description.
    /// </summary>
    /// <value>The texture description.</value>
    public TextureDescription Description { get; private set; }


    /// <summary>
    /// Gets the images.
    /// </summary>
    /// <value>The images.</value>
    public ImageCollection Images { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Texture"/> class.
    /// </summary>
    /// <param name="description">The description.</param>
    /// <exception cref="ArgumentException">
    /// The <paramref name="description"/> is invalid.
    /// </exception>
    public Texture(TextureDescription description)
    {
      ValidateTexture(description);
      Description = description;
      Images = CreateImageCollection(description);
    }


    /// <summary>
    /// Validates the texture.
    /// </summary>
    /// <param name="description">The texture description.</param>
    /// <exception cref="ArgumentException">
    /// The <paramref name="description"/> is invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The specified format is not supported.
    /// </exception>
    private static void ValidateTexture(TextureDescription description)
    {
      if (!TextureHelper.IsValid(description.Format))
        throw new ArgumentException("The specified texture format is not supported.", "description");

      if (TextureHelper.IsPalettized(description.Format))
        throw new ArgumentException("Palettized texture formats are not supported.", "description");

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
          if (description.Width <= 0 || description.Height != 1 || description.Depth != 1 || description.ArraySize <= 0)
            throw new ArgumentException("Invalid texture description.", "description");

          //if (TextureHelper.IsVideo(description.Format))
          //  throw new NotSupportedException("Video formats are not supported.");

          if (!TextureHelper.ValidateMipLevels(description.Width, 1, description.MipLevels))
            throw new ArgumentException("Invalid number of mipmap levels.", "description");
          break;

        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          if (description.Width <= 0 || description.Height <= 0 || description.Depth != 1 || description.ArraySize <= 0)
            throw new ArgumentException("Invalid texture description.", "description");

          if (description.Dimension == TextureDimension.TextureCube)
          {
            if ((description.ArraySize % 6) != 0)
              throw new ArgumentException("All six faces need to be specified for cube maps.", "description");

            //if (TextureHelper.IsVideo(description.Format))
            //  throw new NotSupportedException("Video formats are not supported.");
          }

          if (!TextureHelper.ValidateMipLevels(description.Width, description.Height, description.MipLevels))
            throw new ArgumentException("Invalid number of mipmaps levels.", "description");
          break;

        case TextureDimension.Texture3D:
          if (description.Width <= 0 || description.Height <= 0 || description.Depth <= 0 || description.ArraySize != 1)
            throw new ArgumentException("Invalid texture description.", "description");

          //if (TextureHelper.IsVideo(description.Format) || TextureHelper.IsPlanar(description.Format)
          //    || TextureHelper.IsDepthStencil(description.Format))
          //  throw new NotSupportedException("The specified texture format is not supported.");

          if (!TextureHelper.ValidateMipLevels(description.Width, description.Height, description.Depth, description.MipLevels))
            throw new ArgumentException("Invalid number of mipmaps levels.", "description");
          break;

        default:
          throw new NotSupportedException("The specified texture dimension is not supported.");
      }
    }


    private static ImageCollection CreateImageCollection(TextureDescription description, bool skipMipLevel0 = false)
    {
      int numberOfImages, pixelSize;
      DetermineImages(description, out numberOfImages, out pixelSize);
      var images = new ImageCollection(numberOfImages);

      int index = 0;
      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          Debug.Assert(description.ArraySize != 0);
          Debug.Assert(description.MipLevels > 0);

          for (int item = 0; item < description.ArraySize; item++)
          {
            int w = description.Width;
            int h = description.Height;

            for (int level = 0; level < description.MipLevels; level++)
            {
              if (!skipMipLevel0 || level != 0)
                images[index] = new Image(w, h, description.Format);

              index++;

              if (h > 1)
                h >>= 1;

              if (w > 1)
                w >>= 1;
            }
          }
          break;

        case TextureDimension.Texture3D:
          {
            Debug.Assert(description.MipLevels > 0);
            Debug.Assert(description.Depth > 0);

            int w = description.Width;
            int h = description.Height;
            int d = description.Depth;

            for (int level = 0; level < description.MipLevels; level++)
            {
              for (int slice = 0; slice < d; slice++)
              {
                // We use the same memory organization that Direct3D 11 needs for D3D11_SUBRESOURCE_DATA
                // with all slices of a given mip level being continuous in memory.
                if (!skipMipLevel0 || level != 0)
                  images[index] = new Image(w, h, description.Format);

                index++;
              }

              if (h > 1)
                h >>= 1;

              if (w > 1)
                w >>= 1;

              if (d > 1)
                d >>= 1;
            }
          }
          break;

        default:
          Debug.Fail("Unexpected texture dimension");
          break;
      }

      return images;
    }


    /// <summary>
    /// Determines the number of image array entries and pixel size.
    /// </summary>
    /// <param name="description">The texture description.</param>
    /// <param name="nImages">The number of entries in the image array.</param>
    /// <param name="pixelSize">The total pixel size.</param>
    private static void DetermineImages(TextureDescription description, out int nImages, out int pixelSize)
    {
      Debug.Assert(description.Width > 0 && description.Height > 0 && description.Depth > 0);
      Debug.Assert(description.ArraySize > 0);
      Debug.Assert(description.MipLevels > 0);

      pixelSize = 0;
      nImages = 0;

      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
        case TextureDimension.TextureCube:
          for (int item = 0; item < description.ArraySize; item++)
          {
            int w = description.Width;
            int h = description.Height;

            for (int level = 0; level < description.MipLevels; level++)
            {
              int rowPitch, slicePitch;
              TextureHelper.ComputePitch(description.Format, w, h, out rowPitch, out slicePitch, ComputePitchFlags.None);
              pixelSize += slicePitch;
              nImages++;

              if (h > 1)
                h >>= 1;

              if (w > 1)
                w >>= 1;
            }
          }
          break;

        case TextureDimension.Texture3D:
          {
            int w = description.Width;
            int h = description.Height;
            int d = description.Depth;

            for (int level = 0; level < description.MipLevels; level++)
            {
              int rowPitch, slicePitch;
              TextureHelper.ComputePitch(description.Format, w, h, out rowPitch, out slicePitch, ComputePitchFlags.None);

              for (int slice = 0; slice < d; slice++)
              {
                pixelSize += slicePitch;
                nImages++;
              }

              if (h > 1)
                h >>= 1;

              if (w > 1)
                w >>= 1;

              if (d > 1)
                d >>= 1;
            }
          }
          break;

        default:
          Debug.Fail("Unexpected texture dimension");
          break;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the content of the texture.
    /// </summary>
    /// <returns>The content of the texture.</returns>
    public byte[] GetData()
    {
      using (var stream = new MemoryStream())
      {
        foreach (var image in Images)
          stream.Write(image.Data, 0, image.Data.Length);

        return stream.ToArray();
      }
    }


    /// <summary>
    /// Gets the index of a specific image.
    /// </summary>
    /// <param name="mipIndex">The mipmap level, where 0 is the most detailed level.</param>
    /// <param name="arrayOrFaceIndex">
    /// The array index for texture arrays, or the face index for cube maps. Must be 0 for volume
    /// textures.
    /// </param>
    /// <param name="zIndex">The z index for volume textures.</param>
    /// <returns>
    /// The index of the specified image in the <see cref="Images"/> collection.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mipIndex"/>, <paramref name="arrayOrFaceIndex"/>, or <paramref name="zIndex"/> is
    /// out of range.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Invalid texture dimension.
    /// </exception>
    public int GetImageIndex(int mipIndex, int arrayOrFaceIndex, int zIndex)
    {
      return Description.GetImageIndex(mipIndex, arrayOrFaceIndex, zIndex);
    }


    /// <summary>
    /// Gets the depth of the specified mipmap level.
    /// </summary>
    /// <param name="mipLevel">The mipmap level, where 0 is the most detailed level.</param>
    /// <returns>The depth of texture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mipLevel"/> is out of range.
    /// </exception>
    public int GetDepth(int mipLevel)
    {
      return Description.GetDepth(mipLevel);
    }


    /// <summary>
    /// Determines whether conversion to the specified texture format is supported.
    /// </summary>
    /// <param name="format">The desired texture format.</param>
    /// <returns>
    /// <see langword="true"/> if the conversion from the current format to <paramref name="format"/>
    /// is supported; otherwise, <see langword="false"/>.
    /// </returns>
    public bool CanConvertTo(DataFormat format)
    {
      var srcFormat = Description.Format;
      var dstFormat = format;

      // srcFormat -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, dstFormat))
        return true;

      // srcFormat -> R32G32B32A32_FLOAT -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, DataFormat.R32G32B32A32_FLOAT)
          && TextureHelper.CanConvert(DataFormat.R32G32B32A32_FLOAT, dstFormat))
        return true;

      // srcFormat -> R8G8B8A8_UNORM -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, DataFormat.R8G8B8A8_UNORM)
          && TextureHelper.CanConvert(DataFormat.R8G8B8A8_UNORM, dstFormat))
        return true;

      return false;
    }


    /// <summary>
    /// Converts the specified texture to another format.
    /// </summary>
    /// <param name="format">The desired texture format.</param>
    /// <returns>
    /// The texture using <paramref name="format" />. Does nothing (returns <c>this</c>) if texture
    /// already has the desired format.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Texture conversion to the specified format is not supported.
    /// </exception>
    public Texture ConvertTo(DataFormat format)
    {
      var srcFormat = Description.Format;
      var dstFormat = format;

      if (srcFormat == dstFormat)
        return this;

      // ----- Direct conversion:
      // srcFormat -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, dstFormat))
      {
        var description = Description;
        description.Format = dstFormat;

        var texture = new Texture(description);

#if SINGLE_THREADED
        for (int i = 0; i < Images.Count; i++)
#else
        Parallel.For(0, Images.Count, i =>
#endif
        {
          TextureHelper.Convert(Images[i], texture.Images[i]);
        }
#if !SINGLE_THREADED
        );
#endif

        return texture;
      }

      // ----- Conversion using intermediate formats:
      // srcFormat -> R32G32B32A32_FLOAT -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, DataFormat.R32G32B32A32_FLOAT)
          && TextureHelper.CanConvert(DataFormat.R32G32B32A32_FLOAT, dstFormat))
      {
        var texture = ConvertTo(DataFormat.R32G32B32A32_FLOAT);
        return texture.ConvertTo(dstFormat);
      }

      // srcFormat -> R8G8B8A8_UNORM -> dstFormat
      if (TextureHelper.CanConvert(srcFormat, DataFormat.R8G8B8A8_UNORM)
          && TextureHelper.CanConvert(DataFormat.R8G8B8A8_UNORM, dstFormat))
      {
        var texture = ConvertTo(DataFormat.R8G8B8A8_UNORM);
        return texture.ConvertTo(dstFormat);
      }

      throw new NotSupportedException(string.Format("Texture format conversion from {0} to {1} is not supported.", srcFormat, dstFormat));
    }


    /// <summary>
    /// (Re-)Generates all mipmap levels.
    /// </summary>
    /// <param name="filter">The filter to use for resizing.</param>
    /// <param name="alphaTransparency">
    /// <see langword="true"/> if the image contains uses non-premultiplied alpha; otherwise,
    /// <see langword="false"/> if the image uses premultiplied alpha or has no alpha.
    /// </param>
    /// <param name="wrapMode">
    /// The texture address mode that will be used for sampling the at runtime.
    /// </param>
    public void GenerateMipmaps(ResizeFilter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      var oldDescription = Description;
      var newDescription = Description;

      // Determine number of mipmap levels.
      if (oldDescription.Dimension == TextureDimension.Texture3D)
        newDescription.MipLevels = TextureHelper.CalculateMipLevels(oldDescription.Width, oldDescription.Height, oldDescription.Depth);
      else
        newDescription.MipLevels = TextureHelper.CalculateMipLevels(oldDescription.Width, oldDescription.Height);

      if (oldDescription.MipLevels != newDescription.MipLevels)
      {
        // Update Description and Images.
        var oldImages = Images;
        Description = newDescription;
#if DEBUG
        ValidateTexture(newDescription);
#endif
        // Recreate image collection. (Mipmap level 0 is copied from existing image collection.)
        Images = CreateImageCollection(newDescription, true);
        for (int arrayIndex = 0; arrayIndex < newDescription.ArraySize; arrayIndex++)
        {
          for (int zIndex = 0; zIndex < newDescription.Depth; zIndex++)
          {
            int oldIndex = oldDescription.GetImageIndex(0, arrayIndex, zIndex);
            int newIndex = newDescription.GetImageIndex(0, arrayIndex, zIndex);
            Images[newIndex] = oldImages[oldIndex];
          }
        }
      }

      // Downsample mipmap levels.
      for (int arrayIndex = 0; arrayIndex < newDescription.ArraySize; arrayIndex++)
        for (int mipIndex = 0; mipIndex < newDescription.MipLevels - 1; mipIndex++)
          TextureHelper.Resize(this, mipIndex, arrayIndex, this, mipIndex + 1, arrayIndex, filter, alphaTransparency, wrapMode);
    }


    /// <summary>
    /// Resizes the texture. (If original texture has mipmaps, all mipmap levels are automatically
    /// recreated.)
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    /// <param name="depth">The new depth. Must be 1 for 2D textures and cube map textures.</param>
    /// <param name="filter">The filter to use for resizing.</param>
    /// <param name="alphaTransparency">
    /// <see langword="true"/> if the image contains uses non-premultiplied alpha; otherwise,
    /// <see langword="false"/> if the image uses premultiplied alpha or has no alpha.
    /// </param>
    /// <param name="wrapMode">
    /// The texture address mode that will be used for sampling the at runtime.
    /// </param>
    /// <returns>The resized texture.</returns>
    public Texture Resize(int width, int height, int depth, ResizeFilter filter, bool alphaTransparency, TextureAddressMode wrapMode)
    {
      var description = Description;
      description.Width = width;
      description.Height = height;
      description.Depth = depth;

      var resizedTexture = new Texture(description);

      // Resize mipmap level 0.
      for (int arrayIndex = 0; arrayIndex < description.ArraySize; arrayIndex++)
        TextureHelper.Resize(this, 0, arrayIndex, resizedTexture, 0, arrayIndex, filter, alphaTransparency, wrapMode);

      // Regenerate mipmap levels, if necessary.
      if (description.MipLevels > 1)
        resizedTexture.GenerateMipmaps(filter, alphaTransparency, wrapMode);

      return resizedTexture;
    }
    #endregion
  }
}
