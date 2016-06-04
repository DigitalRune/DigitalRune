// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/* 
   The DynamicAabbTree is based on the dynamic bounding volume tree of Bullet.
   (Note: Our DynamicAabbTree and the original version of Bullet have only the general algorithm
   in common. The implementation is significantly different.)

     Bullet Continuous Collision Detection and Physics Library
     Copyright (c) 2003-2009 Erwin Coumans http://continuousphysics.com/Bullet/

     This software is provided 'as-is', without any express or implied warranty.
     In no event will the authors be held liable for any damages arising from the use of this software.
     Permission is granted to anyone to use this software for any purpose, 
     including commercial applications, and to alter it and redistribute it freely, 
     subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
     2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
     3. This notice may not be removed or altered from any source distribution.
  
     btDbvt implementation by Nathanael Presson
*/
#endregion

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Represents a dynamic bounding volume tree using axis-aligned bounding boxes (AABBs).
  /// </summary>
  /// <typeparam name="T">The type of item in the spatial partition.</typeparam>
  /// <remarks>
  /// <para>
  /// The <see cref="DynamicAabbTree{T}"/> was inspired by the dynamic bounding volume tree 
  /// (<c>btDbvt</c>) as implemented in the <see href="http://bulletphysics.org/">Bullet Continuous 
  /// Collision Detection and Physics Library</see>. (Original <c>btDbvt</c> implementation by 
  /// Nathanael Presson.)
  /// </para>
  /// <para>
  /// The <see cref="DynamicAabbTree{T}"/> was designed to manage deformable objects efficiently. It
  /// should be uses for <see cref="CompositeShape"/>s or <see cref="TriangleMeshShape"/>s when the
  /// the contained shapes or triangles are updated at runtime.
  /// </para>
  /// <para>
  /// It can also be used as a collision detection broad-phase (see 
  /// <see cref="CollisionDomain.BroadPhase"/>).
  /// </para>
  /// <para>
  /// <strong>Incremental Optimization:</strong> When items in the AABB tree are added, removed or
  /// moved the AABB tree might become unbalanced and less optimal for collision detection. 
  /// Therefore, the <see cref="DynamicAabbTree{T}"/> tries to optimize its tree structure over 
  /// time. In each frame (time step) it performs a number of optimization passes. The amount of 
  /// optimization per frame can be controlled by setting <see cref="OptimizationPerFrame"/>.
  /// </para>
  /// <para>
  /// <strong>Motion Prediction:</strong> The dynamic AABB tree is further optimized for 
  /// models/space where items are constantly moving. The <see cref="DynamicAabbTree{T}"/> 
  /// automatically detects when items are moving. It adds a small margin (see 
  /// <see cref="RelativeMargin"/>) to the AABB of these items to account for small random movements
  /// ("jittering") and it extends the AABB in the direction the items are moving (see 
  /// <see cref="MotionPrediction"/>). This reduces the number of required tree updates per frame. 
  /// However, the downside is that the AABB tree is more conservative (safe, but less accurate). It
  /// might return more overlaps ("false positives") than other types of spatial partitions.
  /// </para>
  /// <para>
  /// Motion predication can be enabled by setting <see cref="EnableMotionPrediction"/>. The feature
  /// is disabled by default.
  /// </para>
  /// <para>
  /// <strong><see cref="AdaptiveAabbTree{T}"/> vs. <see cref="DynamicAabbTree{T}"/>:</strong> The 
  /// <see cref="AdaptiveAabbTree{T}"/> and the <see cref="DynamicAabbTree{T}"/> are similar data
  /// structures. As a general rule, the <see cref="AdaptiveAabbTree{T}"/> should be used if
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
  /// Items are added or removed frequently. (Inserting or removing individual items into/from a 
  /// <see cref="DynamicAabbTree{T}"/> is fast.)
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
  /// However, please note these are just general rules. You should always try different 
  /// <see cref="ISpatialPartition{T}"/> types and measure which one yields the best performance in
  /// your application.
  /// </para>
  /// <para>
  /// <strong>Special handling of self-overlaps in 
  /// <see cref="GetOverlaps(ISpatialPartition{T})"/>:</strong> If 
  /// <see cref="GetOverlaps(ISpatialPartition{T})"/> is used to test an AABB tree against itself
  /// then overlaps of an item with itself are not returned. That means, each item A overlaps with
  /// itself, but (A, A) is not returned. And if two different items overlap, only one overlap is 
  /// returned, for example: If item A and item B overlap (A, B) or (B, A) is returned, but not 
  /// both.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public partial class DynamicAabbTree<T> : BasePartition<T>, ISupportClosestPointQueries<T>, ISupportFrustumCulling<T>
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
        node.Item = default(T);
        node.LeftChild = null;
        node.RightChild = null;
        node.Parent = null;
      });
    // ReSharper restore StaticFieldInGenericType


    /// <summary>
    /// The root node of a built tree.
    /// </summary>
    private Node _root;


    /// <summary>
    /// The leaves of the tree.
    /// </summary>
    private readonly Dictionary<T, Node> _leaves;


    /// <summary>
    /// Identifies the path to the node which is next in the incremental optimization procedure.
    /// </summary>
    private uint _optimizationPath;


    /// <summary>
    /// The number of nodes that should be optimized.
    /// </summary>
    private int _numberOfNodesToOptimize;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or set the amount of incremental optimization per frame (time step).
    /// </summary>
    /// <value>
    /// The amount of incremental optimization per frame. A value between 0 (no updates per frame) 
    /// and 1 (100% of the tree is updated per frame). The default value is 0.01.
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range. Allowed range is [0, 1].
    /// </exception>
    public float OptimizationPerFrame
    {
      get { return _optimizationPerFrame; }
      set
      {
        if (value < 0.0f || value > 1.0f)
          throw new ArgumentOutOfRangeException("value", "The amount of optimization per frame must in the range [0,1].");

        _optimizationPerFrame = value;
      }
    }
    private float _optimizationPerFrame = 0.01f;


    /// <summary>
    /// Gets a value indicating whether motion prediction is enabled.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if motion prediction is enabled; otherwise, <see langword="false"/>.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// Motion prediction analyzes the movement of items to estimate their velocity. The AABB of
    /// moving items is increased to avoid reduce tree updates because of movement. Motion
    /// prediction should be enabled if the spatial partition contains many moving objects.
    /// </remarks>
    public bool EnableMotionPrediction
    {
      get { return _enableMotionPrediction; }
      set
      {
        if (_enableMotionPrediction == value)
          return;

        _enableMotionPrediction = value;

        if (!_enableMotionPrediction)
        {
          // Invalidate the spatial partition to recalculate tight AABBs for moving items.
          Invalidate();
        }
      }
    }
    private bool _enableMotionPrediction;


    /// <summary>
    /// Gets or sets a relative margin that is added to the AABB of moving objects. (Only applied if
    /// motion prediction is enabled.)
    /// </summary>
    /// <value>
    /// <para>
    /// The relative margin that is added to the AABB of moving objects. (Only applied if motion 
    /// prediction is enabled.)
    /// </para>
    /// <para>
    /// The default value is 0.05. (The size of the AABB is increased by 5%.)
    /// </para>
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is out of range. Allowed range is [0, 1].
    /// </exception>
    public float RelativeMargin
    {
      get { return _relativeMargin; }
      set
      {
        if (value < 0.0f || 1.0f < value)
          throw new ArgumentOutOfRangeException("value", "The relative margin must in the range [0,1].");

        _relativeMargin = value;
      }
    }
    private float _relativeMargin = 0.05f;


    /// <summary>
    /// Gets or sets the number of frames a linear motion is predicted into the future. (Only 
    /// applied if motion prediction is enabled.)
    /// </summary>
    /// <value>
    /// The number of frames a linear motion is predicted into the future. The default value is 1.
    /// (Only applied if motion prediction is enabled.)
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="value"/> is negative.
    /// </exception>
    public float MotionPrediction
    {
      get { return _motionPrediction; }
      set
      {
        if (value <= 0)
          throw new ArgumentOutOfRangeException("value", "The motion prediction value must not be negative.");

        _motionPrediction = value;
      }
    }
    private float _motionPrediction = 1.0f;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicAabbTree{T}" /> class.
    /// </summary>
    public DynamicAabbTree()
    {
      _leaves = new Dictionary<T, Node>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override BasePartition<T> CreateInstanceCore()
    {
      return new DynamicAabbTree<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(BasePartition<T> source)
    {
      // Clone BasePartition<T> properties.
      base.CloneCore(source);

      // Clone DynamicAabbTree<T> properties.
      var sourceTyped = (DynamicAabbTree<T>)source;
      EnableMotionPrediction = sourceTyped.EnableMotionPrediction;
      MotionPrediction = sourceTyped.MotionPrediction;
      OptimizationPerFrame = sourceTyped.OptimizationPerFrame;
      RelativeMargin = sourceTyped.RelativeMargin;
    }
    #endregion


    private Node GetNode(T item)
    {
      Node node;
      return _leaves.TryGetValue(item, out node) ? node : null;
    }


    /// <inheritdoc/>
    public override void Invalidate(T item)
    {
      if (_enableMotionPrediction)
      {
        // Check whether item is still valid.
        var node = GetNode(item);
        if (node != null)
        {
          var aabb = GetAabbForItem(item);
          if (node.Aabb.Contains(aabb))
            return;
        }
      }

      base.Invalidate(item);
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    internal override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems)
    {
      // Check whether we need to track the nodes that need to be updated in UpdateSelfOverlaps().
      bool trackInvalidNodes = EnableSelfOverlaps                                   // Only keep track of nodes if self-overlaps are enabled.
                               && invalidItems != null                              // When invalidItems is null, we need to make a full update.
                               && (invalidItems.Count > 0 || addedItems.Count > 0); // Nothing to track, if there are no new or invalid items.

      // Keep track of the nodes that were added or updated. (The list is used in UpdateSelfOverlaps() 
      // so we don't need to look up the node for the item a second time.)
      List<Node> invalidNodes = (trackInvalidNodes) ? DigitalRune.ResourcePools<Node>.Lists.Obtain() : null;

      if (_root == null || forceRebuild)
      {
        // ----- Rebuild whole tree.
        Build();

        if (trackInvalidNodes)
        {
          foreach (T item in invalidItems)
            invalidNodes.Add(GetNode(item));

          foreach (T item in addedItems)
            invalidNodes.Add(GetNode(item));
        }

        // Clear optimization counter.
        _numberOfNodesToOptimize = 0;
      }
      else
      {
        // ----- Remove old items.
        if (removedItems.Count > 0)
        {
          foreach (T item in removedItems)
          {
            Node node = GetNode(item);
            RemoveLeaf(node);
            _leaves.Remove(item);
            Nodes.Recycle(node);
          }

          // Reset optimization counter.
          _numberOfNodesToOptimize = Count;
        }

        // ----- Update invalid items.
        if (invalidItems == null)
        {
          // ----- Refit entire tree.
          Refit(_root, null);

          // Reset optimization counter.
          _numberOfNodesToOptimize = Count;
        }
        else
        {
          // ----- Update items marked as invalid.
          foreach (T item in invalidItems)
          {
            Node node = GetNode(item);

            Aabb aabb = GetAabbForItem(item);
            UpdateLeaf(node, aabb);

            if (trackInvalidNodes)
              invalidNodes.Add(node);
          }

          // Reset optimization counter.
          _numberOfNodesToOptimize = Count;
        }

        // ----- Partial optimization.
        Optimize();

        // ----- Add new items.
        if (addedItems.Count > 0)
        {
          foreach (T addedItem in addedItems)
          {
            Node node = Nodes.Obtain();
            node.Aabb = GetAabbForItem(addedItem);
            node.Item = addedItem;
            AddLeaf(_root, node);
            _leaves.Add(addedItem, node);

            if (trackInvalidNodes)
              invalidNodes.Add(node);
          }

          _numberOfNodesToOptimize = Count;   // Reset optimization counter.
        }
      }

      // ----- Finally, update AABB and self-overlaps.
      UpdateAabb();
      UpdateSelfOverlaps(addedItems, removedItems, invalidItems, invalidNodes);

      // Clean up.
      if (invalidNodes != null)
        DigitalRune.ResourcePools<Node>.Lists.Recycle(invalidNodes);
    }


    /// <inheritdoc/>
    internal override void OnUpdate()
    {
      Optimize();
    }


    private void Optimize()
    {
      if (_numberOfNodesToOptimize > 0)
      {
        // Incrementally optimize tree.
        int numberOfPasses = (int)(_leaves.Count * OptimizationPerFrame);
        numberOfPasses = Math.Max(1, numberOfPasses);  // At least 1 pass.
        OptimizeIncrementally(numberOfPasses);
        _numberOfNodesToOptimize -= numberOfPasses;
      }
    }


    /// <summary>
    /// Updates the AABB of the spatial partition.
    /// </summary>
    private void UpdateAabb()
    {
      Aabb = (_root != null) ? _root.Aabb : new Aabb();
    }


    private void UpdateSelfOverlaps(HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems, List<Node> invalidNodes)
    {
      if (!EnableSelfOverlaps)
        return;

      // If the entire partition or a substantial amount of nodes is invalid use a tree vs. tree
      // checks. (Faster than leaf vs. tree checks. However, we need to determine the exact 
      // threshold at which tree vs. tree is cheaper. The current threshold of Count / 2 is just
      // a guess.)
      if (invalidItems == null || addedItems.Count + invalidItems.Count > Count / 2)
      {
        // Recompute all self-overlaps by making a tree vs. tree test.
        SelfOverlaps.Clear();

        if (_root != null)
        {
          // Important: Do not call GetOverlaps(this) because this would lead to recursive
          // Update() calls!
          // Note: Filtering is applied in GetOverlapsImpl(this).
          foreach (var overlap in GetOverlapsImpl(this))
            SelfOverlaps.Add(overlap);
        }
      }
      else
      {
        // Merge invalid and removed items into single set. 
        // Store result in invalidItems.
        if (invalidItems.Count > 0 && removedItems.Count > 0)
        {
          // Merge smaller set into larger set.
          if (invalidItems.Count < removedItems.Count)
            MathHelper.Swap(ref invalidItems, ref removedItems);

          foreach (var item in removedItems)
            invalidItems.Add(item);
        }
        else if (removedItems.Count > 0)
        {
          invalidItems = removedItems;
        }

        // Remove invalid entries from self-overlaps.
        if (invalidItems.Count > 0)
        {
          var invalidOverlaps = DigitalRune.ResourcePools<Pair<T>>.Lists.Obtain();
          foreach (var overlap in SelfOverlaps)
            if (invalidItems.Contains(overlap.First) || invalidItems.Contains(overlap.Second))
              invalidOverlaps.Add(overlap);

          foreach (var overlap in invalidOverlaps)
            SelfOverlaps.Remove(overlap);

          DigitalRune.ResourcePools<Pair<T>>.Lists.Recycle(invalidOverlaps);
        }

        // Compute new overlaps for all nodes that were updated in this frame.
        if (invalidNodes != null)
        {
          foreach (var node in invalidNodes)
          {
            foreach (var touchedItem in GetOverlapsImpl(node))
            {
              var overlap = new Pair<T>(node.Item, touchedItem);
              if (Filter == null || Filter.Filter(overlap))
                SelfOverlaps.Add(overlap);
            }
          }
        }
      }
    }
    #endregion
  }
}
