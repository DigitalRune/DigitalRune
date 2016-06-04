// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Xml.Linq;


namespace DigitalRune.Windows.Docking
{
    partial class DockSerializer
    {
        // Notes: 
        //  - When properties are saved, they are compared with the default values in the
        //    default instances. Default values are not stored.
        //  - For double values, the default value can be NaN. --> Use Equals() to compare because
        //    double.NaN.Equals(double.NaN) is true but double.NaN == double.NaN is false.

        private class SerializationContext
        {
            public readonly IDockControl DockControl;

            // "Default" dock elements to check the default property values.
            public readonly IFloatWindow DefaultFloatWindow;
            public readonly IDockAnchorPane DefaultDockAnchorPane;
            public readonly IDockSplitPane DefaultDockSplitPane;
            public readonly IDockTabPane DefaultDockTabPane;

            public bool SaveNonPersistentItems;

            public SerializationContext(IDockControl dockControl)
            {
                Debug.Assert(dockControl != null);
                Debug.Assert(dockControl.DockStrategy != null);

                DockControl = dockControl;

                var dockStrategy = dockControl.DockStrategy;
                DefaultFloatWindow = dockStrategy.CreateFloatWindow();
                DefaultDockAnchorPane = dockStrategy.CreateDockAnchorPane();
                DefaultDockSplitPane = dockStrategy.CreateDockSplitPane();
                DefaultDockTabPane = dockStrategy.CreateDockTabPane();
            }
        }


        /// <overloads>
        /// <summary>
        /// Saves the docking layout.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Saves the docking layout (including persistent and non-persistent
        /// <see cref="IDockTabItem"/>s).
        /// </summary>
        /// <param name="dockControl">The <see cref="IDockControl"/>.</param>
        /// <returns>The <see cref="XElement" /> with the serialized layout.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockControl"/> or the <see cref="IDockControl.DockStrategy"/> is
        /// <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming",
            "CA2204:Literals should be spelled correctly")]
        public static XElement Save(IDockControl dockControl)
        {
            return Save(dockControl, false);
        }


        /// <summary>
        /// Saves the docking layout.
        /// </summary>
        /// <param name="dockControl">The <see cref="IDockControl"/>.</param>
        /// <param name="excludeNonPersistentItems">
        /// <see langword="true"/> to exclude non-persistent <see cref="IDockTabItem"/>s.
        /// <see langword="false"/> to store the layout of all (persistent and non-persistent)
        /// <see cref="IDockTabItem"/>s.
        /// </param>
        /// <returns>The <see cref="XElement"/> with the serialized layout.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockControl"/> or the <see cref="IDockControl.DockStrategy"/> is
        /// <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public static XElement Save(IDockControl dockControl, bool excludeNonPersistentItems)
        {
            if (dockControl == null)
                throw new ArgumentNullException(nameof(dockControl));
            if (dockControl.DockStrategy == null)
                throw new ArgumentException("The dock control does not have a dock strategy.");

            var context = new SerializationContext(dockControl)
            {
                SaveNonPersistentItems = !excludeNonPersistentItems,
            };

            // Create root node.
            XElement root = new XElement("DockControl");

            // Save properties of DockControl.
            root.Add(new XAttribute("IsLocked", dockControl.IsLocked));

            // Serialize all DockTabItems.
            {
                var xElement = new XElement("DockTabItems");
                var items = dockControl.GetDockElements()
                                       .OfType<IDockTabItem>()
                                       .Distinct()
                                       .OrderBy(item => item.DockId);
                foreach (var item in items)
                {
                    if (excludeNonPersistentItems && !item.IsPersistent)  // Ignore non-persistent items?
                        continue;
                    if (item.DockId == null)
                        throw new DockException("Could not save docking layout. IDockTabItem does not have a valid DockId.");

                    xElement.Add(Save(context, item, false));
                }

                root.Add(xElement);
            }

            // Serialize root pane.
            {
                var xElement = new XElement("RootPane");
                xElement.Add(Save(context, dockControl.RootPane));
                root.Add(xElement);
            }

            // Serialize float windows.
            {
                var xElement = new XElement("FloatWindows");
                if (dockControl.FloatWindows != null)
                    foreach (var floatWindow in dockControl.FloatWindows)
                        xElement.Add(Save(context, floatWindow));
                root.Add(xElement);
            }

            // Serialize auto-hide panes.
            {
                var autoHideBars = new[]
                {
                    new { Bar = dockControl.AutoHideLeft,  Name = "AutoHideLeft" },
                    new { Bar = dockControl.AutoHideRight,  Name = "AutoHideRight" },
                    new { Bar = dockControl.AutoHideTop,  Name = "AutoHideTop" },
                    new { Bar = dockControl.AutoHideBottom,  Name = "AutoHideBottom" },
                };

                foreach (var bar in autoHideBars)
                {
                    var xElement = new XElement(bar.Name);
                    if (bar.Bar != null)
                        foreach (var pane in bar.Bar)
                            xElement.Add(Save(context, pane));
                    root.Add(xElement);
                }
            }

            return root;
        }


        /// <summary>
        /// Saves the specified <see cref="IDockTabItem"/>.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/>.</param>
        /// <param name="onlyReference">
        /// If set to <see langword="true"/>, only the dock ID is written.
        /// </param>
        /// <returns>
        /// The XML element representing <paramref name="dockTabItem"/>.
        /// </returns>
        private static XElement Save(SerializationContext context, IDockTabItem dockTabItem, bool onlyReference)
        {
            if (dockTabItem == null)
                return null;
            if (!context.SaveNonPersistentItems && !dockTabItem.IsPersistent)
                return null;

            var xElement = new XElement("DockTabItem");
            xElement.Add(new XAttribute("DockId", dockTabItem.DockId));

            if (onlyReference)
                return xElement;

            if (context.DockControl.ActiveDockTabItem == dockTabItem)
                xElement.Add(new XAttribute("IsActive", true));

            xElement.Add(new XAttribute("DockState", dockTabItem.DockState));
            xElement.Add(new XAttribute("LastDockState", dockTabItem.LastDockState));
            xElement.Add(new XAttribute("DockWidth", ConvertToString(dockTabItem.DockWidth)));
            xElement.Add(new XAttribute("DockHeight", ConvertToString(dockTabItem.DockHeight)));
            xElement.Add(new XAttribute("AutoHideWidth", dockTabItem.AutoHideWidth));
            xElement.Add(new XAttribute("AutoHideHeight", dockTabItem.AutoHideHeight));
            xElement.Add(new XAttribute("LastActivation", dockTabItem.LastActivation));

            return xElement;
        }



        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static XElement Save(SerializationContext context, IDockElement dockElement)
        {
            if (dockElement == null)
                return null;

            if (dockElement is IDockAnchorPane)
            {
                var pane = (IDockAnchorPane)dockElement;
                var defaultPane = context.DefaultDockAnchorPane;

                var xElement = new XElement("DockAnchorPane");

                if (!pane.DockWidth.Equals(defaultPane.DockWidth))
                    xElement.Add(new XAttribute("DockWidth", ConvertToString(pane.DockWidth)));
                if (!pane.DockHeight.Equals(defaultPane.DockHeight))
                    xElement.Add(new XAttribute("DockHeight", ConvertToString(pane.DockHeight)));

                xElement.Add(Save(context, pane.ChildPane));

                return xElement;
            }

            if (dockElement is IDockSplitPane)
            {
                var pane = (IDockSplitPane)dockElement;

                if (!ContainsItemsToSave(context, pane))
                    return null;

                var defaultPane = context.DefaultDockSplitPane;

                XElement xElement = new XElement("DockSplitPane");

                if (!pane.DockWidth.Equals(defaultPane.DockWidth))
                    xElement.Add(new XAttribute("DockWidth", ConvertToString(pane.DockWidth)));
                if (!pane.DockHeight.Equals(defaultPane.DockHeight))
                    xElement.Add(new XAttribute("DockHeight", ConvertToString(pane.DockHeight)));
                if (pane.Orientation != defaultPane.Orientation)
                    xElement.Add(new XAttribute("Orientation", pane.Orientation));

                if (pane.ChildPanes != null)
                    foreach (var childPane in pane.ChildPanes)
                        xElement.Add(Save(context, childPane));

                return xElement;
            }

            if (dockElement is IDockTabPane)
            {
                var pane = (IDockTabPane)dockElement;

                if (!ContainsItemsToSave(context, pane))
                    return null;

                var defaultPane = context.DefaultDockTabPane;

                XElement xElement = new XElement("DockTabPane");

                if (!pane.DockWidth.Equals(defaultPane.DockWidth))
                    xElement.Add(new XAttribute("DockWidth", ConvertToString(pane.DockWidth)));
                if (!pane.DockHeight.Equals(defaultPane.DockHeight))
                    xElement.Add(new XAttribute("DockHeight", ConvertToString(pane.DockHeight)));

                if (pane.Items != null)
                {
                    if (pane.SelectedItem != null)
                        xElement.Add(new XAttribute("SelectedIndex", pane.Items.IndexOf(pane.SelectedItem)));

                    foreach (var item in pane.Items)
                        xElement.Add(Save(context, item, onlyReference: true));
                }

                return xElement;
            }

            if (dockElement is IDockTabItem)
            {
                return Save(context, (IDockTabItem)dockElement, onlyReference: true);
            }

            return null;
        }


        private static XElement Save(SerializationContext context, IFloatWindow floatWindow)
        {
            if (floatWindow == null)
                return null;

            if (!ContainsItemsToSave(context, floatWindow.RootPane))
                return null;

            var xElement = new XElement("FloatWindow");

            if (!floatWindow.Left.Equals(context.DefaultFloatWindow.Left))
                xElement.Add(new XAttribute("Left", floatWindow.Left));
            if (!floatWindow.Left.Equals(context.DefaultFloatWindow.Top))
                xElement.Add(new XAttribute("Top", floatWindow.Top));
            if (!floatWindow.Left.Equals(context.DefaultFloatWindow.Width))
                xElement.Add(new XAttribute("Width", floatWindow.Width));
            if (!floatWindow.Left.Equals(context.DefaultFloatWindow.Height))
                xElement.Add(new XAttribute("Height", floatWindow.Height));
            if (floatWindow.WindowState != context.DefaultFloatWindow.WindowState)
                xElement.Add(new XAttribute("WindowState", floatWindow.WindowState));

            xElement.Add(Save(context, floatWindow.RootPane));

            return xElement;
        }


        private static string ConvertToString(GridLength gridLength)
        {
            return GridLengthConverter.ConvertToInvariantString(gridLength) ?? string.Empty;
        }


        private static bool ContainsItemsToSave(SerializationContext context, IDockPane dockPane)
        {
            if (dockPane == null)
                return false;

            if (context.SaveNonPersistentItems)
                return true;

            return dockPane.GetDockElements().OfType<IDockTabItem>().Any(item => item.IsPersistent);
        }
    }
}
