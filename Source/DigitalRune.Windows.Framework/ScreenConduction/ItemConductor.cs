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
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using DigitalRune.Linq;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// A <see cref="IConductor"/> that holds on to and activates one item at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="ItemConductor"/> holds a single item - similar to a
    /// <see cref="ContentControl"/>. The currently active item can be changed by either calling
    /// <see cref="ActivateItemAsync"/>/ <see cref="DeactivateItemAsync"/>. Changing the active item
    /// will close any previous item. (Items cannot be deactivated temporarily because the
    /// <see cref="ItemConductor"/> does not maintain a collection of items. Deactivated items are
    /// always closed.)
    /// </para>
    /// <para>
    /// The <see cref="ItemConductor"/> is itself a <see cref="Screen"/>, which can be activated or
    /// deactivated. When the <see cref="ItemConductor"/> is activated/deactivated, its current 
    /// <see cref="Item"/> is activated/deactivated too.
    /// </para>
    /// <para>
    /// The conductor respects the interface <see cref="IGuardClose"/>. Items that implement this
    /// interface may prevent closing. It is therefore not always guaranteed that closing or
    /// activating another item will succeed.
    /// </para>
    /// </remarks>
    public class ItemConductor : Screen, IConductor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerable<object> IConductor.Items
        {
            get { return Item != null ? LinqHelper.Return(Item) : Enumerable.Empty<object>(); }
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        IEnumerable<object> IConductor.ActiveItems
        {
            get { return Item != null ? LinqHelper.Return(Item) : Enumerable.Empty<object>(); }
        }


        /// <summary>
        /// Gets the currently active item.
        /// </summary>
        /// <value>The currently active item.</value>
        public object Item { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <param name="item">
        /// The item to activate. Can be <see langword="null"/>.
        /// </param>
        /// <inheritdoc />
        public async Task<bool> ActivateItemAsync(object item)
        {
            if (Equals(item, Item))
            {
                // The specified item is already selected.
                if (IsActive)
                    (item as IActivatable)?.OnActivate();

                return true;
            }

            // Check whether item can be closed.
            var guardClose = Item as IGuardClose;
            bool canClose = (guardClose != null) ? await guardClose.CanCloseAsync() : true;
            if (canClose)
            {
                // Close previous item and activate the new item.
                ChangeActiveItem(item);
                return true;
            }

            return false;
        }


        /// <inheritdoc/>
        public async Task<bool> DeactivateItemAsync(object item, bool close)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (!item.Equals(Item))
                throw new ArgumentException("Item is not controlled by this conductor.", nameof(item));
            if (!close)
                throw new InvalidOperationException("The ItemConductor does not support temporary deactivations. Items can only be closed.");

            // Check whether item can be closed.
            var guardClose = Item as IGuardClose;
            bool canClose = (guardClose != null) ? await guardClose.CanCloseAsync() : true;
            if (canClose)
            {
                // Close item.
                ChangeActiveItem(null);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Immediately changes the active item.
        /// </summary>
        /// <param name="newItem">The new item to activate.</param>
        private void ChangeActiveItem(object newItem)
        {
            // Deactivate previous item.
            (Item as IActivatable)?.OnDeactivate(true);

            // Clear conductor.
            var screen = Item as IScreen;
            if (screen != null)
                screen.Conductor = null;

            // Change active item.
            Item = newItem;

            // Set conductor.
            screen = newItem as IScreen;
            if (screen != null)
                screen.Conductor = this;

            // Activate new item.
            if (IsActive)
                (newItem as IActivatable)?.OnActivate();

            RaisePropertyChanged(() => Item);
        }


        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            (Item as IActivatable)?.OnActivate();
            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            (Item as IActivatable)?.OnDeactivate(eventArgs.Closed);
            base.OnDeactivated(eventArgs);
        }


        /// <inheritdoc/>
        public override Task<bool> CanCloseAsync()
        {
            var guardClose = Item as IGuardClose;
            return (guardClose != null) ? guardClose.CanCloseAsync() : TaskHelper.FromResult(true);
        }
        #endregion
    }
}
