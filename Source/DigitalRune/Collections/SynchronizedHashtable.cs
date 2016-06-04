// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;


namespace DigitalRune.Collections
{
  /// <summary>
  /// A thread-safe, fixed-size hash table.
  /// </summary>
  /// <typeparam name="TKey">The type of keys in the hash table.</typeparam>
  /// <typeparam name="TValue">The type of value in the hash table.</typeparam>
  internal sealed class SynchronizedHashtable<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    private class Node
    {
      internal TKey Key;
      internal TValue Value;
      internal volatile Node Next;
    }


    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
      private readonly Node[] _buckets;
      private int _index;
      private Node _node;

      object IEnumerator.Current
      {
        get
        {
          if (_node == null)
            throw new InvalidOperationException("The enumerator is positioned before the first element of the collection or after the last element.");

          return new KeyValuePair<TKey, TValue>(_node.Key, _node.Value);
        }
      }

      public KeyValuePair<TKey, TValue> Current
      {
        get
        {
          if (_node == null)
            return new KeyValuePair<TKey, TValue>();

          return new KeyValuePair<TKey, TValue>(_node.Key, _node.Value);
        }
      }

      public Enumerator(SynchronizedHashtable<TKey, TValue> table)
      {
        _buckets = table._buckets;
        _index = -1;
        _node = null;
      }

      public void Dispose()
      {
      }

      public bool MoveNext()
      {
        if (_node != null)
          _node = _node.Next;

        while (_node == null)
        {
          if (_index == _buckets.Length - 1)
            return false;

          _index++;
          _node = _buckets[_index];
        }

        return true;
      }

      public void Reset()
      {
        _index = -1;
        _node = null;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly EqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;
    // ReSharper restore StaticFieldInGenericType

    private Node[] _buckets;
    private readonly object _writeLock = new object();
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedHashtable{TKey,TValue}"/> class.
    /// </summary>
    /// <param name="capacity">The number of buckets in the hash table.</param>
    /// <remarks>
    /// For efficiency the <paramref name="capacity"/> is automatically incremented to the next 
    /// prime number.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> is less than 1.
    /// </exception>
    public SynchronizedHashtable(int capacity)
    {
      if (capacity < 1)
        throw new ArgumentOutOfRangeException("capacity", "The initial capacity must be greater than 0.");

      capacity = PrimeHelper.NextPrime(capacity);
      _buckets = new Node[capacity];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Adds an item with the provided key and value to the 
    /// <see cref="SynchronizedHashtable{TKey,TValue}"/>. (Requires locking.)
    /// </summary>
    /// <param name="key">The key of the item to add.</param>
    /// <param name="value">The value of the item to add.</param>
    public void Add(TKey key, TValue value)
    {
      int hash = (KeyComparer.GetHashCode(key) & int.MaxValue) % _buckets.Length;
      var node = new Node { Key = key, Value = value };
      lock (_writeLock)
      {
        node.Next = _buckets[hash];
        _buckets[hash] = node;
      }
    }


    /// <summary>
    /// Removes all keys and values from the <see cref="SynchronizedHashtable{TKey,TValue}"/>.
    /// (Requires locking.) 
    /// </summary>
    public void Clear()
    {
      lock (_writeLock)
        _buckets = new Node[_buckets.Length];
    }


    /// <summary>
    /// Removes the item associated with the specified key from the 
    /// <see cref="SynchronizedHashtable{TKey,TValue}"/>. (Requires locking.)
    /// </summary>
    /// <param name="key">The key of the item to remove.</param>
    /// <returns>
    /// <see langword="true"/> if item was successfully removed from the 
    /// <see cref="SynchronizedHashtable{TKey,TValue}"/>; otherwise, <see langword="false"/>. This 
    /// method returns <see langword="false"/> if <paramref name="key"/> is not found in the 
    /// <see cref="SynchronizedHashtable{TKey,TValue}"/>. 
    /// </returns>
    public bool Remove(TKey key)
    {
      int hash = (KeyComparer.GetHashCode(key) & int.MaxValue) % _buckets.Length;

      lock (_writeLock)
      {
        var previousNode = (Node)null;
        var currentNode = _buckets[hash];
        while (currentNode != null)
        {
          if (KeyComparer.Equals(currentNode.Key, key))
          {
            if (previousNode == null)
              _buckets[hash] = currentNode.Next;
            else
              previousNode.Next = currentNode.Next;

            return true;
          }

          previousNode = currentNode;
          currentNode = currentNode.Next;
        }
      }

      return false;
    }


    /// <summary>
    /// Gets the value associated with the specified key (without locking).
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the value parameter. This parameter is passed 
    /// uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="SynchronizedHashtable{TKey,TValue}"/> contains an 
    /// item with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(TKey key, out TValue value)
    {
      int hash = (KeyComparer.GetHashCode(key) & int.MaxValue) % _buckets.Length;
      var node = _buckets[hash];
#if NETFX_CORE || NET45
      Interlocked.MemoryBarrier();
#else
      Thread.MemoryBarrier();
#endif

      while (node != null)
      {
        if (KeyComparer.Equals(node.Key, key))
        {
          value = node.Value;
          return true;
        }

        node = node.Next;
      }

      value = default(TValue);
      return false;
    }


    #region ----- IEnumerable -----
        
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
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
      return GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }
    #endregion

    #endregion
  }
}
