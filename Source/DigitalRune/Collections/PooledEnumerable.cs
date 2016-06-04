// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides a base implementation for an <see cref="IEnumerable{T}"/> and an
  /// <see cref="IEnumerator{T}"/> supporting resource pooling. (For internal use only.)
  /// </summary>
  /// <typeparam name="T">The type of objects to enumerate.</typeparam>
  /// <remarks>
  /// A <see cref="PooledEnumerable{T}"/> object can only be enumerated once. When the enumeration
  /// is finished and <see cref="IDisposable.Dispose"/> is called, the object is automatically 
  /// recycled.
  /// </remarks>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
  public abstract class PooledEnumerable<T> : IEnumerable<T>, IEnumerator<T>
  {
    // This base class implements IEnumerable<T> and IEnumerator<T>. It can be enumerated exactly
    // once! It is not allowed to get more than 1 enumerator or to enumerate several times.
    // When the enumerator is disposed, the OnRecycle is called which should be used to clean and 
    // recycle the whole object.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private int _state; // 0 = in pool, 1 = in use, 2 = in use and enumerator was given out.
    private T _current;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    object IEnumerator.Current
    {
      get { return _current; }
    }


    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <value>The element in the collection at the current position of the enumerator.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    T IEnumerator<T>.Current
    {
      get { return _current; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// This enumerable/enumerator is already in use.
    /// </exception>
    public void Initialize()
    {
      if (_state > 0)
        throw new InvalidOperationException("This enumerable/enumerator is already in use.");

      _state = 1;
    }


    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting 
    /// unmanaged resources.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
    void IDisposable.Dispose()
    {
      if (_state == 0)
      {
        // The enumerable/enumerator has already been disposed.
        return;
      }

      // Reset flag. (This must be done before OnRecycle is called because when this instance
      // is recycled in OnRecycle it might already be reused before OnRecycle returns!)
      _state = 0;

      _current = default(T);
      OnRecycle();
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      if (_state == 0)
        throw new InvalidOperationException("This enumerable/enumerator has already been disposed.");
      if (_state == 2)
        throw new InvalidOperationException("This enumerator is already in use. GetEnumerator() of this IEnumerable can only be called once.");

      return this;
    }


    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
    /// <see langword="false"/> if the enumerator has passed the end of the collection.
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IEnumerator.MoveNext()
    {
      return OnNext(out _current);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <see cref="IEnumerator.Reset"/> is not supported.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    void IEnumerator.Reset()
    {
      throw new NotSupportedException("IEnumerator.Reset() is not supported.");
    }


    /// <summary>
    /// Called when the enumerator should move to the next object.
    /// </summary>
    /// <param name="current">The next object.</param>
    /// <returns>
    /// <see langword="true"/> if the enumerator was successfully advanced to the next element; 
    /// <see langword="false"/> if the enumerator has passed the end of the collection. 
    /// </returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
    protected abstract bool OnNext(out T current);


    /// <summary>
    /// Called when this instance should be recycled.
    /// </summary>
    protected abstract void OnRecycle();
    #endregion
  }
}
