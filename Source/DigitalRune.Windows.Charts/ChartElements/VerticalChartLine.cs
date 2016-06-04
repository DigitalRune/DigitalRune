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
    /// Draws a vertical line inside a chart area.
    /// </summary>
    /// <remarks>
    /// <see cref="X"/> defines the position of the vertical lines. The line style can be defined 
    /// using the property <see cref="LineStyle"/>.
    /// </remarks>
    [StyleTypedProperty(Property = "LineStyle", StyleTargetType = typeof(Line))]
    [TemplatePart(Name = "PART_Line", Type = typeof(Line))]
    public class VerticalChartLine : ChartElement
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Line _line;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="LineStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register(
            "LineStyle",
            typeof(Style),
            typeof(VerticalChartLine),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the vertical line.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style of the vertical line.")]
        [Category(ChartCategories.Styles)]
        public Style LineStyle
        {
            get { return (Style)GetValue(LineStyleProperty); }
            set { SetValue(LineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
            "X",
            typeof(double),
            typeof(VerticalChartLine),
            new PropertyMetadata(Boxed.DoubleZero, OnXChanged));

        /// <summary>
        /// Gets or sets the x-coordinate of the vertical line. 
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        [Description("Gets or sets the x-coordinate of the vertical line.")]
        [Category(ChartCategories.Default)]
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

#if SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="VerticalChartLine"/> class.
        /// </summary>
        public VerticalChartLine()
        {
            DefaultStyleKey = typeof(VerticalChartLine);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="VerticalChartLine"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static VerticalChartLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VerticalChartLine), new FrameworkPropertyMetadata(typeof(VerticalChartLine)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnXChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (VerticalChartLine)dependencyObject;
            element.Invalidate();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _line = null;

            base.OnApplyTemplate();
            _line = GetTemplateChild("PART_Line") as Line;

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
            double xValue = X;
            if (Numeric.IsFinite(xValue))
            {
                Axis xAxis = XAxis;
                Axis yAxis = YAxis;
                AxisScale xScale = xAxis.Scale;
                AxisScale yScale = yAxis.Scale;
                double yMin = yAxis.GetPosition(yScale.Min);
                double yMax = yAxis.GetPosition(yScale.Max);
                double xPosition = xAxis.GetPosition(xValue);

                if (_line != null)
                {
                    _line.X1 = xPosition;
                    _line.X2 = xPosition;
                    _line.Y1 = yMin;
                    _line.Y2 = yMax;
                    _line.Visibility = (xScale.Min <= xValue && xValue <= xScale.Max)
                                       ? Visibility.Visible
                                       : Visibility.Collapsed;
                }                  
            }
            else
            {
                if (_line != null)
                    _line.Visibility = Visibility.Collapsed;
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

            var line = new Line { X1 = 8, X2 = 8, Y1 = 0, Y2 = 16, Stretch = Stretch.Uniform };
            line.SetBinding(StyleProperty, new Binding("LineStyle") { Source = this });
            grid.Children.Add(line);

            return grid;
        }
        #endregion
    }
}
