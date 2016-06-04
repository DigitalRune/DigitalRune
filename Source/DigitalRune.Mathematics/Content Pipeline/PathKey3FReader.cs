// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Content
{
  /// <summary>
  /// Reads a <see cref="PathKey3F"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class PathKey3FReader : ContentTypeReader<PathKey3F>
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
    protected override PathKey3F Read(ContentReader input, PathKey3F existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new PathKey3F();

      existingInstance.Interpolation = input.ReadRawObject<SplineInterpolation>();
      existingInstance.Parameter = input.ReadSingle();
      existingInstance.Point = input.ReadRawObject<Vector3F>();
      existingInstance.TangentIn = input.ReadRawObject<Vector3F>();
      existingInstance.TangentOut = input.ReadRawObject<Vector3F>();

      return existingInstance;
    }
  }
}
