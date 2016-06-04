// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Stores two unordered related objects.
  /// </summary>
  /// <typeparam name="T">The type of the contained objects.</typeparam>
  /// <remarks>
  /// <see cref="Pair{T}"/> overloads the method <see cref="Equals(Pair{T})"/> and the equality
  /// operators. Two <see cref="Pair{T}"/> objects are considered as equal if they contain the same
  /// objects. The order of the objects does not matter. If the order of the objects is relevant,
  /// use <see cref="Pair{TFirst, TSecond}"/> instead of <see cref="Pair{T}"/>.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct Pair<T> : IEquatable<Pair<T>>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly EqualityComparer<T> EqualityComparer = EqualityComparer<T>.Default;
    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first object.
    /// </summary>
    /// <value>The first object.</value>
    public T First
    {
      get { return _first; }
      set { _first = value; }
    }
    private T _first;


    /// <summary>
    /// Gets or sets the second object.
    /// </summary>
    /// <value>The second object.</value>
    public T Second
    {
      get { return _second; }
      set { _second = value; }
    }
    private T _second;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Pair{T}"/> class with the given objects.
    /// </summary>
    /// <param name="first">The first object.</param>
    /// <param name="second">The second object.</param>
    public Pair(T first, T second)
    {
      _first = first;
      _second = second;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Determines whether the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>.
    /// </summary>
    /// <param name="obj">
    /// The <see cref="Object"/> to compare with the current <see cref="Object"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
    /// <see cref="Object"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public override bool Equals(object obj)
    {
      return obj is Pair<T> && this == (Pair<T>)obj;
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Pair<T> other)
    {
      return (EqualityComparer.Equals(First, other.First)
              && EqualityComparer.Equals(Second, other.Second))
             || (EqualityComparer.Equals(First, other.Second)
                 && EqualityComparer.Equals(Second, other.First));
    }


    /// <summary>
    /// Compares two <see cref="Pair{T}"/> objects to determine whether they are the 
    /// same.
    /// </summary>
    /// <param name="pair1">The first pair.</param>
    /// <param name="pair2">The second pair.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="pair1"/> and <paramref name="pair2"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Pair<T> pair1, Pair<T> pair2)
    {
      return (EqualityComparer.Equals(pair1.First, pair2.First)
              && EqualityComparer.Equals(pair1.Second, pair2.Second))
             || (EqualityComparer.Equals(pair1.First, pair2.Second)
                 && EqualityComparer.Equals(pair1.Second, pair2.First));
    }


    /// <summary>
    /// Compares two <see cref="Pair{T}"/> objects to determine whether they are 
    /// different.
    /// </summary>
    /// <param name="pair1">The first pair.</param>
    /// <param name="pair2">The second pair.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="pair1"/> and <paramref name="pair2"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Pair<T> pair1, Pair<T> pair2)
    {
      return !((EqualityComparer.Equals(pair1.First, pair2.First) && EqualityComparer.Equals(pair1.Second, pair2.Second))
              || (EqualityComparer.Equals(pair1.First, pair2.Second) && EqualityComparer.Equals(pair1.Second, pair2.First)));
    }


    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    /// <returns>
    /// A 32-bit signed integer that is the hash code for this instance.
    /// </returns>
    public override int GetHashCode()
    {
      unchecked
      {
        int hash1 = EqualityComparer.GetHashCode(First);
        int hash2 = EqualityComparer.GetHashCode(Second);
        if (hash1 != hash2)
          return hash1 ^ hash2;

        return hash1;    // Return only hash1. Because hash1 ^ hash2 would be 0!!!
      }
    }


    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents the current <see cref="Object"/>.
    /// </returns>
    public override string ToString()
    {
      return String.Format(CultureInfo.InvariantCulture, "({0}; {1})", First, Second);
    }
    #endregion
  }
}
