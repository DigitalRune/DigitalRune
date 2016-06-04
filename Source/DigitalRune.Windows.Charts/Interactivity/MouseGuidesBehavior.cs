// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Draws a horizontal and vertical line on the chart area at the position of the mouse cursor.
    /// </summary>
    public class MouseGuidesBehavior : Behavior<ChartPanel>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties and Events
        //--------------------------------------------------------------

        private Line _horizontalLine;
        private Line _verticalLine;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------    

        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(MouseGuidesBehavior),
#if SILVERLIGHT
            new PropertyMetadata(Color.FromArgb(196, 0, 0, 0), OnPropertyChanged));
#else
            new PropertyMetadata(Color.FromArgb(255, 0, 0, 0), OnPropertyChanged));
#endif

        /// <summary>
        /// Gets or sets the color of the lines.
        /// This is a dependency property.
        /// </summary>
        /// <value>The color of the lines.</value>
        [Description("Gets or sets the color of the lines.")]
        [Category(Categories.Appearance)]
#if !SILVERLIGHT
        [TypeConverter(typeof(ColorConverter))]
#endif
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(MouseGuidesBehavior),
            new PropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether this behavior is enabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this behavior is enabled.")]
        [Category(Categories.Default)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Thickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
            "Thickness",
            typeof(double),
            typeof(MouseGuidesBehavior),
#if SILVERLIGHT
            new PropertyMetadata(1.0, OnPropertyChanged));
#else
            new PropertyMetadata(0.8, OnPropertyChanged));
#endif

        /// <summary>
        /// Gets or sets the thickness of the lines.
        /// This is a dependency property.
        /// </summary>
        /// <value>The thickness of the lines.</value>
        [Description("Gets or sets the thickness of the lines.")]
        [Category(Categories.Appearance)]
        public double Thickness
        {
            get { return (double)GetValue(ThicknessProperty); }
            set { SetValue(ThicknessProperty, value); }
        }


        #region ----- Axes -----

        /// <summary>
        /// Identifies the <see cref="XAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.Register(
            "XAxis",
            typeof(Axis),
            typeof(MouseGuidesBehavior),
            new PropertyMetadata(null, OnXAxisChangedStatic));

        /// <summary>
        /// Gets or sets the x-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated x-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>. The property is set automatically by the
        /// <see cref="ChartPanel"/> when the <see cref="Chart"/> is added to the panel.
        /// </value>
        /// <remarks>
        /// <para>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.XAxis</strong> attached dependency property.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the x-axis.")]
        [Category(ChartCategories.Default)]
        public Axis XAxis
        {
            get { return (Axis)GetValue(XAxisProperty); }
            set { SetValue(XAxisProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="YAxis"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.Register(
            "YAxis",
            typeof(Axis),
            typeof(MouseGuidesBehavior),
            new PropertyMetadata(null, OnYAxisChangedStatic));

        /// <summary>
        /// Gets or sets the y-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated y-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>. The property is set automatically by the
        /// <see cref="ChartPanel"/> when the <see cref="Chart"/> is added to the panel.
        /// </value>
        /// <remarks>
        /// <para>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.YAxis</strong> attached dependency property.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the y-axis.")]
        [Category(ChartCategories.Default)]
        public Axis YAxis
        {
            get { return (Axis)GetValue(YAxisProperty); }
            set { SetValue(YAxisProperty, value); }
        }
        #endregion
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnXAxisChangedStatic(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            // Synchronize the attached dependency property ChartPanel.XAxis with
            // the dependency property ChartElement.XAxis.
            Axis xAxis = (Axis)eventArgs.NewValue;
            ChartPanel.SetXAxis(dependencyObject, xAxis);
        }


        private static void OnYAxisChangedStatic(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            // Synchronize the attached dependency property ChartPanel.YAxis with
            // the dependency property ChartElement.YAxis.
            Axis yAxis = (Axis)eventArgs.NewValue;
            ChartPanel.SetYAxis(dependencyObject, yAxis);
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the <see cref="Behavior{T}.AssociatedObject"/>.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();

            ChartPanel chartPanel = AssociatedObject;
            if (chartPanel != null)
            {
                // Create lines.
                var stroke = GetStrokeBrush();
                _horizontalLine = new Line
                {
                    Stroke = stroke,
                    StrokeThickness = Thickness,
                    IsHitTestVisible = false
                };
                _verticalLine = new Line
                {
                    Stroke = stroke,
                    StrokeThickness = Thickness,
                    IsHitTestVisible = false
                };

                // Position lines in front.
#if SILVERLIGHT
                Canvas.SetZIndex(_horizontalLine, 1000);
                Canvas.SetZIndex(_verticalLine, 1000);
#else
                Panel.SetZIndex(_horizontalLine, 1000);
                Panel.SetZIndex(_verticalLine, 1000);
#endif

                // Add lines to canvas.
                chartPanel.Children.Add(_horizontalLine);
                chartPanel.Children.Add(_verticalLine);

                // Register event handlers.
                chartPanel.MouseMove += OnMouseChanged;
            }
        }


        private SolidColorBrush GetStrokeBrush()
        {
            var stroke = new SolidColorBrush(Color);
#if !SILVERLIGHT
            stroke.Freeze();
#endif
            return stroke;
        }


        /// <summary>
        /// Called when the <see cref="Behavior{T}"/> is about to detach from the
        /// <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// When this method is called, detaching can not be canceled. The
        /// <see cref="Behavior{T}.AssociatedObject"/> is still set.
        /// </remarks>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            ChartPanel chartPanel = AssociatedObject;
            if (chartPanel != null)
            {
                // Remove lines.
                chartPanel.Children.Remove(_horizontalLine);
                _horizontalLine = null;
                chartPanel.Children.Remove(_verticalLine);
                _verticalLine = null;

                // Unregister event handlers.
                chartPanel.MouseMove -= OnMouseChanged;
            }
        }


        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (MouseGuidesBehavior)dependencyObject;
            behavior.OnPropertyChanged();
        }


        private void OnPropertyChanged()
        {
            if (_horizontalLine != null && _verticalLine != null)
            {
                var stroke = GetStrokeBrush();
                _horizontalLine.Stroke = stroke;
                _verticalLine.Stroke = stroke;

                double strokeThickness = Thickness;
                _horizontalLine.StrokeThickness = strokeThickness;
                _verticalLine.StrokeThickness = strokeThickness;
            }
        }


        private void OnMouseChanged(object sender, MouseEventArgs eventArgs)
        {
            ChartPanel chartPanel = AssociatedObject;

            // Get new mouse position.
            Point position = eventArgs.GetPosition(chartPanel);
            UpdateGuidelines(position);
        }


        private void UpdateGuidelines(Point position)
        {
            if (IsEnabled)
            {
                ChartPanel chartPanel = AssociatedObject;
                Rect chartAreaBounds = new Rect();
                Axis xAxis = XAxis ?? chartPanel.Children
                                                .OfType<Axis>()
                                                .FirstOrDefault(axis => axis.IsXAxis);
                Axis yAxis = YAxis ?? chartPanel.Children
                                                .OfType<Axis>()
                                                .FirstOrDefault(axis => axis.IsYAxis);
                if (xAxis != null && yAxis != null)
                    chartAreaBounds = ChartPanel.GetChartAreaBounds(xAxis, yAxis);

                if (chartAreaBounds.Contains(position))
                {
                    // Mouse is in chart area --> update lines.
                    _horizontalLine.Visibility = Visibility.Visible;
                    _verticalLine.Visibility = Visibility.Visible;

                    // Change line position with a render transform. We do not 
                    // set the position directly to avoid a layout pass.
                    _horizontalLine.RenderTransform = new TranslateTransform { X = 0, Y = position.Y };
                    _verticalLine.RenderTransform = new TranslateTransform { X = position.X, Y = 0 };

                    // Update line lengths. Layouting does only occur if line length changes.
                    _verticalLine.Y1 = chartAreaBounds.Top;
                    _verticalLine.Y2 = chartAreaBounds.Bottom;
                    _horizontalLine.X1 = chartAreaBounds.Left;
                    _horizontalLine.X2 = chartAreaBounds.Right;
                }
                else
                {
                    // Mouse is outside chart area --> hide lines.
                    _horizontalLine.Visibility = Visibility.Collapsed;
                    _verticalLine.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Behavior is disabled. Hide lines.
                _horizontalLine.Visibility = Visibility.Collapsed;
                _verticalLine.Visibility = Visibility.Collapsed;
            }
        }
        #endregion
    }
}
