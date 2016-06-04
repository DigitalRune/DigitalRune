// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Draws coordinate cross (a horizontal and a vertical line) inside a chart area.
    /// </summary>
    /// <remarks>
    /// ( <see cref="X"/>, <see cref="Y"/>) defines the position of the horizontal and
    /// vertical lines. The line style can be defined using the properties
    /// <see cref="HorizontalLineStyle"/> and <see cref="VerticalLineStyle"/>.
    /// </remarks>
    [StyleTypedProperty(Property = "HorizontalLineStyle", StyleTargetType = typeof(Line))]
    [StyleTypedProperty(Property = "VerticalLineStyle", StyleTargetType = typeof(Line))]
    [TemplatePart(Name = "PART_HorizontalLine", Type = typeof(Line))]
    [TemplatePart(Name = "PART_VerticalLine", Type = typeof(Line))]
    public class ChartCross : ChartElement
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Line _horizontalLine;
        private Line _verticalLine;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="HorizontalLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalLineStyleProperty = DependencyProperty.Register(
            "HorizontalLineStyle",
            typeof(Style),
            typeof(ChartCross),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the horizontal line.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style of the horizontal line.")]
        [Category(ChartCategories.Styles)]
        public Style HorizontalLineStyle
        {
            get { return (Style)GetValue(HorizontalLineStyleProperty); }
            set { SetValue(HorizontalLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="VerticalLineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalLineStyleProperty = DependencyProperty.Register(
            "VerticalLineStyle",
            typeof(Style),
            typeof(ChartCross),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the vertical lines.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style of the vertical line.")]
        [Category(ChartCategories.Styles)]
        public Style VerticalLineStyle
        {
            get { return (Style)GetValue(VerticalLineStyleProperty); }
            set { SetValue(VerticalLineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(double),
            typeof(ChartCross),
            new PropertyMetadata(Boxed.DoubleZero, OnOriginChanged));

        /// <summary>
        /// Gets or sets x-coordinate of the vertical line.
        /// This is a dependency property.
        /// </summary>
        /// <value>The x-coordinate of the vertical line.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X"), Description("Gets or sets the x-coordinate of the vertical line.")]
        [Category(ChartCategories.Default)]
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(ChartCross),
            new PropertyMetadata(Boxed.DoubleZero, OnOriginChanged));

        /// <summary>
        /// Gets or sets y-coordinate of the horizontal line.
        /// This is a dependency property.
        /// </summary>
        /// <value>The x-coordinate of the horizontal line.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y"), Description("Gets or sets the y-coordinate of the horizontal line.")]
        [Category(ChartCategories.Default)]
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="ChartCross"/> class.
        /// </summary>
        public ChartCross()
        {
            DefaultStyleKey = typeof(ChartCross);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="ChartCross"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ChartCross()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartCross), new FrameworkPropertyMetadata(typeof(ChartCross)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnOriginChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (ChartCross)dependencyObject;
            element.Invalidate();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _horizontalLine = null;
            _verticalLine = null;

            base.OnApplyTemplate();
            _horizontalLine = GetTemplateChild("PART_HorizontalLine") as Line;
            _verticalLine = GetTemplateChild("PART_VerticalLine") as Line;

            Invalidate();
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
            Axis xAxis = XAxis;
            Axis yAxis = YAxis;
            AxisScale xScale = xAxis.Scale;
            AxisScale yScale = yAxis.Scale;

            double xValue = X;
            if (Numeric.IsFinite(xValue))
            {
                double yMin = yAxis.GetPosition(yScale.Min);
                double yMax = yAxis.GetPosition(yScale.Max);
                double xPosition = xAxis.GetPosition(xValue) + 0.5;

                if (_verticalLine != null)
                {
                    _verticalLine.X1 = xPosition;
                    _verticalLine.X2 = xPosition;
                    _verticalLine.Y1 = yMin;
                    _verticalLine.Y2 = yMax;
                    _verticalLine.Visibility = (xScale.Min <= xValue && xValue <= xScale.Max)
                                               ? Visibility.Visible
                                               : Visibility.Collapsed;
                }
            }
            else
            {
                if (_verticalLine != null)
                    _verticalLine.Visibility = Visibility.Collapsed;
            }

            double yValue = Y;
            if (Numeric.IsFinite(yValue))
            {
                double xMin = xAxis.GetPosition(xScale.Min);
                double xMax = xAxis.GetPosition(xScale.Max);
                double yPosition = yAxis.GetPosition(yValue) + 0.5;

                if (_horizontalLine != null)
                {
                    _horizontalLine.X1 = xMin;
                    _horizontalLine.X2 = xMax;
                    _horizontalLine.Y1 = yPosition;
                    _horizontalLine.Y2 = yPosition;
                    _horizontalLine.Visibility = (yScale.Min <= yValue && yValue <= yScale.Max)
                                                 ? Visibility.Visible
                                                 : Visibility.Collapsed;
                }
            }
            else
            {
                if (_horizontalLine != null)
                    _horizontalLine.Visibility = Visibility.Collapsed;
            }

            base.OnUpdate();
        }


        /// <inheritdoc/>
        protected override UIElement OnGetLegendSymbol()
        {
            var grid = new Grid
            {
                MinWidth = 16,
                MinHeight = 16,
            };

            var horizontalLine = new Line { X1 = 0, X2 = 16, Y1 = 8, Y2 = 8, Stretch = Stretch.Uniform };
            horizontalLine.SetBinding(StyleProperty, new Binding("HorizontalLineStyle") { Source = this });
            grid.Children.Add(horizontalLine);

            var verticalLine = new Line { X1 = 8, X2 = 8, Y1 = 0, Y2 = 16, Stretch = Stretch.Uniform };
            verticalLine.SetBinding(StyleProperty, new Binding("VerticalLineStyle") { Source = this });
            grid.Children.Add(verticalLine);

            return grid;
        }
        #endregion
    }
}
