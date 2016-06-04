// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Collisions.Algorithms
{
  internal static partial class CcdHelper
  {
    // Notes:
    // For complex CCD results we can use something like this:
    //  public struct ContinuousCollisionResult
    //  {
    //    public float TimeOfImpact { get; set; }
    //    public Contact Contact { get; set; }
    //  }
    // or we integrate CCD results into the ContactSet class.
    //
    // Currently, the only reasonable result of a TOI query is the TimeOfImpact.
    // The CA algorithm creates a contact too, but the LinearConvexCast does not
    // create a good contact.
    //
    // TODOs:
    // - If the shapes support a bounding sphere computation, then we could get 
    //   tighter fitting bounding spheres.

    // Internal iteration limit for CCD algorithms.
    private const int MaxNumberOfIterations = 64;


    /// <summary>
    /// Gets the bounding sphere of a geometric object.
    /// </summary>
    /// <param name="geometricObject">The geometric object.</param>
    /// <param name="radius">The radius of the bounding sphere.</param>
    /// <param name="center">The center of the bounding sphere in world space.</param>
    internal static void GetBoundingSphere(IGeometricObject geometricObject, out float radius, out Vector3F center)
    {
      // Get sphere from AABB.
      Aabb aabb = geometricObject.Aabb;
      center = aabb.Center;
      radius = aabb.Extent.Length * 0.5f;
    }


    /// <summary>
    /// Gets the radius for a bounding sphere centered at the position of the geometric object.
    /// </summary>
    /// <param name="geometricObject">The geometric object.</param>
    /// <returns>
    /// The radius of the bounding sphere if the sphere's center is identical to the origin of the
    /// geometric object.
    /// </returns>
    internal static float GetBoundingRadius(IGeometricObject geometricObject)
    {
      // Get offset bounding sphere radius + sphere offset.
      Vector3F center;
      float radius;
      GetBoundingSphere(geometricObject, out radius, out center);
      return (center - geometricObject.Pose.Position).Length + radius;
    }
  }
}
