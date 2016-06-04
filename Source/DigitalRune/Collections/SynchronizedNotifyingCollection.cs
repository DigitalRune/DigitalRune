// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if !NETFX_CORE && !SILVERLIGHT && !WP7 && !WP8 && !XBOX && !UNITY && !PORTABLE && !ANDROID && !IOS
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Represents a collection of objects that sends notifications (events) when the collection is 
  /// modified where access is thread-safe. (Not available on these platforms: Silverlight, Windows 
  /// Phone 7, Xbox 360)
  /// </summary>
  /// <typeparam name="T">The type of elements in the collection.</typeparam>
  /// <remarks>
  /// This type is not available on the following platforms: Silverlight, Windows Phone 7/8, Xbox 360
  /// </remarks>
  public class SynchronizedNotifyingCollection<T> : Collection<T>
  {
    //--------------------------------------------------------------
    #region Nested Types
    //--------------------------------------------------------------

    /// <summary>
    /// A <see cref="SynchronizedCollection{TNested}"/> with an enumerator that avoids garbage.
    /// </summary>
    private sealed class ImprovedSynchronizedCollection : SynchronizedCollection<T>
    {
      /// <summary>
      /// Returns an enumerator that iterates through the collection. 
      /// </summary>
      /// <returns>
      /// An <see cref="List{T}.Enumerator"/>.
      /// </returns>
      public new List<T>.Enumerator GetEnumerator()
      {
        return Items.GetEnumerator();
      }
    }
    #endregion
    

    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Occurs when items were added, replaced or removed.
    /// </summary>
    public event EventHandler<CollectionChangedEventArgs<T>> CollectionChanged;


    /// <summary>
    /// Gets or sets a value indicating whether <see langword="null"/> items are allowed 
    /// in the collection.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see langword="null"/> items are allowed; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool AllowNull { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether duplicate items are allowed in 
    /// the collection.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if duplicate items are allowed; otherwise, <see langword="false"/>.
    /// The default value is <see langword="true"/>.
    /// </value>
    public bool AllowDuplicates { get; private set; }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedNotifyingCollection{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedNotifyingCollection{T}"/> class.
    /// </summary>
    public SynchronizedNotifyingCollection()
      : this(true, true)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="SynchronizedNotifyingCollection{T}"/> class
    /// with the given settings.
    /// </summary>
    /// <param name="allowNull">
    /// If set to <see langword="true"/> null items are allowed.
    /// </param>
    /// <param name="allowDuplicates">
    /// If set to <see langword="true"/> duplicate items are allowed.
    /// </param>
    public SynchronizedNotifyingCollection(bool allowNull, bool allowDuplicates)
      : base(new ImprovedSynchronizedCollection())
    {
      AllowNull = allowNull;
      AllowDuplicates = allowDuplicates;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Removes all elements from the <see cref="Collection{T}"/>.
    /// </summary>
    protected override void ClearItems()
    {
      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Clear;
        args.OldItemsIndex = 0;
        for (int i = 0; i < Count; i++)
          args.OldItems.Add(Items[i]);

        base.ClearItems();

        OnCollectionChanged(args);
      }
      finally
      {
        args.Recycle();
      }
    }


    /// <summary>
    /// Inserts an element into the <see cref="Collection{T}"/> at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert. The value can be null for reference types.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. The collection does not allow 
    /// <see langword="null"/> values. See <see cref="AllowNull"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. The collection does not 
    /// allow duplicate items. See <see cref="AllowDuplicates"/>.
    /// </exception>
    protected override void InsertItem(int index, T item)
    {
      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (item == null)
      {
        if (!AllowNull)
          throw new ArgumentNullException("item");
      }
      else
      {
        if (!AllowDuplicates && Contains(item))
          throw new ArgumentException("Duplicate items are not allowed in the collection.");
      }
      // ReSharper restore CompareNonConstrainedGenericWithNull

      base.InsertItem(index, item);

      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Add;
        args.NewItems.Add(item);
        args.NewItemsIndex = index;
        OnCollectionChanged(args);
      }
      finally
      {
        args.Recycle();
      }      
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or 
    /// greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    protected override void RemoveItem(int index)
    {
      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Remove;
        args.OldItems.Add(Items[index]);
        args.OldItemsIndex = index;

        base.RemoveItem(index);

        OnCollectionChanged(args);
      }
      finally
      {
        args.Recycle();
      }   
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">
    /// The new value for the element at the specified index. The value can be null for reference 
    /// types.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than 
    /// <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>. The collection does not allow 
    /// <see langword="null"/> values. See <see cref="AllowNull"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="item"/> is already contained in the collection. The collection does not 
    /// allow duplicate items. See <see cref="AllowDuplicates"/>.
    /// </exception>
    protected override void SetItem(int index, T item)
    {
      T removedObject = Items[index];
      if (EqualityComparer<T>.Default.Equals(item, removedObject))
        return;

      // ReSharper disable CompareNonConstrainedGenericWithNull
      if (item == null)
      {
        if (!AllowNull)
          throw new ArgumentNullException("item");
      }
      else
      {
        if (!AllowDuplicates && Contains(item))
          throw new ArgumentException("Duplicate items are not allowed in the collection.");
      }
      // ReSharper restore CompareNonConstrainedGenericWithNull

      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Replace;
        args.NewItems.Add(item);
        args.NewItemsIndex = index;
        args.OldItems.Add(removedObject);
        args.OldItemsIndex = index;

        base.SetItem(index, item);

        OnCollectionChanged(args);
      }
      finally
      {
        args.Recycle();
      }      
    }


    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="CollectionChangedEventArgs{T}"/> object that provides the arguments for the
    /// event.
    /// </param>
    protected virtual void OnCollectionChanged(CollectionChangedEventArgs<T> eventArgs)
    {
      EventHandler<CollectionChangedEventArgs<T>> handler = CollectionChanged;

      if (handler != null)
        handler(this, eventArgs);
    }


    /// <summary>
    /// Returns an enumerator that iterates through the 
    /// <see cref="SynchronizedNotifyingCollection{T}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="SynchronizedNotifyingCollection{T}"/>.
    /// </returns>
    public new List<T>.Enumerator GetEnumerator()
    {
      var syncedCollection = (ImprovedSynchronizedCollection)Items;
      var enumerator = syncedCollection.GetEnumerator();
      return enumerator;
    }


    /// <summary>
    /// Moves the item at the specified index to a new location in the collection. 
    /// </summary>
    /// <param name="oldIndex">
    /// The zero-based index specifying the location of the item to be moved. 
    /// </param>
    /// <param name="newIndex">
    /// The zero-based index specifying the new location of the item.
    /// </param>
    /// <remarks>
    /// Subclasses can override the <see cref="MoveItem"/> method to provide custom behavior for
    /// this method. 
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="oldIndex"/> or <paramref name="newIndex"/> is out of range.
    /// </exception>
    public void Move(int oldIndex, int newIndex)
    {
      if (oldIndex < 0 || oldIndex >= Count)
        throw new ArgumentOutOfRangeException("oldIndex");
      if (newIndex < 0 || newIndex >= Count)
        throw new ArgumentOutOfRangeException("oldIndex");

      if (oldIndex == newIndex)
        return;

      MoveItem(oldIndex, newIndex);
    }


    /// <summary>
    /// Moves the item at the specified index to a new location in the collection.
    /// </summary>
    /// <param name="oldIndex">
    /// The zero-based index specifying the location of the item to be moved. 
    /// </param>
    /// <param name="newIndex">
    /// The zero-based index specifying the new location of the item.
    /// </param>
    /// <remarks>
    /// <para>
    /// This implementation raises the <see cref="CollectionChanged"/> event. Subclasses can
    /// override this protected method to provide custom behavior for the 
    /// <see cref="Move"/> method. 
    /// </para>
    /// <para>
    /// This method is called by <see cref="Move"/>. The range of <paramref name="oldIndex"/> and 
    /// <paramref name="newIndex"/> is checked in <see cref="Move"/> before this method is called.
    /// </para>
    /// </remarks>
    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        var item = Items[oldIndex];
        Items.RemoveAt(oldIndex);
        Items.Insert(newIndex, item);

        args.Action = CollectionChangedAction.Move;
        args.NewItemsIndex = newIndex;
        args.OldItemsIndex = oldIndex;
        args.OldItems.Add(item);

        OnCollectionChanged(args);
      }
      finally
      {
        args.Recycle();
      }
    }
    #endregion
  }
}
#endif
