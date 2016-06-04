// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Partitioning;


namespace DigitalRune.Geometry.Collisions
{
  /// <summary>
  /// A collision detection broad phase method which computes candidates for narrow phase
  /// collision detection and filters out objects which cannot collide.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Collision detection with a <see cref="CollisionDomain"/> works in two phases: 
  /// <list type="bullet">
  /// <item>
  /// <description>
  /// In the broad phase object pairs which cannot collide are sorted out. Object pairs which can 
  /// collide are stored in a list of <see cref="CandidatePairs"/>.
  /// </description>
  /// </item>
  /// <item>
  /// <description>
  /// In the narrow phase the collision info for the candidate pairs is computed.
  /// </description>
  /// </item>
  /// </list>
  /// </para>
  /// </remarks>
  internal sealed class CollisionDetectionBroadPhase : IBroadPhase<CollisionObject>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly CollisionDomain _collisionDomain;

    // A partition that has special broad phase support. If a spatial partition implements
    // ISupportBroadPhase, it calls the callbacks OnClear/OnAdded/OnRemoved which makes
    // the bookkeeping of ProcessOverlaps unnecessary.
    private ISupportBroadPhase<CollisionObject> _broadPhasePartition;

    // List that stores contact set that should be recycled.
    // (We cannot immediately recycle contact sets because the physics simulation might 
    // still store a reference in one of the contact constraints. The contact sets need 
    // to be valid until the next update of the collision domain.)
    private readonly List<ContactSet> _obsoleteContactSetList = new List<ContactSet>();
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the candidate pairs.
    /// </summary>
    /// <value>The candidate pairs.</value>
    /// <remarks>
    /// This collection contains all pairs which may collide and should be checked in the collision
    /// detection narrow phase. Do not keep a reference to this object, it may change!
    /// </remarks>
    public ContactSetCollection CandidatePairs { get; private set; }


    /// <summary>
    /// Gets or sets the spatial partition that does the broad phase work.
    /// </summary>
    /// <value>The spatial partition.</value>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.
    /// </exception>
    public ISpatialPartition<CollisionObject> SpatialPartition
    {
      get { return _spatialPartition; }
      set
      {
        if (value == null)
          throw new ArgumentNullException("value");

        if (value == _spatialPartition)
          return;

        // Clear old spatial partition before we throw it away.
        if (_spatialPartition != null)
        {
          _spatialPartition.GetAabbForItem = null;
          _spatialPartition.Clear();

          // Unregister broad phase.
          if (_broadPhasePartition != null)
            _broadPhasePartition.BroadPhase = this;
        }

        // Set new spatial partition.
        _spatialPartition = value;

        // Set a callback method that computes the AABB of each collision object.
        _spatialPartition.GetAabbForItem = collisionObject => collisionObject.GeometricObject.Aabb;

        // Check for ISupportBroadPhase.
        _broadPhasePartition = _spatialPartition as ISupportBroadPhase<CollisionObject>;
        if (_broadPhasePartition != null)
        {
          // Register broad phase.
          _broadPhasePartition.BroadPhase = this;
        }
        else
        {
          // We need self-overlaps.
          _spatialPartition.EnableSelfOverlaps = true;
        }

        // Tabula rasa.
        if (CandidatePairs != null)
        {
          // Recycle contacts and contact sets.
          foreach (var contactSet in CandidatePairs)
            contactSet.Recycle(true);

          CandidatePairs.Clear();
        }

        // Add all collision objects and set Changed flags.
        // (We have removed the contact sets and must recompute all collisions.)
        _spatialPartition.Clear();
        foreach (var collisionObject in _collisionDomain.CollisionObjects)
        {
          collisionObject.Changed = true;
          _spatialPartition.Add(collisionObject);
        }
      }
    }
    private ISpatialPartition<CollisionObject> _spatialPartition;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CollisionDetectionBroadPhase"/> class.
    /// </summary>
    public CollisionDetectionBroadPhase(CollisionDomain collisionDomain)
    {
      _collisionDomain = collisionDomain;

      // Register event handler.
      _collisionDomain.CollisionObjects.CollectionChanged += OnCollisionObjectsChanged;

      // Per default we use Sweep and Prune.
      SpatialPartition = new SweepAndPruneSpace<CollisionObject>();

      CandidatePairs = new ContactSetCollection();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    // Adds/Removes objects in the spatial partition.
    private void OnCollisionObjectsChanged(object sender, CollectionChangedEventArgs<CollisionObject> eventArgs)
    {
      if (eventArgs.Action == CollectionChangedAction.Move)
        return;

      var oldItems = eventArgs.OldItems;
      var numberOfOldItems = oldItems.Count;
      for (int i = 0; i < numberOfOldItems; i++)
      {
        CollisionObject oldItem = oldItems[i];
        SpatialPartition.Remove(oldItem);
      }

      var newItems = eventArgs.NewItems;
      var numberOfNewItems = newItems.Count;
      for (int i = 0; i < numberOfNewItems; i++)
      {
        CollisionObject newItem = newItems[i];
        SpatialPartition.Add(newItem);
      }
    }


    /// <summary>
    /// Needs to be called before <see cref="Update()"/> at the start of a new frame.
    /// </summary>
    internal void NewFrame()
    {
      // Recycle all contact sets which were removed in the previous frame.
      foreach (var contactSet in _obsoleteContactSetList)
        contactSet.Recycle(true);

      _obsoleteContactSetList.Clear();
    }


    /// <summary>
    /// Updates the candidate pairs.
    /// </summary>
    internal void Update()
    {
      // Invalidate all Changed objects.
      int numberOfCollisionObjects = _collisionDomain.CollisionObjects.Count;
      for (int i = 0; i < numberOfCollisionObjects; i++)
      {
        CollisionObject collisionObject = _collisionDomain.CollisionObjects[i];

        if (collisionObject.Changed)
          SpatialPartition.Invalidate(collisionObject);
      }

      SpatialPartition.Update(false);

      // If the spatial partition does not implement ISupportBroadPhase, we have
      // to use SpatialPartition.GetOverlaps and do the bookkeeping.
      if (_broadPhasePartition == null)
        Synchronize();
    }


    /// <summary>
    /// Updates the candidate pairs for a single collision object.
    /// </summary>
    /// <param name="collisionObject">The collision object.</param>
    internal void Update(CollisionObject collisionObject)
    {
      Debug.Assert(collisionObject != null);

      SpatialPartition.Invalidate(collisionObject);
      SpatialPartition.Update(false);

      if (_broadPhasePartition == null)
        Synchronize();
    }


    /// <summary>
    /// Synchronizes the contact set collections with self-overlaps in the spatial partition.
    /// </summary>
    private void Synchronize()
    {
      var overlaps = SpatialPartition.GetOverlaps();

      // We know that SAP uses a HashSet. If possible use a HashSet in foreach to 
      // avoid allocating an enumerator on the heap.
      var overlapsHashSet = overlaps as HashSet<Pair<CollisionObject>>;
      if (overlapsHashSet != null)
      {
        // Use struct Enumerator of HashSet.
        foreach (var overlap in overlapsHashSet)
          CandidatePairs.AddOrMarkAsUsed(overlap);
      }
      else
      {
        // Use IEnumerator<T>.
        foreach (var overlap in overlaps)
          CandidatePairs.AddOrMarkAsUsed(overlap);
      }

      CandidatePairs.RemoveUnused(_obsoleteContactSetList);
      Debug.Assert(overlaps.Count() == CandidatePairs.Count);
    }


    #region ----- IBroadPhase<T> -----

    /// <summary>
    /// Called when all self-overlaps of the spatial partition were removed.
    /// </summary>
    void IBroadPhase<CollisionObject>.Clear()
    {
      foreach (var contactSet in CandidatePairs)
        _obsoleteContactSetList.Add(contactSet);

      CandidatePairs.Clear();
    }


    /// <summary>
    /// Called when the spatial partition detected a new overlap.
    /// </summary>
    /// <param name="overlap">The overlapping pair of collision objects.</param>
    void IBroadPhase<CollisionObject>.Add(Pair<CollisionObject> overlap)
    {
      CandidatePairs.Add(overlap);
    }


    /// <summary>
    /// Called when the spatial partition detected that a collision object was removed.
    /// </summary>
    /// <param name="collisionObject">The collision objects to remove.</param>
    void IBroadPhase<CollisionObject>.Remove(CollisionObject collisionObject)
    {
      CandidatePairs.Remove(collisionObject, _obsoleteContactSetList);
    }


    /// <summary>
    /// Called when the spatial partition detected that an old overlap was removed.
    /// </summary>
    /// <param name="overlap">The overlapping pair of collision objects.</param>
    void IBroadPhase<CollisionObject>.Remove(Pair<CollisionObject> overlap)
    {
      var contactSet = CandidatePairs.Remove(overlap);
      if (contactSet != null)
        _obsoleteContactSetList.Add(contactSet);
    }


    void IBroadPhase<CollisionObject>.AddOrMarkAsUsed(Pair<CollisionObject> overlap)
    {
      CandidatePairs.AddOrMarkAsUsed(overlap);
    }


    void IBroadPhase<CollisionObject>.RemoveUnused()
    {
      CandidatePairs.RemoveUnused(_obsoleteContactSetList);
    }
    #endregion

    #endregion
  }
}
