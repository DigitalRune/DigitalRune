// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// A <see cref="ChartPanel"/> that provides a fixed set of x- and y-axes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DefaultChartPanel"/> is a specialized <see cref="ChartPanel"/> that provides
    /// two pairs of axes: the primary axes ( <see cref="XAxis1"/> and <see cref="YAxis1"/>) and
    /// secondary axes ( <see cref="XAxis2"/> and <see cref="YAxis2"/>). The axes share the same
    /// chart area. The primary axes are drawn below the chart area ( <see cref="XAxis1"/>) and at
    /// the left of the chart area ( <see cref="YAxis1"/>). The secondary are drawn above the chart
    /// area ( <see cref="XAxis2"/>) and at the right of the chart area ( <see cref="YAxis2"/>).
    /// </para>
    /// <para>
    /// By default, the axes are positioned automatically so that the chart area takes up the
    /// maximum of the available space. The axes can be positioned manually by setting
    /// <see cref="AutoAxisSpacing"/> to <see langword="false"/> and specifying the
    /// <see cref="AxisSpacing"/>.
    /// </para>
    /// <para>
    /// The properties of the axes can be specified directly in code by accessing the properties
    /// <see cref="XAxis1"/>, <see cref="YAxis1"/>, <see cref="XAxis2"/>, or <see cref="YAxis2"/>.
    /// In XAML the properties can be set by defining styles of the axis. The styles can be assigned
    /// using the properties: <see cref="XAxis1Style"/>, <see cref="YAxis1Style"/>,
    /// <see cref="XAxis2Style"/>, and <see cref="YAxis2Style"/>.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "XAxis1Style", StyleTargetType = typeof(Axis))]
    [StyleTypedProperty(Property = "YAxis1Style", StyleTargetType = typeof(Axis))]
    [StyleTypedProperty(Property = "XAxis2Style", StyleTargetType = typeof(Axis))]
    [StyleTypedProperty(Property = "YAxis2Style", StyleTargetType = typeof(Axis))]
    public class DefaultChartPanel : ChartPanel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const double DefaultWidth = 600;
        private const double DefaultHeight = 400;
        private const double MinChartAreaWidth = 10;
        private const double MinChartAreaHeight = 10;
        private const int AxisZIndex = 0;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _positioningAxes;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the bottom x-axis (primary x-axis).
        /// </summary>
        [Description("Gets the primary x-axis.")]
        [Category(ChartCategories.Default)]
        public Axis XAxis1 { get; private set; }


        /// <summary>
        /// Gets the left y-axis (primary y-axis).
        /// </summary>
        [Description("Gets the primary y-axis.")]
        [Category(ChartCategories.Default)]
        public Axis YAxis1 { get; private set; }


        /// <summary>
        /// Gets the top x-axis (secondary x-axis).
        /// </summary>
        [Description("Gets the secondary x-axis.")]
        [Category(ChartCategories.Default)]
        public Axis XAxis2 { get; private set; }


        /// <summary>
        /// Gets the right y-axis (secondary y-axis).
        /// </summary>
        [Description("Gets the secondary y-axis.")]
        [Category(ChartCategories.Default)]
        public Axis YAxis2 { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        #region ----- Axis Styles -----

        /// <summary>
        /// Identifies the <see cref="XAxis1Style"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxis1StyleProperty = DependencyProperty.Register(
            "XAxis1Style",
            typeof(Style),
            typeof(DefaultChartPanel),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is used for the primary x-axis.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the primary x-axis.")]
        [Category(ChartCategories.Styles)]
        public Style XAxis1Style
        {
            get { return (Style)GetValue(XAxis1StyleProperty); }
            set { SetValue(XAxis1StyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="YAxis1Style"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxis1StyleProperty = DependencyProperty.Register(
            "YAxis1Style",
            typeof(Style),
            typeof(DefaultChartPanel),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is used for the primary y-axis.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the primary y-axis.")]
        [Category(ChartCategories.Styles)]
        public Style YAxis1Style
        {
            get { return (Style)GetValue(YAxis1StyleProperty); }
            set { SetValue(YAxis1StyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="XAxis2Style"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxis2StyleProperty = DependencyProperty.Register(
            "XAxis2Style",
            typeof(Style),
            typeof(DefaultChartPanel),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is used for the secondary x-axis.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the secondary x-axis.")]
        [Category(ChartCategories.Styles)]
        public Style XAxis2Style
        {
            get { return (Style)GetValue(XAxis2StyleProperty); }
            set { SetValue(XAxis2StyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="YAxis2Style"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxis2StyleProperty = DependencyProperty.Register(
            "YAxis2Style",
            typeof(Style),
            typeof(DefaultChartPanel),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is used for the secondary y-axis.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the secondary y-axis.")]
        [Category(ChartCategories.Styles)]
        public Style YAxis2Style
        {
            get { return (Style)GetValue(YAxis2StyleProperty); }
            set { SetValue(YAxis2StyleProperty, value); }
        }
        #endregion


        /// <summary>
        /// Identifies the <see cref="AutoAxisSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoAxisSpacingProperty = DependencyProperty.Register(
            "AutoAxisSpacing",
            typeof(bool),
            typeof(DefaultChartPanel),
            new PropertyMetadata(Boxed.BooleanTrue, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the space for the axes is computed
        /// automatically. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the space used up by the axes is computed automatically;
        /// otherwise, <see langword="false"/>. Default value is <see langword="true"/>.
        /// </value>
        /// <seealso cref="AxisSpacing"/>
        [Description("Gets or sets a value indicating whether the space for the axes is computed automatically.")]
        [Category(ChartCategories.Default)]
        public bool AutoAxisSpacing
        {
            get { return (bool)GetValue(AutoAxisSpacingProperty); }
            set { SetValue(AutoAxisSpacingProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="AxisSpacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisSpacingProperty = DependencyProperty.Register(
            "AxisSpacing",
            typeof(Thickness),
            typeof(DefaultChartPanel),
            new PropertyMetadata(new Thickness(40), OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the space that is reserved for the axes if <see cref="AutoAxisSpacing"/> is
        /// <see langword="false"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// The space that is reserved for the axes. Default value is: 40 (on all sides).
        /// (Only relevant when <see cref="AutoAxisSpacing"/> is <see langword="false"/>.)
        /// </value>
        /// <remarks>
        /// <para>
        /// <see cref="AxisSpacing"/> defines the space around the chart area where the axes are
        /// drawn.
        /// </para>
        /// <see cref="AxisSpacing"/> is of the type <see cref="Thickness"/>, where
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// <see cref="Thickness.Left"/> is the space between the primary (left) y-axis and the
        /// border of the <see cref="DefaultChartPanel"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="Thickness.Bottom"/> is the space between the primary (bottom) x-axis and the
        /// border of the <see cref="DefaultChartPanel"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="Thickness.Right"/> is the space between the secondary (right) y-axis and the
        /// border of the <see cref="DefaultChartPanel"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// <see cref="Thickness.Top"/> is the space between the secondary (top) y-axis and the
        /// border of the <see cref="DefaultChartPanel"/>.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// By default, when <see cref="AutoAxisSpacing"/> is set to <see langword="false"/>, the
        /// extents are calculated automatically. In this case <see cref="AxisSpacing"/> is ignored.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the space that is reserved for the axes if AutoAxisSpacing is false.")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [TypeConverter(typeof(ThicknessConverter))]
#endif
        public Thickness AxisSpacing
        {
            get { return (Thickness)GetValue(AxisSpacingProperty); }
            set { SetValue(AxisSpacingProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if !SILVERLIGHT
        /// <summary>
        /// Initializes static members of the <see cref="DefaultChartPanel"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DefaultChartPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DefaultChartPanel), new FrameworkPropertyMetadata(typeof(DefaultChartPanel)));
        }
#endif


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultChartPanel"/> class.
        /// </summary>
        public DefaultChartPanel()
        {
            // Add the axis as the first canvas children.
            XAxis1 = new Axis { Tag = "XAxis1", LabelsAboveAxis = false, Orientation = Orientation.Horizontal };
            YAxis1 = new Axis { Tag = "YAxis1", LabelsAboveAxis = true, Orientation = Orientation.Vertical };
            XAxis2 = new Axis { Tag = "XAxis2", LabelsAboveAxis = true, Orientation = Orientation.Horizontal };
            YAxis2 = new Axis { Tag = "YAxis2", LabelsAboveAxis = false, Orientation = Orientation.Vertical };

            // Bind styles of axes
            Binding binding = new Binding("XAxis1Style") { Source = this };
            XAxis1.SetBinding(StyleProperty, binding);
            binding = new Binding("YAxis1Style") { Source = this };
            YAxis1.SetBinding(StyleProperty, binding);
            binding = new Binding("XAxis2Style") { Source = this };
            XAxis2.SetBinding(StyleProperty, binding);
            binding = new Binding("YAxis2Style") { Source = this };
            YAxis2.SetBinding(StyleProperty, binding);

            // Set z-index of axes (usually 0).
#if SILVERLIGHT
            Canvas.SetZIndex(XAxis1, AxisZIndex);
            Canvas.SetZIndex(YAxis1, AxisZIndex);
            Canvas.SetZIndex(XAxis2, AxisZIndex);
            Canvas.SetZIndex(YAxis2, AxisZIndex);
#else
            SetZIndex(XAxis1, AxisZIndex);
            SetZIndex(YAxis1, AxisZIndex);
            SetZIndex(XAxis2, AxisZIndex);
            SetZIndex(YAxis2, AxisZIndex);
#endif

            // Add axes as the first children of the panel.
            Children.Add(XAxis1);
            Children.Add(YAxis1);
            Children.Add(XAxis2);
            Children.Add(YAxis2);

            XAxis1.Invalidated += OnAxisInvalidated;
            YAxis1.Invalidated += OnAxisInvalidated;
            XAxis2.Invalidated += OnAxisInvalidated;
            YAxis2.Invalidated += OnAxisInvalidated;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnRelevantPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chartPanel = (DefaultChartPanel)dependencyObject;
            chartPanel.InvalidateMeasure();
        }


#if SILVERLIGHT
        /// <exclude/>
        public void OnAxisInvalidated(object sender, EventArgs eventArgs)
        {
            if (!_positioningAxes && AutoAxisSpacing)
                Dispatcher.BeginInvoke(new Action(InvalidateMeasure));
        }
#else
        private void OnAxisInvalidated(object sender, EventArgs eventArgs)
        {
            if (!_positioningAxes && AutoAxisSpacing)
                Dispatcher.BeginInvoke(new Action(InvalidateMeasure));
        }
#endif


        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            VerifyChildren();
            UpdateAxes();

            // Set availableSize to reasonable values.
            // (We need a finite extent to position our axes.)
            if (double.IsPositiveInfinity(availableSize.Width))
                availableSize.Width = DefaultWidth;
            if (double.IsPositiveInfinity(availableSize.Height))
                availableSize.Height = DefaultHeight;

            PositionAxes(availableSize);
            base.MeasureOverride(availableSize);
            return availableSize;
        }


        /// <summary>
        /// Verifies the children and ensures that the user does not remove the axes or the chart
        /// area rectangle.
        /// </summary>
        private void VerifyChildren()
        {
            if (!Children.Contains(XAxis1))
                Children.Add(XAxis1);
            if (!Children.Contains(YAxis1))
                Children.Add(YAxis1);
            if (!Children.Contains(XAxis2))
                Children.Add(XAxis2);
            if (!Children.Contains(YAxis2))
                Children.Add(YAxis2);
        }


        /// <summary>
        /// Updates the axes, if required.
        /// </summary>
        private void UpdateAxes()
        {
            XAxis1.Update();
            YAxis1.Update();
            XAxis2.Update();
            YAxis2.Update();
        }


        /// <summary>
        /// Determines the positions of the primary and secondary axes.
        /// </summary>
        /// <param name="bounds">The bounds of the chart panel.</param>
        private void PositionAxes(Size bounds)
        {
            // The AxisSpacing properties or the size of the chart panel has changed.
            // The axes need to be repositioned.
            _positioningAxes = true;

            if (AutoAxisSpacing)
                PositionAxisAutomatically(bounds);
            else
                PositionAxisAbsolute(bounds);

            _positioningAxes = false;
        }


        /// <summary>
        /// Positions the axis absolute using <see cref="AxisSpacing"/>.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        private void PositionAxisAbsolute(Size bounds)
        {
            double leftSpace = AxisSpacing.Left;
            double rightSpace = AxisSpacing.Right;
            double topSpace = AxisSpacing.Top;
            double bottomSpace = AxisSpacing.Bottom;
            PositionAxesBySpacing(bounds, leftSpace, rightSpace, topSpace, bottomSpace);
        }


        /// <summary>
        /// Positions the axis automatically.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        private void PositionAxisAutomatically(Size bounds)
        {
            // Initial pass
            UpdateAxes();
            Size xAxis1Size = XAxis1.OptimalSize;
            Size yAxis1Size = YAxis1.OptimalSize;
            Size xAxis2Size = XAxis2.OptimalSize;
            Size yAxis2Size = YAxis2.OptimalSize;

            double xAxis1Height = (XAxis1.Visibility != Visibility.Collapsed) ? XAxis1.OptimalSize.Height : 0;
            double xAxis2Height = (XAxis2.Visibility != Visibility.Collapsed) ? XAxis2.OptimalSize.Height : 0;
            double yAxis1Width = (YAxis1.Visibility != Visibility.Collapsed) ? YAxis1.OptimalSize.Width : 0;
            double yAxis2Width = (YAxis2.Visibility != Visibility.Collapsed) ? YAxis2.OptimalSize.Width : 0;
            double leftSpace = yAxis1Width;
            double rightSpace = yAxis2Width;
            double topSpace = xAxis2Height;
            double bottomSpace = xAxis1Height;
            PositionAxesBySpacing(bounds, leftSpace, rightSpace, topSpace, bottomSpace);

            // Check whether second pass is necessary.
            UpdateAxes();
            if (xAxis1Size != XAxis1.OptimalSize
                || yAxis1Size != YAxis1.OptimalSize
                || xAxis2Size != XAxis2.OptimalSize
                || yAxis2Size != YAxis2.OptimalSize)
            {
                // Use a second pass to optimize placement.
                xAxis1Height = (XAxis1.Visibility != Visibility.Collapsed) ? XAxis1.OptimalSize.Height : 0;
                xAxis2Height = (XAxis2.Visibility != Visibility.Collapsed) ? XAxis2.OptimalSize.Height : 0;
                yAxis1Width = (YAxis1.Visibility != Visibility.Collapsed) ? YAxis1.OptimalSize.Width : 0;
                yAxis2Width = (YAxis2.Visibility != Visibility.Collapsed) ? YAxis2.OptimalSize.Width : 0;
                leftSpace = yAxis1Width;
                rightSpace = yAxis2Width;
                topSpace = xAxis2Height;
                bottomSpace = xAxis1Height;
                PositionAxesBySpacing(bounds, leftSpace, rightSpace, topSpace, bottomSpace);
            }
        }


        /// <summary>
        /// Positions the axes by the specified spacing.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <param name="leftSpace">The space between left border and the left axis.</param>
        /// <param name="rightSpace">The space between the right border and the right axis.</param>
        /// <param name="topSpace">The space between the top border and the top axis.</param>
        /// <param name="bottomSpace">The space between the bottom border and the bottom axis.</param>
        private void PositionAxesBySpacing(Size bounds, double leftSpace, double rightSpace, double topSpace, double bottomSpace)
        {
            // Ensure that we are not getting negative numbers.
            double minWidth = leftSpace + rightSpace + MinChartAreaWidth;
            if (bounds.Width <= minWidth)
                bounds.Width = minWidth;

            double minHeight = topSpace + bottomSpace + MinChartAreaHeight;
            if (bounds.Height <= minHeight)
                bounds.Height = minHeight;

            double verticalOffsetOfXAxis1 = bounds.Height - bottomSpace;
            double horizontalOffsetOfYAxis2 = bounds.Width - rightSpace;
            double xAxisLength = bounds.Width - leftSpace - rightSpace;
            double yAxisLength = bounds.Height - topSpace - bottomSpace;

            // Primary axes (bottom, left)
            XAxis1.OriginX = leftSpace;
            XAxis1.OriginY = verticalOffsetOfXAxis1;
            XAxis1.Length = Math.Max(xAxisLength, 0);
            YAxis1.OriginX = leftSpace;
            YAxis1.OriginY = verticalOffsetOfXAxis1;
            YAxis1.Length = Math.Max(yAxisLength, 0);

            // Secondary axes (top, right)
            XAxis2.OriginX = leftSpace;
            XAxis2.OriginY = topSpace;
            XAxis2.Length = Math.Max(xAxisLength, 0);
            YAxis2.OriginX = horizontalOffsetOfYAxis2;
            YAxis2.OriginY = verticalOffsetOfXAxis1;
            YAxis2.Length = Math.Max(yAxisLength, 0);
        }
        #endregion
    }
}
