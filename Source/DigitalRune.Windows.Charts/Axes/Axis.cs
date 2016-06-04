// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalRune.Collections;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a horizontal or vertical chart axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is responsible for drawing an axis line with a title, ticks, and tick labels. The
    /// <see cref="AxisScale"/> determines which values are mapped to the axis. The axis is also
    /// used to convert data values to positions in the chart area.
    /// </para>
    /// <para>
    /// Axis elements are positioned absolutely within the <see cref="ChartPanel"/>. The property
    /// <see cref="UIElement.DesiredSize"/> does not return the full required size. Instead, you
    /// should use the property <see cref="OptimalSize"/>. This property specifies the actual space
    /// that the axis requires.
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "AxisStyle", StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = "MajorTickStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "MinorTickStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "LabelStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "TitleStyle", StyleTargetType = typeof(ContentControl))]
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_AxisLine", Type = typeof(Line))]
    [TemplatePart(Name = "PART_MajorTicks", Type = typeof(Path))]
    [TemplatePart(Name = "PART_MinorTicks", Type = typeof(Path))]
    [TemplatePart(Name = "PART_Title", Type = typeof(ContentControl))]
    public class Axis : Control
    {
        // Note: This class is performance critical and hence contains some performance
        // optimization, such as avoiding dependency properties by locally caching their values,
        // caching intermediate results, etc.
        // 
        // TODO: Add overloads that work on arrays/lists to reduce overhead and cache misses.
        // For example: GetPositions(double[] valueIn, int index, int count, double[] valuesOut)


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        /// <summary>
        /// The default value of the <see cref="Length"/> dependency property.
        /// </summary>
        public const double DefaultLength = 240;


        /// <summary>
        /// The default value of the <see cref="Title"/> dependency property.
        /// </summary>
        private const string DefaultTitle = "Axis";


        /// <summary>
        /// The default value of the <see cref="TitleOffset"/> dependency property.
        /// </summary>
        public const double DefaultTitleOffset = 0.0;


        /// <summary>
        /// The default value of the <see cref="MajorTickLength"/> dependency property.
        /// </summary>
        public const double DefaultMajorTickLength = 6.0;


        /// <summary>
        /// The default value of the <see cref="MinorTickLength"/> dependency property.
        /// </summary>
        public const double DefaultMinorTickLength = 3.0;


        /// <summary>
        /// The default value of the <see cref="MinTickDistance"/> dependency property.
        /// </summary>
        public const double DefaultMinTickDistance = 60.0;


        /// <summary>
        /// The default value of the <see cref="OriginX"/> dependency property.
        /// </summary>
        public const double DefaultOriginX = 0.0;


        /// <summary>
        /// The default value of the <see cref="OriginY"/> dependency property.
        /// </summary>
        public const double DefaultOriginY = 0.0;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly WeakCollection<IChartElement> _chartElements = new WeakCollection<IChartElement>();
        private Canvas _canvas;
        private Line _axisLine;
        private ContentControl _titleContentControl;

        private PathRenderer _majorTickRenderer;
        private PathRenderer _minorTickRenderer;

        // Performance measurement has shown that adding controls to the canvas is expensive.
        // Therefore we internally cache the controls and only update and show/hide them.
        private readonly List<TextBlock> _tickLabels = new List<TextBlock>();

        // Create an invisible rectangle which is only used for hit testing
        private Rectangle _hitRectangle;    // The invisible shape rendered in the background.
        private Rect _hitRectangleBounds;   // The position and size of the invisible rectangle.

        // Cached measurements.
        private Size _titleSize;                    // Size of the label content control.
        private double _internalTitleOffset;        // Auto computed label offset.
        private double _effectiveTitleOffset;       // The actual label offset.

        // Weak event subscription
        private IDisposable _scaleSubscription;

        // Weak events
        private readonly WeakEvent<EventHandler> _invalidatedEvent = new WeakEvent<EventHandler>();
        private readonly WeakEvent<EventHandler> _updatedEvent = new WeakEvent<EventHandler>();

        // The following members cached dependency properties to improve performance.
        private AxisScale _scale;
        private double _originX, _originY;
        private double _length;
        private double _startPosition;
        private double _endPosition;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the default scale that should be used if <see cref="Scale"/> is not set.
        /// </summary>
        /// <value>The default scale that should be used if <see cref="Scale"/> is not set.</value>
        private AxisScale DefaultScale
        {
            get
            {
                if (_defaultScale == null)
                    _defaultScale = new LinearScale(0, 1);

                return _defaultScale;
            }
        }
        private AxisScale _defaultScale;


        /// <summary>
        /// Gets a value indicating whether this <see cref="Axis"/> is valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="Update()"/>
        /// <seealso cref="Updated"/>
        public bool IsValid
        {
            get { return _isValid; }
        }
        private bool _isValid;


        /// <summary>
        /// Gets a value indicating whether this instance is an x-axis.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is an x-axis; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <seealso cref="IsYAxis"/>
        public bool IsXAxis
        {
            get { return _isXAxis; }
        }
        private bool _isXAxis;


        /// <summary>
        /// Gets a value indicating whether this instance is an y-axis.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is an y-axis; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <seealso cref="IsXAxis"/>
        public bool IsYAxis
        {
            get { return !_isXAxis; }
        }


        /// <summary>
        /// Gets the values at which labels are drawn.
        /// </summary>
        /// <remarks>
        /// This is an array of data values at which labels are be drawn. The tick values are
        /// computed by the <see cref="Scale"/> of the axis in <see cref="AxisScale.ComputeTicks"/>.
        /// In most cases the <see cref="LabelValues"/> are identically to the
        /// <see cref="MajorTicks"/>. But some scales use a different placement.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public double[] LabelValues { get; private set; }


        /// <summary>
        /// Gets the major tick values.
        /// </summary>
        /// <remarks>
        /// This is an array of data values at which major ticks are drawn. The tick values are
        /// computed by the <see cref="Scale"/> of the axis in <see cref="AxisScale.ComputeTicks"/>.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public double[] MajorTicks { get; private set; }


        /// <summary>
        /// Gets the minor tick values.
        /// </summary>
        /// <remarks>
        /// This is an array of data values at which minor ticks are drawn. The tick values are
        /// computed by the <see cref="Scale"/> of the axis in <see cref="AxisScale.ComputeTicks"/>.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public double[] MinorTicks { get; private set; }


        /// <summary>
        /// Gets the optimal size of the axis.
        /// </summary>
        /// <value>The optimal size of the axis.</value>
        /// <remarks>
        /// This property is computed in <see cref="Invalidate"/>. The optimal size is the minimal
        /// size which is needed for the axis, tick labels, and axis title.
        /// </remarks>
        public Size OptimalSize { get; private set; }


        /// <summary>
        /// Occurs when the scale or layout of the axis is changed and the visual appearance becomes
        /// invalid.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is created either implicitly by changing a property of the <see cref="Axis"/> or
        /// its <see cref="AxisScale"/>, or by explicitly calling <see cref="Invalidate"/>. The
        /// <see cref="Axis"/> is updated automatically when it is measured 
        /// (<see cref="UIElement.Measure"/>). It can be updated explicitly by calling
        /// <see cref="Update()"/>.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> This event is implemented using the weak-event pattern. It
        /// stores the event handlers as weak references. Weak-events use reflection to call the
        /// event handler. In Silverlight the event handlers needs to be public because reflection
        /// on private members does not work with partial trust.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Update()"/>
        /// <seealso cref="Updated"/>
        public event EventHandler Invalidated
        {
            add { _invalidatedEvent.Add(value); }
            remove { _invalidatedEvent.Remove(value); }
        }


        /// <summary>
        /// Occurs when the <see cref="Axis"/> is updated.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="Axis"/> becomes invalid when a property of the <see cref="Axis"/> or its
        /// <see cref="AxisScale"/> changes, or when <see cref="Invalidate"/> is called. The
        /// <see cref="Axis"/> is updated automatically when it is measured (see
        /// <see cref="UIElement.Measure"/>). It can be updated immediately by calling
        /// <see cref="Update()"/>.
        /// </para>
        /// <para>
        /// Elements that depend on the <see cref="Axis"/>, such as <see cref="Chart"/> object,
        /// should listen for the <see cref="Updated"/> event and redraw themselves when this event
        /// occurs.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> This event is implemented using the weak-event pattern. It
        /// stores the event handlers as weak references. Weak-events use reflection to call the
        /// event handler. In Silverlight the event handlers needs to be public because reflection
        /// on private members does not work with partial trust.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Updated"/>
        public event EventHandler Updated
        {
            add { _updatedEvent.Add(value); }
            remove { _updatedEvent.Remove(value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        #region ----- Styles -----

        /// <summary>
        /// Identifies the <see cref="AxisStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisStyleProperty = DependencyProperty.Register(
            "AxisStyle",
            typeof(Style),
            typeof(Axis),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the axis line.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the axis line.")]
        [Category(ChartCategories.Styles)]
        public Style AxisStyle
        {
            get { return (Style)GetValue(AxisStyleProperty); }
            set { SetValue(AxisStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MajorTickStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorTickStyleProperty = DependencyProperty.Register(
            "MajorTickStyle",
            typeof(Style),
            typeof(Axis),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the major tick marks.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the major tick marks.")]
        [Category(ChartCategories.Styles)]
        public Style MajorTickStyle
        {
            get { return (Style)GetValue(MajorTickStyleProperty); }
            set { SetValue(MajorTickStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinorTickStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorTickStyleProperty = DependencyProperty.Register(
            "MinorTickStyle",
            typeof(Style),
            typeof(Axis),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the minor tick marks.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the minor tick marks.")]
        [Category(ChartCategories.Styles)]
        public Style MinorTickStyle
        {
            get { return (Style)GetValue(MinorTickStyleProperty); }
            set { SetValue(MinorTickStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="LabelStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelStyleProperty = DependencyProperty.Register(
            "LabelStyle",
            typeof(Style),
            typeof(Axis),
            new PropertyMetadata(null, OnRelevantPropertyChanged));


        /// <summary>
        /// Gets or sets the style that is used for the tick labels.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the tick labels.")]
        [Category(ChartCategories.Styles)]
        public Style LabelStyle
        {
            get { return (Style)GetValue(LabelStyleProperty); }
            set { SetValue(LabelStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TitleStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleStyleProperty = DependencyProperty.Register(
            "TitleStyle",
            typeof(Style),
            typeof(Axis),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the style that is used for the axis title.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style that is used for the axis title.")]
        [Category(ChartCategories.Styles)]
        public Style TitleStyle
        {
            get { return (Style)GetValue(TitleStyleProperty); }
            set { SetValue(TitleStyleProperty, value); }
        }
        #endregion


        /// <summary>
        /// Identifies the <see cref="AutoScale"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AutoScaleProperty = DependencyProperty.Register(
            "AutoScale",
            typeof(bool),
            typeof(Axis),
            new PropertyMetadata(Boxed.BooleanFalse, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the scales of the axis is set automatically.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to set the scale of the axis automatically; otherwise,
        /// <see langword="false"/>. Default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the scales of the axis is set automatically.")]
        [Category(ChartCategories.Default)]
        public bool AutoScale
        {
            get { return (bool)GetValue(AutoScaleProperty); }
            set { SetValue(AutoScaleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ClipLabels"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClipLabelsProperty = DependencyProperty.Register(
            "ClipLabels",
            typeof(bool),
            typeof(Axis),
            new PropertyMetadata(Boxed.BooleanFalse, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the axis tick labels shall be clipped if they
        /// exceed the length of the axis. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to clip the axis tick labels if they exceed the length of the
        /// axis. The default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the tick labels shall be clipped if they exceed the length of the axis.")]
        [Category(ChartCategories.Default)]
        public bool ClipLabels
        {
            get { return (bool)GetValue(ClipLabelsProperty); }
            set { SetValue(ClipLabelsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(object),
            typeof(Axis),
            new PropertyMetadata(DefaultTitle, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the title of the axis.
        /// This is a dependency property.
        /// </summary>
        /// <remarks>
        /// This is typically a <see cref="string"/> with the name of the axis. Any object that can
        /// be presented inside a <see cref="ContentControl"/> is valid. The default value is
        /// "Axis".
        /// </remarks>
        [Description("Gets or sets the title of the axis.")]
#if !SILVERLIGHT
        [TypeConverter(typeof(StringConverter))]
#endif
        [Category(ChartCategories.Default)]
        public object Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="LabelsAboveAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelsAboveAxisProperty = DependencyProperty.Register(
            "LabelsAboveAxis",
            typeof(bool),
            typeof(Axis),
            new PropertyMetadata(Boxed.BooleanFalse, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the title and the tick labels are above the
        /// axis. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to draw the labels above the axis line; <see langword="false"/>
        /// to draw the labels below the axis line. The default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the axis title and the tick labels are above the axis.")]
        [Category(ChartCategories.Default)]
        public bool LabelsAboveAxis
        {
            get { return (bool)GetValue(LabelsAboveAxisProperty); }
            set { SetValue(LabelsAboveAxisProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TitleOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleOffsetProperty = DependencyProperty.Register(
            "TitleOffset",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultTitleOffset, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the offset of the axis title from the axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>The offset of the axis title from the axis. The default value is 0.</value>
        /// <remarks>
        /// If this value is positive, the name title is moved further away from the axis.
        /// <see cref="UseAbsoluteTitleOffset"/> defines whether the specified offset is the
        /// absolute distance from the axis line, or a relative offset that is added to the default
        /// offset.
        /// </remarks>
        [Description("Gets or sets the offset of the axis title from the axis.")]
        [Category(ChartCategories.Default)]
        public double TitleOffset
        {
            get { return (double)GetValue(TitleOffsetProperty); }
            set { SetValue(TitleOffsetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MajorTickLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MajorTickLengthProperty = DependencyProperty.Register(
            "MajorTickLength",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultMajorTickLength, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the size of the large ticks.
        /// This is a dependency property.
        /// </summary>
        /// <value>The size of large ticks. The default value is 6.</value>
        [Description("Gets or sets the length of large axis ticks.")]
        [Category(ChartCategories.Default)]
        public double MajorTickLength
        {
            get { return (double)GetValue(MajorTickLengthProperty); }
            set { SetValue(MajorTickLengthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinorTickLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinorTickLengthProperty = DependencyProperty.Register(
            "MinorTickLength",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultMinorTickLength, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the size of the small ticks.
        /// This is a dependency property.
        /// </summary>
        /// <value>The small size of small ticks. The default value is 3.</value>
        [Description("Gets or sets the size of the small ticks.")]
        [Category(ChartCategories.Default)]
        public double MinorTickLength
        {
            get { return (double)GetValue(MinorTickLengthProperty); }
            set { SetValue(MinorTickLengthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinTickDistance"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinTickDistanceProperty = DependencyProperty.Register(
            "MinTickDistance",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultMinTickDistance, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the minimum distance between major ticks in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The minimum distance between major ticks in device-independent pixels. The default value
        /// is <see cref="DefaultMinTickDistance"/>.
        /// </value>
        [Description("Gets or sets the minimum distance between ticks in device-independent pixels.")]
        [Category(Categories.Appearance)]
        public double MinTickDistance
        {
            get { return (double)GetValue(MinTickDistanceProperty); }
            set { SetValue(MinTickDistanceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultLength, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the length of the axis line in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The length of the axis line in device-independent pixels. The default value is
        /// <see cref="DefaultLength"/>. When an invalid value, such as <see cref="Double.NaN"/> or
        /// <see cref="Double.PositiveInfinity"/>, is set the value is automatically reset to the
        /// default value.
        /// </value>
        [Description("Gets or sets length of the axis.")]
#if !SILVERLIGHT
        [TypeConverter(typeof(LengthConverter))]
#endif
        [Category(ChartCategories.Default)]
        public double Length
        {
            get { return (double)GetValue(LengthProperty); }
            set { SetValue(LengthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OneSidedTicks"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OneSidedTicksProperty = DependencyProperty.Register(
            "OneSidedTicks",
            typeof(bool),
            typeof(Axis),
            new PropertyMetadata(Boxed.BooleanFalse, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the ticks appear on both sides of the axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether the ticks appear on both sides of the axis. The default value
        /// is <see langword="false"/> which indicates that the ticks are only drawn at the side of
        /// the axis labels.
        /// </value>
        [Description("Gets or sets a value indicating whether the ticks appear on both sides of the axis.")]
        [Category(ChartCategories.Default)]
        public bool OneSidedTicks
        {
            get { return (bool)GetValue(OneSidedTicksProperty); }
            set { SetValue(OneSidedTicksProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(Axis),
            new PropertyMetadata(Orientation.Horizontal, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the orientation of the axis (horizontal = x-axis, vertical = y-axis).
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The orientation of the axis. The default value is
        /// <see cref="System.Windows.Controls.Orientation.Horizontal"/>.
        /// </value>
        [Description("Gets or sets the orientation of the axis (horizontal = x-axis, vertical = y-axis).")]
        [Category(ChartCategories.Default)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OriginX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginXProperty = DependencyProperty.Register(
            "OriginX",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultOriginX, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the horizontal position of the axis origin.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The position of the axis origin is relative to the upper left corner. The default
        /// value is (<see cref="DefaultOriginX"/>).
        /// </value>
        [Description("Gets or sets the horizontal position of the axis origin.")]
        [Category(ChartCategories.Default)]
        public double OriginX
        {
            get { return (double)GetValue(OriginXProperty); }
            set { SetValue(OriginXProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OriginY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OriginYProperty = DependencyProperty.Register(
            "OriginY",
            typeof(double),
            typeof(Axis),
            new PropertyMetadata(DefaultOriginY, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the vertical position of the axis origin.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The position of the axis origin is relative to the upper left corner. The default
        /// value is (<see cref="DefaultOriginY"/>).
        /// </value>
        [Description("Gets or sets the horizontal position of the axis origin.")]
        [Category(ChartCategories.Default)]
        public double OriginY
        {
            get { return (double)GetValue(OriginYProperty); }
            set { SetValue(OriginYProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Scale"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale",
            typeof(AxisScale),
            typeof(Axis),
            new PropertyMetadata(null, OnScaleChanged));

        /// <summary>
        /// Gets or sets the scale.
        /// This is a dependency property.
        /// </summary>
        /// <value>The axis scale. The default scale is a linear scale from 0 to 1.</value>
        /// <remarks>
        /// <para>
        /// The getter of this property will either return the value of the dependency property or a
        /// default scale (a linear scale with a range from 0 to 1) if the value of the dependency
        /// property is <see langword="null"/>.
        /// </para>
        /// <para>
        /// When <see cref="AutoScale"/> is <see langword="true"/> an appropriate scale is set
        /// automatically depending on the charts that use this axis.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the scale.")]
        [Category(ChartCategories.Default)]
        public AxisScale Scale
        {
            get { return (AxisScale)GetValue(ScaleProperty) ?? DefaultScale; }
            set { SetValue(ScaleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="UseAbsoluteTitleOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty UseAbsoluteTitleOffsetProperty = DependencyProperty.Register(
            "UseAbsoluteTitleOffset",
            typeof(bool),
            typeof(Axis),
            new PropertyMetadata(Boxed.BooleanFalse, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TitleOffset"/> is absolute.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if <see cref="TitleOffset"/> is absolute; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// If <see langword="true"/>, the value specified by <see cref="TitleOffset"/> is the
        /// absolute distance from the axis to the title. If <see langword="false"/>, the value
        /// specified by <see cref="TitleOffset"/> is added to the pre-calculated value to determine
        /// the position of the title.
        /// </remarks>
        [Description("Gets or sets a value indicating whether TitleOffset is absolute.")]
        [Category(ChartCategories.Default)]
        public bool UseAbsoluteTitleOffset
        {
            get { return (bool)GetValue(UseAbsoluteTitleOffsetProperty); }
            set { SetValue(UseAbsoluteTitleOffsetProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="Axis"/> class.
        /// </summary>
        public Axis()
        {
            DefaultStyleKey = typeof(Axis);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="Axis"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static Axis()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Axis), new FrameworkPropertyMetadata(typeof(Axis)));
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
            if (_canvas != null)
            {
                // Clean up previous canvas
                foreach (TextBlock tickTextBlock in _tickLabels)
                {
                    _canvas.Children.Remove(tickTextBlock);
                    _tickLabels.Clear();
                }

                _canvas.Children.Remove(_hitRectangle);
                _hitRectangle = null;
                _canvas = null;
            }

            _axisLine = null;
            _majorTickRenderer = null;
            _minorTickRenderer = null;

            if (_titleContentControl != null)
            {
                _titleContentControl.SizeChanged -= OnTitleSizeChanged;
                _titleContentControl = null;
            }

            base.OnApplyTemplate();

            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _axisLine = GetTemplateChild("PART_AxisLine") as Line;
            var majorTicksPath = GetTemplateChild("PART_MajorTicks") as Path;
            var minorTicksPath = GetTemplateChild("PART_MinorTicks") as Path;
            _titleContentControl = GetTemplateChild("PART_Title") as ContentControl;

            if (_canvas != null)
            {
                // Create an invisible rectangle that is used only for hit-testing.
                _hitRectangle = new Rectangle { Fill = new SolidColorBrush(Colors.Transparent) };
#if SILVERLIGHT
                Canvas.SetZIndex(_hitRectangle, -1);
#else
                Panel.SetZIndex(_hitRectangle, -1);
#endif
                _canvas.Children.Add(_hitRectangle);
            }

            if (majorTicksPath != null)
                _majorTickRenderer = new PathRenderer(majorTicksPath);

            if (minorTicksPath != null)
                _minorTickRenderer = new PathRenderer(minorTicksPath);

            if (_titleContentControl != null)
                _titleContentControl.SizeChanged += OnTitleSizeChanged;

            Invalidate();
        }


        /// <summary>
        /// Called when size of the axis title changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="SizeChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnTitleSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            Invalidate();
        }


        private static void OnRelevantPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var axis = (Axis)dependencyObject;
            axis.Invalidate();
        }


        private static void OnScaleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var axis = (Axis)dependencyObject;
            var newValue = (AxisScale)eventArgs.NewValue;
            axis.OnScaleChanged(newValue);
        }


        private void OnScaleChanged(AxisScale scale)
        {
            // Unsubscribe from previous scale.
            if (_scaleSubscription != null)
            {
                _scaleSubscription.Dispose();
                _scaleSubscription = null;
            }

            // Subscribe to scale using weak event pattern.
            if (scale != null)
            {
                _scaleSubscription =
                    WeakEventHandler<PropertyChangedEventHandler, PropertyChangedEventArgs>.Register(
                        scale,
                        this,
                        handler => new PropertyChangedEventHandler(handler),
                        (sender, handler) => sender.PropertyChanged += handler,
                        (sender, handler) => sender.PropertyChanged -= handler,
                        (listener, sender, eventArgs) => listener.Invalidate());
            }

            Invalidate();
        }


        /// <summary>
        /// Registers the chart element.
        /// </summary>
        /// <param name="chartElement">The chart element to register.</param>
        /// <remarks>
        /// <para>
        /// Registering a chart element is necessary when <see cref="AutoScale"/> is set to
        /// <see langword="true"/>. In this case the axis calls
        /// <see cref="IChartElement.SuggestXScale"/> or <see cref="IChartElement.SuggestYScale"/>
        /// on all registered elements to determine the optimal scale.
        /// </para>
        /// <para>
        /// Elements derived from <see cref="ChartElement"/> are automatically registered when an
        /// axis is assigned (see <see cref="IChartElement.XAxis"/>,
        /// <see cref="IChartElement.YAxis"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="chartElement"/> is <see langword="null"/>.
        /// </exception>
        internal void RegisterChartElement(IChartElement chartElement)
        {
            if (chartElement == null)
                throw new ArgumentNullException("chartElement");

            if (_chartElements.Contains(chartElement))
                return;

            _chartElements.Add(chartElement);

            if (AutoScale)
                Invalidate();
        }


        /// <summary>
        /// Unregisters the chart element.
        /// </summary>
        /// <param name="chartElement">The chart element to unregister.</param>
        internal void UnregisterChartElement(IChartElement chartElement)
        {
            _chartElements.Remove(chartElement);

            if (AutoScale)
                Invalidate();
        }


        /// <summary>
        /// Invalidates the axis.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method raises the <see cref="Invalidated"/> event. The axis will be updated at the
        /// beginning of the next measure pass. To update the axis immediately the method
        /// <see cref="Update()"/> should be called.
        /// </para>
        /// <para>
        /// When <see cref="Invalidate"/> is called multiple times before a new measure pass, only
        /// the first call raises the <see cref="Invalidated"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Update()"/>
        /// <seealso cref="Updated"/>
        public void Invalidate()
        {
            if (_isValid)
            {
                _isValid = false;
                InvalidateMeasure();
                OnInvalidate();
            }
        }


        /// <summary>
        /// Raises the <see cref="Invalidated"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnInvalidate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnInvalidate"/> method so
        /// that registered delegates receive the event.
        /// </para>
        /// </remarks>
        protected virtual void OnInvalidate()
        {
            _invalidatedEvent.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Updates (recalculates) the axis.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The axis is updated immediately, no <see cref="Invalidated"/> event is raised. After the
        /// axis is updated the event <see cref="Updated"/> is raised. All elements that depend on
        /// the axis (such as charts), need to be redrawn when this event occurs.
        /// </para>
        /// <para>
        /// When possible, <see cref="Invalidate"/> should be called instead of
        /// <see cref="Update()"/>. <see cref="Invalidate"/> performs a lazy update. The update
        /// occurs at the beginning of the next measure pass. Calling <see cref="Invalidate"/>
        /// multiple times is fast, whereas calling <see cref="Update()"/> multiple times is costly.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Updated"/>
        public void Update()
        {
            Size constraintSize = new Size(ActualWidth, ActualHeight);
            if (constraintSize == new Size(0, 0))
                constraintSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

            Update(constraintSize);
        }


        /// <summary>
        /// Updates (recalculates) the axis.
        /// </summary>
        /// <param name="constraintSize">The size constraint.</param>
        private void Update(Size constraintSize)
        {
            if (_isValid)
                return;

            ApplyTemplate();

            if (AutoScale)
                AutoGenerateScale();

            // Cache values of dependency properties for performance.
            _isXAxis = (Orientation == Orientation.Horizontal);
            _scale = Scale;
            _originX = OriginX;
            _originY = OriginY;
            _length = Length;
            _startPosition = GetComponent(new Point(_originX, _originY));
            _endPosition = _isXAxis ? (_startPosition + _length) : (_startPosition - _length);


            if (!Numeric.IsZeroOrPositiveFinite(_length)
                || !Numeric.IsZeroOrPositiveFinite(_originX)
                || !Numeric.IsZeroOrPositiveFinite(_originY)
                || _scale == null)
            {
                // Invalid values.
                // (Set IsValid to true. IsValid will be automatically cleared when a property changes.)
                _isValid = true;
                return;
            }

            // Compute tick values.
            double[] labelValues, majorTicks, minorTicks;
            _scale.ComputeTicks(_length, MinTickDistance, out labelValues, out majorTicks, out minorTicks);
            LabelValues = labelValues;
            MajorTicks = majorTicks;
            MinorTicks = minorTicks;
            _isValid = true;

            double perpendicularLength = 0;
            if (_canvas != null && Visibility != Visibility.Collapsed)
            {
                // Add child elements to canvas.
                UpdateAxis();
                UpdateTicks(constraintSize);
                UpdateTitle();

                // Update invisible rectangle which is used for hit testing.
                _hitRectangle.Width = _hitRectangleBounds.Width;
                _hitRectangle.Height = _hitRectangleBounds.Height;
                Canvas.SetLeft(_hitRectangle, _hitRectangleBounds.Left);
                Canvas.SetTop(_hitRectangle, _hitRectangleBounds.Top);

                // ----- Compute the optimal size.
                // In the axis direction the ideal size is the axis length.
                // In the other direction the ideal size depends on the tick size, label size and offsets.

                // Length normal (perpendicular) to the axis.
                perpendicularLength = _titleSize.Height + _effectiveTitleOffset;
                perpendicularLength = Math.Max(perpendicularLength, 0);
            }

            // Compute optimal size
            OptimalSize = (IsXAxis) ? new Size(_length, perpendicularLength)
                                    : new Size(perpendicularLength, _length);

            OnUpdate();
        }


        /// <summary>
        /// Raises the <see cref="Updated"/> event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUpdate"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUpdate"/> method so that
        /// registered delegates receive the event.
        /// </para>
        /// </remarks>
        protected virtual void OnUpdate()
        {
            _updatedEvent.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// Called to remeasure a control.
        /// </summary>
        /// <param name="constraint">The maximum size that the method can return.</param>
        /// <returns>
        /// The size of the control, up to the maximum specified by <paramref name="constraint"/>.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Update(constraint);
            base.MeasureOverride(constraint);

            // Compute desired size.
            Size size;
            if (IsXAxis)
            {
                // A horizontal axis.
                if (LabelsAboveAxis)
                    size = new Size(_originX + _length, _originY);
                else
                    size = new Size(_originX + _length, _originY + OptimalSize.Height);
            }
            else
            {
                // A vertical axis.
                if (LabelsAboveAxis)
                    size = new Size(_originX, _originY);
                else
                    size = new Size(_originX + OptimalSize.Width, _originY);
            }

            // Ensure that desired size does not exceed available size.
            if (!Numeric.IsNaN(constraint.Width) && constraint.Width < size.Width)
                size.Width = constraint.Width;

            if (!Numeric.IsNaN(constraint.Height) && constraint.Height < size.Height)
                size.Height = constraint.Height;

            return size;
        }


        /// <summary>
        /// Automatically generates the scale of the axis.
        /// </summary>
        /// <remarks>
        /// The scale is computed from the scales that are suggested by the chart elements (see
        /// <see cref="IChartElement.SuggestXScale"/>, <see cref="IChartElement.SuggestXScale"/>).
        /// </remarks>
        private void AutoGenerateScale()
        {
            AxisScale newScale = null;
            bool isXAxis = IsXAxis;

            foreach (var chartElement in _chartElements)
            {
                AxisScale suggestedScale = (isXAxis) ? chartElement.SuggestXScale()
                                                     : chartElement.SuggestYScale();
                if (suggestedScale != null)
                {
                    if (newScale == null)
                        newScale = suggestedScale;
                    else
                        newScale.Add(suggestedScale);
                }
            }

            Scale = newScale ?? new LinearScale();
        }


        private void UpdateAxis()
        {
            if (_axisLine == null)
                return;

            // Compute start and end of axis line.
            Point start = new Point(_originX, _originY);
            Point end = (IsXAxis) ? new Point(_originX + _length, _originY)
                                  : new Point(OriginX, _originY - _length);

            _axisLine.X1 = start.X;
            _axisLine.Y1 = start.Y;
            _axisLine.X2 = end.X;
            _axisLine.Y2 = end.Y;

            // Update invisible rectangle which is used for hit testing.
            _hitRectangleBounds = new Rect(start, end);
        }


        /// <summary>
        /// Creates the axis ticks and tick labels.
        /// </summary>
        /// <param name="constraintSize">The size constraint.</param>
        /// <remarks>
        /// The bounds are cached for hit testing.
        /// </remarks>
        private void UpdateTicks(Size constraintSize)
        {
            _internalTitleOffset = 0;

            // Add major tick marks.
            if (_majorTickRenderer != null)
            {
                _majorTickRenderer.Clear();
                using (var renderContext = _majorTickRenderer.Open())
                {
                    double tickLength = MajorTickLength;
                    if (tickLength > 0)
                    {
                        for (int i = 0; i < MajorTicks.Length; ++i)
                        {
                            Rect tickBounds = AddTick(MajorTicks[i], tickLength, renderContext);
                            if (tickBounds != Rect.Empty)
                                _hitRectangleBounds.Union(tickBounds);
                        }
                    }
                }
            }

            // Add minor tick marks.
            if (_minorTickRenderer != null)
            {
                _minorTickRenderer.Clear();
                using (var renderContext = _minorTickRenderer.Open())
                {
                    double tickLength = MinorTickLength;

                    if (tickLength > 0)
                    {
                        for (int i = 0; i < MinorTicks.Length; ++i)
                        {
                            Rect tickBounds = AddTick(MinorTicks[i], tickLength, renderContext);
                            if (tickBounds != Rect.Empty)
                                _hitRectangleBounds.Union(tickBounds);
                        }
                    }
                }
            }

            // Add/update tick labels.
            double offsetToAxis = Math.Max(MajorTickLength, MinorTickLength);
            offsetToAxis = ((OneSidedTicks) ? offsetToAxis : 0.5 * offsetToAxis);
            offsetToAxis += 2;
            for (int i = 0; i < LabelValues.Length; ++i)
            {
                Rect labelBounds = AddTickLabel(i, offsetToAxis, constraintSize);
                if (labelBounds != Rect.Empty)
                    _hitRectangleBounds.Union(labelBounds);
            }

            // Hide tick labels that are no longer used.
            for (int i = LabelValues.Length; i < _tickLabels.Count; ++i)
                _tickLabels[i].Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// Creates a tick on the axis.
        /// </summary>
        /// <param name="value">The value at which the ticks should be drawn.</param>
        /// <param name="tickLength">Length of the tick.</param>
        /// <param name="renderContext">The render context.</param>
        /// <returns>The bounding rectangle of the tick.</returns>
        private Rect AddTick(double value, double tickLength, PathRenderer.Context renderContext)
        {
            // Determine start and end point of the tick.
            double position = GetPosition(value);
            Point start;
            Point end;

            bool oneSidedTicks = OneSidedTicks;

            if (_isXAxis)
            {
                if (LabelsAboveAxis)
                {
                    start = oneSidedTicks ? new Point(position, _originY) : new Point(position, _originY + 0.5 * tickLength);
                    end = new Point(start.X, start.Y - tickLength);
                }
                else
                {
                    start = oneSidedTicks ? new Point(position, _originY) : new Point(position, _originY - 0.5 * tickLength);
                    end = new Point(start.X, start.Y + tickLength);
                }
            }
            else
            {
                if (LabelsAboveAxis)
                {
                    start = oneSidedTicks ? new Point(_originX, position) : new Point(_originX + 0.5 * tickLength, position);
                    end = new Point(start.X - tickLength, start.Y);
                }
                else
                {
                    start = oneSidedTicks ? new Point(_originX, position) : new Point(_originX - 0.5 * tickLength, position);
                    end = new Point(start.X + tickLength, start.Y);
                }
            }

            renderContext.DrawLine(start, end);

            _internalTitleOffset = Math.Max(_internalTitleOffset, tickLength);

            // Estimate bounds
            Rect bounds;
            if (IsXAxis)
            {
                if (LabelsAboveAxis)
                    bounds = new Rect(end.X - 0.5, end.Y, 1, tickLength);
                else
                    bounds = new Rect(start.X - 0.5, start.Y, 1, tickLength);
            }
            else
            {
                if (LabelsAboveAxis)
                    bounds = new Rect(end.X, end.Y - 0.5, tickLength, 1);
                else
                    bounds = new Rect(start.X, start.Y - 0.5, tickLength, 1);
            }

            return bounds;
        }


        /// <summary>
        /// Creates a tick label.
        /// </summary>
        /// <returns>The bounding rectangle of the tick labels.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", MessageId = "DigitalRune.Windows.Charts.AxisScale.GetText(System.Double)")]
        private Rect AddTickLabel(int index, double offsetToAxis, Size constraintSize)
        {
            AxisScale scale = _scale;
            double value = LabelValues[index];
            string text = scale.GetText(value);

            TextBlock tickLabel;
            if (index < _tickLabels.Count)
            {
                // Reuse existing TextBlock
                tickLabel = _tickLabels[index];

                // Reset size
                tickLabel.Width = Double.NaN;
                tickLabel.Height = Double.NaN;
            }
            else
            {
                // Add new TextBlock to canvas.
                tickLabel = new TextBlock();
                _tickLabels.Add(tickLabel);
                _canvas.Children.Add(tickLabel);
            }

            tickLabel.Text = text;
#if !SILVERLIGHT
            tickLabel.SnapsToDevicePixels = SnapsToDevicePixels;
#endif
            tickLabel.Style = LabelStyle;
            tickLabel.Tag = value;
            tickLabel.ClearValue(VisibilityProperty);

            // Hide text that is outside the scale or empty.
            if (value < scale.Min || value > scale.Max || string.IsNullOrEmpty(text))
            {
                tickLabel.Visibility = Visibility.Collapsed;
                return Rect.Empty;
            }

            // Measure size.
            tickLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            Size textSize = tickLabel.DesiredSize;
            if (textSize == new Size(0, 0))
            {
                // DesiredSize is not set in Silverlight. Use actual size instead.
                textSize.Width = tickLabel.ActualWidth;
                textSize.Height = tickLabel.ActualHeight;
            }

            // Compute the maximal width (in axis direction) of the text.
            // -> Get values for previous and next labels and make sure that they do not overlap.
            double position = GetPosition(value);
            double maxExtent = _length;

            if (index > 0)
            {
                // Make sure the label does not overlap with previous label.
                double previousValue = LabelValues[index - 1];
                double previousPosition = GetPosition(previousValue);
                maxExtent = Math.Min(maxExtent, Math.Abs(position - previousPosition));
            }

            if (index < LabelValues.Length - 1)
            {
                // Make sure label does not overlap with next label.
                double nextValue = LabelValues[index + 1];
                double nextPosition = GetPosition(nextValue);
                maxExtent = Math.Min(maxExtent, Math.Abs(nextPosition - position));
            }

            // Make sure that maxExtent is >= 0
            maxExtent = Math.Max(0, maxExtent);

            // Limit width or height.
            if (IsXAxis)
                textSize.Width = Math.Min(textSize.Width, maxExtent);
            else
                textSize.Height = Math.Min(textSize.Height, maxExtent);

            // Set bounds of tick label.
            tickLabel.Width = textSize.Width;
            tickLabel.Height = textSize.Height;

            // Determine start and end point of the tick.
            Point textStart;
            if (_isXAxis)
            {
                if (LabelsAboveAxis)
                {
                    Point pointOnAxis = new Point(position, _originY);
                    textStart = new Point(pointOnAxis.X - 0.5 * textSize.Width, pointOnAxis.Y - offsetToAxis - textSize.Height);
                }
                else
                {
                    Point pointOnAxis = new Point(position, _originY);
                    textStart = new Point(pointOnAxis.X - 0.5 * textSize.Width, pointOnAxis.Y + offsetToAxis);
                }

                // Keep labels inside available space.
                if (textStart.X < 0.0 && textStart.X + textSize.Width > 0.0)
                    textStart.X = 0.0;
                else if (textStart.X < constraintSize.Width && textStart.X + textSize.Width > constraintSize.Width)
                    textStart.X = constraintSize.Width - textSize.Width;

                _internalTitleOffset = Math.Max(_internalTitleOffset, offsetToAxis + textSize.Height);
            }
            else
            {
                if (LabelsAboveAxis)
                {
                    Point pointOnAxis = new Point(_originX, position);
                    textStart = new Point(pointOnAxis.X - offsetToAxis - textSize.Width, pointOnAxis.Y - 0.5 * textSize.Height);
                }
                else
                {
                    Point pointOnAxis = new Point(_originX, position);
                    textStart = new Point(pointOnAxis.X + offsetToAxis, pointOnAxis.Y - 0.5 * textSize.Height);
                }

                // Keep labels inside available space.
                if (textStart.Y < 0.0 && textStart.Y + textSize.Height > 0.0)
                    textStart.Y = 0.0;
                else if (textStart.Y < constraintSize.Height && textStart.Y + textSize.Height > constraintSize.Height)
                    textStart.Y = constraintSize.Height - textSize.Height;

                _internalTitleOffset = Math.Max(_internalTitleOffset, offsetToAxis + textSize.Width);
            }

            Rect bounds = new Rect(textStart.X, textStart.Y, textSize.Width, textSize.Height);
            Rect clipRectangle = (_isXAxis)
                                   ? new Rect(_originX, 0, Length, Double.PositiveInfinity)
                                   : new Rect(0, _originY - Length, Double.PositiveInfinity, _length);

            // Draw text if the text is inside the clip rectangle defined by the axis origin and
            // axis length, or draw if ClipLabels == false.
            if (!ClipLabels
                || (clipRectangle.Contains(new Point(bounds.Left, bounds.Top))
                    && clipRectangle.Contains(new Point(bounds.Right, bounds.Bottom))))
            {
                Canvas.SetLeft(tickLabel, textStart.X);
                Canvas.SetTop(tickLabel, textStart.Y);
            }
            else
            {
                // Text is clipped.
                tickLabel.Visibility = Visibility.Collapsed;
            }

            return bounds;
        }


        /// <summary>
        /// Creates the title that shows the name of the axis.
        /// </summary>
        private void UpdateTitle()
        {
            if (_titleContentControl == null)
                return;

            // Compute offset;
            _effectiveTitleOffset = UseAbsoluteTitleOffset ? TitleOffset : TitleOffset + _internalTitleOffset;

            // Determine middle of the axis
            Point center = _isXAxis ? new Point(_originX + _length / 2, _originY) : new Point(_originX, _originY - _length / 2);

            // Measure the size of the axis label.
            _titleContentControl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            _titleSize = _titleContentControl.DesiredSize;
            if (_titleSize == new Size(0, 0))
            {
                // DesiredSize is not set in Silverlight. Use actual size instead.
                _titleSize.Width = _titleContentControl.ActualWidth;
                _titleSize.Height = _titleContentControl.ActualHeight;
            }

            // Rotate text on y-axis.
            if (!_isXAxis)
                _titleContentControl.RenderTransform = new RotateTransform { Angle = -90 };
            else
                _titleContentControl.RenderTransform = null;

            // Determine position of label.
            Point titlePosition;
            if (_isXAxis)
            {
                if (LabelsAboveAxis)
                    titlePosition = new Point(center.X - _titleSize.Width / 2.0f, center.Y - _effectiveTitleOffset - _titleSize.Height);
                else
                    titlePosition = new Point(center.X - _titleSize.Width / 2.0f, center.Y + _effectiveTitleOffset);
            }
            else
            {
                if (LabelsAboveAxis)
                    titlePosition = new Point(center.X - _effectiveTitleOffset - _titleSize.Height, center.Y + _titleSize.Width / 2.0f);
                else
                    titlePosition = new Point(center.X + _effectiveTitleOffset, center.Y + _titleSize.Width / 2.0f);
            }

            // Position text on canvas.
            Canvas.SetLeft(_titleContentControl, titlePosition.X);
            Canvas.SetTop(_titleContentControl, titlePosition.Y);
        }


        #region ----- Other Methods -----

        /// <summary>
        /// Clips a data value against the scale.
        /// </summary>
        /// <param name="value">The data value.</param>
        /// <returns>
        /// The <paramref name="value"/> if it is inside the range of the scale; otherwise
        /// <see cref="AxisScale.Min"/> or <see cref="AxisScale.Max"/> is returned.
        /// </returns>
        public double ClampValue(double value)
        {
            return _scale.Clamp(value);
        }


        /// <summary>
        /// Gets the x component of a point if this is an x-axis, otherwise the y component.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        /// <see cref="Point.X"/> of <paramref name="point"/> if this is a horizontal axis;
        /// otherwise <see cref="Point.Y"/> of <paramref name="point"/>.
        /// </returns>
        private double GetComponent(Point point)
        {
            return _isXAxis ? point.X : point.Y;
        }


        /// <summary>
        /// Gets the position on the axis for a specified data value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The position of the data value on the axis (in device-independent pixels). If the axis
        /// is an x-axis, then this value is an x position. Otherwise it is a y position.
        /// </returns>
        /// <remarks>
        /// The positions are not clipped to the minimum and maximum axis positions; the returned
        /// position may be outside of the chart area.
        /// </remarks>
        /// <exception cref="ChartException">
        /// <see cref="Scale"/> is <see langword="null"/>.
        /// </exception>
        public double GetPosition(double value)
        {
            if (!_isValid)  // Note: Check is performed here to avoid unnecessary method call.
                Update();

            if (_scale == null)
                throw new ChartException("Axis.Scale is null.");

            return _scale.GetPosition(value, _startPosition, _endPosition);
        }


        /// <summary>
        /// Returns the data value of the projection of the given point onto the axis.
        /// </summary>
        /// <param name="point">The point to project onto the axis.</param>
        /// <returns>The data value of the projection of the point onto the axis.</returns>
        /// <remarks>
        /// The returned value is not clipped; so the returned value can be outside of the data
        /// range of the axis.
        /// </remarks>
        /// <exception cref="ChartException">
        /// <see cref="Scale"/> is <see langword="null"/>.
        /// </exception>
        public double GetValue(Point point)
        {
            if (!_isValid)  // Note: Check is performed here to avoid unnecessary method call.
                Update();

            if (_scale == null)
                throw new ChartException("Axis.Scale is null.");

            return _scale.GetValue(GetComponent(point), _startPosition, _endPosition);
        }


        /// <summary>
        /// Return the data value of the given position.
        /// </summary>
        /// <param name="position">
        /// The x position if the axis an x-axis; otherwise the y position.
        /// </param>
        /// <returns>The data value that corresponds to this position.</returns>
        /// <exception cref="ChartException">
        /// <see cref="Scale"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// The returned value is not clipped; so the returned value can be outside of the data
        /// range of the axis.
        /// </remarks>
        public double GetValue(double position)
        {
            if (!_isValid)  // Note: Check is performed here to avoid unnecessary method call.
                Update();

            if (_scale == null)
                throw new ChartException("Axis.Scale is null.");

            return _scale.GetValue(position, _startPosition, _endPosition);
        }


        /// <summary>
        /// Pans the axis by changing the scale.
        /// </summary>
        /// <param name="translation">The translation in device-independent pixels.</param>
        /// <remarks>
        /// <para>
        /// Panning does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/>
        /// or when the <see cref="Axis.Scale"/> is read-only.
        /// </para>
        /// </remarks>
        /// <exception cref="ChartException">
        /// <see cref="Scale"/> is <see langword="null"/>.
        /// </exception>
        public void Pan(Point translation)
        {
            if (_scale == null)
                throw new ChartException("Axis.Scale is null.");

            double relativeTranslation = GetComponent(translation) / _length;
            if (IsYAxis)
                relativeTranslation = -relativeTranslation;

            _scale.Pan(relativeTranslation);
        }


        /// <summary>
        /// Zooms the axis by changing its <see cref="Scale"/>.
        /// </summary>
        /// <param name="anchorPoint">
        /// The anchor position in device-independent pixels. When the scale is changed the data
        /// value at this position will remain the same.
        /// </param>
        /// <param name="zoomFactor">
        /// The relative zoom factor in the range (-1, 1). For example: A value of -0.5 increases
        /// the range of the scale by 50% ("zoom out"). A value of 0.1 reduces the range of the
        /// scale by 10% ("zoom in"). The scale does not change if the zoom factor is 0.
        /// </param>
        /// <remarks>
        /// Zooming does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/>
        /// or when the <see cref="Axis.Scale"/> is read-only.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="zoomFactor"/> is out of range. The zoom factor must be a value between
        /// -1 and 1. (-1 and 1 not included.)
        /// </exception>
        /// <exception cref="ChartException">
        /// <see cref="Scale"/> is <see langword="null"/>.
        /// </exception>
        public void Zoom(Point anchorPoint, double zoomFactor)
        {
            if (_scale == null)
                throw new ChartException("Axis.Scale is null.");

            double mouseValue = GetValue(anchorPoint);
            _scale.Zoom(mouseValue, zoomFactor);
        }
        #endregion

        #endregion
    }
}
