// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Provides basic functionality of an <see cref="ISpatialPartition{T}"/>. (For internal use 
  /// only.)
  /// </summary>
  /// <typeparam name="T">The type of items in the spatial partition.</typeparam>

  // <remarks>
  // <para>
  // This abstract class can be used as the base of a spatial partitioning scheme that uses
  // axis-aligned bounding boxes.
  // </para>
  // <para>
  // <strong>AABB Computation of Items:</strong> When creating an instance of an 
  // <see cref="BasePartition{T}"/> a callback that computes the <see cref="Shapes.Aabb"/> for
  // a given item must be specified. The spatial partition does not know how to compute the 
  // positions and extents of the items. The <see cref="GetAabbForItem"/> delegate is used to 
  // compute an <see cref="Shapes.Aabb"/>s for each item. The computed <see cref="Shapes.Aabb"/> is 
  // used to define the spatial properties of an item. For a single item the method must always 
  // return the same <see cref="Aabb"/>. If the AABB of an item has changed (e.g. the item has 
  // moved or changed shape), <see cref="Invalidate(T)"/> must be called.
  // </para>
  // <para>
  // <strong>Notes to Inheritors:</strong> This abstract class can be used as a base class for 
  // <see cref="ISpatialPartition{T}"/>. Derived classes must implement the methods 
  // <see cref="GetOverlaps(DigitalRune.Geometry.Shapes.Aabb)"/> and <see cref="OnUpdate"/>. This
  // class provides basic implementations for all other methods. For better performance it is
  // recommended that derived classes override the other <strong>GetOverlaps</strong> methods. 
  // (Note: Do not forget to automatically call <see cref="Update"/> in the 
  // <strong>GetOverlaps</strong> methods if required.)
  // </para>
  // <para>
  // When creating self-overlaps derived classes can use the helper method 
  // <see cref="FilterSelfOverlap"/> (see method description).
  // </para>
  // </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public abstract partial class BasePartition<T> : ISpatialPartition<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    /// <summary>
    /// The comparer that is used to compare items.
    /// </summary>
    internal static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;


    // True if the spatial partition is invalid because of new/removed/invalidated items, filter 
    // changes, ...
    private volatile bool _isInvalid = true;     // volatile because it is possibly accessed from several threads in Update!
    private volatile bool _needsRebuild = true;  // volatile because it is possibly accessed from several threads in Update!

    // True if multiple items have changed, but it is unclear which.
    private bool _invalidateAll;

    // Added/removed/invalided items will be collected in these collections.
    // The sets must be disjoint, even if the user adds A, invalidates A and then calls Update().
    private HashSet<T> _addedItems;
    private HashSet<T> _removedItems;
    private HashSet<T> _invalidItems;

    // Synchronization object for Update().
    private readonly object _syncRoot = new object();
    private bool _updateInProgress;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the axis-aligned bounding box (AABB) that contains all items.
    /// </summary>
    /// <value>The axis-aligned bounding box (AABB) that contains all items.</value>
    public Aabb Aabb
    {
      get
      {
        UpdateInternal();   // Make sure we are up-to-date.
        return _aabb;
      }
      protected set
      {
        _aabb = value;
      }
    }
    private Aabb _aabb;


    /// <summary>
    /// Gets the number of items contained in the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <value>The number of items contained in the <see cref="ICollection{T}"/>.</value>
    public int Count
    {
      get { return Items.Count; }
    }


    /// <inheritdoc/>
    public bool EnableSelfOverlaps
    {
      get { return _selfOverlaps != null; }
      set
      {
        if (value)
        {
          // Enable self-overlaps if not already enabled.
          if (_selfOverlaps == null)
          {
            _selfOverlaps = new HashSet<Pair<T>>();

            // Simply force a total rebuild.
            // EnableSelfOverlaps should not be changed frequently.
            Invalidate();
            _needsRebuild = true;
          }
        }
        else
        {
          // Disable self-overlaps.
          _selfOverlaps = null;
        }

        OnEnableSelfOverlapsChanged();
      }
    }


    /// <inheritdoc/>
    public IPairFilter<T> Filter
    {
      get { return _filter; }
      set
      {
        if (_filter != value)
        {
          if (_filter != null)
            _filter.Changed -= OnFilterChanged;

          _filter = value;

          // Call event handler manually to invalidate the partition.
          OnFilterChanged(this, EventArgs.Empty);

          // Call virtual method OnFilterChanged to notify derived classes.
          OnFilterChanged();

          if (_filter != null)
            _filter.Changed += OnFilterChanged;
        }
      }
    }
    private IPairFilter<T> _filter;


    /// <inheritdoc/>
    public Func<T, Aabb> GetAabbForItem
    {
      get { return _getAabbForItem; }
      set
      {
        _getAabbForItem = value;

        // Call virtual method OnGetAabbForItemChanged() to notify derived classes.
        OnGetAabbForItemChanged();
      }
    }
    private Func<T, Aabb> _getAabbForItem;


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<T>.IsReadOnly { get { return false; } }


    /// <summary>
    /// Gets or sets the items in the spatial partition.
    /// </summary>
    /// <value>The items in the spatial partition.</value>
    internal HashSet<T> Items
    {
      get { return _items; }
    }
    private readonly HashSet<T> _items;


    /// <summary>
    /// Gets the self-overlaps.
    /// </summary>
    /// <value>
    /// The self-overlaps. The default value is <see langword="null"/>. When 
    /// <see cref="EnableSelfOverlaps"/> is set to <see langword="true"/> then this property will be 
    /// initialized with an empty <see cref="HashSet{T}"/>. (When <see cref="EnableSelfOverlaps"/>
    /// is set to <see langword="false"/> this property will be reset to <see langword="null"/>.)
    /// </value>
    /// <remarks>
    /// This set must be managed in <see cref="OnUpdate(bool,HashSet{T},HashSet{T},HashSet{T})"/> of
    /// derived classes (if <see cref="EnableSelfOverlaps"/> is <see langword="true"/>). This base 
    /// class does not add overlaps to this set - but it automatically removes overlaps of items 
    /// that are removed using <see cref="Remove"/>.
    /// </remarks>
    internal HashSet<Pair<T>> SelfOverlaps
    {
      get { return _selfOverlaps; }
    }
    private HashSet<Pair<T>> _selfOverlaps;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="BasePartition{T}"/> class.
    /// </summary>
    protected BasePartition()
    {
      _items = new HashSet<T>();
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    ISpatialPartition<T> ISpatialPartition<T>.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="BasePartition{T}"/> that is a clone (deep copy) of the current
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="BasePartition{T}"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> The method <see cref="Clone"/> calls 
    /// <see cref="CreateInstanceCore"/> which is responsible for creating a new instance of the 
    /// <see cref="BasePartition{T}"/> derived class and <see cref="CloneCore"/> to create a copy
    /// of the current instance. Classes that derive from <see cref="BasePartition{T}"/> need to
    /// implement <see cref="CreateInstanceCore"/> and <see cref="CloneCore"/>.
    /// </remarks>
    public BasePartition<T> Clone()
    {
      var clone = CreateInstance();
      clone.CloneCore(this);
      return clone;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="BasePartition{T}"/> class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// This is a protected method, and the actual object-specific implementations for the behavior 
    /// are dependent on the override implementation of the <see cref="CreateInstanceCore"/> method,
    /// which this method calls internally. 
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Cannot clone shape. A class derived from <see cref="BasePartition{T}"/> does not
    /// implement <see cref="CreateInstanceCore"/>."
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private BasePartition<T> CreateInstance()
    {
      var newInstance = CreateInstanceCore();
      if (GetType() != newInstance.GetType())
      {
        string message = String.Format(
          CultureInfo.InvariantCulture,
          "Cannot clone shape. The derived class {0} does not implement CreateInstanceCore().",
          GetType());

        throw new InvalidOperationException(message);
      }

      return newInstance;
    }


    /// <summary>
    /// When implemented in a derived class, creates a new instance of the 
    /// <see cref="BasePartition{T}"/> derived class. 
    /// </summary>
    /// <returns>The new instance.</returns>
    /// <remarks>
    /// <para>
    /// Do not call this method directly (except when calling base in an implementation). This 
    /// method is called internally by the <see cref="Clone"/> method whenever a new instance of the
    /// <see cref="BasePartition{T}"/> is created. 
    /// </para>
    /// <para>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="BasePartition{T}"/> derived class 
    /// must implement this method. A typical implementation is to simply call the default
    /// constructor and return the result. 
    /// </para>
    /// </remarks>
    protected abstract BasePartition<T> CreateInstanceCore();


    /// <summary>
    /// Makes the instance a clone (deep copy) of the specified <see cref="BasePartition{T}"/>.
    /// </summary>
    /// <param name="source">The object to clone.</param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> Every <see cref="BasePartition{T}"/> derived class 
    /// must implement this method. A typical implementation is to call <c>base.CloneCore(this)</c> 
    /// to copy all properties of the base class and then copy all properties of the derived class.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
    protected virtual void CloneCore(BasePartition<T> source)
    {
      EnableSelfOverlaps = source.EnableSelfOverlaps;
      Filter = source.Filter;
      GetAabbForItem = source.GetAabbForItem;
    }
    #endregion


    private static void EnsureSet(ref HashSet<T> set)
    {
      if (set == null)
        set = DigitalRune.ResourcePools<T>.HashSets.Obtain();
    }


    private static void ClearSet(ref HashSet<T> set)
    {
      if (set != null)
      {
        DigitalRune.ResourcePools<T>.HashSets.Recycle(set);
        set = null;
      }
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A enumerator that can be used to iterate through the collection.
    /// </returns>
    public HashSet<T>.Enumerator GetEnumerator()
    {
      return Items.GetEnumerator();
    }


    /// <summary>
    /// Adds an item to the <see cref="BasePartition{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="BasePartition{T}"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    public void Add(T item)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (item == null)
        throw new ArgumentNullException("item");
      // ReSharper restore CompareNonConstrainedGenericWithNull

      _isInvalid = true;

      Items.Add(item);

      // If the item was in the collection, was removed and is now re-added.
      // --> Put item into invalid-items list instead of the added-items list.
      var wasRemoved = (_removedItems != null && _removedItems.Remove(item));
      if (wasRemoved)
      {
        if (!_invalidateAll)
        {
          EnsureSet(ref _invalidItems);
          _invalidItems.Add(item);
        }
      }
      else
      {
        EnsureSet(ref _addedItems);
        _addedItems.Add(item);
      }
    }


    /// <summary>
    /// Removes all items from the <see cref="BasePartition{T}"/>.
    /// </summary>
    public void Clear()
    {
      _isInvalid = true;

      // Throw away new items.
      ClearSet(ref _addedItems);

      // Move all previous items to removed-items list.
      EnsureSet(ref _removedItems);
      foreach (var item in Items)
        _removedItems.Add(item);

      // Clear invalid-items list.
      ClearSet(ref _invalidItems);
      _invalidateAll = false;

      Items.Clear();
      if (SelfOverlaps != null)
        SelfOverlaps.Clear();
    }


    /// <summary>
    /// Determines whether the <see cref="BasePartition{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="BasePartition{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="BasePartition{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(T item)
    {
      return Items.Contains(item);
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>, starting 
    /// at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// One of the following conditions:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <paramref name="array"/> is multidimensional.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <paramref name="arrayIndex"/> is equal to or greater than the length of 
    /// <paramref name="array"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// The number of elements in the source <see cref="ICollection{T}"/> is greater than the 
    /// available space from <paramref name="arrayIndex"/> to the end of the destination 
    /// <paramref name="array"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Type <typeparamref name="T"/> cannot be cast automatically to the type of the destination 
    /// <paramref name="array"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
      Items.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="BasePartition{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to remove from the <see cref="BasePartition{T}"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the
    /// <see cref="BasePartition{T}"/>; otherwise, <see langword="false"/>. This method also
    /// returns <see langword="false"/> if <paramref name="item"/> is not found in the original
    /// <see cref="BasePartition{T}"/>.
    /// </returns>
    public bool Remove(T item)
    {
      // Remove item - abort if the item was not in the Items list.
      bool wasRemoved = Items.Remove(item);
      if (!wasRemoved)
        return false;

      // Check if the item was one of the new ones.
      bool wasNew = (_addedItems != null && _addedItems.Remove(item));
      if (wasNew)
      {
        // The item was new. So we don't have anything to clean up.
        return true;
      }

      // The item was in the Items list.
      _isInvalid = true;

      if (_invalidItems != null)
        _invalidItems.Remove(item);

      EnsureSet(ref _removedItems);
      _removedItems.Add(item);

      return true;
    }


    /// <summary>
    /// Checks if the pair of items is a valid self-overlap.
    /// </summary>
    /// <param name="pair">The pair of items.</param>
    /// <returns>
    /// <see langword="true"/> if the pair should be accepted; otherwise, <see langword="false"/> if
    /// the pair should be rejected.
    /// </returns>
    /// <remarks>
    /// This method returns <see langword="false"/> if the given items are identical or if the 
    /// <see cref="Filter"/> returns <see langword="false"/>. This method does NOT check AABB
    /// overlaps.
    /// </remarks>
    internal bool FilterSelfOverlap(Pair<T> pair)
    {
      // Check for equality.
      if (Comparer.Equals(pair.First, pair.Second))
        return false;

      // Check Filter.
      return Filter == null || Filter.Filter(pair);
    }


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
    public void Invalidate()
    {
      _isInvalid = true;

      // All items need to be updated.
      ClearSet(ref _invalidItems);
      _invalidateAll = true;
    }


    /// <summary>
    /// Invalidates the cached spatial information of the specified item.
    /// </summary>
    /// <param name="item">The item that has moved or changed its shape.</param>
    /// <remarks>
    /// This method informs the spatial partition that a specific item has moved or changed its
    /// shape.
    /// </remarks>
    public virtual void Invalidate(T item)
    {
      _isInvalid = true;

      if (_invalidateAll)
      {
        // All items are already marked as invalid.
        return;
      }

      // Add item to invalid-items list.
      if (_addedItems == null || !_addedItems.Contains(item))    // New items should not be added to both lists.
      {
        EnsureSet(ref _invalidItems);
        _invalidItems.Add(item);
      }
    }


    /// <summary>
    /// Updates the internal structure of this <see cref="ISpatialPartition{T}"/>.
    /// </summary>
    /// <param name="forceRebuild">
    /// If set to <see langword="true"/> the internal structure will be rebuilt from scratch. If set 
    /// to <see langword="false"/> the spatial partition can decide to rebuild everything or refit 
    /// only the invalidated parts.
    /// </param>
    /// <exception cref="GeometryException">
    /// Cannot update spatial partition. The property <see cref="GetAabbForItem"/> is not set.
    /// </exception>
    public void Update(bool forceRebuild)
    {
      Update(forceRebuild, false);
    }


    /// <summary>
    /// Updates the internal structure of this <see cref="ISpatialPartition{T}"/>. Should be called
    /// by derived classes instead of <see cref="Update(bool)"/>!
    /// </summary>
    /// <exception cref="GeometryException">
    /// Cannot update spatial partition. The property <see cref="GetAabbForItem"/> is not set.
    /// </exception>
    internal void UpdateInternal()
    {
      Update(false, true);
    }


    /// <summary>
    /// Updates the internal structure of this <see cref="ISpatialPartition{T}"/>.
    /// </summary>
    /// <param name="forceRebuild">
    /// If set to <see langword="true"/> the internal structure will be rebuilt from scratch. If set
    /// to <see langword="false"/> the spatial partition can decide to rebuild everything or refit
    /// only the invalidated parts.
    /// </param>
    /// <param name="isInternalUpdate">
    /// <see langword="true"/> if the update is caused by an internal method. Internal updates can 
    /// occur frequently. <see langword="false"/> if the update is caused by an external class such 
    /// as the collision detection broad phase. External updates occur less frequently, e.g. once 
    /// per frame.
    /// </param>
    /// <exception cref="GeometryException">
    /// Cannot update spatial partition. The property <see cref="GetAabbForItem"/> is not set.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void Update(bool forceRebuild, bool isInternalUpdate)
    {
      // Important: This method must not be called recursively!!!

      Debug.Assert(
        _addedItems == null
        || _removedItems == null
        || !_addedItems.Intersect(_removedItems).Any(),
        "_addedItems and _removedItems must be disjoint.");

      Debug.Assert(
        _removedItems == null
        || _invalidItems == null
        || !_removedItems.Intersect(_invalidItems).Any(),
        "_removedItems and _invalidItems must be disjoint.");

      // Check to avoid unnecessary lock.
      if (!forceRebuild && !_isInvalid && !_needsRebuild && isInternalUpdate)
        return;

      if (GetAabbForItem == null)
        throw new GeometryException("Cannot build spatial partition. The property GetAabbForItem of the spatial partition is not set.");

      lock (_syncRoot)
      {
        // Check state again after we have acquired the lock.
        if (!forceRebuild && !_isInvalid && !_needsRebuild)
        {
          if (!isInternalUpdate)
          {
            // Special update where Items collection was not changed, but
            // DynamicAabbTree wants to optimize itself.
            OnUpdate();
          }

          return;
        }

        if (_updateInProgress)
          throw new InvalidOperationException("Recursive call of Update() detected. Update() must not be called recursively!!!");

        try
        {
          _updateInProgress = true;

          // Rebuild is demanded or something was added/invalidated/removed or filter
          // was changed...

          bool rebuild = forceRebuild || _needsRebuild;

          // Set invalid aabb.
          _aabb = new Aabb(Vector3F.One, Vector3F.Zero);
          Debug.Assert(_aabb.Minimum > _aabb.Maximum);

          EnsureSet(ref _addedItems);
          EnsureSet(ref _removedItems);

          HashSet<T> invalidItems;
          if (_invalidateAll)
          {
            // Update all items in OnUpdate():
            // invalidItems == null indicates that all items should be updated.
            invalidItems = null;
          }
          else
          {
            // Update only the items marked as invalid.
            EnsureSet(ref _invalidItems);
            invalidItems = _invalidItems;
          }

          if ((GlobalSettings.ValidationLevelInternal & GlobalSettings.ValidationLevelUserHighExpensive) != 0)
          {
            ValidateItems(_addedItems);
            ValidateItems(invalidItems);
          }

          OnUpdate(rebuild, _addedItems, _removedItems, invalidItems);

          ClearSet(ref _addedItems);
          ClearSet(ref _removedItems);
          ClearSet(ref _invalidItems);
          _invalidateAll = false;

          if (_aabb.Minimum.X > _aabb.Maximum.X)
            UpdateAabb();

          _isInvalid = false;
          _needsRebuild = false;
        }
        finally
        {
          _updateInProgress = false;
        }
      }
    }


    private void ValidateItems(IEnumerable<T> items)
    {
      if (items == null)
        return;

      foreach (var item in items)
      {
        var aabb = GetAabbForItem(item);

        // Check for NaN.
        if (Numeric.IsNaN(aabb.Extent.X) || Numeric.IsNaN(aabb.Extent.Y) || Numeric.IsNaN(aabb.Extent.Z))
          throw new GeometryException("Cannot build spatial partition because the AABB of an item is NaN.");
      }
    }


    private void UpdateAabb()
    {
      int numberOfItems = Items.Count;
      if (numberOfItems == 0)
      {
        // AABB is undefined.
        Aabb = new Aabb();
      }
      else
      {
        // Create union of all items.
        var enumerator = Items.GetEnumerator();

        // Start with AABB of first item.
        enumerator.MoveNext();
        Aabb aabb = GetAabbForItem(enumerator.Current);

        // Grow AABB.
        while (enumerator.MoveNext())
          aabb.Grow(GetAabbForItem(enumerator.Current));

        Aabb = aabb;
      }
    }


    private void OnFilterChanged(object sender, EventArgs eventArgs)
    {
      Invalidate();
      _needsRebuild = true;
    }


    // Note: The OnXxx() methods are internal because they should not publicly visible!

    /// <summary>
    /// Called when the property <see cref="Filter"/> has changed. (Note: This method is not called
    /// when a filter raises the <see cref="IPairFilter{T}.Changed"/> event!)
    /// </summary>
    internal virtual void OnFilterChanged()
    {
      // This method was added because DualPartition<T> needs to propagate the change to its 
      // internal partitions.
    }


    /// <summary>
    /// Called when the property <see cref="EnableSelfOverlaps"/> has changed.
    /// </summary>
    internal virtual void OnEnableSelfOverlapsChanged()
    {
      // This method was added because DualPartition<T> needs to propagate the change to its 
      // internal partitions.
    }


    /// <summary>
    /// Called when the property <see cref="GetAabbForItem"/> has changed.
    /// </summary>
    internal virtual void OnGetAabbForItemChanged()
    {
      // This method was added because DualPartition<T> needs to propagate the change to its 
      // internal partitions.
    }


    /// <summary>
    /// Called when the items in the spatial partition have changed and spatial partition should be 
    /// updated.
    /// </summary>
    /// <param name="forceRebuild">
    /// If set to <see langword="true"/> the spatial partitioning should be rebuilt from scratch.
    /// </param>
    /// <param name="addedItems">
    /// The added items. (Guaranteed to be not <see langword="null"/>.)
    /// </param>
    /// <param name="removedItems">
    /// The removed items. (Guaranteed to be not <see langword="null"/>.)
    /// </param>
    /// <param name="invalidItems">
    /// The invalid items. This set is either.
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// an empty set to indicate that all items are valid,
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// a set containing exactly the invalid items that need to be updated,
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// or <see langword="null"/> to indicate that some items have changed, but it is unclear which.
    /// </description>
    /// </item>
    /// </list>
    /// </param>
    /// <remarks>
    /// <para>
    /// <strong>Preconditions:</strong>
    /// When this method is called following conditions are <see langword="true"/>.
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The <see cref="BasePartition{T}.Items"/> collection is up-to-date, which means for 
    /// example that the items in <paramref name="removedItems"/> are not in the
    /// <see cref="BasePartition{T}.Items"/> collection anymore.
    /// </description>
    /// </item>
    /// <item>
    /// <description>The <see cref="Aabb"/> is invalid!</description>
    /// </item>
    /// <item>
    /// <description>
    /// The sets <paramref name="addedItems"/>, <paramref name="removedItems"/> and 
    /// <paramref name="invalidItems"/> are disjoint and not <see langword="null"/>.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Required Actions:</strong>
    /// When <see cref="OnUpdate(bool,HashSet{T},HashSet{T},HashSet{T})"/> is called the derived 
    /// class must do following:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// The new <see cref="Aabb"/> for the whole partition must be computed. (If this is not done 
    /// the <see cref="BasePartition{T}"/> will compute the new <see cref="Aabb"/> 
    /// automatically, but this is slower.)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// If <see cref="BasePartition{T}.EnableSelfOverlaps"/> is <see langword="true"/>, the 
    /// <see cref="BasePartition{T}.SelfOverlaps"/> must be updated.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// Important: This method must not directly or indirectly call <see cref="Update(bool)"/>! This
    /// would lead to recursive calls.
    /// </para>
    /// </remarks>
    internal abstract void OnUpdate(bool forceRebuild, HashSet<T> addedItems, HashSet<T> removedItems, HashSet<T> invalidItems);


    /// <summary>
    /// Called when the spatial partition should be updated, but items have not changed.
    /// </summary>
    /// <remarks>
    /// This <strong>OnUpdate</strong>-overload is called when the spatial partition is updated
    /// (<see cref="Update(bool)"/> was called), but items haven't changed since the last update. 
    /// Derived classes can override this method to perform code which should be executed regularly, 
    /// independent of whether the items in the spatial partition have changed.
    /// </remarks>
    internal virtual void OnUpdate()
    {
      // This method was added because DualPartition<T> needs to perform certain cleanups
      // even if the items have not changed.
    }


    //protected virtual void OnOverlapsChanged(IList<Pair<T,T>> newOverlaps, IList<Pair<T,T>> oldOverlaps)
    //{
    //  var handler = OverlapsChanged;

    //  if (handler != null)
    //  {
    //    if (_eventArgs == null)
    //      _eventArgs = new OverlapEventArgs<T>();

    //    _eventArgs.NewOverlaps = newOverlaps;
    //    _eventArgs.OldOverlaps = oldOverlaps;

    //    handler(this, _eventArgs);
    //  }
    //}
    #endregion
  }
}
