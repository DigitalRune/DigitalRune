// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="DirectionalLookupTableF{T}"/> from binary format.
  /// </summary>
  /// <typeparam name="T">The type of data stored in the lookup table.</typeparam>
  public class DirectionalLookupTableFReader<T> : ContentTypeReader<DirectionalLookupTableF<T>>
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
    protected override DirectionalLookupTableF<T> Read(ContentReader input, DirectionalLookupTableF<T> existingInstance)
    {
      int width = input.ReadInt32();
      T[, ,] cubeMap = new T[6, width, width];
      for (int face = 0; face < 6; face++)
        for (int y = 0; y < width; y++)
          for (int x = 0; x < width; x++)
            cubeMap[face, y, x] = input.ReadRawObject<T>();

      if (existingInstance == null)
        existingInstance = new DirectionalLookupTableF<T>(cubeMap);
      else
        existingInstance.CubeMap = cubeMap;

      return existingInstance;
    }
  }


  /// <exclude/>
  [CLSCompliant(false)]
  public class DirectionalLookupTableUInt16FReader : ContentTypeReader<DirectionalLookupTableUInt16F>
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
    protected override DirectionalLookupTableUInt16F Read(ContentReader input, DirectionalLookupTableUInt16F existingInstance)
    {
      int width = input.ReadInt32();
      var cubeMap = new ushort[6, width, width];
      for (int face = 0; face < 6; face++)
        for (int y = 0; y < width; y++)
          for (int x = 0; x < width; x++)
            cubeMap[face, y, x] = input.ReadRawObject<ushort>();

      if (existingInstance == null)
        existingInstance = new DirectionalLookupTableUInt16F(cubeMap);
      else
        existingInstance.CubeMap = cubeMap;

      return existingInstance;
    }
  }
}
