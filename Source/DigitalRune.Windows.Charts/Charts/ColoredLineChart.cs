// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a line chart where the stroke and fill color is defined in
    /// <see cref="Palette"/>s.
    /// </summary>
    /// <remarks>
    /// If a <see cref="ColoredLineChart"/> is rendered with
    /// <see cref="UIElement.SnapsToDevicePixels"/> set to <see langword="true"/>, small gaps can
    /// appear between line chart segments because of rounding issues.
    /// </remarks>
    public class ColoredLineChart : LineChart
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
            typeof(AreaFillMode),
            typeof(ColoredLineChart),
            new PropertyMetadata(AreaFillMode.GradientVertical, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the fill mode applied to the area of the line chart.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the fill mode applied to the area of the line chart.")]
        [Category(ChartCategories.Default)]
        public AreaFillMode FillMode
        {
            get { return (AreaFillMode)GetValue(FillModeProperty); }
            set { SetValue(FillModeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="FillPalette"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillPaletteProperty = DependencyProperty.Register(
            "FillPalette",
            typeof(Palette),
            typeof(ColoredLineChart),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette used to fill the area of the line chart.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette used to fill the area of the line chart.")]
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
          typeof(ColoredLineChart),
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
          typeof(ColoredLineChart),
          new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the palette used to fill the line of the line chart.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the palette used to fill the line of the line chart.")]
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
        /// Initializes a new instance of the <see cref="ColoredLineChart"/> class.
        /// </summary>
        public ColoredLineChart()
        {
            DefaultStyleKey = typeof(ColoredLineChart);
        }
#else
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ColoredLineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColoredLineChart), new FrameworkPropertyMetadata(typeof(ColoredLineChart)));
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
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "startIndex-1")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected override void OnUpdateLines(int startIndex, int endIndexExclusive, double[] xPositions, double[] basePositions, double[] yPositions)
        {
            if (Data.Count <= 1)
                return;

            bool reversed = XAxis.Scale.Reversed;
            bool horizontalStepLineVisible = HorizontalStepLineVisible;
            bool verticalStepLineVisible = VerticalStepLineVisible;
            bool filled = Filled;
            var interpolation = EffectiveInterpolation;

            int numberOfDataPoints = Data.Count;

            // Lines are drawn from index i to i+1.
            startIndex = Math.Max(0, startIndex - 1);

            // For centered steps one additional data point needs to be rendered.
            if (interpolation == ChartInterpolation.CenteredSteps)
                endIndexExclusive = Math.Min(numberOfDataPoints, endIndexExclusive + 1);

            for (int i = startIndex; i < endIndexExclusive; i++)
            {
                if (Numeric.IsNaN(xPositions[i]) || Numeric.IsNaN(yPositions[i]))
                    continue;

                Point previousPoint; //, previousPointBase;
                Point point, pointBase;
                Point nextPoint, nextPointBase;

                if (i - 1 >= 0)
                {
                    previousPoint = new Point(xPositions[i - 1], yPositions[i - 1]);
                    //previousPointBase = new Point(xPositions[i - 1], basePositions[i - 1]);
                }
                else
                {
                    previousPoint = new Point(double.NaN, double.NaN);
                    //previousPointBase = new Point(double.NaN, double.NaN);
                }

                point = new Point(xPositions[i], yPositions[i]);
                pointBase = new Point(xPositions[i], basePositions[i]);

                if (i + 1 < numberOfDataPoints)
                {
                    nextPoint = new Point(xPositions[i + 1], yPositions[i + 1]);
                    nextPointBase = new Point(xPositions[i + 1], basePositions[i + 1]);
                }
                else
                {
                    nextPoint = new Point(double.NaN, double.NaN);
                    nextPointBase = new Point(double.NaN, double.NaN);
                }

                var lineFigures = new PathFigureCollection();
                var areaFigures = new PathFigureCollection();

                // Draw lines and area from i to i+1.
                if (interpolation == ChartInterpolation.Linear)
                {
                    // Linear interpolation
                    if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                    {
                        var lineFigure = new PathFigure { StartPoint = point };
                        lineFigure.Segments.Add(new LineSegment { Point = nextPoint });
                        lineFigures.Add(lineFigure);

                        if (filled && !Numeric.IsNaN(pointBase.Y) && !Numeric.IsNaN(nextPointBase.Y))
                        {
                            var areaFigure = new PathFigure { StartPoint = point };
                            areaFigure.Segments.Add(new LineSegment { Point = nextPoint });
                            areaFigure.Segments.Add(new LineSegment { Point = nextPointBase });
                            areaFigure.Segments.Add(new LineSegment { Point = pointBase });
                            areaFigures.Add(areaFigure);
                        }
                    }
                }
                else if (interpolation == ChartInterpolation.CenteredSteps)
                {
                    // Centered steps
                    double centerBefore, centerAfter;
                    GetCenteredStep(previousPoint, point, nextPoint, out centerBefore, out centerAfter);

                    if (horizontalStepLineVisible)
                    {
                        var lineFigure = new PathFigure { StartPoint = new Point(centerBefore, point.Y) };
                        lineFigure.Segments.Add(new LineSegment { Point = new Point(centerAfter, point.Y) });
                        lineFigures.Add(lineFigure);
                    }

                    if (verticalStepLineVisible && !Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                    {
                        var lineFigure = new PathFigure { StartPoint = new Point(centerAfter, point.Y) };
                        lineFigure.Segments.Add(new LineSegment { Point = new Point(centerAfter, nextPoint.Y) });
                        lineFigures.Add(lineFigure);
                    }

                    if (filled && !Numeric.IsNaN(pointBase.Y))
                    {
                        var areaFigure = new PathFigure { StartPoint = new Point(centerBefore, point.Y) };
                        areaFigure.Segments.Add(new LineSegment { Point = new Point(centerAfter, point.Y) });
                        areaFigure.Segments.Add(new LineSegment { Point = new Point(centerAfter, pointBase.Y) });
                        areaFigure.Segments.Add(new LineSegment { Point = new Point(centerBefore, pointBase.Y) });
                        areaFigures.Add(areaFigure);
                    }
                }
                else if (interpolation == ChartInterpolation.LeftSteps && !reversed
                         || interpolation == ChartInterpolation.RightSteps && reversed)
                {
                    // LeftSteps or Reversed RightSteps
                    if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                    {
                        if (verticalStepLineVisible)
                        {
                            var lineFigure = new PathFigure { StartPoint = point };
                            lineFigure.Segments.Add(new LineSegment { Point = new Point(point.X, nextPoint.Y) });
                            lineFigures.Add(lineFigure);
                        }

                        if (horizontalStepLineVisible)
                        {
                            var lineFigure = new PathFigure { StartPoint = new Point(point.X, nextPoint.Y) };
                            lineFigure.Segments.Add(new LineSegment { Point = nextPoint });
                            lineFigures.Add(lineFigure);
                        }

                        if (filled && !Numeric.IsNaN(nextPointBase.Y))
                        {
                            var areaFigure = new PathFigure { StartPoint = new Point(point.X, nextPoint.Y) };
                            areaFigure.Segments.Add(new LineSegment { Point = nextPoint });
                            areaFigure.Segments.Add(new LineSegment { Point = nextPointBase });
                            areaFigure.Segments.Add(new LineSegment { Point = new Point(point.X, nextPointBase.Y) });
                            areaFigures.Add(areaFigure);
                        }
                    }
                }
                else
                {
                    // RightSteps or Reversed LeftSteps
                    if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                    {
                        if (horizontalStepLineVisible)
                        {
                            var lineFigure = new PathFigure { StartPoint = point };
                            lineFigure.Segments.Add(new LineSegment { Point = new Point(nextPoint.X, point.Y) });
                            lineFigures.Add(lineFigure);
                        }

                        if (verticalStepLineVisible)
                        {
                            var lineFigure = new PathFigure { StartPoint = new Point(nextPoint.X, point.Y) };
                            lineFigure.Segments.Add(new LineSegment { Point = nextPoint });
                            lineFigures.Add(lineFigure);
                        }

                        if (filled && !Numeric.IsNaN(pointBase.Y) && !Numeric.IsNaN(nextPointBase.Y))
                        {
                            var areaFigure = new PathFigure { StartPoint = point };
                            areaFigure.Segments.Add(new LineSegment { Point = new Point(nextPoint.X, point.Y) });
                            areaFigure.Segments.Add(new LineSegment { Point = new Point(nextPointBase.X, pointBase.Y) });
                            areaFigure.Segments.Add(new LineSegment { Point = pointBase });
                            areaFigures.Add(areaFigure);
                        }
                    }
                }
                AddSegment(i, lineFigures, areaFigures);
            }

            Canvas.InvalidateMeasure();
        }


        private void AddSegment(int index, PathFigureCollection lineFigures, PathFigureCollection areaFigures)
        {
            Brush stroke;
            Brush fill;
            OnGetStrokeAndFillForSegment(index, out stroke, out fill);

            // Add the geometries to the canvas.
            var areaPath = new Path
            {
                Data = new PathGeometry { Figures = areaFigures },
#if SILVERLIGHT
                // Note: Clip is a copy of the clip geometry. 
                // Silverlight crashes if we reuse the existing AreaClipGeometry multiple times.
                Clip = new RectangleGeometry { Rect = AreaClipGeometry.Rect },
#else
                Clip = AreaClipGeometry,
#endif
                Style = AreaStyle
            };



            if (fill != null)
                areaPath.Fill = fill;

#if SILVERLIGHT
            Canvas.SetZIndex(areaPath, -1); // Filled area should be in the background.
#else
            Panel.SetZIndex(areaPath, -1); // Filled area should be in the background.
#endif

            Canvas.Children.Add(areaPath);

            var linePath = new Path
            {
                Data = new PathGeometry { Figures = lineFigures },
#if SILVERLIGHT
                Clip = new RectangleGeometry { Rect = LineClipGeometry.Rect },
#else
                Clip = LineClipGeometry,
#endif
                Style = LineStyle
            };

            if (stroke != null)
                linePath.Stroke = stroke;

            Canvas.Children.Add(linePath);
        }


        /// <summary>
        /// Gets the <see cref="Brush"/>es used paint the segment at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="stroke">The stroke brush.</param>
        /// <param name="fill">The fill brush.</param>
        /// <remarks>
        /// The line chart drawing is done in segments: From one data point to the next data point.
        /// This method returns the <see cref="Brush"/>es that should be used to draw the line and
        /// fill the segment from data point <i><paramref name="index"/></i> to data point
        /// <i><paramref name="index"/> + 1</i>.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PaletteIndex")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        protected virtual void OnGetStrokeAndFillForSegment(int index, out Brush stroke, out Brush fill)
        {
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;

            // Check coloring parameters.
            if (PaletteIndex == PaletteIndex.Index && FillMode == AreaFillMode.GradientVertical
                || PaletteIndex == PaletteIndex.XValue && FillMode == AreaFillMode.GradientVertical)
            {
                throw new NotSupportedException("Vertical gradients are only supported if the palette is indexed by y values.");
            }

            // Find palette index.
            double paletteIndex = 0;
            switch (PaletteIndex)
            {
                case PaletteIndex.Index:
                    paletteIndex = index;
                    break;
                case PaletteIndex.XValue:
                    paletteIndex = Data[index].X;
                    break;
                case PaletteIndex.YValue:
                    paletteIndex = Data[index].Y;
                    break;
                default:
                    throw new NotSupportedException("PaletteIndex type is not supported.");
            }

            // Set default values for stroke and fill brush.
            stroke = null;
            fill = null;

            // Try to find colors in palette.
            if (FillMode == AreaFillMode.Solid)
            {
                // Solid:
                // Get a solid color fill for fill and stroke.
                if (FillPalette != null && FillPalette.Count > 0)
                    fill = new SolidColorBrush(FillPalette.GetColor(paletteIndex));

                if (StrokePalette != null && StrokePalette.Count > 0)
                    stroke = new SolidColorBrush(StrokePalette.GetColor(paletteIndex));
            }
            else
            {
                // Get data values.
                DataPoint data = Data[index];
                DataPoint dataBase = new DataPoint(data.X, 0, null);                          // The base is currently always y = 0.
                DataPoint previousData = (index > 0) ? Data[index - 1] : data;
                DataPoint previousDataBase = new DataPoint(previousData.X, 0, null);          // The base is currently always y = 0.
                DataPoint nextData = (index < Data.Count - 1) ? Data[index + 1] : data;
                DataPoint nextDataBase = new DataPoint(nextData.X, 0, null);                  // The base is currently always y = 0.

                // Gradient:
                double fillStart;
                double fillEnd;
                double strokeStart;
                double strokeEnd;

                // Define palette index values.
                ChartInterpolation interpolation = EffectiveInterpolation;
                if (PaletteIndex == PaletteIndex.Index)
                {
                    // Index-based
                    if (interpolation == ChartInterpolation.CenteredSteps)
                    {
                        fillStart = strokeStart = Math.Max(index - 0.5, 0);
                        fillEnd = strokeEnd = Math.Min(index + 0.5, Data.Count - 1);
                    }
                    else if (interpolation == ChartInterpolation.LeftSteps)
                    {
                        fillStart = strokeStart = Math.Max(index - 1, 0);
                        fillEnd = strokeEnd = index;
                    }
                    else
                    {
                        fillStart = strokeStart = index;
                        fillEnd = strokeEnd = index + 1;
                    }
                }
                else if (PaletteIndex == PaletteIndex.XValue)
                {
                    // x value based
                    if (interpolation == ChartInterpolation.CenteredSteps)
                    {
                        fillStart = strokeStart = (previousData.X + data.X) / 2;
                        fillEnd = strokeEnd = (data.X + nextData.X) / 2;
                    }
                    else if (interpolation == ChartInterpolation.LeftSteps)
                    {
                        fillStart = strokeStart = previousData.X;
                        fillEnd = strokeEnd = data.X;
                    }
                    else
                    {
                        fillStart = strokeStart = data.X;
                        fillEnd = strokeEnd = nextData.X;
                    }
                }
                else
                {
                    // y value based
                    if (FillMode == AreaFillMode.GradientHorizontal)
                    {
                        if (interpolation == ChartInterpolation.CenteredSteps)
                        {
                            fillStart = strokeStart = (previousData.Y + data.Y) / 2;
                            fillEnd = strokeEnd = (data.Y + nextData.Y) / 2;
                        }
                        else if (interpolation == ChartInterpolation.LeftSteps)
                        {
                            fillStart = strokeStart = previousData.Y;
                            fillEnd = strokeEnd = data.Y;
                        }
                        else
                        {
                            fillStart = strokeStart = data.Y;
                            fillEnd = strokeEnd = nextData.Y;
                        }
                    }
                    else
                    {
                        fillStart = Math.Min(0, Math.Min(data.Y, dataBase.Y));
                        fillEnd = Math.Max(0, Math.Max(data.Y, dataBase.Y));
                        if (interpolation == ChartInterpolation.Linear)
                        {
                            fillStart = Math.Min(fillStart, Math.Min(nextData.Y, nextDataBase.Y));
                            fillEnd = Math.Max(fillEnd, Math.Max(nextData.Y, nextDataBase.Y));
                        }

                        // This is the only case where the stroke has a different start and end value than the fill.
                        // (Because the fill starts at y=0, but the line starts at the data point.)
                        strokeStart = data.Y;
                        strokeEnd = nextData.Y;
                        if (strokeStart > strokeEnd)
                            ChartHelper.Swap(ref strokeStart, ref strokeEnd);
                    }
                }

                // Define gradient direction.
                Point startPoint;
                Point endPoint;
                if (PaletteIndex == PaletteIndex.Index || PaletteIndex == PaletteIndex.XValue || FillMode == AreaFillMode.GradientHorizontal)
                {
                    startPoint = new Point(0, 0);
                    endPoint = new Point(1, 0);
                    if (xAxis != null && xAxis.Scale != null && xAxis.Scale.Reversed)
                        ChartHelper.Swap(ref startPoint, ref endPoint);
                }
                else
                {
                    startPoint = new Point(0, 1);
                    endPoint = new Point(0, 0);
                    if (yAxis != null && yAxis.Scale != null && yAxis.Scale.Reversed)
                        ChartHelper.Swap(ref startPoint, ref endPoint);
                }

                // Get brush for fill and stroke.
                if (FillPalette != null && FillPalette.Count > 0)
                {
                    fill = new LinearGradientBrush
                    {
                        GradientStops = FillPalette.GetGradient(fillStart, fillEnd),
                        StartPoint = startPoint,
                        EndPoint = endPoint,
                        ColorInterpolationMode = FillPalette.ColorInterpolationMode
                    };
                }

                if (StrokePalette != null && StrokePalette.Count > 0)
                {
                    stroke = new LinearGradientBrush
                    {
                        GradientStops = StrokePalette.GetGradient(strokeStart, strokeEnd),
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
            var grid = new Grid
            {
                MinWidth = 16,
                MinHeight = 16,
            };

            double paletteIndex = 1;
            if (FillPalette != null && FillPalette.Count > 0)
                paletteIndex = FillPalette[FillPalette.Count - 1].Value;
            else if (StrokePalette != null && StrokePalette.Count > 0)
                paletteIndex = StrokePalette[StrokePalette.Count - 1].Value;

            Brush fillBrush;
            Brush strokeBrush;
            OnGetStrokeAndFillForLegendSymbol(paletteIndex, out strokeBrush, out fillBrush);

            if (Filled)
            {
                var area = new Path { Width = 16, Height = 16 };
                area.SetBinding(StyleProperty, new Binding("AreaStyle") { Source = this });
                var areaFigure = new PathFigure { StartPoint = new Point(0, 16) };
                areaFigure.Segments.Add(new LineSegment { Point = new Point(0, 12) });
                areaFigure.Segments.Add(new LineSegment { Point = new Point(16, 4) });
                areaFigure.Segments.Add(new LineSegment { Point = new Point(16, 16) });
                var areaGeometry = new PathGeometry();
                areaGeometry.Figures.Add(areaFigure);
                area.Data = areaGeometry;
                if (fillBrush != null)
                    area.Fill = fillBrush;
                grid.Children.Add(area);
            }

            var line = new Path { Width = 16, Height = 16 };
            line.SetBinding(StyleProperty, new Binding("LineStyle") { Source = this });
            var lineGeometry = new LineGeometry { StartPoint = new Point(0, 12), EndPoint = new Point(16, 4) };
            line.Data = lineGeometry;
            if (strokeBrush != null)
                line.Stroke = strokeBrush;
            grid.Children.Add(line);

            if (DataPointTemplate != null)
            {
                var legendSymbol = CreateDataPoint(null);
                if (legendSymbol != null)
                {
                    legendSymbol.HorizontalAlignment = HorizontalAlignment.Center;
                    legendSymbol.VerticalAlignment = VerticalAlignment.Center;
                    legendSymbol.IsHitTestVisible = false;
                    grid.Children.Add(legendSymbol);
                }
            }

            return grid;
        }


        /// <summary>
        /// Gets the stroke and fill brushes for the legend symbol.
        /// </summary>
        /// <param name="paletteIndex">Value used to index the <see cref="Palette"/>s.</param>
        /// <param name="stroke">The stroke brush.</param>
        /// <param name="fill">The fill brush.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        protected virtual void OnGetStrokeAndFillForLegendSymbol(double paletteIndex, out Brush stroke, out Brush fill)
        {
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;

            // Check coloring parameters.
            if (PaletteIndex == PaletteIndex.Index && FillMode == AreaFillMode.GradientVertical
                || PaletteIndex == PaletteIndex.XValue && FillMode == AreaFillMode.GradientVertical)
                throw new NotSupportedException("Vertical gradients are only supported if the palette is indexed by y values.");

            // Set default values for stroke and fill brush.
            stroke = null;
            fill = null;

            // Try to find colors in palette.
            // We draw from 0 upwards or from a negative value upwards to 0.
            double lower = Math.Min(0, paletteIndex);
            double higher = Math.Max(0, paletteIndex);

            //if (FillMode == AreaFillMode.Solid)
            //{
            //  // Fill legend symbol with single color.
            //  lower = higher;
            //}

            // Define gradient direction.
            Point startPoint;
            Point endPoint;
            if (PaletteIndex == PaletteIndex.Index || PaletteIndex == PaletteIndex.XValue ||
                FillMode == AreaFillMode.GradientHorizontal || FillMode == AreaFillMode.Solid)
            {
                startPoint = new Point(0, 0);
                endPoint = new Point(1, 0);
                if (xAxis != null && xAxis.Scale != null && xAxis.Scale.Reversed)
                    ChartHelper.Swap(ref startPoint, ref endPoint);
            }
            else
            {
                startPoint = new Point(0, 1);
                endPoint = new Point(0, 0);
                if (yAxis != null && yAxis.Scale != null && yAxis.Scale.Reversed)
                    ChartHelper.Swap(ref startPoint, ref endPoint);
            }

            // Get brush for fill and stroke.
            if (FillPalette != null && FillPalette.Count > 0)
            {
                fill = new LinearGradientBrush
                {
                    GradientStops = FillPalette.GetGradient(lower, higher),
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    ColorInterpolationMode = FillPalette.ColorInterpolationMode
                };
            }

            if (StrokePalette != null && StrokePalette.Count > 0)
            {
                stroke = new LinearGradientBrush
                {
                    GradientStops = StrokePalette.GetGradient(lower, higher),
                    StartPoint = startPoint,
                    EndPoint = endPoint,
                    ColorInterpolationMode = StrokePalette.ColorInterpolationMode
                };
            }
        }
        #endregion
    }
}
