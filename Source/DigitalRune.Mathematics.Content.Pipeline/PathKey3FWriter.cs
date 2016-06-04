// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Interpolation;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Mathematics.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="PathKey3F"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class PathKey3FWriter : ContentTypeWriter<PathKey3F>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(PathKey3F).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(PathKey3FReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, PathKey3F value)
    {
      output.WriteRawObject(value.Interpolation);
      output.Write(value.Parameter);
      output.WriteRawObject(value.Point);
      output.WriteRawObject(value.TangentIn);
      output.WriteRawObject(value.TangentOut);
    }
  }
}
