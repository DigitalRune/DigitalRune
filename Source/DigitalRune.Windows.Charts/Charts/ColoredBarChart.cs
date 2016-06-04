// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a bar chart where the stroke and fill of the bars is defined in
    /// <see cref="Palette"/> s.
    /// </summary>
    /// <remarks>
    /// The <see cref="Chart.DataPointTemplate"/> which is the template for each bar should be of
    /// type <see cref="Control"/> of <see cref="Shape"/>. When the bar is a <see cref="Control"/>
    /// then the properties <see cref="Control.Background"/> and <see cref="Control.BorderBrush"/>
    /// will be set to a color gradient as defined by <see cref="FillPalette"/> and
    /// <see cref="StrokePalette"/>. When the bar is of type <see cref="Shape"/> the properties
    /// <see cref="Shape.Fill"/> and <see cref="Shape.Stroke"/> will be overwritten.
    /// </remarks>
    public class ColoredBarChart : BarChart
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
        /// Identifies the <see cref="FillMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillModeProperty = DependencyProperty.Register(
            "FillMode",
            typeof(BarFillMode),
            typeof(ColoredBarChart),
            new PropertyMetadata(BarFillMode.Gradient, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the fill mode applied to the bars.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the fill mode applied to the bars.")]
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
            typeof(ColoredBarChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette for used to fill the bars.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette for used to fill the bars.")]
        [Category(ChartCategories.Default)]
        public Palette FillPalette
        {
            get { return (Palette)GetValue(FillPaletteProperty); }
            set { SetValue(FillPaletteProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="PaletteIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaletteIndexProperty = DependencyProperty.Register(
            "PaletteIndex",
            typeof(PaletteIndex),
            typeof(ColoredBarChart),
            new PropertyMetadata(PaletteIndex.YValue, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets how the palettes should be indexed.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets how the palettes should be indexed.")]
        [Category(ChartCategories.Default)]
        public PaletteIndex PaletteIndex
        {
            get { return (PaletteIndex)GetValue(PaletteIndexProperty); }
            set { SetValue(PaletteIndexProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StrokePalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokePaletteProperty = DependencyProperty.Register(
            "StrokePalette",
            typeof(Palette),
            typeof(ColoredBarChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette used to fill the outline of the bars.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette used to fill the outline of the bars.")]
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
        /// Initializes a new instance of the <see cref="ColoredBarChart"/> class.
        /// </summary>
        public ColoredBarChart()
        {
            DefaultStyleKey = typeof(ColoredBarChart);
        }
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ColoredBarChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColoredBarChart), new FrameworkPropertyMetadata(typeof(ColoredBarChart)));
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
        /// Can be overwritten in derived classes to customize the appearance of a bar.
        /// </summary>
        /// <param name="index">The index of <paramref name="data"/> in the data source.</param>
        /// <param name="data">The data point.</param>
        /// <param name="bar">The bar.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PaletteIndex")]
        protected override void OnPrepareBar(int index, DataPoint data, FrameworkElement bar)
        {
            double paletteIndex;
            switch (PaletteIndex)
            {
                case PaletteIndex.Index:
                    paletteIndex = index;
                    break;
                case PaletteIndex.XValue:
                    paletteIndex = data.X;
                    break;
                case PaletteIndex.YValue:
                    paletteIndex = data.Y;
                    break;
                default:
                    throw new NotSupportedException("PaletteIndex type is not supported.");
            }

            Brush fillBrush;
            Brush strokeBrush;
            GetBrushes(paletteIndex, out strokeBrush, out fillBrush);
            ChartHelper.SetStrokeAndFill(bar, strokeBrush, fillBrush);
        }


        private void GetBrushes(double paletteIndex, out Brush strokeBrush, out Brush fillBrush)
        {
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;

            strokeBrush = null;
            fillBrush = null;

            if (FillMode == BarFillMode.Solid)
            {
                // Solid:
                // Get a solid color brush for fill and stroke.
                if (FillPalette != null && FillPalette.Count > 0)
                    fillBrush = new SolidColorBrush(FillPalette.GetColor(paletteIndex));

                if (StrokePalette != null && StrokePalette.Count > 0)
                    strokeBrush = new SolidColorBrush(StrokePalette.GetColor(paletteIndex));
            }
            else
            {
                // Gradient:
                // We draw from 0 upwards or from a negative value upwards to 0.
                double lower = Math.Min(0, paletteIndex);
                double higher = Math.Max(0, paletteIndex);

                // Define end point for gradient direction.
                Point startPoint;
                Point endPoint;
                if (PaletteIndex == PaletteIndex.XValue)
                {
                    // Horizontal
                    startPoint = new Point(0, 0);
                    endPoint = new Point(1, 0);
                    if (xAxis != null && xAxis.Scale != null && xAxis.Scale.Reversed)
                        ChartHelper.Swap(ref startPoint, ref endPoint);
                }
                else
                {
                    // Vertical
                    startPoint = new Point(0, 1);
                    endPoint = new Point(0, 0);
                    if (yAxis != null && yAxis.Scale != null && yAxis.Scale.Reversed)
                        ChartHelper.Swap(ref startPoint, ref endPoint);
                }

                // Get brush for fill and stroke.
                if (FillPalette != null && FillPalette.Count > 0)
                {
                    fillBrush = new LinearGradientBrush
                    {
                        GradientStops = FillPalette.GetGradient(lower, higher),
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        ColorInterpolationMode = FillPalette.ColorInterpolationMode
                    };
                }

                if (StrokePalette != null && StrokePalette.Count > 0)
                {
                    strokeBrush = new LinearGradientBrush
                    {
                        GradientStops = StrokePalette.GetGradient(lower, higher),
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        ColorInterpolationMode = StrokePalette.ColorInterpolationMode
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
                legendSymbol.SetBinding(StyleProperty, new Binding("DataPointStyle") { Source = this });
                legendSymbol.IsHitTestVisible = false;

                double paletteIndex = 1;
                if (FillPalette != null && FillPalette.Count > 0)
                    paletteIndex = FillPalette[FillPalette.Count - 1].Value;
                else if (StrokePalette != null && StrokePalette.Count > 0)
                    paletteIndex = StrokePalette[StrokePalette.Count - 1].Value;

                Brush fillBrush;
                Brush strokeBrush;
                GetBrushes(paletteIndex, out strokeBrush, out fillBrush);
                ChartHelper.SetStrokeAndFill(legendSymbol, strokeBrush, fillBrush);

                grid.Children.Add(legendSymbol);
                return grid;
            }

            return base.OnGetLegendSymbol();
        }
        #endregion
    }
}
