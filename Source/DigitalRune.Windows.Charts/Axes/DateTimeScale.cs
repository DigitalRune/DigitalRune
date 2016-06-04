// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a scale that contains dates and times.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Time values need to be specified in Coordinated Universal Time (UTC). The tick labels of the
    /// axis can be localized by specifying a certain time zone. See property <see cref="TimeZone"/>
    /// for additional information.
    /// </para>
    /// <para>
    /// <strong>Important:</strong> The <see cref="DateTime"/> values are internally converted to
    /// <c>double</c> values (using the property <see cref="DateTime.Ticks"/>). This type of scale
    /// is not suitable for representing time values below 1 ms. To represent smaller time ranges
    /// another type of axis scale needs to be used!
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "TimeScale")]
    public class DateTimeScale : AxisScale
    {
        // Some useful facts about time zones:
        // - Time zone offsets range from -12 to +13 h.
        // - Some time zones are off by 30 min!
        // - In addition to regular time zone offsets there are additional offsets because of
        //   Daylight Saving Time and other adjustment rules!
        // - A DateTime value in a local time zone may not exist in UTC! See method
        //   TimeZoneInfo.IsInvalid().
        // - A DateTime value in a local time zone may correspond to multiple DateTime values in
        //   UTC! See method TimeZoneInfo.IsAmbiguous().
        // - Some time zones have really, really strange adjustment rules!
        // - In most cultures the Monday or Sunday marks the start of a week. But there are also
        //   cultures that start the week at a Friday or Saturday!


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private static readonly double[] EmptyArrayOfDoubles = new double[0];

        // Epsilon tolerance used for comparisons of ticks:
        //   Ticks are in the range 10^17 to 10^18. 
        //   Doubles have about 15 decimal digits precision.
        //   => 10^18 / 10^15 = 10^3 is our epsilon tolerance.
        private const int Epsilon = 1000;   // 1000 ticks = 100 µs

        private static readonly TimeSpan[] MillisecondsMajorSteps =
        {
            new TimeSpan(0, 0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 0, 2),
            new TimeSpan(0, 0, 0, 0, 5),
            new TimeSpan(0, 0, 0, 0, 10),
            new TimeSpan(0, 0, 0, 0, 20),
            new TimeSpan(0, 0, 0, 0, 50),
            new TimeSpan(0, 0, 0, 0, 100),
            new TimeSpan(0, 0, 0, 0, 200),
            new TimeSpan(0, 0, 0, 0, 500),
        };

        private static readonly TimeSpan[] MillisecondsMinorSteps =
        {
            new TimeSpan(0, 0, 0, 0, 0),
            new TimeSpan(0, 0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 0, 5),
            new TimeSpan(0, 0, 0, 0, 10),
            new TimeSpan(0, 0, 0, 0, 10),
            new TimeSpan(0, 0, 0, 0, 50),
            new TimeSpan(0, 0, 0, 0, 100),
            new TimeSpan(0, 0, 0, 0, 100),
        };

        private static readonly TimeSpan[] SecondsMajorSteps =
        {
            new TimeSpan(0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 2),
            new TimeSpan(0, 0, 0, 5),
            new TimeSpan(0, 0, 0, 10),
            new TimeSpan(0, 0, 0, 15),
            new TimeSpan(0, 0, 0, 30),
        };

        private static readonly TimeSpan[] SecondsMinorSteps =
        {
            new TimeSpan(0, 0, 0, 500),
            new TimeSpan(0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 1),
            new TimeSpan(0, 0, 0, 5),
            new TimeSpan(0, 0, 0, 5),
            new TimeSpan(0, 0, 0, 15),
        };

        private static readonly TimeSpan[] MinutesMajorSteps =
        {
            new TimeSpan(0, 0, 1, 0),
            new TimeSpan(0, 0, 2, 0),
            new TimeSpan(0, 0, 5, 0),
            new TimeSpan(0, 0, 10, 0),
            new TimeSpan(0, 0, 15, 0),
            new TimeSpan(0, 0, 30, 0),
        };

        private static readonly TimeSpan[] MinutesMinorSteps =
        {
            new TimeSpan(0, 0, 0, 30),
            new TimeSpan(0, 0, 1, 0),
            new TimeSpan(0, 0, 1, 0),
            new TimeSpan(0, 0, 5, 0),
            new TimeSpan(0, 0, 5, 0),
            new TimeSpan(0, 0, 15, 0),
        };

        private static readonly TimeSpan[] HoursMajorSteps =
        {
            new TimeSpan(0, 1, 0, 0),
            new TimeSpan(0, 2, 0, 0),
            new TimeSpan(0, 6, 0, 0),
            new TimeSpan(0, 12, 0, 0),
        };

        private static readonly TimeSpan[] HoursMinorSteps =
        {
            new TimeSpan(0, 0, 30, 0),
            new TimeSpan(0, 1, 0, 0),
            new TimeSpan(0, 3, 0, 0),
            new TimeSpan(0, 6, 0, 0),
        };

        private static readonly TimeSpan[] DaysMajorSteps =
        {
            new TimeSpan(1, 0, 0, 0),
            new TimeSpan(2, 0, 0, 0),
            new TimeSpan(7, 0, 0, 0),
            new TimeSpan(14, 0, 0, 0),
        };

        private static readonly TimeSpan[] DaysMinorSteps =
        {
            new TimeSpan(0, 12, 0, 0),
            new TimeSpan(1, 0, 0, 0),
            new TimeSpan(1, 0, 0, 0),
            new TimeSpan(7, 0, 0, 0),
        };

        private static readonly int[] MonthsMajorSteps = { 1, 2, 3, 6 };
        private static readonly int[] YearsMajorSteps = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000 };
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the type of the major tick labels.
        /// </summary>
        /// <value>The type of the major tick labels.</value>
        public DateTimeLabel TickLabel
        {
            get { return _tickLabel; }
            private set
            {
                if (_tickLabel == value)
                    return;

                _tickLabel = value;
                OnPropertyChanged("TickLabel");
            }
        }
        private DateTimeLabel _tickLabel = DateTimeLabel.None;


        /// <summary>
        /// Gets or sets the distance between major ticks.
        /// </summary>
        /// <value>
        /// The distance between major ticks. If this is set to <see cref="TimeSpan.Zero"/>
        /// (default), this distance will be calculated automatically.
        /// </value>
        /// <remarks>
        /// This property can be set to directly control where the major ticks are placed. The first
        /// major tick will be placed at <see cref="MinDateTime"/>. All subsequent ticks will be
        /// placed at a distance of <see cref="MajorTickStep"/>.
        /// </remarks>
#if !SILVERLIGHT
        [TypeConverter(typeof(TimeSpanConverter))]
#endif
        public TimeSpan MajorTickStep
        {
            get { return _majorTickStepAsTimeSpan; }
            set
            {
                if (_majorTickStepAsTimeSpan == value)
                    return;

                _majorTickStepAsTimeSpan = value;
                OnPropertyChanged("MajorTickStep");
            }
        }
        private TimeSpan _majorTickStepAsTimeSpan = TimeSpan.Zero;


        /// <summary>
        /// Gets or sets the date/time range of the axis.
        /// </summary>
        /// <value>The range of the axis.</value>
        /// <exception cref="ArgumentException">
        /// The specified range is invalid.
        /// </exception>
        public DateTimeRange RangeDateTime
        {
            get { return _rangeDateTime; }
            set
            {
                if (_rangeDateTime == value)
                    return;

                var oldRange = new DoubleRange(_rangeDateTime.Min.Ticks, _rangeDateTime.Max.Ticks);
                var newRange = new DoubleRange(value.Min.Ticks, value.Max.Ticks);
                if (oldRange == newRange)
                    return;

                if (!Numeric.IsLessOrEqual(newRange.Min, newRange.Max, Epsilon))
                    throw new ArgumentException("Invalid range.");

                _rangeDateTime = value;
                Range = newRange;
                OnPropertyChanged("RangeDateTime");
                OnPropertyChanged("MinDateTime");
                OnPropertyChanged("MaxDateTime");
            }
        }
        private DateTimeRange _rangeDateTime =
            new DateTimeRange(
                new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc));


        /// <summary>
        /// Gets the <see cref="AxisScale.Min"/> limit as a <see cref="DateTime"/> value.
        /// </summary>
        /// <value>The <see cref="AxisScale.Min"/> as a <see cref="DateTime"/> value.</value>
#if !SILVERLIGHT
        [TypeConverter(typeof(DateTimeConverter))]
#endif
        public DateTime MinDateTime
        {
            get { return _rangeDateTime.Min; }
        }


        /// <summary>
        /// Gets the <see cref="AxisScale.Max"/> limit as a <see cref="DateTime"/> value.
        /// </summary>
        /// <value>The <see cref="AxisScale.Max"/> as a <see cref="DateTime"/> value.</value>
#if !SILVERLIGHT
        [TypeConverter(typeof(DateTimeConverter))]
#endif
        public DateTime MaxDateTime
        {
            get { return _rangeDateTime.Max; }
        }


        /// <summary>
        /// Gets or sets the format string used for drawing tick labels.
        /// </summary>
        /// <value>
        /// The format string used for drawing the tick labels. The default value is
        /// <see cref="String.Empty"/>.
        /// </value>
        /// <remarks>
        /// See <see cref="StringBuilder.AppendFormat(string, object[])"/> for a description of this
        /// format string. By default, when <see cref="FormatString"/> is <see langword="null"/> or
        /// empty, the format is determined automatically. This property only needs to be set, when
        /// the default formatting should be overridden.
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
        private string _formatString = String.Empty;


        /// <summary>
        /// Gets or sets the time zone (see remarks).
        /// </summary>
        /// <value>
        /// <para>
        /// The time zone info (see remarks). When the value is <see langword="null"/> the
        /// <see cref="DateTimeScale"/> assumes <see cref="TimeZoneInfo.Utc"/>. The default value is
        /// <see cref="TimeZoneInfo.Local"/>.
        /// </para>
        /// <para>
        /// WPF supports all time zones. Silverlight supports only <see cref="TimeZoneInfo.Local"/>,
        /// <see cref="TimeZoneInfo.Utc"/>, or <see langword="null"/> - other time zones are not
        /// supported in Silverlight.
        /// </para>
        /// </value>
        /// <remarks>
        /// <para>
        /// All date/time values need to be specified in Coordinated Universal Time (UTC). That
        /// means, all properties of a <see cref="DateTimeScale"/> such as <see cref="MinDateTime"/>
        /// or <see cref="MaxDateTime"/> are given in UTC. The date/time values of a chart data
        /// source are <strong>always</strong> in UTC.
        /// </para>
        /// <para>
        /// By using this property a specific time zone can be specified to localize the appearance
        /// of the axis. The specified time zone affects only the placement and the text of the tick
        /// labels. The date/time values of a chart data source are not affected. Date/time values
        /// of a data source must always be given in UTC.
        /// </para>
        /// <para>
        /// <strong>Silverlight:</strong> Allowed values are <see cref="TimeZoneInfo.Local"/>
        /// (default), <see cref="TimeZoneInfo.Utc"/> and <see langword="null"/>.
        /// </para>
        /// </remarks>
        public TimeZoneInfo TimeZone
        {
            get { return _timeZone; }
            set
            {
#if SILVERLIGHT
                if (value != null && value != TimeZoneInfo.Local && value != TimeZoneInfo.Utc)
                    throw new ArgumentException("TimeZone must be either Local, Utc or null.", "value");
#endif
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (_timeZone == value)
                    return;

                _timeZone = value;
                OnPropertyChanged("TimeZone");
            }
        }
        private TimeZoneInfo _timeZone = TimeZoneInfo.Local;
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScale"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScale"/> class.
        /// </summary>
        public DateTimeScale()
            : this(new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScale"/> class with the given range
        /// in ticks.
        /// </summary>
        /// <param name="range">The range of the axis.</param>
        /// <exception cref="ArgumentException">
        /// <paramref name="range"/> is invalid.
        /// </exception>
        public DateTimeScale(DateTimeRange range)
            : this(range.Min, range.Max)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScale"/> class with the given range
        /// in ticks.
        /// </summary>
        /// <param name="min">The minimum value in ticks.</param>
        /// <param name="max">The maximum value in ticks.</param>
        /// <exception cref="ArgumentException">
        /// [<paramref name="min"/>, <paramref name="max"/>] is not a valid range.
        /// </exception>
        public DateTimeScale(double min, double max)
            : this(new DateTime((long)min, DateTimeKind.Utc),
                   new DateTime((long)max, DateTimeKind.Utc))
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeScale"/> class with the given
        /// <see cref="DateTime"/> range.
        /// </summary>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <exception cref="ArgumentException">
        /// [<paramref name="min"/>, <paramref name="max"/>] is not a valid range.
        /// </exception>
        public DateTimeScale(DateTime min, DateTime max)
            : base(min.Ticks, max.Ticks)
        {
            _rangeDateTime = new DateTimeRange(min, max);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="AxisScale.PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property that has changed.</param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnPropertyChanged"/> in
        /// a derived class, be sure to call the base class's <see cref="OnPropertyChanged"/> method
        /// so that registered delegates receive the event.
        /// </remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == "Range")
            {
                RangeDateTime = new DateTimeRange(
                    new DateTime((long)Min, DateTimeKind.Utc),
                    new DateTime((long)Max, DateTimeKind.Utc));
            }

            base.OnPropertyChanged(propertyName);
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
        public override void ComputeTicks(double axisLength,
                                          double minDistance,
                                          out double[] labelValues,
                                          out double[] majorTicks,
                                          out double[] minorTicks)
        {
            if (Numeric.IsZero(axisLength))
            {
                // The length of the axis is 0.
                labelValues = EmptyArrayOfDoubles;
                majorTicks = EmptyArrayOfDoubles;
                minorTicks = EmptyArrayOfDoubles;
                return;
            }

            // Check Min and Max properties.
            if (Numeric.IsNaN(Min) || Numeric.IsNaN(Max) || Min > Max)
                throw new ChartException("Properties Min and Max of the scale have invalid values.");

            // Check with custom epsilon because the time is specified in ticks.
            if (Numeric.AreEqual(Min, Max, Epsilon))
                throw new ChartException("Properties Min and Max of the scale have invalid values. The values are identical or nearly identical.");

            double maxNumberOfTicks = axisLength / minDistance;

            TimeSpan minorTickStep;
            majorTicks = ComputeMajorTicks(maxNumberOfTicks, out minorTickStep);
            minorTicks = ComputeMinorTicks(majorTicks, minorTickStep);
            labelValues = majorTicks;
        }


        /// <summary>
        /// Determines the data values of the major ticks and the step size to use for minor ticks.
        /// </summary>
        /// <param name="maxNumberOfTicks">The maximal number of ticks.</param>
        /// <param name="minorTickStep">Out: The minor tick step.</param>
        /// <returns>The data values at which major ticks should be drawn.</returns>
        private double[] ComputeMajorTicks(double maxNumberOfTicks, out TimeSpan minorTickStep)
        {
            var min = new DateTime((long)Min, DateTimeKind.Utc);
            var max = new DateTime((long)Max, DateTimeKind.Utc);
            var majorTicks = new List<double>();

            if (MajorTickStep == TimeSpan.Zero)
            {
                // ----- Automatic tick placement
                int majorTickStep;
                DetermineTickStep(maxNumberOfTicks, out majorTickStep, out minorTickStep);

                switch (TickLabel)
                {
                    case DateTimeLabel.Milliseconds:
                        AddTicksInMilliseconds(min, max, majorTickStep, majorTicks);
                        break;
                    case DateTimeLabel.Seconds:
                        AddTicksInSeconds(min, max, majorTickStep, majorTicks);
                        break;
                    case DateTimeLabel.Minutes:
                        AddTicksInMinutes(min, max, majorTickStep, majorTicks);
                        break;
                    case DateTimeLabel.Hours:
                        AddTicksInHours(min, max, majorTickStep, majorTicks);
                        break;
                    case DateTimeLabel.Days:
                        AddTicksInDays(min, max, majorTickStep, majorTicks);
                        break;
                    case DateTimeLabel.Months:
                        AddTicksInMonths(min, max, majorTickStep, majorTicks);
                        break;
                    default:
                        AddTicksInYears(min, max, majorTickStep, majorTicks);
                        break;
                }
            }
            else
            {
                // ----- User-defined ticks
                // Place major ticks at a distance of MajorTimeStep starting at MinDateTime.
                for (DateTime time = min; time <= max; time += MajorTickStep)
                    majorTicks.Add(time.Ticks);

                // No minor ticks.
                minorTickStep = TimeSpan.Zero;
            }

            return majorTicks.ToArray();
        }


        /// <summary>
        /// Determines the major tick step.
        /// </summary>
        /// <param name="maxNumberOfTicks">The max number of ticks.</param>
        /// <param name="majorTickStep">
        /// Out: The major tick step. The unit is stored in <see cref="TickLabel"/>.
        /// </param>
        /// <param name="minorTickStep">Out: The minor tick step.</param>
        private void DetermineTickStep(double maxNumberOfTicks, out int majorTickStep, out TimeSpan minorTickStep)
        {
            long range = MaxDateTime.Ticks - MinDateTime.Ticks;

            // Try milliseconds
            for (int i = 0; i < MillisecondsMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / MillisecondsMajorSteps[i].Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = MillisecondsMajorSteps[i].Milliseconds;
                    minorTickStep = MillisecondsMinorSteps[i];
                    TickLabel = DateTimeLabel.Milliseconds;
                    return;
                }
            }

            // Try seconds
            for (int i = 0; i < SecondsMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / SecondsMajorSteps[i].Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = SecondsMajorSteps[i].Seconds;
                    minorTickStep = SecondsMinorSteps[i];
                    TickLabel = DateTimeLabel.Seconds;
                    return;
                }
            }

            // Try minutes
            for (int i = 0; i < MinutesMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / MinutesMajorSteps[i].Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = MinutesMajorSteps[i].Minutes;
                    minorTickStep = MinutesMinorSteps[i];
                    TickLabel = DateTimeLabel.Minutes;
                    return;
                }
            }

            // Try hours
            for (int i = 0; i < HoursMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / HoursMajorSteps[i].Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = HoursMajorSteps[i].Hours;
                    minorTickStep = HoursMinorSteps[i];
                    TickLabel = DateTimeLabel.Hours;
                    return;
                }
            }

            // Try days
            for (int i = 0; i < DaysMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / DaysMajorSteps[i].Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = DaysMajorSteps[i].Days;
                    minorTickStep = DaysMinorSteps[i];
                    TickLabel = DateTimeLabel.Days;
                    return;
                }
            }

            // Try months
            for (int i = 0; i < MonthsMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / new TimeSpan((int)(30.5 * MonthsMajorSteps[i]), 0, 0, 0).Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = MonthsMajorSteps[i];
                    minorTickStep = TimeSpan.Zero;
                    TickLabel = DateTimeLabel.Months;
                    return;
                }
            }

            // Try years
            for (int i = 0; i < YearsMajorSteps.Length; ++i)
            {
                long numberOfTicks = range / new TimeSpan((int)(365.5 * YearsMajorSteps[i]), 0, 0, 0).Ticks;
                if (numberOfTicks < maxNumberOfTicks)
                {
                    majorTickStep = YearsMajorSteps[i];
                    minorTickStep = TimeSpan.Zero;
                    TickLabel = DateTimeLabel.Years;
                    return;
                }
            }

            // Fallback
            majorTickStep = 2000;
            minorTickStep = TimeSpan.Zero;
            TickLabel = DateTimeLabel.Years;
        }


        private void AddTicksInMilliseconds(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            int milliseconds = min.Millisecond;
            milliseconds -= milliseconds % tickStep;
            var time = new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, min.Second, milliseconds, DateTimeKind.Utc);

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                    ticks.Add(tick);

                time = time.AddMilliseconds(tickStep);
            }
        }


        private void AddTicksInSeconds(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            int second = min.Second;
            second -= second % tickStep;
            var time = new DateTime(min.Year, min.Month, min.Day, min.Hour, min.Minute, second, DateTimeKind.Utc);

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                    ticks.Add(tick);

                time = time.AddSeconds(tickStep);
            }
        }


        private void AddTicksInMinutes(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            int minute = min.Minute;
            minute -= minute % tickStep;
            var time = new DateTime(min.Year, min.Month, min.Day, min.Hour, minute, 0, DateTimeKind.Utc);

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                    ticks.Add(tick);

                time = time.AddMinutes(tickStep);
            }
        }


        private void AddTicksInHours(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            // Go back to the start of the day.
            var time = new DateTime(min.Year, min.Month, min.Day, 0, 0, 0, DateTimeKind.Utc);

            if (TimeZone != null && !TimeZone.Equals(TimeZoneInfo.Utc))
            {
                // ----- Place ticks in local time zone.
                // Some time zones are off by 30 min. In addition there might be Daylight
                // Saving Time or other adjustments, which makes it difficult to find the
                // right hour in the local time zone.
                // In order to avoid time zone issues, start one day earlier and advance in
                // 1 hour increments.
                time = time.AddDays(-1);

                // Round time to the start of the day in the time zone.
                time = RoundToDay(time, TimeZone);
                DateTime localTime = TimeZoneInfo.ConvertTime(time, TimeZone);

                while (time <= max)
                {
                    double tick = time.Ticks;
                    if (Min <= tick && tick <= Max)
                    {
                        // Align ticks with hours in specified time zone.
                        Debug.Assert(localTime.Minute == 0, "The time should have been rounded to full hours.");

                        // Set ticks every n hours according to tickStep.
                        if ((localTime.Hour % tickStep) == 0)
                        {
                            Debug.Assert(!ticks.Contains(tick), "Duplicate ticks.");
                            ticks.Add(tick);
                        }
                    }

                    // Advance in 1 hour increments.
                    time = time.AddHours(1);
                    time = RoundToHour(time, TimeZone, out localTime);
                }
            }
            else
            {
                // ----- Place ticks in UTC.
                while (time <= max)
                {
                    double tick = time.Ticks;
                    if (Min <= tick && tick <= Max)
                    {
                        // Set ticks every n hours according to tickStep.
                        if ((time.Hour % tickStep) == 0)
                        {
                            Debug.Assert(!ticks.Contains(tick), "Duplicate ticks.");
                            ticks.Add(tick);
                        }
                    }

                    // Advance in 1 hour increments.
                    time = time.AddHours(1);
                }
            }
        }


        private void AddTicksInDays(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            DateTime time = new DateTime(min.Year, min.Month, min.Day, 0, 0, 0, DateTimeKind.Utc);

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (tickStep == 2.0)
            {
                // When making steps of 2 we cannot align the ticks with the start of the weeks.
                // Just start at an even number of days to ensure that the ticks are consistent
                // and do not 'jump' when moving the axis.
                TimeSpan timeSinceBeginning = time - DateTime.MinValue;
                if (timeSinceBeginning.Days % 2 == 1)
                    time = time.AddDays(-1.0);
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (tickStep == 7 || tickStep == 14.0)
            {
                // Align the ticks with the start of a week.
                var dateTimeFormatInfo = DateTimeFormatInfo.CurrentInfo ?? DateTimeFormatInfo.InvariantInfo;
                int startOfWeek = (int)dateTimeFormatInfo.FirstDayOfWeek;
                int dayOfWeek = (int)time.DayOfWeek;
                int offset = -((7 + dayOfWeek - startOfWeek) % 7);
                time = time.AddDays(offset);
                Debug.Assert(time.DayOfWeek == (DayOfWeek)startOfWeek, "Day should be first day of week according to current culture.");
            }

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (tickStep == 14.0f)
            {
                // When making steps of 2 weeks start at an even number of weeks to ensure
                // that the ticks are consistent and do not 'jump' when moving the axis.
                TimeSpan timeSinceBeginning = time - DateTime.MinValue;
                if ((timeSinceBeginning.Days / 7) % 2 == 1)
                    time = time.AddDays(-7.0);
            }

            // Round time to the start of the day in the specified time zone.
            if (TimeZone != null && !TimeZone.Equals(TimeZoneInfo.Utc))
            {
                // The time in UTC needs to be close to the start of the day in the given
                // time zone, otherwise it is impossible to round to the correct day.
                // (Time zone offsets range from -12 to +13 h!)

                // Apply base offset get start of the day in the local time zone (standard time).
                time = time - TimeZone.BaseUtcOffset;

                // Apply Daylight Saving Time or other adjustment rules.
                time = RoundToDay(time, TimeZone);
            }

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                {
                    Debug.Assert(!ticks.Contains(tick), "Duplicate ticks.");
                    ticks.Add(tick);
                }

                time = time.AddDays(tickStep);
                time = RoundToDay(time, TimeZone);
            }
        }


        private void AddTicksInMonths(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            DateTime time = new DateTime(min.Year, min.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            // Go back to last major tick step.
            time = time.AddMonths(-(time.Month - 1) % tickStep);

            // Round time to the start of the month in the specified time zone.
            time = RoundToMonth(time, TimeZone);

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                {
                    Debug.Assert(!ticks.Contains(tick), "Duplicate ticks.");
                    ticks.Add(tick);
                }

                time = time.AddMonths(tickStep);
                time = RoundToMonth(time, TimeZone);
            }
        }


        private void AddTicksInYears(DateTime min, DateTime max, int tickStep, List<double> ticks)
        {
            DateTime time = new DateTime(min.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Go back to last major tick step.
            time = time.AddYears(-time.Year % tickStep);

            // Round time to the start of the year in the specified time zone.
            time = RoundToYear(time, TimeZone);

            while (time <= max)
            {
                double tick = time.Ticks;
                if (Min <= tick && tick <= Max)
                {
                    Debug.Assert(!ticks.Contains(tick), "Duplicate ticks.");
                    ticks.Add(tick);
                }

                time = time.AddYears(tickStep);
                time = RoundToYear(time, TimeZone);
            }
        }


        /// <summary>
        /// Rounds the time to the start of the hour in the specified time zone.
        /// </summary>
        /// <param name="time">The time in UTC.</param>
        /// <param name="timeZone">The time zone. (Can be <see langword="null"/>.)</param>
        /// <param name="localTime">Out: The time in the specified time zone.</param>
        /// <returns>
        /// The start of the day in the specified time zone. The returned value is given in UTC.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method returns <paramref name="time"/> unchanged if <paramref name="timeZone"/> is
        /// <see cref="TimeZoneInfo.Utc"/> or <see langword="null"/>.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> The returned <paramref name="localTime"/> is valid (i.e. has
        /// an equivalent in UTC), but it may be ambiguous (i.e. has more than one equivalent in
        /// UTC)!
        /// </para>
        /// </remarks>
        private static DateTime RoundToHour(DateTime time, TimeZoneInfo timeZone, out DateTime localTime)
        {
            Debug.Assert(time.Kind == DateTimeKind.Utc, "Time is expected to be UTC.");
            if (timeZone == null || timeZone.Equals(TimeZoneInfo.Utc))
            {
                localTime = time;
                return time;
            }

            // Convert time to specified time zone.
            localTime = TimeZoneInfo.ConvertTime(time, timeZone);

            // Add half an hour for rounding.
            localTime = localTime.AddMinutes(30);

#if SILVERLIGHT
            // Silverlight supports only conversion Local <-> UTC:
            localTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0, DateTimeKind.Local);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            // When switching back from Daylight Saving Time to normal time, the time in the
            // local time zone is ambiguous and can be mapped to different time values in UTC.
            if (timeZone.IsAmbiguousTime(localTime))
            {
                // Map the local time to the time in UTC which is closest to the original value.
                TimeSpan[] offsets = timeZone.GetAmbiguousTimeOffsets(localTime);
                TimeSpan minDistance = TimeSpan.MaxValue;
                DateTime closestTime = new DateTime();
                foreach (var offset in offsets)
                {
                    DateTime timeUtc = localTime - offset;
                    TimeSpan distance = (timeUtc - time).Duration();
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTime = timeUtc;
                    }
                }

                time = DateTime.SpecifyKind(closestTime, DateTimeKind.Utc);
            }
            else
            {
                time = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
            }
#else
            localTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, localTime.Hour, 0, 0);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            // When switching back from Daylight Saving Time to normal time, the time in the 
            // local time zone is ambiguous and can be mapped to different time values in UTC.  
            if (timeZone.IsAmbiguousTime(localTime))
            {
                // Map the local time to the time in UTC which is closest to the original value.
                TimeSpan[] offsets = timeZone.GetAmbiguousTimeOffsets(localTime);
                TimeSpan minDistance = TimeSpan.MaxValue;
                DateTime closestTime = new DateTime();
                foreach (var offset in offsets)
                {
                    DateTime timeUtc = localTime - offset;
                    TimeSpan distance = (timeUtc - time).Duration();
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTime = timeUtc;
                    }
                }

                time = DateTime.SpecifyKind(closestTime, DateTimeKind.Utc);
            }
            else
            {
                time = TimeZoneInfo.ConvertTime(localTime, timeZone, TimeZoneInfo.Utc);
            }
#endif

            return time;
        }


        /// <summary>
        /// Rounds the time to the start of the day in the specified time zone.
        /// </summary>
        /// <param name="time">
        /// The time in UTC, which should to be close to the actual start of the day in specified
        /// time zone!
        /// </param>
        /// <param name="timeZone">The time zone. (Can be <see langword="null"/>.)</param>
        /// <returns>
        /// The start of the day in the specified time zone. The returned value is given in UTC.
        /// </returns>
        /// <remarks>
        /// The method returns <paramref name="time"/> unchanged if <paramref name="timeZone"/> is
        /// <see cref="TimeZoneInfo.Utc"/> or <see langword="null"/>.
        /// </remarks>
        private static DateTime RoundToDay(DateTime time, TimeZoneInfo timeZone)
        {
            Debug.Assert(time.Kind == DateTimeKind.Utc, "Time is expected to be UTC.");
            if (timeZone == null || timeZone.Equals(TimeZoneInfo.Utc))
                return time;

            // Rounding can be difficult because time zone offsets range from -12 to +13 h.
            // Precondition: The specified time in UTC must be close to the start of the day
            // in the local time zone!

            // Convert time to current time zone to get the correct number of the day.
            var localTime = TimeZoneInfo.ConvertTime(time, timeZone);

            // Add a half day for safety.
            localTime = localTime.AddHours(12);

#if SILVERLIGHT
            // Silverlight supports only conversion Local <-> UTC:
            localTime = new DateTime(localTime.Year, localTime.Month, localTime.Day, 0, 0, 0, DateTimeKind.Local);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
#else
            localTime = new DateTime(localTime.Year, localTime.Month, localTime.Day);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, timeZone, TimeZoneInfo.Utc);
#endif

            return time;
        }


        /// <summary>
        /// Rounds the time to the start of the month in the specified time zone.
        /// </summary>
        /// <param name="time">The time in UTC.</param>
        /// <param name="timeZone">The time zone. (Can be <see langword="null"/>.)</param>
        /// <returns>
        /// The start of the month in the specified time zone. The returned value is given in UTC.
        /// </returns>
        /// <remarks>
        /// The method returns <paramref name="time"/> unchanged if <paramref name="timeZone"/> is
        /// <see cref="TimeZoneInfo.Utc"/> or <see langword="null"/>.
        /// </remarks>
        private static DateTime RoundToMonth(DateTime time, TimeZoneInfo timeZone)
        {
            Debug.Assert(time.Kind == DateTimeKind.Utc, "Time is expected to be UTC.");
            if (timeZone == null || timeZone.Equals(TimeZoneInfo.Utc))
                return time;

            // Add a half month for safety and then round down. (See RoundToYear for comments.)
            time = time.AddDays(15);

#if SILVERLIGHT
            // Silverlight supports only conversion Local <-> UTC:
            var localTime = new DateTime(time.Year, time.Month, 1, 0, 0, 0, DateTimeKind.Local);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
#else
            var localTime = new DateTime(time.Year, time.Month, 1);
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, timeZone, TimeZoneInfo.Utc);
#endif

            return time;
        }


        /// <summary>
        /// Rounds the time to the start of the year in the specified time zone.
        /// </summary>
        /// <param name="time">The time in UTC.</param>
        /// <param name="timeZone">The time zone. (Can be <see langword="null"/>.)</param>
        /// <returns>
        /// The start of the year in the specified time zone. The returned value is given in UTC.
        /// </returns>
        /// <remarks>
        /// The method returns <paramref name="time"/> unchanged if <paramref name="timeZone"/> is
        /// <see cref="TimeZoneInfo.Utc"/> or <see langword="null"/>.
        /// </remarks>
        private static DateTime RoundToYear(DateTime time, TimeZoneInfo timeZone)
        {
            Debug.Assert(time.Kind == DateTimeKind.Utc, "Time is expected to be UTC.");
            if (timeZone == null || timeZone.Equals(TimeZoneInfo.Utc))
                return time;

            // The time in UTC does not exactly match the start of the year in the local
            // time zone. In order to ensure that year is correct we can simply add one
            // month for safety and then round down.
            time = time.AddMonths(1);

#if SILVERLIGHT
            // Silverlight supports only conversion Local <-> UTC:
            var localTime = new DateTime(time.Year, 1, 1, 0, 0, 0, DateTimeKind.Local);

            // The local time may be invalid. For example, the date 2009-01-01 0:00 
            // does not exist in the time zone "(UTC -3:00) Buenos Aires"!
            // --> Pick the first valid hour as the start of the year.
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Utc);
#else
            var localTime = new DateTime(time.Year, 1, 1);

            // The local time may be invalid. For example, the date 2009-01-01 0:00
            // does not exist in the time zone "(UTC -3:00) Buenos Aires"!
            // --> Pick the first valid hour as the start of the year.
            while (timeZone.IsInvalidTime(localTime))
                localTime = localTime.AddHours(1);

            time = TimeZoneInfo.ConvertTime(localTime, timeZone, TimeZoneInfo.Utc);
#endif

            return time;
        }


        /// <summary>
        /// Compute the minor tick values.
        /// </summary>
        /// <param name="majorTicks">The major ticks.</param>
        /// <param name="minorTickStep">The minor tick step.</param>
        /// <returns>The data values at which minor ticks should be drawn.</returns>
        private double[] ComputeMinorTicks(double[] majorTicks, TimeSpan minorTickStep)
        {
            // Precondition:
            // If major ticks have distance of 1 h or more then the major ticks are aligned
            // with full hours in the specified time zone.

            // In certain cases there are no minor ticks.
            if (majorTicks.Length <= 0 || minorTickStep == TimeSpan.Zero)
                return EmptyArrayOfDoubles;

            var minorTicks = new List<double>();

            if (TimeZone != null && !TimeZone.Equals(TimeZoneInfo.Utc) && minorTickStep > new TimeSpan(1, 0, 0))
            {
                // ----- Align minor ticks with the specified time zone.
                if (minorTickStep >= new TimeSpan(1, 0, 0, 0))
                {
                    // ----- Align to days.
                    // Add minor ticks before first major tick.
                    int step = minorTickStep.Days;
                    DateTime time = new DateTime((long)majorTicks[0], DateTimeKind.Utc);
                    time = time.AddDays(-step);
                    time = RoundToDay(time, TimeZone);
                    while (MinDateTime <= time)
                    {
                        minorTicks.Add(time.Ticks);
                        time = time.AddDays(-step);
                        time = RoundToDay(time, TimeZone);
                    }

                    // Add minor ticks between major tick.
                    for (int i = 0; i < majorTicks.Length - 1; i++)
                    {
                        DateTime previousTick = new DateTime((long)majorTicks[i], DateTimeKind.Utc);
                        DateTime nextTick = new DateTime((long)majorTicks[i + 1], DateTimeKind.Utc);
                        time = previousTick.AddDays(step);
                        time = RoundToDay(time, TimeZone);
                        while (time < nextTick)
                        {
                            minorTicks.Add(time.Ticks);
                            time = time.AddDays(step);
                            time = RoundToDay(time, TimeZone);
                        }
                    }

                    // Add minor ticks after last major tick.
                    time = new DateTime((long)majorTicks[majorTicks.Length - 1], DateTimeKind.Utc);
                    time = time.AddDays(step);
                    time = RoundToDay(time, TimeZone);
                    while (time <= MaxDateTime)
                    {
                        minorTicks.Add(time.Ticks);
                        time = time.AddDays(step);
                        time = RoundToDay(time, TimeZone);
                    }
                }
                else
                {
                    // ----- Align to hours.

                    // Add minor ticks before first major tick.
                    int step = minorTickStep.Hours;
                    DateTime time = new DateTime((long)majorTicks[0], DateTimeKind.Utc);
                    DateTime localTime;
                    time = time.AddHours(-1);
                    time = RoundToHour(time, TimeZone, out localTime);
                    while (MinDateTime <= time)
                    {
                        if ((localTime.Hour % step) == 0)
                            minorTicks.Add(time.Ticks);

                        time = time.AddHours(-1);
                        time = RoundToHour(time, TimeZone, out localTime);
                    }

                    // Add minor ticks between major tick.
                    for (int i = 0; i < majorTicks.Length - 1; i++)
                    {
                        DateTime previousTick = new DateTime((long)majorTicks[i], DateTimeKind.Utc);
                        DateTime nextTick = new DateTime((long)majorTicks[i + 1], DateTimeKind.Utc);
                        time = previousTick.AddHours(1);
                        time = RoundToHour(time, TimeZone, out localTime);
                        while (time < nextTick)
                        {
                            if ((localTime.Hour % step) == 0)
                                minorTicks.Add(time.Ticks);

                            time = time.AddHours(1);
                            time = RoundToHour(time, TimeZone, out localTime);
                        }
                    }

                    // Add minor ticks after last major tick.
                    time = new DateTime((long)majorTicks[majorTicks.Length - 1], DateTimeKind.Utc);
                    time = time.AddHours(1);
                    time = RoundToHour(time, TimeZone, out localTime);
                    while (time <= MaxDateTime)
                    {
                        if ((localTime.Hour % step) == 0)
                            minorTicks.Add(time.Ticks);

                        time = time.AddHours(1);
                        time = RoundToHour(time, TimeZone, out localTime);
                    }
                }
            }
            else
            {
                // ----- Add minor ticks in UTC - no special treatment required.
                double step = minorTickStep.Ticks;

                // Add minor ticks before first major tick.
                double tick = majorTicks[0] - step;
                while (Min <= tick)
                {
                    minorTicks.Add(tick);
                    tick -= step;
                }

                // Add minor ticks between major ticks.
                for (int i = 0; i < majorTicks.Length - 1; i++)
                {
                    double previousTick = majorTicks[i];
                    double nextTick = majorTicks[i + 1];
                    tick = previousTick + step;
                    while (Numeric.IsLess(tick, nextTick, Epsilon))
                    {
                        minorTicks.Add(tick);
                        tick += step;
                    }
                }

                // Add minor ticks after last major tick.
                tick = majorTicks[majorTicks.Length - 1] + step;
                while (tick <= Max)
                {
                    minorTicks.Add(tick);
                    tick += step;
                }
            }

            minorTicks.Sort();
            return minorTicks.ToArray();
        }


        /// <overloads>
        /// <summary>
        /// Gets the label text for a specified value.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Gets the label text for a specified tick value.
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
            return GetText(new DateTime((long)value, DateTimeKind.Utc), cultureInfo);
        }


        /// <summary>
        /// Gets the label text for a specified <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="formatProvider">
        /// The <see cref="IFormatProvider"/> that supplies culture-specific formatting information.
        /// </param>
        /// <returns>The label text.</returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="value"/> is not given in Coordinated Universal Time (UTC).
        /// </exception>
        public string GetText(DateTime value, IFormatProvider formatProvider)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The date/time value needs to be given in Coordinated Universal Time (UTC).", "value");

            // Convert DateTime value to local time zone.
            if (TimeZone != null && !TimeZone.Equals(TimeZoneInfo.Utc))
            {
                value = TimeZoneInfo.ConvertTime(value, TimeZone);

                // Because of certain adjustment rules in may happen that a time value
                // does not exist in the local time zone.
                if (TimeZone.IsInvalidTime(value))
                    return string.Empty;
            }

            string label = String.Empty;

            if (String.IsNullOrEmpty(FormatString))
            {
                if (TickLabel == DateTimeLabel.Years)
                {
                    // E.g. "2008"
                    label = value.ToString("yyyy", formatProvider);
                }
                else if (TickLabel == DateTimeLabel.Months)
                {
                    // E.g. "Nov 2008" when cultureInfo is de-AT.
                    label = value.ToString("MMM yyyy", formatProvider);
                }
                else if (TickLabel == DateTimeLabel.Days)
                {
                    // E.g. "24.11.2008" when cultureInfo is de-AT.
                    label = value.ToString("d", formatProvider);
                }
                else if (TickLabel == DateTimeLabel.Hours || TickLabel == DateTimeLabel.Minutes)
                {
                    if (value.Hour == 0 && value.Minute == 0)
                    {
                        // E.g. "24.11.2008 00:00"
                        //label = string.Format(
                        //  "{0}\n{1}", 
                        //  value.ToString("d", cultureInfo), 
                        //  value.ToString("HH:mm", cultureInfo));

                        // E.g. "00:00 24.11.2008"
                        label = string.Format(
                          CultureInfo.InvariantCulture,
                          "{0}\n{1}",
                          value.ToString("HH:mm", formatProvider),
                          value.ToString("d", formatProvider));
                    }
                    else
                    {
                        // E.g. "19:09"          
                        label = value.ToString("HH:mm", formatProvider);
                    }
                }
                else if (TickLabel == DateTimeLabel.Seconds)
                {
                    label = value.ToString("HH:mm:ss", formatProvider);
                }
                else if (TickLabel == DateTimeLabel.Milliseconds)
                {
                    if (value.Hour == 0 && value.Minute == 0)
                        label = value.ToString("s.", formatProvider);
                    else if (value.Hour == 0)
                        label = value.ToString("mm:ss.", formatProvider);
                    else
                        label = value.ToString("HH:mm:ss.", formatProvider);

                    // We need to manually round to milliseconds for precision.
                    int milliseconds = (int)(value.TimeOfDay.TotalMilliseconds + 0.5) % 1000;
                    label = label + milliseconds.ToString("d3", formatProvider);
                }
            }
            else
            {
                label = value.ToString(FormatString, formatProvider);
            }

            return label;
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

            double range = max - min;
            double translation = range * relativeTranslation;
            if (Reversed)
                translation = -translation;

            min += translation;
            max += translation;

            if (min >= DateTime.MinValue.Ticks
                || max <= DateTime.MaxValue.Ticks
                || Numeric.IsLess(min, max, Epsilon))
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
            if (!Numeric.IsFinite(zoomFactor) || 1 <= zoomFactor)
                throw new ArgumentOutOfRangeException("zoomFactor", "The zoom factor must be a value in the range ]-∞, 1[.");

            double min = Min;
            double max = Max;

            if (Numeric.IsZero(zoomFactor))
                return;

            // Change scale limits depending on the current mouse position.
            min += zoomFactor * (anchorValue - min);
            max -= zoomFactor * (max - anchorValue);

            if (min >= DateTime.MinValue.Ticks
                && max <= DateTime.MaxValue.Ticks
                && max - min >= TimeSpan.FromMilliseconds(1).Ticks
                && max - min <= TimeSpan.FromDays(365.25 * 300).Ticks
                && Numeric.IsLess(min, max, Epsilon))
            {
                Range = new DoubleRange(min, max);
            }
        }
        #endregion
    }
}
