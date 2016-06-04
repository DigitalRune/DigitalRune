// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;
using DigitalRune.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace DigitalRune.Graphics
{
  partial class TerrainRoadLayer
  {
    // Simple static dummy mesh for debugging.
    //public static void CreateMesh(GraphicsDevice graphicsDevice, Path3F path, out Submesh submesh, out Aabb aabb)
    //{
    //  if (path == null)
    //    throw new ArgumentNullException("path");

    //  //var flattenedPoints = new List<Vector3F>();
    //  //path.Flatten(flattenedPoints, MaxNumberOfIterations, Tolerance);

    //  // Compute vertices and indices.
    //  var vertices = new TerrainLayerVertex[4];
    //  vertices[0] = new TerrainLayerVertex(new Vector2(0, 0), new Vector2(0, 0));
    //  vertices[1] = new TerrainLayerVertex(new Vector2(2, 0), new Vector2(1, 0));
    //  vertices[2] = new TerrainLayerVertex(new Vector2(0.5f, 2), new Vector2(0, 1));
    //  vertices[3] = new TerrainLayerVertex(new Vector2(1.5f, 1.5f), new Vector2(1, 1));

    //  var indices = new int[6];
    //  indices[0] = 0;
    //  indices[1] = 1;
    //  indices[2] = 3;
    //  indices[3] = 0;
    //  indices[4] = 2;
    //  indices[5] = 3;

    //  // Convert to submesh.
    //  submesh = new Submesh();
    //  submesh.PrimitiveCount = 2;
    //  submesh.PrimitiveType = PrimitiveType.TriangleList;
    //  submesh.VertexCount = vertices.Length;
    //  submesh.VertexBuffer = new VertexBuffer(graphicsDevice, TerrainLayerVertex.VertexDeclaration, 4, BufferUsage.WriteOnly);
    //  submesh.VertexBuffer.SetData(vertices);
    //  submesh.IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
    //  submesh.IndexBuffer.SetData(indices);

    //  // Update AABB.
    //  if (vertices != null && vertices.Length != 0)
    //  {
    //    float yLimit = 1e20f;
    //    var v0 = new Vector3F(vertices[0].Position.X, 0, vertices[0].Position.Y);
    //    aabb = new Aabb(v0, v0);
    //    for (int i = 1; i < vertices.Length; i++)
    //    {
    //      aabb.Minimum.X = Math.Min(aabb.Minimum.X, vertices[i].Position.X);
    //      aabb.Maximum.X = Math.Max(aabb.Maximum.X, vertices[i].Position.X);
    //      aabb.Minimum.Z = Math.Min(aabb.Minimum.Z, vertices[i].Position.Y);
    //      aabb.Maximum.Z = Math.Max(aabb.Maximum.Z, vertices[i].Position.Y);
    //    }
    //    aabb.Minimum.Y = -yLimit;
    //    aabb.Maximum.Y = yLimit;
    //  }
    //  else
    //  {
    //    aabb = new Aabb(new Vector3F(float.NegativeInfinity), new Vector3F(float.NegativeInfinity));
    //  }
    //}



    /// <summary>
    /// Creates a road mesh for use with a <see cref="TerrainRoadLayer"/>.
    /// </summary>
    /// <param name="graphicsDevice">The graphics device.</param>
    /// <param name="path">The path that represents the road.</param>
    /// <param name="defaultWidth">The default road width.</param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations (used when tessellating the path).
    /// </param>
    /// <param name="tolerance">
    /// The tolerance in world space units (used when tessellating the path).
    /// </param>
    /// <param name="submesh">
    /// The resulting road mesh - or <see langword="null"/> if <paramref name="path"/> is empty.
    /// </param>
    /// <param name="aabb">The axis-aligned bounding box of the road mesh.</param>
    /// <param name="roadLength">The length of the road.</param>
    /// <remarks>
    /// <para>
    /// A road is defined by the specified 3D path. The path keys can be of type
    /// <see cref="TerrainRoadPathKey"/>. This allows to add additional information to the path,
    /// like varying road <see cref="TerrainRoadPathKey.Width"/>. If the path keys are of any
    /// other <see cref="PathKey3F"/> type, the <paramref name="defaultWidth"/> is used.
    /// </para>
    /// <para>
    /// To create the road mesh, the path is tessellated. <paramref name="maxNumberOfIterations"/>
    /// and <paramref name="tolerance"/> define how detailed the tessellation will be.
    /// </para>
    /// <para>
    /// <strong>Limitations:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// The path should not have any gaps.
    /// </item>
    /// <items>
    /// The path should be sufficiently smooth. Tight curves can lead to tessellation problems.
    /// </items>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="graphicsDevice"/> or <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "6")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void CreateMesh(GraphicsDevice graphicsDevice, Path3F path, float defaultWidth,
                                  int maxNumberOfIterations, float tolerance,
                                  out Submesh submesh, out Aabb aabb, out float roadLength)
    {
      if (graphicsDevice == null)
        throw new ArgumentNullException("graphicsDevice");
      if (path == null)
        throw new ArgumentNullException("path");

      // Compute list of line segments. (2 points per line segment!)
      var flattenedPoints = new List<Vector3F>();
      path.Flatten(flattenedPoints, maxNumberOfIterations, tolerance);

      // Abort if path is empty.
      int numberOfLineSegments = flattenedPoints.Count / 2;
      if (numberOfLineSegments <= 0)
      {
        submesh = null;
        aabb = new Aabb();
        roadLength = 0;
        return;
      }

      // Compute accumulated lengths. (One entry for each entry in flattenedPoints.)
      float[] accumulatedLengths = new float[flattenedPoints.Count];
      accumulatedLengths[0] = 0;
      for (int i = 1; i < flattenedPoints.Count; i += 2)
      {
        Vector3F previous = flattenedPoints[i - 1];
        Vector3F current = flattenedPoints[i];
        float length = (current - previous).Length;

        accumulatedLengths[i] = accumulatedLengths[i - 1] + length;
        if (i + 1 < flattenedPoints.Count)
          accumulatedLengths[i + 1] = accumulatedLengths[i];
      }

      // Total road length.
      roadLength = accumulatedLengths[accumulatedLengths.Length - 1];

      // Create a mapping between accumulatedLength and the path key widths.
      // (accumulatedLength --> TerrainRoadPathKey.Width)
      var widthKeys = new List<Pair<float, float>>();
      {
        int index = 0;
        foreach (var key in path)
        {
          Vector3F position = key.Point;
          var roadKey = key as TerrainRoadPathKey;
          float width = (roadKey != null) ? roadKey.Width : defaultWidth;

          for (; index < flattenedPoints.Count; index++)
          {
            if (Vector3F.AreNumericallyEqual(position, flattenedPoints[index]))
            {
              widthKeys.Add(new Pair<float, float>(accumulatedLengths[index], width));
              break;
            }

            bool isLastLineSegment = (index + 2 == flattenedPoints.Count);
            if (!isLastLineSegment)
              index++;
          }

          index++;
        }
      }

      // Create a list of interpolated road widths. (One entry for each entry in flattenedPoints.)
      var widths = new float[flattenedPoints.Count];
      int previousKeyIndex = 0;
      var previousKey = widthKeys[0];
      var nextKey = widthKeys[1];
      widths[0] = widthKeys[0].Second;
      for (int i = 1; i < flattenedPoints.Count; i += 2)
      {
        if (accumulatedLengths[i] > nextKey.First)
        {
          previousKey = nextKey;
          previousKeyIndex++;
          nextKey = widthKeys[previousKeyIndex + 1];
        }

        float p = (accumulatedLengths[i] - previousKey.First) / (nextKey.First - previousKey.First);
        widths[i] = InterpolationHelper.Lerp(previousKey.Second, nextKey.Second, p);

        if (i + 1 < flattenedPoints.Count)
          widths[i + 1] = widths[i];
      }

      // Compute vertices and indices.
      var vertices = new List<TerrainLayerVertex>(numberOfLineSegments * 2 + 2);
      var indices = new List<int>(numberOfLineSegments * 6);  // Two triangles per line segment.
      Vector3F lastOrthonormal = Vector3F.UnitX;
      aabb = new Aabb(flattenedPoints[0], flattenedPoints[0]);
      bool isClosed = Vector3F.AreNumericallyEqual(flattenedPoints[0], flattenedPoints[flattenedPoints.Count - 1]);
      for (int i = 0; i < flattenedPoints.Count; i++)
      {
        Vector3F start = flattenedPoints[i];

        Vector3F previous;
        bool isFirstPoint = (i == 0);
        if (!isFirstPoint)
          previous = flattenedPoints[i - 1];
        else if (isClosed && path.SmoothEnds)
          previous = flattenedPoints[flattenedPoints.Count - 2];
        else
          previous = start;

        Vector3F next;
        bool isLastPoint = (i + 1 == flattenedPoints.Count);
        if (!isLastPoint)
          next = flattenedPoints[i + 1];
        else if (isClosed && path.SmoothEnds)
          next = flattenedPoints[1];
        else
          next = start;

        Vector3F tangent = next - previous;

        Vector3F orthonormal = new Vector3F(tangent.Z, 0, -tangent.X);
        if (!orthonormal.TryNormalize())
          orthonormal = lastOrthonormal;

        // Add indices to add two triangles between the current and the next vertices.
        if (!isLastPoint)
        {
          int baseIndex = vertices.Count;

          //  2      3 
          //   x----x  
          //   |\   |   ^
          //   | \  |   | road
          //   |  \ |   | direction
          //   |   \|   | 
          //   x----x   
          //  0      1 

          indices.Add(baseIndex);
          indices.Add(baseIndex + 1);
          indices.Add(baseIndex + 2);

          indices.Add(baseIndex + 1);
          indices.Add(baseIndex + 3);
          indices.Add(baseIndex + 2);
        }

        // Add two vertices.
        Vector3F leftVertex = start - orthonormal * (widths[i] / 2);
        Vector3F rightVertex = start + orthonormal * (widths[i] / 2);
        vertices.Add(new TerrainLayerVertex(new Vector2(leftVertex.X, leftVertex.Z), new Vector2(0, accumulatedLengths[i])));
        vertices.Add(new TerrainLayerVertex(new Vector2(rightVertex.X, rightVertex.Z), new Vector2(1, accumulatedLengths[i])));

        // Grow AABB
        aabb.Grow(leftVertex);
        aabb.Grow(rightVertex);

        lastOrthonormal = orthonormal;

        // The flattened points list contains 2 points per line segment, which means that there
        // are duplicate intermediate points, which we skip.
        bool isLastLineSegment = (i + 2 == flattenedPoints.Count);
        if (!isLastLineSegment)
          i++;
      }

      Debug.Assert(vertices.Count == (numberOfLineSegments * 2 + 2));
      Debug.Assert(indices.Count == (numberOfLineSegments * 6));

      // The road is projected onto the terrain, therefore the computed y limits are not correct.
      // (unless the terrain was clamped to the road).
      aabb.Minimum.Y = 0;
      aabb.Maximum.Y = 0;

      // Convert to submesh.
      submesh = new Submesh
      {
        PrimitiveCount = indices.Count / 3,
        PrimitiveType = PrimitiveType.TriangleList,
        VertexCount = vertices.Count,
        VertexBuffer = new VertexBuffer(graphicsDevice, TerrainLayerVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly),
        IndexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly)
      };
      submesh.VertexBuffer.SetData(vertices.ToArray());
      submesh.IndexBuffer.SetData(indices.ToArray());
    }


    /// <summary>
    /// Clamps a road path to the terrain height.
    /// </summary>
    /// <param name="road">The path that represents the road.</param>
    /// <param name="terrain">The terrain represented by a <see cref="HeightField"/>.</param>
    /// <remarks>
    /// The y position of each path key is set to the terrain height at the xz position.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="terrain"/> is <see langword="null"/>.
    /// </exception>
    public static void ClampRoadToTerrain(Path3F road, HeightField terrain)
    {
      if (road == null)
        return;
      if (terrain == null)
        throw new ArgumentNullException("terrain");

      foreach (var key in road)
      {
        Vector3F position = key.Point;
        float height = terrain.GetHeight(position.X, position.Z);
        if (!Numeric.IsNaN(height))
        {
          position.Y = height;
          key.Point = position;
        }
      }
    }


    /// <summary>
    /// Clamps the terrain height to the road ("carves the road into the terrain").
    /// </summary>
    /// <param name="terrain">The terrain represented by a <see cref="HeightField"/>.</param>
    /// <param name="road">The path that represents the road.</param>
    /// <param name="defaultWidth">The default road width.</param>
    /// <param name="defaultSideFalloff">The default side falloff.</param>
    /// <param name="maxNumberOfIterations">
    /// The maximum number of iterations (used when tessellating the path).
    /// </param>
    /// <param name="tolerance">
    /// The tolerance in world space units (used when tessellating the path).
    /// </param>
    /// <remarks>
    /// <para>
    /// A road is defined by the specified 3D path. The path keys can be of type
    /// <see cref="TerrainRoadPathKey"/>. This allows to add additional information to the path,
    /// like varying road <see cref="TerrainRoadPathKey.Width"/> and 
    /// <see cref="TerrainRoadPathKey.SideFalloff"/>. If the path keys are of any
    /// other <see cref="PathKey3F"/> type, the <paramref name="defaultWidth"/> and
    /// <paramref name="defaultSideFalloff"/> are used.
    /// </para>
    /// <para>
    /// The heights of the given height field are adjusted to the road height - the road is
    /// "carved" into the terrain. At the side of the roads the terrain heights fall off smoothly
    /// to the existing terrain heights. The influence range on each side is defined by the
    /// <see cref="TerrainRoadPathKey.SideFalloff"/> in the <see cref="TerrainRoadPathKey"/>s or
    /// by the <paramref name="defaultSideFalloff"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="terrain"/> or <paramref name="road"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public static void ClampTerrainToRoad(HeightField terrain, Path3F road,
                                          float defaultWidth, float defaultSideFalloff,
                                          int maxNumberOfIterations, float tolerance)
    {
      if (terrain == null)
        throw new ArgumentNullException("terrain");
      if (road == null)
        throw new ArgumentNullException("road");

      // Compute list of line segments. (2 points per line segment!)
      var flattenedPoints = new List<Vector3F>();
      road.Flatten(flattenedPoints, maxNumberOfIterations, tolerance);

      // Abort if path is empty.
      int numberOfLineSegments = flattenedPoints.Count / 2;
      if (numberOfLineSegments <= 0)
        return;

      // Compute accumulated lengths. (One entry for each entry in flattenedPoints.)
      float[] accumulatedLengths = new float[flattenedPoints.Count];
      accumulatedLengths[0] = 0;
      for (int i = 1; i < flattenedPoints.Count; i += 2)
      {
        Vector3F previous = flattenedPoints[i - 1];
        Vector3F current = flattenedPoints[i];
        float length = (current - previous).Length;

        accumulatedLengths[i] = accumulatedLengths[i - 1] + length;
        if (i + 1 < flattenedPoints.Count)
          accumulatedLengths[i + 1] = accumulatedLengths[i];
      }

      // Create a mapping between accumulatedLength and the path keys.
      // (accumulatedLength --> key)
      var pathLengthsAndKeys = new List<Pair<float, TerrainRoadPathKey>>();
      {
        int index = 0;
        foreach (var key in road)
        {
          Vector3F position = key.Point;
          var roadKey = key as TerrainRoadPathKey;
          if (roadKey == null)
          {
            roadKey = new TerrainRoadPathKey
            {
              Point = key.Point,
              Width = defaultWidth,
              SideFalloff = defaultSideFalloff,
            };
          }

          for (; index < flattenedPoints.Count; index++)
          {
            if (Vector3F.AreNumericallyEqual(position, flattenedPoints[index]))
            {
              pathLengthsAndKeys.Add(new Pair<float, TerrainRoadPathKey>(accumulatedLengths[index], roadKey));
              break;
            }

            bool isLastLineSegment = (index + 2 == flattenedPoints.Count);
            if (!isLastLineSegment)
              index++;
          }

          index++;
        }
      }

      // Create a list of interpolated road widths and side falloffs. (One entry for each entry in flattenedPoints.)
      var halfWidths = new float[flattenedPoints.Count];
      var sideFalloffs = new float[flattenedPoints.Count];
      int previousKeyIndex = 0;
      var previousKey = pathLengthsAndKeys[0];
      var nextKey = pathLengthsAndKeys[1];
      halfWidths[0] = 0.5f * pathLengthsAndKeys[0].Second.Width;
      sideFalloffs[0] = pathLengthsAndKeys[0].Second.SideFalloff;
      for (int i = 1; i < flattenedPoints.Count; i += 2)
      {
        if (accumulatedLengths[i] > nextKey.First)
        {
          previousKey = nextKey;
          previousKeyIndex++;
          nextKey = pathLengthsAndKeys[previousKeyIndex + 1];
        }

        float p = (accumulatedLengths[i] - previousKey.First) / (nextKey.First - previousKey.First);
        halfWidths[i] = 0.5f * InterpolationHelper.Lerp(previousKey.Second.Width, nextKey.Second.Width, p);
        sideFalloffs[i] = InterpolationHelper.Lerp(previousKey.Second.SideFalloff, nextKey.Second.SideFalloff, p);

        if (i + 1 < flattenedPoints.Count)
        {
          halfWidths[i + 1] = halfWidths[i];
          sideFalloffs[i + 1] = sideFalloffs[i];
        }
      }

      // Get AABB of road with the side falloff.
      Aabb aabbWithSideFalloffs;
      {
        Vector3F p = flattenedPoints[0];
        float r = halfWidths[0] + sideFalloffs[0];
        aabbWithSideFalloffs = new Aabb(new Vector3F(p.X - r, 0, p.Z - r),
                                        new Vector3F(p.X + r, 0, p.Z + r));
        for (int i = 1; i < flattenedPoints.Count; i += 2)
        {
          p = flattenedPoints[i];
          r = halfWidths[i] + sideFalloffs[i];
          aabbWithSideFalloffs.Grow(new Vector3F(p.X - r, 0, p.Z - r));
          aabbWithSideFalloffs.Grow(new Vector3F(p.X + r, 0, p.Z + r));
        }
      }

      // Terrain properties.
      int numberOfSamplesX = terrain.NumberOfSamplesX;
      int numberOfSamplesZ = terrain.NumberOfSamplesZ;
      int numberOfCellsX = numberOfSamplesX - 1;
      int numberOfCellsZ = numberOfSamplesZ - 1;
      float widthX = terrain.WidthX;
      float cellSizeX = widthX / numberOfCellsX;
      float widthZ = terrain.WidthZ;
      float cellSizeZ = widthZ / numberOfCellsZ;
      float cellSizeDiagonal = (float)Math.Sqrt(cellSizeX * cellSizeX + cellSizeZ * cellSizeZ);

      bool isClosed = Vector3F.AreNumericallyEqual(flattenedPoints[0], flattenedPoints[flattenedPoints.Count - 1]);

      {
        // Get the line segments which of the road border.
        List<Vector4F> segments = new List<Vector4F>();  // 2 points per segment.
        Vector3F lastOrthonormal = Vector3F.Right;
        Vector4F previousV1 = Vector4F.Zero;
        Vector4F previousV2 = Vector4F.Zero;
        for (int i = 0; i < flattenedPoints.Count; i++)
        {
          Vector3F start = flattenedPoints[i];

          Vector3F previous;
          bool isFirstPoint = (i == 0);
          if (!isFirstPoint)
            previous = flattenedPoints[i - 1];
          else if (isClosed && road.SmoothEnds)
            previous = flattenedPoints[flattenedPoints.Count - 2];
          else
            previous = start;

          Vector3F next;
          bool isLastPoint = (i + 1 == flattenedPoints.Count);
          if (!isLastPoint)
            next = flattenedPoints[i + 1];
          else if (isClosed && road.SmoothEnds)
            next = flattenedPoints[1];
          else
            next = start;

          Vector3F tangent = next - previous;

          Vector3F orthonormal = new Vector3F(tangent.Z, 0, -tangent.X);
          if (!orthonormal.TryNormalize())
            orthonormal = lastOrthonormal;

          // Add 2 vertices two segments for the road side border.
          //
          //  pV1        pV2 (previous vertices)
          //  x           x
          //  |           |
          //  x           x
          //  v1          v2 (current vertices)
          //
          // We store the side falloff with the vertex:
          // Vectors are 4D. Height is y. Side falloff is w.
          Vector4F v1 = new Vector4F(start - orthonormal * (halfWidths[i] + 0), sideFalloffs[i]);
          Vector4F v2 = new Vector4F(start + orthonormal * (halfWidths[i] + 0), sideFalloffs[i]);

          if (i > 0)
          {
            segments.Add(previousV1);
            segments.Add(v1);
            segments.Add(previousV2);
            segments.Add(v2);

            if (isLastPoint && !isClosed)
            {
              // A segment for the end of the road.
              segments.Add(v1);
              segments.Add(v2);
            }
          }
          else
          {
            if (!isClosed)
            {
              // A segment for the start of the road.
              segments.Add(v1);
              segments.Add(v2);
            }
          }

          previousV1 = v1;
          previousV2 = v2;

          lastOrthonormal = orthonormal;

          // The flattened points list contains 2 points per line segment, which means that there
          // are duplicate intermediate points, which we skip.
          bool isLastLineSegment = (i + 2 == flattenedPoints.Count);
          if (!isLastLineSegment)
            i++;
        }

        // Apply the side falloff to the terrain heights.
        // We use a padding where the road influence is 100% because we want road width to be flat 
        // but that means that we also have to set some triangle vertices outside the road width to 
        // full 100% road height.
        float padding = cellSizeDiagonal;
        ClampHeightsToLineSegments(terrain, aabbWithSideFalloffs, segments, padding);
      }

      // Clamp the terrain heights to the inner part of the road.
      // We create quads for the road mesh and clamp the heights to the quad triangles.
      {
        Vector3F previousV1 = Vector3F.Zero;
        Vector3F previousV2 = Vector3F.Zero;
        Vector3F lastOrthonormal = Vector3F.Right;
        for (int i = 0; i < flattenedPoints.Count; i++)
        {
          Vector3F start = flattenedPoints[i];

          Vector3F previous;
          bool isFirstPoint = (i == 0);
          if (!isFirstPoint)
            previous = flattenedPoints[i - 1];
          else if (isClosed && road.SmoothEnds)
            previous = flattenedPoints[flattenedPoints.Count - 2];
          else
            previous = start;

          Vector3F next;
          bool isLastPoint = (i + 1 == flattenedPoints.Count);
          if (!isLastPoint)
            next = flattenedPoints[i + 1];
          else if (isClosed && road.SmoothEnds)
            next = flattenedPoints[1];
          else
            next = start;

          Vector3F tangent = next - previous;

          Vector3F orthonormal = new Vector3F(tangent.Z, 0, -tangent.X);
          if (!orthonormal.TryNormalize())
            orthonormal = lastOrthonormal;

          // Add 2 vertices to create a mesh like this:
          //
          //  pV1             pV2 (previous vertices)
          //  x---------------x
          //  |               |
          //  x---------------x
          //  v1              v2 (current vertices)
          //
          // Then we check all height samples against these triangles.

          // Vectors are 4D. Height is y. Influence is w.
          Vector3F v1 = start - orthonormal * halfWidths[i];
          Vector3F v2 = start + orthonormal * halfWidths[i];

          if (i > 0)
            ClampHeightsToQuad(terrain, previousV1, previousV2, v1, v2);

          previousV1 = v1;
          previousV2 = v2;

          lastOrthonormal = orthonormal;

          // The flattened points list contains 2 points per line segment, which means that there
          // are duplicate intermediate points, which we skip.
          bool isLastLineSegment = (i + 2 == flattenedPoints.Count);
          if (!isLastLineSegment)
            i++;
        }
      }

      terrain.Invalidate();
    }


    private static void ClampHeightsToQuad(HeightField terrain, Vector3F pV1, Vector3F pV2,
                                              Vector3F v1, Vector3F v2)
    {
      // Handle 2 triangles:
      //
      //  pV1  pV2 (previous vertices)
      //  x----x
      //  |   /|
      //  |  / |
      //  | /  |
      //  |/   |
      //  x----x
      //  v1   v2 (current vertices)

      ClampHeightsToTriangle(terrain, pV1, pV2, v1);
      ClampHeightsToTriangle(terrain, pV2, v2, v1);
    }


    private static void ClampHeightsToTriangle(HeightField terrain, Vector3F vertexA, Vector3F vertexB, Vector3F vertexC)
    {
      // This code is a like an unoptimized software rasterizer.
      // TODO: Optimize this (see software rasterizers).

      float originX = terrain.OriginX;
      float originZ = terrain.OriginZ;
      int numberOfSamplesX = terrain.NumberOfSamplesX;
      int numberOfSamplesZ = terrain.NumberOfSamplesZ;
      int numberOfCellsX = numberOfSamplesX - 1;
      int numberOfCellsZ = numberOfSamplesZ - 1;
      float widthX = terrain.WidthX;
      float cellSizeX = widthX / numberOfCellsX;
      float widthZ = terrain.WidthZ;
      float cellSizeZ = widthZ / numberOfCellsZ;
      float[] heights = terrain.Samples;

      // Get min and max indices (inclusive).
      float minX = Min(vertexA.X, vertexB.X, vertexC.X);
      float maxX = Max(vertexA.X, vertexB.X, vertexC.X);
      float minZ = Min(vertexA.Z, vertexB.Z, vertexC.Z);
      float maxZ = Max(vertexA.Z, vertexB.Z, vertexC.Z);

      Vector2F a = new Vector2F(vertexA.X, vertexA.Z);
      Vector2F b = new Vector2F(vertexB.X, vertexB.Z);
      Vector2F c = new Vector2F(vertexC.X, vertexC.Z);

      // Get min and max indices (inclusive).
      int indexXMin = Math.Max(0, (int)Math.Floor((minX - originX) / cellSizeX));
      int indexZMin = Math.Max(0, (int)Math.Floor((minZ - originZ) / cellSizeZ));
      int indexXMax = Math.Min(numberOfSamplesX - 1, (int)Math.Ceiling((maxX - originX) / cellSizeX));
      int indexZMax = Math.Min(numberOfSamplesZ - 1, (int)Math.Ceiling((maxZ - originZ) / cellSizeZ));

      // Values for the barycentric computation:
      Vector2F v0 = b - a;
      Vector2F v1 = c - a;
      float den = v0.X * v1.Y - v1.X * v0.Y;

      Parallel.For(indexZMin, indexZMax + 1, indexZ =>
      //for (int indexZ = indexZMin; indexZ <= indexZMax; indexZ++)
      {
        for (int indexX = indexXMin; indexX <= indexXMax; indexX++)
        {
          Vector2F p = new Vector2F(originX + cellSizeX * indexX, originZ + cellSizeZ * indexZ);

          // Get barycentric coordinates.
          Vector2F v2 = p - a;
          float v = (v2.X * v1.Y - v1.X * v2.Y) / den;
          float w = (v0.X * v2.Y - v2.X * v0.Y) / den;
          float u = 1.0f - v - w;

          var epsilonF = 0.0;
          if (u < -epsilonF || u > 1 + epsilonF)
            continue;
          if (v < -epsilonF || v > 1 + epsilonF)
            continue;
          if (w < -epsilonF || w > 1 + epsilonF)
            continue;

          float height = u * vertexA.Y + v * vertexB.Y + w * vertexC.Y;
          heights[indexZ * numberOfSamplesX + indexX] = height;
        }
      });
    }


    private static void ClampHeightsToLineSegments(HeightField terrain, Aabb aabb, List<Vector4F> segments, float padding)
    {
      // TODO: Optimize this (see software rasterizers).

      float originX = terrain.OriginX;
      float originZ = terrain.OriginZ;
      int numberOfSamplesX = terrain.NumberOfSamplesX;
      int numberOfSamplesZ = terrain.NumberOfSamplesZ;
      int numberOfCellsX = numberOfSamplesX - 1;
      int numberOfCellsZ = numberOfSamplesZ - 1;
      float widthX = terrain.WidthX;
      float cellSizeX = widthX / numberOfCellsX;
      float widthZ = terrain.WidthZ;
      float cellSizeZ = widthZ / numberOfCellsZ;
      float[] heights = terrain.Samples;

      // Get min and max indices (inclusive).
      float minX = aabb.Minimum.X;
      float maxX = aabb.Maximum.X;
      float minZ = aabb.Minimum.Z;
      float maxZ = aabb.Maximum.Z;

      // Get min and max indices (inclusive).
      int indexXMin = Math.Max(0, (int)Math.Floor((minX - originX) / cellSizeX));
      int indexZMin = Math.Max(0, (int)Math.Floor((minZ - originZ) / cellSizeZ));
      int indexXMax = Math.Min(numberOfSamplesX - 1, (int)Math.Ceiling((maxX - originX) / cellSizeX));
      int indexZMax = Math.Min(numberOfSamplesZ - 1, (int)Math.Ceiling((maxZ - originZ) / cellSizeZ));
      Parallel.For(indexZMin, indexZMax + 1, indexZ =>
      //for (int indexZ = indexZMin; indexZ <= indexZMax; indexZ++)
      {
        for (int indexX = indexXMin; indexX <= indexXMax; indexX++)
        {
          Vector3F terrainPointFlat = new Vector3F(originX + cellSizeX * indexX, 0, originZ + cellSizeZ * indexZ);

          float bestSegmentInfluence = 0;
          float bestSegmentHeight = 0; 
          for (int segmentIndex = 0; segmentIndex < segments.Count / 2; segmentIndex++)
          {
            var segmentStartFlat = new Vector3F(segments[segmentIndex * 2].X, 0, segments[segmentIndex * 2].Z);
            var segmentEndFlat = new Vector3F(segments[segmentIndex * 2 + 1].X, 0, segments[segmentIndex * 2 + 1].Z);
            var segment = new LineSegment(segmentStartFlat, segmentEndFlat);
            float parameter;
            GetLineParameter(ref segment, ref terrainPointFlat, out parameter);
            Vector4F closestPoint = segments[segmentIndex * 2] + parameter * (segments[segmentIndex * 2 + 1] - segments[segmentIndex * 2]);
            Vector3F closestPointFlat = new Vector3F(closestPoint.X, 0, closestPoint.Z);
            float distance = (closestPointFlat - terrainPointFlat).Length - padding;
            float influence = MathHelper.Clamp(1 - distance / (closestPoint.W - padding), 0, 1);
            if (influence > bestSegmentInfluence)
            {
              bestSegmentInfluence = influence;
              bestSegmentHeight = closestPoint.Y;
            }
          }

          if (bestSegmentInfluence > 0)
          {
            heights[indexZ * numberOfSamplesX + indexX] = InterpolationHelper.Lerp(
              heights[indexZ * numberOfSamplesX + indexX],
              bestSegmentHeight,
              InterpolationHelper.HermiteSmoothStep(bestSegmentInfluence));
          }
        }
      });
    }


    internal static void GetLineParameter(ref LineSegment lineSegment, ref Vector3F point, out float parameter)
    {
      float lengthSquared = lineSegment.LengthSquared;
      if (lengthSquared < Numeric.EpsilonFSquared)
      {
        // Segment has zero length.
        parameter = 0;
        return;
      }

      Vector3F lineToPoint = point - lineSegment.Start;

      // Parameter computed from equation 20.
      parameter = Vector3F.Dot(lineSegment.End - lineSegment.Start, lineToPoint) / lengthSquared;

      parameter = Math.Max(0, Math.Min(1, parameter));
    }


    private static float Min(float value0, float value1, float value2)
    {
      return Math.Min(Math.Min(value0, value1), value2);
    }


    private static float Max(float value0, float value1, float value2)
    {
      return Math.Max(Math.Max(value0, value1), value2);
    }
  }
}
