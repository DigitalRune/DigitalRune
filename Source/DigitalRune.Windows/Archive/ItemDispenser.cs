// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Markup;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Manages a pool of items from which the user can allocate items.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <remarks>
    /// <para>
    /// The items in the pool can be set using the property <see cref="Items"/>. The items are
    /// either available or in use. The number of items can be queried with the properties
    /// <see cref="TotalNumberOfItems"/>, <see cref="NumberOfItemsAvailable"/>, and
    /// <see cref="NumberOfItemsInUse"/>. The user can allocate an item by calling
    /// <see cref="NextItem"/>. This will retrieve the next available item and mark it as 'in use'.
    /// By calling <see cref="ReleaseItem"/> the user can release an item and make in available
    /// again.
    /// </para>
    /// <para>
    /// For example: The user has chart were each series should be drawn with a different pen. The
    /// <see cref="ItemDispenser{T}"/> could manage all available pens. Each series gets its pen
    /// from the <see cref="ItemDispenser{T}"/> by calling <see cref="NextItem"/>. When a series is
    /// removed from the chart it returns its pen back to the pool by calling
    /// <see cref="ReleaseItem"/>. In this way all series are automatically drawn in distinct
    /// colors. The user needs to make sure that the <see cref="ItemDispenser{T}"/> has enough pens
    /// stored. Otherwise, when more there are more series than pens the
    /// <see cref="ItemDispenser{T}"/> throws a <see cref="ItemNotAvailableException"/> when it runs
    /// out of pens.
    /// </para>
    /// </remarks>
    [ContentProperty("Items")]
    public class ItemDispenser<T>
    {
        // TODO: Rename ItemDispenser to ItemPool or ObjectPool.
        // TODO: Rename NextItem/ReleaseItem to Obtain/Acquire/Release/Return...
        // TODO: Test if ItemDispensers can be defined in XAML. 
        //       E.g. It should be possible to define StyleDispenser completely in XAML without code.
        // TODO: Find a way to use an ItemDispenser to give items to the items of an ItemsControl.
        //       E.g. ItemsControl of LineCharts. Each LineChart should get a unique style!?


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private IList<T> _items;

        // Note: We could use a HashSet<T>. However, Silverlight 3 does not yet support HashSets.
        private readonly Dictionary<T, bool> _usedItems = new Dictionary<T, bool>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the items in the pool.
        /// </summary>
        /// <value>The items in the pool. The default value is an empty collection.</value>
        /// <remarks>
        /// The collection can be changed, items can be added or removed at any time. However, items
        /// that are already in use will stay in use until the currently owning object releases the
        /// item. Other objects cannot be forced to release an item.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IList<T> Items
        {
            get { return _items; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (value != _items)
                {
                    _items = value;
                    _usedItems.Clear();
                }
            }
        }


        /// <summary>
        /// Gets the number of available items.
        /// </summary>
        /// <value>The number of available items.</value>
        public int NumberOfItemsAvailable
        {
            get { return _items.Count(item => !_usedItems.ContainsKey(item)); }
        }


        /// <summary>
        /// Gets the number of items that are in use.
        /// </summary>
        /// <value>The number of items in use.</value>
        public int NumberOfItemsInUse
        {
            get { return _items.Count(item => _usedItems.ContainsKey(item)); }
        }


        /// <summary>
        /// Gets the number of available items.
        /// </summary>
        /// <value>The number of available items.</value>
        public int TotalNumberOfItems
        {
            get { return _items.Count; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemDispenser{T}"/> class.
        /// </summary>
        public ItemDispenser()
        {
            _items = new List<T>();
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ItemDispenser{T}"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        public ItemDispenser(IEnumerable<T> items)
        {
            _items = new List<T>(items);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Retrieves the next available item and marks it as 'in use'.
        /// </summary>
        /// <returns>The next available item.</returns>
        /// <exception cref="ItemNotAvailableException">
        /// No more items are available.
        /// </exception>
        public T NextItem()
        {
            Cleanup();

            if (_items.Count == 0)
                throw new ItemNotAvailableException("No item available. The list of items is empty.");

            if (_items.Count == _usedItems.Count)
                throw new ItemNotAvailableException("No item available. All items are in use.");

            IEnumerable<T> availableItems = _items.Where(item => item != null && !_usedItems.ContainsKey(item));
            Debug.Assert(availableItems.Count() > 0);
            T nextItem = availableItems.First();
            _usedItems.Add(nextItem, true);
            return nextItem;
        }


        /// <summary>
        /// Releases the specified item and marks it as 'available'.
        /// </summary>
        /// <param name="item">The item to release.</param>
        /// <remarks>
        /// The methods simply returns when <paramref name="item"/> is not found in the pool or
        /// already is available. (No exception is thrown.)
        /// </remarks>
        public void ReleaseItem(T item)
        {
            Cleanup();
            _usedItems.Remove(item);
        }


        /// <summary>
        /// Cleans up the pool and removes any orphaned references.
        /// </summary>
        private void Cleanup()
        {
            // If an item has been removed from _items it needs to be removed from _usedItems to
            // prevent memory leaks.
            T[] orphanedItems = _usedItems.Keys.Except(_items).ToArray();

            foreach (T orphanedItem in orphanedItems)
                _usedItems.Remove(orphanedItem);
        }


        /// <summary>
        /// Releases all items and marks them as 'available'.
        /// </summary>
        public void ReleaseAllItems()
        {
            _usedItems.Clear();
        }
        #endregion
    }
}
