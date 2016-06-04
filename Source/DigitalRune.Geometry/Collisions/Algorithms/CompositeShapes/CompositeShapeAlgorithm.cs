// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="CompositeShape"/> vs. any other 
  /// <see cref="Shape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes. This
  /// algorithm will call other algorithms to compute collision of child shapes.
  /// </remarks>
  public partial class CompositeShapeAlgorithm : CollisionAlgorithm
  {
    private static readonly ResourcePool<ClosestPointCallback> ClosestPointsCallbacks =
      new ResourcePool<ClosestPointCallback>(
        () => new ClosestPointCallback(),
        null,
        callback =>
        {
          callback.CollisionAlgorithm = null;
          callback.ContactSet = null;
          callback.Swapped = false;
          callback.TestContactSet = null;
          callback.TestGeometricObjectA = null;
          callback.TestGeometricObjectB = null;
        });


    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeShapeAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public CompositeShapeAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="CompositeShape"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="contactSet"/> contains a <see cref="CompositeShape"/> with a non-uniform
    /// scaling. One of its children has a local rotation. Computing collisions for composite shapes
    /// with non-uniform scaling and rotated children is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;

      // Object A should be the composite, swap objects if necessary.
      // When testing CompositeShape vs. CompositeShape with BVH, object A should be the 
      // CompositeShape with BVH.
      CompositeShape compositeShapeA = geometricObjectA.Shape as CompositeShape;
      CompositeShape compositeShapeB = geometricObjectB.Shape as CompositeShape;
      bool swapped = false;
      if (compositeShapeA == null)
      {
        // Object A is something else. Object B must be a composite shape.
        swapped = true;
      }
      else if (compositeShapeA.Partition == null 
               && compositeShapeB != null 
               && compositeShapeB.Partition != null)
      {
        // Object A has no BVH, object B is CompositeShape with BVH.
        swapped = true;
      }

      if (swapped)
      {
        MathHelper.Swap(ref collisionObjectA, ref collisionObjectB);
        MathHelper.Swap(ref geometricObjectA, ref geometricObjectB);
        MathHelper.Swap(ref compositeShapeA, ref compositeShapeB);
      }

      // Check if collision objects shapes are correct.
      if (compositeShapeA == null)
        throw new ArgumentException("The contact set must contain a composite shape.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;

      // Check if transforms are supported.
      if (compositeShapeA != null                                           // When object A is a CompositeShape
          && (scaleA.X != scaleA.Y || scaleA.Y != scaleA.Z)                 // non-uniform scaling is not supported
          && compositeShapeA.Children.Any(child => child.Pose.HasRotation)) // when a child has a local rotation.
      {                                                                     // Note: Any() creates garbage, but non-uniform scalings should not be used anyway.
        throw new NotSupportedException("Computing collisions for composite shapes with non-uniform scaling and rotated children is not supported.");
      }

      // Same check for object B.
      if (compositeShapeB != null
          && (scaleB.X != scaleB.Y || scaleB.Y != scaleB.Z)
          && compositeShapeB.Children.Any(child => child.Pose.HasRotation)) // Note: Any() creates garbage, but non-uniform scalings should not be used anyway.
      {
        throw new NotSupportedException("Computing collisions for composite shapes with non-uniform scaling and rotated children is not supported.");
      }

      // ----- A few fixed objects which are reused to avoid GC garbage.
      var testCollisionObjectA = ResourcePools.TestCollisionObjects.Obtain();
      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();

      // Create a test contact set and initialize with dummy objects.
      // (The actual collision objects are set below.)
      var testContactSet = ContactSet.Create(testCollisionObjectA, testCollisionObjectB);
      var testGeometricObjectA = TestGeometricObject.Create();
      var testGeometricObjectB = TestGeometricObject.Create();

      try
      {
        if (compositeShapeA.Partition != null
            && (type != CollisionQueryType.ClosestPoints
                || compositeShapeA.Partition is ISupportClosestPointQueries<int>))
        {
          if (compositeShapeB != null && compositeShapeB.Partition != null)
          {
            #region ----- Composite with BVH vs. Composite with BVH -----

            Debug.Assert(swapped == false, "Why did we swap the objects? Order of objects is fine.");

            if (type != CollisionQueryType.ClosestPoints)
            {
              // ----- Boolean or Contact Query

              // Heuristic: Test large BVH vs. small BVH.
              Aabb aabbOfA = geometricObjectA.Aabb;
              Aabb aabbOfB = geometricObjectB.Aabb;
              float largestExtentA = aabbOfA.Extent.LargestComponent;
              float largestExtentB = aabbOfB.Extent.LargestComponent;
              IEnumerable<Pair<int>> overlaps;
              bool overlapsSwapped = largestExtentA < largestExtentB;
              if (overlapsSwapped)
              {
                overlaps = compositeShapeB.Partition.GetOverlaps(
                  scaleB,
                  geometricObjectB.Pose,
                  compositeShapeA.Partition,
                  scaleA,
                  geometricObjectA.Pose);
              }
              else
              {
                overlaps = compositeShapeA.Partition.GetOverlaps(
                  scaleA,
                  geometricObjectA.Pose,
                  compositeShapeB.Partition,
                  scaleB,
                  geometricObjectB.Pose);
              }

              foreach (var overlap in overlaps)
              {
                if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
                  break; // We can abort early.

                AddChildChildContacts(
                  contactSet,
                  overlapsSwapped ? overlap.Second : overlap.First,
                  overlapsSwapped ? overlap.First : overlap.Second,
                  type,
                  testContactSet,
                  testCollisionObjectA,
                  testGeometricObjectA,
                  testCollisionObjectB,
                  testGeometricObjectB);
              }
            }
            else
            {
              // Closest-Point Query

              var callback = ClosestPointsCallbacks.Obtain();
              callback.CollisionAlgorithm = this;
              callback.Swapped = false;
              callback.ContactSet = contactSet;
              callback.TestCollisionObjectA = testCollisionObjectA;
              callback.TestCollisionObjectB = testCollisionObjectB;
              callback.TestGeometricObjectA = testGeometricObjectA;
              callback.TestGeometricObjectB = testGeometricObjectB;
              callback.TestContactSet = testContactSet;

              ((ISupportClosestPointQueries<int>)compositeShapeA.Partition)
                .GetClosestPointCandidates(
                  scaleA,
                  geometricObjectA.Pose,
                  compositeShapeB.Partition,
                  scaleB,
                  geometricObjectB.Pose,
                  callback.HandlePair);

              ClosestPointsCallbacks.Recycle(callback);
            }
            #endregion
          }
          else
          {
            #region ----- Composite with BVH vs. * -----

            // Compute AABB of B in local space of the CompositeShape.
            Aabb aabbBInA = geometricObjectB.Shape.GetAabb(
              scaleB, geometricObjectA.Pose.Inverse * geometricObjectB.Pose);

            // Apply inverse scaling to do the AABB checks in the unscaled local space of A.
            aabbBInA.Scale(Vector3F.One / scaleA);

            if (type != CollisionQueryType.ClosestPoints)
            {
              // Boolean or Contact Query

              foreach (var childIndex in compositeShapeA.Partition.GetOverlaps(aabbBInA))
              {
                if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
                  break; // We can abort early.

                AddChildContacts(
                  contactSet, 
                  swapped, 
                  childIndex, 
                  type, 
                  testContactSet, 
                  testCollisionObjectA,
                  testGeometricObjectA);
              }
            }
            else if (type == CollisionQueryType.ClosestPoints)
            {
              // Closest-Point Query

              var callback = ClosestPointsCallbacks.Obtain();
              callback.CollisionAlgorithm = this;
              callback.Swapped = swapped;
              callback.ContactSet = contactSet;
              callback.TestCollisionObjectA = testCollisionObjectA;
              callback.TestCollisionObjectB = testCollisionObjectB;
              callback.TestGeometricObjectA = testGeometricObjectA;
              callback.TestGeometricObjectB = testGeometricObjectB;
              callback.TestContactSet = testContactSet;

              ((ISupportClosestPointQueries<int>)compositeShapeA.Partition)
                .GetClosestPointCandidates(
                  aabbBInA,
                  float.PositiveInfinity,
                  callback.HandleItem);

              ClosestPointsCallbacks.Recycle(callback);
            }
            #endregion
          }
        }
        else
        {
          #region ----- Composite vs. *-----

          // Compute AABB of B in local space of the composite.
          Aabb aabbBInA = geometricObjectB.Shape.GetAabb(scaleB, geometricObjectA.Pose.Inverse * geometricObjectB.Pose);
          
          // Apply inverse scaling to do the AABB checks in the unscaled local space of A.
          aabbBInA.Scale(Vector3F.One / scaleA);

          // Go through list of children and find contacts.
          int numberOfChildGeometries = compositeShapeA.Children.Count;
          for (int i = 0; i < numberOfChildGeometries; i++)
          {
            IGeometricObject child = compositeShapeA.Children[i];

            // NOTE: For closest-point queries we could be faster estimating a search space.
            // See TriangleMeshAlgorithm or BVH queries.
            // But the current implementation is sufficient. If the CompositeShape is more complex
            // the user should be using spatial partitions anyway.

            // For boolean or contact queries, we make an AABB test first.
            // For closest points where we have not found a contact yet, we have to search
            // all children.
            if ((type == CollisionQueryType.ClosestPoints && !contactSet.HaveContact)
                || GeometryHelper.HaveContact(aabbBInA, child.Shape.GetAabb(child.Scale, child.Pose)))
            {
              // TODO: We could compute the minDistance of the child AABB and the AABB of the
              // other shape. If the minDistance is greater than the current closestPairDistance
              // we can ignore this pair. - This could be a performance boost.

              // Get contacts/closest pairs of this child.
              AddChildContacts(
                contactSet, 
                swapped, 
                i, 
                type, 
                testContactSet,
                testCollisionObjectA,
                testGeometricObjectA);

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
        Debug.Assert(collisionObjectA.GeometricObject.Shape == compositeShapeA, "Shape was altered and not restored.");

        testContactSet.Recycle();
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectA);
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
        testGeometricObjectB.Recycle();
        testGeometricObjectA.Recycle();
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
      // This method is also used in RayCompositeAlgorithm.cs. Keep changes in sync!

      // Object A should be the CompositeShape.
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

        // TODO: We could add existing contacts with the same child shape to childContactSet.
        // Collision algorithms could take advantage of existing contact information to speed up
        // calculations. However, at the moment the collision algorithms ignore existing contacts.
        // If we add the exiting contacts to childContactSet we need to uncomment the comment
        // code lines below.

        // Transform contacts into space of child shape. 
        //foreach (Contact c in childContactSet)
        //{
        //  if (childContactSet.ObjectA == childCollisionObject)
        //    c.PositionALocal = childPose.ToLocalPosition(c.PositionALocal);
        //  else
        //    c.PositionBLocal = childPose.ToLocalPosition(c.PositionBLocal);
        //}

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
            //if (contact.Lifetime.Ticks == 0) // Currently, all contacts are new, so this check is not necessary.
            //{
            contact.FeatureB = childIndex;
            //}
          }
          else
          {
            contact.PositionALocal = childPose.ToWorldPosition(contact.PositionALocal);
            //if (contact.Lifetime.Ticks == 0) // Currently, all contacts are new, so this check is not necessary.
            //{
            contact.FeatureA = childIndex;
            //}
          }
        }

        // Merge child contacts.
        ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);
      }
    }


    // Compute contacts between child shapes of two <see cref="CompositeShape"/>s.
    // testXxx are initialized objects which are re-used to avoid a lot of GC garbage.
    private void AddChildChildContacts(ContactSet contactSet, 
                                       int childIndexA, 
                                       int childIndexB, 
                                       CollisionQueryType type,
                                       ContactSet testContactSet, 
                                       CollisionObject testCollisionObjectA,
                                       TestGeometricObject testGeometricObjectA,
                                       CollisionObject testCollisionObjectB,
                                       TestGeometricObject testGeometricObjectB)
    {
      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      CompositeShape shapeA = (CompositeShape)geometricObjectA.Shape;
      CompositeShape shapeB = (CompositeShape)geometricObjectB.Shape;
      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;
      IGeometricObject childA = shapeA.Children[childIndexA];
      IGeometricObject childB = shapeB.Children[childIndexB];

      // Find collision algorithm. 
      CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[childA, childB];

      // ----- Set the shape temporarily to the current children.
      // (Note: The scaling is either uniform or the child has no local rotation. Therefore, we only
      // need to apply the scale of the parent to the scale and translation of the child. We can 
      // ignore the rotation.)
      Debug.Assert(
        (scaleA.X == scaleA.Y && scaleA.Y == scaleA.Z) || !childA.Pose.HasRotation,
        "CompositeShapeAlgorithm should have thrown an exception. Non-uniform scaling is not supported for rotated children.");
      Debug.Assert(
        (scaleB.X == scaleB.Y && scaleB.Y == scaleB.Z) || !childB.Pose.HasRotation,
        "CompositeShapeAlgorithm should have thrown an exception. Non-uniform scaling is not supported for rotated children.");

      var childAPose = childA.Pose;
      childAPose.Position *= scaleA;                                  // Apply scaling to local translation.
      testGeometricObjectA.Pose = geometricObjectA.Pose * childAPose;
      testGeometricObjectA.Shape = childA.Shape;
      testGeometricObjectA.Scale = scaleA * childA.Scale;             // Apply scaling to local scale.

      testCollisionObjectA.SetInternal(collisionObjectA, testGeometricObjectA);

      var childBPose = childB.Pose;
      childBPose.Position *= scaleB;                                  // Apply scaling to local translation.
      testGeometricObjectB.Pose = geometricObjectB.Pose * childBPose;
      testGeometricObjectB.Shape = childB.Shape;
      testGeometricObjectB.Scale = scaleB * childB.Scale;             // Apply scaling to local scale.

      testCollisionObjectB.SetInternal(collisionObjectB, testGeometricObjectB);

      Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");
      testContactSet.Reset(testCollisionObjectA, testCollisionObjectB);

      if (type == CollisionQueryType.Boolean)
      {
        // Boolean queries.
        collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Boolean);
        contactSet.HaveContact = (contactSet.HaveContact || testContactSet.HaveContact);
      }
      else
      {
        // TODO: We could add existing contacts with the same child shape to childContactSet.

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

          contact.PositionALocal = childAPose.ToWorldPosition(contact.PositionALocal);
          //if (contact.Lifetime.Ticks == 0) // Currently, all contacts are new, so this check is not necessary.
          //{
          contact.FeatureA = childIndexA;
          //}

          contact.PositionBLocal = childBPose.ToWorldPosition(contact.PositionBLocal);
          //if (contact.Lifetime.Ticks == 0) // Currently, all contacts are new, so this check is not necessary.
          //{
          contact.FeatureB = childIndexB;
          //}
        }

        // Merge child contacts.
        ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);
      }
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Neither <paramref name="objectA"/> nor <paramref name="objectB"/> is a 
    /// <see cref="CompositeShape"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      // We get the children and compute the minimal TOI for all children. The child movement is 
      // linearly interpolated from start to end pose. - This is not correct if the real center
      // of rotation and the center of the child shape are not equal! But for small rotational
      // movement and small offsets this is acceptable.
      // The correct solution:
      // Conservative Advancement must not use linear motion interpolation. Instead the correct
      // intermediate motion must be computed relative to the parent space.
      // The faster solution:
      // Use Hierarchical Conservative Advancement as described in the papers by Kim Young et al:
      // FAST, C2A, ...

      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // B should be the composite, swap objects if necessary.
      if (!(objectB.GeometricObject.Shape is CompositeShape))
      {
        MathHelper.Swap(ref objectA, ref objectB);
        MathHelper.Swap(ref targetPoseA, ref targetPoseB);
      }

      CompositeShape compositeShapeB = objectB.GeometricObject.Shape as CompositeShape;

      // Check if collision objects shapes are correct.
      if (compositeShapeB == null)
        throw new ArgumentException("One object must be a composite shape.");

      IGeometricObject geometricObjectB = objectB.GeometricObject;
      Pose poseB = geometricObjectB.Pose;
      Vector3F scaleB = geometricObjectB.Scale;

      // Note: Non-uniform scaling for rotated child objects is not supported
      // but we might still get a usable TOI query result.

      float timeOfImpact = 1;

      // Use temporary object.
      var testGeometricObjectB = TestGeometricObject.Create();
      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();

      // Go through list of children and find minimal TOI.
      int numberOfChildren = compositeShapeB.Children.Count;
      for (int i = 0; i < numberOfChildren; i++)
      {
        IGeometricObject childGeometricObject = compositeShapeB.Children[i];

        // Following code is taken from the TransformedShapeAlgorithm:
        Pose childPose = childGeometricObject.Pose;
        childPose.Position *= scaleB;

        testGeometricObjectB.Shape = childGeometricObject.Shape;
        testGeometricObjectB.Scale = scaleB * childGeometricObject.Scale;
        testGeometricObjectB.Pose = poseB * childPose;

        testCollisionObjectB.SetInternal(objectB, testGeometricObjectB);

        var collisionAlgorithm = CollisionDetection.AlgorithmMatrix[objectA, testCollisionObjectB];
        float childTimeOfImpact = collisionAlgorithm.GetTimeOfImpact(
          objectA, targetPoseA,
          testCollisionObjectB, targetPoseB * childPose,
          allowedPenetration);

        timeOfImpact = Math.Min(timeOfImpact, childTimeOfImpact);
      }

      // Recycle temporary objects.
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
      testGeometricObjectB.Recycle();

      return timeOfImpact;
    }
  }
}
