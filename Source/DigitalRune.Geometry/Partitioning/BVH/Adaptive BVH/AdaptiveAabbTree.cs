// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Builds a bounding volume tree using axis-aligned bounding boxes (AABBs), which adapts
  /// automatically when items are added, moved, or removed.
  /// </summary>
  /// <typeparam name="T">The type of item in the spatial partition.</typeparam>
  /// <remarks>
  /// <para>
  /// The <see cref="AdaptiveAabbTree{T}"/> is based on 
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Thomas Larsson: <i>"Adaptive Bounding Volume Hierarchies for Efficient Collision Queries"</i>, 
  /// Ph D Thesis, Mälardalen University Press, January, 2009
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Thomas Larsson, Tomas Akenine-Möller: <i>"A dynamic bounding volume hierarchy for generalized 
  /// collision detection"</i>, Computers &amp; Graphics, Volume 30, Issue 3, p.451-460, June, 2006
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// The <see cref="AdaptiveAabbTree{T}"/> was designed to manage deformable objects efficiently.
  /// It should be used for <see cref="CompositeShape"/>s or <see cref="TriangleMeshShape"/>s when
  /// the contained shapes or triangles are updated at runtime.
  /// </para>
  /// <para>
  /// <strong><see cref="AdaptiveAabbTree{T}"/> vs. <see cref="DynamicAabbTree{T}"/>:</strong>
  /// The <see cref="AdaptiveAabbTree{T}"/> and the <see cref="DynamicAabbTree{T}"/> are similar
  /// data structures. As a general rule, the <see cref="AdaptiveAabbTree{T}"/> should be used if
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// The entire spatial partition is invalidated regularly by calling 
  /// <see cref="ISpatialPartition{T}.Invalidate()"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// The spatial partition is used to compute inter-object overlaps and intra-object overlaps (see 
  /// <see cref="ISpatialPartition{T}.EnableSelfOverlaps"/>) are disabled.
  /// </description>
  /// </item>
  /// </list>
  /// Whereas the <see cref="DynamicAabbTree{T}"/> should be used if
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// Individual items are invalidated frequently by calling 
  /// <see cref="ISpatialPartition{T}.Invalidate(T)"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// Inter-objects and/or intra-object overlaps (see 
  /// <see cref="ISpatialPartition{T}.EnableSelfOverlaps"/>) are computed.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// <para>
  /// The <see cref="AdaptiveAabbTree{T}"/> should not be used as the collision detection 
  /// broad-phase (see <see cref="CollisionDomain.BroadPhase"/>). Whereas, a 
  /// <see cref="DynamicAabbTree{T}"/> can be used as the collision detection broad-phase.
  /// </para>
  /// <para>
  /// However, please note these are just general guidelines. You should always try different 
  /// <see cref="ISpatialPartition{T}"/> types and measure which one yields the best performance in
  /// your application.
  /// </para>
  /// <para>
  /// <strong>Special handling of self-overlaps in <see cref="GetOverlaps(ISpatialPartition{T})"/>:</strong> 
  /// If <see cref="GetOverlaps(ISpatialPartition{T})"/> is used to test an AABB tree against itself
  /// then overlaps of an item with itself are not returned; that means, each item A overlaps with
  /// itself but (A, A) is not returned. And if two different items overlap, only one overlap is 
  /// returned, for example: If item A and item B overlap (A, B) or (B, A) is returned but not both.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public partial class AdaptiveAabbTree<T> : BasePartition<T>, ISupportClosestPointQueries<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<Node> Nodes = new ResourcePool<Node>(
      () => new Node(),
      null,
      node =>
      {
        node.LeftChild = null;
        node.RightChild = null;
        node.Item = default(T);
        node.IsActive = false;
        node.IsDegenerate = true;
        if (node.Leaves != null)
        {
          DigitalRune.ResourcePools<Node>.Lists.Recycle(node.Leaves);
          node.Leaves = null;
        }
      });
    // ReSharper restore StaticFieldInGenericType


    /// <summary>
    /// The root node of a built tree.
    /// </summary>
    private Node _root;


    /// <summary>
    /// The leaves of the tree.
    /// </summary>
    private readonly List<Node> _leaves = new List<Node>();
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
      return new AdaptiveAabbTree<T>();
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
      if (_root == null || forceRebuild)
      {
        // ----- Rebuild whole tree.
        Build();
      }
      else
      {
        // Perform a full refit if all items or a substantial amount of items is invalid.
        bool fullRefit = (invalidItems == null || invalidItems.Count > 0.66f * Count);

        // ----- Remove old items.
        foreach (T removedItem in removedItems)
          RemoveLeaf(removedItem);

        // ----- Refit tree.
        if (_root != null)  // _root might become null if all items are removed above.
        {
          if (fullRefit)
            FullRefit(_root);
          else
            PartialRefit(_root, invalidItems);
        }

        // ----- Add new items.
        foreach (T addedItem in addedItems)
          AddLeaf(addedItem);
      }

      // ----- Finally, update AABB and self-overlaps.
      UpdateAabb();
      UpdateSelfOverlaps();
    }


    /// <summary>
    /// Updates the AABB of the spatial partition.
    /// </summary>
    private void UpdateAabb()
    {
      Aabb = (_root != null) ? _root.Aabb : new Aabb();
    }


    /// <summary>
    /// Updates the self-overlaps.
    /// </summary>
    private void UpdateSelfOverlaps()
    {
      if (EnableSelfOverlaps)
      {
        // Recompute all self-overlaps by making a tree vs. tree test.
        SelfOverlaps.Clear();

        if (_root != null)
        {
          // Important: Do not call GetOverlaps(this) because this would lead to recursive
          // Update() calls!
          foreach (var overlap in GetOverlapsImpl(this))
          {
            Debug.Assert(FilterSelfOverlap(overlap), "Filtering should have been applied.");
            SelfOverlaps.Add(overlap);
          }
        }
      }
    }
    #endregion
  }
}
