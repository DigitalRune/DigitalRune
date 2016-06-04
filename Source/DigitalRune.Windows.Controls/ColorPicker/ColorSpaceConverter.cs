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
    /// Converts RGB color components from one color space to another.
    /// </summary>
    [ValueConversion(typeof(double), typeof(double), ParameterType = typeof(ColorSpace))]
    internal class ColorSpaceConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the source color space.
        /// </summary>
        /// <value>The source color space.</value>
        public ColorSpace SourceColorSpace { get; set; }


        /// <summary>
        /// Gets or sets the target color space.
        /// </summary>
        /// <value>The target color space.</value>
        public ColorSpace TargetColorSpace { get; set; }


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
            if (value is double && targetType == typeof(double))
                return Convert((double)value, SourceColorSpace, TargetColorSpace);

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
            if (value is double && targetType == typeof(double))
                return Convert((double)value, TargetColorSpace, SourceColorSpace);

            return DependencyProperty.UnsetValue;
        }


        private static double Convert(double c, ColorSpace sourceSpace, ColorSpace targetSpace)
        {
            // Convert source space --> sRGB --> target space.
            switch (sourceSpace)
            {
                case ColorSpace.SRgb:
                    break;
                case ColorSpace.Linear:
                    c = ColorHelper.ToSRgb(c);
                    break;
            }

            switch (targetSpace)
            {
                case ColorSpace.SRgb:
                    break;
                case ColorSpace.Linear:
                    c = ColorHelper.ToLinear(c);
                    break;
            }

            return c;
        }
    }
}
