// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DigitalRune.Linq;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor
{
    partial class EditorViewModel
    {
        // ----- Screen conduction:
        // The EditorViewModel conducts the EditorTabItemViewModels.
        // - IScreen.Conductor is set in EditorDockTabPaneViewModel.
        // - IScreen.Conductor is reset in EditorDockStrategy.
        // - IActivatable.OnActivate/OnDeactivate is handled in EditorDockTabPaneViewModel.
        // - IActivatable.OnDeactivate(close = true) is handled in EditorDockStrategy.
        // - IGuardClose is handled in EditorDockStrategy.
        // - Screen conduction when loading a new layout is handled in LoadLayout().
        // - The rest is handled here.
        //
        // IActivatable.OnActivate/OnDeactivate and IScreen.Conductor may be called/set redundantly.
        // (Redundant calls should have no effect - see base class Screen.)
        //
        // IMPORTANT: To ensure proper screen conduction, all dock operation should be handled by
        // the EditorDockStrategy.


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        IEnumerable<object> IConductor.Items
        {
            get { return GetItems(false); }
        }


        /// <inheritdoc/>
        IEnumerable<object> IConductor.ActiveItems
        {
            get { return GetItems(true); }
        }


        /// <inheritdoc/>
        public IEnumerable<EditorDockTabItemViewModel> Items
        {
            get { return GetItems(false); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private IEnumerable<EditorDockTabItemViewModel> GetItems(bool onlyActive)
        {
            var items = this.GetDockElements()
                            .OfType<EditorDockTabItemViewModel>()
                            .Where(item => item.DockState != DockState.Hide);

            if (onlyActive)
                items = items.Where(item => item.IsActive);

            return items.Distinct();
        }


        /// <inheritdoc/>
        Task<bool> IConductor.ActivateItemAsync(object item)
        {
            // Explicit interface implementation because this method is not really async.

            ActivateItem(item);
            return TaskHelper.FromResult(true);
        }


        /// <inheritdoc/>
        public void ActivateItem(object item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var dockTabItem = item as IDockTabItem;
            if (dockTabItem == null)
                throw new ArgumentException("Item needs to implement IDockTabItem");

            DockStrategy.Show(dockTabItem);

#if DEBUG
            // Validation
            var activatable = dockTabItem as IActivatable;
            if (activatable != null)
            {
                Debug.Assert(activatable.IsOpen);
                Debug.Assert(activatable.IsActive);
                Debug.Assert(GetItems(true).Contains(item));
            }

            var screen = dockTabItem as IScreen;
            if (screen != null)
                Debug.Assert(screen.Conductor == this);
#endif
        }


        /// <inheritdoc/>
        public Task<bool> DeactivateItemAsync(object item, bool close)
        {
            var dockTabItem = item as IDockTabItem;
            if (dockTabItem == null
                || !close   // Temporary deactivation is not supported.
                || !DockStrategy.CanClose(dockTabItem))
            {
                return TaskHelper.FromResult(false);
            }

            DockStrategy.Close(dockTabItem);

#if DEBUG
            // Validation
            var activatable = dockTabItem as IActivatable;
            if (activatable != null)
            {
                Debug.Assert(!activatable.IsActive);
                Debug.Assert(!activatable.IsOpen);
                Debug.Assert(!GetItems(false).Contains(item));
            }

            var screen = dockTabItem as IScreen;
            if (screen != null)
                Debug.Assert(screen.Conductor == null);
#endif

            return TaskHelper.FromResult(true);
        }


        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            base.OnActivated(eventArgs);

            // Activate all selected items.
            this.GetDockElements()
                .OfType<IDockTabPane>()
                .Where(dockTabPane => dockTabPane.IsVisible)
                .Select(dockTabPane => dockTabPane.SelectedItem)
                .OfType<IActivatable>()
                .ForEach(activatable => activatable.OnActivate());

            if (eventArgs.Opened)
            {
                // Note: Some extensions register command bindings when in 
                // EditorViewModel.Activated. Therefore, base.OnActivate must be called before this.
                UpdateInputAndCommandBindings();
            }
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                // Close all items.
                var dockTabItems = this.GetDockElements()
                                       .OfType<IDockTabItem>()
                                       .Distinct();
                foreach (var dockTabItem in dockTabItems)
                {
                    (dockTabItem as IActivatable)?.OnDeactivate(true);

                    var screen = dockTabItem as IScreen;
                    if (screen != null)
                        screen.Conductor = null;
                }
            }
            else
            {
                // Deactivate all selected items.
                this.GetDockElements()
                    .OfType<IDockTabPane>()
                    .Where(dockTabPane => dockTabPane.IsVisible)
                    .Select(dockTabPane => dockTabPane.SelectedItem)
                    .OfType<IActivatable>()
                    .ForEach(activatable => activatable.OnDeactivate(false));
            }

            base.OnDeactivated(eventArgs);
        }


        /// <inheritdoc/>
        public override async Task<bool> CanCloseAsync()
        {
            foreach (var extension in OrderedExtensions.OfType<IGuardClose>())
            {
                bool canClose = await extension.CanCloseAsync();
                if (!canClose)
                    return false;
            }

            foreach (var item in Items)
            {
                bool canClose = await item.CanCloseAsync();
                if (!canClose)
                    return false;
            }

            return await base.CanCloseAsync();
        }
        #endregion
    }
}
