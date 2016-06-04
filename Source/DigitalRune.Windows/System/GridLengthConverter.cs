// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if SILVERLIGHT || WINDOWS_PHONE
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using DigitalRune.Mathematics;


namespace System.Windows
{
    /// <summary>
    /// Converts instances of other types to and from <see cref="GridLength"/> instances.
    /// </summary>
    public class GridLengthConverter : TypeConverter
    {
        private static readonly double[] PixelUnitFactors = new[] { 96.0, 37.795275590551178, 1.3333333333333333 };
        private static readonly string[] PixelUnitStrings = new[] { "in", "cm", "pt" };
        private static readonly string[] UnitStrings = new[] { "auto", "px", "*" };


        /// <summary>
        /// Determines whether a class can be converted from a given type to an instance of
        /// <see cref="GridLength"/>.
        /// </summary>
        /// <param name="typeDescriptorContext">Describes the context information of a type.</param>
        /// <param name="sourceType">
        /// The type of the source that is being evaluated for conversion.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the converter can convert from the specified type to an
        /// instance of GridLength; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            switch (Type.GetTypeCode(sourceType))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Determines whether an instance of <see cref="GridLength"/> can be converted to a
        /// different type.
        /// </summary>
        /// <param name="typeDescriptorContext">Describes the context information of a type.</param>
        /// <param name="destinationType">
        /// The desired type that this instance of GridLength is being evaluated for conversion.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the converter can convert this instance of GridLength to the
        /// specified type; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return (destinationType == typeof(string));
        }


        /// <summary>
        /// Converts the given string to the type of this converter, using the invariant culture.
        /// </summary>
        /// <param name="text">The <see cref="string"/> to convert.</param>
        /// <returns>An <see cref="object"/> that represents the converted text.</returns>
        public object ConvertFromInvariantString(string text)
        {
            return ConvertFromString(null, CultureInfo.InvariantCulture, text);
        }


        /// <summary>
        /// Converts the given string to the type of this converter, using the invariant culture.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="text">The <see cref="string"/> to convert.</param>
        /// <returns>An <see cref="object"/> that represents the converted text.</returns>
        public object ConvertFromInvariantString(ITypeDescriptorContext context, string text)
        {
            return ConvertFromString(context, CultureInfo.InvariantCulture, text);
        }


        /// <summary>
        /// Converts the given string to the type of this converter.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="text">The <see cref="string"/> to convert.</param>
        /// <returns>An <see cref="object"/> that represents the converted text.</returns>
        public object ConvertFromString(ITypeDescriptorContext context, string text)
        {
            return ConvertFrom(context, CultureInfo.CurrentCulture, text);
        }


        /// <summary>
        /// Converts the given string to the type of this converter.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="culture">
        /// Cultural specific information that should be respected during conversion.
        /// </param>
        /// <param name="text">The <see cref="string"/> to convert.</param>
        /// <returns>An <see cref="object"/> that represents the converted text.</returns>
        public object ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text)
        {
            return ConvertFrom(context, culture, text);
        }


        /// <summary>
        /// Attempts to convert a specified object to an instance of GridLength.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="culture">
        /// Cultural specific information that should be respected during conversion.
        /// </param>
        /// <param name="source">The object being converted.</param>
        /// <returns>
        /// The instance of <see cref="GridLength"/> that is created from the converted
        /// <paramref name="source"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="source"/> is <see langword="null"/>.
        /// </exception>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (source is string)
                return FromString((string)source, culture);

            double length = Convert.ToDouble(source, culture);

            if (Numeric.IsNaN(length))
                return new GridLength(1.0, GridUnitType.Auto);

            return new GridLength(length, GridUnitType.Pixel);
        }


        /// <summary>
        /// Converts the specified value to a culture-invariant string representation.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="string"/> that represents the converted value.</returns>
        public string ConvertToInvariantString(object value)
        {
            return ConvertToString(null, CultureInfo.InvariantCulture, value);
        }


        /// <summary>
        /// Converts the specified value to a culture-invariant string representation.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="string"/> that represents the converted value.</returns>
        public string ConvertToInvariantString(ITypeDescriptorContext context, object value)
        {
            return ConvertToString(context, CultureInfo.InvariantCulture, value);
        }


        /// <summary>
        /// Converts the specified value to a string representation.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="string"/> that represents the converted value.</returns>
        public string ConvertToString(ITypeDescriptorContext context, object value)
        {
            return (string)ConvertTo(context, CultureInfo.CurrentCulture, value, typeof(string));
        }


        /// <summary>
        /// Converts the specified value to a string representation.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="culture">
        /// The culture info. Cultural specific information that should be respected during
        /// conversion.
        /// </param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="string"/> that represents the converted value.</returns>
        public string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return (string)ConvertTo(context, culture, value, typeof(string));
        }


        /// <summary>
        /// Attempts to convert an instance of GridLength to a specified type.
        /// </summary>
        /// <param name="context">Describes the context information of a type.</param>
        /// <param name="culture">
        /// The culture info. Cultural specific information that should be respected during
        /// conversion.
        /// </param>
        /// <param name="value">The instance of <see cref="GridLength"/> to convert.</param>
        /// <param name="type">
        /// The type that this instance of <see cref="GridLength"/> is converted to.
        /// </param>
        /// <returns>The object that is created from the converted instance of GridLength.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="value"/> or <paramref name="type"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="type"/> is not one of the valid types for conversion.
        /// </exception>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type type)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            if (!(value is GridLength))
                throw new ArgumentException("value is not of type GridLength.");

            if (type == null)
                throw new ArgumentNullException("type");
            if (type != typeof(string))
                throw new ArgumentException("Destination type is not one of the valid types for conversion.", "type");

            GridLength gridLength = (GridLength)value;
            return ToString(gridLength, culture);
        }


        internal static GridLength FromString(string text, CultureInfo cultureInfo)
        {
            text = text.Trim().ToLowerInvariant();
            int textLength = text.Length;

            double value;
            GridUnitType unitType = GridUnitType.Pixel;
            int unitStringLength = 0;
            double unitFactor = 1.0;

            int index = 0;
            if (text == UnitStrings[index])
            {
                // "auto"
                unitStringLength = UnitStrings[index].Length;
                unitType = (GridUnitType)index;
            }
            else
            {
                // "px" or "*"
                index = 1;
                while (index < UnitStrings.Length)
                {
                    if (text.EndsWith(UnitStrings[index], StringComparison.Ordinal))
                    {
                        unitStringLength = UnitStrings[index].Length;
                        unitType = (GridUnitType)index;
                        break;
                    }
                    index++;
                }
            }

            if (index >= UnitStrings.Length)
            {
                // GridUnitType is Pixel with unit "in", "cm", "pt".
                Debug.Assert(unitType == GridUnitType.Pixel);
                for (index = 0; index < PixelUnitStrings.Length; index++)
                {
                    if (text.EndsWith(PixelUnitStrings[index], StringComparison.Ordinal))
                    {
                        unitStringLength = PixelUnitStrings[index].Length;
                        unitFactor = PixelUnitFactors[index];
                        break;
                    }
                }
            }

            if (textLength == unitStringLength && (unitType == GridUnitType.Auto || unitType == GridUnitType.Star))
            {
                value = 1.0;
            }
            else
            {
                string valueString = text.Substring(0, textLength - unitStringLength);
                value = Convert.ToDouble(valueString, cultureInfo) * unitFactor;
            }

            return new GridLength(value, unitType);
        }


        private static string ToString(GridLength gridLength, CultureInfo cultureInfo)
        {
            if (gridLength.GridUnitType == GridUnitType.Auto)
            {
                return "Auto";
            }

            if (gridLength.GridUnitType == GridUnitType.Star)
            {
                if (Numeric.AreEqual(1.0, gridLength.Value))
                    return "*";

                return (Convert.ToString(gridLength.Value, cultureInfo) + "*");
            }

            return Convert.ToString(gridLength.Value, cultureInfo);
        }
    }
}
#endif
