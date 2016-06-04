// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="PoseD"/> from binary format.
  /// </summary>
  public class PoseDReader : ContentTypeReader<PoseD>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override PoseD Read(ContentReader input, PoseD existingInstance)
    {
      Vector3D position = input.ReadRawObject<Vector3D>();
      Matrix33D orientation = input.ReadRawObject<Matrix33D>();
      return new PoseD(position, orientation);
    }
  }
}
