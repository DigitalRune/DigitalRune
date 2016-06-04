// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Linq;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Manages a collection of search scopes.
    /// </summary>
    /// <remarks>
    /// <see langword="null"/> or duplicate entries are not allowed in the collection. If a search
    /// scope is removed, the <see cref="SearchQuery.Scope"/> property of the
    /// <see cref="SearchQuery"/> is updated if necessary.
    /// </remarks>
    internal class SearchScopeCollection : Collection<ISearchScope>
    {
        private readonly SearchExtension _searchExtension;


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchScopeCollection"/> class.
        /// </summary>
        /// <param name="searchExtension">The search extension.</param>
        public SearchScopeCollection(SearchExtension searchExtension)
        {
            _searchExtension = searchExtension;
        }


        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// </summary>
        protected override void ClearItems()
        {
            base.ClearItems();
            _searchExtension.Query.Scope = null;
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
        protected override void InsertItem(int index, ISearchScope item)
        {
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
            var removedScope = Items[index];

            base.RemoveItem(index);

            // Update SearchQuery.Scope.
            if (removedScope == _searchExtension.Query.Scope)
                _searchExtension.Query.Scope = this.FirstOrDefault();
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
        protected override void SetItem(int index, ISearchScope item)
        {
            var removedScope = Items[index];
            if (removedScope == item)
                return;

            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (Contains(item))
                throw new ArgumentException("Duplicate items are not allowed in the collection.", nameof(item));

            base.SetItem(index, item);

            // Update SearchQuery.Scope.
            if (_searchExtension.Query.Scope == removedScope)
                _searchExtension.Query.Scope = item;
        }
    }
}
