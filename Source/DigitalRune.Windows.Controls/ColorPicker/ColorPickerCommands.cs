// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Provides commands handled by the <see cref="ColorPicker"/>.
    /// </summary>
    public static class ColorPickerCommands
    {
        /// <summary>
        /// Gets the value that represents the <strong>Reset Color</strong> command.
        /// </summary>
        /// <value>The command.</value>
        public static RoutedCommand ResetColor
        {
            get
            {
                if (_resetColor == null)
                    _resetColor = new RoutedCommand("ResetColor", typeof(ColorPickerCommands));

                return _resetColor;
            }
        }
        private static RoutedCommand _resetColor;
    }
}
