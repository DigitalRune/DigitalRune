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
    /// Returns the string representation of an <see cref="object"/>.
    /// </summary>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(object), typeof(string))]
#endif
    public class ValueToStringConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="ValueToStringConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="ValueToStringConverter"/>.</value>
        public static ValueToStringConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ValueToStringConverter();

                return _instance;
            }
        }
        private static ValueToStringConverter _instance;


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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (value is string)
                return value;

            try
            {
                var converter = ObjectHelper.GetTypeConverter(value.GetType());
                var result = converter.ConvertToString(value);
                return result;
            }
            catch
            {
                return value.ToString();
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
