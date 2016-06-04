// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents a box using triangles.
    /// (The box is centered at the origin. The side length is 1.)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of a box. This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateBox"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetBox(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Box";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateBox(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a box using triangles.
    /// (The box is centered at the origin. The side length is 1.)
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a box.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetBox"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateBox(GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      var vertices = new List<VertexPositionNormal>();
      var indices = new List<ushort>();

      var p0 = new Vector3(-0.5f, -0.5f, -0.5f);
      var p1 = new Vector3(-0.5f, -0.5f, +0.5f);
      var p2 = new Vector3(-0.5f, +0.5f, -0.5f);
      var p3 = new Vector3(-0.5f, +0.5f, +0.5f);
      var p4 = new Vector3(+0.5f, -0.5f, -0.5f);
      var p5 = new Vector3(+0.5f, -0.5f, +0.5f);
      var p6 = new Vector3(+0.5f, +0.5f, -0.5f);
      var p7 = new Vector3(+0.5f, +0.5f, +0.5f);

      var normal = Vector3.UnitX;
      vertices.Add(new VertexPositionNormal(p4, normal));
      vertices.Add(new VertexPositionNormal(p5, normal));
      vertices.Add(new VertexPositionNormal(p6, normal));
      vertices.Add(new VertexPositionNormal(p7, normal));

      indices.Add(0);
      indices.Add(1);
      indices.Add(2);

      indices.Add(1);
      indices.Add(3);
      indices.Add(2);

      normal = Vector3.UnitY;
      vertices.Add(new VertexPositionNormal(p6, normal));
      vertices.Add(new VertexPositionNormal(p7, normal));
      vertices.Add(new VertexPositionNormal(p2, normal));
      vertices.Add(new VertexPositionNormal(p3, normal));

      indices.Add(4);
      indices.Add(5);
      indices.Add(6);

      indices.Add(5);
      indices.Add(7);
      indices.Add(6);

      normal = Vector3.UnitZ;
      vertices.Add(new VertexPositionNormal(p5, normal));
      vertices.Add(new VertexPositionNormal(p1, normal));
      vertices.Add(new VertexPositionNormal(p7, normal));
      vertices.Add(new VertexPositionNormal(p3, normal));

      indices.Add(8);
      indices.Add(9);
      indices.Add(10);

      indices.Add(9);
      indices.Add(11);
      indices.Add(10);

      normal = -Vector3.UnitX;
      vertices.Add(new VertexPositionNormal(p1, normal));
      vertices.Add(new VertexPositionNormal(p0, normal));
      vertices.Add(new VertexPositionNormal(p3, normal));
      vertices.Add(new VertexPositionNormal(p2, normal));

      indices.Add(12);
      indices.Add(13);
      indices.Add(14);

      indices.Add(13);
      indices.Add(15);
      indices.Add(14);

      normal = -Vector3.UnitY;
      vertices.Add(new VertexPositionNormal(p4, normal));
      vertices.Add(new VertexPositionNormal(p0, normal));
      vertices.Add(new VertexPositionNormal(p5, normal));
      vertices.Add(new VertexPositionNormal(p1, normal));

      indices.Add(16);
      indices.Add(17);
      indices.Add(18);

      indices.Add(17);
      indices.Add(19);
      indices.Add(18);

      normal = -Vector3.UnitZ;
      vertices.Add(new VertexPositionNormal(p0, normal));
      vertices.Add(new VertexPositionNormal(p4, normal));
      vertices.Add(new VertexPositionNormal(p2, normal));
      vertices.Add(new VertexPositionNormal(p6, normal));

      indices.Add(20);
      indices.Add(21);
      indices.Add(22);

      indices.Add(21);
      indices.Add(23);
      indices.Add(22);

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

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
