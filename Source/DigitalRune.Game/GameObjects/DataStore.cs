// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;


namespace DigitalRune.Game
{
  /// <summary>
  /// Stores data that can be accessed using an ID or index.
  /// </summary>
  /// <typeparam name="T">The type of the data items.</typeparam>
  /// <remarks>
  /// <para>
  /// Data in the <see cref="DataStore{T}"/> can be be accessed using an ID or index.
  /// <strong>ID:</strong>
  /// Each data item must have a unique ID. The ID is required to read/write data items. Storing a
  /// data item with a certain ID will add a new entry to the data store - or overwrite an existing 
  /// entry, if an entry with for given ID already exits. Data items are sorted ascending based on 
  /// their ID for fast retrieval. Accessing data items based on their ID is O(log <i>n</i>).
  /// </para>
  /// <para>
  /// <strong>Index:</strong>
  /// Data items can also be read based on their index in the data store. However, this approach is
  /// not recommended as the index will change when data items are added or removed. Reading data 
  /// based on their index is O(<i>1</i>).
  /// </para>
  /// </remarks>
  public class DataStore<T>
  {
    // This data store uses two arrays: one for IDs and one for items.
    // The arrays are sorted by the IDs. The array does not contain any gaps, which means that
    // insert/remove operations can cause large parts of the array to be copied.
    // Retrieval of items is done using binary search.


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    private const int DefaultCapacity = 4;

    // ReSharper disable StaticFieldInGenericType

    // Empty arrays that will be used when Capacity is set to 0.
    private static readonly int[] EmptyIds = new int[0];
    private static readonly T[] EmptyData = new T[0];
    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int[] _ids;   // = Keys
    private T[] _data;    // = Values
    private int _count;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    private int Capacity
    {
      get { return _ids.Length; }
      set
      {
        Debug.Assert(value >= _count, "Cannot set Capacity to a value less than Count.");

        if (value != _ids.Length)
        {
          if (value > 0)
          {
            // New capacity > 0. Allocate new array and copy data.

            int[] newIds = new int[value];
            T[] newData = new T[value];
            if (_count > 0)
            {
              Array.Copy(_ids, 0, newIds, 0, _count);
              Array.Copy(_data, 0, newData, 0, _count);
            }

            _ids = newIds;
            _data = newData;
          }
          else
          {
            // New capacity <= 0.

            _ids = EmptyIds;
            _data = EmptyData;
          }
        }
      }
    }


    /// <summary>
    /// Gets the number of items in the data store.
    /// </summary>
    /// <value>The number of items in the data store.</value>
    public int Count
    {
      get { return _count; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStore{T}"/> class.
    /// </summary>
    public DataStore()
    {
      _ids = EmptyIds;
      _data = EmptyData;
      _count = 0;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Adds the given item to the data store.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <param name="data">The data.</param>
    public void Set(int id, T data)
    {
      int index = IndexOfId(id);
      if (index >= 0)
        _data[index] = data;
      else
        Insert(~index, id, data);
    }


    /// <summary>
    /// Removes all items from the data store.
    /// </summary>
    public void Clear()
    {
      Array.Clear(_ids, 0, _count);
      Array.Clear(_data, 0, _count);
      _count = 0;
    }


    private void EnsureCapacity(int min)
    {
      // The capacity of the array grows from 0 to DefaultCapacity, and then it is always
      // doubled.
      int newCapacity = (_ids.Length == 0) ? DefaultCapacity : (_ids.Length * 2);
      if (newCapacity < min)
        newCapacity = min;

      // Set new capacity. The property setter grows the arrays when needed.
      Capacity = newCapacity;
    }


    /// <summary>
    /// Gets the item for the given ID.
    /// </summary>
    /// <param name="id">The ID.</param>
    /// <returns>
    /// The data with the given ID, or the default value of <typeparamref name="T"/> if no 
    /// data for the given <paramref name="id"/> is in the data store.
    /// </returns>
    public T Get(int id)
    {
      int index = IndexOfId(id);
      if (index >= 0)
        return _data[index];

      return default(T);
    }


    /// <summary>
    /// Gets the item at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the data.</param>
    /// <returns>
    /// The data that is stored at the given index. If the index is out of range, the default value
    /// of <typeparamref name="T"/> is returned.
    /// </returns>
    public T GetByIndex(int index)
    {
      if (0 <= index && index < _count)
        return _data[index];

      return default(T);
    }


    /// <summary>
    /// Gets the ID of the item at the given index.
    /// </summary>
    /// <param name="index">The index of the item.</param>
    /// <returns>The ID of the data stored at the given index.</returns>
    public int GetIdByIndex(int index)
    {
      return _ids[index];
    }


    /// <summary>
    /// Searches for the index of the item with the given ID.
    /// </summary>
    /// <param name="id">The ID to search for.</param>
    /// <returns>
    /// <para>
    /// The index of the specified item, if <paramref name="id"/> is found. 
    /// </para>
    /// <para>
    /// If <paramref name="id"/> is not found and <paramref name="id"/> is less than one or more IDs
    /// in the collection, a negative number which is the bitwise complement of the index of the
    /// first ID that is larger. 
    /// </para>
    /// <para>
    /// If <paramref name="id"/> is not found and <paramref name="id"/> is greater than any of the 
    /// IDs in the collection, a negative number which is the bitwise complement of (the index of 
    /// the last ID plus 1).
    /// </para>
    /// </returns>
    public int IndexOfId(int id)
    {
      //return Array.BinarySearch(_ids, 0, _count, id);

      // ----- Binary Search (inlined for performance)
      int start = 0;
      int end = _count - 1;
      while (start <= end)
      {
        int index = start + (end - start >> 1);
        int comparison = _ids[index] - id;
        if (comparison == 0)
        {
          return index;
        }

        if (comparison < 0)
        {
          Debug.Assert(id > _ids[index]);
          start = index + 1;
        }
        else
        {
          Debug.Assert(id < _ids[index]);
          end = index - 1;
        }
      }

      return ~start;
    }


    /// <summary>
    /// Gets the index of the given item.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <returns>
    /// The zero-based index of the given item; or -1 if the data is not in the data store.
    /// </returns>
    public int IndexOf(T data)
    {
      return Array.IndexOf(_data, data, 0, _count);
    }


    private void Insert(int index, int id, T data)
    {
      if (_count == _ids.Length)
      {
        // The array is full and must grow.
        EnsureCapacity(_count + 1);
      }

      if (index < _count)
      {
        Array.Copy(_ids, index, _ids, index + 1, _count - index);
        Array.Copy(_data, index, _data, index + 1, _count - index);
      }

      _ids[index] = id;
      _data[index] = data;
      _count++;
    }


    /// <summary>
    /// Removes data with the given ID.
    /// </summary>
    /// <param name="id">The ID of the data that should be removed.</param>
    /// <returns>
    /// <see langword="true"/> if the data was removed; <see langword="false"/> if the data was not 
    /// stored in the data store.
    /// </returns>
    public bool Remove(int id)
    {
      int index = IndexOfId(id);
      if (index >= 0)
        RemoveAt(index);

      return (index >= 0);
    }


    /// <summary>
    /// Removes the item at the given index.
    /// </summary>
    /// <param name="index">The index of the data that should be removed.</param>
    public void RemoveAt(int index)
    {
      if (0 <= index && index < _count)
      {
        _count--;

        if (index < _count)
        {
          Array.Copy(_ids, index + 1, _ids, index, _count - index);
          Array.Copy(_data, index + 1, _data, index, _count - index);
        }

        _ids[_count] = 0;
        _data[_count] = default(T);
      }
    }


    //public void TrimExcess()
    //{
    //  int num = (int)(_ids.Length * 0.9);
    //  if (_count < num)
    //  {
    //    Capacity = _count;
    //  }
    //}

    #endregion
  }
}
