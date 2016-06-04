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
    /// Returns "visible" if the conversion value is equal to the converter parameter.
    /// </summary>
    /// <example>
    /// This converter can be used in XAML like this:
    /// <code lang="xaml">
    /// <![CDATA[
    /// <TextBlock.Visibility>
    ///   <Binding Path="Type" Converter="{StaticResource ComparisonToVisibilityConverter}">
    ///     <Binding.ConverterParameter>
    ///       <vm:MessageType>Warning</vm:MessageType>
    ///     </Binding.ConverterParameter>
    ///   </Binding>
    /// </TextBlock.Visibility>
    /// ]]>
    /// </code>
    /// In this example 'MessageType' is a user-defined enumeration.
    /// </example>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(object), typeof(Visibility))]
#endif
    public class ComparisonToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="ComparisonToVisibilityConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="ComparisonToVisibilityConverter"/>.</value>
        public static ComparisonToVisibilityConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ComparisonToVisibilityConverter();

                return _instance;
            }
        }
        private static ComparisonToVisibilityConverter _instance;


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
            return Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed;
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
