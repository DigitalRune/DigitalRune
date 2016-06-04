// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Animation.Character;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;


namespace DigitalRune.Animation.Content.Pipeline
{
  /// <summary>
  /// Writes a <see cref="Skeleton"/> to binary format.
  /// </summary>
  [ContentTypeWriter]
  public class SkeletonWriter : ContentTypeWriter<Skeleton>
  {
    /// <summary>
    /// Gets the assembly qualified name of the runtime target type.
    /// </summary>
    /// <param name="targetPlatform">The target platform.</param>
    /// <returns>The qualified name.</returns>
    public override string GetRuntimeType(TargetPlatform targetPlatform)
    {
      return typeof(Skeleton).AssemblyQualifiedName;
    }


    /// <summary>
    /// Gets the assembly qualified name of the runtime loader for this type.
    /// </summary>
    /// <param name="targetPlatform">Name of the platform.</param>
    /// <returns>Name of the runtime loader.</returns>
    public override string GetRuntimeReader(TargetPlatform targetPlatform)
    {
      return typeof(SkeletonReader).AssemblyQualifiedName;
    }


    /// <summary>
    /// Compiles a strongly typed object into binary format.
    /// </summary>
    /// <param name="output">The content writer serializing the value.</param>
    /// <param name="value">The value to write.</param>
    protected override void Write(ContentWriter output, Skeleton value)
    {
      dynamic internals = value.Internals;
      int[] boneParents = internals.BoneParents;
      SrtTransform[] bindPosesRelative = internals.BindPosesRelative;

      output.Write(value.Name != null);
      if (value.Name != null)
        output.Write(value.Name);

      int numberOfBones = value.NumberOfBones;
      output.Write(numberOfBones);
      
      for (int i = 0; i < numberOfBones; i++)
        output.Write(boneParents[i]);

      for (int i = 0; i < numberOfBones; i++)
        output.Write(value.GetName(i));

      for (int i = 0; i < numberOfBones; i++)
        output.WriteRawObject(bindPosesRelative[i]);
    }
  }
}
