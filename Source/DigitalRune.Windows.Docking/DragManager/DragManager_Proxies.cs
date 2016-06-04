// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Diagnostics;


namespace DigitalRune.Windows.Docking
{
    // Helper methods
    internal partial class DragManager
    {
        /// <summary>
        /// Replaces the items with invisible proxies.
        /// </summary>
        /// <param name="currentPane">
        /// The <see cref="IDockTabPane"/> where the items are currently visible. Item proxies will
        /// be added to this pane, but the original items will be kept.
        /// </param>
        private void ReplaceItemsWithProxies(IDockTabPane currentPane)
        {
            Debug.Assert(_dockStrategy != null);

            ReplaceItemsWithProxies(_dockStrategy.DockControl.RootPane, _draggedItems, currentPane);

            foreach (var floatWindow in _dockStrategy.DockControl.FloatWindows)
                ReplaceItemsWithProxies(floatWindow.RootPane, _draggedItems, currentPane);

            ReplaceItemsWithProxies(_dockStrategy.DockControl.AutoHideLeft, _draggedItems, currentPane);
            ReplaceItemsWithProxies(_dockStrategy.DockControl.AutoHideRight, _draggedItems, currentPane);
            ReplaceItemsWithProxies(_dockStrategy.DockControl.AutoHideTop, _draggedItems, currentPane);
            ReplaceItemsWithProxies(_dockStrategy.DockControl.AutoHideBottom, _draggedItems, currentPane);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void ReplaceItemsWithProxies(IDockPane dockPane, List<IDockTabItem> items, IDockTabPane currentPane)
        {
            Debug.Assert(dockPane != null);
            Debug.Assert(items != null);

            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                if (dockAnchorPane.ChildPane != null)
                    ReplaceItemsWithProxies(dockAnchorPane.ChildPane, items, currentPane);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                ReplaceItemsWithProxies(dockSplitPane.ChildPanes, items, currentPane);
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;
                if (dockTabPane == currentPane)
                {
                    // Special: If the IDockTabPane is the current pane, add the item proxies but
                    // keep the original items.
                    for (int i = 0; i < dockTabPane.Items.Count; i++)
                    {
                        if (items.IndexOf(dockTabPane.Items[i]) >= 0)
                        {
                            dockTabPane.Items.Insert(i + 1, new DockTabItemProxy(dockTabPane.Items[i]));
                            i++;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < dockTabPane.Items.Count; i++)
                    {
                        if (items.IndexOf(dockTabPane.Items[i]) >= 0)
                            dockTabPane.Items[i] = new DockTabItemProxy(dockTabPane.Items[i]);
                    }
                }
            }
        }


        private static void ReplaceItemsWithProxies(IReadOnlyList<IDockPane> dockPanes, List<IDockTabItem> items, IDockTabPane currentPane)
        {
            Debug.Assert(dockPanes != null);
            Debug.Assert(items != null);

            for (int i = 0; i < dockPanes.Count; i++)
                ReplaceItemsWithProxies(dockPanes[i], items, currentPane);
        }


        /// <summary>
        /// Removes the item proxies from the specified <see cref="IDockPane"/>.
        /// </summary>
        /// <param name="dockPane">The <see cref="IDockPane"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void RemoveItemProxies(IDockPane dockPane)
        {
            Debug.Assert(dockPane != null);

            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                if (dockAnchorPane.ChildPane != null)
                    RemoveItemProxies(dockAnchorPane.ChildPane);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                RemoveItemProxies(dockSplitPane.ChildPanes);
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;
                for (int i = dockTabPane.Items.Count - 1; i >= 0; i--)
                {
                    if (dockTabPane.Items[i] is DockTabItemProxy)
                        dockTabPane.Items.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// Removes the item proxies with the specified dock state.
        /// </summary>
        /// <param name="dockState">The dock state.</param>
        private void RemoveItemProxies(DockState dockState)
        {
            switch (dockState)
            {
                case DockState.Dock:
                    RemoveItemProxies(_dockStrategy.DockControl.RootPane);
                    break;
                case DockState.Float:
                    foreach (var floatWindow in _dockStrategy.DockControl.FloatWindows)
                        RemoveItemProxies(floatWindow.RootPane);
                    break;
                case DockState.AutoHide:
                    RemoveItemProxies(_dockStrategy.DockControl.AutoHideLeft);
                    RemoveItemProxies(_dockStrategy.DockControl.AutoHideRight);
                    RemoveItemProxies(_dockStrategy.DockControl.AutoHideTop);
                    RemoveItemProxies(_dockStrategy.DockControl.AutoHideBottom);
                    break;
            }
        }


        /// <summary>
        /// Removes the item proxies from the specified list of <see cref="IDockPane"/>s.
        /// </summary>
        /// <param name="dockPanes">The list of <see cref="IDockPane"/>s.</param>
        private static void RemoveItemProxies(IReadOnlyList<IDockPane> dockPanes)
        {
            Debug.Assert(dockPanes != null);

            for (int i = 0; i < dockPanes.Count; i++)
                RemoveItemProxies(dockPanes[i]);
        }


        /// <summary>
        /// Restores the items from their item proxies.
        /// </summary>
        private void RestoreItemsFromProxies()
        {
            Debug.Assert(_dockStrategy != null);

            RestoreItemsFromProxies(_dockStrategy.DockControl.RootPane);

            foreach (var floatWindow in _dockStrategy.DockControl.FloatWindows)
                RestoreItemsFromProxies(floatWindow.RootPane);

            RestoreItemsFromProxies(_dockStrategy.DockControl.AutoHideLeft);
            RestoreItemsFromProxies(_dockStrategy.DockControl.AutoHideRight);
            RestoreItemsFromProxies(_dockStrategy.DockControl.AutoHideTop);
            RestoreItemsFromProxies(_dockStrategy.DockControl.AutoHideBottom);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void RestoreItemsFromProxies(IDockPane dockPane)
        {
            Debug.Assert(dockPane != null);

            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                if (dockAnchorPane.ChildPane != null)
                    RestoreItemsFromProxies(dockAnchorPane.ChildPane);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                RestoreItemsFromProxies(dockSplitPane.ChildPanes);
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;
                for (int i = dockTabPane.Items.Count - 1; i >= 0; i--)
                {
                    var proxy = dockTabPane.Items[i] as DockTabItemProxy;
                    if (proxy != null)
                        dockTabPane.Items[i] = proxy.Item;
                }
            }
        }


        private static void RestoreItemsFromProxies(IReadOnlyList<IDockPane> dockPanes)
        {
            Debug.Assert(dockPanes != null);

            for (int i = 0; i < dockPanes.Count; i++)
                RestoreItemsFromProxies(dockPanes[i]);
        }
    }
}
