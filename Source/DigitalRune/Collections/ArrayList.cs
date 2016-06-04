// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Represents array-based list with minimal overhead.
  /// </summary>
  /// <typeparam name="T">The value type stored in the array.</typeparam>
  /// <remarks>
  /// <para>
  /// The list is intended to store value types (<c>struct</c>). The list's internal array is 
  /// exposed for fast, direct access. The internal array should only be used for read or replace 
  /// operations. Items can be added using <see cref="Add(ref T)"/>, which may resize the internal 
  /// array.
  /// </para>
  /// <para>
  /// <strong>Important:</strong> Only the first <see cref="Count"/> array entries are valid!
  /// </para>
  /// </remarks>
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(ArrayList<>.ArrayListCollectionView))]
  internal class ArrayList<T> : IList<T> where T : struct
  {
    // Note about sorting performance:
    // The sorting performance decreases if T is a large struct because T needs 
    // to be copied for each comparison  and swapped during sorting.
    // The alternative is to use an additional index array and only sort indices
    // using an IComparer<int> instead of an IComparer<T>.
    // The thresholds are
    //   x86: sizeof(T) ≥ 24 bytes
    //   x64: sizeof(T) ≥ 40 bytes
    //   ARM (WP7): sizeof(T) ≥ 20-24 bytes
    // Currently all T are below the threshold, so sorting performance should be
    // fine.
    // 
    // Additional observation: Running the performance test on x64 is ~50% faster 
    // than on x86, which is surprising because T is significantly bigger (references 
    // are 8 instead of 4 bytes)!


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    // This view is used as DebuggerTypeProxy.
    internal sealed class ArrayListCollectionView
    {
      private readonly ArrayList<T> _list;
      public ArrayListCollectionView(ArrayList<T> list)
      {
        _list = list;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public T[] Items
      {
        get { return _list.Array.Take(_list.Count).ToArray(); }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private T[] _items;
    private int _count;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the list's internal array.
    /// </summary>
    /// <value>The list's internal array.</value>
    public T[] Array
    {
      get { return _items; }
    }


    /// <summary>
    /// Gets the number of items in the list.
    /// </summary>
    /// <value>The number of items in the list.</value>
    public int Count
    {
      get { return _count; }
    }


    bool ICollection<T>.IsReadOnly
    {
      get { return false; }
    }


    T IList<T>.this[int index]
    {
      get { return _items[index]; }
      set { _items[index] = value; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayList{T}" /> class.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity.</param>
    public ArrayList(int initialCapacity)
    {
      _items = new T[initialCapacity];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all items from the list.
    /// </summary>
    public void Clear()
    {
      if (_count > 0)
      {
        System.Array.Clear(_items, 0, _count);
        _count = 0;
      }
    }


    /// <overrides>
    /// <summary>
    /// Adds the specified item to the list.
    /// </summary>
    /// </overrides>
    /// <summary>
    /// Adds the specified item to the list.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public void Add(ref T item)
    {
      if (_count == _items.Length)
      {
        int capacity = _items.Length * 2;
        Resize(capacity);
      }

      _items[_count] = item;
      _count++;
    }


    /// <summary>
    /// Adds the specified item to the list.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public void Add(T item)
    {
      if (_count == _items.Length)
      {
        int capacity = _items.Length * 2;
        Resize(capacity);
      }

      _items[_count] = item;
      _count++;
    }


    /// <summary>
    /// Adds the specified number of (uninitialized!) items to the list.
    /// </summary>
    /// <param name="count">The number of items.</param>
    public void AddRange(int count)
    {
      count += _count;
      if (count > _items.Length)
      {
        int capacity = _items.Length * 2;
        while (capacity < count)
          capacity *= 2;

        Resize(capacity);
      }

      _count = count;
    }


    /// <summary>
    /// Adds the specified items to the list.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public void AddRange(ICollection<T> items)
    {
      if (items != null)
      {
        int startIndex = _count;
        _count += items.Count;
        EnsureCapacity(_count);
        items.CopyTo(_items, startIndex);
      }
    }


    /// <summary>
    /// Ensures the list's internal array is large enough to store a certain number of items.
    /// </summary>
    /// <param name="capacity">The number of items to be stored in the list.</param>
    public void EnsureCapacity(int capacity)
    {
      if (_items.Length < capacity)
        Resize(capacity);
    }


    /// <overloads>
    /// <summary>
    /// Inserts an item to the list at the specified index.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item" /> should be inserted.
    /// </param>
    /// <param name="item">The item to be inserted.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index.
    /// </exception>
    public void Insert(int index, ref T item)
    {
      if ((uint)index > (uint)_count)
        throw new ArgumentOutOfRangeException("index");

      if (_count == _items.Length)
      {
        int capacity = _items.Length * 2;
        Resize(capacity);
      }

      if (index < _count)
        System.Array.Copy(_items, index, _items, index + 1, _count - index);

      _items[index] = item;
      _count++;
    }


    /// <summary>
    /// Inserts an item to the list at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item" /> should be inserted.
    /// </param>
    /// <param name="item">The item to be inserted.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index.
    /// </exception>
    public void Insert(int index, T item)
    {
      if ((uint)index > (uint)_count)
        throw new ArgumentOutOfRangeException("index");

      if (_count == _items.Length)
      {
        int capacity = _items.Length * 2;
        Resize(capacity);
      }

      if (index < _count)
        System.Array.Copy(_items, index, _items, index + 1, _count - index);

      _items[index] = item;
      _count++;
    }


    /// <summary>
    /// Resizes the list's internal array.
    /// </summary>
    /// <param name="capacity">The array length.</param>
    private void Resize(int capacity)
    {
      T[] newArray = new T[capacity];
      System.Array.Copy(_items, newArray, Math.Min(_count, capacity));
      _items = newArray;
    }


    /// <summary>
    /// Copies the elements of the <see cref="ArrayList{T}"/> to an <see cref="System.Array"/>, 
    /// starting at a particular <see cref="System.Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements 
    /// copied from <see cref="ArrayList{T}"/>. The <see cref="System.Array"/> must have zero-based 
    /// indexing.
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
    /// source <see cref="ArrayList{T}"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
      System.Array.Copy(_items, 0, array, arrayIndex, _count);
    }


    /// <summary>
    /// Removes the item at the specified index from the list.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index.
    /// </exception>
    public void RemoveAt(int index)
    {
      if ((uint)index >= (uint)_count)
        throw new ArgumentOutOfRangeException("index");

      _count--;
      if (index < _count)
        System.Array.Copy(_items, index + 1, _items, index, _count - index);

      _items[_count] = default(T);
    }


    /// <summary>
    /// Sorts the items in the list.
    /// </summary>
    /// <param name="comparer">The <see cref="IComparer{T}"/> that defines the sort order.</param>
    public void Sort(IComparer<T> comparer)
    {
      System.Array.Sort(_items, 0, _count, comparer);
    }
    #endregion


    //--------------------------------------------------------------
    #region Interface IList<T>
    //--------------------------------------------------------------

    // Not all methods are used. (Add methods when needed.)

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      throw new NotImplementedException();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }


    bool ICollection<T>.Contains(T item)
    {
      throw new NotImplementedException();
    }


    int IList<T>.IndexOf(T item)
    {
      throw new NotImplementedException();
    }


    bool ICollection<T>.Remove(T item)
    {
      throw new NotImplementedException();
    }
    #endregion
  }
}
