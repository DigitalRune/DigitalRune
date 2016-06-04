// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Renders a sector of the pie chart including labels.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="PieChartItem"/> renders a sector of the pie chart including labels. The
    /// element supports two labels: An inner label (see <see cref="InnerLabel"/>) which is drawn
    /// inside the sector and an outer label (see <see cref="OuterLabel"/>) which is drawn next to
    /// the sector. A leader line connects the outer label to the sector.
    /// </para>
    /// <para>
    /// The sector is drawn using a <see cref="Path"/>. By default, <see cref="Control.Background"/>
    /// and the <see cref="Control.BorderBrush"/> of the <see cref="PieChartItem"/> determine the
    /// fill and the outline of the sector. However, this can be changed by changed by overriding
    /// the control template of the <see cref="PieChartItem"/> or by changing the
    /// <see cref="SectorStyle"/>.
    /// </para>
    /// <para>
    /// Labels can be set using the properties <see cref="InnerLabel"/> and
    /// <see cref="OuterLabel"/>. The default values of these properties are <see langword="null"/>
    /// - no data bindings are set. The labels are hidden if they are empty. The representation of
    /// the labels can be customized by setting data templates (see <see cref="InnerLabelTemplate"/>
    /// and <see cref="OuterLabelTemplate"/>).
    /// </para>
    /// <para>
    /// In certain cases, when sectors are too small, the labels should be hidden. The properties
    /// <see cref="InnerLabelClipAngle"/> and <see cref="OuterLabelClipAngle"/> define the
    /// thresholds at which the inner and outer labels are drawn. The threshold value is the angle
    /// of the sector in radians.
    /// </para>
    /// <para>
    /// The leader line from the sector to the outer label can be customized using the properties
    /// <see cref="LeaderLineLength"/> and <see cref="LeaderLineStyle"/>.
    /// </para>
    /// <para>
    /// The property <see cref="Offset"/> defines a translation of the sector from the center of the
    /// pie chart. This property can be set to create an "exploded pie chart".
    /// </para>
    /// </remarks>
    [StyleTypedProperty(Property = "SectorStyle", StyleTargetType = typeof(Path))]
    [StyleTypedProperty(Property = "LeaderLineStyle", StyleTargetType = typeof(Path))]
    [TemplatePart(Name = "PART_Sector", Type = typeof(Path))]
    [TemplatePart(Name = "PART_LeaderLine", Type = typeof(Path))]
    [TemplatePart(Name = "PART_InnerLabel", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_OuterLabel", Type = typeof(ContentControl))]
    public class PieChartItem : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isValid;
        private Path _sectorPath;
        private PathGeometry _sectorGeometry;
        private Path _leaderLinePath;
        private PathGeometry _leaderLineGeometry;
        private ContentControl _innerLabel;
        private ContentControl _outerLabel;
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
            typeof(PieChartItem),
            new PropertyMetadata(75.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the horizontal center of the pie chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>The horizontal center of the pie chart.</value>
        [Description("Gets or sets the horizontal center of the pie chart.")]
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
            typeof(PieChartItem),
            new PropertyMetadata(75.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the vertical center of the pie chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>The vertical center of the pie chart.</value>
        [Description("Gets or sets the vertical center of the pie chart.")]
        [Category(Categories.Layout)]
        public double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InnerRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register(
            "InnerRadius",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(Boxed.DoubleZero, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the inner radius of the pie chart sector in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The inner radius of the pie chart sector in device-independent pixels. The default value
        /// is 0.
        /// </value>
        [Description("Gets or sets the inner radius of the pie chart sector in device-independent pixels.")]
        [Category(Categories.Layout)]
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OuterRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterRadiusProperty = DependencyProperty.Register(
            "OuterRadius",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(50.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the outer radius of the pie chart sector in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>The outer radius of the pie chart sector. The default value is 50.</value>
        [Description("Gets or sets the outer radius of the pie chart sector in device-independent pixels.")]
        [Category(Categories.Layout)]
        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Offset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
            "Offset",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(Boxed.DoubleZero, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the offset of the sector from the center of the pie chart in
        /// device-independent pixels. This is a dependency property.
        /// </summary>
        /// <value>
        /// The offset of the sector from the center of the pie chart in device-independent pixels.
        /// The default value is 0.
        /// </value>
        [Description("Gets or sets the offset of the sector from the center of the pie chart in device-independent pixels.")]
        [Category(Categories.Layout)]
        public double Offset
        {
            get { return (double)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StartAngle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty = DependencyProperty.Register(
            "StartAngle",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(Math.PI / 6.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the start angle of the sector.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The start angle of the sector in radians. The default value is Pi/6 (= 30°).
        /// </value>
        /// <remarks>
        /// Start angle and end angle are measured clockwise starting at the top of the pie chart.
        /// </remarks>
        [Description("Gets or sets the start angle of the sector.")]
        [Category(Categories.Layout)]
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="EndAngle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty = DependencyProperty.Register(
            "EndAngle",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(Math.PI / 2.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the end angle of the sector.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The end angle of the sector in radians. The default value is Pi/2 (= 90°).
        /// </value>
        [Description("Gets or sets the end angle of the sector.")]
        [Category(Categories.Layout)]
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InnerLabel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelProperty = DependencyProperty.Register(
            "InnerLabel",
            typeof(object),
            typeof(PieChartItem),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the inner label which is shown inside the sector.
        /// This is a dependency property.
        /// </summary>
        /// <value>The inner label which is shown inside the sector.</value>
        [Description("Gets or sets the inner label which is shown inside the sector.")]
        [Category(ChartCategories.Default)]
        public object InnerLabel
        {
            get { return GetValue(InnerLabelProperty); }
            set { SetValue(InnerLabelProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InnerLabelClipAngle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelClipAngleProperty = DependencyProperty.Register(
            "InnerLabelClipAngle",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(Boxed.DoubleZero, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the angle at which inner labels are clipped.
        /// This is a dependency property.
        /// </summary>
        /// <value>The angle in radians at which inner labels are clipped.</value>
        /// <remarks>
        /// Inner and outer labels can be clipped when the angle of the pie chart sector becomes to
        /// small.
        /// </remarks>
        [Description("Gets or sets the angle at which inner labels are clipped.")]
        [Category(Categories.Layout)]
        public double InnerLabelClipAngle
        {
            get { return (double)GetValue(InnerLabelClipAngleProperty); }
            set { SetValue(InnerLabelClipAngleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InnerLabelTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerLabelTemplateProperty = DependencyProperty.Register(
          "InnerLabelTemplate",
          typeof(DataTemplate),
          typeof(PieChartItem),
          new PropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the data template that is applied to the inner label. 
        /// This is a dependency property.
        /// </summary>
        /// <value>The data template that is applied to the inner label.</value>
        [Description("Gets or sets the data template that is applied to the inner label.")]
        [Category(Categories.Default)]
        public DataTemplate InnerLabelTemplate
        {
            get { return (DataTemplate)GetValue(InnerLabelTemplateProperty); }
            set { SetValue(InnerLabelTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OuterLabel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelProperty = DependencyProperty.Register(
            "OuterLabel",
            typeof(object),
            typeof(PieChartItem),
            new PropertyMetadata(null, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the outer label which is shown next to the sector.
        /// This is a dependency property.
        /// </summary>
        /// <value>The outer label which is shown next to the sector.</value>
        [Description("Gets or sets the outer label which is shown next to the sector.")]
        [Category(ChartCategories.Default)]
        public object OuterLabel
        {
            get { return GetValue(OuterLabelProperty); }
            set { SetValue(OuterLabelProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OuterLabelClipAngle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelClipAngleProperty = DependencyProperty.Register(
          "OuterLabelClipAngle",
          typeof(double),
          typeof(PieChartItem),
          new PropertyMetadata(Boxed.DoubleZero, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the angle at which outer labels are clipped.
        /// This is a dependency property.
        /// </summary>
        /// <value>The angle in radians at which outer labels are clipped.</value>
        /// <inheritdoc cref="InnerLabelClipAngle"/>
        [Description("Gets or sets the angle at which outer labels are clipped.")]
        [Category(Categories.Layout)]
        public double OuterLabelClipAngle
        {
            get { return (double)GetValue(OuterLabelClipAngleProperty); }
            set { SetValue(OuterLabelClipAngleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OuterLabelTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OuterLabelTemplateProperty = DependencyProperty.Register(
            "OuterLabelTemplate",
            typeof(DataTemplate),
            typeof(PieChartItem),
            new PropertyMetadata((DataTemplate)null));

        /// <summary>
        /// Gets or sets the data template that is applied to the outer label.
        /// This is a dependency property.
        /// </summary>
        /// <value>The data template that is applied to the outer label.</value>
        [Description("Gets or sets the data template that is applied to the outer label.")]
        [Category(Categories.Default)]
        public DataTemplate OuterLabelTemplate
        {
            get { return (DataTemplate)GetValue(OuterLabelTemplateProperty); }
            set { SetValue(OuterLabelTemplateProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="LeaderLineLength"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeaderLineLengthProperty = DependencyProperty.Register(
            "LeaderLineLength",
            typeof(double),
            typeof(PieChartItem),
            new PropertyMetadata(20.0, OnRelevantPropertyChanged));

        /// <summary>
        /// Gets or sets the length of the leader line between the pie chart and the outer label in
        /// device-independent pixels. This is a dependency property.
        /// </summary>
        /// <value>
        /// The length of the line between the pie chart and the outer label in device-independent
        /// pixels.
        /// </value>
        [Description("Gets or sets the length of the leader line between the pie chart and the outer label in device-independent pixels.")]
        [Category(Categories.Layout)]
        public double LeaderLineLength
        {
            get { return (double)GetValue(LeaderLineLengthProperty); }
            set { SetValue(LeaderLineLengthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="LeaderLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LeaderLineStyleProperty = DependencyProperty.Register(
            "LeaderLineStyle",
            typeof(Style),
            typeof(PieChartItem),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the leader line.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style that is used for the leader line.</value>
        [Description("Gets or sets the style that is used for the leader line.")]
        [Category(ChartCategories.Styles)]
        public Style LeaderLineStyle
        {
            get { return (Style)GetValue(LeaderLineStyleProperty); }
            set { SetValue(LeaderLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SectorStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SectorStyleProperty = DependencyProperty.Register(
            "SectorStyle",
            typeof(Style),
            typeof(PieChartItem),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the sector of the pie chart.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style that is used for the sector of the pie chart.</value>
        [Description("Gets or sets the style that is used for the sector of the pie chart.")]
        [Category(ChartCategories.Styles)]
        public Style SectorStyle
        {
            get { return (Style)GetValue(SectorStyleProperty); }
            set { SetValue(SectorStyleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="PieChartItem"/> class.
        /// </summary>
        public PieChartItem()
        {
            DefaultStyleKey = typeof(PieChartItem);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="PieChartItem"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static PieChartItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PieChartItem), new FrameworkPropertyMetadata(typeof(PieChartItem)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when a relevant property is changed and the element needs to be updated.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnRelevantPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var pieChartItem = (PieChartItem)dependencyObject;
            pieChartItem.Invalidate();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Clean up.
            _sectorPath = null;
            _sectorGeometry = null;
            _leaderLinePath = null;
            _leaderLineGeometry = null;
            _innerLabel = null;
            _outerLabel = null;

            // Apply template.
            base.OnApplyTemplate();

            // Get template parts and prepare path geometries.
            _sectorPath = GetTemplateChild("PART_Sector") as Path ?? new Path();
            _leaderLinePath = GetTemplateChild("PART_LeaderLine") as Path ?? new Path();
            _innerLabel = GetTemplateChild("PART_InnerLabel") as ContentControl ?? new ContentControl();
            _outerLabel = GetTemplateChild("PART_OuterLabel") as ContentControl ?? new ContentControl();

            _sectorGeometry = _sectorPath.SetPathGeometry();
            _leaderLineGeometry = _leaderLinePath.SetPathGeometry();
            Invalidate();
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
            Update();
            return base.MeasureOverride(constraint);
        }


        private void Invalidate()
        {
            if (!_isValid)
                return;

            _isValid = false;
            InvalidateMeasure();
        }


        private void Update()
        {
            if (_isValid)
                return;

            _sectorGeometry.Clear();
            _leaderLineGeometry.Clear();

            // Fetch dependency properties. (Accessing dependency properties is expensive.)
            double centerX = CenterX;
            double centerY = CenterY;
            double innerRadius = InnerRadius;
            double outerRadius = OuterRadius;
            double offset = Offset;
            double startAngle = StartAngle;
            double endAngle = EndAngle;

            // Correct invalid values.
            if (innerRadius < 0)
                innerRadius = 0;
            if (outerRadius < 0)
                outerRadius = 0;
            if (innerRadius > outerRadius)
                innerRadius = outerRadius;
            if (startAngle > endAngle)
                ChartHelper.Swap(ref startAngle, ref endAngle);

            // The sector angle.
            double angle = endAngle - startAngle;

            // The thickness of the ring.
            double width = outerRadius - innerRadius;

            // The direction vector pointing from the center to the start of the sector.
            double startX, startY;
            GetDirection(startAngle, out startX, out startY);

            // The direction vector pointing from the center to the end of the sector.
            double endX, endY;
            GetDirection(endAngle, out endX, out endY);

            // The direction vector pointing from the center to the middle of the sector.
            double midX, midY;
            GetDirection((startAngle + endAngle) / 2, out midX, out midY);

            // Exploded pie charts: Translate the center of the pie chart.
            centerX += offset * midX;
            centerY += offset * midY;

            if (angle < 2 * Math.PI)
            {
                // ---- Draw sector.
                Point p0 = new Point(centerX + innerRadius * startX, centerY + innerRadius * startY);
                Point p1 = new Point(centerX + outerRadius * startX, centerY + outerRadius * startY);
                Point p2 = new Point(centerX + outerRadius * endX, centerY + outerRadius * endY);
                Point p3 = new Point(centerX + innerRadius * endX, centerY + innerRadius * endY);

                var figure = new PathFigure { StartPoint = p0 };
                figure.Segments.Add(new LineSegment { Point = p1 });
                figure.Segments.Add(new ArcSegment { Point = p2, Size = new Size(outerRadius, outerRadius), IsLargeArc = angle > Math.PI, SweepDirection = SweepDirection.Clockwise });
                figure.Segments.Add(new LineSegment { Point = p3 });
                figure.Segments.Add(new ArcSegment { Point = p0, Size = new Size(innerRadius, innerRadius), IsLargeArc = angle > Math.PI, SweepDirection = SweepDirection.Counterclockwise });
                _sectorGeometry.Figures.Add(figure);
            }
            else
            {
                if (innerRadius > 0)
                {
                    // ----- Draw full ring.
                    Point p0 = new Point(centerX, centerY - outerRadius);
                    Point p1 = new Point(centerX, centerY + outerRadius);
                    Point p2 = new Point(centerX, centerY - innerRadius);
                    Point p3 = new Point(centerX, centerY + innerRadius);

                    // Outer circle (= two half-circles).
                    var figure = new PathFigure { StartPoint = p0 };
                    figure.Segments.Add(new ArcSegment { Point = p1, Size = new Size(outerRadius, outerRadius), SweepDirection = SweepDirection.Clockwise });
                    figure.Segments.Add(new ArcSegment { Point = p0, Size = new Size(outerRadius, outerRadius), SweepDirection = SweepDirection.Clockwise });
                    _sectorGeometry.Figures.Add(figure);

                    // Inner circle (= two half-circles).
                    figure = new PathFigure { StartPoint = p2 };
                    figure.Segments.Add(new ArcSegment { Point = p3, Size = new Size(innerRadius, innerRadius), SweepDirection = SweepDirection.Clockwise });
                    figure.Segments.Add(new ArcSegment { Point = p2, Size = new Size(innerRadius, innerRadius), SweepDirection = SweepDirection.Clockwise });
                    _sectorGeometry.Figures.Add(figure);
                }
                else
                {
                    // ----- Draw full disc.
                    Point p0 = new Point(centerX, centerY - outerRadius);
                    Point p1 = new Point(centerX, centerY + outerRadius);

                    // Outer circle (= two half-circles).
                    var figure = new PathFigure { StartPoint = p0 };
                    figure.Segments.Add(new ArcSegment { Point = p1, Size = new Size(outerRadius, outerRadius), SweepDirection = SweepDirection.Clockwise });
                    figure.Segments.Add(new ArcSegment { Point = p0, Size = new Size(outerRadius, outerRadius), SweepDirection = SweepDirection.Clockwise });
                    _sectorGeometry.Figures.Add(figure);
                }
            }

            // ----- Draw inner label.
            if (InnerLabel != null && angle > InnerLabelClipAngle)
            {
                _innerLabel.Visibility = Visibility.Visible;

                _innerLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size size = _innerLabel.DesiredSize;
                if (size.Width > 0 && size.Height > 0)
                {
                    double x = centerX + innerRadius * midX + 0.6 * width * midX;
                    double y = centerY + innerRadius * midY + 0.6 * width * midY;
                    x -= size.Width / 2;
                    y -= size.Height / 2;
                    Canvas.SetLeft(_innerLabel, x);
                    Canvas.SetTop(_innerLabel, y);
                }
            }
            else
            {
                _innerLabel.Visibility = Visibility.Collapsed;
            }

            // ----- Draw outer label.
            if (OuterLabel != null && angle > OuterLabelClipAngle)
            {
                _outerLabel.Visibility = Visibility.Visible;

                _outerLabel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size size = _outerLabel.DesiredSize;
                if (size.Width > 0 && size.Height > 0)
                {
                    // Draw leader line.
                    double outerLabelDistance = LeaderLineLength;
                    double l1 = 0.667 * outerLabelDistance; // Segment normal to circle.
                    double l2 = 0.333 * outerLabelDistance; // Horizontal segment.        
                    double d = 0.5 * l2; // Distance between line and label.

                    Point p0 = new Point(centerX + outerRadius * midX, centerY + outerRadius * midY);
                    Point p1 = new Point(centerX + (outerRadius + l1) * midX, centerY + (outerRadius + l1) * midY);
                    Point p2 = new Point(p1.X + l2 * Math.Sign(midX), p1.Y);

                    var line = new PathFigure { StartPoint = p0 };
                    line.Segments.Add(new LineSegment { Point = p1 });
                    line.Segments.Add(new LineSegment { Point = p2 });
                    _leaderLineGeometry.Figures.Add(line);

                    if (midX >= 0)
                    {
                        // Label on the right.
                        Canvas.SetLeft(_outerLabel, p2.X + d);
                        Canvas.SetTop(_outerLabel, p2.Y - size.Height / 2);
                    }
                    else
                    {
                        // Label on the left.
                        Canvas.SetLeft(_outerLabel, p2.X - d - size.Width);
                        Canvas.SetTop(_outerLabel, p2.Y - size.Height / 2);
                    }
                }
            }
            else
            {
                _outerLabel.Visibility = Visibility.Collapsed;
            }

            _isValid = true;
        }


        /// <summary>
        /// Gets the unit vector pointing from the center of the pie chart into a specified
        /// direction.
        /// </summary>
        /// <param name="angle">
        /// The angle in radians. The angle is measured clockwise starting at the top of the pie
        /// chart.
        /// </param>
        /// <param name="x">The x-component of the direction.</param>
        /// <param name="y">The y-component of the direction.</param>
        private static void GetDirection(double angle, out double x, out double y)
        {
            x = Math.Sin(angle);
            y = -Math.Cos(angle);
        }
        #endregion
    }
}
