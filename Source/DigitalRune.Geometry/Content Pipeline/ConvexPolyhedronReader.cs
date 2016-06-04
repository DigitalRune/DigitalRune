// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="ConvexPolyhedron"/> from binary format.
  /// </summary>
  public class ConvexPolyhedronReader : ContentTypeReader<ConvexPolyhedron>
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
    protected override ConvexPolyhedron Read(ContentReader input, ConvexPolyhedron existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new ConvexPolyhedron();

      int numberOfVertices = input.ReadInt32();
      Vector3F[] vertices = new Vector3F[numberOfVertices];
      for (int i = 0; i < numberOfVertices; i++)
        vertices[i] = input.ReadRawObject<Vector3F>();

      Aabb aabb = input.ReadRawObject<Aabb>();
      Vector3F innerPoint = input.ReadRawObject<Vector3F>();
      var directionalLookupTable = input.ReadObject<DirectionalLookupTableUInt16F>();
      VertexAdjacency vertexAdjacency = input.ReadObject<VertexAdjacency>();

      existingInstance.Set(vertices, aabb, innerPoint, directionalLookupTable, vertexAdjacency);

      return existingInstance;
    }
  }
}
