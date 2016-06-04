// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Manages a collection of <see cref="EditorExtension"/>s.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> or duplicate entries are not allowed in the collection.
    /// </remarks>
    public class EditorExtensionCollection : Collection<EditorExtension>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the collection is locked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the collection is locked; otherwise, <see langword="false"/>.
        /// </value>
        internal bool IsLocked { get; set; }


        private void ThrowIfLocked()
        {
            if (IsLocked)
                throw new EditorException("Adding/removing extensions is not allowed after the editor was started.");
        }


        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            ThrowIfLocked();
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
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection.
        /// </exception>
        protected override void InsertItem(int index, EditorExtension item)
        {
            ThrowIfLocked();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (Contains(item))
                throw new ArgumentException("Duplicate items are not allowed in the collection.", nameof(item));

            base.InsertItem(index, item);
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
            ThrowIfLocked();
            base.RemoveItem(index);
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
        /// <exception cref="ArgumentException">
        /// <paramref name="item"/> is already contained in the collection.
        /// </exception>
        protected override void SetItem(int index, EditorExtension item)
        {
            if (Items[index] == item)
                return;

            ThrowIfLocked();
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (Contains(item))
                throw new ArgumentException("Duplicate items are not allowed in the collection.", nameof(item));

            base.SetItem(index, item);
        }
    }
}
