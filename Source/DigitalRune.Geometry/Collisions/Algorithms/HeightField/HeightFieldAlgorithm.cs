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
  /// Computes contact or closest-point information for <see cref="HeightField"/> vs. any other 
  /// <see cref="Shape"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// This algorithm will fail if it is called for collision objects with other shapes. This
  /// algorithm will call other algorithms to compute collision with parts of the height field.
  /// </para>
  /// <para>
  /// The height field is treated as a collection of triangles. The algorithm gives the triangle a
  /// "thickness" so that collisions are detected, even if the colliding object is under the height
  /// field surface. This avoids tunneling problems.
  /// </para>
  /// </remarks>
  public partial class HeightFieldAlgorithm : CollisionAlgorithm
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
    /// The default value is 0.999.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
    public static float WeldingLimit = 0.999f;


    private readonly Action<ContactSet> _computeContactsMethod;


    /// <summary>
    /// Initializes a new instance of the <see cref="HeightFieldAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public HeightFieldAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      // Store test method to avoid garbage when using TestWithPerturbations.
      _computeContactsMethod = contactSet => ComputeCollision(contactSet, CollisionQueryType.Contacts);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// <paramref name="contactSet"/> does not contain a <see cref="HeightField"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="contactSet"/> contains a <see cref="HeightField"/> with a negative scaling.
    /// Computing collisions for height fields with a negative scaling is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      // Object A should be the height field.
      CollisionObject heightFieldCollisionObject = contactSet.ObjectA;
      CollisionObject otherCollisionObject = contactSet.ObjectB;

      // Swap objects if necessary.
      bool swapped = !(heightFieldCollisionObject.GeometricObject.Shape is HeightField);
      if (swapped)
        MathHelper.Swap(ref heightFieldCollisionObject, ref otherCollisionObject);

      IGeometricObject heightFieldGeometricObject = heightFieldCollisionObject.GeometricObject;
      IGeometricObject otherGeometricObject = otherCollisionObject.GeometricObject;
      HeightField heightField = heightFieldGeometricObject.Shape as HeightField;
      Shape otherShape = otherGeometricObject.Shape;

      // Check if collision object shapes are correct.
      if (heightField == null)
        throw new ArgumentException("The contact set must contain a height field.", "contactSet");

      if (heightField.UseFastCollisionApproximation && type != CollisionQueryType.ClosestPoints)
      {
        // If other object is convex, use the new fast collision detection algorithm.
        ConvexShape convex = otherShape as ConvexShape;
        if (convex != null)
        {
          ComputeCollisionFast(
            contactSet,
            type,
            heightFieldGeometricObject,
            otherGeometricObject,
            heightField,
            convex,
            swapped);
          return;
        }
      }

      #region ----- Precomputations -----

      Vector3F scaleHeightField = heightFieldGeometricObject.Scale;
      Vector3F scaleOther = otherGeometricObject.Scale;
      Pose heightFieldPose = heightFieldGeometricObject.Pose;

      // We do not support negative scaling. It is not clear what should happen when y is
      // scaled with a negative factor and triangle orders would be wrong... Not worth the trouble.
      if (scaleHeightField.X < 0 || scaleHeightField.Y < 0 || scaleHeightField.Z < 0)
        throw new NotSupportedException("Computing collisions for height fields with a negative scaling is not supported.");

      // Get height field and basic info.
      Vector3F heightFieldUpAxis = heightFieldPose.ToWorldDirection(Vector3F.UnitY);
      int arrayLengthX = heightField.NumberOfSamplesX;
      int arrayLengthZ = heightField.NumberOfSamplesZ;
      Debug.Assert(arrayLengthX > 1 && arrayLengthZ > 1, "A height field should contain at least 2 x 2 elements (= 1 cell).");
      float cellWidthX = heightField.WidthX * scaleHeightField.X / (arrayLengthX - 1);
      float cellWidthZ = heightField.WidthZ * scaleHeightField.Z / (arrayLengthZ - 1);

      // The search-space is the rectangular region on the height field where the closest points 
      // must lie in. For contacts we do not have to search neighbor cells. For closest-point 
      // queries and separation we have to search neighbor cells.
      // We compute the search-space using a current maximum search distance.
      float currentSearchDistance = 0;
      Contact guessedClosestPair = null;
      if (!contactSet.HaveContact && type == CollisionQueryType.ClosestPoints)
      {
        // Make a guess for the closest pair using SupportMapping or InnerPoints.
        bool isOverHole;
        guessedClosestPair = GuessClosestPair(contactSet, swapped, out isOverHole);
        if (isOverHole)
        {
          // Guesses over holes are useless. --> Check the whole terrain.
          currentSearchDistance = heightFieldGeometricObject.Aabb.Extent.Length;
        }
        else if (guessedClosestPair.PenetrationDepth < 0)
        {
          currentSearchDistance = -guessedClosestPair.PenetrationDepth;
        }
        else
        {
          contactSet.HaveContact = true;
        }
      }
      else
      {
        // Assume no contact.
        contactSet.HaveContact = false;
      }

      // Get AABB of the other object in local space of the height field.
      Aabb aabbOfOther = otherShape.GetAabb(scaleOther, heightFieldPose.Inverse * otherGeometricObject.Pose);

      float originX = heightField.OriginX * scaleHeightField.X;
      float originZ = heightField.OriginZ * scaleHeightField.Z;

      // ----- Compute the cell indices of the search-space.
      // Estimate start and end indices from our search distance.
      int xIndexStartEstimated = (int)((aabbOfOther.Minimum.X - currentSearchDistance - originX) / cellWidthX);
      int xIndexEndEstimated = (int)((aabbOfOther.Maximum.X + currentSearchDistance - originX) / cellWidthX);
      int zIndexStartEstimated = (int)((aabbOfOther.Minimum.Z - currentSearchDistance - originZ) / cellWidthZ);
      int zIndexEndEstimated = (int)((aabbOfOther.Maximum.Z + currentSearchDistance - originZ) / cellWidthZ);

      // Clamp indices to valid range.
      int xIndexMax = arrayLengthX - 2;
      int zIndexMax = arrayLengthZ - 2;

      int xIndexStart = Math.Max(xIndexStartEstimated, 0);
      int xIndexEnd = Math.Min(xIndexEndEstimated, xIndexMax);
      int zIndexStart = Math.Max(zIndexStartEstimated, 0);
      int zIndexEnd = Math.Min(zIndexEndEstimated, zIndexMax);

      // Find collision algorithm for MinkowskiSum vs. other object's shape. 
      CollisionAlgorithm collisionAlgorithm = CollisionDetection.AlgorithmMatrix[typeof(ConvexShape), otherShape.GetType()];

      int numberOfContactsInLastFrame = contactSet.Count;
      #endregion

      #region ----- Test all height field cells in the search space. -----

      // Create several temporary test objects:
      // Instead of the original height field geometric object, we test against a shape for each
      // height field triangle. For the test shape we "extrude" the triangle under the height field.
      // To create the extrusion we "add" a line segment to the triangle using a Minkowski sum.
      // TODO: We can make this faster with a special shape that knows that the child poses are Identity (instead of the standard MinkowskiSumShape).
      // This special shape could compute its InnerPoint without applying the poses.

      var triangleShape = ResourcePools.TriangleShapes.Obtain();
      // (Vertices will be set in the loop below.)

      var triangleGeometricObject = TestGeometricObject.Create();
      triangleGeometricObject.Shape = triangleShape;

      var lineSegment = ResourcePools.LineSegmentShapes.Obtain();
      lineSegment.Start = Vector3F.Zero;
      lineSegment.End = -heightField.Depth * Vector3F.UnitY;

      var lineSegmentGeometricObject = TestGeometricObject.Create();
      lineSegmentGeometricObject.Shape = lineSegment;

      var extrudedTriangleShape = TestMinkowskiSumShape.Create();
      extrudedTriangleShape.ObjectA = triangleGeometricObject;
      extrudedTriangleShape.ObjectB = lineSegmentGeometricObject;

      var extrudedTriangleGeometricObject = TestGeometricObject.Create();
      extrudedTriangleGeometricObject.Shape = extrudedTriangleShape;
      extrudedTriangleGeometricObject.Pose = heightFieldPose;

      var testCollisionObject = ResourcePools.TestCollisionObjects.Obtain();
      testCollisionObject.SetInternal(heightFieldCollisionObject, extrudedTriangleGeometricObject);

      var testContactSet = swapped ? ContactSet.Create(contactSet.ObjectA, testCollisionObject)
                                   : ContactSet.Create(testCollisionObject, contactSet.ObjectB);
      testContactSet.IsPerturbationTestAllowed = false;

      // We compute closest points with a preferred normal direction: the height field up-axis.

      // Loop over the cells in the search space.
      // (The inner loop can reduce the search space. Therefore, when we increment the indices
      // xIndex and zIndex we also check if the start indices have changed.)
      for (int xIndex = xIndexStart; xIndex <= xIndexEnd; xIndex = Math.Max(xIndexStart, xIndex + 1))
      {
        for (int zIndex = zIndexStart; zIndex <= zIndexEnd; zIndex = Math.Max(zIndexStart, zIndex + 1))
        {
          // Test the two cell triangles.
          for (int triangleIndex = 0; triangleIndex < 2; triangleIndex++)
          {
            // Get triangle 0 or 1.
            var triangle = heightField.GetTriangle(xIndex, zIndex, triangleIndex != 0);

            var triangleIsHole = Numeric.IsNaN(triangle.Vertex0.Y * triangle.Vertex1.Y * triangle.Vertex2.Y);
            if (triangleIsHole)
              continue;

            triangleShape.Vertex0 = triangle.Vertex0 * scaleHeightField;
            triangleShape.Vertex1 = triangle.Vertex1 * scaleHeightField;
            triangleShape.Vertex2 = triangle.Vertex2 * scaleHeightField;

            if (type == CollisionQueryType.Boolean)
            {
              collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Boolean);
              contactSet.HaveContact = contactSet.HaveContact || testContactSet.HaveContact;
              if (contactSet.HaveContact)
              {
                // We can stop tests here for boolean queries. 
                // Update end indices to exit the outer loops.
                xIndexEnd = -1;
                zIndexEnd = -1;
                break;
              }
            }
            else
            {
              Debug.Assert(testContactSet.Count == 0, "testContactSet needs to be cleared.");

              // If we know that we have a contact, then we can make a faster contact query
              // instead of a closest-point query.
              CollisionQueryType queryType = (contactSet.HaveContact) ? CollisionQueryType.Contacts : type;

              collisionAlgorithm.ComputeCollision(testContactSet, queryType);

              if (testContactSet.HaveContact)
              {
                contactSet.HaveContact = true;

                if (testContactSet.Count > 0)
                {
                  // Get neighbor triangle.

                  // To compute the triangle normal we take the normal of the unscaled triangle and transform
                  // the normal with: (M^-1)^T = 1 / scale
                  // Note: We cannot use the scaled vertices because negative scalings change the 
                  // face-order of the vertices.
                  Vector3F triangleNormal = triangle.Normal / scaleHeightField;
                  triangleNormal = heightFieldPose.ToWorldDirection(triangleNormal);
                  triangleNormal.TryNormalize();

                  // Assuming the last contact is the newest. (With closest-point queries
                  // and the CombinedCollisionAlgo, testContactSet[0] could be a (not so useful)
                  // closest-point result, and testContactSet[1] the better contact query result.)
                  var testContact = testContactSet[testContactSet.Count - 1];
                  var contactNormal = swapped ? -testContact.Normal : testContact.Normal;
                  if (Vector3F.Dot(contactNormal, triangleNormal) < WeldingLimit)
                  {
                    // Contact normal deviates by more than the welding limit. --> Check the contact.

                    // If we do not find a neighbor, we assume the neighbor has the same normal.
                    var neighborNormal = triangleNormal;

                    #region ----- Get Neighbor Triangle Normal -----

                    // Get barycentric coordinates of contact position.
                    Vector3F contactPositionOnHeightField = swapped ? testContact.PositionBLocal / scaleHeightField : testContact.PositionALocal / scaleHeightField;
                    float u, v, w;
                    // TODO: GetBaryCentricFromPoint computes the triangle normal, which we already know - optimize.
                    GeometryHelper.GetBarycentricFromPoint(triangle, contactPositionOnHeightField, out u, out v, out w);

                    // If one coordinate is near 0, the contact is near an edge.
                    if (u < 0.05f || v < 0.05f || w < 0.05f)
                    {
                      if (triangleIndex == 0)
                      {
                        if (u < v && u < w)
                        {
                          neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex, zIndex, true).Normal / scaleHeightField);
                          neighborNormal.TryNormalize();
                        }
                        else if (v < w)
                        {
                          if (zIndex > 0)
                          {
                            neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex, zIndex - 1, true).Normal / scaleHeightField);
                            neighborNormal.TryNormalize();
                          }
                          else
                          {
                            // The contact is at the border of the whole height field. Set a normal which disables all bad contact filtering.
                            neighborNormal = new Vector3F(float.NaN);
                          } 
                        }
                        else
                        {
                          if (xIndex > 0)
                          {
                            neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex - 1, zIndex, true).Normal / scaleHeightField);
                            neighborNormal.TryNormalize();
                          }
                          else
                          {
                            neighborNormal = new Vector3F(float.NaN);
                          }
                        }
                      }
                      else
                      {
                        if (u < v && u < w)
                        {
                          if (xIndex + 2 < arrayLengthX)
                          {
                            neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex + 1, zIndex, false).Normal / scaleHeightField);
                            neighborNormal.TryNormalize();
                          }
                          else
                          {
                            neighborNormal = new Vector3F(float.NaN);
                          }
                        }
                        else if (v < w)
                        {
                          neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex, zIndex, false).Normal / scaleHeightField);
                          neighborNormal.TryNormalize();
                        }
                        else
                        {
                          if (zIndex + 2 < arrayLengthZ)
                          {
                            neighborNormal = heightFieldPose.ToWorldDirection(heightField.GetTriangle(xIndex, zIndex + 1, true).Normal / scaleHeightField);
                            neighborNormal.TryNormalize();
                          }
                          else
                          {
                            neighborNormal = new Vector3F(float.NaN);
                          }
                        }
                      }
                    }
                    #endregion

                    // Contact normals in the range triangleNormal - neighborNormal are allowed. 
                    // Others, especially vertical contacts in slopes or horizontal normals are not 
                    // allowed.
                    var cosMinAngle = Vector3F.Dot(neighborNormal, triangleNormal) - CollisionDetection.Epsilon;
                    RemoveBadContacts(swapped, testContactSet, triangleNormal, cosMinAngle);

                    // If we have no contact yet, we retry with a preferred normal identical to the up axis.
                    // (Note: contactSet.Count will be > 0 for closest-point queries but will 
                    // probably constraint separated contacts and not real contacts.)
                    if (testContactSet.Count == 0
                        && (contactSet.Count == 0 || type == CollisionQueryType.ClosestPoints))
                    {
                      testContactSet.PreferredNormal = (swapped) ? -heightFieldUpAxis : heightFieldUpAxis;
                      collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Contacts);
                      testContactSet.PreferredNormal = Vector3F.Zero;
                      RemoveBadContacts(swapped, testContactSet, triangleNormal, cosMinAngle);
                    }

                    // If we have no contact yet, we retry with a preferred normal identical to the triangle normal.
                    // But only if the triangle normal differs significantly from the up axis.
                    if (testContactSet.Count == 0
                        && (contactSet.Count == 0 || type == CollisionQueryType.ClosestPoints)
                        && Vector3F.Dot(heightFieldUpAxis, triangleNormal) < WeldingLimit)
                    {
                      testContactSet.PreferredNormal = (swapped) ? -triangleNormal : triangleNormal;
                      collisionAlgorithm.ComputeCollision(testContactSet, CollisionQueryType.Contacts);
                      testContactSet.PreferredNormal = Vector3F.Zero;
                      RemoveBadContacts(swapped, testContactSet, triangleNormal, cosMinAngle);
                    }
                  }
                }
              }

              if (testContactSet.Count > 0)
              {
                // Remember separation distance for later.
                float separationDistance = -testContactSet[0].PenetrationDepth;

                // Set the shape feature of the new contacts.
                // The features is the height field triangle index (see HeightField documentation).
                int numberOfContacts = testContactSet.Count;
                for (int i = 0; i < numberOfContacts; i++)
                {
                  Contact contact = testContactSet[i];
                  int featureIndex = (zIndex * (arrayLengthX - 1) + xIndex) * 2 + triangleIndex;
                  if (swapped)
                    contact.FeatureB = featureIndex;
                  else
                    contact.FeatureA = featureIndex;
                }

                // Merge the contact info. (Contacts in testContactSet are recycled!)
                ContactHelper.Merge(contactSet, testContactSet, type, CollisionDetection.ContactPositionTolerance);

                #region ----- Update search space -----

                // Update search space if possible.
                // The best search distance is 0. For separation we can use the current smallest
                // separation as search distance. As soon as we have a contact, we set the 
                // search distance to 0.
                if (currentSearchDistance > 0   // No need to update search space if search distance is already 0.
                    && (contactSet.HaveContact  // If we have a contact, we set the search distance to 0.
                        || separationDistance < currentSearchDistance)) // If we have closer separation, we use this.
                {
                  // Note: We only check triangleContactSet[0] in the if condition. 
                  // triangleContactSet could contain several contacts, but we don't bother with
                  // this special case.

                  // Update search distance.
                  if (contactSet.HaveContact)
                    currentSearchDistance = 0;
                  else
                    currentSearchDistance = Math.Max(0, separationDistance);

                  // Update search space indices.
                  xIndexStartEstimated = (int)((aabbOfOther.Minimum.X - currentSearchDistance - originX) / cellWidthX);
                  xIndexEndEstimated = (int)((aabbOfOther.Maximum.X + currentSearchDistance - originX) / cellWidthX);
                  zIndexStartEstimated = (int)((aabbOfOther.Minimum.Z - currentSearchDistance - originZ) / cellWidthZ);
                  zIndexEndEstimated = (int)((aabbOfOther.Maximum.Z + currentSearchDistance - originZ) / cellWidthZ);

                  xIndexStart = Math.Max(xIndexStart, xIndexStartEstimated);
                  xIndexEnd = Math.Min(xIndexEndEstimated, xIndexMax);
                  zIndexStart = Math.Max(zIndexStart, zIndexStartEstimated);
                  zIndexEnd = Math.Min(zIndexEndEstimated, zIndexMax);
                }
                #endregion
              }
            }
          }
        }
      }

      // Recycle temporary objects.
      testContactSet.Recycle();
      ResourcePools.TestCollisionObjects.Recycle(testCollisionObject);
      extrudedTriangleGeometricObject.Recycle();
      extrudedTriangleShape.Recycle();
      lineSegmentGeometricObject.Recycle();
      ResourcePools.LineSegmentShapes.Recycle(lineSegment);
      triangleGeometricObject.Recycle();
      ResourcePools.TriangleShapes.Recycle(triangleShape);
      #endregion

      #region ----- Handle missing contact info -----

      if (contactSet.Count == 0
          && (contactSet.HaveContact && type == CollisionQueryType.Contacts || type == CollisionQueryType.ClosestPoints))
      {
        // ----- Bad contact info: 
        // We should have contact data because this is either a contact query and the objects touch
        // or this is a closest-point query.
        // Use our guess as the contact info.
        Contact closestPair = guessedClosestPair;
        bool isOverHole = false;
        if (closestPair == null)
          closestPair = GuessClosestPair(contactSet, swapped, out isOverHole);

        // Guesses over holes are useless. :-(
        if (!isOverHole)
          ContactHelper.Merge(contactSet, closestPair, type, CollisionDetection.ContactPositionTolerance);
      }
      #endregion


      if (CollisionDetection.FullContactSetPerFrame
          && type == CollisionQueryType.Contacts
          && numberOfContactsInLastFrame == 0
          && contactSet.Count > 0
          && contactSet.Count < 4)
      {
        // Try to find full contact set.
        // TODO: This can be optimized by not doing the whole overhead of ComputeCollision again.
        ContactHelper.TestWithPerturbations(
          CollisionDetection,
          contactSet,
          !swapped,   // Perturb objectB not the height field.
          _computeContactsMethod);
      }
    }


    // Remove all contacts where the angle between the contact normal and the triangle normal
    // is less than the given angle. The limit angle is given as cos(angle) (= dot product).
    private static void RemoveBadContacts(bool swapped, ContactSet testContactSet, Vector3F triangleNormal, float cosMinAngle)
    {
      // Note: We assume that we have only one contact per set.
      if (testContactSet.Count > 0)
      {
        if (!swapped)
        {
          if (Vector3F.Dot(testContactSet[0].Normal, triangleNormal) < cosMinAngle)
          {
            foreach (var contact in testContactSet)
              contact.Recycle();

            testContactSet.Clear();
          }
        }
        else
        {
          if (Vector3F.Dot(-testContactSet[0].Normal, triangleNormal) < cosMinAngle)
          {
            foreach (var contact in testContactSet)
              contact.Recycle();

            testContactSet.Clear();
          }
        }
      }
    }


    /// <summary>
    /// Guesses the closest pair.
    /// </summary>
    /// <param name="contactSet">The contact set.</param>
    /// <param name="swapped">
    /// Object A in <paramref name="contactSet"/> should be the height field. This parameter 
    /// indicates whether object A and object B in the contact set are swapped.
    /// </param>
    /// <param name="isOverHole">
    /// <see langword="true"/> if the guessed contact is over a hole and probably shouldn't be used.
    /// </param>
    /// <returns>Guess for closest pair.</returns>
    /// <remarks>
    /// For general shapes: Inner point of B to height field point "under" inner point of B. 
    /// For convex shapes: Support point of B in the "down" direction to height field point "under" 
    /// this support point.
    /// </remarks>
    private static Contact GuessClosestPair(ContactSet contactSet, bool swapped, out bool isOverHole)
    {
      // Object A should be the height field.
      IGeometricObject objectA = contactSet.ObjectA.GeometricObject;
      IGeometricObject objectB = contactSet.ObjectB.GeometricObject;

      // Swap if necessary.
      if (swapped)
        MathHelper.Swap(ref objectA, ref objectB);

      HeightField heightFieldA = (HeightField)objectA.Shape;
      Shape shapeB = objectB.Shape;

      Vector3F scaleA = objectA.Scale;
      Vector3F scaleB = objectB.Scale;
      Pose poseA = objectA.Pose;
      Pose poseB = objectB.Pose;

      // Get the height field up-axis in world space.
      Vector3F heightFieldUpAxis = poseA.ToWorldDirection(Vector3F.UnitY);

      // Get point on other object.
      Vector3F positionBLocal;
      ConvexShape shapeBAsConvex = shapeB as ConvexShape;
      if (shapeBAsConvex != null)
      {
        // Use support point for convex shapes.
        positionBLocal = shapeBAsConvex.GetSupportPoint(poseB.ToLocalDirection(-heightFieldUpAxis), scaleB);
      }
      else
      {
        // Use inner point for general shapes.
        positionBLocal = shapeB.InnerPoint * scaleB;
      }

      // Convert point (object B) into height field space (object A).
      Vector3F positionB = poseB.ToWorldPosition(positionBLocal);
      Vector3F positionBInA = poseA.ToLocalPosition(positionB);

      // Get point on the surface of the height field (object A):
      // Clamp x and z coordinate to height field widths.
      // For y coordinate get the height of the height field at the x-z position.
      float originX = heightFieldA.OriginX;
      float originZ = heightFieldA.OriginZ;
      float x = MathHelper.Clamp(positionBInA.X, originX * scaleA.X, (originX + heightFieldA.WidthX) * scaleA.X);
      float z = MathHelper.Clamp(positionBInA.Z, originZ * scaleA.Z, (originZ + heightFieldA.WidthZ) * scaleA.Z);
      float y = heightFieldA.GetHeight(x / scaleA.X, z / scaleA.Z) * scaleA.Y;        // Inverse scale applied in GetHeight() parameters.

      Vector3F positionALocal = new Vector3F(x, y, z);

      // Special handling of holes.
      isOverHole = Numeric.IsNaN(y);
      if (isOverHole)
        positionALocal = heightFieldA.InnerPoint * scaleA;

      // Convert point on height field to world space.
      Vector3F positionA = poseA.ToWorldPosition(positionALocal);

      // Use the world positions (positionA, positionB) as our closest-pair/contact guess.

      // Compute contact information.
      Vector3F position = (positionA + positionB) / 2;
      float penetrationDepth = (positionA - positionB).Length;
      bool haveContact = (positionALocal.Y >= positionBInA.Y);

      Vector3F normal = positionA - positionB;
      if (!normal.TryNormalize())
        normal = heightFieldUpAxis;

      if (swapped)
        normal = -normal;

      bool isRayHit = haveContact && shapeB is RayShape;

      if (!haveContact)
      {
        // For separation: Switch the normal and make the penetration depth negative to indicate
        // separation.
        normal = -normal;
        penetrationDepth = -penetrationDepth;
      }

      return ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, isRayHit);
    }


    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Neither <paramref name="objectA"/> nor <paramref name="objectB"/> contains a 
    /// <see cref="HeightField"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// <paramref name="objectA"/> or <paramref name="objectB"/> contains a 
    /// <see cref="HeightField"/> with a negative scaling. Computing collisions for height fields 
    /// with a negative scaling is not supported.
    /// </exception>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      if (objectA == null)
        throw new ArgumentNullException("objectA");
      if (objectB == null)
        throw new ArgumentNullException("objectB");

      // Object A should be the height field, swap objects if necessary.
      if (!(objectA.GeometricObject.Shape is HeightField))
      {
        MathHelper.Swap(ref objectA, ref objectB);
        MathHelper.Swap(ref targetPoseA, ref targetPoseB);
      }

      IGeometricObject geometricObjectA = objectA.GeometricObject;
      IGeometricObject geometricObjectB = objectB.GeometricObject;

      HeightField heightFieldA = geometricObjectA.Shape as HeightField;

      // Check if collision object shapes are correct.
      if (heightFieldA == null)
        throw new ArgumentException("One object must be a height field.");

      // Height field vs height field makes no sense.
      if (objectB.GeometricObject.Shape is HeightField)
        return 1;

      Pose startPoseA = geometricObjectA.Pose;
      Pose startPoseB = geometricObjectB.Pose;
      Vector3F scaleA = geometricObjectA.Scale;
      Vector3F scaleB = geometricObjectB.Scale;

      // We do not support negative scaling (see comments in ComputeCollision). 
      if (scaleA.X < 0 || scaleA.Y < 0 || scaleA.Z < 0)
        throw new NotSupportedException("Computing collisions for height fields with a negative scaling is not supported.");

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

      // Get height field and basic info.
      int arrayLengthX = heightFieldA.NumberOfSamplesX;
      int arrayLengthZ = heightFieldA.NumberOfSamplesZ;
      Debug.Assert(arrayLengthX > 1 && arrayLengthZ > 1, "A height field should contain at least 2 x 2 elements (= 1 cell).");
      float cellWidthX = heightFieldA.WidthX * scaleA.X / (arrayLengthX - 1);
      float cellWidthZ = heightFieldA.WidthZ * scaleA.Z / (arrayLengthZ - 1);

      float originX = heightFieldA.OriginX;
      float originZ = heightFieldA.OriginZ;

      // ----- Compute the cell indices for the AABB.
      // Estimate start and end indices from our search distance.
      int xIndexStartEstimated = (int)((aabbSweptBInA.Minimum.X - originX) / cellWidthX);
      int xIndexEndEstimated = (int)((aabbSweptBInA.Maximum.X - originX) / cellWidthX);
      int zIndexStartEstimated = (int)((aabbSweptBInA.Minimum.Z - originZ) / cellWidthZ);
      int zIndexEndEstimated = (int)((aabbSweptBInA.Maximum.Z - originZ) / cellWidthZ);

      // Clamp indices to valid range.
      int xIndexMax = arrayLengthX - 2;
      int zIndexMax = arrayLengthZ - 2;

      int xIndexStart = Math.Max(xIndexStartEstimated, 0);
      int xIndexEnd = Math.Min(xIndexEndEstimated, xIndexMax);
      int zIndexStart = Math.Max(zIndexStartEstimated, 0);
      int zIndexEnd = Math.Min(zIndexEndEstimated, zIndexMax);

      float timeOfImpact = 1;

      for (int xIndex = xIndexStart; xIndex <= xIndexEnd; xIndex = Math.Max(xIndexStart, xIndex + 1))
      {
        for (int zIndex = zIndexStart; zIndex <= zIndexEnd; zIndex = Math.Max(zIndexStart, zIndex + 1))
        {
          // Test the two cell triangles.
          for (int triangleIndex = 0; triangleIndex < 2; triangleIndex++)
          {
            // Get triangle 0 or 1.
            var triangle = heightFieldA.GetTriangle(xIndex, zIndex, triangleIndex != 0);

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
              testCollisionObject,
              targetPoseA,
              objectB,
              targetPoseB,
              allowedPenetration);

            timeOfImpact = Math.Min(timeOfImpact, triangleTimeOfImpact);
          }
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
