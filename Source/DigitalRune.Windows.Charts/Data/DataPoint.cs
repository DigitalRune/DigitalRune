// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines a data point in a chart.
    /// </summary>
    /// <remarks>
    /// For example: <see cref="DataPoint"/>s are used to represent the points in a scatter plot or
    /// the bars in a bar chart.
    /// </remarks>
    public struct DataPoint : IFormattable, IEquatable<DataPoint>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private double _x;
        private double _y;
        private object _dataContext;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the point (X, Y).
        /// </summary>
        /// <value>The point (X, Y).</value>
        public Point Point
        {
            get { return new Point(X, Y); }
        }


        /// <summary>
        /// Gets or sets the x value.
        /// </summary>
        /// <value>The x value.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }


        /// <summary>
        /// Gets or sets the y value.
        /// </summary>
        /// <value>The y value.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }


        /// <summary>
        /// Gets or sets the data context.
        /// </summary>
        /// <value>The data context, or <see langword="null"/>.</value>
        /// <remarks>
        /// <para></para>
        /// <para>
        /// If the value is not <see langword="null"/>, it will be set as the
        /// <see cref="FrameworkElement.DataContext"/> of the visual element.
        /// </para>
        /// <para>
        /// If the value is <see langword="null"/>, the <see cref="DataPoint"/> itself will be set
        /// as the <see cref="FrameworkElement.DataContext"/> of the visual element.
        /// </para>
        /// </remarks>
        public object DataContext
        {
            get { return _dataContext; }
            set { _dataContext = value; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPoint"/> struct.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="DataPoint"/> struct from x, y.
        /// </summary>
        /// <param name="x">The x value.</param>
        /// <param name="y">The y value.</param>
        /// <param name="dataContext">The data context. Can be <see langword="null"/>.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public DataPoint(double x, double y, object dataContext)
        {
            _x = x;
            _y = y;
            _dataContext = dataContext;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataPoint"/> struct from a
        /// <see cref="Point"/>.
        /// </summary>
        /// <param name="point">The point (X, Y).</param>
        /// <param name="dataContext">The data context. Can be <see langword="null"/>.</param>
        public DataPoint(Point point, object dataContext)
        {
            _x = point.X;
            _y = point.Y;
            _dataContext = dataContext;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Compares two <see cref="DataPoint"/> structures for equality.
        /// </summary>
        /// <param name="point1">The first <see cref="DataPoint"/> structure to compare.</param>
        /// <param name="point2">The second <see cref="DataPoint"/> structure to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="X"/>, <see cref="Y"/>, and
        /// <see cref="DataContext"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DataPoint point1, DataPoint point2)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return point1.X == point2.X
                   && point1.Y == point2.Y
                   && point1.DataContext == point2.DataContext;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


        /// <summary>
        /// Compares two <see cref="DataPoint"/> structures for inequality.
        /// </summary>
        /// <param name="point1">The first <see cref="DataPoint"/> structure to compare.</param>
        /// <param name="point2">The second <see cref="DataPoint"/> structure to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="X"/>, <see cref="Y"/>, or
        /// <see cref="DataContext"/> are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(DataPoint point1, DataPoint point2)
        {
            return !(point1 == point2);
        }


        /// <summary>
        /// Determines whether the specified object is a <see cref="DataPoint"/> and whether it
        /// contains the same values as this <see cref="DataPoint"/>.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="DataPoint"/> and
        /// contains the same values as this <see cref="DataPoint"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is DataPoint && Equals((DataPoint)obj);
        }

        /// <summary>
        /// Compares two <see cref="DataPoint"/> structures for equality.
        /// </summary>
        /// <param name="other">
        /// The <see cref="DataPoint"/> structure to compare to this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="X"/>, <see cref="Y"/>, and
        /// <see cref="DataContext"/> are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(DataPoint other)
        {
            return X.Equals(other.X)
                   && Y.Equals(other.Y)
                   && Equals(DataContext, other.DataContext);
        }


        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ (DataContext != null ? DataContext.GetHashCode() : 0);
                return hashCode;
            }
        }


        /// <summary>
        /// Returns string representation of this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> representation of this <see cref="DataPoint"/> instance.
        /// </returns>
        public override string ToString()
        {
            return ConvertToString("G5", null);
        }


        /// <summary>
        /// Returns string representation of this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <returns>
        /// A <see cref="String"/> representation of this <see cref="DataPoint"/> instance.
        /// </returns>
        public string ToString(IFormatProvider provider)
        {
            return ConvertToString("G5", provider);
        }


        /// <summary>
        /// Returns string representation of this <see cref="DataPoint"/> instance.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="provider">Culture-specific formatting information.</param>
        /// <returns>
        /// A <see cref="String"/> representation of this <see cref="DataPoint"/> instance.
        /// </returns>
        string IFormattable.ToString(string format, IFormatProvider provider)
        {
            return ConvertToString(format, provider);
        }


        private string ConvertToString(string format, IFormatProvider provider)
        {
            string formatString;
            if (string.IsNullOrEmpty(format))
            {
                formatString = "{0};{1};{2}";
            }
            else
            {
                // Example: "{0:G5};{1:G5};{2}"
                formatString = string.Format(CultureInfo.InvariantCulture, "{{0:{0}}};{{1:{0}}};{{2}}", format);
            }

            string dataContextString = (DataContext != null) ? DataContext.ToString() : "null";
            return string.Format(provider, formatString, X, Y, dataContextString);
        }
        #endregion
    }
}
