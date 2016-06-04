// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif
using System.Diagnostics;


namespace DigitalRune.Collections
{
  /// <summary>
  /// Represents collection of objects that sends notifications (events) when the collection is 
  /// modified.
  /// </summary>
  /// <typeparam name="T">The type of elements in the collection.</typeparam>
  /// <remarks>
  /// <para>
  /// This collection is similar to the <strong>ObservableCollection{T}</strong> in the .NET
  /// Framework. (In .NET 3.5 the <strong>ObservableCollection{T}</strong> is located in the 
  /// WindowsBase.dll. Therefore it is not usable in non-WPF applications. This has been solved in 
  /// .NET 4.0 where the class was moved into the System.dll.)
  /// </para>
  /// </remarks>
  public class NotifyingCollection<T> : Collection<T>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private bool _suppressCollectionChangedEvent;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Occurs when items were added, replaced or removed.
    /// </summary>
    public event EventHandler<CollectionChangedEventArgs<T>> CollectionChanged;


    /// <summary>
    /// Gets or sets a value indicating whether <see langword="null"/> items are allowed in the
    /// collection.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if <see langword="null"/> items are allowed; otherwise, 
    /// <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    public bool AllowNull { get; private set; }


    /// <summary>
    /// Gets or sets a value indicating whether duplicate items are allowed in the collection.
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
    /// Initializes a new instance of the <see cref="NotifyingCollection{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyingCollection{T}"/> class.
    /// </summary>
    public NotifyingCollection()
    {
      AllowNull = true;
      AllowDuplicates = true;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyingCollection{T}"/> class with the given
    /// settings.
    /// </summary>
    /// <param name="allowNull">
    /// If set to <see langword="true"/> null items are allowed.
    /// </param>
    /// <param name="allowDuplicates">
    /// If set to <see langword="true"/> duplicate items are allowed.
    /// </param>
    public NotifyingCollection(bool allowNull, bool allowDuplicates)
    {
      AllowNull = allowNull;
      AllowDuplicates = allowDuplicates;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="NotifyingCollection{T}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="NotifyingCollection{T}"/>.
    /// </returns>
    public new List<T>.Enumerator GetEnumerator()
    {
      return ((List<T>)Items).GetEnumerator();
    }


    /// <summary>
    /// Removes all elements from the <see cref="Collection{T}"/>.
    /// </summary>
    protected override void ClearItems()
    {
      if (_suppressCollectionChangedEvent)
      {
        base.ClearItems();
        return;
      }

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
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or greater than 
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
          throw new ArgumentException("Duplicate items are not allowed in the collection.", "item");
      }
      // ReSharper restore CompareNonConstrainedGenericWithNull

      base.InsertItem(index, item);

      if (_suppressCollectionChangedEvent)
        return;

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
    /// <paramref name="index"/> is less than zero or <paramref name="index"/> is equal to or 
    /// greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    protected override void RemoveItem(int index)
    {
      if (_suppressCollectionChangedEvent)
      {
        base.RemoveItem(index);
        return;
      }

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
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero or is greater than 
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
          throw new ArgumentException("Duplicate items are not allowed in the collection.", "item");
      }
      // ReSharper restore CompareNonConstrainedGenericWithNull

      if (_suppressCollectionChangedEvent)
      {
        base.SetItem(index, item);
        return;
      }

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
    /// Adds the elements of the specified collection to the end of the 
    /// <see cref="NotifyingCollection{T}"/>. 
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of the 
    /// <see cref="NotifyingCollection{T}"/>. The collection itself cannot be 
    /// <see langword="null"/>, but it can contain elements that are <see langword="null"/>, if type 
    /// <typeparamref name="T"/> is a reference type and <see cref="AllowNull"/> is set to 
    /// <see langword="true"/>. 
    /// </param>
    /// <remarks>
    /// The order of the elements in the collection is preserved in the 
    /// <see cref="NotifyingCollection{T}"/>. 
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <seealso cref="List{T}.AddRange"/>
    public void AddRange(IEnumerable<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Add;
        args.NewItemsIndex = Items.Count;

        _suppressCollectionChangedEvent = true;

        foreach (var item in collection)
        {
          Add(item);
          args.NewItems.Add(item);
        }

        _suppressCollectionChangedEvent = false;

        if (args.NewItems.Count > 0)    // collection could be an empty list!
          OnCollectionChanged(args);
      }
      finally
      {
        _suppressCollectionChangedEvent = false;
        args.Recycle();
      }
    }


    /// <summary>
    /// Inserts the elements of a collection into the <see cref="NotifyingCollection{T}"/> at the
    /// specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the new elements should be inserted.
    /// </param>
    /// <param name="collection">
    /// The collection whose elements should be inserted into the 
    /// <see cref="NotifyingCollection{T}"/>. The collection itself cannot be 
    /// <see langword="null"/>, but it can contain elements that are <see langword="null"/>, if type 
    /// <typeparamref name="T"/> is a reference type and <see cref="AllowNull"/> is set to 
    /// <see langword="true"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// If index is equal to <see cref="Collection{T}.Count"/>, the elements are added to the end of 
    /// <see cref="NotifyingCollection{T}"/>.
    /// </para>
    /// <para>
    /// The order of the elements in the collection is preserved in the
    /// <see cref="NotifyingCollection{T}"/>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0 or greater than <see cref="Collection{T}.Count"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
    /// <seealso cref="List{T}.InsertRange"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "index+1")]
    public void InsertRange(int index, IEnumerable<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection");

      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Add;
        args.NewItemsIndex = index;

        _suppressCollectionChangedEvent = true;

        foreach (var item in collection)
        {
          Insert(index, item);
          index++;
          args.NewItems.Add(item);
        }

        _suppressCollectionChangedEvent = false;

        if (args.NewItems.Count > 0)        // collection could be an empty list!
          OnCollectionChanged(args);
      }
      finally
      {
        _suppressCollectionChangedEvent = false;
        args.Recycle();
      }
    }


    /// <summary>
    /// Removes a range of elements from the <see cref="NotifyingCollection{T}"/>. 
    /// </summary>
    /// <param name="index">
    /// The zero-based starting index of the range of elements to remove.
    /// </param>
    /// <param name="count">
    /// The number of elements to remove. 
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of 
    /// elements in the <see cref="NotifyingCollection{T}"/>.
    /// </exception>
    /// <seealso cref="List{T}.RemoveRange"/>
    public void RemoveRange(int index, int count)
    {
      if (index < 0)
        throw new ArgumentOutOfRangeException("index", "The starting index must be equal to or greater than 0.");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", "The number of elements to remove must be equal to or greater than 0.");
      if (index + count > Count)
        throw new ArgumentException("index and count do not denote a valid range of elements in the collection.");

      if (count == 0)
        return;

      var args = CollectionChangedEventArgs<T>.Create();
      try
      {
        args.Action = CollectionChangedAction.Remove;
        args.OldItemsIndex = index;

        _suppressCollectionChangedEvent = true;

        for (int i = 0; i < count; i++)
        {
          args.OldItems.Add(Items[index]);
          RemoveAt(index);
        }

        _suppressCollectionChangedEvent = false;

        OnCollectionChanged(args);
      }
      finally
      {
        _suppressCollectionChangedEvent = false;
        args.Recycle();
      }
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


    /// <summary>
    /// Raises the <see cref="CollectionChanged"/> event.
    /// </summary>
    /// <param name="eventArgs">
    /// <see cref="CollectionChangedEventArgs{T}"/> object that provides the arguments for the
    /// event.
    /// </param>
    /// <remarks>
    /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnCollectionChanged"/> in a
    /// derived class, be sure to call the base class's <see cref="OnCollectionChanged"/> method so
    /// that registered delegates receive the event.
    /// </remarks>
    protected virtual void OnCollectionChanged(CollectionChangedEventArgs<T> eventArgs)
    {
      Debug.Assert(!_suppressCollectionChangedEvent);

      var handler = CollectionChanged;
      if (handler != null)
        handler(this, eventArgs);
    }
    #endregion
  }
}
