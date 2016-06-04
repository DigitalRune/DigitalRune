// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="RayShape"/> vs. 
  /// <see cref="PlaneShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class PlaneRayAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaneRayAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public PlaneRayAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="PlaneShape"/> and a 
    /// <see cref="RayShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      Debug.Assert(contactSet.Count <= 1, "Ray vs. plane should have at max 1 contact.");

      // Object A should be the plane.
      // Object B should be the ray.
      IGeometricObject planeObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject rayObject = contactSet.ObjectB.GeometricObject;

      // Swap objects if necessary.
      bool swapped = (rayObject.Shape is PlaneShape);
      if (swapped)
        MathHelper.Swap(ref planeObject, ref rayObject);

      PlaneShape planeShape = planeObject.Shape as PlaneShape;
      RayShape rayShape = rayObject.Shape as RayShape;

      // Check if A is really a plane and B is a ray.
      if (planeShape == null || rayShape == null)
        throw new ArgumentException("The contact set must contain a plane and a ray.", "contactSet");

      // Get transformations.
      Vector3F planeScale = planeObject.Scale;
      Vector3F rayScale = rayObject.Scale;
      Pose rayPose = rayObject.Pose;
      Pose planePose = planeObject.Pose;

      // Apply scale to plane.
      Plane plane = new Plane(planeShape);
      plane.Scale(ref planeScale);

      // Apply scale to ray and transform ray into local space of plane.
      Ray ray = new Ray(rayShape);
      ray.Scale(ref rayScale);      // Scale ray.
      ray.ToWorld(ref rayPose);     // Transform ray to world space.
      ray.ToLocal(ref planePose);   // Transform ray to local space of plane.

      // Convert ray into a line segment.
      LineSegment segment = new LineSegment { Start = ray.Origin, End = ray.Origin + ray.Direction * ray.Length };

      // Check if ray origin is inside the plane. Otherwise call plane vs. ray query.
      Vector3F linePoint;
      Vector3F planePoint = Vector3F.Zero;
      if (Vector3F.Dot(segment.Start, plane.Normal) <= plane.DistanceFromOrigin)
      {
        // The origin of the ray is below the plane.
        linePoint = segment.Start;
        contactSet.HaveContact = true;
      }
      else
      {
        // The origin of the ray is above the plane.
        contactSet.HaveContact = GeometryHelper.GetClosestPoints(plane, segment, out linePoint, out planePoint);
      }

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // ----- Create contact info.
      Vector3F position;
      float penetrationDepth;
      if (contactSet.HaveContact)
      {
        // We have a contact.
        position = planePose.ToWorldPosition(linePoint);
        penetrationDepth = (linePoint - segment.Start).Length;
      }
      else
      {
        // Closest points, but separated.
        position = planePose.ToWorldPosition((planePoint + linePoint) / 2);
        penetrationDepth = -(linePoint - planePoint).Length;
      }

      Vector3F normal = planePose.ToWorldDirection(plane.Normal);
      if (swapped)
        normal = -normal;

      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, contactSet.HaveContact);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }
  }
}
