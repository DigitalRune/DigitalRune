// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines a bounded interval [min, max].
    /// </summary>
    [TypeConverter(typeof(DoubleRangeConverter))]
    public struct DoubleRange : IEquatable<DoubleRange>
    {
        // Notes:
        // Equals() and == behave differently! Comparisons are consistent with double.
        //   double.NaN.Equals(double.NaN) ... true
        //   double.NaN == double.NaN ........ false
        //
        //   var rangeNaN = new DateTimeRange(double.NaN, doubleNaN);
        //   rangeNaN.Equals(rangeNaN) ...... true
        //   rangeNaN == rangeNaN ........... false


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly double _min;
        private readonly double _max;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public double Min
        {
            get { return _min; }
        }


        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public double Max
        {
            get { return _max; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleRange"/> struct.
        /// </summary>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        public DoubleRange(double min, double max)
        {
            _min = min;
            _max = max;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------


        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(DoubleRange other)
        {
            return _min.Equals(other._min) && _max.Equals(other._max);
        }


        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="object"/> is equal to this instance;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is DoubleRange && Equals((DoubleRange)obj);
        }


        /// <summary>
        /// Compares two <see cref="DoubleRange"/> objects to determine whether they are the same.
        /// </summary>
        /// <param name="left">The first range.</param>
        /// <param name="right">The second range.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DoubleRange left, DoubleRange right)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return left._min == right._min && left._max == right._max;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


        /// <summary>
        /// Compares two <see cref="DoubleRange"/> objects to determine whether they are different.
        /// </summary>
        /// <param name="left">The first range.</param>
        /// <param name="right">The second range.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(DoubleRange left, DoubleRange right)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return left._min != right._min || left._max != right._max;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data
        /// structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (_min.GetHashCode() * 397) ^ _max.GetHashCode();
            }
        }


        /// <overloads>
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Returns the string representation of this instance using the current culture.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString()
        {
            return ToString(CultureInfo.CurrentCulture);
        }


        /// <summary>
        /// Returns the string representation of this instance using the specified culture-specific
        /// format information.
        /// </summary>
        /// <param name="provider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The string representation of this instance.</returns>
        public string ToString(IFormatProvider provider)
        {
            char separator = FormatHelper.GetNumberListSeparator(provider);
            return string.Format(provider, "{0}{1} {2}", _min, separator, _max);
        }


        /// <overloads>
        /// <summary>
        /// Converts the string representation of a range to its <see cref="DoubleRange"/>
        /// equivalent.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Converts the string representation of a range to its <see cref="DoubleRange"/>
        /// equivalent.
        /// </summary>
        /// <param name="s">A string representation of a range.</param>
        /// <returns>
        /// A <see cref="DoubleRange"/> that represents the range specified by the
        /// <paramref name="s"/> parameter.
        /// </returns>
        /// <exception cref="FormatException">
        /// <paramref name="s"/> is not a valid <see cref="DoubleRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static DoubleRange Parse(string s)
        {
            return Parse(s, CultureInfo.CurrentCulture);
        }


        /// <summary>
        /// Converts the string representation of a range in a specified culture-specific format to
        /// its <see cref="DoubleRange"/> equivalent.
        /// </summary>
        /// <param name="s">A string representation of a range.</param>
        /// <param name="provider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information
        /// about <paramref name="s"/>. 
        /// </param>
        /// <returns>
        /// A <see cref="DoubleRange"/> that represents the range specified by the
        /// <paramref name="s"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="s"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="s"/> is not a valid <see cref="DoubleRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DoubleRange")]
        public static DoubleRange Parse(string s, IFormatProvider provider)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            var tokens = s.Split(new[] { FormatHelper.GetNumberListSeparator(provider) }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                return new DoubleRange(
                    double.Parse(tokens[0], provider),
                    double.Parse(tokens[1], provider));
            }

            throw new FormatException("String is not a valid DoubleRange.");
        }


        /// <summary>
        /// Clamps the specified value to the interval [<see cref="_min"/>, <see cref="_max"/>].
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public double Clamp(double value)
        {
            if (value < _min)
                return _min;

            if (value > _max)
                return _max;

            return value;
        }


        /// <summary>
        /// Determines whether a value is inside the interval
        /// [<see cref="_min"/>, <see cref="_max"/>].
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="value"/> is inside
        /// [<see cref="_min"/>, <see cref="_max"/>]; otherwise <see langword="false"/>.
        /// </returns>
        public bool Contains(double value)
        {
            return _min <= value && value <= _max;
        }
        #endregion
    }
}
