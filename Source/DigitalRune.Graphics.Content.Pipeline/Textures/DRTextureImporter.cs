// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Path = System.IO.Path;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Provides methods for reading texture files for use in the Content Pipeline. 
  /// </summary>
  [ContentImporter(".image_file_extension",  // Do not set file extension, otherwise it conflicts with XNA TextureImporter.
                   DisplayName = "Texture - DigitalRune Graphics",
                   DefaultProcessor = "DRTextureProcessor")]
  public class DRTextureImporter : TextureImporter
  {
    /// <summary>
    /// Called by the XNA Framework when importing an texture file to be used as a game asset. This
    /// is the method called by the XNA Framework when an asset is to be imported into an object
    /// that can be recognized by the Content Pipeline.
    /// </summary>
    /// <param name="filename">Name of a game asset file.</param>
    /// <param name="context">
    /// Contains information for importing a game asset, such as a logger interface.
    /// </param>
    /// <returns>Resulting game asset.</returns>
    public override TextureContent Import(string filename, ContentImporterContext context)
    {
      string extension = Path.GetExtension(filename);
      if (extension != null)
      {
        Texture texture = null;
        if (extension.Equals(".DDS", StringComparison.OrdinalIgnoreCase))
        {
          using (var stream = File.OpenRead(filename))
            texture = DdsHelper.Load(stream, DdsFlags.ForceRgb | DdsFlags.ExpandLuminance);
        }
        else if (extension.Equals(".TGA", StringComparison.OrdinalIgnoreCase))
        {
          using (var stream = File.OpenRead(filename))
            texture = TgaHelper.Load(stream);
        }

        if (texture != null)
        {
#if !MONOGAME
          // When using the XNA content pipeline, check for MonoGame content.
          if (!string.IsNullOrEmpty(ContentHelper.GetMonoGamePlatform()))
#endif
          {
            // These formats are not (yet) available in MonoGame.
            switch (texture.Description.Format)
            {
              case DataFormat.B5G5R5A1_UNORM:  // (16-bit TGA files.)
              case DataFormat.R8_UNORM:
              case DataFormat.A8_UNORM:
                texture = texture.ConvertTo(DataFormat.R8G8B8A8_UNORM);
                break;
            }
          }

          // Convert DigitalRune Texture to XNA TextureContent.
          var identity = new ContentIdentity(filename, "DigitalRune");
          return TextureHelper.ToContent(texture, identity);
        }
      }

      return base.Import(filename, context);
    }
  }
}
