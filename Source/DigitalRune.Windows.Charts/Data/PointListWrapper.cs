// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Wraps an existing list of <see cref="Point"/>s for use as a chart data source.
    /// </summary>
    internal class PointListWrapper : IList<DataPoint>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IList<Point> _list;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="ICollection{T}"/>.</returns>
        public int Count
        {
            get { return _list.Count; }
        }


        /// <summary>
        /// Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="ICollection{T}"/> is read-only; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        bool ICollection<DataPoint>.IsReadOnly
        {
            get { return _list.IsReadOnly; }
        }


        /// <summary>
        /// Gets or sets the <see cref="DataPoint"/> at the specified index.
        /// </summary>
        public DataPoint this[int index]
        {
            get { return new DataPoint(_list[index], null); }
            set { _list[index] = value.Point; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PointListWrapper"/> class.
        /// </summary>
        /// <param name="list">The list of points that should be wrapped.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="list"/> is <see langword="null"/>.
        /// </exception>
        public PointListWrapper(IList<Point> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            _list = list;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<DataPoint> GetEnumerator()
        {
            return new PointListEnumerator(_list);
        }


        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only.
        /// </exception>
        public void Add(DataPoint item)
        {
            _list.Add(item.Point);
        }


        /// <summary>
        /// Removes all items from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only.
        /// </exception>
        public void Clear()
        {
            _list.Clear();
        }


        /// <summary>
        /// Determines whether the <see cref="ICollection{T}"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> is found in the
        /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(DataPoint item)
        {
            return _list.Contains(item.Point);
        }


        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied
        /// from <see cref="ICollection{T}"/>. The <see cref="Array"/> must have zero-based
        /// indexing.
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
        /// <paramref name="array"/> is multidimensional. Or <paramref name="arrayIndex"/> is equal
        /// to or greater than the length of <paramref name="array"/>. Or the number of elements in
        /// the source <see cref="ICollection{T}"/> is greater than the available space from
        /// <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>. Or
        /// the type <see cref="DataPoint"/> cannot be cast automatically to the type of the
        /// destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(DataPoint[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
            if (arrayIndex >= array.Length)
                throw new ArgumentException("arrayIndex is equal to or greater than the length of array.", "arrayIndex");
            if (array.Rank > 1)
                throw new ArgumentException("array is multidimensional", "array");

            for (int i = 0; i < _list.Count; ++i, ++arrayIndex)
                array[arrayIndex] = new DataPoint(_list[i], null);
        }


        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="item"/> was successfully removed from the
        /// <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>. This method also
        /// returns <see langword="false"/> if <paramref name="item"/> is not found in the original
        /// <see cref="ICollection{T}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ICollection{T}"/> is read-only.
        /// </exception>
        public bool Remove(DataPoint item)
        {
            return _list.Remove(item.Point);
        }


        /// <summary>
        /// Determines the index of a specific item in the <see cref="IList{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="IList{T}"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(DataPoint item)
        {
            return _list.IndexOf(item.Point);
        }


        /// <summary>
        /// Inserts an item to the <see cref="IList{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which <paramref name="item"/> should be inserted.
        /// </param>
        /// <param name="item">The object to insert into the <see cref="IList{T}"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IList{T}"/> is read-only.
        /// </exception>
        public void Insert(int index, DataPoint item)
        {
            _list.Insert(index, item.Point);
        }


        /// <summary>
        /// Removes the <see cref="IList{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="IList{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="IList{T}"/> is read-only.
        /// </exception>
        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
        #endregion
    }
}
