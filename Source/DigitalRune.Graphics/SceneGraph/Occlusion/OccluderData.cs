// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.SceneGraph
{
  /// <summary>
  /// Stores the occluder geometry in world space, ready for drawing.
  /// </summary>
  internal class OccluderData
  {
    /// <summary>The vertices in world space.</summary>
    public Vector3F[] Vertices;

    /// <summary>The indices.</summary>
    public ushort[] Indices;


    /// <summary>
    /// Initializes a new instance of the <see cref="OccluderData"/> class.
    /// </summary>
    /// <param name="occluder">The occluder.</param>
    public OccluderData(Occluder occluder)
    {
      // The occluder is given in local space. The vertices need to be transformed
      // to world space before submitting the vertices to the render batch. 
      Vertices = new Vector3F[occluder.Vertices.Length];

      // The indices are copied as is. (Indices are updated on-the-fly when the
      // values are copied to the render batch.)
      Indices = occluder.Indices;
    }


    /// <summary>
    /// Updates the occluder data.
    /// </summary>
    /// <param name="occluder">The occluder.</param>
    /// <param name="pose">The pose of the <see cref="OccluderNode"/>.</param>
    /// <param name="scale">The scale of the <see cref="OccluderNode"/>.</param>
    public void Update(Occluder occluder, Pose pose, Vector3F scale)
    {
      Debug.Assert(
        Vertices.Length == occluder.Vertices.Length
        && Indices == occluder.Indices,
        "OccluderData does not match.");

      Vector3F[] localVertices = occluder.Vertices;
      if (scale == Vector3F.One)
      {
        for (int i = 0; i < Vertices.Length; i++)
          Vertices[i] = pose.ToWorldPosition(localVertices[i]);
      }
      else
      {
        for (int i = 0; i < Vertices.Length; i++)
          Vertices[i] = pose.ToWorldPosition(scale * localVertices[i]);
      }

      // Update of large occluders could be accelerated by using a parallel for-loop.
      // However, most occluders are small, occluders are already updated in parallel,
      // and static occluders only need to be updated once.
    }
  }
}
