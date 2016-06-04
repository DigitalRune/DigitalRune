// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Base class for collections of type <see cref="IDockElement"/> or <see cref="IFloatWindow"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the collection.
    /// </typeparam>
    /// <remarks>
    /// <para>
    /// Duplicate elements and <see langword="null"/> are not allowed in the collection.
    /// </para>
    /// </remarks>
    public abstract class DockElementCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="DockElementCollection{T}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="List{T}.Enumerator"/> for <see cref="DockElementCollection{T}"/>.
        /// </returns>
        public new List<T>.Enumerator GetEnumerator()
        {
            return ((List<T>)Items).GetEnumerator();
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
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection. The collection does not 
        /// allow duplicate items.
        /// </exception>
        protected override void InsertItem(int index, T item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (Contains(item))
                throw new ArgumentException("Duplicate items are not allowed in the collection.", nameof(item));

            base.InsertItem(index, item);
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
        /// <see langword="null"/> values.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection. The collection does not 
        /// allow duplicate items.
        /// </exception>
        protected override void SetItem(int index, T item)
        {
            T removedObject = Items[index];
            if (EqualityComparer<T>.Default.Equals(item, removedObject))
                return;

            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (Contains(item))
                throw new ArgumentException("Duplicate items are not allowed in the collection.");

            base.SetItem(index, item);
        }
    }
}
