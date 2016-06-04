// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="RayShape"/> vs. 
  /// <see cref="CompositeShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// This algorithm will call other algorithms to compute collision of child shapes.
  /// </remarks>
  public class RayCompositeAlgorithm : CollisionAlgorithm
  {
    // TODO: Possible optimizations:
    // - Closest point queries could be made faster with explicit ray vs. AABB checks.
    // - Contact queries could be made faster by checking if the ray vs. AABB hit is closer
    //   than the current best hit.

    private CompositeShapeAlgorithm _compositeAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="RayCompositeAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayCompositeAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      _compositeAlgorithm = new CompositeShapeAlgorithm(collisionDetection);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Just use normal composite shape algorithm.
        _compositeAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      Debug.Assert(type != CollisionQueryType.ClosestPoints, "Closest point queries should have already been handled!");

      // Composite = A, Ray = B
      IGeometricObject compositeObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject rayObject = contactSet.ObjectB.GeometricObject;

      // Object A should be the composite, swap objects if necessary.
      bool swapped = (compositeObject.Shape is RayShape);
      if (swapped)
        MathHelper.Swap(ref rayObject, ref compositeObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      CompositeShape compositeShape = compositeObject.Shape as CompositeShape;

      // Check if shapes are correct.
      if (rayShape == null || compositeShape == null)
        throw new ArgumentException("The contact set must contain a ray and a composite shape.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      // Get transformations.
      Vector3F rayScale = rayObject.Scale;
      Pose rayPose = rayObject.Pose;
      Vector3F compositeScale = compositeObject.Scale;
      Pose compositePose = compositeObject.Pose;

      // Check if transforms are supported.
      // Same check for object B.
      if (compositeShape != null
          && (compositeScale.X != compositeScale.Y || compositeScale.Y != compositeScale.Z)
          && compositeShape.Children.Any(child => child.Pose.HasRotation))  // Note: Any() creates garbage, but non-uniform scalings should not be used anyway.
      {
        throw new NotSupportedException("Computing collisions for composite shapes with non-uniform scaling and rotated children is not supported.");
      }

      // ----- A few fixed objects which are reused to avoid GC garbage.
      var testCollisionObject = ResourcePools.TestCollisionObjects.Obtain();
      var testGeometricObject = TestGeometricObject.Create();

      // Create a test contact set and initialize with dummy objects.
      // (The actual collision objects are set below.)
      var testContactSet = ContactSet.Create(testCollisionObject, contactSet.ObjectB);  // Dummy arguments! They are changed later.
      
      // Scale ray and transform ray to local unscaled space of composite.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);     // Scale ray.
      rayWorld.ToWorld(ref rayPose);    // Transform ray to world space.
      Ray ray = rayWorld;
      ray.ToLocal(ref compositePose);   // Transform ray to local space of composite.
      var inverseCompositeScale = Vector3F.One / compositeScale;
      ray.Scale(ref inverseCompositeScale);

      try
      {
        if (compositeShape.Partition != null)
        {
          #region ----- Composite with BVH vs. * -----

          foreach (var childIndex in compositeShape.Partition.GetOverlaps(ray))
          {
            if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
              break; // We can abort early.

            AddChildContacts(
              contactSet,
              swapped,
              childIndex,
              type,
              testContactSet,
              testCollisionObject,
              testGeometricObject);
          }
          #endregion
        }
        else
        {
          #region ----- Composite vs. *-----

          var rayDirectionInverse = new Vector3F(
            1 / ray.Direction.X,
            1 / ray.Direction.Y,
            1 / ray.Direction.Z);

          float epsilon = Numeric.EpsilonF * (1 + compositeObject.Aabb.Extent.Length);

          // Go through list of children and find contacts.
          int numberOfChildGeometries = compositeShape.Children.Count;
          for (int i = 0; i < numberOfChildGeometries; i++)
          {
            IGeometricObject child = compositeShape.Children[i];

            if (GeometryHelper.HaveContact(child.Shape.GetAabb(child.Scale, child.Pose), ray.Origin, rayDirectionInverse, ray.Length, epsilon))
            {
              AddChildContacts(
                contactSet,
                swapped,
                i,
                type,
                testContactSet,
                testCollisionObject,
                testGeometricObject);

              // We have contact and stop for boolean queries.
              if (contactSet.HaveContact && type == CollisionQueryType.Boolean)
                break;
            }
          }
          #endregion
        }
      }
      finally
      {
        Debug.Assert(compositeObject.Shape == compositeShape, "Shape was altered and not restored.");

        testContactSet.Recycle();
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObject);
        testGeometricObject.Recycle();
      }
    }


    // Compute contacts between a shape and the child shapes of a <see cref="CompositeShape"/>.
    // testXxx are initialized objects which are re-used to avoid a lot of GC garbage.
    private void AddChildContacts(ContactSet contactSet,
                                  bool swapped,
                                  int childIndex,
                                  CollisionQueryType type,
                                  ContactSet testContactSet,
                                  CollisionObject testCollisionObject,
                                  TestGeometricObject testGeometricObject)
    {
      // This method is taken from CompositeShapeAlgorithm.cs and slightly modified. Keep changes
      // in sync with CompositeShapeAlgorithm.cs!

      // !!! Object A should be the composite. - This is different then in ComputeContacts() above!!!
      CollisionObject collisionObjectA = (swapped) ? contactSet.ObjectB : contactSet.ObjectA;
      CollisionObject collisionObjectB = (swapped) ? contactSet.ObjectA : contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      Vector3F scaleA = geometricObjectA.Scale;
      IGeometricObject childA = ((CompositeShape)geometricObjectA.Shape).Children[childIndex];

      // Find collision algorithm. 
      CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[childA, geometricObjectB];

      // ----- Set the shape temporarily to the current child.
      // (Note: The scaling is either uniform or the child has no local rotation. Therefore, we only
      // need to apply the scale of the parent to the scale and translation of the child. We can 
      // ignore the rotation.)
      Debug.Assert(
        (scaleA.X == scaleA.Y && scaleA.Y == scaleA.Z) || !childA.Pose.HasRotation,
        "CompositeShapeAlgorithm should have thrown an exception. Non-uniform scaling is not supported for rotated children.");

      var childPose = childA.Pose;
      childPose.Position *= scaleA;                                  // Apply scaling to local translation.
      testGeometricObject.Pose = geometricObjectA.Pose * childPose;
      testGeometricObject.Shape = childA.Shape;
      testGeometricObject.Scale = scaleA * childA.Scale;             // Apply scaling to local scale.

      testCollisionObject.SetInternal(collisionObjectA, testGeometricObject);

      // Create a temporary contact set. 
      // (ObjectA and ObjectB should have the same order as in contactSet; otherwise we couldn't 
      // simply merge them.)
      Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");
      if (swapped)
        testContactSet.Reset(collisionObjectB, testCollisionObject);
      else
        testContactSet.Reset(testCollisionObject, collisionObjectB);

      if (type == CollisionQueryType.Boolean)
      {
        // Boolean queries.
        collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Boolean);
        contactSet.HaveContact = (contactSet.HaveContact || testContactSet.HaveContact);
      }
      else
      {
        // No perturbation test. Most composite shapes are either complex and automatically
        // have more contacts. Or they are complex and will not be used for stacking
        // where full contact sets would be needed.
        testContactSet.IsPerturbationTestAllowed = false;

        // Make collision check. As soon as we have found contact, we can make faster
        // contact queries instead of closest-point queries.
        CollisionQueryType queryType = (contactSet.HaveContact) ? CollisionQueryType.Contacts : type;
        collisionAlgorithm.ComputeCollision(testContactSet, queryType);
        contactSet.HaveContact = (contactSet.HaveContact || testContactSet.HaveContact);

        // Transform contacts into space of composite shape.
        // And set the shape feature of the contact.
        int numberOfContacts = testContactSet.Count;
        for (int i = 0; i < numberOfContacts; i++)
        {
          Contact contact = testContactSet[i];
          if (swapped)
          {
            contact.PositionBLocal = childPose.ToWorldPosition(contact.PositionBLocal);
            contact.FeatureB = childIndex;
          }
          else
          {
            contact.PositionALocal = childPose.ToWorldPosition(contact.PositionALocal);
            contact.FeatureA = childIndex;
          }
        }

        // Merge child contacts.
        ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);
      }
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      return _compositeAlgorithm.GetTimeOfImpact(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
    }
  }
}
