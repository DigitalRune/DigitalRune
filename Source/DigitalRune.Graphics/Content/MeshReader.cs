// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content;
#if ANIMATION
using System.Collections.Generic;
using DigitalRune.Animation.Character;
#endif


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="Mesh"/> from binary format.
  /// </summary>
  public class MeshReader : ContentTypeReader<Mesh>
  {
#if !MONOGAME
    /// <summary>
    /// Determines if deserialization into an existing object is possible.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the type can be deserialized into an existing instance; 
    /// <see langword="false"/> otherwise.
    /// </value>
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Mesh Read(ContentReader input, Mesh existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new Mesh();

      existingInstance.BoundingShape = input.ReadObject<Shape>();

      int numberOfSubmeshes = input.ReadInt32();
      for (int i = 0; i < numberOfSubmeshes; i++)
      {
        // Add submesh to Mesh.Submeshes before loading it. This is necessary 
        // because the submesh tries to store its material in Mesh.Materials.
        Submesh submesh = new Submesh();
        existingInstance.Submeshes.Add(submesh);
        input.ReadObject(submesh);
      }

      existingInstance.Name = input.ReadString();

      bool hasOccluder = input.ReadBoolean();
      if (hasOccluder)
      {
        using (var helper = AssetLoadHelper.Get(input.AssetName))
        {
          input.ReadSharedResource(helper.Fixup<Occluder>(o => existingInstance.Occluder = o));
        }
      }

      bool hasSkeleton = input.ReadBoolean();
      if (hasSkeleton)
      {
#if ANIMATION
        using (var helper = AssetLoadHelper.Get(input.AssetName))
        {
          input.ReadSharedResource(helper.Fixup<Skeleton>(s => existingInstance.Skeleton = s));
        }
#else
        throw new ContentLoadException("Mesh contains a skeleton, but this build of DigitalRune Graphics does not support animations.");
#endif
      }

      var hasAnimations = input.ReadBoolean();
      if (hasAnimations)
      {
#if ANIMATION
        using (var helper = AssetLoadHelper.Get(input.AssetName))
        {
          input.ReadSharedResource(helper.Fixup<Dictionary<string, SkeletonKeyFrameAnimation>>(a => existingInstance.Animations = a));
        }
#else
        throw new ContentLoadException("Mesh contains animations, but this build of DigitalRune Graphics does not support animations.");
#endif
      }

      input.ReadSharedResource<object>(userData => existingInstance.UserData = userData);

      return existingInstance;
    }
  }
}
