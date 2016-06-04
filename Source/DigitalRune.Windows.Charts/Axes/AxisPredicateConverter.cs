// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Converts string values to a <see cref="Predicate{T}"/> for <see cref="Axis"/>.
    /// </summary>
    public class AxisPredicateConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of
        /// this converter, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="Type"/> that represents the type you want to convert from.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }


        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context and
        /// culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current culture.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        /// <exception cref="NotSupportedException">The conversion cannot be performed.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                switch ((string)value)
                {
                    case "AllAxes":
                        return AxisPredicates.AllAxes;
                    case "None":
                        return AxisPredicates.None;
                    case "PrimaryAxes":
                        return AxisPredicates.PrimaryAxes;
                    case "SecondaryAxes":
                        return AxisPredicates.SecondaryAxes;
                    case "XAxes":
                        return AxisPredicates.XAxes;
                    case "XAxis1":
                        return AxisPredicates.XAxis1;
                    case "XAxis2":
                        return AxisPredicates.XAxis2;
                    case "YAxes":
                        return AxisPredicates.YAxes;
                    case "YAxis1":
                        return AxisPredicates.YAxis1;
                    case "YAxis2":
                        return AxisPredicates.YAxis2;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
