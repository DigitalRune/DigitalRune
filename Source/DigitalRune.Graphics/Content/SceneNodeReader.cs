// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="SceneNode"/> from binary format.
  /// </summary>
  public class SceneNodeReader : ContentTypeReader<SceneNode>
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
    protected override SceneNode Read(ContentReader input, SceneNode existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new SceneNode();

      int numberOfChildren = input.ReadInt32();
      if (numberOfChildren > 0)
        existingInstance.Children = new SceneNodeCollection();

      for (int i = 0; i < numberOfChildren; i++)
      {
        var child = input.ReadObject<SceneNode>();
        existingInstance.Children.Add(child);
      }

      existingInstance.Name = input.ReadString();
      existingInstance.PoseLocal = input.ReadRawObject<Pose>();
      existingInstance.ScaleLocal = input.ReadRawObject<Vector3F>();
      existingInstance.MaxDistance = input.ReadSingle();
      input.ReadSharedResource<object>(userData => existingInstance.UserData = userData);

      return existingInstance;
    }
  }
}
