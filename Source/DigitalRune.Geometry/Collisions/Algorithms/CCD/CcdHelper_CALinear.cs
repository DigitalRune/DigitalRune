// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  internal static partial class CcdHelper
  {
    // See FAST paper: "Interactive Continuous Collision Detection for Non-Convex Polyhedra", Kim Young et al.

    /// <summary>
    /// Gets the time of impact using Conservative Advancement (ignoring rotational movement).
    /// </summary>
    /// <param name="objectA">The object A.</param>
    /// <param name="targetPoseA">The target pose of A.</param>
    /// <param name="objectB">The object B.</param>
    /// <param name="targetPoseB">The target pose of B.</param>
    /// <param name="allowedPenetration">The allowed penetration depth.</param>
    /// <param name="collisionDetection">The collision detection.</param>
    /// <returns>
    /// The time of impact in the range [0, 1].
    /// </returns>
    /// <remarks>
    /// This algorithm does not work for concave objects.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/>, <paramref name="objectB"/> or 
    /// <paramref name="collisionDetection"/> is <see langword="null"/>.
    /// </exception>
    internal static float GetTimeOfImpactLinearCA(CollisionObject objectA, Pose targetPoseA,
                                                  CollisionObject objectB, Pose targetPoseB, float allowedPenetration,
                                                  CollisionDetection collisionDetection)   // Required for collision algorithm matrix.
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");
      if (collisionDetection == null)
        throw new ArgumentNullException("collisionDetection");

      IGeometricObject geometricObjectA = objectA.GeometricObject;
      IGeometricObject geometricObjectB = objectB.GeometricObject;

      Pose startPoseA = geometricObjectA.Pose;
      Pose startPoseB = geometricObjectB.Pose;

      // Compute relative linear velocity.
      // (linearRelVel ∙ normal > 0 if objects are getting closer.)
      Vector3F linearVelocityA = targetPoseA.Position - startPoseA.Position;
      Vector3F linearVelocityB = targetPoseB.Position - startPoseB.Position;
      Vector3F linearVelocityRelative = linearVelocityA - linearVelocityB;

      // Abort if relative movement is zero.
      if (Numeric.IsZero(linearVelocityRelative.Length))
        return 1;

      var distanceAlgorithm = collisionDetection.AlgorithmMatrix[objectA, objectB];

      // Use temporary test objects.
      var testGeometricObjectA = TestGeometricObject.Create();
      testGeometricObjectA.Shape = geometricObjectA.Shape;
      testGeometricObjectA.Scale = geometricObjectA.Scale;
      testGeometricObjectA.Pose = startPoseA;

      var testGeometricObjectB = TestGeometricObject.Create();
      testGeometricObjectB.Shape = geometricObjectB.Shape;
      testGeometricObjectB.Scale = geometricObjectB.Scale;
      testGeometricObjectB.Pose = startPoseB;

      var testCollisionObjectA = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObjectA.SetInternal(objectA, testGeometricObjectA);

      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObjectB.SetInternal(objectB, testGeometricObjectB);

      var testContactSet = ContactSet.Create(testCollisionObjectA, testCollisionObjectB);

      try
      {
        distanceAlgorithm.UpdateClosestPoints(testContactSet, 0);

        if (testContactSet.Count < 0)
        {
          // No closest-distance result. --> Abort.
          return 1;
        }

        Vector3F normal = testContactSet[0].Normal;
        float distance = -testContactSet[0].PenetrationDepth;

        float λ = 0;
        float λPrevious = 0;

        for (int i = 0; i < MaxNumberOfIterations && distance > 0; i++)
        {
          // |v∙n|
          float velocityProjected = Vector3F.Dot(linearVelocityRelative, normal);

          // Abort for separating objects.
          if (Numeric.IsLess(velocityProjected, 0))
            break;

          // Increase TOI.
          float μ = (distance + allowedPenetration) / velocityProjected;
          λ = λ + μ;

          if (λ < 0 || λ > 1)
            break;

          Debug.Assert(λPrevious < λ);

          if (λ <= λPrevious)
            break;

          // Get new interpolated poses - only positions are changed.
          Vector3F positionA = startPoseA.Position + λ * (targetPoseA.Position - startPoseA.Position);
          testGeometricObjectA.Pose = new Pose(positionA, startPoseA.Orientation);

          Vector3F positionB = startPoseB.Position + λ * (targetPoseB.Position - startPoseB.Position);
          testGeometricObjectB.Pose = new Pose(positionB, startPoseB.Orientation);

          // Get new closest point distance.
          distanceAlgorithm.UpdateClosestPoints(testContactSet, 0);
          if (testContactSet.Count == 0)
            break;

          normal = testContactSet[0].Normal;
          distance = -testContactSet[0].PenetrationDepth;

          λPrevious = λ;
        }

        if (testContactSet.HaveContact && λ > 0 && λ < 1 && testContactSet.Count > 0)
        {
          return λ;
          // We already have a contact that we could use.
          // result.Contact = testContactSet[0];
        }
      }
      finally
      {
        // Recycle temporary objects.
        testContactSet.Recycle(true);
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectA);
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
        testGeometricObjectA.Recycle();
        testGeometricObjectB.Recycle();
      }

      return 1;
    }
  }
}
