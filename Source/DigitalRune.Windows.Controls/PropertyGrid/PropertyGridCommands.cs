// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Provides a set of commands for manipulating a <see cref="PropertyGrid"/>.
    /// </summary>
    public static class PropertyGridCommands
    {
        /// <summary>
        /// Gets the value that represents the <strong>Clear Filter</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Clear Filter".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to clear the <see cref="PropertyGrid.Filter"/> of
        /// the <see cref="PropertyGrid"/>
        /// </remarks>
        public static RoutedUICommand ClearFilter
        {
            get
            {
                if (_clearFilter == null)
                    _clearFilter = new RoutedUICommand("Clear filter", "ClearFilter", typeof(PropertyGridCommands));

                return _clearFilter;
            }
        }
        private static RoutedUICommand _clearFilter;


        /// <summary>
        /// Gets the value that represents the <strong>Reset Property</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Reset".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to reset the value of the selected property.
        /// </remarks>
        public static RoutedUICommand ResetProperty
        {
            get
            {
                if (_reset == null)
                    _reset = new RoutedUICommand("Reset", "ResetProperty", typeof(PropertyGridCommands));

                return _reset;
            }
        }
        private static RoutedUICommand _reset;
    }
}
