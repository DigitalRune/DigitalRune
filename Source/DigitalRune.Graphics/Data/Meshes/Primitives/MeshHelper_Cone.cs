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
    /// Gets the default submesh that represents a cone using triangles.
    /// (The cone is standing on the xz plane pointing along the y axis. Radius = 1. Height = 1.) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of cone. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateCone"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetCone(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Cone";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateCone(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a cone using triangles.
    /// (The cone is standing on the xz plane pointing along the y axis. Radius = 1. Height = 1.) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a cone.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetCone"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateCone(GraphicsDevice graphicsDevice, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      var vertices = new List<VertexPositionNormal>();

      // Base
      Vector3 normal = -Vector3.UnitY;
      vertices.Add(new VertexPositionNormal(new Vector3(0, 0, 0), normal));
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new VertexPositionNormal(new Vector3(x, 0, z), normal));
      }

      // Side
      vertices.Add(new VertexPositionNormal(new Vector3(0, 1, 0), new Vector3(0, 1, 0)));
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        const float cos45 = 0.707106781f; // cos(45°)
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new VertexPositionNormal(new Vector3(x, 0, z), new Vector3(x * cos45, cos45, z * cos45)));
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      var indices = new List<ushort>();

      // Base
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add(0);
        indices.Add((ushort)(i + 1));
        indices.Add((ushort)(i + 2));
      }

      // Last base triangle.
      indices.Add(0);
      indices.Add((ushort)numberOfSegments);
      indices.Add(1);

      // Side triangle.
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)(numberOfSegments + 1));
        indices.Add((ushort)(numberOfSegments + i + 3));
        indices.Add((ushort)(numberOfSegments + i + 2));
      }

      // Last side triangle.
      indices.Add((ushort)(numberOfSegments + 1));
      indices.Add((ushort)(numberOfSegments + 2));
      indices.Add((ushort)(numberOfSegments + numberOfSegments + 1));

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
