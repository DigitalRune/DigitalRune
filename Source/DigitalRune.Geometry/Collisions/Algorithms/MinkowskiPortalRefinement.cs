// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// The Minkowski Portal Refinement (MPR) algorithm for computing contacts between convex objects.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This algorithm is designed only for boolean or contact queries. 
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public class MinkowskiPortalRefinement : CollisionAlgorithm
  {
    // (We cannot put the following remarks in the public XML comments because the property 
    // PreferredNormal is only internal.)
    //
    // Additional remarks:
    // If <see cref="ContactSet.PreferredNormal"/> is set in the <see cref="ContactSet"/>, then this 
    // algorithm searches for a contact for this normal. Warning: Some separation are not detected
    // when a <see cref="ContactSet.PreferredNormal"/> is used, for example if two triangle are 
    // "back to back" and not touching. To avoid false contacts, call the MPR or another
    // algorithm without a <see cref="ContactSet.PreferredNormal"/> first.

    // From 
    // - Game Programming Gems 7, 2.5 XenoCollide, pp. 165.
    // - XenoCollide website.
    // MPR does not compute the "minimum penetration depth" (a.k.a. MTD = minimum translation
    // distance". But it computes consistent useful penetration depth for physics.
    // 1 call of MPR will tell whether objects have contact and if they have contact it will
    // give contact information. It can happen that the normal is not optimal and then
    // MPR must be called iteratively and the normal vector will converge against a local
    // optimum. - To find the global minimum penetration depth a different algorithm like
    // GJK with EPA has to be used. EPA samples the CSO in all direction. MPR start at a good
    // guess and refines from there.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // To avoid unnecessary memory allocations we do not use lambda expressions to wrap the 
    // DoMpr() methods calls for perturbations tests. The lambda expression is a closure that 
    // requires one additional parameter. Each closure would be allocated on the heap. Instead we
    // define our own named wrapper. The wrappers are reused using resource pooling.
    private sealed class TestMethodWrapper
    {
      // The original DoMpr() method.
      public Func<CollisionQueryType, ContactSet, Vector3F, Vector3F> OriginalMethod;

      // The initial direction used in MPR.
      public Vector3F V0;

      // The method required by TestWithPerturbations.
      public readonly Action<ContactSet> Method;

      public TestMethodWrapper()
      {
        Method = contactSet => OriginalMethod(CollisionQueryType.Contacts, contactSet, V0);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // A pool of reusable test-methods wrappers.
    private readonly static ResourcePool<TestMethodWrapper> TestMethodWrappers =
      new ResourcePool<TestMethodWrapper>(
        () => new TestMethodWrapper(),
        null,
        wrapper => wrapper.OriginalMethod = null);

    private readonly Func<CollisionQueryType, ContactSet, Vector3F, Vector3F> _doMprMethod;

    // We use GJK as a safety check in degenerate cases.
    private Gjk _gjk;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="MinkowskiPortalRefinement"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public MinkowskiPortalRefinement(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      // Store DoMpr method in delegate to avoid garbage when using TestWithPerturbations.
      _doMprMethod = DoMpr;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain two <see cref="ConvexShape"/>s.
    /// </exception>
    /// <exception cref="GeometryException">
    /// <paramref name="type"/> is set to <see cref="CollisionQueryType.ClosestPoints"/>. This 
    /// collision algorithm cannot handle closest-point queries. Use <see cref="Gjk"/> instead.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Note: When comparing this implementation with the GJK (see Gjk.cs), be aware that the 
      // GJK implementation computes the CSO as the Minkowski difference A-B whereas the MPR uses
      // B-A. Both representations of the CSO are equivalent, we just have to invert the vectors 
      // here and there. (B-A was chosen because the original description of the MPR used B-A.)

      if (type == CollisionQueryType.ClosestPoints)
        throw new GeometryException("MPR cannot handle closest-point queries. Use GJK instead.");

      CollisionObject collisionObjectA = contactSet.ObjectA;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      ConvexShape shapeA = geometricObjectA.Shape as ConvexShape;
      Vector3F scaleA = geometricObjectA.Scale;
      Pose poseA = geometricObjectA.Pose;

      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      ConvexShape shapeB = geometricObjectB.Shape as ConvexShape;
      Vector3F scaleB = geometricObjectB.Scale;
      Pose poseB = geometricObjectB.Pose;

      if (shapeA == null || shapeB == null)
        throw new ArgumentException("The contact set must contain convex shapes.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      Vector3F v0;
      if (contactSet.IsPreferredNormalAvailable && type == CollisionQueryType.Contacts)
      {
        // Set v0, so to shoot into preferred direction.
        v0 = contactSet.PreferredNormal;

        // Perform only 1 MPR iteration.
        DoMpr(type, contactSet, v0);
        return;
      }

      // Find first point v0 (which determines the ray direction).
      // Inner point in CSO (Minkowski difference B-A).
      Vector3F v0A = poseA.ToWorldPosition(shapeA.InnerPoint * scaleA);
      Vector3F v0B = poseB.ToWorldPosition(shapeB.InnerPoint * scaleB);
      v0 = v0B - v0A;

      // If v0 == origin then we have contact.
      if (v0.IsNumericallyZero)
      {
        // The inner points overlap. Probably there are two objects centered on the same point.
        contactSet.HaveContact = true;
        if (type == CollisionQueryType.Boolean)
          return;

        // Choose a v0 different from Zero. Any direction is ok. 
        // The point should still be in the Minkowski difference.
        v0.X = CollisionDetection.Epsilon / 10;
      }

      // Call MPR in iteration until the MPR ray has converged.
      int iterationCount = 0;
      const int iterationLimit = 10;
      Vector3F oldMprRay;

      // Use a temporary contact set.
      var testContactSet = ContactSet.Create(collisionObjectA, collisionObjectB);
      testContactSet.IsPerturbationTestAllowed = contactSet.IsPerturbationTestAllowed;
      testContactSet.PreferredNormal = contactSet.PreferredNormal;

      Contact oldContact = null;
      do
      {
        oldMprRay = v0;
        if (iterationCount == 0)
          oldMprRay.TryNormalize();

        // Call MPR. v0 of the next iteration is simply -returned portal normal.

        Debug.Assert(testContactSet.Count == 0 || testContactSet.Count == 1, "testContactSet in MPR should have 0 or 1 contacts.");
        Debug.Assert(testContactSet.Count == 0 || testContactSet[0] == oldContact);
        testContactSet.Clear();

        // Because of numerical problems (for example with long thin ellipse vs. capsule)
        // it is possible that the last iteration was a contact but in this iteration
        // no contact is found. Therefore we also reset the HaveContact flag to avoid
        // an end result where HaveContact is set but no Contact is in the ContactSet.
        testContactSet.HaveContact = false;
        v0 = -DoMpr(type, testContactSet, v0);

        if (testContactSet.Count > 0)
        {
          var newContact = testContactSet[0];
          if (oldContact != null)
          {
            if (oldContact.PenetrationDepth < newContact.PenetrationDepth)
            {
              // The new penetration depth is larger then the old penetration depth.
              // In this case we keep the old contact.
              // This can happen for nearly parallel boxes. First we get a good contact.
              // Then we get a contact another side. Normal has changed 90°. The new
              // penetration depth can be nearly the whole box side length :-(.
              newContact.Recycle();
              testContactSet[0] = oldContact;
              break;
            }
          }

          if (newContact != oldContact)
          {
            if (oldContact != null)
              oldContact.Recycle();

            oldContact = newContact;
          }
        }

        iterationCount++;
      } while (testContactSet.HaveContact         // Separation? - No contact which we could refine.
               && iterationCount < iterationLimit // Iteration limit reached?
               && v0 != Vector3F.Zero             // Is normal useful to go on?
               && !Vector3F.AreNumericallyEqual(-v0, oldMprRay, CollisionDetection.Epsilon));
                                                  // Normal hasn't converged yet?

      if (testContactSet.Count > 0)
      {
        // Recycle oldContact if not used.
        if (testContactSet[0] != oldContact)
        {
          if (oldContact != null)
          {
            oldContact.Recycle();
            oldContact = null;
          }
        }        
      }

      if (CollisionDetection.FullContactSetPerFrame
          && type == CollisionQueryType.Contacts
          && testContactSet.Count > 0
          && contactSet.Count < 3)
      {
        // Try to find full contact set.
        var wrapper = TestMethodWrappers.Obtain();
        wrapper.OriginalMethod = _doMprMethod;
        wrapper.V0 = testContactSet[0].Normal;  // The MPR ray will point along the normal of the first contact.

        ContactHelper.TestWithPerturbations(
          CollisionDetection,
          testContactSet,
          true,
          wrapper.Method);

        TestMethodWrappers.Recycle(wrapper);
      }

      contactSet.HaveContact = testContactSet.HaveContact;
      ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);

      // Recycle temporary objects.
      testContactSet.Recycle();
    }


    // Performs Minkowski Portal Refinement. 
    // The normal of the current portal is returned, if another MPR iteration makes sense -
    // otherwise (0, 0, 0)
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
      private Vector3F DoMpr(CollisionQueryType type, ContactSet contactSet, Vector3F v0)
    {
      int iterationCount = 0;
      const int iterationLimit = 100;

      CollisionObject collisionObjectA = contactSet.ObjectA;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      ConvexShape shapeA = (ConvexShape)geometricObjectA.Shape;
      Vector3F scaleA = geometricObjectA.Scale;
      Pose poseA = geometricObjectA.Pose;

      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      ConvexShape shapeB = (ConvexShape)geometricObjectB.Shape;
      Vector3F scaleB = geometricObjectB.Scale;
      Pose poseB = geometricObjectB.Pose;

      // Cache inverted rotations.
      var orientationAInverse = poseA.Orientation.Transposed;
      var orientationBInverse = poseB.Orientation.Transposed;

      Vector3F n = -v0;  // Shoot from v0 to the origin.
      Vector3F v1A = poseA.ToWorldPosition(shapeA.GetSupportPoint(orientationAInverse * -n, scaleA));
      Vector3F v1B = poseB.ToWorldPosition(shapeB.GetSupportPoint(orientationBInverse * n, scaleB));
      Vector3F v1 = v1B - v1A;

      // Separating axis test:
      if (Vector3F.Dot(v1, n) < 0)
      {
        // TODO: We could cache the separating axis n in ContactSet for future collision checks.
        //       Also in the separating axis tests below. 
        return Vector3F.Zero;
      }

      // Second support direction = perpendicular to plane of origin, v0 and v1.
      n = Vector3F.Cross(v1, v0);

      // If n is a zero vector, then origin, v0 and v1 are on a line with the origin inside the support plane.
      if (n.IsNumericallyZero)
      {
        // Contact found.
        contactSet.HaveContact = true;
        if (type == CollisionQueryType.Boolean)
          return Vector3F.Zero;

        // Compute contact information.
        // (v0 is an inner point. v1 is a support point on the CSO. => The contact normal is -v1. 
        // However, v1 could be close to the origin. To avoid numerical
        // problems we use v0 - v1, which is the same direction.)
        Vector3F normal = v0 - v1;
        if (!normal.TryNormalize())
        {
          // This happens for Point vs. flat object when they are on the same position. 
          // Maybe we could even find a better normal.
          normal = Vector3F.UnitY;
        }

        Vector3F position = (v1A + v1B) / 2;
        float penetrationDepth = v1.Length;
        Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);

        return Vector3F.Zero;
      }

      Vector3F v2A = poseA.ToWorldPosition(shapeA.GetSupportPoint(orientationAInverse * -n, scaleA));
      Vector3F v2B = poseB.ToWorldPosition(shapeB.GetSupportPoint(orientationBInverse * n, scaleB));
      Vector3F v2 = v2B - v2A;

      // Separating axis test:
      if (Vector3F.Dot(v2, n) < 0)
        return Vector3F.Zero;

      // Third support direction = perpendicular to plane of v0, v1 and v2.
      n = Vector3F.Cross(v1 - v0, v2 - v0);

      // If the origin is on the negative side of the plane, then reverse the plane direction.
      // n must point into the origin direction and not away...
      if (Vector3F.Dot(n, v0) > 0)
      {
        MathHelper.Swap(ref v1, ref v2);
        MathHelper.Swap(ref v1A, ref v2A);
        MathHelper.Swap(ref v1B, ref v2B);
        n = -n;
      }

      if (n.IsNumericallyZero)
      {
        // Degenerate case: 
        // Interpretation (HelmutG): v2 is on the line with v0 and v1. I think this can only happen
        // if the CSO is flat and in the plane of (origin, v0, v1).
        // This happens for example in Point vs. Line Segment, or triangle vs. triangle when both 
        // triangles are in the same plane.
        // Simply ignore this case (Infinite small/flat objects do not touch).
        return Vector3F.Zero;
      }

      // Search for a valid portal.
      Vector3F v3, v3A, v3B;
      while (true)
      {
        iterationCount++;

        // Abort if we cannot find a valid portal.
        if (iterationCount > iterationLimit)
          return Vector3F.Zero;

        // Get next support point.
        //v3A = poseA.ToWorldPosition(shapeA.GetSupportPoint(orientationAInverse * -n, scaleA));
        //v3B = poseB.ToWorldPosition(shapeB.GetSupportPoint(orientationBInverse * n, scaleB));
        //v3 = v3B - v3A;

        // ----- Optimized version:
        Vector3F supportDirectionA;
        supportDirectionA.X = -(orientationAInverse.M00 * n.X + orientationAInverse.M01 * n.Y + orientationAInverse.M02 * n.Z);
        supportDirectionA.Y = -(orientationAInverse.M10 * n.X + orientationAInverse.M11 * n.Y + orientationAInverse.M12 * n.Z);
        supportDirectionA.Z = -(orientationAInverse.M20 * n.X + orientationAInverse.M21 * n.Y + orientationAInverse.M22 * n.Z);
        Vector3F supportPointA = shapeA.GetSupportPoint(supportDirectionA, scaleA);
        v3A.X = poseA.Orientation.M00 * supportPointA.X + poseA.Orientation.M01 * supportPointA.Y + poseA.Orientation.M02 * supportPointA.Z + poseA.Position.X;
        v3A.Y = poseA.Orientation.M10 * supportPointA.X + poseA.Orientation.M11 * supportPointA.Y + poseA.Orientation.M12 * supportPointA.Z + poseA.Position.Y;
        v3A.Z = poseA.Orientation.M20 * supportPointA.X + poseA.Orientation.M21 * supportPointA.Y + poseA.Orientation.M22 * supportPointA.Z + poseA.Position.Z;
        Vector3F supportDirectionB;
        supportDirectionB.X = orientationBInverse.M00 * n.X + orientationBInverse.M01 * n.Y + orientationBInverse.M02 * n.Z;
        supportDirectionB.Y = orientationBInverse.M10 * n.X + orientationBInverse.M11 * n.Y + orientationBInverse.M12 * n.Z;
        supportDirectionB.Z = orientationBInverse.M20 * n.X + orientationBInverse.M21 * n.Y + orientationBInverse.M22 * n.Z;
        Vector3F supportPointB = shapeB.GetSupportPoint(supportDirectionB, scaleB);
        v3B.X = poseB.Orientation.M00 * supportPointB.X + poseB.Orientation.M01 * supportPointB.Y + poseB.Orientation.M02 * supportPointB.Z + poseB.Position.X;
        v3B.Y = poseB.Orientation.M10 * supportPointB.X + poseB.Orientation.M11 * supportPointB.Y + poseB.Orientation.M12 * supportPointB.Z + poseB.Position.Y;
        v3B.Z = poseB.Orientation.M20 * supportPointB.X + poseB.Orientation.M21 * supportPointB.Y + poseB.Orientation.M22 * supportPointB.Z + poseB.Position.Z;
        v3 = v3B - v3A;

        // Separating axis test:
        //if (Vector3F.Dot(v3, n) < 0)
        if (v3.X * n.X + v3.Y * n.Y + v3.Z * n.Z < 0)
          return Vector3F.Zero;

        // v0, v1, v2, v3 form a tetrahedron.
        // v0 is an inner point of the CSO and v1, v2, v3 are support points.
        // v1, v2, v3 should form a valid portal.

        // If origin is outside the plane of v0, v1, v3 then the portal is invalid and we choose a new n.
        //if (Vector3F.Dot(Vector3F.Cross(v1, v3), v0) < 0) // ORIENT3D test, see Ericson: "Real-Time Collision Detection"
        if ((v1.Y * v3.Z - v1.Z * v3.Y) * v0.X
             + (v1.Z * v3.X - v1.X * v3.Z) * v0.Y
             + (v1.X * v3.Y - v1.Y * v3.X) * v0.Z < 0)
        {
          v2 = v3; // Get rid of v2. A new v3 will be chosen in the next iteration.
          v2A = v3A;
          v2B = v3B;
          //n = Vector3F.Cross(v1 - v0, v3 - v0);
          // ----- Optimized version:
          Vector3F v1MinusV0;
          v1MinusV0.X = v1.X - v0.X;
          v1MinusV0.Y = v1.Y - v0.Y;
          v1MinusV0.Z = v1.Z - v0.Z;
          Vector3F v3MinusV0;
          v3MinusV0.X = v3.X - v0.X;
          v3MinusV0.Y = v3.Y - v0.Y;
          v3MinusV0.Z = v3.Z - v0.Z;
          n.X = v1MinusV0.Y * v3MinusV0.Z - v1MinusV0.Z * v3MinusV0.Y;
          n.Y = v1MinusV0.Z * v3MinusV0.X - v1MinusV0.X * v3MinusV0.Z;
          n.Z = v1MinusV0.X * v3MinusV0.Y - v1MinusV0.Y * v3MinusV0.X;
          continue;
        }

        // If origin is outside the plane of v0, v2, v3 then the portal is invalid and we choose a new n.
        //if (Vector3F.Dot(Vector3F.Cross(v3, v2), v0) < 0)
        if ((v3.Y * v2.Z - v3.Z * v2.Y) * v0.X
             + (v3.Z * v2.X - v3.X * v2.Z) * v0.Y
             + (v3.X * v2.Y - v3.Y * v2.X) * v0.Z < 0)
        {
          v1 = v3; // Get rid of v1. A new v3 will be chosen in the next iteration.
          v1A = v3A;
          v1B = v3B;
          //n = Vector3F.Cross(v3 - v0, v2 - v0);
          // ----- Optimized version:
          Vector3F v3MinusV0;
          v3MinusV0.X = v3.X - v0.X;
          v3MinusV0.Y = v3.Y - v0.Y;
          v3MinusV0.Z = v3.Z - v0.Z;
          Vector3F v2MinusV0;
          v2MinusV0.X = v2.X - v0.X;
          v2MinusV0.Y = v2.Y - v0.Y;
          v2MinusV0.Z = v2.Z - v0.Z;
          n.X = v3MinusV0.Y * v2MinusV0.Z - v3MinusV0.Z * v2MinusV0.Y;
          n.Y = v3MinusV0.Z * v2MinusV0.X - v3MinusV0.X * v2MinusV0.Z;
          n.Z = v3MinusV0.X * v2MinusV0.Y - v3MinusV0.Y * v2MinusV0.X;
          continue;
        }

        // If come to here, then we have found a valid portal to begin with.
        // (We have a tetrahedron that contains the ray (v0 to origin)).
        break;
      }

      // Refine the portal
      while (true)
      {
        iterationCount++;

        // Store old n. Numerical inaccuracy can lead to endless loops where n is constant.
        Vector3F oldN = n;

        // Compute outward pointing normal of the portal
        //n = Vector3F.Cross(v2 - v1, v3 - v1);
        Vector3F v2MinusV1;
        v2MinusV1.X = v2.X - v1.X;
        v2MinusV1.Y = v2.Y - v1.Y;
        v2MinusV1.Z = v2.Z - v1.Z;
        Vector3F v3MinusV1;
        v3MinusV1.X = v3.X - v1.X;
        v3MinusV1.Y = v3.Y - v1.Y;
        v3MinusV1.Z = v3.Z - v1.Z;
        n.X = v2MinusV1.Y * v3MinusV1.Z - v2MinusV1.Z * v3MinusV1.Y;
        n.Y = v2MinusV1.Z * v3MinusV1.X - v2MinusV1.X * v3MinusV1.Z;
        n.Z = v2MinusV1.X * v3MinusV1.Y - v2MinusV1.Y * v3MinusV1.X;

        //if (!n.TryNormalize())
        // ----- Optimized version:
        float nLengthSquared = n.LengthSquared;
        if (nLengthSquared < Numeric.EpsilonFSquared)
        {
          // The portal is degenerate (some vertices of v1, v2, v3 are identical).
          // This can happen for coplanar shapes, e.g. long thin triangles in the 
          // same plane. The portal (v1, v2, v3) is a line segment.
          // This might be a contact or not. We use the GJK as a fallback to check this case.

          if (_gjk == null)
            _gjk = new Gjk(CollisionDetection);

          _gjk.ComputeCollision(contactSet, CollisionQueryType.Boolean);
          if (contactSet.HaveContact == false)
            return Vector3F.Zero;

          // GJK reports a contact - but it cannot compute contact positions.
          // We use the best point on the current portal as the contact point.

          // Find the point closest to the origin.
          float u, v, w;
          GeometryHelper.GetClosestPoint(new Triangle(v1, v2, v3), Vector3F.Zero, out u, out v, out w);
          Vector3F vClosest = u * v1 + v * v2 + w * v3;

          // We have not found a separating axis so far. --> Contact.
          contactSet.HaveContact = true;
          if (type == CollisionQueryType.Boolean)
            return Vector3F.Zero;

          // The points on the objects have the same barycentric coordinates.
          Vector3F pointOnA = u * v1A + v * v2A + w * v3A;
          Vector3F pointOnB = u * v1B + v * v2B + w * v3B;

          Vector3F normal = pointOnA - pointOnB;
          if (!normal.TryNormalize())
          {
            if (contactSet.IsPreferredNormalAvailable)
              normal = contactSet.PreferredNormal;
            else
              normal = Vector3F.UnitY;
          }

          Vector3F position = (pointOnA + pointOnB) / 2;
          float penetrationDepth = vClosest.Length;
          Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
          ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);

          return Vector3F.Zero;
        }

        // ----- Optimized version: Rest of n.TryNormalize():
        float nLength = (float)Math.Sqrt(nLengthSquared);
        float scale = 1.0f / nLength;
        n.X *= scale;
        n.Y *= scale;
        n.Z *= scale;

        // Separating axis test:
        // Testing > instead of >= is important otherwise coplanar triangles may report false contacts 
        // because the portal is in the same plane as the origin.
        if (!contactSet.HaveContact 
            && v1.X * n.X + v1.Y * n.Y + v1.Z * n.Z > 0) // Optimized version of && Vector3F.Dot(v1, n) > 0)
        {
          // Portal points aways from origin --> Origin is in the tetrahedron.
          contactSet.HaveContact = true;
          if (type == CollisionQueryType.Boolean)
            return Vector3F.Zero;
        }

        // Find new support point.
        //Vector3F v4A = poseA.ToWorldPosition(shapeA.GetSupportPoint(orientationAInverse * -n, scaleA));
        //Vector3F v4B = poseB.ToWorldPosition(shapeB.GetSupportPoint(orientationBInverse * n, scaleB));
        //Vector3F v4 = v4B - v4A;

        // ----- Optimized version:
        Vector3F supportDirectionA;
        supportDirectionA.X = -(orientationAInverse.M00 * n.X + orientationAInverse.M01 * n.Y + orientationAInverse.M02 * n.Z);
        supportDirectionA.Y = -(orientationAInverse.M10 * n.X + orientationAInverse.M11 * n.Y + orientationAInverse.M12 * n.Z);
        supportDirectionA.Z = -(orientationAInverse.M20 * n.X + orientationAInverse.M21 * n.Y + orientationAInverse.M22 * n.Z);
        Vector3F supportPointA = shapeA.GetSupportPoint(supportDirectionA, scaleA);
        Vector3F v4A;
        v4A.X = poseA.Orientation.M00 * supportPointA.X + poseA.Orientation.M01 * supportPointA.Y + poseA.Orientation.M02 * supportPointA.Z + poseA.Position.X;
        v4A.Y = poseA.Orientation.M10 * supportPointA.X + poseA.Orientation.M11 * supportPointA.Y + poseA.Orientation.M12 * supportPointA.Z + poseA.Position.Y;
        v4A.Z = poseA.Orientation.M20 * supportPointA.X + poseA.Orientation.M21 * supportPointA.Y + poseA.Orientation.M22 * supportPointA.Z + poseA.Position.Z;
        Vector3F supportDirectionB;
        supportDirectionB.X = orientationBInverse.M00 * n.X + orientationBInverse.M01 * n.Y + orientationBInverse.M02 * n.Z;
        supportDirectionB.Y = orientationBInverse.M10 * n.X + orientationBInverse.M11 * n.Y + orientationBInverse.M12 * n.Z;
        supportDirectionB.Z = orientationBInverse.M20 * n.X + orientationBInverse.M21 * n.Y + orientationBInverse.M22 * n.Z;
        Vector3F supportPointB = shapeB.GetSupportPoint(supportDirectionB, scaleB);
        Vector3F v4B;
        v4B.X = poseB.Orientation.M00 * supportPointB.X + poseB.Orientation.M01 * supportPointB.Y + poseB.Orientation.M02 * supportPointB.Z + poseB.Position.X;
        v4B.Y = poseB.Orientation.M10 * supportPointB.X + poseB.Orientation.M11 * supportPointB.Y + poseB.Orientation.M12 * supportPointB.Z + poseB.Position.Y;
        v4B.Z = poseB.Orientation.M20 * supportPointB.X + poseB.Orientation.M21 * supportPointB.Y + poseB.Orientation.M22 * supportPointB.Z + poseB.Position.Z;
        Vector3F v4 = v4B - v4A;

        // Separating axis test:
        if (!contactSet.HaveContact       // <--- New (see below).
            && v4.X * n.X + v4.Y * n.Y + v4.Z * n.Z < 0)  // Optimized version of && Vector3F.Dot(v4, n) < 0)
        {
          // Following assert can fail. For example if the above dot product returns -0.000000001
          // for nearly perfectly touching objects. Therefore I have added the condition
          // hit == false to the condition.
          return Vector3F.Zero;
        }

        // Test if we have refined more than the collision epsilon.
        // Condition 1: Project the point difference v4-v3 onto normal n and check whether we have
        // improved in this direction.
        // Condition 2: If n has not changed, then we couldn't improve anymore. This is caused
        // by numerical problems, e.g. when a large object (>10000) is checked.
        //if (Vector3F.Dot(v4 - v3, n) <= CollisionDetection.Epsilon
        // ----- Optimized version:
        if ((v4.X - v3.X) * n.X + (v4.Y - v3.Y) * n.Y + (v4.Z - v3.Z) * n.Z <= CollisionDetection.Epsilon
           || Vector3F.AreNumericallyEqual(n, oldN)
           || iterationCount >= iterationLimit)
        {
          // We have the final portal.
          if (!contactSet.HaveContact)
            return Vector3F.Zero;

          if (type == CollisionQueryType.Boolean)
            return Vector3F.Zero;

          // Find the point closest to the origin.
          float u, v, w;
          GeometryHelper.GetClosestPoint(new Triangle(v1, v2, v3), Vector3F.Zero, out u, out v, out w);

          // Note: If u, v or w is 0 or 1, then the point was probably outside portal triangle.
          // We can use the returned data, but re-running MPR will give us a better contact.

          Vector3F closest = u * v1 + v * v2 + w * v3;

          // The points on the objects have the same barycentric coordinates.
          Vector3F pointOnA = u * v1A + v * v2A + w * v3A;
          Vector3F pointOnB = u * v1B + v * v2B + w * v3B;

          // Use difference between points as normal direction, only if it can be normalized.
          Vector3F normal = pointOnA - pointOnB;
          if (!normal.TryNormalize())
            normal = -n;   // Else use the inverted normal of the portal.

          Vector3F position = (pointOnA + pointOnB) / 2;
          float penetrationDepth = closest.Length;
          Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
          ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);

          // If real closest point is outside the portal triangle, then one of u, v, w will
          // be exactly 0 or 1. In this case we should run a new MPR with the portal ray n.
          if (u == 0 || v == 0 || w == 0 || u == 1 || v == 1 || w == 1)
            return n;

          return Vector3F.Zero;
        }

        // Now we have a new point v4 and have to make a new portal by eliminating v1, v2 or v3.
        // The possible new tetrahedron faces are: (v0, v1, v4), (v0, v4, v2), (v0, v4, v3) 
        // We don't know the orientation yet.
        // Test with the ORIENT3D test.
        //Vector3F cross = Vector3F.Cross(v4, v0);
        // ----- Optimized version:
        Vector3F cross;
        cross.X = v4.Y * v0.Z - v4.Z * v0.Y;
        cross.Y = v4.Z * v0.X - v4.X * v0.Z;
        cross.Z = v4.X * v0.Y - v4.Y * v0.X;

        //if (Vector3F.Dot(v1, cross) > 0)
        if (v1.X * cross.X + v1.Y * cross.Y + v1.Z * cross.Z > 0)
        {
          // Eliminate v3 or v1.
          //if (Vector3F.Dot(v2, cross) > 0)
          if (v2.X * cross.X + v2.Y * cross.Y + v2.Z * cross.Z > 0)
          {
            v1 = v4;
            v1A = v4A;
            v1B = v4B;
          }
          else
          {
            v3 = v4;
            v3A = v4A;
            v3B = v4B;
          }
        }
        else
        {
          // Eliminate v1 or v2.
          //if (Vector3F.Dot(v3, cross) > 0)
          if (v3.X * cross.X + v3.Y * cross.Y + v3.Z * cross.Z > 0)
          {
            v2 = v4;
            v2A = v4A;
            v2B = v4B;
          }
          else
          {
            v1 = v4;
            v1A = v4A;
            v1B = v4B;
          }
        }
      }
    }
    #endregion
  }
}
