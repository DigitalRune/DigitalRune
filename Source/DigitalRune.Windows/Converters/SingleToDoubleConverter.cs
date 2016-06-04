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
    /// Converts a floating-point value between single and double precision (that means between the
    /// types <see cref="Single"/> and <see cref="Double"/>).
    /// </summary>
    /// <remarks>
    /// This converter works both ways: If the input value is a <see cref="Double"/> then the
    /// converted value is a <see cref="Single"/> value. If the input value is a
    /// <see cref="Single"/> then the converted value is a <see cref="Double"/> value.
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(float), typeof(double))]
#endif
    public class SingleToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="SingleToDoubleConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="SingleToDoubleConverter"/>.</value>
        public static SingleToDoubleConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SingleToDoubleConverter();

                return _instance;
            }
        }
        private static SingleToDoubleConverter _instance;



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
            if (value is double)
                return (float)(double)value;
            if (value is float)
                return (double)(float)value;

            return DependencyProperty.UnsetValue;
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
