// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Determines whether two points P and Q are on opposite sides of a plane. The plane is
    /// determined by three points (A, B, C).
    /// </summary>
    /// <param name="pointP">The point P.</param>
    /// <param name="pointQ">The point Q.</param>
    /// <param name="planePointA">The plane point A.</param>
    /// <param name="planePointB">The plane point B.</param>
    /// <param name="planePointC">The plane point C.</param>
    /// <returns>
    /// <see langword="true"/> if P and Q are on opposite sides; <see langword="false"/> if they are
    /// on the same side; and <see langword="null"/> if the point is on the plane (within a 
    /// numerical tolerance) or if the case is degenerate (A, B, C does not form a valid triangle).
    /// </returns>
    /// <remarks>
    /// This method creates a plane from the three points A, B, and C. The points must be in ordered
    /// counter-clockwise. The front-face (which points to the outside of the shape - the empty 
    /// half-space) is defined through the counter-clockwise order of the points.
    /// </remarks>
    public static bool? ArePointsOnOppositeSides(Vector3F pointP, Vector3F pointQ, Vector3F planePointA, Vector3F planePointB, Vector3F planePointC)
    {
      Vector3F normal = Vector3F.Cross(planePointB - planePointA, planePointC - planePointA);
      float signP = Vector3F.Dot(pointP - planePointA, normal);
      float signQ = Vector3F.Dot(pointQ - planePointA, normal);

      if (Numeric.IsZero(signP * signQ, Numeric.EpsilonFSquared))
      {
        // A degenerate/affine dependent case.
        return null;
      }

      // The points are on opposite sides if the signs are different.
      return signP * signQ < 0;
    }


    /// <overloads>
    /// <summary>
    /// Gets the point on a primitive that is closest to a given point.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the point on a plane surface that is closest to a given point.
    /// </summary>
    /// <param name="point">The point position.</param>
    /// <param name="plane">The plane.</param>
    /// <param name="pointOnPlane">
    /// The point on the surface of the plane that is closest to <paramref name="point"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the point lies in the plane (<paramref name="pointOnPlane"/> and 
    /// <paramref name="point"/> are numerically identical); otherwise <see langword="false"/> if 
    /// the point is either above or below the plane.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static bool GetClosestPoint(Plane plane, Vector3F point, out Vector3F pointOnPlane)
    {
      float distance = GetDistance(plane, point);
      pointOnPlane = point - plane.Normal * distance;
      return Numeric.IsZero(distance);
    }


    /// <summary>
    /// Gets the point on a plane surface that is closest to a given point.
    /// </summary>
    /// <param name="point">The point position.</param>
    /// <param name="plane">The plane.</param>
    /// <returns>
    /// The point on the surface of the plane that is closest to <paramref name="point"/>.
    /// </returns>
    internal static Vector3F GetClosestPoint(Plane plane, Vector3F point)
    {
      float distance = GetDistance(plane, point);
      return point - plane.Normal * distance;
    }


    /// <summary>
    /// Gets the closest points of a line and a plane.
    /// </summary>
    /// <param name="plane">The plane.</param>
    /// <param name="line">The line.</param>
    /// <param name="pointOnPlane">
    /// The point on <paramref name="plane"/> that is closest to <paramref name="line"/>.
    /// </param>
    /// <param name="pointOnLine">
    /// The point on <paramref name="line"/> that is closest to <paramref name="plane"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the line and the plane are touching 
    /// (<paramref name="pointOnLine"/> and <paramref name="pointOnPlane"/> are identical); 
    /// otherwise <see langword="false"/>
    /// </returns>
    /// <remarks>
    /// The plane is treated as a real 2D plane - not as a 3D half-space.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public static bool GetClosestPoints(Plane plane, Line line, out Vector3F pointOnPlane, out Vector3F pointOnLine)
    {
      // See Coutinho: "Dynamic Simulations of Multibody Systems", p. 267

      float directionDotNormal = Vector3F.Dot(line.Direction, plane.Normal);
      if (Numeric.IsZero(directionDotNormal))
      {
        // Line is parallel to plane.
        pointOnLine = line.PointOnLine;
        pointOnPlane = GetClosestPoint(plane, pointOnLine);
        return false;
      }

      // Non-parallel line will hit the plane.
      float parameter = (plane.DistanceFromOrigin - Vector3F.Dot(line.PointOnLine, plane.Normal)) / directionDotNormal;
      pointOnLine = pointOnPlane = line.PointOnLine + parameter * line.Direction;
      return true;
    }


    /// <summary>
    /// Gets the closest points of a line and a plane.
    /// </summary>
    /// <param name="plane">The plane.</param>
    /// <param name="lineSegment">The line segment.</param>
    /// <param name="pointOnPlane">
    /// The point on <paramref name="plane"/> that is closest to <paramref name="lineSegment"/>.
    /// </param>
    /// <param name="pointOnLineSegment">
    /// The point on <paramref name="lineSegment"/> that is closest to <paramref name="plane"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the line segment and the plane are touching 
    /// (<paramref name="pointOnLineSegment"/> and <paramref name="pointOnPlane"/> are identical);
    /// otherwise <see langword="false"/>
    /// </returns>
    /// <remarks>
    /// The plane is treated as a real 2D plane - not as a 3D half-space.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "OnLine")]
    public static bool GetClosestPoints(Plane plane, LineSegment lineSegment, out Vector3F pointOnPlane, out Vector3F pointOnLineSegment)
    {
      // See Coutinho: "Dynamic Simulations of Multibody Systems", p. 267

      Vector3F segmentVector = lineSegment.End - lineSegment.Start;
      float segmentDotNormal = Vector3F.Dot(segmentVector, plane.Normal);
      if (Numeric.IsZero(segmentDotNormal))
      {
        // Line is parallel
        pointOnLineSegment = lineSegment.Start;
        pointOnPlane = GetClosestPoint(plane, pointOnLineSegment);
        return false;
      }

      float parameter = (plane.DistanceFromOrigin - Vector3F.Dot(lineSegment.Start, plane.Normal)) / segmentDotNormal;

      // If parameter is outside [0, 1], we have to clamp it to the segment.
      if (parameter < 0)
      {
        pointOnLineSegment = lineSegment.Start;
        pointOnPlane = GetClosestPoint(plane, pointOnLineSegment);
        return false;
      }
      if (parameter > 1)
      {
        pointOnLineSegment = lineSegment.End;
        pointOnPlane = GetClosestPoint(plane, pointOnLineSegment);
        return false;
      }

      // Parameter is in [0,1], so the segment intersects with the plane.
      pointOnLineSegment = pointOnPlane = lineSegment.Start + parameter * segmentVector;
      return true;
    }


    /// <summary>
    /// Gets the signed distance of a point to a plane surface.
    /// </summary>
    /// <param name="point">The point.</param>
    /// <param name="plane">The plane.</param>
    /// <returns>
    /// The signed distance. This value is positive if the point is in the positive half-space
    /// (separation); otherwise, negative (penetration).
    /// </returns>
    public static float GetDistance(Plane plane, Vector3F point)
    {
      float projectedLength = Vector3F.Dot(point, plane.Normal);  // Length of projection on the normal.
      return projectedLength - plane.DistanceFromOrigin;
    }


    /// <summary>
    /// Gets the intersection point of three planes.
    /// </summary>
    /// <param name="planeA">The first plane.</param>
    /// <param name="planeB">The second plane.</param>
    /// <param name="planeC">The third plane.</param>
    /// <returns>
    /// The point that touches all three planes. (<see cref="float.NaN"/>, <see cref="float.NaN"/>, 
    /// <see cref="float.NaN"/>) is returned if there is no unique intersection point, for example,
    /// when two planes are parallel or the planes intersect in a line.
    /// </returns>
    public static Vector3F GetIntersection(Plane planeA, Plane planeB, Plane planeC)
    {
      // Get a point that meets this requirements: Dot(plane.Normal, point) == plane.DistanceFromOrigin
      Matrix33F matrix = new Matrix33F(planeA.Normal.X, planeA.Normal.Y, planeA.Normal.Z,
                                       planeB.Normal.X, planeB.Normal.Y, planeB.Normal.Z,
                                       planeC.Normal.X, planeC.Normal.Y, planeC.Normal.Z);
      Vector3F distances = new Vector3F(planeA.DistanceFromOrigin, planeB.DistanceFromOrigin, planeC.DistanceFromOrigin);
      bool isInvertible = matrix.TryInvert();
      if (isInvertible)
        return matrix * distances;
      else
        return new Vector3F(float.NaN);
    }


    /// <summary>
    /// Determines whether the given axis-aligned bounding box (AABB) and plane overlap.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="plane">The plane.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the plane have a contact; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The <paramref name="plane"/> defines a "half-space". This method returns 
    /// <see langword="true"/> if the AABB is intersecting the plane or if the
    /// AABB is completely behind the plane in the negative half-space.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabb, Plane plane)
    {
      // Get support point of AABB nearest to the plane.
      Vector3F support = new Vector3F(
        plane.Normal.X > 0 ? aabb.Minimum.X : aabb.Maximum.X,
        plane.Normal.Y > 0 ? aabb.Minimum.Y : aabb.Maximum.Y,
        plane.Normal.Z > 0 ? aabb.Minimum.Z : aabb.Maximum.Z);

      float projectedLength = Vector3F.Dot(support, plane.Normal);
      return projectedLength <= plane.DistanceFromOrigin;
    }
  }
}
