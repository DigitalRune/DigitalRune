#region ----- Copyright -----
/*
  This is a port of DirectXMesh (see http://directxmesh.codeplex.com/) which is licensed under the
  MIT license.


  Copyright (c) 2015 Microsoft Corp

  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
  associated documentation files (the "Software"), to deal in the Software without restriction,
  including without limitation the rights to use, copy, modify, merge, publish, distribute,
  sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in all copies or
  substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
  NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
  NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
  DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
  OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Defines the algorithm for calculating vertex normals.
  /// </summary>
  internal enum VertexNormalAlgorithm
  {
    /// <summary>
    /// Compute normals using the <i>"mean weighted by angle"</i> algorithm (default).
    /// </summary>
    WeightedByAngle,

    /// <summary>
    /// Compute normals using <i>"mean weighted by areas of adjacent triangles"</i> algorithm.
    /// </summary>
    WeightedByArea,

    /// <summary>
    /// Compute normals using <i>"mean weighted equally"</i> algorithm.
    /// </summary>
    WeightedEqually
  };


  /// <summary>
  /// Defines options for mesh validation.
  /// </summary>
  [Flags]
  internal enum MeshValidationOptions
  {
    /// <summary>
    /// Check for most basic problems such as invalid index entries. If adjacency information is
    /// provided, then that list is also validated.
    /// </summary>
    Default = 0x0,

    /// <summary>
    /// Check for duplicate neighbors which usually indicate inconsistent winding order. This
    /// requires adjacency information.
    /// </summary>
    Backfacing = 0x1,

    /// <summary>
    /// Check for two fans of triangles that use the same vertex, but are not adjacent. This
    /// requires adjacency information.
    /// </summary>
    Bowties = 0x2,

    /// <summary>Check for degenerate triangles.</summary>
    Degenerate = 0x4,

    /// <summary>
    /// Check for issues with 'unused' triangles such as partial 'unused' faces. If adjacency is
    /// provided, it also validates that 'unused' faces are not neighbors of other faces.
    /// </summary>
    Unused = 0x8,

    /// <summary>
    /// Check that every neighbor face links back to the original face. This requires adjacency.
    /// </summary>
    AsymmetricAdjacency = 0x10
  };


  /// <summary>
  /// Provides various geometry processing functions.
  /// </summary>
  /// <remarks>
  /// <para>
  /// <see href="https://directxmesh.codeplex.com/">DirectXMesh</see>.
  /// Copyright (c) 2015 Microsoft Corp. Available under
  /// <see href="https://directxmesh.codeplex.com/license">MIT license</see>.
  /// </para>
  /// <para>
  /// This is a port of the D3DX geometry processing functions.
  /// </para>
  /// <para>
  /// <strong>Normals, tangents, and bi-tangents:</strong><br/>
  /// Geometric meshes often must include surface information for lighting computations.
  /// </para>
  /// <para>
  /// A <i>normal</i> is defined as surface normal perpendicular to the mesh. Triangular meshes
  /// imply a face normal defined by the winding order of the vertices of the triangles (typically
  /// counter-clockwise, although clockwise winding can also be used). For efficient rendering,
  /// vertex normals are used for rendering rather than face normals.
  /// </para>
  /// <para>
  /// A tangent and a bi-tangent are gradient direction vectors along the surface of a mesh.
  /// Together with the normal they describe a full 3-axis coordinate system useful for many
  /// advanced lighting techniques.
  /// </para>
  /// <para>
  /// See: <see cref="ComputeNormals"/>, <see cref="ComputeTangentFrame(IList{int},IList{Vector3F},IList{Vector3F},IList{Vector2F},out Vector3F[],out Vector3F[])"/>
  /// </para>
  /// <para>
  /// <strong>Adjacency computation:</strong><br/>
  /// A useful property of geometric meshes is the adjacency relationship between faces of the mesh 
  /// as defined by sharing triangle edges.
  /// </para>
  /// <para>
  /// The adjacency is represented as an array of <see cref="Int32"/> index values with 3 entries
  /// for each triangular face in the mesh. Each entry is the face index of the triangular face that
  /// is adjacent to one edge of the triangular face (hence as there are three edges, there can be
  /// up to 3 triangular faces that share an edge). If there is no adjacent face, the index is set
  /// to -1.
  /// </para>
  /// <para>
  /// The point representative (aka point rep) is an array of <see cref="Int32"/> index values with
  /// one entry for each vertex in the mesh. Each entry is the vertex index of a vertex with a
  /// unique position. If there are no vertices that share the same location, each entry of the
  /// <c>pointRep</c> array is the identity (i.e. <c>pointRep[i] == i</c>). If there are
  /// duplicated vertex positions in the mesh, then some entry will point to another
  /// 'representative' vertex index.
  /// </para>
  /// <para>
  /// See: <see cref="GenerateAdjacencyAndPointReps"/>, <see cref="ConvertPointRepToAdjacency"/>,
  /// <see cref="GenerateGSAdjacency"/>
  /// </para>
  /// <para>
  /// <strong>Cleanup and validation:</strong><br/>
  /// Triangular mesh descriptions can contain a number of errors which result in invalid or failed
  /// geometric operations. There are also a number of cases where a triangular mesh description can
  /// cause mesh algorithms to fail. These functions can detect and resolve such issues.
  /// </para>
  /// <para>
  /// See: <see cref="Validate(IList{int},int, IList{int}, MeshValidationOptions,IList{string})"/>, <see cref="Clean"/>
  /// </para>
  /// <para>
  /// <strong>Mesh optimization:</strong><br/>
  /// Direct3D can render valid meshes with the same visual results no matter how the data is
  /// ordered, but the efficiency of the rendering performance can be impacted by ordering that is
  /// well-matched to modern GPUs. Mesh optimization is a process for reordering faces and vertices
  /// to provide the same visual result, with improved utilization of hardware resources.
  /// </para>
  /// <para>
  /// See: <see cref="AttributeSort"/>, <see cref="OptimizeFaces"/>, <see cref="OptimizeVertices"/>,
  /// </para>
  /// <para>
  /// A complete mesh optimization includes
  /// <list type="number">
  /// <item>
  /// <description><see cref="GenerateAdjacencyAndPointReps"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="Clean"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="AttributeSort"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="ReorderIBAndAdjacency(IList{int},IList{int},int[])"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="OptimizeFaces"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="ReorderIB(IList{int},int[])"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="OptimizeVertices"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="FinalizeIB(IList{int},int[])"/></description>
  /// </item>
  /// <item>
  /// <description><see cref="FinalizeVB(byte[],int,int,int[])"/></description>
  /// </item>
  /// </list>
  /// </para>
  ///  </remarks>
  internal static class DirectXMesh
  {
    // Notes:
    // face ... The index of the triangle. face * 3 == offset in indices.
    // point ... The index (0, 1, or 2) of the point on the triangle.

    // Additional resources:
    // http://directxmesh.codeplex.com/wikipage?title=Resources


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The maximum number of elements per vertex declaration.
    /// </summary>
    internal const int D3D11_IA_VERTEX_INPUT_STRUCTURE_ELEMENT_COUNT = 32;


    /// <summary>
    /// The maximum number input assembler vertex input resource slots.
    /// </summary>
    internal const int D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT = 32;


    /// <summary>
    /// The maximum buffer structure size (multi-element).
    /// </summary>
    internal const int D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES = 2048;


    /// <summary>
    /// Constant for automatically calculating the <see cref="VertexElement.AlignedByteOffset"/>.
    /// The current element is defined directly after the previous one, including any packing if
    /// necessary.
    /// </summary>
    internal const int D3D11_APPEND_ALIGNED_ELEMENT = -1;


    /// <summary>
    /// The default vertex cache size which is considered 'device independent'.
    /// </summary>
    public const int OPTFACES_V_DEFAULT = 12;

    /// <summary>
    /// The default restart threshold which is considered 'device independent'.
    /// </summary>
    public const int OPTFACES_R_DEFAULT = 7;

    /// <summary>
    /// Indicates no vertex cache optimization, only reordering into strips.
    /// </summary>
    public const int OPTFACES_V_STRIPORDER = 0;
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshAdjacency
    //--------------------------------------------------------------

    private class VertexHashEntry
    {
      public Vector3F V;
      public int Index;
      public VertexHashEntry Next;
    }


    private class EdgeHashEntry
    {
      public int V1;
      public int V2;
      public int VOther;
      public int Face;
      public EdgeHashEntry Next;
    }


    private static void MakeXHeap(Vector3F[] positions, out int[] indices)
    {
      int numberOfVertices = positions.Length;
      indices = Enumerable.Range(0, numberOfVertices).ToArray();

      if (numberOfVertices > 1)
      {
        // Create the heap.
        int iulLim = numberOfVertices;

        for (int vert = numberOfVertices >> 1; --vert != -1; )
        {
          // Percolate down.
          int iulI = vert;
          int iulJ = vert + vert + 1;
          int ulT = indices[iulI];

          while (iulJ < iulLim)
          {
            int ulJ = indices[iulJ];

            if (iulJ + 1 < iulLim)
            {
              int ulJ1 = indices[iulJ + 1];
              if (positions[ulJ1].X <= positions[ulJ].X)
              {
                iulJ++;
                ulJ = ulJ1;
              }
            }

            if (positions[ulJ].X > positions[ulT].X)
              break;

            indices[iulI] = indices[iulJ];
            iulI = iulJ;
            iulJ += iulJ + 1;
          }

          indices[iulI] = ulT;
        }

        // Sort the heap
        while (--iulLim != -1)
        {
          int ulT = indices[iulLim];
          indices[iulLim] = indices[0];

          // Percolate down
          int iulI = 0;
          int iulJ = 1;

          while (iulJ < iulLim)
          {
            int ulJ = indices[iulJ];

            if (iulJ + 1 < iulLim)
            {
              int ulJ1 = indices[iulJ + 1];
              if (positions[ulJ1].X <= positions[ulJ].X)
              {
                iulJ++;
                ulJ = ulJ1;
              }
            }

            if (positions[ulJ].X > positions[ulT].X)
              break;

            indices[iulI] = indices[iulJ];
            iulI = iulJ;
            iulJ += iulJ + 1;
          }

          Debug.Assert(iulI < numberOfVertices);
          indices[iulI] = ulT;
        }
      }
    }


    /// <summary>
    /// Generates the adjacency and/or point representatives for a mesh.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="positions">
    /// The vertex positions of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="epsilon">
    /// The threshold to use when comparing positions for shared/duplicate positions. This value can
    /// be 0 for bit-wise identical vertex positions.
    /// </param>
    /// <param name="pointRep">
    /// A 32-bit index array with numberOfVertices entries containing the point representatives for
    /// each vertex in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> or <paramref name="positions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the number of positions is 0.
    /// </exception>
    public static void GenerateAdjacencyAndPointReps(IList<int> indices, IList<Vector3F> positions, float epsilon, out int[] pointRep, out int[] adjacency)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positions.Count == 0)
        throw new ArgumentException("Positions must not be empty.", "positions");

      GeneratePointReps(indices, positions, epsilon, out pointRep);
      ConvertPointRepToAdjacency(indices, positions, pointRep, out adjacency);
    }


    private static void GeneratePointReps(IList<int> indices, IList<Vector3F> positions, float epsilon, out int[] pointRep)
    {
      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;

      // Cast/convert positions to array for faster access.
      var positionsArray = positions as Vector3F[] ?? positions.ToArray();

      pointRep = new int[numberOfVertices];

      int[] vertexToCorner = new int[numberOfVertices];
      for (int i = 0; i < vertexToCorner.Length; i++)
        vertexToCorner[i] = -1;

      int[] vertexCornerList = new int[numberOfFaces * 3];
      for (int j = 0; j < vertexCornerList.Length; j++)
        vertexCornerList[j] = -1;

      // Build initial lists and validate indices.
      for (int j = 0; j < numberOfFaces * 3; j++)
      {
        int k = indices[j];
        if (k == -1)
          continue;

        if (k >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        vertexCornerList[j] = vertexToCorner[k];
        vertexToCorner[k] = j;
      }

      // ReSharper disable once CompareOfFloatsByEqualityOperator
      if (epsilon == 0.0f)
      {
        int hashSize = numberOfVertices / 3;
        var hashTable = new VertexHashEntry[hashSize];

        var hashEntries = new VertexHashEntry[numberOfVertices];
        for (int i = 0; i < hashEntries.Length; i++)
          hashEntries[i] = new VertexHashEntry();

        int freeEntry = 0;

        for (int vert = 0; vert < numberOfVertices; vert++)
        {
          int hashKey = Math.Abs(positionsArray[vert].GetHashCode()) % hashSize;
          int found = -1;

          for (var current = hashTable[hashKey]; current != null; current = current.Next)
          {
            if (current.V == positionsArray[vert])
            {
              int head = vertexToCorner[vert];

              bool ispresent = false;

              while (head != -1)
              {
                int face = head / 3;
                Debug.Assert(face < numberOfFaces);
                Debug.Assert((indices[face * 3 + 0] == vert) || (indices[face * 3 + 1] == vert) || (indices[face * 3 + 2] == vert));

                if ((indices[face * 3 + 0] == current.Index) || (indices[face * 3 + 1] == current.Index) || (indices[face * 3 + 2] == current.Index))
                {
                  ispresent = true;
                  break;
                }

                head = vertexCornerList[head];
              }

              if (!ispresent)
              {
                found = current.Index;
                break;
              }
            }
          }

          if (found != -1)
          {
            pointRep[vert] = found;
          }
          else
          {
            Debug.Assert(freeEntry < numberOfVertices);

            var newEntry = hashEntries[freeEntry];
            freeEntry++;

            newEntry.V = positionsArray[vert];
            newEntry.Index = vert;
            newEntry.Next = hashTable[hashKey];
            hashTable[hashKey] = newEntry;

            pointRep[vert] = vert;
          }
        }

        Debug.Assert(freeEntry <= numberOfVertices);
      }
      else
      {
        int[] xorder;

        // Order in descending order.
        MakeXHeap(positionsArray, out xorder);

        for (int i = 0; i < pointRep.Length; i++)
          pointRep[i] = -1;

        float epsilonSquared = epsilon * epsilon;

        int head = 0;
        int tail = 0;

        while (tail < numberOfVertices)
        {
          // Move head until just out of epsilon.
          while (head < numberOfVertices && positionsArray[tail].X - positionsArray[head].X <= epsilon)
            head++;

          // Check new tail against all points up to the head.
          int tailIndex = xorder[tail];
          Debug.Assert(tailIndex < numberOfVertices);
          if (pointRep[tailIndex] == -1)
          {
            pointRep[tailIndex] = tailIndex;

            Vector3F outer = positionsArray[tailIndex];

            for (int current = tail + 1; current < head; current++)
            {
              int curIndex = xorder[current];
              Debug.Assert(curIndex < numberOfVertices);

              // If the point is already assigned, ignore it.
              if (pointRep[curIndex] == -1)
              {
                Vector3F inner = positionsArray[curIndex];

                float diff = (inner - outer).LengthSquared;

                if (diff < epsilonSquared)
                {
                  int headvc = vertexToCorner[tailIndex];

                  bool ispresent = false;

                  while (headvc != -1)
                  {
                    int face = headvc / 3;
                    Debug.Assert(face < numberOfFaces);

                    Debug.Assert((indices[face * 3 + 0] == tailIndex) || (indices[face * 3 + 1] == tailIndex) || (indices[face * 3 + 2] == tailIndex));

                    if (indices[face * 3] == curIndex || indices[face * 3 + 1] == curIndex || indices[face * 3 + 2] == curIndex)
                    {
                      ispresent = true;
                      break;
                    }

                    headvc = vertexCornerList[headvc];
                  }

                  if (!ispresent)
                  {
                    pointRep[curIndex] = tailIndex;
                  }
                }
              }
            }
          }

          tail++;
        }
      }
    }


    /// <summary>
    /// Converts a supplied point representatives array to mesh adjacency.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="positions">
    /// The vertex positions of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="pointRep">
    /// <para>
    /// A 32-bit index array with numberOfVertices entries containing the point representatives for
    /// each vertex in a mesh. Can be -1 to indicate an unused entry.
    /// </para>
    /// <para>
    /// This parameter can be <see langword="null"/>, in which case identity is assumed (i.e.
    /// <c>pointRep[i] == i</c>).
    /// </para>
    /// </param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <remarks>
    /// This operation is included as part of <see cref="GenerateAdjacencyAndPointReps"/> where the
    /// <paramref name="pointRep"/> data is also generated. These conversion functions are provided
    /// for cases where you already have a <paramref name="pointRep"/> and need to convert to
    /// adjacency.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> or <paramref name="positions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, <paramref name="positions"/> is empty.<br/>
    /// Or, <paramref name="pointRep"/> does not match the number of vertices.
    /// </exception>
    public static void ConvertPointRepToAdjacency(IList<int> indices, IList<Vector3F> positions, IList<int> pointRep, out int[] adjacency)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positions.Count == 0)
        throw new ArgumentException("Positions must not be empty.", "positions");

      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;

      if (pointRep == null)
        pointRep = Enumerable.Range(0, numberOfVertices).ToArray();
      else if (pointRep.Count != numberOfVertices)
        throw new ArgumentException("The length of pointRep does not match number of vertices.", "pointRep");


      int hashSize = numberOfVertices / 3;
      var hashTable = new EdgeHashEntry[hashSize];

      var hashEntries = new EdgeHashEntry[3 * numberOfFaces];
      for (int j = 0; j < hashEntries.Length; j++)
        hashEntries[j] = new EdgeHashEntry();

      int freeEntry = 0;

      // Add face edges to hash table and validate indices.
      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        if (i0 >= numberOfVertices || i1 >= numberOfVertices || i2 >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        int v1 = pointRep[i0];
        int v2 = pointRep[i1];
        int v3 = pointRep[i2];

        // Filter out degenerate triangles.
        if (v1 == v2 || v1 == v3 || v2 == v3)
          continue;

        for (int point = 0; point < 3; point++)
        {
          int va = pointRep[indices[face * 3 + point]];
          int vb = pointRep[indices[face * 3 + ((point + 1) % 3)]];
          int vOther = pointRep[indices[face * 3 + ((point + 2) % 3)]];

          int hashKey = va % hashSize;

          Debug.Assert(freeEntry < 3 * numberOfFaces);

          var newEntry = hashEntries[freeEntry];
          freeEntry++;

          newEntry.V1 = va;
          newEntry.V2 = vb;
          newEntry.VOther = vOther;
          newEntry.Face = face;
          newEntry.Next = hashTable[hashKey];
          hashTable[hashKey] = newEntry;
        }
      }

      Debug.Assert(freeEntry <= 3 * numberOfFaces);

      adjacency = new int[numberOfFaces * 3];
      for (int j = 0; j < adjacency.Length; j++)
        adjacency[j] = -1;

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        // Filter out unused triangles.
        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        Debug.Assert(i0 < numberOfVertices);
        Debug.Assert(i1 < numberOfVertices);
        Debug.Assert(i2 < numberOfVertices);

        int v1 = pointRep[i0];
        int v2 = pointRep[i1];
        int v3 = pointRep[i2];

        // Filter out degenerate triangles.
        if (v1 == v2 || v1 == v3 || v2 == v3)
          continue;

        for (int point = 0; point < 3; point++)
        {
          if (adjacency[face * 3 + point] != -1)
            continue;

          // See if edge already entered, if not then enter it.
          int va = pointRep[indices[face * 3 + ((point + 1) % 3)]];
          int vb = pointRep[indices[face * 3 + point]];
          int vOther = pointRep[indices[face * 3 + ((point + 2) % 3)]];

          int hashKey = va % hashSize;

          var current = hashTable[hashKey];
          var prev = (EdgeHashEntry)null;

          int foundFace = -1;

          while (current != null)
          {
            if ((current.V2 == vb) && (current.V1 == va))
            {
              foundFace = current.Face;
              break;
            }

            prev = current;
            current = current.Next;
          }

          var found = current;
          var foundPrev = prev;

          float bestDiff = -2.0f;

          // Scan for additional matches.
          if (current != null)
          {
            prev = current;
            current = current.Next;

            // Find 'better' match.
            while (current != null)
            {
              if ((current.V2 == vb) && (current.V1 == va))
              {
                Vector3F pB1 = positions[vb];
                Vector3F pB2 = positions[va];
                Vector3F pB3 = positions[vOther];

                Vector3F v12 = pB1 - pB2;
                Vector3F v13 = pB1 - pB3;

                Vector3F bnormal =  Normalize(Vector3F.Cross(v12, v13));

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (bestDiff == -2.0f)
                {
                  Vector3F pA1 = positions[found.V1];
                  Vector3F pA2 = positions[found.V2];
                  Vector3F pA3 = positions[found.VOther];

                  v12 = pA1 - pA2;
                  v13 = pA1 - pA3;

                  Vector3F anormal = Normalize(Vector3F.Cross(v12, v13));

                  bestDiff = Vector3F.Dot(anormal, bnormal);
                }

                float diff;
                {
                  Vector3F pA1 = positions[current.V1];
                  Vector3F pA2 = positions[current.V2];
                  Vector3F pA3 = positions[current.VOther];

                  v12 = pA1 - pA2;
                  v13 = pA1 - pA3;

                  Vector3F anormal = Normalize(Vector3F.Cross(v12, v13));

                  diff = Vector3F.Dot(anormal, bnormal);
                }

                // If face normals are closer, use new match.
                if (diff > bestDiff)
                {
                  found = current;
                  foundPrev = prev;
                  foundFace = current.Face;
                  bestDiff = diff;
                }
              }

              prev = current;
              current = current.Next;
            }
          }

          if (foundFace != -1)
          {
            Debug.Assert(found != null);

            // Remove found face from hash table.
            if (foundPrev != null)
              foundPrev.Next = found.Next;
            else
              hashTable[hashKey] = found.Next;

            Debug.Assert(adjacency[face * 3 + point] == -1);
            adjacency[face * 3 + point] = foundFace;

            // Check for other edge.
            int hashKey2 = vb % hashSize;

            current = hashTable[hashKey2];
            prev = null;

            while (current != null)
            {
              if (current.Face == face && current.V2 == va && current.V1 == vb)
              {
                // Trim edge from hash table.
                if (prev != null)
                  prev.Next = current.Next;
                else
                  hashTable[hashKey2] = current.Next;

                break;
              }

              prev = current;
              current = current.Next;
            }

            // Mark neighbor to point back.
            bool linked = false;

            for (int point2 = 0; point2 < point; point2++)
            {
              if (foundFace == adjacency[face * 3 + point2])
              {
                linked = true;
                adjacency[face * 3 + point] = -1;
                break;
              }
            }

            if (!linked)
            {
              int point2 = 0;
              for (; point2 < 3; point2++)
              {
                int k = indices[foundFace * 3 + point2];
                if (k == -1)
                  continue;

                Debug.Assert(k < numberOfVertices);

                if (pointRep[k] == va)
                  break;
              }

              if (point2 < 3)
              {
#if DEBUG
                int testPoint = indices[foundFace * 3 + ((point2 + 1) % 3)];
                testPoint = pointRep[testPoint];
                Debug.Assert(testPoint == vb);
#endif
                Debug.Assert(adjacency[foundFace * 3 + point2] == -1);

                // Update neighbor to point back to this face match edge.
                adjacency[foundFace * 3 + point2] = face;
              }
            }
          }
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshGSAdjacency
    //--------------------------------------------------------------

    /// <summary>
    /// Generates an index buffer suited for use with the Geometry Shader including adjacency
    /// information.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="pointRep">
    /// A 32-bit index array with numberOfVertices entries containing the point representatives for
    /// each vertex in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="indicesAdj">
    /// The index buffer for use with Geometry Shader (D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST_ADJ).
    /// </param>
    /// <remarks>
    /// This method generates an IB triangle list with adjacency
    /// (D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST_ADJ). For more information, see
    /// <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/bb205124.aspx">Primitive
    /// Topologies</see>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/>, <paramref name="pointRep"/> or <paramref name="adjacency"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, <paramref name="pointRep"/> is empty.<br/>
    /// Or, the length of <paramref name="adjacency"/> does not match <paramref name="indices"/>.
    /// </exception>
    public static void GenerateGSAdjacency(IList<int> indices, IList<int> pointRep, IList<int> adjacency, out int[] indicesAdj)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (pointRep == null)
        throw new ArgumentNullException("pointRep");
      if (pointRep.Count == 0)
        throw new ArgumentException("pointRep must not be empty.", "pointRep");
      if (adjacency == null)
        throw new ArgumentNullException("adjacency");
      if (adjacency.Count != indices.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacency.Count, indices.Count);
        throw new ArgumentException(message, "adjacency");
      }

      int numberOfVertices = pointRep.Count;
      int numberOfFaces = indices.Count / 3;
      if ((long)numberOfFaces * 6 >= int.MaxValue)
        throw new OverflowException("The number of entries in indicesAdj exceeds Int32.MaxValue.");

      indicesAdj = new int[numberOfFaces * 6];

      int inputi = 0;
      int outputi = 0;

      for (int face = 0; face < numberOfFaces; face++)
      {
        for (int point = 0; point < 3; point++)
        {
          Debug.Assert(outputi < (numberOfFaces * 6));

          indicesAdj[outputi] = indices[inputi];
          outputi++;
          inputi++;

          Debug.Assert(outputi < (numberOfFaces * 6));

          int a = adjacency[face * 3 + point];
          if (a == -1)
          {
            indicesAdj[outputi] = indices[face * 3 + ((point + 2) % 3)];
          }
          else
          {
            int v1 = indices[face * 3 + point];
            int v2 = indices[face * 3 + ((point + 1) % 3)];

            if (v1 == -1 || v2 == -1)
            {
              indicesAdj[outputi] = -1;
            }
            else
            {
              if (v1 >= numberOfVertices || v2 >= numberOfVertices)
                throw new IndexOutOfRangeException("Index exceeds number of vertices.");

              v1 = pointRep[v1];
              v2 = pointRep[v2];

              int vOther = -1;

              // Find other vertex.
              for (int k = 0; k < 3; k++)
              {
                Debug.Assert(a < numberOfFaces);

                int ak = indices[a * 3 + k];
                if (ak == -1)
                  break;

                if (ak >= numberOfVertices)
                  throw new IndexOutOfRangeException("Index exceeds number of vertices.");

                if (pointRep[ak] == v1)
                  continue;

                if (pointRep[ak] == v2)
                  continue;

                vOther = ak;
              }

              if (vOther == -1)
                indicesAdj[outputi] = indices[face * 3 + ((point + 2) % 3)];
              else
                indicesAdj[outputi] = vOther;
            }
          }
          outputi++;
        }
      }

      Debug.Assert(inputi == numberOfFaces * 3);
      Debug.Assert(outputi == numberOfFaces * 6);
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshClean
    //--------------------------------------------------------------

    // unordered_multimap<>
    private class UnorderedMultiMap<TKey, TValue>
    {
      readonly Dictionary<TKey, List<TValue>> _dictionary = new Dictionary<TKey, List<TValue>>();

      public void Add(TKey key, TValue value)
      {
        List<TValue> bucket;
        if (!_dictionary.TryGetValue(key, out bucket))
        {
          bucket = new List<TValue>();
          _dictionary.Add(key, bucket);
        }

        bucket.Add(value);
      }

      public List<TValue> GetValues(TKey key)
      {
        List<TValue> bucket;
        _dictionary.TryGetValue(key, out bucket);
        return bucket;
      }
    }


    /// <summary>
    /// Eliminates common problems by modifying mesh indices, adjacency, and/or duplicating
    /// vertices. (See <see cref="Validate(IList{int},int,IList{int},MeshValidationOptions,IList{string})"/>.)
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="adjacency">
    /// <para>
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </para>
    /// <para>
    /// If <paramref name="adjacency"/> is provided, then
    /// <see cref="MeshValidationOptions.Backfacing"/> cleanup is performed. Any neighbor adjacency
    /// connections that are asymmetric are removed.
    /// </para>
    /// </param>
    /// <param name="attributes">
    /// A 32-bit index array with numberOfFaces entries which contains the attribute id for each
    /// face in the mesh. If <paramref name="attributes"/> is provided
    /// the cleanup ensure that each vertex is only used by one attribute.
    /// </param>
    /// <param name="breakBowties">
    /// <see langword="true"/> to break bowties by duplicating the vertex that connects the two
    /// triangle fans; <see langword="false"/> to ignore bowties.
    /// </param>
    /// <param name="duplicateVertices">
    /// The duplicated vertices that need to be added to the end of the vertex buffer.
    /// <see cref="Clean"/> automatically updates <paramref name="indices"/> to reference these new
    /// vertices by the function. Each element of the <paramref name="duplicateVertices"/> array
    /// indicates the original vertex index to duplicate at that position at the end of the existing
    /// vertex buffer. See <see cref="FinalizeVB(byte[],int,int,int[],int[],out byte[])"/> and
    /// <see cref="FinalizeVBAndPointReps(byte[],int,int,int[],int[],int[],out byte[],out int[])"/>
    /// for more details.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if successful; otherwise, <see langword="false"/>. In case of failure
    /// call <see cref="Validate(IList{int},int,IList{int},MeshValidationOptions,IList{string})"/>
    /// to get information about problems.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Since that cleanup occurs in phases, so some changes may have already been applied to
    /// provided buffers even on an error result.
    /// </para>
    /// <para>
    /// This does not eliminate degenerate triangles, but if adjacency is provided it ensures that
    /// degenerate triangles are not neighbors of other faces.
    /// </para>
    /// <para>
    /// This method will ensure partial 'unused' faces are fully marked as unused, and if adjacency
    /// is provided it ensures that unused triangles are not neighbors of other faces.
    /// </para>
    /// <para>
    /// This is an initial step in performing full mesh optimization, particularly the attribute
    /// duplication. Use of <paramref name="breakBowties"/> is optional for mesh optimization.
    ///  </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the length of <paramref name="adjacency"/> does not match <paramref name="indices"/>.<br/>
    /// Or, invalid number of attributes.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> is out of range.
    /// </exception>
    public static bool Clean(IList<int> indices, int numberOfVertices, IList<int> adjacency, IList<int> attributes, bool breakBowties, out int[] duplicateVertices)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (adjacency != null && adjacency.Count != indices.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacency.Count, indices.Count);
        throw new ArgumentException(message, "adjacency");
      }

      int numberOfIndices = indices.Count;
      int numberOfFaces = numberOfIndices / 3;
      if (attributes != null && attributes.Count != numberOfFaces)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "Invalid number of attributes ({0}). The number of attributes needs to match the number of faces ({1}).",
          attributes.Count, numberOfFaces);
        throw new ArgumentException(message);
      }

      if (!Validate(indices, numberOfVertices, adjacency, MeshValidationOptions.Default, null))
      {
        duplicateVertices = null;
        return false;
      }

      var dupVerts = new List<int>();
      int curNewVert = numberOfVertices;
      bool[] faceSeen = new bool[numberOfIndices];
      int[] ids = new int[numberOfVertices];

      // ----- MeshValidationOptions.Unused, MeshValidationOptions.Degenerate cleanup
      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
        {
          // Ensure all index entries in the unused face are 'unused'.
          indices[face * 3 + 0] =
          indices[face * 3 + 1] =
          indices[face * 3 + 2] = -1;

          // Ensure no neighbor references the unused face.
          if (adjacency != null)
          {
            for (int point = 0; point < 3; point++)
            {
              int k = adjacency[face * 3 + point];
              if (k != -1)
              {
                Debug.Assert(k < numberOfFaces);

                if (adjacency[k * 3 + 0] == face)
                  adjacency[k * 3] = -1;

                if (adjacency[k * 3 + 1] == face)
                  adjacency[k * 3 + 1] = -1;

                if (adjacency[k * 3 + 2] == face)
                  adjacency[k * 3 + 2] = -1;

                adjacency[face * 3 + point] = -1;
              }
            }
          }
        }
        else if (i0 == i1 || i0 == i2 || i1 == i2)
        {
          // Clean doesn't trim out degenerates as most other functions ignore them.

          // Ensure no neighbor references the degenerate face.
          if (adjacency != null)
          {
            for (int point = 0; point < 3; point++)
            {
              int k = adjacency[face * 3 + point];
              if (k != -1)
              {
                Debug.Assert(k < numberOfFaces);

                if (adjacency[k * 3 + 0] == face)
                  adjacency[k * 3] = -1;

                if (adjacency[k * 3 + 1] == face)
                  adjacency[k * 3 + 1] = -1;

                if (adjacency[k * 3 + 2] == face)
                  adjacency[k * 3 + 2] = -1;

                adjacency[face * 3 + point] = -1;
              }
            }
          }
        }
      }

      // ----- MeshValidationOptions.AsymmetricAdjacency cleanup
      if (adjacency != null)
      {
        for (;;)
        {
          bool unlinked = false;
          for (int face = 0; face < numberOfFaces; face++)
          {
            for (int point = 0; point < 3; point++)
            {
              int k = adjacency[face * 3 + point];
              if (k != -1)
              {
                Debug.Assert(k < numberOfFaces);

                int edge = FindEdge(adjacency, k * 3, face);
                if (edge >= 3)
                {
                  unlinked = true;
                  adjacency[face * 3 + point] = -1;
                }
              }
            }
          }

          if (!unlinked)
            break;
        }
      }

      // ----- MeshValidationOptions.Backfacing cleanup
      if (adjacency != null)
      {
        for (int face = 0; face < numberOfFaces; face++)
        {
          int i0 = indices[face * 3 + 0];
          int i1 = indices[face * 3 + 1];
          int i2 = indices[face * 3 + 2];

          if (i0 == -1 || i1 == -1 || i2 == -1)
          {
            // Ignore unused faces.
            continue;
          }

          Debug.Assert(i0 < numberOfVertices);
          Debug.Assert(i1 < numberOfVertices);
          Debug.Assert(i2 < numberOfVertices);

          if (i0 == i1 || i0 == i2 || i1 == i2)
          {
            // Ignore degenerate faces.
            continue;
          }

          int j0 = adjacency[face * 3 + 0];
          int j1 = adjacency[face * 3 + 1];
          int j2 = adjacency[face * 3 + 2];

          if ((j0 == j1 && j0 != -1)
               || (j0 == j2 && j0 != -1)
               || (j1 == j2 && j1 != -1))
          {
            int neighbor;
            if (j0 == j1 || j0 == j2)
              neighbor = j0;
            else
              neighbor = j1;

            // Remove links then break bowties will clean up any remaining issues.
            for (int edge = 0; edge < 3; edge++)
            {
              if (adjacency[face * 3 + edge] == neighbor)
                adjacency[face * 3 + edge] = -1;

              if (adjacency[neighbor * 3 + edge] == face)
                adjacency[neighbor * 3 + edge] = -1;
            }
          }
        }
      }

      int[] indicesNew = new int[numberOfIndices];
      indices.CopyTo(indicesNew, 0);

      // ----- MeshValidationOptions.Bowties cleanup
      if (adjacency != null && breakBowties)
      {
        for (int i = 0; i < ids.Length; i++)
          ids[i] = -1;

        var ovi = new OrbitIterator(indices, adjacency);

        for (int face = 0; face < numberOfFaces; face++)
        {
          int i0 = indices[face * 3 + 0];
          int i1 = indices[face * 3 + 1];
          int i2 = indices[face * 3 + 2];

          if (i0 == -1 || i1 == -1 || i2 == -1)
          {
            // Ignore unused faces.
            faceSeen[face * 3 + 0] = true;
            faceSeen[face * 3 + 1] = true;
            faceSeen[face * 3 + 2] = true;
            continue;
          }

          Debug.Assert(i0 < numberOfVertices);
          Debug.Assert(i1 < numberOfVertices);
          Debug.Assert(i2 < numberOfVertices);

          if (i0 == i1 || i0 == i2 || i1 == i2)
          {
            // Ignore degenerate faces.
            faceSeen[face * 3 + 0] = true;
            faceSeen[face * 3 + 1] = true;
            faceSeen[face * 3 + 2] = true;
            continue;
          }

          for (int point = 0; point < 3; point++)
          {
            if (faceSeen[face * 3 + point])
              continue;

            faceSeen[face * 3 + point] = true;

            int i = indices[face * 3 + point];
            if (i == -1)
              continue;

            Debug.Assert(i < numberOfVertices);

            ovi.Initialize(face, i, WalkType.All);
            ovi.MoveToCounterClockwise();

            int replaceVertex = -1;
            int replaceValue = -1;

            while (!ovi.Done)
            {
              int curFace = ovi.NextFace();
              if (curFace == -1)
              {
                duplicateVertices = null;
                return false;
              }

              int curPoint = ovi.Point;
              if (curPoint > 2)
              {
                duplicateVertices = null;
                return false;
              }

              faceSeen[curFace * 3 + curPoint] = true;

              int j = indices[curFace * 3 + curPoint];
              if (j == -1)
                continue;

              Debug.Assert(j < numberOfVertices);

              if (j == replaceVertex)
              {
                indicesNew[curFace * 3 + curPoint] = replaceValue;
              }
              else if (ids[j] == -1)
              {
                ids[j] = face;
              }
              else if (ids[j] != face)
              {
                // We found a bowtie, duplicate a vertex.
                replaceVertex = j;
                replaceValue = curNewVert;
                indicesNew[curFace * 3 + curPoint] = replaceValue;
                curNewVert++;

                dupVerts.Add(j);
              }
            }
          }
        }

        Debug.Assert(numberOfVertices + dupVerts.Count == curNewVert);
      }

      // Ensure no vertex is used by more than one attribute.
      if (attributes != null)
      {
        for (int i = 0; i < ids.Length; i++)
          ids[i] = -1;

        var dupAttr = new List<int>(dupVerts.Count);
        for (int j = 0; j < dupVerts.Count; j++)
          dupAttr.Add(-1);

        var dups = new UnorderedMultiMap<int, int>();

        for (int face = 0; face < numberOfFaces; face++)
        {
          int a = attributes[face];
          for (int point = 0; point < 3; point++)
          {
            int j = indicesNew[face * 3 + point];

            int k = (j >= numberOfVertices) ? dupAttr[j - numberOfVertices] : ids[j];

            if (k == -1)
            {
              if (j >= numberOfVertices)
                dupAttr[j - numberOfVertices] = a;
              else
                ids[j] = a;
            }
            else if (k != a)
            {
              // Look for a duplicate with the correct attribute.
              var duplicates = dups.GetValues(j);

              bool duplicateFound = false;
              if (duplicates != null)
              {
                foreach (int d in duplicates)
                {
                  int m = (d >= numberOfVertices) ? dupAttr[d - numberOfVertices] : ids[d];
                  if (m == a)
                  {
                    indicesNew[face * 3 + point] = d;
                    duplicateFound = true;
                    break;
                  }
                }
              }

              if (!duplicateFound)
              {
                // Duplicate the vertex.
                dups.Add(j, curNewVert);

                indicesNew[face * 3 + point] = curNewVert;
                curNewVert++;

                if (j >= numberOfVertices)
                  dupVerts.Add(dupVerts[j - numberOfVertices]);
                else
                  dupVerts.Add(j);

                dupAttr.Add(a);

                Debug.Assert(dupVerts.Count == dupAttr.Count);
              }
            }
          }
        }

        Debug.Assert((numberOfVertices + dupVerts.Count) == curNewVert);

#if DEBUG
        foreach (int i in dupVerts)
          Debug.Assert(i < numberOfVertices);
#endif
      }

      if ((long)numberOfVertices + dupVerts.Count >= int.MaxValue)
        throw new OverflowException("The number of vertices after cleanup exceeds Int32.MaxValue.");

      if (dupVerts.Count > 0)
      {
        for (int j = 0; j < numberOfIndices; j++)
          indices[j] = indicesNew[j];
      }

      duplicateVertices = dupVerts.ToArray();
      return true;
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshNormals
    //--------------------------------------------------------------

    /// <summary>
    /// Generates the vertex normals.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="positions">
    /// The vertex positions of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="clockwiseOrder">
    /// <see langword="true"/> if vertices of a front face are ordered clockwise;
    /// <see langword="false"/> if counter-clockwise.
    /// </param>
    /// <param name="normalAlgorithm">The algorithm for calculating vertex normals.</param>
    /// <returns>The vertex normals. The resulting array has the same length as
    /// <paramref name="positions"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> or <paramref name="positions"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// A vertex index exceeds the number of vertices.
    /// </exception>
    public static Vector3F[] ComputeNormals(IList<int> indices, IList<Vector3F> positions, bool clockwiseOrder, VertexNormalAlgorithm normalAlgorithm)
    {
      // References:
      // - S Jin, R R Lewis, and D West: "A comparison of algorithms for vertex normal computation"
      //   http://www.tricity.wsu.edu/~bobl/personal/mypubs/2003_vertnorm_tvc.pdf
      // - Nelson Max: "Weights for Computing Vertex Normals from Facet Normals"
      //   https://computing.llnl.gov/vis/images/pdf/max_jgt99.pdf
      // - Max Wagner: "Generating Vertex Normals"
      //   http://www.emeyex.com/site/tuts/VertexNormals.pdf

      if (positions == null)
        throw new ArgumentNullException("positions");
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");

      switch (normalAlgorithm)
      {
        case VertexNormalAlgorithm.WeightedByArea:
          return ComputeNormalsWeightedByArea(positions, indices, clockwiseOrder);
        case VertexNormalAlgorithm.WeightedEqually:
          return ComputeNormalsWeighedEqually(positions, indices, clockwiseOrder);
        default:
          return ComputeNormalsWeightedByAngle(positions, indices, clockwiseOrder);
      }
    }


    private static Vector3F[] ComputeNormalsWeightedByAngle(IList<Vector3F> positions, IList<int> indices, bool clockwiseOrder)
    {
      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;
      Vector3F[] normals = new Vector3F[numberOfVertices];

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        if (i0 >= numberOfVertices || i1 >= numberOfVertices || i2 >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        Vector3F p0 = positions[i0];
        Vector3F p1 = positions[i1];
        Vector3F p2 = positions[i2];

        Vector3F u = p1 - p0;
        Vector3F v = p2 - p0;

        Vector3F n = Normalize(Vector3F.Cross(u, v));

        // Corner 0:
        Vector3F a = Normalize(u);
        Vector3F b = Normalize(v);
        float w0 = Vector3F.Dot(a, b);
        w0 = MathHelper.Clamp(w0, -1, 1);
        w0 = (float)Math.Acos(w0);

        // Corner 1:
        Vector3F c = Normalize(p2 - p1);
        Vector3F d = Normalize(p0 - p1);
        float w1 = Vector3F.Dot(c, d);
        w1 = MathHelper.Clamp(w1, -1, 1);
        w1 = (float)Math.Acos(w1);

        // Corner 2:
        Vector3F e = Normalize(p0 - p2);
        Vector3F f = Normalize(p1 - p2);
        float w2 = Vector3F.Dot(e, f);
        w2 = MathHelper.Clamp(w2, -1, 1);
        w2 = (float)Math.Acos(w2);

        normals[i0] += n * w0;
        normals[i1] += n * w1;
        normals[i2] += n * w2;
      }

      StoreNormals(normals, clockwiseOrder);
      return normals;
    }


    private static Vector3F[] ComputeNormalsWeightedByArea(IList<Vector3F> positions, IList<int> indices, bool clockwiseOrder)
    {
      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;
      var normals = new Vector3F[numberOfVertices];

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        if (i0 >= numberOfVertices || i1 >= numberOfVertices || i2 >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        Vector3F p0 = positions[i0];
        Vector3F p1 = positions[i1];
        Vector3F p2 = positions[i2];

        Vector3F u = p1 - p0;
        Vector3F v = p2 - p0;

        Vector3F n = Normalize(Vector3F.Cross(u, v));

        // Corner 0
        float w0 = Vector3F.Cross(u, v).Length;

        // Corner 1:
        Vector3F c = p2 - p1;
        Vector3F d = p0 - p1;
        float w1 = Vector3F.Cross(c, d).Length;

        // Corner 2:
        Vector3F e = p0 - p2;
        Vector3F f = p1 - p2;
        float w2 = Vector3F.Cross(e, f).Length;

        normals[i0] += n * w0;
        normals[i1] += n * w1;
        normals[i2] += n * w2;
      }

      StoreNormals(normals, clockwiseOrder);
      return normals;
    }


    private static Vector3F[] ComputeNormalsWeighedEqually(IList<Vector3F> positions, IList<int> indices, bool clockwiseOrder)
    {
      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;
      var normals = new Vector3F[numberOfVertices];

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        if (i0 >= numberOfVertices || i1 >= numberOfVertices || i2 >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        Vector3F p0 = positions[i0];
        Vector3F p1 = positions[i1];
        Vector3F p2 = positions[i2];

        Vector3F u = p1 - p0;
        Vector3F v = p2 - p0;

        Vector3F n = Normalize(Vector3F.Cross(u, v));

        normals[i0] += n;
        normals[i1] += n;
        normals[i2] += n;
      }

      StoreNormals(normals, clockwiseOrder);
      return normals;
    }


    private static void StoreNormals(Vector3F[] normals, bool clockwiseOrder)
    {
      if (clockwiseOrder)
      {
        for (int i = 0; i < normals.Length; i++)
          normals[i] = -Normalize(normals[i]);
      }
      else
      {
        for (int i = 0; i < normals.Length; i++)
          normals[i] = Normalize(normals[i]);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshTangentFrame
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Generates per-vertex tangent and bi-tangent information.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Generates per-vertex tangent and bi-tangent information.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="positions">
    /// The vertex positions of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="normals">The vertex normals.</param>
    /// <param name="textureCoordinates">
    /// The UV texture coordinates of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="tangents">The vertex tangents.</param>
    /// <param name="bitangents">The vertex bitangents.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/>, <paramref name="positions"/>, <paramref name="normals"/>, or
    /// <paramref name="textureCoordinates"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the number of positions is 0.<br/>
    /// Or, the length of <paramref name="normals"/> does not match <paramref name="positions"/>.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// A vertex index exceeds the number of vertices.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// <paramref name="normals"/> contains an invalid normal (length = 0).
    /// </exception>
    public static void ComputeTangentFrame(IList<int> indices, IList<Vector3F> positions, IList<Vector3F> normals, IList<Vector2F> textureCoordinates, out Vector3F[] tangents, out Vector3F[] bitangents)
    {

      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positions.Count == 0)
        throw new ArgumentException("Positions must not be empty.", "positions");
      if (normals == null)
        throw new ArgumentNullException("normals");
      if (normals.Count != positions.Count)
        throw new ArgumentException("The number of normals does not match number of positions.", "normals");
      if (textureCoordinates == null)
        throw new ArgumentNullException("textureCoordinates");

      int numberOfVertices = positions.Count;
      tangents = new Vector3F[numberOfVertices];
      bitangents = new Vector3F[numberOfVertices];

      ComputeTangentFrame(indices, positions, normals, textureCoordinates, null, tangents, bitangents);
    }


    /// <summary>
    /// Generates per-vertex tangent and handedness information.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="positions">
    /// The vertex positions of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="normals">The vertex normals.</param>
    /// <param name="textureCoordinates">
    /// The UV texture coordinates of the mesh as indexed by entries in indices. This must have
    /// numberOfVertices entries.
    /// </param>
    /// <param name="tangentsAndHandedness">
    /// The vertex tangents + handedness (see remarks). Can be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// Instead of storing tangents + bi-tangents, the information can be stored as tangent +
    /// handedness: The tangents can be returned as a 4D vector where the W component indicates
    /// 'handedness'. This allows for an easy reconstruction of the bi-tangent in the shader.
    /// <code lang="none">
    /// <![CDATA[
    /// float3 normal;
    /// float4 tangentAndHandedness;
    /// 
    /// ...
    /// 
    /// float3 tangent = tangentAndHandedness.xyz;
    /// float3 handedness = tangentAndHandedness.w;
    /// float3 bitangent = cross(normal, tangent) * handedness;
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/>, <paramref name="positions"/>, <paramref name="normals"/>, or
    /// <paramref name="textureCoordinates"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the number of positions is 0.<br/>
    /// Or, the length of <paramref name="normals"/> does not match <paramref name="positions"/>.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// A vertex index exceeds the number of vertices.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// <paramref name="normals"/> contains an invalid normal (length = 0).
    /// </exception>
    public static void ComputeTangentFrame(IList<int> indices, IList<Vector3F> positions, IList<Vector3F> normals, IList<Vector2F> textureCoordinates, out Vector4F[] tangentsAndHandedness)
    {
      // References:
      // - Lengyel, Eric: “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon
      //   Software 3D Graphics Library, 2001. Online: http: //www.terathon.com/code/tangent.html
      // - Mittring, Martin: "Triangle Mesh Tangent Space Calculation". Shader X^4 Advanced
      //   Rendering Techniques, 2006

      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (positions == null)
        throw new ArgumentNullException("positions");
      if (positions.Count == 0)
        throw new ArgumentException("Positions must not be empty.", "positions");
      if (normals == null)
        throw new ArgumentNullException("normals");
      if (normals.Count != positions.Count)
        throw new ArgumentException("The number of normals does not match number of positions.", "normals");
      if (textureCoordinates == null)
        throw new ArgumentNullException("textureCoordinates");

      int numberOfVertices = positions.Count;
      tangentsAndHandedness = new Vector4F[numberOfVertices];

      ComputeTangentFrame(indices, positions, normals, textureCoordinates, tangentsAndHandedness, null, null);
    }


    private static void ComputeTangentFrame(IList<int> indices, IList<Vector3F> positions, IList<Vector3F> normals, IList<Vector2F> textureCoordinates, Vector4F[] tangentsAndHandedness, Vector3F[] tangents, Vector3F[] bitangents)
    {
      // References:
      // - Lengyel, Eric: “Computing Tangent Space Basis Vectors for an Arbitrary Mesh”. Terathon
      //   Software 3D Graphics Library, 2001. Online: http: //www.terathon.com/code/tangent.html
      // - Mittring, Martin: "Triangle Mesh Tangent Space Calculation". Shader X^4 Advanced
      //   Rendering Techniques, 2006

      const float epsilon = 0.0001f;
      Vector4F sFlips = new Vector4F(1, -1, -1, 1);

      int numberOfVertices = positions.Count;
      int numberOfFaces = indices.Count / 3;

      var tangents0 = new Vector3F[numberOfVertices];
      var tangents1 = new Vector3F[numberOfVertices];

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
          continue;

        if (i0 >= numberOfVertices
            || i1 >= numberOfVertices
            || i2 >= numberOfVertices)
        {
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }

        Vector2F uv0 = textureCoordinates[i0];
        Vector2F uv1 = textureCoordinates[i1];
        Vector2F uv2 = textureCoordinates[i2];

        Vector4F s;
        s.X = uv1.X - uv0.X;
        s.Y = uv2.X - uv0.X;
        s.Z = uv1.Y - uv0.Y;
        s.W = uv2.Y - uv0.Y;

        float d = s.X * s.W - s.Y * s.Z;
        d = (Math.Abs(d) < epsilon) ? 1.0f : 1.0f / d;
        s *= d;
        s = s * sFlips;

        Matrix44F m0 = new Matrix44F(s.W, s.Z, 0, 0,
                                     s.Y, s.X, 0, 0,
                                     0, 0, 0, 0,
                                     0, 0, 0, 0);

        Vector4F p0 = new Vector4F(positions[i0], 0);
        Vector4F p1 = new Vector4F(positions[i1], 0);
        Vector4F p2 = new Vector4F(positions[i2], 0);

        Matrix44F m1 = new Matrix44F();
        m1.SetRow(0, p1 - p0);
        m1.SetRow(1, p2 - p0);
        //m1.SetRow(2, Vector4F.Zero);
        //m1.SetRow(3, Vector4F.Zero);

        Matrix44F uv = m0 * m1;

        Vector3F sDir = uv.GetRow(0).XYZ;
        tangents0[i0] += sDir;
        tangents0[i1] += sDir;
        tangents0[i2] += sDir;

        var tDir = uv.GetRow(1).XYZ;
        tangents1[i0] += tDir;
        tangents1[i1] += tDir;
        tangents1[i2] += tDir;
      }

      for (int i = 0; i < numberOfVertices; i++)
      {
        // Gram-Schmidt orthonormalization.
        Vector3F b0 = normals[i];
        b0 = Normalize(b0);

        Vector3F tan0 = tangents0[i];
        Vector3F b1 = tan0 - Vector3F.Dot(b0, tan0) * b0;
        b1 = Normalize(b1);

        Vector3F tan1 = tangents1[i];
        Vector3F b2 = tan1 - Vector3F.Dot(b0, tan1) * b0 - Vector3F.Dot(b1, tan1) * b1;
        b2 = Normalize(b2);

        // Handle degenerate vectors.
        float length1 = b1.Length;
        float length2 = b2.Length;

        if (length1 <= epsilon || length2 <= epsilon)
        {
          if (length1 > 0.5f)
          {
            // Reset bi-tangent from tangent and normal.
            b2 = Vector3F.Cross(b0, b1);
          }
          else if (length2 > 0.5f)
          {
            // Reset tangent from bi-tangent and normal.
            b1 = Vector3F.Cross(b2, b0);
          }
          else
          {
            Vector3F axis;

            // Reset both tangent and bi-tangent from normal.
            float d0 = Math.Abs(Vector3F.Dot(Vector3F.UnitX, b0));
            float d1 = Math.Abs(Vector3F.Dot(Vector3F.UnitY, b0));
            float d2 = Math.Abs(Vector3F.Dot(Vector3F.UnitZ, b0));
            if (d0 < d1)
            {
              axis = (d0 < d2) ? Vector3F.UnitX : Vector3F.UnitZ;
            }
            else if (d1 < d2)
            {
              axis = Vector3F.UnitY;
            }
            else
            {
              axis = Vector3F.UnitZ;
            }

            b1 = Vector3F.Cross(b0, axis);
            b2 = Vector3F.Cross(b0, b1);
          }
        }

        if (tangentsAndHandedness != null)
        {
          // Calculate handedness.
          Vector3F bi = Vector3F.Cross(b0, tan0);
          float w = (Vector3F.Dot(bi, tan1) < 0) ? 1.0f : 1.0f;
          tangentsAndHandedness[i] = new Vector4F(b1, w);
        }

        if (tangents != null)
          tangents[i] = b1;

        if (bitangents != null)
          bitangents[i] = b2;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshOptimize
    //--------------------------------------------------------------

    private class MeshStatus
    {
      private struct ListElement
      {
        public bool Processed;
        public int Unprocessed;
        public int Prev;
        public int Next;
      }

      private readonly int[] _unprocessed = new int[4];
      private int _faceOffset;
      private int _faceCount;
      private int _maxSubset;
      private int _totalFaces;
      private ListElement[] _listElements;
      private int[][] _physicalNeighbors; // int[numberOFaces][3]
      // int[3] neighbors = _physicalNeighbors[face].

      public void Initialize(IList<int> indices, IList<int> adjacency, IList<Pair<int, int>> subsets)
      {
        if (indices == null)
          throw new ArgumentNullException("indices");
        if (adjacency == null)
          throw new ArgumentNullException("adjacency");
        if (subsets == null)
          throw new ArgumentNullException("subsets");
        if (subsets.Count == 0)
          throw new ArgumentException("List of subsets must not be empty.", "subsets");

        int numberOfFaces = indices.Count / 3;

        // Convert adjacency to 'physical' adjacency
        _physicalNeighbors = new int[numberOfFaces][];
        for (int j = 0; j < _physicalNeighbors.Length; j++)
          _physicalNeighbors[j] = new int[3];

        _faceOffset = 0;
        _faceCount = 0;
        _maxSubset = 0;
        _totalFaces = numberOfFaces;

        foreach (var subset in subsets)
        {
          if ((long)subset.First + subset.Second >= int.MaxValue)
            throw new OverflowException("The range of the subset exceeds Int32.MaxValue.");


          if (subset.Second > _maxSubset)
            _maxSubset = subset.Second;

          int faceOffset = subset.First;
          int faceMax = subset.First + subset.Second;

          for (int face = faceOffset; face < faceMax; face++)
          {
            if (face >= numberOfFaces)
              throw new ArgumentOutOfRangeException("subsets", "The face index exceeds the total number of faces.");

            int i0 = indices[face * 3 + 0];
            int i1 = indices[face * 3 + 1];
            int i2 = indices[face * 3 + 2];

            if (i0 == -1 || i1 == -1 || i2 == -1
                || i0 == i1 || i0 == i2 || i1 == i2)
            {
              // Unused and degenerate faces should not have neighbors.
              for (int point = 0; point < 3; point++)
              {
                int k = adjacency[face * 3 + point];

                if (k != -1)
                {
                  if (k >= numberOfFaces)
                    throw new ArgumentOutOfRangeException("adjacency", "The face index exceeds the total number of faces.");

                  if (adjacency[k * 3 + 0] == face)
                    _physicalNeighbors[k][0] = -1;

                  if (adjacency[k * 3 + 1] == face)
                    _physicalNeighbors[k][1] = -1;

                  if (adjacency[k * 3 + 2] == face)
                    _physicalNeighbors[k][2] = -1;
                }

                _physicalNeighbors[face][point] = -1;
              }
            }
            else
            {
              for (int n = 0; n < 3; n++)
              {
                int neighbor = adjacency[face * 3 + n];

                if (neighbor != -1)
                {
                  if ((neighbor < faceOffset) || (neighbor >= faceMax)
                       || (neighbor == adjacency[face * 3 + ((n + 1) % 3)])
                       || (neighbor == adjacency[face * 3 + ((n + 2) % 3)]))
                  {
                    // Break links for any neighbors outside of our attribute set, and remove duplicate neighbors.
                    neighbor = -1;
                  }
                  else
                  {
                    int edgeBack = FindEdge(adjacency, neighbor * 3, face);
                    if (edgeBack < 3)
                    {
                      int p1 = indices[face * 3 + n];
                      int p2 = indices[face * 3 + ((n + 1) % 3)];

                      int pn1 = indices[neighbor * 3 + edgeBack];
                      int pn2 = indices[neighbor * 3 + ((edgeBack + 1) % 3)];

                      // if wedge not identical on shared edge, drop link
                      if ((p1 != pn2) || (p2 != pn1))
                      {
                        neighbor = -1;
                      }
                    }
                    else
                    {
                      neighbor = -1;
                    }
                  }
                }

                _physicalNeighbors[face][n] = neighbor;
              }
            }
          }
        }

        if (_maxSubset == 0)
          throw new Exception("MeshStatus initialization failed.");

        _listElements = new ListElement[_maxSubset];
        for (int j = 0; j < _listElements.Length; j++)
          _listElements[j] = new ListElement();
      }


      public void SetSubset(IList<int> indices, int faceOffset, int faceCount)
      {
        if (faceCount == 0)
          throw new ArgumentOutOfRangeException("faceCount", "faceCount must not be 0.");
        if (indices == null)
          throw new ArgumentNullException("indices");
        if (indices.Count == 0)
          throw new ArgumentException("Indices must not be empty.", "indices");

        if (faceCount > _maxSubset)
          throw new ArgumentOutOfRangeException("faceCount");

        if (_listElements == null)
          throw new InvalidOperationException("_listElements is not initialized.");

        if ((long)(faceOffset) + faceCount >= int.MaxValue)
          throw new OverflowException("The range of the subset exceeds Int32.MaxValue.");

        int numberOfFaces = indices.Count / 3;
        int faceMax = faceOffset + faceCount;

        if (faceMax > numberOfFaces)
          throw new ArgumentException("The range of the subset exceeds the total number of faces.");

        _faceOffset = faceOffset;
        _faceCount = faceCount;

        _unprocessed[0] = -1;
        _unprocessed[1] = -1;
        _unprocessed[2] = -1;
        _unprocessed[3] = -1;

        for (int face = faceOffset; face < faceMax; face++)
        {
          int i0 = indices[face * 3 + 0];
          int i1 = indices[face * 3 + 1];
          int i2 = indices[face * 3 + 2];

          if (i0 == -1 || i1 == -1 || i2 == -1)
          {
            // filter out unused triangles
            continue;
          }

          int unprocessed = 0;

          for (int n = 0; n < 3; n++)
          {
            if (_physicalNeighbors[face][n] != -1)
            {
              unprocessed += 1;

              Debug.Assert(_physicalNeighbors[face][n] >= _faceOffset);
              Debug.Assert(_physicalNeighbors[face][n] < faceMax);
            }
          }

          int faceIndex = face - faceOffset;
          _listElements[faceIndex].Processed = false;
          _listElements[faceIndex].Unprocessed = unprocessed;

          PushFront(faceIndex);
        }
      }


      public bool IsProcessed(int face)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert(face >= _faceOffset || face < _faceOffset + _faceCount);
        return _listElements[face - _faceOffset].Processed;
      }


      public int UnprocessedCount(int face)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert(face >= _faceOffset || face < _faceOffset + _faceCount);
        return _listElements[face - _faceOffset].Unprocessed;
      }


      public int FindInitial()
      {
        for (int j = 0; j < 4; j++)
        {
          if (_unprocessed[j] != -1)
            return _unprocessed[j] + _faceOffset;
        }

        return -1;
      }


      public void Mark(int face)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert(face >= _faceOffset || face < _faceOffset + _faceCount);

        int faceIndex = face - _faceOffset;

        Debug.Assert(!_listElements[faceIndex].Processed);
        _listElements[faceIndex].Processed = true;

        Remove(faceIndex);

        for (int n = 0; n < 3; n++)
        {
          int neighbor = _physicalNeighbors[face][n];
          if ((neighbor != -1) && !IsProcessed(neighbor))
            Decrement(neighbor);
        }
      }


      public int FindNext(int face)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert((face >= _faceOffset) || (face < (_faceOffset + _faceCount)));

        int iret = 3;
        int minNeighbor = -1;
        int minNextNeighbor = 0;

        for (int n = 0; n < 3; n++)
        {
          int neighbor = _physicalNeighbors[face][n];

          if ((neighbor == -1) || IsProcessed(neighbor))
            continue;

          int unprocessed = UnprocessedCount(neighbor);
          Debug.Assert(unprocessed < 3);

          int mintemp = -1;

          for (int nt = 0; nt < 3; nt++)
          {
            int neighborTemp = _physicalNeighbors[neighbor][nt];

            if ((neighborTemp == -1) || IsProcessed(neighborTemp))
              continue;

            int next_count = UnprocessedCount(neighborTemp);
            if (next_count < mintemp)
              mintemp = next_count;
          }

          if (mintemp == -1)
            mintemp = 0;

          if (unprocessed < minNeighbor)
          {
            iret = n;
            minNeighbor = unprocessed;
            minNextNeighbor = mintemp;
          }
          else if ((unprocessed == minNeighbor) && (mintemp < minNextNeighbor))
          {
            iret = n;
            minNextNeighbor = mintemp;
          }
        }

        return iret;
      }


      public int GetNeighbor(int face, int n)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert(n < 3);
        return _physicalNeighbors[face][n];
      }


      public int[] GetNeighbors(int face)
      {
        Debug.Assert(face < _totalFaces);
        return _physicalNeighbors[face];
      }


      private void PushFront(int faceIndex)
      {
        Debug.Assert(faceIndex < _faceCount);

        int unprocessed = _listElements[faceIndex].Unprocessed;

        int head = _unprocessed[unprocessed];
        _listElements[faceIndex].Next = head;

        if (head != -1)
          _listElements[head].Prev = faceIndex;

        _unprocessed[unprocessed] = faceIndex;

        _listElements[faceIndex].Prev = -1;
      }


      private void Remove(int faceIndex)
      {
        Debug.Assert(faceIndex < _faceCount);

        if (_listElements[faceIndex].Prev != -1)
        {
          Debug.Assert(_unprocessed[_listElements[faceIndex].Unprocessed] != faceIndex);

          int prev = _listElements[faceIndex].Prev;
          int next = _listElements[faceIndex].Next;

          _listElements[prev].Next = next;

          if (next != -1)
          {
            _listElements[next].Prev = prev;
          }
        }
        else
        {
          // remove head of the list
          Debug.Assert(_unprocessed[_listElements[faceIndex].Unprocessed] == faceIndex);

          int unprocessed = _listElements[faceIndex].Unprocessed;

          _unprocessed[unprocessed] = _listElements[faceIndex].Next;

          if (_unprocessed[unprocessed] != -1)
          {
            _listElements[_unprocessed[unprocessed]].Prev = -1;
          }
        }

        _listElements[faceIndex].Prev =
        _listElements[faceIndex].Next = -1;
      }

      private void Decrement(int face)
      {
        Debug.Assert(face < _totalFaces);
        Debug.Assert(face >= _faceOffset || face < (_faceOffset + _faceCount));
        Debug.Assert(!IsProcessed(face));

        int faceIndex = face - _faceOffset;

        Debug.Assert(_listElements[faceIndex].Unprocessed >= 1 && _listElements[faceIndex].Unprocessed <= 3);

        Remove(faceIndex);

        _listElements[faceIndex].Unprocessed -= 1;

        PushFront(faceIndex);
      }
    }


    private static Pair<int, int> CounterClockwiseCorner(Pair<int, int> corner, MeshStatus status)
    {
      Debug.Assert(corner.Second != -1);
      int edge = (corner.Second + 2) % 3;
      int neighbor = status.GetNeighbor(corner.First, edge);
      int point = (neighbor == -1) ? -1 : FindEdge(status.GetNeighbors(neighbor), 0, corner.First);
      return new Pair<int, int>(neighbor, point);
    }


    private class SimVCache
    {
      private int _tail;
      private int[] _fifo;


      public void Initialize(int cacheSize)
      {
        if (cacheSize == 0)
          throw new ArgumentOutOfRangeException("cacheSize");

        _fifo = new int[cacheSize];

        Clear();
      }


      public void Clear()
      {
        Debug.Assert(_fifo != null);
        _tail = 0;
        for (int i = 0; i < _fifo.Length; i++)
          _fifo[i] = -1;
      }

      public bool Access(int vertex)
      {
        Debug.Assert(vertex != -1);
        Debug.Assert(_fifo != null);

        for (int ptr = 0; ptr < _fifo.Length; ptr++)
          if (_fifo[ptr] == vertex)
            return true;

        _fifo[_tail] = vertex;
        _tail += 1;
        if (_tail == _fifo.Length)
          _tail = 0;

        return false;
      }
    }


    private static void StripReorder(IList<int> indices, IList<int> adjacency, IList<int> attributes, out int[] faceRemap)
    {
      int numberOfFaces = indices.Count / 3;
      var subsets = ComputeSubsets(attributes, numberOfFaces);

      Debug.Assert(subsets != null && subsets.Count != 0);

      MeshStatus status = new MeshStatus();
      status.Initialize(indices, adjacency, subsets);

      int[] faceRemapInverse = new int[numberOfFaces];
      for (int j = 0; j < faceRemapInverse.Length; j++)
        faceRemapInverse[j] = -1;

      foreach (var subset in subsets)
      {
        status.SetSubset(indices, subset.First, subset.Second);

        int curface = 0;

        for (;;)
        {
          int face = status.FindInitial();
          if (face == -1)
            break;

          status.Mark(face);

          int next = status.FindNext(face);

          for (;;)
          {
            Debug.Assert(face != -1);
            faceRemapInverse[face] = curface + subset.First;
            curface += 1;

            // if at end of strip, break out
            if (next >= 3)
              break;

            face = status.GetNeighbor(face, next);
            Debug.Assert(face != -1);

            status.Mark(face);

            next = status.FindNext(face);
          }
        }
      }

      // Inverse remap.
      faceRemap = new int[numberOfFaces];
      for (int j = 0; j < faceRemap.Length; j++)
        faceRemap[j] = -1;

      for (int j = 0; j < numberOfFaces; j++)
      {
        int f = faceRemapInverse[j];
        if ((uint)f < numberOfFaces)
          faceRemap[f] = j;
      }
    }


    private static void VertexCacheStripReorder(IList<int> indices, IList<int> adjacency, IList<int> attributes, out int[] faceRemap, int vertexCache, int restart)
    {
      int nFaces = indices.Count / 3;
      var subsets = ComputeSubsets(attributes, nFaces);

      Debug.Assert(subsets != null && subsets.Count != 0);

      var status = new MeshStatus();
      status.Initialize(indices, adjacency, subsets);

      var vcache = new SimVCache();
      vcache.Initialize(vertexCache);

      int[] faceRemapInverse = new int[nFaces];
      for (int j = 0; j < faceRemapInverse.Length; j++)
        faceRemapInverse[j] = -1;

      Debug.Assert(vertexCache >= restart);
      int desired = vertexCache - restart;

      foreach (var subset in subsets)
      {
        status.SetSubset(indices, subset.First, subset.Second);

        vcache.Clear();

        int locnext = 0;
        var nextCorner = new Pair<int, int>(-1, -1);
        var curCorner = new Pair<int, int>(-1, -1);

        int curface = 0;

        for (;;)
        {
          Debug.Assert(nextCorner.First == -1);

          curCorner.First = status.FindInitial();
          if (curCorner.First == -1)
            break;

          int n0 = status.GetNeighbor(curCorner.First, 0);
          if ((n0 != -1) && !status.IsProcessed(n0))
          {
            curCorner.Second = 1;
          }
          else
          {
            int n1 = status.GetNeighbor(curCorner.First, 1);
            if ((n1 != -1) && !status.IsProcessed(n1))
            {
              curCorner.Second = 2;
            }
            else
            {
              curCorner.Second = 0;
            }
          }

          bool striprestart = false;
          for (;;)
          {
            Debug.Assert(curCorner.First != -1);
            Debug.Assert(!status.IsProcessed(curCorner.First));

            // Decision: either add a ring of faces or restart strip
            if (nextCorner.First != -1)
            {
              int nf = 0;
              for (Pair<int, int> temp = curCorner;;)
              {
                Pair<int, int> next = CounterClockwiseCorner(temp, status);
                if ((next.First == -1) || status.IsProcessed(next.First))
                  break;
                nf++;
                temp = next;
              }

              if (locnext + nf > desired)
              {
                // restart
                if (!status.IsProcessed(nextCorner.First))
                {
                  curCorner = nextCorner;
                }

                nextCorner.First = -1;
              }
            }

            for (;;)
            {
              Debug.Assert(curCorner.First != -1);
              status.Mark(curCorner.First);

              faceRemapInverse[curCorner.First] = curface + subset.First;
              curface += 1;

              Debug.Assert(indices[curCorner.First * 3] != -1);
              if (!vcache.Access(indices[curCorner.First * 3]))
                locnext += 1;

              Debug.Assert(indices[curCorner.First * 3 + 1] != -1);
              if (!vcache.Access(indices[curCorner.First * 3 + 1]))
                locnext += 1;

              Debug.Assert(indices[curCorner.First * 3 + 2] != -1);
              if (!vcache.Access(indices[curCorner.First * 3 + 2]))
                locnext += 1;

              var intCorner = CounterClockwiseCorner(curCorner, status);
              bool interiornei = (intCorner.First != -1) && !status.IsProcessed(intCorner.First);

              var extCorner = CounterClockwiseCorner(new Pair<int, int>(curCorner.First, (curCorner.Second + 2) % 3), status);
              bool exteriornei = (extCorner.First != -1) && !status.IsProcessed(extCorner.First);

              if (interiornei)
              {
                if (exteriornei)
                {
                  if (nextCorner.First == -1)
                  {
                    nextCorner = extCorner;
                    locnext = 0;
                  }
                }
                curCorner = intCorner;
              }
              else if (exteriornei)
              {
                curCorner = extCorner;
                break;
              }
              else
              {
                curCorner = nextCorner;
                nextCorner.First = -1;

                if ((curCorner.First == -1) || status.IsProcessed(curCorner.First))
                {
                  striprestart = true;
                  break;
                }
              }
            }

            if (striprestart)
              break;
          }
        }
      }

      // Inverse remap.
      faceRemap = new int[nFaces];
      for (int j = 0; j < faceRemap.Length; j++)
        faceRemap[j] = -1;

      for (int j = 0; j < nFaces; j++)
      {
        int f = faceRemapInverse[j];
        if ((uint)f < nFaces)
          faceRemap[f] = j;
      }
    }


    /// <summary>
    /// Reorders the faces grouping together all those that use the same attribute id.
    /// </summary>
    /// <param name="numberOfFaces">The number of faces in the mesh.</param>
    /// <param name="attributes">
    /// A 32-bit index array with numberOfFaces entries which contains the attribute id for each
    /// face in the mesh. The array is sorted by this function.
    /// </param>
    /// <param name="faceRemap">
    /// The array describing the reordering. See <see cref="ReorderIB(IList{int},int[],out int[])"/>
    /// and <see cref="ReorderIBAndAdjacency(IList{int},IList{int},int[],out int[],out int[])"/> for
    /// details.
    /// </param>
    /// <remarks>
    /// This function does not duplicate any vertices. The ideal attribute sort for vertex
    /// pre-transform cache optimization ensures that each vertex is only used once for a given
    /// attribute. This can be accomplished by calling <see cref="Clean"/> before doing this sort.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="attributes"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of attributes does not match the number of faces.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfFaces"/> is 0 or negative.
    /// </exception>
    public static void AttributeSort(int numberOfFaces, IList<int> attributes, out int[] faceRemap)
    {
      if (numberOfFaces <= 0)
        throw new ArgumentOutOfRangeException("numberOfFaces");
      if (attributes == null)
        throw new ArgumentNullException("attributes");
      if (attributes.Count != numberOfFaces)
        throw new ArgumentException("The number of attributes does not match the number of faces.");
      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      var list = new Pair<int, int>[numberOfFaces];
      for (int j = 0; j < numberOfFaces; j++)
        list[j] = new Pair<int, int>(attributes[j], j);

      // Use Enumerable.OrderBy for sorting because it is stable. (Array.Sort is unstable!)
      list = list.OrderBy(pair => pair.First).ToArray();

      faceRemap = new int[numberOfFaces];
      for (int j = 0; j < numberOfFaces; j++)
      {
        attributes[j] = list[j].First;
        faceRemap[j] = list[j].Second;
      }
    }


    /// <summary>
    /// Reorders faces to improve post-transform vertex cache reuse.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="attributes">
    /// A 32-bit index array with numberOfFaces entries which contains the attribute id for each
    /// face in the mesh. Can be <see langword="null"/>.
    /// </param>
    /// <param name="faceRemap">
    /// The array describing the reordering. See <see cref="ReorderIB(IList{int},int[],out int[])"/>
    /// and <see cref="ReorderIBAndAdjacency(IList{int},IList{int},int[],out int[],out int[])"/> for
    /// details.
    /// </param>
    /// <param name="vertexCache">
    /// The size of the vertex cache to assume for the optimization. If
    /// <see cref="OPTFACES_V_STRIPORDER"/> is provided, then the vertex cache simulation is not
    /// used and the faces are put in "strip order". This number should typically range from 0 to
    /// 32. The default value is <see cref="OPTFACES_V_DEFAULT"/>.
    /// </param>
    /// <param name="restart">
    /// The threshold used to control when strips are restarted based. This number must be less than
    /// or equal to <paramref name="vertexCache"/>, and is ignored
    /// <see cref="OPTFACES_V_STRIPORDER"/>. The default value is <see cref="OPTFACES_R_DEFAULT"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This implements the same algorithm as D3DX with explicit control over the simulated vertex
    /// cache size. OPTFACES_V_DEFAULT / OPTFACES_R_DEFAULT is the same value that D3DXOptimizeFaces
    /// used (i.e. D3DXMESHOPT_DEVICEINDEPENDENT).
    /// </para>
    /// <para>
    /// Some vendors support a Direct3D 9 query D3DQUERYTYPE_VCACHE that reports the vertex cache
    /// optimization settings that are device specific.
    /// </para>
    /// <para>
    /// Note that optimizing for a <paramref name="vertexCache"/> larger than is present on the
    /// hardware can result in poorer performance than the original mesh, so this value should be
    /// picked either for a known fixed device or conservatively.
    /// </para>
    /// <para>
    /// Degenerate and 'unused' faces are skipped by the optimization, so they do not appear in the
    /// remap order.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> or <paramref name="adjacency"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, a validation option is specified that requires <paramref name="adjacency"/> which is
    /// missing.<br/>
    /// Or, the length of <paramref name="adjacency"/> does not match <paramref name="indices"/>.
    /// </exception>
    public static void OptimizeFaces(IList<int> indices, IList<int> adjacency, IList<int> attributes, out int[] faceRemap, int vertexCache = OPTFACES_V_DEFAULT, int restart = OPTFACES_R_DEFAULT)
    {
      // References:
      // - http://www.opengl.org/wiki/Post_Transform_Cache
      // - Hoppe, H.: "Optimization of mesh locality for transparent vertex caching",
      //   ACM SIGGRAPH 1999 Proceedings, http://research.microsoft.com/en-us/um/people/hoppe/proj/tvc/

      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (adjacency == null)
        throw new ArgumentNullException("adjacency");
      if (adjacency.Count != indices.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacency.Count, indices.Count);
        throw new ArgumentException(message, "adjacency");
      }

      int numberOfFaces = indices.Count / 3;
      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      if (vertexCache == OPTFACES_V_STRIPORDER)
      {
        StripReorder(indices, adjacency, attributes, out faceRemap);
      }
      else
      {
        if (restart > vertexCache)
          throw new ArgumentOutOfRangeException("restart", "The restart threshold must be less than or equal to the vertex cache size.");

        VertexCacheStripReorder(indices, adjacency, attributes, out faceRemap, vertexCache, restart);
      }
    }


    /// <summary>
    /// Reorders vertices in order of use by the index buffer which optimizes for the vertex shader
    /// pre-transform cache.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="vertexRemap">
    /// The array describing the reordering. See
    /// <see cref="FinalizeIB(IList{int},int[],out int[])"/>,
    /// <see cref="FinalizeVB(byte[],int,int,int[],int[],out byte[])"/>, and
    /// <see cref="FinalizeVBAndPointReps(byte[],int,int,int[],int[],int[],out byte[],out int[])"/>
    /// for details.
    /// </param>
    /// <remarks>
    /// Any 'unused' vertices are eliminated and the extra space is left at the end of the vertex
    /// buffer when applied.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> is out of range.
    /// </exception>
    public static void OptimizeVertices(IList<int> indices, int numberOfVertices, out int[] vertexRemap)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices", "The number of vertices must not be 0 or negative.");

      int numberOfFaces = indices.Count / 3;
      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      int[] tempRemap = new int[numberOfVertices];
      for (int i = 0; i < tempRemap.Length; i++)
        tempRemap[i] = -1;

      int curvertex = 0;
      for (int j = 0; j < numberOfFaces * 3; j++)
      {
        int curindex = indices[j];
        if (curindex == -1)
          continue;

        if (curindex >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        if (tempRemap[curindex] == -1)
        {
          tempRemap[curindex] = curvertex;
          curvertex++;
        }
      }

      vertexRemap = tempRemap;

      /* [DIGITALRUNE] FIX: DirectXMesh inverts the lookup, which does not make sense.

      // Inverse lookup.
      vertexRemap = new int[numberOfVertices];
      for (int i = 0; i < vertexRemap.Length; i++)
        vertexRemap[i] = -1;

      for (int j = 0; j < numberOfVertices; j++)
      {
        int vertindex = tempRemap[j];
        if (vertindex != -1)
          vertexRemap[vertindex] = j;
      }
      */
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshRemap
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Reorders a 32-bit index buffer based on a face remap array.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Reorders a 32-bit index buffer based on a face remap array.
    /// </summary>
    /// <param name="ibIn">The index buffer.</param>
    /// <param name="faceRemap">
    /// An array with one entry per face that describes how to reorder the faces of the original
    /// mesh. For each face in the optimized mesh, it provides the original location of that face.
    /// See <see cref="AttributeSort"/> and <see cref="OptimizeFaces"/>.
    /// </param>
    /// <param name="ibOut">The reordered index buffer.</param>
    /// <remarks>
    /// This is the pseudo-code of how to apply a face remap.
    /// <code lang="none">
    /// <![CDATA[
    /// for each j in nFaces
    ///   origFace = faceRemap[ j ]
    ///   if ( origFace != -1 )
    ///     for each i in 0..2
    ///       newIndices[ j*3 + i ] = indices[ origFace*3 + i ]
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ibIn"/> or <paramref name="faceRemap"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// The length of <paramref name="faceRemap"/> does not match the number of faces.
    /// </exception>
    public static void ReorderIB(IList<int> ibIn, int[] faceRemap, out int[] ibOut)
    {
      if (ibIn == null)
        throw new ArgumentNullException("ibIn");
      if (ibIn.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ibIn");
      if (ibIn.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ibIn");
      if (faceRemap == null)
        throw new ArgumentNullException("faceRemap");

      int numberOfFaces = ibIn.Count / 3;
      if (faceRemap.Length != numberOfFaces)
        throw new ArgumentException("The length of faceRemap does not match the number of faces.");

      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      int[] dummy;
      ReorderFaces(ibIn, null, faceRemap, out ibOut, out dummy);
    }


    /// <summary>
    /// Reorders a 32-bit index buffer in-place based on a face remap array.
    /// </summary>
    /// <param name="ib">The index buffer.</param>
    /// <param name="faceRemap">
    /// An array with one entry per face that describes how to reorder the faces of the original
    /// mesh. For each face in the optimized mesh, it provides the original location of that face.
    /// See <see cref="AttributeSort"/> and <see cref="OptimizeFaces"/>.
    /// </param>
    /// <remarks>
    /// This is the pseudo-code of how to apply a face remap.
    /// <code lang="none">
    /// <![CDATA[
    /// for each j in nFaces
    ///   origFace = faceRemap[ j ]
    ///   if ( origFace != -1 )
    ///     for each i in 0..2
    ///       newIndices[ j*3 + i ] = indices[ origFace*3 + i ]
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ib"/> or <paramref name="faceRemap"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// The length of <paramref name="faceRemap"/> does not match the number of faces.
    /// </exception>
    public static void ReorderIB(IList<int> ib, int[] faceRemap)
    {
      if (ib == null)
        throw new ArgumentNullException("ib");
      if (ib.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ib");
      if (ib.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ib");
      if (faceRemap == null)
        throw new ArgumentNullException("faceRemap");

      int numberOfFaces = ib.Count / 3;
      if (faceRemap.Length != numberOfFaces)
        throw new ArgumentException("The length of faceRemap does not match the number of faces.");

      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      SwapFaces(ib, null, faceRemap);
    }


    /// <overloads>
    /// <summary> Reorders a 32-bit index buffer and adjacency based on a face remap array.
    /// </summary>
    /// </overloads>
    /// <summary>
    /// Reorders a 32-bit index buffer and adjacency based on a face remap array.
    /// </summary>
    /// <param name="ibIn">The index buffer.</param>
    /// <param name="adjacencyIn">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="faceRemap">
    /// An array with one entry per face that describes how to reorder the faces of the original
    /// mesh. For each face in the optimized mesh, it provides the original location of that face.
    /// See <see cref="AttributeSort"/> and <see cref="OptimizeFaces"/>.
    /// </param>
    /// <param name="ibOut">The reordered index buffer.</param>
    /// <param name="adjacencyOut">The reordered adjacency.</param>
    /// <remarks>
    /// This is the pseudo-code of how to apply a face remap.
    /// <code lang="none">
    /// <![CDATA[
    /// for each j in nFaces
    ///   origFace = faceRemap[ j ]
    ///   if (origFace != -1)
    ///     for each i in 0..2
    ///       newAdjacency[ j*3 + i ] = adjacency[ origFace*3 + i ]
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ibIn"/>, <paramref name="adjacencyIn"/>, or <paramref name="faceRemap"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the length of <paramref name="adjacencyIn"/> does not match <paramref name="ibIn"/>.<br/>
    /// Or, the length of <paramref name="faceRemap"/> does not match the number of faces.
    /// </exception>
    public static void ReorderIBAndAdjacency(IList<int> ibIn, IList<int> adjacencyIn, int[] faceRemap, out int[] ibOut, out int[] adjacencyOut)
    {
      if (ibIn == null)
        throw new ArgumentNullException("ibIn");
      if (ibIn.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ibIn");
      if (ibIn.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ibIn");
      if (adjacencyIn == null)
        throw new ArgumentNullException("adjacencyIn");
      if (adjacencyIn.Count != ibIn.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacencyIn.Count, ibIn.Count);
        throw new ArgumentException(message, "adjacencyIn");
      }
      if (faceRemap == null)
        throw new ArgumentNullException("faceRemap");

      int numberOfFaces = ibIn.Count / 3;
      if (faceRemap.Length != numberOfFaces)
        throw new ArgumentException("The length of faceRemap does not match the number of faces.");

      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      ReorderFaces(ibIn, adjacencyIn, faceRemap, out ibOut, out adjacencyOut);
    }


    /// <summary>
    /// Reorders a 32-bit index buffer and adjacency in-place based on a face remap array.
    /// </summary>
    /// <param name="ib">The index buffer.</param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="faceRemap">
    /// An array with one entry per face that describes how to reorder the faces of the original
    /// mesh. For each face in the optimized mesh, it provides the original location of that face.
    /// See <see cref="AttributeSort"/> and <see cref="OptimizeFaces"/>.
    /// </param>
    /// <remarks>
    /// This is the pseudo-code of how to apply a face remap.
    /// <code lang="none">
    /// <![CDATA[
    /// for each j in nFaces
    ///   origFace = faceRemap[ j ]
    ///   if (origFace != -1)
    ///     for each i in 0..2
    ///       newAdjacency[ j*3 + i ] = adjacency[ origFace*3 + i ]
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ib"/>, <paramref name="adjacency"/>, or <paramref name="faceRemap"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, the length of <paramref name="adjacency"/> does not match <paramref name="ib"/>.<br/>
    /// Or, the length of <paramref name="faceRemap"/> does not match the number of faces.
    /// </exception>
    public static void ReorderIBAndAdjacency(IList<int> ib, IList<int> adjacency, int[] faceRemap)
    {
      if (ib == null)
        throw new ArgumentNullException("ib");
      if (ib.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ib");
      if (ib.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ib");
      if (adjacency == null)
        throw new ArgumentNullException("adjacency");
      if (adjacency.Count != ib.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacency.Count, ib.Count);
        throw new ArgumentException(message, "adjacency");
      }
      if (faceRemap == null)
        throw new ArgumentNullException("faceRemap");

      int numberOfFaces = ib.Count / 3;
      if (faceRemap.Length != numberOfFaces)
        throw new ArgumentException("The length of faceRemap does not match the number of faces.");

      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      SwapFaces(ib, adjacency, faceRemap);
    }


    private static void ReorderFaces(IList<int> ibin, IList<int> adjin, int[] faceRemap, out int[] ibout, out int[] adjout)
    {
      Debug.Assert(ibin != null && faceRemap != null);

      ibout = new int[ibin.Count];
      if (adjin != null)
        adjout = new int[adjin.Count];
      else
        adjout = null;

      int numberOfFaces = ibin.Count / 3;
      for (int j = 0; j < numberOfFaces; j++)
      {
        int src = faceRemap[j];

        if (src == -1)
          continue;

        if (src < numberOfFaces)
        {
          ibout[j * 3 + 0] = ibin[src * 3 + 0];
          ibout[j * 3 + 1] = ibin[src * 3 + 1];
          ibout[j * 3 + 2] = ibin[src * 3 + 2];

          if (adjin != null)
          {
            adjout[j * 3 + 0] = adjin[src * 3 + 0];
            adjout[j * 3 + 1] = adjin[src * 3 + 1];
            adjout[j * 3 + 2] = adjin[src * 3 + 2];
          }
        }
        else
        {
          throw new IndexOutOfRangeException("Face remap index exceeds number of faces.");
        }
      }
    }


    private static void SwapFaces(IList<int> ib, IList<int> adj, int[] faceRemap)
    {
      Debug.Assert(ib != null && faceRemap != null);

      int numberOfFaces = ib.Count / 3;

      int[] faceRemapInverse = new int[numberOfFaces];
      for (int j = 0; j < faceRemapInverse.Length; j++)
        faceRemapInverse[faceRemap[j]] = j;

      bool[] moved = new bool[numberOfFaces];

      for (int j = 0; j < numberOfFaces; j++)
      {
        if (moved[j])
          continue;

        int dest = faceRemapInverse[j];

        if (dest == -1)
          continue;

        if (dest >= numberOfFaces)
          throw new IndexOutOfRangeException("Index exceeds number of faces.");

        while (dest != j)
        {
          // Swap face
          int i0 = ib[dest * 3 + 0];
          int i1 = ib[dest * 3 + 1];
          int i2 = ib[dest * 3 + 2];

          ib[dest * 3 + 0] = ib[j * 3 + 0];
          ib[dest * 3 + 1] = ib[j * 3 + 1];
          ib[dest * 3 + 2] = ib[j * 3 + 2];

          ib[j * 3 + 0] = i0;
          ib[j * 3 + 1] = i1;
          ib[j * 3 + 2] = i2;

          if (adj != null)
          {
            int a0 = adj[dest * 3 + 0];
            int a1 = adj[dest * 3 + 1];
            int a2 = adj[dest * 3 + 2];

            adj[dest * 3 + 0] = adj[j * 3 + 0];
            adj[dest * 3 + 1] = adj[j * 3 + 1];
            adj[dest * 3 + 2] = adj[j * 3 + 2];

            adj[j * 3 + 0] = a0;
            adj[j * 3 + 1] = a1;
            adj[j * 3 + 2] = a2;
          }

          moved[dest] = true;

          dest = faceRemapInverse[dest];

          if (dest == -1 || moved[dest])
            break;

          if (dest >= numberOfFaces)
            throw new IndexOutOfRangeException("Index exceeds number of faces.");
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Finishes mesh optimization by updating an index buffer based on a vertex remap.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Finishes mesh optimization by updating an index buffer based on a vertex remap.
    /// </summary>
    /// <param name="ibIn">The index buffer.</param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the original
    /// mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <param name="ibOut">The finalized index buffer.</param>
    /// <remarks>
    /// <para>
    /// This should be done after all required face reordering. See
    /// <see cref="ReorderIB(System.Collections.Generic.IList{int},int[],out int[])"/> and
    /// <see cref="ReorderIBAndAdjacency(System.Collections.Generic.IList{int},System.Collections.Generic.IList{int},int[],out int[],out int[])"/>.
    /// </para>
    /// <para>
    /// This is the pseudo-code for how to apply a vertex remap to an index buffer:
    /// </para>
    /// <code lang="none">
    /// <![CDATA[
    /// for each i in ( nFaces * 3 )
    ///   newIndices[ i ] = vertexRemap[ indices[ i ] ]
    ///  ]]>
    ///  </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ibIn"/> or <paramref name="vertexRemap"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// <paramref name="vertexRemap"/> is empty.
    /// </exception>
    public static void FinalizeIB(IList<int> ibIn, int[] vertexRemap, out int[] ibOut)
    {
      if (ibIn == null)
        throw new ArgumentNullException("ibIn");
      if (ibIn.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ibIn");
      if (ibIn.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ibIn");
      if (vertexRemap == null)
        throw new ArgumentNullException("vertexRemap");
      if (vertexRemap.Length == 0)
        throw new ArgumentException("vertexRemap must not be empty.", "vertexRemap");

      int numberOfVertices = vertexRemap.Length;
      int numberOfFaces = ibIn.Count / 3;
      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      ibOut = new int[ibIn.Count];

      for (int j = 0; j < numberOfFaces * 3; j++)
      {
        int i = ibIn[j];
        if (i == -1)
        {
          ibOut[j] = -1;
          continue;
        }

        if (i >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        int dest = vertexRemap[i];
        if (dest == -1)
        {
          ibOut[j] = i;
          continue;
        }

        if (dest < numberOfVertices)
        {
          ibOut[j] = dest;
        }
        else
        {
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }
      }
    }


    /// <summary>
    /// Finishes mesh optimization by updating an index buffer in-place based on a vertex remap.
    /// </summary>
    /// <param name="ib">The index buffer.</param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the original
    /// mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This should be done after all required face reordering. See
    /// <see cref="ReorderIB(System.Collections.Generic.IList{int},int[],out int[])"/> and
    /// <see cref="ReorderIBAndAdjacency(System.Collections.Generic.IList{int},System.Collections.Generic.IList{int},int[],out int[],out int[])"/>.
    /// </para>
    /// <para>
    /// This is the pseudo-code for how to apply a vertex remap to an index buffer:
    /// </para>
    /// <code lang="none">
    /// <![CDATA[
    /// for each i in ( nFaces * 3 )
    ///   newIndices[ i ] = vertexRemap[ indices[ i ] ]
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="ib"/> or <paramref name="vertexRemap"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// <paramref name="vertexRemap"/> is empty.
    /// </exception>
    public static void FinalizeIB(IList<int> ib, int[] vertexRemap)
    {
      if (ib == null)
        throw new ArgumentNullException("ib");
      if (ib.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "ib");
      if (ib.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "ib");
      if (vertexRemap == null)
        throw new ArgumentNullException("vertexRemap");
      if (vertexRemap.Length == 0)
        throw new ArgumentException("vertexRemap must not be empty.", "vertexRemap");

      int numberOfVertices = vertexRemap.Length;
      int numberOfFaces = ib.Count / 3;
      if ((long)numberOfFaces * 3 >= int.MaxValue)
        throw new OverflowException("The total number of indices exceeds Int32.MaxValue.");

      for (int j = 0; j < numberOfFaces * 3; j++)
      {
        int i = ib[j];
        if (i == -1)
          continue;

        if (i >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        int dest = vertexRemap[i];
        if (dest == -1)
          continue;

        if (dest < numberOfVertices)
        {
          ib[j] = dest;
        }
        else
        {
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated
    /// vertices for the vertex buffer.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated vertices for the
    /// vertex buffer.
    /// </summary>
    /// <param name="vbIn">The vertex buffer.</param>
    /// <param name="stride">The vertex stride (= size of a vertex) in bytes.</param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="duplicateVertices">
    /// An array containing an entry for each vertex that needs duplicating. Each entry indicates
    /// the index of the original vertex buffer to duplicate. The <paramref name="vertexRemap"/>
    /// array also indicates reorder information for the duplicated vertices.
    /// </param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the
    /// original mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <param name="vbOut">The finalized vertex buffer.</param>
    /// <remarks>
    /// <para>
    /// This is the pseudo-code for applying a vertex remap to a vertex buffer:
    /// </para>
    /// <code lang="none">
    /// <![CDATA[
    /// for each j in nVerts
    ///   newIndex = vertexRemap[j]
    ///   if (newIndex != -1)
    ///     memcpy(newVB + newIndex * stride,
    ///            oldVB + j * stride,
    ///            stride)
    /// 
    /// for each j in nDupVerts
    ///   newIndex = vertexRemap[j + nVerts]
    ///   if (newIndex != -1)
    ///     memcpy(newVB + newIndex * stride,
    ///            oldVB + dup[j] * stride,
    ///            stride)
    /// ]]>
    /// </code>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vbIn"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vbIn"/> does not match <paramref name="stride"/> and
    /// <paramref name="numberOfVertices"/>.<br/>
    /// Or, <paramref name="duplicateVertices"/> is provided but is empty.<br/>
    /// Or, <paramref name="duplicateVertices"/> and <paramref name="vertexRemap"/> are both
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="stride"/> or <paramref name="numberOfVertices"/> is 0 or negative.
    /// </exception>
    public static void FinalizeVB(byte[] vbIn, int stride, int numberOfVertices, int[] duplicateVertices, int[] vertexRemap, out byte[] vbOut)
    {
      if (vbIn == null)
        throw new ArgumentNullException("vbIn");
      if (stride <= 0)
        throw new ArgumentOutOfRangeException("stride");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices");
      if (vbIn.Length != stride * numberOfVertices)
        throw new ArgumentException("Vertex stride and number of vertices does not match size of vertex buffer.");

      if (duplicateVertices == null && vertexRemap == null)
        throw new ArgumentException("Either duplicateVertices or vertexRemap needs to be provided.");

      int nDupVerts = 0;
      if (duplicateVertices != null)
      {
        nDupVerts = duplicateVertices.Length;
        if (nDupVerts == 0)
          throw new ArgumentException("When duplicateVertices is provided, it must not be empty.", "duplicateVertices");
      }

      if (stride > 2048) // D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES
        throw new ArgumentOutOfRangeException("stride", "Vertex stride must not exceed D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES.");

      if ((long)numberOfVertices + nDupVerts >= int.MaxValue)
        throw new OverflowException("The total number of vertices exceeds Int32.MaxValue.");

      int newVerts = numberOfVertices + nDupVerts;
      vbOut = new byte[newVerts * stride];

      for (int j = 0; j < numberOfVertices; j++)
      {
        int dest = (vertexRemap != null) ? vertexRemap[j] : j;

        if (dest == -1)
        {
          // Remap entry is unused.
        }
        else if (dest < newVerts)
        {
          Buffer.BlockCopy(vbIn, j * stride, vbOut, dest * stride, stride);
        }
        else
        {
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }
      }

      if (duplicateVertices != null)
      {
        for (int j = 0; j < nDupVerts; j++)
        {
          int dup = duplicateVertices[j];
          int dest = (vertexRemap != null) ? vertexRemap[numberOfVertices + j] : numberOfVertices + j;

          if (dest == -1)
          {
            // Remap entry is unused.
          }
          else if (dup < numberOfVertices && dest < newVerts)
          {
            Buffer.BlockCopy(vbIn, dup * stride, vbOut, dest * stride, stride);
          }
          else
          {
            throw new IndexOutOfRangeException("Index exceeds number of vertices.");
          }
        }
      }
    }


    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated vertices for the
    /// vertex buffer in-place.
    /// </summary>
    /// <param name="vb">The vertex buffer.</param>
    /// <param name="stride">The vertex stride (= size of a vertex) in bytes.</param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the
    /// original mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vb"/> or <paramref name="vertexRemap"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vb"/> does not match <paramref name="stride"/> and
    /// <paramref name="numberOfVertices"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="stride"/> or <paramref name="numberOfVertices"/> is 0 or negative.
    /// </exception>
    public static void FinalizeVB(byte[] vb, int stride, int numberOfVertices, int[] vertexRemap)
    {
      SwapVertices(vb, stride, numberOfVertices, null, vertexRemap);
    }


    private static void SwapVertices(byte[] vb, int stride, int numberOfVertices, int[] pointRep, int[] vertexRemap)
    {
      if (vb == null)
        throw new ArgumentNullException("vb");
      if (stride <= 0)
        throw new ArgumentOutOfRangeException("stride");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices");
      if (vb.Length != stride * numberOfVertices)
        throw new ArgumentException("Vertex stride and number of vertices does not match size of vertex buffer.");
      if (vertexRemap == null)
        throw new ArgumentNullException("vertexRemap");

      if (stride > 2048) // D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES
        throw new ArgumentOutOfRangeException("stride", "Vertex stride must not exceed D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES.");

      bool[] moved = new bool[numberOfVertices];
      byte[] vbtemp = new byte[stride];

      for (int j = 0; j < numberOfVertices; j++)
      {
        if (moved[j])
          continue;

        int dest = vertexRemap[j];

        if (dest == -1)
          continue;

        if (dest >= numberOfVertices)
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");

        bool next = false;

        while (dest != j)
        {
          // Swap vertex
          Buffer.BlockCopy(vb, dest * stride, vbtemp, 0, stride);
          Buffer.BlockCopy(vb, j * stride, vb, dest * stride, stride);
          Buffer.BlockCopy(vbtemp, 0, vb, j * stride, stride);

          if (pointRep != null)
          {
            MathHelper.Swap(ref pointRep[dest], ref pointRep[j]);

            // Remap
            int pr = pointRep[dest];
            if ((uint)pr < numberOfVertices)
            {
              pointRep[dest] = vertexRemap[pr];
            }
          }

          moved[dest] = true;

          dest = vertexRemap[dest];

          if (dest == -1 || moved[dest])
          {
            next = true;
            break;
          }

          if (dest >= numberOfVertices)
            throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }

        if (next)
          continue;

        if (pointRep != null)
        {
          // Remap
          int pr = pointRep[j];
          if ((uint)pr < numberOfVertices)
          {
            pointRep[j] = vertexRemap[pr];
          }
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated vertices for the
    /// vertex buffer, and the point representatives.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated vertices for the
    /// vertex buffer, and the point representatives.
    /// </summary>
    /// <param name="vbIn">The vertex buffer.</param>
    /// <param name="stride">The vertex stride (= size of a vertex) in bytes.</param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="pointRepIn">
    /// A 32-bit index array with numberOfVertices entries containing the point representatives for
    /// each vertex in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="duplicateVertices">
    /// An array containing an entry for each vertex that needs duplicating. Each entry indicates
    /// the index of the original vertex buffer to duplicate. The <paramref name="vertexRemap"/>
    /// array also indicates reorder information for the duplicated vertices.
    /// </param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the
    /// original mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <param name="vbOut">The finalized vertex buffer.</param>
    /// <param name="pointRepOut">The finalized point representation.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vbIn"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vbIn"/> does not match <paramref name="stride"/> and
    /// <paramref name="numberOfVertices"/>.<br/>
    /// Or, <paramref name="pointRepIn"/> does not match the number of vertices.<br/>
    /// Or, <paramref name="duplicateVertices"/> is provided but is empty.<br/>
    /// Or, <paramref name="duplicateVertices"/> and <paramref name="vertexRemap"/> are both
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="stride"/> or <paramref name="numberOfVertices"/> is 0 or negative.
    /// </exception>
    public static void FinalizeVBAndPointReps(byte[] vbIn, int stride, int numberOfVertices, int[] pointRepIn, int[] duplicateVertices, int[] vertexRemap, out byte[] vbOut, out int[] pointRepOut)
    {
      if (vbIn == null)
        throw new ArgumentNullException("vbIn");
      if (stride <= 0)
        throw new ArgumentOutOfRangeException("stride");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices");
      if (vbIn.Length != stride * numberOfVertices)
        throw new ArgumentException("Vertex stride and number of vertices does not match size of vertex buffer.");
      if (pointRepIn.Length != numberOfVertices)
        throw new ArgumentException("The length of pointRepIn does not match number of vertices.", "pointRepIn");

      if (duplicateVertices == null && vertexRemap == null)
        throw new ArgumentException("Either duplicateVertices or vertexRemap needs to be provided.");

      int nDupVerts = 0;
      if (duplicateVertices != null)
      {
        nDupVerts = duplicateVertices.Length;
        if (nDupVerts == 0)
          throw new ArgumentException("When duplicateVertices is provided, it must not be empty.", "duplicateVertices");
      }

      if (stride > 2048) // D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES
        throw new ArgumentOutOfRangeException("stride", "Vertex stride must not exceed D3D11_REQ_MULTI_ELEMENT_STRUCTURE_SIZE_IN_BYTES.");

      if ((long)numberOfVertices + nDupVerts >= int.MaxValue)
        throw new OverflowException("The total number of vertices exceeds Int32.MaxValue.");

      int newVerts = numberOfVertices + nDupVerts;
      vbOut = new byte[newVerts * stride];
      pointRepOut = new int[newVerts];

      int[] pointRep = new int[newVerts];
      pointRepIn.CopyTo(pointRep, 0);

      for (int i = 0; i < nDupVerts; i++)
        pointRep[i + numberOfVertices] = pointRepIn[duplicateVertices[i]];

      if (vertexRemap != null)
      {
        // Clean up point reps for any removed vertices.
        for (int i = 0; i < newVerts; i++)
        {
          if (vertexRemap[i] != -1)
          {
            int old = pointRep[i];
            if (old != -1 && vertexRemap[old] == -1)
            {
              pointRep[i] = i;

              for (int k = i + 1; k < newVerts; k++)
              {
                if (pointRep[k] == old)
                  pointRep[k] = i;
              }
            }
          }
        }
      }

      int j = 0;

      for (; j < numberOfVertices; j++)
      {
        int dest = (vertexRemap != null) ? vertexRemap[j] : j;

        if (dest == -1)
        {
          // Remap entry is unused.
        }
        else if (dest < newVerts)
        {
          Buffer.BlockCopy(vbIn, j * stride, vbOut, dest * stride, stride);

          int pr = pointRep[j];
          if (pr < newVerts)
          {
            pointRepOut[dest] = (vertexRemap != null) ? vertexRemap[pr] : pr;
          }
        }
        else
        {
          throw new IndexOutOfRangeException("Index exceeds number of vertices.");
        }
      }

      if (duplicateVertices != null)
      {
        for (int k = 0; k < nDupVerts; k++)
        {
          int dup = duplicateVertices[k];
          int dest = (vertexRemap != null) ? vertexRemap[numberOfVertices + k] : numberOfVertices + k;

          if (dest == -1)
          {
            // Remap entry is unused.
          }
          else if (dup < numberOfVertices && dest < newVerts)
          {
            Buffer.BlockCopy(vbIn, dup * stride, vbOut, dest * stride, stride);

            int pr = pointRep[numberOfVertices + k];
            if (pr < (numberOfVertices + nDupVerts))
              pointRepOut[dest] = (vertexRemap != null) ? vertexRemap[pr] : pr;
          }
          else
          {
            throw new IndexOutOfRangeException("Index exceeds number of vertices.");
          }
        }
      }
    }


    /// <summary>
    /// Finishes mesh optimization by reordering vertices and/or adding duplicated vertices for the
    /// vertex buffer, and the point representatives in-place.
    /// </summary>
    /// <param name="vb">The vertex buffer.</param>
    /// <param name="stride">The vertex stride (= size of a vertex) in bytes.</param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="pointRep">
    /// A 32-bit index array with numberOfVertices entries containing the point representatives for
    /// each vertex in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="vertexRemap">
    /// An array with one entry per vertex that describes how to reorder the vertices of the
    /// original mesh. This maps the original vertex location to the optimized location. See
    /// <see cref="OptimizeVertices"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vb"/>, <paramref name="pointRep"/>, or <paramref name="vertexRemap"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vb"/> does not match <paramref name="stride"/> and
    /// <paramref name="numberOfVertices"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="stride"/> or <paramref name="numberOfVertices"/> is 0 or negative.
    /// </exception>
    public static void FinalizeVBAndPointReps(byte[] vb, int stride, int numberOfVertices, int[] pointRep, int[] vertexRemap)
    {
      if (pointRep == null)
        throw new ArgumentNullException("pointRep");
      if (pointRep.Length != numberOfVertices)
        throw new ArgumentException("The length of pointRep does not match number of vertices.", "pointRep");
      if (vertexRemap == null)
        throw new ArgumentNullException("vertexRemap");

      // Clean up point reps for any removed vertices.
      for (int i = 0; i < numberOfVertices; i++)
      {
        if (vertexRemap[i] != -1)
        {
          int old = pointRep[i];
          if (old != -1 && vertexRemap[old] == -1)
          {
            pointRep[i] = i;

            for (int k = i + 1; k < numberOfVertices; k++)
            {
              if (pointRep[k] == old)
                pointRep[k] = i;
            }
          }
        }
      }

      SwapVertices(vb, stride, numberOfVertices, pointRep, vertexRemap);
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshValidate
    //--------------------------------------------------------------

    /// <summary>
    /// Checks whether the mesh is valid. Optionally includes diagnostic messages describing the
    /// problem(s) encountered.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="adjacency">
    /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for each
    /// face in a mesh. Can be -1 to indicate an unused entry.
    /// </param>
    /// <param name="options">The validation options.</param>
    /// <param name="messages">
    /// The list to which diagnostic messages are added. Can be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if mesh is valid; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Use <see cref="Clean"/> to fix most of these issues!
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.<br/>
    /// Or, a validation option is specified that requires <paramref name="adjacency"/> which is
    /// missing.<br/>
    /// Or, the length of <paramref name="adjacency"/> does not match <paramref name="indices"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> is out of range.
    /// </exception>
    public static bool Validate(IList<int> indices, int numberOfVertices, IList<int> adjacency, MeshValidationOptions options, IList<string> messages)
    {
      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices", "The number of vertices must not be 0 or negative.");
      if (adjacency != null && adjacency.Count != indices.Count)
      {
        string message = string.Format(
          CultureInfo.InvariantCulture,
          "The adjacency information ({0} entries) does not match the number of indices {1}.",
          adjacency.Count, indices.Count);
        throw new ArgumentException(message, "adjacency");
      }

      if (!ValidateIndices(indices, numberOfVertices, adjacency, options, messages))
        return false;

      if ((options & MeshValidationOptions.Bowties) != 0)
        if (ValidateNoBowties(indices, numberOfVertices, adjacency, messages))
          return false;

      return true;
    }


    private static bool ValidateIndices(IList<int> indices, int numberOfVertices, IList<int> adjacency, MeshValidationOptions options, IList<string> messages)
    {
      bool result = true;

      if (adjacency == null)
      {
        if ((options & MeshValidationOptions.Backfacing) != 0)
          throw new ArgumentException("Missing adjacency information required to check for MeshValidationOptions.Backfacing.");

        if ((options & MeshValidationOptions.AsymmetricAdjacency) != 0)
          throw new ArgumentException("Missing adjacency information required to check for MeshValidationOptions.AsymmetricAdjacency.");
      }

      int numberOfFaces = indices.Count / 3;
      for (int face = 0; face < numberOfFaces; face++)
      {
        // Check for values in-range.
        for (int point = 0; point < 3; point++)
        {
          int i = indices[face * 3 + point];
          if (i >= numberOfVertices)
          {
            if (messages == null)
              return false;

            result = false;

            string message = string.Format(CultureInfo.InvariantCulture, "An invalid index value ({0}) was found on face {1}.", i, face);
            messages.Add(message);
          }

          if (adjacency != null)
          {
            int j = adjacency[face * 3 + point];
            if (j >= numberOfFaces)
            {
              if (messages == null)
                return false;

              result = false;

              string message = string.Format(CultureInfo.InvariantCulture, "An invalid neighbor index value ({0}) was found on face {1}.", j, face);
              messages.Add(message);
            }
          }
        }

        // Check for unused faces.
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == -1 || i1 == -1 || i2 == -1)
        {
          if ((options & MeshValidationOptions.Unused) != 0)
          {
            if (i0 != -1 || i1 != -1 || i2 != -1)
            {
              if (messages == null)
                return false;

              result = false;

              string message = string.Format(CultureInfo.InvariantCulture,
                "An unused face ({0}) contains 'valid' but ignored vertices ({1}, {2}, {3}).", face, i0, i1, i2);
              messages.Add(message);
            }

            if (adjacency != null)
            {
              for (int point = 0; point < 3; point++)
              {
                int k = adjacency[face * 3 + point];
                if (k != -1)
                {
                  if (messages == null)
                    return false;

                  result = false;

                  string message = string.Format(CultureInfo.InvariantCulture,
                    "An unused face ({0}) has a neighbor {1}.", face, k);
                  messages.Add(message);
                }
              }
            }
          }

          // Ignore unused triangles for remaining tests.
          continue;
        }

        // Check for degenerate triangles.
        if (i0 == i1 || i0 == i2 || i1 == i2)
        {
          if ((options & MeshValidationOptions.Degenerate) != 0)
          {
            if (messages == null)
              return false;

            result = false;

            int bad;
            if (i0 == i1)
              bad = i0;
            else if (i1 == i2)
              bad = i2;
            else
              bad = i0;

            {
              string message = string.Format(CultureInfo.InvariantCulture, "A point ({0}) was found more than once in triangle {1}.", bad, face);
              messages.Add(message);
            }

            if (adjacency != null)
            {
              for (int point = 0; point < 3; point++)
              {
                int k = adjacency[face * 3 + point];
                if (k != -1)
                {
                  result = false;

                  string message = string.Format(CultureInfo.InvariantCulture, "A degenerate face ({0}) has a neighbor {1}.", face, k);
                  messages.Add(message);
                }
              }
            }
          }

          // Ignore degenerate triangles for remaining tests.
          continue;
        }


        // Check for symmetric neighbors.
        if ((options & MeshValidationOptions.AsymmetricAdjacency) != 0 && adjacency != null)
        {
          for (int point = 0; point < 3; point++)
          {
            int k = adjacency[face * 3 + point];
            if (k == -1)
              continue;

            Debug.Assert(k < numberOfFaces);

            int edge = FindEdge(adjacency, k * 3, face);
            if (edge >= 3)
            {
              if (messages == null)
                return false;

              result = false;

              string message = string.Format(CultureInfo.InvariantCulture, "A neighbor triangle ({0}) does not reference back to face ({1}) as expected.", k, face);
              messages.Add(message);
            }
          }
        }

        // Check for duplicate neighbor.
        if ((options & MeshValidationOptions.Backfacing) != 0 && adjacency != null)
        {
          int j0 = adjacency[face * 3 + 0];
          int j1 = adjacency[face * 3 + 1];
          int j2 = adjacency[face * 3 + 2];

          if ((j0 == j1 && j0 != -1)
              || (j0 == j2 && j0 != -1)
              || (j1 == j2 && j1 != -1))
          {
            if (messages == null)
              return false;

            result = false;

            int bad;
            if (j0 == j1 && j0 != -1)
              bad = j0;
            else if (j0 == j2 && j0 != -1)
              bad = j0;
            else
              bad = j1;

            string message = string.Format(
              CultureInfo.InvariantCulture,
              "A neighbor triangle ({0}) was found more than once on triangle {1} " +
              "(likely problem is that two triangles share same points with opposite direction).", bad, face);
            messages.Add(message);
          }
        }
      }

      return result;
    }


    private static bool ValidateNoBowties(IList<int> indices, int numberOfVertices, IList<int> adjacency, IList<string> messages)
    {
      if (adjacency == null)
        throw new ArgumentException("Missing adjacency information required to check for MeshValidationOptions.Bowties.");

      int numberOfIndices = indices.Count;
      int numberOfFaces = numberOfIndices / 3;

      bool[] faceSeen = new bool[numberOfIndices];
      int[] faceIds = new int[numberOfVertices];
      for (int j = 0; j < faceIds.Length; j++)
        faceIds[j] = -1;
      int[] faceUsing = new int[numberOfVertices];
      bool[] vertexBowtie = new bool[numberOfVertices];

      var ovi = new OrbitIterator(indices, adjacency);

      bool result = true;

      for (int face = 0; face < numberOfFaces; face++)
      {
        int i0 = indices[face * 3 + 0];
        int i1 = indices[face * 3 + 1];
        int i2 = indices[face * 3 + 2];

        if (i0 == i1 || i0 == i2 || i1 == i2)
        {
          // Ignore degenerate faces.
          faceSeen[face * 3 + 0] = true;
          faceSeen[face * 3 + 1] = true;
          faceSeen[face * 3 + 2] = true;
          continue;
        }

        for (int point = 0; point < 3; point++)
        {
          if (faceSeen[face * 3 + point])
            continue;

          faceSeen[face * 3 + point] = true;

          int i = indices[face * 3 + point];
          ovi.Initialize(face, i, WalkType.All);
          ovi.MoveToCounterClockwise();
          while (!ovi.Done)
          {
            int curFace = ovi.NextFace();
            if (curFace == -1)
              return false;

            int curPoint = ovi.Point;
            if (curPoint > 2)
              return false;

            faceSeen[curFace * 3 + curPoint] = true;

            int j = indices[curFace * 3 + curPoint];

            if (faceIds[j] == -1)
            {
              faceIds[j] = face;
              faceUsing[j] = curFace;
            }
            else if (faceIds[j] != face && !vertexBowtie[j])
            {
              // We found a (unique) bowtie!

              if (messages == null)
                return false;

              if (result)
              {
                // If this is the first bowtie found, add a quick explanation.
                messages.Add(
                  "A bowtie was found. Bowties can be fixed by calling Clean. " +
                  "A bowtie is the usage of a single vertex by two separate fans of triangles. " +
                  "The fix is to duplicate the vertex so that each fan has its own vertex.");
                result = false;
              }

              vertexBowtie[j] = true;

              string message = string.Format(
                CultureInfo.InvariantCulture,
                "Bowtie found around vertex {0} shared by faces {1} and {2}\n",
                j, curFace, faceUsing[j]);
              messages.Add(message);
            }
          }
        }
      }

      return result;
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshP
    //--------------------------------------------------------------

    private enum WalkType
    {
      All,
      Clockwise,
      CounterClockwise
    }


    /// <summary>
    /// Iterates over all triangles connected to a specific vertex.
    /// </summary>
    private class OrbitIterator
    {
      private int _face;
      private int _pointIndex;
      private int _currentFace;
      private int _currentEdge;
      private int _nextEdge;

      private readonly IList<int> _indices;
      private readonly IList<int> _adjacency;
      private readonly int _numberOfFaces;

      private bool _clockwise;
      private bool _stopOnBoundary;


      /// <summary>
      /// Gets a value indicating whether all triangles connected to the vertex have been visited.
      /// </summary>
      /// <value>
      /// <see langword="true"/> if all triangles have been visited; otherwise,
      /// <see langword="false"/>.
      /// </value>
      public bool Done
      {
        get { return _currentFace == -1; }
      }


      /// <summary>
      /// Gets the point of the current triangle.
      /// </summary>
      /// <value>The point (0, 1, or 2) of the current triangle.</value>
      public int Point
      {
        get { return _clockwise ? _currentEdge : (_currentEdge + 1) % 3; }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="OrbitIterator"/> class.
      /// </summary>
      /// <param name="indices">
      /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
      /// entries, and every group of 3 describes the vertices for a triangle face. This is the
      /// index buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of
      /// -1 is reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart).
      /// Any face containing one or more -1 entries is considered an 'unused' face.
      /// </param>
      /// <param name="adjacency">
      /// A 32-bit index array with numberOfFaces * 3 entries containing the edge adjacencies for
      /// each face in a mesh. Can be -1 to indicate an unused entry.
      /// </param>
      public OrbitIterator(IList<int> indices, IList<int> adjacency)
      {
        _face = -1;
        _pointIndex = -1;
        _currentFace = -1;
        _currentEdge = -1;
        _nextEdge = -1;
        _indices = indices;
        _adjacency = adjacency;
        _numberOfFaces = _indices.Count / 3;
        _clockwise = false;
        _stopOnBoundary = false;
      }


      /// <summary>
      /// Resets the iterator and positions it at the specified vertex.
      /// </summary>
      /// <param name="face">The index of the triangle.</param>
      /// <param name="vertexIndex">The index of the vertex.</param>
      /// <param name="walkType">Type of the walk.</param>
      public void Initialize(int face, int vertexIndex, WalkType walkType)
      {
        _face = _currentFace = face;
        _pointIndex = vertexIndex;
        _clockwise = (walkType != WalkType.CounterClockwise);
        _stopOnBoundary = (walkType != WalkType.All);

        _nextEdge = Find(face, vertexIndex);
        Debug.Assert(_nextEdge < 3);

        if (!_clockwise)
          _nextEdge = (_nextEdge + 2) % 3;

        _currentEdge = _nextEdge;
      }


      private int Find(int face, int point)
      {
        Debug.Assert(face < _numberOfFaces);

        if (_indices[face * 3 + 0] == point)
          return 0;
        if (_indices[face * 3 + 1] == point)
          return 1;

        Debug.Assert(_indices[face * 3 + 2] == point);
        return 2;
      }


      /// <summary>
      /// Moves to the next face.
      /// </summary>
      /// <returns>The current face.</returns>
      public int NextFace()
      {
        Debug.Assert(!Done);

        int ret = _currentFace;
        _currentEdge = _nextEdge;

        for (;;)
        {
          int prevFace = _currentFace;

          Debug.Assert(_currentFace * 3 + _nextEdge < _numberOfFaces * 3);

          _currentFace = _adjacency[_currentFace * 3 + _nextEdge];

          if (_currentFace == _face)
          {
            // Wrapped around after a full orbit, so finished.
            _currentFace = -1;
            break;
          }
          else if (_currentFace != -1)
          {
            Debug.Assert(_currentFace * 3 + 2 < _numberOfFaces * 3);

            if (_adjacency[_currentFace * 3 + 0] == prevFace)
              _nextEdge = 0;
            else if (_adjacency[_currentFace * 3 + 1] == prevFace)
              _nextEdge = 1;
            else
            {
              Debug.Assert(_adjacency[_currentFace * 3 + 2] == prevFace);
              _nextEdge = 2;
            }

            if (_clockwise)
              _nextEdge = (_nextEdge + 1) % 3;
            else
              _nextEdge = (_nextEdge + 2) % 3;

            break;
          }
          else if (_clockwise && !_stopOnBoundary)
          {
            // Hit boundary and need to restart to go counter-clockwise.
            _clockwise = false;
            _currentFace = _face;

            _nextEdge = Find(_face, _pointIndex);
            Debug.Assert(_nextEdge < 3);

            _nextEdge = (_nextEdge + 2) % 3;
            _currentEdge = (_currentEdge + 2) % 3;

            // Don't break out of loop so we can go the other way.
          }
          else
          {
            // Hit boundary and should stop.
            break;
          }
        }

        return ret;
      }


      /// <summary>
      /// Moves counter-clockwise around the vertex until a boundary is reached or the start face is
      /// reached again.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if a boundary was reached; otherwise; <see langword="false"/>.
      /// </returns>
      public bool MoveToCounterClockwise()
      {
        _currentFace = _face;

        _nextEdge = Find(_currentFace, _pointIndex);
        int initialNextEdge = _nextEdge;
        Debug.Assert(_nextEdge < 3);

        _nextEdge = (_nextEdge + 2) % 3;

        bool ret = false;

        int prevFace;
        do
        {
          prevFace = _currentFace;
          _currentFace = _adjacency[_currentFace * 3 + _nextEdge];

          if (_currentFace != -1)
          {
            if (_adjacency[_currentFace * 3] == prevFace)
              _nextEdge = 0;
            else if (_adjacency[_currentFace * 3 + 1] == prevFace)
              _nextEdge = 1;
            else
            {
              Debug.Assert(_adjacency[_currentFace * 3 + 2] == prevFace);
              _nextEdge = 2;
            }

            _nextEdge = (_nextEdge + 2) % 3;
          }
        }
        while ((_currentFace != _face) && (_currentFace != -1));

        if (_currentFace == -1)
        {
          _currentFace = prevFace;
          _nextEdge = (_nextEdge + 1) % 3;

          _pointIndex = _indices[_currentFace * 3 + _nextEdge];

          ret = true;
        }
        else
        {
          _nextEdge = initialNextEdge;
        }

        _clockwise = true;
        _currentEdge = _nextEdge;
        _face = _currentFace;
        return ret;
      }
    }


    /// <summary>
    /// Walks the edges of a triangles and searches for the specified value.
    /// </summary>
    /// <param name="indicesOrAdjacency">The indices list or adjacency information.</param>
    /// <param name="start">The start index in the list.</param>
    /// <param name="match">The index (indices) or triangle (adjacency) to find.</param>
    /// <returns>
    /// The edge at which the value was found. Returns 3 if the value was not found.
    /// </returns>
    private static int FindEdge(IList<int> indicesOrAdjacency, int start, int match)
    {
      Debug.Assert(indicesOrAdjacency != null);
      Debug.Assert(start + 3 <= indicesOrAdjacency.Count);

      int edge;
      for (edge = 0; edge < 3; edge++)
      {
        if (indicesOrAdjacency[start + edge] == match)
          break;
      }

      return edge;
    }
    #endregion


    //--------------------------------------------------------------
    #region DirectXMeshUtil
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a list of face offsets and counts based on the input attributes id array.
    /// </summary>
    /// <param name="attributes">
    /// A 32-bit index array with numberOfFaces entries which contains the attribute id for each
    /// face in the mesh.
    /// </param>
    /// <param name="numberOfFaces">The number of faces in the mesh.</param>
    /// <returns>
    /// A list of value pairs where each first value is the offset in the faces array for the start
    /// of the attribute subset, and second is the number of faces in that attribute subset.
    /// </returns>
    private static IList<Pair<int, int>> ComputeSubsets(IList<int> attributes, int numberOfFaces)
    {
      var subsets = new List<Pair<int, int>>();

      if (numberOfFaces == 0)
        return subsets;

      if (attributes == null)
      {
        subsets.Add(new Pair<int, int>(0, numberOfFaces));
        return subsets;
      }

      int lastAttr = attributes[0];
      int offset = 0;
      int count = 1;

      for (int j = 1; j < numberOfFaces; j++)
      {
        if (attributes[j] != lastAttr)
        {
          subsets.Add(new Pair<int, int>(offset, count));
          lastAttr = attributes[j];
          offset = j;
          count = 1;
        }
        else
        {
          count += 1;
        }
      }

      if (count > 0)
        subsets.Add(new Pair<int, int>(offset, count));

      return subsets;
    }


    /// <summary>
    /// Determines whether the specified format is valid for use in a vertex buffer input layout.
    /// </summary>
    /// <param name="format">The resource data format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is valid for use in a vertex buffer
    /// input layout; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidVB(DataFormat format)
    {
      return BytesPerElement(format) != 0;
    }


    /// <summary>
    /// Determines whether the specified format is valid for use as an index buffer format.
    /// </summary>
    /// <param name="format">The resource data format.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="format"/> is valid for use as an index buffer
    /// format; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValidIB(DataFormat format)
    {
      return format == DataFormat.R32_UINT || format == DataFormat.R16_UINT;
    }


    /// <summary>
    /// Gets the byte per element.
    /// </summary>
    /// <param name="format">The vertex element format.</param>
    /// <returns>The bytes per element. Returns 0 on failure.</returns>
    public static int BytesPerElement(DataFormat format)
    {
      // This list only includes those formats that are valid for use by IB or VB.

      switch (format)
      {
        case DataFormat.R32G32B32A32_FLOAT:
        case DataFormat.R32G32B32A32_UINT:
        case DataFormat.R32G32B32A32_SINT:
          return 16;

        case DataFormat.R32G32B32_FLOAT:
        case DataFormat.R32G32B32_UINT:
        case DataFormat.R32G32B32_SINT:
          return 12;

        case DataFormat.R16G16B16A16_FLOAT:
        case DataFormat.R16G16B16A16_UNORM:
        case DataFormat.R16G16B16A16_UINT:
        case DataFormat.R16G16B16A16_SNORM:
        case DataFormat.R16G16B16A16_SINT:
        case DataFormat.R32G32_FLOAT:
        case DataFormat.R32G32_UINT:
        case DataFormat.R32G32_SINT:
          return 8;

        case DataFormat.R10G10B10A2_UNORM:
        case DataFormat.R10G10B10A2_UINT:
        case DataFormat.R11G11B10_FLOAT:
        case DataFormat.R8G8B8A8_UNORM:
        case DataFormat.R8G8B8A8_UINT:
        case DataFormat.R8G8B8A8_SNORM:
        case DataFormat.R8G8B8A8_SINT:
        case DataFormat.R16G16_FLOAT:
        case DataFormat.R16G16_UNORM:
        case DataFormat.R16G16_UINT:
        case DataFormat.R16G16_SNORM:
        case DataFormat.R16G16_SINT:
        case DataFormat.R32_FLOAT:
        case DataFormat.R32_UINT:
        case DataFormat.R32_SINT:
        case DataFormat.B8G8R8A8_UNORM:
        case DataFormat.B8G8R8X8_UNORM:
          return 4;

        case DataFormat.R8G8_UNORM:
        case DataFormat.R8G8_UINT:
        case DataFormat.R8G8_SNORM:
        case DataFormat.R8G8_SINT:
        case DataFormat.R16_FLOAT:
        case DataFormat.R16_UNORM:
        case DataFormat.R16_UINT:
        case DataFormat.R16_SNORM:
        case DataFormat.R16_SINT:
        case DataFormat.B5G6R5_UNORM:
        case DataFormat.B5G5R5A1_UNORM:
          return 2;

        case DataFormat.R8_UNORM:
        case DataFormat.R8_UINT:
        case DataFormat.R8_SNORM:
        case DataFormat.R8_SINT:
          return 1;

        case DataFormat.B4G4R4A4_UNORM:
          return 2;

        default:
          // No BC, sRGB, X2Bias, SharedExp, Typeless, Depth, or Video formats
          return 0;
      }
    }


    /// <summary>
    /// Throws an exception if the specified input layout description is valid.
    /// </summary>
    /// <param name="vertexDeclaration">The input layout description.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="vertexDeclaration"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="vertexDeclaration"/> is invalid.
    /// </exception>
    internal static void Validate(IList<VertexElement> vertexDeclaration)
    {
      if (vertexDeclaration == null)
        throw new ArgumentNullException("vertexDeclaration");

      if (vertexDeclaration.Count == 0)
      {
        // Note that 0 is allowed by the runtime for degenerate cases, but is not defined for DirectXMesh.
        throw new ArgumentException("Vertex declaration is empty. (An empty vertex declaration is allowed by the runtime for degenerate cases, but it is not supported during content processing.", "vertexDeclaration");
      }

      if (vertexDeclaration.Count > D3D11_IA_VERTEX_INPUT_STRUCTURE_ELEMENT_COUNT)
      {
        // The upper-limit depends on feature level, so we assume highest value here.
        string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration (Count = {0}) exceeds the maximum allowed number of vertex elements (32).", vertexDeclaration.Count);
        throw new ArgumentException(message, "vertexDeclaration");
      }

      for (int j = 0; j < vertexDeclaration.Count; j++)
      {
        int bpe = BytesPerElement(vertexDeclaration[j].Format);
        if (bpe == 0)
        {
          // Not a valid DXGI format or it's not valid for VB usage.
          string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration (element {0}) contains an invalid format ({1}).", j, vertexDeclaration[j].Format);
          throw new ArgumentException(message, "vertexDeclaration");
        }

        int alignment;

        if (bpe == 1)
          alignment = 1;
        else if (bpe == 2)
          alignment = 2;
        else
          alignment = 4;

        if (vertexDeclaration[j].AlignedByteOffset != D3D11_APPEND_ALIGNED_ELEMENT
            && (vertexDeclaration[j].AlignedByteOffset % alignment) != 0)
        {
          // Invalid alignment for element.
          string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration (element {0}) contains an invalid offset ({1}).", j, vertexDeclaration[j].AlignedByteOffset);
          throw new ArgumentException(message, "vertexDeclaration");
        }

        /*
        if (vertexDeclaration[j].InputSlot >= D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT)
        {
          // The upper-limit depends on feature level, so we assume highest value here
          return false;
        }

        switch (vertexDeclaration[j].InputSlotClass)
        {
          case D3D11_INPUT_PER_VERTEX_DATA:
            if (vertexDeclaration[j].InstanceDataStepRate != 0)
            {
              return false;
            }
            break;

          case D3D11_INPUT_PER_INSTANCE_DATA:
            break;

          default:
            return false;
        }

        if (!vertexDeclaration[j].SemanticName)
        {
          return false;
        }
        */

        for (int i = 0; i < j; i++)
        {
          if (vertexDeclaration[i].Semantic == vertexDeclaration[j].Semantic
              && vertexDeclaration[i].SemanticIndex == vertexDeclaration[j].SemanticIndex)
          {
            // Duplicate semantic.
            string message = string.Format(CultureInfo.InvariantCulture, "Vertex declaration contains a duplicate semantic ({0}{1}).", vertexDeclaration[i].Semantic, vertexDeclaration[i].SemanticIndex);
            throw new ArgumentException(message, "vertexDeclaration");
          }
        }
      }
    }


    /// <summary>
    /// Calculates the byte offsets for each element of an input layout and the implied vertex
    /// stride from a given Direct3D 11 input layout description.
    /// </summary>
    /// <param name="vbDecl">The input layout description.</param>
    /// <param name="offsets">The offsets per element.</param>
    /// <param name="strides">The vertex strides per resource slot.</param>
    internal static void ComputeInputLayout(IList<VertexElement> vbDecl, out int[] offsets, out int[] strides)
    {
      Debug.Assert(vbDecl != null);
      Debug.Assert(vbDecl.Count > 0);

      offsets = new int[vbDecl.Count];
      strides = new int[D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT];

      // Store the current aligned byte offset for each vertex input resource slot.
      int[] prevABO = new int[D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT];

      for (int j = 0; j < vbDecl.Count; j++)
      {
        int slot = vbDecl[j].InputSlot;
        if (slot >= D3D11_IA_VERTEX_INPUT_RESOURCE_SLOT_COUNT)
        {
          // ignore bad input slots
          continue;
        }

        int bpe = BytesPerElement(vbDecl[j].Format);
        if (bpe == 0)
        {
          // ignore invalid format
          continue;
        }

        int alignment;

        if (bpe == 1)
          alignment = 1;
        else if (bpe == 2)
          alignment = 2;
        else
          alignment = 4;

        int alignedByteOffset = vbDecl[j].AlignedByteOffset;

        if (alignedByteOffset == D3D11_APPEND_ALIGNED_ELEMENT)
          alignedByteOffset = prevABO[slot];

        offsets[j] = alignedByteOffset;

        int istride = alignedByteOffset + bpe;
        strides[slot] = Math.Max(strides[slot], istride);

        prevABO[slot] = alignedByteOffset + bpe + (bpe % alignment);
      }
    }


    /// <summary>
    /// Calculates the average cache miss ratio (ACMR) and average triangle vertex re-use (ATVR) for
    /// the post-transform vertex cache.
    /// </summary>
    /// <param name="indices">
    /// A 32-bit indexed description of the triangles in a mesh. This must have 3 * numberOfFaces
    /// entries, and every group of 3 describes the vertices for a triangle face. This is the index
    /// buffer data suited for use with D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST. An entry of -1 is
    /// reserved as 'unused' (Direct3D 11 interprets this special value as a strip restart). Any
    /// face containing one or more -1 entries is considered an 'unused' face.
    /// </param>
    /// <param name="numberOfVertices">The number of vertices in the mesh.</param>
    /// <param name="cacheSize">
    /// The size of the cache. Pass OPTFACES_V_DEFAULT as the cache size to evaluate the 'device
    /// independent' optimization.
    /// </param>
    /// <param name="acmr">The average cache miss ratio (ACMR).</param>
    /// <param name="atvr">The average triangle vertex re-use (ATVR).</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="indices"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of indices is 0 or not a multiple of 3.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="numberOfVertices"/> or <paramref name="cacheSize"/> is 0 or negative.
    /// </exception>
    public static void ComputeVertexCacheMissRate(IList<int> indices, int numberOfVertices, int cacheSize, out float acmr, out float atvr)
    {
      // Reference: http://www.realtimerendering.com/blog/acmr-and-atvr/

      if (indices == null)
        throw new ArgumentNullException("indices");
      if (indices.Count == 0)
        throw new ArgumentException("Indices must not be empty.", "indices");
      if (indices.Count % 3 != 0)
        throw new ArgumentException("The number of indices must be a multiple of 3.", "indices");
      if (numberOfVertices <= 0)
        throw new ArgumentOutOfRangeException("numberOfVertices", "The number of vertices must be greater than 0.");
      if (cacheSize <= 0)
        throw new ArgumentOutOfRangeException("cacheSize", "The cache size must be greater than 0.");

      int numberOfFaces = indices.Count / 3;
      int misses = 0;

      int[] fifo = new int[cacheSize];
      for (int j = 0; j < fifo.Length; j++)
        fifo[j] = -1;

      int tail = 0;

      for (int j = 0; j < numberOfFaces * 3; j++)
      {
        if (indices[j] == -1)
          continue;

        bool found = false;

        for (int ptr = 0; ptr < cacheSize; ptr++)
        {
          if (fifo[ptr] == indices[j])
          {
            found = true;
            break;
          }
        }

        if (!found)
        {
          misses++;
          fifo[tail] = indices[j];
          tail++;
          if (tail == cacheSize)
            tail = 0;
        }
      }

      // Ideal is 0.5, individual tris have 3.0.
      acmr = (float)misses / numberOfFaces;

      // Ideal is 1.0, worst case is 6.0.
      atvr = (float)misses / numberOfVertices;
    }
    #endregion


    //--------------------------------------------------------------
    #region Misc
    //--------------------------------------------------------------

    private static Vector3F Normalize(Vector3F v)
    {
      float length = v.Length;
      if (length > 0)
        length = 1.0f / length;

      v.X *= length;
      v.Y *= length;
      v.Z *= length;

      return v;
    }
    #endregion
    
  }
}
