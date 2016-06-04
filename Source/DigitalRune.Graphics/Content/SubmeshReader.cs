// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="Submesh"/> from binary format.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class SubmeshReader : ContentTypeReader<Submesh>
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
    protected override Submesh Read(ContentReader input, Submesh existingInstance)
    {
      // The submesh is deserialized into an existing instance, where Submesh.Mesh is
      // already set!
      if (existingInstance == null)
        throw new ArgumentNullException("existingInstance", "A submesh must be deserialized into an existing instance.");

      using (var helper = AssetLoadHelper.Get(input.AssetName))
      {
        input.ReadSharedResource(helper.Fixup<VertexBuffer>(vb => existingInstance.VertexBuffer = vb));
        existingInstance.StartVertex = input.ReadInt32();
        existingInstance.VertexCount = input.ReadInt32();

        input.ReadSharedResource(helper.Fixup<IndexBuffer>(ib => existingInstance.IndexBuffer = ib));
        existingInstance.StartIndex = input.ReadInt32();
        existingInstance.PrimitiveCount = input.ReadInt32();

        int numberOfMorphs = input.ReadInt32();
        if (numberOfMorphs > 0)
        {
          var morphs = new MorphTargetCollection();
          for (int i = 0; i < numberOfMorphs; i++)
            morphs.Add(input.ReadObject<MorphTarget>());

          existingInstance.MorphTargets = morphs;
        }

        bool hasSharedMaterial = input.ReadBoolean();
        if (hasSharedMaterial)
        {
          // Load external material, which is shared between models.
          existingInstance.SetMaterial(input.ReadExternalReference<Material>());
        }
        else
        {
          // Load local material, which is only shared within the model.
          input.ReadSharedResource(helper.Fixup<Material>(existingInstance.SetMaterial));
        }

        // No fixup helper for the UserData because the UserData can be null (see AssetLoadHelper)!
        input.ReadSharedResource<object>(userData => existingInstance.UserData = userData);
      }

      return existingInstance;
    }
  }
}
