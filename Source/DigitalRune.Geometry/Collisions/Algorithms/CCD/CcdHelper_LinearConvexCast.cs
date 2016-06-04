// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  internal static partial class CcdHelper
  {
    // This method performs ray casting on the CSO. The ray casting code is similar to the
    // RayConvexAlgorithm code and also described in the paper:
    //    Gino van den Bergen “Ray Casting against General Convex Objects with 
    //    Application to Continuous Collision Detection”, GDC 2005. www.dtecta.com 
    // When a new hit point on the ray is found, the object A and B are moved instead
    // of selecting a new hit point on the ray. - So the CSO is always tested against the
    // origin and the CSO is deforming. See this paper:
    // http://www.continuousphysics.com/BulletContinuousCollisionDetection.pdf
    // (TODO: Here we only consider linear movement. But the method in the paper should also work for rotating objects!)


    /// <summary>
    /// Gets the time of impact of the linear sweeps of both objects (ignoring rotational movement).
    /// </summary>
    /// <param name="objectA">The object A.</param>
    /// <param name="targetPoseA">The target pose of A.</param>
    /// <param name="objectB">The object B.</param>
    /// <param name="targetPoseB">The target pose of B.</param>
    /// <param name="allowedPenetration">The allowed penetration.</param>
    /// <returns>The time of impact in the range [0, 1].</returns>
    /// <remarks>
    /// Both objects are moved from the current positions to their target positions. Angular
    /// movement is ignored. 
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="objectA"/> and <paramref name="objectB"/> are not convex shapes.
    /// </exception>
    internal static float GetTimeOfImpactLinearSweep(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      IGeometricObject geometricObjectA = objectA.GeometricObject;
      IGeometricObject geometricObjectB = objectB.GeometricObject;

      ConvexShape convexA = geometricObjectA.Shape as ConvexShape;
      ConvexShape convexB = geometricObjectB.Shape as ConvexShape;

      // Check if shapes are correct.
      if (convexA == null || convexB == null)
        throw new ArgumentException("objectA and objectB must have convex shapes.");

      Pose startPoseA = geometricObjectA.Pose;
      Pose startPoseB = geometricObjectB.Pose;

      // Compute relative linear velocity.
      Vector3F linearVelocityA = targetPoseA.Position - startPoseA.Position;
      Vector3F linearVelocityB = targetPoseB.Position - startPoseB.Position;
      Vector3F linearVelocityRelative = linearVelocityA - linearVelocityB;

      // Abort if relative movement is zero.
      float linearVelocityRelativeMagnitude = linearVelocityRelative.Length;
      if (Numeric.IsZero(linearVelocityRelativeMagnitude))
        return 1;

      // Get a simplex solver from a resource pool.
      var simplex = GjkSimplexSolver.Create();

      try
      {
        Vector3F scaleA = geometricObjectA.Scale;
        Vector3F scaleB = geometricObjectB.Scale;

        Pose poseA = startPoseA;
        Pose poseB = startPoseB;

        Vector3F r = linearVelocityRelative;        // ray
        float λ = 0;                                // ray parameter
        //Vector3F n = new Vector3F();              // normal

        // First point on the CSO.
        Vector3F supportA = poseA.ToWorldPosition(convexA.GetSupportPoint(poseA.ToLocalDirection(-r), scaleA));
        Vector3F supportB = poseB.ToWorldPosition(convexB.GetSupportPoint(poseB.ToLocalDirection(r), scaleB));
        Vector3F v = supportA - supportB;

        float distanceSquared = v.LengthSquared;    // ||v||²
        int iterationCount = 0;
        while (distanceSquared > Numeric.EpsilonF   // We could use a higher EpsilonF to abort earlier.
               && iterationCount < MaxNumberOfIterations)
        {
          iterationCount++;

          // To get a contact at the TOI, the objects are shrunk by an amount proportional to the
          // allowed penetration. Therefore we need this values:
          Vector3F vNormalized = v;
          vNormalized.TryNormalize();
          Vector3F vA = poseA.ToLocalDirection(-vNormalized);
          Vector3F vB = poseB.ToLocalDirection(vNormalized);

          // Get support point on each object and subtract the half allowed penetration.
          supportA = poseA.ToWorldPosition(convexA.GetSupportPoint(vA, scaleA) - 0.5f * vA * allowedPenetration);
          supportB = poseB.ToWorldPosition(convexB.GetSupportPoint(vB, scaleB) - 0.5f * vB * allowedPenetration);

          // The new CSO point.
          Vector3F w = supportA - supportB;

          float vDotW = Vector3F.Dot(v, w);         // v∙w
          if (vDotW > 0)
          {
            float vDotR = Vector3F.Dot(v, r);       // v∙r
            if (vDotR >= -Numeric.EpsilonF)         // TODO: vDotR >= -Epsilon^2 ?
              return 1;                             // No Hit. 

            λ = λ - vDotW / vDotR;

            // Instead of moving the hit point on the ray, we move the objects, so that
            // the hit point stays at the origin. - See Erwin Coumans' Bullet paper.
            //x = λ * r;
            //simplex.Clear();  // Configuration space obstacle (CSO) is translated whenever x is updated.

            poseA.Position = startPoseA.Position + λ * (targetPoseA.Position - startPoseA.Position);
            poseB.Position = startPoseB.Position + λ * (targetPoseB.Position - startPoseB.Position);

            //n = v;
          }

          if (!simplex.Contains(w))
            simplex.Add(w, supportA, supportB);

          simplex.Update();
          v = simplex.ClosestPoint;
          distanceSquared = (simplex.IsValid && !simplex.IsFull) ? v.LengthSquared : 0;
        }

        // We have a contact if the hit is inside the ray length.
        if (0 < λ && λ <= 1)
        {
          return λ;
          // This would be a contact, but the local contact positions would not be optimal.
          //result.Contact = ContactHelper.CreateContact(objectA, objectB, simplex.ClosestPoint, -n.Normalized, 0, false);
        }
      }
      finally
      {
        // Recycle temporary heap objects.
        simplex.Recycle();
      }

      return 1;
    }
  }
}
