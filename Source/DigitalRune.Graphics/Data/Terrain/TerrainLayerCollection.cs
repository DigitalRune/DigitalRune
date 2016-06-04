// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages the layers of a terrain tile.
  /// </summary>
  public class TerrainLayerCollection
    : Collection<TerrainLayer>,
      ICollection<TerrainLayer>  // The interface is necessary for the VS class diagrams!
  {
    /// <summary>
    /// Gets the terrain tile that owns this collection.
    /// </summary>
    /// <value>The terrain tile that own this collection.</value>
    public TerrainTile Parent { get; private set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="TerrainLayerCollection"/> class.
    /// </summary>
    /// <param name="parent">The terrain tile that owns this collection.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="parent"/> is <see langword="null"/>.
    /// </exception>
    public TerrainLayerCollection(TerrainTile parent)
    {
      if (parent == null)
        throw new ArgumentNullException("parent");

      Parent = parent;
    }


    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="TerrainLayerCollection"/>.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> for <see cref="TerrainLayerCollection"/>.</returns>
    public new List<TerrainLayer>.Enumerator GetEnumerator()
    {
      return ((List<TerrainLayer>)Items).GetEnumerator();
    }


    /// <summary>
    /// Removes all elements from the <see cref="Collection{T}"/>.
    /// </summary>
    protected override void ClearItems()
    {
      base.ClearItems();
      Parent.Invalidate();
    }


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void InsertItem(int index, TerrainLayer item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a terrain layer collection must not be null.");

      base.InsertItem(index, item);
      Parent.Invalidate(item);
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      var item = Items[index];
      base.RemoveItem(index);
      Parent.Invalidate(item);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    protected override void SetItem(int index, TerrainLayer item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a terrain layer collection must not be null.");

      base.SetItem(index, item);
      Parent.Invalidate(item);
    }
  }
}
