// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  public partial class TriangleMeshAlgorithm
  {
    private CollisionAlgorithm _triangleTriangleAlgorithm;


    // The parameters 'testXxx' are initialized objects which are re-used to avoid a lot of GC garbage.
    private void AddTriangleTriangleContacts(
      ContactSet contactSet, int triangleIndexA, int triangleIndexB, CollisionQueryType type,
      ContactSet testContactSet, CollisionObject testCollisionObjectA, TestGeometricObject testGeometricObjectA,
      TriangleShape testTriangleA, CollisionObject testCollisionObjectB, TestGeometricObject testGeometricObjectB,
      TriangleShape testTriangleB)
    {
      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;
      TriangleMeshShape triangleMeshShapeA = (TriangleMeshShape)geometricObjectA.Shape;
      Triangle triangleA = triangleMeshShapeA.Mesh.GetTriangle(triangleIndexA);
      TriangleMeshShape triangleMeshShapeB = (TriangleMeshShape)geometricObjectB.Shape;
      Triangle triangleB = triangleMeshShapeB.Mesh.GetTriangle(triangleIndexB);
      Pose poseA = geometricObjectA.Pose;
      Pose poseB = geometricObjectB.Pose;
      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;

      // Apply SRT.
      Triangle transformedTriangleA;
      transformedTriangleA.Vertex0 = poseA.ToWorldPosition(triangleA.Vertex0 * scaleA);
      transformedTriangleA.Vertex1 = poseA.ToWorldPosition(triangleA.Vertex1 * scaleA);
      transformedTriangleA.Vertex2 = poseA.ToWorldPosition(triangleA.Vertex2 * scaleA);
      Triangle transformedTriangleB;
      transformedTriangleB.Vertex0 = poseB.ToWorldPosition(triangleB.Vertex0 * scaleB);
      transformedTriangleB.Vertex1 = poseB.ToWorldPosition(triangleB.Vertex1 * scaleB);
      transformedTriangleB.Vertex2 = poseB.ToWorldPosition(triangleB.Vertex2 * scaleB);

      // Make super-fast boolean check first. This is redundant if we have to compute
      // a contact with SAT below. But in stochastic benchmarks it seems to be 10% faster.
      bool haveContact = GeometryHelper.HaveContact(ref transformedTriangleA, ref transformedTriangleB);
      if (type == CollisionQueryType.Boolean)
      {
        contactSet.HaveContact = (contactSet.HaveContact || haveContact);
        return;
      }

      if (haveContact)
      {
        // Make sure the scaled triangles have the correct normal.
        // (A negative scale changes the normal/winding order. See unit test in TriangleTest.cs.)
        if (scaleA.X * scaleA.Y * scaleA.Z < 0)
          MathHelper.Swap(ref transformedTriangleA.Vertex0, ref transformedTriangleA.Vertex1);
        if (scaleB.X * scaleB.Y * scaleB.Z < 0)
          MathHelper.Swap(ref transformedTriangleB.Vertex0, ref transformedTriangleB.Vertex1);

        // Compute contact.
        Vector3F position, normal;
        float penetrationDepth;
        haveContact = TriangleTriangleAlgorithm.GetContact(
          ref transformedTriangleA, ref transformedTriangleB,
          !triangleMeshShapeA.IsTwoSided, !triangleMeshShapeB.IsTwoSided,
          out position, out normal, out penetrationDepth);

        if (haveContact)
        {
          contactSet.HaveContact = true;

          // In deep interpenetrations we might get no contact (penDepth = NaN).
          if (!Numeric.IsNaN(penetrationDepth))
          {
            Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
            contact.FeatureA = triangleIndexA;
            contact.FeatureB = triangleIndexB;
            ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
          }

          return;
        }

        // We might come here if the boolean test reports contact but the SAT test
        // does not because of numerical errors.
      }

      Debug.Assert(!haveContact);

      if (type == CollisionQueryType.Contacts)
        return;

      Debug.Assert(type == CollisionQueryType.ClosestPoints);

      if (contactSet.HaveContact)
      {
        // These triangles are separated but other parts of the meshes touches.
        // --> Abort.
        return;
      }

      // We do not have a specialized triangle-triangle closest points algorithm.
      // Fall back to the default algorithm (GJK).

      // Initialize temporary test contact set and test objects.
      // Note: We assume the triangle-triangle does not care about front/back faces.
      testTriangleA.Vertex0 = transformedTriangleA.Vertex0;
      testTriangleA.Vertex1 = transformedTriangleA.Vertex1;
      testTriangleA.Vertex2 = transformedTriangleA.Vertex2;
      testGeometricObjectA.Shape = testTriangleA;
      Debug.Assert(testGeometricObjectA.Scale == Vector3F.One);
      Debug.Assert(testGeometricObjectA.Pose == Pose.Identity);
      testCollisionObjectA.SetInternal(collisionObjectA, testGeometricObjectA);

      testTriangleB.Vertex0 = transformedTriangleB.Vertex0;
      testTriangleB.Vertex1 = transformedTriangleB.Vertex1;
      testTriangleB.Vertex2 = transformedTriangleB.Vertex2;
      testGeometricObjectB.Shape = testTriangleB;
      Debug.Assert(testGeometricObjectB.Scale == Vector3F.One);
      Debug.Assert(testGeometricObjectB.Pose == Pose.Identity);
      testCollisionObjectB.SetInternal(collisionObjectB, testGeometricObjectB);

      Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");
      testContactSet.Reset(testCollisionObjectA, testCollisionObjectB);

      testContactSet.IsPerturbationTestAllowed = false;
      _triangleTriangleAlgorithm.ComputeCollision(testContactSet, type);

      // Note: We expect no contact but because of numerical differences the triangle-triangle
      // algorithm could find a shallow surface contact.
      contactSet.HaveContact = (contactSet.HaveContact || testContactSet.HaveContact);

      #region ----- Merge testContactSet into contactSet -----

      if (testContactSet.Count > 0)
      {
        // Set the shape feature of the new contacts.
        int numberOfContacts = testContactSet.Count;
        for (int i = 0; i < numberOfContacts; i++)
        {
          Contact contact = testContactSet[i];
          //if (contact.Lifetime.Ticks == 0) // Currently, this check is not necessary because triangleSet does not contain old contacts.
          //{
          contact.FeatureA = triangleIndexA;
          contact.FeatureB = triangleIndexB;
          //}
        }

        // Merge the contact info.
        ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);
      }
      #endregion
    }
  }
}
