// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Associates a color with a data value.
    /// </summary>
    public struct PaletteEntry : IEquatable<PaletteEntry>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the data value.
        /// </summary>
        /// <value>The data value.</value>
        public double Value
        {
            get { return _value; }
            set { _value = value; }
        }
        private double _value;


        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
        private Color _color;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PaletteEntry"/> class.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="color">The color.</param>
        public PaletteEntry(double value, Color color)
        {
            _value = value;
            _color = color;
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
        public override bool Equals(object obj)
        {
            return obj is PaletteEntry && this == (PaletteEntry)obj;
        }


        /// <summary>
        /// Determines whether the specified <see cref="PaletteEntry"/> is equal to the current
        /// <see cref="PaletteEntry"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="PaletteEntry"/> to compare with the current <see cref="PaletteEntry"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="PaletteEntry"/> is equal to the
        /// current <see cref="PaletteEntry"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(PaletteEntry other)
        {
            return this == other;
        }


        /// <summary>
        /// Compares two <see cref="PaletteEntry"/> objects to determine whether they are the same.
        /// </summary>
        /// <param name="entry1">The first <see cref="PaletteEntry"/>.</param>
        /// <param name="entry2">The second <see cref="PaletteEntry"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="entry1"/> and <paramref name="entry2"/>
        /// are the same; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(PaletteEntry entry1, PaletteEntry entry2)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return entry1.Value == entry2.Value
                   && entry1.Color == entry2.Color;
        }


        /// <summary>
        /// Compares two <see cref="PaletteEntry"/> objects to determine whether they are different.
        /// </summary>
        /// <param name="entry1">The first <see cref="PaletteEntry"/>.</param>
        /// <param name="entry2">The second <see cref="PaletteEntry"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="entry1"/> and <paramref name="entry2"/>
        /// are different; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(PaletteEntry entry1, PaletteEntry entry2)
        {
            return !(entry1 == entry2);

        }


        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Object"/>.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
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
            return String.Format(CultureInfo.InvariantCulture, "PaletteEntry{{Value={0}, Color={1}}}", Value, Color);
        }
        #endregion
    }
}
