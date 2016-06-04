// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Statistics;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Computes the minimum box that contains the given points.
    /// </summary>
    /// <param name="points">The points.</param>
    /// <param name="extent">The box extent (the size in x, y and z).</param>
    /// <param name="pose">The pose of the box.</param>
    /// <remarks>
    /// If a box with the dimensions given in <paramref name="extent"/> is positioned with 
    /// <paramref name="pose"/> all given points will be inside the box. The box is an 
    /// approximation and not necessarily optimal.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="points"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="points"/> is empty.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    public static void ComputeBoundingBox(IList<Vector3F> points, out Vector3F extent, out Pose pose)
    {
      // PCA of the convex hull is used (see "Physics-Based Animation", pp. 483, and others.)
      // in addition to a brute force search. The optimum is returned.

      if (points == null)
        throw new ArgumentNullException("points");
      if (points.Count == 0)
        throw new ArgumentException("The list of 'points' is empty.");

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

          // Use reduced point list - if we have found a useful one. (Line objects do not have
          // a useful triangle mesh.)
          if (mesh.Vertices.Count > 0)
          {
            points = mesh.Vertices;
            cov = ComputeCovarianceMatrixFromSurface(mesh);
          }
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
      {
        // Make copy of points list because ComputeBoundingBox() will reorder the points.
        points = points.ToList();
        cov = ComputeCovarianceMatrixFromPoints(points);
      }

      // Perform Eigenvalue decomposition.
      EigenvalueDecompositionF evd = new EigenvalueDecompositionF(cov);

      // v transforms from local coordinate space of the box into world space.
      var v = evd.V.ToMatrix33F();

      Debug.Assert(v.GetColumn(0).IsNumericallyNormalized);
      Debug.Assert(v.GetColumn(1).IsNumericallyNormalized);
      Debug.Assert(v.GetColumn(2).IsNumericallyNormalized);

      // v is like a rotation matrix, but the coordinate system is not necessarily right handed.
      // --> Make sure it is right-handed.
      v.SetColumn(2, Vector3F.Cross(v.GetColumn(0), v.GetColumn(1)));

      // Another way to do this:
      //// Make sure that V is a rotation matrix. (V could be an orthogonal matrix that
      //// contains a mirror operation. In other words, V could be a rotation matrix for 
      //// a left handed coordinate system.)
      //if (!v.IsRotation)
      //{
      //  // Swap two columns to create a right handed rotation matrix.
      //  Vector3F col1 = v.GetColumn(2);
      //  Vector3F col2 = v.GetColumn(2);
      //  v.SetColumn(1, col2);
      //  v.SetColumn(2, col1);
      //}

      // If the box axes are parallel to the world axes, create a box with NO rotation.
      TryToMakeIdentityMatrix(ref v);

      Vector3F center;
      float volume = ComputeBoundingBox(points, v, float.PositiveInfinity, out extent, out center);

      // Brute force search for better box.
      // This was inspired by the OBB algorithm of John Ratcliff, www.codesuppository.com.

      Vector3F αBest = Vector3F.Zero;         // Search for optimal angles.
      float αMax = MathHelper.ToRadians(45);  // On each axis we rotate from -αMax to +αMax.
      float αMin = MathHelper.ToRadians(1);   // We abort when αMax == 1°.
      const float numberOfSteps = 7;          // In each iteration we divide αMax in this number of steps.

      // In each loop we test angles between -αMax and +αMax. 
      // Then we half αMax and search again. 
      while (αMax >= αMin)
      {
        bool foundBetterAngles = false;  // Better angles found?
        float αStep = αMax / numberOfSteps;

        // We test around this angle:
        Vector3F α = αBest;

        for (float αX = α.X - αMax; αX <= α.X + αMax; αX += αStep)
        {
          for (float αY = α.Y - αMax; αY <= α.Y + αMax; αY += αStep)
          {
            for (float αZ = α.Z - αMax; αZ <= α.Z + αMax; αZ += αStep)
            {
              Vector3F centerNew;
              Vector3F boxExtentNew;
              Matrix33F vNew = QuaternionF.CreateRotation(αX, Vector3F.UnitX, αY, Vector3F.UnitY, αZ, Vector3F.UnitZ, true).ToRotationMatrix33();
              float volumeNew = ComputeBoundingBox(points, vNew, volume, out boxExtentNew, out centerNew);
              if (volumeNew < volume)
              {
                foundBetterAngles = true;
                center = centerNew;
                extent = boxExtentNew;
                v = vNew;
                volume = volumeNew;
                αBest = new Vector3F(αX, αY, αZ);
              }
            }
          }
        }

        // Search again in half the interval around the best angles or abort.
        if (foundBetterAngles)
          αMax *= 0.5f;
        else
          αMax = 0;
      }

      pose = new Pose(center, v);
    }    


    // Computes the covariance matrix for a list of points.
    private static MatrixF ComputeCovarianceMatrixFromPoints(IList<Vector3F> points)
    {
      // Convert IList<Vector3F> to IList<VectorF> which is required for PCA.
      int numberOfPoints = points.Count;
      List<VectorF> pointsCopy = new List<VectorF>(numberOfPoints);
      for (int i = 0; i < numberOfPoints; i++)
      {
        var point = points[i];
        pointsCopy.Add(point.ToVectorF());
      }

      return StatisticsHelper.ComputeCovarianceMatrix(pointsCopy);
    }


    // Computes the surface covariance matrix for a convex hull of the given points.
    private static MatrixF ComputeCovarianceMatrixFromSurface(ITriangleMesh mesh)
    {
      // Compute covariance matrix for the surface of the triangle mesh.
      // See Physics-Based Animation for a derivation. Variable names are the same as in
      // the book. 
      // ... Better look at Real-Time Collision Detection p. 108. The Physics-Based Animation
      // book has errors.

      MatrixF C = new MatrixF(3, 3);          // The covariance matrix.
      float A = 0;                            // Total surface area.
      Vector3F mS = Vector3F.Zero;            // Mean point of the entire surface.
      for (int k = 0; k < mesh.NumberOfTriangles; k++)
      {
        var triangle = mesh.GetTriangle(k);
        var pK = triangle.Vertex0;
        var qK = triangle.Vertex1;
        var rK = triangle.Vertex2;

        var mK = 1f / 3f * (pK + qK + rK);

        var uK = qK - pK;
        var vK = rK - pK;
        var Ak = 0.5f * Vector3F.Cross(uK, vK).Length;
        A += Ak;

        mS += Ak * mK;

        for (int i = 0; i < 3; i++)
          for (int j = i; j < 3; j++)
            C[i, j] += Ak / 12f * (9 * mK[i] * mK[j]
                                     + pK[i] * pK[j]
                                     + qK[i] * qK[j]
                                     + rK[i] * rK[j]);
      }

      mS /= A;

      for (int i = 0; i < 3; i++)
        for (int j = i; j < 3; j++)
          C[i, j] = 1 / A * C[i, j] - mS[i] * mS[j];

      // Set the other half of the symmetric matrix. 
      for (int i = 0; i < 3; i++)
        for (int j = i + 1; j < 3; j++)
          C[j, i] = C[i, j];

      return C;
    }


    // Computes a bounding box that contains the given points. The bounding box orientation
    // is determined by the given orientation. The outputs are the box extent and the box center.
    // volumeLimit is used for early out.
    // Returns the volume of the new box.
    // If the new box volume is smaller than volumeLimit, the 6 extreme vertices are moved to the
    // front of the list.
    private static float ComputeBoundingBox(IList<Vector3F> points, Matrix33F boxOrientation, float volumeLimit, out Vector3F boxExtent, out Vector3F boxCenter)
    {
      boxExtent = new Vector3F(float.PositiveInfinity);
      boxCenter = Vector3F.Zero;
      float volume = float.PositiveInfinity;

      // The inverse orientation.
      Matrix33F orientationInverse = boxOrientation.Transposed;

      // The min and max points in the local box space.
      Vector3F min = new Vector3F(float.PositiveInfinity);
      Vector3F max = new Vector3F(float.NegativeInfinity);

      // Remember the extreme points.
      Vector3F minX = Vector3F.Zero; 
      Vector3F maxX = Vector3F.Zero; 
      Vector3F minY = Vector3F.Zero; 
      Vector3F maxY = Vector3F.Zero; 
      Vector3F minZ = Vector3F.Zero; 
      Vector3F maxZ = Vector3F.Zero; 

      for (int i = 0; i < points.Count; i++)
      {
        var point = points[i];

        // Rotate point into local box space.
        var localPoint = orientationInverse * point;

        // Is this vertex on the box surface?
        bool isExtreme = false;

        // Store min and max coordinates.
        if (localPoint.X < min.X)
        {
          min.X = localPoint.X;
          minX = point;
          isExtreme = true;
        }
        
        if (localPoint.X > max.X)
        {
          max.X = localPoint.X;
          maxX = point;
          isExtreme = true;
        }

        if (localPoint.Y < min.Y)
        {
          min.Y = localPoint.Y;
          minY = point;
          isExtreme = true;
        }
        
        if (localPoint.Y > max.Y)
        {
          max.Y = localPoint.Y;
          maxY = point;
          isExtreme = true;
        }

        if (localPoint.Z < min.Z)
        {
          min.Z = localPoint.Z;
          minZ = point;
          isExtreme = true;
        }
        
        if (localPoint.Z > max.Z)
        {
          max.Z = localPoint.Z;
          maxZ = point;
          isExtreme = true;
        }

        if (isExtreme)
        {
          // The box has grown. 
          boxExtent = max - min;
          volume = boxExtent.X * boxExtent.Y * boxExtent.Z;

          // Early out, if volume is beyond the limit.
          if (volume > volumeLimit)
            return float.PositiveInfinity;
        }
      }

      // If we come to here, the new box is better than the old known box.
      // Move the extreme vertices to the front of the list. This improves
      // the early out drastically. - See Real-Time Collision Detection.
      points.Remove(minX);
      points.Insert(0, minX);
      points.Remove(minY);
      points.Insert(0, minY);
      points.Remove(minZ);
      points.Insert(0, minZ);
      points.Remove(maxX);
      points.Insert(0, maxX);
      points.Remove(maxY);
      points.Insert(0, maxY);
      points.Remove(maxZ);
      points.Insert(0, maxZ);

      // Compute center and convert it to world space.
      Vector3F localCenter = (min + max) / 2;
      boxCenter = boxOrientation * localCenter;

      return volume;
    }


    private static void TryToMakeIdentityMatrix(ref Matrix33F m)
    {
      // If the rotated space is parallel to the world space, then we want to use
      // the identity matrix directly.
      if (Numeric.AreEqual(1, Math.Abs(m.M00) + Math.Abs(m.M10) + Math.Abs(m.M20))      // Is the first column UnitX, UnitY or UnitZ?
          && Numeric.AreEqual(1, Math.Abs(m.M01) + Math.Abs(m.M11) + Math.Abs(m.M21)))  // Is the second column UnitX, UnitY or UnitZ?
      {
        m = Matrix33F.Identity;
      }
    }



    /// <summary>
    /// Gets the point on or in an axis-aligned bounding box (AABB) that is closest to a given 
    /// point.
    /// </summary>
    /// <param name="aabb">The AABB.</param>
    /// <param name="point">The point.</param>
    /// <param name="pointOnAabb">
    /// The point on or in <paramref name="aabb"/> that is closest to <paramref name="point"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="aabb"/> and <paramref name="point"/> have 
    /// contact (<paramref name="pointOnAabb"/> is identical to <paramref name="point"/>); 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool GetClosestPoint(Aabb aabb, Vector3F point, out Vector3F pointOnAabb)
    {
      // Short version: Fast when using SIMD instructions.
      //pointOnAabb = point;
      //pointOnAabb = Vector3F.Max(pointOnAabb, aabb.Minimum);
      //pointOnAabb = Vector3F.Min(pointOnAabb, aabb.Maximum);
      //return (point == pointOnAabb);

      bool haveContact = true;
      pointOnAabb = point;
      if (pointOnAabb.X < aabb.Minimum.X)
      {
        pointOnAabb.X = aabb.Minimum.X;
        haveContact = false;
      }
      else if (pointOnAabb.X > aabb.Maximum.X)
      {
        pointOnAabb.X = aabb.Maximum.X;
        haveContact = false;
      }
      if (pointOnAabb.Y < aabb.Minimum.Y)
      {
        pointOnAabb.Y = aabb.Minimum.Y;
        haveContact = false;
      }
      else if (pointOnAabb.Y > aabb.Maximum.Y)
      {
        pointOnAabb.Y = aabb.Maximum.Y;
        haveContact = false;
      }
      if (pointOnAabb.Z < aabb.Minimum.Z)
      {
        pointOnAabb.Z = aabb.Minimum.Z;
        haveContact = false;
      }
      else if (pointOnAabb.Z > aabb.Maximum.Z)
      {
        pointOnAabb.Z = aabb.Maximum.Z;
        haveContact = false;
      }

      return haveContact;
    }


    /// <overloads>
    /// <summary>
    /// Computes the distance between the two objects.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Computes the distance between the two axis-aligned bounding boxes (AABBs).
    /// </summary>
    /// <param name="aabbA">The first AABB.</param>
    /// <param name="aabbB">The second AABB.</param>
    /// <returns>
    /// The distance between the two AABBs. 0 if the AABB are touching or intersecting.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static float GetDistance(Aabb aabbA, Aabb aabbB)
    {
      return (float)Math.Sqrt(GetDistanceSquared(aabbA, aabbB));
    }


    /// <summary>
    /// Computes the squared distance between the two AABBs.
    /// </summary>
    /// <param name="aabbA">The first AABB.</param>
    /// <param name="aabbB">The second AABB.</param>
    /// <returns>
    /// The squared distance between the two AABBs.
    /// </returns>
    internal static float GetDistanceSquared(Aabb aabbA, Aabb aabbB)
    {
      float distanceSquared = 0;

      if (aabbA.Minimum.X > aabbB.Maximum.X)
      {
        float delta = aabbA.Minimum.X - aabbB.Maximum.X;
        distanceSquared += delta * delta;
      }
      else if (aabbB.Minimum.X > aabbA.Maximum.X)
      {
        float delta = aabbB.Minimum.X - aabbA.Maximum.X;
        distanceSquared += delta * delta;
      }

      if (aabbA.Minimum.Y > aabbB.Maximum.Y)
      {
        float delta = aabbA.Minimum.Y - aabbB.Maximum.Y;
        distanceSquared += delta * delta;
      }
      else if (aabbB.Minimum.Y > aabbA.Maximum.Y)
      {
        float delta = aabbB.Minimum.Y - aabbA.Maximum.Y;
        distanceSquared += delta * delta;
      }

      if (aabbA.Minimum.Z > aabbB.Maximum.Z)
      {
        float delta = aabbA.Minimum.Z - aabbB.Maximum.Z;
        distanceSquared += delta * delta;
      }
      else if (aabbB.Minimum.Z > aabbA.Maximum.Z)
      {
        float delta = aabbB.Minimum.Z - aabbA.Maximum.Z;
        distanceSquared += delta * delta;
      }

      return distanceSquared;
    }


    /// <summary>
    /// Computes a squared lower bound for the distance between the two oriented boxes.
    /// </summary>
    /// <param name="boxExtentA">The extent (the widths in x, y and z) of the first box.</param>
    /// <param name="poseA">The pose of the first box.</param>
    /// <param name="boxExtentB">The extent (the widths in x, y and z) of the second box.</param>
    /// <param name="poseB">The pose of second box.</param>
    /// <returns>The squared lower bound for the distance between the two oriented boxes.</returns>
    internal static float GetDistanceLowerBoundSquared(Vector3F boxExtentA, Pose poseA, Vector3F boxExtentB, Pose poseB)
    {
      Vector3F aToB = poseB.Position - poseA.Position;
      float distanceSquared = aToB.LengthSquared;
      if (Numeric.IsZero(distanceSquared))
        return 0;

      Vector3F closestPointOnB = poseB.ToWorldPosition(GetSupportPoint(boxExtentB, poseB.ToLocalDirection(-aToB)));
      Vector3F closestPointOnA = poseA.ToWorldPosition(GetSupportPoint(boxExtentA, poseA.ToLocalDirection(aToB)));
      Vector3F closestPointVector = closestPointOnB - closestPointOnA;

      // Use dot product to project closest-point pair vector onto center line.
      float dot = Vector3F.Dot(aToB, closestPointVector);

      // If dot is negative we can have contact.
      if (dot <= 0)
        return 0;

      // Compute rest of vector projection.
      return dot * dot / distanceSquared;
    }


    /// <summary>
    /// Gets a support point of a box for a given direction.
    /// </summary>
    /// <param name="boxExtent">The box extent (the widths in x, y and z).</param>
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
    private static Vector3F GetSupportPoint(Vector3F boxExtent, Vector3F direction)
    {
      Vector3F supportVertex = new Vector3F
      {
        X = ((direction.X >= 0) ? boxExtent.X / 2 : -boxExtent.X / 2),
        Y = ((direction.Y >= 0) ? boxExtent.Y / 2 : -boxExtent.Y / 2),
        Z = ((direction.Z >= 0) ? boxExtent.Z / 2 : -boxExtent.Z / 2)
      };
      return supportVertex;
    }


    /// <summary>
    /// Gets the box outcode of a point.
    /// </summary>
    /// <param name="boxExtent">The box extent.</param>
    /// <param name="point">The point position in the local box space.</param>
    /// <returns>The outcode.</returns>
    /// <remarks>
    /// The outcode is a code that contains information of the point position relative to the box.
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Bit 0 is set if the point is outside of the plane defined by the -x face of the box.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Bit 1 is set if the point is outside the +x plane.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Bit 2 is set if the point is outside the -y plane.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Bit 3 is set if the point is outside the +y plane.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Bit 4 is set if the point is outside the -z plane.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Bit 5 is set if the point is outside the +z plane.
    /// </description>
    /// </item>
    /// </list>
    /// If the outcode is 0, then the point is inside the box.
    /// </remarks>
    //[CLSCompliant(false)]  // CLSCompliant is only relevant for public members!
    internal static uint GetOutcode(Vector3F boxExtent, Vector3F point)
    {
      float halfExtentX = 0.5f * boxExtent.X;
      float halfExtentY = 0.5f * boxExtent.Y;
      float halfExtentZ = 0.5f * boxExtent.Z;
      uint outcode = 0;

      if (point.X < -halfExtentX)
        outcode |= 1;
      else if (point.X > halfExtentX)
        outcode |= 2;

      if (point.Y < -halfExtentY)
        outcode |= 4;
      else if (point.Y > halfExtentY)
        outcode |= 8;

      if (point.Z < -halfExtentZ)
        outcode |= 16;
      else if (point.Z > halfExtentZ)
        outcode |= 32;

      return outcode;
    }


    /// <summary>
    /// Determines whether two axis-aligned bounding boxes (AABBs) overlap.
    /// </summary>
    /// <param name="aabbA">The first axis-aligned bounding box (AABB).</param>
    /// <param name="aabbB">The second axis-aligned bounding box (AABB).</param>
    /// <returns>
    /// <see langword="true"/> if the AABBs overlap; otherwise <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabbA, Aabb aabbB)
    {
      // Note: The following check is safe if one AABB is undefined (NaN).
      // Do not change the comparison operator!
      return aabbA.Minimum.X <= aabbB.Maximum.X
             && aabbA.Maximum.X >= aabbB.Minimum.X
             && aabbA.Minimum.Y <= aabbB.Maximum.Y
             && aabbA.Maximum.Y >= aabbB.Minimum.Y
             && aabbA.Minimum.Z <= aabbB.Maximum.Z
             && aabbA.Maximum.Z >= aabbB.Minimum.Z;
    }


    /// <summary>
    /// Determines whether the axis-aligned bounding box (AABB) contains or touches the given point.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="point">The point.</param>
    /// <returns>
    /// <see langword="true"/> if AABB and the point have contact; otherwise 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabb, Vector3F point)
    {
      // Note: The following check is safe if one AABB is undefined (NaN).
      // Do not change the comparison operator!
      return aabb.Minimum.X <= point.X
             && aabb.Maximum.X >= point.X
             && aabb.Minimum.Y <= point.Y
             && aabb.Maximum.Y >= point.Y
             && aabb.Minimum.Z <= point.Z
             && aabb.Maximum.Z >= point.Z;
    }


    /// <overloads>
    /// <summary>
    /// Determines whether two primitives have contact.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the axis-aligned bounding box (AABB) and a box have contact.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="boxExtent">The box extent (the widths in x, y, and z).</param>
    /// <param name="boxPose">The pose of the box in the space of the AABB.</param>
    /// <param name="makeEdgeTests">
    /// If set to <see langword="true"/> the 9 edge-edge tests of the separating-axis-test (SAT) are 
    /// performed; otherwise, the edge-edge tests are left out and the returned value is 
    /// conservative, which means that a contact can be reported even if there is no contact.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the box are touching or penetrating; otherwise, 
    /// <see langword="false"/> if the object are separated.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabb, Vector3F boxExtent, Pose boxPose, bool makeEdgeTests)
    {
      Debug.Assert(boxExtent >= Vector3F.Zero, "Box extent must be positive.");

      // The following variables are in local space of a centered AABB.
      Vector3F cB = boxPose.Position - aabb.Center;            // Center of box.
      Matrix33F mB = boxPose.Orientation;                      // Orientation matrix of box.
      Matrix33F aMB = Matrix33F.Absolute(mB);                  // Absolute of mB.

      // Half extent vectors of AABB and box
      Vector3F eA = 0.5f * aabb.Extent;
      Vector3F eB = 0.5f * boxExtent;

      // ----- Separating Axis tests
      // See also: BoxBoxAlgorithm
      float separation;  // Separation distance.

      // Case 1: Separating Axis: (1, 0, 0)
      separation = Math.Abs(cB.X) - (eA.X + eB.X * aMB.M00 + eB.Y * aMB.M01 + eB.Z * aMB.M02);
      if (separation > 0)
        return false;

      // Case 2: Separating Axis: (0, 1, 0)
      separation = Math.Abs(cB.Y) - (eA.Y + eB.X * aMB.M10 + eB.Y * aMB.M11 + eB.Z * aMB.M12);
      if (separation > 0)
        return false;

      // Case 3: Separating Axis: (0, 0, 1) 
      separation = Math.Abs(cB.Z) - (eA.Z + eB.X * aMB.M20 + eB.Y * aMB.M21 + eB.Z * aMB.M22);
      if (separation > 0)
        return false;

      float ex; // Variable for an expression part.

      // Case 4: Separating Axis: OrientationB * (1, 0, 0) 
      ex = cB.X * mB.M00 + cB.Y * mB.M10 + cB.Z * mB.M20;
      separation = Math.Abs(ex) - (eB.X + eA.X * aMB.M00 + eA.Y * aMB.M10 + eA.Z * aMB.M20);
      if (separation > 0)
        return false;

      // Case 5: Separating Axis: OrientationB * (0, 1, 0) -----
      ex = cB.X * mB.M01 + cB.Y * mB.M11 + cB.Z * mB.M21;
      separation = Math.Abs(ex) - (eB.Y + eA.X * aMB.M01 + eA.Y * aMB.M11 + eA.Z * aMB.M21);
      if (separation > 0)
        return false;

      // Case 6: Separating Axis: OrientationB * (0, 0, 1) -----
      ex = cB.X * mB.M02 + cB.Y * mB.M12 + cB.Z * mB.M22;
      separation = Math.Abs(ex) - (eB.Z + eA.X * aMB.M02 + eA.Y * aMB.M12 + eA.Z * aMB.M22);
      if (separation > 0)
        return false;

      // ----- The next 9 tests are edge-edge cases. 
      if (makeEdgeTests == false)
        return true;

      // Case 7: Separating Axis: (1, 0, 0) x (OrientationB * (1, 0, 0))
      ex = cB.Z * mB.M10 - cB.Y * mB.M20;
      separation = Math.Abs(ex) - (eA.Y * aMB.M20 + eA.Z * aMB.M10 + eB.Y * aMB.M02 + eB.Z * aMB.M01);
      if (separation > 0)
        return false;

      // Case 8: Separating Axis: (1, 0, 0) x (OrientationB * (0, 1, 0))
      ex = cB.Z * mB.M11 - cB.Y * mB.M21;
      separation = Math.Abs(ex) - (eA.Y * aMB.M21 + eA.Z * aMB.M11 + eB.X * aMB.M02 + eB.Z * aMB.M00);
      if (separation > 0)
        return false;

      // Case 9: Separating Axis: (1, 0, 0) x (OrientationB * (0, 0, 1)) 
      ex = cB.Z * mB.M12 - cB.Y * mB.M22;
      separation = Math.Abs(ex) - (eA.Y * aMB.M22 + eA.Z * aMB.M12 + eB.X * aMB.M01 + eB.Y * aMB.M00);
      if (separation > 0)
        return false;

      // Case 10: Separating Axis: (0, 1, 0) x (OrientationB * (1, 0, 0)) 
      ex = cB.X * mB.M20 - cB.Z * mB.M00;
      separation = Math.Abs(ex) - (eA.X * aMB.M20 + eA.Z * aMB.M00 + eB.Y * aMB.M12 + eB.Z * aMB.M11);
      if (separation > 0)
        return false;

      // Case 11: Separating Axis: (0, 1, 0) x (OrientationB * (0, 1, 0)) 
      ex = cB.X * mB.M21 - cB.Z * mB.M01;
      separation = Math.Abs(ex) - (eA.X * aMB.M21 + eA.Z * aMB.M01 + eB.X * aMB.M12 + eB.Z * aMB.M10);
      if (separation > 0)
        return false;

      // Case 12: Separating Axis: (0, 1, 0) x (OrientationB * (0, 0, 1)) 
      ex = cB.X * mB.M22 - cB.Z * mB.M02;
      separation = Math.Abs(ex) - (eA.X * aMB.M22 + eA.Z * aMB.M02 + eB.X * aMB.M11 + eB.Y * aMB.M10);
      if (separation > 0)
        return false;

      // Case 13: Separating Axis: (0, 0, 1) x (OrientationB * (1, 0, 0)) 
      ex = cB.Y * mB.M00 - cB.X * mB.M10;
      separation = Math.Abs(ex) - (eA.X * aMB.M10 + eA.Y * aMB.M00 + eB.Y * aMB.M22 + eB.Z * aMB.M21);
      if (separation > 0)
        return false;

      // Case 14: Separating Axis: (0, 0, 1) x (OrientationB * (0, 1, 0)) 
      ex = cB.Y * mB.M01 - cB.X * mB.M11;
      separation = Math.Abs(ex) - (eA.X * aMB.M11 + eA.Y * aMB.M01 + eB.X * aMB.M22 + eB.Z * aMB.M20);
      if (separation > 0)
        return false;

      // Case 15: Separating Axis: (0, 0, 1) x (OrientationB * (0, 0, 1))
      ex = cB.Y * mB.M02 - cB.X * mB.M12;
      separation = Math.Abs(ex) - (eA.X * aMB.M12 + eA.Y * aMB.M02 + eB.X * aMB.M21 + eB.Y * aMB.M20);
      return (separation <= 0);
    }


    /// <summary>
    /// Determines whether the specified point is inside the box.
    /// </summary>
    /// <param name="boxExtent">The box extent (the widths in x, y, and z).</param>
    /// <param name="point">The point (in the local space of the box).</param>
    /// <returns>
    /// <see langword="true"/> if the specified point is inside; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HaveContact(Vector3F boxExtent, Vector3F point)
    {
      // To fight numerical problems: Extrude the box.
      Vector3F halfExtent = 0.5f * boxExtent;
      halfExtent = halfExtent + Vector3F.Max(halfExtent, new Vector3F(1)) * Numeric.EpsilonF;
      return point.X >= -halfExtent.X && point.X <= halfExtent.X
             && point.Y >= -halfExtent.Y && point.Y <= halfExtent.Y
             && point.Z >= -halfExtent.Z && point.Z <= halfExtent.Z;
    }
  }
}
