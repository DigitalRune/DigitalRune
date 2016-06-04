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
    /// Compares a value of type <see cref="Double"/> with the converter parameter and returns the
    /// minimum.
    /// </summary>
    /// <remarks>
    /// This converter can also be used for values that can be parsed <see cref="Double"/> values.
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(double), typeof(double), ParameterType = typeof(double))]
#endif
    public class MinValueConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="MinValueConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="MinValueConverter"/>.</value>
        public static MinValueConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MinValueConverter();

                return _instance;
            }
        }
        private static MinValueConverter _instance;


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
                double value1 = ObjectHelper.ConvertTo<double>(value, culture);
                double value2 = ObjectHelper.ConvertTo<double>(parameter, culture);
                return Math.Min(value1, value2);
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }


        /// <summary>
        /// Not implemented.
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
