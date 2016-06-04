// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a horizontal or vertical bar chart.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This chart represents information in the form of horizontal bars or vertical bars (columns).
    /// </para>
    /// <para>
    /// The <see cref="Orientation"/> defines whether horizontal or vertical bars are drawn. The
    /// axis from which the bars are drawn is called the base axis. The other axis which determines
    /// the height of the bars is called the data axis.
    /// </para>
    /// <para>
    /// The data pairs of the bars must be ordered. For instance, if the base axis is the x-axis
    /// then the data pairs must be ordered (ascending) by their x values.
    /// </para>
    /// <para>
    /// A bar can be a custom element.The properties <see cref="Chart.DataPointStyle"/> and
    /// <see cref="Chart.DataPointTemplate"/> defines the visual appearance of a bar. The
    /// <see cref="FrameworkElement.DataContext"/> of the bar will be set to the corresponding item
    /// in the <see cref="Chart.DataSource"/>. The property <see cref="FrameworkElement.Tag"/> will
    /// be set to a <see cref="Point"/> containing the x and y value of the bar.
    /// </para>
    /// </remarks>
    public class BarChart : Chart
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the effective orientation. (Group settings overrule local settings.)
        /// </summary>
        protected Orientation EffectiveOrientation
        {
            get
            {
                var group = Group as BarChartGroup;
                return (group != null) ? group.Orientation : Orientation;
            }
        }


        /// <summary>
        /// Gets the effective width of the bar gap. (Group settings overrule local settings.)
        /// </summary>
        protected double EffectiveBarGapWidth
        {
            get
            {
                var group = Group as BarChartGroup;
                return (group != null) ? group.BarGap : BarGap;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="BarGap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BarGapProperty = DependencyProperty.Register(
            "BarGap",
            typeof(double),
            typeof(BarChart),
            new PropertyMetadata(0.4, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the width of the gap between two bars relative to the bar width.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The width of the gap between to bars relative to the bar width.
        /// The default value is 0.4.
        /// </value>
        /// <remarks>
        /// Examples:
        /// 0 means no gap.
        /// 1 means the gap width is equal to the bar width.
        /// 2 means the gap width is twice the bar width.
        /// </remarks>
        [Description("Gets or sets the width of the gap between two bars relative to the bar width.")]
        [Category(ChartCategories.Default)]
        public double BarGap
        {
            get { return (double)GetValue(BarGapProperty); }
            set { SetValue(BarGapProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(BarChart),
            new PropertyMetadata(Orientation.Vertical, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation of the bars.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/> if the x-axis is the base
        /// axis and y-axis shows the data values; otherwise,
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/> if the y-axis is the base
        /// axis and the x-axis shows the data values. Default is
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/>.
        /// </value>
        [Description("Gets or sets the orientation of the bars.")]
        [Category(ChartCategories.Default)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="BarChart"/> class.
        /// </summary>
        public BarChart()
        {
            DefaultStyleKey = typeof(BarChart);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="BarChart"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static BarChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BarChart), new FrameworkPropertyMetadata(typeof(BarChart)));
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
            var chart = (Chart)dependencyObject;
            chart.Invalidate();
        }


        /// <summary>
        /// Raises the <see cref="ChartElement.Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// the base class <see cref="Chart"/> can update the data source if required.
        /// </remarks>
        protected override void OnUpdate()
        {
            base.OnUpdate(); // Updates the data source, if required.

            Debug.Assert(Canvas.Children.Count == 0, "Canvas should be cleared in base class.");

            if (Data == null || Data.Count == 0 || DataPointTemplate == null)
            {
                // A relevant property is not set.
                return;
            }

            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            Orientation orientation = EffectiveOrientation;
            Axis baseAxis = (orientation == Orientation.Vertical) ? xAxis : yAxis;

            // Number of bars in a cluster:
            int numberOfBarCharts = GetNumberOfBarCharts();

            // Index of this bar in the cluster:
            int currentClusterPosition = GetClusterPosition();

            double barWidth = GetBarWidth();
            double effectiveBarGapWidth = EffectiveBarGapWidth;

            var group = Group as BarChartGroup;
            bool isClustered = (group != null && group.Grouping == BarChartGrouping.Clustered);

            // Prepare for clipping
            double leftCutoff = baseAxis.Scale.Min;
            double rightCutoff = baseAxis.Scale.Max;

            int numberOfDataPoints = Data.Count;
            for (int i = 0; i < numberOfDataPoints; ++i)
            {
                // Check to see if any values are null. If so, then continue.
                double barHeight;
                DataPoint data = GetPointWithGrouping(i, out barHeight);
                if (Numeric.IsNaN(data.X) || Numeric.IsNaN(data.Y))
                    continue;

                #region ----- Horizontal clipping of data values for speed-up -----
                {
                    // We cannot clip the current value because the value could be outside of the
                    // scale, but the bar could still be visible.
                    // Therefore we use the left and the right neighbor to check if the current
                    // value is too far off the scale.
                    if (i > 0)
                    {
                        double xPrevious = GetPointWithGrouping(i - 1).X;
                        double yPrevious = GetPointWithGrouping(i - 1).Y;
                        if (orientation == Orientation.Vertical && xPrevious > rightCutoff
                            || orientation == Orientation.Horizontal && yPrevious > rightCutoff)
                        {
                            continue;
                        }
                    }

                    if (i < numberOfDataPoints - 1)
                    {
                        double xNext = GetPointWithGrouping(i + 1).X;
                        double yNext = GetPointWithGrouping(i + 1).Y;
                        if (orientation == Orientation.Vertical && xNext < leftCutoff
                            || orientation == Orientation.Horizontal && yNext < leftCutoff)
                        {
                            continue;
                        }
                    }
                }
                #endregion

                double basePosition;      // Position on base axis.
                double dataPosition;      // Position on data axis
                double baseLinePosition;  // Get position of zero value base line on data axis.
                if (orientation == Orientation.Vertical)
                {
                    basePosition = xAxis.GetPosition(data.X);
                    dataPosition = yAxis.GetPosition(data.Y);
                    baseLinePosition = yAxis.GetPosition(data.Y - barHeight);
                }
                else
                {
                    basePosition = yAxis.GetPosition(data.Y);
                    dataPosition = xAxis.GetPosition(data.X);
                    baseLinePosition = xAxis.GetPosition(data.X - barHeight);
                }

                if (isClustered)
                {
                    // Clustered drawing of bars:
                    double leftBarPosition = basePosition - (((double)numberOfBarCharts) / 2 - 0.5f) * barWidth - ((double)numberOfBarCharts - 1) / 2 * barWidth * effectiveBarGapWidth;
                    basePosition = leftBarPosition + currentClusterPosition * (barWidth + barWidth * effectiveBarGapWidth);
                }

                // Get bar bounding rectangle.
                Point upperLeftCorner = new Point();
                double width;
                double height;
                if (orientation == Orientation.Vertical)
                {
                    // Base is X.
                    upperLeftCorner.X = basePosition - barWidth / 2;
                    upperLeftCorner.Y = Math.Min(dataPosition, baseLinePosition);
                    width = barWidth;
                    height = Math.Abs(dataPosition - baseLinePosition);
                }
                else
                {
                    // Base is Y.
                    upperLeftCorner.X = Math.Min(dataPosition, baseLinePosition);
                    upperLeftCorner.Y = basePosition - barWidth / 2;
                    width = Math.Abs(dataPosition - baseLinePosition);
                    height = barWidth;
                }

                // Create the bar from the data template.
                var bar = CreateDataPoint(data.DataContext ?? data);
                if (bar == null)
                    return;

                Canvas.SetLeft(bar, upperLeftCorner.X);
                Canvas.SetTop(bar, upperLeftCorner.Y);
                bar.Width = width;
                bar.Height = height;
                bar.Tag = Data[i].Point;

                // Let derived classes change the appearance of the bar.
                OnPrepareBar(i, data, bar);

                Canvas.Children.Add(bar);
            }
        }


        /// <summary>
        /// Can be overwritten in derived classes to customize the appearance of a bar.
        /// </summary>
        /// <param name="index">The index of <paramref name="data"/> in the data source.</param>
        /// <param name="data">The data point.</param>
        /// <param name="bar">The bar.</param>
        protected virtual void OnPrepareBar(int index, DataPoint data, FrameworkElement bar)
        {
            return;
        }


        /// <summary>
        /// Gets the width of the bar.
        /// </summary>
        /// <returns>The width of the bar.</returns>
        private double GetBarWidth()
        {
            Orientation orientation = EffectiveOrientation;
            Axis baseAxis = (orientation == Orientation.Vertical) ? XAxis : YAxis;

            int numberOfBars = Data.Count;

            // Get range from first to last bar.
            double maxValue;
            double minValue;
            if (orientation == Orientation.Vertical)
            {
                maxValue = Data[Data.Count - 1].X;
                minValue = Data[0].X;
            }
            else
            {
                maxValue = Data[Data.Count - 1].Y;
                minValue = Data[0].Y;
            }

            int numberOfBarCharts = GetNumberOfBarCharts();
            double maxPosition = baseAxis.GetPosition(maxValue);
            double minPosition = baseAxis.GetPosition(minValue);

            // The width depends on the number of bars, the grouping and the gap widths.
            var group = Group as BarChartGroup;
            if (group != null && group.Grouping == BarChartGrouping.Clustered && numberOfBarCharts > 1)
            {
                if (numberOfBars == 1)
                    return Math.Abs(baseAxis.GetPosition(1) - baseAxis.GetPosition(0))
                           / (numberOfBarCharts + (numberOfBarCharts - 1) * group.BarGap + group.ClusterGap);
                else
                    return Math.Abs(maxPosition - minPosition) / (numberOfBars - 1)
                           / (numberOfBarCharts + (numberOfBarCharts - 1) * group.BarGap + group.ClusterGap);
            }
            else
            {
                if (numberOfBars == 1)
                    return Math.Abs(baseAxis.GetPosition(1) - baseAxis.GetPosition(0)) / (1 + BarGap);
                else
                    return Math.Abs(maxPosition - minPosition) / (numberOfBars - 1) / (1 + BarGap);
            }
        }


        /// <summary>
        /// Gets the position of this bar chart in the cluster of bar charts. This position is
        /// zero-based.
        /// </summary>
        /// <returns>The cluster position (zero-based index).</returns>
        /// <remarks>
        /// <para>
        /// Normally several bar charts are drawn clustered. This method returns the index of this
        /// bar chart in the cluster of bar charts.
        /// </para>
        /// <para>
        /// Example: If this method returns <c>2</c>, then there are 2 other bars in the cluster
        /// before the bar of this chart.
        /// </para>
        /// </remarks>
        private int GetClusterPosition()
        {
            var group = Group as BarChartGroup;
            if (group == null || group.Grouping != BarChartGrouping.Clustered)
                return 0;

            return group.ItemContainerGenerator.IndexFromContainer(this);
        }


        /// <summary>
        /// Gets the number of bar charts in the current group.
        /// </summary>
        /// <returns>The number of bar charts in the group.</returns>
        private int GetNumberOfBarCharts()
        {
            var group = Group as BarChartGroup;
            if (group == null)
                return 1;     // This is the only bar chart.

            return group.Items.Count;
        }


        /// <summary>
        /// Gets a data point for the specified index. Grouping of charts (stacking) is handled.
        /// </summary>
        /// <param name="index">The index of the data point.</param>
        /// <returns>
        /// The data point. This point includes the offset that is required when bar charts are
        /// clustered or stacked.
        /// </returns>
        private DataPoint GetPointWithGrouping(int index)
        {
            double dummy;
            return GetPointWithGrouping(index, out dummy);
        }


        /// <summary>
        /// Gets a data pair for the specified index. Grouping of charts (stacking) is handled.
        /// </summary>
        /// <param name="index">The index of the data point.</param>
        /// <param name="height">The height (in bar direction).</param>
        /// <returns>
        /// The data point. This point includes the offset that is required when bar charts are
        /// clustered or stacked.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private DataPoint GetPointWithGrouping(int index, out double height)
        {
            Orientation orientation = EffectiveOrientation;
            var group = Group as BarChartGroup;
            if (group == null || group.Grouping == BarChartGrouping.Clustered)
            {
                // Without stacking:
                DataPoint result = Data[index];
                height = (orientation == Orientation.Vertical) ? result.Y : result.X;
                return result;
            }
            else if (group.Grouping == BarChartGrouping.StackedAbsolute || group.Grouping == BarChartGrouping.StackedRelative)
            {
                // Stacking: Add values of all charts before this chart.
                int indexOfThisChart = group.ItemContainerGenerator.IndexFromContainer(this);
                DataPoint point = Data[index];
                height = (orientation == Orientation.Vertical) ? point.Y : point.X;
                for (int i = indexOfThisChart - 1; i >= 0; i--)
                {
                    BarChart chart = group.ItemContainerGenerator.ContainerFromIndex(i) as BarChart;
                    if (chart == null || chart.Data == null)
                        continue;

                    DataPoint point2 = chart.Data[index];
                    if (orientation == Orientation.Vertical && point2.X != point.X
                        || (orientation == Orientation.Horizontal) && point2.Y != point.Y)
                    {
                        throw new ChartDataException("Bar charts are stacked, but the base axis values of the data pairs do not fit together.");
                    }
                    if ((orientation == Orientation.Vertical))
                        point.Y += point2.Y;
                    else
                        point.X += point2.X;
                }

                if (group.Grouping == BarChartGrouping.StackedRelative)
                {
                    // For relative stacking: Add the values of all following stacks.
                    double totalSum = (orientation == Orientation.Vertical) ? point.Y : point.X;

                    // Add y-values of other bar charts.
                    for (int i = indexOfThisChart + 1; i < group.Items.Count; i++)
                    {
                        BarChart chart = group.ItemContainerGenerator.ContainerFromIndex(i) as BarChart;
                        if (chart == null || chart.Data == null)
                            continue;

                        DataPoint point2 = chart.Data[index];
                        if (orientation == Orientation.Vertical && point2.X != point.X
                            || orientation == Orientation.Horizontal && point2.Y != point.Y)
                        {
                            throw new ChartDataException("Bar charts are stacked, but the base axis values of the data pairs do not fit together.");
                        }
                        totalSum += (orientation == Orientation.Vertical) ? point2.Y : point2.X;
                    }
                    if (totalSum != 0)
                    {
                        // Compute data value in percent.
                        if (orientation == Orientation.Vertical)
                            point.Y = point.Y / totalSum * 100.0f;
                        else
                            point.X = point.X / totalSum * 100.0f;
                        height = height / totalSum * 100.0f;
                    }
                }
                return point;
            }
            else
            {
                throw new NotSupportedException("Bar chart does not support current chart grouping setting.");
            }
        }



        /// <inheritdoc/>
        protected override AxisScale OnSuggestXScale()
        {
            if (Data == null || Data.Count == 0)
                return null;

            if (EffectiveOrientation == Orientation.Vertical)
            {
                return SuggestBaseScale();
            }
            else
            {
                // Make sure that scale contains 0.
                AxisScale scale = base.OnSuggestXScale();
                EnsureScaleIncludesZero(scale);
                return scale;
            }
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            if (Data == null || Data.Count == 0)
                return null;

            if (EffectiveOrientation == Orientation.Vertical)
            {
                // Make sure that scale contains 0.
                AxisScale scale = base.OnSuggestYScale();
                EnsureScaleIncludesZero(scale);
                return scale;
            }
            else
            {
                return SuggestBaseScale();
            }
        }


        private static void EnsureScaleIncludesZero(AxisScale scale)
        {
            double min = scale.Min;
            double max = scale.Max;
            if (min > 0 && max > 0)
                scale.Range = new DoubleRange(0, max);
            else if (min < 0 && max < 0)
                scale.Range = new DoubleRange(min, 0);
        }


        /// <summary>
        /// Suggests a scale for the base axis (independent values).
        /// </summary>
        /// <returns>
        /// A suitable <see cref="TextScale"/> with ticks between the text labels.
        /// </returns>
        public AxisScale SuggestBaseScale()
        {
            if (Data == null || Data.Count == 0)
                return null;

            CultureInfo culture = ChartHelper.GetCulture(this);
            Orientation orientation = EffectiveOrientation;
            if (Data.Count == 1)
            {
                DataPoint data = Data[0];
                double value = (orientation == Orientation.Vertical) ? data.X : data.Y;
                TextScale textScale = new TextScale(value - 0.5, value + 0.5);
                textScale.Labels.Add(new TextLabel(value, value.ToString(culture), null));
                textScale.TicksBetweenLabels = true;
                return textScale;
            }
            else
            {
                DataPoint dataN = Data[0];
                DataPoint dataNPlus1 = Data[1];
                DataPoint dataNMinus2 = Data[Data.Count - 2];
                DataPoint dataNMinus1 = Data[Data.Count - 1];
                double valueN;
                double valueNPlus1;
                double valueNMinus2;
                double valueNMinus1;
                if (orientation == Orientation.Vertical)
                {
                    valueN = dataN.X;
                    valueNPlus1 = dataNPlus1.X;
                    valueNMinus2 = dataNMinus2.X;
                    valueNMinus1 = dataNMinus1.X;
                }
                else
                {
                    valueN = dataN.Y;
                    valueNPlus1 = dataNPlus1.Y;
                    valueNMinus2 = dataNMinus2.Y;
                    valueNMinus1 = dataNMinus1.Y;
                }

                double min = valueN - (valueNPlus1 - valueN) / 2;
                double max = valueNMinus1 + (valueNMinus1 - valueNMinus2) / 2;
                TextScale textScale = new TextScale(min, max)
                {
                    TicksBetweenLabels = true
                };
                for (int i = 0; i < Data.Count; i++)
                {
                    DataPoint data = Data[i];
                    double value = (orientation == Orientation.Vertical) ? data.X : data.Y;
                    textScale.Labels.Add(new TextLabel(value, value.ToString(culture), null));
                }
                return textScale;
            }
        }


        /// <inheritdoc/>
        public override void ValidateData()
        {
            // Check DataSource
            if (DataSource == null)
                return;

            // Check for NaN values.
            for (int i = 0; i < Data.Count; i++)
                if (Numeric.IsNaN(Data[i].X) || Numeric.IsNaN(Data[i].Y))
                    throw new ChartDataException("Data with NaN values is not supported for bar charts.");

            // Check if base values are ascending.
            Orientation orientation = EffectiveOrientation;
            for (int i = 1; i < Data.Count; i++)
            {
                {
                    if (orientation == Orientation.Vertical && Data[i - 1].X >= Data[i].X
                      || orientation == Orientation.Horizontal && Data[i - 1].Y >= Data[i].Y)
                    {
                        throw new ChartDataException("The data values for the base axis must be sorted (ascending)!");
                    }
                }
            }
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            if (DataPointTemplate != null)
            {
                var grid = new Grid
                {
                    MinWidth = 16,
                    MinHeight = 16,
                };

                var legendSymbol = CreateDataPoint(null);
                if (legendSymbol != null)
                {
                    if (EffectiveOrientation == Orientation.Vertical)
                    {
                        legendSymbol.Width = 10;
                        legendSymbol.Height = 14;
                        legendSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                        legendSymbol.VerticalAlignment = VerticalAlignment.Bottom;
                    }
                    else
                    {
                        legendSymbol.Width = 14;
                        legendSymbol.Height = 10;
                        legendSymbol.HorizontalAlignment = HorizontalAlignment.Left;
                        legendSymbol.VerticalAlignment = VerticalAlignment.Center;
                    }

                    legendSymbol.IsHitTestVisible = false;
                    grid.Children.Add(legendSymbol);
                }

                return grid;
            }

            return base.OnGetLegendSymbol();
        }
        #endregion
    }
}
