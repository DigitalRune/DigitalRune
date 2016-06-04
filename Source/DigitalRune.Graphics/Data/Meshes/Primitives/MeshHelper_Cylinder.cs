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
    /// Gets the default submesh that represents a cylinder using triangles.
    /// (The cylinder is centered at the origin. Radius = 1. Height = 2 (along the y axis).) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of cylinder. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateCylinder"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetCylinder(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Cylinder";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateCylinder(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a cylinder using triangles.
    /// (The cylinder is centered at the origin. Radius = 1. Height = 2 (along the y axis).) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.
    /// </param>
    /// <returns>A new <see cref="Submesh"/> that represents a cylinder.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetCylinder"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateCylinder(GraphicsDevice graphicsDevice, int numberOfSegments)
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

      // Top cap
      Vector3 normal = Vector3.UnitY;
      vertices.Add(new VertexPositionNormal(new Vector3(0, 1, 0), normal));
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new VertexPositionNormal(new Vector3(x, 1, z), normal));
      }

      // Bottom cap
      normal = -Vector3.UnitY;
      vertices.Add(new VertexPositionNormal(new Vector3(0, -1, 0), normal));
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new VertexPositionNormal(new Vector3(x, -1, z), normal));
      }

      // Side
      int baseIndex = vertices.Count;
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

      var indices = new List<ushort>();

      // Top cap
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add(0);
        indices.Add((ushort)(i + 2));
        indices.Add((ushort)(i + 1));
      }

      // Last triangle of top cap.
      indices.Add(0);
      indices.Add(1);
      indices.Add((ushort)numberOfSegments);

      // Bottom cap
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)(numberOfSegments + 1));
        indices.Add((ushort)(numberOfSegments + i + 2));
        indices.Add((ushort)(numberOfSegments + i + 3));
      }

      // Last triangle of bottom cap.
      indices.Add((ushort)(numberOfSegments + 1));
      indices.Add((ushort)(numberOfSegments + numberOfSegments + 1));
      indices.Add((ushort)(numberOfSegments + 2));

      // Side
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)(baseIndex + 2 * i));
        indices.Add((ushort)(baseIndex + 2 * i + 2));
        indices.Add((ushort)(baseIndex + 2 * i + 1));

        indices.Add((ushort)(baseIndex + 2 * i + 2));
        indices.Add((ushort)(baseIndex + 2 * i + 3));
        indices.Add((ushort)(baseIndex + 2 * i + 1));
      }

      // Indices of last 2 triangle.
      indices.Add((ushort)(baseIndex + numberOfSegments + numberOfSegments - 2));
      indices.Add((ushort)(baseIndex));
      indices.Add((ushort)(baseIndex + numberOfSegments + numberOfSegments - 1));

      indices.Add((ushort)(baseIndex));
      indices.Add((ushort)(baseIndex + 1));
      indices.Add((ushort)(baseIndex + numberOfSegments + numberOfSegments - 1));

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
