// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;
using DigitalRune.Windows.Docking.Resources;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Provides a commands for adding, moving or removing elements from the docking layout.
    /// </summary>
    public static class DockCommands
    {
        /// <summary>
        /// Gets the value that represents the <strong>Auto Hide</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Auto-Hide".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to move an item into an auto-hide pane.
        /// </remarks>
        public static RoutedUICommand AutoHide
        {
            get
            {
                if (_autoHide == null)
                    _autoHide = new RoutedUICommand(StringResources.CommandAutoHideText, "AutoHide", typeof(DockCommands));

                return _autoHide;
            }
        }
        private static RoutedUICommand _autoHide;


        /// <summary>
        /// Gets the value that represents the <strong>Dock</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Dock".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to add an item to the docking layout.
        /// </remarks>
        public static RoutedUICommand Dock
        {
            get
            {
                if (_dock == null)
                    _dock = new RoutedUICommand(StringResources.CommandDockText, "Dock", typeof(DockCommands));

                return _dock;
            }
        }
        private static RoutedUICommand _dock;


        /// <summary>
        /// Gets the value that represents the <strong>Float</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Float".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to show an item in a <see cref="FloatWindow"/>.
        /// </remarks>
        public static RoutedUICommand Float
        {
            get
            {
                if (_float == null)
                    _float = new RoutedUICommand(StringResources.CommandFloatText, "Float", typeof(DockCommands));

                return _float;
            }
        }
        private static RoutedUICommand _float;


        /// <summary>
        /// Gets the value that represents the <strong>Show</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Show".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to make an item visible in the docking layout.
        /// </remarks>
        public static RoutedUICommand Show
        {
            get
            {
                if (_show == null)
                    _show = new RoutedUICommand(StringResources.CommandShowText, "Show", typeof(DockCommands));

                return _show;
            }
        }
        private static RoutedUICommand _show;


        /// <summary>
        /// Gets the value that represents the <strong>Next</strong> command.
        /// </summary>
        /// <value>
        /// The command. The default key gesture is "Ctrl+Tab". The default UI text is "Next".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to move the focus to the next item.
        /// </remarks>
        public static RoutedUICommand Next
        {
            get
            {
                if (_next == null)
                {
                    _next = new RoutedUICommand(StringResources.CommandNextText, "Next", typeof(DockCommands));
                    _next.InputGestures.Add(new KeyGesture(Key.Tab, ModifierKeys.Control));
                }

                return _next;
            }
        }
        private static RoutedUICommand _next;


        /// <summary>
        /// Gets the value that represents the <strong>Previous</strong> command.
        /// </summary>
        /// <value>
        /// The command. The default key gesture is "Ctrl+Shift+Tab". The default UI text is
        /// "Previous".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to move the focus to the previous item.
        /// </remarks>
        public static RoutedUICommand Previous
        {
            get
            {
                if (_previous == null)
                {
                    _previous = new RoutedUICommand(StringResources.CommandPreviousText, "Previous", typeof(DockCommands));
                    _previous.InputGestures.Add(new KeyGesture(Key.Tab, ModifierKeys.Control | ModifierKeys.Shift));
                }

                return _previous;
            }
        }
        private static RoutedUICommand _previous;


        /// <summary>
        /// Gets the value that represents the <strong>Show Menu</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Show Menu".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to show a menu of all items in a pane.
        /// </remarks>
        public static RoutedUICommand ShowMenu
        {
            get
            {
                if (_showMenu == null)
                    _showMenu = new RoutedUICommand(StringResources.CommandShowMenuText, "ShowMenu", typeof(DockCommands));

                return _showMenu;
            }
        }
        private static RoutedUICommand _showMenu;
    }
}
