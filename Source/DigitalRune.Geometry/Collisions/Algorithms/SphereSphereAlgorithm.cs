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
  /// Computes contact or closest-point information for two <see cref="SphereShape"/>s.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class SphereSphereAlgorithm : CollisionAlgorithm
  {
    // Non-uniformly scaled spheres are not handled by this algorithm. We use 
    // a convex-convex algorithm as fallback.
    private CollisionAlgorithm _fallbackAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="SphereSphereAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public SphereSphereAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain two <see cref="SphereShape"/> shapes.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      IGeometricObject sphereObjectA = contactSet.ObjectA.GeometricObject;
      IGeometricObject sphereObjectB = contactSet.ObjectB.GeometricObject;
      SphereShape sphereShapeA = sphereObjectA.Shape as SphereShape;
      SphereShape sphereShapeB = sphereObjectB.Shape as SphereShape;

      // Check if collision objects are spheres.
      if (sphereShapeA == null || sphereShapeB == null)
        throw new ArgumentException("The contact set must contain sphere shapes.", "contactSet");

      Vector3F scaleA = Vector3F.Absolute(sphereObjectA.Scale);
      Vector3F scaleB = Vector3F.Absolute(sphereObjectB.Scale);

      // Call MPR for non-uniformly scaled spheres.
      if (scaleA.X != scaleA.Y || scaleA.Y != scaleA.Z 
          || scaleB.X != scaleB.Y || scaleB.Y != scaleB.Z)
      {
        if (_fallbackAlgorithm == null)
          _fallbackAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(ConvexShape), typeof(ConvexShape)];

        _fallbackAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      // Apply uniform scale.
      float radiusA = sphereShapeA.Radius * scaleA.X;
      float radiusB = sphereShapeB.Radius * scaleB.X;

      // Vector from center of A to center of B.
      Vector3F centerA = sphereObjectA.Pose.Position;
      Vector3F centerB = sphereObjectB.Pose.Position;
      Vector3F aToB = centerB - centerA; 
      float lengthAToB = aToB.Length;

      // Check radius of spheres.
      float penetrationDepth = radiusA + radiusB - lengthAToB;
      contactSet.HaveContact = penetrationDepth >= 0;

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // ----- Create contact information.
      Vector3F normal;
      if (Numeric.IsZero(lengthAToB))
      {
        // Spheres are on the same position, we can choose any normal vector.
        // Possibly it would be better to consider the object movement (velocities), but 
        // it is not important since this case should be VERY rare.
        normal = Vector3F.UnitY;
      }
      else
      {
        normal = aToB.Normalized;
      }

      // The contact point lies in the middle of the intersecting volume.
      Vector3F position = centerA + normal * (radiusA - penetrationDepth / 2);

      // Update contact set.
      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }
  }
}
