// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Collections
{
  /// <summary>
  /// Describes the action that caused a <see cref="NotifyingCollection{T}.CollectionChanged"/> 
  /// event.
  /// </summary>
  public enum CollectionChangedAction
  {
    /// <summary>
    /// New items were added to the collection.
    /// </summary>
    Add,
    /// <summary>
    /// Items were removed from the collection.
    /// </summary>
    Remove,
    /// <summary>
    /// One item was replaced in the collection.
    /// </summary>
    Replace,
    /// <summary>
    /// All items were removed from the collection.
    /// </summary>
    Clear,
    /// <summary>
    /// An item was moved within the collection.
    /// </summary>
    Move,
  }
}
