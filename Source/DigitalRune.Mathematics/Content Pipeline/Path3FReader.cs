// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Interpolation;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Content
{
  /// <summary>
  /// Reads a <see cref="Path3F"/> from binary format. (Only available in the XNA-compatible build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class Path3FReader : ContentTypeReader<Path3F>
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
    protected override Path3F Read(ContentReader input, Path3F existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new Path3F();
      else
        existingInstance.Clear();

      existingInstance.PreLoop = input.ReadRawObject<CurveLoopType>();
      existingInstance.PostLoop = input.ReadRawObject<CurveLoopType>();
      existingInstance.SmoothEnds = input.ReadBoolean();

      int count = input.ReadInt32();
      for (int i = 0; i < count; i++)
        existingInstance.Add(input.ReadRawObject<PathKey3F>());

      return existingInstance;
    }
  }
}
