// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a 1D heat chart that represents data using a color gradient.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The y values of the data source are used to index a <see cref="Palette"/>. The palette
    /// should contain an entry for each y value. There are two palettes: <see cref="FillPalette"/>
    /// and <see cref="StrokePalette"/>. The <see cref="FillPalette"/> is used to fill the
    /// heat chart whereas the <see cref="StrokePalette"/> is used to color the outline of the
    /// heat chart. Both palette are optional. In typical cases only the
    /// <see cref="FillPalette"/> will be set.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "DataPointStyle", StyleTargetType = typeof(Rectangle))]
    public class HeatChart : Chart
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the width of the bar.
        /// </summary>
        private double BarWidth
        {
            get
            {
                var yAxis = YAxis;
                var group = Group as HeatChartGroup;
                if (group != null)
                {
                    double barGapWidth = Math.Max(group.BarGap, 0);
                    return Math.Abs(yAxis.GetPosition(0) - yAxis.GetPosition(1)) / (1 + barGapWidth);
                }

                return Math.Abs(yAxis.GetPosition(0) - yAxis.GetPosition(1));
            }
        }


        private Palette EffectiveFillPalette
        {
            get
            {
                var group = Group as HeatChartGroup;
                if (group != null && group.FillPalette != null)
                    return group.FillPalette;

                return FillPalette;
            }
        }


        private Palette EffectiveStrokePalette
        {
            get
            {
                var group = Group as HeatChartGroup;
                if (group != null && group.StrokePalette != null)
                    return group.StrokePalette;

                return StrokePalette;
            }
        }


        /// <summary>
        /// Gets the y value of this heat chart. The y value is the index of this bar in the group.
        /// </summary>
        private double Y
        {
            get
            {
                var group = Group as HeatChartGroup;
                if (group == null)
                    return 0;

                return group.ItemContainerGenerator.IndexFromContainer(this);
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="FillMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillModeProperty = DependencyProperty.Register(
            "FillMode",
            typeof(BarFillMode),
            typeof(HeatChart),
            new PropertyMetadata(BarFillMode.Solid, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the fill mode applied to the heat chart.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the fill mode applied to the heat chart.")]
        [Category(ChartCategories.Default)]
        public BarFillMode FillMode
        {
            get { return (BarFillMode)GetValue(FillModeProperty); }
            set { SetValue(FillModeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="FillPalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillPaletteProperty = DependencyProperty.Register(
            "FillPalette",
            typeof(Palette),
            typeof(HeatChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette for used to fill the heat chart.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette for used to fill the heat chart.")]
        [Category(ChartCategories.Default)]
        public Palette FillPalette
        {
            get { return (Palette)GetValue(FillPaletteProperty); }
            set { SetValue(FillPaletteProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StrokePalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokePaletteProperty = DependencyProperty.Register(
            "StrokePalette",
            typeof(Palette),
            typeof(HeatChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette used to fill the outline of the heat chart.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette used to fill the outline of the heat chart.")]
        [Category(ChartCategories.Default)]
        public Palette StrokePalette
        {
            get { return (Palette)GetValue(StrokePaletteProperty); }
            set { SetValue(StrokePaletteProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="HeatChart"/> class.
        /// </summary>
        public HeatChart()
        {
            DefaultStyleKey = typeof(HeatChart);
        }
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HeatChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HeatChart), new FrameworkPropertyMetadata(typeof(HeatChart)));
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected override void OnUpdate()
        {
            base.OnUpdate(); // Updates the data source, if required.

            Debug.Assert(Canvas.Children.Count == 0, "Canvas should be cleared in base class.");

            if (Data == null || Data.Count == 0 || DataPointTemplate == null)
            {
                // A relevant property is not (yet) set.
                return;
            }

            Axis xAxis = XAxis;
            Axis yAxis = YAxis;

            double y = Y;
            double barWidth = BarWidth;

            // Loop over data pairs and draw filled rectangles.
            int numberOfDataPoints = Data.Count;
            for (int i = 0; i < numberOfDataPoints; ++i)
            {
                // check to see if any values null. If so, then continue.
                DataPoint data = Data[i];
                DataPoint nextData = (i < (numberOfDataPoints - 1))
                                       ? Data[i + 1]
                                       : new DataPoint(xAxis.Scale.Max, data.Y, null);

                if (nextData.X < xAxis.Scale.Min || data.X >= xAxis.Scale.Max)
                    continue;

                // Get brushes from the palette.
                Brush strokeBrush, fillBrush;
                GetBrushesFromPalette(data.Y, nextData.Y, out strokeBrush, out fillBrush);

                double xPosition = xAxis.GetPosition(data.X);
                double nextXPosition = xAxis.GetPosition(nextData.X);
                double yPosition = yAxis.GetPosition(y);

                var element = CreateDataPoint(data.DataContext ?? data) ?? new Rectangle();
                element.Width = Math.Abs(nextXPosition - xPosition);
                element.Height = barWidth;
                element.Style = DataPointStyle;
                element.Tag = data.Point;
                if (element is Shape)
                {
                    var shape = (Shape)element;
                    shape.Fill = fillBrush;
                    shape.Stroke = strokeBrush;
                }
                Canvas.SetLeft(element, xPosition);
                Canvas.SetTop(element, yPosition - barWidth / 2);
                Canvas.Children.Add(element);
            }
        }


        private void GetBrushesFromPalette(double currentValue, double nextValue, out Brush strokeBrush, out Brush fillBrush)
        {
            // Set default values.
            strokeBrush = BorderBrush;
            fillBrush = Background;

            Palette fillPalette = EffectiveFillPalette;
            Palette strokePalette = EffectiveStrokePalette;
            if (FillMode == BarFillMode.Solid)
            {
                // Solid:
                // Get a solid color brush for fill and stroke.
                if (fillPalette != null && fillPalette.Count > 0)
                    fillBrush = new SolidColorBrush(fillPalette.GetColor(currentValue));

                if (strokePalette != null && strokePalette.Count > 0)
                    strokeBrush = new SolidColorBrush(strokePalette.GetColor(nextValue));
            }
            else
            {
                // Gradient:
                // Define end point for gradient direction.
                Point startPoint = new Point(0, 0);
                Point endPoint = new Point(1, 0);
                Axis xAxis = XAxis;
                if (xAxis.Scale.Reversed)
                    ChartHelper.Swap(ref startPoint, ref endPoint);

                // Get brush for fill and stroke.
                if (fillPalette != null && fillPalette.Count > 0)
                {
                    fillBrush = new LinearGradientBrush
                    {
                        GradientStops = fillPalette.GetGradient(currentValue, nextValue),
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        ColorInterpolationMode = fillPalette.ColorInterpolationMode
                    };
                }

                if (strokePalette != null && strokePalette.Count > 0)
                {
                    strokeBrush = new LinearGradientBrush
                    {
                        GradientStops = strokePalette.GetGradient(currentValue, nextValue),
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        ColorInterpolationMode = strokePalette.ColorInterpolationMode
                    };
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

                var legendSymbol = CreateDataPoint(null) ?? new Rectangle();
                legendSymbol.Width = 16;
                legendSymbol.Height = 10;
                legendSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                legendSymbol.VerticalAlignment = VerticalAlignment.Center;
                legendSymbol.SetBinding(StyleProperty, new Binding("DataPointStyle") { Source = this });
                legendSymbol.IsHitTestVisible = false;

                Brush fillBrush = null;
                Brush strokeBrush = null;

                Palette fillPalette = EffectiveFillPalette;
                if (fillPalette != null && fillPalette.Count > 0)
                {
                    double start = fillPalette[0].Value;
                    double end = fillPalette[fillPalette.Count - 1].Value;
                    fillBrush = new LinearGradientBrush
                    {
                        GradientStops = fillPalette.GetGradient(start, end),
                        StartPoint = new Point(0, 0.5),
                        EndPoint = new Point(1, 0.5),
                        ColorInterpolationMode = fillPalette.ColorInterpolationMode
                    };
                }

                Palette strokePalette = EffectiveStrokePalette;
                if (strokePalette != null && strokePalette.Count > 0)
                {
                    double start = strokePalette[0].Value;
                    double end = strokePalette[strokePalette.Count - 1].Value;
                    strokeBrush = new LinearGradientBrush
                    {
                        GradientStops = strokePalette.GetGradient(start, end),
                        StartPoint = new Point(0, 0.5),
                        EndPoint = new Point(1, 0.5),
                        ColorInterpolationMode = strokePalette.ColorInterpolationMode
                    };
                }

                ChartHelper.SetStrokeAndFill(legendSymbol, strokeBrush, fillBrush);

                grid.Children.Add(legendSymbol);
                return grid;
            }

            return base.OnGetLegendSymbol();
        }


        /// <inheritdoc/>
        protected override AxisScale OnSuggestYScale()
        {
            TextScale scale = new TextScale(-0.5, 0.5);
            scale.Labels.Add(new TextLabel(0, Title));
            return scale;
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
                    throw new ChartDataException("Data with NaN values is not supported for heat charts.");
        }
        #endregion
    }
}
