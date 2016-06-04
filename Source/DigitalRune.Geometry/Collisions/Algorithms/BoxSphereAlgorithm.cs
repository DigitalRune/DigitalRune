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
  /// Computes contact or closest-point information for <see cref="BoxShape"/> vs. 
  /// <see cref="SphereShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class BoxSphereAlgorithm : CollisionAlgorithm
  {
    // Non-uniformly scaled spheres are not handled by the BoxSphereAlgorithm. We use 
    // a box-convex algorithm as fallback.
    private CollisionAlgorithm _fallbackAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="BoxSphereAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public BoxSphereAlgorithm(CollisionDetection collisionDetection) 
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="BoxShape"/> and a 
    /// <see cref="SphereShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // From Coutinho: "Dynamic Simulations of Multibody Systems" and 
      // Bergen: "Collision Detection in Interactive 3D Environments".

      // Object A should be the box.
      // Object B should be the sphere.
      IGeometricObject boxObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject sphereObject = contactSet.ObjectB.GeometricObject;

      // Swap objects if necessary.
      bool swapped = (sphereObject.Shape is BoxShape);
      if (swapped)
        MathHelper.Swap(ref boxObject, ref sphereObject);

      BoxShape boxShape = boxObject.Shape as BoxShape;
      SphereShape sphereShape = sphereObject.Shape as SphereShape;

      // Check if collision objects shapes are correct.
      if (boxShape == null || sphereShape == null)
        throw new ArgumentException("The contact set must contain a box and a sphere.", "contactSet");

      Vector3F scaleBox = Vector3F.Absolute(boxObject.Scale);
      Vector3F scaleSphere = Vector3F.Absolute(sphereObject.Scale);

      // Call other algorithm for non-uniformly scaled spheres.
      if (scaleSphere.X != scaleSphere.Y || scaleSphere.Y != scaleSphere.Z)
      {
        if (_fallbackAlgorithm == null)
          _fallbackAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(BoxShape), typeof(ConvexShape)];

        _fallbackAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      // Apply scale.
      Vector3F boxExtent = boxShape.Extent * scaleBox;
      float sphereRadius = sphereShape.Radius * scaleSphere.X;

      // ----- First transform sphere center into the local space of the box.
      Pose boxPose = boxObject.Pose;
      Vector3F sphereCenterWorld = sphereObject.Pose.Position;
      Vector3F sphereCenter = boxPose.ToLocalPosition(sphereCenterWorld);

      Vector3F p = Vector3F.Zero;
      bool sphereCenterIsContainedInBox = true;

      // When sphere center is on a box surface we have to choose a suitable normal.
      // otherwise the normal will be computed later.
      Vector3F normal = Vector3F.Zero;

      #region ----- Look for the point p of the box that is closest to center of the sphere. -----
      Vector3F boxHalfExtent = 0.5f * boxExtent;

      // x component
      if (sphereCenter.X < -boxHalfExtent.X)
      {
        p.X = -boxHalfExtent.X;
        sphereCenterIsContainedInBox = false;
      }
      else if (sphereCenter.X > boxHalfExtent.X)
      {
        p.X = boxHalfExtent.X;
        sphereCenterIsContainedInBox = false;
      }
      else
      {
        p.X = sphereCenter.X;
      }

      // y component
      if (sphereCenter.Y < -boxHalfExtent.Y)
      {
        p.Y = -boxHalfExtent.Y;
        sphereCenterIsContainedInBox = false;
      }
      else if (sphereCenter.Y > boxHalfExtent.Y)
      {
        p.Y = boxHalfExtent.Y;
        sphereCenterIsContainedInBox = false;
      }
      else
      {
        p.Y = sphereCenter.Y;
      }

      // z component
      if (sphereCenter.Z < -boxHalfExtent.Z)
      {
        p.Z = -boxHalfExtent.Z;
        sphereCenterIsContainedInBox = false;
      }
      else if (sphereCenter.Z > boxHalfExtent.Z)
      {
        p.Z = boxHalfExtent.Z;
        sphereCenterIsContainedInBox = false;
      }
      else
      {
        p.Z = sphereCenter.Z;
      }

      if (sphereCenterIsContainedInBox || (sphereCenter - p).IsNumericallyZero)
      {
        // Special case: Sphere center is within box. In this case p == center.
        // Lets return a point on the surface of the box.
        // Lets find the axis with the smallest way out (penetration depth).        
        Vector3F diff = boxHalfExtent - Vector3F.Absolute(sphereCenter);
        if (diff.X <= diff.Y && diff.X <= diff.Z)
        {
          // Point on one of the x surfaces is nearest.
          // Check whether positive or negative surface.
          bool positive = (sphereCenter.X > 0);
          p.X = positive ? boxHalfExtent.X : -boxHalfExtent.X;

          if (Numeric.IsZero(diff.X))
          {
            // Sphere center is on box surface.
            normal = positive ? Vector3F.UnitX : -Vector3F.UnitX;
          }
        }
        else if (diff.Y <= diff.X && diff.Y <= diff.Z)
        {
          // Point on one of the y surfaces is nearest.
          // Check whether positive or negative surface.
          bool positive = (sphereCenter.Y > 0);
          p.Y = positive ? boxHalfExtent.Y : -boxHalfExtent.Y;

          if (Numeric.IsZero(diff.Y))
          {
            // Sphere center is on box surface.
            normal = positive ? Vector3F.UnitY : -Vector3F.UnitY;
          }
        }
        else
        {
          // Point on one of the z surfaces is nearest.
          // Check whether positive or negative surface.
          bool positive = (sphereCenter.Z > 0);
          p.Z = positive ? boxHalfExtent.Z : -boxHalfExtent.Z;

          if (Numeric.IsZero(diff.Z))
          {
            // Sphere center is on box surface.
            normal = positive ? Vector3F.UnitZ : -Vector3F.UnitZ;
          }
        }
      }
      #endregion
      
      // ----- Convert back to world space
      p = boxPose.ToWorldPosition(p);
      Vector3F sphereCenterToP = p - sphereCenterWorld;

      // Compute penetration depth.
      float penetrationDepth = sphereCenterIsContainedInBox
                                 ? sphereRadius + sphereCenterToP.Length
                                 : sphereRadius - sphereCenterToP.Length;
      contactSet.HaveContact = (penetrationDepth >= 0);

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // ----- Create collision info.
      // Compute normal if we haven't set one yet.
      if (normal == Vector3F.Zero)
      {
        Debug.Assert(!sphereCenterToP.IsNumericallyZero, "When the center of the sphere lies on the box surface a normal should be have been set explicitly.");
        normal = sphereCenterIsContainedInBox ? sphereCenterToP : -sphereCenterToP;
        normal.Normalize();
      }
      else
      {
        normal = boxPose.ToWorldDirection(normal);
      }

      // Position = point between sphere and box surface.
      Vector3F position = p - normal * (penetrationDepth / 2);
      if (swapped)
        normal = -normal;

      // Update contact set.
      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }
  }
}
