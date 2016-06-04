// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
#if XNA || MONOGAME
using Microsoft.Xna.Framework.Content;
#endif


namespace DigitalRune.Mathematics.Content
{
  /// <summary>
  /// Reads a <see cref="Matrix44F"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class Matrix44FReader : ContentTypeReader<Matrix44F>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Matrix44F Read(ContentReader input, Matrix44F existingInstance)
    {
      float m00 = input.ReadSingle();
      float m01 = input.ReadSingle();
      float m02 = input.ReadSingle();
      float m03 = input.ReadSingle();
      float m10 = input.ReadSingle();
      float m11 = input.ReadSingle();
      float m12 = input.ReadSingle();
      float m13 = input.ReadSingle();
      float m20 = input.ReadSingle();
      float m21 = input.ReadSingle();
      float m22 = input.ReadSingle();
      float m23 = input.ReadSingle();
      float m30 = input.ReadSingle();
      float m31 = input.ReadSingle();
      float m32 = input.ReadSingle();
      float m33 = input.ReadSingle();

      return new Matrix44F(m00, m01, m02, m03,
                           m10, m11, m12, m13,
                           m20, m21, m22, m23,
                           m30, m31, m32, m33);
    }
  }
}
