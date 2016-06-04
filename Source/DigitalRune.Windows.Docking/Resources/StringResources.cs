// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking.Resources
{
    /// <summary>
    /// Manages resource strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class manages all localizable strings. Per default, the strings are initialized from a
    /// resource file.
    /// </para>
    /// <para>
    /// The strings (properties in this class) can be replaced with custom strings - this must be
    /// done at the start-up of you application, before the strings are used. Setting a string to
    /// <see langword="null"/> restores the default value.
    /// </para>
    /// </remarks>
    public static class StringResources
    {
        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.AutoHide"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.AutoHide"/> command.</value>
        public static string CommandAutoHideText
        {
            get
            {
                if (_commandAutoHideText == null)
                    _commandAutoHideText = Strings.CommandAutoHideText;

                return _commandAutoHideText;
            }
            set { _commandAutoHideText = value; }
        }
        private static string _commandAutoHideText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.Dock"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.Dock"/> command.</value>
        public static string CommandDockText
        {
            get
            {
                if (_commandDockText == null)
                    _commandDockText = Strings.CommandDockText;

                return _commandDockText;
            }
            set { _commandDockText = value; }
        }
        private static string _commandDockText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.Float"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.Float"/> command.</value>
        public static string CommandFloatText
        {
            get
            {
                if (_commandFloatText == null)
                    _commandFloatText = Strings.CommandFloatText;

                return _commandFloatText;
            }
            set { _commandFloatText = value; }
        }
        private static string _commandFloatText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.Show"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.Show"/> command.</value>
        public static string CommandShowText
        {
            get
            {
                if (_commandShowText == null)
                    _commandShowText = Strings.CommandShowText;

                return _commandShowText;
            }
            set { _commandShowText = value; }
        }
        private static string _commandShowText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.Next"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.Next"/> command.</value>
        public static string CommandNextText
        {
            get
            {
                if (_commandNextText == null)
                    _commandNextText = Strings.CommandNextText;

                return _commandNextText;
            }
            set { _commandNextText = value; }
        }
        private static string _commandNextText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.Previous"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.Previous"/> command.</value>
        public static string CommandPreviousText
        {
            get
            {
                if (_commandPreviousText == null)
                    _commandPreviousText = Strings.CommandPreviousText;

                return _commandPreviousText;
            }
            set { _commandPreviousText = value; }
        }
        private static string _commandPreviousText;


        /// <summary>
        /// Gets or sets the text for the <see cref="DockCommands.ShowMenu"/> command.
        /// </summary>
        /// <value>The text for the <see cref="DockCommands.ShowMenu"/> command.</value>
        public static string CommandShowMenuText
        {
            get
            {
                if (_commandShowWindowListText == null)
                    _commandShowWindowListText = Strings.CommandShowWindowListText;

                return _commandShowWindowListText;
            }
            set { _commandShowWindowListText = value; }
        }
        private static string _commandShowWindowListText;


        /// <summary>
        /// Gets or sets the tool-tip text for the <see cref="DockCommands.AutoHide"/> buttons.
        /// </summary>
        /// <value>The tool-tip text for <see cref="DockCommands.AutoHide"/> buttons.</value>
        public static string ToolTipAutoHide
        {
            get
            {
                if (_toolTipAutoHide == null)
                    _toolTipAutoHide = Strings.ToolTipAutoHide;

                return _toolTipAutoHide;
            }
            set { _toolTipAutoHide = value; }
        }
        private static string _toolTipAutoHide;


        /// <summary>
        /// Gets or sets the tool-tip text <see cref="DockCommands.Dock"/> buttons.
        /// </summary>
        /// <value>The tool-tip text for <see cref="DockCommands.Dock"/> buttons.</value>
        public static string ToolTipDock
        {
            get
            {
                if (_toolTipDock == null)
                    _toolTipDock = Strings.ToolTipDock;

                return _toolTipDock;
            }
            set { _toolTipDock = value; }
        }
        private static string _toolTipDock;


        /// <summary>
        /// Gets or sets the tool-tip text Close buttons.
        /// </summary>
        /// <value>The tool-tip text for Close buttons.</value>
        public static string ToolTipClose
        {
            get
            {
                if (_toolTipClose == null)
                    _toolTipClose = Strings.ToolTipClose;

                return _toolTipClose;
            }
            set { _toolTipClose = value; }
        }
        private static string _toolTipClose;


        /// <summary>
        /// Gets or sets the tool-tip text <see cref="DockCommands.ShowMenu"/> buttons.
        /// </summary>
        /// <value>The tool-tip text for <see cref="DockCommands.ShowMenu"/> buttons.</value>
        public static string ToolTipShowWindowList
        {
            get
            {
                if (_toolTipShowWindowList == null)
                    _toolTipShowWindowList = Strings.ToolTipShowWindowList;

                return _toolTipShowWindowList;
            }
            set { _toolTipShowWindowList = value; }
        }
        private static string _toolTipShowWindowList;
    }
}
