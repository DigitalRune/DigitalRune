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
  /// <see cref="HeightField"/>.
  /// </summary>
  /// <remarks>
  /// This algorithm will fail if it is called for collision objects with other shapes.
  /// </remarks>
  public class RayHeightFieldAlgorithm : CollisionAlgorithm
  {
    private readonly HeightFieldAlgorithm _heightFieldAlgorithm;


    /// <summary>
    /// Initializes a new instance of the <see cref="RayHeightFieldAlgorithm"/> class.
    /// </summary>
    /// <param name="collisionDetection">The collision detection service.</param>
    public RayHeightFieldAlgorithm(CollisionDetection collisionDetection)
      : base(collisionDetection)
    {
      _heightFieldAlgorithm = new HeightFieldAlgorithm(collisionDetection);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    public override void ComputeCollision(ContactSet contactSet, CollisionQueryType type)
    {
      if (type == CollisionQueryType.ClosestPoints)
      {
        // Just use normal height field shape algorithm.
        _heightFieldAlgorithm.ComputeCollision(contactSet, type);
        return;
      }

      Debug.Assert(type != CollisionQueryType.ClosestPoints, "Closest point queries should have already been handled!");

      // HeightField = A, Ray = B
      IGeometricObject heightFieldObject = contactSet.ObjectA.GeometricObject;
      IGeometricObject rayObject = contactSet.ObjectB.GeometricObject;

      // Object A should be the height field, swap objects if necessary.
      bool swapped = (heightFieldObject.Shape is RayShape);
      if (swapped)
        MathHelper.Swap(ref rayObject, ref heightFieldObject);

      RayShape rayShape = rayObject.Shape as RayShape;
      HeightField heightField = heightFieldObject.Shape as HeightField;

      // Check if shapes are correct.
      if (rayShape == null || heightField == null)
        throw new ArgumentException("The contact set must contain a ray and a height field.", "contactSet");

      // Assume no contact.
      contactSet.HaveContact = false;

      // Get transformations.
      Vector3F rayScale = rayObject.Scale;
      Pose rayPose = rayObject.Pose;
      Vector3F heightFieldScale = heightFieldObject.Scale;
      Pose heightFieldPose = heightFieldObject.Pose;

      // We do not support negative scaling. It is not clear what should happen when y is
      // scaled with a negative factor and triangle orders would be wrong... Not worth the trouble.
      if (heightFieldScale.X < 0 || heightFieldScale.Y < 0 || heightFieldScale.Z < 0)
        throw new NotSupportedException("Computing collisions for height fields with a negative scaling is not supported.");

      // Ray in world space.
      Ray rayWorld = new Ray(rayShape);
      rayWorld.Scale(ref rayScale);
      rayWorld.ToWorld(ref rayPose);

      // Ray in local scaled space of the height field.
      Ray rayScaled = rayWorld;
      rayScaled.ToLocal(ref heightFieldPose);

      // Ray in local unscaled space of the mesh.
      Ray rayUnscaled = rayScaled;
      var inverseCompositeScale = Vector3F.One / heightFieldScale;
      rayUnscaled.Scale(ref inverseCompositeScale);

      // Get height field and basic info.
      int arrayLengthX = heightField.NumberOfSamplesX;
      int arrayLengthZ = heightField.NumberOfSamplesZ;
      int numberOfCellsX = arrayLengthX - 1;
      int numberOfCellsZ = arrayLengthZ - 1;
      Debug.Assert(arrayLengthX > 1 && arrayLengthZ > 1, "A height field should contain at least 2 x 2 elements (= 1 cell).");
      float cellWidthX = heightField.WidthX / numberOfCellsX; // Unscaled!
      float cellWidthZ = heightField.WidthZ / numberOfCellsZ; // Unscaled!

      // We use a 2D-DDA traversal of the height field cells. In other words: Look at it from 
      // above. The height field is our screen and we will select the cells as if we draw
      // a pixel line. This could be made more efficient when we do not recompute values and 
      // reuse values and make incremental steps Bresenham-style.
      // See GeometryHelper_Casts.cs method HaveContact(Aabb, ray) for explanation of the 
      // ray parameter formula.

      var rayUnscaledDirectionInverse = new Vector3F(
        1 / rayUnscaled.Direction.X,
        1 / rayUnscaled.Direction.Y,
        1 / rayUnscaled.Direction.Z);

      // The position where the ray enters the current cell.
      var cellEnter = rayUnscaled.Origin; // Unscaled!!!

      var originX = heightField.OriginX;
      var originZ = heightField.OriginZ;

      // ----- Find first cell.
      int indexX = (cellEnter.X >= originX) ? (int)((cellEnter.X - originX) / cellWidthX) : -1; // (int)(...) does not return the desired result for negative values!
      if (indexX < 0)
      {
        if (rayUnscaled.Direction.X <= 0)
          return;

        float parameter = (originX - rayUnscaled.Origin.X) * rayUnscaledDirectionInverse.X;
        if (parameter > rayUnscaled.Length)
          return;      // The ray does not reach the height field.

        cellEnter = rayUnscaled.Origin + parameter * rayUnscaled.Direction;
        indexX = 0;
      }
      else if (indexX >= numberOfCellsX)
      {
        if (rayUnscaled.Direction.X >= 0)
          return;

        float parameter = (originX + heightField.WidthX - rayUnscaled.Origin.X) * rayUnscaledDirectionInverse.X;
        if (parameter > rayUnscaled.Length)
          return;      // The ray does not reach the height field.

        cellEnter = rayUnscaled.Origin + parameter * rayUnscaled.Direction;
        indexX = numberOfCellsX - 1;
      }

      int indexZ = (cellEnter.Z >= originZ) ? (int)((cellEnter.Z - originZ) / cellWidthZ) : -1;
      if (indexZ < 0)
      {
        if (rayUnscaled.Direction.Z <= 0)
          return;

        float parameter = (originZ - rayUnscaled.Origin.Z) * rayUnscaledDirectionInverse.Z;
        if (parameter > rayUnscaled.Length)
          return;      // The ray does not reach the next height field.

        cellEnter = rayUnscaled.Origin + parameter * rayUnscaled.Direction;
        // We also have to correct the indexX!
        indexX = (cellEnter.X >= originX) ? (int)((cellEnter.X - originX) / cellWidthX) : -1;
        indexZ = 0;
      }
      else if (indexZ >= numberOfCellsZ)
      {
        if (rayUnscaled.Direction.Z >= 0)
          return;

        float parameter = (originZ + heightField.WidthZ - rayUnscaled.Origin.Z) * rayUnscaledDirectionInverse.Z;
        if (parameter > rayUnscaled.Length)
          return;      // The ray does not reach the next height field.

        cellEnter = rayUnscaled.Origin + parameter * rayUnscaled.Direction;
        indexX = (cellEnter.X >= originX) ? (int)((cellEnter.X - originX) / cellWidthX) : -1;
        indexZ = numberOfCellsZ - 1;
      }

      if (indexX < 0 || indexX >= numberOfCellsX || indexZ < 0 || indexZ >= numberOfCellsZ)
        return;

      while (true)
      {
        // ----- Get triangles of current cell.
        var triangle0 = heightField.GetTriangle(indexX, indexZ, false);
        var triangle1 = heightField.GetTriangle(indexX, indexZ, true);

        // Index of first triangle.
        var triangleIndex = (indexZ * numberOfCellsX + indexX) * 2;

        float xRelative = (cellEnter.X - originX) / cellWidthX - indexX;
        float zRelative = (cellEnter.Z - originZ) / cellWidthZ - indexZ;
        bool enterSecondTriangle = (xRelative + zRelative) > 1;  // The diagonal is where xRel + zRel == 1.

        // ----- Find cell exit and move indices to next cell.
        // The position where the ray leaves the current cell.
        Vector3F cellExit;
        float nextXParameter = float.PositiveInfinity;
        if (rayUnscaled.Direction.X > 0)
          nextXParameter = (originX + (indexX + 1) * cellWidthX - rayUnscaled.Origin.X) * rayUnscaledDirectionInverse.X;
        else if (rayUnscaled.Direction.X < 0)
          nextXParameter = (originX + indexX * cellWidthX - rayUnscaled.Origin.X) * rayUnscaledDirectionInverse.X;

        float nextZParameter = float.PositiveInfinity;
        if (rayUnscaled.Direction.Z > 0)
          nextZParameter = (originZ + (indexZ + 1) * cellWidthZ - rayUnscaled.Origin.Z) * rayUnscaledDirectionInverse.Z;
        else if (rayUnscaled.Direction.Z < 0)
          nextZParameter = (originZ + indexZ * cellWidthZ - rayUnscaled.Origin.Z) * rayUnscaledDirectionInverse.Z;

        bool isLastCell = false;
        if (nextXParameter < nextZParameter)
        {
          if (rayUnscaled.Direction.X > 0)
          {
            indexX++;
            if (indexX >= numberOfCellsX) // Abort if we have left the height field.
              isLastCell = true;
          }
          else
          {
            indexX--;
            if (indexX < 0)
              isLastCell = true;
          }

          if (nextXParameter > rayUnscaled.Length)
          {
            isLastCell = true;  // The ray does not reach the next cell.
            nextXParameter = rayUnscaled.Length;
          }

          cellExit = rayUnscaled.Origin + nextXParameter * rayUnscaled.Direction;
        }
        else
        {
          if (rayUnscaled.Direction.Z > 0)
          {
            indexZ++;
            if (indexZ >= numberOfCellsZ)
              isLastCell = true;
          }
          else
          {
            indexZ--;
            if (indexZ < 0)
              isLastCell = true;
          }

          if (nextZParameter > rayUnscaled.Length)
          {
            isLastCell = true;
            nextZParameter = rayUnscaled.Length;
          }

          cellExit = rayUnscaled.Origin + nextZParameter * rayUnscaled.Direction;
        }


        // ----- We can skip cell if cell AABB is below the ray.
        var rayMinY = Math.Min(cellEnter.Y, cellExit.Y) - CollisionDetection.Epsilon;  // Apply to avoid missing collisions when ray hits a cell border.

        // The ray is above if no height field height is higher the ray height.
        // (This check handles NaN height values (holes) correctly.)
        bool rayIsAbove = !(triangle0.Vertex0.Y >= rayMinY
                            || triangle0.Vertex1.Y >= rayMinY
                            || triangle0.Vertex2.Y >= rayMinY
                            || triangle1.Vertex1.Y >= rayMinY);   // Vertex1 of triangle1 is the fourth quad vertex! 

        // ----- Test ray against the 2 triangles of the cell.
        bool triangle0IsHole = false;
        bool triangle1IsHole = false;
        if (!rayIsAbove)
        {
          // Abort if a height value is NaN (hole).
          triangle0IsHole = Numeric.IsNaN(triangle0.Vertex0.Y * triangle0.Vertex1.Y * triangle0.Vertex2.Y);
          triangle1IsHole = Numeric.IsNaN(triangle1.Vertex0.Y * triangle1.Vertex1.Y * triangle1.Vertex2.Y);

          bool contactAdded = false;
          if (enterSecondTriangle)
          {
            // Test second triangle first.
            if (!triangle1IsHole)
              contactAdded = AddContact(contactSet, swapped, type, ref rayWorld, ref rayScaled, ref triangle1, triangleIndex + 1, ref heightFieldPose, ref heightFieldScale);
            if (!contactAdded && !triangle0IsHole)
              contactAdded = AddContact(contactSet, swapped, type, ref rayWorld, ref rayScaled, ref triangle0, triangleIndex, ref heightFieldPose, ref heightFieldScale);
          }
          else
          {
            // Test first triangle first.
            if (!triangle0IsHole)
              contactAdded = AddContact(contactSet, swapped, type, ref rayWorld, ref rayScaled, ref triangle0, triangleIndex, ref heightFieldPose, ref heightFieldScale);
            if (!contactAdded && !triangle1IsHole)
              contactAdded = AddContact(contactSet, swapped, type, ref rayWorld, ref rayScaled, ref triangle1, triangleIndex + 1, ref heightFieldPose, ref heightFieldScale);
          }

          if (contactAdded)
            return;

          // We have contact and stop for boolean queries.
          if (contactSet.HaveContact && type == CollisionQueryType.Boolean)
            return;
        }

        // ----- Return simplified contact if cellEnter is below the cell.
        if (!rayIsAbove)
        {
          if (!enterSecondTriangle && !triangle0IsHole && GeometryHelper.IsInFront(triangle0, cellEnter) < 0
              || enterSecondTriangle && !triangle1IsHole && GeometryHelper.IsInFront(triangle1, cellEnter) < 0)
          {
            contactSet.HaveContact = true;

            if (type == CollisionQueryType.Boolean)
              return;

            var position = heightFieldPose.ToWorldPosition(cellEnter * heightFieldScale);
            var normal = heightFieldPose.ToWorldDirection(Vector3F.UnitY);
            if (swapped)
              normal = -normal;

            float penetrationDepth = (position - rayWorld.Origin).Length;
            Contact contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, true);
            ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
            return;
          }
        }

        // ----- Move to next cell.
        if (isLastCell)
          return;

        cellEnter = cellExit;
      }
    }


    // Returns true if a contact was added.
    private bool AddContact(ContactSet contactSet,
                            bool swapped,
                            CollisionQueryType type,
                            ref Ray rayWorld,             // The ray in world space.
                            ref Ray rayInField,           // The ray in the scaled height field space.
                            ref Triangle triangle,        // The unscaled triangle in the mesh space.
                            int triangleIndex,
                            ref Pose trianglePose,
                            ref Vector3F triangleScale)
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

      Vector3F r = rayInField.Direction * rayInField.Length;

      float δ = -Vector3F.Dot(r, n);

      // Degenerate triangle --> No hit.
      if (ε == 0.0f || Numeric.IsZero(δ, ε))
        return false;

      Vector3F triangleToRayOrigin = rayInField.Origin - v0;
      float λ = Vector3F.Dot(triangleToRayOrigin, n) / δ;
      if (λ < 0 || λ > 1)
        return false;

      // The ray hit the triangle plane.
      Vector3F u = Vector3F.Cross(triangleToRayOrigin, r);
      float μ1 = Vector3F.Dot(d2, u) / δ;
      float μ2 = Vector3F.Dot(-d1, u) / δ;
      if (μ1 + μ2 <= 1 + ε && μ1 >= -ε && μ2 >= -ε)
      {
        // Hit!
        contactSet.HaveContact = true;

        if (type == CollisionQueryType.Boolean)
          return false;

        if (δ < 0)
          return false;   // Shooting into the back of a one-sided triangle - no contact.

        float penetrationDepth = λ * rayInField.Length;

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
          "RayHeightFieldAlgorithm has set the wrong feature property.");

        ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
        return true;
      }

      return false;
    }


    /// <inheritdoc/>
    public override float GetTimeOfImpact(CollisionObject objectA, Pose targetPoseA, CollisionObject objectB, Pose targetPoseB, float allowedPenetration)
    {
      return _heightFieldAlgorithm.GetTimeOfImpact(objectA, targetPoseA, objectB, targetPoseB, allowedPenetration);
    }
  }
}
