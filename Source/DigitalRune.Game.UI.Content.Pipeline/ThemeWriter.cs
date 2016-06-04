// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Game.UI.Rendering;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Write a UI theme to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class ThemeWriter : ContentTypeWriter<ThemeContent>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(Theme).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(ThemeReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles an object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="theme">The resultant object.</param>
    protected override void Write(ContentWriter output, ThemeContent theme)
    {
      // Write cursors.
      output.Write(theme.Cursors.Count);
      foreach (var cursor in theme.Cursors)
      {
        output.Write(cursor.Name);
        output.Write(cursor.IsDefault);
        output.Write(cursor.FileName);
      }

      // Write fonts.
      output.Write(theme.Fonts.Count);
      foreach (var font in theme.Fonts)
      {
        output.Write(font.Name);
        output.Write(font.IsDefault);
        output.WriteExternalReference(font.Font);
      }

      // Write textures.
      output.Write(theme.Textures.Count);
      foreach (var texture in theme.Textures)
      {
        output.Write(texture.Name);
        output.Write(texture.IsDefault);
        output.WriteExternalReference(texture.Texture);
      }

      // Write styles.
      output.Write(theme.Styles.Count);
      foreach (var style in theme.Styles)
      {
        output.Write(style.Name);
        output.Write(style.Inherits ?? string.Empty);

        // Write attributes.
        output.Write(style.Attributes.Count);
        foreach (var attribute in style.Attributes)
        {
          output.Write(attribute.Name);
          output.Write(attribute.Value);
        }

        // Write states.
        output.Write(style.States.Count);
        foreach (var state in style.States)
        {
          output.Write(state.Name);
          output.Write(state.IsInherited);

          // Write images.
          output.Write(state.Images.Count);
          foreach (var image in state.Images)
          {
            output.Write(image.Name ?? string.Empty);
            output.Write(image.Texture ?? string.Empty);
            output.WriteRawObject(image.SourceRectangle);
            output.WriteRawObject(image.Margin);
            output.Write((int)image.HorizontalAlignment);
            output.Write((int)image.VerticalAlignment);
            output.Write((int)image.TileMode);
            output.WriteRawObject(image.Border);
            output.Write(image.IsOverlay);
            output.Write(image.Color);
          }

          bool hasBackground = state.Background.HasValue;
          output.Write(hasBackground);
          if (hasBackground)
            output.Write(state.Background.Value);

          bool hasForeground = state.Foreground.HasValue;
          output.Write(hasForeground);
          if (hasForeground)
            output.Write(state.Foreground.Value);

          bool hasOpacity = state.Opacity.HasValue;
          output.Write(hasOpacity);
          if (hasOpacity)
            output.Write(state.Opacity.Value);

        }
      }
    }
  }
}
