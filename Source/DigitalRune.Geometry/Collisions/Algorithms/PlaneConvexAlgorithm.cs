// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="PlaneShape"/> vs. 
  /// <see cref="ConvexShape"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// On of the shapes must be a <see cref="PlaneShape"/>, the other shape must be a convex that
  /// provides support mappings. For example, plane vs. plane, or plane vs. line will not work.
  /// </para>
  /// </remarks>
  public class PlaneConvexAlgorithm : CollisionAlgorithm
  {
    private readonly Action<ContactSet> _computeContactsMethod;


    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneConvexAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public PlaneConvexAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      // Store test method to avoid garbage when using TestWithPerturbations.
      _computeContactsMethod = contactSet => ComputeCollision(contactSet, CollisionQueryType.Contacts);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="PlaneShape"/> and a 
    /// <see cref="ConvexShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object A should be the plane.
      // Object B should be the other object.
      IGeometricObject planeObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject convexObject = contactSet.ObjectB.GeometricObject;

      // Swap objects if necessary.
      bool swapped = (convexObject.Shape is PlaneShape);
      if (swapped)
        MathHelper.Swap(ref planeObject, ref convexObject);

      PlaneShape planeShape = planeObject.Shape as PlaneShape;
      ConvexShape convexShape = convexObject.Shape as ConvexShape;

      // Check if shapes are correct.
      if (planeShape == null || convexShape == null)
        throw new ArgumentException("The contact set must contain a plane and a convex shape.", "contactSet");

      // Get transformations.
      Vector3F scalePlane = planeObject.Scale;
      Vector3F scaleB = convexObject.Scale;
      Pose planePose = planeObject.Pose;
      Pose poseB = convexObject.Pose;

      // Apply scale to plane and transform plane into world space.
      Plane planeWorld = new Plane(planeShape);
      planeWorld.Scale(ref scalePlane);         // Scale plane.
      planeWorld.ToWorld(ref planePose);        // Transform plane to world space.

      // Transform plane normal to local space of convex.
      Vector3F planeNormalLocalB = poseB.ToLocalDirection(planeWorld.Normal);

      // Get support vertex nearest to the plane.
      Vector3F supportVertexBLocal = convexShape.GetSupportPoint(-planeNormalLocalB, scaleB);

      // Transform support vertex into world space.
      Vector3F supportVertexBWorld = poseB.ToWorldPosition(supportVertexBLocal);

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

      // Position is between support vertex and plane.
      Vector3F position = supportVertexBWorld + planeWorld.Normal * (penetrationDepth / 2);
      Vector3F normal = (swapped) ? -planeWorld.Normal : planeWorld.Normal;

      // Update contact set.
      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);

      if (CollisionDetection.FullContactSetPerFrame
          && type == CollisionQueryType.Contacts
          && contactSet.Count > 0
          && contactSet.Count < 4)
      {
        // Special treatment for tetrahedra: Test all vertices against plane.
        IList<Vector3F> vertices = null;
        if (convexShape is ConvexHullOfPoints)
        {
          var convexHullOfPoints = (ConvexHullOfPoints)convexShape;
          vertices = convexHullOfPoints.Points;
        }
        else if (convexShape is ConvexPolyhedron)
        {
          var convexPolyhedron = (ConvexPolyhedron)convexShape;
          vertices = convexPolyhedron.Vertices;
        }

        if (vertices != null && vertices.Count <= 8)
        {
          // Convex has 8 or less vertices. Explicitly test all vertices against the plane.
          int numberOfVertices = vertices.Count;
          for (int i = 0; i < numberOfVertices; i++)
          {
            // Test is the same as above.
            var vertex = vertices[i];
            Vector3F scaledVertex = vertex * scaleB;
            if (scaledVertex != supportVertexBLocal) // supportVertexBLocal has already been added.
            {
              Vector3F vertexWorld = poseB.ToWorldPosition(scaledVertex);
              distance = Vector3F.Dot(vertexWorld, planeWorld.Normal);
              penetrationDepth = planeWorld.DistanceFromOrigin - distance;
              if (penetrationDepth >= 0)
              {
                position = vertexWorld + planeWorld.Normal * (penetrationDepth / 2);
                normal = (swapped) ? -planeWorld.Normal : planeWorld.Normal;
                contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
                ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
              }
            }
          }
        }
        else
        {
          // Convex is a complex shape with more than 4 vertices.
          ContactHelper.TestWithPerturbations(
            CollisionDetection,
            contactSet,
            !swapped,    // Perturb the convex object, not the plane.
            _computeContactsMethod);
        }
      }
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
