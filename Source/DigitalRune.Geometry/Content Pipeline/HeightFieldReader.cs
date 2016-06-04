// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="HeightField"/> from binary format.
  /// </summary>
  public class HeightFieldReader : ContentTypeReader<HeightField>
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional")]
    protected override HeightField Read(ContentReader input, HeightField existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new HeightField();

      existingInstance.OriginX = input.ReadSingle();
      existingInstance.OriginZ = input.ReadSingle();
      existingInstance.WidthX = input.ReadSingle();
      existingInstance.WidthZ = input.ReadSingle();
      existingInstance.Depth = input.ReadSingle();
      var numberOfSamplesX = input.ReadInt32();
      var numberOfSamplesZ = input.ReadInt32();

      float[] samples = new float[numberOfSamplesX * numberOfSamplesZ];
      for (int i = 0; i < samples.Length; i++)
        samples[i] = input.ReadSingle();

      existingInstance.SetSamples(samples, numberOfSamplesX, numberOfSamplesZ);

      return existingInstance;
    }
  }
}
