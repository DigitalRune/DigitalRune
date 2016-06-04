#region ----- Copyright -----
// The HashSet implementation is missing in the .NET Compact Framework. This
// port is based on the Mono implementation, which is licensed under the MIT 
// license. 
// 
// See https://github.com/mono/mono/blob/master/mcs/class/System.Core/System.Collections.Generic/HashSet.cs
//
//   Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//  
//   Permission is hereby granted, free of charge, to any person obtaining
//   a copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be
//   included in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//   EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//   NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//   LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//   OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//   WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//
// The following changes have been made [MartinG]:
// - Reimplemented the class step by step, mainly for learning purposes. Using 
//   DigitalRune coding style and naming conventions.
// - Items and links are stored in a single array instead of separate arrays. 
//   (Should be more cache-friendly.)
// - No load factor is applied to the array size. (We double the capacity and 
//   use the next prime when the set is full. The performance of the set only 
//   degrades when the set is more than 75% full at runtime. But this should not 
//   be the norm.)
// - Storing a negative hash code to indicate that a slot is unused instead of 
//   0. (No workaround required to store a hash code = 0!)
// - Resize(): Instead of going through the hash table and randomly accessing 
//   the slots, go through the slots continuously.
// - Additional comments added.
// - Bugfix: CopyTo() does not correctly count elements.
// - Bugfix: TrimExcess() in the Mono implementation increases (doubles) the 
//   capacity.
// - Bugfix: Some set operations throw InvalidOperationException during 
//   enumeration when tested against itself.
// - Using struct enumerator in set operations to avoid unnecessary memory 
//   allocation.
// - Some code inlined in set operations to improve performance.
#endregion

#if WP7 || XBOX
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

#if !SILVERLIGHT && !WP7 && !XBOX
using System.Runtime.Serialization;
#endif


namespace DigitalRune.Collections
{

  /// <summary>
  /// Represents a set of values. 
  /// </summary>
  /// <typeparam name="T">The type of the elements in this set.</typeparam>
  /// <remarks>
  /// <see cref="HashSetEx{T}"/> is a custom implementation of <see cref="HashSet{T}"/>, which is 
  /// missing on Windows Phone 7 and Xbox 360. This implementation tries to avoid any unnecessary 
  /// heap memory allocations. Set operations (<see cref="IntersectWith"/>, <see cref="UnionWith"/>, 
  /// etc.) between two <see cref="HashSetEx{T}"/> objects are fast; set operations between a 
  /// <see cref="HashSetEx{T}"/> and other types of sets are slower and may require additional
  /// memory allocations.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  [ComVisible(false)]
#if !SILVERLIGHT && !WP7 && !XBOX
  [Serializable]
#endif
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
  public class HashSetEx<T> :
#if !SILVERLIGHT && !WP7 && !XBOX
    ISet<T>, ISerializable, IDeserializationCallback
#else
    ICollection<T>
#endif
  {
    // The implementation of this class uses a hash table with chaining:
    // A hash table stores the index of the first slot in the chain.
    // Cleared slots are prepended to a "free list".

    // References:
    // http://en.wikipedia.org/wiki/Hash_table
    // http://msdn.microsoft.com/en-us/library/ms379571(VS.80).aspx
    // https://github.com/mono/mono/blob/master/mcs/class/System.Core/System.Collections.Generic/HashSet.cs


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private struct Slot
    {
      // The hash code of the item (0 or a positive value). 
      // If the slot has been freed, then hash code is set to -1.
      public int HashCode;

      // If the slot is used, the index of the next slot in the chain.
      // If the slot has been freed, the index of the next slot in the free list.
      // -1 indicates the end of the chain/free list.
      public int Next;

      // The item stored in the slot.
      public T Item;
    }


    /// <summary>
    /// Enumerates the elements of a <see cref="HashSetEx{T}"/>. 
    /// </summary>
#if !SILVERLIGHT && !WP7 && !XBOX
    [Serializable]
#endif
    public struct Enumerator : IEnumerator<T>
    {
      private HashSetEx<T> _hashSet;
      private int _index;
      private readonly int _version;
      private T _current;


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      public T Current
      {
        get { return _current; }
      }


      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <value>The element in the collection at the current position of the enumerator.</value>
      /// <exception cref="InvalidOperationException">
      /// The enumerator is positioned before the first element of the collection or after the last 
      /// element.
      /// </exception>
      object IEnumerator.Current
      {
        get
        {
          CheckState();
          if (_index <= 0)
            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection or after the last element.");

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="HashSetEx{T}.Enumerator"/> struct.
      /// </summary>
      /// <param name="hashSet">The <see cref="HashSetEx{T}"/> to be enumerated.</param>
      internal Enumerator(HashSetEx<T> hashSet)
      {
        _hashSet = hashSet;
        _index = 0;
        _version = hashSet._version;
        _current = default(T);
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _hashSet = null;
        _current = default(T);
      }


      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "HashSetEx")]
      void CheckState()
      {
        if (_hashSet == null)
          throw new ObjectDisposedException(null);
        if (_hashSet._version != _version)
          throw new InvalidOperationException("HashSetEx<T> has been modified while it was iterated over.");
      }


      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
      /// <see langword="false"/> if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="InvalidOperationException">
      /// The collection was modified after the enumerator was created.
      /// </exception>
      public bool MoveNext()
      {
        CheckState();

        if (_index < 0)
          return false;

        while (_index < _hashSet._touchedSlots)
        {
          if (_hashSet._slots[_index].HashCode >= 0)
          {
            _current = _hashSet._slots[_index].Item;
            _index++;
            return true;
          }

          _index++;
        }

        _index = -1;
        _current = default(T);
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="HashSetEx{T}"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="HashSetEx{T}"/> was modified after the enumerator was created.
      /// </exception>
      void IEnumerator.Reset()
      {
        CheckState();
        _index = 0;
        _current = default(T);
      }
    }


    private sealed class HashSetEqualityComparer : IEqualityComparer<HashSetEx<T>>
    {
      public bool Equals(HashSetEx<T> lhs, HashSetEx<T> rhs)
      {
        if (lhs == rhs)
          return true;

        if (lhs == null || rhs == null || lhs.Count != rhs.Count)
          return false;

        // The following check assumes that both HashSetEx<T> use the same IEqualityComparer<T>!
        foreach (var item in lhs)
          if (!rhs.Contains(item))
            return false;

        return true;
      }


      public int GetHashCode(HashSetEx<T> hashSet)
      {
        if (hashSet == null)
          return 0;

        var comparer = EqualityComparer<T>.Default;
        int hash = 0;
        foreach (var item in hashSet)
          hash ^= comparer.GetHashCode(item);

        return hash;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Constants
    //--------------------------------------------------------------

    private const int InitialSize = 4;
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private static HashSetEqualityComparer _setComparer;

    // The hash table stores the index into the slots array plus 1.
    // A value of 0 indicates that there is no entry.
    //  tableIndex = (hashCode & int.MaxValue) % _table.Length
    //  _table[tableIndex] = slotIndex + 1
    //  slotIndex = _table[tableIndex] - 1
    private int[] _table;

    private Slot[] _slots;

    // The number of slot that are in use (i.e. filled with data) or have been used
    // and are not in the "free list". The index of the first untouched slot.
    private int _touchedSlots;

    // The index of the first free slot in the free list.
    // Remove() prepends the cleared slots to the free list.
    // Add() takes the first slot from the free list, or increases _touchedSlots
    // if the free list is empty.
    private int _freeList;

    // The number of items in the set.
    private int _count;

    // The IEqualityComparer that computes the hash codes or compares items.
    private IEqualityComparer<T> _comparer;

    // The version number is incremented with every change. Used by the enumerator
    // to detect changes.
    private int _version;

#if !SILVERLIGHT && !WP7 && !XBOX
    private SerializationInfo _serializationInfo;
#endif
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for 
    /// comparing values of type <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for comparing 
    /// values of type <typeparamref name="T"/>.
    /// </value>
    public IEqualityComparer<T> Comparer
    {
      get { return _comparer; }
    }


    /// <summary>
    /// Gets the number of elements contained in the <see cref="HashSetEx{T}"/>. 
    /// </summary>
    /// <value>The number of elements contained in the <see cref="HashSetEx{T}"/>.</value>
    public int Count
    {
      get { return _count; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<T>.IsReadOnly
    {
      get { return false; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEx{T}" /> class.
    /// </summary>
    public HashSetEx()
    {
      Initialize(InitialSize, null);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEx{T}" /> class.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for comparing 
    /// values of type <typeparamref name="T"/>.
    /// </param>
    public HashSetEx(IEqualityComparer<T> comparer)
    {
      Initialize(InitialSize, comparer);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEx{T}" /> class.
    /// </summary>
    /// <param name="collection">The initial content of the set.</param>
    public HashSetEx(IEnumerable<T> collection)
      : this(collection, null)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEx{T}" /> class.
    /// </summary>
    /// <param name="collection">The initial content of the set.</param>
    /// <param name="comparer">
    /// The <see cref="IEqualityComparer{T}"/> used for calculating hash codes and for comparing 
    /// values of type <typeparamref name="T"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    public HashSetEx(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      var col = collection as ICollection<T>;
      Initialize((col != null) ? col.Count : InitialSize, comparer);

      foreach (var item in collection)
        Add(item);
    }


#if !SILVERLIGHT && !WP7 && !XBOX
    /// <summary>
    /// Initializes a new instance of the <see cref="HashSetEx{T}" /> class.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The serialization context.</param>
    protected HashSetEx(SerializationInfo info, StreamingContext context)
    {
      _serializationInfo = info;
    }
#endif


    private void Initialize(int capacity, IEqualityComparer<T> comparer)
    {
      capacity = PrimeHelper.NextPrime(capacity);

      _table = new int[capacity];
      _slots = new Slot[capacity];
      _touchedSlots = 0;
      _freeList = -1;
      _count = 0;
      _comparer = comparer ?? EqualityComparer<T>.Default;
      _version = 0;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

#if !SILVERLIGHT && !WP7 && !XBOX
    /// <summary>
    /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object. 
    /// </summary>
    /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
    /// <param name="context">
    /// The destination (see <see cref="StreamingContext"/>) for this serialization.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="info"/> is <see langword="null"/>.
    /// </exception>
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new ArgumentNullException("info");

      info.AddValue("Version", _version);
      info.AddValue("Comparer", _comparer, typeof(IEqualityComparer<T>));
      info.AddValue("Capacity", _table.Length);

      T[] elements = new T[_table.Length];
      CopyTo(elements);
      info.AddValue("Elements", elements, typeof(T[]));
    }


    /// <summary>
    /// Runs when the entire object graph has been deserialized.
    /// </summary>
    /// <param name="sender">
    /// The object that initiated the callback. The functionality for this parameter is not 
    /// currently implemented.
    /// </param>
    public virtual void OnDeserialization(object sender)
    {
      if (_serializationInfo != null)
      {
        _version = (int)_serializationInfo.GetValue("Version", typeof(int));
        _comparer = (IEqualityComparer<T>)_serializationInfo.GetValue("Comparer", typeof(IEqualityComparer<T>));
        int capacity = (int)_serializationInfo.GetValue("Capacity", typeof(int));

        if (capacity > 0)
        {
          _table = new int[capacity];
          _slots = new Slot[capacity];
          _touchedSlots = 0;
          _freeList = -1;
          _count = 0;

          T[] elements = (T[])_serializationInfo.GetValue("Elements", typeof(T[]));
          if (elements == null)
            throw new SerializationException("Missing Elements");

          for (int i = 0; i < elements.Length; i++)
            Add(elements[i]);
        }
        else
        {
          Initialize(InitialSize, null);
        }

        _serializationInfo = null;
      }
    }
#endif


    int GetItemHashCode(T item)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (item == null)
        return 0;
      // ReSharper restore CompareNonConstrainedGenericWithNull

      // Clear sign bit. (Only positive hash codes allowed!)
      return _comparer.GetHashCode(item) & int.MaxValue;
    }


    private bool SlotsContainsAt(int tableIndex, int hashCode, T item)
    {
      // Find head of chain.
      int slotIndex = _table[tableIndex] - 1;

      // Check items in chain.
      while (slotIndex != -1)
      {
        var slot = _slots[slotIndex];
        if (slot.HashCode == hashCode && _comparer.Equals(slot.Item, item))
          return true;

        slotIndex = slot.Next;
      }

      return false;
    }


    /// <summary>
    /// Adds an element to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to add to the <see cref="ICollection{T}"/>.
    /// </param>
    void ICollection<T>.Add(T item)
    {
      Add(item);
    }


    /// <summary>
    /// Adds an element to the <see cref="HashSetEx{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to add to the <see cref="HashSetEx{T}"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was added to the set; otherwise, 
    /// <see langword="false"/> if <paramref name="item"/> was already in the set. 
    /// </returns>
    public bool Add(T item)
    {
      int hashCode = GetItemHashCode(item);
      int tableIndex = hashCode % _table.Length;

      if (SlotsContainsAt(tableIndex, hashCode, item))
        return false;

      // Use free slot, if available.
      int slotIndex = _freeList;
      if (slotIndex == -1)
      {
        // Free list is empty. Use new slot.
        slotIndex = _touchedSlots;
        if (slotIndex == _slots.Length)
        {
          // Increase capacity.
          Resize(2 * _count);
          tableIndex = hashCode % _table.Length;
        }
        _touchedSlots++;
      }
      else
      {
        // Remove slot from head of free list.
        _freeList = _slots[slotIndex].Next;
      }

      // Prepend the new item to the linked list and update the hash table.
      // (The hash table points to the head of the linked list.)
      _slots[slotIndex].HashCode = hashCode;
      _slots[slotIndex].Next = _table[tableIndex] - 1;
      _slots[slotIndex].Item = item;
      _table[tableIndex] = slotIndex + 1;

      _count++;
      _version++;
      return true;
    }


    /// <summary>
    /// Removes all elements from the <see cref="HashSetEx{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Count"/> is set to zero, and references to other objects from elements of the 
    /// collection are also released. 
    /// </para>
    /// <para>
    /// The capacity remains unchanged. To reset the capacity of the <see cref="HashSetEx{T}"/>,
    /// call <see cref="TrimExcess"/>. Trimming an empty <see cref="HashSetEx{T}"/> sets the
    /// capacity of the <see cref="HashSetEx{T}"/> to the default capacity. 
    /// </para>
    /// </remarks>
    public void Clear()
    {
      if (_touchedSlots > 0)
      {
        Array.Clear(_table, 0, _table.Length);
        Array.Clear(_slots, 0, _touchedSlots);
        _touchedSlots = 0;
        _freeList = -1;
        _count = 0;
        _version++;
      }
    }


    /// <summary>
    /// Determines whether the <see cref="HashSetEx{T}"/> contains a specific element.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="HashSetEx{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the <see cref="HashSetEx{T}"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(T item)
    {
      int hashCode = GetItemHashCode(item);
      int tableIndex = hashCode % _table.Length;
      return SlotsContainsAt(tableIndex, hashCode, item);
    }


    /// <summary>
    /// Copies the elements of the <see cref="HashSetEx{T}"/> to an <see cref="Array"/>, starting at
    /// a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="HashSetEx{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or the number of elements in the source 
    /// <see cref="HashSetEx{T}"/> is greater than the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(T[] array)
    {
      CopyTo(array, 0, _count);
    }


    /// <summary>
    /// Copies the elements of the <see cref="HashSetEx{T}"/> to an <see cref="Array"/>, starting at a 
    /// particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="HashSetEx{T}"/>. The <see cref="Array"/> must have zero-based indexing.
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
    /// source <see cref="HashSetEx{T}"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
      CopyTo(array, arrayIndex, _count);
    }


    /// <summary>
    /// Copies the specified number of elements of the <see cref="HashSetEx{T}"/> to an 
    /// <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="HashSetEx{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <param name="count">The number of elements to copy.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements to copy is
    /// is greater than the available space from <paramref name="arrayIndex"/> to the end of the 
    /// destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(T[] array, int arrayIndex, int count)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex");
      if (arrayIndex > array.Length)
        throw new ArgumentException("index larger than largest valid index of array");
      if (array.Length - arrayIndex < count)
        throw new ArgumentException("Destination array cannot hold the requested elements!");

      for (int i = 0, n = 0; i < _touchedSlots && n < count; i++)
      {
        if (_slots[i].HashCode >= 0)
        {
          array[arrayIndex + n] = _slots[i].Item;
          n++;
        }
      }
    }


    /// <summary>
    /// Removes the specified element from the <see cref="HashSetEx{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="HashSetEx{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="HashSetEx{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="HashSetEx{T}"/>.
    /// </returns>
    public bool Remove(T item)
    {
      int hashCode = GetItemHashCode(item);
      return Remove(item, hashCode);
    }


    private bool Remove(T item, int hashCode)
    {
      int tableIndex = hashCode % _table.Length;
      int slotIndex = _table[tableIndex] - 1;

      if (slotIndex == -1)
        return false;

      // Walk linked list until right slot (and its predecessor) is found or end 
      // is reached.
      int prevSlotIndex = -1;
      do
      {
        var slot = _slots[slotIndex];
        if (slot.HashCode == hashCode && _comparer.Equals(slot.Item, item))
          break;

        prevSlotIndex = slotIndex;
        slotIndex = slot.Next;
      } while (slotIndex != -1);

      // If we reached the end of the chain, return false.
      if (slotIndex == -1)
        return false;

      // Remove slot from linked list.
      if (prevSlotIndex == -1)
      {
        // The slot is the head of the linked list.
        _table[tableIndex] = _slots[slotIndex].Next + 1;
      }
      else
      {
        // The slot is in the middle or end of the linked list.
        _slots[prevSlotIndex].Next = _slots[slotIndex].Next;
      }

      // Clear slot and prepend it to the free list.
      _slots[slotIndex].HashCode = -1;
      _slots[slotIndex].Next = _freeList;
      _slots[slotIndex].Item = default(T);
      _freeList = slotIndex;

      _count--;
      _version++;
      return true;
    }


    /// <summary>
    /// Removes all elements that matches a certain criterion from the <see cref="HashSetEx{T}"/>.
    /// </summary>
    /// <param name="match">The predicate that defines which elements should be removed.</param>
    /// <returns>
    /// The number of elements that were removed from the <see cref="HashSetEx{T}"/>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="match"/> is <see langword="null"/>.
    /// </exception>
    public int RemoveWhere(Predicate<T> match)
    {
      if (match == null)
        throw new ArgumentNullException("match");

      int removed = 0;
      for (int i = 0; i < _touchedSlots; i++)
      {
        int hashCode = _slots[i].HashCode;
        if (hashCode >= 0)
        {
          T item = _slots[i].Item;
          if (match(item) && Remove(item, hashCode))
            removed++;
        }
      }

      return removed;
    }


    private void Resize(int newSize)
    {
      // Similar to TrimAccess(), except that Resize() does not compact the items.
      // The free list remains!

      // Hash table size needs to be prime.
      newSize = PrimeHelper.NextPrime(newSize);

      var newTable = new int[newSize];
      var newSlots = new Slot[newSize];

      // Copy current slots.
      Array.Copy(_slots, 0, newSlots, 0, _touchedSlots);

      // Update hash table and rebuild chains.
      for (int slotIndex = 0; slotIndex < _touchedSlots; slotIndex++)
      {
        int tableIndex = newSlots[slotIndex].HashCode % newSize;
        newSlots[slotIndex].Next = newTable[tableIndex] - 1;
        newTable[tableIndex] = slotIndex + 1;
      }

      _table = newTable;
      _slots = newSlots;
    }


    /// <summary>
    /// Sets the capacity of the <see cref="HashSetEx{T}"/> to a value suitable for the current
    /// number of elements in the set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method can be used to minimize a collection's memory overhead if no new elements will 
    /// be added to the collection.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is <see cref="Count"/>. 
    /// </para>
    /// <para>
    /// To reset a <see cref="HashSetEx{T}"/> to its initial state, call the <see cref="Clear"/> 
    /// method before calling <see cref="TrimExcess"/> method. Trimming an empty 
    /// <see cref="HashSetEx{T}"/> sets the capacity of the <see cref="HashSetEx{T}"/> to the
    /// default capacity.
    /// </para>
    /// </remarks>
    public void TrimExcess()
    {
      // Similar to Resize(), except that TrimExcess() compacts the items and clears 
      // the free list!

      int newSize = PrimeHelper.NextPrime((_count > 0) ? _count : InitialSize);
      int[] newTable = new int[newSize];
      var newSlots = new Slot[newSize];

      // Insert items in new hash table and slots (without recomputing the hash code).
      int newIndex = 0;
      for (int oldIndex = 0; oldIndex < _touchedSlots; oldIndex++)
      {
        if (_slots[oldIndex].HashCode >= 0)
        {
          newSlots[newIndex].HashCode = _slots[oldIndex].HashCode;
          newSlots[newIndex].Item = _slots[oldIndex].Item;

          int tableIndex = newSlots[newIndex].HashCode % newSize;
          newSlots[newIndex].Next = newTable[tableIndex] - 1;
          newTable[tableIndex] = newIndex + 1;
          newIndex++;
        }
      }

      _table = newTable;
      _slots = newSlots;
      _touchedSlots = newIndex;
      _freeList = -1;
      _version++;
    }


    #region ----- IEnumerable, IEnumerable<T> -----

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }
    #endregion


    #region ----- Set Operations -----

    HashSetEx<T> ToHashSetEx(IEnumerable<T> enumerable)
    {
      var hashSet = enumerable as HashSetEx<T>;
      if (hashSet == null || !_comparer.Equals(hashSet.Comparer))
        hashSet = new HashSetEx<T>(enumerable, _comparer);

      return hashSet;
    }


    /// <summary>
    /// Removes all elements in the specified collection from the current set.
    /// </summary>
    /// <param name="other">The collection of items to remove from the set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public void ExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (_count == 0)
        return;

      if (ReferenceEquals(this, other))
      {
        Clear();
      }
      else
      {
        var otherSet = other as HashSetEx<T>;
        if (otherSet != null)
        {
          // Use HashSetEx<T>.Enumerator (struct).
          foreach (var item in otherSet)
            Remove(item);
        }
        else
        {
          // Use IEnumerator<T> (class).
          foreach (var item in other)
            Remove(item);
        }
      }
    }


    /// <summary>
    /// Modifies the current set so that it contains only elements that are also in a specified 
    /// collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public void IntersectWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (_count == 0)
        return;

      var otherSet = ToHashSetEx(other);

      // Inlined: RemoveWhere(item => !otherSet.Contains(item));
      for (int i = 0; i < _touchedSlots; i++)
      {
        int hashCode = _slots[i].HashCode;
        if (hashCode >= 0)
        {
          T item = _slots[i].Item;
          if (!otherSet.Contains(item))
            Remove(item, hashCode);
        }
      }
    }


    /// <summary>
    /// Determines whether the current set overlaps with the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set and <paramref name="other"/> share at least one 
    /// common element; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool Overlaps(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (_count == 0)
        return false;

      var otherSet = other as HashSetEx<T>;
      if (otherSet != null)
      {
        // Use HashSetEx<T>.Enumerator (struct).
        foreach (var item in otherSet)
          if (Contains(item))
            return true;
      }
      else
      {
        // Use IEnumerator<T> (class).
        foreach (var item in other)
          if (Contains(item))
            return true;
      }

      return false;
    }


    /// <summary>
    /// Determines whether the current set and the specified collection contain the same elements.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set is equal to <paramref name="other"/>; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool SetEquals(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToHashSetEx(other);
      if (_count != otherSet.Count)
        return false;

      foreach (var item in this)
        if (!otherSet.Contains(item))
          return false;

      return true;
    }


    /// <summary>
    /// Modifies the current set so that it contains only elements that are present either in the 
    /// current set or in the specified collection, but not both.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (ReferenceEquals(this, other))
      {
        Clear();
      }
      else
      {
        var otherSet = ToHashSetEx(other);
        foreach (var item in otherSet)
          if (!Add(item))
            Remove(item);
      }
    }


    /// <summary>
    /// Modifies the current set so that it contains all elements that are present in both the 
    /// current set and in the specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public void UnionWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (!ReferenceEquals(this, other))
      {
        var otherSet = other as HashSetEx<T>;
        if (otherSet != null)
        {
          // Use HashSetEx<T>.Enumerator (struct).
          foreach (var item in otherSet)
            Add(item);
        }
        else
        {
          // Use IEnumerator<T> (class).
          foreach (var item in other)
            Add(item);
        }
      }
    }


    bool CheckIsSubsetOf(HashSetEx<T> other)
    {
      foreach (var item in this)
        if (!other.Contains(item))
          return false;

      return true;
    }


    /// <summary>
    /// Determines whether a set is a subset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set is a subset of <paramref name="other"/>; 
    /// otherwise, <see langword="false"/>. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool IsSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      if (_count == 0)
        return true;

      var otherSet = ToHashSetEx(other);
      if (_count > otherSet.Count)
        return false;

      return CheckIsSubsetOf(otherSet);
    }


    /// <summary>
    /// Determines whether the current set is a proper (strict) subset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set is a correct subset of <paramref name="other"/>; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToHashSetEx(other);

      // The other set must have at least one item not in this set.
      if (_count >= otherSet.Count)
        return false;

      return CheckIsSubsetOf(otherSet);
    }


    bool CheckIsSupersetOf(HashSetEx<T> other)
    {
      foreach (var item in other)
        if (!Contains(item))
          return false;

      return true;
    }


    /// <summary>
    /// Determines whether the current set is a superset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set is a superset of <paramref name="other"/>; 
    /// otherwise, <see langword="false"/>. 
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool IsSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToHashSetEx(other);

      if (_count < otherSet.Count)
        return false;

      return CheckIsSupersetOf(otherSet);
    }


    /// <summary>
    /// Determines whether the current set is a correct superset of a specified collection.
    /// </summary>
    /// <param name="other">The collection to compare to the current set.</param>
    /// <returns>
    /// <see langword="true"/> if the current set is a correct superset of <paramref name="other"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="other"/> is <see langword="null"/>.
    /// </exception>
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");

      var otherSet = ToHashSetEx(other);

      // This set must have at least one item not in the other set.
      if (_count <= otherSet.Count)
        return false;

      return CheckIsSupersetOf(otherSet);
    }
    #endregion


    /// <summary>
    /// Returns an <see cref="IEqualityComparer{T}"/> that can be used to compare 
    /// <see cref="HashSetEx{T}"/> objects.
    /// </summary>
    /// <returns>
    /// The <see cref="IEqualityComparer{T}"/> that can be used to compare 
    /// <see cref="HashSetEx{T}"/> objects.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
    public static IEqualityComparer<HashSetEx<T>> CreateSetComparer()
    {
      if (_setComparer == null)
        _setComparer = new HashSetEqualityComparer();

      return _setComparer;
    }
    #endregion
  }
}
#endif
