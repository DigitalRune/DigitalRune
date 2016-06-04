// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Converts a command item to a tooltip with text and shortcut info.
    /// </summary>
    [ValueConversion(typeof(CommandItem), typeof(string))]
    public class CommandItemToToolTipConverter : IValueConverter
    {
        /// <summary>
        /// An instance of the <see cref="CommandItemToToolTipConverter"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly CommandItemToToolTipConverter Instance = new CommandItemToToolTipConverter();


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
        /// used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Create a string of the form "Tooltip text.".
            var commandItem = value as ICommandItem;
            if (commandItem == null)
                return DependencyProperty.UnsetValue;

            var commandItem2 = value as CommandItem;
            if (commandItem2?.InputGestures == null || commandItem2.InputGestures.Count == 0)
                return commandItem.ToolTip;

            // Create a string of the form "Tooltip text. (Shortcut)".
            var toolTip = new StringBuilder();
            if (commandItem2.ToolTip != null)
                toolTip.Append(commandItem2.ToolTip);

            var converter = KeyGestureToStringConverter.Instance;
            var keyGestureString = converter.Convert(commandItem2.InputGestures[0], typeof(string), null, culture) as string;
            if (keyGestureString != null)
            {
                toolTip.Append(" (");
                toolTip.Append(keyGestureString);
                toolTip.Append(')');
            }

            return toolTip.ToString();
        }


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
        /// used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
