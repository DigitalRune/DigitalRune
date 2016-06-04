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
    /// Converts a <see cref="bool"/> value to a <see cref="Visibility"/> value where
    /// <see langword="false"/> is converted to <see cref="Visibility.Visible"/> and
    /// <see langword="true"/> is converted to <see cref="Visibility.Collapsed"/>.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="InvertedBooleanToVisibilityConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="InvertedBooleanToVisibilityConverter"/>.</value>
        public static InvertedBooleanToVisibilityConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new InvertedBooleanToVisibilityConverter();

                return _instance;
            }
        }
        private static InvertedBooleanToVisibilityConverter _instance;


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
            try
            {
                return ObjectHelper.ConvertTo<bool>(value, culture) ? Visibility.Collapsed : Visibility.Visible;
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }


        /// <summary>
        /// Modifies the target data before passing it to the source object. This method is called
        /// only in <see cref="BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">
        /// The <see cref="Type"/> of data expected by the source object.
        /// </param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Visibility? visibility = null;
                if (value is Visibility)
                    visibility = (Visibility)value;
                else if (value is string)
                    visibility = (Visibility)Enum.Parse(typeof(Visibility), (string)value, true);

                if (visibility.HasValue)
                {
                    switch (visibility.Value)
                    {
                        case Visibility.Visible:
                            return false;
                        case Visibility.Collapsed:
                            return true;
#if !SILVERLIGHT && !WINDOWS_PHONE
                        case Visibility.Hidden:
                            return DependencyProperty.UnsetValue;
#endif
                    }
                }
            }
            catch
            {
            }

            return DependencyProperty.UnsetValue;
        }
    }
}
