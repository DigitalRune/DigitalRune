// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Graphics.SceneGraph;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="LodGroupNode"/> from binary format.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class LodGroupNodeReader : ContentTypeReader<LodGroupNode>
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
    protected override LodGroupNode Read(ContentReader input, LodGroupNode existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new LodGroupNode();

      // Use AssetLoadHelper to receive an event when the asset (including all 
      // shared resources) is loaded.
      using (var helper = AssetLoadHelper.Get(input.AssetName))
      {
        // ----- SceneNode properties (base class).
        input.ReadRawObject<SceneNode>(existingInstance);

        // ----- LodGroupNode properties
        existingInstance.BeginUpdate();
        existingInstance.Levels.Clear();

        int numberOfLevels = input.ReadInt32();
        for (int i = 0; i < numberOfLevels; i++)
        {
          float distance = input.ReadSingle();
          var lodNode = input.ReadObject<SceneNode>();
          existingInstance.Levels.Add(distance, lodNode);
        }

        helper.AssetLoaded += (s, e) => existingInstance.EndUpdate();
      }

      return existingInstance;
    }
  }
}
