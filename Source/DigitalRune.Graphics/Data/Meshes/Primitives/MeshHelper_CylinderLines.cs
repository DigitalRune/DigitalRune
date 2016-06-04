// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents a cylinder using lines.
    /// (The cylinder is centered at the origin. Radius = 1. Height = 2 (along the y axis).) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of cylinder line list. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateCylinderLines"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetCylinderLines(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__CylinderLines";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateCylinderLines(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a cylinder using lines.
    /// (The cylinder is centered at the origin. Radius = 1. Height = 2 (along the y axis).) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a cylinder line list.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetCylinderLines"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateCylinderLines(GraphicsDevice graphicsDevice, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.LineList,
      };

      var vertices = new List<Vector3F>();

      // Top circle.
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        vertices.Add(new Vector3F((float)Math.Cos(angle), 1, -(float)Math.Sin(angle)));
      }

      // Bottom circle.
      for (int i = 0; i < numberOfSegments; i++)
      {
        Vector3F p = vertices[i];
        vertices.Add(new Vector3F(p.X, -1, p.Z));
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      var indices = new List<ushort>();

      // Top circle.
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)i);          // Line start (= same as previous line end)
        indices.Add((ushort)(i + 1));    // Line end
      }

      // Last line of top circle.
      indices.Add((ushort)(numberOfSegments - 1));
      indices.Add(0);

      // Bottom circle.
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)(numberOfSegments + i));      // Line start (= same as previous line end)
        indices.Add((ushort)(numberOfSegments + i + 1));  // Line end
      }

      // Last line of bottom circle.
      indices.Add((ushort)(numberOfSegments + numberOfSegments - 1));
      indices.Add((ushort)(numberOfSegments));

      // Side (represented by 4 lines).
      for (int i = 0; i < 4; i++)
      {
        indices.Add((ushort)(i * numberOfSegments / 4));
        indices.Add((ushort)(numberOfSegments + i * numberOfSegments / 4));
      }

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Count,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices.ToArray());

      submesh.PrimitiveCount = indices.Count / 2;

      return submesh;
    }
  }
}
