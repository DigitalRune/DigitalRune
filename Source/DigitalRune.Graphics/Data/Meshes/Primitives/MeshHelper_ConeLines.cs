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
    /// Gets the default submesh that represents a cone using lines.
    /// (The cone is standing on the xz plane pointing along the y axis. Radius = 1. Height = 1.) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of cone line list. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateConeLines"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetConeLines(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__ConeLines";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateConeLines(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a cone using lines.
    /// (The cone is standing on the xz plane pointing along the y axis. Radius = 1. Height = 1.) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a cone line list.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetConeLines"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateConeLines(GraphicsDevice graphicsDevice, int numberOfSegments)
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

      // Base
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        float x = (float)Math.Cos(angle);
        float z = -(float)Math.Sin(angle);
        vertices.Add(new Vector3F(x, 0, z));
      }

      // Tip
      vertices.Add(new Vector3F(0, 1, 0));

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;
      
      var indices = new List<ushort>();

      // Base circle.
      for (int i = 0; i < numberOfSegments - 1; i++)
      {
        indices.Add((ushort)i);          // Line start (= same as previous line end)
        indices.Add((ushort)(i + 1));    // Line end
      }

      // Last line of base circle.
      indices.Add((ushort)(numberOfSegments - 1));
      indices.Add(0);

      // Side represented by 4 lines.
      for (int i = 0; i < 4; i++)
      {
        indices.Add((ushort)(i * numberOfSegments / 4));
        indices.Add((ushort)numberOfSegments);
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
