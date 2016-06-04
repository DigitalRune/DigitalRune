// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Allows the user to pan (scroll) the charts by dragging the chart area with the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Panning does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/> or
    /// when the <see cref="Axis.Scale"/> is read-only.
    /// </para>
    /// <para>
    /// By default, all axes are affected by this behavior. Optionally, a predicate can be specified
    /// that defines which axes are affected. See <see cref="Axes"/> for more information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Axis.Pan"/>
    public class ChartPanBehavior : Behavior<UIElement>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ChartPanel _chartPanel;     // The chart panel is set during panning.
        private List<Axis> _affectedAxes;
        private double[] _axisMin;
        private double[] _axisMax;
        private Point _lastMousePosition;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------    
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Axes"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxesProperty = DependencyProperty.Register(
            "Axes",
            typeof(Predicate<Axis>),
            typeof(ChartPanBehavior),
            new PropertyMetadata((Predicate<Axis>)null));

        /// <summary>
        /// Gets or sets a predicate that determines which axes are affected by this behavior.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A predicate that determines which axes are affected by this behavior. The default value
        /// is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// By default, this behavior affects all axes in <see cref="ChartPanel"/>. By using the
        /// <see cref="Axes"/> property it is possible to define a predicate that selects the axes
        /// that are affected. The class <see cref="AxisPredicates"/> provides a set of criteria
        /// that can be used. But is also possible to provide any custom
        /// <see cref="Predicate{Axis}"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The standard predicates (see <see cref="AxisPredicates"/>) can be specified directly as
        /// a string.
        /// <code lang="xaml">
        /// <![CDATA[
        /// <dr:ChartPanBehavior Axes="AllAxes"/>
        /// <dr:ChartZoomBehavior Axes="XAxes"/>
        /// ]]>
        /// </code>
        /// When using a custom <see cref="Predicate{Axis}"/>, the predicate needs to specified
        /// using the <c>x:Static</c> markup extension or by using a binding.
        /// <code lang="xaml">
        /// <![CDATA[
        /// <dr:ChartPanBehavior Axes="{x:Static local:MyClass.MyPredicateForPanning}"/>
        /// <dr:ChartZoomBehavior Axes="{Binding MyPredicateForZooming}"/>
        /// ]]>
        /// </code>
        /// </example>
        [Description("Gets or sets a predicate that determines which axes are affected by this behavior.")]
        [Category(Categories.Default)]
        [TypeConverter(typeof(AxisPredicateConverter))]
        public Predicate<Axis> Axes
        {
            get { return (Predicate<Axis>)GetValue(AxesProperty); }
            set { SetValue(AxesProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(ChartPanBehavior),
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
        /// Identifies the <see cref="ModifierKeys"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register(
            "ModifierKeys",
            typeof(ModifierKeys),
            typeof(ChartPanBehavior),
            new PropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// Gets or sets the modifier keys that need to be pressed for dragging the chart area.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The modifier keys. The default value is
        /// <see cref="System.Windows.Input.ModifierKeys.None"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// In WPF: The mouse button specified in <see cref="MouseButton"/> needs to be pressed
        /// together with the keys specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// <para>
        /// In Silverlight: The left mouse button needs to be pressed together with the keys
        /// specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the modifier keys that need to be pressed for zooming.")]
        [Category(Categories.Default)]
        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }


#if !SILVERLIGHT
        /// <summary>
        /// Identifies the <see cref="MouseButton"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseButtonProperty = DependencyProperty.Register(
            "MouseButton",
            typeof(MouseButton),
            typeof(ChartPanBehavior),
            new PropertyMetadata(MouseButton.Left));

        /// <summary>
        /// Gets or sets the mouse button that needs to be pressed for dragging the chart area.
        /// This is a dependency property. (Not available in Silverlight.)
        /// </summary>
        /// <value>
        /// The mouse button that needs to be pressed for dragging the chart area. the default value
        /// is <see cref="System.Windows.Input.MouseButton.Left"/>.
        /// </value>
        /// <remarks>
        /// <para>This property is not available in Silverlight.</para>
        /// <para>
        /// In WPF the mouse button specified in <see cref="MouseButton"/> needs to be pressed
        /// together with the keys specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the mouse button that needs to be pressed for dragging a scale of an axis.")]
        [Category(Categories.Default)]
        public MouseButton MouseButton
        {
            get { return (MouseButton)GetValue(MouseButtonProperty); }
            set { SetValue(MouseButtonProperty, value); }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        /// <remarks>
        /// Override this to hook up functionality to the <see cref="Behavior{T}.AssociatedObject"/>.
        /// </remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
#if SILVERLIGHT
            AssociatedObject.MouseLeftButtonDown += OnMouseDown;
#else
            AssociatedObject.MouseDown += OnMouseDown;
#endif
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
#if SILVERLIGHT
            AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
#else
            AssociatedObject.MouseDown -= OnMouseDown;
#endif
            base.OnDetaching();
        }


        private static ChartPanel GetChartPanel(DependencyObject element)
        {
            while (element != null)
            {
                var chartPanel = element as ChartPanel;
                if (chartPanel != null)
                    return chartPanel;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
#if SILVERLIGHT
            bool buttonPressed = true;
#else
            bool buttonPressed = (eventArgs.ChangedButton == MouseButton);
#endif

            if (IsEnabled && buttonPressed)
            {
                _chartPanel = GetChartPanel(eventArgs.OriginalSource as DependencyObject);
                if (_chartPanel == null)
                    return;

                Point mousePosition = eventArgs.GetPosition(_chartPanel);
                var xAxes = _chartPanel.Axes
                                       .Where(axis => axis.IsXAxis)
                                       .ToArray();
                var yAxes = _chartPanel.Axes
                                       .Where(axis => axis.IsYAxis)
                                       .ToArray();

                Debug.Assert(_affectedAxes == null);
                Debug.Assert(_axisMin == null);
                Debug.Assert(_axisMax == null);

                _affectedAxes = new List<Axis>();
                foreach (Axis xAxis in xAxes)
                {
                    foreach (Axis yAxis in yAxes)
                    {
                        Rect chartAreaBounds = ChartPanel.GetChartAreaBounds(xAxis, yAxis);
                        if (chartAreaBounds.Contains(mousePosition))
                        {
                            // Mouse is on the chart area spanned by current axis pair.
                            // --> Axes will be affected by the panning.
                            if (!_affectedAxes.Contains(xAxis))
                                _affectedAxes.Add(xAxis);
                            if (!_affectedAxes.Contains(yAxis))
                                _affectedAxes.Add(yAxis);
                        }
                    }
                }

                if (_affectedAxes.Count > 0)
                {
                    // Begin dragging the chart area.
                    BeginPanning(mousePosition);
                }
                else
                {
                    // Do nothing.
                    _chartPanel = null;
                    _affectedAxes = null;
                }
            }
        }


        private void BeginPanning(Point mousePosition)
        {
            Debug.Assert(_chartPanel != null);

            // Save original scales
            _axisMin = _affectedAxes.Select(axis => axis.Scale.Min).ToArray();
            _axisMax = _affectedAxes.Select(axis => axis.Scale.Max).ToArray();

            // Try to capture the mouse.
#if SILVERLIGHT
            // In Silverlight: Always capture mouse.
            bool captureMouse = true;
#else
            // Only capture mouse if it is not already captured by a different element.
            bool captureMouse = (Mouse.Captured == null || Mouse.Captured == _chartPanel);
#endif

            bool mouseCaptured = false;
            if (captureMouse)
                mouseCaptured = _chartPanel.CaptureMouse();

            if (mouseCaptured)
            {
                _lastMousePosition = mousePosition;
#if !SILVERLIGHT
                _chartPanel.Focus();
                _chartPanel.PreviewKeyDown += OnPreviewKeyDown;
#endif
                _chartPanel.LostMouseCapture += OnLostMouseCapture;
                _chartPanel.MouseMove += OnMouseMove;

#if SILVERLIGHT
                _chartPanel.MouseLeftButtonUp += OnMouseUp;
#else
                _chartPanel.MouseUp += OnMouseUp;
                Mouse.OverrideCursor = Cursors.ScrollAll;
#endif
            }
            else
            {
                _chartPanel = null;
                _affectedAxes = null;
                _axisMin = null;
                _axisMax = null;
            }
        }


        private void EndPanning(bool commit)
        {
            if (_chartPanel == null)
                return;

#if !SILVERLIGHT
            _chartPanel.PreviewKeyDown -= OnPreviewKeyDown;
#endif

            _chartPanel.LostMouseCapture -= OnLostMouseCapture;
            _chartPanel.MouseMove -= OnMouseMove;

#if SILVERLIGHT
            _chartPanel.MouseLeftButtonUp -= OnMouseUp;
#else
            _chartPanel.MouseUp -= OnMouseUp;
#endif

            _chartPanel.ReleaseMouseCapture();

#if !SILVERLIGHT
            Mouse.OverrideCursor = null;
#endif

            if (!commit)
            {
                // Revert to the original scales
                for (int i = 0; i < _affectedAxes.Count; i++)
                    _affectedAxes[i].Scale.Range = new DoubleRange(_axisMin[i], _axisMax[i]);
            }

            _chartPanel = null;
            _affectedAxes = null;
            _axisMin = null;
            _axisMax = null;
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (_chartPanel == null)
                return;

            Point mousePosition = eventArgs.GetPosition(_chartPanel);
            UpdatePanning(mousePosition);
        }


#if !SILVERLIGHT
        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                // Cancel panning because user pressed <Escape>.
                EndPanning(false);
            }
        }
#endif


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            // End panning because ChartPanel lost the mouse capture.
            // (Possible application switch, or another interaction as released the mouse capture.)
#if SILVERLIGHT
            bool commit = true;
#else
            bool commit = !Keyboard.IsKeyDown(Key.Escape);
#endif
            EndPanning(commit);
        }


        private void OnMouseUp(object sender, MouseButtonEventArgs eventArgs)
        {
#if SILVERLIGHT
            bool buttonPressed = true;
#else
            bool buttonPressed = eventArgs.ChangedButton == MouseButton;
#endif

            if (buttonPressed)
            {
                // Commit the panning.
                EndPanning(true);
            }
        }


        private void UpdatePanning(Point mousePosition)
        {
            if (_chartPanel != null && PanningCondition())
                UpdateAxes(mousePosition);

            _lastMousePosition = mousePosition;
        }


        private void UpdateAxes(Point mousePosition)
        {
            Point translation = new Point(
              _lastMousePosition.X - mousePosition.X,
              _lastMousePosition.Y - mousePosition.Y);

            Predicate<Axis> isAxisAffected = Axes;
            foreach (Axis axis in _affectedAxes)
                if (isAxisAffected == null || isAxisAffected(axis))
                    axis.Pan(translation);
        }


        /// <summary>
        /// Checks whether the condition for panning is fulfilled (mouse buttons, keyboard modifiers).
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if <see cref="MouseButton"/> and <see cref="ModifierKeys"/> are 
        /// pressed.
        /// </returns>
        private bool PanningCondition()
        {
            return MouseButtonPressed() && ModifierKeys == Keyboard.Modifiers;
        }


        /// <summary>
        /// Returns <see langword="true"/> when exactly the button specified in 
        /// <see cref="MouseButton"/> is pressed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when exactly the button specified in <see cref="MouseButton"/> is 
        /// pressed.
        /// </returns>
        private bool MouseButtonPressed()
        {
#if SILVERLIGHT
            return true;
#else
            return (MouseButton == MouseButton.Left) == (Mouse.LeftButton == MouseButtonState.Pressed)
                   && (MouseButton == MouseButton.Middle) == (Mouse.MiddleButton == MouseButtonState.Pressed)
                   && (MouseButton == MouseButton.Right) == (Mouse.RightButton == MouseButtonState.Pressed)
                   && (MouseButton == MouseButton.XButton1) == (Mouse.XButton1 == MouseButtonState.Pressed)
                   && (MouseButton == MouseButton.XButton2) == (Mouse.XButton2 == MouseButtonState.Pressed);
#endif
        }
#endregion
    }
}
