// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Stores two ordered related objects.
  /// </summary>
  /// <typeparam name="TFirst">The type of the first object.</typeparam>
  /// <typeparam name="TSecond">The type of the second object.</typeparam>
  /// <remarks>
  /// <see cref="Pair{TFirst,TSecond}"/> overloads the method 
  /// <see cref="Equals(Pair{TFirst,TSecond})"/> and the equality operators. Two 
  /// <see cref="Pair{TFirst,TSecond}"/> objects are considered as equal if they contain the same
  /// objects in the same order. If <typeparamref name="TFirst"/> and <typeparamref name="TSecond"/>
  /// are the same type and the order of the objects does not matter, use <see cref="Pair{T}"/>
  /// instead of <see cref="Pair{TFirst,TSecond}"/>.
  /// </remarks>
#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE
  [Serializable]
#endif
  public struct Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // ReSharper disable StaticFieldInGenericType
    private static readonly EqualityComparer<TFirst> EqualityComparer1 = EqualityComparer<TFirst>.Default;
    private static readonly EqualityComparer<TSecond> EqualityComparer2 = EqualityComparer<TSecond>.Default;
    // ReSharper restore StaticFieldInGenericType
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets or sets the first object.
    /// </summary>
    /// <value>The first object.</value>
    public TFirst First
    {
      get { return _first; }
      set { _first = value; }
    }
    private TFirst _first;


    /// <summary>
    /// Gets or sets the second object.
    /// </summary>
    /// <value>The second object.</value>
    public TSecond Second
    {
      get { return _second; }
      set { _second = value; }
    }
    private TSecond _second;
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="Pair{TFirst, TSecond}"/> class with the given
    /// objects.
    /// </summary>
    /// <param name="first">The first object.</param>
    /// <param name="second">The second object.</param>
    public Pair(TFirst first, TSecond second)
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
      return obj is Pair<TFirst, TSecond> && this == (Pair<TFirst, TSecond>)obj;
    }


    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns>
    /// <see langword="true"/> if the current object is equal to the other parameter; otherwise, 
    /// <see langword="false"/>.
    /// </returns>
    public bool Equals(Pair<TFirst, TSecond> other)
    {
      return EqualityComparer1.Equals(First, other.First)
             && EqualityComparer2.Equals(Second, other.Second);
    }


    /// <summary>
    /// Compares two <see cref="Pair{TFirst,TSecond}"/> objects to determine whether they are the 
    /// same.
    /// </summary>
    /// <param name="pair1">The first pair.</param>
    /// <param name="pair2">The second pair.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="pair1"/> and <paramref name="pair2"/> are the 
    /// same; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator ==(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
    {
      return EqualityComparer1.Equals(pair1.First, pair2.First)
             && EqualityComparer2.Equals(pair1.Second, pair2.Second);
    }


    /// <summary>
    /// Compares two <see cref="Pair{TFirst,TSecond}"/> objects to determine whether they are 
    /// different.
    /// </summary>
    /// <param name="pair1">The first pair.</param>
    /// <param name="pair2">The second pair.</param>
    /// <returns>
    /// <see langword="true"/> if the <paramref name="pair1"/> and <paramref name="pair2"/> are 
    /// different; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool operator !=(Pair<TFirst, TSecond> pair1, Pair<TFirst, TSecond> pair2)
    {
      return (!EqualityComparer1.Equals(pair1.First, pair2.First)
               || !EqualityComparer2.Equals(pair1.Second, pair2.Second));
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
        int hashCode = EqualityComparer1.GetHashCode(First);
        hashCode = (hashCode * 397) ^ EqualityComparer2.GetHashCode(Second);
        return hashCode;
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
