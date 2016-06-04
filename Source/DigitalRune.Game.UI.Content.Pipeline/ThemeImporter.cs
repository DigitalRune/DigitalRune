// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Game.UI.Content.Pipeline
{
  /// <summary>
  /// Imports the XML description of a UI theme.
  /// </summary>
  [ContentImporter(".xml", DisplayName = "UI Theme - DigitalRune", DefaultProcessor = "ThemeProcessor")]
  public class ThemeImporter : ContentImporter<ThemeContent>
  {
    /// <summary>
    /// Called by the framework when importing a game asset. This is the method called by XNA when
    /// an asset is to be imported into an object that can be recognized by the Content Pipeline.
    /// </summary>
    /// <param name="filename">Name of a game asset file.</param>
    /// <param name="context">
    /// Contains information for importing a game asset, such as a logger interface.
    /// </param>
    /// <returns>Resulting game asset.</returns>
    public override ThemeContent Import(string filename, ContentImporterContext context)
    {
      var description = XDocument.Load(filename, LoadOptions.SetLineInfo);
      var identity = new ContentIdentity(filename);
      return new ThemeContent(identity, description);
    }
  }
}
