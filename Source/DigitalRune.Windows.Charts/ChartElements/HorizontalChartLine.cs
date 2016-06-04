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
    /// Draws a horizontal line inside a chart area.
    /// </summary>
    /// <remarks>
    /// <see cref="Y"/> defines the position of the horizontal lines. The line style can be defined
    /// using the property <see cref="LineStyle"/>.
    /// </remarks>
    [StyleTypedProperty(Property = "LineStyle", StyleTargetType = typeof(Line))]
    [TemplatePart(Name = "PART_Line", Type = typeof(Line))]
    public class HorizontalChartLine : ChartElement
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
            typeof(HorizontalChartLine),
            new PropertyMetadata((Style)null));

        /// <summary>
        /// Gets or sets the style that is used for the horizontal line.
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the style of the horizontal line.")]
        [Category(ChartCategories.Styles)]
        public Style LineStyle
        {
            get { return (Style)GetValue(LineStyleProperty); }
            set { SetValue(LineStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
            "Y",
            typeof(double),
            typeof(HorizontalChartLine),
            new PropertyMetadata(Boxed.DoubleZero, OnYChanged));

        /// <summary>
        /// Gets or sets the y-coordinate of the horizontal line.
        /// This is a dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        [Description("Gets or sets the y-coordinate of the horizontal line.")]
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
        /// Initializes a new instance of the <see cref="HorizontalChartLine"/> class.
        /// </summary>
        public HorizontalChartLine()
        {
            DefaultStyleKey = typeof(HorizontalChartLine);
        }
#else
        /// <summary>
        /// Initializes static members of the <see cref="HorizontalChartLine"/> class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static HorizontalChartLine()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HorizontalChartLine), new FrameworkPropertyMetadata(typeof(HorizontalChartLine)));
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnYChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (HorizontalChartLine)dependencyObject;
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
            double yValue = Y;
            if (Numeric.IsFinite(yValue))
            {
                Axis xAxis = XAxis;
                Axis yAxis = YAxis;
                AxisScale xScale = xAxis.Scale;
                AxisScale yScale = yAxis.Scale;
                double xMin = xAxis.GetPosition(xScale.Min);
                double xMax = xAxis.GetPosition(xScale.Max);
                double yPosition = yAxis.GetPosition(yValue);

                if (_line != null)
                {
                    _line.X1 = xMin;
                    _line.X2 = xMax;
                    _line.Y1 = yPosition;
                    _line.Y2 = yPosition;
                    _line.Visibility = (yScale.Min <= yValue && yValue <= yScale.Max)
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

            var line = new Line { X1 = 0, X2 = 16, Y1 = 8, Y2 = 8, Stretch = Stretch.Uniform };
            line.SetBinding(StyleProperty, new Binding("LineStyle") { Source = this });
            grid.Children.Add(line);

            return grid;
        }
        #endregion
    }
}
