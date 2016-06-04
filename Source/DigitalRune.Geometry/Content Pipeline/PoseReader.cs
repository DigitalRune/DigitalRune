// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Geometry.Content
{
  /// <summary>
  /// Reads a <see cref="Pose"/> from binary format.
  /// </summary>
  public class PoseReader : ContentTypeReader<Pose>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Pose Read(ContentReader input, Pose existingInstance)
    {
      Vector3F position = input.ReadRawObject<Vector3F>();
      Matrix33F orientation = input.ReadRawObject<Matrix33F>();
      return new Pose(position, orientation);
    }
  }
}
