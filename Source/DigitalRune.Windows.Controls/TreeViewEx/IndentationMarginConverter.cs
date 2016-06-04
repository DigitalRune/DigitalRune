// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Calculates the margin for the specified indentation level.
    /// </summary>
    /// <remarks>
    /// The converter parameter is the margin size of one intendation level.
    /// </remarks>
    [ValueConversion(typeof(int), typeof(Thickness), ParameterType = typeof(double))]
    public class IndentationMarginConverter : IValueConverter
    {
        private static readonly object DefaultThickness = new Thickness();


        /// <summary>
        /// Gets an instance of the <see cref="IndentationMarginConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="IndentationMarginConverter"/>.</value>
        public static IndentationMarginConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IndentationMarginConverter();

                return _instance;
            }
        }
        private static IndentationMarginConverter _instance;


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">
        /// The indentation size in device-independent pixels.
        /// </param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is
        /// used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                int level = ObjectHelper.ConvertTo<int>(value);
                if (level == 0)
                    return DefaultThickness;

                double size = ObjectHelper.ConvertTo<double>(parameter);
                return new Thickness(level * size, 0, 0, 0);
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
