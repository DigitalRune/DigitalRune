// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Mathematics.Interpolation;


namespace DigitalRune.Graphics
{
  /// <summary>
  /// Manages a collection of items owned by a <see cref="Figure"/>.
  /// </summary>
  /// <typeparam name="T">The type of items.</typeparam>
  public class FigureDataCollection<T> : Collection<T> where T : class
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Figure _figure;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="FigureDataCollection{T}"/> class.
    /// </summary>
    /// <param name="owner">The figure that owns the collection.</param>
    internal FigureDataCollection(Figure owner)
    {
      Debug.Assert(owner != null, "owner must not be null.");
      _figure = owner;
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    
    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="FigureDataCollection{T}"/>. 
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> for <see cref="FigureDataCollection{T}"/>.
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
      _figure.Invalidate();
      base.ClearItems();
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
    /// <paramref name="item"/> is <see langword="null"/>. 
    /// </exception>
    protected override void InsertItem(int index, T item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "null items are not allowed in this collection.");

      base.InsertItem(index, item);
      _figure.Invalidate();
    }


    /// <summary>
    /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    protected override void RemoveItem(int index)
    {
      base.RemoveItem(index);
      _figure.Invalidate();
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
    /// <paramref name="item"/> is <see langword="null"/>. 
    /// </exception>
    protected override void SetItem(int index, T item)
    {
      if (item == null)
        throw new ArgumentNullException("item", "null items are not allowed in this collection.");

      base.SetItem(index, item);
      _figure.Invalidate();
    }    
    #endregion
  }


  /// <summary>
  /// Manages a collection of child figures.
  /// </summary>
  public class FigureCollection : FigureDataCollection<Figure>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="FigureCollection"/> class.
    /// </summary>
    /// <param name="owner">The figure that owns the collection.</param>
    internal FigureCollection(Figure owner)
      : base(owner)
    {
    }
  }


  /// <summary>
  /// Manages a collection of 2D path segments.
  /// </summary>
  public class PathSegment2FCollection : FigureDataCollection<ICurve<float, Vector2F>>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegment2FCollection"/> class.
    /// </summary>
    /// <param name="owner">The figure that owns the collection.</param>
    internal PathSegment2FCollection(Figure owner)
      : base(owner)
    {
    }
  }


  /// <summary>
  /// Manages a collection of 3D path segments.
  /// </summary>
  public class PathSegment3FCollection : FigureDataCollection<ICurve<float, Vector3F>>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="PathSegment3FCollection"/> class.
    /// </summary>
    /// <param name="owner">The figure that owns the collection.</param>
    internal PathSegment3FCollection(Figure owner)
      : base(owner)
    {
    }
  }
}
