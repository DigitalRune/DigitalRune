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
  /// Reads a <see cref="CurveKey2F"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class CurveKey2FReader : ContentTypeReader<CurveKey2F>
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
    protected override CurveKey2F Read(ContentReader input, CurveKey2F existingInstance)
    {
      if (existingInstance == null)
        existingInstance = new CurveKey2F();

      existingInstance.Interpolation = input.ReadRawObject<SplineInterpolation>();
      existingInstance.Point = input.ReadRawObject<Vector2F>();
      existingInstance.TangentIn = input.ReadRawObject<Vector2F>();
      existingInstance.TangentOut = input.ReadRawObject<Vector2F>();

      return existingInstance;
    }
  }
}
