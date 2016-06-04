// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Meshes
{
  /// <summary>
  /// The type of the convex hull.
  /// </summary>
  internal enum ConvexHullType
  {
    /// <summary>
    /// The convex hull is empty.
    /// </summary>
    Empty,
    /// <summary>
    /// The convex hull is a single point.
    /// </summary>
    Point,
    /// <summary>
    /// The convex hull is a line segment.
    /// </summary>
    Linear,
    /// <summary>
    /// The convex hull is a polygon. The convex hull consists of two faces (front and back).
    /// </summary>
    Planar,
    /// <summary>
    /// The convex hull is a polyhedron.
    /// </summary>
    Spatial,
  }


  /// <summary>
  /// Creates a convex hull for a given point list.
  /// </summary>
  internal sealed class ConvexHullBuilder
  {
    // Notes:
    // This class uses incremental construction as described in Geometric Tools for Computer 
    // Graphics.
    //
    // Numerical problems:
    // This algorithm performs "IsInFront" tests. Collinear features can create numerical problems.
    // This is especially evident with 32-bit float. It can also occur with 64-bit double values.
    // (According to "Game Programming Gems 4: 5.5. Carving Static Shadows into Geometry" similar
    // problems occur regularly with 64-bit floating-point values.)
    // The algorithm fails for very flat long things, like a 1 x 1 x 0.001 box. Such an object can 
    // be used as a test case in the future. Currently we accept that algorithm fails for such
    // extreme cases.
    //
    // Possible improvements:
    // - Use 64 bit.
    // - Transform input data into a unit cube.
    // - Use AabbTree to speed up support mapping of point clouds.
    // - For collinearity test: If result is not accurate enough, repeat computation with 
    //   custom high precision data types (e.g. Googol).
    // - Use topology tests and concavity tests to make sure that the faces visible from a point
    //   form a valid region. Or use an unfolding step to remove any concave folds.
    // - Optimize cache usage.
    // - Make vertex welding optional.
    // - More optimizations.
    //
    // See also KB and http://code.google.com/p/juliohull/.
    //
    // See also public comments in GeometryHelper.CreateConvexHull().
    // 
    // Creation of convex from planes
    // Torque engine has a function that computes this. They cut two planes to create an
    // intersection line. Then they find the edge points on this line that are within all
    // planes. Thus, they collect edges. Edges can then be connected when shared vertices
    // are found.

    // The faces of the convex hull. This list is used within the Grow methods.
    // Faces which are on the final hull will get Tag -1 in Create().
    private List<DcelFace> _faces;

    // A list of tagged faces, where the tag must be reset.
    // (A temp list used in GrowSpatialConvex()).
    private List<DcelEdge> _taggedEdges;

    // True if the input points are all in a plane.
    private bool _isPlanar;


    // The type of the current convex hull.
    public ConvexHullType Type { get { return _type; } }
    private ConvexHullType _type;


    // The convex hull mesh that is built.
    // If you want to continue to use the ConvexHullBuilder, then do not modify this mesh!
    public DcelMesh Mesh { get { return _mesh; } }
    private DcelMesh _mesh;


    public ConvexHullBuilder()
    {
      Reset();
    }


    public ConvexHullBuilder Clone()
    {
      var clone = new ConvexHullBuilder
      {
        _isPlanar = _isPlanar,
        _type = _type,
        _mesh = new DcelMesh(_mesh),
      };

      return clone;
    }


    public void Reset()
    {
      // Reset fields.
      _faces = new List<DcelFace>();
      _taggedEdges = new List<DcelEdge>();
      _isPlanar = false;
      _type = ConvexHullType.Empty;
      _mesh = new DcelMesh();
    }


    /// <summary>
    /// Creates a convex hull from the given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="vertexLimit">The vertex limit.</param>
    /// <param name="skinWidth">Width of the skin.</param>
    /// <remarks>
    /// This method assumes that all duplicate vertices have been removed from 
    /// <paramref name="points"/>.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public void Grow(IEnumerable<Vector3F> points, int vertexLimit, float skinWidth)
    {
      Debug.Assert(_taggedEdges.Count == 0, "ConvexHullBuilder is in an invalid state.");
      Debug.Assert(_faces.Count == 0, "ConvexHullBuilder is in an invalid state.");

      var pointList = points.ToList();
      if (pointList.Count == 0)
        return;

      // If the current convex is not spatial, we restart. This allows us to start
      // the convex hull with a more robust initial point selection.
      if (_type != ConvexHullType.Spatial)
      {
        pointList.AddRange(_mesh.Vertices.Select(v => v.Position));
        Reset();
      }

      Matrix44D toUnitCube;
      Matrix44D fromUnitCube;
      #region ----- Convert to Unit Cube -----
      {
        // Get AABB of existing and new points.
        var aabb = new Aabb(pointList[0], pointList[0]);
        foreach (var v in _mesh.Vertices)
          aabb.Grow(v.Position);
        foreach (var p in pointList)
          aabb.Grow(p);
        var extent = aabb.Extent;

        if (!Numeric.IsFinite(extent.X) || !Numeric.IsFinite(extent.Y) || !Numeric.IsFinite(extent.Z))
          throw new GeometryException("Cannot build convex hull because the input positions are invalid (e.g. NaN or infinity).");

        // Avoid division by 0 for planar point clouds.
        if (Numeric.IsZero(extent.X))
          extent.X = 1;
        if (Numeric.IsZero(extent.Y))
          extent.Y = 1;
        if (Numeric.IsZero(extent.Z))
          extent.Z = 1;

        toUnitCube = Matrix44D.CreateScale(2 / extent.X, 2 / extent.Y, 2 / extent.Z) * Matrix44D.CreateTranslation(-aabb.Center);
        fromUnitCube = Matrix44D.CreateTranslation(aabb.Center) * Matrix44D.CreateScale(extent / 2);

        foreach (var v in _mesh.Vertices)
          v.Position = (Vector3F)toUnitCube.TransformPosition(v.Position);
        for (int i = 0; i < pointList.Count; i++)
          pointList[i] = (Vector3F)toUnitCube.TransformPosition(pointList[i]);
      }
      #endregion

      // Remove duplicate vertices.
      GeometryHelper.MergeDuplicatePositions(pointList, Numeric.EpsilonF);

      // Find initial tetrahedron and check if points are in a plane.
      if (_type != ConvexHullType.Spatial)
        _isPlanar = SortPoints(pointList);
      else
        _isPlanar = false;

      // Create convex hull using Incremental Construction. 
      var numberOfPoints = pointList.Count;
      for (int i = 0; i < numberOfPoints; i++)
      {
        bool prune = false;

        // Find a face with a good support point.
        // The face is stored in this variable. The point is sorted to the front of the list.
        DcelFace supportPointFace = null; 
        if (_type == ConvexHullType.Spatial)
        {
          foreach (var face in _faces)
          {
            // Skip faces which are already on the hull.
            if (face.Tag == -1)
              continue;

            var normal = face.Normal;
            if (!normal.TryNormalize())       // Ignore degenerate faces.
              continue;

            // Plane distance from origin.
            var d = Vector3F.Dot(normal, face.Boundary.Origin.Position);

            // Find support point for this face under remaining points.
            var maxDistance = d;
            var maxIndex = i;
            for (int j = i; j < numberOfPoints; j++)
            {
              float distance = Vector3F.Dot(normal, pointList[j]);
              if (distance > maxDistance)
              {
                maxDistance = distance;
                maxIndex = j;
              }
            }

            if (maxDistance > d)
            {
              supportPointFace = face;
              var supportPoint = pointList[maxIndex];

              // Extrude in support direction to make it numerically more robust.
              // This step proved to be very important for the numerical stability!
              supportPoint += Numeric.EpsilonF * normal;

              // Only prune if we make a significant step (5% of the unit cube size).
              prune = (maxDistance - d) > 0.1;

              // Swap support point to front.
              pointList[maxIndex] = pointList[i];
              pointList[i] = supportPoint;

              break;
            }
            else
            {
              // No support point found. Face must be on the convex hull.
              face.Tag = -1;
            }
          }
        }

        GrowConvex(pointList[i], supportPointFace);

        if (i == 3 && !_isPlanar)
          prune = true;

        // Prune vertices.
        #region ----- Prune Vertices -----
        if (prune)
        {
          for (int j = i + 1; j < numberOfPoints; j++)
          {
            bool isVisible = false;
            foreach (var face in _faces)
            {
              if (face.Tag >= 0                  // No need to test hull faces (tag == -1).
                  && DcelMesh.GetCollinearity(pointList[j], face) != Collinearity.NotCollinearBehind)
              {
                // Face is "probably" visible. Point is outside and must be kept.
                // (Pruning uses a conservative test. Exact test is made in the Grow methods.) 
                isVisible = true;
                break;
              }
            }
            if (!isVisible)
            {
              // Sort point to start of list and increase outer loop index.
              var point = pointList[j];
              pointList[j] = pointList[i + 1];
              pointList[i + 1] = point;      // Not strictly necessary but kept for asserts.
              i++;
            }
          }
        }
        #endregion
      }

      _mesh.ResetTags();
      _faces.Clear();

      // TODO: Remove degenerate triangles. (Needs more testing.)
      //RemoveDegenerateTriangles();

//#if DEBUG
//      foreach (var point in pointList)
//        Debug.Assert(_mesh.Contains(point, Numeric.EpsilonF * 10), "A point is outside the convex hull after GrowConvex().");
//#endif

      if (_mesh.Faces.Count > 2 && (!Numeric.IsZero(skinWidth) || _mesh.Vertices.Count >= vertexLimit))
      {
        // ----- Apply Vertex Limit and/or Skin Width 

        // Skin width must be converted to unit cube.
        var skinWidthScale = (Vector3F)toUnitCube.TransformDirection(new Vector3D(skinWidth));

        // The assert after the cutting may fail for very low skin widths. But not when
        // the debugger is attached :-(. 
        //skinWidthScale = Vector3F.Max(skinWidthScale, new Vector3F(100 * Numeric.EpsilonF));

        _mesh.ModifyConvex(vertexLimit, skinWidthScale);

#if DEBUG
        // TODO: This assert may fail - but not when the debugger is attached :-(
        //foreach (var point in pointList)
        //  Debug.Assert(_mesh.Contains(point, 0.01f), "A point is outside the convex hull after plane cutting.");
#endif
      }

      // Convert back from unit cube.
      foreach (var v in _mesh.Vertices)
        v.Position = (Vector3F)fromUnitCube.TransformPosition(v.Position);
    }


    // Sorts points and check if points are all in a plane.
    // We find 4 points that create a large tetrahedron and we move these points to the
    // start of the list.
    private static bool SortPoints(List<Vector3F> pointList)
    {
      if (pointList.Count < 4)
        return false;

      bool isPlanar = false;

      // First we find 4 points that create a large tetrahedron.

      // Find extreme points in x direction of AABB.
      Vector3F minimum = pointList[0];
      int minimumIndex = 0;
      for (int i = 1; i < pointList.Count; i++)
      {
        Vector3F point = pointList[i];
        if (point.X < minimum.X)
        {
          minimum = point;
          minimumIndex = i;
        }
      }

      // Swap points to start of list.
      pointList[minimumIndex] = pointList[0];
      pointList[0] = minimum;

      // Now the maximum in x direction.
      Vector3F maximum = pointList[0];
      int maximumIndex = 0;
      for (int i = 1; i < pointList.Count; i++)
      {
        Vector3F point = pointList[i];
        if (point.X > maximum.X)
        {
          maximum = point;
          maximumIndex = i;
        }
      }

      pointList[maximumIndex] = pointList[1];
      pointList[1] = maximum;

      // Now we have to find a point normal to this initial line segment. We 
      // search for the point which is farthest away from the line segment.
      var lineDirection = (maximum - minimum);
      if (!lineDirection.TryNormalize())
        isPlanar = true;

      Line line = new Line(minimum, lineDirection);
      Vector3F third = pointList[0];
      int thirdIndex = 0;
      float maxDistanceSquared = GetDistanceFromLineSquared(ref line, ref third);
      for (int i = 2; i < pointList.Count; i++)
      {
        Vector3F point = pointList[i];
        float distance = GetDistanceFromLineSquared(ref line, ref point);
        if (distance > maxDistanceSquared)
        {
          maxDistanceSquared = distance;
          third = point;
          thirdIndex = i;
        }
      }

      // Swap third point to start of list.
      if (thirdIndex > 2)
      {
        pointList[thirdIndex] = pointList[2];
        pointList[2] = third;
      }

      // Now we search for the 4th point with the maximal distance to this triangle.
      Triangle triangle = new Triangle(minimum, maximum, third);
      var triangleNormal = triangle.Normal;
      Vector3F fourth = pointList[0];
      int fourthIndex = 0;
      float maxDistance = Math.Abs(Vector3F.Dot(triangleNormal, fourth - triangle.Vertex0));
      // This computes a value proportional to the distance from the face.
      for (int i = 3; i < pointList.Count; i++)
      {
        Vector3F point = pointList[i];
        float distance = Math.Abs(Vector3F.Dot(triangleNormal, point - triangle.Vertex0));
        if (distance > maxDistance)
        {
          maxDistance = distance;
          fourth = point;
          fourthIndex = i;
        }
      }

      // Swap fourth point to start of list.
      if (fourthIndex > 3)
      {
        pointList[fourthIndex] = pointList[3];
        pointList[3] = fourth;
      }

      // Remember that this is a planar object (See GrowPlanarConvex()).
      if (maxDistance < Numeric.EpsilonF)
        isPlanar = true;

      return isPlanar;
    }


    /// <summary>
    /// Grows the mesh so that it includes the given point.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="face">
    /// A face which is visible from the given point. Can be <see langword="null"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// This method assumes that this DCEL mesh represents a convex, closed mesh. The convex mesh
    /// is grown so that it includes the given <paramref name="point"/>. The final mesh will
    /// contain the original mesh.
    /// </para>
    /// </remarks>
    private void GrowConvex(Vector3F point, DcelFace face)
    {
      // Depending on the current convex hull type we have to use different growing methods.
      switch (_type)
      {
        case ConvexHullType.Empty:
          GrowEmptyConvex(point);
          break;
        case ConvexHullType.Point:
          GrowPointConvex(point);
          break;
        case ConvexHullType.Linear:
          GrowLinearConvex(point);
          break;
        case ConvexHullType.Planar:
          GrowPlanarConvex(point);
          break;
        default:
          Debug.Assert(_type == ConvexHullType.Spatial);
          //bool merged =
          GrowSpatialConvex(point, face);

          // Degenerate face removal is currently disabled because it is too slow for large 
          // input sets.
          //// Check for degenerate faces
          //if (merged)
          //{
          //  bool degenerate = false;
          //  foreach (var face in _faces)
          //  {
          //    if (face.Normal.LengthSquared < Numeric.EpsilonF * Numeric.EpsilonF)
          //    {
          //      degenerate = true;
          //      break;
          //    }
          //  }
          //  if (degenerate)
          //  {
          //    // Remove degenerate faces.
          //    // Since there are no duplicate vertices. This method will only rearrange faces
          //    // it will not add faces.
          //    RemoveDegenerateTriangles();
          //  }
          //}
          break;
      }

#if DEBUG
      // These tests might be too slow for some unit tests 
      // (e.g. DigitalRune.Physics.Tests.MassTest.ScaledConvexMass()).
      //Debug.Assert(_mesh.IsValid(), "DCEL mesh is not valid.");
      //Debug.Assert(_mesh.IsConvex(), "DCEL mesh is not convex.");
      //Debug.Assert(_mesh.Contains(point, Numeric.EpsilonF * 10), "Point is outside convex hull.");
      //Debug.Assert(_type != ConvexHullType.Spatial || _mesh.IsTriangleMesh(), "Convex hull should be a triangle mesh but isn't.");
      //Debug.Assert(_mesh.Faces.All(f => f.Tag <= 0), "DCEL mesh tags are not reset.");
#endif
    }


    /// <summary>
    /// Merges a point to an empty convex hull.
    /// </summary>
    /// <param name="point">The point.</param>
    private void GrowEmptyConvex(Vector3F point)
    {
      Debug.Assert(_mesh != null);
      Debug.Assert(_mesh.Vertices.Count == 0, "DCEL mesh is not empty.");
      Debug.Assert(_type == ConvexHullType.Empty);

      // Create a single vertex.
      _mesh.Vertex = new DcelVertex(point, null);
      _mesh.Dirty = true;
      _type = ConvexHullType.Point;
    }


    /// <summary>
    /// Merges a point to a convex hull that is also a point.
    /// </summary>
    /// <param name="point">The point.</param>
    private void GrowPointConvex(Vector3F point)
    {
      Debug.Assert(_mesh != null);
      Debug.Assert(_mesh.Vertices.Count == 1, "DCEL mesh with 1 vertex was expected.");
      Debug.Assert(_type == ConvexHullType.Point);

      // Abort if the point is equal to the existing point in the mesh.
      if (Vector3F.AreNumericallyEqual(_mesh.Vertex.Position, point))
        return;

      DcelVertex newVertex = new DcelVertex(point, null);
      DcelEdge edge = new DcelEdge();
      DcelEdge twin = new DcelEdge();

      // Link everything.
      edge.Origin = _mesh.Vertex;
      twin.Origin = newVertex;
      edge.Twin = twin;
      twin.Twin = edge;
      _mesh.Vertex.Edge = edge;
      newVertex.Edge = twin;

      _mesh.Dirty = true;
      _type = ConvexHullType.Linear;
    }


    /// <summary>
    /// Merges a point to a convex hull that is a line segment.
    /// </summary>
    /// <param name="point">The point.</param>
    private void GrowLinearConvex(Vector3F point)
    {
      Debug.Assert(_mesh != null);
      Debug.Assert(_mesh.Vertices.Count == 2, "DCEL mesh with 2 vertices was expected.");
      Debug.Assert(_type == ConvexHullType.Linear);
      Debug.Assert(!Vector3F.AreNumericallyEqual(_mesh.Vertex.Position, point));
      Debug.Assert(!Vector3F.AreNumericallyEqual(_mesh.Vertices[1].Position, point));

      // Abort if the point is equal to an existing point.
      if (Vector3F.AreNumericallyEqual(point, _mesh.Vertex.Position)
          || Vector3F.AreNumericallyEqual(point, _mesh.Vertex.Edge.Twin.Origin.Position))
        return;

      var edge = _mesh.Vertex.Edge;
      var collinearity = DcelMesh.GetCollinearity(point, _mesh.Vertex.Edge);
      switch (collinearity)
      {
        case Collinearity.CollinearBefore:     // The point is the new start of the line segment.
          edge.Origin.Position = point;
          _mesh.Dirty = true;
          return;
        case Collinearity.CollinearAfter:      // The point is the new end of the line segment.
          edge.Twin.Origin.Position = point;
          _mesh.Dirty = true;
          return;
        case Collinearity.CollinearContained:  // The point is in the line segment.
          // Point is in convex hull.
          return;
      }

      Debug.Assert(collinearity == Collinearity.NotCollinear
                   || collinearity == Collinearity.NotCollinearBehind
                   || collinearity == Collinearity.NotCollinearInFront);

      // Not Collinear --> Make triangle.
      _type = ConvexHullType.Planar;

      // Create new components.
      DcelVertex v0 = _mesh.Vertex;
      DcelVertex v1 = edge.Twin.Origin;

      DcelVertex v2 = new DcelVertex(point, null);
      DcelEdge e01 = edge;
      DcelEdge e10 = e01.Twin;
      DcelEdge e12 = new DcelEdge();
      DcelEdge e21 = new DcelEdge();
      DcelEdge e02 = new DcelEdge();
      DcelEdge e20 = new DcelEdge();
      DcelFace f012 = new DcelFace();
      DcelFace f210 = new DcelFace();

      // Link with existing components.
      e01.Next = e12; e01.Previous = e20;
      e10.Next = e02; e10.Previous = e21;

      // Link new components.
      v2.Edge = e20;

      e12.Origin = v1;
      e21.Origin = v2;
      e20.Origin = v2;
      e02.Origin = v0;

      e12.Twin = e21;
      e21.Twin = e12;
      e20.Twin = e02;
      e02.Twin = e20;

      e12.Previous = e01;
      e21.Previous = e02;
      e20.Previous = e12;
      e02.Previous = e10;

      e12.Next = e20;
      e21.Next = e10;
      e20.Next = e01;
      e02.Next = e21;

      // Link face.
      f012.Boundary = e01;
      f210.Boundary = e21;
      e01.Face = e12.Face = e20.Face = f012;
      e21.Face = e10.Face = e02.Face = f210;

      // Update _faces.
      _faces.Add(f012);
      _faces.Add(f210);

      _mesh.Dirty = true;

      Debug.Assert(_mesh.Faces.Count == _faces.Count, "Internal error in ConvexHullBuilder.");
      Debug.Assert(_mesh.IsTriangleMesh(), "DCEL mesh should be a triangle mesh.");
      Debug.Assert(_mesh.IsTwoSidedPolygon(), "DCEL mesh should be a two-sided polygon.");
    }


    /// <summary>
    /// Merges a point to a convex hull that is a polygon.
    /// </summary>
    /// <param name="point">The point.</param>
    private void GrowPlanarConvex(Vector3F point)
    {
      Debug.Assert(_mesh != null);
      Debug.Assert(_mesh.Vertices.Count > 2, "DCEL mesh with 3 or more vertices expected.");
      Debug.Assert(_type == ConvexHullType.Planar, "DCEL mesh with 2 faces expected.");

      DcelFace frontFace = _mesh.Vertex.Edge.Face;
      DcelFace backFace = frontFace.Boundary.Twin.Face;

      // This check: if (!_isPlanar) 
      // is important! Because for planar input points we could not find an initial
      // tetrahedron. All points are in a plane. But sometimes GetCollinearity detects that
      // a point is not in the plane and this creates very degenerate spatial convex hulls.
      // For real spatial input points we have no problem because we have a good initial
      // tetrahedron.
      if (!_isPlanar)
      {
        var collinearity = DcelMesh.GetCollinearity(point, frontFace);
        if (collinearity == Collinearity.NotCollinearInFront)
        {
          // We build a pyramid where the front face is removed.
          GrowPlanarConvexToSpatial(frontFace, point);
          return;
        }

        if (collinearity == Collinearity.NotCollinearBehind)
        {
          // We build a pyramid where the back face is removed.
          GrowPlanarConvexToSpatial(backFace, point);
          return;
        }
      }

      // ----- The point is in the plane.

      // If we get to here, the initial tetrahedron is flat, so we create a flat hull.
      _isPlanar = true;

      // Get normal of front face.
      Vector3F faceNormal = frontFace.Normal / frontFace.Normal.Length;

      // Find a visible edge.
      DcelEdge currentEdge = _mesh.Vertex.Edge;
      DcelEdge visibleEdge = null;
      do
      {
        float distance;
        DcelMesh.GetCollinearity(point, currentEdge, faceNormal, out distance);
        if (distance >= 0)
        {
          // Current is visible.
          visibleEdge = currentEdge;
          break;
        }

        // Get next edge.
        currentEdge = currentEdge.Next;

      } while (currentEdge != _mesh.Vertex.Edge);

      if (visibleEdge == null)
      {
        // Point is in the convex hull.
        return;
      }

      // Move the point a bit away to avoid numerical problems.
      Vector3F v0 = visibleEdge.Origin.Position;
      Vector3F v1 = visibleEdge.Twin.Origin.Position;
      Vector3F segment = v1 - v0;
      Vector3F normal = Vector3F.Cross(segment, faceNormal);
      point += Numeric.EpsilonF * normal;

      // Now we need to find the lower vertex and upper vertex between which
      // the edges will be removed. 
      // Two new edges will be added: (lowerVertex, Point) and (Point, upperVertex).
      DcelVertex lowerVertex = null;
      DcelVertex upperVertex = null;

      // Find upperVertex.
      currentEdge = visibleEdge.Next;
      do
      {
        float distance;
        DcelMesh.GetCollinearity(point, currentEdge, faceNormal, out distance);
        if (distance < 0)
        {
          // Edge is not visible. We have found the upper vertex.
          upperVertex = currentEdge.Origin;
          break;
        }

        currentEdge = currentEdge.Next;
      } while (currentEdge != visibleEdge.Next);

      // upperVertex == null should not happen. But numerical problems can always occur.
      // In that case we do nothing.
      Debug.Assert(upperVertex != null, "Could not find upperVertex.");
      if (upperVertex == null)
        return;

      // Find lowerVertex
      currentEdge = visibleEdge.Previous;
      do
      {
        float distance;
        DcelMesh.GetCollinearity(point, currentEdge, faceNormal, out distance);
        if (distance < 0)
        {
          // Edge is not visible. The end point of the edge is the lower vertex.
          lowerVertex = currentEdge.Next.Origin;
          break;
        }

        currentEdge = currentEdge.Previous;
      } while (currentEdge != visibleEdge.Previous);

      // lowerVertex == null should not happen. But numerical problems can always occur. 
      // In that case we do nothing.
      Debug.Assert(lowerVertex != null, "Could not find lowerVertex.");
      if (lowerVertex == null)
        return;

      Debug.Assert(lowerVertex != upperVertex, "upperVertex and lowerVertex should not be identical.");

#if DEBUG
      // Get the first visible edge of front face. This edge starts at lowerVertex.
      var firstVisibleEdge = lowerVertex.Edge;
      if (firstVisibleEdge.Face != frontFace)
        firstVisibleEdge = lowerVertex.Edge.Previous.Twin;

      // Check if all edges between lowerVertex and upperVertex are visible.
      var edge = firstVisibleEdge;
      while (edge.Origin != upperVertex)
      {
        float distance;
        DcelMesh.GetCollinearity(point, edge, faceNormal, out distance);
        Debug.Assert(distance >= 0);
        edge = edge.Next;
      }

      // Check if all edges between upperVertex and lowerVertex are not visible.
      while (edge.Origin != lowerVertex)
      {
        float distance;
        DcelMesh.GetCollinearity(point, edge, faceNormal, out distance);
        Debug.Assert(distance < 0);
        edge = edge.Next;
      }
#endif

      // Create a new vertex for the point.
      DcelVertex newVertex = new DcelVertex(point, null);
      newVertex.Edge = new DcelEdge();

      // Add two new edges between lowerVertex and upperVertex.
      DcelEdge lowerEdge = new DcelEdge();
      DcelEdge lowerEdgeTwin = new DcelEdge();
      DcelEdge upperEdge = newVertex.Edge;
      DcelEdge upperEdgeTwin = new DcelEdge();

      lowerEdge.Origin = lowerVertex;
      lowerEdge.Face = frontFace;
      lowerEdge.Next = upperEdge;
      lowerEdge.Twin = lowerEdgeTwin;

      lowerEdgeTwin.Origin = newVertex;
      lowerEdgeTwin.Face = backFace;
      lowerEdgeTwin.Previous = upperEdgeTwin;
      lowerEdgeTwin.Twin = lowerEdge;

      upperEdge.Origin = newVertex;
      upperEdge.Face = frontFace;
      upperEdge.Previous = lowerEdge;
      upperEdge.Twin = upperEdgeTwin;

      upperEdgeTwin.Origin = upperVertex;
      upperEdgeTwin.Face = backFace;
      upperEdgeTwin.Next = lowerEdgeTwin;
      upperEdgeTwin.Twin = upperEdge;

      lowerEdge.Previous = (lowerVertex.Edge.Face == frontFace) ? lowerVertex.Edge.Previous : lowerVertex.Edge.Twin;
      upperEdge.Next = (upperVertex.Edge.Face == frontFace) ? upperVertex.Edge : upperVertex.Edge.Twin.Next;
      lowerEdgeTwin.Next = lowerEdge.Previous.Twin;
      upperEdgeTwin.Previous = upperEdge.Next.Twin;

      // Correct faces.
      frontFace.Boundary = currentEdge;   // The last currentEdge was not visible, so it will stay.
      backFace.Boundary = currentEdge.Twin;

      // Correct lower and upper vertex.
      lowerVertex.Edge = lowerEdge;
      upperVertex.Edge = upperEdge.Next;

      // Correct edges before lowerEdge and after upperEdge.
      lowerEdge.Previous.Next = lowerEdge;
      lowerEdgeTwin.Next.Previous = lowerEdgeTwin;
      upperEdge.Next.Previous = upperEdge;
      upperEdgeTwin.Previous.Next = upperEdgeTwin;

      // _mesh.Vertex could be one of the removed vertices. --> Set a vertex that is in the hull.
      _mesh.Vertex = lowerVertex;

      _mesh.Dirty = true;

      Debug.Assert(_mesh.IsTwoSidedPolygon(), "DCEL mesh should be a two-sided polygon.");
    }


    /// <summary>
    /// Removes the given face and connects the face edges to the given point.
    /// </summary>
    /// <param name="face">The face.</param>
    /// <param name="point">The point.</param>
    private void GrowPlanarConvexToSpatial(DcelFace face, Vector3F point)
    {
      _faces.Remove(face);

      // Create a new vertex for the point.
      DcelVertex newVertex = new DcelVertex(point, null);

      // Traverse the boundary of the face.
      DcelEdge currentEdge = face.Boundary;
      DcelEdge previousEdge = null;
      do
      {
        // Create new face.
        DcelFace newface = new DcelFace();
        newface.Boundary = currentEdge;
        currentEdge.Face = newface;

        _faces.Add(newface);

        // Create 2 half-edges.
        currentEdge.Next = new DcelEdge();
        currentEdge.Previous = new DcelEdge();

        currentEdge.Next.Face = newface;
        currentEdge.Next.Origin = currentEdge.Twin.Origin;
        currentEdge.Next.Previous = currentEdge;
        currentEdge.Next.Next = currentEdge.Previous;

        currentEdge.Previous.Face = newface;
        currentEdge.Previous.Origin = newVertex;
        currentEdge.Previous.Previous = currentEdge.Next;
        currentEdge.Previous.Next = currentEdge;

        // Link new face with previous new face.
        if (previousEdge != null)
        {
          previousEdge.Next.Twin = currentEdge.Previous;
          currentEdge.Previous.Twin = previousEdge.Next;
        }

        // Go to next edge.
        previousEdge = currentEdge;
        currentEdge = currentEdge.Twin.Previous.Twin;

        // Go on until no edge points to face anymore.
      } while (currentEdge.Face == face);

      // Link the last new face with the first new face.
      previousEdge.Next.Twin = currentEdge.Previous;
      currentEdge.Previous.Twin = previousEdge.Next;

      Debug.Assert(previousEdge != null);
      Debug.Assert(currentEdge == face.Boundary);
      Debug.Assert(currentEdge.Face != face);

      // Set the data for the new vertex.
      newVertex.Edge = currentEdge.Previous;

      _mesh.Dirty = true;
      _type = ConvexHullType.Spatial;

      Debug.Assert(_mesh.Faces.Count == _faces.Count, "Internal error in ConvexHullBuilder.");
    }


    /// <summary>
    /// Merges a point to a 3D convex hull.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="startFace">
    /// A face which is visible from the given point. Can be <see langword="null"/>.
    /// </param>
    /// <returns><see langword="true"/> if the convex hull was grown.</returns>
    private bool GrowSpatialConvex(Vector3F point, DcelFace startFace)
    {
      Debug.Assert(_mesh != null);
      Debug.Assert(_mesh.Vertices.Count > 3, "DCEL mesh with more than 3 vertices expected.");
      Debug.Assert(_mesh.Faces.Count > 2, "DCEL mesh with more than 2 face expected.");
      
      // ----- General convex hull growth.

      // Find a face which is visible from point.
      DcelFace visibleFace = null;

      // TODO: It can happen that startFace is not in _mesh.Faces.
      // That means, _faces might contain more faces than _mesh.Faces!
      //Debug.Assert(startFace == null || _mesh.Faces.Contains(startFace), "Internal error in ConvexHullBuilder.");

      // Test the given face first. It should be visible, but the caller might have
      // made a different collinearity check which is not robust.
      if (startFace != null)
      {
        float distance;
        DcelMesh.GetCollinearity(point, startFace, out distance);
        if (distance > 0)
          visibleFace = startFace;
      }

      // startFace was not robust. Find another visible face.
      if (visibleFace == null)
      {
        int numberOfFaces = _faces.Count;
        for (int i = 0; i < numberOfFaces; i++)
        {
          DcelFace face = _faces[i];

          if (face.Tag < 0) // No need to check faces which are on the final hull.
            continue;

          float distance;
          DcelMesh.GetCollinearity(point, face, out distance);
          if (distance > 0)
          {
            visibleFace = face;
            break;
          }
        }
      }

      if (visibleFace == null)
      {
        // Nothing to do because point is in the hull.
        return false;
      }

      // Check if tags are all <= 0.
      // Temporarily removed because this occurs with the Sponza model:
      //      Debug.Assert(_mesh.Faces.All(f => f.Tag <= 0), "All tags should be <= 0.");

      // ----- Find and remove visible faces.
      // Use of tags: 
      // - Face.Tag = -1 ... face on final hull. (Only set by of Create()).
      // - Face.Tag = 0 ... default 
      // - Face.Tag = 1 ... face is visible and on stack; this face will be removed from the mesh.
      // - Edge.Tag = 0 ... default
      // - Edge.Tag = 1 ... edge is on boundary of visible area, visible area will be replaced
      //                    so this edge is on the boundary of a temporary hole.
      // - Edge.Tag = 2 ... hole edge that was handled

      _taggedEdges.Clear();

      // Push not handled visible faces on a stack.
      Stack<DcelFace> visibleFaces = new Stack<DcelFace>();
      visibleFaces.Push(visibleFace);
      visibleFace.Tag = 1; // Mark as "on the stack".

      _faces.Remove(visibleFace);

      // We store the hole edge where the neighbor triangle faces away most extreme from point.
      double minInFrontValue = double.PositiveInfinity;
      DcelEdge startEdge = null;

      // Search for all visible faces.
      while (visibleFaces.Count > 0)
      {
        visibleFace = visibleFaces.Pop();

        // Get the 3 triangle edges of the visible face.
        DcelEdge[] edges = new DcelEdge[3];
        edges[0] = visibleFace.Boundary;
        edges[1] = edges[0].Next;
        edges[2] = edges[1].Next;

        // Add visible neighbors to the stack.
        for (int i = 0; i < 3; i++)
        {
          DcelFace neighbor = edges[i].Twin.Face;

          // Maybe neighbor was already removed?
          if (neighbor == null)
            continue;

          // Get a measure for how visible the face is (> 0 means visible).
          float inFrontValue;
          //var collinearity = 
          DcelMesh.GetCollinearity(point, neighbor, out inFrontValue);

          // If the point is in front of the face or in the face plane, then we can remove
          // the face. 
          if (inFrontValue >= 0)    // Using >= 0 without epsilon is important for numerical stability!
          {
            // Neighbor is visible or collinear. Push to stack (unless it is already on the stack).
            if (neighbor.Tag != 1)
            {
              _faces.Remove(neighbor);
              visibleFaces.Push(neighbor);
              neighbor.Tag = 1;
            }
          }
          else
          {
            if (minInFrontValue > inFrontValue)
            {
              startEdge = edges[i];
              minInFrontValue = inFrontValue;
            }

            // Neighbor is not visible. This edge is part of the boundary of the hole. 
            // Set tag to 1 to mark the edge.
            edges[i].Tag = 1;
            _taggedEdges.Add(edges[i]);
          }
        }
      }

      // Temporarily removed because this occurs with the Sponza model:
      //      Debug.Assert(startEdge != null);
      if (startEdge == null)
        throw new GeometryException("Could not generate convex hull.");

      // Create a new vertex for the point.
      DcelVertex newVertex = new DcelVertex(point, null);

      // Traverse the boundary of the hole beginning at startEdge.
      DcelEdge currentEdge = startEdge;
      DcelEdge previousEdge = null;
      do
      {
        // Mark currentEdge as handled.
        currentEdge.Tag = 2;

        // Update the vertex-edge link because the link could point to a removed edge.
        currentEdge.Origin.Edge = currentEdge;

        // The next edge is part of the hole boundary and has not been handled yet.

        // Create new face.
        DcelFace face = new DcelFace();
        face.Boundary = currentEdge;
        currentEdge.Face = face;
        _faces.Add(face);

        // Create 2 half-edges.
        currentEdge.Next = new DcelEdge();
        currentEdge.Previous = new DcelEdge();
        _taggedEdges.Add(currentEdge.Next);
        _taggedEdges.Add(currentEdge.Previous);

        currentEdge.Next.Face = face;
        currentEdge.Next.Origin = currentEdge.Twin.Origin;
        currentEdge.Next.Previous = currentEdge;
        currentEdge.Next.Next = currentEdge.Previous;
        currentEdge.Next.Tag = 2;

        currentEdge.Previous.Face = face;
        currentEdge.Previous.Origin = newVertex;
        currentEdge.Previous.Previous = currentEdge.Next;
        currentEdge.Previous.Next = currentEdge;
        currentEdge.Previous.Tag = 2;

        // Link new face with previous new face.
        if (previousEdge != null)
        {
          previousEdge.Next.Twin = currentEdge.Previous;
          currentEdge.Previous.Twin = previousEdge.Next;
        }

        // Search for next edge that is marked as a hole boundary edge.
        previousEdge = currentEdge;
        currentEdge = currentEdge.Twin.Previous.Twin;
        int i = 0;
        while (currentEdge.Tag == 0)
        {
          currentEdge = currentEdge.Previous.Twin;
          i++;
          if (i == 100)
            throw new GeometryException("Could not generate convex hull.");
        }

      } while (currentEdge.Tag == 1);

      Debug.Assert(previousEdge != null);
      Debug.Assert(currentEdge == startEdge);

      // Link the last new face with the first new face.
      previousEdge.Next.Twin = currentEdge.Previous;
      currentEdge.Previous.Twin = previousEdge.Next;

      // Set the data for the new vertex.
      newVertex.Edge = startEdge.Previous;

      // Set mesh.Vertex to a vertex that is part of the hull.
      _mesh.Vertex = startEdge.Origin;

      // Reset edge tags. No need to reset face tags because marked faces are not
      // part of the mesh anymore.
      int numberOfEdges = _taggedEdges.Count;
      for (int i = 0; i < numberOfEdges; i++)
        _taggedEdges[i].Tag = 0;

      _taggedEdges.Clear();

      _mesh.Dirty = true;

      return true;
    }


    /// <summary>
    /// Gets the squared distance from a line to a point.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <param name="point">The point.</param>
    /// <returns>The distance squared.</returns>
    private static float GetDistanceFromLineSquared(ref Line line, ref Vector3F point)
    {
      Vector3F lineToPoint = point - line.PointOnLine;
      Vector3F normalFromLineToPoint = lineToPoint - Vector3F.ProjectTo(lineToPoint, line.Direction);
      return normalFromLineToPoint.LengthSquared;
    }


    // Only works for triangle meshes. Vertex welding must be done before calling this.
    // This method assumes that the triangle vertices are not equal.
    internal void RemoveDegenerateTriangles()
    {
      for (int i = 0; i < _faces.Count; i++)
      {
        DcelFace face = _faces[i];
        if (Numeric.IsZero(face.Normal.Length))
        {
          // The triangle is degenerate and must be removed. 
          
          // Find the "long" edge.
          var edge = face.Boundary;
          DcelEdge longEdge = edge;
          float longEdgeLength = longEdge.Length;

          edge = edge.Next;
          float length = edge.Length;
          if (longEdgeLength < length)
          {
            longEdge = edge;
            longEdgeLength = length;
          }

          edge = edge.Next;
          length = edge.Length;
          if (longEdgeLength < length)
          {
            longEdge = edge;
            longEdgeLength = length;
          }

          var edge0 = longEdge.Next;
          var edge1 = edge0.Next;
          Debug.Assert(edge1.Next == longEdge);

          // Now, we have the long edge.

          // The neighbor face next to the long edge.
          var neighborFace = longEdge.Twin.Face;
          var neighborEdge0 = longEdge.Twin.Next;
          var neighborEdge1 = neighborEdge0.Next;
          Debug.Assert(neighborEdge1.Next == longEdge.Twin);

          if (neighborFace == null)
          {
            // Open meshes are not yet supported.
            continue;
          }

          if (Numeric.IsZero(neighborFace.Normal.Length))
          {
            // Neighbor face is degenerate as well.
            // Both faces next to the long edge are removed.
            _faces.Remove(face);
            _faces.Remove(neighborFace);

            // Correct vertices.
            longEdge.Origin.Edge = edge1.Twin;
            edge0.Origin.Edge = neighborEdge1.Twin;
            edge1.Origin.Edge = edge0.Twin;

            // Relink edges.
            edge0.Twin.Twin = neighborEdge1.Twin;
            neighborEdge1.Twin.Twin = edge0.Twin;
            edge1.Twin.Twin = neighborEdge0.Twin;
            neighborEdge0.Twin.Twin = edge1.Twin;

            // Remove the vertex neighborEdge1.Origin. We make sure all edges
            // that originate in this vertex do now originate in edge1.Origin.
            edge = neighborEdge1;
            while (edge != neighborEdge1)
            {
              edge.Origin = edge1.Origin;
              edge = edge.Previous.Twin;
            }
          }
          else
          {
            // The long edge is relocated.
            // First correct the vertices of the long edge.
            neighborEdge0.Origin.Edge = neighborEdge0;
            edge0.Origin.Edge = edge0;
            longEdge.Origin = neighborEdge1.Origin;
            longEdge.Twin.Origin = edge1.Origin;

            // Then, correct the faces.
            face.Boundary = longEdge;
            edge1.Face = face;
            neighborEdge0.Face = face;

            neighborFace.Boundary = longEdge.Twin;
            longEdge.Twin.Face = neighborFace;
            edge0.Face = neighborFace;

            // Then, correct edge links.
            neighborEdge0.Next = longEdge;
            longEdge.Next = edge1;
            edge1.Next = neighborEdge0;
            neighborEdge0.Previous = edge1;
            edge1.Previous = longEdge;
            longEdge.Previous = neighborEdge0;

            neighborEdge1.Next = edge0;
            edge0.Next = longEdge.Twin;
            longEdge.Twin.Next = neighborEdge1;
            neighborEdge1.Previous = longEdge.Twin;
            longEdge.Twin.Previous = edge0;
            edge0.Previous = neighborEdge1;
          }

          _mesh.Dirty = true;

          // Restart search!
          i = -1;
        }
      }
    }
  }
}
