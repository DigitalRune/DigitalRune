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
  /// Reads a <see cref="Matrix44D"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class Matrix44DReader : ContentTypeReader<Matrix44D>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Matrix44D Read(ContentReader input, Matrix44D existingInstance)
    {
      double m00 = input.ReadDouble();
      double m01 = input.ReadDouble();
      double m02 = input.ReadDouble();
      double m03 = input.ReadDouble();
      double m10 = input.ReadDouble();
      double m11 = input.ReadDouble();
      double m12 = input.ReadDouble();
      double m13 = input.ReadDouble();
      double m20 = input.ReadDouble();
      double m21 = input.ReadDouble();
      double m22 = input.ReadDouble();
      double m23 = input.ReadDouble();
      double m30 = input.ReadDouble();
      double m31 = input.ReadDouble();
      double m32 = input.ReadDouble();
      double m33 = input.ReadDouble();

      return new Matrix44D(m00, m01, m02, m03,
                           m10, m11, m12, m13,
                           m20, m21, m22, m23,
                           m30, m31, m32, m33);
    }
  }
}
