// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Graphics
{
  partial class MeshHelper
  {
    /// <summary>
    /// Creates a new submesh that represents a rectangular patch on a sphere using triangles.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="originRadius">
    /// The radius where the origin of the submesh is placed. See remarks.
    /// </param>
    /// <param name="sphereRadius">
    /// The radius of the sphere.
    /// </param>
    /// <param name="numberOfDivisions">
    /// The number of divisions in the range [0, 255]. This parameter controls the detail of the
    /// mesh.
    /// </param>    
    /// <returns>A new <see cref="Submesh"/> that represents a sphere.</returns>
    /// <remarks>
    /// <para>
    /// The returned mesh is like a quad in the xz plane where the vertices are pulled down over a
    /// sphere to match  the curvature. This mesh is useful for cloud planes and sky domes.
    /// </para>
    /// <para>
    /// The patch center is centered in the xz plane and under the patch. 
    /// <paramref name="originRadius"/> defines relative y positions of the dome, for example: If
    /// this mesh is used for a cloud plane, set <paramref name="originRadius"/> to the earth radius
    /// and <paramref name="sphereRadius"/> to the cloud plane radius. 
    /// </para>
    /// <para>
    /// The returned <see cref="Submesh"/> will have textured coordinates.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfDivisions"/> is less than or equal to 2.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    internal static Submesh CreateSpherePatch(
      GraphicsDevice graphicsDevice, 
      float originRadius, float sphereRadius, 
      int numberOfDivisions)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfDivisions < 1 ||numberOfDivisions > 255)
        throw new ArgumentOutOfRangeException("numberOfDivisions", "numberOfDivisions must be in the range [0, 255].");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      int numberOfVertices = (numberOfDivisions + 1) * (numberOfDivisions + 1);
      var vertices = new VertexPositionNormalTexture[numberOfVertices];

      float groundPlaneSize = 2 * (float)Math.Sqrt(sphereRadius * sphereRadius - originRadius * originRadius);
      int index = 0;
      for (int i = 0; i <= numberOfDivisions; i++)
      {
        for (int j = 0; j <= numberOfDivisions; j++)
        {
          float x = -groundPlaneSize / 2 + groundPlaneSize / numberOfDivisions * j;
          float z = -groundPlaneSize / 2 + groundPlaneSize / numberOfDivisions * i;

          //float groundDist2 = x * x + z * z;

          var direction = new Vector3(x, originRadius, z);
          direction.Normalize();

          var p = direction * sphereRadius - new Vector3(0, originRadius, 0);

          var t = new Vector2(
            j / (float)numberOfDivisions,
            i / (float)numberOfDivisions);

          vertices[index++] = new VertexPositionNormalTexture(p, new Vector3(0, -1, 0),  t);
        }
      }

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        VertexPositionNormalTexture.VertexDeclaration,
        vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices);
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      // Build array of indices.
      int numberOfTriangles = numberOfDivisions * numberOfDivisions * 2;
      int numberOfIndices = 3 * numberOfTriangles;

      var indices = new ushort[numberOfIndices];

      index = 0;
      for (int i = 0; i < numberOfDivisions; i++)
      {
        for (int j = 0; j < numberOfDivisions; j++)
        {
          int baseIndex = i * (numberOfDivisions + 1) + j;

          indices[index++] = (ushort)baseIndex;
          indices[index++] = (ushort)(baseIndex + numberOfDivisions + 1);
          indices[index++] = (ushort)(baseIndex + 1);
          
          indices[index++] = (ushort)(baseIndex + 1);
          indices[index++] = (ushort)(baseIndex + numberOfDivisions + 1);
          indices[index++] = (ushort)(baseIndex + numberOfDivisions + 2);          
        }
      }

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
