// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents an uncapped (open) cylinder using triangles. 
    /// (The cylinder centered at the origin. Radius = 1. Height = 2 (along the y axis).)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of an uncapped cylinder. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateUncappedCylinder"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetUncappedCylinder(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__UncappedCylinder";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateUncappedCylinder(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents an uncapped (open) cylinder using triangles. 
    /// (The cylinder centered at the origin. Radius = 1. Height = 2 (along the y axis).)
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>
    /// A new <see cref="Submesh"/> that represents an uncapped cylinder (a cylinder without
    /// flat circle caps).
    /// </returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetUncappedCylinder"/> to retrieve a shared <see cref="Submesh"/> 
    /// instance.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateUncappedCylinder(GraphicsDevice graphicsDevice, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      // ----- Vertices
      var vertices = new List<VertexPositionNormal>();
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new VertexPositionNormal(new Vector3(x, 1, z), new Vector3(x, 0, z)));
        vertices.Add(new VertexPositionNormal(new Vector3(x, -1, z), new Vector3(x, 0, z)));
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      // ----- Indices
      var indices = new List<ushort>();
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)(2 * i));
        indices.Add((ushort)(2 * i + 2));
        indices.Add((ushort)(2 * i + 1));

        indices.Add((ushort)(2 * i + 2));
        indices.Add((ushort)(2 * i + 3));
        indices.Add((ushort)(2 * i + 1));
      }

      // Indices of last 2 triangle.
      indices.Add((ushort)(numberOfSegments + numberOfSegments - 2));
      indices.Add(0);
      indices.Add((ushort)(numberOfSegments + numberOfSegments - 1));

      indices.Add(0);
      indices.Add(1);
      indices.Add((ushort)(numberOfSegments + numberOfSegments - 1));

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Count,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices.ToArray());

      submesh.PrimitiveCount = indices.Count / 3;

      return submesh;
    }
  }
}
