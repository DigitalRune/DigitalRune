// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
#if !UNITY
using System.Collections.ObjectModel;
#else
using DigitalRune.Collections.ObjectModel;
#endif


namespace DigitalRune.Collections
{
  /// <summary>
  /// Stores objects by their names.
  /// </summary>
  /// <typeparam name="T">
  /// The type of the objects. Must be derived from <see cref="INamedObject"/>.
  /// </typeparam>
  /// <remarks>
  /// <para>
  /// This collection stores <see cref="INamedObject"/>s. The name of each object is used as key
  /// when inserting a new object. The collection ensures that each object is properly named.
  /// </para>
  /// <para>
  /// The <see cref="NamedObjectCollection{T}"/> assumes that the names of the objects are constant.
  /// The collection might not work as expected if the object's names are changed while they are
  /// part of the collection.
  /// </para>
  /// </remarks>
  public class NamedObjectCollection<T> : KeyedCollection<string, T> where T : INamedObject
  {
    // Notes:
    // The collections throws a NullReferenceException instead of an ArgumentNullException
    // when the items is null. (Can't check for null because T can be a struct.)
    // Derived types can override the virtual methods and throw an ArgumentNullException.

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
    #endregion


    //--------------------------------------------------------------
    #region Creation
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedObjectCollection{T}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="NamedObjectCollection{T}"/> class.
    /// </summary>
    /// <remarks>
    /// By default, the <see cref="StringComparer.Ordinal"/> is used to compare names.
    /// </remarks>
    public NamedObjectCollection()
      : base(StringComparer.Ordinal)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="NamedObjectCollection{T}"/> class using the
    /// given comparer.
    /// </summary>
    /// <param name="comparer">The comparer that compares whether two names are equal.</param>
    public NamedObjectCollection(IEqualityComparer<string> comparer)
      : base(comparer)
    {
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="NamedObjectCollection{T}"/> class that uses the
    /// specified equality comparer and creates a lookup dictionary when the specified threshold is 
    /// exceeded. 
    /// </summary>
    /// <param name="comparer">The comparer that compares whether two names are equal.</param>
    /// <param name="dictionaryCreationThreshold">
    /// The number of elements the collection can hold without creating a lookup dictionary (0
    /// creates the lookup dictionary when the first item is added), or –1 to specify that a lookup
    /// dictionary is never created. 
    /// </param>
    public NamedObjectCollection(IEqualityComparer<string> comparer, int dictionaryCreationThreshold)
      : base(comparer, dictionaryCreationThreshold)
    {
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="KeyedCollection{TKey,TItem}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{T}.Enumerator"/> for <see cref="KeyedCollection{TKey,TItem}"/>.
    /// </returns>
    public new List<T>.Enumerator GetEnumerator()
    {
      return ((List<T>)Items).GetEnumerator();
    }


    /// <summary>
    /// When implemented in a derived class, extracts the key from the specified element.
    /// </summary>
    /// <param name="item">The element from which to extract the key.</param>
    /// <returns>The key for the specified element.</returns>
    protected override string GetKeyForItem(T item)
    {
      return item.Name;
    }


    /// <summary>
    /// Removes all elements from the <see cref="KeyedCollection{TKey,TItem}" />.
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
    /// Inserts an element into the <see cref="KeyedCollection{TKey,TItem}"/> at the specified
    /// index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    protected override void InsertItem(int index, T item)
    {
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
    /// Removes the element at the specified index of the <see cref="KeyedCollection{TKey,TItem}" />.
    /// </summary>
    /// <param name="index">The index of the element to remove.</param>
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
    /// Replaces the item at the specified index with the specified item.
    /// </summary>
    /// <param name="index">The zero-based index of the item to be replaced.</param>
    /// <param name="item">The new item.</param>
    protected override void SetItem(int index, T item)
    {
      T removedObject = Items[index];
      if (EqualityComparer<T>.Default.Equals(item, removedObject))
        return;

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
    /// <see cref="NamedObjectCollection{T}"/>. 
    /// </summary>
    /// <param name="collection">
    /// The collection whose elements should be added to the end of the 
    /// <see cref="NamedObjectCollection{T}"/>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collection"/> is <see langword="null"/>.
    /// </exception>
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
    /// Inserts the elements of a collection into the <see cref="NamedObjectCollection{T}"/> at the
    /// specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the new elements should be inserted.
    /// </param>
    /// <param name="collection">
    /// The collection whose elements should be inserted into the
    /// <see cref="NamedObjectCollection{T}"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// If index is equal to <see cref="Collection{T}.Count"/>, the elements are added to the end of
    /// <see cref="NamedObjectCollection{T}"/>.
    /// </para>
    /// <para>
    /// The order of the elements in the collection is preserved in the
    /// <see cref="NamedObjectCollection{T}"/>.
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
    /// Removes a range of elements from the <see cref="NamedObjectCollection{T}"/>.
    /// </summary>
    /// <param name="index">
    /// The zero-based starting index of the range of elements to remove.
    /// </param>
    /// <param name="count">The number of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="count"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of
    /// elements in the <see cref="NamedObjectCollection{T}"/>.
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


    /// <overloads>
    /// <summary>
    /// Gets the object associated with a specified key.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Gets the object associated with the specified key.
    /// </summary>
    /// <param name="key">The name of the element to get.</param>
    /// <param name="value">
    /// When this method returns, the object associated with the specified key, if 
    /// the key is found; otherwise, the default value for the type of the value 
    /// parameter. (This parameter is passed uninitialized.)
    /// </param>
    /// <returns><see langword="true"/> if the <see cref="NamedObjectCollection{T}"/> contains 
    /// an element with the specified key; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet(string key, out T value)
    {
      if (Dictionary == null)
      {
        // Linear search.
        foreach (var item in this)
        {
          if (Comparer.Equals(item.Name, key))
          {
            value = item;
            return true;
          }
        }

        value = default(T);
        return false;
      }

      return Dictionary.TryGetValue(key, out value);
    }


    /// <summary>
    /// Gets the object associated with the specified key. The object needs to be of a certain type.
    /// </summary>
    /// <typeparam name="TExpected">The expected type of the object.</typeparam>
    /// <param name="key">The name of the element to get.</param>
    /// <param name="value">
    /// The object associated with the specified key, if the key is found and the object is of type
    /// <typeparamref name="TExpected"/>; otherwise, the default value for 
    /// <typeparamref name="TExpected"/> is returned. (This parameter is passed uninitialized.)
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the <see cref="NamedObjectCollection{T}"/> contains an element 
    /// with the specified key which is of the expected type; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGet<TExpected>(string key, out TExpected value) where TExpected : T
    {
      value = default(TExpected);

      if (Dictionary != null)
      {
        T t;
        if (Dictionary.TryGetValue(key, out t) && t is TExpected)
        {
          value = (TExpected)t;
          return true;
        }

        return false;
      }

      // No Dictionary --> Linear search.
      foreach (var item in this)
      {
        if (Comparer.Equals(item.Name, key))
        {
          if (item is TExpected)
          {
            value = (TExpected)item;
            return true;
          }

          return false;
        }
      }

      return false;
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
