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
  /// Reads a <see cref="Matrix33D"/> from binary format. (Only available in the XNA-compatible
  /// build.)
  /// </summary>
  /// <remarks>
  /// This type is available only in the XNA-compatible build of the DigitalRune.Mathematics.dll.
  /// </remarks>
  public class Matrix33DReader : ContentTypeReader<Matrix33D>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Matrix33D Read(ContentReader input, Matrix33D existingInstance)
    {
      double m00 = input.ReadDouble();
      double m01 = input.ReadDouble();
      double m02 = input.ReadDouble();
      double m10 = input.ReadDouble();
      double m11 = input.ReadDouble();
      double m12 = input.ReadDouble();
      double m20 = input.ReadDouble();
      double m21 = input.ReadDouble();
      double m22 = input.ReadDouble();

      return new Matrix33D(m00, m01, m02,
                           m10, m11, m12,
                           m20, m21, m22);
    }
  }
}
