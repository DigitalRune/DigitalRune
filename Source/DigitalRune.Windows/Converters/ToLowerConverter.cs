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
    /// Converts a <see cref="String"/> to lowercase.
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public class ToLowerConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="ToLowerConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="ToLowerConverter"/>.</value>
        public static ToLowerConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ToLowerConverter();

                return _instance;
            }
        }
        private static ToLowerConverter _instance;


        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the target dependency property.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            string valueAsString = value as string;

            // If the given value is not a string, then we convert the value.ToString() result.
            if (valueAsString == null)
                valueAsString = value.ToString();

            return valueAsString.ToLower(culture);
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the source object.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
