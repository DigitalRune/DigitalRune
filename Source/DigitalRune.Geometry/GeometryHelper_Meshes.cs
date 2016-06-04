// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    // Handling of non-uniform scaling:
    // http://en.wikipedia.org/wiki/Scaling_(geometry) about non-uniform scaling: 
    // "Such a scaling changes ... the volume by the product of all three [scale factors]."


    /// <summary>
    /// Gets the contact of ray with a triangle mesh.
    /// </summary>
    /// <param name="triangleMesh">The triangle mesh.</param>
    /// <param name="ray">The ray.</param>
    /// <param name="hitDistance">
    /// The hit distance. This is the distance on the ray from the ray origin to the contact.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the ray hits the front face of a triangle mesh triangle; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method returns any contact, not necessarily the first contact of the ray with the
    /// triangle mesh! The mesh triangles are treated as one-sided.
    /// </remarks>
    internal static bool GetContact(ITriangleMesh triangleMesh, Ray ray, out float hitDistance)
    {
      for (int i = 0; i < triangleMesh.NumberOfTriangles; i++)
      {
        var triangle = triangleMesh.GetTriangle(i);
        bool hit = GetContact(ray, triangle, false, out hitDistance);
        if (hit)
          return true;
      }

      hitDistance = float.NaN;
      return false;
    }


    /// <summary>
    /// Gets the enclosed volume of a triangle mesh.
    /// </summary>
    /// <param name="triangleMesh">The triangle mesh.</param>
    /// <returns>
    /// The enclosed volume of the given triangle mesh.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method assumes that the given triangle mesh is a closed mesh without holes.
    /// </para>
    /// <para>
    /// Remember: To compute the volume of a scaled mesh, you can compute the volume of the
    /// unscaled mesh and multiply the result with the scaling factors: 
    /// </para>
    /// <para>
    /// <i>volume<sub>scaled</sub> = volume<sub>unscaled</sub> * scale<sub>X</sub> * scale<sub>Y</sub> * scale<sub>Z</sub></i>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="triangleMesh"/> is <see langword="null"/>.
    /// </exception>
    public static float GetVolume(this ITriangleMesh triangleMesh)
    {
      if (triangleMesh == null)
        throw new ArgumentNullException("triangleMesh");

      int numberOfTriangles = triangleMesh.NumberOfTriangles;
      if (numberOfTriangles == 0)
        return 0;

      // The reference points is on the first triangle. So the first tetrahedron has no volume.
      var p = triangleMesh.GetTriangle(0).Vertex0;
      float volume = 0;
      for (int i = 1; i < numberOfTriangles; i++)
      {
        var triangle = triangleMesh.GetTriangle(i);
        float tetrahedronVolume = GetSignedTetrahedronVolume(p, ref triangle);
        volume += tetrahedronVolume;
      }

      return volume;
    }


    // Computes the volume of a tretrahedron where point is the tip and (vertex0/1/2) is the
    // triangle base. The volume is negative if point is in front of the triangle face and 
    // positive if the point is in behind the triangle face.
    private static float GetSignedTetrahedronVolume(Vector3F point, ref Triangle triangle)
    {
      // See Game Programming Gems 6 - Chapter Buoyancy
      var a = triangle.Vertex1 - triangle.Vertex0;
      var b = triangle.Vertex2 - triangle.Vertex0;
      var r = point - triangle.Vertex0;

      float volume = 1.0f / 6.0f * Vector3F.Dot(Vector3F.Cross(b, a), r);
      return volume;
    }


    /// <summary>
    /// Creates a sphere (or hemisphere) by successively subdividing an icosahedron.
    /// </summary>
    /// <param name="subdivisions">The number of subdivisions. See remarks.</param>
    /// <param name="hemisphere">
    /// If set to <see langword="true"/> only the upper half (positive y) of the sphere is created;
    /// otherwise, the full sphere is created.
    /// </param>
    /// <returns>The triangle mesh of the sphere (or hemisphere).</returns>
    /// <remarks>
    /// <para>
    /// The resulting triangles use counter-clockwise vertex order. The triangles face outward, i.e.
    /// when looking at a triangle from the outside its vertices are ordered counter-clockwise.
    /// </para>
    /// <para>
    /// A minimum of 1 subdivision is required to create hemisphere. If you plan to render the mesh
    /// using a 16-bit index buffer then the max number of subdivisions is 5!
    /// <list type="bullet">
    /// <item><description>0 subdivisions: 20 triangles</description></item>
    /// <item><description>1 subdivisions: 240 triangles</description></item>
    /// <item><description>2 subdivisions: 320 triangles</description></item>
    /// <item><description>3 subdivisions: 1280 triangles</description></item>
    /// <item><description>4 subdivisions: 5120 triangles</description></item>
    /// <item><description>5 subdivisions: 20480 triangles</description></item>
    /// <item><description>6 subdivisions: 81920 triangles</description></item>
    /// <item><description>7 subdivisions: ...</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="subdivisions"/> is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="hemisphere"/> is <see langword="true"/>, but <paramref name="subdivisions"/>
    /// is less than 1. At least 1 subdivision is required to create a hemisphere.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static TriangleMesh CreateIcosphere(int subdivisions, bool hemisphere)
    {
      if (subdivisions < 0)
        throw new ArgumentOutOfRangeException("subdivisions", "The number of subdivisions must not be negative.");

      if (hemisphere && subdivisions < 1)
        throw new ArgumentException("The number of subdivisions must greater than 0 to create a hemisphere.");

      // The icosphere is created by subdividing an icosahedron.
      // (http://en.wikipedia.org/wiki/Icosahedron)

      var mesh = CreateIcosahedron();
      var tempMesh = new TriangleMesh();
      for (int s = 0; s < subdivisions; s++)
      {
        int numberOfTriangles = mesh.NumberOfTriangles;
        for (int i = 0; i < numberOfTriangles; i++)
        {
          // Split triangle into 4 triangles.
          Triangle triangle = mesh.GetTriangle(i);

          Vector3F v01 = (triangle.Vertex0 + triangle.Vertex1) / 2;
          Vector3F v12 = (triangle.Vertex1 + triangle.Vertex2) / 2;
          Vector3F v20 = (triangle.Vertex2 + triangle.Vertex0) / 2;

          tempMesh.Add(new Triangle(triangle.Vertex0, v01, v20));
          tempMesh.Add(new Triangle(v01, v12, v20));
          tempMesh.Add(new Triangle(v01, triangle.Vertex1, v12));
          tempMesh.Add(new Triangle(v20, v12, triangle.Vertex2));
        }

        MathHelper.Swap(ref mesh, ref tempMesh);
        tempMesh.Vertices.Clear();
        tempMesh.Indices.Clear();

        if (hemisphere && s == 0)
        {
          // Delete negative hemisphere after first subdivision.
          numberOfTriangles = mesh.NumberOfTriangles;
          for (int i = 0; i < numberOfTriangles; i++)
          {
            Triangle triangle = mesh.GetTriangle(i);
            if (Numeric.IsLess(triangle.Vertex0.Y, 0) || Numeric.IsLess(triangle.Vertex1.Y, 0) || Numeric.IsLess(triangle.Vertex2.Y, 0))
            {
              // Skip triangles in negative hemisphere.
              continue;
            }

            tempMesh.Add(triangle);
          }

          MathHelper.Swap(ref mesh, ref tempMesh);
          tempMesh.Vertices.Clear();
          tempMesh.Indices.Clear();
        }
      }

      mesh.WeldVertices();

      // The mesh is now a refined icosahedron. Adjust the position of the 
      // vertices to approximate a sphere.
      int numberOfVertices = mesh.Vertices.Count;
      for (int i = 0; i < numberOfVertices; i++)
      {
        Vector3F v = mesh.Vertices[i];

        // Unit sphere: ||v|| = 1
        v.Normalize();

        mesh.Vertices[i] = v;
      }

      return mesh;
    }


    private static TriangleMesh CreateIcosahedron()
    {
      var triangleMesh = new TriangleMesh();

      // 12 vertices
      var vertices = triangleMesh.Vertices;
      vertices.Add(new Vector3F(0.0f, 1.0f, 0.0f));
      vertices.Add(new Vector3F(0.894425f, 0.447215f, 0.0f));
      vertices.Add(new Vector3F(0.276385f, 0.447215f, -0.85064f));
      vertices.Add(new Vector3F(-0.7236f, 0.447215f, -0.52572f));
      vertices.Add(new Vector3F(-0.7236f, 0.447215f, 0.52572f));
      vertices.Add(new Vector3F(0.276385f, 0.447215f, 0.85064f));
      vertices.Add(new Vector3F(0.7236f, -0.447215f, -0.52572f));
      vertices.Add(new Vector3F(-0.276385f, -0.447215f, -0.85064f));
      vertices.Add(new Vector3F(-0.894425f, -0.447215f, 0.0f));
      vertices.Add(new Vector3F(-0.276385f, -0.447215f, 0.85064f));
      vertices.Add(new Vector3F(0.7236f, -0.447215f, 0.52572f));
      vertices.Add(new Vector3F(0.0f, -1.0f, 0.0f));

      // 20 faces
      var indices = triangleMesh.Indices;
      indices.Add(0); indices.Add(1); indices.Add(2);
      indices.Add(0); indices.Add(2); indices.Add(3);
      indices.Add(0); indices.Add(3); indices.Add(4);
      indices.Add(0); indices.Add(4); indices.Add(5);
      indices.Add(0); indices.Add(5); indices.Add(1);
      indices.Add(1); indices.Add(10); indices.Add(6);
      indices.Add(1); indices.Add(6); indices.Add(2);
      indices.Add(2); indices.Add(6); indices.Add(7);
      indices.Add(2); indices.Add(7); indices.Add(3);
      indices.Add(3); indices.Add(7); indices.Add(8);
      indices.Add(3); indices.Add(8); indices.Add(4);
      indices.Add(4); indices.Add(8); indices.Add(9);
      indices.Add(4); indices.Add(9); indices.Add(5);
      indices.Add(5); indices.Add(9); indices.Add(10);
      indices.Add(5); indices.Add(10); indices.Add(1);
      indices.Add(11); indices.Add(10); indices.Add(9);
      indices.Add(11); indices.Add(9); indices.Add(8);
      indices.Add(11); indices.Add(8); indices.Add(7);
      indices.Add(11); indices.Add(7); indices.Add(6);
      indices.Add(11); indices.Add(6); indices.Add(10);

      return triangleMesh;
    }


    #region ----- Vertex welding -----

    private struct WeldVertex
    {
      public Vector3F Position;  // Vertex position.
      public int OriginalIndex;  // Index in Vertices array.
      public float SortValue;    // Absolute component sum of position: |X|+|Y|+|Z|
      public int MergedIndex;    // >= 0 if this vertex was merged.

      public static int CompareSortValue(WeldVertex v0, WeldVertex v1)
      {
        return v0.SortValue.CompareTo(v1.SortValue);
      }

      public static int CompareOriginalIndex(WeldVertex v0, WeldVertex v1)
      {
        return v0.OriginalIndex.CompareTo(v1.OriginalIndex);
      }
    }


    /// <overloads>
    /// <summary>
    /// Merges duplicate positions.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Merges duplicate positions.
    /// </summary>
    /// <param name="positions">
    /// In: The positions.<br/>
    /// Out: The positions without duplicates.
    /// </param>
    /// <param name="positionTolerance">
    /// The position tolerance. If the distance between two positions is less than this value,
    /// the positions are merged.
    /// </param>
    /// <returns>The number of removed positions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="positions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="positionTolerance"/> is negative or 0.
    /// </exception>
    public static int MergeDuplicatePositions(IList<Vector3F> positions, float positionTolerance)
    {
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positionTolerance <= 0)
        throw new ArgumentOutOfRangeException("positionTolerance", "The position tolerance must be greater than 0.");

      int numberOfVertices = positions.Count;
      if (numberOfVertices <= 1)
        return 0;

      return MergeDuplicatePositions(positions, positionTolerance, null);
    }


    /// <summary>
    /// Merges duplicate positions.
    /// </summary>
    /// <param name="positions">
    /// In: The positions.<br/>
    /// Out: The positions without duplicates.
    /// </param>
    /// <param name="positionTolerance">
    /// The position tolerance. If the distance between two positions is less than this value,
    /// the positions are merged.
    /// </param>
    /// <param name="positionRemap">
    /// An array with one entry per position that describes how to reorder the original positions.
    /// This maps the original position index to the new position index. The array is
    /// <see langword="null"/> if no positions were removed.
    /// </param>
    /// <returns>The number of removed positions.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="positions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="positionTolerance"/> is negative or 0.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
    public static int MergeDuplicatePositions(IList<Vector3F> positions, float positionTolerance, out int[] positionRemap)
    {
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positionTolerance <= 0)
        throw new ArgumentOutOfRangeException("positionTolerance", "The position tolerance must be greater than 0.");

      int numberOfVertices = positions.Count;
      if (numberOfVertices <= 1)
      {
        positionRemap = null;
        return 0;
      }

      positionRemap = new int[numberOfVertices];
      return MergeDuplicatePositions(positions, positionTolerance, positionRemap);
    }


    private static int MergeDuplicatePositions(IList<Vector3F> positions, float positionTolerance, int[] positionRemap)
    {
      Debug.Assert(positions != null);
      Debug.Assert(positions.Count > 0);
      Debug.Assert(positionTolerance > 0);
      Debug.Assert(positionRemap == null || positionRemap.Length == positions.Count);

      // Create working data.
      int numberOfVertices = positions.Count;
      var data = new WeldVertex[numberOfVertices];
      for (int i = 0; i < numberOfVertices; i++)
      {
        Vector3F position = positions[i];
        data[i].Position = position;
        data[i].OriginalIndex = i;
        data[i].SortValue = Math.Abs(position.X) + Math.Abs(position.Y) + Math.Abs(position.Z);
        data[i].MergedIndex = -1;
      }

      // Sort positions by absolute component sum of position |X|+|Y|+|Z|.
      Array.Sort(data, WeldVertex.CompareSortValue);

      int numberOfMergedVertices = 0;

      // Loop over positions. Try to merge each vertex with the next positions in the array.
      for (int i = 0; i < numberOfVertices; i++)
      {
        // For positions that have already been merged, the index was set to > -1.
        if (data[i].MergedIndex >= 0)
        {
          // Vertex is a duplicate. - Nothing to do.
          continue;
        }

        // Now, we compare vertex i against the next positions in the array.
        for (int j = i + 1; j < numberOfVertices; j++)
        {
          // We can stop comparing if the SortValue differs by more than 3 * epsilon.
          if (data[j].SortValue - data[i].SortValue > 3 * positionTolerance)
            break;

          //if (Vector3F.AreNumericallyEqual(data[i].Position, data[j].Position, positionTolerance))
          // Optimized version: (Probably does not work for infinite float values!)
          float delta = Math.Abs(data[i].Position.X - data[j].Position.X);
          if (delta <= positionTolerance)
          {
            delta = Math.Abs(data[i].Position.Y - data[j].Position.Y);
            if (delta <= positionTolerance)
            {
              delta = Math.Abs(data[i].Position.Z - data[j].Position.Z);
              if (delta <= positionTolerance)
              {
                // Vertex positions are near each other and should be merged.
                numberOfMergedVertices++;
                data[j].MergedIndex = data[i].OriginalIndex;
              }
            }
          }
        }
      }

      if (numberOfMergedVertices == 0)
        return 0;

      // Sort by original index.
      Array.Sort(data, WeldVertex.CompareOriginalIndex);

      // Rebuild positions (omitting the merged positions).
      positions.Clear();
      if (positionRemap == null)
      {
        for (int i = 0; i < numberOfVertices; i++)
          if (data[i].MergedIndex < 0)
            positions.Add(data[i].Position);
      }
      else
      {
        // Rebuild positions and at the same time we fill the position remap table.
        for (int i = 0; i < numberOfVertices; i++)
        {
          if (data[i].MergedIndex < 0)
          {
            positionRemap[i] = positions.Count;
            positions.Add(data[i].Position);
          }
          else
          {
            positionRemap[i] = -1;
          }
        }

        // Now, fill in the other entries in the index redirection table.
        for (int i = 0; i < numberOfVertices; i++)
        {
          if (data[i].MergedIndex >= 0)
            positionRemap[i] = positionRemap[data[i].MergedIndex];

          Debug.Assert(positionRemap[i] != -1);
        }
      }

      return numberOfMergedVertices;
    }
    #endregion
  }
}
