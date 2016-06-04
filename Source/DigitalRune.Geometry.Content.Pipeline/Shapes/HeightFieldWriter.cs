// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Geometry.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="HeightField"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class HeightFieldWriter : ContentTypeWriter<HeightField>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(HeightField).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(HeightFieldReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override void Write(ContentWriter output, HeightField value)
    {
      output.Write(value.OriginX);
      output.Write(value.OriginZ);
      output.Write(value.WidthX);
      output.Write(value.WidthZ);
      output.Write(value.Depth);
      output.Write(value.NumberOfSamplesX);
      output.Write(value.NumberOfSamplesZ);

      int numberOfElements = value.NumberOfSamplesX * value.NumberOfSamplesZ;
      if (numberOfElements > value.Samples.Length)
        throw new InvalidContentException("HeightField.Samples array has less than NumberOfSamplesX x NumberOfSamplesZ elements.");

      var samples = value.Samples;
      for (int i = 0; i < numberOfElements; i++)
        output.Write(samples[i]);
    }
  }
}
