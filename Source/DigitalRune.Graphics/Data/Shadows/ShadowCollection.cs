// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of <see cref="Shadow"/>s.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Duplicates items, <see langword="null"/> and <see cref="CompositeShadow"/>s are not allowed
  /// in the collection.
  /// </para>
  /// </remarks>
  public class ShadowCollection : Collection<Shadow>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ShadowCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="ShadowCollection"/>.
    /// </returns>
    public new List<Shadow>.Enumerator GetEnumerator()
    {
      return ((List<Shadow>)Items).GetEnumerator();
    }


    /// <summary>
    /// Inserts an element into the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. The collection does not allow 
    /// <see langword="null"/> values.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is a <see cref="CompositeShadow"/>. <see cref="CompositeShadow"/>s
    /// cannot be added to a shadow collection.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. The collection does not 
    /// allow duplicate items.
    /// </exception>
    protected override void InsertItem(int index, Shadow item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a shadow collection must not be null.");
      if (item is CompositeShadow)
        throw new ArgumentException("CompositeShadows cannot be added to a shadow collection.", "item");
      if (Contains(item))
        throw new ArgumentException("Duplicate items are not allowed in the shadow collection.", "item");

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. The collection does not allow 
    /// <see langword="null"/> values.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is a <see cref="CompositeShadow"/>. <see cref="CompositeShadow"/>s
    /// cannot be added to a shadow collection.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. The collection does not 
    /// allow duplicate items.
    /// </exception>
    protected override void SetItem(int index, Shadow item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a shadow collection must not be null.");
      if (item is CompositeShadow)
        throw new ArgumentException("CompositeShadows cannot be added to a shadow collection.", "item");

      var removedItem = Items[index];
      if (removedItem == item)
        return;

      if (Contains(item))
        throw new ArgumentException("Duplicate items are not allowed in the shadow collection.", "item");
      
      base.SetItem(index, item);
    }
  }
}
