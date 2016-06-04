// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows.Data;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Converts a <see cref="string"/> to uppercase. (Global instance to be used by the main menu
    /// of an application.)
    /// </summary>
    [ValueConversion(typeof(string), typeof(string))]
    public sealed class MenuToUpperConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether text should be converted to upper-case.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to convert text to upper-case; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public static bool IsEnabled { get; set; } = true;


        /// <summary>
        /// Gets an instance of the <see cref="MenuToUpperConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="MenuToUpperConverter"/>.</value>
        public static MenuToUpperConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new MenuToUpperConverter();

                return _instance;
            }
        }
        private static MenuToUpperConverter _instance;


        /// <summary>
        /// Prevents a default instance of the <see cref="MenuToUpperConverter"/> class from being created.
        /// </summary>
        private MenuToUpperConverter()
        {
        }


        /// <summary>
        /// Converts a value.
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
            return IsEnabled ? ToUpperConverter.Instance.Convert(value, targetType, parameter, culture) : value;
        }


        /// <summary>
        /// Not supported.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the target dependency property.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
