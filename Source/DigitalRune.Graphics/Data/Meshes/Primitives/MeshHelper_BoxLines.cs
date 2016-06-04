// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents a box using lines.
    /// (The box is centered at the origin. The side length is 1.)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of box line list. This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateBoxLines"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetBoxLines(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__BoxLines";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateBoxLines(graphicsService.GraphicsDevice);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a box using lines.
    /// (The box is centered at the origin. The side length is 1.)
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a box line list.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetBoxLines"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateBoxLines(GraphicsDevice graphicsDevice)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.LineList,
      };

      var vertices = new[]
      {
        new Vector3F(-0.5f, -0.5f, +0.5f),
        new Vector3F(+0.5f, -0.5f, +0.5f),
        new Vector3F(+0.5f, +0.5f, +0.5f),
        new Vector3F(-0.5f, +0.5f, +0.5f),
        new Vector3F(-0.5f, -0.5f, -0.5f),
        new Vector3F(+0.5f, -0.5f, -0.5f),
        new Vector3F(+0.5f, +0.5f, -0.5f),
        new Vector3F(-0.5f, +0.5f, -0.5f)
      };

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices);
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      var indices = new ushort[]
      {
        0, 1,
        1, 2,
        2, 3,
        3, 0,
        
        4, 5,
        5, 6,
        6, 7,
        7, 4,
        
        0, 4,
        1, 5,
        2, 6,
        3, 7
      };

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Length,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices);

      submesh.PrimitiveCount = indices.Length / 2;

      return submesh;
    }
  }
}
