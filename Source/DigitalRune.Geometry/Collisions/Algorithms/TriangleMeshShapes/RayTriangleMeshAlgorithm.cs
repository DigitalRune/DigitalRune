// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Geometry.Meshes;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  /// <summary>
  /// Computes contact or closest-point information for <see cref="RayShape"/> vs. 
  /// <see cref="TriangleMeshShape"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class RayTriangleMeshAlgorithm : CollisionAlgorithm
  {
    // TODO: Possible optimizations:
    // - Closest point queries could be made faster with explicit ray vs. AABB checks.
    // - Contact queries could be made faster by checking if the ray vs. AABB hit is closer
    //   than the current best hit.

    private readonly TriangleMeshAlgorithm _triangleMeshAlgorithm;



    /// <summary>
    /// Initializes a new instance of the <see cref="RayTriangleMeshAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayTriangleMeshAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      _triangleMeshAlgorithm = new TriangleMeshAlgorithm(collisionDetection);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Just use normal composite shape algorithm.
        _triangleMeshAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      Debug.Assert(type != CollisionQueryType.ClosestPoints, "Closest point queries should have already been handled!");

      // Mesh = A, Ray = B
      IGeometricObject meshObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject rayObject = contactSet.ObjectB.GeometricObject;

      // Object A should be the mesh, swap objects if necessary.
      bool swapped = (meshObject.Shape is RayShape);
      if (swapped)
        MathHelper.Swap(ref rayObject, ref meshObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      TriangleMeshShape meshShape = meshObject.Shape as TriangleMeshShape;

      // Check if shapes are correct.
      if (rayShape == null || meshShape == null)
        throw new ArgumentException("The contact set must contain a ray and a triangle mesh shape.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      // Get transformations.
      Vector3F rayScale = rayObject.Scale;
      Pose rayPose = rayObject.Pose;
      Vector3F meshScale = meshObject.Scale;
      Pose meshPose = meshObject.Pose;

      // Ray in world space.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);     // Scale ray.
      rayWorld.ToWorld(ref rayPose);    // Transform ray to world space.
      
      // Ray in local scaled space of the mesh.
      Ray ray = rayWorld;
      ray.ToLocal(ref meshPose);   // Transform ray to local space of composite.

      // Ray in local unscaled space of the mesh.
      Ray rayUnscaled = ray;
      var inverseCompositeScale = Vector3F.One / meshScale;
      rayUnscaled.Scale(ref inverseCompositeScale);

      ITriangleMesh triangleMesh = meshShape.Mesh;
      bool isTwoSided = meshShape.IsTwoSided;

      if (meshShape.Partition != null)
      {
        // ----- Mesh with BVH vs. Ray -----
        foreach (var childIndex in meshShape.Partition.GetOverlaps(rayUnscaled))
        {
          Triangle triangle = triangleMesh.GetTriangle(childIndex);

          AddContact(contactSet, swapped, type, ref rayWorld, ref ray, ref triangle, childIndex, ref meshPose, ref meshScale, isTwoSided);

          if (type == CollisionQueryType.Boolean && contactSet.HaveContact)
            break; // We can abort early.
        }
      }
      else
      {
        // ----- Mesh vs. Ray -----
        var rayUnscaledDirectionInverse = new Vector3F(
        1 / rayUnscaled.Direction.X,
        1 / rayUnscaled.Direction.Y,
        1 / rayUnscaled.Direction.Z);

        float epsilon = Numeric.EpsilonF * (1 + meshObject.Aabb.Extent.Length);

        int numberOfTriangles = triangleMesh.NumberOfTriangles;
        for (int i = 0; i < numberOfTriangles; i++)
        {
          Triangle triangle = triangleMesh.GetTriangle(i);

          // Make ray vs AABB check first. We could skip this because the ray vs. triangle test
          // is also fast. But experiments (ray vs sphere mesh) have shown that making an 
          // additional ray vs. AABB test first makes the worst case more than 20% faster.
          if (GeometryHelper.HaveContact(triangle.Aabb, rayUnscaled.Origin, rayUnscaledDirectionInverse, rayUnscaled.Length, epsilon))
          {
            AddContact(contactSet, swapped, type, ref rayWorld, ref ray, ref triangle, i, ref meshPose, ref meshScale, isTwoSided);

            // We have contact and stop for boolean queries.
            if (contactSet.HaveContact && type == CollisionQueryType.Boolean)
              break;
          }
        }
      }
    }


    private void AddContact(ContactSet contactSet, 
                            bool swapped, 
                            CollisionQueryType type, 
                            ref Ray rayWorld,             // The ray in world space.
                            ref Ray rayInMesh,            // The ray in the scaled triangle mesh space.
                            ref Triangle triangle,        // The unscaled triangle in the mesh space.
                            int triangleIndex, 
                            ref Pose trianglePose,
                            ref Vector3F triangleScale, 
                            bool isTwoSided)
    {
      // This code is from GeometryHelper_Triangles.cs. Sync changes!

      Vector3F v0 = triangle.Vertex0 * triangleScale;
      Vector3F v1 = triangle.Vertex1 * triangleScale;
      Vector3F v2 = triangle.Vertex2 * triangleScale;

      Vector3F d1 = (v1 - v0);
      Vector3F d2 = (v2 - v0);
      Vector3F n = Vector3F.Cross(d1, d2);

      // Tolerance value, see SOLID, Bergen: "Collision Detection in Interactive 3D Environments".
      float ε = n.Length * Numeric.EpsilonFSquared;

      Vector3F r = rayInMesh.Direction * rayInMesh.Length;

      float δ = -Vector3F.Dot(r, n);

      // Degenerate triangle --> No hit.
      if (ε == 0.0f || Numeric.IsZero(δ, ε))
        return;

      Vector3F triangleToRayOrigin = rayInMesh.Origin - v0;
      float λ = Vector3F.Dot(triangleToRayOrigin, n) / δ;
      if (λ < 0 || λ > 1)
        return;

      // The ray hit the triangle plane.
      Vector3F u = Vector3F.Cross(triangleToRayOrigin, r);
      float μ1 = Vector3F.Dot(d2, u) / δ;
      float μ2 = Vector3F.Dot(-d1, u) / δ;
      if (μ1 + μ2 <= 1 + ε && μ1 >= -ε && μ2 >= -ε)
      {
        // Hit!
        contactSet.HaveContact = true;

        if (type == CollisionQueryType.Boolean)
          return;

        if (δ < 0 && !isTwoSided)
          return;   // Shooting into the back of a one-sided triangle - no contact.

        float penetrationDepth = λ * rayInMesh.Length;

        // Create contact info.
        Vector3F position = rayWorld.Origin + rayWorld.Direction * penetrationDepth;
        n = trianglePose.ToWorldDirection(n);

        Debug.Assert(!n.IsNumericallyZero, "Degenerate cases of ray vs. triangle should be treated above.");
        n.Normalize();

        if (δ < 0)
          n = -n;

        if (swapped)
          n = -n;
        
        Contact contact = ContactHelper.CreateContact(contactSet, position, n, penetrationDepth, true);
        
        if (swapped)
          contact.FeatureB = triangleIndex;
        else
          contact.FeatureA = triangleIndex;

        Debug.Assert(
          contactSet.ObjectA.GeometricObject.Shape is RayShape && contact.FeatureA == -1 ||
          contactSet.ObjectB.GeometricObject.Shape is RayShape && contact.FeatureB == -1,
          "RayTriangleMeshAlgorithm has set the wrong feature property.");

        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
      }
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      return _triangleMeshAlgorithm.GetTimeOfImpact(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
    }
  }
}
