// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Gets the default submesh that represents a spherical cap using triangles.
    /// (The sphere is centered at the origin. Radius = 1. The submesh contains only the 
    /// top half (+y) of the sphere.) 
    /// </summary>
    /// <param name="graphicsService">The graphics service.</param>
    /// <returns>
    /// The default submesh of a hemisphere. 
    /// This submesh is shared and must not be modified!
    /// </returns>
    /// <remarks>
    /// The returned <see cref="Submesh"/> is a shared instance that must not be modified. 
    /// Use <see cref="CreateHemisphere"/> to create a new <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsService"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh GetHemisphere(IGraphicsService graphicsService)
    {
      if (graphicsService == null)
        throw new ArgumentNullException("graphicsService");

      const string key = "__Hemisphere";
      object submesh;
      if (!graphicsService.Data.TryGetValue(key, out submesh))
      {
        submesh = CreateHemisphere(graphicsService.GraphicsDevice, 32);
        graphicsService.Data[key] = submesh;
      }

      return (Submesh)submesh;
    }


    /// <summary>
    /// Creates a new submesh that represents a spherical cap using triangles.
    /// (The sphere is centered at the origin. Radius = 1. The submesh contains only the 
    /// top half (+y) of the sphere.) 
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfSegments">
    /// The number of segments. This parameter controls the detail of the mesh.</param>
    /// <returns>A new <see cref="Submesh"/> that represents a hemisphere.</returns>
    /// <remarks>
    /// If the returned <see cref="Submesh"/> is not going to be modified, then it is better
    /// to call <see cref="GetHemisphere"/> to retrieve a shared <see cref="Submesh"/> instance.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfSegments"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static Submesh CreateHemisphere(GraphicsDevice graphicsDevice, int numberOfSegments)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfSegments < 3)
        throw new ArgumentOutOfRangeException("numberOfSegments", "numberOfSegments must be greater than 2");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      // The number of rings.
      int numberOfRings = numberOfSegments / 4;

      int numberOfVertices = numberOfSegments * numberOfRings + 1;
      var vertices = new VertexPositionNormal[numberOfVertices];

      // Create rings.
      float angle = ConstantsF.TwoPi / numberOfSegments;

      // Next free index in vertices.
      int i = 0;

      // Top vertex.
      vertices[i++] = new VertexPositionNormal(new Vector3(0, 1, 0), new Vector3(0, 1, 0));

      // Compute vertices for rings from pole to equator and from the x-axis in
      // counterclockwise direction (when viewed from top).
      for (int ring = 0; ring < numberOfRings; ring++)
      {
        float upAngle = angle * (ring + 1);
        float y = (float)Math.Cos(upAngle);
        float ringRadius = (float)Math.Sin(upAngle);

        for (int segment = 0; segment < numberOfSegments; segment++)
        {
          float x = ringRadius * (float)Math.Cos(angle * segment);
          float z = ringRadius * (float)Math.Sin(angle * segment);
          vertices[i++] = new VertexPositionNormal(new Vector3(x, y, z), new Vector3(x, y, z));
        }
      }

      Debug.Assert(i == numberOfVertices);

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormal.VertexDeclaration,
        vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices);

      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      // Build array of indices.
      int numberOfTriangles = numberOfSegments  // Triangles in top cap.
                                    + numberOfSegments * 2 * (numberOfRings - 1);
      int numberOfIndices = 3 * numberOfTriangles;

      var indices = new ushort[numberOfIndices];
      i = 0;

      // Indices for top cap.
      for (int segment = 0; segment < numberOfSegments; segment++)
      {
        indices[i++] = 0;
        indices[i++] = (ushort)(segment + 1);
        if (segment + 1 < numberOfSegments)
          indices[i++] = (ushort)(segment + 2);
        else
          indices[i++] = 1; // Wrap around to first vertex of the first ring.
      }

      // Indices for rings between the caps.
      for (int ring = 1; ring < numberOfRings; ring++)
      {
        for (int segment = 0; segment < numberOfSegments; segment++)
        {
          // Each segment has 2 triangles.
          if (segment + 1 < numberOfSegments)
          {
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments + segment);
            indices[i++] = (ushort)(1 + ring * numberOfSegments + segment);
            indices[i++] = (ushort)(1 + ring * numberOfSegments + segment + 1);

            indices[i++] = (ushort)(1 + ring * numberOfSegments + segment + 1);
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments + segment + 1);
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments + segment);
          }
          else
          {
            // Handle wrap around.
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments + segment);
            indices[i++] = (ushort)(1 + ring * numberOfSegments + segment);
            indices[i++] = (ushort)(1 + ring * numberOfSegments);

            indices[i++] = (ushort)(1 + ring * numberOfSegments);
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments);
            indices[i++] = (ushort)(1 + (ring - 1) * numberOfSegments + segment);
          }
        }
      }

      Debug.Assert(i == numberOfIndices);

      submesh.IndexBuffer = new IndexBuffer(
        graphicsDevice,
        IndexElementSize.SixteenBits,
        indices.Length,
        BufferUsage.None);
      submesh.IndexBuffer.SetData(indices);

      submesh.PrimitiveCount = indices.Length / 3;

      return submesh;
    }
  }
}
