// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper 
  {
    /// <summary>
    /// Gets the default submesh that represents a circle using lines.
    /// (The circle lies in the xy plane and is centered at the origin. Radius = 1.)
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of circle line list. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateCircleLines"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetCircleLines(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Circle";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateCircleLines(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a circle using lines.
    /// (The circle lies in the xy plane and is centered at the origin. Radius = 1.)
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a circle line list.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetCircleLines"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateCircleLines(GraphicsDevice graphicsDevice, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.LineList,
      };

      // Create vertices for a circle on the floor.
      var vertices = new Vector3F[numberOfSegments];
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;

        float x = (float)Math.Cos(angle);
        float y = (float)Math.Sin(angle);
        vertices[i] = new Vector3F(x, y, 0);
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices);
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      // Create indices for base circle.
      var indices = new ushort[2 * numberOfSegments];
      for (int i = 0; i < numberOfSegments; i++)
      {
        indices[2 * i] = (ushort)i;
        indices[2 * i + 1] = (ushort)(i + 1);
      }

      // Correct last index to be 0 to close circle.
      indices[2 * numberOfSegments - 1] = 0;

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
