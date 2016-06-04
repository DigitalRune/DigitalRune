// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Indicates that an object (normally a <see cref="ISpatialPartition{T}"/>) has special support 
  /// for frustum culling.
  /// </summary>
  /// <typeparam name="T">The type of the queried items.</typeparam>
  /// <remarks>
  /// <para>
  /// It is recommended to use a type of <see cref="ISpatialPartition{T}"/> that implements this 
  /// interface in the <see cref="CollisionDomain.BroadPhase"/> of a collision domain, if the 
  /// collision domain is used for frustum culling.
  /// </para>
  /// <para>
  /// In general frustum culling can be done by adding a collision object for the viewing frustum 
  /// (usually a <see cref="ViewVolume"/> shape) to a collision domain. The objects inside the 
  /// viewing frustum can then be queried by calling <see cref="CollisionDomain.GetContactObjects"/>.
  /// This approach is acceptable for small scenes, but it does not perform well in large, complex 
  /// scenes.
  /// </para>
  /// <para>
  /// A viewing frustum of a camera has certain characteristics that reduce performance of a 
  /// collision domain: The viewing frustum is usually extremely large and can cover entire scenes.
  /// The camera usually moves every frame. Therefore, when the object is added to a collision 
  /// domain a large number of contact sets is created and updated every frame.
  /// </para>
  /// <para>
  /// Luckily frustum culling does not need to be exact: In most cases it is sufficient to only 
  /// check the axis-aligned bounding boxes (AABBs) of the scene objects against the viewing 
  /// frustum. This can be done manually by calling <see cref="GetOverlaps"/> against the 
  /// <see cref="CollisionDomain.BroadPhase"/> of a collision domain and skip the narrow phase of 
  /// the collision detection. In this case the viewing frustum does not need to be added to the 
  /// collision domain!
  /// </para>
  /// </remarks>
  public interface ISupportFrustumCulling<T>
  {
    /// <summary>
    /// Gets the items that touch the bounding volume ("k-DOP") defined by a set of planes.
    /// </summary>
    /// <param name="planes">
    /// The planes that define the bounding volume (k-DOP). Max 31 planes. The plane normals are
    /// pointing outwards. The plane does not need to be normalized, i.e. the plane normal does not
    /// need to be a unit vector.
    /// </param>
    /// <returns>
    /// All items that touch the bounding volume. (The result is conservative: It is guaranteed that
    /// the list contains all items that touch the bounding volume. But I it may also contain a few
    /// items that do not touch the bounding volume!)
    /// </returns>
    /// <remarks>
    /// Hint: The method <see cref="GeometryHelper.ExtractPlanes(Matrix44F,IList{Plane},bool)"/> can
    /// be called to extract the viewing frustum planes of a world-view-projection matrix.
    /// </remarks>
    /// <remarks>
    /// Filtering (see <see cref="ISpatialPartition{T}.Filter"/>) is not applied.
    /// </remarks>
    IEnumerable<T> GetOverlaps(IList<Plane> planes);
  }
}
