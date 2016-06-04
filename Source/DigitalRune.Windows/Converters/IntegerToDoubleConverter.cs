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
    /// Converts a floating-point double precision <see cref="double"/> to an integer 
    /// (<see cref="byte"/>, <see cref="short"/>, <see cref="int"/>, etc.) and back.
    /// </summary>
#if !SILVERLIGHT && !WINDOWS_PHONE
    //[ValueConversion(typeof(int), typeof(double))]
#endif
    public class IntegerToDoubleConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="IntegerToDoubleConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="IntegerToDoubleConverter"/>.</value>
        public static IntegerToDoubleConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new IntegerToDoubleConverter();

                return _instance;
            }
        }
        private static IntegerToDoubleConverter _instance;



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
            {
                if (targetType == typeof(sbyte))
                    return (sbyte)(double)value;
                if (targetType == typeof(byte))
                    return (byte)(double)value;
                if (targetType == typeof(short))
                    return (short)(double)value;
                if (targetType == typeof(ushort))
                    return (ushort)(double)value;
                if (targetType == typeof(int))
                    return (int)(double)value;
                if (targetType == typeof(uint))
                    return (uint)(double)value;
                if (targetType == typeof(long))
                    return (long)(double)value;
                if (targetType == typeof(ulong))
                    return (ulong)(double)value;

                return DependencyProperty.UnsetValue;
            }

            if (value is byte)
                return (double)(byte)value;
            if (value is sbyte)
                return (double)(sbyte)value;
            if (value is short)
                return (double)(short)value;
            if (value is ushort)
                return (double)(ushort)value;
            if (value is int)
                return (double)(int)value;
            if (value is uint)
                return (double)(uint)value;
            if (value is long)
                return (double)(long)value;
            if (value is ulong)
                return (double)(ulong)value;

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
            return Convert(value, targetType, parameter, culture);
        }
    }
}
