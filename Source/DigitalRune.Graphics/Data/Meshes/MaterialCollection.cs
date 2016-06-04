// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of materials.
  /// </summary>
  /// <remarks>
  /// Items in this collection must not be <see langword="null"/>.
  /// </remarks>
  public class MaterialCollection : Collection<Material>
  {
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="MaterialCollection"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> for <see cref="MaterialCollection"/>.
    /// </returns>
    public new List<Material>.Enumerator GetEnumerator()
    {
      return ((List<Material>)Items).GetEnumerator();
    }


    /// <summary>
    /// Inserts a material into the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The material to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, Material item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Null values are not allowed in a MaterialCollection.");

      base.InsertItem(index, item);
    }


    /// <summary>
    /// Replaces the material at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the material to replace.</param>
    /// <param name="item">The new value for the material at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, Material item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Null values are not allowed in a MaterialCollection.");

      base.SetItem(index, item);
    }
  }
}
