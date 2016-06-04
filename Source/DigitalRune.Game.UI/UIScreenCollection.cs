// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using DigitalRune.Collections;
using DigitalRune.Game.UI.Controls;


namespace DigitalRune.Game.UI
{
  /// <summary>
  /// Represents a collection of <see cref="UIScreen"/>.
  /// </summary>
  public class UIScreenCollection : NamedObjectCollection<UIScreen> 
  {
    // Special tasks of this collection:
    // - Calls item.Load/Unload. 
    // - Sets item.UIService.
    // - Sets IsDirty when the collection was changed.

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly UIManager _uiManager;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    // This flag is set when the collection was modified. It is reset by the UIManager.
    internal bool IsDirty { get; set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="UIScreenCollection"/> class.
    /// </summary>
    /// <param name="uiManager">The <see cref="UIManager"/>.</param>
    internal UIScreenCollection(UIManager uiManager) 
    {
      Debug.Assert(uiManager != null);
      _uiManager = uiManager;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all elements from the <see cref="UIScreenCollection"/>. 
    /// </summary>
    protected override void ClearItems()
    {
      IsDirty = true;

      foreach (var item in this)
      {
        item.Unload();
        item.UIService = null;
      }

      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the <see cref="UIScreenCollection"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which item should be inserted.</param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0, or <paramref name="index"/> is greater than 
    /// <see cref="System.Collections.ObjectModel.Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Cannot add item to <see cref="UIScreenCollection"/>. The item is already part of a 
    /// different <see cref="UIScreenCollection"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void InsertItem(int index, UIScreen item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item != null && item.UIService != null)
        throw new InvalidOperationException("Cannot add item to UIScreenCollection. The item is already part of a different UIScreenCollection.");

      IsDirty = true;

      // Base method must be called first. It may throw exceptions!
      base.InsertItem(index, item);

      item.UIService = _uiManager;
      item.Load();
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="UIScreenCollection"/>.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      IsDirty = true;

      UIScreen removedItem = null;
      if (0 <= index && index < Count)
        removedItem = this[index];

      // Base method must be called first. It may throw exceptions!
      base.RemoveItem(index);

      if (removedItem != null)
      {
        removedItem.Unload();
        removedItem.UIService = null;
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
    /// <exception cref="InvalidOperationException">
    /// Cannot add item to <see cref="UIScreenCollection"/>. The item is already part of a different 
    /// <see cref="UIScreenCollection"/>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    protected override void SetItem(int index, UIScreen item)
    {
      if (item == null)
        throw new ArgumentNullException("item");
      if (item != null && item.UIService != null)
        throw new InvalidOperationException("Cannot add item to UIScreenCollection. The item is already part of a different UIScreenCollection.");

      IsDirty = true;

      UIScreen removedItem = null;
      if (0 <= index && index < Count)
        removedItem = this[index];

      // Base method must be called first. It may throw exceptions!
      base.SetItem(index, item);

      if (removedItem != null)
      {
        removedItem.Unload();
        removedItem.UIService = null;
      }
      
      item.UIService = _uiManager;
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
