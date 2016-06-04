// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines a range of date/time values.
    /// </summary>
    [TypeConverter(typeof(DateTimeRangeConverter))]
    public struct DateTimeRange : IEquatable<DateTimeRange>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly DateTime _min;
        private readonly DateTime _max;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the minimum time.
        /// </summary>
        /// <value>The minimum time.</value>
        public DateTime Min
        {
            get { return _min; }
        }


        /// <summary>
        /// Gets the maximum time.
        /// </summary>
        /// <value>The maximum time.</value>
        public DateTime Max
        {
            get { return _max; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeRange"/> struct.
        /// </summary>
        /// <param name="min">The minimum time.</param>
        /// <param name="max">The maximum time.</param>
        public DateTimeRange(DateTime min, DateTime max)
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
        public bool Equals(DateTimeRange other)
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
            return obj is DateTimeRange && Equals((DateTimeRange)obj);
        }


        /// <summary>
        /// Compares two <see cref="DateTimeRange"/> objects to determine whether they are the same.
        /// </summary>
        /// <param name="left">The first range.</param>
        /// <param name="right">The second range.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DateTimeRange left, DateTimeRange right)
        {
            return left.Equals(right);
        }


        /// <summary>
        /// Compares two <see cref="DateTimeRange"/> objects to determine whether they are different.
        /// </summary>
        /// <param name="left">The first range.</param>
        /// <param name="right">The second range.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="left"/> and <paramref name="right"/> are
        /// different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(DateTimeRange left, DateTimeRange right)
        {
            return !left.Equals(right);
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
        /// Converts the string representation of a range to its <see cref="DateTimeRange"/>
        /// equivalent.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Converts the string representation of a range to its <see cref="DateTimeRange"/>
        /// equivalent.
        /// </summary>
        /// <param name="s">A string representation of a range.</param>
        /// <returns>
        /// A <see cref="DateTimeRange"/> that represents the range specified by the
        /// <paramref name="s"/> parameter.
        /// </returns>
        /// <exception cref="FormatException">
        /// <paramref name="s"/> is not a valid <see cref="DateTimeRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        public static DateTimeRange Parse(string s)
        {
            return Parse(s, CultureInfo.CurrentCulture);
        }


        /// <summary>
        /// Converts the string representation of a range in a specified culture-specific format to
        /// its <see cref="DateTimeRange"/> equivalent.
        /// </summary>
        /// <param name="s">A string representation of a range.</param>
        /// <param name="provider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific formatting information
        /// about <paramref name="s"/>. 
        /// </param>
        /// <returns>
        /// A <see cref="DateTimeRange"/> that represents the range specified by the
        /// <paramref name="s"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="s"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref name="s"/> is not a valid <see cref="DateTimeRange"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "s")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DateTimeRange")]
        public static DateTimeRange Parse(string s, IFormatProvider provider)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            var tokens = s.Split(new[] { FormatHelper.GetNumberListSeparator(provider) }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 2)
            {
                return new DateTimeRange(
                    DateTime.Parse(tokens[0], provider),
                    DateTime.Parse(tokens[1], provider));
            }

            throw new FormatException("String is not a valid DateTimeRange.");
        }


        /// <summary>
        /// Clamps the specified value to the interval [<see cref="_min"/>, <see cref="_max"/>].
        /// </summary>
        /// <param name="value">The value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public DateTime Clamp(DateTime value)
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
        public bool Contains(DateTime value)
        {
            return _min <= value && value <= _max;
        }
        #endregion
    }
}
