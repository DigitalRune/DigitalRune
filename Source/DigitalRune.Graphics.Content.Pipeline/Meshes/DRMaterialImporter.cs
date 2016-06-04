// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Imports a material definition (.drmat file).
  /// </summary>
  [ContentImporter(".drmat", DisplayName = "Material - DigitalRune Graphics", DefaultProcessor = "DRMaterialProcessor")]
  public class DRMaterialImporter : ContentImporter<DRMaterialContent>
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
    public override DRMaterialContent Import(string filename, ContentImporterContext context)
    {
      string name = Path.GetFileNameWithoutExtension(filename);
      var identity = new ContentIdentity(filename);
      var definition = XDocument.Load(filename, LoadOptions.SetLineInfo);

      return new DRMaterialContent
      {
        Name = name,
        Identity = identity,
        Definition = definition
      };
    }
  }
}
