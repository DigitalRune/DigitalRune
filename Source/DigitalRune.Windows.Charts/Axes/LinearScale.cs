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
    /// A linear scale.
    /// </summary>
    public class LinearScale : AxisScale
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        /// <summary>
        /// If <see cref="MajorTickStep"/> isn't specified, then a suitable value is calculated
        /// automatically. The value will be of the form <i>m</i> * 10^<i>e</i> for some <i>m</i>
        /// in this set.
        /// </summary>
        private static readonly double[] Mantissas = { 1.0, 2.0, 5.0 };


        /// <summary>
        /// If <see cref="NumberOfMinorTicks"/> isn't specified then a value of this list is used.
        /// </summary>
        /// <remarks>
        /// The array elements correspond to the elements of <see cref="Mantissas"/>. If
        /// <see cref="MajorTickStep"/> is specified by the user, then the user has to specify the
        /// <see cref="NumberOfMinorTicks"/> too and this list is not used.
        /// </remarks>
        private static readonly int[] MinorTickCounts = { 1, 1, 4 };
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------    
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets a value indicating whether this linear scale is an ordinal scale.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is an ordinal scale; otherwise,
        /// <see langword="false"/>. The default values is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// For ordinal scales only integer values are drawn.
        /// </remarks>
        public bool IsOrdinal
        {
            get { return _isOrdinal; }
            set
            {
                if (_isOrdinal == value)
                    return;

                _isOrdinal = value;
                OnPropertyChanged("IsOrdinal");
            }
        }
        private bool _isOrdinal;


        /// <summary>
        /// Gets or sets the data value distance between major ticks.
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
        /// Gets or sets the number format used for drawing tick labels.
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
        /// Initializes a new instance of the <see cref="LinearScale"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="LinearScale"/> class.
        /// </summary>
        public LinearScale()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="LinearScale"/> class with the given range.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public LinearScale(double min, double max)
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
                // The size of the axis is 0. Do nothing.
                labelValues = majorTicks = minorTicks = new double[0];
                return;
            }

            // Check Min and Max.
            if (Numeric.IsNaN(Min) || Numeric.IsNaN(Max) || Min > Max)
                throw new ChartException("Properties Min and Max of the scale have invalid values.");

            if (Numeric.AreEqual(Min, Max))
                throw new ChartException("Properties Min and Max of the scale have invalid values (nearly identical).");

            var internalMajorTicks = new List<double>();
            var internalMinorTicks = new List<double>();

            ComputeTicks_FirstPass(axisLength, minDistance, internalMajorTicks, internalMinorTicks);
            ComputeTicks_SecondPass(axisLength, minDistance, internalMajorTicks, internalMinorTicks);

            if (IsOrdinal)
            {
                // Remove all ticks that are not on integer values.
                // TODO: This check should be integrated in the creation of tick values.
                for (int i = 0; i < internalMajorTicks.Count; i++)
                {
                    if (!Numeric.AreEqual(internalMajorTicks[i], (int)Math.Round(internalMajorTicks[i])))
                    {
                        internalMajorTicks.RemoveAt(i);
                        i--;
                    }
                }

                for (int i = 0; i < internalMinorTicks.Count; i++)
                {
                    if (!Numeric.AreEqual(internalMinorTicks[i], (int)Math.Round(internalMinorTicks[i])))
                    {
                        internalMinorTicks.RemoveAt(i);
                        i--;
                    }
                }
            }

            majorTicks = internalMajorTicks.ToArray();
            minorTicks = internalMinorTicks.ToArray();
            labelValues = majorTicks;
        }


        /// <summary>
        /// Determines the data values of the major ticks.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <param name="majorTicks">The list of major ticks.</param>
        /// <param name="minorTicks">The list of minor ticks.</param>
        /// <remarks>
        /// When the drawing extent of the axis is small, some of the positions that were generated
        /// in this pass may be converted to minor tick positions and returned as well. If the
        /// <see cref="MajorTickStep"/> isn't set then this is calculated automatically and depends
        /// on the physical extent of the axis.
        /// </remarks>
        /// <exception cref="ChartException">
        /// Internal error in <see cref="LinearScale"/>: Tick distance is negative.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "LinearScale")]
        private void ComputeTicks_FirstPass(double axisLength, double minDistance, List<double> majorTicks, List<double> minorTicks)
        {
            // Determine distance between large ticks.
            bool shouldCullMiddle;
            double majorTickStep = DetermineMajorTickStep(axisLength, minDistance, out shouldCullMiddle);

            if (majorTickStep < 0.0)
                throw new ChartException("Internal error in LinearScale: Tick distance is negative.");

            // Determine starting position.
            double first;
            if (!Numeric.IsNaN(MajorTickAnchor))
            {
                first = MajorTickAnchor + Math.Ceiling((Min - MajorTickAnchor) / majorTickStep) * majorTickStep;
            }
            else
            {
                if (Min > 0.0)
                {
                    double n = Math.Floor(Min / majorTickStep) + 1.0f;
                    first = n * majorTickStep;
                }
                else
                {
                    double n = Math.Floor(-Min / majorTickStep) - 1.0f;
                    first = -n * majorTickStep;
                }

                // The above calculation misses the first tick, if it is exactly at Min.
                if ((first - majorTickStep) >= Min)
                    first -= majorTickStep;
            }

            // Now make list of major tick positions.
            majorTicks.Clear();

            double position = first;
            int safetyCount = 0;
            while (position < Max && ++safetyCount < 5000)
            {
                majorTicks.Add(position);

                position += majorTickStep;

                // Clamp near zero-Values to zero, otherwise we could get a value like 5.553434e-17 instead of 0.        
                if (Math.Abs(majorTickStep) > 1e-7 && Math.Abs(position) < 1e-7)  // Arbitrary epsilon value.
                    position = 0;
            }

            // The last value could be the Max limit + an epsilon. In this case we add a tick for the 
            // Max value.
            if (Numeric.AreEqual(position, Max))
                majorTicks.Add(Max);

            // If the physical extent is too small, and the middle ticks should be turned into minor 
            // ticks, then do this now.
            minorTicks.Clear();
            if (shouldCullMiddle)
            {
                if (majorTicks.Count > 2)
                    for (int i = 1; i < majorTicks.Count - 1; ++i)
                        minorTicks.Add(majorTicks[i]);

                var culledPositions = new List<double>
                {
                    majorTicks[0],
                    majorTicks[majorTicks.Count - 1]
                };
                majorTicks.Clear();

                foreach (double value in culledPositions)
                    majorTicks.Add(value);
            }
        }


        /// <summary>
        /// Determines the data value of the minor ticks if they have not already been generated.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <param name="majorTicks">The list of major ticks.</param>
        /// <param name="minorTicks">The list of minor ticks.</param>
        private void ComputeTicks_SecondPass(double axisLength, double minDistance, List<double> majorTicks, List<double> minorTicks)
        {
            if (minorTicks.Count != 0)
            {
                // Minor ticks have already been set.
                return;
            }

            minorTicks.Clear();
            bool shouldCullMiddle;
            double majorTickStep = DetermineMajorTickStep(axisLength, minDistance, out shouldCullMiddle);

            int numberOfMinorTicks = DetermineNumberOfMinorTicks(majorTickStep);
            double minorTickStep = majorTickStep / (numberOfMinorTicks + 1);

            // If there is at least one big tick
            if (majorTicks.Count > 0)
            {
                double pos1 = majorTicks[0] - minorTickStep;
                while (Min <= pos1)
                {
                    minorTicks.Add(pos1);
                    pos1 -= minorTickStep;
                }
            }

            for (int i = 0; i < majorTicks.Count; ++i)
            {
                for (int j = 1; j < (numberOfMinorTicks + 1); ++j)
                {
                    double pos = majorTicks[i] + j * minorTickStep;
                    if (pos <= Max)
                        minorTicks.Add(pos);
                }
            }
        }


        /// <summary>
        /// Calculates the world spacing between major ticks, based on the drawing length of the
        /// axis, axis data value range, <see cref="Mantissas"/> values and the min tick distance.
        /// </summary>
        /// <param name="axisLength">Length of the axis in device-independent pixels.</param>
        /// <param name="minDistance">
        /// The minimum distance between major ticks in device-independent pixels.
        /// </param>
        /// <param name="shouldCullMiddle">
        /// Returns <see langword="true"/> if we were forced to make spacing of major ticks too
        /// small in order to ensure that there are at least two of them. The draw ticks method
        /// should not draw more than two major ticks if this returns <see langword="true"/>.
        /// </param>
        /// <returns>Large tick spacing</returns>
        /// <exception cref="ChartException">
        /// <see cref="MajorTickStep"/> is zero or negative.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "MajorTickStep")]
        private double DetermineMajorTickStep(double axisLength, double minDistance, out bool shouldCullMiddle)
        {
            shouldCullMiddle = false;

            // If the major tick has been explicitly set, then return this.
            if (!Numeric.IsNaN(MajorTickStep))
            {
                if (MajorTickStep <= 0.0f)
                    throw new ChartException("MajorTickStep must be greater than zero.");

                return MajorTickStep;
            }

            // Otherwise we need to calculate the major tick step ourselves.

            // Adjust world max and min for offset and scale properties of axis.
            double range = Max - Min;

            // If axis has zero range, then return arbitrary number.
            if (Numeric.AreEqual(Min, Max))
                return 1.0f;

            double approxTickStep = minDistance / axisLength * range;
            double exponent = Math.Floor(Math.Log10(approxTickStep));
            double mantissa = Math.Pow(10.0, Math.Log10(approxTickStep) - exponent);

            // Determine next whole mantissa below the approx one.
            int mantissaIndex = Mantissas.Length - 1;
            for (int i = 1; i < Mantissas.Length; ++i)
            {
                if (mantissa < Mantissas[i])
                {
                    mantissaIndex = i - 1;
                    break;
                }
            }

            // Then choose next largest spacing. 
            mantissaIndex += 1;
            if (mantissaIndex == Mantissas.Length)
            {
                mantissaIndex = 0;
                exponent += 1.0;
            }

            // Now make sure that the returned value is such that at least two major tick marks will be 
            // displayed.
            double tickStep = Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
            double physicalStep = tickStep / range * axisLength;

            while (physicalStep > axisLength / 2)
            {
                shouldCullMiddle = true;

                mantissaIndex -= 1;
                if (mantissaIndex == -1)
                {
                    mantissaIndex = Mantissas.Length - 1;
                    exponent -= 1.0;
                }

                tickStep = Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
                physicalStep = tickStep / range * axisLength;
            }

            // And we're done.
            return Math.Pow(10.0, exponent) * Mantissas[mantissaIndex];
        }


        /// <summary>
        /// Given the major tick step, determine the number of minor ticks that should be placed in
        /// between.
        /// </summary>
        /// <param name="majorTickDistance">The major tick step.</param>
        /// <returns>The number of minor ticks to place between major ticks.</returns>
        private int DetermineNumberOfMinorTicks(double majorTickDistance)
        {
            if (NumberOfMinorTicks >= 0)
                return NumberOfMinorTicks;

            Debug.Assert(MinorTickCounts.Length == Mantissas.Length, "Internal static data for choosing ticks steps is inconsistent.");

            if (majorTickDistance > 0.0f)
            {
                double exponent = Math.Floor(Math.Log10(majorTickDistance));
                double mantissa = Math.Pow(10.0, Math.Log10(majorTickDistance) - exponent);

                for (int i = 0; i < Mantissas.Length; ++i)
                    if (Math.Abs(mantissa - Mantissas[i]) < 0.001)
                        return MinorTickCounts[i];
            }

            return 0;
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
        #endregion
    }
}
