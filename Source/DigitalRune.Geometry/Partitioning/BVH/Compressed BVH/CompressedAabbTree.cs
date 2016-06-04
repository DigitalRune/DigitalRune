// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/* 
   The CompressedAabbTree is based on the quantized/optimized bounding volume hierarchy of Bullet.
   (Note: Our CompressedAabbTree and the original version of Bullet have only the general algorithm
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
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Geometry.Shapes;
using DigitalRune.Linq;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
#if !PORTABLE
using System.ComponentModel;
#endif
#if PORTABLE || WINDOWS
using System.Dynamic;
#endif


namespace DigitalRune.Geometry.Partitioning
{
  /// <summary>
  /// Represents a compressed and optimized bounding volume tree using axis-aligned bounding boxes 
  /// (AABBs).
  /// </summary>
  /// <remarks>
  /// <para>
  /// The <see cref="CompressedAabbTree"/> is a specialized version of an <see cref="AabbTree{T}"/>
  /// for items of type <see cref="int"/>. It requires significantly less memory than an 
  /// <see cref="AabbTree{T}"/>, but building or updating ("refitting") a
  /// <see cref="CompressedAabbTree"/> is more expensive. It should be used for partitioning static
  /// <see cref="CompositeShape"/>s or <see cref="TriangleMeshShape"/>s that consist of many shapes
  /// or triangles.
  /// </para>
  /// <para>
  /// The <see cref="CompressedAabbTree"/> can store up to 2<sup>32-1</sup> data values of type
  /// <see cref="int"/> (range 0 - 2,147,483,647).
  /// </para>
  /// <para>
  /// <strong>Limitations:</strong> Objects organized by the <see cref="CompressedAabbTree"/> need 
  /// to have finite size. The <see cref="CompressedAabbTree"/> cannot be used for extremely large, 
  /// or infinitely large objects. For example: A <see cref="CompositeShape"/> using a 
  /// <see cref="CompressedAabbTree"/> must not contain an <see cref="InfiniteShape"/>, a 
  /// <see cref="LineShape"/>, or a <see cref="PlaneShape"/>.
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public sealed partial class CompressedAabbTree : ISpatialPartition<int>, ISupportClosestPointQueries<int>
  {
    // TODO: We could optimize the compressed AABB tree regarding cache.
    // Cache-aware for PS3: see Bullet, btQuantizedBvh/btOptimizedBvh
    // Cache-oblivious implementation: see Ericson, pp. 530 and Game Programming Gems 5, pp. 159


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    [Flags]
    internal enum State
    {
      IsUpToDate = 0,           // Nothing to do in Update(). Note: IsUpToDate does not mean that the tree is valid.
      UpdateSelfOverlaps = 1,   // Need to recompute the self-overlaps.
      Invalid = 3,              // Tree needs to be rebuilt or refit.
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    /// <summary>
    /// A margin which is added to the tree's AABB to avoid divisions by zero.
    /// </summary>
    private static readonly float AabbMargin = Numeric.EpsilonF;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Note: When the items are added or removed the tree becomes invalid.
    // All items are copied into plain List<int>. In Update() the tree is rebuilt.

    internal volatile State _state; // volatile because it is possibly accessed from several threads in Update!
    internal int _numberOfItems;    // The number of items. (Only set if the tree is valid.)
    internal List<int> _items;      // The items list. (Only set if the tree is invalid.)
    internal Node[] _nodes;         // All internal nodes and leaf nodes. (Only set if the tree is valid.)

    private HashSet<Pair<int>> _selfOverlaps;

    internal Aabb _aabb;
    internal Vector3F _quantizationFactor;
    internal Vector3F _dequantizationFactor;

    // Synchronization object for Update().
    private readonly object _syncRoot = new object();
    private bool _updateInProgress;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public Aabb Aabb
    {
      get
      {
        Update(false);   // Make sure we are up-to-date.
        return _aabb;
      }
    }


    /// <summary>
    /// Gets the number of items contained in the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <value>The number of items contained in the <see cref="ICollection{T}"/>.</value>
    public int Count
    {
      get { return (_items != null) ? _items.Count : _numberOfItems; }
    }


    /// <inheritdoc/>
    public bool EnableSelfOverlaps
    {
      get { return _selfOverlaps != null; }
      set
      {
        if (value)
        {
          // Enable self-overlaps, if not already enabled.
          if (_selfOverlaps == null)
          {
            _selfOverlaps = new HashSet<Pair<int>>();
            _state |= State.UpdateSelfOverlaps;
          }
        }
        else
        {
          // Disable self-overlaps.
          _selfOverlaps = null;
        }
      }
    }


    /// <inheritdoc/>
    public IPairFilter<int> Filter
    {
      get { return _filter; }
      set
      {
        if (_filter != value)
        {
          if (_filter != null)
            _filter.Changed -= OnFilterChanged;

          _filter = value;
          OnFilterChanged(this, EventArgs.Empty); // Call OnFilterChanged manually.

          if (_filter != null)
            _filter.Changed += OnFilterChanged;
        }
      }
    }
    internal IPairFilter<int> _filter;


    /// <inheritdoc/>
    public Func<int, Aabb> GetAabbForItem
    {
      get { return _getAabbForItem; }
      set { _getAabbForItem = value; }
    }
    private Func<int, Aabb> _getAabbForItem;


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; 
    /// otherwise, <see langword="false"/>.
    /// </value>
    bool ICollection<int>.IsReadOnly { get { return false; } }


#if PORTABLE || WINDOWS
    /// <exclude/>
#if !PORTABLE
    [Browsable(false)]
#endif
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public /*dynamic*/ object Internals
    {
      // Make internals visible to assemblies that cannot be added with InternalsVisibleTo().
      get
      {
        int[] data = new int[_nodes.Length * 7];
        for (int i = 0; i < _nodes.Length; i++)
        {
          data[i * 7 + 0] = _nodes[i].MinimumX;
          data[i * 7 + 1] = _nodes[i].MinimumY;
          data[i * 7 + 2] = _nodes[i].MinimumZ;
          data[i * 7 + 3] = _nodes[i].MaximumX;
          data[i * 7 + 4] = _nodes[i].MaximumY;
          data[i * 7 + 5] = _nodes[i].MaximumZ;
          data[i * 7 + 6] = _nodes[i].EscapeOffsetOrItem;
        }

        // ----- PCL Profile136 does not support dynamic.
        //dynamic internals = new ExpandoObject();
        //internals.State = (int)_state;
        //internals.NumberOfItems = _numberOfItems;
        //internals.Items = _items;
        //internals.NumberOfNodes = _nodes.Length;
        //internals.Data = data;
        //internals.QuantizationFactor = _quantizationFactor;
        //internals.DequantizationFactor = _dequantizationFactor;
        //return internals;

        IDictionary<string, Object> internals = new ExpandoObject();
        internals["State"] = (int)_state;
        internals["NumberOfItems"] = _numberOfItems;
        internals["Items"] = _items;
        internals["NumberOfNodes"] = _nodes.Length;
        internals["Data"] = data;
        internals["QuantizationFactor"] = _quantizationFactor;
        internals["DequantizationFactor"] = _dequantizationFactor;
        return internals;
      }
    }
#endif
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="CompressedAabbTree"/> class.
    /// </summary>
    public CompressedAabbTree()
    {
      _items = DigitalRune.ResourcePools<int>.Lists.Obtain();

      // Compressed AABB tree is initially up-to-date. (Nothing to do in Update().)
      _state = State.IsUpToDate;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    #region ----- Cloning -----

    /// <inheritdoc/>
    ISpatialPartition<int> ISpatialPartition<int>.Clone()
    {
      return Clone();
    }


    /// <summary>
    /// Creates a new <see cref="CompressedAabbTree"/> that is a clone (deep copy) of the current
    /// instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="CompressedAabbTree"/> that is a clone (deep copy) of the current instance.
    /// </returns>
    public CompressedAabbTree Clone()
    {
      return new CompressedAabbTree
      {
        EnableSelfOverlaps = EnableSelfOverlaps,
        Filter = Filter,
        GetAabbForItem = GetAabbForItem,
      };
    }
    #endregion


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
    public IEnumerator<int> GetEnumerator()
    {
      if (_items != null)
      {
        // The items are stored in a plain list.
        return _items.GetEnumerator();
      }
      else if (_nodes != null)
      {
        // The items are stored in the compressed tree.
        return _nodes.Where(node => node.IsLeaf)
                     .Select(node => node.Item)
                     .GetEnumerator();
      }
      else
      {
        // The tree is empty.
        return LinqHelper.Empty<int>().GetEnumerator();
      }
    }


    /// <summary>
    /// Adds an item to the <see cref="CompressedAabbTree"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="CompressedAabbTree"/>.</param>
    /// <remarks>
    /// Duplicate items or <see langword="null"/> are not allowed in the 
    /// <see cref="CompressedAabbTree"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    public void Add(int item)
    {
      InvalidateTree();
      _items.Add(item);
    }


    /// <summary>
    /// Removes all items from the <see cref="CompressedAabbTree"/>.
    /// </summary>
    public void Clear()
    {
      if (_items != null)
      {
        _items.Clear();
      }
      else
      {
        _items = DigitalRune.ResourcePools<int>.Lists.Obtain();
        _nodes = null;
        _numberOfItems = 0;

        if (_selfOverlaps != null)
          _selfOverlaps.Clear();
      }

      _aabb = new Aabb();
      _state = State.IsUpToDate;  // Nothing to do in Update().
    }


    /// <summary>
    /// Determines whether the <see cref="CompressedAabbTree"/> contains a specific item.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="CompressedAabbTree"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="CompressedAabbTree"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(int item)
    {
      if (_items != null)
      {
        return _items.Contains(item);
      }
      else
      {
        foreach (var node in _nodes)
        {
          if (node.IsLeaf && node.Item == item)
            return true;
        }

        return false;
      }
    }


    /// <summary>
    /// Copies the elements of the tree to an <see cref="Array"/>, starting at a particular 
    /// <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="CompressedAabbTree"/>. The <see cref="Array"/> must have zero-based indexing.
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
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="CompressedAabbTree"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="CompressedAabbTree"/> is not modified. The order of the elements in the new 
    /// array is the same as the order of the elements in the tree in depth-first order.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is the number of nodes in the tree!
    /// </para>
    /// </remarks>
    public void CopyTo(int[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");

      if (_items != null)
      {
        _items.CopyTo(array, arrayIndex);
      }
      else
      {
        if (arrayIndex < 0)
          throw new ArgumentOutOfRangeException("arrayIndex");
        if (arrayIndex + _numberOfItems > array.Length)
          throw new ArgumentException("The number of elements is greater than the available space from arrayIndex to the end of the destination array.");

        foreach (var node in _nodes)
        {
          if (node.IsLeaf)
          {
            array[arrayIndex] = node.Item;
            arrayIndex++;
          }
        }
      }
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
    private bool FilterSelfOverlap(Pair<int> pair)
    {
      // Check for equality.
      if (pair.First == pair.Second)
        return false;

      // Check Filter.
      return Filter == null || Filter.Filter(pair);
    }


    /// <summary>
    /// Removes the first occurrence of a specific item from the <see cref="CompressedAabbTree"/>.
    /// </summary>
    /// <param name="item">
    /// The object to remove from the <see cref="CompressedAabbTree"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="CompressedAabbTree"/>; otherwise, <see langword="false"/>. This method also 
    /// returns <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="CompressedAabbTree"/>.
    /// </returns>
    public bool Remove(int item)
    {
      InvalidateTree();
      return _items.Remove(item);
    }


    /// <inheritdoc/>
    public void Invalidate()
    {
      _state = State.Invalid;
    }


    /// <inheritdoc/>
    public void Invalidate(int item)
    {
      // The compressed AABB does not keep track of individual items.
      // Invalidate the entire tree.
      _state = State.Invalid;
    }


    /// <summary>
    /// Invalidates the tree. (Converts the internal tree to a plain list.)
    /// </summary>
    private void InvalidateTree()
    {
      _state = State.Invalid;

      if (_items != null)
      {
        // The items are already stored in a list.
        return;
      }

      // Move items from tree to plain List<int>.
      _items = DigitalRune.ResourcePools<int>.Lists.Obtain();
      foreach (var node in _nodes)
      {
        if (node.IsLeaf)
          _items.Add(node.Item);
      }

      Debug.Assert(_items.Count == _numberOfItems);

      _nodes = null;
      _numberOfItems = 0;

      if (_selfOverlaps != null)
        _selfOverlaps.Clear();
    }


    internal void OnFilterChanged(object sender, EventArgs eventArgs)
    {
      // Recompute self-overlaps in Update().
      _state |= State.UpdateSelfOverlaps;
    }


    /// <inheritdoc/>
    public void Update(bool forceRebuild)
    {
      // Important: This method must not be called recursively!!!

      if (_state == State.IsUpToDate)
        return;  // Early out.

      if (forceRebuild)
        InvalidateTree();

      lock (_syncRoot)
      {
        // Check state again after we have acquired the lock.
        if (_state == State.IsUpToDate)
          return;

        if (_updateInProgress)
          throw new InvalidOperationException("Recursive call of Update() detected. Update() must not be called recursively!!!");

        _updateInProgress = true;

        if (_state == State.Invalid)
        {
          // Compressed AABB tree requires rebuild or refit.
          if (_items != null && _items.Count > 0)
          {
            // Build compressed AABB tree.
            Build();
            UpdateSelfOverlaps();
          }
          else if (_numberOfItems > 0)
          {
            // Refit compressed AABB tree.
            Refit();
            UpdateSelfOverlaps();
          }
        }
        else
        {
          // Update self-overlaps.
          Debug.Assert(_state == State.UpdateSelfOverlaps);
          UpdateSelfOverlaps();
        }

        _state = State.IsUpToDate;
        _updateInProgress = false;
      }
    }


    /// <summary>
    /// Sets the reference AABB and prepares the factors for quantization.
    /// </summary>
    /// <param name="aabb">The AABB of the spatial partition.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    private void SetQuantizationValues(Aabb aabb)
    {
      _aabb = aabb;

      // ----- Compute quantization factor.
      // Add a margin to the AABB to avoid divisions by zero.
      Vector3F margin = new Vector3F(AabbMargin);
      _aabb.Minimum = aabb.Minimum - margin;
      _aabb.Maximum = aabb.Maximum + margin;

      Vector3F extent = _aabb.Extent;
      if (Numeric.IsNaN(extent.X) || Numeric.IsNaN(extent.Y) || Numeric.IsNaN(extent.Z))
        throw new GeometryException("Cannot build CompressedAabbTree. The AABB of some items contains NaN.");
      if (float.IsInfinity(extent.X) || float.IsInfinity(extent.Y) || float.IsInfinity(extent.Z))
        throw new GeometryException("Cannot build CompressedAabbTree. All objects need to have finite size. The CompressedAabbTree cannot be used for infinitely large objects.");

      // Cache the quantization factors.
      // The max value used for quantization is ushort.MaxValue minus 2: In order to calculate 
      // a conservative AABB we need to round the quantized min values down and the max values 
      // up.
      _quantizationFactor = new Vector3F(65533) / extent;
      _dequantizationFactor = Vector3F.One / _quantizationFactor;
    }


    /// <summary>
    /// Gets the AABB using dequantization.
    /// </summary>
    /// <param name="node">The compressed AABB node.</param>
    /// <returns>The dequantized AABB.</returns>
    private Aabb GetAabb(Node node)
    {
      Aabb reference = _aabb;
      Aabb aabb;
      aabb.Minimum.X = reference.Minimum.X + node.MinimumX * _dequantizationFactor.X;
      aabb.Minimum.Y = reference.Minimum.Y + node.MinimumY * _dequantizationFactor.Y;
      aabb.Minimum.Z = reference.Minimum.Z + node.MinimumZ * _dequantizationFactor.Z;
      aabb.Maximum.X = reference.Minimum.X + node.MaximumX * _dequantizationFactor.X;
      aabb.Maximum.Y = reference.Minimum.Y + node.MaximumY * _dequantizationFactor.Y;
      aabb.Maximum.Z = reference.Minimum.Z + node.MaximumZ * _dequantizationFactor.Z;
      return aabb;
    }


    /// <summary>
    /// Sets the AABB using quantization.
    /// </summary>
    /// <param name="node">The compressed AABB node.</param>
    /// <param name="aabb">The AABB to be quantized.</param>
    private void SetAabb(ref Node node, Aabb aabb)
    {
      Debug.Assert(
        aabb.Minimum >= _aabb.Minimum
        && aabb.Minimum <= _aabb.Maximum
        && aabb.Maximum >= _aabb.Minimum
        && aabb.Maximum <= _aabb.Maximum,
        "Child node has invalid AABB. Child AABB must be contained in root AABB.");

      Vector3F quantizedMinimum = (aabb.Minimum - _aabb.Minimum) * _quantizationFactor;
      Vector3F quantizedMaximum = (aabb.Maximum - _aabb.Minimum) * _quantizationFactor;

      // Convert quantized minimum to ushort. (Subtract 1 to ensure that AABB is conservative.)
      node.MinimumX = (quantizedMinimum.X > 1.0f) ? (ushort)(quantizedMinimum.X - 1.0f) : (ushort)0;
      node.MinimumY = (quantizedMinimum.Y > 1.0f) ? (ushort)(quantizedMinimum.Y - 1.0f) : (ushort)0;
      node.MinimumZ = (quantizedMinimum.Z > 1.0f) ? (ushort)(quantizedMinimum.Z - 1.0f) : (ushort)0;

      // Convert quantized maximum to ushort. (Add 1 to ensure that AABB is conservative.)
      node.MaximumX = (quantizedMaximum.X < (ushort.MaxValue - 1)) ? (ushort)(quantizedMaximum.X + 1.0f) : ushort.MaxValue;
      node.MaximumY = (quantizedMaximum.Y < (ushort.MaxValue - 1)) ? (ushort)(quantizedMaximum.Y + 1.0f) : ushort.MaxValue;
      node.MaximumZ = (quantizedMaximum.Z < (ushort.MaxValue - 1)) ? (ushort)(quantizedMaximum.Z + 1.0f) : ushort.MaxValue;

      // Check whether quantized AABB is conservative.
      // ! This assert can fail. The numerical error is tiny.
      //Debug.Assert(GetAabb(node).Contains(aabb), String.Format("Quantization failed: Quantized AABB must be bigger than or equal to original AABB. Quantized: {0} Original: {1}", GetAabb(node), aabb));

      // Note: Our tests have shown that Bullet's approach is non-conservative in certain cases. Our
      // approach is conservative in all of our test cases. However, when we were using Bullet's 
      // method all test cases returned the correct results - even when the AABBs were non-conservative.
      // Being conservative guarantees that no overlaps are missed, but returns more overlap pairs
      // (false positives).
    }


    private void UpdateSelfOverlaps()
    {
      if (!EnableSelfOverlaps)
        return;

      _selfOverlaps.Clear();

      // Abort if tree is empty.
      if (_nodes == null)
        return;

      // Check leaves against AABB tree.
      foreach (Node node in _nodes)
      {
        if (node.IsLeaf)
        {
          Aabb aabb = GetAabb(node);

          // Important: Do not call GetOverlaps(this) because this would lead to recursive
          // Update() calls!
          foreach (var touchedItem in GetOverlapsImpl(aabb))
          {
            var overlap = new Pair<int>(node.Item, touchedItem);

            if (FilterSelfOverlap(overlap))
              _selfOverlaps.Add(overlap);
          }
        }
      }
    }
    #endregion
  }
}
