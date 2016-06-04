// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows
{
    /// <summary>
    /// A value converter that returns <see langword="true"/> if the given floating-point value is
    /// NaN.
    /// </summary>
#if !SILVERLIGHT
    [ValueConversion(typeof(float), typeof(bool))]
    [ValueConversion(typeof(double), typeof(bool))]
#endif
    public class IsNaNConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="IsNaNConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="IsNaNConverter"/>.</value>
        public static IsNaNConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IsNaNConverter();

                return _instance;
            }
        }
        private static IsNaNConverter _instance;


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
                return Numeric.IsNaN((double)value);

            if (value is float)
                return Numeric.IsNaN((float)value);

            if (value != null)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    double d = ObjectHelper.ConvertTo<double>(value, culture);
                    return Numeric.IsNaN(d);
                }
                catch
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
            }

            return DependencyProperty.UnsetValue;
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
