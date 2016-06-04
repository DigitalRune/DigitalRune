// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Represents a line chart.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A line chart presents information as a series of data points connected by lines.
    /// </para>
    /// <para>
    /// A marker is drawn at the position of each data point. The properties
    /// <see cref="Chart.DataPointStyle"/> and <see cref="Chart.DataPointTemplate"/> defines the
    /// visual appearance of a marker. The <see cref="FrameworkElement.DataContext"/> of the marker
    /// will be set to the corresponding item in the <see cref="Chart.DataSource"/>. The property
    /// <see cref="FrameworkElement.Tag"/> will be set to a <see cref="Point"/> containing the x and
    /// y value where the marker is placed.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "LineStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "AreaStyle", StyleTargetType = typeof(Path))]
    [TemplatePart(Name = "PART_Line", Type = typeof(Path))]
    [TemplatePart(Name = "PART_Area", Type = typeof(Path))]
    public class LineChart : Chart
    {
        // Notes:
        // - Line segments and steps are drawn from index i to i+1.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Path _linePath;
        private Path _areaPath;
        private PathRenderer _lineRenderer;
        private PathRenderer _areaRenderer;

#if !SILVERLIGHT
        private bool _updatePending;
        private CacheMode _cacheMode;
        private EdgeMode _edgeMode;
#endif

        // Visible data points:
        // - _startIndex is the first point on/inside the chart area.
        // - _endIndexExclusive is the first point right outside the chart area.
        // - _startIndex == _endIndexExclusive means that no data points are visible.
        private int _startIndex;
        private int _endIndexExclusive;

        // Cached pixel positions:
        // - The length of the arrays is Data.Count.
        // - Only the positions of visible data points are cached. Two additional positions left and
        //   right may be needed for interpolation. Therefore, cached entries are valid from
        //   (_startIndex - 2) to (endIndexExclusive + 2).
        private double[] _xPositions;      // The x position.
        private double[] _yPositions;      // The y position.
        private double[] _basePositions;   // The y position at the bottom of the chart (for filled area charts).
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the effective interpolation type. (Group settings overrule local settings.)
        /// </summary>
        protected ChartInterpolation EffectiveInterpolation
        {
            get
            {
                var group = Group as ILineChartGroup;
                return (group != null) ? group.Interpolation : Interpolation;
            }
        }


        /// <summary>
        /// Gets or sets the clip geometry for the lines.
        /// </summary>
        protected RectangleGeometry LineClipGeometry { get; set; }


        /// <summary>
        /// Gets or sets the clip geometry for the chart area.
        /// </summary>
        protected RectangleGeometry AreaClipGeometry { get; set; }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        #region ----- Styles -----

        /// <summary>
        /// Identifies the <see cref="LineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register(
            "LineStyle",
            typeof(Style),
            typeof(LineChart),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the line. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the line.")]
        [Category(ChartCategories.Styles)]
        public Style LineStyle
        {
            get { return (Style)GetValue(LineStyleProperty); }
            set { SetValue(LineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="AreaStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AreaStyleProperty = DependencyProperty.Register(
            "AreaStyle",
            typeof(Style),
            typeof(LineChart),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the area of the line chart.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the area of the line chart.")]
        [Category(ChartCategories.Styles)]
        public Style AreaStyle
        {
            get { return (Style)GetValue(AreaStyleProperty); }
            set { SetValue(AreaStyleProperty, value); }
        }
        #endregion


        /// <summary>
        /// Identifies the <see cref="Filled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FilledProperty = DependencyProperty.Register(
            "Filled",
            typeof(bool),
            typeof(LineChart),
            new PropertyMetadata(Boxed.BooleanTrue, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the area of the line chart is filled.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the line chart is filled; otherwise <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the area of the line chart is filled.")]
        [Category(ChartCategories.Default)]
        public bool Filled
        {
            get { return (bool)GetValue(FilledProperty); }
            set { SetValue(FilledProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="HorizontalStepLineVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalStepLineVisibleProperty = DependencyProperty.Register(
            "HorizontalStepLineVisible",
            typeof(bool),
            typeof(LineChart),
            new PropertyMetadata(Boxed.BooleanTrue, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether horizontal line segments should be visible.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> draw the horizontal line of a step; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// This property is only applicable if data values are interpolated with "steps" (see
        /// <see cref="Interpolation"/>).
        /// </remarks>
        [Description("Gets or sets a value indicating whether horizontal line segments should be visible.")]
        [Category(ChartCategories.Default)]
        public bool HorizontalStepLineVisible
        {
            get { return (bool)GetValue(HorizontalStepLineVisibleProperty); }
            set { SetValue(HorizontalStepLineVisibleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="VerticalStepLineVisible"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalStepLineVisibleProperty = DependencyProperty.Register(
            "VerticalStepLineVisible",
            typeof(bool),
            typeof(LineChart),
            new PropertyMetadata(Boxed.BooleanTrue, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether vertical line segments should be visible.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to draw the vertical lines of steps; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// This property is only applicable if data values are interpolated with "steps" (see
        /// <see cref="Interpolation"/>).
        /// </remarks>
        [Description("Gets or sets a value indicating whether vertical line segments should be visible.")]
        [Category(ChartCategories.Default)]
        public bool VerticalStepLineVisible
        {
            get { return (bool)GetValue(VerticalStepLineVisibleProperty); }
            set { SetValue(VerticalStepLineVisibleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Interpolation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InterpolationProperty = DependencyProperty.Register(
            "Interpolation",
            typeof(ChartInterpolation),
            typeof(LineChart),
            new PropertyMetadata(ChartInterpolation.Linear, OnRelevantPropertyChanged));

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


#if !SILVERLIGHT
        /// <summary>
        /// Identifies the <see cref="RenderMode"/> dependency property. 
        /// (Not available in Silverlight.)
        /// </summary>
        public static readonly DependencyProperty RenderModeProperty = DependencyProperty.Register(
            "RenderMode",
            typeof(ChartRenderMode),
            typeof(LineChart),
            new PropertyMetadata(ChartRenderMode.Quality));

        /// <summary>
        /// Gets or sets render mode used for drawing the line chart.
        /// This is a dependency property. (Not available in Silverlight.)
        /// </summary>
        /// <value>
        /// A valid <see cref="ChartRenderMode"/>. The default mode is
        /// <see cref="ChartRenderMode.Quality"/>.
        /// </value>
        [Description("Gets or sets render mode used for drawing the line chart.")]
        [Category(ChartCategories.Default)]
        public ChartRenderMode RenderMode
        {
            get { return (ChartRenderMode)GetValue(RenderModeProperty); }
            set { SetValue(RenderModeProperty, value); }
        }
#endif


        /// <summary>
        /// Identifies the <see cref="MarkerThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MarkerThresholdProperty = DependencyProperty.Register(
            "MarkerThreshold",
            typeof(double),
            typeof(LineChart),
            new PropertyMetadata(Boxed.DoubleNaN, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the threshold for showing/hiding markers representing data points. This is a
        /// dependency property.
        /// </summary>
        /// <value>
        /// The threshold for showing/hiding markers representing data points. The default value is
        /// NaN.
        /// </value>
        /// <remarks>
        /// <para>
        /// The marker threshold can be set to only show markers for data points if the number of 
        /// data points is small.
        /// </para>
        /// <para>
        /// The marker threshold is relative to the x-axis length. It is defined as:
        /// <i>number of markers</i> / <i>x-axis length</i>
        /// </para>
        /// <para>
        /// If <c>number of data points ≤ XAxis.Length * MarkerThreshold</c>, the markers are
        /// rendered. If <c>number of data points &gt; XAxis.Length * MarkerThreshold</c>, the
        /// markers are hidden.
        /// </para>
        /// <para>
        /// A value of 0 means that the markers are always hidden. NaN means that the markers are
        /// always visible).
        /// </para>
        /// </remarks>
        [Description("Gets or sets the threshold for showing/hiding markers representing data points.")]
        [Category(ChartCategories.Default)]
        public double MarkerThreshold
        {
            get { return (double)GetValue(MarkerThresholdProperty); }
            set { SetValue(MarkerThresholdProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="LineChart"/> class.
        /// </summary>
        public LineChart()
        {
            DefaultStyleKey = typeof(LineChart);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="LineChart"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChart), new FrameworkPropertyMetadata(typeof(LineChart)));
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
        public override void OnApplyTemplate()
        {
            _lineRenderer = null;
            _areaRenderer = null;

            base.OnApplyTemplate();

            _linePath = GetTemplateChild("PART_Line") as Path ?? new Path { Style = LineStyle };
            _areaPath = GetTemplateChild("PART_Area") as Path ?? new Path { Style = AreaStyle };

            _lineRenderer = new PathRenderer(_linePath);
            _areaRenderer = new PathRenderer(_areaPath);

            Invalidate();
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
            base.OnUpdate();  // Updates the data source, if required.

            // Cleanup
            _lineRenderer.Clear();
            _areaRenderer.Clear();

            Debug.Assert(Canvas.Children.Count == 0, "Canvas should be cleared in base class.");
            Canvas.Children.Add(_areaPath);
            Canvas.Children.Add(_linePath);

            if (Data != null && Data.Count != 0)
            {
                // Clip filled area and lines to the chart area.
                Rect chartArea = ChartPanel.GetChartAreaBounds(XAxis, YAxis);

                // Allow line to draw on chart axes.
                Rect lineClipRect = new Rect(chartArea.Left - 2, chartArea.Top - 2, chartArea.Width + 4, chartArea.Height + 4);
                LineClipGeometry = new RectangleGeometry { Rect = lineClipRect };

                // Keep area inside the chart area.
                Rect areaClipRect = chartArea;
                AreaClipGeometry = new RectangleGeometry { Rect = areaClipRect };

                FindVisibleDataPoints();
                CachePositions();

#if SILVERLIGHT
                OnUpdateLines(_startIndex, _endIndexExclusive, _xPositions, _basePositions, _yPositions);
                UpdateMarkers();
#else
                switch (RenderMode)
                {
                    case ChartRenderMode.Quality:
                        // Update lines and markers immediately.
                        OnUpdateLines(_startIndex, _endIndexExclusive, _xPositions, _basePositions, _yPositions);
                        UpdateMarkers();
                        break;

                    case ChartRenderMode.Performance:
                        // Immediately update lines.
                        OnUpdateLines(_startIndex, _endIndexExclusive, _xPositions, _basePositions, _yPositions);

                        // Temporarily disable bitmap cache and anti-aliasing.
                        // Update markers when application is idle.
                        if (!_updatePending)
                        {
                            _updatePending = true;
                            ClearCacheMode();
                            ClearEdgeMode();
                            ChartHelper.Defer(Dispatcher, () =>
                            {
                                if (_updatePending)
                                {
                                    _updatePending = false;
                                    UpdateMarkers();
                                    RestoreCacheMode();
                                    RestoreEdgeMode();
                                }
                            });
                        }
                        break;

                    case ChartRenderMode.DoNotRender:
                        // Do nothing.
                        break;
                }
#endif
            }
        }


#if !SILVERLIGHT
        private void ClearCacheMode()
        {
            _cacheMode = CacheMode;
            CacheMode = null;
        }


        private void RestoreCacheMode()
        {
            CacheMode = _cacheMode;
            _cacheMode = null;
        }


        private void ClearEdgeMode()
        {
            _edgeMode = RenderOptions.GetEdgeMode(this);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }


        private void RestoreEdgeMode()
        {
            RenderOptions.SetEdgeMode(this, _edgeMode);
        }
#endif


        /// <summary>
        /// Determines the range: _startIndex - _endIndexExclusive
        /// </summary>
        private void FindVisibleDataPoints()
        {
            _startIndex = 0;
            _endIndexExclusive = 0;

            int numberOfDataPoints = Data.Count;
            if (numberOfDataPoints == 0)
                return;

            var xAxis = XAxis;
            double min = xAxis.Scale.Min;
            double max = xAxis.Scale.Max;

            // Find _startIndex:
            // _startIndex is the first point on/inside the chart area.
            // Check with !(x >= min) instead of (x < min) because x can be NaN.
            int index = 0;
            while (index < numberOfDataPoints && !(Data[index].X >= min))
                index++;

            if (index == numberOfDataPoints)
                return;

            _startIndex = index;

            // Find _endIndexExclusive:
            // _endIndexExclusive is the first point right outside the chart area.
            // Check with !(x > max) instead of (x <= max) because x can be NaN.
            while (index < numberOfDataPoints && !(Data[index].X > max))
                index++;

            _endIndexExclusive = index;
        }


        /// <summary>
        /// Caches the positions of all data points required for rendering.
        /// </summary>
        private void CachePositions()
        {
            int numberOfDataPoints = Data.Count;
            if (_xPositions == null || _xPositions.Length < numberOfDataPoints)
            {
                _xPositions = new double[numberOfDataPoints];
                _yPositions = new double[numberOfDataPoints];
                _basePositions = new double[numberOfDataPoints];
            }

            // Cache the visible data points + two additional data points on each side.
            int startIndex = Math.Max(0, _startIndex - 2);
            int endIndexExclusive = Math.Min(numberOfDataPoints, _endIndexExclusive + 2);

            // Get data points.
            GetValues(startIndex, endIndexExclusive, _xPositions, _basePositions, _yPositions);

            // Convert data points to pixel positions.
            var xAxis = XAxis;
            for (int i = startIndex; i < endIndexExclusive; i++)
                _xPositions[i] = xAxis.GetPosition(_xPositions[i]);

            var yAxis = YAxis;
            for (int i = startIndex; i < endIndexExclusive; i++)
                _yPositions[i] = yAxis.GetPosition(_yPositions[i]);

            for (int i = startIndex; i < endIndexExclusive; i++)
                _basePositions[i] = yAxis.GetPosition(_basePositions[i]);
        }


        /// <summary>
        /// Updates/draws the lines.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow", MessageId = "startIndex-1")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        protected virtual void OnUpdateLines(int startIndex, int endIndexExclusive, double[] xPositions, double[] basePositions, double[] yPositions)
        {
            if (Data.Count == 0)
                return;

            bool reversed = XAxis.Scale.Reversed;
            bool horizontalStepLineVisible = HorizontalStepLineVisible;
            bool verticalStepLineVisible = VerticalStepLineVisible;
            bool filled = Filled;
            var interpolation = EffectiveInterpolation;

#if !SILVERLIGHT
            // Perform the large data optimization (see below) if the line chart contains vertical lines.
            bool largeDataOptimization = interpolation == ChartInterpolation.Linear
                                         || interpolation == ChartInterpolation.CenteredSteps && verticalStepLineVisible
                                         || interpolation == ChartInterpolation.LeftSteps && verticalStepLineVisible
                                         || interpolation == ChartInterpolation.RightSteps && verticalStepLineVisible;
            double pixelSize = WindowsHelper.GetPixelSize(this).Width;
#endif

            using (var lineRenderContext = _lineRenderer.Open())
            using (var areaRenderContext = _areaRenderer.Open())
            {
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

#if !SILVERLIGHT
                    if (largeDataOptimization)
                    {
                        // Draw a single vertical line for all data points that lie on the same pixel column.
                        double xPixel = WindowsHelper.RoundToDevicePixelsCenter(xPositions[i], pixelSize);
                        double yMin = yPositions[i];
                        double yMax = yPositions[i];
                        int overlap = 0;

                        for (int j = i + 1; j < endIndexExclusive; j++)
                        {
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (Numeric.IsNaN(xPositions[j])
                                || Numeric.IsNaN(yPositions[j])
                                || xPixel != WindowsHelper.RoundToDevicePixelsCenter(xPositions[j], pixelSize))
                            {
                                break;
                            }

                            overlap++;
                            if (yPositions[j] < yMin)          // Safe for NaN.
                                yMin = yPositions[j];
                            else if (yPositions[j] > yMax)     // Safe for NaN.
                                yMax = yPositions[j];
                        }

                        if (overlap > 1)
                        {
                            lineRenderContext.DrawLine(new Point(xPixel, yMin), new Point(xPixel, yMax));

                            // i ............. index of first point in overlap.
                            // i + overlap ... index of last point in overlap.

                            if (filled && !Numeric.IsNaN(basePositions[i]) && !Numeric.IsNaN(basePositions[i + overlap]))
                                areaRenderContext.DrawPolygon(
                                    new Point(xPositions[i], yMin),
                                    new Point(xPositions[i + overlap], yMin),
                                    new Point(xPositions[i + overlap], basePositions[i + overlap]),
                                    new Point(xPositions[i], basePositions[i]));

                            // Jump ahead to last point in overlap.
                            i += overlap;

                            if (Numeric.IsNaN(yPositions[i]))
                                continue;
                        }
                    }
#endif

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

                    // Draw lines and area from i to i+1.
                    if (interpolation == ChartInterpolation.Linear)
                    {
                        // Linear interpolation
                        if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                        {
                            lineRenderContext.DrawLine(point, nextPoint);

                            if (filled && !Numeric.IsNaN(pointBase.Y) && !Numeric.IsNaN(nextPointBase.Y))
                                areaRenderContext.DrawPolygon(
                                    point,
                                    nextPoint,
                                    nextPointBase,
                                    pointBase);
                        }
                    }
                    else
                    {
                        if (interpolation == ChartInterpolation.CenteredSteps)
                        {
                            // Centered steps
                            double centerBefore, centerAfter;
                            GetCenteredStep(previousPoint, point, nextPoint, out centerBefore, out centerAfter);

                            if (horizontalStepLineVisible)
                                lineRenderContext.DrawLine(new Point(centerBefore, point.Y), new Point(centerAfter, point.Y));

                            if (verticalStepLineVisible && !Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                                lineRenderContext.DrawLine(new Point(centerAfter, point.Y), new Point(centerAfter, nextPoint.Y));

                            if (filled && !Numeric.IsNaN(pointBase.Y))
                            {
                                areaRenderContext.DrawPolygon(
                                    new Point(centerBefore, point.Y),
                                    new Point(centerAfter, point.Y),
                                    new Point(centerAfter, pointBase.Y),
                                    new Point(centerBefore, pointBase.Y));
                            }
                        }
                        else
                        {
                            if (interpolation == ChartInterpolation.LeftSteps && !reversed
                                || interpolation == ChartInterpolation.RightSteps && reversed)
                            {
                                // LeftSteps or Reversed RightSteps
                                if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                                {
                                    if (verticalStepLineVisible)
                                        lineRenderContext.DrawLine(point, new Point(point.X, nextPoint.Y));

                                    if (horizontalStepLineVisible)
                                        lineRenderContext.DrawLine(new Point(point.X, nextPoint.Y), nextPoint);

                                    if (filled && !Numeric.IsNaN(nextPointBase.Y))
                                    {
                                        areaRenderContext.DrawPolygon(
                                            new Point(point.X, nextPoint.Y),
                                            nextPoint,
                                            nextPointBase,
                                            new Point(point.X, nextPointBase.Y));
                                    }
                                }
                            }
                            else
                            {
                                // RightSteps or Reversed LeftSteps
                                if (!Numeric.IsNaN(nextPoint.X) && !Numeric.IsNaN(nextPoint.Y))
                                {
                                    if (horizontalStepLineVisible)
                                        lineRenderContext.DrawLine(point, new Point(nextPoint.X, point.Y));

                                    if (verticalStepLineVisible)
                                        lineRenderContext.DrawLine(new Point(nextPoint.X, point.Y), nextPoint);

                                    if (filled && !Numeric.IsNaN(pointBase.Y) && !Numeric.IsNaN(nextPointBase.Y))
                                    {
                                        areaRenderContext.DrawPolygon(
                                            point,
                                            new Point(nextPoint.X, point.Y),
                                            new Point(nextPointBase.X, pointBase.Y),
                                            pointBase);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (_linePath != null)
                _linePath.Clip = LineClipGeometry;

            if (_areaPath != null)
                _areaPath.Clip = AreaClipGeometry;

            // Bugfix:
            // Sometimes the first line of a Path is not drawn.
            // We need to explicitly call InvalidateMeasure() on the canvas.
            Canvas.InvalidateMeasure();
        }


        internal void GetCenteredStep(Point previousPoint, Point point, Point nextPoint, out double centerBefore, out double centerAfter)
        {
            Debug.Assert(!Numeric.IsNaN(point.X), "The point must be valid.");
            bool isPreviousValid = !Numeric.IsNaN(previousPoint.X);
            bool isNextValid = !Numeric.IsNaN(nextPoint.X);

            if (isPreviousValid && isNextValid)
            {
                // Calculate centers from points.
                centerBefore = (previousPoint.X + point.X) / 2;
                centerAfter = (point.X + nextPoint.X) / 2;
            }
            else if (isPreviousValid)
            {
                // Calculate center before current point and mirror the line.
                double delta = (point.X - previousPoint.X) / 2;
                centerBefore = point.X - delta;
                centerAfter = point.X + delta;
            }
            else if (isNextValid)
            {
                // Calculate center after current point and mirror the line.
                double delta = (nextPoint.X - point.X) / 2;
                centerBefore = point.X - delta;
                centerAfter = point.X + delta;
            }
            else
            {
                // Assume that length of line is 1 unit.
                Axis axis = XAxis;
                double delta = (axis.GetPosition(1) - axis.GetPosition(0)) / 2;
                centerBefore = point.X - delta;
                centerAfter = point.X + delta;
            }
        }


        private void UpdateMarkers()
        {
            if (DataPointTemplate == null)
            {
                // This is a line chart without markers.
                return;
            }

            double markerThreshold = MarkerThreshold;
            if (!Numeric.IsNaN(markerThreshold))
            {
                int numberOfDataPoints = _endIndexExclusive - _startIndex;
                int allowedNumberOfDataPoints = (int)(XAxis.Length * markerThreshold);
                if (numberOfDataPoints > allowedNumberOfDataPoints)
                {
                    // The number of data points exceeds the MarkerThreshold.
                    return;
                }
            }

            var yAxis = YAxis;
            var originY = yAxis.OriginY;
            var length = yAxis.Length;
            var yRange = new DoubleRange(originY - length, originY);

            // Loop over data points and create markers.
            for (int i = _startIndex; i < _endIndexExclusive; i++)
            {
                // Draw only data which is in the visible data range.
                if (!yRange.Contains(_yPositions[i]))
                    continue;

                // Create marker
                DataPoint data = Data[i];
                FrameworkElement marker = CreateDataPoint(data.DataContext ?? data);
                if (marker == null)
                    return;

                Canvas.Children.Add(marker);

                // Save index in tag for PositionMarker().
                // Alternatively, we could set the property as an attached property.
                marker.Tag = i;

#if SILVERLIGHT
                // In Silverlight: Position marker immediately, because some elements do not raise a 
                // SizeChanged event.
                PositionMarker(marker);
#endif

                // Position the marker as soon as it is measured.
                marker.SizeChanged += MarkerSizeChanged;
            }
        }


        private void MarkerSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            PositionMarker((FrameworkElement)sender);
        }


        private void PositionMarker(FrameworkElement marker)
        {
            if (!(marker.Tag is int))
                return;

            int index = (int)marker.Tag;

            double x = _xPositions[index];
            double y = _yPositions[index];

            double width = marker.DesiredSize.Width;
            double height = marker.DesiredSize.Height;

            if (width == 0.0 && height == 0.0)
            {
                // Fix for Silverlight.
                width = marker.ActualWidth;
                height = marker.ActualHeight;
            }

            // Position child horizontally.
            switch (marker.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    // Default: do nothing.
                    break;
                case HorizontalAlignment.Center:
                case HorizontalAlignment.Stretch:
                    x = x - width / 2.0;
                    break;
                case HorizontalAlignment.Right:
                    x = x - width;
                    break;
            }

            // Position child vertically.
            switch (marker.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    // Default: do nothing.
                    break;
                case VerticalAlignment.Center:
                case VerticalAlignment.Stretch:
                    y = y - height / 2.0;
                    break;
                case VerticalAlignment.Bottom:
                    y = y - height;
                    break;
            }

            Canvas.SetLeft(marker, x);
            Canvas.SetTop(marker, y);
        }


        private void GetValues(int startIndex, int endIndexExclusive, double[] xValues, double[] baseValues, double[] yValues)
        {
            var group = Group as ILineChartGroup;
            if (group != null)
            {
                group.GetValues(this, startIndex, endIndexExclusive, xValues, baseValues, yValues);
                return;
            }

            for (int index = startIndex; index < endIndexExclusive; index++)
            {
                var data = Data[index];
                xValues[index] = data.X;
                baseValues[index] = 0;
                yValues[index] = data.Y;
            }
        }


        /// <inheritdoc/>
        public override void ValidateData()
        {
            // Check DataSource
            if (Data == null)
                return;

            // Check 1: Check for NaN values.
            // X values must not be NaN.
            // Y values can be NaN to create gaps in the chart.
            for (int i = 0; i < Data.Count; i++)
                if (Numeric.IsNaN(Data[i].X))
                    throw new ChartDataException("X data values must not be NaN in line charts.");
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            var grid = new Grid
            {
                MinWidth = 16,
                MinHeight = 16
            };

            if (Filled)
            {
                var area = new Path { Width = 16, Height = 16 };
                area.SetBinding(StyleProperty, new Binding("AreaStyle") { Source = this });
                var areaRectangle = new RectangleGeometry { Rect = new Rect(0, 8, 16, 8) };
                area.Data = areaRectangle;
                grid.Children.Add(area);
            }

            var line = new Path { Width = 16, Height = 16 };
            line.SetBinding(StyleProperty, new Binding("LineStyle") { Source = this });
            var lineGeometry = new LineGeometry { StartPoint = new Point(0, 8), EndPoint = new Point(16, 8) };
            line.Data = lineGeometry;
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
        #endregion
    }
}
