// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Provides methods for formatting strings.
    /// </summary>
    internal static class FormatHelper
    {
        /// <summary>
        /// Provides the default character (',') for separating lists of numbers.
        /// </summary>
        /// <remarks>
        /// The character ',' is used for separating lists of numbers unless a culture uses the
        /// character as the decimal separator. In this case the character ';' is used.
        /// </remarks>
        /// <seealso cref="AltNumberListSeparator"/>
        /// <seealso cref="GetNumberListSeparator"/>
        public const char NumberListSeparator = ',';


        /// <summary>
        /// Provides the alternate character (';') for separating list of numbers.
        /// </summary>
        /// <inheritdoc cref="NumberListSeparator"/>
        /// <seealso cref="NumberListSeparator"/>
        /// <seealso cref="GetNumberListSeparator"/>
        public const char AltNumberListSeparator = ';';


        /// <summary>
        /// Gets the string that is used as the decimal separator in numeric values for the
        /// specified provider.
        /// </summary>
        /// <param name="provider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The string that is used as the decimal separator in numeric values.</returns>
        public static string GetNumberDecimalSeparator(IFormatProvider provider)
        {
            var numberFormatInfo = NumberFormatInfo.GetInstance(provider);
            return numberFormatInfo.NumberDecimalSeparator;
        }


        /// <summary>
        /// Gets the character used to separate lists of numbers for the specified provider.
        /// </summary>
        /// <param name="provider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The character used to separate lists of numbers.</returns>
        public static char GetNumberListSeparator(IFormatProvider provider)
        {
            string numberDecimalSeparator = GetNumberDecimalSeparator(provider);
            if (numberDecimalSeparator.Length > 0 && numberDecimalSeparator[0] == NumberListSeparator)
                return AltNumberListSeparator;

            return NumberListSeparator;
        }
    }
}
