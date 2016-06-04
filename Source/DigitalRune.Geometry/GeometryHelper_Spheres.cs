// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Computes the minimum sphere that contains the given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="center">The center of the sphere.</param>
    /// <remarks>
    /// The computed sphere is minimal and not an approximation. 
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public static void ComputeBoundingSphere(IEnumerable<Vector3F> points, out float radius, out Vector3F center)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      List<Vector3F> pointList = DigitalRune.ResourcePools<Vector3F>.Lists.Obtain();

      // Copy points into new list because the order of the points need to be changed.
      // (Try to avoid garbage when enumerating 'points'.)
      if (points is Vector3F[])
      {
        foreach (Vector3F p in (Vector3F[])points)
          pointList.Add(p);
      }
      else if (points is List<Vector3F>)
      {
        foreach (Vector3F p in (List<Vector3F>)points)
          pointList.Add(p);
      }
      else
      {
        foreach (Vector3F p in points)
          pointList.Add(p);
      }

      if (pointList.Count == 0)
        throw new ArgumentException("The list of 'points' is empty.");

      ComputeWelzlSphere(pointList, 0, pointList.Count - 1, 0, out radius, out center);

      DigitalRune.ResourcePools<Vector3F>.Lists.Recycle(pointList);
    }


    /// <summary>
    /// Computes the minimum sphere using the Welzl algorithm.
    /// </summary>
    /// <param name="points">The list of points.</param>
    /// <param name="firstPoint">The index of the first point to consider.</param>
    /// <param name="lastPoint">The index of the last point to consider.</param>
    /// <param name="numberOfSupportPoints">The number of support points.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="center">The sphere center.</param>
    /// <remarks>
    /// <paramref name="points"/> is organized like this: At the index <paramref name="firstPoint"/> 
    /// is the first new point that should be handled. All support points are stored directly before
    /// <paramref name="firstPoint"/>. All points up to the index <paramref name="lastPoint"/> 
    /// (inclusive) should be handled in this call.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private static void ComputeWelzlSphere(List<Vector3F> points, int firstPoint, int lastPoint, int numberOfSupportPoints, out float radius, out Vector3F center)
    {
      // This is an implementation of the Welzl algorithm.
      // See http://www.flipcode.com/archives/Smallest_Enclosing_Spheres.shtml.
      // The algorithm is also described in "Computational Tools for Computer Graphics" section 13.11.4,
      // "Real-Time Collision Detection" or "Physics-Based Animation".

      center = new Vector3F(float.NaN);
      radius = float.NaN;

      // Start with a sphere containing the support points.
      switch (numberOfSupportPoints)
      {
        case 0:
          break;
        case 1:
          center = points[firstPoint - 1];
          radius = 0;
          break;
        case 2:
          ComputeCircumscribedSphere(points[firstPoint - 1], points[firstPoint - 2], out radius, out center);
          break;
        case 3:
          ComputeCircumscribedSphere(points[firstPoint - 1], points[firstPoint - 2], points[firstPoint - 3], out radius, out center);
          break;
        case 4:
          ComputeCircumscribedSphere(points[firstPoint - 1], points[firstPoint - 2], points[firstPoint - 3], points[firstPoint - 4], out radius, out center);
          return;   // Return! 4 support points are the maximum.
        default:
          throw new InvalidOperationException("Unexpected case in ComputeWelzlSphere().");
      }

      // Handle all points from firstPoint to lastPoint (included).
      for (int i = firstPoint; i <= lastPoint; i++)
      {
        Vector3F point = points[i];

        if (!HaveContact(radius, point - center))
        {
          // If the point is not in the sphere, we move it to the beginning of the out point range, 
          // to have all support points at the beginning.
          if (i > firstPoint)
          {
            points.RemoveAt(i);
            points.Insert(firstPoint, point);
          }

          // Recursively compute the sphere. The current point is treated as additional support
          // point. The recursive sphere must contain all points from firstPoint to the current index i.
          ComputeWelzlSphere(points, firstPoint + 1, i, numberOfSupportPoints + 1, out radius, out center);
        }
      }
    }


    /// <overloads>
    /// <summary>
    /// Computes a sphere where all given points touch the surface.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes a sphere where all given points touch the surface.
    /// </summary>
    /// <param name="point0">The first point.</param>
    /// <param name="point1">The second point.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="center">The sphere center.</param>
    /// <remarks>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void ComputeCircumscribedSphere(Vector3F point0, Vector3F point1, out float radius, out Vector3F center)
    {
      center = (point0 + point1) / 2;
      radius = (point1 - point0).Length / 2;
    }


    /// <summary>
    /// Computes a sphere where all given points touch the surface.
    /// </summary>
    /// <param name="point0">The first point.</param>
    /// <param name="point1">The second point.</param>
    /// <param name="point2">The third point.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="center">The sphere center.</param>
    /// <remarks>
    /// <para>
    /// A circumscribed sphere is not necessarily the minimal sphere that contains the given points.
    /// There might be a smaller sphere where the surface touches only 2 points and the third points
    /// is inside the sphere. To get a minimal sphere call <see cref="ComputeBoundingSphere"/>.
    /// </para>
    /// <para>
    /// The given points must form a valid triangle with an area greater than 0; otherwise, the
    /// result is not defined (radius is <see cref="float.NaN"/>).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void ComputeCircumscribedSphere(Vector3F point0, Vector3F point1, Vector3F point2, out float radius, out Vector3F center)
    {
      // Compute barycentric coordinates of the circumcenter.
      // See http://en.wikipedia.org/wiki/Circumscribed_circle

      // Get the squared side lengths.
      float a2 = (point2 - point1).LengthSquared;
      float b2 = (point2 - point0).LengthSquared;
      float c2 = (point1 - point0).LengthSquared;

      float d = 2 * a2 * b2 + 2 * a2 * c2 + 2 * b2 * c2 - a2 * a2 - b2 * b2 - c2 * c2;
      float oneOverD = 1 / d;

      float u = a2 * (b2 + c2 - a2) * oneOverD;
      float v = b2 * (a2 + c2 - b2) * oneOverD;
      float w = c2 * (a2 + b2 - c2) * oneOverD;

      center = u * point0 + v * point1 + w * point2;
      radius = (point0 - center).Length;
    }


    /// <summary>
    /// Computes a sphere where all given points touch the surface.
    /// </summary>
    /// <param name="point0">The first point.</param>
    /// <param name="point1">The second point.</param>
    /// <param name="point2">The third point.</param>
    /// <param name="point3">The fourth point.</param>
    /// <param name="radius">The sphere radius.</param>
    /// <param name="center">The sphere center.</param>
    /// <remarks>
    /// <para>
    /// A circumscribed ball is not necessarily the minimal sphere that contains the given points.
    /// There might be a smaller sphere where the surface touches only 2 points and the other points
    /// are inside the sphere. To get a minimal sphere call <see cref="ComputeBoundingSphere"/>.
    /// </para>
    /// <para>
    /// The given points must form a tetrahedron with a volume greater than 0; otherwise, the
    /// result is not defined (radius is <see cref="float.NaN"/>).
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void ComputeCircumscribedSphere(Vector3F point0, Vector3F point1, Vector3F point2, Vector3F point3, out float radius, out Vector3F center)
    {
      // See "Geometric Tools for Computer Graphics" p. 801.

      // Set point0 to the origin and compute relative points for 1, 2 and 3.
      Vector3F p1 = point1 - point0;
      Vector3F p2 = point2 - point0;
      Vector3F p3 = point3 - point0;

      // Compute the distances to point0.
      float length1Squared = p1.LengthSquared;
      float length2Squared = p2.LengthSquared;
      float length3Squared = p3.LengthSquared;

      // Compute the volume of the tetrahedron formed by the four points.
      float volume = 1f / 6 * (Vector3F.Dot(p1, Vector3F.Cross(p2, p3)));

      // Compute the center.
      center = new Vector3F();
      float k = 1 / (12 * volume);
      center.X = point0.X + k * (+(p2.Y * p3.Z - p3.Y * p2.Z) * length1Squared
                                 - (p1.Y * p3.Z - p3.Y * p1.Z) * length2Squared
                                 + (p1.Y * p2.Z - p2.Y * p1.Z) * length3Squared);
      center.Y = point0.Y + k * (-(p2.X * p3.Z - p3.X * p2.Z) * length1Squared
                                 + (p1.X * p3.Z - p3.X * p1.Z) * length2Squared
                                 - (p1.X * p2.Z - p2.X * p1.Z) * length3Squared);
      center.Z = point0.Z + k * (+(p2.X * p3.Y - p3.X * p2.Y) * length1Squared
                                 - (p1.X * p3.Y - p3.X * p1.Y) * length2Squared
                                 + (p1.X * p2.Y - p2.X * p1.Y) * length3Squared);

      float dx = center.X - point0.X;
      float dy = center.Y - point0.Y;
      float dz = center.Z - point0.Z;
      radius = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }


    /// <summary>
    /// Determines whether the specified point is inside the sphere.
    /// </summary>
    /// <param name="sphereRadius">The sphere radius.</param>
    /// <param name="point">The point (in the local space of the sphere).</param>
    /// <returns>
    /// <see langword="true"/> if the specified point is inside; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HaveContact(float sphereRadius, Vector3F point)
    {
      // Point distance to the sphere center.
      float distanceToCenterSquared = point.LengthSquared;

      // To fight numerical problems: Extrude the sphere.
      float extendedRadius = sphereRadius + Math.Max(1, sphereRadius) * Numeric.EpsilonF;

      // Distance from the surface (positive = outside, negative = inside).
      float distanceFromSurfaceSquared = distanceToCenterSquared - extendedRadius * extendedRadius;

      return distanceFromSurfaceSquared <= 0;
    }
  }
}
