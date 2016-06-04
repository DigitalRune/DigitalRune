// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !NETFX_CORE && !WP7 && !XBOX
using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// Describes a link between two <see cref="CDIsland"/>s.
  /// </summary>
  /// <remarks>
  /// Linked islands are candidates for merging. 
  /// </remarks>
  [DebuggerDisplay("Link {IslandA.Id}-{IslandB.Id}: Concavity = {Concavity}, DecimationCost = {DecimationCost}")]
  internal sealed class CDIslandLink
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    public readonly CDIsland IslandA;
    public readonly CDIsland IslandB;

    // The concavity of the merged island of IslandA + IslandB.
    public readonly float Concavity;

    // The cost of merging IslandA and IslandB. Lower costs are better for merging.
    public readonly float DecimationCost;

    // The AABB enclosing both islands.
    public readonly Aabb Aabb;

    // The hull vertices of the hull over IslandA + IslandB.
    public Vector3F[] Vertices;

    // Either null or an the convex hull of the island.
    public ConvexHullBuilder ConvexHullBuilder;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    public CDIslandLink(CDIsland islandA, CDIsland islandB, float allowedConcavity, float smallIslandBoost, int vertexLimit, bool sampleVertices, bool sampleCenters)
    {
      IslandA = islandA;
      IslandB = islandB;

      Aabb aabb = IslandA.Aabb;
      aabb.Grow(IslandB.Aabb);
      Aabb = aabb;

      float aabbExtentLength = aabb.Extent.Length;

      Concavity = GetConcavity(vertexLimit, sampleVertices, sampleCenters);

      float alpha = allowedConcavity / (10 * aabbExtentLength);

      // Other options for alpha that we could evaluate: 
      //float alpha = 0.03f / aabbExtentLength; // Independent from concavity.
      //alpha = 0.001f;

      float aspectRatio = GetAspectRatio();

      float term1 = Concavity / aabbExtentLength;
      float term2 = alpha * aspectRatio;

      // This third term is not in the original paper. 
      // The goal is to encourage the merging of two small islands. Without this factor
      // it can happen that there are few large islands and in each step a single triangle
      // is merged into a large island. Resulting in approx. O(n) speed.
      // It is much faster if small islands merge first to target an O(log n) speed.
      float term3 = smallIslandBoost * Math.Max(IslandA.Triangles.Length, IslandB.Triangles.Length);

      DecimationCost = term1 + term2 + term3;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Computes the aspect ratio: 
    // AspectRatio = (Perimeter of merged Islands)² / (4 * π * Surface Area of merged Islands)
    // The AspectRatio is 1 for discs.
    private float GetAspectRatio()
    {
      float perimeter = 0;
      float area = 0;

      // Visit all triangles of A and B.
      foreach (var triangle in IslandA.Triangles.Union(IslandB.Triangles))
      {
        // Check all 3 edges. If a neighbor on this edge belongs to a different island,
        // this is a perimeter edge and we add it to the perimeter length.
        for (int i = 0; i < 3; i++)
        {
          if (triangle.Neighbors[i] != null
              && (triangle.Neighbors[i].Island == IslandA || triangle.Neighbors[i].Island == IslandB))
          {
            continue;
          }

          var edge = triangle.Vertices[(i + 2) % 3] - triangle.Vertices[(i + 1) % 3];
          perimeter += edge.Length;
        }

        // Add area of triangle.
        var edge0 = triangle.Vertices[1] - triangle.Vertices[0];
        var edge1 = triangle.Vertices[2] - triangle.Vertices[0];
        area += Vector3F.Cross(edge0, edge1).Length / 2;
      }

      float aspectRatio = perimeter * perimeter / (4 * ConstantsF.Pi * area);
      return aspectRatio;
    }


    // Computes the Concavity and sets the Vertices.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private float GetConcavity(int vertexLimit, bool sampleVertices, bool sampleCenters)
    {
      // Initially we assume that the new vertices are simply the union of the islands' vertices.
      Vertices = IslandA.Vertices.Union(IslandB.Vertices).ToArray();

      try
      {
        // Create hull mesh.

        // Incremental hull building.
        // Note: Commented out because this building the hull this way is less stable.
        //bool rebuild = true;
        //try
        //{
        //  if (IslandA.ConvexHullBuilder != null)
        //  {
        //    if (IslandB.ConvexHullBuilder == null || IslandA.Vertices.Length > IslandB.Vertices.Length)
        //    {
        //      ConvexHullBuilder = IslandA.ConvexHullBuilder.Clone();
        //      ConvexHullBuilder.Grow(IslandB.Vertices, vertexLimit, 0);
        //    }
        //    else
        //    {
        //      ConvexHullBuilder = IslandB.ConvexHullBuilder.Clone();
        //      ConvexHullBuilder.Grow(IslandA.Vertices, vertexLimit, 0);
        //    }
        //    rebuild = false;
        //  }
        //  else if (IslandB.ConvexHullBuilder != null)
        //  {
        //    ConvexHullBuilder = IslandB.ConvexHullBuilder.Clone();
        //    ConvexHullBuilder.Grow(IslandA.Vertices, vertexLimit, 0);
        //    rebuild = false;
        //  }
        //}
        //catch (GeometryException)
        //{
        //  rebuild = true;
        //}

        //if (rebuild)
        {
          try
          {
            ConvexHullBuilder = new ConvexHullBuilder();
            ConvexHullBuilder.Grow(Vertices, vertexLimit, 0);
          }
          catch (GeometryException)
          {
            // Hull building failed. Try again with a randomized order.
            var random = new Random(1234567);
            // Fisher-Yates shuffle:
            for (int i = Vertices.Length - 1; i >= 1; i--)
            {
              var v = Vertices[i];
              var j = random.NextInteger(0, i);
              Vertices[i] = Vertices[j];
              Vertices[j] = v;
            }
          }
        }

        var hullMesh = ConvexHullBuilder.Mesh.ToTriangleMesh();

        // Now, we have a reduced set of vertices.
        Vertices = hullMesh.Vertices.ToArray();

        // For larger meshes we create an AabbTree as acceleration structure.
        AabbTree<int> partition = null;
        if (hullMesh.NumberOfTriangles > 12)
        {
          partition = new AabbTree<int>
          {
            GetAabbForItem = i => hullMesh.GetTriangle(i).Aabb,
            BottomUpBuildThreshold = 0,
          };
          for (int i = 0; i < hullMesh.NumberOfTriangles; i++)
            partition.Add(i);

          partition.Update(true);
        }

        Aabb aabb = Aabb;
        float aabbExtent = aabb.Extent.Length;

        // Note: For a speed-up we could skip some ray tests in the next loop and only sample
        // a few vertices if there would be a lot of tests.

        // The next loop performs ray casts against the hull mesh to determine the maximum
        // concavity. We ensure that we make only one ray cast per vertex even if a vertex
        // is shared by many triangles.
        float maxConcavity = 0;
        foreach (var triangle in IslandA.Triangles.Union(IslandB.Triangles))
        {
          if (sampleVertices)
          {
            for (int i = 0; i < triangle.Vertices.Length; i++)
            {
              // Each vertex can be shared by several triangles of the current islands.
              // Therefore, we check the edges that contain this vertex. If an edge neighbor
              // is in the same island, we make sure that the vertex concavity is computed only once
              // in the triangle with the smallest Id.
              var neighbor0 = triangle.Neighbors[(i + 1) % 3];
              var neighbor1 = triangle.Neighbors[(i + 2) % 3];

              if (neighbor0 != null
                  && (neighbor0.Island == IslandA || neighbor0.Island == IslandB)
                  && triangle.Id > neighbor0.Id)
              {
                // No need to test: The neighbor is in the same islands and this triangle Id is larger.
                continue;
              }

              if (neighbor1 != null
                  && (neighbor1.Island == IslandA || neighbor1.Island == IslandB)
                  && triangle.Id > neighbor1.Id)
              {
                // No need to test: The neighbor is in the same islands and this triangle Id is larger.
                continue;
              }

              var position = triangle.Vertices[i];
              var normal = triangle.VertexNormals[i];

              // Degenerate triangles are ignored.
              if (normal.IsNumericallyZero)
                continue;

              // Shoot a ray from outside the hull mesh to the vertex. 
              float hitDistance;
              Vector3F rayOrigin = position + normal * aabbExtent;
              float rayLength = (position - rayOrigin).Length;
              var ray = new Ray(rayOrigin, -normal, rayLength);
              if (partition != null)
              {
                // Use AABB tree for better performance.
                foreach (var triangleIndex in partition.GetOverlaps(ray))
                {
                  var candidateTriangle = hullMesh.GetTriangle(triangleIndex);
                  var hit = GeometryHelper.GetContact(ray, candidateTriangle, false, out hitDistance);
                  if (hit)
                  {
                    // The concavity is the distance from the hull to the vertex.
                    float concavity = rayLength - hitDistance;
                    maxConcavity = Math.Max(maxConcavity, concavity);
                    break;
                  }
                }
              }
              else
              {
                // No AABB tree. 
                var hit = GeometryHelper.GetContact(hullMesh, ray, out hitDistance);
                if (hit)
                {
                  float concavity = rayLength - hitDistance;
                  maxConcavity = Math.Max(maxConcavity, concavity);
                }
              }
            }
          }

          if (sampleCenters)
          {
            // Test: Also shoot from the triangle centers.
            var center = (triangle.Vertices[0] + triangle.Vertices[1] + triangle.Vertices[2]) / 3;
            var normal = triangle.Normal;

            // Degenerate triangles are ignored.
            if (normal.IsNumericallyZero)
              continue;

            // Shoot a ray from outside the hull mesh to the vertex. 
            float hitDistance;
            Vector3F rayOrigin = center + normal * aabbExtent;
            float rayLength = (center - rayOrigin).Length;
            var ray = new Ray(rayOrigin, -normal, rayLength);
            if (partition != null)
            {
              // Use AABBTree for better performance.
              foreach (var triangleIndex in partition.GetOverlaps(ray))
              {
                var candidateTriangle = hullMesh.GetTriangle(triangleIndex);
                var hit = GeometryHelper.GetContact(ray, candidateTriangle, false, out hitDistance);
                if (hit)
                {
                  // The concavity is the distance from the hull to the vertex.
                  float concavity = rayLength - hitDistance;
                  maxConcavity = Math.Max(maxConcavity, concavity);
                  break;
                }
              }
            }
            else
            {
              // No AABBTree. 
              var hit = GeometryHelper.GetContact(hullMesh, ray, out hitDistance);
              if (hit)
              {
                float concavity = rayLength - hitDistance;
                maxConcavity = Math.Max(maxConcavity, concavity);
              }
            }
          }
        }

        return maxConcavity;
      }
      catch (GeometryException)
      {
        // Ouch, the convex hull generation failed. This can happen for degenerate inputs
        // and numerical problems in the convex hull builder. 
        ConvexHullBuilder = null;
        return 0;
      }
    }
    #endregion
  }
}
#endif
