// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="TriangleMeshShape"/> from binary format.
  /// </summary>
  public class TriangleMeshShapeReader : ContentTypeReader<TriangleMeshShape>
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
    protected override TriangleMeshShape Read(ContentReader input, TriangleMeshShape existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new TriangleMeshShape();

      existingInstance._aabbLocal = input.ReadRawObject<Aabb>();
      var partition = input.ReadObject<ISpatialPartition<int>>();

      input.ReadSharedResource<ITriangleMesh>(triangleMesh =>
                                              {
                                                existingInstance.SetMesh(triangleMesh);
                                                existingInstance.SetPartition(partition);
                                              });

      bool enableContactWelding = input.ReadBoolean();
      if (enableContactWelding)
      {
        int count = input.ReadInt32();
        existingInstance.TriangleNeighbors = new List<int>(count);
        for (int i = 0; i < count; i++)
          existingInstance.TriangleNeighbors.Add(input.ReadInt32());
      }

      return existingInstance;
    }
  }
}
