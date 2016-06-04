// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Stores the adjacency lists for the vertices of a mesh. (For internal use only.)
  /// </summary>
  /// <remarks>
  /// <para>
  /// An adjacency list is a list of indices. Each entry is the index of an adjacent vertex.
  /// </para>
  /// <para>
  /// The adjacency list of vertex i starts at <c>Lists[ListIndices[i]]</c> and ends at 
  /// <c>Lists[ListIndices[i + 1] - 1]</c>.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class VertexAdjacency
  {
    // Note: DirectionalLookupTableF<T> is only binary serializable, not xml-serializable.


    /// <summary>
    /// The start indices of the adjacency list of a given vertex.
    /// </summary>
    /// <remarks>
    /// This array contains n + 1 entries where n is the number of vertices. The last entry is a
    /// dummy entry that is only used to determine the end of the last adjacency list. This way, the
    /// start and end indices of a adjacency list can be determined using <c>ListIndices[i]</c> and 
    /// <c>ListIndices[i + 1] - 1</c>. No need to check the indices.
    /// </remarks>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Indices")]
    public ushort[] ListIndices;


    /// <summary>
    /// The adjacency lists of all vertices stored in a single array.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An adjacency list is a list of indices. Each entry is the index of an adjacent vertex.
    /// </para>
    /// <para>
    /// The adjacency list of vertex i starts at <c>Lists[ListIndices[i]]</c> and ends at 
    /// <c>Lists[ListIndices[i + 1] - 1]</c>.
    /// </para>
    /// </remarks>
    [CLSCompliant(false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
    public ushort[] Lists;


    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAdjacency"/> class.
    /// </summary>
    internal VertexAdjacency()
    {      
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VertexAdjacency"/> class.
    /// </summary>
    /// <param name="mesh">The mesh for which the adjacency information is built.</param>
    /// <exception cref="NotSupportedException">
    /// Too many vertices in convex hull. Max. 65534 vertices in convex hull are supported.
    /// </exception>
    public VertexAdjacency(DcelMesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      int numberOfVertices = mesh.Vertices.Count;
      if (numberOfVertices >= ushort.MaxValue)
        throw new NotSupportedException("Too many vertices in convex hull. Max. 65534 vertices in convex hull are supported.");

      Debug.Assert(mesh.Vertices.All(v => v.Tag == 0), "Tags of DcelMesh should be cleared.");

      // Store the index of each vertex in its tag for fast lookup.
      for (int i = 0; i < numberOfVertices; i++)
        mesh.Vertices[i].Tag = i;

      ListIndices = new ushort[numberOfVertices + 1];
      List<ushort> adjacencyLists = new List<ushort>(numberOfVertices * 4);
      for (int i = 0; i < numberOfVertices; i++)
      {
        DcelVertex vertex = mesh.Vertices[i];
        ListIndices[i] = (ushort)adjacencyLists.Count;

        // Gather all adjacent vertices.
        DcelEdge startEdge = vertex.Edge;
        DcelEdge edge = startEdge;
        do
        {
          DcelVertex adjacentVertex = edge.Next.Origin;
          int adjacentIndex = adjacentVertex.Tag;
          Debug.Assert(mesh.Vertices[adjacentIndex] == adjacentVertex, "Indices stored in DcelVertex.Tag are invalid.");
          adjacencyLists.Add((ushort)adjacentIndex);
          edge = edge.Twin.Next;
        } while (edge != startEdge);
      }

      // Add one additional entry which determines the end of the last adjacency list.
      ListIndices[numberOfVertices] = (ushort)adjacencyLists.Count;

      // Copy adjacency lists into static array.
      Lists = adjacencyLists.ToArray();
    }
  }
}
