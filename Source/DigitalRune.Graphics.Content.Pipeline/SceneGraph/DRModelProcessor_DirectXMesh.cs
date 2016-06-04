// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DigitalRune.Geometry;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace DigitalRune.Graphics.Content.Pipeline
{
  partial class DRModelProcessor
  {
    private static void CalculateNormals(MeshContent mesh, bool overwriteExistingNormals)
    {
      Debug.Assert(mesh != null);

      string name = VertexChannelNames.Normal();
      if (!overwriteExistingNormals && mesh.Geometry.All(geometry => geometry.Vertices.Channels.Contains(name)))
        return; // Nothing to do.

      // Accumulate triangle normals at vertices.
      // IMPORTANT: Calculating normals per submesh does not work!
      // - Normals at submesh borders are only correct if we consider adjacent submeshes.
      // - GeometryContent.Positions contains duplicated entries if neighbor triangles do
      //   have the same texture coordinates. MeshContent.Positions are unique (no duplicates).
      var positions = mesh.Positions.Select(p => (Vector3F)p).ToArray();
      var indices = mesh.Geometry
                        .SelectMany(geometry => geometry.Indices.Select(i => geometry.Vertices.PositionIndices[i]))
                        .ToArray();

      // Calculate vertex normals.
      var normals = DirectXMesh.ComputeNormals(indices, positions, true, VertexNormalAlgorithm.WeightedByAngle);

      // Copy normals to vertex channels.
      foreach (var geometry in mesh.Geometry)
      {
        if (!geometry.Vertices.Channels.Contains(name))
          geometry.Vertices.Channels.Add<Vector3>(name, null);
        else if (!overwriteExistingNormals)
          continue;

        var normalChannel = geometry.Vertices.Channels.Get<Vector3>(name);
        var positionIndices = geometry.Vertices.PositionIndices;
        for (int i = 0; i < normalChannel.Count; i++)
          normalChannel[i] = (Vector3)normals[positionIndices[i]];
      }
    }


    private static void CalculateTangentFrames(GeometryContent geometry, string textureCoordinateChannelName, string tangentChannelName, string binormalChannelName)
    {
      Debug.Assert(!string.IsNullOrWhiteSpace(textureCoordinateChannelName));

      var indices = geometry.Indices;
      var positions = geometry.Vertices.Positions.Select(p => (Vector3F)p).ToArray();
      var normals = geometry.Vertices.Channels.Get<Vector3>(VertexChannelNames.Normal()).Select(n => (Vector3F)n).ToArray();
      var textureCoordinates = geometry.Vertices.Channels.Get<Vector2>(textureCoordinateChannelName).Select(n => (Vector2F)n).ToArray();

      Vector3F[] tangents;
      Vector3F[] bitangents;
      DirectXMesh.ComputeTangentFrame(indices, positions, normals, textureCoordinates, out tangents, out bitangents);

      if (!string.IsNullOrEmpty(tangentChannelName))
        geometry.Vertices.Channels.Add(tangentChannelName, tangents.Select(t => (Vector3)t));

      if (!string.IsNullOrEmpty(binormalChannelName))
        geometry.Vertices.Channels.Add(binormalChannelName, bitangents.Select(b => (Vector3)b));
    }


    private static void MergeDuplicatePositions(MeshContent mesh, float tolerance)
    {
      Debug.Assert(mesh != null);
      Debug.Assert(tolerance > 0);

      var positions = mesh.Positions.Select(p => (Vector3F)p).ToList();
      int[] positionRemap;
      int numberOfDuplicates = GeometryHelper.MergeDuplicatePositions(positions, tolerance, out positionRemap);

      if (numberOfDuplicates > 0)
      {
        mesh.Positions.Clear();
        for (int i = 0; i < positions.Count; i++)
          mesh.Positions[i] = (Vector3)positions[i];

        foreach (var geometry in mesh.Geometry)
        {
          var positionIndices = geometry.Vertices.PositionIndices;
          int numberOfVertices = geometry.Vertices.VertexCount;
          for (int i = 0; i < numberOfVertices; i++)
            positionIndices[i] = positionRemap[positionIndices[i]];
        }
      }
    }


    private /*static*/ void OptimizeForCache(MeshContent mesh)
    {
      Debug.Assert(mesh != null);

      foreach (var geometry in mesh.Geometry)
      {
        var vertices = geometry.Vertices;
        int numberOfVertices = vertices.VertexCount;
        var positions = vertices.Positions.Select(p => (Vector3F)p).ToArray();
        var indices = geometry.Indices.ToList();

        int[] vertexRemap;
        int[] duplicateVertices;
        OptimizeForCache(positions, indices, out vertexRemap, out duplicateVertices, mesh.Identity);

        // ----- Recreate vertices and indices.
        // Pseuso-code (see DirectXMesh.FinalizeVB):
        //
        // for each j in nVerts
        //    newIndex = vertexRemap[j]
        //    if (newIndex != -1)
        //      memcpy(newVB + newIndex * stride,
        //             oldVB + j * stride,
        //             stride)
        // 
        // for each j in nDupVerts
        //   newIndex = vertexRemap[j + nVerts]
        //   if (newIndex != -1)
        //     memcpy(newVB + newIndex * stride,
        //            oldVB + dup[j] * stride,
        //            stride)

        // Copy original vertices.
        int[] positionIndices = vertices.PositionIndices.ToArray();
        var channels = vertices.Channels.ToArray();

        vertices.Channels.Clear();
        vertices.RemoveRange(0, vertices.VertexCount);

        // Reserve vertex entries.
        for (int i = 0; i < vertexRemap.Length; i++)
        {
          if (vertexRemap[i] != -1)
            vertices.Add(0);
        }

        // Add reordered vertices.
        for (int oldIndex = 0; oldIndex < numberOfVertices; oldIndex++)
        {
          int newIndex = vertexRemap[oldIndex];
          if (newIndex != -1)
            vertices.PositionIndices[newIndex] = positionIndices[oldIndex];
        }

        Debug.Assert(vertexRemap.Length == numberOfVertices + duplicateVertices.Length);

        // Add duplicate vertices.
        for (int i = 0; i < duplicateVertices.Length; i++)
        {
          int newIndex = vertexRemap[numberOfVertices + i];
          if (newIndex != -1)
            vertices.PositionIndices[newIndex] = positionIndices[duplicateVertices[i]];
        }

        // Add vertex channels.
        foreach (var oldChannel in channels)
        {
          var newChannel = vertices.Channels.Add(oldChannel.Name, oldChannel.ElementType, null);

          // Add reordered vertices.
          for (int oldIndex = 0; oldIndex < numberOfVertices; oldIndex++)
          {
            int newIndex = vertexRemap[oldIndex];
            if (newIndex != -1)
              newChannel[newIndex] = oldChannel[oldIndex];
          }

          // Add duplicate vertices.
          for (int i = 0; i < duplicateVertices.Length; i++)
          {
            int newIndex = vertexRemap[numberOfVertices + i];
            if (newIndex != -1)
              newChannel[newIndex] = oldChannel[duplicateVertices[i]];
          }
        }

        // Add new indices.
        geometry.Indices.Clear();
        geometry.Indices.AddRange(indices);
      }
    }


    private /*static*/ void OptimizeForCache(IList<Vector3F> positions, IList<int> indices, ContentIdentity identity)
    {
      Debug.Assert(positions != null);
      Debug.Assert(indices != null);

      var originalPositions = positions.ToArray();
      int numberOfVertices = originalPositions.Length;

      int[] vertexRemap;
      int[] duplicateVertices;
      OptimizeForCache(positions, indices, out vertexRemap, out duplicateVertices, identity);

      positions.Clear();
      for (int i = 0; i < vertexRemap.Length; i++)
      {
        if (vertexRemap[i] != -1)
          positions.Add(new Vector3F());
      }

      // Add reordered vertices.
      for (int oldIndex = 0; oldIndex < numberOfVertices; oldIndex++)
      {
        int newIndex = vertexRemap[oldIndex];
        if (newIndex != -1)
          positions[newIndex] = originalPositions[oldIndex];
      }

      Debug.Assert(vertexRemap.Length == numberOfVertices + duplicateVertices.Length);

      // Add duplicate vertices.
      for (int i = 0; i < duplicateVertices.Length; i++)
      {
        int newIndex = vertexRemap[numberOfVertices + i];
        if (newIndex != -1)
          positions[newIndex] = originalPositions[duplicateVertices[i]];
      }
    }


    private /*static*/ void OptimizeForCache(IList<Vector3F> positions,   // In: Original positions.
                                         IList<int> indices,          // In: Original indices. Out: Optimized indices.
                                         out int[] vertexRemap,       // Maps original vertex location to optimized vertex location.
                                         out int[] duplicateVertices, // Original locations of duplicate vertices.
                                         ContentIdentity identity)
    {
      Debug.Assert(positions != null);
      Debug.Assert(indices != null);

#if COMPUTE_VERTEX_CACHE_MISS_RATE
      float acmrOld;
      float atvrOld;
      DirectXMesh.ComputeVertexCacheMissRate(indices, positions.Count, DirectXMesh.OPTFACES_V_DEFAULT, out acmrOld, out atvrOld);
#endif

      int numberOfVertices = positions.Count;

      int[] pointRep;
      int[] adjacency;
      DirectXMesh.GenerateAdjacencyAndPointReps(indices, positions, Numeric.EpsilonF, out pointRep, out adjacency);

      if (!DirectXMesh.Clean(indices, numberOfVertices, adjacency, null, false, out duplicateVertices))
      {
        List<string> validationMessages = new List<string>();
        DirectXMesh.Validate(indices, numberOfVertices, adjacency, MeshValidationOptions.Default, validationMessages);

        string message;
        if (validationMessages.Count == 0)
        {
          message = "Mesh cleaning failed.";
        }
        else
        {
          var messageBuilder = new StringBuilder();
          messageBuilder.AppendLine("Mesh cleaning failed:");
          foreach (var validationMessage in validationMessages)
            messageBuilder.AppendLine(validationMessage);

          message = messageBuilder.ToString();
        }

        throw new InvalidContentException(message, identity);
      }

      // Skip DirectXMesh.AttributeSort and DirectXMesh.ReorderIBAndAdjacency.
      // (GeometryContent already sorted.)

      int[] faceRemap;
      DirectXMesh.OptimizeFaces(indices, adjacency, null, out faceRemap);

      DirectXMesh.ReorderIB(indices, faceRemap);

      DirectXMesh.OptimizeVertices(indices, numberOfVertices, out vertexRemap);

      DirectXMesh.FinalizeIB(indices, vertexRemap);

      // Skip DirectXMesh.FinalizeVB.
      // (Needs to be handled by caller.)

      Debug.Assert(vertexRemap.Length == numberOfVertices + duplicateVertices.Length);

#if COMPUTE_VERTEX_CACHE_MISS_RATE
      int newNumberOfVertices = vertexRemap.Count(i => i != -1);
      float acmrNew;
      float atvrNew;
      DirectXMesh.ComputeVertexCacheMissRate(indices, newNumberOfVertices, DirectXMesh.OPTFACES_V_DEFAULT, out acmrNew, out atvrNew);

      _context.Logger.LogMessage(
        "Mesh optimization: Vertices before {0}, after {1}; ACMR before {2}, after {3}; ATVR before {4}, after {5}", 
        numberOfVertices, newNumberOfVertices, acmrOld, acmrNew, atvrOld, atvrNew);
#endif
    }
  }
}
