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
  /// Represents a collection of objects of type <typeparamref name="T"/> using weak references. 
  /// </summary>
  /// <typeparam name="T">The type of the elements in the collection.</typeparam>
  [DebuggerDisplay("{GetType().Name,nq}(Count = {Count})")]
  [DebuggerTypeProxy(typeof(CollectionDebugView<>))]
  public sealed class WeakCollection<T> : ICollection<T>, ICollection where T : class
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// Enumerates the elements of a <see cref="WeakCollection{T}"/>. 
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
      private readonly WeakCollection<T> _weakCollection;
      private readonly int _version;
      private int _index;
      private T _current;
      private int _numberOfDeadItems;


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
          if (_index < 0)
          {
            if (_index == -1)
              throw new InvalidOperationException("The enumerator is positioned before the first element of the collection.");

            throw new InvalidOperationException("The enumerator is positioned after the last element of the collection.");
          }

          return _current;
        }
      }


      /// <summary>
      /// Initializes a new instance of the <see cref="WeakCollection{T}.Enumerator"/> struct.
      /// </summary>
      /// <param name="weakCollection">The <see cref="WeakCollection{T}"/> to be enumerated.</param>
      internal Enumerator(WeakCollection<T> weakCollection)
      {
        _weakCollection = weakCollection;
        _version = weakCollection._version;
        _index = -1;
        _current = default(T);
        _numberOfDeadItems = 0;
      }


      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting 
      /// unmanaged resources.
      /// </summary>
      public void Dispose()
      {
        _index = -2;
        _current = default(T);

        if (_numberOfDeadItems >= PurgeThreshold)
        {
          _weakCollection.Purge();
          _numberOfDeadItems = 0;
        }
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
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
      public bool MoveNext()
      {
        if (_version != _weakCollection._version)
          throw new InvalidOperationException("The WeakCollection<T> was modified after the enumerator was created.");

        if (_index == -2)
          return false;

        _index++;
        while (_index < _weakCollection._size)
        {
          var item = _weakCollection._weakHandles[_index].Target as T;
          if (item != null)
          {
            _current = item;
            return true;
          }

          _numberOfDeadItems++;
          _index++;
        }


        _index = -2;
        _current = default(T);
        return false;
      }


      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the 
      /// <see cref="WeakCollection{T}"/>.
      /// </summary>
      /// <exception cref="InvalidOperationException">
      /// The <see cref="WeakCollection{T}"/> was modified after the enumerator was created.
      /// </exception>
      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
      public void Reset()
      {
        if (_version != _weakCollection._version)
          throw new InvalidOperationException("The WeakCollection<T> was modified after the enumerator was created.");

        _index = -1;
        _current = default(T);
        _numberOfDeadItems = 0;
      }

    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private const int PurgeThreshold = 1;
    private WeakReference[] _weakHandles;
    private int _size;
    private int _version;
    private object _syncRoot;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of items contained in the <see cref="WeakCollection{T}"/>.
    /// </summary>
    /// <value>The number of items contained in the <see cref="WeakCollection{T}"/>.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "item")]
    public int Count
    {
      get
      {
        // ReSharper disable UnusedVariable
        int count = 0;
        foreach (T item in this)
          count++;

        return count;
        // ReSharper restore UnusedVariable
      }
    }


    /// <summary>
    /// Gets a value indicating whether the collection is read only. Always returns 
    /// <see langword="false"/>.
    /// </summary>
    bool ICollection<T>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized 
    /// (thread safe).
    /// </summary>
    /// <value>
    /// <see langword="true"/> if access to the <see cref="ICollection"/> is synchronized (thread 
    /// safe); otherwise, <see langword="false"/>.
    /// </value>
    bool ICollection.IsSynchronized
    {
      get { return false; }
    }


    /// <summary>
    /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
    /// </summary>
    /// <value>
    /// An object that can be used to synchronize access to the <see cref="ICollection"/>.
    /// </value>
    object ICollection.SyncRoot
    {
      get
      {
        if (_syncRoot == null)
          Interlocked.CompareExchange(ref _syncRoot, new object(), null);

        return _syncRoot;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="WeakCollection{T}"/> class.
    /// </summary>
    public WeakCollection()
    {
      _weakHandles = CollectionHelper.EmptyWeakReferenceArray;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns a read-only wrapper for the current collection.
    /// </summary>
    /// <returns>
    /// A <see cref="ReadOnlyWeakCollection{T}"/> that acts as a read-only wrapper around the 
    /// current <see cref="WeakCollection{T}"/>.
    /// </returns>
    public ReadOnlyWeakCollection<T> AsReadOnly()
    {
      return new ReadOnlyWeakCollection<T>(this);
    }


    /// <summary>
    /// Removes all items from the <see cref="WeakCollection{T}"/>.
    /// </summary>
    public void Clear()
    {
      if (_size > 0)
      {
        Array.Clear(_weakHandles, 0, _size);
        _size = 0;
      }

      _version++;
    }


    /// <summary>
    /// Determines whether the <see cref="WeakCollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="WeakCollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a specific value; <see langword="false"/> 
    /// if it does not.
    /// </returns>
    public bool Contains(T item)
    {
      if (item == null)
        return false;

      var comparer = EqualityComparer<T>.Default;
      foreach (T t in this)
        if (comparer.Equals(item, t))
          return true;

      return false;
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at 
    /// a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="index">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="index"/> is equal to
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the
    /// source <see cref="ICollection"/> is greater than the available space from 
    /// <paramref name="index"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The type of the source <see cref="ICollection"/> cannot be cast automatically to the type of
    /// the destination <paramref name="array"/>.
    /// </exception>
    void ICollection.CopyTo(Array array, int index)
    {
      if (array == null)
        throw new ArgumentNullException("array");

      if (index < 0)
        throw new ArgumentOutOfRangeException("index", "Array index must be equal to or greater than 0.");

      if (array.Length > 0 && array.Length <= index)
        throw new ArgumentOutOfRangeException("index", "Array index must be less than the length of the array.");

      foreach (T item in this)
      {
        if (index >= array.Length)
          throw new ArgumentException("The number of elements is greater than the available space from index to the end of the destination array.");

        array.SetValue(item, index);
        index++;
      }
    }


    /// <summary>
    /// Copies the elements of the <see cref="WeakCollection{T}"/> to an <see cref="Array"/>, 
    /// starting at a particular <see cref="Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="WeakCollection{T}"/>. The <see cref="Array"/> must have zero-based indexing.
    /// </param>
    /// <param name="index">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="index"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="WeakCollection{T}"/> is greater than the available space from 
    /// <paramref name="index"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The <see cref="WeakCollection{T}"/> is not modified. The order of the elements in the new 
    /// array is the same as the order of the live elements in the <see cref="WeakCollection{T}"/>.
    /// </para>
    /// </remarks>
    void ICollection<T>.CopyTo(T[] array, int index)
    {
      ((ICollection)this).CopyTo(array, index);
    }


    private void SetCapacity(int capacity)
    {
      var array = new WeakReference[capacity];
      Array.Copy(_weakHandles, 0, array, 0, _size);
      _weakHandles = array;
    }


    /// <summary>
    /// Adds an item to the <see cref="WeakCollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="WeakCollection{T}"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. The <see cref="WeakCollection{T}"/> does 
    /// not support null entries.
    /// </exception>
    public void Add(T item)
    {
      if (item == null)
        throw new ArgumentNullException("item");

      if (_size == _weakHandles.Length)
      {
        int capacity = (_weakHandles.Length == 0) ? 4 : _weakHandles.Length * 2;
        SetCapacity(capacity);
      }

      _weakHandles[_size] = new WeakReference(item);
      _size++;
      _version++;
    }


    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="WeakCollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="WeakCollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ICollection{T}"/>.
    /// </returns>
    public bool Remove(T item)
    {
      if (item == null)
        return false;

      var comparer = EqualityComparer<T>.Default;
      int index = -1;
      for (int i = 0; i < _size; i++)
      {
        var target = _weakHandles[i].Target as T;
        if (target != null && comparer.Equals(item, target))
        {
          index = i;
          break;
        }
      }

      if (index == -1)
        return false;

      _size--;
      if (index < _size)
        Array.Copy(_weakHandles, index + 1, _weakHandles, index, _size - index);

      _weakHandles[_size] = null;
      _version++;
      return true;
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
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }


    /// <summary>
    /// Removes all dead objects from the collection.
    /// </summary>
    private void Purge()
    {
      // Compact live handles at start of list.
      int writeIndex = 0;
      for (int readIndex = 0; readIndex < _size; readIndex++)
      {
        if (_weakHandles[readIndex].Target != null)
        {
          // Object is alive.
          if (readIndex != writeIndex)
            _weakHandles[writeIndex] = _weakHandles[readIndex];

          writeIndex++;
        }
      }

      _size = writeIndex;
      _version++;
    }
    #endregion
  }
}
