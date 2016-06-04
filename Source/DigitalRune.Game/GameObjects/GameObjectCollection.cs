// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Collections;


namespace DigitalRune.Game
{
  /// <summary>
  /// Manages a collection of game objects.
  /// </summary>
  public class GameObjectCollection : NamedObjectCollection<GameObject>  // TODO: Change to NotifyingCollection?
  {
    // This collection calls GameObject.Load/Unload when game objects are added/removed.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    // This flag is set when the collection is modified. It is read and reset by the 
    // GameObjectManager.
    internal bool IsDirty;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    internal GameObjectCollection()
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all elements from the <see cref="GameObjectCollection"/>. 
    /// </summary>
    protected override void ClearItems()
    {
      IsDirty = true;
      foreach (var gameObject in this)
        gameObject.Unload();

      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the <see cref="GameObjectCollection"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already loaded, probably belongs to another service. Or the name 
    /// of the game object is not unique.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0, or <paramref name="index"/> is greater than 
    /// <see cref="System.Collections.ObjectModel.Collection{T}.Count"/>.
    /// </exception>
    protected override void InsertItem(int index, GameObject item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.IsLoaded)
        throw new ArgumentException("Cannot add game object. The game object was already loaded by another owner.");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("Game objects must have a unique name. The name must not be null or an empty string.");

      // Base method must be called first. It may throw exceptions!
      base.InsertItem(index, item);

      IsDirty = true;
      item.Load();
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="GameObjectCollection"/>.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      GameObject removedItem = null;
      if (0 <= index && index < Count)
        removedItem = this[index];

      // Base method must be called first. It may throw exceptions!
      base.RemoveItem(index);

      if (removedItem != null)
      {
        IsDirty = true;
        removedItem.Unload();
      }
    }


    /// <summary>
    /// Replaces the item at the specified index with the specified item.
    /// </summary>
    /// <param name="index">The zero-based index of the item to be replaced.</param>
    /// <param name="item">The new item.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already loaded, probably belongs to another service. Or the name 
    /// of the game object is not unique.
    /// </exception>
    protected override void SetItem(int index, GameObject item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item.IsLoaded)
        throw new ArgumentException("Cannot add game object. The game element was already loaded by another owner.");
      if (string.IsNullOrEmpty(item.Name))
        throw new ArgumentException("Game objects must have a unique name. The name must not be null or an empty string.");

      GameObject removedItem = null;
      if (index >= 0 && index < Count)
        removedItem = this[index];

      // Base method must be called first. It may throw exceptions!
      base.SetItem(index, item);

      IsDirty = true;

      if (removedItem != null)
        removedItem.Unload();

      item.Load();
    }


    /// <inheritdoc/>
    protected override void MoveItem(int oldIndex, int newIndex)
    {
      IsDirty = true;
      base.MoveItem(oldIndex, newIndex);
    }
    #endregion
  }
}
