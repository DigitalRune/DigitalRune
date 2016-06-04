// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/* 
   The DualPartition is based on the dynamic bounding volume tree broadphase of Bullet.
   (Note: Our DualPartition and the original version of Bullet have only the general algorithm
   in common. The implementation is significantly different.)

     Bullet Continuous Collision Detection and Physics Library
     Copyright (c) 2003-2009 Erwin Coumans  http://continuousphysics.com/Bullet/ 

     This software is provided 'as-is', without any express or implied warranty.
     In no event will the authors be held liable for any damages arising from the use of this software.
     Permission is granted to anyone to use this software for any purpose, 
     including commercial applications, and to alter it and redistribute it freely, 
     subject to the following restrictions:

     1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
     2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
     3. This notice may not be removed or altered from any source distribution.
  
     btDbvtBroadphase implementation by Nathanael Presson
*/
#endregion


using System;
using System.Collections.Generic;
using System.Diagnostics;
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif
using DigitalRune.Collections;
using DigitalRune.Geometry.Collisions;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Represents a spatial partition that internally uses two spatial partitions to manage items: 
  /// one for static/sleeping items and one for dynamic items.
  /// </summary>
  /// <typeparam name="T">The type of item in the spatial partition.</typeparam>
  /// <remarks>
  /// <para>
  /// The <see cref="DualPartition{T}"/> can be used as a broad-phase algorithm (see 
  /// <see cref="CollisionDomain.BroadPhase"/>). It can in certain cases outperform the default
  /// <see cref="SweepAndPruneSpace{T}"/>.
  /// </para>
  /// <para>
  /// By default, two spatial partitions of type <see cref="DynamicAabbTree{T}"/> are used (with
  /// settings optimized for the static and dynamic case).
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public partial class DualPartition<T> : BasePartition<T>, ISupportBroadPhase<T>, ISupportFrustumCulling<T>
  {
    // TODO: The DualPartition<T> is not as efficient as the btDbvtBroadphase.
    // Create a DualDynamicAabbTree<T> where the static and dynamic partitions are
    // of type DynamicAabbTree<T>:
    // - DualDynamicAabbTree<T> and DynamicAabbTree<T> should not derive from BasePartition<T>. 
    // - If an item is invalidated, immediately check whether it is in the dynamic partition.
    //   If motion prediction is enabled and the AABB is still within the expanded AABB nothing
    //   needs to be done!
    // - Implement ISupportBroadPhase<T> and perform incremental updates: Instead of updating 
    //   all overlaps only test the nodes that have changed against the static and dynamic 
    //   partition. UpdateSelfOverlaps only adds overlaps. Perform a partial cleanup per frame
    //   to remove invalid overlaps.
    //   The static and dynamic partition do not have to track self-overlaps 
    //   (EnableSelfOverlaps = false). All checks are done in the DualDynamicAabbTree<T>.
    // - Create a custom collection for SelfOverlaps: HashSet<T> does not have an indexer and 
    //   is therefore not suited for partial cleanups!


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // New or invalid items are stored in stages. (Stage 0 and 1 are alternated.)
    private readonly HashSet<T> _stage0;
    private readonly HashSet<T> _stage1;
    private HashSet<T> _currentStage;
    private HashSet<T> _previousStage;

    // During update all items that are invalidated are moved from the static partition
    // or the previous stage into the current stage. At the end of the update:
    //   - The StaticPartition contains all items that were previously static and are still static.
    //   - The DynamicPartition contains all new and dynamic items.
    //   - The current stage contains all items that were added/moved in the current frame.
    //   - The previous stage contains all items that were previously dynamic, but haven't moved in
    //     the current frame.
    // All static items stay in the static partition.
    // All dynamic items are now in the dynamic partition.
    // All items in the previous stage are moved from the dynamic partition into the static
    // partition.
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the spatial partition that manages all static/sleeping objects.
    /// </summary>
    /// <value>The spatial partition that manages all static/sleeping objects.</value>
    internal ISpatialPartition<T> StaticPartition { get; set; }


    /// <summary>
    /// Gets or sets the spatial partition that manages all dynamic objects.
    /// </summary>
    /// <value>The spatial partition that manages all dynamic objects.</value>
    internal ISpatialPartition<T> DynamicPartition { get; set; }


    /// <inheritdoc/>
    IBroadPhase<T> ISupportBroadPhase<T>.BroadPhase
    {
      get { return _broadPhase; }
      set
      {
        _broadPhase = value;
        OnEnableSelfOverlapsChanged();
      }
    }
    private IBroadPhase<T> _broadPhase;


#if PORTABLE || WINDOWS
    /// <exclude/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      get
      {
        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.StaticPartition = StaticPartition;
        //internals.DynamicPartition = DynamicPartition;
        //return internals;

        IDictionary<string, Object> internals = new ExpandoObject();
        internals["StaticPartition"] = StaticPartition;
        internals["DynamicPartition"] = DynamicPartition;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="DualPartition{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="DualPartition{T}"/> class which uses two
    /// <see cref="DynamicAabbTree{T}"/> partitions.
    /// </summary>
    public DualPartition()
      : this(
          // Static partition
          new DynamicAabbTree<T>
          {
            OptimizationPerFrame = 0.01f,
            EnableMotionPrediction = false,
            BottomUpBuildThreshold = 0,
          },

          // Dynamic partition
          new DynamicAabbTree<T>
          {
            OptimizationPerFrame = 0.00f,
            EnableMotionPrediction = true,
            BottomUpBuildThreshold = 0,
          })
    {      
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="DualPartition{T}"/> class using the given
    /// pair of spatial partitions.
    /// </summary>
    /// <param name="staticPartition">
    /// The spatial partition used for static/sleeping objects.
    /// </param>
    /// <param name="dynamicPartition">
    /// The spatial partition used for dynamic partition.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="staticPartition"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dynamicPartition"/> is <see langword="null"/>.
    /// </exception>
    public DualPartition(ISpatialPartition<T> staticPartition, ISpatialPartition<T> dynamicPartition)
    {
      if (staticPartition == null)
        throw new ArgumentNullException("staticPartition");
      if (dynamicPartition == null)
        throw new ArgumentNullException("dynamicPartition");

      StaticPartition = staticPartition;
      DynamicPartition = dynamicPartition;

      _stage0 = new HashSet<T>();
      _stage1 = new HashSet<T>();
      _currentStage = _stage0;
      _previousStage = _stage1;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    protected override BasePartition<T> CreateInstanceCore()
    {
      return new DualPartition<T>();
    }


    /// <inheritdoc/>
    protected override void CloneCore(BasePartition<T> source)
    {
      // Clone BasePartition<T> properties.
      base.CloneCore(source);

      // Clone DualPartition<T> properties.
      var sourceTyped = (DualPartition<T>)source;
      sourceTyped.StaticPartition = sourceTyped.StaticPartition.Clone();
      sourceTyped.DynamicPartition = sourceTyped.DynamicPartition.Clone();
    }
    #endregion


    /// <inheritdoc/>
    internal override void OnFilterChanged()
    {
      StaticPartition.Filter = Filter;
      DynamicPartition.Filter = Filter;
    }


    /// <inheritdoc/>
    internal override void OnEnableSelfOverlapsChanged()
    {
      // If a collision detection broad phase is attached, the self-overlaps of
      // the static and dynamic partition need to be enabled.
      bool enableSelfOverlaps = (_broadPhase != null) || EnableSelfOverlaps;
      StaticPartition.EnableSelfOverlaps = enableSelfOverlaps;
      DynamicPartition.EnableSelfOverlaps = enableSelfOverlaps;
    }


    /// <inheritdoc/>
    internal override void OnGetAabbForItemChanged()
    {
      StaticPartition.GetAabbForItem = GetAabbForItem;
      DynamicPartition.GetAabbForItem = GetAabbForItem;
    }


    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    internal override void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems)
    {
      // First, remove invalid items.
      foreach (T removedItem in removedItems)
      {
        if (!StaticPartition.Remove(removedItem))
        {
          // Item was not in the static partition, must be in dynamic partition.
          bool removed = DynamicPartition.Remove(removedItem);
          Debug.Assert(removed, "Item not found in dynamic partition.");

          removed = _previousStage.Remove(removedItem);
          Debug.Assert(removed, "Item not found in previous stage.");
        }
      }

      // Second, add all new items.
      // New items are considered as dynamic.
      foreach (T addedItem in addedItems)
      {
        _currentStage.Add(addedItem);
        DynamicPartition.Add(addedItem);
      }

      // Handle invalid items.
      if (invalidItems != null)
      {
        // Partial invalidation.
        foreach (T invalidItem in invalidItems)
        {
          if (StaticPartition.Remove(invalidItem))
          {
            // Item was previously in static partition.
            // --> Move it into the dynamic partition.
            _currentStage.Add(invalidItem);
            DynamicPartition.Add(invalidItem);
          }
          else
          {
            // Item was not in the static partition, must be in dynamic partition.
            bool removed = _previousStage.Remove(invalidItem);
            Debug.Assert(removed, "Item not found in previous stage.");

            _currentStage.Add(invalidItem);
            DynamicPartition.Invalidate(invalidItem);
          }
        }

        if (_previousStage.Count > 0)
        {
          // Move static objects in dynamic partition back to static partition.
          foreach (T item in _previousStage)
          {
            bool removed = DynamicPartition.Remove(item);
            Debug.Assert(removed, "Item not found in dynamic partition.");

            StaticPartition.Add(item);
          }

          _previousStage.Clear();
        }
      }
      else
      {
        // Full invalidation.
        StaticPartition.Invalidate();
        DynamicPartition.Invalidate();

        // We could move all static items into the dynamic partition. However, we do not know the
        // cause of the invalidation. For now, we keep all static items in static stage and move all 
        // previously dynamic items into the current stage.
        // --> All partitions keep their items. (We need to validate whether this is the right
        //     solution in practice. However, in practice the DualPartition<T> should only be used 
        //     in the collision detection broad-phase where Invalidate() is never used.)
        foreach (T item in _previousStage)
          _currentStage.Add(item);

        _previousStage.Clear();
      }

      // Swap current and previous stage.
      MathHelper.Swap(ref _currentStage, ref _previousStage);

      // Update static and dynamic partition.
      // TODO: Update partitions in parallel!
      StaticPartition.Update(forceRebuild);
      DynamicPartition.Update(forceRebuild);

      UpdateAabb();
      UpdateBroadPhase();
      UpdateSelfOverlaps();
    }


    /// <inheritdoc/>
    internal override void OnUpdate()
    {
      StaticPartition.Update(false);
      DynamicPartition.Update(false);
    }


    /// <summary>
    /// Updates the AABB of the spatial partition.
    /// </summary>
    private void UpdateAabb()
    {
      bool staticPartitionValid = (StaticPartition.Count > 0);
      bool dynamicPartitionValid = (DynamicPartition.Count > 0);

      if (staticPartitionValid && dynamicPartitionValid)
        Aabb = Aabb.Merge(StaticPartition.Aabb, DynamicPartition.Aabb);
      else if (staticPartitionValid)
        Aabb = StaticPartition.Aabb;
      else if (dynamicPartitionValid)
        Aabb = DynamicPartition.Aabb;
      else
        Aabb = new Aabb();
    }


    private void UpdateBroadPhase()
    {
      if (_broadPhase == null)
        return;

      // Mark intra-overlaps of static partition.
      AddOrMarkAsUsed(StaticPartition.GetOverlaps());

      // Mark intra-overlaps of dynamic partition.
      AddOrMarkAsUsed(DynamicPartition.GetOverlaps());

      // Mark inter-overlaps between static and dynamic partition.
      AddOrMarkAsUsed(StaticPartition.GetOverlaps(DynamicPartition));

      // Remove all unused candidate pairs from broad phase.
      _broadPhase.RemoveUnused();
    }


    private void AddOrMarkAsUsed(IEnumerable<Pair<T>> overlaps)
    {
      // If possible cast to HashSet<T> to avoid garbage.
      var hashSet = overlaps as HashSet<Pair<T>>;
      if (hashSet != null)
      {
        foreach (var overlap in hashSet)
          _broadPhase.AddOrMarkAsUsed(overlap);
      }
      else
      {
        foreach (var overlap in overlaps)
          _broadPhase.AddOrMarkAsUsed(overlap);
      }
    }


    /// <summary>
    /// Recomputes the self-overlaps.
    /// </summary>
    private void UpdateSelfOverlaps()
    {
      if (!EnableSelfOverlaps)
        return;

      SelfOverlaps.Clear();

      // Get intra-overlaps from static partition.
      AddSelfOverlaps(StaticPartition.GetOverlaps());

      // Get intra-overlaps from dynamic partition.
      AddSelfOverlaps(DynamicPartition.GetOverlaps());

      // Get inter-overlaps between static and dynamic partition.
      AddSelfOverlaps(StaticPartition.GetOverlaps(DynamicPartition));
    }


    private void AddSelfOverlaps(IEnumerable<Pair<T>> overlaps)
    {
      var hashSet = overlaps as HashSet<Pair<T>>;
      if (hashSet != null)
      {
        // Cast to HashSet<T> to avoid garbage.
        foreach (var overlap in hashSet)
          AddSelfOverlap(overlap);
      }
      else
      {
        foreach (var overlap in overlaps)
          AddSelfOverlap(overlap);
      }
    }


    private void AddSelfOverlap(Pair<T> overlap)
    {
      Debug.Assert(FilterSelfOverlap(overlap), "Filtering should have been applied.");
      SelfOverlaps.Add(overlap);
    }
    #endregion
  }
}
