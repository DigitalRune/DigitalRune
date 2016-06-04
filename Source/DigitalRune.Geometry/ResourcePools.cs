// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry
{
  /// <summary>
  /// Provides resource pools for reusable items. (For internal use only.)
  /// </summary>
  /// <remarks>
  /// Some objects such as <see cref="Contact"/>, <see cref="ContactSet"/>, etc. are always pooled.
  /// Therefore they have an internal resource pool and use the Create-Recycle pattern. But some
  /// types should not use a resource pool by default - only in special cases. The resource pools
  /// for such types are gather in this class.
  /// </remarks>
  internal static class ResourcePools
  {
    public static readonly ResourcePool<BoxShape> BoxShapes =
      new ResourcePool<BoxShape>(
        () => new BoxShape(),
        null,
        null);


    public static readonly ResourcePool<LineSegmentShape> LineSegmentShapes =
      new ResourcePool<LineSegmentShape>(
        () => new LineSegmentShape(),
        null,
        null);


    public static readonly ResourcePool<TriangleShape> TriangleShapes =
      new ResourcePool<TriangleShape>(
        () => new TriangleShape(),
        null,
        null);


    // Dummy CollisionObjects required for un-/initialization of ContactSets.
    private static readonly TestGeometricObject DummyGeometricObject = TestGeometricObject.Create();
    public static readonly ResourcePool<CollisionObject> TestCollisionObjects =
      new ResourcePool<CollisionObject>(
        () => new CollisionObject(DummyGeometricObject),
        null,
        obj => obj.ResetInternal());
  }
}
