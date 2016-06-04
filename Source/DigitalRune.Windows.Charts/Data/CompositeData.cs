// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Combines the x and y item from a <see cref="CompositeDataSource"/> into a single object.
    /// (Stored as the <see cref="DataPoint.DataContext"/> in each data point.)
    /// </summary>
    public class CompositeData
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the original item that contains the x value.
        /// </summary>
        /// <value>The original item that contains the x value.</value>
        public object XValue { get; set; }


        /// <summary>
        /// Gets or sets the original item that contains the y value.
        /// </summary>
        /// <value>The original item that contains the y value.</value>
        public object YValue { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeData"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeData"/> class.
        /// </summary>
        public CompositeData()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeData"/> class with the x and y
        /// item.
        /// </summary>
        /// <param name="x">The original item that contains the x value.</param>
        /// <param name="y">The original item that contains the y value.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        public CompositeData(object x, object y)
        {
            XValue = x;
            YValue = y;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current 
        /// <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">
        /// The <see cref="Object"/> to compare with the current <see cref="Object"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
        /// <see cref="Object"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// The <paramref name="obj"/> parameter is <see langword="null"/>.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            CompositeData other = (CompositeData)obj;
            return XValue == other.XValue && YValue == other.YValue;
        }


        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ((XValue != null) ? XValue.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ((YValue != null) ? YValue.GetHashCode() : 0);
                return hashCode;
            }
        }


        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current <see cref="Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "(X={0} Y={1})", XValue, YValue);
        }
        #endregion
    }
}