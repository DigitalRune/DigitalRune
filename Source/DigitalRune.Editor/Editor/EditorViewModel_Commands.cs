// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DigitalRune.Collections;


namespace DigitalRune.Editor
{
    public partial class EditorViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Merge DockTabItem context menu nodes and create the menu items. (Note: The managers need
        // to be kept alive because they manage the visibility of the items but are weak event
        // listeners.)
        internal MenuManager MenuManager;       // Used in EditorWindow.
        internal ToolBarManager ToolBarManager; // Used in EditorWindow.
        private MenuManager _contextMenuManager;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public ICollection<MergeableNodeCollection<ICommandItem>> MenuNodeCollections { get; } = new Collection<MergeableNodeCollection<ICommandItem>>();


        /// <inheritdoc/>
        public ICollection<MergeableNodeCollection<ICommandItem>> ToolBarNodeCollections { get; } = new Collection<MergeableNodeCollection<ICommandItem>>();


        /// <inheritdoc/>
        public ICollection<MergeableNodeCollection<ICommandItem>> DockContextMenuNodeCollections { get; } = new Collection<MergeableNodeCollection<ICommandItem>>();


        /// <inheritdoc/>
        public MenuItemViewModelCollection Menu
        {
            get { return MenuManager.Menu; }
        }


        /// <inheritdoc/>
        public ToolBarViewModelCollection ToolBars
        {
            get { return ToolBarManager.ToolBars; }
        }


        /// <inheritdoc/>
        public MenuItemViewModelCollection DockContextMenu
        {
            get { return _contextMenuManager.Menu; }
        }


        /// <inheritdoc/>
        public MenuItemViewModelCollection ToolBarContextMenu { get; } = new MenuItemViewModelCollection();


        /// <inheritdoc/>
        public event EventHandler<EventArgs> UIInvalidated;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        private void InitializeCommandItems()
        {
            MenuManager = new MenuManager();
            ToolBarManager = new ToolBarManager();
            _contextMenuManager = new MenuManager();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void InvalidateUI()
        {
            Logger.Debug("Invalidating UI.");

            // ----- Update menu
            MenuManager.Update(MenuNodeCollections);

            // ----- Update toolbars
            // Backup previous toolbars to restore layout.
            var previousToolBars = ToolBars?.ToArray();

            // Create new toolbars.
            ToolBarManager.Update(ToolBarNodeCollections);

            // Copy Band/BandIndex/Visibility from old view models to new view models.
            if (previousToolBars != null)
            {
                foreach (var toolBar in ToolBars)
                {
                    var previousToolbar = previousToolBars.FirstOrDefault(tb => tb.CommandGroup.Name == toolBar.CommandGroup.Name);
                    if (previousToolbar != null)
                    {
                        // Restore layout.
                        toolBar.Band = previousToolbar.Band;
                        toolBar.BandIndex = previousToolbar.BandIndex;
                        toolBar.IsVisible = previousToolbar.IsVisible;
                    }
                }
            }

            // ----- Update IDockTabItem context menu
            _contextMenuManager.Update(DockContextMenuNodeCollections);

            // ----- Update input bindings
            UpdateInputAndCommandBindings();

            OnUIInvalidated(EventArgs.Empty);
        }


        /// <summary>
        /// Raises the <see cref="UIInvalidated"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUIInvalidated"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUIInvalidated"/> method so
        /// that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnUIInvalidated(EventArgs eventArgs)
        {
            UIInvalidated?.Invoke(this, eventArgs);
        }
        #endregion
    }
}
