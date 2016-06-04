// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Efficiently manages items in a space or of a model using their spatial properties.
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  /// <remarks>
  /// <para>
  /// A spatial partition is a <see cref="ICollection{T}"/> of items of a model or in a space. 
  /// Spatial partitioning structures items according to their position and extents in space. This
  /// is usually used to manage the objects of a 3D model (using bounding box trees or other 
  /// methods) or objects in a 3D space (using grids, octrees, etc.). Spatial partitions allow fast
  /// queries on the items. For example: "Give me all items that touch a given axis aligned bounding
  /// box." Or: "Give me all pairs of touching items."
  /// </para>
  /// <para>
  /// The items in the spatial partitions (see type parameter <typeparamref name="T"/>) can be of 
  /// any kind: integer values that define triangle indices of a triangle mesh, or 
  /// <see cref="CollisionObject"/> in a space, ...
  /// </para>
  /// <para>
  /// <strong>Creating Spatial Partitions:</strong> A <see cref="ISpatialPartition{T}"/> is a 
  /// <see cref="ICollection{T}"/>, so items can be added and removed. The internal structure will
  /// be built when <see cref="Update"/> is called. When items move or change their shape, the
  /// spatial partition must be informed using <see cref="Invalidate(T)"/> (for a single item) or 
  /// <see cref="Invalidate()"/> (if more or all items have changed). Then <see cref="Update"/> must
  /// be called to rebuild the internal structure.
  /// </para>
  /// <para>
  /// Calling Add/Remove/Invalidate methods are fast operations. The internal work is done when 
  /// <see cref="Update"/> is called. If <see cref="Update"/> is not called by the owner of the
  /// spatial partition, then it will be automatically called when <see cref="GetOverlaps()"/> (or 
  /// one of its overloads) is called.
  /// </para>
  /// <para>
  /// <strong>Querying Spatial Partitions:</strong> A <see cref="ISpatialPartition{T}"/> has several
  /// <strong>GetOverlaps</strong> methods that allow to get all the items that touch a specific
  /// item or region. These queries are more efficient than enumerating and testing all contained
  /// items manually. Some queries return <see cref="Pair{T}"/>s which describe pairs of touching
  /// objects.
  /// </para>
  /// <para>
  /// Spatial partitions use approximate representations of the managed items - usually bounding 
  /// volumes instead of the actual geometry. For example, when using a <see cref="AabbTree{T}"/> 
  /// items are represented using their axis-aligned bounding box (AABB). The 
  /// <strong>GetOverlaps</strong> methods only test the bounding volumes against each other to 
  /// check for potential intersections. When a <strong>GetOverlaps</strong> method returns an item 
  /// or an item pair, it is not guaranteed that the items are actually touching - for returned 
  /// items the spatial partition computed that it is very likely that they are touching. For 
  /// example: When managing triangles with the help of an <see cref="AabbTree{T}"/> the method 
  /// <see cref="GetOverlaps()"/> returns all triangles where the bounding boxes overlap. 
  /// Overlapping bounding volumes do not guarantee that the contained items are actually 
  /// intersecting - the triangles could still be separated.
  /// <para>
  /// A spatial partition does not replace a detailed collision detection. It only helps to 
  /// efficiently sort out items that do not intersect.
  /// </para>
  /// </para>
  /// <para>
  /// <strong>AABB Computation of Items:</strong> When creating an instance of an 
  /// <see cref="ISpatialPartition{T}"/> a callback that computes the <see cref="Shapes.Aabb"/> for 
  /// a given item must be specified. The spatial partition does not know how to compute the 
  /// positions and extents of the items. The <see cref="ISpatialPartition{T}.GetAabbForItem"/>
  /// delegate is used to compute an <see cref="Shapes.Aabb"/>s for each item. The computed 
  /// <see cref="Shapes.Aabb"/> is used to define the spatial properties of an item. For a single
  /// item the method must always return the same <see cref="Aabb"/>. If the AABB of an item has
  /// changed (e.g. the item has moved or changed shape), 
  /// <see cref="BasePartition{T}.Invalidate()"/> must be called.
  /// </para>
  /// <para>
  /// <strong>Self Overlaps:</strong> A self-overlap is an overlap of two items where both items are
  /// contained in the spatial partition. Self-overlaps are only computed if 
  /// <see cref="EnableSelfOverlaps"/> is set. Self-overlaps can be queried using 
  /// <see cref="GetOverlaps()"/>. Overlaps of a single item with itself are never returned.
  /// </para>
  /// <para>
  /// <strong>Filtering:</strong> Per default, no filter is set and <strong>GetOverlaps</strong>
  /// methods return all found overlaps. A <see cref="Filter"/> can be set. Then, whenever a pair of
  /// items is tested the overlap will only be accepted if <see cref="IPairFilter{T}.Filter"/>
  /// returns <see langword="true"/>. The filter is not used if an item is tested against an 
  /// <see cref="Shapes.Aabb"/> or a <see cref="Ray"/>.
  /// </para>
  /// <para>
  /// <strong>Rebuild versus Refit:</strong> The spatial partitioning is performed when 
  /// <see cref="Update"/> is called. The spatial partition will build a new internal structure if
  /// many or all items are new or were invalidated (see <see cref="Invalidate(T)"/>). If only a few
  /// items were changed, the spatial partition will perform a faster "refit" operation that changes
  /// only the relevant parts of the internal structure. Depending on the type of spatial
  /// partitioning, refit operations can lead to less optimal internal structures. The 
  /// <see cref="Update"/> method has a <i>forceRebuild</i> parameter with which a complete rebuild 
  /// can be demanded. 
  /// </para>
  /// <para>
  /// <strong>Cloning:</strong> Spatial partitions are cloneable. Cloning creates a deep copy of the
  /// spatial partition. All properties and internal data structures are duplicated. However, the
  /// items contained in the spatial partitions are not copied. The clone will be an empty spatial 
  /// partition that can be used independently from the original spatial partitions.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public interface ISpatialPartition<T> : ICollection<T>
  {
    //
    // Notes:
    //
    // Possible spatial partitions:
    // - AabbTree, CompressedAabbTree, QuantizedAabbTree, QuantizedCompressedAabbTree
    // - SweepAndPruneSpace, SAP with DBVT for ray casts and ad-hoc queries.
    // - kD-Tree, BSP-Tree, Octree, Quadtree
    // - Grid, HierarchicalGrid, HashGrid, HierarchicalHashGrid
    // - DBVT (dynamic bounding volume tree, see Bullet), DBVT + DBVT (for broad phase with
    //   static and dynamic objects)
    //
    // Possible improvements:
    // - Add GetRay callback. With this the partition can check if an item is a ray and
    //   do special ray tests. - But be careful if an object changes from ray to normal
    //   object, then it could have a new overlap even if it did not move!
    // - Add GetMovement callback. With this the partition sees which objects are moving
    //   and can support Continuous Collision Detection.
    //
    // If the broad phase needs special methods that should not be public, then an 
    // interface ISupportCollisionBroadPhase can be created. 
    // Example: ISupportCollisionBroadPhase { void Update(broadPhaseCallback, ...) }
    // To allow for optimizations the interface would be commented as "subject to change".

    // TODO: Do we need this?
    // IEnumerable<T> GetOverlaps(IEnumerable<Aabb> aabbs);
    // IEnumerable<T> GetOverlaps(Aabb aabb, Vector3F start, Vector3F end)
    // event EventHandler<OverlapEventArgs<T>> OverlapsChanged;


    /// <summary>
    /// Gets the axis-aligned bounding box (AABB) that contains all items.
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB) that contains all items.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    Aabb Aabb { get; }


    /// <summary>
    /// Gets or sets the method that computes the <see cref="Shapes.Aabb"/> of an item.
    /// </summary>
    /// <value>
    /// The method that computes the axis-aligned bounding box of an item.
    /// </value>
    /// <remarks>
    /// <para>
    /// When creating a <see cref="ISpatialPartition{T}"/> a callback that computes the 
    /// <see cref="Shapes.Aabb"/> for a given item, must be specified. The spatial partition does 
    /// not know how to compute the positions and extents of the items. Only the 
    /// <see cref="GetAabbForItem"/> delegate is used to compute an <see cref="Shapes.Aabb"/>s for
    /// each item. The computed <see cref="Shapes.Aabb"/> is used to define the spatial properties 
    /// of a property. For a single item the method must always return the same <see cref="Aabb"/>. 
    /// If the AABB of an item has changed (e.g. the item has moved or changed shape), 
    /// <see cref="Invalidate(T)"/> must be called.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> Changing this property does not automatically invalidate the
    /// spatial partition. The spatial partition assumes the previous AABBs are still valid. If, 
    /// however, the spatial partition should to be recomputed, the method 
    /// <see cref="Invalidate()"/> needs to be called manually.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    Func<T, Aabb> GetAabbForItem { get; set; }


    /// <summary>
    /// Gets or sets the filter that is used to filter overlaps of two items.
    /// </summary>
    /// <value>The filter that is used to filter item overlap pairs.</value>
    /// <remarks>
    /// The <see cref="GetOverlaps()"/> methods check whether the items overlap. Then - when a 
    /// <see cref="Filter"/> is set - the <see cref="GetOverlaps()"/> methods apply the filter to
    /// all pairs of overlapping items. A <see cref="Filter"/> can be set to check the item pairs
    /// for additional criteria and reject item pairs that do not meet these criteria.
    /// </remarks>
    IPairFilter<T> Filter { get; set; }


    /// <summary>
    /// Gets or sets a value indicating whether self-overlaps are computed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if self-overlaps are computed; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Self-overlaps can be queried with <see cref="GetOverlaps()"/> if this flag is set.
    /// </remarks>
    bool EnableSelfOverlaps { get; set; }


    /// <summary>
    /// Creates a new spatial partition that is a clone (deep copy) of the current instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="ISpatialPartition{T}"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// Cloning creates a deep copy of the spatial partition. All properties and internal data 
    /// structures are duplicated. However, the items contained in the spatial partitions are not 
    /// copied. The clone will be an empty spatial  partition that can be used independently from 
    /// the original spatial partitions.
    /// </remarks>
    ISpatialPartition<T> Clone();


    /// <overloads>
    /// <summary>
    /// Gets the overlaps between items of this spatial partition and another object.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the items that touch the given axis-aligned bounding box (AABB).
    /// </summary>
    /// <param name="aabb">The axis-aligned bounding box (AABB).</param>
    /// <returns>All items that touch the given AABB.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is not applied.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    IEnumerable<T> GetOverlaps(Aabb aabb);


    /// <summary>
    /// Gets the items that touch the given item.
    /// </summary>
    /// <param name="item">
    /// The item. (The given item must be part of the spatial partition. In some cases external 
    /// objects are also supported. See documentation of derived types.)
    /// </param>
    /// <returns>All items that touch the given item.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </remarks>
    IEnumerable<T> GetOverlaps(T item);


    /// <summary>
    /// Gets the items that touch the given ray.
    /// </summary>
    /// <param name="ray">The ray.</param>
    /// <returns>All items that are hit by the ray.</returns>
    /// <remarks>
    /// Filtering (see <see cref="Filter"/>) is not applied.
    /// </remarks>
    IEnumerable<T> GetOverlaps(Ray ray); 


    /// <summary>
    /// Gets overlaps of all items contained in this spatial partition.
    /// </summary>
    /// <returns>
    /// All pairs of overlapping items of this spatial partition. Overlaps of an item with itself
    /// are not returned.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method can only be called if <see cref="EnableSelfOverlaps"/> is 
    /// <see langword="true"/>.
    /// </para>
    /// <para>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </para>
    /// </remarks>
    /// <exception cref="GeometryException">
    /// <see cref="EnableSelfOverlaps"/> is <see langword="false"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    IEnumerable<Pair<T>> GetOverlaps();


    /// <summary>
    /// Gets overlaps between all items of this spatial partition and the items of another spatial 
    /// partition.
    /// </summary>
    /// <param name="otherPartition">The spatial partition to test against.</param>
    /// <returns>
    /// All pairwise overlaps between items of this spatial partition and 
    /// <paramref name="otherPartition"/>. In each returned <see cref="Pair{T}"/> the first item
    /// (see <see cref="Pair{T}.First"/>) is from this partition and the second item (see 
    /// <see cref="Pair{T}.Second"/>) is from <paramref name="otherPartition"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="otherPartition"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    IEnumerable<Pair<T>> GetOverlaps(ISpatialPartition<T> otherPartition);


    /// <summary>
    /// Gets overlaps between all items of this spatial partition and the items of another spatial 
    /// partition.
    /// </summary>
    /// <param name="scale">The scale of this spatial partition.</param>
    /// <param name="pose">The pose of this spatial partition.</param>
    /// <param name="otherPartition">The other spatial partition to test against.</param>
    /// <param name="otherScale">The scale of the <paramref name="otherPartition"/>.</param>
    /// <param name="otherPose">The pose of the <paramref name="otherPartition"/>.</param>
    /// <returns>
    /// All pairwise overlaps between items of this spatial partition and 
    /// <paramref name="otherPartition"/>. In each returned <see cref="Pair{T}"/> the first item
    /// (see <see cref="Pair{T}.First"/>) is from this partition and the second item (see 
    /// <see cref="Pair{T}.Second"/>) is from <paramref name="otherPartition"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Both spatial partitions are unscaled and defined in local space. The scales and the poses
    /// transform the spatial partitions from their local space to world space. The scale is applied 
    /// before the pose.
    /// </para>
    /// <para>
    /// Filtering (see <see cref="Filter"/>) is applied to filter overlaps.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="otherPartition"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    IEnumerable<Pair<T>> GetOverlaps(Vector3F scale, Pose pose, ISpatialPartition<T> otherPartition, Vector3F otherScale, Pose otherPose);


    /// <overloads>
    /// <summary>
    /// Invalidates the cached spatial information.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Invalidates the cached spatial information of all items in the spatial partition.
    /// </summary>
    /// <remarks>
    /// This method informs the spatial partition that many or all items have moved or changed 
    /// shape.
    /// </remarks>
    void Invalidate();


    /// <summary>
    /// Invalidates the cached spatial information of the specified item.
    /// </summary>
    /// <param name="item">The item that has moved or changed its shape.</param>
    /// <remarks>
    /// This method informs the spatial partition that a specific item has moved or changed its 
    /// shape.
    /// </remarks>
    void Invalidate(T item);


    /// <summary>
    /// Updates the internal structure of this <see cref="ISpatialPartition{T}"/>.
    /// </summary>
    /// <param name="forceRebuild">
    /// If set to <see langword="true"/> the internal structure will be rebuilt from scratch. If set
    /// to <see langword="false"/> the spatial partition can decide to rebuild everything or refit 
    /// only the invalidated parts.
    /// </param>
    void Update(bool forceRebuild);
  }


  //public class OverlapEventArgs<T> : EventArgs
  //{
  //  /// <summary>
  //  /// Gets the new overlaps that were added.
  //  /// </summary>
  //  /// <value>The new overlaps (can be <see langword="null"/>).</value>
  //  public IList<Pair<T, T>> NewOverlaps { get; set; }


  //  /// <summary>
  //  /// Gets the old overlaps that were removed.
  //  /// </summary>
  //  /// <value>The old overlaps (can be <see langword="null"/>).</value>
  //  public IList<Pair<T, T>> OldOverlaps { get; set; }
  //}
}
