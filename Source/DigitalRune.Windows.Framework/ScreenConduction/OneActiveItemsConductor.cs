// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#region ----- Credits -----
/*
  The "screen conduction" pattern implemented in DigitalRune.Windows.Framework was 
  inspired by the Caliburn.Micro framework (see http://caliburnmicro.codeplex.com/).
*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DigitalRune.Linq;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A <see cref="IConductor"/> that holds on to many items but only activates on at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="OneActiveItemsConductor"/> maintains a collection of items, but max one item
    /// will be active at any time - similar to a <strong>TabControl</strong>.
    /// </para>
    /// <para>
    /// The property <see cref="ActiveItem"/> indicates which item is currently active. When a new
    /// item is added by calling <see cref="ActivateItem"/>, it will be added to the items
    /// collection and will become the new active item. The previously active item will be
    /// deactivated, but it will remain in the items collection.
    /// </para>
    /// <para>
    /// The currently active item can be explicitly deactivated by calling
    /// <see cref="DeactivateItemAsync"/>. This will automatically activate another item. The method
    /// <see cref="GetNextItemToActivate"/> can be overwritten to use a custom strategy for choosing
    /// the next active item.
    /// </para>
    /// <para>
    /// Items can be either deactivated or closed using <see cref="DeactivateItemAsync"/>:
    /// Deactivated items won't be visible, but will remain in the items collection. Closed items
    /// will be removed from the items collection.
    /// </para>
    /// <para>
    /// Note that items can also be added or removed by adding them to or removing them from the
    /// <see cref="Items"/> collection. However, these item won't be activated or deactivated
    /// properly. It is recommended to call <see cref="ActivateItem"/> or
    /// <see cref="DeactivateItemAsync"/> instead of directly manipulation the <see cref="Items"/>
    /// collection.
    /// </para>
    /// <para>
    /// The conductor respects the interface <see cref="IGuardClose"/>. Items that implement this
    /// interface may prevent closing. It is therefore not always guaranteed that closing or
    /// activating another item will succeed.
    /// </para>
    /// <para>
    /// The method <see cref="CanCloseAsync"/> checks whether all items can be closed. Note that, by
    /// default, the method immediately closes all items that can be closed. This behavior can be
    /// changed by settings <see cref="RemoveItemsOnCanClose"/> to <see langword="false"/>; or by
    /// overriding the method <see cref="CanCloseAsync"/>.
    /// </para>
    /// </remarks>
    public class OneActiveItemsConductor : Screen, IConductor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        IEnumerable<object> IConductor.Items
        {
            get { return _items; }
        }


        /// <summary>
        /// Gets the items that are controlled by this conductor.
        /// </summary>
        /// <value>A collection of items that are conducted by this conductor.</value>
        /// <remarks>
        /// When items are added or removed from the <see cref="Items"/> collection, they will not
        /// be properly activated or deactivated. It is recommended to add or remove items by
        /// calling <see cref="ActivateItem"/> or <see cref="DeactivateItemAsync"/>.
        /// </remarks>
        public BindableCollection<object> Items
        {
            get { return _items; }
        }
        private readonly BindableCollection<object> _items;


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerable<object> IConductor.ActiveItems
        {
            get { return _activeItem != null ? LinqHelper.Return(_activeItem) : Enumerable.Empty<object>(); }
        }


        /// <summary>
        /// Gets or sets the currently active item.
        /// </summary>
        /// <value>The currently active item.</value>
        public object ActiveItem
        {
            get { return _activeItem; }
            set { ActivateItem(value); }
        }
        private object _activeItem;


        /// <summary>
        /// Gets or sets a value indicating whether the conductor should immediately remove items
        /// that can be closed in <see cref="CanCloseAsync"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to remove items that can be closed in
        /// <see cref="CanCloseAsync"/>; otherwise, <see langword="false"/>. The default value is
        /// <see langword="true"/>.
        /// </value>
        protected bool RemoveItemsOnCanClose { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the conductor should immediately remove all
        /// items when it is being closed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to remove the items when the conductor is closed; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        protected bool RemoveItemsOnClose { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="OneActiveItemsConductor"/> class.
        /// </summary>
        public OneActiveItemsConductor()
        {
            RemoveItemsOnCanClose = true;
            RemoveItemsOnClose = true;

            _items = new BindableCollection<object>();
            _items.CollectionChanged += OnItemsCollectionsChanged;
        }


        private void OnItemsCollectionsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    eventArgs.NewItems.OfType<IScreen>().ForEach(item => item.Conductor = this);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    eventArgs.OldItems.OfType<IScreen>().ForEach(item => item.Conductor = null);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    eventArgs.OldItems.OfType<IScreen>().ForEach(item => item.Conductor = null);
                    eventArgs.NewItems.OfType<IScreen>().ForEach(item => item.Conductor = this);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // Sadly, the ObservableCollection does not report removed items when reset.
                    // Therefore we cannot reset the conductor of the removed items. :-(
                    _items.OfType<IScreen>().ForEach(item => item.Conductor = this);
                    break;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        Task<bool> IConductor.ActivateItemAsync(object item)
        {
            // Explicit interface implementation because this method is not really async.

            ActivateItem(item);
            return TaskHelper.FromResult(true);
        }


        /// <summary>
        /// Activates the specified item.
        /// </summary>
        /// <param name="item">The item to activate. Can be <see langword="null"/>.</param>
        public void ActivateItem(object item)
        {
            if (item != null && item.Equals(_activeItem))
            {
                // The specified item is already selected.
                if (IsActive)
                    (item as IActivatable)?.OnActivate();
            }
            else
            {
                // Deactivate previous item and activate the new item.
                ChangeActiveItem(item, false);
            }
        }


        /// <inheritdoc/>
        public async Task<bool> DeactivateItemAsync(object item, bool close)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!_items.Contains(item))
                throw new ArgumentException("Item is not controlled by this conductor.", nameof(item));

            if (!close)
            {
                // Temporarily deactivate the specified item.
                if (!item.Equals(_activeItem))
                {
                    // The specified item is already deactivated.
                    return true;
                }

                // Deactivate item by activating another item.
                var nextItem = GetNextItemToActivate(_items, item);
                if (nextItem != null)
                {
                    ChangeActiveItem(nextItem, false);
                    return true;
                }

                // Item is the only item and cannot be deactivated.
                return false;
            }

            // Check whether item can be closed.
            var guardClose = item as IGuardClose;
            bool canClose = (guardClose != null) ? await guardClose.CanCloseAsync() : true;
            if (canClose)
            {
                // Close item.
                CloseItem(item);
                return true;
            }

            return false;
        }


        private void CloseItem(object item)
        {
            if (item.Equals(_activeItem))
            {
                var next = GetNextItemToActivate(_items, item);
                ChangeActiveItem(next, true);
            }
            else
            {
                (item as IActivatable)?.OnDeactivate(true);
            }

            _items.Remove(item);
        }


        /// <summary>
        /// Immediately changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        /// <param name="closePrevious">
        /// <see langword="true"/> if the previous item is being closed; <see langword="false"/> if the 
        /// previous item is only deactivated temporarily.
        /// </param>
        private void ChangeActiveItem(object newItem, bool closePrevious)
        {
            // Deactivate previous item.
            (_activeItem as IActivatable)?.OnDeactivate(closePrevious);

            if (newItem == null)
            {
                newItem = GetNextItemToActivate(_items, _activeItem);
            }
            else
            {
                var index = _items.IndexOf(newItem);
                if (index == -1)
                {
                    // Add new item to Items collection.
                    _items.Add(newItem);
                }
                else
                {
                    // Get instance from collection in case Equals()-method is overridden.
                    newItem = _items[index];
                }
            }

            // Change active item.
            _activeItem = newItem;

            // Activate new item.
            if (IsActive)
                (newItem as IActivatable)?.OnActivate();

            RaisePropertyChanged(() => ActiveItem);
        }


        /// <summary>
        /// Determines the next item to be activated.
        /// </summary>
        /// <param name="items">The items collection.</param>
        /// <param name="activeItem">The currently active item.</param>
        /// <returns>The next item to be activated.</returns>
        protected virtual object GetNextItemToActivate(IList items, object activeItem)
        {
            if (items == null)
                return null;

            // Get index of currently active item.
            int index = items.IndexOf(activeItem);

            if (index > 0)
            {
                // Active item is in the middle of the collection.
                // --> Return previous item.
                return items[index - 1];
            }

            if (index == 0)
            {
                // Active item is the first item in the collection.
                // --> Return next item.
                return (items.Count > 1) ? items[1] : null;
            }

            Debug.Assert(index < 0, "Sanity check.");

            // Active item is not in the list.
            // --> Return first available item.
            return (items.Count > 0) ? items[0] : null;
        }


        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            (_activeItem as IActivatable)?.OnActivate();
            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                _items.OfType<IActivatable>().ForEach(item => item.OnDeactivate(true));
                if (RemoveItemsOnClose)
                {
                    _items.OfType<IScreen>().ForEach(item => item.Conductor = null);
                    _items.Clear();
                }
            }
            else
            {
                (_activeItem as IActivatable)?.OnDeactivate(false);
            }

            base.OnDeactivated(eventArgs);
        }


        /// <inheritdoc/>
        public override async Task<bool> CanCloseAsync()
        {
            foreach (var item in Items.OfType<IGuardClose>().ToArray())
            {
                bool canClose = await item.CanCloseAsync();
                if (!canClose)
                    return false;

                if (RemoveItemsOnCanClose)
                    CloseItem(item);
            }

            return true;
        }
        #endregion
    }
}
