// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Represents a bounding volume tree using axis-aligned bounding boxes (AABBs).
  /// </summary>
  /// <typeparam name="T">The type of item in the spatial partition.</typeparam>
  /// <remarks>
  /// <para>
  /// <see cref="AabbTree{T}"/> partitions are good for partitioning static models or spaces where
  /// items are not changed at runtime or when the changes are small or local. For example, an 
  /// <see cref="AabbTree{T}"/> is appropriate for managing large static triangle meshes
  /// efficiently. But they are not suitable for dynamic models or spaces where items are
  /// added/removed at runtime or when the large changes are applied to items.
  /// </para>
  /// <para>
  /// Consider using the <see cref="CompressedAabbTree"/> instead of the <see cref="AabbTree{T}"/>
  /// if items of type <see cref="int"/> need to be stored. The <see cref="CompressedAabbTree"/>
  /// reduced the memory requirements considerably.
  /// </para>
  /// <para>
  /// <strong>Special handling of self-overlaps in 
  /// <see cref="GetOverlaps(ISpatialPartition{T})"/>:</strong> If 
  /// <see cref="GetOverlaps(ISpatialPartition{T})"/> is used to test an AABB tree against itself
  /// then overlaps of an item with itself are not returned; that means, each item A overlaps with
  /// itself but (A, A) is not returned. And if two different items overlap, only one overlap is 
  /// returned, for example: If item A and item B overlap (A, B) or (B, A) is returned but not both.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  public partial class AabbTree<T> : BasePartition<T>, ISupportClosestPointQueries<T>
  {
    // Note: We could create a common abstract base class 'AbstractAabbTree<T>' for all AABB trees.
    // The base class could implement ISpatialPartition<T> and ISupportClosestPointQueries<T> and
    // even automatically handle tree vs. tree tests. However, tests have shown that this reduces
    // performance by 50%! (The GetOverlap-methods take twice as long! The AABB tree nodes must use 
    // properties instead of fields, ...)


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    internal static readonly ResourcePool<FastStack<AabbTree<T>.Node>> Stacks =
      new ResourcePool<FastStack<AabbTree<T>.Node>>(
        () => new FastStack<AabbTree<T>.Node>(32),
        null,
        stack => stack.Clear());


    /// <summary>
    /// The root node of a built tree.
    /// </summary>
    private Node _root;


    /// <summary>
    /// The leaves of the tree.
    /// </summary>
    private Node[] _leaves;

    private int _height;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override BasePartition<T> CreateInstanceCore()
    {
      return new AabbTree<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(BasePartition<T> source)
    {
      base.CloneCore(source);
    }
    #endregion


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    internal override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems)
    {
      if (forceRebuild || addedItems.Count > 0 || removedItems.Count > 0)
      {
        // Rebuild whole tree if forceBuild is set or the items collection has changed.
        Build();
      }
      else
      {
        // Update leave AABBs.
        Debug.Assert(addedItems.Count == 0 && removedItems.Count == 0);
        Debug.Assert(_root != null && _leaves != null, "Cannot refit an empty tree.");

        Refit(_root, invalidItems);
      }

      UpdateAabb();
      UpdateSelfOverlaps();
    }


    private void UpdateAabb()
    {
      Aabb = (_root != null) ? _root.Aabb : new Aabb();
    }


    private void UpdateSelfOverlaps()
    {
      if (EnableSelfOverlaps)
      {
        // Update self-overlaps.
        SelfOverlaps.Clear();

        // ----- Compute self-overlaps using tree vs. tree test.

        if (_root == null)
          return;

        // Important: Do not call GetOverlaps(this) because this would lead to recursive
        // Update() calls!
        foreach (var overlap in GetOverlapsImpl(this))
        {
          Debug.Assert(FilterSelfOverlap(overlap), "Filtering should have been applied.");
          SelfOverlaps.Add(overlap);
        }

        // ----- Compute self-overlaps using leaf vs. tree test.
        //if (_root != null)
        //{
        //  foreach (var leave in _leaves)
        //  {
        //    foreach (var touchedItem in GetOverlaps(leave.Aabb))
        //    {
        //      var overlap = new Pair<T>(leave.Item, touchedItem);

        //      if (FilterSelfOverlap(overlap))
        //        SelfOverlaps.Add(overlap);
        //    }
        //  }
        //}
      }
    }
    #endregion
  }
}
