// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Manages a simplex of a CSO (configuration space obstacle, Minkowski difference A-B).
  /// </summary>
  /// <remarks>
  /// This is a tool for GJK-like algorithms. Vertices can be added. The 
  /// <see cref="GjkSimplexSolver"/> removes unneeded vertices and computes points closest to the 
  /// origin. See <see cref="Gjk"/> for a usage example.
  /// </remarks>
  internal sealed class GjkSimplexSolver : IRecyclable
  {
    // See 
    // - [Bullet]
    // - Ericson: "Real-Time Collision Detection", p. 403 
    // - Bergen: "Collision Detection in Interactive 3D Environments"
    //
    // This is an alternative to Johnson's distance algorithm as described in 
    // Bergen: "Collision Detection in Interactive 3D Environments", [SOLID DT_GJK.cpp].
    // Closest Point = The point of the simplex closest to the origin and the corresponding closest 
    // points pair on the shapes.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// A structure that contains info used in an Update() step.
    /// </summary>
    private struct ClosestPointInfo
    {
      // Barycentric coordinates
      public float U, V, W, X;

      // Closest point in simplex.
      public Vector3F ClosestPoint;

      public void SetBarycentricCoordinates(float u, float v, float w, float x)
      {
        U = u; V = v; W = w; X = x;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static readonly ResourcePool<GjkSimplexSolver> Pool =
      new ResourcePool<GjkSimplexSolver>(
        () => new GjkSimplexSolver(),
        null,
        null);


    // The simplex is made up of vertices w, which are differences of points on A and
    // on B. Only the first NumberOfVertices in each array are valid.
    // The simplex points w_i.
    private readonly Vector3F[] _w = new Vector3F[4];
    // The points in A which correspond to the simplex points.
    private readonly Vector3F[] _pointsA = new Vector3F[4];
    // The points in B which correspond to the simplex points.
    private readonly Vector3F[] _pointsB = new Vector3F[4];

    // True if a vertex was added since the last update.
    private bool _needsUpdate = true;

    // The last added w.
    private Vector3F _lastW = new Vector3F(float.NaN);

    // Data for backup in error cases.
    private Vector3F _lastClosestPointOnA, _lastClosestPointOnB, _lastClosestPoint;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the point in the simplex which is closest to the origin.
    /// </summary>
    /// <value>The point closest to the origin.</value>
    /// <remarks>
    /// <para>
    /// <see cref="Update"/> must be called before this property is valid. This point is equal to
    /// the separating axis vector v or the difference of the currently known closest points of A
    /// and B (see <see cref="ClosestPointOnA"/> and <see cref="ClosestPointOnB"/>).
    /// </para>
    /// <para>
    /// If the simplex is full (containing the origin) or if the simplex is degenerated, this 
    /// property contains the closest point info of the last step and not for the new simplex.
    /// </para>
    /// </remarks>
    public Vector3F ClosestPoint
    {
      get
      {
        Debug.Assert(NumberOfVertices > 0, "Simplex must not be empty.");
        Debug.Assert(!_needsUpdate, "GjkSimplexSolver.Update() must be called before this property can be used.");
        return _closestPoint;
      }
      private set
      {
        _lastClosestPoint = _closestPoint;
        _closestPoint = value;
      }
    }
    private Vector3F _closestPoint;


    /// <summary>
    /// Gets the point on A which is closest to B (in world space coordinates).
    /// </summary>
    /// <value>The closest points on A.</value>
    /// <remarks>
    /// <para>
    /// <see cref="Update"/> must be called before this property is valid.
    /// </para>
    /// <para>
    /// If the simplex is full (containing the origin) or if the simplex is degenerated, this 
    /// property contains the closest point info of the last step and not for the new simplex.
    /// </para>
    /// </remarks>
    public Vector3F ClosestPointOnA
    {
      get
      {
        Debug.Assert(NumberOfVertices > 0, "Simplex must not be empty.");
        Debug.Assert(!_needsUpdate, "GjkSimplexSolver.Update() must be called before this property can be used.");
        return _closestPointOnA;
      }
      private set
      {
        _lastClosestPointOnA = _closestPointOnA;
        _closestPointOnA = value;
      }
    }
    private Vector3F _closestPointOnA;


    /// <summary>
    /// Gets the point on B which is closest to A (in world space coordinates).
    /// </summary>
    /// <value>The closest points on B.</value>
    /// <remarks>
    /// <para>
    /// <see cref="Update"/> must be called before this property is valid.
    /// </para>
    /// <para>
    /// If the simplex is full (containing the origin) or if the simplex is degenerated, this 
    /// property contains the closest point info of the last step and not for the new simplex.
    /// </para>
    /// </remarks>
    public Vector3F ClosestPointOnB
    {
      get
      {
        Debug.Assert(NumberOfVertices > 0, "Simplex must not be empty.");
        Debug.Assert(!_needsUpdate, "GjkSimplexSolver.Update() must be called before this property can be used.");
        return _closestPointOnB;
      }
      private set
      {
        _lastClosestPointOnB = _closestPointOnB;
        _closestPointOnB = value;
      }
    }
    private Vector3F _closestPointOnB;


    /// <summary>
    /// Gets a value indicating whether this the simplex is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the simplex is empty; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The simplex is empty if <see cref="NumberOfVertices"/> is 0.
    /// </remarks>
    public bool IsEmpty
    {
      get { return NumberOfVertices == 0; }
    }


    /// <summary>
    /// Gets a value indicating whether the simplex is full.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the simplex is full; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// The simplex is full if <see cref="NumberOfVertices"/> is 4. If the simplex is full and 
    /// <see cref="Update"/> does not reduce the <see cref="NumberOfVertices"/> then the origin must
    /// be inside the simplex.
    /// </remarks>
    public bool IsFull
    {
      get { return NumberOfVertices == 4; }
    }


    /// <summary>
    /// Gets or sets a value indicating whether the cached closest-point info is valid for the new
    /// simplex.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the closest-point info is valid; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// If this value is <see langword="false"/>, the closest point properties contain the closest
    /// point info of the last valid simplex.
    /// </remarks>
    public bool IsValid { get; private set; }


    /// <summary>
    /// Gets the maximum squared distance to the origin of all simplex vertices.
    /// </summary>
    /// <value>The maximum squared vertex distance.</value>
    public float MaxVertexDistanceSquared
    {
      get
      {
        float maxDistance = 0;
        for (int i = 0; i < NumberOfVertices; i++)
        {
          float distance = _w[i].LengthSquared;
          if (distance > maxDistance)
            maxDistance = distance;
        }

        return maxDistance;
      }
    }


    /// <summary>
    /// Gets the number of vertices in the simplex.
    /// </summary>
    /// <value>The number of vertices in the simplex.</value>
    public int NumberOfVertices { get; private set; }


    #region ----- EPA properties (not used anymore) -----

    // Following properties where necessary for the EPA. EPA has been removed.
    // If these methods are required, modify them so that they do not allocate 
    // unnecessary heap memory.

    //// Create list with points on A.
    //public List<Vector3F> PointsOnA
    //{
    //  get
    //  {
    //    List<Vector3F> list = new List<Vector3F>();
    //    for (int i = 0; i < NumberOfVertices; i++)
    //      list.Add(_pointsA[i]);
    //    return list;
    //  }
    //}

    //// Create list with points on B.
    //public List<Vector3F> PointsOnB
    //{
    //  get
    //  {
    //    List<Vector3F> list = new List<Vector3F>();
    //    for (int i = 0; i < NumberOfVertices; i++)
    //      list.Add(_pointsB[i]);
    //    return list;
    //  }
    //}


    //// Create list with simplex points.
    //public List<Vector3F> SimplexPoints
    //{
    //  get
    //  {
    //    List<Vector3F> list = new List<Vector3F>();
    //    for (int i = 0; i < NumberOfVertices; i++)
    //      list.Add(_w[i]);
    //    return list;
    //  }
    //}
    #endregion
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="GjkSimplexSolver"/> class.
    /// </summary>
    private GjkSimplexSolver()
    {
    }


    /// <summary>
    /// Creates an instance of the <see cref="GjkSimplexSolver"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="GjkSimplexSolver"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    public static GjkSimplexSolver Create()
    {
      var solver = Pool.Obtain();
      solver.Clear();
      return solver;
    }


    /// <summary>
    /// Recycles this instance of the <see cref="GjkSimplexSolver"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Pool.Recycle(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Adds a new simplex point in the Minkowski difference A-B.
    /// </summary>
    /// <param name="w">The point in the Minkowski difference A-B.</param>
    /// <param name="pointA">The point in object A.</param>
    /// <param name="pointB">The point in object B.</param>
    /// <remarks>
    /// <paramref name="w"/> = <paramref name="pointA"/> - <paramref name="pointB"/>.
    /// </remarks>
    public void Add(Vector3F w, Vector3F pointA, Vector3F pointB)
    {
      Debug.Assert(Vector3F.AreNumericallyEqual(pointA - pointB, w), "w is unequal to pointA - pointB.");

      _w[NumberOfVertices] = w;
      _lastW = w;

      _pointsA[NumberOfVertices] = pointA;
      _pointsB[NumberOfVertices] = pointB;
      NumberOfVertices++;

      _needsUpdate = true;
    }


    /// <summary>
    /// Backup closest point info to previous state.
    /// </summary>
    public void Backup()
    {
      _closestPoint = _lastClosestPoint;
      _closestPointOnA = _lastClosestPointOnA;
      _closestPointOnB = _lastClosestPointOnB;
    }


    /// <summary> 
    /// Resets this instance. All cached data is deleted. 
    /// </summary>
    public void Clear()
    {
      for (int i = 0; i < 4; i++)
      {
        _w[i] = new Vector3F(float.NaN);
        _pointsA[i] = new Vector3F(float.NaN);
        _pointsB[i] = new Vector3F(float.NaN);
      }

      ClosestPointOnA = new Vector3F(float.NaN);
      ClosestPointOnB = new Vector3F(float.NaN);
      ClosestPoint = new Vector3F(float.NaN);
      _needsUpdate = true;
      _lastW = new Vector3F(float.NaN);
      IsValid = false;
      NumberOfVertices = 0;
    }


    /// <summary>
    /// Determines whether the simplex contains the given point.
    /// </summary>
    /// <param name="w">The point which is tested.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="w"/> was already added to the simplex; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Only the vertices of the simplex are tested and the last added vertex <paramref name="w"/> 
    /// is checked.
    /// </remarks>
    public bool Contains(Vector3F w)
    {
      for (int i = 0; i < NumberOfVertices; i++)
        if (Vector3F.AreNumericallyEqual(_w[i], w))
          return true;

      // w could be already removed.
      if (Vector3F.AreNumericallyEqual(_lastW, w))
        return true;

      return false;
    }


    /// <summary>
    /// Removes unused entries from the arrays.
    /// </summary>
    private void Reduce(ref ClosestPointInfo info)
    {
      // We can remove an simplex vertices which do not contribute to the 
      // closest point. (If their barycentric coordinate/weight is 0, they do
      // not contribute.)
      if (NumberOfVertices >= 4 && info.X <= 0)
        RemoveAt(3);
      if (NumberOfVertices >= 3 && info.W <= 0)
        RemoveAt(2);
      if (NumberOfVertices >= 2 && info.V <= 0)
        RemoveAt(1);
      if (NumberOfVertices >= 1 && info.U <= 0)
        RemoveAt(0);
    }


    /// <summary>
    /// Removes the entry at the given index.
    /// </summary>
    /// <param name="index">The index.</param>
    private void RemoveAt(int index)
    {
      Debug.Assert(0 <= index && index < NumberOfVertices, "Wrong index.");

      NumberOfVertices--;

      // Copy last vertex into the now empty slot.
      _w[index] = _w[NumberOfVertices];
      _pointsA[index] = _pointsA[NumberOfVertices];
      _pointsB[index] = _pointsB[NumberOfVertices];
    }


    /// <summary>
    /// Updates the simplex data.
    /// </summary>
    /// <remarks>
    /// This method tries to remove unneeded vertices from the simplex. The closest point data
    /// is updated. 
    /// </remarks>
    public void Update()
    {
      if (_needsUpdate == false)
        return;

      _needsUpdate = false;

      IsValid = false;

      // Call a method for each simplex type.
      // The methods compute the closest points, remove vertices if possible and set 
      // IsValid.
      switch (NumberOfVertices)
      {
        case 0: break; // No points in the simplex --> then we don't have any valid information.
        case 1: UpdatePointSimplex(); break;
        case 2: UpdateLineSimplex(); break;
        case 3: UpdateTriangleSimplex(); break;
        default:
          Debug.Assert(NumberOfVertices == 4, "GJK simplex must not have more than 4 vertices!");
          UpdateTetrahedronSimplex();
          break;
      }

      // cached valid closest must be in _closestPoint!
    }


    private void UpdatePointSimplex()
    {
      // The simplex consists of 1 point. --> This point is the closest point.
      ClosestPointOnA = _pointsA[0];
      ClosestPointOnB = _pointsB[0];
      ClosestPoint = _w[0];
      Debug.Assert(ClosestPoint == ClosestPointOnA - ClosestPointOnB);
      IsValid = true;
    }


    private void UpdateLineSimplex()
    {
      // The simplex is a line segment.
      // Find closest point of line segment W[0] to W[1] to origin.
      Vector3F segmentVector = _w[1] - _w[0];
      Vector3F segmentStartToOrigin = Vector3F.Zero - _w[0];
      float param = Vector3F.Dot(segmentVector, segmentStartToOrigin);

      // Clamp parameter to [0,1].
      if (param <= 0)
      {
        // Clamp to 0.
        param = 0;
      }
      else
      {
        // Line parameter is > 0.
        float lengthSquared = segmentVector.LengthSquared;
        if (param > lengthSquared)
        {
          // Clamp to 1.
          param = 1;
        }
        else
        {
          // Closest point is in the segment.
          Debug.Assert(param > 0 && param <= lengthSquared);

          // Normalize parameter;
          param /= lengthSquared;
        }
      }

      ClosestPointOnA = _pointsA[0] + param * (_pointsA[1] - _pointsA[0]);
      ClosestPointOnB = _pointsB[0] + param * (_pointsB[1] - _pointsB[0]);
      ClosestPoint = ClosestPointOnA - ClosestPointOnB;

      var info = new ClosestPointInfo();
      info.SetBarycentricCoordinates(1 - param, param, 0, 0);
      Reduce(ref info);

      IsValid = true;
      Debug.Assert(
        info.U >= 0
        && info.V >= 0
        && info.W >= 0
        && info.X >= 0);
    }


    private void UpdateTriangleSimplex()
    {
      // The simplex is a triangle. 

      var info = new ClosestPointInfo();

      GetClosestPointInTriangle(Vector3F.Zero, _w[0], _w[1], _w[2], ref info);

      ClosestPointOnA = _pointsA[0] * info.U
                          + _pointsA[1] * info.V
                          + _pointsA[2] * info.W;
      ClosestPointOnB = _pointsB[0] * info.U
                          + _pointsB[1] * info.V
                          + _pointsB[2] * info.W;

      ClosestPoint = info.ClosestPoint;

      Reduce(ref info);

      IsValid = true;

      // Following assert can fail, for example, for ray convex test if objects
      // are far away from the origin.
      //Debug.Assert(Vector3F.AreNumericallyEqual(ClosestPoint, ClosestPointOnA - ClosestPointOnB, Math.Max(1, ClosestPoint.Length) * 0.0001f), "ClosestPoint computed from barycentric coordinates must be equal to ClosestPointOnA - ClosestPointOnB.");
      Debug.Assert(Numeric.IsGreaterOrEqual(info.U, 0));
      Debug.Assert(Numeric.IsGreaterOrEqual(info.V, 0));
      Debug.Assert(Numeric.IsGreaterOrEqual(info.W, 0));
      Debug.Assert(Numeric.AreEqual(info.X, 0));
    }


    private void UpdateTetrahedronSimplex()
    {
      // Simplex is a tetrahedron.

      ClosestPointInfo info = new ClosestPointInfo();

      // Find closest point of tetrahedron to origin.
      bool? containsOrigin = GetClosestPoints(Vector3F.Zero, _w[0], _w[1], _w[2], _w[3], ref info);

      // Origin is not contained.
      ClosestPointOnA = _pointsA[0] * info.U
                         + _pointsA[1] * info.V
                         + _pointsA[2] * info.W
                         + _pointsA[3] * info.X;
      ClosestPointOnB = _pointsB[0] * info.U
                        + _pointsB[1] * info.V
                        + _pointsB[2] * info.W
                        + _pointsB[3] * info.X;

      ClosestPoint = info.ClosestPoint;

      if (containsOrigin.HasValue)
      {
        IsValid = true;

        if (!containsOrigin.Value)
          Reduce(ref info);
      }
      else
      {
        // Degenerated.
        IsValid = false;
      }

      // Following assert can fail, for example, for ray convex test if objects
      // are far away from the origin.
      //Debug.Assert(Vector3F.AreNumericallyEqual(ClosestPoint, ClosestPointOnA - ClosestPointOnB, Math.Max(1, ClosestPoint.Length) * 0.0001f), "ClosestPoint computed from barycentric coordinates must be equal to ClosestPointOnA - ClosestPointOnB.");
      Debug.Assert(Numeric.IsGreaterOrEqual(info.U, 0));
      Debug.Assert(Numeric.IsGreaterOrEqual(info.V, 0));
      Debug.Assert(Numeric.IsGreaterOrEqual(info.W, 0));
      Debug.Assert(Numeric.IsGreaterOrEqual(info.X, 0));
    }


    /// <summary>
    /// Computes the point in the triangle which is closest to <paramref name="point"/>
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="vertex0">The first vertex of the triangle.</param>
    /// <param name="vertex1">The second vertex of the triangle.</param>
    /// <param name="vertex2">The third vertex of the triangle.</param>
    /// <param name="closestPointInfo">The closest-point information that will be set.</param>
    private static void GetClosestPointInTriangle(Vector3F point, Vector3F vertex0, Vector3F vertex1, Vector3F vertex2, ref ClosestPointInfo closestPointInfo)
    {
      float u, v, w;
      GeometryHelper.GetClosestPoint(new Triangle(vertex0, vertex1, vertex2), point, out u, out v, out w);
      closestPointInfo.SetBarycentricCoordinates(u, v, w, 0);
      closestPointInfo.ClosestPoint = u * vertex0 + v * vertex1 + w * vertex2;
    }


    // See Ericson: "Real-Time Collision Detection", p. 143
    // return true if point is inside the tetrahedron, false if point is outside. 
    // null if simplex is degenerated
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private static bool? GetClosestPoints(Vector3F point, Vector3F tetrahedronVertexA, Vector3F tetrahedronVertexB, Vector3F tetrahedronVertexC, Vector3F tetrahedronVertexD, ref ClosestPointInfo info)
    {
      // Assume that the point is outside the tetrahedron. (This is the most likely case.)
      bool? containsPoint = false;

      // Check all tetrahedron faces to see if point is outside or inside the according plane.
      bool? isPointOutsideABC = GeometryHelper.ArePointsOnOppositeSides(point, tetrahedronVertexD, tetrahedronVertexA, tetrahedronVertexB, tetrahedronVertexC);
      bool? isPointOutsideACD = GeometryHelper.ArePointsOnOppositeSides(point, tetrahedronVertexB, tetrahedronVertexA, tetrahedronVertexC, tetrahedronVertexD);
      bool? isPointOutsideADB = GeometryHelper.ArePointsOnOppositeSides(point, tetrahedronVertexC, tetrahedronVertexA, tetrahedronVertexD, tetrahedronVertexB);
      bool? isPointOutsideBDC = GeometryHelper.ArePointsOnOppositeSides(point, tetrahedronVertexA, tetrahedronVertexB, tetrahedronVertexD, tetrahedronVertexC);

      // The checks return null to indicate a degenerate/undecidable case. The origin 
      // touches a plane of a tetrahedron face or face of the tetrahedron are not valid triangles.
      if (isPointOutsideABC == null || isPointOutsideACD == null || isPointOutsideADB == null || isPointOutsideBDC == null)
      {
        // Degenerate case.
        containsPoint = null;

        // Set all "undecided" face flags to true to test them for closest point.
        if (isPointOutsideABC == null)
          isPointOutsideABC = true;
        if (isPointOutsideACD == null)
          isPointOutsideACD = true;
        if (isPointOutsideADB == null)
          isPointOutsideADB = true;
        if (isPointOutsideBDC == null)
          isPointOutsideBDC = true;
      }
      else if (!(isPointOutsideABC.Value || isPointOutsideACD.Value || isPointOutsideADB.Value || isPointOutsideBDC.Value))
      {
        // The point is inside.
        containsPoint = true;

        // The GJK cannot compute the correct penetrating contact. We need EPA or MPR for this.
        // But we can use the closest point of the tetrahedron as an approximate result for
        // shallow contacts. --> Set all face flags to true to test them below.
        isPointOutsideABC = true;
        isPointOutsideACD = true;
        isPointOutsideADB = true;
        isPointOutsideBDC = true;

        // Warning: It can happen that ABCD are all in a plane (e.g. when line segment vs line segment)
        // is tested but the origin is outside! ArePointsOnOppositeSides does not detect all 
        // degenerate cases. 
      }

      float bestDistanceSquared = float.MaxValue;
      var tempInfo = new ClosestPointInfo();

      // ----- ABC
      // If points is outside of face ABC then compute closest point on ABC.
      if (isPointOutsideABC.Value)
      {
        GetClosestPointInTriangle(point, tetrahedronVertexA, tetrahedronVertexB, tetrahedronVertexC, ref tempInfo);
        Vector3F closestPoint = tempInfo.ClosestPoint;

        // No comparison of actual distance with best distance required because this is the first test.
        bestDistanceSquared = (point - closestPoint).LengthSquared;
        info.ClosestPoint = closestPoint;
        info.SetBarycentricCoordinates(tempInfo.U, tempInfo.V, tempInfo.W, 0);
      }

      // ----- ACD
      // Repeat test for ACD.
      if (isPointOutsideACD.Value)
      {
        GetClosestPointInTriangle(point, tetrahedronVertexA, tetrahedronVertexC, tetrahedronVertexD, ref tempInfo);
        Vector3F closestPoint = tempInfo.ClosestPoint;
        float distance = (point - closestPoint).LengthSquared;
        if (distance < bestDistanceSquared)
        {
          bestDistanceSquared = distance;
          info.ClosestPoint = closestPoint;
          info.SetBarycentricCoordinates(tempInfo.U, 0, tempInfo.V, tempInfo.W);
        }
      }

      // ----- ADB
      // Repeat test for ADB.
      if (isPointOutsideADB.Value)
      {
        GetClosestPointInTriangle(point, tetrahedronVertexA, tetrahedronVertexD, tetrahedronVertexB, ref tempInfo);
        Vector3F closestPoint = tempInfo.ClosestPoint;
        float distance = (point - closestPoint).LengthSquared;
        if (distance < bestDistanceSquared)
        {
          bestDistanceSquared = distance;
          info.ClosestPoint = closestPoint;
          info.SetBarycentricCoordinates(tempInfo.U, tempInfo.W, 0, tempInfo.V);
        }
      }

      // ----- BDC
      // Repeat test for BDC.
      if (isPointOutsideBDC.Value)
      {
        GetClosestPointInTriangle(point, tetrahedronVertexB, tetrahedronVertexD, tetrahedronVertexC, ref tempInfo);
        Vector3F closestPoint = tempInfo.ClosestPoint;
        float distance = (point - closestPoint).LengthSquared;
        if (distance < bestDistanceSquared)
        {
          //bestDistanceSquared = distance; // Not needed anymore.
          info.ClosestPoint = closestPoint;
          info.SetBarycentricCoordinates(0, tempInfo.U, tempInfo.W, tempInfo.V);
        }
      }

      // If we do not contain the point, then the simplex must not be full.
      Debug.Assert(containsPoint == null
                   || containsPoint == true
                   || info.U <= 0
                   || info.V <= 0
                   || info.W <= 0
                   || info.X <= 0);

      return containsPoint;
    }
    #endregion
  }
}
