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
    /// Gets the default submesh that represents a spherical cap using lines.
    /// (The sphere is centered at the origin. Radius = 1. The submesh contains only the 
    /// top half (+y) of the sphere.) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of a hemisphere line list. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateHemisphereLines"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetHemisphereLines(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__HemisphereLines";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateHemisphereLines(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a spherical cap using lines.
    /// (The sphere is centered at the origin. Radius = 1. The submesh contains only the 
    /// top half (+y) of the sphere.) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a hemisphere line list.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetHemisphereLines"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateHemisphereLines(GraphicsDevice graphicsDevice, int numberOfSegments)
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
      var vertices = new List<Vector3F>();
      for (int i = 0; i < numberOfSegments; i++)
      {
        float angle = i * ConstantsF.TwoPi / numberOfSegments;
        vertices.Add(new Vector3F((float)Math.Cos(angle), 0, -(float)Math.Sin(angle)));
      }

      // Top vertex of the sphere.
      var topVertexIndex = vertices.Count;
      vertices.Add(new Vector3F(0, 1, 0));

      // 4 quarter arcs. Each arc starts at the base circle and ends at the top vertex. We already
      // have the first and last vertex.
      // Arc from +x to top.
      int firstArcIndex = vertices.Count;
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        float angle = (i + 1) * ConstantsF.TwoPi / numberOfSegments;
        vertices.Add(new Vector3F((float)Math.Cos(angle), (float)Math.Sin(angle), 0));
      }

      // Arc from -z to top. (Copy from first arc.)
      int secondArcIndex = vertices.Count;
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        Vector3F p = vertices[firstArcIndex + i];
        vertices.Add(new Vector3F(0, p.Y, -p.X));
      }

      // Arc from -x to top. (Copy from first arc.)
      int thirdArcIndex = vertices.Count;
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        Vector3F p = vertices[firstArcIndex + i];
        vertices.Add(new Vector3F(-p.X, p.Y, 0));
      }

      // Arc from +z to top. (Copy from first arc.)
      int fourthArcIndex = vertices.Count;
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        Vector3F p = vertices[firstArcIndex + i];
        vertices.Add(new Vector3F(0, p.Y, p.X));
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPosition.VertexDeclaration,
        vertices.Count,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices.ToArray());

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      var indices = new List<ushort>();

      // Create indices for base circle.
      for (int i = 0; i < numberOfSegments; i++)
      {
        indices.Add((ushort)i);          // Line start (= same as previous line end)
        indices.Add((ushort)(i + 1));    // Line end
      }

      // Correct last index to be 0 to close circle.
      indices[(ushort)(2 * numberOfSegments - 1)] = 0;

      // Indices for first arc.
      indices.Add(0);                             // Line start
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        indices.Add((ushort)(firstArcIndex + i));  // Line end
        indices.Add((ushort)(firstArcIndex + i));  // Line start (= same as previous line end)
      }
      indices.Add((ushort)topVertexIndex);         // Line end

      // Next arcs
      indices.Add((ushort)(numberOfSegments / 4));
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        indices.Add((ushort)(secondArcIndex + i));
        indices.Add((ushort)(secondArcIndex + i));
      }
      indices.Add((ushort)topVertexIndex);

      indices.Add((ushort)(2 * numberOfSegments / 4));
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        indices.Add((ushort)(thirdArcIndex + i));
        indices.Add((ushort)(thirdArcIndex + i));
      }
      indices.Add((ushort)topVertexIndex);

      indices.Add((ushort)(3 * numberOfSegments / 4));
      for (int i = 0; i < numberOfSegments / 4 - 1; i++)
      {
        indices.Add((ushort)(fourthArcIndex + i));
        indices.Add((ushort)(fourthArcIndex + i));
      }
      indices.Add((ushort)topVertexIndex);

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Count,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices.ToArray());

      submesh.PrimitiveCount = indices.Count / 2;

      return submesh;
    }


    //private void RenderHemicircle(float radius, ref Vector3F center, ref Vector3F right, ref Vector3F normal, ref Color color)
    //{
    //  var numberOfSegments = 16;

    //  var start = center + radius * right;
    //  for (int i = 1; i <= numberOfSegments; i++)
    //  {
    //    var angle = i * ConstantsF.Pi / numberOfSegments;

    //    var end = center + radius * Matrix33F.CreateRotation(normal, angle) * right;
    //    _lineRenderer.Lines.Add(new LineBatch.Line(start, end, color));
    //    start = end;
    //  }
    //}
  }
}
