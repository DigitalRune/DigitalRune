// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics.Algebra;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif


namespace DigitalRune.Geometry.Shapes
{
  /// <summary>
  /// Represents a convex polyhedron.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A <see cref="ConvexPolyhedron"/> is similar to a <see cref="ConvexHullOfPoints"/> except that
  /// the shape is fixed and the points that represent the shape cannot be changed. The 
  /// <see cref="ConvexPolyhedron"/> is optimized and provides a faster support mapping (see 
  /// <see cref="GetSupportPoint"/>) than the <see cref="ConvexHullOfPoints"/>.
  /// </para>
  /// <para>
  /// Use a <see cref="ConvexHullOfPoints"/> if the points in the shape need to be modified at 
  /// runtime.
  /// </para>
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public class ConvexPolyhedron : ConvexShape
  {
    // ConvexPolyhedron uses a directional lookup table (LUT) and vertex adjacency lists for support 
    // point queries when the number of vertices exceed a certain vertex threshold.
    // The implementation is based on Ehmann, S., Lin, M.C.: Accelerated proximity queries between 
    // convex polyhedra using multi-level voronoi marching."
    // We use the center of the circumscribed sphere instead of the center of mass for building the 
    // lookup table.

    #region ----- Version 1: Spherical Lookup Table (Obsolete) -----

    // The initial version uses spherical lookup table as described by Ehmann and Lin. 
    // That means the tables is indexed by inclination and azimuth angle.

    // Memory consumption
    // ==================
    // 
    // The directional lookup table has a significant memory overhead. Below is a table showing
    // the memory required depending on the number of entries. (The last column is just for
    // comparison. It shows the number of vertices that equal the same amount of memory.)
    // 
    //   Inclination entries | Azimuth entries | Angle  | Size of table | Number of vertices
    //   -----------------------------------------------------------------------------------
    //   65                  | 128             | 2.8°   | 31.5 KB       | 2688 vertices
    //   33                  | 64              | 5.625° | 7.75 KB       | 662 vertices
    //   17                  | 32              | 11.25° | 1920 B        | 160 vertices
    //   9                   | 16              | 22.5°  | 448 B         | 38 vertices
    //   5                   | 8               | 45°    | 96 B          | 8 vertices
    //
    //   Note: Ehmann and Lin use a LUT with 5.625° with radius factor 2 for their comparisons 
    //   with Dopkin-Kirkpatrick and QSlim hierarchies. 
    //   The table assumes that the LUT stores Int32. If Int16 are used the size is half.
    //
    // Increasing the size of the lookup table always improves the performance of the support points 
    // queries. However, the performance gain is minimal and probably not worth the enormous memory
    // costs.
    //
    // In additional to the lookup table, the algorithm requires vertex adjacency information. The
    // memory costs of the adjacency lists is not included in the table above.

    
    // Performance
    // ===========
    //
    // Experiments regarding the number of required checks in a support point query have shown:
    //   - The brute force approach using only the vertices in the convex hull requires exactly n 
    //     checks (one distance computation per vertex).
    //   - Hill-climbing using the adjacency information starting at a random vertex performs worse 
    //     than the brute force approach when the number of vertices is small (<12 vertices). That 
    //     means, it requires more than >=n checks! With increasing number of vertices (>12 vertices) 
    //     it requires on average less checks than the brute force approach. It performs about >2 
    //     times faster when the number of vertices is very large (>40 vertices).
    //   - Using the directional lookup table and hill-climbing requires on average less than n 
    //     checks. At already 12 vertices it requires only n/2 checks. Interestingly, as the number 
    //     of vertices increases, the number of checks stays almost constant: Most support point 
    //     queries require 8-12 checks independent of the number vertices. Even convex polyhedra 
    //     with 100 vertices require on average only 8-12 checks. The algorithm performs slightly
    //     worse when objects are extremely flat: With a width:height ratio of 100:1 it requires on 
    //     average 12-16 checks when objects are large (50-100 vertices).
    // 
    // The experiments were performed with convex hulls creates from random point clouds. Random
    // support directions were sampled from these convex polyhedras.
    //
    // The experiments above show only the number of required checks (distance computations in 
    // the support point query). The experiments did not measure the actual time required for all
    // checks.
    // Directional lookup table + hill-climbing has an additional overhead:
    //   - A constant overhead: The lookup in the table.
    //   - And a linear overhead: Accessing the adjacency information during hill-climbing.
    //
    // Additional experiments were performed to measure the overhead and to find the break-even 
    // point: The break even points seems to be at 16 vertices.
    //   
    //   # vertices in convex hull | Speed-up (time brute force / time LUT)
    //   ----------------------------------------------------
    //   16                        | 1.05
    //   24                        | 1.45
    //   32                        | 1.75
    //   48                        | 2.4
    //   64                        | 3.0
    //   100                       | 4.0
    //
    //   Note: Performance comparisons were measure using a 22.5° LUT. A 11.25° LUT performs only 
    //   marginally better - speed-up increase ≤ 10%. Random convex objects with width:height:depth 
    //   ratio of 10:5:2 were tested. 

    #endregion


    #region ----- Version 2: Cube Map Lookup Table -----

    // The Cartesian to spherical coordinates conversion was expensive and a spherical coordinates
    // show a non-uniform distribution along the unit sphere with poles at the top and bottom.
    // The new version uses a lookup table based on a cube map. Advantages:
    //   - The table can be indexed directly with the direction vector - no conversion needed. 
    //   - The samples are much more uniformly distributed.
    //   - The size of the cube map is easier to control.

    
    // Performance
    // ===========
    //
    // Experiments have shown that the break even point is at ~12 vertices. (However, cache misses 
    // due to the additional memory overhead were not considered in the experiments.)
    // The speed-up is significantly better than version 1. With 100 vertices the speed-up is ~7.

    #endregion

    
    #region ----- Further Optimizations -----

    // Further Optimizations
    // =====================
    //
    // Further measurement and optimizations should be applied if extremely large polyhedra are 
    // used. Hints: 
    //   - Increase number of entries in the lookup table. Examples are shown in the table above.
    //   - Try different radius factors. Radius factors should lie in the range [1, ∞).
    //   - Try using a different center. Currently center of circumscribed sphere. Try center of mass.
    //   - The performance test above did not consider cache misses, so the performance should also
    //     be checked in a real world application.

    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// The vertex threshold. If the number of vertices exceeds the vertex threshold a directional
    /// lookup table and vertex adjacency lists are used internally.
    /// </summary>
    private const int VertexThreshold = 16;   // Break even point performance-wise is 12.
                                              // But there is a memory overhead which should be 
                                              // avoided when the number of vertices is small.
                                              // 16 vertices require the same amount of memory
                                              // as the lookup table. I think that's a good time
                                              // to enable the lookup table.
    
    /// <summary>
    /// The radius factor used for building the directional lookup table.
    /// </summary>
    private const float RadiusFactor = 2;


    /// <summary>
    /// The width of the directional lookup table (= the length of a cube map side).
    /// </summary>
    private const int LookupTableWidth = 4;
    #endregion

    
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // The cached local space AABB
    private Aabb _aabbLocal = new Aabb(new Vector3F(float.NaN), new Vector3F(float.NaN));
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets an inner point.
    /// </summary>
    /// <value>
    /// An inner point which is the average of all vertices; or (0, 0, 0) if the convex polyhedron
    /// is empty.
    /// </value>
    /// <remarks>
    /// This point is a "deep" inner point of the shape (in local space).
    /// </remarks>
    public override Vector3F InnerPoint
    {
      get { return _innerPoint; }
    }
    private Vector3F _innerPoint = new Vector3F(float.NaN);


    /// <summary>
    /// Gets a read-only list with the vertices of the convex polyhedron.
    /// </summary>
    /// <value>
    /// A read-only list with the vertices of the convex polyhedron.
    /// </value>
    [XmlIgnore]
    public ReadOnlyCollection<Vector3F> Vertices
    {
      get
      {
        if (_verticesReadOnly == null)
         _verticesReadOnly = new ReadOnlyCollection<Vector3F>(_vertices);

        return _verticesReadOnly;
      }
    }
    private Vector3F[] _vertices;
    private ReadOnlyCollection<Vector3F> _verticesReadOnly;


    /// <summary>
    /// Gets the directional lookup table. (For internal use only.)
    /// </summary>
    /// <value>The directional lookup table.</value>
    internal DirectionalLookupTableUInt16F DirectionalLookupTable
    {
      get { return _directionLookupTable; }
    }
    private DirectionalLookupTableUInt16F _directionLookupTable;


    /// <summary>
    /// Gets the vertex adjacency lists. (For internal use only.)
    /// </summary>
    /// <value>The vertex adjacency.</value>
    internal VertexAdjacency VertexAdjacency
    {
      get { return _vertexAdjacency; }
    }
    private VertexAdjacency _vertexAdjacency;


#if PORTABLE || WINDOWS
    /// <exclude/>
#if !PORTABLE
    [Browsable(false)]
#endif
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      get
      {
        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.DirectionalLookupTable = DirectionalLookupTable;
        //internals.VertexAdjacency = VertexAdjacency;
        //return internals;

        IDictionary<string, Object> internals = new ExpandoObject();
        internals["DirectionalLookupTable"] = DirectionalLookupTable;
        internals["VertexAdjacency"] = VertexAdjacency;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexPolyhedron"/> class. (For internal use 
    /// only.)
    /// </summary>
    internal ConvexPolyhedron()
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ConvexPolyhedron"/> class.
    /// </summary>
    /// <param name="points">
    /// A collection of points which from which a convex polyhedron is built.
    /// </param>
    /// <remarks>
    /// The convex polyhedron is the convex hull of <paramref name="points"/>. Points within the
    /// convex hull are automatically removed.
    /// </remarks>
    /// <exception cref="NotSupportedException">
    /// Too many vertices in convex hull. Max. 65534 vertices in a convex polyhedron are supported.
    /// </exception>
    public ConvexPolyhedron(IEnumerable<Vector3F> points)
    {
      BuildConvexPolyhedron(points);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    internal void Set(Vector3F[] vertices, Aabb aabb, Vector3F innerPoint, DirectionalLookupTableUInt16F directionalLookupTable, VertexAdjacency vertexAdjacency)
    {
      _vertices = vertices;
      _aabbLocal = aabb;
      _innerPoint = innerPoint;
      _directionLookupTable = directionalLookupTable;
      _vertexAdjacency = vertexAdjacency;
    }


    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override Shape CreateInstanceCore()
    {
      return new ConvexPolyhedron();
    }


    /// <inheritdoc/>
    protected override void CloneCore(Shape sourceShape)
    {
      var source = (ConvexPolyhedron)sourceShape;
      _vertices = source._vertices;
      _aabbLocal = source._aabbLocal;
      _innerPoint = source._innerPoint;
      _directionLookupTable = source._directionLookupTable;
      _vertexAdjacency = source._vertexAdjacency;
    }
    #endregion


    #region ----- Shape -----

    /// <inheritdoc/>
    public override Aabb GetAabb(Vector3F scale, Pose pose)
    {
      // Apply scale and pose to AABB.
      return _aabbLocal.GetAabb(scale, pose);
    }


    /// <summary>
    /// Gets a support point for a given direction.
    /// </summary>
    /// <param name="direction">
    /// The direction for which to get the support point. The vector does not need to be normalized.
    /// The result is undefined if the vector is a zero vector.
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3F GetSupportPoint(Vector3F direction)
    {
      return GetSupportPointInternal(ref direction);
    }


    /// <summary>
    /// Gets a support point for a given normalized direction vector.
    /// </summary>
    /// <param name="directionNormalized">
    /// The normalized direction vector for which to get the support point. 
    /// </param>
    /// <returns>A support point regarding the given direction.</returns>
    /// <remarks>
    /// <para>
    /// A support point regarding a direction is an extreme point of the shape that is furthest away 
    /// from the center regarding the given direction. This point is not necessarily unique.
    /// </para>
    /// </remarks>
    public override Vector3F GetSupportPointNormalized(Vector3F directionNormalized)
    {
      return GetSupportPointInternal(ref directionNormalized);
    }


    private Vector3F GetSupportPointInternal(ref Vector3F direction)
    {
      Vector3F supportVertex = new Vector3F();
      if (_directionLookupTable != null)
      {
        GetSupportPointLut(ref direction, out supportVertex);
      }
      else
      {
        // ----- Brute force search.
        // Return point with the largest distance in the given direction.
        float maxDistance = float.NegativeInfinity;
        for (int i = 0; i < _vertices.Length; i++)
        {
          Vector3F vertex = _vertices[i];
          float distance = Vector3F.Dot(vertex, direction);
          if (distance > maxDistance)
          {
            supportVertex = vertex;
            maxDistance = distance;
          }
        }
      }

      return supportVertex;
    }


    /// <summary>
    /// Called when a mesh should be generated for the shape.
    /// </summary>
    /// <param name="absoluteDistanceThreshold">The absolute distance threshold.</param>
    /// <param name="iterationLimit">The iteration limit.</param>
    /// <returns>The triangle mesh for this shape.</returns>
    protected override TriangleMesh OnGetMesh(float absoluteDistanceThreshold, int iterationLimit)
    {
      DcelMesh mesh = GeometryHelper.CreateConvexHull(_vertices);
      return mesh.ToTriangleMesh();
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "ConvexPolyhedron {{ Count = {0} }}", _vertices.Length);
    }
    #endregion


    /// <summary>
    /// Initializes the convex polyhedron.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <exception cref="NotSupportedException">
    /// Too many vertices in convex hull. Max. 65534 vertices in a convex polyhedron are supported.
    /// </exception>
    private void BuildConvexPolyhedron(IEnumerable<Vector3F> points)
    {
      // Build convex hull and throw away inner points.
      DcelMesh convexHull = GeometryHelper.CreateConvexHull(points);
      int numberOfVertices = (convexHull != null) ? convexHull.Vertices.Count : 0;
      if (numberOfVertices >= ushort.MaxValue)
        throw new NotSupportedException("Too many vertices in convex hull. Max. 65534 vertices in convex hull are supported.");

      _vertices = (convexHull != null) ? convexHull.Vertices.Select(v => v.Position).ToArray() : new Vector3F[0];
      CacheAabb();
      CacheInnerPoint();

      Debug.Assert(VertexThreshold > 4, "Vertex threshold should be greater than 4.");
      if (numberOfVertices > VertexThreshold)
      {
        // Use an internal lookup table and vertex adjacency lists for speed up.
        BuildLookupTable();
        BuildVertexAdjacencyLists(convexHull);
      }
    }


    private void CacheAabb()
    {
      _aabbLocal = base.GetAabb(Vector3F.One, Pose.Identity);
    }


    private void CacheInnerPoint()
    {
      // Compute the average of all points.
      int numberOfPoints = _vertices.Length;
      Vector3F innerPoint = new Vector3F();
      if (numberOfPoints > 0)
      {
        for (int i = 0; i < numberOfPoints; i++)
        {
          Vector3F point = _vertices[i];
          innerPoint += point;
        }

        innerPoint /= numberOfPoints;
      }

      _innerPoint = innerPoint;
    }


    private void BuildLookupTable()
    {
      // Compute center and radius of the sphere where points are sampled.
      float polyhedronRadius;
      Vector3F center;
      GeometryHelper.ComputeBoundingSphere(_vertices, out polyhedronRadius, out center);
      float radius = polyhedronRadius * RadiusFactor;

      // Create directional lookup table. (The poles are excluded.)
      _directionLookupTable = new DirectionalLookupTableUInt16F(LookupTableWidth);

      // Sample points on the sphere and determine the indices of the closest vertices on the 
      // convex polyhedron. The indices of the closest vertices are the entries in the lookup
      // table.
      foreach (Vector3F direction in _directionLookupTable.GetSampleDirections())
      {
        direction.Normalize();
        Vector3F samplePoint = direction * radius;
        _directionLookupTable[direction] = GetClosestVertex(samplePoint);
      }
    }


    private ushort GetClosestVertex(Vector3F samplePoint)
    {
      // Brute force search for closest vertex.
      int minIndex = -1;
      float minDistanceSquared = float.MaxValue;
      for (ushort i = 0; i < _vertices.Length; i++)
      {
        float distanceSquared = (samplePoint - _vertices[i]).LengthSquared;
        if (distanceSquared < minDistanceSquared)
        {
          minDistanceSquared = distanceSquared;
          minIndex = i;
        }
      }

      Debug.Assert(0 <= minIndex && minIndex < _vertices.Length, "GetClosestVertex() has failed.");
      return (ushort)minIndex;
    }


    private void BuildVertexAdjacencyLists(DcelMesh convexHull)
    {
      _vertexAdjacency = new VertexAdjacency(convexHull);
    }


    // JIT Optimization BUG!!! When the JIT compiler compiles this code with optimizations,
    // it removes the adjacentDistance = ... code and creates an endless loop!!!! This is a serious 
    // JIT optimization bug. The bug does not occur when JIT optimization is turned off or when we 
    // inline Vector3F.Dot() below.
    //[MethodImpl(MethodImplOptions.NoOptimization)]
    private void GetSupportPointLut(ref Vector3F direction, out Vector3F supportVertex)
    {
      // The direction vector does not need to be normalized: Below we project the points onto
      // the direction vector and measure the length of the projection. We do not need the correct 
      // length, we only need a value which we can compare.

      // First we need to find a start index where we start to search for the support point.
      ushort index = _directionLookupTable[direction];

      // The index from the directional lookup table is our initial guess.
      Vector3F vertex = _vertices[index];
      float distance = Vector3F.Dot(vertex, direction);
      ushort maxIndex = index;
      ushort lastIndex = ushort.MaxValue;
      do
      {
        index = maxIndex;

        // Get the adjacency list.
        ushort start = _vertexAdjacency.ListIndices[index];     // Start index of adjacency list.
        ushort end = _vertexAdjacency.ListIndices[index + 1];   // End index of adjacency list.

        // Check whether an adjacent vertex yields a better support point.
        for (ushort i = start; i < end; i++)
        {
          // Get the index of the adjacent vertex from the adjacency list.
          ushort adjacentIndex = _vertexAdjacency.Lists[i];

          // Skip the vertex where we came from.
          if (adjacentIndex == lastIndex)
            continue;

          // Check whether the adjacent vertex is a better support point.
          Vector3F adjacent = _vertices[adjacentIndex];

          // JIT Optimization Bug!!! See comment above.
          //float adjacentDistance = Vector3F.Dot(adjacent, direction);
          float adjacentDistance = adjacent.X * direction.X + adjacent.Y * direction.Y + adjacent.Z * direction.Z;

          if (adjacentDistance > distance)
          {
            vertex = adjacent;
            distance = adjacentDistance;
            maxIndex = adjacentIndex;
          }
        }

        lastIndex = index;
      } while (index != maxIndex);

      supportVertex = vertex;
    }
    #endregion
  }
}
