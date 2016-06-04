// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics.Content
{
  partial class TextureHelper
  {
    //--------------------------------------------------------------
    #region XNA Content Pipeline
    //--------------------------------------------------------------

    /// <summary>
    /// Converts an XNA <see cref="TextureContent"/> to a DigitalRune <see cref="Texture"/>.
    /// </summary>
    /// <param name="textureContent">The <see cref="TextureContent"/>.</param>
    /// <returns>The <see cref="Texture"/>.</returns>
    public static Texture ToTexture(TextureContent textureContent)
    {
      SurfaceFormat surfaceFormat;
      var bitmapContent0 = textureContent.Faces[0][0];
      if (!bitmapContent0.TryGetFormat(out surfaceFormat))
        throw new InvalidContentException("Invalid surface format.", textureContent.Identity);

      var texture2DContent = textureContent as Texture2DContent;
      if (texture2DContent != null)
      {
        var description = new TextureDescription
        {
          Dimension = TextureDimension.Texture2D,
          Width = bitmapContent0.Width,
          Height = bitmapContent0.Height,
          Depth = 1,
          MipLevels = texture2DContent.Mipmaps.Count,
          ArraySize = 1,
          Format = surfaceFormat.ToDataFormat()
        };

        var texture = new Texture(description);
        for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
        {
          var bitmapContent = texture2DContent.Mipmaps[mipIndex];
          var image = texture.Images[texture.GetImageIndex(mipIndex, 0, 0)];
          Buffer.BlockCopy(bitmapContent.GetPixelData(), 0, image.Data, 0, image.Data.Length);
        }

        return texture;
      }

      var textureCubeContent = textureContent as TextureCubeContent;
      if (textureCubeContent != null)
      {
        var description = new TextureDescription
        {
          Dimension = TextureDimension.TextureCube,
          Width = bitmapContent0.Width,
          Height = bitmapContent0.Height,
          Depth = 1,
          MipLevels = textureCubeContent.Faces[0].Count,
          ArraySize = 6,
          Format = surfaceFormat.ToDataFormat()
        };

        var texture = new Texture(description);
        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
        {
          for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
          {
            var bitmapContent = textureCubeContent.Faces[faceIndex][mipIndex];
            var image = texture.Images[texture.GetImageIndex(mipIndex, faceIndex, 0)];
            Buffer.BlockCopy(bitmapContent.GetPixelData(), 0, image.Data, 0, image.Data.Length);
          }
        }

        return texture;
      }

      var texture3DContent = textureContent as Texture3DContent;
      if (texture3DContent != null)
      {
        var description = new TextureDescription
        {
          Dimension = TextureDimension.Texture3D,
          Width = bitmapContent0.Width,
          Height = bitmapContent0.Height,
          Depth = texture3DContent.Faces.Count,
          MipLevels = texture3DContent.Faces[0].Count,
          ArraySize = 1,
          Format = surfaceFormat.ToDataFormat()
        };

        var texture = new Texture(description);
        for (int zIndex = 0; zIndex < description.Depth; zIndex++)
        {
          for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
          {
            var bitmapContent = texture3DContent.Faces[zIndex][mipIndex];
            var image = texture.Images[texture.GetImageIndex(mipIndex, 0, zIndex)];
            Buffer.BlockCopy(bitmapContent.GetPixelData(), 0, image.Data, 0, image.Data.Length);
          }
        }

        return texture;
      }

      throw new InvalidOperationException("Invalid texture dimension.");
    }


    /// <summary>
    /// Converts a DigitalRune <see cref="Texture"/> to an XNA <see cref="TextureContent"/>.
    /// </summary>
    /// <param name="texture">The <see cref="Texture"/>.</param>
    /// <param name="identity">The content identity.</param>
    /// <returns>The <see cref="TextureContent"/>.</returns>
    public static TextureContent ToContent(Texture texture, ContentIdentity identity)
    {
      var description = texture.Description;
      switch (description.Dimension)
      {
        case TextureDimension.Texture1D:
        case TextureDimension.Texture2D:
          {
            var textureContent = new Texture2DContent { Identity = identity };
            for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
            {
              var image = texture.Images[texture.GetImageIndex(mipIndex, 0, 0)];
              textureContent.Mipmaps.Add(ToContent(image));
            }

            return textureContent;
          }
        case TextureDimension.TextureCube:
          {
            var textureContent = new TextureCubeContent { Identity = identity };
            for (int faceIndex = 0; faceIndex < 6; faceIndex++)
            {
              for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
              {
                var image = texture.Images[texture.GetImageIndex(mipIndex, faceIndex, 0)];
                textureContent.Faces[faceIndex].Add(ToContent(image));
              }
            }

            return textureContent;
          }
        case TextureDimension.Texture3D:
          {
            var textureContent = new Texture3DContent { Identity = identity };
            for (int zIndex = 0; zIndex < description.Depth; zIndex++)
            {
              textureContent.Faces.Add(new MipmapChain());
              for (int mipIndex = 0; mipIndex < description.MipLevels; mipIndex++)
              {
                var image = texture.Images[texture.GetImageIndex(mipIndex, 0, zIndex)];
                textureContent.Faces[zIndex].Add(ToContent(image));
              }
            }

            return textureContent;
          }
      }

      throw new InvalidOperationException("Invalid texture dimension.");
    }


    /// <summary>
    /// Converts a DigitalRune <see cref="Image"/> to an XNA <see cref="BitmapContent"/>.
    /// </summary>
    /// <param name="image">The <see cref="Image"/>.</param>
    /// <returns>The <see cref="BitmapContent"/>.</returns>
    public static BitmapContent ToContent(Image image)
    {
      BitmapContent content;
      switch (image.Format)
      {
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UNORM_SRGB:
          content = new PixelBitmapContent<Color>(image.Width, image.Height);
          break;
        case DataFormat.B5G6R5_UNORM:
          content = new PixelBitmapContent<Bgr565>(image.Width, image.Height);
          break;
#if !MONOGAME
        case DataFormat.B5G5R5A1_UNORM:
          content = new PixelBitmapContent<Bgra5551>(image.Width, image.Height);
          break;
#endif
        case DataFormat.B4G4R4A4_UNORM:
          content = new PixelBitmapContent<Bgra4444>(image.Width, image.Height);
          break;
        case DataFormat.BC1_UNORM:
        case DataFormat.BC1_UNORM_SRGB:
          content = new Dxt1BitmapContent(image.Width, image.Height);
          break;
        case DataFormat.BC2_UNORM:
        case DataFormat.BC2_UNORM_SRGB:
          content = new Dxt3BitmapContent(image.Width, image.Height);
          break;
        case DataFormat.BC3_UNORM:
        case DataFormat.BC3_UNORM_SRGB:
            content = new Dxt5BitmapContent(image.Width, image.Height);
            break;
        case DataFormat.R8G8_SNORM:
            content = new PixelBitmapContent<NormalizedByte2>(image.Width, image.Height);
            break;
        case DataFormat.R8G8B8A8_SNORM:
            content = new PixelBitmapContent<NormalizedByte4>(image.Width, image.Height);
            break;
#if !MONOGAME
        case DataFormat.R10G10B10A2_UNORM:
          content = new PixelBitmapContent<Rgba1010102>(image.Width, image.Height);
          break;
        case DataFormat.R16G16_UNORM:
          content = new PixelBitmapContent<Rg32>(image.Width, image.Height);
          break;
        case DataFormat.R16G16B16A16_UNORM:
          content = new PixelBitmapContent<Rgba64>(image.Width, image.Height);
          break;
        case DataFormat.A8_UNORM:
        case DataFormat.R8_UNORM:
          content = new PixelBitmapContent<Alpha8>(image.Width, image.Height);
          break;
#endif
        case DataFormat.R32_FLOAT:
            content = new PixelBitmapContent<float>(image.Width, image.Height);
            break;
        case DataFormat.R32G32_FLOAT:
            content = new PixelBitmapContent<Vector2>(image.Width, image.Height);
            break;
        case DataFormat.R32G32B32A32_FLOAT:
            content = new PixelBitmapContent<Vector4>(image.Width, image.Height);
            break;
        case DataFormat.R16_FLOAT:
            content = new PixelBitmapContent<HalfSingle>(image.Width, image.Height);
            break;
        case DataFormat.R16G16_FLOAT:
            content = new PixelBitmapContent<HalfVector2>(image.Width, image.Height);
            break;
        case DataFormat.R16G16B16A16_FLOAT:
            content = new PixelBitmapContent<HalfVector4>(image.Width, image.Height);
            break;
#if MONOGAME
        case DataFormat.PVRTCI_2bpp_RGB:
            content = new PvrtcRgb2BitmapContent(image.Width, image.Height);
            break;
        case DataFormat.PVRTCI_4bpp_RGB:
            content = new PvrtcRgb4BitmapContent(image.Width, image.Height);
            break;
        case DataFormat.PVRTCI_2bpp_RGBA:
            content = new PvrtcRgba2BitmapContent(image.Width, image.Height);
            break;
        case DataFormat.PVRTCI_4bpp_RGBA:
            content = new PvrtcRgba4BitmapContent(image.Width, image.Height);
            break;

        case DataFormat.ETC1:
            content = new Etc1BitmapContent(image.Width, image.Height);
            break;

        //case DataFormat.ATC_RGB: Not supported in MonoGame.
        case DataFormat.ATC_RGBA_EXPLICIT_ALPHA:
            content = new AtcExplicitBitmapContent(image.Width, image.Height);
            break;
        case DataFormat.ATC_RGBA_INTERPOLATED_ALPHA:
            content = new AtcInterpolatedBitmapContent(image.Width, image.Height);
            break;
#endif

        default:
          string message = string.Format("The texture format {0} is not supported in MonoGame.", image.Format);
          throw new NotSupportedException(message);

        // Not supported:
        //  SurfaceFormat.HdrBlendable  // Only needed as render target format.
        //  SurfaceFormat.Bgr32         // Only used as WPF render target.
        //  SurfaceFormat.Bgra32        // Only used as WPF render target.
        //  SurfaceFormat.Dxt1a
      }

      Debug.Assert(content != null);
#if !MONOGAME
      // content.GetPixelData() is null in MonoGame.
      Debug.Assert(image.Data.Length == content.GetPixelData().Length);
#endif

      content.SetPixelData(image.Data);
      return content;
    }
    #endregion
  }
}
