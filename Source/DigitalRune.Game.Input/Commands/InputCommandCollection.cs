// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Collections;


namespace DigitalRune.Game.Input
{
  /// <summary>
  /// Manages a collection of <see cref="IInputCommand"/>s.
  /// </summary>
  /// <remarks>
  /// Null items cannot be added to this collection.
  /// </remarks>
  public class InputCommandCollection : NamedObjectCollection<IInputCommand>
  {
    // This collection sets/unsets the InputCommand.InputService property.
    private IInputService InputService { get; set; }


    /// <summary>
    /// Initializes a new instance of the <see cref="InputCommandCollection"/> class.
    /// </summary>
    /// <param name="inputService">The input service.</param>
    internal InputCommandCollection(IInputService inputService)
    {
      Debug.Assert(inputService != null);
      
      InputService = inputService;
    }


    /// <summary>
    /// Removes all elements from the <see cref="InputCommandCollection"/>. 
    /// </summary>
    protected override void ClearItems()
    {
      // If we set InputService first, we assume that ClearItems does not throw an exception!
      foreach (var item in this)
        item.InputService = null;

      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the <see cref="InputCommandCollection"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0, or <paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Cannot add command to input service. The command is already added to an input service.
    /// </exception>
    protected override void InsertItem(int index, IInputCommand item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.InputService != null)
        throw new InvalidOperationException("Cannot add command to input service. The command was already added to an input service.");

      base.InsertItem(index, item);

      // Only set InputService if base method didn't throw an exception.
      item.InputService = InputService;
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="InputCommandCollection"/>.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      // Remember removal candidate.
      IInputCommand removedItem = null;
      if (0 <= index && index < Count)
        removedItem = this[index];

      base.RemoveItem(index);

      // Only set InputService if base method didn't throw an exception.
      if (removedItem != null)
        removedItem.InputService = null;
    }


    /// <summary>
    /// Replaces the item at the specified index with the specified item.
    /// </summary>
    /// <param name="index">The zero-based index of the item to be replaced.</param>
    /// <param name="item">The new item.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Cannot add command to input service. The command is already added to an input service.
    /// </exception>
    protected override void SetItem(int index, IInputCommand item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.InputService != null)
        throw new InvalidOperationException("Cannot add command to input service. The command was already added to an input service.");

      // Remember removal candidate.
      IInputCommand removedItem = null;
      if (0 <= index && index < Count)
        removedItem = this[index];

      base.SetItem(index, item);

      // Only set InputService if base method didn't throw an exception.
      if (removedItem != null)
        removedItem.InputService = null;

      item.InputService = InputService;
    }
  }
}
