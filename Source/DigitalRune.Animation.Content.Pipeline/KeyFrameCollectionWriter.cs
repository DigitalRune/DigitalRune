// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Animation.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="KeyFrameCollection{T}"/> to binary format.
  /// </summary>
  /// <typeparam name="T">The type of the animation value.</typeparam>
  [ContentTypeWriter]
  public class KeyFrameCollectionWriter<T> : ContentTypeWriter<KeyFrameCollection<T>> 
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(KeyFrameCollection<T>).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(KeyFrameCollectionReader<T>).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    protected override void Write(ContentWriter output, KeyFrameCollection<T> value)
    {
      output.Write(value.Count);
      for (int i = 0; i < value.Count; i++)
      {
        var keyFrame = value[i];
        output.WriteRawObject(keyFrame.Time);
        output.WriteRawObject(keyFrame.Value);
      }
    }
  }
}
