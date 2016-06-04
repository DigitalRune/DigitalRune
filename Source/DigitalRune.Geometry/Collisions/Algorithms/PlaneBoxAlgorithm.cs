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
  /// Computes contact or closest-point information for <see cref="PlaneShape"/> vs. 
  /// <see cref="BoxShape"/>s.
  /// </summary>
  /// <remarks>
  /// </remarks>
  public class PlaneBoxAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneBoxAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public PlaneBoxAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="PlaneShape"/> and a 
    /// <see cref="ConvexShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object A should be the plane.
      // Object B should be the other object.
      IGeometricObject planeObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject boxObject = contactSet.ObjectB.GeometricObject;

      // Swap objects if necessary.
      bool swapped = (boxObject.Shape is PlaneShape);
      if (swapped)
        MathHelper.Swap(ref planeObject, ref boxObject);

      PlaneShape planeShape = planeObject.Shape as PlaneShape;
      BoxShape boxShape = boxObject.Shape as BoxShape;

      // Check if shapes are correct.
      if (planeShape == null || boxShape == null)
        throw new ArgumentException("The contact set must contain a plane and a box shape.", "contactSet");

      // Get transformations.
      Vector3F scalePlane = planeObject.Scale;
      Vector3F scaleBox = boxObject.Scale;
      Pose posePlane = planeObject.Pose;
      Pose poseBox = boxObject.Pose;

      // Apply scale to plane and transform plane into world space.
      Plane planeWorld = new Plane(planeShape);
      planeWorld.Scale(ref scalePlane);         // Scale plane.
      planeWorld.ToWorld(ref posePlane);        // Transform plane to world space.

      // Transform plane normal to local space of box.
      Vector3F planeNormalLocalB = poseBox.ToLocalDirection(planeWorld.Normal);

      // Get support vertex nearest to the plane.
      Vector3F supportVertexBLocal = boxShape.GetSupportPoint(-planeNormalLocalB, scaleBox);

      // Transform support vertex into world space.
      Vector3F supportVertexBWorld = poseBox.ToWorldPosition(supportVertexBLocal);

      // Project vertex onto separating axis (given by plane normal).
      float distance = Vector3F.Dot(supportVertexBWorld, planeWorld.Normal);

      // Check for collision.
      float penetrationDepth = planeWorld.DistanceFromOrigin - distance;
      contactSet.HaveContact = (penetrationDepth >= 0);

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // We have contact. 
      if (!CollisionDetection.FullContactSetPerFrame || type == CollisionQueryType.ClosestPoints)
      {
        // Use only the already known contact.
        AddContact(ref supportVertexBWorld, ref planeWorld, penetrationDepth, swapped, contactSet, type);
      }
      else
      {
        // Apply scale to box extent.
        Vector3F boxHalfExtent = boxShape.Extent * scaleBox * 0.5f;

        // Check all box vertices against plane.
        CheckContact(ref planeWorld, new Vector3F(-boxHalfExtent.X, -boxHalfExtent.Y, -boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(-boxHalfExtent.X, -boxHalfExtent.Y, boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(-boxHalfExtent.X, boxHalfExtent.Y, -boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(-boxHalfExtent.X, boxHalfExtent.Y, boxHalfExtent.Z), ref poseBox, swapped, contactSet);

        CheckContact(ref planeWorld, new Vector3F(boxHalfExtent.X, -boxHalfExtent.Y, -boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(boxHalfExtent.X, -boxHalfExtent.Y, boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(boxHalfExtent.X, boxHalfExtent.Y, -boxHalfExtent.Z), ref poseBox, swapped, contactSet);
        CheckContact(ref planeWorld, new Vector3F(boxHalfExtent.X, boxHalfExtent.Y, boxHalfExtent.Z), ref poseBox, swapped, contactSet);  
      }
    }


    // Checks a vertex and adds a contact if the vertex touches the plane.
    private void CheckContact(ref Plane planeWorld, Vector3F vertexLocal, ref Pose poseBox, bool swapped, ContactSet contactSet)
    {
      Vector3F vertex = poseBox.ToWorldPosition(vertexLocal);

      float distance = Vector3F.Dot(vertex, planeWorld.Normal);
      
      float penetrationDepth = planeWorld.DistanceFromOrigin - distance;

      if (penetrationDepth > 0)
      {
        // Position is between support vertex and plane.
        AddContact(ref vertex, ref planeWorld, penetrationDepth, swapped, contactSet, CollisionQueryType.Contacts);
      }
    }


    // Adds a contact to the contact set.
    private void AddContact(ref Vector3F vertex, ref Plane planeWorld, float penetrationDepth, bool swapped, ContactSet contactSet, CollisionQueryType type)
    {
      Vector3F position = vertex + planeWorld.Normal * (penetrationDepth / 2);
      Vector3F normal = (swapped) ? -planeWorld.Normal : planeWorld.Normal;

      // Update contact set.
      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      // We can use conservative advancement because a plane is convex enough so it will work.
      switch (CollisionDetection.ContinuousCollisionDetectionMode)
      {
        case ContinuousCollisionDetectionMode.Full:
          return CcdHelper.GetTimeOfImpactCA(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration, CollisionDetection);
        default:
          return CcdHelper.GetTimeOfImpactLinearCA(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration, CollisionDetection);
      }
    }
  }
}
