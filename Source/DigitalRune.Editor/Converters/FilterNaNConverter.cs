// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows.Data;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Removes all NaN from a <see cref="double"/> value (NaN values are replaced by 0).
    /// </summary>
    [ValueConversion(typeof(double), typeof(double))]
    public class FilterNaNConverter : IValueConverter
    {
        /// <summary>
        /// An instance of the <see cref="MenuIconConverter"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly FilterNaNConverter Instance = new FilterNaNConverter();


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
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
                var d = (double)value;
                if (double.IsNaN(d))
                    d = 0;
                return d;
            }

            return value;
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
            throw new NotImplementedException();
        }
    }
}
