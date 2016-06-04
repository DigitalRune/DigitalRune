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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using DigitalRune.Linq;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A <see cref="IConductor"/> that holds on to many items which are all activated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="AllActiveItemsConductor"/> maintains a collection of items. All contained items
    /// are active/visible when the <see cref="AllActiveItemsConductor"/> is active - similar to an 
    /// <see cref="ItemsControl"/>.
    /// </para>
    /// <para>
    /// The <see cref="ItemConductor"/> is itself a <see cref="Screen"/>, which can be activated or
    /// deactivated. When the <see cref="ItemConductor"/> is activated/deactivated, its current 
    /// <see cref="Items"/> are activated/deactivated too.
    /// </para>
    /// <para>
    /// The methods <see cref="ActivateItem"/> and <see cref="DeactivateItemAsync"/> can be
    /// called to add or remove items.
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
    public class AllActiveItemsConductor : Screen, IConductor
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
            get { return Items; }
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
        public BindableCollection<object> Items { get; }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerable<object> IConductor.ActiveItems
        {
            get { return Items; }
        }


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
        /// Initializes a new instance of the <see cref="AllActiveItemsConductor"/> class.
        /// </summary>
        public AllActiveItemsConductor()
        {
            RemoveItemsOnCanClose = true;
            RemoveItemsOnClose = true;

            Items = new BindableCollection<object>();
            Items.CollectionChanged += OnItemsCollectionsChanged;
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
                    Items.OfType<IScreen>().ForEach(item => item.Conductor = this);
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
        /// <param name="item">The item to activate. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> is <see langword="null"/>.
        /// </exception>
        public void ActivateItem(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var index = Items.IndexOf(item);
            if (index == -1)
            {
                // Add new item to Items collections.
                Items.Add(item);
            }
            else
            {
                // Get instance from collection in case Equals()-method is overridden.
                item = Items[index];
            }

            // Activate the new item.
            if (IsActive)
                (item as IActivatable)?.OnActivate();
        }

        /// <inheritdoc/>
        public async Task<bool> DeactivateItemAsync(object item, bool close)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!Items.Contains(item))
                throw new ArgumentException("IItem is not controlled by this conductor.", nameof(item));
            if (close == false)
                throw new InvalidOperationException("The AllActiveItemsConductor does not support temporary deactivations. Items can only be closed.");

            // Check whether item can be closed.
            var guardClose = item as IGuardClose;
            bool canClose = (guardClose != null) ? await guardClose.CanCloseAsync() : true;
            if (canClose)
            {
                // Close item.
                (item as IActivatable)?.OnDeactivate(true);
                Items.Remove(item);
                return true;
            }

            return false;
        }


        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            Items.OfType<IActivatable>().ForEach(item => item.OnActivate());
            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                Items.OfType<IActivatable>().ForEach(item => item.OnDeactivate(true));
                if (RemoveItemsOnClose)
                {
                    Items.OfType<IScreen>().ForEach(item => item.Conductor = null);
                    Items.Clear();
                }
            }
            else
            {
                Items.OfType<IActivatable>().ForEach(item => item.OnDeactivate(false));
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
                {
                    (item as IActivatable)?.OnDeactivate(true);
                    Items.Remove(item);
                }
            }

            return true;
        }
        #endregion
    }
}
