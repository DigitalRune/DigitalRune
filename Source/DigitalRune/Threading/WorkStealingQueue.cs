// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Threading;


namespace DigitalRune.Threading
{
  /// <summary>
  /// A special kind of queue in that it has two ends, and allows lock-free pushes and pops from one 
  /// end ("private"), but requires synchronization from the other end ("public").
  /// </summary>
  /// <typeparam name="T">The type of item stored in the queue.</typeparam>
  /// <remarks>
  /// This type of queue is usually own by a thread to queue work items or other resources. The 
  /// thread can access its own queue without locking. A thread can access another thread's queue 
  /// and try to steal items. Accessing the queue of another thread requires synchronization which
  /// is automatically handled by the queue.
  /// </remarks>
  internal sealed class WorkStealingQueue<T>
  {
    // Implementation from Joe Duffy's blog:
    // http://www.bluebytesoftware.com/blog/2008/08/12/BuildingACustomThreadPoolSeriesPart2AWorkStealingQueue.aspx

    private const int InitialSize = 32;
    private T[] _array = new T[InitialSize];              // The size must always be a power of two, because we use a bit mask.
    private int _mask = InitialSize - 1;
    private volatile int _headIndex;                      // The public end.
    private volatile int _tailIndex;                      // The private end.
    private readonly object _foreignLock = new object();  // Synchronizes access at the public end.


    /// <summary>
    /// Gets a value indicating whether this queue is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this queue is empty; otherwise, <see langword="false"/>.
    /// </value>
    public bool IsEmpty
    {
      get { return _headIndex >= _tailIndex; }
    }


    /// <summary>
    /// Gets the number of items in the queue.
    /// </summary>
    /// <value>The number of items in the queue.</value>
    public int Count
    {
      get { return _tailIndex - _headIndex; }
    }


    /// <summary>
    /// Adds an item to the "private" end of the queue.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    public void LocalPush(T item)
    {
      int tail = _tailIndex;
      if (tail < _headIndex + _mask)
      {
        _array[tail & _mask] = item;
        _tailIndex = tail + 1;
      }
      else
      {
        lock (_foreignLock)
        {
          int head = _headIndex;
          int count = _tailIndex - _headIndex;

          if (count >= _mask)
          {
            T[] newArray = new T[_array.Length << 1];
            for (int i = 0; i < count; i++)
              newArray[i] = _array[(i + head) & _mask];

            // Reset the field values, incl. the mask.
            _array = newArray;
            _headIndex = 0;
            _tailIndex = tail = count;
            _mask = (_mask << 1) | 1;
          }
          _array[tail & _mask] = item;
          _tailIndex = tail + 1;
        }
      }
    }


    /// <summary>
    /// Tries to removes an item from the "private" end of the queue.
    /// </summary>
    /// <param name="item">The item that is removed from the queue.</param>
    /// <returns>
    /// <see langword="true"/> if an item was successfully removed; otherwise, 
    /// <see langword="false"/> if the queue is empty.
    /// </returns>
    public bool LocalPop(ref T item)
    {
      int tail = _tailIndex;
      if (_headIndex >= tail)
      {
        return false;
      }

#pragma warning disable 0420

      tail -= 1;
      Interlocked.Exchange(ref _tailIndex, tail);

      if (_headIndex <= tail)
      {
        item = _array[tail & _mask];
        return true;
      }
      else
      {
        lock (_foreignLock)
        {
          if (_headIndex <= tail)
          {
            // Element still available. Take it.
            item = _array[tail & _mask];
            return true;
          }
          else
          {
            // We lost the race, element was stolen, restore the tail.
            _tailIndex = tail + 1;
            return false;
          }
        }
      }
    }


    /// <summary>
    /// Tries the steal an item from the "public" end of the queue.
    /// </summary>
    /// <param name="item">The item removed from the queue.</param>
    /// <param name="millisecondsTimeout">
    /// The number of millisecond to wait for a lock. If the value equals 
    /// <see cref="Timeout.Infinite"/>, this method blocks until the lock is acquired. If the value
    /// equals 0, this method immediately returns without blocking if the lock cannot be acquired.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if an item was successfully removed; otherwise,
    /// <see langword="false"/> if the queue is empty.
    /// </returns>
    public bool TrySteal(ref T item, int millisecondsTimeout)
    {
      bool taken = false;
      try
      {
        taken = Monitor.TryEnter(_foreignLock, millisecondsTimeout);
        if (taken)
        {
          int head = _headIndex;
          Interlocked.Exchange(ref _headIndex, head + 1);
          if (head < _tailIndex)
          {
            item = _array[head & _mask];
            return true;
          }
          else
          {
            _headIndex = head;
            return false;
          }
        }
      }
      finally
      {
        if (taken)
          Monitor.Exit(_foreignLock);
      }

      return false;
    }
  }
}
