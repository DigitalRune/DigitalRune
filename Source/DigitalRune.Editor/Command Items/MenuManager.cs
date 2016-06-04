// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using DigitalRune.Collections;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Merges and manages a collection of command item nodes that represent menu items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Menus in the editor are created dynamically. Every editor extension can register
    /// <see cref="MergeableNode{T}"/> instances containing <see cref="ICommandItem"/>s. This class
    /// merges the nodes and stores the merged collection in <see cref="Nodes"/>. The
    /// <see cref="ICommandItem"/>s are also stores in <see cref="CommandItems"/> for convenience.
    /// The <see cref="MenuManager"/> creates menu view models and stores them in
    /// <see cref="Menu"/>.
    /// </para>
    /// <para>
    /// The nodes are merged in <see cref="Update"/>. When content of the input node collection was
    /// changed, <see cref="Update"/> can be called again to update the merged collections. The
    /// merged collections (<see cref="Nodes"/>, <see cref="CommandItems"/> and
    /// <see cref="Menu"/>) are created once when the <see cref="MenuManager"/> is created.
    /// <see cref="Update"/> changes only the content of the collections.
    /// </para>
    /// <para>
    /// This class also observes the command item visibility and updates the menu items, e.g. to
    /// hide empty menus and unnecessary separators.
    /// </para>
    /// </remarks>
    /// <seealso cref="ToolBarManager"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    internal class MenuManager : IWeakEventListener
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly Action _coerceVisibilityAction;
        private DispatcherOperation _coerceVisibilityOperation;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// The merged command item nodes.
        /// </summary>
        /// <value>The merged command item nodes.</value>
        /// <remarks>
        /// This collection is the result of the merge operation.
        /// </remarks>
        public MergeableNodeCollection<ICommandItem> Nodes { get; }


        /// <summary>
        /// Gets the command items.
        /// </summary>
        /// <value>The command items.</value>
        /// <remarks>
        /// <para>
        /// This list contains all command items that are contained in the resulting command item
        /// nodes (<see cref="Nodes"/>).
        /// </para>
        /// <para>
        /// This collection does not store <see cref="CommandSeparator"/>s or
        /// <see cref="CommandGroup"/>s.
        /// </para>
        /// </remarks>
        public List<ICommandItem> CommandItems { get; }


        /// <summary>
        /// Gets the menu items.
        /// </summary>
        /// <value>The menu items.</value>
        public MenuItemViewModelCollection Menu { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuManager"/> class.
        /// </summary>
        public MenuManager()
        {
            _coerceVisibilityAction = CoerceVisibility;
            Nodes = new MergeableNodeCollection<ICommandItem>();
            CommandItems = new List<ICommandItem>();
            Menu = new MenuItemViewModelCollection();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Merges the node collections.
        /// </summary>
        /// <param name="nodeCollections">
        /// The collections of command item nodes that define the menu.
        /// </param>
        public void Update(params IEnumerable<MergeableNodeCollection<ICommandItem>>[] nodeCollections)
        {
            // Remove event handlers.
            foreach (var item in CommandItems)
                PropertyChangedEventManager.RemoveListener(item, this, nameof(ICommandItem.IsVisible));

            // Merge all command item collections.
            Nodes.Clear();
            MergeNodes(Nodes, nodeCollections);

            // Remove empty menus.
            RemoveEmptyGroups(Nodes);

            // Add items to CommandItems list.
            CommandItems.Clear();
            CollectCommandItems(Nodes);

            // Create menu items.
            Menu.Clear();
            CreateMenuItems(Nodes, Menu);

            // Coerce visibility of all newly generated items.
            CoerceVisibility();

            // Add event handler to command items to observe visibility changes.
            foreach (var item in CommandItems)
                PropertyChangedEventManager.AddListener(item, this, nameof(ICommandItem.IsVisible));
        }


        private static void MergeNodes(MergeableNodeCollection<ICommandItem> nodes, params IEnumerable<MergeableNodeCollection<ICommandItem>>[] nodeCollections)
        {
            var mergeAlgorithm = new MergeAlgorithm<ICommandItem> { CloneNodesOnMerge = true };
            foreach (var nodeCollection in nodeCollections)
                foreach (var additionalNodes in nodeCollection)
                    mergeAlgorithm.Merge(nodes, additionalNodes);
        }


        private static void RemoveEmptyGroups(MergeableNodeCollection<ICommandItem> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            foreach (var node in nodes.ToArray())
            {
                // Recursively remove empty groups in children.
                RemoveEmptyGroups(node.Children);

                if (node.Content is CommandGroup)
                {
                    // Count all children, except separators.
                    int numberOfItems = node.Children?.Count(n => !(n.Content is CommandSeparator)) ?? 0;
                    if (numberOfItems == 0)
                    {
                        // node is a group node and contains no children or only separators.
                        // --> Remove this node.
                        nodes.Remove(node);
                    }
                }
            }
        }


        private void CollectCommandItems(MergeableNodeCollection<ICommandItem> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return;

            // Recursively get all nodes (excluding separators).
            foreach (var node in nodes)
            {
                if (node.Content is CommandSeparator)
                {
                    // Ignore
                }
                else if (node.Content is CommandGroup)
                {
                    // Ignore
                }
                else
                {
                    Debug.Assert(!CommandItems.Contains(node.Content), "Warning: Duplicate command items in menu.");
                    CommandItems.Add(node.Content);
                }

                CollectCommandItems(node.Children);
            }
        }


        private static void CreateMenuItems(MergeableNodeCollection<ICommandItem> nodes, MenuItemViewModelCollection menuItems)
        {
            foreach (var node in nodes)
            {
                if (node?.Content == null)
                    continue;

                var menuItem = node.Content.CreateMenuItem();
                menuItems.Add(menuItem);

                if (node.Children != null && node.Children.Count > 0)
                {
                    menuItem.Submenu = new MenuItemViewModelCollection();
                    CreateMenuItems(node.Children, menuItem.Submenu);
                }
            }
        }


        /// <summary>
        /// Receives the weak event.
        /// </summary>
        /// <param name="managerType">
        /// The type of the <see cref="WeakEventManager"/> calling this method.
        /// </param>
        /// <param name="sender">The object that originated the event.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the listener handled the event. It is considered an error by
        /// the <see cref="WeakEventManager"/> handling in WPF to register a listener for an event
        /// that the listener does not handle. Regardless, the method should return
        /// <see langword="false"/> if it receives an event that it does not recognize or handle.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs eventArgs)
        {
            var propertyChangedEventArgs = eventArgs as PropertyChangedEventArgs;
            if (propertyChangedEventArgs == null)
                return false;

            Debug.Assert(propertyChangedEventArgs.PropertyName == nameof(ICommandItem.IsVisible), "Unexpected PropertyChanged event occurred.");

            // Avoid unnecessary updates.
            if (_coerceVisibilityOperation != null && _coerceVisibilityOperation.Status == DispatcherOperationStatus.Pending)
                return true;

            // Do not take action immediately. There could be several visibility changes coming.
            _coerceVisibilityOperation = WindowsHelper.Dispatcher.BeginInvoke(_coerceVisibilityAction);

            return true;
        }


        private void CoerceVisibility()
        {
           CoerceVisibility(Menu);
        }


        private static void CoerceVisibility(MenuItemViewModelCollection menuItems)
        {
            if (menuItems == null || menuItems.Count == 0)
                return;

            // ----- Hide empty sub-menus.
            for (int i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];
                if (menuItem is MenuSeparatorViewModel)
                    continue;

                // Recursively coerce visibility in children.
                CoerceVisibility(menuItem.Submenu);

                menuItem.IsVisible = menuItem.CommandItem.IsVisible
                                     && (menuItem.Submenu == null || menuItem.Submenu.Any(mi => mi.IsVisible));
            }

            // ----- Hide unnecessary separators.
            bool hasVisiblePredecessor = false;
            for (int i = 0; i < menuItems.Count; i++)
            {
                var menuItem = menuItems[i];

                var menuSeparator = menuItem as MenuSeparatorViewModel;
                if (menuSeparator == null)
                {
                    // Menu item is not a separator.
                    if (menuItem.IsVisible)
                        hasVisiblePredecessor = true;

                    continue;
                }

                // Menu item is a separator.
                // A separator is only visible if there are visible items before and after.
                if (!hasVisiblePredecessor)
                {
                    menuSeparator.IsVisible = false;
                    continue;
                }

                bool hasVisibleSuccessor = false;
                for (int j = i + 1; j < menuItems.Count; j++)
                {
                    var successor = menuItems[j];

                    // Skip separators.
                    if (successor.CommandItem is CommandSeparator)
                        continue;

                    if (!successor.IsVisible)
                        continue;

                    hasVisibleSuccessor = true;
                    break;
                }

                if (!hasVisibleSuccessor)
                {
                    menuSeparator.IsVisible = false;
                    continue;
                }
                
                menuSeparator.IsVisible = true;
                hasVisiblePredecessor = false;
            }
        }
        #endregion
    }
}
