// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Partitioning;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="TriangleMeshShape"/> vs. any 
  /// other <see cref="Shape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes. This
  /// algorithm will call other algorithms to compute collision of triangles.
  /// </remarks>
  public partial class TriangleMeshAlgorithm : CollisionAlgorithm
  {
    /// <summary>
    /// This value determines when contact welding should be performed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To avoid bad contact normals, a process called "welding" is performed when the contact
    /// normal deviates from the triangle normals. When the dot product of a contact normal and the
    /// related triangle normal is less than <see cref="WeldingLimit"/>, the welding process checks
    /// and improves the contact.
    /// </para>
    /// <para>
    /// The default value is 0.99.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
    public static float WeldingLimit = 0.99f;


    private static readonly ResourcePool<ClosestPointCallback> ClosestPointCallbacks =
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
          callback.TestTriangleA = null;
          callback.TestTriangleB = null;
        });


    /// <summary>
    /// Initializes a new instance of the <see cref="TriangleMeshAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public TriangleMeshAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="TriangleMeshShape"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      CollisionObject collisionObjectA = contactSet.ObjectA;
      CollisionObject collisionObjectB = contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      IGeometricObject geometricObjectB = collisionObjectB.GeometricObject;

      // Object A should be the triangle mesh, swap objects if necessary.
      // When testing TriangleMeshShape vs. TriangleMeshShape with BVH, object A should be the 
      // TriangleMeshShape with BVH - except when the TriangleMeshShape with BVH is a lot smaller 
      // than the other TriangleMeshShape. (See tests below.)
      TriangleMeshShape triangleMeshShapeA = geometricObjectA.Shape as TriangleMeshShape;
      TriangleMeshShape triangleMeshShapeB = geometricObjectB.Shape as TriangleMeshShape;
      bool swapped = false;

      // Check if collision objects shapes are correct.
      if (triangleMeshShapeA == null && triangleMeshShapeB == null)
        throw new ArgumentException("The contact set must contain a triangle mesh.", "contactSet");

      Pose poseA = geometricObjectA.Pose;
      Pose poseB = geometricObjectB.Pose;
      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;

      // First we assume that we do not have a contact.
      contactSet.HaveContact = false;

      // ----- Use temporary test objects.
      var testCollisionObjectA = ResourcePools.TestCollisionObjects.Obtain();
      var testCollisionObjectB = ResourcePools.TestCollisionObjects.Obtain();

      // Create a test contact set and initialize with dummy objects.
      // (The actual collision objects are set below.)
      var testContactSet = ContactSet.Create(testCollisionObjectA, testCollisionObjectB);
      var testGeometricObjectA = TestGeometricObject.Create();
      var testGeometricObjectB = TestGeometricObject.Create();
      var testTriangleShapeA = ResourcePools.TriangleShapes.Obtain();
      var testTriangleShapeB = ResourcePools.TriangleShapes.Obtain();

      try
      {
        if (triangleMeshShapeA != null
            && triangleMeshShapeA.Partition != null
            && triangleMeshShapeB != null
            && triangleMeshShapeB.Partition != null
            && (type != CollisionQueryType.ClosestPoints
                || triangleMeshShapeA.Partition is ISupportClosestPointQueries<int>))
        {
          #region ----- TriangleMesh with BVH vs. TriangleMesh with BVH -----

          Debug.Assert(swapped == false, "Why did we swap the objects? Order of objects is fine.");

          // Find collision algorithm for triangle vs. triangle (used in AddTriangleTriangleContacts()).
          if (_triangleTriangleAlgorithm == null)
            _triangleTriangleAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(TriangleShape), typeof(TriangleShape)];

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
              overlaps = triangleMeshShapeB.Partition.GetOverlaps(
                scaleB,
                geometricObjectB.Pose,
                triangleMeshShapeA.Partition,
                scaleA,
                geometricObjectA.Pose);
            }
            else
            {
              overlaps = triangleMeshShapeA.Partition.GetOverlaps(
                scaleA,
                geometricObjectA.Pose,
                triangleMeshShapeB.Partition,
                scaleB,
                geometricObjectB.Pose);
            }

            foreach (var overlap in overlaps)
            {
              if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
                break;   // We can abort early.

              AddTriangleTriangleContacts(
                contactSet,
                overlapsSwapped ? overlap.Second : overlap.First,
                overlapsSwapped ? overlap.First : overlap.Second,
                type,
                testContactSet,
                testCollisionObjectA,
                testGeometricObjectA,
                testTriangleShapeA,
                testCollisionObjectB,
                testGeometricObjectB,
                testTriangleShapeB);
            }
          }
          else
          {
            // ----- Closest-Point Query
            var callback = ClosestPointCallbacks.Obtain();
            callback.CollisionAlgorithm = this;
            callback.Swapped = false;
            callback.ContactSet = contactSet;
            callback.TestContactSet = testContactSet;
            callback.TestCollisionObjectA = testCollisionObjectA;
            callback.TestCollisionObjectB = testCollisionObjectB;
            callback.TestGeometricObjectA = testGeometricObjectA;
            callback.TestGeometricObjectB = testGeometricObjectB;
            callback.TestTriangleA = testTriangleShapeA;
            callback.TestTriangleB = testTriangleShapeB;

            ((ISupportClosestPointQueries<int>)triangleMeshShapeA.Partition)
              .GetClosestPointCandidates(
                scaleA,
                geometricObjectA.Pose,
                triangleMeshShapeB.Partition,
                scaleB,
                geometricObjectB.Pose,
                callback.HandlePair);

            ClosestPointCallbacks.Recycle(callback);
          }
          #endregion
        }
        else
        {
          Aabb aabbOfA = geometricObjectA.Aabb;
          Aabb aabbOfB = geometricObjectB.Aabb;
          float largestExtentA = aabbOfA.Extent.LargestComponent;
          float largestExtentB = aabbOfB.Extent.LargestComponent;

          // Choose which object should be A.
          if (triangleMeshShapeA == null)
          {
            // A is no TriangleMesh. B must be a TriangleMesh.
            swapped = true;
          }
          else if (triangleMeshShapeB == null)
          {
            // A is a TriangleMesh and B is no TriangleMesh.
          }
          else if (triangleMeshShapeA.Partition != null)
          {
            // A is a TriangleMesh with BVH and B is a TriangleMesh.
            // We want to test TriangleMesh with BVH vs. * - unless the TriangleMesh with BVH is a lot 
            // smaller than the TriangleMesh.
            if (largestExtentA * 2 < largestExtentB)
              swapped = true;
          }
          else if (triangleMeshShapeB.Partition != null)
          {
            // A is a TriangleMesh and B is a TriangleMesh with BVH.
            // We want to test TriangleMesh with BVH vs. * - unless the TriangleMesh BVH is a lot 
            // smaller than the TriangleMesh.
            if (largestExtentB * 2 >= largestExtentA)
              swapped = true;
          }
          else
          {
            // A and B are normal triangle meshes. A should be the larger object.
            if (largestExtentA < largestExtentB)
              swapped = true;
          }

          if (swapped)
          {
            // Swap all variables.
            MathHelper.Swap(ref collisionObjectA, ref collisionObjectB);
            MathHelper.Swap(ref geometricObjectA, ref geometricObjectB);
            MathHelper.Swap(ref aabbOfA, ref aabbOfB);
            MathHelper.Swap(ref largestExtentA, ref largestExtentB);
            MathHelper.Swap(ref triangleMeshShapeA, ref triangleMeshShapeB);
            MathHelper.Swap(ref poseA, ref poseB);
            MathHelper.Swap(ref scaleA, ref scaleB);
          }

          if (triangleMeshShapeB == null
              && type != CollisionQueryType.ClosestPoints
              && largestExtentA * 2 < largestExtentB)
          {
            // B is a very large object and no triangle mesh. 
            // Make a AABB vs. Shape of B test for quick rejection.
            BoxShape testBoxShape = ResourcePools.BoxShapes.Obtain();
            testBoxShape.Extent = aabbOfA.Extent;
            testGeometricObjectA.Shape = testBoxShape;
            testGeometricObjectA.Scale = Vector3F.One;
            testGeometricObjectA.Pose = new Pose(aabbOfA.Center);

            testCollisionObjectA.SetInternal(collisionObjectA, testGeometricObjectA);

            Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");
            testContactSet.Reset(testCollisionObjectA, collisionObjectB);

            CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[testContactSet];
            collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Boolean);

            ResourcePools.BoxShapes.Recycle(testBoxShape);

            if (!testContactSet.HaveContact)
            {
              contactSet.HaveContact = false;
              return;
            }
          }

          if (triangleMeshShapeA.Partition != null
              && (type != CollisionQueryType.ClosestPoints
                || triangleMeshShapeA.Partition is ISupportClosestPointQueries<int>))
          {
            #region ----- TriangleMesh BVH vs. * -----

            // Get AABB of B in local space of A.
            var aabbBInA = geometricObjectB.Shape.GetAabb(scaleB, poseA.Inverse * poseB);

            // Apply inverse scaling to do the AABB-tree checks in the unscaled local space of A.
            aabbBInA.Scale(Vector3F.One / scaleA);

            if (type != CollisionQueryType.ClosestPoints)
            {
              // Boolean or Contact Query
              foreach (var triangleIndex in triangleMeshShapeA.Partition.GetOverlaps(aabbBInA))
              {
                if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
                  break; // We can abort early.

                AddTriangleContacts(
                  contactSet,
                  swapped,
                  triangleIndex,
                  type,
                  testContactSet,
                  testCollisionObjectA,
                  testGeometricObjectA,
                  testTriangleShapeA);
              }
            }
            else if (type == CollisionQueryType.ClosestPoints)
            {
              // Closest-Point Query

              var callback = ClosestPointCallbacks.Obtain();
              callback.CollisionAlgorithm = this;
              callback.Swapped = swapped;
              callback.ContactSet = contactSet;
              callback.TestContactSet = testContactSet;
              callback.TestCollisionObjectA = testCollisionObjectA;
              callback.TestCollisionObjectB = testCollisionObjectB;
              callback.TestGeometricObjectA = testGeometricObjectA;
              callback.TestGeometricObjectB = testGeometricObjectB;
              callback.TestTriangleA = testTriangleShapeA;
              callback.TestTriangleB = testTriangleShapeB;

              ((ISupportClosestPointQueries<int>)triangleMeshShapeA.Partition)
                .GetClosestPointCandidates(
                  aabbBInA,
                  float.PositiveInfinity,
                  callback.HandleItem);

              ClosestPointCallbacks.Recycle(callback);
            }
            #endregion
          }
          else
          {
            #region ----- TriangleMesh vs. * -----

            // Find an upper bound for the distance we have to search.
            // If object are in contact or we make contact/boolean query, then the distance is 0.
            float closestPairDistance;
            if (contactSet.HaveContact || type != CollisionQueryType.ClosestPoints)
            {
              closestPairDistance = 0;
            }
            else
            {
              // Make first guess for closest pair: inner point of B to inner point of mesh.
              Vector3F innerPointA = poseA.ToWorldPosition(geometricObjectA.Shape.InnerPoint * scaleA);
              Vector3F innerPointB = poseB.ToWorldPosition(geometricObjectB.Shape.InnerPoint * scaleB);
              closestPairDistance = (innerPointB - innerPointA).Length + CollisionDetection.Epsilon;
            }

            // The search-space is a space where the closest points must lie in.
            Vector3F minimum = aabbOfB.Minimum - new Vector3F(closestPairDistance);
            Vector3F maximum = aabbOfB.Maximum + new Vector3F(closestPairDistance);
            Aabb searchSpaceAabb = new Aabb(minimum, maximum);

            // Test all triangles.
            ITriangleMesh triangleMeshA = triangleMeshShapeA.Mesh;
            int numberOfTriangles = triangleMeshA.NumberOfTriangles;
            for (int i = 0; i < numberOfTriangles; i++)
            {
              // TODO: GetTriangle is performed twice! Here and in AddTriangleContacts() below!
              Triangle triangle = triangleMeshA.GetTriangle(i);

              testTriangleShapeA.Vertex0 = triangle.Vertex0 * scaleA;
              testTriangleShapeA.Vertex1 = triangle.Vertex1 * scaleA;
              testTriangleShapeA.Vertex2 = triangle.Vertex2 * scaleA;

              // Make AABB test with search space.
              if (GeometryHelper.HaveContact(searchSpaceAabb, testTriangleShapeA.GetAabb(poseA)))
              {
                // IMPORTANT: Info in triangleShape is destroyed in this method!
                // Triangle is given to the method so that method does not allocate garbage.
                AddTriangleContacts(
                  contactSet,
                  swapped,
                  i,
                  type,
                  testContactSet,
                  testCollisionObjectA,
                  testGeometricObjectA,
                  testTriangleShapeA);

                // We have contact and stop for boolean queries.
                if (contactSet.HaveContact && type == CollisionQueryType.Boolean)
                  break;

                if (closestPairDistance > 0 && contactSet.HaveContact
                    || contactSet.Count > 0 && -contactSet[contactSet.Count - 1].PenetrationDepth < closestPairDistance)
                {
                  // Reduce search space
                  // Note: contactSet can contain several contacts. We assume that the last contact
                  // is the newest one and check only this.
                  if (contactSet.Count > 0)
                    closestPairDistance = Math.Max(0, -contactSet[contactSet.Count - 1].PenetrationDepth);
                  else
                    closestPairDistance = 0;

                  searchSpaceAabb.Minimum = aabbOfB.Minimum - new Vector3F(closestPairDistance);
                  searchSpaceAabb.Maximum = aabbOfB.Maximum + new Vector3F(closestPairDistance);
                }
              }
            }
            #endregion
          }
        }
      }
      finally
      {
        testContactSet.Recycle();
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectA);
        ResourcePools.TestCollisionObjects.Recycle(testCollisionObjectB);
        testGeometricObjectB.Recycle();
        testGeometricObjectA.Recycle();
        ResourcePools.TriangleShapes.Recycle(testTriangleShapeB);
        ResourcePools.TriangleShapes.Recycle(testTriangleShapeA);
      }
    }


    // testXxx are initialized objects which are re-used to avoid a lot of GC garbage.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    private void AddTriangleContacts(ContactSet contactSet,
                                     bool swapped,
                                     int triangleIndex,
                                     CollisionQueryType type,
                                     ContactSet testContactSet,
                                     CollisionObject testCollisionObject,
                                     TestGeometricObject testGeometricObject,
                                     TriangleShape testTriangle)
    {
      // Object A should be the triangle mesh.
      CollisionObject collisionObjectA = (swapped) ? contactSet.ObjectB : contactSet.ObjectA;
      CollisionObject collisionObjectB = (swapped) ? contactSet.ObjectA : contactSet.ObjectB;
      IGeometricObject geometricObjectA = collisionObjectA.GeometricObject;
      var triangleMeshShape = ((TriangleMeshShape)geometricObjectA.Shape);
      Triangle triangle = triangleMeshShape.Mesh.GetTriangle(triangleIndex);
      Pose poseA = geometricObjectA.Pose;
      Vector3F scaleA = geometricObjectA.Scale;

      // Find collision algorithm. 
      CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(TriangleShape), collisionObjectB.GeometricObject.Shape.GetType()];

      // Apply scaling.
      testTriangle.Vertex0 = triangle.Vertex0 * scaleA;
      testTriangle.Vertex1 = triangle.Vertex1 * scaleA;
      testTriangle.Vertex2 = triangle.Vertex2 * scaleA;

      // Set the shape temporarily to the current triangles.
      testGeometricObject.Shape = testTriangle;
      testGeometricObject.Scale = Vector3F.One;
      testGeometricObject.Pose = poseA;

      testCollisionObject.SetInternal(collisionObjectA, testGeometricObject);

      // Make a temporary contact set.
      // (Object A and object B should have the same order as in contactSet; otherwise we couldn't 
      // simply merge them.)
      Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");
      if (swapped)
        testContactSet.Reset(collisionObjectB, testCollisionObject);
      else
        testContactSet.Reset(testCollisionObject, collisionObjectB);

      if (type == CollisionQueryType.Boolean)
      {
        collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Boolean);
        contactSet.HaveContact = contactSet.HaveContact || testContactSet.HaveContact;
      }
      else
      {
        // No perturbation test. Most triangle mesh shapes are either complex and automatically
        // have more contacts. Or they are complex and will not be used for stacking
        // where full contact sets would be needed.
        testContactSet.IsPerturbationTestAllowed = false;

        // TODO: Copy separating axis info and similar things into triangleContactSet. 
        // But currently this info is not used in the queries.

        // For closest points: If we know that we have a contact, then we can make a 
        // faster contact query instead of a closest-point query.
        CollisionQueryType queryType = (contactSet.HaveContact) ? CollisionQueryType.Contacts : type;
        collisionAlgorithm.ComputeCollision(testContactSet, queryType);
        contactSet.HaveContact = contactSet.HaveContact || testContactSet.HaveContact;

        if (testContactSet.HaveContact && testContactSet.Count > 0 && !triangleMeshShape.IsTwoSided)
        {
          // To compute the triangle normal in world space we take the normal of the unscaled 
          // triangle and transform the normal with: (M^-1)^T = 1 / scale
          Vector3F triangleNormalLocal = Vector3F.Cross(triangle.Vertex1 - triangle.Vertex0, triangle.Vertex2 - triangle.Vertex0) / scaleA;
          Vector3F triangleNormal = poseA.ToWorldDirection(triangleNormalLocal);
          if (triangleNormal.TryNormalize())
          {
            var preferredNormal = swapped ? -triangleNormal : triangleNormal;

            // ----- Remove bad normal.
            // Triangles are double sided, but meshes are single sided.
            // --> Remove contacts where the contact normal points into the wrong direction.
            ContactHelper.RemoveBadContacts(testContactSet, preferredNormal, -Numeric.EpsilonF);

            if (testContactSet.Count > 0 && triangleMeshShape.EnableContactWelding)
            {
              var contactDotTriangle = Vector3F.Dot(testContactSet[0].Normal, preferredNormal);
              if (contactDotTriangle < WeldingLimit)
              {
                // Bad normal. Perform welding.

                Vector3F contactPositionOnTriangle = swapped
                                                       ? testContactSet[0].PositionBLocal / scaleA
                                                       : testContactSet[0].PositionALocal / scaleA;

                Vector3F neighborNormal;
                float triangleDotNeighbor;
                GetNeighborNormal(triangleIndex, triangle, contactPositionOnTriangle, triangleNormal, triangleMeshShape, poseA, scaleA, out neighborNormal, out triangleDotNeighbor);

                if (triangleDotNeighbor < float.MaxValue && Numeric.IsLess(contactDotTriangle, triangleDotNeighbor))
                {
                  // Normal is not in allowed range.
                  // Test again in triangle normal direction.

                  Contact c0 = testContactSet[0];
                  testContactSet.RemoveAt(0);

                  testContactSet.Clear();
                  testContactSet.PreferredNormal = preferredNormal;
                  collisionAlgorithm.ComputeCollision(testContactSet, queryType);
                  testContactSet.PreferredNormal = Vector3F.Zero;

                  if (testContactSet.Count > 0)
                  {
                    Contact c1 = testContactSet[0];
                    float contact1DotTriangle = Vector3F.Dot(c1.Normal, preferredNormal);

                    // We use c1 instead of c0 if it has lower penetration depth (then it is simply
                    // better). Or we use c1 if the penetration depth increase is in an allowed range
                    // and c1 has a normal in the allowed range.
                    if (c1.PenetrationDepth < c0.PenetrationDepth 
                        || Numeric.IsGreaterOrEqual(contact1DotTriangle, triangleDotNeighbor)
                           && c1.PenetrationDepth < c0.PenetrationDepth + CollisionDetection.ContactPositionTolerance)
                    {
                      c0.Recycle();
                      c0 = c1;
                      testContactSet.RemoveAt(0);
                      contactDotTriangle = contact1DotTriangle;
                    }
                  }

                  if (Numeric.IsLess(contactDotTriangle, triangleDotNeighbor))
                  {
                    // Clamp contact to allowed normal:
                    // We keep the contact position on the mesh and the penetration depth. We set
                    // a new normal and compute the other related values for this normal.
                    if (!swapped)
                    {
                      var positionAWorld = c0.PositionAWorld;
                      c0.Normal = neighborNormal;
                      var positionBWorld = positionAWorld - c0.Normal * c0.PenetrationDepth;
                      c0.Position = (positionAWorld + positionBWorld) / 2;
                      c0.PositionBLocal = testContactSet.ObjectB.GeometricObject.Pose.ToLocalPosition(positionBWorld);
                    }
                    else
                    {
                      var positionBWorld = c0.PositionBWorld;
                      c0.Normal = -neighborNormal;
                      var positionAWorld = positionBWorld + c0.Normal * c0.PenetrationDepth;
                      c0.Position = (positionAWorld + positionBWorld) / 2;
                      c0.PositionALocal = testContactSet.ObjectA.GeometricObject.Pose.ToLocalPosition(positionAWorld);
                    }
                  }

                  c0.Recycle();
                }
              }
            }
          }
        }

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
            if (swapped)
              contact.FeatureB = triangleIndex;
            else
              contact.FeatureA = triangleIndex;
            //}
          }

          // Merge the contact info.
          ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);
        }
        #endregion
      }
    }


    // Gets neighbor normal and dot product of normals for contact welding.
    private static void GetNeighborNormal(
      int triangleIndex, Triangle triangle, Vector3F contactPositionOnTriangle, Vector3F triangleNormal, 
      TriangleMeshShape triangleMeshShape, Pose poseA, Vector3F scaleA, 
      out Vector3F neighborNormal, out float triangleDotNeighbor)
    {
      // Get barycentric coordinates of contact position.
      float u, v, w;
      // TODO: GetBaryCentricFromPoint computes the triangle normal, which we already know - optimize.
      GeometryHelper.GetBarycentricFromPoint(triangle, contactPositionOnTriangle, out u, out v, out w);

      // Find neighbor triangle normal.
      // If we do not find a neighbor, we assume the neighbor has the same normal.
      neighborNormal = triangleNormal;
      triangleDotNeighbor = float.MaxValue;

      // TODO: Optimize: We could trade memory for performance and store the precomputed triangle normals.

      // If one coordinate is near 0, the contact is near an edge.
      if (u < 0.05f || v < 0.05f || w < 0.05f)
      {
        if (u < 0.05f)
        {
          int neighborIndex = triangleMeshShape.TriangleNeighbors[triangleIndex * 3 + 0];
          if (neighborIndex >= 0)
          {
            Triangle neighbor = triangleMeshShape.Mesh.GetTriangle(neighborIndex);
            var newNeighborNormal = poseA.ToWorldDirection(neighbor.Normal / scaleA);
              // TODO: Optimize: neighbor.Normal normalizes the normal but we denormalize it with 1/scaleA.
            if (newNeighborNormal.TryNormalize())
            {
              float dot = Vector3F.Dot(triangleNormal, newNeighborNormal);
              if (dot < triangleDotNeighbor)
              {
                triangleDotNeighbor = dot;
                neighborNormal = newNeighborNormal;
              }
            }
          }
        }
        if (v < 0.05f)
        {
          int neighborIndex = triangleMeshShape.TriangleNeighbors[triangleIndex * 3 + 1];
          if (neighborIndex >= 0)
          {
            Triangle neighbor = triangleMeshShape.Mesh.GetTriangle(neighborIndex);
            var newNeighborNormal = poseA.ToWorldDirection(neighbor.Normal / scaleA);
            if (newNeighborNormal.TryNormalize())
            {
              float dot = Vector3F.Dot(triangleNormal, newNeighborNormal);
              if (dot < triangleDotNeighbor)
              {
                triangleDotNeighbor = dot;
                neighborNormal = newNeighborNormal;
              }
            }
          }
        }
        if (w < 0.05f)
        {
          int neighborIndex = triangleMeshShape.TriangleNeighbors[triangleIndex * 3 + 2];
          if (neighborIndex >= 0)
          {
            Triangle neighbor = triangleMeshShape.Mesh.GetTriangle(neighborIndex);
            var newNeighborNormal = poseA.ToWorldDirection(neighbor.Normal / scaleA);
            if (newNeighborNormal.TryNormalize())
            {
              float dot = Vector3F.Dot(triangleNormal, newNeighborNormal);
              if (dot < triangleDotNeighbor)
              {
                triangleDotNeighbor = dot;
                neighborNormal = newNeighborNormal;
              }
            }
          }
        }
      }
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Neither <paramref name="objectA"/> nor <paramref name="objectB"/> is a 
    /// <see cref="TriangleMeshShape"/>.
    /// </exception>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // Object A should be the triangle mesh, swap objects if necessary.
      if (!(objectA.GeometricObject.Shape is TriangleMeshShape))
      {
        MathHelper.Swap(ref objectA, ref objectB);
        MathHelper.Swap(ref targetPoseA, ref targetPoseB);
      }

      IGeometricObject geometricObjectA = objectA.GeometricObject;
      IGeometricObject geometricObjectB = objectB.GeometricObject;

      TriangleMeshShape triangleMeshShapeA = geometricObjectA.Shape as TriangleMeshShape;

      // Check if collision objects shapes are correct.
      if (triangleMeshShapeA == null)
        throw new ArgumentException("One object must be a triangle mesh.");

      // Currently mesh vs. mesh CCD is not supported.
      if (objectB.GeometricObject.Shape is TriangleMeshShape)
        return 1;

      ITriangleMesh triangleMeshA = triangleMeshShapeA.Mesh;

      Pose startPoseA = geometricObjectA.Pose;
      Pose startPoseB = geometricObjectB.Pose;
      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;

      // Get an AABB of the swept B in the space of A.
      // This simplified AABB can miss some rotational movement.
      // To simplify, we assume that A is static and B is moving relative to A. 
      // In general, this is not correct! But for CCD we make this simplification.
      // We convert everything to the space of A.
      var aabbSweptBInA = geometricObjectB.Shape.GetAabb(scaleB, startPoseA.Inverse * startPoseB);
      aabbSweptBInA.Grow(geometricObjectB.Shape.GetAabb(scaleB, targetPoseA.Inverse * targetPoseB));

      // Use temporary object.
      var triangleShape = ResourcePools.TriangleShapes.Obtain();
      // (Vertices will be set in the loop below.)

      var testGeometricObject = TestGeometricObject.Create();
      testGeometricObject.Shape = triangleShape;
      testGeometricObject.Scale = Vector3F.One;
      testGeometricObject.Pose = startPoseA;

      var testCollisionObject = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObject.SetInternal(objectA, testGeometricObject);

      var collisionAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(TriangleShape), geometricObjectB.Shape.GetType()];

      float timeOfImpact = 1;
      if (triangleMeshShapeA.Partition != null)
      {
        // Apply inverse scaling to do the AABB-tree checks in the unscaled local space of A.
        aabbSweptBInA.Scale(Vector3F.One / scaleA);

        foreach (var triangleIndex in triangleMeshShapeA.Partition.GetOverlaps(aabbSweptBInA))
        {
          Triangle triangle = triangleMeshA.GetTriangle(triangleIndex);

          // Apply scale.
          triangle.Vertex0 = triangle.Vertex0 * scaleA;
          triangle.Vertex1 = triangle.Vertex1 * scaleA;
          triangle.Vertex2 = triangle.Vertex2 * scaleA;

          triangleShape.Vertex0 = triangle.Vertex0;
          triangleShape.Vertex1 = triangle.Vertex1;
          triangleShape.Vertex2 = triangle.Vertex2;

          float triangleTimeOfImpact = collisionAlgorithm.GetTimeOfImpact(
            testCollisionObject, targetPoseA,
            objectB, targetPoseB,
            allowedPenetration);

          timeOfImpact = Math.Min(timeOfImpact, triangleTimeOfImpact);
        }
      }
      else
      {
        // Test all triangles.
        int numberOfTriangles = triangleMeshA.NumberOfTriangles;
        for (int triangleIndex = 0; triangleIndex < numberOfTriangles; triangleIndex++)
        {
          Triangle triangle = triangleMeshA.GetTriangle(triangleIndex);

          // Apply scale.
          triangle.Vertex0 = triangle.Vertex0 * scaleA;
          triangle.Vertex1 = triangle.Vertex1 * scaleA;
          triangle.Vertex2 = triangle.Vertex2 * scaleA;

          // Make AABB test of triangle vs. sweep of B.
          if (!GeometryHelper.HaveContact(aabbSweptBInA, triangle.Aabb))
            continue;

          triangleShape.Vertex0 = triangle.Vertex0;
          triangleShape.Vertex1 = triangle.Vertex1;
          triangleShape.Vertex2 = triangle.Vertex2;

          float triangleTimeOfImpact = collisionAlgorithm.GetTimeOfImpact(
            testCollisionObject, targetPoseA,
            objectB, targetPoseB,
            allowedPenetration);

          timeOfImpact = Math.Min(timeOfImpact, triangleTimeOfImpact);
        }
      }

      // Recycle temporary objects.
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObject);
      testGeometricObject.Recycle();
      ResourcePools.TriangleShapes.Recycle(triangleShape);

      return timeOfImpact;
    }
  }
}
