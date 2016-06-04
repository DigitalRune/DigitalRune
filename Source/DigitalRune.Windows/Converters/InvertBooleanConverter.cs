// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Inverts a <see cref="bool"/> value.
    /// </summary>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(object), typeof(bool))]
#endif
    public class InvertBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="InvertBooleanConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="InvertBooleanConverter"/>.</value>
        public static InvertBooleanConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InvertBooleanConverter();

                return _instance;
            }
        }
        private static InvertBooleanConverter _instance;


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
            try
            {
                bool b = ObjectHelper.ConvertTo<bool>(value, culture);
                return !b;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
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
            return Convert(value, targetType, parameter, culture);
        }
    }
}
