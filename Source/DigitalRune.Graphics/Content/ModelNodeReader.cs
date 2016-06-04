// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="ModelNode"/> from binary format.
  /// </summary>
  public class ModelNodeReader : ContentTypeReader<ModelNode>
  {
#if !MONOGAME
    /// <summary>
    /// Determines if deserialization into an existing object is possible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the type can be deserialized into an existing instance; 
    /// <see langword="false"/> otherwise.
    /// </value>
    public override bool CanDeserializeIntoExistingObject
    {
      get { return true; }
    }
#endif


    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    protected override ModelNode Read(ContentReader input, ModelNode existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new ModelNode();

      // Use AssetLoadHelper to receive an event when the asset (including all 
      // shared resources) is loaded.
      using (var helper = AssetLoadHelper.Get(input.AssetName))
      {
        input.ReadRawObject<SceneNode>(existingInstance);
        helper.AssetLoaded += existingInstance.OnAssetLoaded;
      }

      return existingInstance;
    }
  }
}
