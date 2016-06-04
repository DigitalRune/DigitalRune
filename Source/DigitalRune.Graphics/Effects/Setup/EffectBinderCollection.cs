// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Graphics.Effects
{
  /// <summary>
  /// Manages a collection of <see cref="IEffectBinder"/>s.
  /// </summary>
  /// <remarks>
  /// Null entries or duplicate entries are not allowed in this collection.
  /// </remarks>
  public class EffectBinderCollection : Collection<IEffectBinder>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="EffectBinderCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> for <see cref="EffectBinderCollection"/>.
    /// </returns>
    public new List<IEffectBinder>.Enumerator GetEnumerator()
    {
      return ((List<IEffectBinder>)Items).GetEnumerator();
    }


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. 
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. 
    /// </exception>
    protected override void InsertItem(int index, IEffectBinder item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "null items are not allowed in this collection.");

      if (Contains(item))
        throw new ArgumentException("Duplicate items are not allowed in this collection.");

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. 
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. 
    /// </exception>
    protected override void SetItem(int index, IEffectBinder item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "null items are not allowed in this collection.");
      
      var oldIndex = IndexOf(item);
      if (oldIndex >= 0 && oldIndex != index)
        throw new ArgumentException("Duplicate items are not allowed in this collection.");

      base.SetItem(index, item);
    }
  }
}
