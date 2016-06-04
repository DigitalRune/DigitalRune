// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="DirectionalLookupTableF{T}"/> to binary format.
  /// </summary>
  /// <typeparam name="T">The type of data stored in the lookup table.</typeparam>
  [ContentTypeWriter]
  public class DirectionalLookupTableFWriter<T> : ContentTypeWriter<DirectionalLookupTableF<T>>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(DirectionalLookupTableF<T>).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(DirectionalLookupTableFReader<T>).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, DirectionalLookupTableF<T> value)
    {
      dynamic internals = value.Internals;
      int width = internals.Width;
      T[,,] cubeMap = internals.CubeMap;

      output.Write(width);
      for (int face = 0; face < 6; face++)
        for (int y = 0; y < width; y++)
          for (int x = 0; x < width; x++)
            output.WriteRawObject(cubeMap[face, y, x]);
    }
  }


  /// <exclude />
  [ContentTypeWriter]
  public class DirectionalLookupTableUInt16FWriter : ContentTypeWriter<DirectionalLookupTableUInt16F>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(DirectionalLookupTableUInt16F).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(DirectionalLookupTableUInt16FReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, DirectionalLookupTableUInt16F value)
    {
      dynamic internals = value.Internals;
      int width = internals.Width;
      var cubeMap = internals.CubeMap;

      output.Write(width);
      for (int face = 0; face < 6; face++)
        for (int y = 0; y < width; y++)
          for (int x = 0; x < width; x++)
            output.WriteRawObject(cubeMap[face, y, x]);
    }
  }
}
