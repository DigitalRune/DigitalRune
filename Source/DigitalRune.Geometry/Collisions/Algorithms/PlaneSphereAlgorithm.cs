// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="SphereShape"/> vs. 
  /// <see cref="PlaneShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class PlaneSphereAlgorithm : CollisionAlgorithm
  {
    // Non-uniformly scaled spheres are not handled by this algorithm. We use 
    // a plane-convex algorithm as fallback.
    private CollisionAlgorithm _fallbackAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneSphereAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public PlaneSphereAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="PlaneShape"/> and 
    /// <see cref="SphereShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object A should be the plane.
      // Object B should be the sphere.
      IGeometricObject planeObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject sphereObject = contactSet.ObjectB.GeometricObject;

      // A should be the plane, swap objects if necessary.
      bool swapped = (sphereObject.Shape is PlaneShape);
      if (swapped)
        MathHelper.Swap(ref planeObject, ref sphereObject);

      PlaneShape planeShape = planeObject.Shape as PlaneShape;
      SphereShape sphereShape = sphereObject.Shape as SphereShape;

      // Check if collision object shapes are correct.
      if (planeShape == null || sphereShape == null)
        throw new ArgumentException("The contact set must contain a plane and a sphere.", "contactSet");

      // Get scalings.
      Vector3F planeScale = planeObject.Scale;
      Vector3F sphereScale = Vector3F.Absolute(sphereObject.Scale);

      // Call other algorithm for non-uniformly scaled spheres.
      if (sphereScale.X != sphereScale.Y || sphereScale.Y != sphereScale.Z)
      {
        if (_fallbackAlgorithm == null)
          _fallbackAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(PlaneShape), typeof(ConvexShape)];

        _fallbackAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      // Get poses.
      Pose planePose = planeObject.Pose;
      Pose spherePose = sphereObject.Pose;

      // Apply scaling to plane and transform plane to world space.
      Plane plane = new Plane(planeShape);
      plane.Scale(ref planeScale);           // Scale plane.
      plane.ToWorld(ref planePose);          // Transform plane to world space.

      // Calculate distance from plane to sphere surface.
      float sphereRadius = sphereShape.Radius * sphereScale.X;
      Vector3F sphereCenter = spherePose.Position;
      float planeToSphereDistance = Vector3F.Dot(sphereCenter, plane.Normal) - sphereRadius - plane.DistanceFromOrigin;

      float penetrationDepth = -planeToSphereDistance;
      contactSet.HaveContact = (penetrationDepth >= 0);

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // Compute contact details.
      Vector3F position = sphereCenter - plane.Normal * (sphereRadius - penetrationDepth / 2);
      Vector3F normal = (swapped) ? -plane.Normal : plane.Normal;

      // Update contact set.
      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      // We can use conservative advancement because a plane is convex enough so it will work.
      // We use a linear test because usually planes do not rotate.
      return CcdHelper.GetTimeOfImpactLinearCA(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration, CollisionDetection);
    }
  }
}
