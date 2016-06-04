// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Computes a tight-fitting capsule that contains the given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="radius">The radius of the capsule.</param>
    /// <param name="height">The height of the capsule.</param>
    /// <param name="pose">The pose of the capsule.</param>
    /// <remarks>
    /// The computed capsule is an approximation. Capsules work best for long objects.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void ComputeBoundingCapsule(IList<Vector3F> points, out float radius, out float height, out Pose pose)
    {
      if (points == null)
        throw new ArgumentNullException("points");

      // Covariance matrix.
      MatrixF cov = null;

      // ReSharper disable EmptyGeneralCatchClause
      try
      {
        if (points.Count > 4)
        {
          // Reduce point list to convex hull.
          DcelMesh dcelMesh = CreateConvexHull(points);
          TriangleMesh mesh = dcelMesh.ToTriangleMesh();

          // Use reduced point list - if we have found a useful one. (Line objects 
          // have not useful triangle mesh.)
          if (mesh.Vertices.Count > 0)
            points = mesh.Vertices;

          cov = ComputeCovarianceMatrixFromSurface(mesh);
        }
      }
      catch
      {
      }
      // ReSharper restore EmptyGeneralCatchClause

      // If anything happens in the convex hull creation, we can still go on including the
      // interior points and compute the covariance matrix for the points instead of the 
      // surface.
      if (cov == null || Numeric.IsNaN(cov.Determinant))
        cov = ComputeCovarianceMatrixFromPoints(points);

      // Perform Eigenvalue decomposition.
      EigenvalueDecompositionF evd = new EigenvalueDecompositionF(cov);

      // v transforms from local coordinate space of the capsule into world space.
      var v = evd.V.ToMatrix33F();

      Debug.Assert(v.GetColumn(0).IsNumericallyNormalized);
      Debug.Assert(v.GetColumn(1).IsNumericallyNormalized);
      Debug.Assert(v.GetColumn(2).IsNumericallyNormalized);

      // v is like a rotation matrix, but the coordinate system is not necessarily right handed.
      // --> Make sure it is right-handed.
      v.SetColumn(2, Vector3F.Cross(v.GetColumn(0), v.GetColumn(1)));

      // Make local Y the largest axis. (Y is the long capsule axis.)
      Vector3F eigenValues = evd.RealEigenvalues.ToVector3F();
      int largestComponentIndex = eigenValues.IndexOfLargestComponent;
      if (largestComponentIndex != 1)
      {
        // Swap two columns to create a right handed rotation matrix.
        Vector3F colLargest = v.GetColumn(largestComponentIndex);
        Vector3F col1 = v.GetColumn(1);
        v.SetColumn(1, colLargest);
        v.SetColumn(largestComponentIndex, col1);
        v.SetColumn(2, Vector3F.Cross(v.GetColumn(0), v.GetColumn(1)));
      }

      // Compute capsule for the orientation given by v.
      Vector3F center;
      ComputeBoundingCapsule(points, v, out radius, out height, out center);
      pose = new Pose(center, v);
    }


    // Computes the bounding capsule with the given orientation.
    // Returns the volume of the capsule.
    internal static float ComputeBoundingCapsule(IList<Vector3F> points, Matrix33F orientation, out float radius, out float height, out Vector3F center)
    {
      // Transform all points to local space. 
      var localPoints = new Vector3F[points.Count];
      for (int i = 0; i < localPoints.Length; i++)
        localPoints[i] = orientation.Transposed * points[i];

      // Get highest and lowest coordinates.
      float minY = float.PositiveInfinity;
      float maxY = float.NegativeInfinity;
      for (int i = 0; i < localPoints.Length; i++)
      {
        var p = localPoints[i];
        if (p.Y < minY)
          minY = p.Y;
        else if (p.Y > maxY)
          maxY = p.Y;
      }

      // Now all points are in local space.
      // Project points to xz-plane.
      var projectedPoints = new Vector3F[localPoints.Length];
      for (int i = 0; i < localPoints.Length; i++)
        projectedPoints[i] = new Vector3F(localPoints[i].X, 0, localPoints[i].Z);

      // Compute Welzl sphere for projected points. This gives us the final radius and 
      // x and z of the final center.
      ComputeBoundingSphere(projectedPoints, out radius, out center);

      Debug.Assert(center.Y == 0);

      // We still have to determine the height.
      // Let's assume that the cylindric part is as small as possible. And then we extend
      // the cylinder part until the spherical caps contain all points.
      float centerY = (minY + maxY) / 2;
      float cylinderTop = Math.Max(centerY, maxY - radius);
      float cylinderBottom = Math.Min(centerY, minY + radius);

      // Extent the cylinder.
      float radiusSquared = radius * radius;
      for (int i = 0; i < localPoints.Length; i++)
      {
        // Get point relative to cylinder axis.
        var p = localPoints[i] - center;
        if (p.Y > cylinderTop)
        {
          // Point is above cylinder. We might have to grow the cylinder upwards.
          if ((p - new Vector3F(0, cylinderTop, 0)).LengthSquared > radiusSquared)
          {
            // p is not contained by the spherical cap. :-(
            // Grow cylinderTop so that the point is exactly on the spherical cap surface.
            // We use Pythagoras: radius² = distanceOfPToAxis² + yPart²
            // And the cylinderTop must be grown to p.Y - yPart. 
            // In other words:
            cylinderTop = p.Y; // Now the point is in the cylinder.
            // Move cylinder down as far as possible.
            float distanceToAxisSquared = new Vector3F(p.X, 0, p.Z).LengthSquared;
            if (distanceToAxisSquared < radiusSquared)
              cylinderTop -= (float)Math.Sqrt(radiusSquared - distanceToAxisSquared);
          }
        }
        else if (p.Y < cylinderBottom)
        {
          // Same as above but with the bottom of the cylinder.
          if ((p - new Vector3F(0, cylinderBottom, 0)).LengthSquared > radiusSquared)
          {
            cylinderBottom = p.Y;
            float distanceToAxisSquared = new Vector3F(p.X, 0, p.Z).LengthSquared;
            if (distanceToAxisSquared < radiusSquared)
              cylinderBottom += (float)Math.Sqrt(radiusSquared - distanceToAxisSquared);
          }
        }
      }

      center.Y = (cylinderBottom + cylinderTop) / 2;
      height = Math.Max(cylinderTop - cylinderBottom + 2 * radius, 2 * radius);

      // center must be converted to world space.
      center = orientation * center;

      // Volume of both spherical caps
      float sphereVolume = 4.0f / 3.0f * ConstantsF.Pi * radius * radius * radius;

      // Volume of cylinder
      float cylinderVolume = ConstantsF.Pi * radius * radius * (height - 2 * radius);

      return sphereVolume + cylinderVolume;
    }
  }
}
