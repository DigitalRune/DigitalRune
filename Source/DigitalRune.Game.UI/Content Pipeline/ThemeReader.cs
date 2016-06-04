// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Storages;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Game.UI.Content
{
  /// <summary>
  /// Reads a UI theme from binary format.
  /// </summary>
  public class ThemeReader : ContentTypeReader<Theme>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
    protected override Theme Read(ContentReader input, Theme existingInstance)
    {
      if (existingInstance == null)
      {
        existingInstance = new Theme();
      }
      else
      {
        foreach (var cursor in existingInstance.Cursors)
          PlatformHelper.DestroyCursor(cursor.Cursor);

        existingInstance.Cursors.Clear();
        existingInstance.Fonts.Clear();
        existingInstance.Styles.Clear();
        existingInstance.Textures.Clear();
      }

      existingInstance.Content = input.ContentManager;

      // Read cursors.
      int numberOfCursors = input.ReadInt32();
      for (int i = 0; i < numberOfCursors; i++)
      {
        var cursor = new ThemeCursor();
        cursor.Name = input.ReadString();
        cursor.IsDefault = input.ReadBoolean();
        cursor.Cursor = LoadCursor(input);
        existingInstance.Cursors.Add(cursor);
      }

      // Read fonts.
      int numberOfFonts = input.ReadInt32();
      for (int i = 0; i < numberOfFonts; i++)
      {
        var font = new ThemeFont();
        font.Name = input.ReadString();
        font.IsDefault = input.ReadBoolean();
        font.Font = input.ReadExternalReference<SpriteFont>();
        existingInstance.Fonts.Add(font);
      }

      // Read textures.
      int numberOfTextures = input.ReadInt32();
      for (int i = 0; i < numberOfTextures; i++)
      {
        var texture = new ThemeTexture();
        texture.Name = input.ReadString();
        texture.IsDefault = input.ReadBoolean();
        texture.Texture = input.ReadExternalReference<Texture2D>();
        existingInstance.Textures.Add(texture);
      }

      // Read styles.
      int numberOfStyles = input.ReadInt32();
      var inheritanceTable = new Dictionary<ThemeStyle, string>();
      for (int i = 0; i < numberOfStyles; i++)
      {
        var style = new ThemeStyle();
        style.Name = input.ReadString();
        inheritanceTable.Add(style, input.ReadString());
        
        // Read attributes
        int numberOfAttributes = input.ReadInt32();
        for (int j = 0; j < numberOfAttributes; j++)
        {
          var attribute = new ThemeAttribute();
          attribute.Name = input.ReadString();
          attribute.Value = input.ReadString();
          style.Attributes.Add(attribute);
        }

        // Read states.
        int numberOfStates = input.ReadInt32();
        for (int j = 0; j < numberOfStates; j++)
        {
          var state = new ThemeState();
          state.Name = input.ReadString();
          state.IsInherited = input.ReadBoolean();

          // Read images.
          int numberOfImages = input.ReadInt32();
          for (int k = 0; k < numberOfImages; k++)
          {
            var image = new ThemeImage();
            image.Name = input.ReadString();

            string textureName = input.ReadString();
            if (!string.IsNullOrEmpty(textureName))
            {
              ThemeTexture texture;
              if (existingInstance.Textures.TryGet(textureName, out texture))
                image.Texture = texture;
            }

            image.SourceRectangle = input.ReadRawObject<Rectangle>();
            image.Margin = input.ReadRawObject<Vector4F>();
            image.HorizontalAlignment = (HorizontalAlignment)input.ReadInt32();
            image.VerticalAlignment = (VerticalAlignment)input.ReadInt32();
            image.TileMode = (TileMode)input.ReadInt32();
            image.Border = input.ReadRawObject<Vector4F>();
            image.IsOverlay = input.ReadBoolean();
            image.Color = input.ReadColor();
            state.Images.Add(image);
          }

          bool hasBackground = input.ReadBoolean();
          if (hasBackground)
            state.Background = input.ReadColor();

          bool hasForeground = input.ReadBoolean();
          if (hasForeground)
            state.Foreground = input.ReadColor();

          bool hasOpacity = input.ReadBoolean();
          if (hasOpacity)
            state.Opacity = input.ReadSingle();

          style.States.Add(state);
        }

        existingInstance.Styles.Add(style);
      }

      // Resolve style inheritance.
      foreach (var entry in inheritanceTable)
      {
        var style = entry.Key;
        string parentName = entry.Value;
        if (string.IsNullOrEmpty(parentName))
          continue;

        ThemeStyle parent;
        if (existingInstance.Styles.TryGet(parentName, out parent))
          style.Inherits = parent;
      }

      return existingInstance;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
    private static object LoadCursor(ContentReader input)
    {
#if WP7 || XBOX
      return null;
#else
      string filename = input.ReadString();
      string subFolder = Path.GetDirectoryName(input.AssetName) ?? string.Empty;

      // Try to load from file system.
      string contentFolder = input.ContentManager.RootDirectory;
      try
      {
        string path = Path.Combine(contentFolder, subFolder, filename);
        var cursor = PlatformHelper.CreateCursor(path);
        if (cursor != null)
          return cursor;

        // Cursor was not found on file system. Perhaps it is in a ZIP storage.
        var storageProvider = input.ContentManager as IStorageProvider;
        if (storageProvider != null)
        {
          path = Path.Combine(subFolder, filename);
          using (var stream = storageProvider.Storage.OpenFile(path))
            cursor = PlatformHelper.CreateCursor(stream);
        }

        return cursor;
      }
      catch (ArgumentException)    // This happens with animated cursors in Linux.
      {
        return null;
      }
#endif
    }
  }
}
