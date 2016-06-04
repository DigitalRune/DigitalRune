// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="TriangleMeshShape"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class TriangleMeshShapeWriter : ContentTypeWriter<TriangleMeshShape>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(TriangleMeshShape).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(TriangleMeshShapeReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, TriangleMeshShape value)
    {
      dynamic internals = value.Internals;
      Aabb aabbLocal = internals.AabbLocal;
      List<int> triangleNeighbors = internals.TriangleNeighbors;

      output.WriteRawObject(aabbLocal);
      output.WriteObject(value.Partition);
      output.WriteSharedResource(value.Mesh);

      bool enableContactWelding = value.EnableContactWelding;
      output.Write(enableContactWelding);
      if (enableContactWelding)
      {
        Debug.Assert(triangleNeighbors != null);

        int count = triangleNeighbors.Count;
        output.Write(count);
        for (int i = 0; i < count; i++)
          output.Write(triangleNeighbors[i]);
      }
    }
  }
}
