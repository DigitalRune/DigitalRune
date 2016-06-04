// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides a read-only wrapper around a <see cref="WeakCollection{T}"/>.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the collection.</typeparam>
  public sealed class ReadOnlyWeakCollection<T> : ICollection<T>, ICollection where T : class
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly WeakCollection<T> _weakCollection;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of items contained in the <see cref="ReadOnlyWeakCollection{T}"/>.
    /// </summary>
    /// <value>The number of items contained in the <see cref="ReadOnlyWeakCollection{T}"/>.</value>
    public int Count
    {
      get { return _weakCollection.Count; }
    }


    /// <summary>
    /// Gets a value indicating whether the collection is read only. Always returns 
    /// <see langword="false"/>.
    /// </summary>
    bool ICollection<T>.IsReadOnly
    {
      get { return true; }
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
      get { return ((ICollection)_weakCollection).SyncRoot; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadOnlyWeakCollection{T}"/> class.
    /// </summary>
    /// <param name="weakCollection">The weak collection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="weakCollection"/> is <see langword="null"/>.
    /// </exception>
    public ReadOnlyWeakCollection(WeakCollection<T> weakCollection)
    {
      if (weakCollection == null)
        throw new ArgumentNullException("weakCollection");

      _weakCollection = weakCollection;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return _weakCollection.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _weakCollection.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
    /// </returns>
    public WeakCollection<T>.Enumerator GetEnumerator()
    {
      return _weakCollection.GetEnumerator();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to be added.</param>
    /// <exception cref="NotSupportedException">
    /// This collection is read-only.
    /// </exception>
    void ICollection<T>.Add(T item)
    {
      throw new NotSupportedException("This collection is read-only.");
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// This collection is read-only.
    /// </exception>
    void ICollection<T>.Clear()
    {
      throw new NotSupportedException("This collection is read-only.");
    }


    /// <summary>
    /// Determines whether the <see cref="ReadOnlyWeakCollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ReadOnlyWeakCollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the collection contains a specific value; <see langword="false"/> 
    /// if it does not.
    /// </returns>
    public bool Contains(T item)
    {
      return _weakCollection.Contains(item);
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
    public void CopyTo(Array array, int index)
    {
      ((ICollection)_weakCollection).CopyTo(array, index);
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
      ((ICollection<T>)_weakCollection).CopyTo(array, index);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">The object to be removed.</param>
    /// <returns>.</returns>
    /// <exception cref="NotSupportedException">This collection is read-only.</exception>
    bool ICollection<T>.Remove(T item)
    {
      throw new NotSupportedException("This collection is read-only.");
    }
    #endregion
  }
}
