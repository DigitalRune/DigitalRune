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
    /// Converts an <see cref="Enum"/> to an <see cref="Array"/> with all defined enumeration
    /// values.
    /// </summary>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(Enum), typeof(Array))]
#endif
    public class EnumToArrayConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="EnumToArrayConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="EnumToArrayConverter"/>.</value>
        public static EnumToArrayConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EnumToArrayConverter();

                return _instance;
            }
        }
        private static EnumToArrayConverter _instance;


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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
#if SILVERLIGHT || WINDOWS_PHONE
                return EnumHelper.GetValues(value.GetType());
#else
                return Enum.GetValues(value.GetType());
#endif
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
