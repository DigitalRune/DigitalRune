// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !NETFX_CORE && !WP7 && !XBOX
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Describes a group of triangles that create one convex part.
  /// </summary>
  [DebuggerDisplay("Island {Id}: Triangles={Triangles.Count}")]
  internal sealed class CDIsland
  {
    // A unique number
    public int Id;

    // The triangles.
    public CDTriangle[] Triangles;

    // The AABB enclosing all triangles.
    public Aabb Aabb;

    // The convex hull vertices.
    public Vector3F[] Vertices;

    // The convex hull. It must be either null or up-to-date. 
    public ConvexHullBuilder ConvexHullBuilder;
  }
}
#endif
