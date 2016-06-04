// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// A logarithmic scale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="AxisScale.Min"/> value of a <see cref="LogScale"/> must be greater than 0.
    /// </para>
    /// </remarks>
    public class LogScale : AxisScale
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the data value distance between major ticks in decades
        /// (10<sup><i>x</i></sup>).
        /// </summary>
        /// <value>
        /// The data value distance between major ticks. If this is set to <see cref="Double.NaN"/>
        /// (default) this distance will be calculated automatically.
        /// </value>
        public double MajorTickStep
        {
            get { return _majorTickStep; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_majorTickStep == value)
                    return;

                _majorTickStep = value;
                OnPropertyChanged("MajorTickStep");
            }
        }
        private double _majorTickStep = Double.NaN;


        /// <summary>
        /// Gets or sets the data value of a major tick. All other major ticks will be placed
        /// relative to this tick.
        /// </summary>
        /// <value>
        /// The data value of a major tick that serves as an anchor for tick placement. When
        /// <see cref="Double.NaN"/> is set (default) the tick placement is computed automatically.
        /// </value>
        public double MajorTickAnchor
        {
            get { return _majorTickAnchor; }
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_majorTickAnchor == value)
                    return;

                _majorTickAnchor = value;
                OnPropertyChanged("MajorTickAnchor");
            }
        }
        private double _majorTickAnchor = Double.NaN;


        /// <summary>
        /// Gets or sets the number of minor ticks that are drawn between major ticks.
        /// </summary>
        /// <value>
        /// The number of minor ticks between major ticks. The number of minor ticks is computed
        /// automatically if this property is set to a negative value. The default value is -1;
        /// </value>
        public int NumberOfMinorTicks
        {
            get { return _numberOfMinorTicks; }
            set
            {
                if (_numberOfMinorTicks == value)
                    return;

                _numberOfMinorTicks = value;
                OnPropertyChanged("NumberOfMinorTicks");
            }
        }
        private int _numberOfMinorTicks = -1;


        /// <summary>
        /// Specifies the format used for drawing tick labels.
        /// </summary>
        /// <value>
        /// The number format used for drawing the tick labels. The default value is <c>"g5"</c>.
        /// </value>
        /// <remarks>
        /// See <see cref="StringBuilder.AppendFormat(string, object[])"/> for a description of this
        /// format string.
        /// </remarks>
        public string FormatString
        {
            get { return _formatString; }
            set
            {
                if (_formatString == value)
                    return;

                _formatString = value;
                OnPropertyChanged("FormatString");
            }
        }
        private string _formatString = "g5";
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="LogScale"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="LogScale"/> class.
        /// </summary>
        public LogScale()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LogScale"/> class with the given range.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public LogScale(double min, double max)
            : base(min, max)
        {
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

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
        public override void ComputeTicks(double axisLength,
                                          double minDistance,
                                          out double[] labelValues,
                                          out double[] majorTicks,
                                          out double[] minorTicks)
        {
            if (Numeric.IsZero(axisLength))
            {
                // The size of the axis is 0.
                // Do nothing.
                labelValues = majorTicks = minorTicks = new double[0];
                return;
            }

            // Check Min and Max.
            if (Numeric.IsNaN(Min) || Numeric.IsNaN(Max) || Min > Max)
                throw new ChartException("Properties Min and Max of the scale have invalid values.");

            if (Numeric.AreEqual(Min, Max))
                throw new ChartException("Properties Min and Max of the scale have invalid values (nearly identical).");

            if (Min <= 0)
                throw new ChartException("Min must be greater than 0.");

            // majorTickStep specifies the number of decades(!) between major ticks.
            double majorTickStep = DetermineMajorTickStep(axisLength, minDistance);

            majorTicks = ComputeMajorTicks(majorTickStep);
            minorTicks = ComputeMinorTicks(majorTickStep, majorTicks);
            labelValues = majorTicks;
        }


        /// <summary>
        /// Determines the data values of the major ticks.
        /// </summary>
        /// <param name="majorTickStep">The major tick step (in decades).</param>
        /// <returns>The data values at which major ticks should be drawn.</returns>
        private double[] ComputeMajorTicks(double majorTickStep)
        {
            var majorTicks = new List<double>();

            // Now determine first tick value (given as a decade!).
            double first = 0.0f;

            if (Numeric.IsNaN(MajorTickAnchor))
            {
                // The user hasn't specified an anchor value for major ticks.
                if (Min > 0.0)
                {
                    double n = Math.Floor(Math.Log10(Min) / majorTickStep) + 1.0f;
                    first = n * majorTickStep;
                }

                // The above calculation misses the first tick, if it is exactly at Min.
                if (first - majorTickStep >= Math.Log10(Min))
                    first -= majorTickStep;
            }
            else
            {
                // The user has specified a value where a tick should be placed.
                first = Math.Log10(MajorTickAnchor);

                while (first < Math.Log10(Min))
                    first += majorTickStep;

                while (first > Math.Log10(Min) + majorTickStep)
                    first -= majorTickStep;
            }

            double e = first;
            while (e <= Math.Log10(Max))
            {
                double value = Math.Pow(10.0, e);
                majorTicks.Add(value);
                e += majorTickStep;
            }

            return majorTicks.ToArray();
        }


        /// <summary>
        /// Determines the data values of the minor ticks.
        /// </summary>
        /// <param name="majorTickStep">The major tick step (in decades).</param>
        /// <param name="majorTicks">The data values at which major ticks should be drawn.</param>
        /// <returns>The data values at which minor ticks should be drawn.</returns>
        private double[] ComputeMinorTicks(double majorTickStep, double[] majorTicks)
        {
            var minorTicks = new List<double>();

            // Retrieve the spacing of the major ticks. Remember these are decades.
            int numberOfMinorTicks = DetermineNumberOfMinorTicks(majorTickStep);

            if (majorTickStep > 1.0f)
            {
                // The major tick step is larger than a decade.
                if (majorTicks.Length > 0)
                {
                    // Add the minor ticks before the first major tick.
                    double value = majorTicks[0];
                    while (value > Min)
                    {
                        value = value / 10.0f;
                        minorTicks.Add(value);
                    }
                    // now go on for all other Major ticks
                    for (int i = 0; i < majorTicks.Length; ++i)
                    {
                        value = majorTicks[i];
                        for (int j = 0; j < numberOfMinorTicks; ++j)
                        {
                            value = value * 10.0F;
                            if (value < Max)
                                minorTicks.Add(value);
                        }
                    }
                }
            }
            else
            {
                // The major tick step is one decade.
                double[] m = { 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f };

                if (majorTicks.Length > 0)
                {
                    // Add minor ticks preceding the first major tick.
                    // Get the major tick value that lies before Min.
                    double precedingMajorTickValue = majorTicks[0] / 10.0f;
                    for (int i = 0; i < m.Length; i++)
                    {
                        double value = precedingMajorTickValue * m[i];
                        if (value >= Min)
                            minorTicks.Add(value);
                    }

                    // Add minor ticks between remaining major ticks.
                    for (int i = 0; i < majorTicks.Length; ++i)
                    {
                        double majorTickValue = majorTicks[i];
                        for (int j = 0; j < m.Length; ++j)
                        {
                            double value = majorTickValue * m[j];
                            if (value <= Max)
                                minorTicks.Add(value);
                        }
                    }
                }
                else
                {
                    // No major ticks:
                    // Determine the first major tick value that lies before Min.
                    double e = Math.Floor(Math.Log10(Min));
                    double majorTickValue = Math.Pow(10.0, e);

                    // From here on add minor ticks.
                    for (int i = 0; i < m.Length; i++)
                    {
                        double value = majorTickValue * m[i];
                        if (value >= Min && value <= Max)
                            minorTicks.Add(value);
                    }
                }
            }

            return minorTicks.ToArray();
        }


        /// <summary>
        /// Determines the distance between major ticks in decades.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <returns>The major tick step in decades (10<sup><i>x</i></sup>).</returns>
        /// <exception cref="ChartException">
        /// <see cref="MajorTickStep"/> is zero or negative.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MajorTickStep")]
        private double DetermineMajorTickStep(double axisLength, double minDistance)
        {
            if (!Numeric.IsNaN(MajorTickStep))
            {
                if (MajorTickStep <= 0.0f)
                    throw new ChartException("MajorTickStep cannot be negative or 0.");

                return MajorTickStep;
            }

            Debug.Assert(Max > Min);
            double range = Math.Floor(Math.Log10(Max)) - Math.Floor(Math.Log10(Min)) + 1.0;
            Debug.Assert(range >= 1.0);

            // For now, a simple logic:
            // Start with a major tick every order of magnitude, and increment if in order not to
            // have more than 10 ticks in the plot.
            double tickStep = 1.0F;
            int maxNumberOfTicks = (int)(axisLength / minDistance);
            int n = (int)(range / tickStep);
            while (n > maxNumberOfTicks)
            {
                tickStep++;
                n = (int)(range / tickStep);
            }

            return tickStep;
        }


        /// <summary>
        /// Determines the number of minor ticks between two large ticks.
        /// </summary>
        /// <param name="majorTickStep">The distance between two minor ticks.</param>
        /// <returns>The number of minor ticks between large ticks.</returns>
        /// <exception cref="ChartException">
        /// <paramref name="majorTickStep"/> is invalid.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "majorTickStep")]
        private int DetermineNumberOfMinorTicks(double majorTickStep)
        {
            // If the major tick distance is more than one decade, the minor ticks are every decade,
            // I don't let the user set it.
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (NumberOfMinorTicks >= 0 && majorTickStep == 1.0f)
                return NumberOfMinorTicks;

            // If we are plotting every decade, we have to put the log ticks. As a start, I put
            // every minor tick (2, 3, 4, 5, 6, 7, 8, 9)
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (majorTickStep == 1.0f)
                return 8;

            // Easy, put a tick every missed decade
            if (majorTickStep > 1.0f)
                return (int)majorTickStep - 1;

            throw new ChartException("Invalid value for majorTickStep.");
        }


        /// <summary>
        /// Gets the position on the axis for a specified data value.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <param name="startPosition">The start position (origin) of the axis.</param>
        /// <param name="endPosition">The end position of the axis.</param>
        /// <returns>The position of the data value on the axis.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative.
        /// </exception>
        /// <exception cref="ChartException">
        /// Invalid scale. <see cref="AxisScale.Min"/> is greater than or equal to
        /// <see cref="AxisScale.Max"/>.
        /// </exception>
        public override double GetPosition(double value, double startPosition, double endPosition)
        {
            if (Min >= Max)
                throw new ChartException("Scale.Min must be less than Scale.Max.");

            if (value < 0.0f)
                throw new ArgumentOutOfRangeException("value", "Cannot have negative values for data using a logarithmic scale.");

            if (Reversed)
                ChartHelper.Swap(ref startPosition, ref endPosition);

            double logValue = Math.Log10(value);
            double logMin = Math.Log10(Min);
            double logMax = Math.Log10(Max);
            return (float)(startPosition + (logValue - logMin) / (logMax - logMin) * (endPosition - startPosition));
        }


        /// <summary>
        /// Returns the data value of a position (given in device-independent pixels) on the axis.
        /// </summary>
        /// <param name="position">The position on the axis.</param>
        /// <param name="startPosition">The start position (origin) of the axis.</param>
        /// <param name="endPosition">The end position of the axis.</param>
        /// <returns>The data value of the given position.</returns>
        /// <exception cref="ChartException">
        /// Invalid scale. <see cref="AxisScale.Min"/> is greater than or equal to
        /// <see cref="AxisScale.Max"/>.
        /// </exception>
        /// <remarks>
        /// An <see cref="Axis"/> is one-dimensional, so one component of a 2d position is
        /// neglected. If this scale belongs to an x-axis, then the positions are x-positions;
        /// otherwise the positions are y-positions.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "AxisScale")]
        public override double GetValue(double position, double startPosition, double endPosition)
        {
            if (Min >= Max)
                throw new ChartException("AxisScale.Min must be less than AxisScale.Max.");

            if (Reversed)
                ChartHelper.Swap(ref startPosition, ref endPosition);

            double v = (position - startPosition) / (endPosition - startPosition);
            double value = Min * Math.Pow(Max / Min, v);
            return value;
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
        public override string GetText(double value, CultureInfo cultureInfo)
        {
            return value.ToString(FormatString, cultureInfo);
        }


        /// <summary>
        /// Pans (scrolls) the scale.
        /// </summary>
        /// <param name="relativeTranslation">
        /// The translation relative to <see cref="AxisScale.Min"/> and <see cref="AxisScale.Max"/>
        /// of the scale. A value of 0 indicates no translation. A value of 1 indicates a
        /// translation equal to the distance between <see cref="AxisScale.Min"/> and
        /// <see cref="AxisScale.Max"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// The method does nothing if the pan operation would result in an invalid scale.
        /// </para>
        /// </remarks>
        public override void Pan(double relativeTranslation)
        {
            double min = Min;
            double max = Max;

            double logMin = Math.Log10(min);
            double logMax = Math.Log10(max);
            double rangeInDecades = logMax - logMin;
            double translationInDecades = relativeTranslation * rangeInDecades;
            if (Reversed)
                translationInDecades = -translationInDecades;

            logMin += translationInDecades;
            logMax += translationInDecades;

            // Stop panning left at 1e-100.
            if (logMin < -100)
                return;

            // Stop panning right at 1e100.
            if (logMax > 100)
                return;

            min = Math.Pow(10, logMin);
            max = Math.Pow(10, logMax);

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
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="zoomFactor"/> is out of range. The zoom factor must be a value between
        /// - ∞ and 1 (limits not included).
        /// </exception>
        public override void Zoom(double anchorValue, double zoomFactor)
        {
            if (zoomFactor <= -1 || 1 <= zoomFactor)
                throw new ArgumentOutOfRangeException("zoomFactor", "The zoom factor must be a value between -1 and 1. (-1 and 1 not included.)");

            double min = Min;
            double max = Max;

            if (Numeric.IsZero(zoomFactor))
                return;

            // Change scale limits depending on the current mouse position.
            double logAnchor = Math.Log10(anchorValue);
            double logMin = Math.Log10(min);
            double logMax = Math.Log10(max);

            logMin += zoomFactor * (logAnchor - logMin);
            logMax -= zoomFactor * (logMax - logAnchor);

            // Stop zooming out after 100 decades.
            if (logMax - logMin > 100)
                return;

            // Stop zooming in at 0.5 decades.
            if (logMax - logMin < 0.5)
                return;

            min = Math.Pow(10, logMin);
            max = Math.Pow(10, logMax);

            // Check for numerical problems before setting range.
            if (Numeric.IsLess(min, max))
            {
                Range = new DoubleRange(min, max);
            }
        }
        #endregion
    }
}
