// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;


namespace DigitalRune.Graphics.Content
{
  /// <summary>
  /// Manages the images of a texture.
  /// </summary>
  /// <remarks>
  /// The collection has a fixed length, which is determined when the collection is created.
  /// Images in the collection can be replaced - but only with an image with equal size and format.
  /// </remarks>
  internal class ImageCollection : IList<Image>
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly Image[] _images;
    #endregion


    //--------------------------------------------------------------
    #region Properties & Events
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the number of elements contained in the <see cref="ImageCollection" />.
    /// </summary>
    public int Count
    {
      get { return _images.Length; }
    }


    /// <summary>
    /// Gets a value indicating whether the <see cref="ImageCollection" /> is read-only.
    /// </summary>
    bool ICollection<Image>.IsReadOnly
    {
      get { return false; }
    }


    /// <summary>
    /// Gets or sets the <see cref="Image"/> at the specified index.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <value>The <see cref="Image"/>.</value>
    /// <returns>The <see cref="Image"/>.</returns>
    public Image this[int index]
    {
      get
      {
        if ((uint)index >= (uint)_images.Length)
          throw new ArgumentOutOfRangeException("index");

        return _images[index];
      }
      set
      {
        if ((uint)index >= (uint)_images.Length)
          throw new ArgumentOutOfRangeException("index");

        if (value == null)
          throw new ArgumentNullException();

        var oldImage = _images[index];
        if (oldImage != null)
          if (oldImage.Width != value.Width || oldImage.Height != value.Height || oldImage.Format != value.Format)
            throw new ArgumentException("The size and format of the new image does not match.", "value");

        _images[index] = value;
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation & Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCollection"/> class.
    /// </summary>
    /// <param name="numberOfImages">The number of images.</param>
    /// <remarks>
    /// The constructor reserves space for <paramref name="numberOfImages"/> entries in the
    /// collection. The entries are <see langword="null"/>.
    /// </remarks>
    public ImageCollection(int numberOfImages)
    {
      _images = new Image[numberOfImages];
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}" /> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<Image> GetEnumerator()
    {
      return ((IEnumerable<Image>)_images).GetEnumerator();
    }


    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return _images.GetEnumerator();
    }


    /// <summary>
    /// Determines whether the <see cref="ImageCollection" /> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ImageCollection" />.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="item" /> is found in the
    /// <see cref="ImageCollection" />; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(Image item)
    {
      return Array.IndexOf(_images, item) >= 0;
    }


    /// <summary>
    /// Copies the elements of the <see cref="ImageCollection"/> to an <see cref="System.Array"/>, 
    /// starting at a particular <see cref="System.Array"/> index.
    /// </summary>
    /// <param name="array">
    /// The one-dimensional <see cref="System.Array"/> that is the destination of the elements 
    /// copied from <see cref="ImageCollection"/>. The <see cref="System.Array"/> must have
    /// zero-based indexing.
    /// </param>
    /// <param name="arrayIndex">
    /// The zero-based index in <paramref name="array"/> at which copying begins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="array"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex"/> is less than 0.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal to 
    /// or greater than the length of <paramref name="array"/>. Or the number of elements in the 
    /// source <see cref="ImageCollection"/> is greater than the available space from 
    /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
    /// </exception>
    public void CopyTo(Image[] array, int arrayIndex)
    {
      _images.CopyTo(array, arrayIndex);
    }


    /// <summary>
    /// Determines the index of a specific item in the <see cref="ImageCollection" />.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="ImageCollection" />.</param>
    /// <returns>
    /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
    /// </returns>
    /// <exception cref="System.NotImplementedException"></exception>
    public int IndexOf(Image item)
    {
      return Array.IndexOf(_images, item);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">-</param>
    void ICollection<Image>.Add(Image item)
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    void ICollection<Image>.Clear()
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">-</param>
    /// <param name="item">- </param>
    void IList<Image>.Insert(int index, Image item)
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="item">-</param>
    /// <returns>-</returns>
    bool ICollection<Image>.Remove(Image item)
    {
      throw new NotSupportedException();
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="index">-</param>
    void IList<Image>.RemoveAt(int index)
    {
      throw new NotSupportedException();
    }
    #endregion
  }
}
