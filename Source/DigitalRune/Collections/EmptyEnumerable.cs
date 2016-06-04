// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides an implementation for <see cref="IEnumerable{T}"/> and <see cref="IEnumerator{T}"/> 
  /// for an empty collection.
  /// </summary>
  /// <typeparam name="T">The type of objects to enumerate.</typeparam>
 [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  internal sealed class EmptyEnumerable<T> : Singleton<EmptyEnumerable<T>>,  IEnumerable<T>, IEnumerator<T>
  {
    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    object IEnumerator.Current
    {
      get { return default(T); }
    }


    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    T IEnumerator<T>.Current
    {
      get { return default(T); }
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EmptyEnumerable{T}"/> class.
    /// </summary>
    public EmptyEnumerable()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting 
    /// unmanaged resources.
    /// </summary>
    void IDisposable.Dispose()
    {
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<T>)this).GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// This enumerable/enumerator has already been disposed, or the enumerator is already in use.
    /// </exception>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return this;
    }


    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
    /// <see langword="false"/> if the enumerator has passed the end of the collection.
    /// </returns>
    bool IEnumerator.MoveNext()
    {
      return false;
    }


    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first element in the collection.
    /// </summary>
    void IEnumerator.Reset()
    {
    }
  }
}
