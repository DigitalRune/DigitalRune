// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Builds a UI theme including cursors, textures, fonts, etc.
  /// </summary>
  [ContentProcessor(DisplayName = "UI Theme - DigitalRune")]
  public class ThemeProcessor : ContentProcessor<ThemeContent, ThemeContent>
  {
    private string _themeSourceDirectory;
    private Uri _themeSourceDirectoryUri;
    private string _themeOutputDirectory;
    private Uri _themeOutputDirectoryUri;
    private string _sourceRootDirectory;
    private Uri _sourceRootDirectoryUri;


#if MONOGAME
    // When using MonoGame content pipeline:
    // Target platform is stored in ContentProcessorContext.TargetPlatform.
    internal string MonoGamePlatform
    {
      get { return String.Empty; }
    }
#else
    // When using XNA content pipeline to build MonoGame content:
    // Target platform is stored in environment variable.
    internal string MonoGamePlatform
    {
      get
      {
        if (_monoGamePlatform == "_")     // "_" stands for "uninitialized"
        {
          _monoGamePlatform = Environment.GetEnvironmentVariable("MONOGAME_PLATFORM", EnvironmentVariableTarget.User);
          if (string.IsNullOrEmpty(_monoGamePlatform))
            _monoGamePlatform = null;
        }
        return _monoGamePlatform;
      }
    }
    private string _monoGamePlatform = "_";
#endif


    /// <summary>
    /// Processes the specified input data and returns the result.
    /// </summary>
    /// <param name="theme">Existing content object being processed.</param>
    /// <param name="context">Contains any required custom process parameters.</param>
    /// <returns>A typed object representing the processed input.</returns>
    public override ThemeContent Process(ThemeContent theme, ContentProcessorContext context)
    {
      // Get general info about the involved directories.
      // The path/directory containing the input Theme XML file.
      string themeSourcePath = Path.GetFullPath(theme.Identity.SourceFilename);
      _themeSourceDirectory = Path.GetDirectoryName(themeSourcePath) + Path.DirectorySeparatorChar;
      _themeSourceDirectoryUri = new Uri(_themeSourceDirectory);

      // The path/directory containing the output Theme XML file.
      string themeOutputPath = Path.GetFullPath(context.OutputFilename);
      _themeOutputDirectory = Path.GetDirectoryName(themeOutputPath) + Path.DirectorySeparatorChar;
      _themeOutputDirectoryUri = new Uri(_themeOutputDirectory);

      // Get source content root directory. 
      // (Is there a simpler way or a RootDirectory property somewhere?)
      string outputRootDirectory = Path.GetFullPath(context.OutputDirectory);
      //Trace.Assert(_themeOutputDirectory.StartsWith(outputRootDirectory));
      string relativeOutputFolder = _themeOutputDirectory.Substring(outputRootDirectory.Length);
      //Trace.Assert(_themeSourceDirectory.EndsWith(relativeOutputFolder));
      _sourceRootDirectory = _themeSourceDirectory.Substring(
        0,
        _themeSourceDirectory.Length - relativeOutputFolder.Length);
      _sourceRootDirectoryUri = new Uri(_sourceRootDirectory);

      ProcessCursors(theme, context);
      ProcessFonts(theme, context);
      ProcessTextures(theme, context);
      ProcessStyles(theme, context);
      return theme;
    }


    private void ProcessCursors(ThemeContent theme, ContentProcessorContext context)
    {
      theme.Cursors = new NamedObjectCollection<ThemeCursorContent>();

      // Hardware cursors are only supported on Windows.
      if (context.TargetPlatform != TargetPlatform.Windows)
        return;

      var document = theme.Description;
      if (document.Root == null)
      {
        string message = string.Format("Root element \"<Theme>\" is missing in XML.");
        throw new InvalidContentException(message, theme.Identity);
      }

      var cursorsElement = document.Root.Element("Cursors");
      if (cursorsElement == null)
        return;

      foreach (var cursorElement in cursorsElement.Elements("Cursor"))
      {
        string name = GetMandatoryAttribute(cursorElement, "Name", theme.Identity);
        bool isDefault = (bool?)cursorElement.Attribute("IsDefault") ?? false;
        string filename = GetMandatoryAttribute(cursorElement, "File", theme.Identity);

        // Find cursor file.
        string cursorSourcePath = FindFile(theme, filename);
        string cursorSourceDirectory = Path.GetDirectoryName(cursorSourcePath) + Path.DirectorySeparatorChar;
        context.AddDependency(cursorSourcePath);

        // Get output path.
        // If cursor is in or under the content directory, then we keep the relative directory
        // structure. If cursor is in a parent directory, then we copy the cursor
        // directly into the theme directory.
        string cursorOutputPath;
        if (cursorSourceDirectory.StartsWith(_sourceRootDirectory))
        {
          // Cursor file is in/under the content root directory.
          string relativeCursorSourcePath =
            _themeSourceDirectoryUri.MakeRelativeUri(new Uri(cursorSourcePath)).ToString();
          relativeCursorSourcePath = Uri.UnescapeDataString(relativeCursorSourcePath);
          cursorOutputPath = Path.GetFullPath(Path.Combine(_themeOutputDirectory, relativeCursorSourcePath));
        }
        else
        {
          // Cursor file is not in/under the content root directory.
          string cursorFilename = Path.GetFileName(cursorSourcePath) ?? string.Empty;
          cursorOutputPath = Path.GetFullPath(Path.Combine(_themeOutputDirectory, cursorFilename));
        }
        string cursorOutputDirectory = Path.GetDirectoryName(cursorOutputPath) + Path.DirectorySeparatorChar;

        // Create output directory if it does not exist.
        if (!Directory.Exists(cursorOutputDirectory))
          Directory.CreateDirectory(cursorOutputDirectory);

        // Copy cursor file to output directory.
        File.Copy(cursorSourcePath, cursorOutputPath, true);
        context.AddOutputFile(cursorOutputPath);

        // Get filename relative to the theme directory.
        filename =
          _themeOutputDirectoryUri.MakeRelativeUri(new Uri(cursorOutputPath)).OriginalString;
        filename = Uri.UnescapeDataString(filename);

        var cursor = new ThemeCursorContent
        {
          Name = name,
          IsDefault = isDefault,
          FileName = filename,
        };

        theme.Cursors.Add(cursor);
      }
    }


    private void ProcessFonts(ThemeContent theme, ContentProcessorContext context)
    {
      theme.Fonts = new NamedObjectCollection<ThemeFontContent>();

      var document = theme.Description;
      if (document.Root == null)
      {
        string message = string.Format("Root element \"<Theme>\" is missing in XML.");
        throw new InvalidContentException(message, theme.Identity);
      }

      var fontsElement = document.Root.Element("Fonts");
      if (fontsElement == null)
        throw new InvalidContentException("The given UI theme does not contain a 'Fonts' node.", theme.Identity);

      foreach (var fontElement in fontsElement.Elements("Font"))
      {
        string name = GetMandatoryAttribute(fontElement, "Name", theme.Identity);
        bool isDefault = (bool?)fontElement.Attribute("IsDefault") ?? false;
        string filename = GetMandatoryAttribute(fontElement, "File", theme.Identity);

        // Get path of texture relative to root directory without file extension.
        string fontSourcePath = FindFile(theme, filename);
        string relativeFontSourcePath =
          _sourceRootDirectoryUri.MakeRelativeUri(new Uri(fontSourcePath)).OriginalString;
        relativeFontSourcePath = Uri.UnescapeDataString(relativeFontSourcePath);
        string extension = Path.GetExtension(relativeFontSourcePath);
        relativeFontSourcePath = relativeFontSourcePath.Remove(
          relativeFontSourcePath.Length - extension.Length, extension.Length);

        // Build SpriteFont.
        // The input can be an XML file (.spritefont font description file) or a texture.
        bool isXml = IsXmlFile(fontSourcePath);
        ExternalReference<SpriteFontContent> spriteFont;
        if (isXml)
        {
          // Build sprite font from font description file.
          spriteFont = context.BuildAsset<FontDescription, SpriteFontContent>(
            new ExternalReference<FontDescription>(fontSourcePath),
            string.IsNullOrEmpty(MonoGamePlatform) ? "FontDescriptionProcessor" : "MGSpriteFontDescriptionProcessor",
            null,
            "FontDescriptionImporter",
            relativeFontSourcePath);
        }
        else
        {
          // Build sprite font from texture file.
          spriteFont = context.BuildAsset<Texture2DContent, SpriteFontContent>(
            new ExternalReference<Texture2DContent>(fontSourcePath),
            string.IsNullOrEmpty(MonoGamePlatform) ? "FontTextureProcessor" : "MGSpriteFontTextureProcessor",
            null,
            "TextureImporter",
            relativeFontSourcePath);
        }

        var font = new ThemeFontContent
        {
          Name = name,
          IsDefault = isDefault,
          Font = spriteFont
        };

        theme.Fonts.Add(font);
      }

      if (theme.Fonts.Count == 0)
        throw new InvalidContentException("The UI theme does not contain any fonts. At least 1 font is required.", theme.Identity);
    }


    /// <summary>
    /// Determines whether the given file is an XML file.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    private bool IsXmlFile(string filePath)
    {
      // Open file as text.
      using (var stream = File.OpenText(filePath))
      {
        // Use XmlReader to read an XML node. If the file is not an XML file, the reader throws
        // an exception.
        using (XmlReader reader = XmlReader.Create(stream))
        {
          try
          {
            reader.Read();
            return true;
          }
          catch (XmlException)
          {
            // Not a valid XML file.
            return false;
          }
        }
      }
    }


    private void ProcessTextures(ThemeContent theme, ContentProcessorContext context)
    {
      theme.Textures = new NamedObjectCollection<ThemeTextureContent>();

      var document = theme.Description;
      if (document.Root == null)
      {
        string message = string.Format("Root element \"<Theme>\" is missing in XML.");
        throw new InvalidContentException(message, theme.Identity);
      }

      if (document.Root.Elements("Texture").Any())
      {
        // Issue error because theme file is using old alpha version format.
        throw new InvalidContentException("Given theme file is using a format which is no longer supported. All textures need to be defined inside a 'Textures' node.", theme.Identity);
      }

      var texturesElement = document.Root.Element("Textures");
      if (texturesElement == null)
        throw new InvalidContentException("Given theme file does not contain a 'Textures' node.", theme.Identity);

      foreach (var textureElement in texturesElement.Elements("Texture"))
      {
        string name = GetMandatoryAttribute(textureElement, "Name", theme.Identity);
        bool isDefault = (bool?)textureElement.Attribute("IsDefault") ?? false;
        string filename = GetMandatoryAttribute(textureElement, "File", theme.Identity);
        bool premultiplyAlpha = (bool?)textureElement.Attribute("PremultiplyAlpha") ?? true;

        // Get path of texture relative to root directory without file extension.
        string textureSourcePath = FindFile(theme, filename);
        string relativeTextureSourcePath =
            _sourceRootDirectoryUri.MakeRelativeUri(new Uri(textureSourcePath)).OriginalString;
        relativeTextureSourcePath = Uri.UnescapeDataString(relativeTextureSourcePath);
        string extension = Path.GetExtension(relativeTextureSourcePath);
        relativeTextureSourcePath = relativeTextureSourcePath.Remove(
          relativeTextureSourcePath.Length - extension.Length, extension.Length);

        // Build Texture.
        var processorParameters = new OpaqueDataDictionary();
        processorParameters.Add("PremultiplyAlpha", premultiplyAlpha);
        var textureReference = context.BuildAsset<TextureContent, TextureContent>(
          new ExternalReference<TextureContent>(textureSourcePath),
          "TextureProcessor",
          processorParameters,
          "TextureImporter",
          relativeTextureSourcePath);

        var texture = new ThemeTextureContent
        {
          Name = name,
          IsDefault = isDefault,
          Texture = textureReference,
        };

        theme.Textures.Add(texture);
      }

      if (theme.Textures.Count == 0)
        throw new InvalidContentException("The UI theme does not contain any textures. At least 1 texture is required.", theme.Identity);
    }


    private void ProcessStyles(ThemeContent theme, ContentProcessorContext context)
    {
      theme.Styles = new NamedObjectCollection<ThemeStyleContent>();

      var document = theme.Description;
      if (document.Root == null)
      {
        string message = string.Format("Root element \"<Theme>\" is missing in XML.");
        throw new InvalidContentException(message, theme.Identity);
      }

      var stylesElement = document.Root.Element("Styles");
      if (stylesElement == null)
        return;

      foreach (var styleElement in stylesElement.Elements("Style"))
      {
        var style = new ThemeStyleContent();
        style.Name = GetMandatoryAttribute(styleElement, "Name", theme.Identity);
        style.Inherits = (string)styleElement.Attribute("Inherits");

        foreach (var element in styleElement.Elements())
        {
          if (element.Name == "State")
          {
            try
            {
              var state = new ThemeStateContent
              {
                Name = GetMandatoryAttribute(element, "Name", theme.Identity),
                IsInherited = (bool?)element.Attribute("IsInherited") ?? false,
              };

              foreach (var imageElement in element.Elements("Image"))
              {
                var image = new ThemeImageContent
                {
                  Name = (string)imageElement.Attribute("Name"),
                  Texture = (string)imageElement.Attribute("Texture"),
                  SourceRectangle = ThemeHelper.ParseRectangle((string)imageElement.Attribute("Source")),
                  Margin = ThemeHelper.ParseVector4F((string)imageElement.Attribute("Margin")),
                  HorizontalAlignment = ThemeHelper.ParseHorizontalAlignment((string)imageElement.Attribute("HorizontalAlignment")),
                  VerticalAlignment = ThemeHelper.ParseVerticalAlignment((string)imageElement.Attribute("VerticalAlignment")),
                  TileMode = ThemeHelper.ParseTileMode((string)imageElement.Attribute("TileMode")),
                  Border = ThemeHelper.ParseVector4F((string)imageElement.Attribute("Border")),
                  IsOverlay = (bool?)imageElement.Attribute("IsOverlay") ?? false,
                  Color = ThemeHelper.ParseColor((string)imageElement.Attribute("Color"), Color.White),
                };

                if (!string.IsNullOrEmpty(image.Texture) && !theme.Textures.Contains(image.Texture))
                {
                  string message = string.Format("Missing texture: The image '{0}' in state '{1}' of style '{2}' requires a texture named '{3}'.", image.Name, state.Name, style.Name, image.Texture);
                  throw new InvalidContentException(message, theme.Identity);
                }

                state.Images.Add(image);
              }

              var backgroundElement = element.Element("Background");
              if (backgroundElement != null)
                state.Background = ThemeHelper.ParseColor((string)backgroundElement, Color.Transparent);

              var foregroundElement = element.Element("Foreground");
              if (foregroundElement != null)
                state.Foreground = ThemeHelper.ParseColor((string)foregroundElement, Color.Black);

              state.Opacity = (float?)element.Element("Opacity");

              style.States.Add(state);
            }
            catch (InvalidContentException)
            {
              // Rethrow this exception. It should already have a good message.
              throw;
            }
            catch (Exception exception)
            {
              // Build a good error message.
              string message = string.Format("Could not load state '{0}' of style '{1}'. {2}", element.Attribute("Name"), style.Name, exception.Message);
              throw new InvalidContentException(message, theme.Identity, exception);
            }
          }
          else
          {
            // A custom attribute.
            var attribute = new ThemeAttributeContent
            {
              Name = element.Name.ToString(),
              Value = element.Value,
            };
            style.Attributes.Add(attribute);
          }
        }

        theme.Styles.Add(style);
      }

      // Validate inheritance.
      foreach (var style in theme.Styles)
      {
        if (string.IsNullOrEmpty(style.Inherits))
          continue;

        if (!theme.Styles.Contains(style.Inherits))
        {
          // Parent of the given style not found. Log warning.
          context.Logger.LogWarning(
            null,
            theme.Identity,
            "The parent of style \"{0}\" (Inherits = \"{1}\") not found.",
            style.Name,
            style.Inherits);
        }
      }
    }


    private static string GetMandatoryAttribute(XElement element, string name, ContentIdentity identity)
    {
      var attribute = element.Attribute(name);
      if (attribute == null)
      {
        string message = GetExceptionMessage(element, "\"{0}\" attribute is missing.", name);
        throw new InvalidContentException(message, identity);
      }

      string s = (string)attribute;
      if (s.Length == 0)
      {
        string message = GetExceptionMessage(element, "\"{0}\" attribute must not be empty.", name);
        throw new InvalidContentException(message, identity);
      }

      return s;
    }


    private static string GetExceptionMessage(XElement element, string format, params object[] args)
    {
      string message = string.Format(format, args);

      var lineInfo = (IXmlLineInfo)element;
      if (lineInfo.HasLineInfo())
        message += string.Format(" (Element: \"{0}\", Line: {1}, Position {2})", element.Name, lineInfo.LineNumber, lineInfo.LinePosition);
      else
        message += string.Format(" (Element: \"{0}\")", element.Name);

      return message;
    }


    // Returns the full path to the given file.
    private static string FindFile(ThemeContent theme, string filename)
    {
      // Check whether 'filename' is a valid path.
      if (File.Exists(filename))
        return Path.GetFullPath(filename);

      // Perhaps 'filename' contains a relative path (relative to the Theme file).
      string folder = Path.GetDirectoryName(theme.Identity.SourceFilename) ?? string.Empty;
      string relativeFilename = Path.Combine(folder, filename);
      if (File.Exists(relativeFilename))
        return Path.GetFullPath(relativeFilename);

      string message = string.Format("File \"{0}\" not found.", filename);
      throw new InvalidContentException(message, theme.Identity);
    }
  }
}
