// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Reads an <see cref="Occluder"/> from binary format.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class OccluderReader : ContentTypeReader<Occluder>
  {
    /// <summary>
    /// Reads a strongly typed object from the current stream.
    /// </summary>
    /// <param name="input">The <see cref="ContentReader"/> used to read the object.</param>
    /// <param name="existingInstance">An existing object to read into.</param>
    /// <returns>The type of object to read.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected override Occluder Read(ContentReader input, Occluder existingInstance)
    {
      int numberOfVertices = input.ReadInt32();
      var vertices = new Vector3F[numberOfVertices];
      for (int i = 0; i < numberOfVertices; i++)
        vertices[i] = input.ReadRawObject<Vector3F>();

      int numberOfIndices = input.ReadInt32();
      var indices = new ushort[numberOfIndices];
      for (int i = 0; i < numberOfIndices; i++)
        indices[i] = input.ReadUInt16();

      return new Occluder(vertices, indices);
    }
  }
}
