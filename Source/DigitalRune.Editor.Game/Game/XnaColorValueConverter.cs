// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using XnaColor = Microsoft.Xna.Framework.Color;
using WpfColor = System.Windows.Media.Color;


namespace DigitalRune.Editor.Game
{
    /// <summary>
    /// Converts colors from XNA to WPF (and back).
    /// </summary>
    [ValueConversion(typeof(XnaColor), typeof(WpfColor), ParameterType = typeof(double))]
    public class XnaColorValueConverter : IValueConverter
    {
        /// <summary>
        /// Gets an instance of the <see cref="XnaColorValueConverter"/>.
        /// </summary>
        /// <value>An instance of the <see cref="XnaColorValueConverter"/>.</value>
        public static XnaColorValueConverter Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new XnaColorValueConverter();

                return _instance;
            }
        }
        private static XnaColorValueConverter _instance;


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var color = (XnaColor)value;
                return new WpfColor { A = color.A, B = color.B, G = color.G, R = color.R };
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is WpfColor)
                {
                    var color = (WpfColor)value;
                    return new XnaColor { A = color.A, B = color.B, G = color.G, R = color.R };
                }
                //else if (value is string)
                {
                    var converter = TypeDescriptor.GetConverter(typeof(WpfColor));
                    var color = (WpfColor)converter.ConvertFrom(value);
                    return new XnaColor { A = color.A, B = color.B, G = color.G, R = color.R };
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}
