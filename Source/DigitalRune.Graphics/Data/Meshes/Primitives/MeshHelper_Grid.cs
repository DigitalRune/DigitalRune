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
    /// Creates a new submesh that represents a rectangular grid in the xy plane.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="widthX">
    /// The total width of the grid in x.
    /// </param>
    /// <param name="widthY">
    /// The total width of the grid in y.
    /// </param>
    /// <param name="numberOfCellsX">
    /// The number of grid cells in the x direction.
    /// </param>
    /// <param name="numberOfCellsY">
    /// The number of grid cells in the y direction.
    /// </param>
    /// <returns>A new <see cref="Submesh"/> that represents the grid.</returns>
    /// <remarks>
    /// <para>
    /// The returned mesh represents a tessellated grid in the xy plane. The grid goes from (0, 0)
    /// to (widthX, withY). The returned mesh has texture coordinates which go from (0, 0) to
    /// (1, 1).
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="widthX"/> or <paramref name="widthY"/> is 0 or negative.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfCellsX"/> or <paramref name="numberOfCellsY"/> is less than 1
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    internal static Submesh CreateGrid(GraphicsDevice graphicsDevice, float widthX, float widthY, int numberOfCellsX, int numberOfCellsY)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (widthX <= 0)
        throw new ArgumentOutOfRangeException("widthX", "The number of cells must be greater than 0.");
      if (widthY <= 0)
        throw new ArgumentOutOfRangeException("widthY", "The number of cells must be greater than 0.");
      if (numberOfCellsX < 1)
        throw new ArgumentOutOfRangeException("numberOfCellsX", "The number of cells must be greater than 0.");
      if (numberOfCellsY < 1)
        throw new ArgumentOutOfRangeException("numberOfCellsY", "The number of cells must be greater than 0.");

      var submesh = new Submesh
      {
        PrimitiveType = PrimitiveType.TriangleList,
      };

      int numberOfVertices = (numberOfCellsX + 1) * (numberOfCellsY + 1);
      var vertices = new VertexPositionNormalTexture[numberOfVertices];

      int index = 0;
      for (int y = 0; y <= numberOfCellsY; y++)
      {
        for (int x = 0; x <= numberOfCellsX; x++)
        {
          vertices[index++] = new VertexPositionNormalTexture(
            new Vector3(x * widthX / numberOfCellsX, y * widthY / numberOfCellsY, 0),
            new Vector3(0, 0, 1),
            new Vector2(x / (float)numberOfCellsX, y / (float)numberOfCellsY));
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
      int numberOfTriangles = numberOfCellsX * numberOfCellsY * 2;
      int numberOfIndices = 3 * numberOfTriangles;

      if (numberOfIndices < ushort.MaxValue)
      {
        var indices = new ushort[numberOfIndices];

        index = 0;
        for (int y = 0; y < numberOfCellsY; y++)
        {
          for (int x = 0; x < numberOfCellsX; x++)
          {
            int baseIndex = y * (numberOfCellsX + 1) + x;

            indices[index++] = (ushort)baseIndex;
            indices[index++] = (ushort)(baseIndex + numberOfCellsX + 1);
            indices[index++] = (ushort)(baseIndex + 1);

            indices[index++] = (ushort)(baseIndex + 1);
            indices[index++] = (ushort)(baseIndex + numberOfCellsX + 1);
            indices[index++] = (ushort)(baseIndex + numberOfCellsX + 2);
          }
        }

        submesh.IndexBuffer = new IndexBuffer(
          graphicsDevice,
          IndexElementSize.SixteenBits,
          indices.Length,
          BufferUsage.None);
        submesh.IndexBuffer.SetData(indices);
        submesh.PrimitiveCount = indices.Length / 3;
      }
      else
      {
        var indices = new int[numberOfIndices];

        index = 0;
        for (int y = 0; y < numberOfCellsY; y++)
        {
          for (int x = 0; x < numberOfCellsX; x++)
          {
            int baseIndex = y * (numberOfCellsX + 1) + x;

            indices[index++] = baseIndex;
            indices[index++] = (baseIndex + numberOfCellsX + 1);
            indices[index++] = (baseIndex + 1);

            indices[index++] = (baseIndex + 1);
            indices[index++] = (baseIndex + numberOfCellsX + 1);
            indices[index++] = (baseIndex + numberOfCellsX + 2);
          }
        }

        submesh.IndexBuffer = new IndexBuffer(
          graphicsDevice,
          IndexElementSize.ThirtyTwoBits,
          indices.Length,
          BufferUsage.None);
        submesh.IndexBuffer.SetData(indices);
        submesh.PrimitiveCount = indices.Length / 3;
      }

      return submesh;
    }
  }
}
