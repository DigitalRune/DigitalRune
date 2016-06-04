// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Collections
{
  /// <summary>
  /// A fast implementation of a stack. No overhead, returns <see langword="null"/> instead of
  /// throwing exceptions.
  /// </summary>
  /// <typeparam name="T">The type of the items.</typeparam>
  [DebuggerDisplay("Count = {Count}")]
  internal sealed class FastStack<T> where T : class
  {
    private const int DefaultCapacity = 4;
    private const int ResizingFactor = 2;
    private T[] _buffer;
    private int _numberOfItems;


    public int Count
    {
      get { return _numberOfItems; }
    }

    
    public FastStack() : this(DefaultCapacity)
    {
    }


    public FastStack(int capacity)
    {
      _buffer = new T[capacity];
      _numberOfItems = 0;
    }


    public void Clear()
    {
      Array.Clear(_buffer, 0, _numberOfItems);
      _numberOfItems = 0;
    }


    public bool Contains(T item)
    {
      EqualityComparer<T> comparer = EqualityComparer<T>.Default;
      for (int i = 0; i < _numberOfItems; i++)
      {
        if (comparer.Equals(_buffer[i], item))
          return true;
      }

      return false;
    }


    public T Pop()
    {
      if (_numberOfItems == 0)
        return null;

      _numberOfItems--;
      T item = _buffer[_numberOfItems];
      _buffer[_numberOfItems] = default(T);
      return item;
    }


    public void Push(T item)
    {
      if (_numberOfItems == _buffer.Length)
      {
        T[] newBuffer = new T[ResizingFactor * _buffer.Length];
        Array.Copy(_buffer, 0, newBuffer, 0, _numberOfItems);
        _buffer = newBuffer;
      }

      _buffer[_numberOfItems] = item;
      _numberOfItems++;
    }
  }
}
