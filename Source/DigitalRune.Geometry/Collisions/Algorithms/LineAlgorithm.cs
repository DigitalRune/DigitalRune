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
  /// Computes collision queries between <see cref="LineShape"/>s and other primitives.
  /// </summary>
  /// <remarks>
  /// This class implements a <see cref="CollisionAlgorithm"/> for lines against other objects - not
  /// for line segments against other objects. 
  /// </remarks>
  public class LineAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LineAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public LineAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="Line"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      bool isLineA = contactSet.ObjectA.GeometricObject.Shape is LineShape;
      bool isLineB = contactSet.ObjectB.GeometricObject.Shape is LineShape;
      if (isLineA && isLineB)
      {
        ComputeLineVsLine(contactSet, type);
      }
      else if (isLineA || isLineB)
      {
        ComputeLineVsOther(contactSet, type, isLineA);
      }
      else
      {
        throw new ArgumentException("The contact set must contain a line.", "contactSet");
      }
    }


    /// <summary>
    /// Computes the collision between line vs. line.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="type">The type of collision query.</param>
    private void ComputeLineVsLine(ContactSet contactSet, CollisionQueryType type)
    {
      IGeometricObject objectA = contactSet.ObjectA.GeometricObject;
      IGeometricObject objectB = contactSet.ObjectB.GeometricObject;

      Debug.Assert(objectA.Shape is LineShape && objectB.Shape is LineShape, "LineAlgorithm.ComputeLineVsLine should only be called for 2 line shapes.");
      Debug.Assert(contactSet.Count <= 1, "Two lines should have at max 1 contact point.");

      // Get transformations.
      Vector3F scaleA = objectA.Scale;
      Vector3F scaleB = objectB.Scale;
      Pose poseA = objectA.Pose;
      Pose poseB = objectB.Pose;

      // Create two line objects in world space.
      var lineA = new Line((LineShape)objectA.Shape);
      lineA.Scale(ref scaleA);
      lineA.ToWorld(ref poseA);

      var lineB = new Line((LineShape)objectB.Shape);
      lineB.Scale(ref scaleB);
      lineB.ToWorld(ref poseB);

      // Get closest points.
      Vector3F pointA;
      Vector3F pointB;
      contactSet.HaveContact = GeometryHelper.GetClosestPoints(lineA, lineB, out pointA, out pointB);

      if (type == CollisionQueryType.Boolean || (type == CollisionQueryType.Contacts && !contactSet.HaveContact))
      {
        // HaveContact queries can exit here.
        // GetContacts queries can exit here if we don't have a contact.
        return;
      }

      // Create contact information.
      Vector3F position = (pointA + pointB) / 2;
      Vector3F normal = pointB - pointA;
      float length = normal.Length;
      if (Numeric.IsZero(length))
      {
        // Create normal from cross product of both lines.
        normal = Vector3F.Cross(lineA.Direction, lineB.Direction);
        if (!normal.TryNormalize())
          normal = Vector3F.UnitY;
      }
      else
      {
        // Normalize vector
        normal = normal / length;
      }

      Contact contact = ContactHelper.CreateContact(contactSet, position, normal, -length, false);
      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
    }


    /// <summary>
    /// Computes the collision between line vs. other shape.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="type">The type of collision query.</param>
    /// <param name="objectAIsLine">
    /// <see langword="true"/> if object A in the contact set is the line; otherwise 
    /// <see langword="false"/> if object B is the line.
    /// </param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
    private void ComputeLineVsOther(ContactSet contactSet, CollisionQueryType type, bool objectAIsLine)
    {
      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      Shape shapeA = geometricObjectA.Shape;
      Shape shapeB = geometricObjectB.Shape;

      Debug.Assert(
        shapeA is LineShape && !(shapeB is LineShape)
        || shapeB is LineShape && !(shapeA is LineShape),
        "LineAlgorithm.ComputeLineVsOther should only be called for a line and another shape.");

      CollisionObject lineCollisionObject;
      IGeometricObject lineGeometricObject;
      IGeometricObject otherGeometricObject;
      LineShape lineShape;
      Shape otherShape;
      if (objectAIsLine)
      {
        lineCollisionObject = collisionObjectA;
        lineGeometricObject = geometricObjectA;
        lineShape = (LineShape)shapeA;
        otherGeometricObject = geometricObjectB;
        otherShape = shapeB;
      }
      else
      {
        lineCollisionObject = collisionObjectB;
        lineGeometricObject = geometricObjectB;
        lineShape = (LineShape)shapeB;
        otherGeometricObject = geometricObjectA;
        otherShape = shapeA;
      }

      // Apply scaling to line.
      Line line = new Line(lineShape);
      Vector3F lineScale = lineGeometricObject.Scale;
      line.Scale(ref lineScale);

      // Step 1: Get any bounding sphere that encloses the other object.
      Aabb aabb = otherGeometricObject.Aabb;
      Vector3F center = (aabb.Minimum + aabb.Maximum) / 2;
      float radius = (aabb.Maximum - aabb.Minimum).Length;  // A large safe radius. (Exact size does not matter.)

      // Step 2: Get the closest point of line vs. center. 
      // All computations in local space of the line.
      Vector3F closestPointOnLine;
      Pose linePose = lineGeometricObject.Pose;
      GeometryHelper.GetClosestPoint(line, linePose.ToLocalPosition(center), out closestPointOnLine);

      // Step 3: Crop the line to a line segment that will contain the closest point.
      var lineSegment = ResourcePools.LineSegmentShapes.Obtain();
      lineSegment.Start = closestPointOnLine - line.Direction * radius;
      lineSegment.End = closestPointOnLine + line.Direction * radius;

      // Use temporary test objects.
      var testGeometricObject = TestGeometricObject.Create();
      testGeometricObject.Shape = lineSegment;
      testGeometricObject.Scale = Vector3F.One;
      testGeometricObject.Pose = linePose;

      var testCollisionObject = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObject.SetInternal(lineCollisionObject, testGeometricObject);

      var testContactSet = objectAIsLine ? ContactSet.Create(testCollisionObject, collisionObjectB) 
                                         : ContactSet.Create(collisionObjectA, testCollisionObject);
      testContactSet.IsPerturbationTestAllowed = contactSet.IsPerturbationTestAllowed;

      // Step 4: Call another collision algorithm.
      CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[lineSegment, otherShape];

      // Step 5: Manually chosen preferred direction for MPR.
      // For the MPR we choose the best ray direction ourselves. The ray should be normal
      // to the line, otherwise MPR could try to push the line segment out of the other object
      // in the line direction - this cannot work for infinite lines.
      // Results without a manual MPR ray were ok for normal cases. Problems were only observed
      // for cases where the InnerPoints overlap or for deep interpenetrations.
      Vector3F v0A = geometricObjectA.Pose.ToWorldPosition(shapeA.InnerPoint * geometricObjectA.Scale);
      Vector3F v0B = geometricObjectB.Pose.ToWorldPosition(shapeB.InnerPoint * geometricObjectB.Scale);
      Vector3F n = v0B - v0A; // This is the default MPR ray direction.

      // Make n normal to the line.
      n = n - Vector3F.ProjectTo(n, linePose.ToWorldDirection(lineShape.Direction));
      if (!n.TryNormalize())
        n = lineShape.Direction.Orthonormal1;

      testContactSet.PreferredNormal = n;
      collisionAlgorithm.ComputeCollision(testContactSet, type);

      if (testContactSet.HaveContact)
        contactSet.HaveContact = true;

      ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);

      // Recycle temporary objects.
      testContactSet.Recycle();
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObject);
      testGeometricObject.Recycle();
      ResourcePools.LineSegmentShapes.Recycle(lineSegment);
    }
  }
}
