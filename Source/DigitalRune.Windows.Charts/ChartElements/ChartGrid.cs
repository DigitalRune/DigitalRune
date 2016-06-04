// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Draws grid lines inside a chart area.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The grid lines are drawn at the position of major and minor tick marks. The style of the
    /// lines can be influenced with the style properties: <see cref="HorizontalMajorLineStyle"/>,
    /// <see cref="HorizontalMinorLineStyle"/>, <see cref="VerticalMajorLineStyle"/>, and
    /// <see cref="VerticalMinorLineStyle"/>. Lines will only be drawn if the corresponding style is
    /// not set to <see langword="null"/>.
    /// </para>
    /// <para>
    /// <strong>Performance issues (WPF only):</strong> Be aware that complicated line styles (see
    /// <see cref="HorizontalMajorLineStyle"/>, <see cref="HorizontalMinorLineStyle"/>,
    /// <see cref="VerticalMajorLineStyle"/>, or <see cref="VerticalMinorLineStyle"/>) such as
    /// dashed lines can significantly reduce the rendering performance. To improve the performance
    /// avoid dashed lines or similar.
    /// </para>
    /// <para>
    /// To further improve the performance you can set the <see cref="RenderMode"/> to
    /// <see cref="ChartRenderMode.Performance"/> or even to
    /// <see cref="ChartRenderMode.DoNotRender"/>. The default render mode is
    /// <see cref="ChartRenderMode.Quality"/>.
    /// </para>
    /// <para>
    /// When a render mode is set to <see cref="ChartRenderMode.Performance"/> the rendering of the
    /// minor grid lines is deferred until the application is idle, bitmap caching and anti-aliasing
    /// is temporarily disabled. This is useful to keep the application responsive during
    /// interactions, such as zooming or panning the chart area.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "HorizontalMajorLineStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "HorizontalMinorLineStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "VerticalMajorLineStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "VerticalMinorLineStyle", StyleTargetType = typeof(Path))]
    [TemplatePart(Name = "PART_HorizontalMajorLines", Type = typeof(Path))]
    [TemplatePart(Name = "PART_HorizontalMinorLines", Type = typeof(Path))]
    [TemplatePart(Name = "PART_VerticalMajorLines", Type = typeof(Path))]
    [TemplatePart(Name = "PART_VerticalMinorLines", Type = typeof(Path))]
    public class ChartGrid : ChartElement
    {
        // WPF/Silverlight Differences:
        // Major and minor grid lines are updated immediately in Silverlight.
        // In WPF the update of the minor grid lines can be deferred to a point where the
        // application is idle. See RenderMode.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private PathRenderer _horizontalMajorLinesRenderer;
        private PathRenderer _horizontalMinorLinesRenderer;
        private PathRenderer _verticalMajorLinesRenderer;
        private PathRenderer _verticalMinorLinesRenderer;

#if !SILVERLIGHT
        private bool _updatePending;
        private CacheMode _cacheMode;
        private EdgeMode _edgeMode;
#endif
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        #region ----- Line Styles -----

        /// <summary>
        /// Identifies the <see cref="HorizontalMajorLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalMajorLineStyleProperty = DependencyProperty.Register(
            "HorizontalMajorLineStyle",
            typeof(Style),
            typeof(ChartGrid),
            new PropertyMetadata(null, OnLineStyleChanged));

        /// <summary>
        /// Gets or sets the style that is used for the horizontal, major grid lines.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, no horizontal, major grid lines are
        /// rendered.
        /// </remarks>
        [Description("Gets or sets the style of the horizontal, major grid lines.")]
        [Category(ChartCategories.Styles)]
        public Style HorizontalMajorLineStyle
        {
            get { return (Style)GetValue(HorizontalMajorLineStyleProperty); }
            set { SetValue(HorizontalMajorLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="HorizontalMinorLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalMinorLineStyleProperty = DependencyProperty.Register(
            "HorizontalMinorLineStyle",
            typeof(Style),
            typeof(ChartGrid),
            new PropertyMetadata(null, OnLineStyleChanged));

        /// <summary>
        /// Gets or sets the style that is used for the horizontal, minor grid lines.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, no horizontal, minor grid lines are
        /// rendered.
        /// </remarks>
        [Description("Gets or sets the style of the horizontal, minor grid lines.")]
        [Category(ChartCategories.Styles)]
        public Style HorizontalMinorLineStyle
        {
            get { return (Style)GetValue(HorizontalMinorLineStyleProperty); }
            set { SetValue(HorizontalMinorLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="VerticalMajorLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalMajorLineStyleProperty = DependencyProperty.Register(
            "VerticalMajorLineStyle",
            typeof(Style),
            typeof(ChartGrid),
            new PropertyMetadata(null, OnLineStyleChanged));

        /// <summary>
        /// Gets or sets the style that is used for the vertical, major grid lines.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, no vertical, major grid lines are rendered.
        /// </remarks>
        [Description("Gets or sets the style of the vertical, major grid lines.")]
        [Category(ChartCategories.Styles)]
        public Style VerticalMajorLineStyle
        {
            get { return (Style)GetValue(VerticalMajorLineStyleProperty); }
            set { SetValue(VerticalMajorLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="VerticalMinorLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalMinorLineStyleProperty = DependencyProperty.Register(
            "VerticalMinorLineStyle",
            typeof(Style),
            typeof(ChartGrid),
            new PropertyMetadata(null, OnLineStyleChanged));

        /// <summary>
        /// Gets or sets the style that is used for the vertical, minor grid lines.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// If this property is <see langword="null"/>, no vertical, minor grid lines are rendered.
        /// </remarks>
        [Description("Gets or sets the style of the vertical, minor grid lines.")]
        [Category(ChartCategories.Styles)]
        public Style VerticalMinorLineStyle
        {
            get { return (Style)GetValue(VerticalMinorLineStyleProperty); }
            set { SetValue(VerticalMinorLineStyleProperty, value); }
        }
        #endregion


#if !SILVERLIGHT
        /// <summary>
        /// Identifies the <see cref="RenderMode"/> dependency property. 
        /// (Not available in Silverlight.)
        /// </summary>
        public static readonly DependencyProperty RenderModeProperty = DependencyProperty.Register(
            "RenderMode",
            typeof(ChartRenderMode),
            typeof(ChartGrid),
            new FrameworkPropertyMetadata(ChartRenderMode.Quality));

        /// <summary>
        /// Gets or sets render mode used for drawing the grid lines.
        /// This is a dependency property. (Not available in Silverlight.)
        /// </summary>
        /// <value>
        /// A valid <see cref="ChartRenderMode"/>. The default mode is
        /// <see cref="ChartRenderMode.Quality"/>.
        /// </value>
        [Description("Gets or sets render mode used for drawing the grid lines.")]
        [Category(ChartCategories.Default)]
        public ChartRenderMode RenderMode
        {
            get { return (ChartRenderMode)GetValue(RenderModeProperty); }
            set { SetValue(RenderModeProperty, value); }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartGrid"/> class.
        /// </summary>
        public ChartGrid()
        {
            DefaultStyleKey = typeof(ChartGrid);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="ChartGrid"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ChartGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartGrid), new FrameworkPropertyMetadata(typeof(ChartGrid)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _horizontalMajorLinesRenderer = null;
            _horizontalMinorLinesRenderer = null;
            _verticalMajorLinesRenderer = null;
            _verticalMinorLinesRenderer = null;

            base.OnApplyTemplate();
            var horizontalMajorLinesPath = GetTemplateChild("PART_HorizontalMajorLines") as Path;
            var horizontalMinorLinesPath = GetTemplateChild("PART_HorizontalMinorLines") as Path;
            var verticalMajorLinesPath = GetTemplateChild("PART_VerticalMajorLines") as Path;
            var verticalMinorLinesPath = GetTemplateChild("PART_VerticalMinorLines") as Path;

            if (horizontalMajorLinesPath != null)
                _horizontalMajorLinesRenderer = new PathRenderer(horizontalMajorLinesPath);

            if (horizontalMinorLinesPath != null)
                _horizontalMinorLinesRenderer = new PathRenderer(horizontalMinorLinesPath);

            if (verticalMajorLinesPath != null)
                _verticalMajorLinesRenderer = new PathRenderer(verticalMajorLinesPath);

            if (verticalMinorLinesPath != null)
                _verticalMinorLinesRenderer = new PathRenderer(verticalMinorLinesPath);

            Invalidate();
        }


        private static void OnLineStyleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((ChartGrid)dependencyObject).Invalidate();
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
            _verticalMajorLinesRenderer.Clear();
            _horizontalMajorLinesRenderer.Clear();
            _verticalMinorLinesRenderer.Clear();
            _horizontalMinorLinesRenderer.Clear();

#if SILVERLIGHT
            UpdateMajorGridLines();
            UpdateMinorGridLines();
#else
            switch (RenderMode)
            {
                case ChartRenderMode.Quality:
                    // Update grid lines immediately.
                    UpdateMajorGridLines();
                    UpdateMinorGridLines();
                    break;

                case ChartRenderMode.Performance:
                    // Update major grid lines immediately.
                    UpdateMajorGridLines();

                    // Temporarily disable bitmap cache and anti-aliasing.
                    // Update minor grid lines when application is idle.
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
                                UpdateMinorGridLines();
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

            base.OnUpdate();
        }


        private void UpdateMajorGridLines()
        {
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            if (xAxis != null && yAxis != null)
            {
                AxisScale xScale = xAxis.Scale;
                AxisScale yScale = yAxis.Scale;
                if (xScale != null && yScale != null)
                {
                    double xMin = xAxis.GetPosition(xScale.Min);
                    double xMax = xAxis.GetPosition(xScale.Max);
                    double yMin = yAxis.GetPosition(yScale.Min);
                    double yMax = yAxis.GetPosition(yScale.Max);

                    // Add vertical major lines
                    if (_verticalMajorLinesRenderer != null && VerticalMajorLineStyle != null)
                    {
                        using (var renderContext = _verticalMajorLinesRenderer.Open())
                        {
                            for (int i = 0; i < xAxis.MajorTicks.Length; ++i)
                            {
                                double x = xAxis.GetPosition(xAxis.MajorTicks[i]);
                                renderContext.DrawLine(new Point(x, yMin), new Point(x, yMax));
                            }
                        }
                    }

                    // Add horizontal major lines
                    if (_horizontalMajorLinesRenderer != null && HorizontalMajorLineStyle != null)
                    {
                        using (var renderContext = _horizontalMajorLinesRenderer.Open())
                        {
                            for (int i = 0; i < yAxis.MajorTicks.Length; ++i)
                            {
                                double y = yAxis.GetPosition(yAxis.MajorTicks[i]);
                                renderContext.DrawLine(new Point(xMin, y), new Point(xMax, y));
                            }
                        }
                    }
                }
            }
        }


        private void UpdateMinorGridLines()
        {
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            if (xAxis != null && yAxis != null)
            {
                AxisScale xScale = xAxis.Scale;
                AxisScale yScale = yAxis.Scale;
                if (xScale != null && yScale != null)
                {
                    double xMin = xAxis.GetPosition(xScale.Min);
                    double xMax = xAxis.GetPosition(xScale.Max);
                    double yMin = yAxis.GetPosition(yScale.Min);
                    double yMax = yAxis.GetPosition(yScale.Max);

                    // Add vertical minor lines
                    if (_verticalMinorLinesRenderer != null && VerticalMinorLineStyle != null)
                    {
                        using (var renderContext = _verticalMinorLinesRenderer.Open())
                        {
                            for (int i = 0; i < xAxis.MinorTicks.Length; ++i)
                            {
                                double x = xAxis.GetPosition(xAxis.MinorTicks[i]);
                                renderContext.DrawLine(new Point(x, yMin), new Point(x, yMax));
                            }
                        }
                    }

                    // Add horizontal minor lines
                    if (_horizontalMinorLinesRenderer != null && HorizontalMinorLineStyle != null)
                    {
                        using (var renderContext = _horizontalMinorLinesRenderer.Open())
                        {
                            for (int i = 0; i < yAxis.MinorTicks.Length; ++i)
                            {
                                double y = yAxis.GetPosition(yAxis.MinorTicks[i]);
                                renderContext.DrawLine(new Point(xMin, y), new Point(xMax, y));
                            }
                        }
                    }
                }
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


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            var grid = new Grid
            {
                MinWidth = 16,
                MinHeight = 16,
            };

            if (HorizontalMajorLineStyle != null)
            {
                var horizontalMajorLines = new Path { Width = 16, Height = 16, Stretch = Stretch.Uniform };
                horizontalMajorLines.SetBinding(StyleProperty, new Binding("HorizontalMajorLineStyle") { Source = this });
                using (var renderContext = new PathRenderer(horizontalMajorLines).Open())
                {
                    renderContext.DrawLine(new Point(0, 0), new Point(16, 0));
                    renderContext.DrawLine(new Point(0, 8), new Point(16, 8));
                    renderContext.DrawLine(new Point(0, 16), new Point(16, 16));
                }
                grid.Children.Add(horizontalMajorLines);
            }

            if (HorizontalMinorLineStyle != null)
            {
                var horizontalMinorLines = new Path { Width = 16, Height = 16, Stretch = Stretch.Uniform };
                horizontalMinorLines.SetBinding(StyleProperty, new Binding("HorizontalMinorLineStyle") { Source = this });
                using (var renderContext = new PathRenderer(horizontalMinorLines).Open())
                {
                    renderContext.DrawLine(new Point(0, 4), new Point(16, 4));
                    renderContext.DrawLine(new Point(0, 12), new Point(16, 12));
                }
                grid.Children.Add(horizontalMinorLines);
            }

            if (VerticalMajorLineStyle != null)
            {
                var verticalMajorLines = new Path { Width = 16, Height = 16, Stretch = Stretch.Uniform };
                verticalMajorLines.SetBinding(StyleProperty, new Binding("VerticalMajorLineStyle") { Source = this });
                using (var renderContext = new PathRenderer(verticalMajorLines).Open())
                {
                    renderContext.DrawLine(new Point(0, 0), new Point(0, 16));
                    renderContext.DrawLine(new Point(8, 0), new Point(8, 16));
                    renderContext.DrawLine(new Point(16, 0), new Point(16, 16));
                }
                grid.Children.Add(verticalMajorLines);
            }

            if (VerticalMinorLineStyle != null)
            {
                var verticalMinorLines = new Path { Width = 16, Height = 16, Stretch = Stretch.Uniform };
                verticalMinorLines.SetBinding(StyleProperty, new Binding("VerticalMinorLineStyle") { Source = this });
                using (var renderContext = new PathRenderer(verticalMinorLines).Open())
                {
                    renderContext.DrawLine(new Point(4, 0), new Point(4, 16));
                    renderContext.DrawLine(new Point(12, 0), new Point(12, 16));
                }
                grid.Children.Add(verticalMinorLines);
            }

            return grid;
        }
        #endregion
    }
}
