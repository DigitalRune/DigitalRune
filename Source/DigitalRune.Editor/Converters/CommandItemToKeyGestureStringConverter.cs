// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Converts a <see cref="CommandItem"/> to a string that represents the key gesture that
    /// executes the command item.
    /// </summary>
    [ValueConversion(typeof(CommandItem), typeof(string))]
    public class CommandItemToKeyGestureStringConverter : IValueConverter
    {
        // The System.Windows.Input.KeyGestureConverter.
        private static readonly KeyGestureConverter KeyGestureConverter = new KeyGestureConverter();


        /// <summary>
        /// An instance of the <see cref="CommandItemToKeyGestureStringConverter"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly CommandItemToKeyGestureStringConverter Instance = new CommandItemToKeyGestureStringConverter();


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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var commandItem = value as CommandItem;
            if (commandItem == null)
                return DependencyProperty.UnsetValue;

            if (commandItem.InputGestures == null || commandItem.InputGestures.Count == 0)
                return string.Empty;

            var keyGesture = commandItem.InputGestures[0] as KeyGesture;
            if (keyGesture == null)
                return DependencyProperty.UnsetValue;

            if (!string.IsNullOrEmpty(keyGesture.DisplayString))
                return keyGesture.DisplayString;

            return KeyGestureConverter.ConvertTo(null, culture, keyGesture, typeof(string));
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
