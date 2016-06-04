// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry
{
  public static partial class GeometryHelper
  {
    /// <summary>
    /// Determines whether the given axis-aligned bounding box (AABB) and ray overlap.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="ray">The ray.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the ray have a contact; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabb, Ray ray)
    {
      if (Numeric.IsZero(ray.Length))
        return HaveContact(aabb, ray.Origin);

      var rayDirectionInverse = new Vector3F(
        1 / ray.Direction.X,
        1 / ray.Direction.Y,
        1 / ray.Direction.Z);

      return HaveContact(aabb, ray.Origin, rayDirectionInverse, ray.Length, 0);
    }


    /// <summary>
    /// Determines whether the given axis-aligned bounding box (AABB) and ray overlap.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="ray">The ray.</param>
    /// <param name="epsilon">
    /// A small epsilon value by which the AABB is extended to avoid missing contacts because of
    /// numerical problems. (Especially in ray vs. triangle mesh tests, we do not want to miss
    /// collisions between triangle.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the ray have a contact; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabb, Ray ray, float epsilon)
    {
      if (Numeric.IsZero(ray.Length))
        return HaveContact(aabb, ray.Origin);

      var rayDirectionInverse = new Vector3F(
        1 / ray.Direction.X,
        1 / ray.Direction.Y,
        1 / ray.Direction.Z);

      return HaveContact(aabb, ray.Origin, rayDirectionInverse, ray.Length, epsilon);
    }


    /// <summary>
    /// Determines whether the given axis-aligned bounding box (AABB) and ray overlap. This method
    /// allows false positives!
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="ray">The ray.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the ray have a contact; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    internal static bool HaveContactFast(Aabb aabb, Ray ray)
    {
      if (Numeric.IsZero(ray.Length))
        return HaveContact(aabb, ray.Origin);

      var rayDirectionInverse = new Vector3F(
        1 / ray.Direction.X,
        1 / ray.Direction.Y,
        1 / ray.Direction.Z);

      return HaveContactFast(aabb, ray.Origin, rayDirectionInverse, ray.Length);
    }


    /// <summary>
    /// Determines whether an axis-aligned bounding box (AABB) and a ray have contact. This method
    /// allows false positives!
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="rayOrigin">The ray origin.</param>
    /// <param name="rayDirectionInverse">
    /// The inverse of the ray direction (<c>1 / ray.Direction</c>).
    /// </param>
    /// <param name="rayLength">The length of the ray.</param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the ray have a contact; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method will return <see langword="true"/> if a ray is in the plane that goes through
    /// an AABB side (false positive).
    /// </remarks>
    internal static bool HaveContactFast(Aabb aabb, Vector3F rayOrigin, Vector3F rayDirectionInverse, float rayLength)
    {
      // Note: If HaveContact is called several times for the same ray, we could return
      // tMin and tMax --> Parameter (ref float tMin and ref float tMax). Check tMin and tMax
      // against the first results.

      // The points on a ray are x = rayOrigin + t * rayDirection.
      // The points on a plane follow this equation: x . planeNormal = planeDistanceFromOrigin.
      // Substituting the x in the second equation, we can solve for t and get:
      // t = (planeDistanceFromOrigin - rayOrigin . planeNormal) / (rayDirection . planeNormal)
      // 
      // For Aabbs the planeNormals are the unit vectors.
      // The planeDistances are the components of Aabb.Minimum and Maximum.

      // Compute tMin and tMax for all plane intersections. If a tMin is greater than a
      // tMax we can abort early. If tMin <= rayLength and tMax >= 0 we have a hit.

      // tMinX
      float nearPlaneDistanceX = (rayDirectionInverse.X > 0) ? aabb.Minimum.X : aabb.Maximum.X;
      float tMinX = (nearPlaneDistanceX - rayOrigin.X) * rayDirectionInverse.X;

      // tMaxX
      float farPlaneDistanceX = (rayDirectionInverse.X > 0) ? aabb.Maximum.X : aabb.Minimum.X;
      float tMaxX = (farPlaneDistanceX - rayOrigin.X) * rayDirectionInverse.X;

      // Note: tMinX/tMaxX can be NaN if the ray origin is in the min or max X plane and the
      // ray is parallel to the plane. In this degenerate case we do not care about the
      // result, so no special treatment is needed.
      Debug.Assert(tMinX <= tMaxX || Numeric.IsNaN(tMinX) || Numeric.IsNaN(tMaxX));

      //if (tMinX > tMaxX)  // Not necessary: tMinX is always <= tMaxX
      //  return false;
      float tMin = tMinX;
      float tMax = tMaxX;

      // tMaxY
      float farPlaneDistanceY = (rayDirectionInverse.Y > 0) ? aabb.Maximum.Y : aabb.Minimum.Y;
      float tMaxY = (farPlaneDistanceY - rayOrigin.Y) * rayDirectionInverse.Y;
      if (tMin > tMaxY)
        return false;
      if (tMaxY < tMax)
        tMax = tMaxY;

      // tMinY
      float nearPlaneDistanceY = (rayDirectionInverse.Y > 0) ? aabb.Minimum.Y : aabb.Maximum.Y;
      float tMinY = (nearPlaneDistanceY - rayOrigin.Y) * rayDirectionInverse.Y;
      if (tMinY > tMax)
        return false;
      if (tMinY > tMin)
        tMin = tMinY;

      // tMinZ
      float nearPlaneDistanceZ = (rayDirectionInverse.Z > 0) ? aabb.Minimum.Z : aabb.Maximum.Z;
      float tMinZ = (nearPlaneDistanceZ - rayOrigin.Z) * rayDirectionInverse.Z;
      if (tMinZ > tMax)
        return false;
      if (tMinZ > tMin)
        tMin = tMinZ;

      // tMaxZ
      float farPlaneDistanceZ = (rayDirectionInverse.Z > 0) ? aabb.Maximum.Z : aabb.Minimum.Z;
      float tMaxZ = (farPlaneDistanceZ - rayOrigin.Z) * rayDirectionInverse.Z;
      if (tMin > tMaxZ)
        return false;
      if (tMaxZ < tMax)
        tMax = tMaxZ;

      Debug.Assert(tMin <= tMax || tMax < 0 || tMin >= rayLength || Numeric.IsNaN(tMin) || Numeric.IsNaN(tMax));

      if (tMax < 0)
        return false;
      if (tMin > rayLength)
        return false;
      return true;
    }



    /// <summary>
    /// Determines whether an axis-aligned bounding box (AABB) and a ray have contact.
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <param name="rayOrigin">The ray origin.</param>
    /// <param name="rayDirectionInverse">
    /// The inverse of the ray direction (<c>1 / ray.Direction</c>).
    /// </param>
    /// <param name="rayLength">The length of the ray.</param>
    /// <param name="epsilon">
    /// A small epsilon value by which the AABB is extended to avoid missing contacts because of
    /// numerical problems. (Especially in ray vs. triangle mesh tests, we do not want to miss
    /// collisions between triangle.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the AABB and the ray have a contact; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Unlike <see cref="HaveContactFast(Aabb,Vector3F,Vector3F,float)"/>, this method is exact.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    internal static bool HaveContact(Aabb aabb, Vector3F rayOrigin, Vector3F rayDirectionInverse, float rayLength, float epsilon)
    {
      // Note: If HaveContact is called several times for the same ray, we could return
      // tMin and tMax --> Parameter (ref float tMin and ref float tMax). Check tMin and tMax
      // against the first results.

      // The points on a ray are x = rayOrigin + t * rayDirection.
      // The points on a plane follow this equation: x . planeNormal = planeDistanceFromOrigin.
      // Substituting the x in the second equation, we can solve for t and get:
      // t = (planeDistanceFromOrigin - rayOrigin . planeNormal) / (rayDirection . planeNormal)
      // 
      // For Aabbs the planeNormals are the unit vectors.
      // The planeDistances are the components of Aabb.Minimum and Maximum.

      // Compute tMin and tMax for all plane intersections. If a tMin is greater than a
      // tMax we can abort early. If tMin <= rayLength and tMax >= 0 we have a hit.

      float tMin = 0;
      float tMax = rayLength;

      {
        // tMinX
        float nearPlaneDistanceX = (rayDirectionInverse.X > 0) ? aabb.Minimum.X - epsilon : aabb.Maximum.X + epsilon;
        float tMinX = (nearPlaneDistanceX - rayOrigin.X) * rayDirectionInverse.X;
        if (tMinX > tMax)
          return false;
        if (tMinX > tMin)
          tMin = tMinX;

        // tMaxX
        float farPlaneDistanceX = (rayDirectionInverse.X > 0) ? aabb.Maximum.X + epsilon : aabb.Minimum.X - epsilon;
        float tMaxX = (farPlaneDistanceX - rayOrigin.X) * rayDirectionInverse.X;
        if (tMin > tMaxX)
          return false;
        if (tMaxX < tMax)
          tMax = tMaxX;

        // Note: tMinX/tMaxX can be NaN if the ray origin is in the min or max X plane and the
        // ray is parallel to the plane. In this degenerate case we do not care about the
        // result, so no special treatment is needed.
        Debug.Assert(tMinX <= tMaxX || Numeric.IsNaN(tMinX) || Numeric.IsNaN(tMaxX));
      }

      {
        // tMaxY
        float farPlaneDistanceY = (rayDirectionInverse.Y > 0) ? aabb.Maximum.Y + epsilon: aabb.Minimum.Y - epsilon;
        float tMaxY = (farPlaneDistanceY - rayOrigin.Y) * rayDirectionInverse.Y;
        if (tMin > tMaxY)
          return false;
        if (tMaxY < tMax)
          tMax = tMaxY;

        // tMinY
        float nearPlaneDistanceY = (rayDirectionInverse.Y > 0) ? aabb.Minimum.Y - epsilon : aabb.Maximum.Y + epsilon;
        float tMinY = (nearPlaneDistanceY - rayOrigin.Y) * rayDirectionInverse.Y;
        if (tMinY > tMax)
          return false;
        if (tMinY > tMin)
          tMin = tMinY;
      }

      {
        // tMinZ
        float nearPlaneDistanceZ = (rayDirectionInverse.Z > 0) ? aabb.Minimum.Z - epsilon: aabb.Maximum.Z + epsilon;
        float tMinZ = (nearPlaneDistanceZ - rayOrigin.Z) * rayDirectionInverse.Z;
        if (tMinZ > tMax)
          return false;
        if (tMinZ > tMin)
          tMin = tMinZ;

        // tMaxZ
        float farPlaneDistanceZ = (rayDirectionInverse.Z > 0) ? aabb.Maximum.Z + epsilon : aabb.Minimum.Z - epsilon;
        float tMaxZ = (farPlaneDistanceZ - rayOrigin.Z) * rayDirectionInverse.Z;
        if (tMin > tMaxZ)
          return false;
        if (tMaxZ < tMax)
          tMax = tMaxZ;
      }

      Debug.Assert(tMin <= tMax + epsilon || tMax < 0 || tMin >= rayLength || Numeric.IsNaN(tMin) || Numeric.IsNaN(tMax));

      if (tMax < 0)
        return false;
      if (tMin > rayLength)
        return false;
      return true;
    }


    /// <summary>
    /// Determines whether a given AABB is hit by a moving AABB.
    /// </summary>
    /// <param name="aabbA">The axis-aligned bounding box.</param>
    /// <param name="aabbB">The moving axis-aligned bounding box.</param>
    /// <param name="movementB">
    /// The movement vector of <paramref name="aabbB"/>. <paramref name="aabbB"/> is given at its
    /// start position. The movement vector is added to the start position of 
    /// <paramref name="aabbB"/> to define its end position. 
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the moving AABB (<paramref name="aabbB"/>) overlaps the static
    /// AABB (<paramref name="aabbA"/>) at any time during its movement.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method does not take rotations into account, only linear movement. If the AABBs
    /// represent two rotating objects, make sure that the AABBs are large enough to contain the
    /// rotating objects for all rotation angles. 
    /// </para>
    /// <para>
    /// Remember: A situation where both AABBs are moving can always be re-framed as one static AABB
    /// and the other AABB moving relative to the static AABB.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public static bool HaveContact(Aabb aabbA, Aabb aabbB, Vector3F movementB)
    {
      // TODO: Add overload GeometryHelper.HaveContact(Aabb aabbA, Vector3F movementA, Aabb aabbB, Vector3F movementB).

      // Testing if moving A and B have contact is the same as testing whether (A - B) hits a ray, 
      // where the ray is the relative movement of B (starting at the origin).

      float movementBLength = movementB.Length;
      if (Numeric.IsZero(movementBLength))
      {
        // Both AABBs are static.
        return HaveContact(aabbA, aabbB);
      }
      else
      {
        // Convert to AABB vs. Ray check.
        Vector3F movementBDirection = movementB / movementBLength;
        Aabb aabbAMinusB = new Aabb(aabbA.Minimum - aabbB.Maximum, aabbA.Maximum - aabbB.Minimum);
        Debug.Assert(aabbAMinusB.Minimum <= aabbAMinusB.Maximum);
        return HaveContact(aabbAMinusB, new Ray(Vector3F.Zero, movementBDirection, movementBLength));
      }
    }
  }
}
