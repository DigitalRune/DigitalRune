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
    /// Converts an <see cref="KeyGesture"/> to a string that represents the key gesture that
    /// executes the command item.
    /// </summary>
    [ValueConversion(typeof(KeyGesture), typeof(string))]
    public class KeyGestureToStringConverter : IValueConverter
    {
        // Note: 
        // I first used our ValueToStringConverter which uses the original 
        // System.Windows.Input.KeyGestureConverter. But for a "Ctrl+X" key gesture this 
        // converter.ConvertToString(...) returned "Ctrl+X, Ctrl+X".
        // --> I don't know what is going wrong there, so we use our own converter.


        // The System.Windows.Input.KeyGestureConverter.
        private static readonly KeyGestureConverter KeyGestureConverter = new KeyGestureConverter();


        /// <summary>
        /// An instance of the <see cref="KeyGestureToStringConverter"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly KeyGestureToStringConverter Instance = new KeyGestureToStringConverter();


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

            var gesture = value as KeyGesture;
            if (gesture == null)
                return DependencyProperty.UnsetValue;

            if (!string.IsNullOrEmpty(gesture.DisplayString))
                return gesture.DisplayString;

            return KeyGestureConverter.ConvertTo(null, culture, gesture, typeof(string));
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
