// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Content;
using Microsoft.Xna.Framework.Graphics;
using SharpDX.WIC;
using Texture = DigitalRune.Graphics.Content.Texture;


namespace DigitalRune.Editor.Textures
{
    internal static class TextureHelper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static Texture2D LoadTexture(IGraphicsService graphicsService, string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            if (fileName.Length == 0)
                throw new ArgumentException("The file name must not be empty.", nameof(fileName));

            // Load API-independent texture.
            Texture texture = null;
            using (var stream = File.OpenRead(fileName))
            {
                string extension = Path.GetExtension(fileName);
                if (!string.IsNullOrEmpty(extension))
                {
                    extension = extension.ToUpperInvariant();
                    if (extension == ".DDS")
                        texture = DdsHelper.Load(stream, DdsFlags.ForceRgb | DdsFlags.ExpandLuminance);
                    else if (extension == ".TGA")
                        texture = TgaHelper.Load(stream);
                }

                if (texture == null)
                {
                    // TODO: Register ImagingFactory as service.
                    using (var imagingFactory = new ImagingFactory())
                        texture = WicHelper.Load(imagingFactory, stream, WicFlags.ForceRgb | WicFlags.No16Bpp);
                }
            }

            //Tests(texture);

            // Convert to XNA texture.
            var description = texture.Description;
            if (description.Dimension == TextureDimension.TextureCube)
            {
                var texture2D = new Texture2D(graphicsService.GraphicsDevice, description.Width, description.Height, false, description.Format.ToSurfaceFormat());
                texture2D.SetData(texture.Images[0].Data);
                return texture2D;
            }
            else
            {
                var texture2D = new Texture2D(graphicsService.GraphicsDevice, description.Width, description.Height, description.MipLevels > 1, description.Format.ToSurfaceFormat());
                for (int i = 0; i < texture.Description.MipLevels; i++)
                    texture2D.SetData(i, null, texture.Images[i].Data, 0, texture.Images[i].Data.Length);

                return texture2D;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "texture")]
        private static void Tests(Texture texture)
        {
            //var testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //TextureHelper.FlipX(testTexture);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\FlipX.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //TextureHelper.FlipY(testTexture);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\FlipY.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = TextureHelper.Rotate(testTexture, 90);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Rotate90.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = TextureHelper.Rotate(testTexture, 180);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Rotate180.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = TextureHelper.Rotate(testTexture, 270);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Rotate270.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //var testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = testTexture.Resize(1000, 1000, 1, ResizeFilter.Box, false, WrapMode.Clamp);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Resize1000.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = testTexture.Resize(200, 100, 1, ResizeFilter.Box, false, WrapMode.Clamp);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Resize200x100.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //var testTexture = texture.ConvertTo(TextureFormat.R32G32B32A32_Float);
            //testTexture = testTexture.Resize(1024, 1024, 1, ResizeFilter.Box, true, WrapMode.Repeat);
            //testTexture.GenerateMipmaps(ResizeFilter.Box, true, WrapMode.Repeat);
            //testTexture = testTexture.ConvertTo(TextureFormat.R8G8B8A8_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\Mipmaps.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //var testTexture = texture.ConvertTo(TextureFormat.BC1_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\BC1.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.BC2_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\BC2.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);

            //testTexture = texture.ConvertTo(TextureFormat.BC3_UNorm);
            //using (var stream = File.OpenWrite("c:\\temp\\BC3.dds"))
            //  DdsHelper.Save(testTexture, stream, DdsFlags.None);
        }


        public static ColorChannels GetColorChannels(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                case SurfaceFormat.Dxt5:
                case SurfaceFormat.NormalizedByte4:
                case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Rgba64:
                case SurfaceFormat.Vector4:
                case SurfaceFormat.HalfVector4:
                case SurfaceFormat.HdrBlendable:
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.ColorSRgb:
                case SurfaceFormat.Bgra32SRgb:
                case SurfaceFormat.Dxt1SRgb:
                case SurfaceFormat.Dxt3SRgb:
                case SurfaceFormat.Dxt5SRgb:
                case SurfaceFormat.RgbaPvrtc2Bpp:
                case SurfaceFormat.RgbaPvrtc4Bpp:
                case SurfaceFormat.Dxt1a:
                case SurfaceFormat.RgbaAtcExplicitAlpha:
                case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                    return ColorChannels.RGBA;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgr32:
                case SurfaceFormat.Bgr32SRgb:
                case SurfaceFormat.RgbPvrtc2Bpp:
                case SurfaceFormat.RgbPvrtc4Bpp:
                case SurfaceFormat.RgbEtc1:
                    return ColorChannels.RGB;
                case SurfaceFormat.NormalizedByte2:
                case SurfaceFormat.Rg32:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.HalfVector2:
                    return ColorChannels.Red | ColorChannels.Green;
                case SurfaceFormat.Alpha8:
                    return ColorChannels.Alpha;
                case SurfaceFormat.Single:
                case SurfaceFormat.HalfSingle:
                    return ColorChannels.Red;
            }

            throw new InvalidOperationException("Should never get to here. Incomplete switch statement.");
        }
    }
}
