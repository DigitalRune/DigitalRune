// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using DigitalRune.Graphics.Effects;


namespace DigitalRune.Graphics
{
  partial class Material
  {
    // Explicit interface implementations.

    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this <see cref="ICollection{T}"/> is read-only; otherwise, 
    /// <see langword="false"/>.
    /// </value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool ICollection<KeyValuePair<string, EffectBinding>>.IsReadOnly
    {
      get { return false; }
    }

    
    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the keys of the 
    /// <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <value>
    /// An <see cref="ICollection{T}"/> containing the keys of the object that implements 
    /// <see cref="IDictionary{TKey,TValue}"/>.
    /// </value>
    /// <remarks>
    /// The order of the keys in the returned <see cref="ICollection{T}"/> is unspecified, but it is
    /// guaranteed to be the same order as the corresponding values in the 
    /// <see cref="ICollection{T}"/> returned by the <see cref="IDictionary{TKey,TValue}.Values"/> 
    /// property.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    ICollection<string> IDictionary<string, EffectBinding>.Keys
    {
      get { return _bindingsPerPass.Keys; }
    }


    /// <summary>
    /// Gets an <see cref="ICollection{T}"/> containing the values of the 
    /// <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <value>
    /// An <see cref="ICollection{T}"/> containing the values of the object that implements 
    /// <see cref="IDictionary{TKey,TValue}"/>.
    /// </value>
    /// <remarks>
    /// The order of the values in the returned <see cref="ICollection{T}"/> is unspecified, but it 
    /// is guaranteed to be the same order as the corresponding keys in the 
    /// <see cref="ICollection{T}"/> returned by the <see cref="IDictionary{TKey,TValue}.Keys"/> 
    /// property.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    ICollection<EffectBinding> IDictionary<string, EffectBinding>.Values
    {
      get { return _bindingsPerPass.Values; }
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
      return _bindingsPerPass.GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator<KeyValuePair<string, EffectBinding>> IEnumerable<KeyValuePair<string, EffectBinding>>.GetEnumerator()
    {
      return _bindingsPerPass.GetEnumerator();
    }


    /// <summary>
    /// Adds an item to the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
    void ICollection<KeyValuePair<string, EffectBinding>>.Add(KeyValuePair<string, EffectBinding> item)
    {
      if (item.Value == null)
        throw new ArgumentException("The effect binding must not be null.", "item");

      ((ICollection<KeyValuePair<string, EffectBinding>>)_bindingsPerPass).Add(item);
    }


    /// <summary>
    /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> is found in the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    bool ICollection<KeyValuePair<string, EffectBinding>>.Contains(KeyValuePair<string, EffectBinding> item)
    {
      return ((ICollection<KeyValuePair<string, EffectBinding>>)_bindingsPerPass).Contains(item);
    }


    /// <summary>
    /// Determines whether the <see cref="IDictionary{TKey,TValue}"/> contains an element with the 
    /// specified key.
    /// </summary>
    /// <param name="key">The key to locate in the <see cref="IDictionary{TKey,TValue}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="IDictionary{TKey,TValue}"/> contains an element 
    /// with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IDictionary<string, EffectBinding>.ContainsKey(string key)
    {
      return _bindingsPerPass.ContainsKey(key);
    }


    /// <summary>
    /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>, starting at 
    /// a particular array index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from 
    /// <see cref="ICollection{T}"/>. The array must have zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of elements in the source <see cref="ICollection{T}"/> is greater than the 
    /// available space from <paramref name="arrayIndex"/> to the end of the destination 
    /// <paramref name="array"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    void ICollection<KeyValuePair<string, EffectBinding>>.CopyTo(KeyValuePair<string, EffectBinding>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, EffectBinding>>)_bindingsPerPass).CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the 
    /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also returns 
    /// <see langword="false"/> if <paramref name="item"/> is not found in the original 
    /// <see cref="ICollection{T}"/>.
    /// </returns>
    bool ICollection<KeyValuePair<string, EffectBinding>>.Remove(KeyValuePair<string, EffectBinding> item)
    {
      return ((ICollection<KeyValuePair<string, EffectBinding>>)_bindingsPerPass).Remove(item);
    }


    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">
    /// When this method returns, the value associated with the specified key, if the key is found; 
    /// otherwise, the default value for the type of the <paramref name="value"/> parameter. This 
    /// parameter is passed uninitialized.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the object that implements <see cref="IDictionary{TKey,TValue}"/> 
    /// contains an element with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is <see langword="null"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
    bool IDictionary<string, EffectBinding>.TryGetValue(string key, out EffectBinding value)
    {
      return _bindingsPerPass.TryGetValue(key, out value);
    }
    #endregion
  }
}
