// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Represents a queue of values where the greatest item can be accessed quickly.
  /// </summary>
  /// <typeparam name="T">The type of items in this collection.</typeparam>
  /// <remarks>
  /// <para>
  /// This class implements a heap-based priority queue. Items can be added with 
  /// <see cref="Enqueue"/> (runtime complexity O(log n)). <see cref="Peek"/> can be used to return 
  /// the greatest item in the queue without removing the item (runtime complexity O(1)). 
  /// <see cref="Dequeue"/> returns the greatest item and also removes it from the queue (runtime 
  /// complexity O(log n)).
  /// </para>
  /// <para>
  /// <strong>Important:</strong> The enumerator (see <see cref="GetEnumerator"/>) returns the items
  /// in an arbitrary order - not sorted by priority!
  /// </para>
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
  public class PriorityQueue<T> : ICollection<T>, ICollection
  {
    // Notes:
    // A heap-based priority queue. 
    // References: 
    // - Book: Algorithms in C++, Robert Sedgewick
    // - Book: Informatik-Handbuch, Rechenberg et al.


    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Enumerates the elements of a <see cref="PriorityQueue{T}"/>. 
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
      private PriorityQueue<T> _queue;
      private int _index; // The index of the element after current.
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
          if (_index <= 0)
            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection or after the last element.");

          return _current;
        }
      }


      internal Enumerator(PriorityQueue<T> queue)
      {
        _queue = queue;
        _index = 0;
        _version = queue._version;
        _current = default(T);
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _queue = null;
        _current = default(T);
      }


      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PriorityQueue")]
      void CheckState()
      {
        if (_queue == null)
          throw new ObjectDisposedException(GetType().FullName);
        if (_queue._version != _version)
          throw new InvalidOperationException("PriorityQueue<T> was modified after the enumerator was created.");
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

        if (_index < _queue._count)
        {
          _current = _queue._heap[_index];
          _index++;
          return true;
        }

        _index = -1;
        _current = default(T);
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="PriorityQueue{T}"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="PriorityQueue{T}"/> was modified after the enumerator was created.
      /// </exception>
      void IEnumerator.Reset()
      {
        CheckState();
        _index = 0;
        _current = default(T);
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // Start with 4 heap levels.
    private const int InitialSize = 7;

    private int _size;
    private T[] _heap;
    private int _version;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the <see cref="IComparer{T}"/> used for comparing items type <typeparamref name="T"/>.
    /// </summary>
    /// <value>
    /// The <see cref="IComparer{T}"/> used for comparing items of type <typeparamref name="T"/>.
    /// </value>
    public IComparer<T> Comparer
    {
      get { return _comparer; }
    }
    private readonly IComparer<T> _comparer;


    /// <summary>
    /// Gets the number of items contained in the <see cref="PriorityQueue{T}"></see>.
    /// </summary>
    /// <returns>
    /// The number of items contained in the <see cref="PriorityQueue{T}"></see>.
    /// </returns>
    public int Count
    {
      get { return _count; }
    }
    private int _count;


    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="PriorityQueue{T}" />.
    /// </summary>
    /// <returns>
    /// An object that can be used to synchronize access to the <see cref="PriorityQueue{T}" />.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object ICollection.SyncRoot
    {
      get
      {
        if (_syncRoot == null)
          Interlocked.CompareExchange(ref _syncRoot, new object(), null);

        return _syncRoot;
      }
    }
    private object _syncRoot;


    /// <summary>
    /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized 
    /// (thread safe).
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if access to the <see cref="ICollection" /> is synchronized (thread 
    /// safe); otherwise, <see langword="false"/>.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection.IsSynchronized
    {
      get { return false; }
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
    /// Initializes a new instance of the <see cref="PriorityQueue{T}" /> class.
    /// </summary>
    public PriorityQueue()
    {
      _comparer = Comparer<T>.Default;
      _size = InitialSize;
      _heap = new T[InitialSize];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T}" /> class.
    /// </summary>
    /// <param name="comparer">The comparer used to compare items.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="comparer"/> is <see langword="null"/>.
    /// </exception>
    public PriorityQueue(IComparer<T> comparer)
    {
      if (comparer == null)
        throw new ArgumentNullException("comparer");

      _comparer = comparer;
      _size = InitialSize;
      _heap = new T[InitialSize];
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="PriorityQueue{T}" /> class.
    /// </summary>
    /// <param name="comparison">The comparison delegate used to compare items.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="comparison"/> is <see langword="null"/>.
    /// </exception>
    public PriorityQueue(Comparison<T> comparison)
    {
      if (comparison == null)
        throw new ArgumentNullException("comparison");

      _comparer = new DelegateComparer<T>(comparison);
      _size = InitialSize;
      _heap = new T[InitialSize];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the greatest item in the queue without removing the item.
    /// </summary>
    /// <returns>The greatest item in the queue.</returns>
    /// <exception cref="InvalidOperationException">
    /// The priority queue is empty.
    /// </exception>
    public T Peek()
    {
      if (_count == 0)
        throw new InvalidOperationException("The priority queue is empty.");

      return _heap[0];
    }


    /// <summary>
    /// Adds the specified item to the queue
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public void Enqueue(T item)
    {
      if (_count == _size)
      {
        // Add an additional heap level.
        _size = _size * 2 + 1;
        Array.Resize(ref _heap, _size);
      }

      _version++;

      var index = _count;
      _count++;
      Upheap(index, item);
    }


    /// <summary>
    /// Gets the greatest item in the queue and removes the item.
    /// </summary>
    /// <returns>The greatest item in the queue.</returns>
    /// <exception cref="InvalidOperationException">
    /// The priority queue is empty.
    /// </exception>
    public T Dequeue()
    {
      if (_count == 0)
        throw new InvalidOperationException("The priority queue is empty.");

      _version++;

      var result = _heap[0];
      _count--;

      var lastItem = _heap[_count];
      _heap[_count] = default(T);
      Downheap(0, lastItem);

      return result;
    }


    /// <summary>
    /// Sets the capacity of the <see cref="PriorityQueue{T}"/> to a value suitable for the current 
    /// number of elements in the set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method can be used to minimize a collection's memory overhead if no new elements will 
    /// be added to the collection.
    /// </para>
    /// <para>
    /// To reset a <see cref="PriorityQueue{T}"/> to its initial state, call the <see cref="Clear"/> 
    /// method before calling <see cref="TrimExcess"/> method. Trimming an empty 
    /// <see cref="PriorityQueue{T}"/> sets the capacity of the <see cref="PriorityQueue{T}"/> to 
    /// the default capacity.
    /// </para>
    /// </remarks>
    public void TrimExcess()
    {
      var bestSize = Bitmask((uint)_count);

      if (bestSize < _size)
      {
        _size = Math.Max((int)bestSize, InitialSize);
        Array.Resize(ref _heap, _size);
      }
    }


    // Copied from DigitalRune.Mathematics.MathHelper:
    /// <summary>
    /// Creates the smallest bitmask that is greater than or equal to the given value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This result can also be interpreted as finding the smallest x such that 2<sup>x</sup> &gt; 
    /// <paramref name="value"/> and returning 2<sup>x</sup> - 1.
    /// </para>
    /// </remarks>
    private static uint Bitmask(uint value)
    {
      // Example:                 value = 10000000 00000000 00000000 00000000
      value |= (value >> 1);   // value = 11000000 00000000 00000000 00000000
      value |= (value >> 2);   // value = 11110000 00000000 00000000 00000000
      value |= (value >> 4);   // value = 11111111 00000000 00000000 00000000
      value |= (value >> 8);   // value = 11111111 11111111 00000000 00000000
      value |= (value >> 16);  // value = 11111111 11111111 11111111 11111111
      return value;
    }


    private static int GetLeftChildIndex(int index)
    {
      return 2 * index + 1;
    }


    // Not needed.
    //private static int GetRightChildIndex(int index)
    //{
    //  return 2 * (index + 1);
    //}


    private static int GetParentIndex(int index)
    {
      return (index - 1) / 2;
    }


    /// <summary>
    /// Performs the standard downheap operation with a minor tweak: item is not yet in the array, 
    /// so we do not need to copy it around.
    /// </summary>
    private void Downheap(int index, T item)
    {
      var childIndex = GetLeftChildIndex(index);

      // Abort if the node index does not have children.
      while (childIndex < _count)
      {
        bool hasRightChild = childIndex + 1 < _count;
        if (hasRightChild)
        {
          if (_comparer.Compare(_heap[childIndex], _heap[childIndex + 1]) < 0)
            childIndex++;
        }

        // If all children are smaller than the current item, we can abort.
        if (_comparer.Compare(_heap[childIndex], item) < 0)
          break;

        _heap[index] = _heap[childIndex];
        index = childIndex;
        childIndex = GetLeftChildIndex(index);
      }

      _heap[index] = item;
    }


    /// <summary>
    /// Performs the standard upheap operation with a minor tweak: item is not yet in the array, 
    /// so we do not need to copy it around.
    /// </summary>
    private void Upheap(int index, T item)
    {
      var parentIndex = GetParentIndex(index);
      while (index > 0 && _comparer.Compare(_heap[parentIndex], item) < 0)
      {
        _heap[index] = _heap[parentIndex];
        index = parentIndex;
        parentIndex = GetParentIndex(index);
      }

      _heap[index] = item;
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}" /> object that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Adds the specified item. (Same as <see cref="Enqueue"/>.)
    /// </summary>
    /// <param name="item">The item.</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<T>.Add(T item)
    {
      Enqueue(item);
    }


    /// <summary>
    /// Clears this queue.
    /// </summary>
    public void Clear()
    {
      _version++;
      _count = 0;
      Array.Clear(_heap, 0, _count);
    }


    /// <summary>
    /// Determines whether the queue contains the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/> if the queue contains the specified item; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Contains(T item)
    {
      return IndexOf(item, 0) >= 0;
    }


    // Returns the index of the specified item. The search checks only the sub-heap
    // with the root at subHeapIndex. 
    private int IndexOf(T item, int subHeapIndex)
    {
      if (subHeapIndex >= _count)
        return -1;

      // If the current item is larger than the root of the heap, we can
      // abort.
      var result = _comparer.Compare(item, _heap[subHeapIndex]);
      if (result > 0)
        return -1;

      if (result == 0)
        return subHeapIndex;

      // Check the left sub-heap.
      var leftChildIndex = GetLeftChildIndex(subHeapIndex);
      var index = IndexOf(item, leftChildIndex);
      if (index >= 0)
        return index;

      // No match in the left sub-heap. Check the right sub-heap.
      return IndexOf(item, leftChildIndex + 1);
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at 
    /// a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the
    /// source <see cref="ICollection"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The type of the source <see cref="ICollection"/> cannot be cast automatically to the type of
    /// the destination <paramref name="array"/>.
    /// </exception>
    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      Array.Copy(_heap, 0, array, arrayIndex, _count);
    }


    /// <summary>
    /// Copies the elements of the <see cref="PriorityQueue{T}"/> to an <see cref="Array"/>, 
    /// starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="PriorityQueue{T}"/>. The <see cref="Array"/> must have zero-based indexing.
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
    /// source <see cref="PriorityQueue{T}"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="PriorityQueue{T}"/> is not modified. The order of the elements in the new 
    /// array is not necessarily the same as the order of the elements from the head of the 
    /// <see cref="PriorityQueue{T}"/> to its tail.
    /// </para>
    /// <para>
    /// This method is an O(n) operation, where n is <see cref="Count"/>.
    /// </para>
    /// </remarks>
    public void CopyTo(T[] array, int arrayIndex)
    {
      Array.Copy(_heap, 0, array, arrayIndex, _count);
    }


    /// <summary>
    /// Removes the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>
    /// <see langword="true"/> if the item was found and removed; <see langword="false"/> if the 
    /// item is not contained in the queue.
    /// </returns>
    public bool Remove(T item)
    {
      var index = IndexOf(item, 0);

      if (index < 0)
        return false;

      _version++;
      _count--;
      var lastItem = _heap[_count];
      _heap[_count] = default(T);

      Downheap(index, lastItem);

      return true;
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes", Justification = "Only for debugging.")]
    [Conditional("DEBUG")]
    internal void Validate()
    {
      if (_count > _size)
        throw new Exception("Count is larger than size.");
      if (_size != Bitmask((uint)_size))
        throw new Exception("Invalid size.");

      if (_size != _heap.Length)
        throw new Exception("Count is invalid.");

      var comparer = Comparer<T>.Default;
      for (int i = _count; i < _size; i++)
      {
        if (comparer.Compare(_heap[i], default(T)) != 0)
          throw new Exception("Heap entries were not reset.");
      }
    }
    #endregion
  }
}
