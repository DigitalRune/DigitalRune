// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Compares two items using a user-defined <see cref="Comparison{T}"/> delegate. (Note: In .NET
  /// 4.5 or higher use <strong>Comparer&lt;T&gt;.Create()</strong> instead of this class.)
  /// </summary>
  /// <typeparam name="T">The type of objects to compare</typeparam>
  public sealed class DelegateComparer<T> : IComparer<T>
  {
    private readonly Comparison<T> _delegate;


    /// <summary>
    /// Initializes a new instance of the <see cref="DelegateComparer{T}" /> class.
    /// </summary>
    /// <param name="comparison">The comparison.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="comparison"/> is <see langword="null"/>.
    /// </exception>
    public DelegateComparer(Comparison<T> comparison)
    {
      if (comparison == null)
        throw new ArgumentNullException("comparison");

      _delegate = comparison;
    }


    /// <summary>
    /// Compares two objects and returns a value indicating whether one is less than, equal to, or
    /// greater than the other.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// A signed integer that indicates the relative values of <paramref name="x"/> and 
    /// <paramref name="y"/>. The result is less than 0 if <paramref name="x"/> is less than 
    /// <paramref name="y"/>. The result is 0 if <paramref name="x"/> is equal to 
    /// <paramref name="y"/>. The result is greater than 0 if <paramref name="x"/> is greater than 
    /// <paramref name="y"/>.
    /// </returns>
    public int Compare(T x, T y)
    {
      return _delegate(x, y);
    }
  }
}
