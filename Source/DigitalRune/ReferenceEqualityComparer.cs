// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune
{
  /// <summary>
  /// Compares objects for equality by checking whether the specified <see cref="object"/> instances 
  /// are the same instance.
  /// </summary>
  /// <typeparam name="T">The type of the objects to compare.</typeparam>
  public class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
  {
    // ReSharper disable StaticFieldInGenericType

    /// <summary>
    /// Gets a default <see cref="ReferenceEqualityComparer{T}"/> for the type specified by the 
    /// generic argument. 
    /// </summary>
    /// <value>
    /// The default instance of the <see cref="ReferenceEqualityComparer{T}"/> class for type 
    /// <typeparamref name="T"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static ReferenceEqualityComparer<T> Default
    {
      get
      {
        if (_defaultComparer == null)
          _defaultComparer = new ReferenceEqualityComparer<T>();

        return _defaultComparer;
      }
    }
    private static ReferenceEqualityComparer<T> _defaultComparer;
    // ReSharper restore StaticFieldInGenericType


    /// <overloads>
    /// <summary>
    /// Determines whether this <see cref="ReferenceEqualityComparer{T}"/> is equal to another 
    /// object or whether two objects of type <typeparamref name="T"/> are equal.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">
    /// The first object of type <typeparamref name="T"/> to compare.
    /// </param>
    /// <param name="y">
    /// The second object of type <typeparamref name="T"/> to compare.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified objects are equal; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(T x, T y)
    {
      return ReferenceEquals(x, y);
    }


    /// <overloads>
    /// <summary>
    /// Returns a hash code for this <see cref="ReferenceEqualityComparer{T}"/> or an object of type
    /// <typeparamref name="T"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="ArgumentNullException">
    /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is 
    /// <see langword="null"/>.
    /// </exception>
    public int GetHashCode(T obj)
    {
      if (obj == null)
        throw new ArgumentNullException("obj");

      return obj.GetHashCode();
    }
  }
}
