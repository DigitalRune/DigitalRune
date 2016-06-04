// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Graphics.Effects;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads a <see cref="Material"/> from binary format.
  /// </summary>
  public class MaterialReader : ContentTypeReader<Material>
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
    protected override Material Read(ContentReader input, Material existingInstance)
    {
      if (input == null)
        throw new ArgumentNullException("input");

      if (existingInstance == null)
        existingInstance = new Material();
      else
        existingInstance.Clear();

      existingInstance.Name = input.ReadString();

      int numberOfPasses = input.ReadInt32();
      for (int i = 0; i < numberOfPasses; i++)
      {
        string pass = input.ReadString();
        var binding = input.ReadObject<EffectBinding>();
        existingInstance.Add(pass, binding);
      }

      return existingInstance;
    }
  }
}
