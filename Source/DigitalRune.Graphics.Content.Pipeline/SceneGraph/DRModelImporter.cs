// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  /// <summary>
  /// Imports a model based on the model description (.drmdl file).
  /// </summary>
  [ContentImporter(".drmdl", DisplayName = "Model - DigitalRune Graphics", DefaultProcessor = "DRModelProcessor")]
  public class DRModelImporter : ContentImporter<NodeContent>
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
    public override NodeContent Import(string filename, ContentImporterContext context)
    {
      var wrappedContext = new ContentPipelineContext(context);
      var identity = new ContentIdentity(filename);
      var modelDescription = ModelDescription.Load(filename, wrappedContext, false);
      if (modelDescription == null)
        throw new InvalidContentException("Error loading model description.", identity);

      return new DeferredNodeContent(modelDescription) { Identity = identity };
    }
  }
}
