// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System;
using System.Globalization;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Converts a value to and from a <see cref="bool"/> by comparing the value with a reference
    /// value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Conversion works in both directions:
    /// <list type="bullet">
    /// <item>
    /// When converting to <see cref="bool"/>: The value is compared with the converter parameter.
    /// If the value equals the converter parameter the result is <see langword="true"/>; otherwise
    /// <see langword="false"/>.
    /// </item>
    /// <item>
    /// When converting from <see cref="bool"/>: If the <see cref="bool"/> is <see langword="true"/>
    /// the converter parameter will be returned. If the <see cref="bool"/> is <see langword="false"/> 
    /// the <see cref="DefaultValue"/> will be if returned, otherwise <see langword="null"/> for
    /// reference types or zero for value types.
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
#if !SILVERLIGHT && !WINDOWS_PHONE
    [ValueConversion(typeof(object), typeof(bool), ParameterType = typeof(object))]
#endif
    public class ValueToBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the default value that is set when <see langword="false"/> is converted
        /// back to the original value type.
        /// </summary>
        /// <value>
        /// The default value that is set when a value of <see langword="false"/> is converted back
        /// to the original value type. (This property is optional and only needs to be set when
        /// backward conversions are required.)
        /// </value>
        public object DefaultValue { get; set; }


        /// <summary>
        /// Converts a value to a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value"/> equals <paramref name="parameter"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                object referenceValue = parameter;
                if (value != null && parameter != null && value.GetType() != parameter.GetType())
                {
                    // Parameter is not of the correct type. (Probably a string set in XAML.)
                    // Try to convert the parameter to the correct type.
                    TypeConverter typeConverter = ObjectHelper.GetTypeConverter(value.GetType());
                    if (typeConverter.CanConvertFrom(parameter.GetType()))
                        referenceValue = typeConverter.ConvertFrom(parameter);
                }

                if (targetType == typeof(bool) || targetType == typeof(bool?))
                    return Equals(value, referenceValue);
            }
            catch
            {
            }

            return DependencyProperty.UnsetValue;
        }


        /// <summary>
        /// Converts a value from <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// <paramref name="parameter"/> if <paramref name="value"/> is <see langword="true"/>;
        /// otherwise, <see cref="DefaultValue"/> is returned.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            try
            {
                bool boolValue = (bool)value;
                if (boolValue)
                {
                    // Boolean is true.

                    if (parameter != null)
                    {
                        if (targetType.IsAssignableFrom(parameter.GetType()))
                        {
                            // Parameter can be assigned to the target type.
                            return parameter;
                        }

                        // Parameter is of the wrong type. (Probably a string set in XAML.)
                        // Try to convert the parameter to the correct type.
                        TypeConverter typeConverter = ObjectHelper.GetTypeConverter(targetType);
                        if (typeConverter.CanConvertFrom(parameter.GetType()))
                            return typeConverter.ConvertFrom(parameter);
                    }
                    else
                    {
                        if (!targetType.IsValueType)
                            return null;
                    }
                }
                else
                {
                    // Boolean is false.

                    if (DefaultValue != null)
                    {
                        Type defaultValueType = DefaultValue.GetType();
                        if (targetType.IsAssignableFrom(defaultValueType))
                        {
                            // DefaultValue can be assigned to the target type.
                            return DefaultValue;
                        }

                        // DefaultValue is of the wrong type. (Probably a string set in XAML.)
                        // Try to convert DefaultValue to the correct type.
                        TypeConverter typeConverter = ObjectHelper.GetTypeConverter(targetType);
                        if (typeConverter.CanConvertFrom(defaultValueType))
                            return typeConverter.ConvertFrom(DefaultValue);
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
