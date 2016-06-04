// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Represents a triangle mesh.
  /// </summary>
  public interface ITriangleMesh
  {
    /// <summary>
    /// Gets the number of triangles.
    /// </summary>
    /// <value>The number of triangles.</value>
    int NumberOfTriangles { get; }


    /// <summary>
    /// Gets the triangle with the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The triangle with the given index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is out of range.
    /// </exception>
    Triangle GetTriangle(int index);
  }
}
