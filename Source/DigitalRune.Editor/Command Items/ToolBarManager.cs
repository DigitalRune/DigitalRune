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
    /// Merges and manages a collection of command item nodes that represent toolbars.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Toolbars in the editor are created dynamically. Every editor extension can register
    /// <see cref="MergeableNode{T}"/> instances containing <see cref="ICommandItem"/>s. This class
    /// merges the nodes and stores the merged collection in <see cref="Nodes"/>. The
    /// <see cref="ICommandItem"/>s are also stores in <see cref="CommandItems"/> for convenience.
    /// The <see cref="ToolBarManager"/> creates toolbar view models and stores them in
    /// <see cref="ToolBars"/>.
    /// </para>
    /// <para>
    /// The nodes are merged in <see cref="Update"/>. When content of the input node collection was
    /// changed, <see cref="Update"/> can be called again to update the merged collections. The
    /// merged collections (<see cref="Nodes"/>, <see cref="CommandItems"/> and
    /// <see cref="ToolBars"/>) are created once when the <see cref="MenuManager"/> is created.
    /// <see cref="Update"/> changes only the content of the collections.
    /// </para>
    /// <para>
    /// This class also observes the command item visibility and updates the toolbar items, e.g. to
    /// hide empty toolbars and unnecessary separators.
    /// </para>
    /// </remarks>
    /// <seealso cref="MenuManager"/>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    internal class ToolBarManager : IWeakEventListener
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
        /// Gets the toolbars.
        /// </summary>
        /// <value>The toolbars.</value>
        public ToolBarViewModelCollection ToolBars { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarManager"/> class.
        /// </summary>
        public ToolBarManager()
        {
            _coerceVisibilityAction = CoerceVisibility;
            Nodes = new MergeableNodeCollection<ICommandItem>();
            CommandItems = new List<ICommandItem>();
            ToolBars = new ToolBarViewModelCollection();
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

            // Remove empty toolbars.
            RemoveEmptyGroups(Nodes);

            // Add items to CommandItems list.
            CommandItems.Clear();
            CollectCommandItems(Nodes);

            // Create toolbars.
            ToolBars.Clear();
            CreateToolBars();

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
                    Debug.Assert(!CommandItems.Contains(node.Content), "Warning: Duplicate command items in toolbar.");
                    CommandItems.Add(node.Content);
                }

                CollectCommandItems(node.Children);
            }
        }


        private void CreateToolBars()
        {
            foreach (var node in Nodes)
            {
                // Command items on this level must be all command groups. Other items are ignored.
                var commandGroup = node?.Content as CommandGroup;
                if (commandGroup == null)
                    continue;

                // ToolBarViewModel do not derive from ToolBarItemViewModel. Therefore, we do not
                // call commandGroup.CreateToolBarItem().
                var toolBar = new ToolBarViewModel(commandGroup);
                ToolBars.Add(toolBar);
                
                if (node.Children != null && node.Children.Count > 0)
                    CreateToolBarItems(node.Children, toolBar.Items);
            }
        }


        private static void CreateToolBarItems(MergeableNodeCollection<ICommandItem> nodes, ToolBarItemViewModelCollection toolBarItems)
        {
            foreach (var node in nodes)
            {
                if (node?.Content == null)
                    continue;

                var toolBarItem = node.Content.CreateToolBarItem();
                toolBarItems.Add(toolBarItem);
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
            foreach (var toolBar in ToolBars)
            {
                CoerceVisibility(toolBar.Items);

                // ToolBarViewModel.ActualIsVisible depends on the visibility of its items.
                // The toolbar does not monitor the visibility of its items.
                // --> Raise "ActualIsVisible" property changed event for the toolbar.
                toolBar.RaiseActualIsVisibleChanged();
            }
        }


        private static void CoerceVisibility(ToolBarItemViewModelCollection toolBarItems)
        {
            if (toolBarItems == null || toolBarItems.Count == 0)
                return;

            // ----- Hide unnecessary separators.
            bool hasVisiblePredecessor = false;
            for (int i = 0; i < toolBarItems.Count; i++)
            {
                var toolBarItem = toolBarItems[i];

                var toolBarSeparator = toolBarItem as ToolBarSeparatorViewModel;
                if (toolBarSeparator == null)
                {
                    // Toolbar item is not a separator.

                    if (toolBarItem.CommandItem.IsVisible)
                        hasVisiblePredecessor = true;

                    continue;
                }

                // Toolbar item is a separator.
                // A separator is only visible if there are visible items before and after.
                if (!hasVisiblePredecessor)
                {
                    toolBarSeparator.IsVisible = false;
                    continue;
                }

                bool hasVisibleSuccessor = false;
                for (int j = i + 1; j < toolBarItems.Count; j++)
                {
                    var successor = toolBarItems[j];

                    // Skip separators.
                    if (successor.CommandItem is CommandSeparator)
                        continue;

                    if (!successor.CommandItem.IsVisible)
                        continue;

                    hasVisibleSuccessor = true;
                    break;
                }

                if (!hasVisibleSuccessor)
                {
                    toolBarSeparator.IsVisible = false;
                    continue;
                }
                
                toolBarSeparator.IsVisible = true;
                hasVisiblePredecessor = false;
            }
        }
        #endregion
    }
}
