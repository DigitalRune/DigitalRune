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
  /// Represents a collection of children.
  /// </summary>
  /// <typeparam name="TParent">The type of the parent object.</typeparam>
  /// <typeparam name="TChild">The type of the child object.</typeparam>
  /// <remarks>
  /// <para>
  /// When a new object is added to or removed from the <see cref="ChildCollection{TParent,TChild}"/> 
  /// the method <see cref="SetParent"/> is called to set the parent property of the child object. 
  /// </para>
  /// <para>
  /// Duplicates items or <see langword="null"/> are not allowed.
  /// </para>
  /// </remarks>
  public abstract class ChildCollection<TParent, TChild> : Collection<TChild> 
    where TParent : class 
    where TChild : class
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets (or sets) the parent which owns this child collection.
    /// </summary>
    /// <value>The parent.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The difference between Parent and GetParent() is documented.")]
    public TParent Parent
    {
      get { return _parent; }
      protected set
      {
        if (_parent == value)
          return;

        _parent = value;

        foreach (TChild child in (List<TChild>)Items)
        {
          Debug.Assert(child != null);
          SetParent(child, _parent);
        }
      }
    }
    private TParent _parent;
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <overloads>
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildCollection{TParent, TChild}"/> class.
    /// </summary>
    /// </overloads>
    /// 
    /// <summary>
    /// Initializes a new instance of the <see cref="ChildCollection{TParent, TChild}"/> class.
    /// </summary>
    /// <param name="parent">The parent object that owns this collection.</param>
    protected ChildCollection(TParent parent)
    {
      _parent = parent;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="ChildCollection{TParent, TChild}"/> class that
    /// has the specified initial capacity.
    /// </summary>
    /// <param name="parent">The parent object that owns this collection.</param>
    /// <param name="capacity">The number of elements that the new list can initially store.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="capacity"/> is less than 0.
    /// </exception>
    protected ChildCollection(TParent parent, int capacity) : base(new List<TChild>(capacity))
    {
      _parent = parent;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ChildCollection{TParent,TChild}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="List{TChild}.Enumerator"/> for <see cref="ChildCollection{TParent,TChild}"/>.
    /// </returns>
    public new List<TChild>.Enumerator GetEnumerator()
    {
      return ((List<TChild>)Items).GetEnumerator();
    }


    /// <summary>
    /// Removes all elements from the collection. 
    /// </summary>
    protected override void ClearItems()
    {
      foreach (TChild child in (List<TChild>)Items)
      {
        Debug.Assert(child != null);
        SetParent(child, null);
      }

      base.ClearItems();
    }


    /// <summary>
    /// Inserts an element into the collection at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which <paramref name="item"/> should be inserted.
    /// </param>
    /// <param name="item">The object to insert.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Cannot insert node into collection - <paramref name="item"/> is already in this collection
    /// or child of another object.
    /// </exception>
    protected override void InsertItem(int index, TChild item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a children collection must not be null.");

      TParent parent = GetParent(item);
      if (parent != null)
      {
        if (parent == _parent)
          throw new InvalidOperationException("Cannot insert item into children collection. Item is already part of this collection.");
        else
          throw new InvalidOperationException("Cannot insert item into children collection. Item is already the child of another object.");
      }

      base.InsertItem(index, item);
      SetParent(item, _parent);
    }


    /// <summary>
    /// Removes the element at the specified index of the collection. 
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      TChild removedChild = Items[index];
      base.RemoveItem(index);
      SetParent(removedChild, null);
    }


    /// <summary>
    /// Replaces the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to replace.</param>
    /// <param name="item">The new value for the element at the specified index.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Cannot insert node into collection - <paramref name="item"/> is already in this collection
    /// or child of another object.
    /// </exception>
    protected override void SetItem(int index, TChild item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "Items in a children collection must not be null.");

      TChild removedItem = Items[index];
      if (!ReferenceEquals(item, removedItem))
      {
        TParent parent = GetParent(item);
        if (parent != null)
        {
          if (parent == _parent)
            throw new InvalidOperationException("Cannot insert item into children collection. Item is already part of this collection.");
          else
            throw new InvalidOperationException("Cannot insert item into children collection. Item is already the child of another object.");
        }

        base.SetItem(index, item);
        SetParent(removedItem, null);
        SetParent(item, _parent);
      }
    }


    /// <summary>
    /// Gets the parent of an object.
    /// </summary>
    /// <param name="child">The child object.</param>
    /// <returns>The parent of <paramref name="child"/>.</returns>
    protected abstract TParent GetParent(TChild child);


    /// <summary>
    /// Sets the parent of the given object.
    /// </summary>
    /// <param name="parent">The parent to set.</param>
    /// <param name="child">The child object.</param>
    protected abstract void SetParent(TChild child, TParent parent);


    /// <summary>
    /// Moves the item at the specified index to a new location in the collection. 
    /// </summary>
    /// <param name="oldIndex">
    /// The zero-based index specifying the location of the item to be moved. 
    /// </param>
    /// <param name="newIndex">
    /// The zero-based index specifying the new location of the item.
    /// </param>
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
    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
      var item = Items[oldIndex];
      Items.RemoveAt(oldIndex);
      Items.Insert(newIndex, item);
    }
    #endregion
  }
}
