// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Animation.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="BlendGroup"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class BlendGroupWriter : ContentTypeWriter<BlendGroup>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(BlendGroup).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(BlendGroupReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    protected override void Write(ContentWriter output, BlendGroup value)
    {
      output.WriteRawObject(value.FillBehavior);
      output.Write(value.TargetObject != null);
      if (value.TargetObject != null)
        output.Write(value.TargetObject);

      output.Write(value.Count);
      for (int i = 0; i < value.Count; i++)
      {
        output.WriteSharedResource(value[i]);
        output.Write(value.GetWeight(i));
      }
    }
  }
}
