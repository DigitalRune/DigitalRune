// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Collections;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a pie chart.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Layout:</strong><br/>
    /// Similar to other chart types, a pie chart needs to be placed inside a
    /// <see cref="ChartPanel"/>. The properties <see cref="CenterX"/> and <see cref="CenterY"/>
    /// define center of the pie chart relative to the chart area. The values are relative to the
    /// length of the <see cref="ChartElement.XAxis"/> and <see cref="ChartElement.YAxis"/> and must
    /// be in the range [0, 1]. The value can be <see cref="double.NaN"/> (default) to center the
    /// pie chart inside the chart panel where the <see cref="Control.Padding"/> defines the space
    /// between the axes and the pie chart. The outer labels are drawn within this space.
    /// </para>
    /// <para>
    /// <strong>DataPointTemplate &amp; DataPointStyle:</strong><br/>
    /// The <see cref="Chart.DataPointTemplate"/> needs to be a data template containing a
    /// <see cref="PieChartItem"/>. Likewise, <see cref="Chart.DataPointStyle"/> needs to be a style
    /// for a <see cref="PieChartItem"/>. A <see cref="PieChartItem"/> renders a sector of the pie
    /// including inner and outer labels. <see cref="PieChartItem"/>s can be customized by setting
    /// a new <see cref="Chart.DataPointTemplate"/> and/or a new <see cref="Chart.DataPointStyle"/>.
    /// </para>
    /// <para>
    /// <strong>Doughnut Charts:</strong><br/>
    /// A <i>doughnut chart</i> is a variant of the pie chart with a hole in the center. It can be
    /// created by setting the property <see cref="Hole"/>. The value defines the size of the hole
    /// relative to the pie chart.
    /// </para>
    /// <para>
    /// <strong>Exploded Pie Charts:</strong><br/>
    /// An <i>exploded pie chart</i> is a pie chart where one or more sectors are separated from the
    /// rest of the disk. This can be useful to highlight a specific part of the pie chart. An
    /// exploded pie chart can be created by setting the <see cref="PieChartItem.Offset"/> property
    /// of a <see cref="PieChartItem"/>. This can be done in code, or by using data binding.
    /// </para>
    /// </remarks>
    public class PieChart : Chart
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly WeakCollection<Canvas> _legendSymbols = new WeakCollection<Canvas>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="CenterX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterXProperty = DependencyProperty.Register(
            "CenterX",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(double.NaN, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the horizontal center of the pie chart relative to the chart area.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The horizontal center of the pie chart on relative to the chart area. The value must be
        /// in the range [0, 1]. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// <remarks>
        /// If the value is <see cref="double.NaN"/> the pie chart is centered inside the chart
        /// area, taking the padding into account. The padding defines the space were outer labels
        /// will be drawn.
        /// </remarks>
        [Description("Gets or sets the horizontal center of the pie chart relative to the chart area.")]
        [Category(Categories.Layout)]
        public double CenterX
        {
            get { return (double)GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CenterY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CenterYProperty = DependencyProperty.Register(
            "CenterY",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(Boxed.DoubleNaN, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the vertical center of the pie chart relative to the chart area.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The vertical center of the pie chart relative to the chart area. The value must be in
        /// the range [0, 1]. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// <inheritdoc cref="CenterX"/>
        [Description("Gets or sets the vertical center of the pie chart relative to the chart area.")]
        [Category(Categories.Layout)]
        public double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Radius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
            "Radius",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(Boxed.DoubleNaN, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the radius of the pie chart in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The radius of the pie chart in device-independent pixels. The default value is 
        /// <see cref="double.NaN"/>.
        /// </value>
        [Description("Gets or sets the radius of the pie chart elements.")]
        [Category(Categories.Layout)]
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Hole"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HoleProperty = DependencyProperty.Register(
            "Hole",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(Boxed.DoubleZero, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the hole radius relative to the outer radius of the pie chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The hole radius relative to the outer radius of the pie chart. For example, a value of
        /// 0.5 indicates that the hole radius is 50% of the outer radius. The default value is 0.
        /// </value>
        [Description("Gets or sets the inner radius of the pie chart elements.")]
        [Category(Categories.Layout)]
        public double Hole
        {
            get { return (double)GetValue(HoleProperty); }
            set { SetValue(HoleProperty, value); }
        }


#if SILVERLIGHT
        /// <summary>
        /// Identifies the <see cref="ActualHoleRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActualHoleRadiusProperty = DependencyProperty.Register(
            "ActualHoleRadius",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(Boxed.DoubleZero));

        /// <summary>
        /// Gets or sets the actual radius of the hole (read-only).
        /// This is a dependency property.
        /// </summary>
        /// <value>The actual radius of the hole (read-only).</value>
        [Browsable(false)]
        public double ActualHoleRadius
        {
            get { return (double)GetValue(ActualHoleRadiusProperty); }
            set { SetValue(ActualHoleRadiusProperty, value); }
        }
#else
        private static readonly DependencyPropertyKey ActualHoleRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
            "ActualHoleRadius",
            typeof(double),
            typeof(PieChart),
            new PropertyMetadata(Boxed.DoubleZero));

        /// <summary>
        /// Identifies the <see cref="ActualHoleRadius"/> dependency property.
        /// </summary>    
        public static readonly DependencyProperty ActualHoleRadiusProperty = ActualHoleRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the actual radius of the hole in device-independent pixels (read-only).
        /// This is a dependency property.
        /// </summary>
        /// <value>The actual radius of the hole  in device-independent pixels (read-only).</value>
        [Browsable(false)]
        public double ActualHoleRadius
        {
            get { return (double)GetValue(ActualHoleRadiusProperty); }
            private set { SetValue(ActualHoleRadiusPropertyKey, value); }
        }
#endif


        /// <summary>
        /// Identifies the <see cref="BrushSelector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushSelectorProperty = DependencyProperty.Register(
            "BrushSelector",
            typeof(IBrushSelector),
            typeof(PieChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the selector that provides the colors for the sectors of the pie chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>The selector that provides the colors for the sectors of the pie chart.</value>
        [Description("Gets or sets the selector that provides the colors for the sectors of the pie chart.")]
        [Category(Categories.Appearance)]
        public IBrushSelector BrushSelector
        {
            get { return (IBrushSelector)GetValue(BrushSelectorProperty); }
            set { SetValue(BrushSelectorProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="PieChart"/> class.
        /// </summary>
        public PieChart()
        {
            DefaultStyleKey = typeof(PieChart);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="PieChart"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PieChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieChart), new FrameworkPropertyMetadata(typeof(PieChart)));
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


        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            base.OnUpdate();

            Debug.Assert(Canvas.Children.Count == 0, "Canvas should be cleared in base class.");

            if (Data == null || Data.Count == 0 || DataPointTemplate == null)
            {
                // A relevant property is not set.
                return;
            }

            // Fetch dependency properties. (Accessing dependency properties is expensive.)
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            AxisScale xScale = xAxis.Scale;
            AxisScale yScale = yAxis.Scale;

            double centerX = CenterX;
            double centerY = CenterY;
            double hole = Hole;
            double outerRadius = Radius;
            Thickness padding = Padding;

            if (hole < 0 || hole > 1)
                throw new ChartException("Pie chart has invalid hole size. The hole size is relative to the outer radius and must be in the range [0, 1].");
            if (outerRadius < 0)
                throw new ChartException("Pie chart has invalid radius. The radius must be a equal to or greater than 0.");

            // Determine available space.
            double left = xAxis.GetPosition(xScale.Min);
            double right = xAxis.GetPosition(xScale.Max);
            if (left > right)
            {
                // Scale is reversed.
                ChartHelper.Swap(ref left, ref right);
            }

            double top = yAxis.GetPosition(yScale.Max);
            double bottom = yAxis.GetPosition(yScale.Min);
            if (top > bottom)
            {
                // Scale is reversed.
                ChartHelper.Swap(ref top, ref bottom);
            }

            // Apply padding.
            left += padding.Left;
            right -= padding.Right;
            top += padding.Top;
            bottom -= padding.Bottom;

            if (Numeric.IsNaN(centerX))
            {
                // Center pie chart horizontally.
                centerX = (left + right) / 2;
            }
            else
            {
                centerX = xAxis.OriginX + centerX * xAxis.Length;
            }

            if (Numeric.IsNaN(centerY))
            {
                // Center pie chart vertically.
                centerY = (top + bottom) / 2;
            }
            else
            {
                centerY = yAxis.OriginY - (1.0 - centerY) * yAxis.Length;
            }

            if (Numeric.IsNaN(outerRadius))
            {
                // Fit pie chart into available space.
                double radiusLeft = centerX - left;
                double radiusRight = right - centerX;
                double radiusTop = centerY - top;
                double radiusBottom = bottom - centerY;

                outerRadius = radiusLeft;
                if (outerRadius > radiusRight)
                    outerRadius = radiusRight;
                if (outerRadius > radiusTop)
                    outerRadius = radiusTop;
                if (outerRadius > radiusBottom)
                    outerRadius = radiusBottom;
            }

            if (Numeric.IsNaN(hole))
                hole = 0;

            double innerRadius = hole * outerRadius;
            ActualHoleRadius = innerRadius;

            // Draw pie chart inside chart panel.
            DrawPieChart(Canvas, centerX, centerY, innerRadius, outerRadius, BrushSelector, false);

            // Update legend symbols inside legends.
            // Use a fixed innerRadius
            innerRadius = (Hole > 0) ? 2 : 0;
            foreach (var legendSymbol in _legendSymbols)
                DrawPieChart(legendSymbol, 8, 8, innerRadius, 8, BrushSelector, true);
        }


        private void DrawPieChart(Canvas canvas, double centerX, double centerY, double innerRadius, double outerRadius, IBrushSelector brushSelector, bool isLegendSymbol)
        {
            canvas.Children.Clear();

            // Add pie chart sectors and add up data values.
            double sum = 0;
            for (int i = 0; i < Data.Count; ++i)
            {
                DataPoint data = Data[i];

                // Create pie chart sector.
                var element = CreateDataPoint(data.DataContext ?? data) as PieChartItem;
                if (element == null)
                {
                    // Unsupported DataPointTemplate. Do not draw anything.
                    return;
                }

                element.CenterX = centerX;
                element.CenterY = centerY;
                element.InnerRadius = innerRadius;
                element.OuterRadius = outerRadius;

                if (brushSelector != null)
                {
                    var brush = brushSelector.SelectBrush(data.DataContext, i);
                    if (brush != null)
                        element.Background = brush;
                }

                if (isLegendSymbol)
                {
                    // Override dependency properties which might be set using data binding
                    // or styles.
                    element.InnerLabel = null;
                    element.OuterLabel = null;
                    element.Offset = 0;

                    element.IsHitTestVisible = false;
                }

                canvas.Children.Add(element);

                sum += data.Y;
            }

            // Set start and end angles of sectors.
            double startAngle = 0;
            double valueToAngle = 1 / sum * 2.0 * Math.PI;
            for (int i = 0; i < Data.Count; ++i)
            {
                double angle = Data[i].Y * valueToAngle;
                double endAngle = startAngle + angle;

                var element = (PieChartItem)canvas.Children[i];
                element.StartAngle = startAngle;
                element.EndAngle = endAngle;

                if (Numeric.IsZero(angle))
                    element.Visibility = Visibility.Collapsed;

                startAngle = endAngle;
            }
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            // The symbol for the normal legend is miniature version of the pie chart.
            // The legend symbol is automatically updated in OnUpdate()!
            var canvas = new Canvas
            {
                Width = 16,
                Height = 16,
            };

            // Store legend symbol using weak reference. 
            // (The symbol is drawn in OnUpdate().)
            _legendSymbols.Add(canvas);

            return canvas;
        }


        /// <summary>
        /// Gets the pie chart sectors as symbols for the <see cref="PieChartLegend"/>.
        /// </summary>
        /// <returns>
        /// The pie chart sectors as symbols for the <see cref="PieChartLegend"/>.
        /// </returns>
        /// <remarks>
        /// The data context of the <see cref="PieChartItem"/> hold the data item.
        /// </remarks>
        internal IEnumerable<Canvas> GetPieChartLegendSymbols()
        {
            if (DataSource == null)
                yield break;

            // Use a fixed inner radius if pie chart has a hole.
            double innerRadius = (Hole > 0) ? 6 : 0;

            int index = 0;
            var brushSelector = BrushSelector;
            foreach (object data in DataSource)
            {
                var canvas = new Canvas
                {
                    Width = 16,
                    Height = 16,
                    DataContext = data,
                };

                var element = CreateDataPoint(null) as PieChartItem;
                if (element != null)
                {
                    element.CenterX = 0;
                    element.CenterY = 14;
                    element.InnerRadius = innerRadius;
                    element.OuterRadius = 15;

                    if (brushSelector != null)
                    {
                        var brush = brushSelector.SelectBrush(data, index);
                        if (brush != null)
                            element.Background = brush;
                    }

                    // Override dependency properties which might be set using data binding
                    // or styles.
                    element.Offset = 0;
                    element.InnerLabel = null;
                    element.OuterLabel = null;

                    element.IsHitTestVisible = false;

                    canvas.Children.Add(element);
                }

                index++;
                yield return canvas;
            }
        }


        /// <inheritdoc/>
        public override void ValidateData()
        {
            if (DataSource == null)
                return;

            for (int i = 0; i < Data.Count; i++)
            {
                if (Numeric.IsNaN(Data[i].X) || Numeric.IsNaN(Data[i].Y))
                    throw new ChartDataException("Data with NaN values is not supported for pie charts.");

                if (Data[i].Y < 0)
                    throw new ChartDataException("Pie charts do not support data with negative values.");
            }
        }
        #endregion
    }
}
