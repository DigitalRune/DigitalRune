// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a context menu that shows a list of <see cref="DockTabItem"/>s.
    /// </summary>
    /// <remarks>
    /// The <see cref="FrameworkElement.DataContext"/> of this context menu must be set to a
    /// <see cref="DockTabPane"/>. The <see cref="DockTabItem"/>s of the given
    /// <see cref="DockTabPane"/> are listed in the context menu. If a menu item is clicked, the
    /// associated <see cref="DockTabItem"/> is shown.
    /// </remarks>
    public class DockPaneMenu : ContextMenu
    {
        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockPaneMenu"/> class.
        /// </summary>
        static DockPaneMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPaneMenu), new FrameworkPropertyMetadata(typeof(DockPaneMenu)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="ContextMenu.Opened"/> event occurs.
        /// </summary>
        /// <param name="e">
        /// The event data for the <see cref="ContextMenu.Opened"/> event.
        /// </param>
        protected override void OnOpened(RoutedEventArgs e)
        {
            base.OnOpened(e);

            // ----- Update menu items.
            Items.Clear();

            var pane = DataContext as DockTabPane;
            if (pane == null)
                return;

            //  Add a MenuItem with a Show command for each DockTabItem.
            for (int i = 0; i < pane.Items.Count; i++)
            {
                var dockTabItem = (DockTabItem)pane.ItemContainerGenerator.ContainerFromIndex(i);
                Items.Add(CreateMenuItem(dockTabItem));
            }

            // Show the context menu only if it has content.
            Visibility = (Items.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }


        private static MenuItem CreateMenuItem(DockTabItem dockTabItem)
        {
            var menuItem = new MenuItem
            {
                Header = new TextBlock { Text = dockTabItem.Title }, // Use TextBlock to prevent access keys!
                Command = DockCommands.Show,
                CommandTarget = dockTabItem,
                IsChecked = dockTabItem.IsSelected,
                IsEnabled = dockTabItem.IsEnabled,
            };

            if (dockTabItem.Icon is ImageSource || dockTabItem.Icon is MultiColorGlyph)
                menuItem.Icon = new Icon { Source = dockTabItem.Icon };
            else
                menuItem.Icon = new ContentControl { Content = dockTabItem.Icon };

            return menuItem;
        }
        #endregion
    }
}
