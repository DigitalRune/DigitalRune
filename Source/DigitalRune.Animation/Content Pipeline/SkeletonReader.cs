// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Animation.Character;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Animation.Content
{
  /// <summary>
  /// Reads a <see cref="Skeleton"/> from binary format. 
  /// (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Animation.dll.
  /// </remarks>
  public class SkeletonReader : ContentTypeReader<Skeleton>
  {
#if !MONOGAME
    /// <summary>
    /// Determines if deserialization into an existing object is possible.
    /// </summary>
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
    protected override Skeleton Read(ContentReader input, Skeleton existingInstance)
    {
      string name = null;
      
      if (input.ReadBoolean())
        name = input.ReadString();

      int numberOfBones = input.ReadInt32();

      var boneParents = new int[numberOfBones];
      for (int i = 0; i < numberOfBones; i++)
        boneParents[i] = input.ReadInt32();

      var boneNames = new string[numberOfBones];
      for (int i = 0; i < numberOfBones; i++)
        boneNames[i] = input.ReadString();

      var bindPosesRelative = new SrtTransform[numberOfBones];
      for (int i = 0; i < numberOfBones; i++)
        bindPosesRelative[i] = input.ReadRawObject<SrtTransform>();

      if (existingInstance == null)
        existingInstance = new Skeleton(boneParents, boneNames, bindPosesRelative);
      else
        existingInstance.Initialize(boneParents, boneNames, bindPosesRelative);

      existingInstance.Name = name;

      return existingInstance;
    }
  }
}
