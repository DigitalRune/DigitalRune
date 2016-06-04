// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents a teapot using triangles.
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of a teapot. This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateTeapot"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetTeapot(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Teapot";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateTeapot(graphicsService.GraphicsDevice, 1, 8);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a teapot using triangles.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="size">The size of the teapot.</param>
    /// <param name="tessellation">The tessellation of the teapot in the range [1, 18].</param>
    /// <returns>A new <see cref="Submesh"/> that represents a teapot.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better to call
    /// <see cref="GetTeapot"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="tessellation"/> is less than 1.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateTeapot(GraphicsDevice graphicsDevice, float size, int tessellation)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (tessellation < 1)
        throw new ArgumentOutOfRangeException("tessellation");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      var teapot = new Teapot(size, tessellation);

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        teapot.Vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(teapot.Vertices);

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        teapot.Indices.Length,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(teapot.Indices);

      submesh.PrimitiveCount = teapot.Indices.Length / 3;

      return submesh;
    }
  }
}
