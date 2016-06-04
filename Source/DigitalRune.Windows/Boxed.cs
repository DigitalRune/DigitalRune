// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Media;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Provides boxed values for common values.
    /// </summary>
    public static class Boxed
    {
        /// <summary>
        /// The value <see langword="false"/>.
        /// </summary>
        public static readonly object BooleanFalse = false;


        /// <summary>
        /// The value <see langword="true"/>.
        /// </summary>
        public static readonly object BooleanTrue = true;


        /// <summary>
        /// The value 0 as <see cref="int"/>.
        /// </summary>
        public static readonly object Int32Zero = 0;


        /// <summary>
        /// The value 1 as <see cref="int"/>.
        /// </summary>
        public static readonly object Int32One = 1;


        /// <summary>
        /// The value 0 as <see cref="float"/>.
        /// </summary>
        public static readonly object SingleZero = 0.0f;


        /// <summary>
        /// The value 1 as <see cref="float"/>.
        /// </summary>
        public static readonly object SingleOne = 1.0f;


        /// <summary>
        /// The value +infinity as <see cref="float"/>.
        /// </summary>
        public static readonly object SinglePositiveInfinity = float.PositiveInfinity;


        /// <summary>
        /// The value -infinity as <see cref="float"/>.
        /// </summary>
        public static readonly object SingleNegativeInfinity = float.NegativeInfinity;


        /// <summary>
        /// The value NaN (not a number) as <see cref="float"/>.
        /// </summary>
        public static readonly object SingleNaN = float.NaN;


        /// <summary>
        /// The value 0 as <see cref="double"/>.
        /// </summary>
        public static readonly object DoubleZero = 0.0d;


        /// <summary>
        /// The value 1 as <see cref="double"/>.
        /// </summary>
        public static readonly object DoubleOne = 1.0d;


        /// <summary>
        /// The value +infinity as <see cref="double"/>.
        /// </summary>
        public static readonly object DoublePositiveInfinity = double.PositiveInfinity;


        /// <summary>
        /// The value -infinity as <see cref="double"/>.
        /// </summary>
        public static readonly object DoubleNegativeInfinity = double.NegativeInfinity;


        /// <summary>
        /// The value NaN (not a number) as <see cref="double"/>.
        /// </summary>
        public static readonly object DoubleNaN = double.NaN;


        /// <summary>
        /// The color "white".
        /// </summary>
        public static readonly object ColorWhite = Colors.White;


        /// <summary>
        /// The color "black".
        /// </summary>
        public static readonly object ColorBlack = Colors.Black;


        /// <summary>
        /// A point where X and Y are 0.
        /// </summary>
        public static readonly object PointZero = new Point(0, 0);


        /// <summary>
        /// A point where X and Y are NaN.
        /// </summary>
        public static readonly object PointNaN = new Point(double.NaN, double.NaN);


        /// <summary>
        /// A time span of 0.
        /// </summary>
        public static readonly object TimeSpanZero = new TimeSpan();


        /// <summary>
        /// Returns a boxed value.
        /// </summary>
        /// <param name="value">The value to be boxed.</param>
        /// <returns>The boxed value.</returns>
        public static object Get(bool value)
        {
            return value ? BooleanTrue : BooleanFalse;
        }
    }
}
