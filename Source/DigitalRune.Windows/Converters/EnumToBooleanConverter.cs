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
    /// Converts an <see cref="Enum"/> value to a <see cref="bool"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="Enum"/> value is converted to <see langword="true"/> if it matches the
    /// converter parameter.
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(Enum), typeof(bool), ParameterType = typeof(Enum))]
#endif
    public class EnumToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="EnumToBooleanConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="EnumToBooleanConverter"/>.</value>
        public static EnumToBooleanConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EnumToBooleanConverter();

                return _instance;
            }
        }
        private static EnumToBooleanConverter _instance;


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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return DependencyProperty.UnsetValue;

            if (value is Enum)
            {
                if (parameter is Enum)
                    return value.Equals(parameter);

                string parameterString = parameter as string;
                if (parameterString != null)
                {
                    object parameterEnum = Enum.Parse(value.GetType(), parameterString, true);
                    return value.Equals(parameterEnum);
                }
            }
            else if (parameter is Enum)
            {
                string valueString = value as string;
                if (valueString != null)
                {
                    object valueEnum = Enum.Parse(parameter.GetType(), valueString, true);
                    return parameter.Equals(valueEnum);
                }
            }

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
            try
            {
                bool b = ObjectHelper.ConvertTo<bool>(value, culture);
                if (b)
                {
                    if (parameter is Enum)
                        return parameter;

                    string parameterString = parameter as string;
                    if (parameterString != null)
                        return Enum.Parse(targetType, parameterString, true);
                }
            }
            catch
            {
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
