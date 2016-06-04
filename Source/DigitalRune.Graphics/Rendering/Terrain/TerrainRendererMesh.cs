// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;


namespace DigitalRune.Graphics.Rendering
{
  /// <summary>
  /// Represents the mesh that is used by the <see cref="TerrainRenderer"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This object stores mesh data which is used by the <see cref="TerrainRenderer"/>. The mesh data
  /// is internal. The mesh data represents a geo-clipmap mesh. The mesh is defined by
  /// <see cref="NumberOfLevels"/> and <see cref="CellsPerLevel"/>. These parameters should match
  /// the resolution of the <see cref="TerrainNode.BaseClipmap"/> of the rendered
  /// <see cref="TerrainNode"/>.
  /// </para>
  /// <para>
  /// This mesh is automatically created by the <see cref="TerrainRenderer"/> when needed. However,
  /// the creation of the mesh can take up to several seconds. Therefore, it is possible to save the
  /// mesh to a file (see <see cref="Save"/>) and then load this file the next time (see
  /// <see cref="Load"/>). <see cref="TerrainRenderer.SetMesh"/> must be called to tell the 
  /// <see cref="TerrainRenderer"/> to use a manually loaded mesh.
  /// </para>
  /// </remarks>
  public sealed class TerrainRendererMesh : IDisposable
  {
    /*
      Load/Save test code

        try
        {
          var filename = GetFilename(trm.NumberOfLevels, trm.CellsPerLevel);
          var filePath = filename;
          using (var stream = File.OpenWrite(filePath))
            trm.Save(stream);

          using (var stream = File.OpenRead(filePath))
            trm = TerrainRendererMesh.Load(GraphicsService.GraphicsDevice, stream);
        }
        catch (Exception exception)
        {
          new GraphicsException("Could not cache terrain mesh.", exception);
        }
      
        _graphicsScreen.TerrainRenderer.SetMesh(trm);

        private static string GetFilename(int numberOfLevels, int cellsPerLevel)
        {
          StringBuilder sb = new StringBuilder("TerrainMesh");
          sb.Append(numberOfLevels);
          sb.Append("Levels");
          sb.Append(cellsPerLevel);
          sb.Append("Cells");
          return sb.ToString();
        }
    /*/


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed of.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance has been disposed of; otherwise,
    /// <see langword="false"/>.
    /// </value>
    public bool IsDisposed { get; private set; }


    /// <summary>
    /// Gets the number of cells (texels) per clipmap level.
    /// </summary>
    /// <value>The number of cells (texels) per clipmap level.</value>
    public int CellsPerLevel { get; private set; }


    /// <summary>
    /// Gets the number of clipmap levels.
    /// </summary>
    /// <value>The number of clipmap levels.</value>
    public int NumberOfLevels { get; private set; }


    internal Submesh Submesh { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainRendererMesh"/> class.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfLevels">The number of levels.</param>
    /// <param name="cellsPerLevel">The number of cells per level.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfLevels"/> or <paramref name="cellsPerLevel"/> is less than 1.
    /// </exception>
    public TerrainRendererMesh(GraphicsDevice graphicsDevice, int numberOfLevels, int cellsPerLevel)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (numberOfLevels < 1)
        throw new ArgumentOutOfRangeException("numberOfLevels", "The number of levels must be greater than 0.");
      if (cellsPerLevel < 1)
        throw new ArgumentOutOfRangeException("cellsPerLevel", "The number of cells per level must be greater than 0.");

      NumberOfLevels = numberOfLevels;
      CellsPerLevel = cellsPerLevel;
      Submesh = CreateGeoClipmapMesh(graphicsDevice, numberOfLevels, cellsPerLevel, false);
    }


    // Used in the Load method.
    private TerrainRendererMesh(int numberOfLevels, int cellsPerLevel, Submesh submesh)
    {
      NumberOfLevels = numberOfLevels;
      CellsPerLevel = cellsPerLevel;
      Submesh = submesh;
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting
    /// unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      IsDisposed = true;
      Submesh.Dispose();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Creates a geo-clipmap mesh for terrain rendering.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="numberOfLevels">The number of levels of detail.</param>
    /// <param name="cellsPerLevel">The width per level in cells.</param>
    /// <param name="useDiamondTessellation">
    /// If set to <see langword="true"/> the cells are tessellated using a additional vertices in
    /// the cell center. If set to <see langword="false"/>, each cell is represented by max. 2
    /// triangles.
    /// </param>
    /// <returns>The generated submesh.</returns>
    /// <remarks>
    /// The resulting mesh is similar to a grid with multiple levels of detail. The highest level of
    /// detail is in the center.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="cellsPerLevel"/> is less than 1.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    private static Submesh CreateGeoClipmapMesh(GraphicsDevice graphicsDevice, int numberOfLevels, 
                                         int cellsPerLevel, bool useDiamondTessellation)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (cellsPerLevel < 1)
        throw new ArgumentOutOfRangeException("cellsPerLevel", "The number of cells per level must be greater than 0.");

      // Round to even number of cells.
      if (cellsPerLevel % 2 != 0)
        cellsPerLevel++;

      // Allocate a mesh with conservative list capacities.
      int cellsPerLevelUpperBound = (cellsPerLevel + 3) * (cellsPerLevel + 3);
      int verticesPerLevelUpperBound = useDiamondTessellation ? cellsPerLevelUpperBound * 9 : cellsPerLevelUpperBound * 4;
      int indicesPerLevelUpperBound = useDiamondTessellation ? cellsPerLevelUpperBound * 8 * 3 : cellsPerLevelUpperBound * 2 * 3;
      var triangleMesh = new TriangleMesh(verticesPerLevelUpperBound * numberOfLevels, indicesPerLevelUpperBound * numberOfLevels);

      // The levels are created in a separate mesh and then combined into the final mesh.
      var triangleMeshes = new TriangleMesh[numberOfLevels];
      triangleMeshes[0] = triangleMesh;
      for (int i = 1; i < numberOfLevels; i++)
        triangleMeshes[i] = new TriangleMesh(verticesPerLevelUpperBound, indicesPerLevelUpperBound);

      //for (int level = 0; level < numberOfLevels; level++)
      Parallel.For(0, numberOfLevels, level =>
      {
        var levelTriangleMesh = triangleMeshes[level];
        int cellSize = (1 << level);
        float y = level;  // Store LOD in Y coordinate.

        Debug.Assert(cellsPerLevel % 2 == 0);

        int halfWidthInCells = cellsPerLevel / 2;
        int halfWidth = cellSize * (halfWidthInCells);
        int minInclusive = -halfWidth - cellSize;  // We add an extra border on the top and left.
        int maxInclusive = halfWidth - cellSize;   // Normal loop limit without extra border.

        int previousHalfWidth = (level > 0) ? halfWidth / 2 : -1;

        if (useDiamondTessellation)
        {
          #region ----- Diamond tessellation -----

          int index = 0;
          for (int z = minInclusive; z <= maxInclusive; z += cellSize)
          {
            for (int x = minInclusive; x <= maxInclusive; x += cellSize)
            {
              // No triangles in the area which are covered by previous levels.
              // (We compare cell centers with radius of last LOD.)
              if (Math.Max(Math.Abs(x + cellSize / 2), Math.Abs(z + cellSize / 2)) < previousHalfWidth)
                continue;

              // Each cell will be tessellated like this:
              //   A-----B-----C
              //   | \   |   / |
              //   |   \ | /   |
              //   D-----E-----F
              //   |   / | \   |
              //   | /   |   \ |
              //   G-----H-----I

              var indexA = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x, y, z));

              var indexB = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + 0.5f * cellSize, y, z));

              var indexC = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + cellSize, y, z));

              var indexD = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x, y, z + 0.5f * cellSize));

              var indexE = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + 0.5f * cellSize, y, z + 0.5f * cellSize));

              var indexF = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + cellSize, y, z + 0.5f * cellSize));

              var indexG = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x, y, z + cellSize));

              var indexH = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + 0.5f * cellSize, y, z + cellSize));

              var indexI = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + cellSize, y, z + cellSize));

              // Triangles using ADEG:
              if (x != minInclusive)
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexD);
                levelTriangleMesh.Indices.Add(indexA);

                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexG);
                levelTriangleMesh.Indices.Add(indexD);
              }
              else
              {
                // The outer cells are tessellated differently to stitch to the next level.
                //   A-----B-----C
                //   | \   |   / |
                //   |   \ | /   |
                //   |     E-----F
                //   |   / | \   |
                //   | /   |   \ |
                //   G-----H-----I
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexG);
                levelTriangleMesh.Indices.Add(indexA);
              }

              // Triangles using ABCE:
              if (z != minInclusive)
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexB);
                levelTriangleMesh.Indices.Add(indexC);

                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexA);
                levelTriangleMesh.Indices.Add(indexB);
              }
              else
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexA);
                levelTriangleMesh.Indices.Add(indexC);
              }

              // Triangles using CEFI:
              if (x != maxInclusive)
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexF);
                levelTriangleMesh.Indices.Add(indexI);

                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexC);
                levelTriangleMesh.Indices.Add(indexF);
              }
              else
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexC);
                levelTriangleMesh.Indices.Add(indexI);
              }

              // Triangles using EGHI:
              if (z != maxInclusive)
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexH);
                levelTriangleMesh.Indices.Add(indexG);

                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexI);
                levelTriangleMesh.Indices.Add(indexH);
              }
              else
              {
                levelTriangleMesh.Indices.Add(indexE);
                levelTriangleMesh.Indices.Add(indexI);
                levelTriangleMesh.Indices.Add(indexG);
              }
            }
          }

          Debug.Assert(levelTriangleMesh.Vertices.Count <= verticesPerLevelUpperBound, "Bad estimate for upper bound of vertices.");
          Debug.Assert(levelTriangleMesh.Indices.Count <= indicesPerLevelUpperBound, "Bad estimate for upper bound of indices.");

          levelTriangleMesh.WeldVertices(0.1f);
          #endregion
        }
        else
        {
          #region ----- Simple tessellation -----

          // Add one extra border to hide gaps.
          minInclusive -= cellSize;
          maxInclusive += cellSize;

          int index = 0;
          for (int z = minInclusive; z <= maxInclusive; z += cellSize)
          {
            for (int x = minInclusive; x <= maxInclusive; x += cellSize)
            {
              // No triangles in the area which are covered by previous levels.
              // (We compare cell centers with radius of last LOD.)
              if (Math.Max(Math.Abs(x + cellSize / 2), Math.Abs(z + cellSize / 2)) < previousHalfWidth)
                continue;

              // Each 2x2 cells will be tessellated like this:
              //   A-----B
              //   |   / |
              //   | /   |
              //   C-----D

              int indexA = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x, y, z));

              int indexB = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + cellSize, y, z));

              int indexC = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x, y, z + cellSize));

              int indexD = index++;
              levelTriangleMesh.Vertices.Add(new Vector3F(x + cellSize, y, z + cellSize));

              levelTriangleMesh.Indices.Add(indexA);
              levelTriangleMesh.Indices.Add(indexB);
              levelTriangleMesh.Indices.Add(indexC);

              levelTriangleMesh.Indices.Add(indexC);
              levelTriangleMesh.Indices.Add(indexB);
              levelTriangleMesh.Indices.Add(indexD);
            }
          }

          Debug.Assert(levelTriangleMesh.Vertices.Count <= verticesPerLevelUpperBound, "Bad estimate for upper bound of vertices.");
          Debug.Assert(levelTriangleMesh.Indices.Count <= indicesPerLevelUpperBound, "Bad estimate for upper bound of indices.");

          levelTriangleMesh.WeldVertices(0.1f);
          #endregion
        }
      });

      // Combine meshes.
      for (int i = 1; i < numberOfLevels; i++)
        triangleMesh.Add(triangleMeshes[i]);

      var vertices = new TerrainVertex[triangleMesh.Vertices.Count];
      {
        int index = 0;
        for (int i = 0; i < vertices.Length; i++)
        {
          Vector3F v = triangleMesh.Vertices[i];
          vertices[index++] = new TerrainVertex(new HalfVector4(v.X, v.Y, v.Z, 1));
        }
      }

      var submesh = CreateSubmesh(graphicsDevice, vertices, triangleMesh.Indices.ToArray());

      //Debug.WriteLine("Number of terrain vertices:" + vertices.Length);
      //Debug.WriteLine("Number of terrain triangles:" + triangleMesh.Indices.Count / 3);

      return submesh;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    private static Submesh CreateSubmesh(GraphicsDevice graphicsDevice, TerrainVertex[] vertices, Array indices)
    {
      var submesh = new Submesh();

      submesh.VertexBuffer = new VertexBuffer(
        graphicsDevice,
        TerrainVertex.VertexDeclaration,
        vertices.Length,
        BufferUsage.None);
      submesh.VertexBuffer.SetData(vertices);
      submesh.VertexCount = submesh.VertexBuffer.VertexCount;

      // Build array of indices.
      if (vertices.Length <= ushort.MaxValue)
      {
        var indicesUInt16 = indices as ushort[];
        if (indicesUInt16 == null)
        {
          var indicesInt32 = (int[])indices;
          indicesUInt16 = new ushort[indices.Length];
          for (int i = 0; i < indicesUInt16.Length; i++)
            indicesUInt16[i] = (ushort)indicesInt32[i];
        }

        submesh.IndexBuffer = new IndexBuffer(
          graphicsDevice,
          IndexElementSize.SixteenBits,
          indicesUInt16.Length,
          BufferUsage.None);
        submesh.IndexBuffer.SetData(indicesUInt16);
      }
      else
      {
        var indicesInt32 = (int[])indices;
        submesh.IndexBuffer = new IndexBuffer(
          graphicsDevice,
          IndexElementSize.ThirtyTwoBits,
          indicesInt32.Length,
          BufferUsage.None);
        submesh.IndexBuffer.SetData(indicesInt32);
      }

      submesh.PrimitiveType = PrimitiveType.TriangleList;
      submesh.PrimitiveCount = indices.Length / 3;
      return submesh;
    }


    /// <summary>
    /// Loads a <see cref="TerrainRendererMesh"/> from the specified stream.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="stream">The stream.</param>
    /// <returns>
    /// The <see cref="TerrainRendererMesh"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    public static TerrainRendererMesh Load(GraphicsDevice graphicsDevice, Stream stream)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (stream == null)
        throw new ArgumentNullException("stream");

      var reader = new BinaryReader(stream);

      // Version
      reader.ReadInt32();

      // Number of levels and cells per level.
      int numberOfLevels = reader.ReadInt32();
      int cellsPerLevel = reader.ReadInt32();

      // Number of vertices
      int vertexCount = reader.ReadInt32();

      // Vertices
      var vertices = new TerrainVertex[vertexCount];
      for (int i = 0; i < vertices.Length; i++)
      {
        vertices[i].Position = new HalfVector4();
        vertices[i].Position.PackedValue = reader.ReadUInt64();
      }

      // Number of indices.
      int indexCount = reader.ReadInt32();

      // Indices
      Submesh submesh;
      if (vertexCount <= ushort.MaxValue)
      {
        var indices = new ushort[indexCount];
        for (int i = 0; i < indices.Length; i++)
          indices[i] = reader.ReadUInt16();

        submesh = CreateSubmesh(graphicsDevice, vertices, indices);
      }
      else
      {
        var indices = new int[indexCount];
        for (int i = 0; i < indices.Length; i++)
          indices[i] = reader.ReadInt32();

        submesh = CreateSubmesh(graphicsDevice, vertices, indices);
      }

      return new TerrainRendererMesh(numberOfLevels, cellsPerLevel, submesh);
    }


    /// <summary>
    /// Writes the mesh data to the specified stream.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    public void Save(Stream stream)
    {
      if (stream == null)
        throw new ArgumentNullException("stream");

      var writer = new BinaryWriter(stream);

      // Version
      writer.Write(0);

      // Number of levels and cells per level.
      writer.Write(NumberOfLevels);
      writer.Write(CellsPerLevel);

      // Number of vertices
      int vertexCount = Submesh.VertexCount;
      writer.Write(vertexCount);

      // Vertices
      var vertices = new TerrainVertex[vertexCount];
      Submesh.VertexBuffer.GetData(vertices);

      for (int i = 0; i < vertices.Length; i++)
        writer.Write(vertices[i].Position.PackedValue);

      // Number of indices.
      int indexCount = Submesh.PrimitiveCount * 3;
      writer.Write(indexCount);

      // Indices
      if (vertexCount <= ushort.MaxValue)
      {
        var indices = new ushort[indexCount];
        Submesh.IndexBuffer.GetData(indices);

        for (int i = 0; i < indexCount; i++)
          writer.Write(indices[i]);
      }
      else
      {
        var indices = new int[indexCount];
        Submesh.IndexBuffer.GetData(indices);

        for (int i = 0; i < indexCount; i++)
          writer.Write(indices[i]);
      }

      writer.Flush();
    }
    #endregion
  }
}
