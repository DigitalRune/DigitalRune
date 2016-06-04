// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Determines how data values are distributed and presented along an axis. (The base class for
    /// all axis scales.)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A scale defines the start and end data value of an <see cref="Axis"/>.
    /// </para>
    /// <para>
    /// Subclasses have to implement the method <see cref="ComputeTicks"/> which computes the
    /// position of major/minor ticks and tick labels.
    /// </para>
    /// </remarks>
    public abstract class AxisScale : INotifyPropertyChanged
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the range of the axis.
        /// </summary>
        /// <value>
        /// The range of the axis. The default value is [0, 1].
        /// </value>
        /// <remarks>
        /// <see cref="DoubleRange._min"/> must be less than <see cref="DoubleRange._max"/>. To
        /// reverse a scale on an <see cref="Axis"/>, set the property <see cref="Reversed"/>
        /// property.
        /// </remarks>
        /// <exception cref="ArgumentException">
        /// The specified range is invalid.
        /// </exception>
        public DoubleRange Range
        {
            get { return _range; }
            set
            {
                if (_range == value)
                    return;

                // Test with ! and < instead of >= to catch NaN values.
                if (!(value.Min < value.Max))
                    throw new ArgumentException("Invalid range.");

                _range = value;
                OnPropertyChanged("Range");
                OnPropertyChanged("Min");
                OnPropertyChanged("Max");
            }
        }
        private DoubleRange _range;


        /// <summary>
        /// Gets the minimum data value of the axis.
        /// </summary>
        /// <value>
        /// A <see cref="Double"/> value representing the start value of an axis. The default value
        /// is 0.
        /// </value>
        public double Min
        {
            get { return Range.Min; }
        }


        /// <summary>
        /// Gets the maximum data value of the axis.
        /// </summary>
        /// <value>
        /// A <see cref="Double"/> value representing the end value of an axis. The default value is
        /// 1.
        /// </value>
        public double Max
        {
            get { return Range.Max; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this scale is reversed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if axis is reversed; otherwise, <see langword="false"/>. The
        /// default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// Usually, a scale is used on an <see cref="Axis"/>. The start position (origin) of the
        /// axis corresponds to the <see cref="Min"/> value and the end position of the axis
        /// corresponds to the <see cref="Max"/> value. If <see cref="Reversed"/> is set to
        /// <see langword="true"/>, then <see cref="Min"/> and <see cref="Max"/> are reversed and
        /// <see cref="Max"/> is at the start position (origin) of the <see cref="Axis"/>.
        /// </remarks>
        public bool Reversed
        {
            get { return _reversed; }
            set
            {
                if (_reversed == value)
                    return;

                _reversed = value;
                OnPropertyChanged("Reversed");
            }
        }
        private bool _reversed;


        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="AxisScale"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="AxisScale"/> class with the range [0, 1].
        /// </summary>
        protected AxisScale()
        {
            _range = new DoubleRange(0, 1);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AxisScale"/> class with the given range.
        /// </summary>
        /// <param name="range">The range of the axis.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="range"/> is invalid.
        /// </exception>
        protected AxisScale(DoubleRange range)
        {
            if (!(range.Min < range.Max))
                throw new ArgumentException("Invalid range.", "range");

            _range = range;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AxisScale"/> class with the given range.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <exception cref="ArgumentException">
        /// [<paramref name="min"/>, <paramref name="max"/>] is not a valid range.
        /// </exception>
        protected AxisScale(double min, double max)
        {
            if (!(min < max))
                throw new ArgumentException("min must be less than max.");

            _range = new DoubleRange(min, max);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> in
        /// a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method
        /// so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Sets the <see cref="Min"/> and <see cref="Max"/> properties to be just large enough to
        /// encompass the specified scale.
        /// </summary>
        /// <param name="scale">The scale.</param>
        public void Add(AxisScale scale)
        {
            if (scale == null)
                return;

            double min = Range.Min;
            double max = Range.Max;

            if (scale.Min < min)
                min = scale.Min;

            if (scale.Max > max)
                max = scale.Max;

            Range = new DoubleRange(min, max);
        }


        /// <summary>
        /// Clamps the specified value to the range [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The clipped value.</returns>
        public double Clamp(double value)
        {
            return Range.Clamp(value);
        }


        /// <summary>
        /// Computes the major and minor tick values.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <param name="labelValues">The data values at which labels should be drawn.</param>
        /// <param name="majorTicks">The data values at which major ticks should be drawn.</param>
        /// <param name="minorTicks">The data values at which minor ticks should be drawn.</param>
        /// <exception cref="ChartException">
        /// <see cref="AxisScale.Min"/> and <see cref="AxisScale.Max"/> have invalid values.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public abstract void ComputeTicks(double axisLength,
                                          double minDistance,
                                          out double[] labelValues,
                                          out double[] majorTicks,
                                          out double[] minorTicks);


        /// <summary>
        /// Gets the position on the axis for a specified data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="startPosition">The start position (origin) of the axis.</param>
        /// <param name="endPosition">The end position of the axis.</param>
        /// <returns>The position of the data value on the axis.</returns>
        public virtual double GetPosition(double value, double startPosition, double endPosition)
        {
            if (_reversed)
                ChartHelper.Swap(ref startPosition, ref endPosition);

            return startPosition + (value - Min) / (Max - Min) * (endPosition - startPosition);
        }


        /// <summary>
        /// Gets the label text for a specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The label text.</returns>
        /// <remarks>
        /// Per default, the label text is equal to the string representation of the
        /// <paramref name="value"/>. This behavior can be changed in derived classes.
        /// </remarks>
        public string GetText(double value)
        {
            return GetText(value, CultureInfo.CurrentCulture);
        }


        /// <summary>
        /// Gets the label text for a specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The label text.</returns>
        /// <remarks>
        /// Per default, the label text is equal to the string representation of the
        /// <paramref name="value"/>. This behavior can be changed in derived classes.
        /// </remarks>
        public virtual string GetText(double value, CultureInfo cultureInfo)
        {
            return value.ToString(cultureInfo);
        }


        /// <summary>
        /// Returns the data value of a position (given in device-independent pixels) on the axis.
        /// </summary>
        /// <param name="position">The position on the axis.</param>
        /// <param name="startPosition">The start position (origin) of the axis.</param>
        /// <param name="endPosition">The end position of the axis.</param>
        /// <returns>The data value of the given position.</returns>
        /// <remarks>
        /// An <see cref="Axis"/> is one-dimensional, so one component of a 2d position is
        /// neglected. If this scale belongs to an x-axis, then the positions are x-positions;
        /// otherwise the positions are y-positions.
        /// </remarks>
        public virtual double GetValue(double position, double startPosition, double endPosition)
        {
            if (_reversed)
                ChartHelper.Swap(ref startPosition, ref endPosition);

            return Min + (position - startPosition) / (endPosition - startPosition) * (Max - Min);
        }


        /// <summary>
        /// Determines whether a data value is inside the range [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        /// <param name="value">The data value to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <paramref name="value"/> is inside the limits; otherwise
        /// <see langword="false"/>.
        /// </returns>
        public bool Contains(double value)
        {
            return Range.Contains(value);
        }


        /// <summary>
        /// Pans (scrolls) the scale.
        /// </summary>
        /// <param name="relativeTranslation">
        /// The translation relative to <see cref="Min"/> and <see cref="Max"/> of the scale. A
        /// value of 0 indicates no translation. A value of 1 indicates a translation equal to the
        /// distance between <see cref="Min"/> and <see cref="Max"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// The method does nothing if the pan operation would result in an invalid scale.
        /// </para>
        /// <para>
        /// <strong>Note to Inheritors:</strong> The base class <see cref="AxisScale"/> provides a
        /// default implementation for <see cref="Pan"/>. However, the default implementation only
        /// works for linear scales. Scales with a non-linear distribution of data values need to
        /// override this method and provide a custom implementation.
        /// </para>
        /// </remarks>
        public virtual void Pan(double relativeTranslation)
        {
            double min = Min;
            double max = Max;

            double translation = (max - min) * relativeTranslation;
            if (_reversed)
                translation = -translation;

            min += translation;
            max += translation;

            // Check for numerical problems before setting range.
            if (Numeric.IsLess(min, max))
            {
                Range = new DoubleRange(min, max);
            }
        }


        /// <summary>
        /// Zooms the scale by a given factor.
        /// </summary>
        /// <param name="anchorValue">
        /// The anchor value in device-independent pixels. When the scale is changed this data value
        /// will remain the same.
        /// </param>
        /// <param name="zoomFactor">
        /// The relative zoom factor in the range ]-∞, 1[. For example: A value of -0.5 increases
        /// the range of the scale by 50% ("zoom out"). A value of 0.1 reduces the range of the
        /// scale by 10% ("zoom in"). The scale does not change if the zoom factor is 0.
        /// </param>
        /// <remarks>
        /// <para>
        /// The method does nothing if the zoom operation would result in an invalid scale.
        /// </para>
        /// <para>
        /// <strong>Note to Inheritors:</strong> The base class <see cref="AxisScale"/> provides a
        /// default implementation for <see cref="Zoom"/>. However, the default implementation only
        /// works for linear scales. Scales with a non-linear distribution of data values need to
        /// override this method and provide a custom implementation.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="zoomFactor"/> is out of range. The zoom factor must be a value between
        /// - ∞ and 1 (limits not included).
        /// </exception>
        public virtual void Zoom(double anchorValue, double zoomFactor)
        {
            if (!Numeric.IsFinite(zoomFactor) || 1 <= zoomFactor)
                throw new ArgumentOutOfRangeException("zoomFactor", "The zoom factor must be a value in the range ]-∞, 1[.");

            double min = Range.Min;
            double max = Range.Max;

            if (Numeric.IsZero(zoomFactor))
                return;

            // Change scale limits depending on the current mouse position.
            min += zoomFactor * (anchorValue - min);
            max -= zoomFactor * (max - anchorValue);

            // Check for numerical problems before setting range.
            if (Numeric.IsLess(min, max))
            {
                Range = new DoubleRange(min, max);
            }
        }
        #endregion
    }
}
