// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="ConvexPolyhedron"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class ConvexPolyhedronWriter : ContentTypeWriter<ConvexPolyhedron>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(ConvexPolyhedron).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(ConvexPolyhedronReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, ConvexPolyhedron value)
    {
      dynamic internals = value.Internals;
      DirectionalLookupTableF<ushort> directionalLookupTable = internals.DirectionalLookupTable;
      VertexAdjacency vertexAdjacency = internals.VertexAdjacency;

      var vertices = value.Vertices;
      int numberOfVertices = vertices.Count;
      output.Write(numberOfVertices);
      for (int i = 0; i < numberOfVertices; i++)
        output.WriteRawObject(vertices[i]);

      output.WriteRawObject(value.GetAabb());
      output.WriteRawObject(value.InnerPoint);
      output.WriteObject(directionalLookupTable);
      output.WriteObject(vertexAdjacency);
    }
  }
}
