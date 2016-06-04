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
  public partial class HeightFieldAlgorithm
  {
    private void ComputeCollisionFast(ContactSet contactSet, CollisionQueryType type,
      IGeometricObject heightFieldGeometricObject, IGeometricObject convexGeometricObject,
      HeightField heightField, ConvexShape convex, bool swapped)
    {
      Debug.Assert(type != CollisionQueryType.ClosestPoints);

      // Assume no contact.
      contactSet.HaveContact = false;

      // Get scales and poses
      Pose heightFieldPose = heightFieldGeometricObject.Pose;
      Vector3F heightFieldScale = heightFieldGeometricObject.Scale;
      if (heightFieldScale.X < 0 || heightFieldScale.Y < 0 || heightFieldScale.Z < 0)
        throw new NotSupportedException("Computing collisions for height fields with a negative scaling is not supported.");

      Pose convexPose = convexGeometricObject.Pose;
      Vector3F convexScale = convexGeometricObject.Scale;

      // Get a point in the convex. (Could also use center of AABB.)
      var convexPoint = convexPose.ToWorldPosition(convex.InnerPoint * convexScale);

      // Get height field coordinates.
      convexPoint = heightFieldPose.ToLocalPosition(convexPoint);
      float xUnscaled = convexPoint.X / heightFieldScale.X;
      float zUnscaled = convexPoint.Z / heightFieldScale.Z;

      // If convex point is outside height field, abort.
      var originX = heightField.OriginX;
      var originZ = heightField.OriginZ;
      if (xUnscaled < originX || xUnscaled > originX + heightField.WidthX
          || zUnscaled < originZ || zUnscaled > originZ + heightField.WidthZ)
      {
        return;
      }

      // Get height and normal.
      float height;
      Vector3F normal;
      int featureIndex;
      GetHeight(heightField, xUnscaled, zUnscaled, out height, out normal, out featureIndex);

      // Check for holes.
      if (Numeric.IsNaN(height))
        return;

      // Apply scaling.
      height *= heightFieldScale.Y;
      // Normals are transformed with the inverse transposed matrix --> 1 / scale.
      normal = normal / heightFieldScale;
      normal.Normalize();

      // ----- Now we test convex vs. plane.
      // Convert normal to convex space.
      normal = heightFieldPose.ToWorldDirection(normal);
      var normalInConvex = convexPose.ToLocalDirection(normal);

      // Convert plane point to convex space. 
      Vector3F planePoint = new Vector3F(convexPoint.X, height, convexPoint.Z);
      planePoint = heightFieldPose.ToWorldPosition(planePoint);
      planePoint = convexPose.ToLocalPosition(planePoint);

      // Get convex support point in plane normal direction.
      Vector3F supportPoint = convex.GetSupportPoint(-normalInConvex, convexScale);

      // Get penetration depth.
      float penetrationDepth = Vector3F.Dot((planePoint - supportPoint), normalInConvex);

      // Abort if there is no contact.
      if (penetrationDepth < 0)
        return;

      // Abort if object is too deep under the height field.
      // This is important for height fields with holes/caves. Without this check
      // no objects could enter the cave.
      if (penetrationDepth > heightField.Depth)
        return;

      // We have contact.
      contactSet.HaveContact = true;

      // Return for boolean queries.
      if (type == CollisionQueryType.Boolean)
        return;

      // Contact position is in the "middle of the penetration".
      Vector3F position = convexPose.ToWorldPosition(supportPoint + normalInConvex * (penetrationDepth / 2));

      if (swapped)
        normal = -normal;

      // Add contact
      var contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);

      if (swapped)
        contact.FeatureB = featureIndex;
      else
        contact.FeatureA = featureIndex;

      ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);

      if (CollisionDetection.FullContactSetPerFrame
          && contactSet.Count < 3)
      {
        // Trying to create a full contact set.

        // We use arbitrary orthonormal values to perturb the normal direction.
        var ortho1 = normalInConvex.Orthonormal1;
        var ortho2 = normalInConvex.Orthonormal2;

        // Test 4 perturbed support directions.
        for (int i = 0; i < 4; i++)
        {
          Vector3F direction;
          switch (i)
          {
            case 0:
              direction = -normalInConvex + ortho1;
              break;
            case 1:
              direction = -normalInConvex - ortho1;
              break;
            case 2:
              direction = -normalInConvex + ortho2;
              break;
            default:
              direction = -normalInConvex - ortho2;
              break;
          }

          // Support point vs. plane test as above:
          supportPoint = convex.GetSupportPoint(direction, convexScale);
          penetrationDepth = Vector3F.Dot((planePoint - supportPoint), normalInConvex);
          if (penetrationDepth >= 0)
          {
            position = convexPose.ToWorldPosition(supportPoint + normalInConvex * (penetrationDepth / 2));
            contact = ContactHelper.CreateContact(contactSet, position, normal, penetrationDepth, false);
            ContactHelper.Merge(contactSet, contact, type, CollisionDetection.ContactPositionTolerance);
          }
        }
      }
    }


    // This method is copied from HeightField.cs and modified to return the normal (not normalized!).
    private static void GetHeight(HeightField heightField, float x, float z, out float height, out Vector3F normal, out int featureIndex)
    {
      int arrayLengthX = heightField.NumberOfSamplesX;
      int arrayLengthZ = heightField.NumberOfSamplesZ;

      // x and z without origin.
      var xo = (x - heightField.OriginX);
      var zo = (z - heightField.OriginZ);

      // Compute cell indices.
      float cellWidthX = heightField.WidthX / (arrayLengthX - 1);
      float cellWidthZ = heightField.WidthZ / (arrayLengthZ - 1);

      int indexX = Math.Min((int)(xo / cellWidthX), arrayLengthX - 2);
      int indexZ = Math.Min((int)(zo / cellWidthZ), arrayLengthZ - 2);

      // Determine which triangle we need.
      float xRelative = xo / cellWidthX - indexX;
      float zRelative = zo / cellWidthZ - indexZ;
      Debug.Assert(Numeric.IsGreaterOrEqual(xRelative, 0) && Numeric.IsLessOrEqual(xRelative, 1));
      Debug.Assert(Numeric.IsGreaterOrEqual(zRelative, 0) && Numeric.IsLessOrEqual(zRelative, 1));
      bool useSecondTriangle = (xRelative + zRelative) > 1;  // The diagonal is where xRel + zRel == 1.

      var triangle = heightField.GetTriangle(indexX, indexZ, useSecondTriangle);

      // Store heights of the triangle vertices.
      float height0 = triangle.Vertex0.Y;
      float height1 = triangle.Vertex1.Y;
      float height2 = triangle.Vertex2.Y;

      // Get barycentric coordinates (relative to triangle in xz plane).
      float u, v, w;

      // Project triangle into xz plane.
      triangle.Vertex0.Y = 0;
      triangle.Vertex1.Y = 0;
      triangle.Vertex2.Y = 0;
      GeometryHelper.GetBarycentricFromPoint(triangle, new Vector3F(x, 0, z), out u, out v, out w);

      featureIndex = (indexZ * (arrayLengthX - 1) + indexX) * 2;
      if (useSecondTriangle)
        featureIndex++;

#if DEBUG
      float e = Numeric.EpsilonF * 10;
      Debug.Assert((Numeric.IsGreaterOrEqual(u, 0, e) && Numeric.IsGreaterOrEqual(v, 0, e)) && Numeric.IsLessOrEqual(u + v, 1, e));
#endif

      // Return height (computed with barycentric coordinates).
      height = u * height0 + v * height1 + w * height2;

      // Correct triangle vertex heights because triangle is an out parameter.
      triangle.Vertex0.Y = height0;
      triangle.Vertex1.Y = height1;
      triangle.Vertex2.Y = height2;

      normal = Vector3F.Cross(triangle.Vertex1 - triangle.Vertex0, triangle.Vertex2 - triangle.Vertex0);
    }


    /* ----- Bilinear interpolation of height and normal
    // An experimental method that returns height and normal values that are bilinearly interpolated
    // from the cell corners.
    // Normal is returned unnormalized!
    private static void GetHeight(HeightField heightField, float x, float z, out float height, out Vector3F normal)
    {
      int arrayLengthX = heightField.Array.GetLength(0);
      int arrayLengthZ = heightField.Array.GetLength(1);

      if (heightField.NormalArray == null)
      {
        // First access to the normal array --> initialize array.
        heightField.InitializeNormalArray();
      }

      // Compute cell indices.
      float cellWidthX = heightField.WidthX / (arrayLengthX - 1);
      float cellWidthZ = heightField.WidthZ / (arrayLengthZ - 1);
      int indexX = (int)(x / cellWidthX);
      int indexZ = (int)(z / cellWidthZ);
      float fractionX = (x % cellWidthX) / cellWidthX;
      float fractionZ = (z % cellWidthZ) / cellWidthZ;

      // Get the four height and normals of the corners of this cell.
      float h0 = heightField.Array[indexX, indexZ];
      float h1 = heightField.Array[indexX, indexZ + 1];
      float h2 = heightField.Array[indexX + 1, indexZ + 1];
      float h3 = heightField.Array[indexX + 1, indexZ];
      Vector3F n0 = heightField.NormalArray[indexX, indexZ];
      Vector3F n1 = heightField.NormalArray[indexX, indexZ + 1];
      Vector3F n2 = heightField.NormalArray[indexX + 1, indexZ + 1];
      Vector3F n3 = heightField.NormalArray[indexX + 1, indexZ];

      // Bilinear interpolation of the corners.
      float h01 = InterpolationHelper.Lerp(h0, h1, fractionZ);
      float h32 = InterpolationHelper.Lerp(h3, h2, fractionZ);
      height = InterpolationHelper.Lerp(h01, h32, fractionX);
      Vector3F n01 = InterpolationHelper.Lerp(n0, n1, fractionZ).Normalized;
      Vector3F n32 = InterpolationHelper.Lerp(n3, n2, fractionZ).Normalized;
      normal = InterpolationHelper.Lerp(n01, n32, fractionX);
    }*/
  }
}
