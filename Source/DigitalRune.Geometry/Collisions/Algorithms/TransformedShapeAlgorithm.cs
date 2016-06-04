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
  /// Computes contact or closest-point information for <see cref="TransformedShape"/> vs. any other
  /// <see cref="Shape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes. This
  /// algorithm will call other algorithms to compute collision of the child of the
  /// <see cref="TransformedShape"/>.
  /// </remarks>
  public class TransformedShapeAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TransformedShapeAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public TransformedShapeAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="TransformedShape"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="contactSet"/> contains a <see cref="TransformedShape"/> with a local 
    /// rotation and a non-uniform scaling. Computing collisions for transformed shapes with local 
    /// rotations and non-uniform scaling is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object B should be the transformed shape.
      CollisionObject objectA = contactSet.ObjectA;
      CollisionObject objectB = contactSet.ObjectB;

      // Swap objects if necessary.
      bool swapped = !(objectB.GeometricObject.Shape is TransformedShape);
      if (swapped)
        MathHelper.Swap(ref objectA, ref objectB);

      IGeometricObject geometricObjectB = objectB.GeometricObject;
      TransformedShape transformedShape = geometricObjectB.Shape as TransformedShape;

      // Check if collision objects shapes are correct.
      if (transformedShape == null)
        throw new ArgumentException("The contact set must contain a transformed shape.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      // Currently object B has the following structure:
      //
      //  CollisionObject           objectB
      //    GeometricObject           geometricObjectB
      //      Pose                      poseB
      //      Scale                     scaleB
      //      TransformedShape          transformedShape
      //        GeometricObject             childGeometricObject
      //          Pose                      childPose
      //          Scale                     childScale
      //          Shape                     childShape
      //
      // To compute the collisions we temporarily remove the TransformedShape:
      // We replace the original geometric object with a test geometric object and combine the 
      // transformations:
      //
      //  CollisionObject           testObjectB
      //    GeometricObject           testGeometricObjectB
      //      Pose                      poseB * childPose
      //      Shape                     childShape
      //      Scale                     scaleB * childScale

      Pose poseB = geometricObjectB.Pose;
      Vector3F scaleB = geometricObjectB.Scale;

      // Apply scale to pose and test geometric object.
      // (Note: The scaling is either uniform or the transformed object has no local rotation.
      // Therefore, we only need to apply the scale of the parent to the scale and translation of 
      // the child. We can ignore the rotation.)
      IGeometricObject childGeometricObject = transformedShape.Child;
      Pose childPose = childGeometricObject.Pose;

      // Non-uniform scaling is not supported for rotated child objects.
      if ((scaleB.X != scaleB.Y || scaleB.Y != scaleB.Z) && childPose.HasRotation)
        throw new NotSupportedException("Computing collisions for transformed shapes with local rotations and non-uniform scaling is not supported.");

      childPose.Position *= scaleB;                       // Apply scaling to local translation.
      
      var testGeometricObjectB = TestGeometricObject.Create();
      testGeometricObjectB.Shape = childGeometricObject.Shape;
      testGeometricObjectB.Scale = scaleB * childGeometricObject.Scale;  // Apply scaling to local scale.
      testGeometricObjectB.Pose = poseB * childPose;

      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObjectB.SetInternal(objectB, testGeometricObjectB);

      var testContactSet = swapped ? ContactSet.Create(testCollisionObjectB, objectA) 
                                   : ContactSet.Create(objectA, testCollisionObjectB);
      testContactSet.IsPerturbationTestAllowed = contactSet.IsPerturbationTestAllowed;

      // Transform contacts into space of child and copy them into the testContactSet.
      //int numberOfOldContacts = contactSet.Count;
      //for (int i = 0; i < numberOfOldContacts; i++)
      //{
      //  Contact contact = contactSet[i];
      //  if (swapped)
      //    contact.PositionALocal = childPose.ToLocalPosition(contact.PositionALocal);
      //  else
      //    contact.PositionBLocal = childPose.ToLocalPosition(contact.PositionBLocal);
      //  
      //  testContactSet.Add(contact);
      //}

      // Compute collision.
      var collisionAlgorithm = CollisionDetection.AlgorithmMatrix[objectA, testCollisionObjectB];
      collisionAlgorithm.ComputeCollision(testContactSet, type);

      if (testContactSet.HaveContact)
        contactSet.HaveContact = true;

      // Transform contacts into space of parent TransformShape.
      int numberOfNewContacts = testContactSet.Count;
      for (int i = 0; i < numberOfNewContacts; i++)
      {
        Contact contact = testContactSet[i];
        if (swapped)
          contact.PositionALocal = childPose.ToWorldPosition(contact.PositionALocal);
        else
          contact.PositionBLocal = childPose.ToWorldPosition(contact.PositionBLocal);
      }

      // Merge new contacts to contactSet.
      // (If testContactSet contains all original contacts (see commented out part above), we can 
      // simply clear contactSet and copy all contacts of testContactSet.)
      //contactSet.Clear();
      //foreach (Contact contact in testContactSet)
      //  contactSet.Add(contact);
      ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);

      // Recycle temporary objects.
      testContactSet.Recycle();
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
      testGeometricObjectB.Recycle();
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Neither <paramref name="objectA"/> nor <paramref name="objectB"/> is a 
    /// <see cref="TransformedShape"/>.
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// </exception>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      // Most of this code is copied from ComputeCollision() above. 

      // We get the child and compute the TOI for the child movement. The child movement is 
      // linearly interpolated from start to end pose. - This is not correct if the real center
      // of rotation and the center of the child shape are not equal! But for small rotational
      // movement and small offsets this is acceptable.

      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // B should be the transformed shape, swap objects if necessary.
      bool swapped = !(objectB.GeometricObject.Shape is TransformedShape);
      if (swapped)
      {
        MathHelper.Swap(ref objectA, ref objectB);
        MathHelper.Swap(ref targetPoseA, ref targetPoseB);
      }

      IGeometricObject geometricObjectB = objectB.GeometricObject;
      TransformedShape transformedShape = geometricObjectB.Shape as TransformedShape;

      // Check if collision objects shapes are correct.
      if (transformedShape == null)
        throw new ArgumentException("objectA or objectB must be a TransformedShape.");

      Pose poseB = geometricObjectB.Pose;
      Vector3F scaleB = geometricObjectB.Scale;

      // Note: Non-uniform scaling for rotated child objects is not supported
      // but we might still get a usable TOI query result.

      // Apply scale to pose and test geometric object.
      // (Note: The scaling is either uniform or the transformed object has no local rotation.
      // Therefore, we only need to apply the scale of the parent to the scale and translation of 
      // the child. We can ignore the rotation.)
      IGeometricObject childGeometricObject = transformedShape.Child;
      Pose childPose = childGeometricObject.Pose;
      childPose.Position *= scaleB;                       // Apply scaling to local translation.

      var testGeometricObjectB = TestGeometricObject.Create();
      testGeometricObjectB.Shape = childGeometricObject.Shape;
      testGeometricObjectB.Scale = scaleB * childGeometricObject.Scale;    // Apply scaling to local scale.
      testGeometricObjectB.Pose = poseB * childPose;

      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObjectB.SetInternal(objectB, testGeometricObjectB);

      // Compute TOI.
      var collisionAlgorithm = CollisionDetection.AlgorithmMatrix[objectA, testCollisionObjectB];
      float timeOfImpact = collisionAlgorithm.GetTimeOfImpact(
        objectA, targetPoseA, 
        testCollisionObjectB, targetPoseB * childPose, 
        allowedPenetration);

      // Recycle temporary objects.
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
      testGeometricObjectB.Recycle();

      return timeOfImpact;
    }
  }
}
