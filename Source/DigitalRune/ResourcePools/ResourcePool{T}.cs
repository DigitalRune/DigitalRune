// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;
using DigitalRune.Collections;
using DigitalRune.Threading;



namespace DigitalRune
{
  /// <summary>
  /// Manages a pool of typed, reusable items. (Thread-safe)
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  /// <remarks>
  /// <strong>Thread-Safety:</strong>
  /// The <see cref="ResourcePool{T}"/> is thread-safe. It is safe to access a resource pool from 
  /// multiple threads simultaneously. 
  /// </remarks>
  /// <inheritdoc cref="ResourcePool"/>
  public class ResourcePool<T> : ResourcePool where T : class
  {
    // References:
    // - Interesting implementation of a fixed-size resource pool
    //   https://github.com/mono/monkeywrench/blob/master/ServiceStack/src/ServiceStack.Common/Net30/ObjectPool.cs


#if (NETFX_CORE || (!SILVERLIGHT && !WP7 && !XBOX && !UNITY))

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly SynchronizedHashtable<int, WorkStealingQueue<T>> _queues;
    private readonly Func<T> _create;
    private readonly Action<T> _initialize;
    private readonly Action<T> _uninitialize;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool{T}"/> class with the given
    /// un-/initialize methods.
    /// </summary>
    /// <param name="create">
    /// The function that creates a new item of type <typeparamref name="T"/>.
    /// </param>
    /// <param name="initialize">
    /// The method that is executed on an item when it is obtained from the pool - can be
    /// <see langword="null"/>.
    /// </param>
    /// <param name="uninitialize">
    /// The method that is executed on an item when it is recycled - can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="create"/> is <see langword="null"/>.
    /// </exception>
    public ResourcePool(Func<T> create, Action<T> initialize, Action<T> uninitialize)
    {
      if (create == null)
        throw new ArgumentNullException("create");

      _create = create;
      _initialize = initialize;
      _uninitialize = uninitialize;

      _queues = new SynchronizedHashtable<int, WorkStealingQueue<T>>(Environment.ProcessorCount * 4);

      Register(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override void Clear()
    {
      _queues.Clear();
    }


    /// <summary>
    /// Obtains a new item by reusing an instance from the pool or by creating a new instance if 
    /// necessary.
    /// </summary>
    /// <returns>The item.</returns>
    public T Obtain()
    {
      // Re-use existing item or create a new item.
      T item = null;

      if (Enabled)
      {
        // Get queue that pools items for the current thread.
#if NETFX_CORE || NET45
        int threadId = Environment.CurrentManagedThreadId;
#else
        int threadId = Thread.CurrentThread.ManagedThreadId;
#endif
        WorkStealingQueue<T> queue;
        if (_queues.TryGet(threadId, out queue))
        {
          // Take item from local queue (lock-free).
          if (!queue.LocalPop(ref item))
          {
            // No item on local queue.
            foreach (var entry in _queues)
            {
              if (entry.Key != threadId)
              {
                // Try to steal item from other threads.
                // (Note: Timeout is 0. Do not wait for lock. Try next queue if lock is unavailable.)
                if (entry.Value.TrySteal(ref item, 0))
                  break;
              }
            }
          }
        }
        else
        {
          // No local queue.
          foreach (var entry in _queues)
          {
            if (entry.Key != threadId)
            {
              // Try to steal item from other threads.
              // (Note: Timeout is 0. Do not wait for lock. Try next queue if lock is unavailable.)
              if (entry.Value.TrySteal(ref item, 0))
                break;
            }
          }
        }
      }

      if (item == null)
        item = _create();

      // Initialize item if necessary.
      if (_initialize != null)
        _initialize(item);

      return item;
    }


    /// <summary>
    /// Recycles the given item and places it back in the pool for future reuse.
    /// </summary>
    /// <param name="item">The item to be recycled.</param>
    public void Recycle(T item)
    {
      Debug.Assert(item != null, "ResourcePool.Recycle(item) should not be called with null.");

      // Reset item if necessary.
      if (_uninitialize != null)
        _uninitialize(item);

      if (Enabled)
      {
        // Get queue that pools items for the current thread.
#if NETFX_CORE || NET45
        int threadId = Environment.CurrentManagedThreadId;
#else
        int threadId = Thread.CurrentThread.ManagedThreadId;
#endif
        WorkStealingQueue<T> queue;
        if (!_queues.TryGet(threadId, out queue))
        {
          // This is the first time that the current thread recycles an item of type T.
          // Start with a fresh, empty queue and register the queue for the current thread.
          queue = new WorkStealingQueue<T>();
          _queues.Add(threadId, queue);
        }

        queue.LocalPush(item);
      }
    }
    #endregion

#elif (SILVERLIGHT || WP7 || XBOX || UNITY)

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly FastStack<T> _stack;
    private readonly Func<T> _create;
    private readonly Action<T> _initialize;
    private readonly Action<T> _uninitialize;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourcePool{T}"/> class with the given
    /// un-/initialize methods.
    /// </summary>
    /// <param name="create">
    /// The function that creates a new item of type <typeparamref name="T"/>.
    /// </param>
    /// <param name="initialize">
    /// The method that is executed on an item when it is obtained from the pool - can be
    /// <see langword="null"/>.
    /// </param>
    /// <param name="uninitialize">
    /// The method that is executed on an item when it is recycled - can be <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="create"/> is <see langword="null"/>.
    /// </exception>
    public ResourcePool(Func<T> create, Action<T> initialize, Action<T> uninitialize)
    {
      if (create == null)
        throw new ArgumentNullException("create");

      _create = create;
      _initialize = initialize;
      _uninitialize = uninitialize;
      _stack = new FastStack<T>();

      Register(this);
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <inheritdoc/>
    public override void Clear()
    {
      if (_stack.Count > 0)   // Check to avoid unnecessary locking.
      {
        lock (_stack)
        {
          _stack.Clear();
        }
      }
    }


    /// <summary>
    /// Obtains a new item by reusing an instance from the pool or by creating a new instance if 
    /// necessary.
    /// </summary>
    /// <returns>The item.</returns>
    public T Obtain()
    {
      // Re-use existing item or create a new item.
      T item = null;

      if (Enabled)
      {
        lock (_stack)
        {
          item = _stack.Pop();
        }
      }

      if (item == null)
        item = _create();

      // Initialize item if necessary.
      if (_initialize != null)
        _initialize(item);

      return item;
    }


    /// <summary>
    /// Recycles the given item and places it back in the pool for future reuse.
    /// </summary>
    /// <param name="item">The item to be recycled.</param>
    public void Recycle(T item)
    {
      Debug.Assert(item != null, "ResourcePool.Recycle(item) should not be called with null.");

      // Reset item if necessary.
      if (_uninitialize != null)
        _uninitialize(item);

      if (Enabled)
      {
        lock (_stack)
        {
          Debug.Assert(!_stack.Contains(item), "Cannot recycle item. Item is already in the resource pool.");
          _stack.Push(item);
        }
      }
    }
    #endregion

#endif
  }
}
