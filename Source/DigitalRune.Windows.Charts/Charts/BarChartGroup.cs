// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Groups multiple <see cref="BarChart"/>s into a single chart.
    /// </summary>
    /// <remarks>
    /// The <see cref="BarChartGroup"/> is used to draw several bar charts as one chart. The charts
    /// are either clustered or stacked (see <see cref="Grouping"/> mode).
    /// </remarks>
    public class BarChartGroup : ChartGroup<BarChart>
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
        /// Identifies the <see cref="BarGap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BarGapProperty = DependencyProperty.Register(
            "BarGap",
            typeof(double),
            typeof(BarChartGroup),
            new PropertyMetadata(0.2, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the width of the gap between two bars relative to the bar width.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The width of the gap between to bars relative to the bar width. The default value is 0.2.
        /// </value>
        /// <remarks>
        /// Examples: 
        /// 0 means no gap.
        /// 1 means the gap width is equal to the bar width.
        /// 2 means the gap width is twice the bar width.
        /// </remarks>
        /// <seealso cref="ClusterGap"/>
        [Description("Gets or sets the width of the gap between two bars in relation to the bar width.")]
        [Category(ChartCategories.Default)]
        public double BarGap
        {
            get { return (double)GetValue(BarGapProperty); }
            set { SetValue(BarGapProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ClusterGap"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClusterGapProperty = DependencyProperty.Register(
            "ClusterGap",
            typeof(double),
            typeof(BarChartGroup),
            new PropertyMetadata(0.75, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the width of the gap between two bar clusters relative to the bar width.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The width of the gap between to bars clusters relative to the bar width. The default
        /// value is 0.75.
        /// </value>
        /// <remarks>
        /// <para>
        /// Examples: 
        /// 0 means no gap between clusters.
        /// 1 means the cluster gap width is equal to the bar width.
        /// 2 means the cluster gap width is twice the bar width.</para>
        /// </remarks>
        /// <seealso cref="BarGap"/>
        [Description("Gets or sets the width of the gap between two bar clusters in relation to the bar width.")]
        [Category(ChartCategories.Default)]
        public double ClusterGap
        {
            get { return (double)GetValue(ClusterGapProperty); }
            set { SetValue(ClusterGapProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Grouping"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GroupingProperty = DependencyProperty.Register(
            "Grouping",
            typeof(BarChartGrouping),
            typeof(BarChartGroup),
            new PropertyMetadata(BarChartGrouping.Clustered, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value describing how the bar charts are grouped or stacked.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The type of bar chart grouping. The default value is
        /// <see cref="BarChartGrouping.Clustered"/>.
        /// </value>
        [Description("Gets or sets a value describing how the bar charts are grouped or stacked.")]
        [Category(ChartCategories.Default)]
        public BarChartGrouping Grouping
        {
            get { return (BarChartGrouping)GetValue(GroupingProperty); }
            set { SetValue(GroupingProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(BarChartGroup),
            new PropertyMetadata(Orientation.Vertical, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation of the bars. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/> if the x-axis is the base
        /// axis and y-axis shows the data values; otherwise,
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/> if the y-axis is the base
        /// axis and the x-axis shows the data values. The default value is
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
        /// Initializes a new instance of the <see cref="BarChartGroup"/> class.
        /// </summary>
        public BarChartGroup()
        {
            DefaultStyleKey = typeof(BarChartGroup);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="BarChartGroup"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static BarChartGroup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BarChartGroup), new FrameworkPropertyMetadata(typeof(BarChartGroup)));
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


        /// <summary>
        /// Raises the <see cref="ChartElement.Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            // Set zIndex of children for stacked bars. The first child should be in front of all
            // others. The last child should be behind. Otherwise parts of the lower chart will be
            // hidden by charts which are stacked on top.
            if (Grouping == BarChartGrouping.StackedAbsolute || Grouping == BarChartGrouping.StackedRelative)
            {
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
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestXScale()
        {
            return (Orientation == Orientation.Vertical) ? SuggestBaseScale() : SuggestDataScale();
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            return (Orientation == Orientation.Vertical) ? SuggestDataScale() : SuggestBaseScale();
        }


        /// <summary>
        /// Suggests a scale for the base axis (independent values).
        /// </summary>
        /// <returns>A suitable <see cref="TextScale"/> with ticks between the text labels.</returns>
        public AxisScale SuggestBaseScale()
        {
            // Base values must be identical for all charts in the group.
            // We let the first chart suggest the base scale.
            foreach (var chart in Charts)
                return ((BarChart)chart).SuggestBaseScale();

            return null;
        }


        /// <summary>
        /// Suggests a scale for the data axis (dependent values).
        /// </summary>
        /// <returns>A scale for the data axis (dependent values).</returns>
        public AxisScale SuggestDataScale()
        {
            // For relative stacked charts we use a linear scale with 0 to 100%.
            if (Grouping == BarChartGrouping.StackedRelative)
            {
                return new LinearScale(0, 100);
            }

            if (Grouping == BarChartGrouping.Clustered)
            {
                return (Orientation == Orientation.Vertical) ? base.OnSuggestYScale() : base.OnSuggestXScale();
            }

            Debug.Assert(Grouping == BarChartGrouping.StackedAbsolute);

            var charts = Charts.Cast<BarChart>().ToArray();

            // For absolute stacked charts we add the chart values and find the min
            // and max values.
            double min = 0;
            double max = 0;

            // Get data source of first chart.
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
                    var chart = charts[chartIndex];
                    if (chart.Data == null || chart.Data.Count <= i)
                        continue;

                    DataPoint data = chart.Data[i];
                    double value = (Orientation == Orientation.Vertical) ? data.Y : data.X;
                    if (Numeric.IsNaN(value))
                        continue;

                    sum += value;
                }

                if (sum < min)
                    min = sum;
                else if (sum > max)
                    max = sum;
            }

            if (Numeric.AreEqual(min, max))
                return new LinearScale(min - 0.5, max + 0.5); // Return a scale with range > 0.

            return new LinearScale(min, max);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public override void ValidateData()
        {
            if (Items.Count == 0)
                return;

            var charts = Charts.Cast<BarChart>().ToArray();

            // Check if all children are bar charts and if all children have a valid data source.
            foreach (var child in charts)
            {
                var chart = child;
                if (chart.Data == null)
                    throw new ChartException("Charts of a BarChartGroup must have a valid DataSource.");
            }

            // Check if all values are positive or all are negative for stacked bar charts.
            if (Grouping == BarChartGrouping.StackedAbsolute || Grouping == BarChartGrouping.StackedRelative)
            {
                bool? isPositive = null;
                foreach (var chart in charts)
                {
                    if (chart.Data == null)
                        continue;

                    for (int i = 0; i < chart.Data.Count; i++)
                    {
                        if (isPositive.HasValue == false && Numeric.AreEqual(chart.Data[i].Y, 0) == false)
                            isPositive = chart.Data[i].Y > 0;
                        if (isPositive.HasValue)
                        {
                            if ((isPositive.Value && chart.Data[i].Y < 0)
                                || (isPositive.Value == false && chart.Data[i].Y > 0))
                            {
                                throw new ChartException("Mixing of positive and negative data values is not allowed for stacked bar charts.");
                            }
                        }
                    }
                }
            }

            // Check if the charts have the same number of data values.
            int numberOfValues = charts[0].Data.Count;
            for (int i = 1; i < charts.Length; i++)
            {
                if (charts[i].Data.Count != numberOfValues)
                    throw new ChartException("Stacked charts must have the same number of data values.");
            }

            // Check if the charts have the same x data values.
            for (int i = 0; i < charts[0].Data.Count; i++)
            {
                double baseValue = (Orientation == Orientation.Vertical) ? charts[0].Data[i].X : charts[0].Data[i].Y;
                for (int chartIndex = 1; chartIndex < charts.Length; chartIndex++)
                {
                    if (Orientation == Orientation.Vertical && charts[chartIndex].Data[i].X != baseValue
                        || Orientation == Orientation.Horizontal && charts[chartIndex].Data[i].Y != baseValue)
                    {
                        throw new ChartException("The base values of data of stacked bar charts must fit together.");
                    }
                }
            }
        }
        #endregion
    }
}
