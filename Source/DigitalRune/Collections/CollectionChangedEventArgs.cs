// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Provides data for the <see cref="NotifyingCollection{T}.CollectionChanged"/> event.
  /// </summary>
  /// <typeparam name="T">The type of elements in the collection.</typeparam>
  public sealed class CollectionChangedEventArgs<T> : EventArgs, IRecyclable
  {
    // ReSharper disable StaticFieldInGenericType
    private static readonly ResourcePool<CollectionChangedEventArgs<T>> Pool = new ResourcePool<CollectionChangedEventArgs<T>>(
      () => new CollectionChangedEventArgs<T>(),     // Create
      null,                                          // Initialize
      null);                                         // Uninitialize
    // ReSharper restore StaticFieldInGenericType


    /// <summary>
    /// Gets or sets the action that caused the event.
    /// </summary>
    /// <value>The action.</value>
    public CollectionChangedAction Action { get; set; }


    /// <summary>
    /// Gets or sets the index of the first new item.
    /// </summary>
    /// <value>The index of the first new item. If there are no new items this index is -1.</value>
    public int NewItemsIndex { get; set; }


    /// <summary>
    /// Gets or sets the index where the first old item was placed in the collection.
    /// </summary>
    /// <value>
    /// The (former) index of the first old item. If there are no old items this index is -1.
    /// </value>
    public int OldItemsIndex { get; set; }

    
    /// <summary>
    /// Gets the new items that were involved in the change.
    /// </summary>
    /// <value>The new items. The default is an empty list.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<T> NewItems { get; private set; }


    /// <summary>
    /// Gets the items affected by a replace, remove or move action.
    /// </summary>
    /// <value>The old items. The default is an empty list.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Performance")]
    public List<T> OldItems { get; private set; }


    /// <summary>
    /// Prevents a default instance of the <see cref="CollectionChangedEventArgs{T}"/> class from 
    /// being created.
    /// </summary>
    private CollectionChangedEventArgs()
    {
      NewItemsIndex = -1;
      NewItems = new List<T>();
      OldItemsIndex = -1;
      OldItems = new List<T>();
    }


    /// <summary>
    /// Creates an instance of the <see cref="CollectionChangedEventArgs{T}"/> class. (This method 
    /// reuses a previously recycled instance or allocates a new instance if necessary.)
    /// </summary>
    /// <returns>
    /// A new or reusable instance of the <see cref="CollectionChangedEventArgs{T}"/> class.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method tries to obtain a previously recycled instance from a resource pool if resource
    /// pooling is enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>). If no
    /// object is available, a new instance is automatically allocated on the heap. 
    /// </para>
    /// <para>
    /// The owner of the object should call <see cref="Recycle"/> when the instance is no longer 
    /// needed.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
    public static CollectionChangedEventArgs<T> Create()
    {
      return Pool.Obtain();
    }


    /// <summary>
    /// Recycles this instance of the <see cref="CollectionChangedEventArgs{T}"/> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method resets this instance and returns it to a resource pool if resource pooling is 
    /// enabled (see <see cref="ResourcePool.Enabled">ResourcePool.Enabled</see>).
    /// </para>
    /// </remarks>
    public void Recycle()
    {
      Action = CollectionChangedAction.Add;
      NewItemsIndex = -1;
      NewItems.Clear();
      OldItemsIndex = -1;
      OldItems.Clear();

      Pool.Recycle(this);
    }
  }
}
