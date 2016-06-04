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
    /// Compares two values of type <see cref="Double"/> and returns the comparison result as
    /// <see cref="Boolean"/>.
    /// </summary>
    /// <remarks>
    /// The input value is compared to the converter parameter using a tolerance of
    /// <see cref="Numeric.EpsilonD"/>. The property <see cref="Comparison"/> determines how the two
    /// values are compared.
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(double), typeof(bool), ParameterType = typeof(double))]
#endif
    public class DoubleToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the comparison operator.
        /// </summary>
        /// <value>The comparison operator.</value>
        public ComparisonOperator Comparison { get; set; }


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

                switch (Comparison)
                {
                    case ComparisonOperator.Equal:
                        return Numeric.AreEqual(value1, value2);

                    case ComparisonOperator.NotEqual:
                        return !Numeric.AreEqual(value1, value2);

                    case ComparisonOperator.Greater:
                        return Numeric.IsGreater(value1, value2);

                    case ComparisonOperator.GreaterOrEqual:
                        return Numeric.IsGreaterOrEqual(value1, value2);

                    case ComparisonOperator.Less:
                        return Numeric.IsLess(value1, value2);

                    case ComparisonOperator.LessOrEqual:
                        return Numeric.IsLessOrEqual(value1, value2);

                    default:
                        return DependencyProperty.UnsetValue;
                }
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
