// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Groups multiple <see cref="LineChart"/>s into a single chart.
    /// </summary>
    /// <remarks>
    /// The <see cref="LineChartGroup"/> is used to combine several line charts into one stacked
    /// line charts. The charts are either stacked relative or absolute (see <see cref="Grouping"/>
    /// mode) .
    /// </remarks>
    public class LineChartGroup : ChartGroup<LineChart>, ILineChartGroup
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Interpolation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InterpolationProperty =
#if SILVERLIGHT
            DependencyProperty.Register(
                "Interpolation",
                typeof(ChartInterpolation),
                typeof(LineChartGroup),
                new PropertyMetadata(ChartInterpolation.Linear, OnRelevantPropertyChanged));
#else
            LineChart.InterpolationProperty.AddOwner(
                typeof(LineChartGroup),
                new PropertyMetadata(ChartInterpolation.Linear, OnRelevantPropertyChanged));
#endif

        /// <summary>
        /// Gets or sets the type of data interpolation between data points.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The interpolation that is used to connect individual data points. The default is
        /// <see cref="ChartInterpolation.Linear"/>.
        /// </value>
        [Description("Gets or sets the type of interpolation between data points.")]
        [Category(ChartCategories.Default)]
        public ChartInterpolation Interpolation
        {
            get { return (ChartInterpolation)GetValue(InterpolationProperty); }
            set { SetValue(InterpolationProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Grouping"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupingProperty = DependencyProperty.Register(
            "Grouping",
            typeof(LineChartGrouping),
            typeof(LineChartGroup),
            new PropertyMetadata(LineChartGrouping.StackedAbsolute, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value describing how the line charts are grouped (stacked).
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The type of line chart grouping. The default value is 
        /// <see cref="LineChartGrouping.StackedAbsolute"/>.
        /// </value>
        [Description("Gets or sets a value describing how the line charts are grouped (stacked).")]
        [Category(ChartCategories.Default)]
        public LineChartGrouping Grouping
        {
            get { return (LineChartGrouping)GetValue(GroupingProperty); }
            set { SetValue(GroupingProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="LineChartGroup"/> class.
        /// </summary>
        public LineChartGroup()
        {
            DefaultStyleKey = typeof(LineChartGroup);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="LineChartGroup"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LineChartGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChartGroup), new FrameworkPropertyMetadata(typeof(LineChartGroup)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when a relevant property is changed and the charts needs to be updated.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnRelevantPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chartGroup = (ChartGroup)dependencyObject;
            chartGroup.Invalidate();
        }


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Set zIndex of children. The first child should be in front of all others. The last
            // child should be behind. Otherwise parts of the lower chart will be painted over by
            // charts which are stacked on top.
            int zIndex = Items.Count;
            foreach (var chart in Charts)
            {
#if SILVERLIGHT
                Canvas.SetZIndex(chart, zIndex);
#else
                Panel.SetZIndex(chart, zIndex);
#endif
                --zIndex;
            }
        }


        /// <inheritdoc/>
        public void GetValues(LineChart chart, int startIndex, int endIndexExclusive, double[] xValues, double[] baseValues, double[] yValues)
        {
            if (chart == null)
                throw new ArgumentNullException("chart");
            if (xValues == null)
                throw new ArgumentNullException("xValues");
            if (baseValues == null)
                throw new ArgumentNullException("baseValues");
            if (yValues == null)
                throw new ArgumentNullException("yValues");

            var charts = Charts.OfType<LineChart>().ToArray();

            // For absolute and relative stacking: Sum up values of all previous charts in group.
            int chartIndex = Array.IndexOf(charts, chart);
            for (int index = startIndex; index < endIndexExclusive; index++)
            {
                var data = chart.Data[index];
                double x = data.X;
                double y0 = 0;
                double y1 = data.Y;

                for (int i = chartIndex - 1; i >= 0; i--)
                {
                    var otherChart = charts[i];
                    if (otherChart.Data == null)
                        continue;

                    DataPoint otherData = otherChart.Data[index];
                    if (otherData.X != x)
                        throw new ChartDataException("Line charts are stacked, but the X values of the data pairs do not fit together.");

                    y0 += otherData.Y;
                    y1 += otherData.Y;
                }

                xValues[index] = x;
                baseValues[index] = y0;
                yValues[index] = y1;
            }

            // For relative stacking: Sum up values of all following charts.
            if (Grouping == LineChartGrouping.StackedRelative)
            {
                for (int index = startIndex; index < endIndexExclusive; index++)
                {
                    double y0 = baseValues[index];
                    double y1 = yValues[index];
                    double totalSum = y1;

                    // Add y values of other line charts.
                    for (int i = chartIndex + 1; i < Items.Count; i++)
                    {
                        var otherChart = charts[i];
                        if (otherChart == null || otherChart.Data == null)
                            continue;

                        DataPoint otherData = otherChart.Data[index];
                        if (otherData.X != xValues[index])
                            throw new ChartDataException(
                                "Line charts are stacked, but the X values of the data pairs do not fit together.");

                        totalSum += otherData.Y;
                    }

                    if (totalSum > 0)
                    {
                        y0 = y0 / totalSum * 100.0f;
                        y1 = y1 / totalSum * 100.0f;
                    }
                    else
                    {
                        // Use NaN for a gap in the graph.
                        y0 = double.NaN;
                        y1 = double.NaN;
                    }

                    baseValues[index] = y0;
                    yValues[index] = y1;
                }
            }
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            if (Items.Count <= 0)
                return null;

            // For relative stacked charts we use a linear scale with 0 to 100%.
            if (Grouping == LineChartGrouping.StackedRelative)
                return new LinearScale(0, 100);

            var charts = Charts.Cast<LineChart>().ToArray();

            // For absolute stacked charts we add the chart values and find the min
            // and max values.
            double min = 0;
            double max = 0;

            // Get a data source (data source will be used to iterate all stacked points).
            IList<DataPoint> dataSource = null;
            for (int i = 0; i < charts.Length && dataSource == null; i++)
                dataSource = charts[i].Data;

            if (dataSource == null)
                return null;

            // Find min and max.
            for (int i = 0; i < dataSource.Count; i++)
            {
                double sum = 0;

                // Sum up y values for one x value.
                for (int chartIndex = 0; chartIndex < charts.Length; chartIndex++)
                {
                    LineChart chart = charts[chartIndex];
                    if (chart.Data == null || chart.Data.Count <= i)
                        continue;

                    DataPoint data = chart.Data[i];
                    if (Numeric.IsNaN(data.Y))
                        continue;

                    sum += data.Y;
                }

                if (sum < min)
                    min = sum;
                else if (sum > max)
                    max = sum;
            }

            if (Numeric.AreEqual(min, max))
                return new LinearScale(min - 0.5, max + 0.5); // Return a scale with range > 0.
            else
                return new LinearScale(min, max);
        }


        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public override void ValidateData()
        {
            if (Items.Count == 0)
                return;

            var charts = Charts.ToArray();

            // ----- Check if all children have a valid data source.
            foreach (var chart in charts)
                if (chart.Data == null)
                    throw new ChartException("Charts of a LineChartGroup must have a valid DataSource.");

            // ----- Check if the charts have the same number of data values.
            int numberOfDataPoints = charts[0].Data.Count;
            for (int i = 1; i < charts.Length; i++)
                if (charts[i].Data.Count != numberOfDataPoints)
                    throw new ChartException("Stacked charts must have the same number of data values.");

            // ----- Check if the charts have the same x data values.
            for (int i = 0; i < charts[0].Data.Count; i++)
            {
                double x = charts[0].Data[i].X;
                for (int j = 1; j < charts.Length; j++)
                {
                    if (charts[j].Data[i].X != x)
                        throw new ChartException("The x values of stacked line charts must fit together.");
                }
            }

            // ----- Check if all values are positive or all are negative.
            bool? isPositive = null;
            foreach (var chart in charts)
            {
                for (int i = 0; i < numberOfDataPoints; i++)
                {
                    if (!isPositive.HasValue && !Numeric.AreEqual(chart.Data[i].Y, 0))
                        isPositive = chart.Data[i].Y > 0;

                    if (isPositive.HasValue)
                    {
                        if ((isPositive.Value && chart.Data[i].Y < 0)
                            || (!isPositive.Value && chart.Data[i].Y > 0))
                        {
                            throw new ChartException("Mixing of positive and negative data values is not allowed for stacked line charts.");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
