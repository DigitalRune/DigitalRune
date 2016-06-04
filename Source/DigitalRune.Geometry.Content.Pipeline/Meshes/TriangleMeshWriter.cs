// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Meshes;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="TriangleMesh"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class TriangleMeshWriter : ContentTypeWriter<TriangleMesh>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(TriangleMesh).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(TriangleMeshReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, TriangleMesh value)
    {
      int numberOfVertices = value.Vertices.Count;
      output.Write(numberOfVertices);
      for (int i = 0; i < numberOfVertices; i++)
        output.WriteRawObject(value.Vertices[i]);

      int numberOfIndices = value.Indices.Count;
      output.Write(numberOfIndices);
      for (int i = 0; i < numberOfIndices; i++)
        output.Write(value.Indices[i]);
    }
  }
}
