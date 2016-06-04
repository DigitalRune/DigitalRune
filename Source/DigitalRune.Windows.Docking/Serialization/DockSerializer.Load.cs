// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using static System.FormattableString;


namespace DigitalRune.Windows.Docking
{
    partial class DockSerializer
    {
        private class DeserializationContext
        {
            public IDockControl DockControl;

            // A mapping of DockIds to DockTabItems.
            public readonly Dictionary<string, IDockTabItem> OldItems = new Dictionary<string, IDockTabItem>();
            public readonly Dictionary<string, IDockTabItem> NewItems = new Dictionary<string, IDockTabItem>();

            public IDockTabItem ActiveItem;
            public IXmlLineInfo LineInfo;

            // Updates LineInfo.
            public XAttribute Checked(XAttribute xAttribute)
            {
                LineInfo = xAttribute;
                return xAttribute;
            }

            // Updates LineInfo.
            public XElement Checked(XElement element)
            {
                LineInfo = element;
                return element;
            }

            public void ResetLineInfo()
            {
                LineInfo = null;
            }
        }


        /// <summary>
        /// Loads the docking layout.
        /// </summary>
        /// <param name="dockControl">The <see cref="IDockControl"/>.</param>
        /// <param name="storedLayout">The stored layout.</param>
        /// <remarks>
        /// The method tries to close all persistent <see cref="IDockTabItem"/>s which are currently
        /// loaded. For this it calls <see cref="DockStrategy.CanClose(IDockTabItem)"/> and
        /// <see cref="DockStrategy.Close(IDockTabItem)"/>. <see cref="IDockTabItem"/>s are only
        /// closed if <see cref="DockStrategy.CanClose(IDockTabItem)"/> returns
        /// <see langword="true"/>. The <see cref="FloatWindow"/>s and <see cref="IDockPane"/> of
        /// the current layout are removed without calling the <see cref="DockStrategy"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockControl"/>, <paramref name="storedLayout"/> or the
        /// <see cref="IDockControl.DockStrategy"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public static void Load(IDockControl dockControl, XElement storedLayout /*, bool keepDockTabItems */)
        {
            ///// <param name="keepDockTabItems">
            ///// If set to <see langword="true"/> all existing <see cref="IDockTabItem"/>s are kept. If
            ///// set to <see langword="false"/>, all current <see cref="IDockTabItem"/>s are closed
            ///// before the new layout is loaded.
            ///// </param>

            if (dockControl == null)
                throw new ArgumentNullException(nameof(dockControl));
            if (storedLayout == null)
                throw new ArgumentNullException(nameof(storedLayout));

            var dockStrategy = dockControl.DockStrategy;
            if (dockStrategy == null)
                throw new ArgumentException("The IDockControl does not have a DockStrategy.");

            const bool keepNonPersistentItems = true;  // Non-persistent items are usually documents and should be kept.
            const bool keepPersistentItems = false;    // Persistent items are usually tool windows.

            // Remember current dock items (visible and hidden items).
            var items = dockControl.GetDockElements()
                                   .OfType<IDockTabItem>()
                                   .Distinct()
                                   .ToArray();

            // Validate DockIds.
            foreach (var item in items)
                if (item.DockId == null)
                    throw new DockException("Could not load docking layout. IDockTabItem does not have a valid DockId.");

            // Check for duplicate DockIds.
            bool duplicateDockIds = items.GroupBy(item => item.DockId)
                                         .Select(group => group.Count())
                                         .Any(count => count > 1);
            if (duplicateDockIds)
                throw new DockException("Could not load docking layout. Two or more IDockTabItems have the same DockId.");

            var context = new DeserializationContext { DockControl = dockControl };

            dockStrategy.Begin();
            try
            {
                // Remember current IDockTabItems.
                // context.OldItems stores all (because OldItems is used in LoadDockTabItem).
                // Another list stores only the old items which are visible.
                var oldVisibleItems = new List<IDockTabItem>();
                foreach (var item in items)
                {
                    context.OldItems[item.DockId] = item;

                    if (item.DockState != DockState.Hide)
                        oldVisibleItems.Add(item);
                }

                // Load IDockTabItems. Do not add to the dock control yet.
                foreach (var itemXElement in storedLayout.Elements("DockTabItems").Elements())
                {
                    var item = LoadDockTabItem(context, itemXElement, onlyReference: false);
                    if (item != null)
                        context.NewItems[item.DockId] = item;
                }

                // Try to close all IDockTabItems which we will not keep or which will be hidden.
                // We keep a list of items which we need to keep open.
                var oldItemsToShow = new List<IDockTabItem>();
                foreach (var oldDockTabItem in oldVisibleItems)
                {
                    if (context.NewItems.ContainsKey(oldDockTabItem.DockId)
                        && oldDockTabItem.DockState != DockState.Hide)
                    {
                        // Item remains in the layout and visible.
                        continue;
                    }

                    // Previously visible item is removed from layout or hidden. 
                    // Try to close it. Items that cannot be closed will be shown at the end.
                    bool closed = false;
                    if ((oldDockTabItem.IsPersistent && !keepPersistentItems)
                        || (!oldDockTabItem.IsPersistent && !keepNonPersistentItems))
                    {
                        if (dockStrategy.CanClose(oldDockTabItem))
                        {
                            // Successfully closed.
                            dockStrategy.Close(oldDockTabItem);
                            closed = true;
                        }
                    }

                    if (!closed)
                        oldItemsToShow.Add(oldDockTabItem);
                }

                // The DockControl still contains the old layout with the previous IDockPane
                // view model. The old view models are still attached to the DockControl and
                // react to changes, which may cause the wrong screen conduction.
                // --> Detach IDockTabItems from old layout.
                foreach (var item in items)
                    DockHelper.Remove(dockControl, item);

                dockControl.FloatWindows.Clear();
                dockControl.AutoHideLeft.Clear();
                dockControl.AutoHideRight.Clear();
                dockControl.AutoHideTop.Clear();
                dockControl.AutoHideBottom.Clear();

                var isLocked = (bool?)context.Checked(storedLayout.Attribute("IsLocked"));
                if (isLocked != null)
                    dockControl.IsLocked = isLocked.Value;

                // Load float windows.
                {
                    foreach (var xElement in storedLayout.Elements("FloatWindows").Elements())
                    {
                        var floatWindow = LoadFloatWindow(context, xElement);
                        if (floatWindow != null)
                            dockControl.FloatWindows.Add(floatWindow);
                    }
                }

                // Load auto-hide panes.
                {
                    var autoHideBars = new[]
                    {
                        new { Bar = dockControl.AutoHideLeft, Name = "AutoHideLeft" },
                        new { Bar = dockControl.AutoHideRight, Name = "AutoHideRight" },
                        new { Bar = dockControl.AutoHideTop, Name = "AutoHideTop" },
                        new { Bar = dockControl.AutoHideBottom, Name = "AutoHideBottom" },
                    };

                    foreach (var bar in autoHideBars)
                    {
                        foreach (var xElement in storedLayout.Elements(bar.Name).Elements())
                        {
                            var dockPane = LoadDockPane(context, xElement);
                            if (dockPane != null)
                                bar.Bar.Add((IDockTabPane)dockPane);
                        }
                    }
                }

                // Load root pane. (We do this after loading the float windows and auto-hide bars,
                // because the dock strategy might want to activate an item in a float window or
                // auto-hide bar).
                dockControl.RootPane = LoadDockPane(context, storedLayout.Elements("RootPane").Elements().FirstOrDefault());

                // Run cleanup to update IsVisible flags. Those should be up-to-date before calling
                // Show() to find good default dock target positions.
                dockStrategy.Cleanup();

                // This is not done in cleanup if we are inside Begin()/End().
                dockControl.ActiveDockTabPane = null;
                dockControl.ActiveDockTabItem = null;

                // Show all old items that are not visible in the loaded layout but could not be closed.
                foreach (var item in oldItemsToShow)
                    dockStrategy.Show(item);

                // Activate item.
                if (context.ActiveItem != null)
                    dockStrategy.Show(context.ActiveItem);

                context.ResetLineInfo();
            }
            catch (Exception exception)
            {
                var message = "Could not load docking layout.";

                if (context.LineInfo != null && context.LineInfo.HasLineInfo())
                    message += Invariant($" Error at line {context.LineInfo.LineNumber}, column {context.LineInfo.LinePosition}.");

                message += " See inner exception for more details.";

                throw new DockException(message, exception);
            }
            finally
            {
                dockStrategy.End();
            }
        }


        /// <summary>
        /// Loads the <see cref="IDockTabItem"/>.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="xElement">
        /// The XML element representing the <see cref="IDockTabItem"/>.
        /// </param>
        /// <param name="onlyReference">
        /// If set to <see langword="true"/>, only the dock ID is loaded.
        /// </param>
        /// <returns>The <see cref="IDockTabItem"/>.</returns>
        private static IDockTabItem LoadDockTabItem(DeserializationContext context, XElement xElement, bool onlyReference)
        {
            context.ResetLineInfo();

            if (xElement == null)
                return null;

            string dockId = (string)context.Checked(xElement.Attribute("DockId"));

            // Get the IDockTabItem from OldItems.
            IDockTabItem item;
            if (!context.OldItems.TryGetValue(dockId, out item))
            {
                if (!context.NewItems.TryGetValue(dockId, out item))
                {
                    // Not found. Create new item.
                    item = context.DockControl.DockStrategy.CreateDockTabItem(dockId);
                }
            }

            if (onlyReference)
                return item;

            if (item == null)
                return null;

            bool? isActive = (bool?)context.Checked(xElement.Attribute("IsActive"));
            if (isActive.HasValue && isActive.Value)
                context.ActiveItem = item;

            {
                var xAttribute = context.Checked(xElement.Attribute("DockState"));
                if (xAttribute != null)
                    item.DockState = ObjectHelper.Parse<DockState>(xAttribute.Value);
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("LastDockState"));
                if (xAttribute != null)
                    item.LastDockState = ObjectHelper.Parse<DockState>(xAttribute.Value);
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("DockWidth"));
                if (xAttribute != null)
                    item.DockWidth = ConvertToGridLength(xAttribute.Value);
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("DockHeight"));
                if (xAttribute != null)
                    item.DockHeight = ConvertToGridLength(xAttribute.Value);
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("AutoHideWidth"));
                if (xAttribute != null)
                    item.AutoHideWidth = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("AutoHideHeight"));
                if (xAttribute != null)
                    item.AutoHideHeight = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("LastActivation"));
                if (xAttribute != null)
                    item.LastActivation = (DateTime)xAttribute;
            }

            context.ResetLineInfo();
            return item;
        }


        private static IDockPane LoadDockPane(DeserializationContext context, XElement xElement)
        {
            context.ResetLineInfo();

            if (xElement == null)
                return null;

            if (xElement.Name == "DockAnchorPane")
            {
                var pane = context.DockControl.DockStrategy.CreateDockAnchorPane();

                {
                    var xAttribute = context.Checked(xElement.Attribute("DockWidth"));
                    if (xAttribute != null)
                        pane.DockWidth = ConvertToGridLength(xAttribute.Value);
                }
                {
                    var xAttribute = context.Checked(xElement.Attribute("DockHeight"));
                    if (xAttribute != null)
                        pane.DockHeight = ConvertToGridLength(xAttribute.Value);
                }
                {
                    var childXElement = context.Checked(xElement.Elements().FirstOrDefault());
                    if (childXElement != null)
                        pane.ChildPane = LoadDockPane(context, childXElement);
                }

                context.ResetLineInfo();
                return pane;
            }

            if (xElement.Name == "DockSplitPane")
            {
                var pane = context.DockControl.DockStrategy.CreateDockSplitPane();

                {
                    var xAttribute = context.Checked(xElement.Attribute("DockWidth"));
                    if (xAttribute != null)
                        pane.DockWidth = ConvertToGridLength(xAttribute.Value);
                }
                {
                    var xAttribute = context.Checked(xElement.Attribute("DockHeight"));
                    if (xAttribute != null)
                        pane.DockHeight = ConvertToGridLength(xAttribute.Value);
                }
                {
                    var xAttribute = context.Checked(xElement.Attribute("Orientation"));
                    if (xAttribute != null)
                        pane.Orientation = ObjectHelper.Parse<Orientation>(xAttribute.Value);
                }
                {
                    foreach (var childXElement in xElement.Elements())
                    {
                        var dockPane = LoadDockPane(context, context.Checked(childXElement));
                        if (dockPane != null)
                            pane.ChildPanes.Add(dockPane);
                    }
                }

                context.ResetLineInfo();
                return pane;
            }

            if (xElement.Name == "DockTabPane")
            {
                var pane = context.DockControl.DockStrategy.CreateDockTabPane();

                {
                    var xAttribute = context.Checked(xElement.Attribute("DockWidth"));
                    if (xAttribute != null)
                        pane.DockWidth = ConvertToGridLength(xAttribute.Value);
                }
                {
                    var xAttribute = context.Checked(xElement.Attribute("DockHeight"));
                    if (xAttribute != null)
                        pane.DockHeight = ConvertToGridLength(xAttribute.Value);
                }

                int selectedIndex = 0;
                {
                    var xAttribute = context.Checked(xElement.Attribute("SelectedIndex"));
                    if (xAttribute != null)
                        selectedIndex = (int)xAttribute;
                }

                {
                    foreach (var dockTabItemXElement in xElement.Elements())
                    {
                        var dockTabItem = LoadDockTabItem(context, context.Checked(dockTabItemXElement), onlyReference: true);
                        if (dockTabItem != null)
                            pane.Items.Add(dockTabItem);
                    }
                }

                context.ResetLineInfo();
                if (selectedIndex < pane.Items.Count)
                    pane.SelectedItem = pane.Items[selectedIndex];
                return pane;
            }

            context.ResetLineInfo();
            return null;
        }


        private static IFloatWindow LoadFloatWindow(DeserializationContext context, XElement xElement)
        {
            context.ResetLineInfo();

            if (xElement == null)
                return null;

            var floatWindow = context.DockControl.DockStrategy.CreateFloatWindow();

            {
                var xAttribute = context.Checked(xElement.Attribute("Left"));
                if (xAttribute != null)
                    floatWindow.Left = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("Top"));
                if (xAttribute != null)
                    floatWindow.Top = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("Width"));
                if (xAttribute != null)
                    floatWindow.Width = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("Height"));
                if (xAttribute != null)
                    floatWindow.Height = (double)xAttribute;
            }
            {
                var xAttribute = context.Checked(xElement.Attribute("WindowState"));
                if (xAttribute != null)
                    floatWindow.WindowState = ObjectHelper.Parse<WindowState>(xAttribute.Value);
            }
            {
                var childXElement = context.Checked(xElement.Elements().FirstOrDefault());
                if (childXElement != null)
                    floatWindow.RootPane = LoadDockPane(context, childXElement);
            }

            context.ResetLineInfo();

            return floatWindow;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private static GridLength ConvertToGridLength(string str)
        {
            var result = GridLengthConverter.ConvertFromInvariantString(str);
            if (result != null)
                return (GridLength)result;

            throw new DockException("Could not deserialize attribute of type GridLength.");
        }
    }
}
