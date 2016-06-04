// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Allows the user to pan (scroll) the scale of an axis by dragging the axis with the mouse.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Panning does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/> or
    /// when the <see cref="Axis.Scale"/> is read-only.
    /// </para>
    /// </remarks>
    /// <seealso cref="Axis.Pan"/>
    public class AxisPanBehavior : Behavior<UIElement>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _isPanning;
        private Axis _selectedAxis;
        private double _axisMin, _axisMax;
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
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(AxisPanBehavior),
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
            typeof(AxisPanBehavior),
            new PropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// Gets or sets the modifier keys that need to be pressed for panning the scale of an axis.
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
        [Description("Gets or sets the modifier keys that need to be pressed for panning.")]
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
            typeof(AxisPanBehavior),
            new PropertyMetadata(MouseButton.Left));

        /// <summary>
        /// Gets or sets the mouse button that needs to be pressed for dragging a scale of an axis.
        /// This is a dependency property. (Not available in Silverlight.)
        /// </summary>
        /// <value>
        /// The mouse button that needs to be pressed for dragging a scale of an axis. The default
        /// value is <see cref="System.Windows.Input.MouseButton.Left"/>.
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


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
#if SILVERLIGHT
            bool buttonPressed = true;
#else
            bool buttonPressed = (eventArgs.ChangedButton == MouseButton);
#endif

            if (IsEnabled && buttonPressed && Keyboard.Modifiers == ModifierKeys)
            {
                var hitTestResult = ChartPanel.HitTest(AssociatedObject, eventArgs) as AxisHitTestResult;
                if (hitTestResult != null)
                {
                    _selectedAxis = hitTestResult.Axis;
                    if (_selectedAxis != null)
                    {
                        Point mousePosition = eventArgs.GetPosition(_selectedAxis);
                        eventArgs.Handled = BeginPanning(mousePosition);
                    }
                }
            }
        }


        private bool BeginPanning(Point mousePosition)
        {
            // Save original scales
            _axisMin = _selectedAxis.Scale.Min;
            _axisMax = _selectedAxis.Scale.Max;

            // Try to capture the mouse.
#if SILVERLIGHT
            bool captureMouse = true;
#else
            bool captureMouse = (Mouse.Captured == null || Mouse.Captured == AssociatedObject);
#endif

            bool mouseCaptured = false;
            if (captureMouse)
                mouseCaptured = AssociatedObject.CaptureMouse();

            if (mouseCaptured)
            {
                _isPanning = true;
                _lastMousePosition = mousePosition;
#if !SILVERLIGHT
                AssociatedObject.Focus();
                AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
#endif

                AssociatedObject.LostMouseCapture += OnLostMouseCapture;
                AssociatedObject.MouseMove += OnMouseMove;

#if SILVERLIGHT
                AssociatedObject.MouseLeftButtonUp += OnMouseUp;
#else
                AssociatedObject.MouseUp += OnMouseUp;
#endif

#if !SILVERLIGHT
                Mouse.OverrideCursor = _selectedAxis.IsYAxis ? Cursors.ScrollNS : Cursors.ScrollWE;
#endif
            }

            return true;
        }


        private void EndPanning(bool commit)
        {
            if (!_isPanning)
                return;

            _isPanning = false;


#if !SILVERLIGHT
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
#endif

            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.MouseMove -= OnMouseMove;

#if SILVERLIGHT
            AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
#else
            AssociatedObject.MouseUp -= OnMouseUp;
#endif

            AssociatedObject.ReleaseMouseCapture();

#if !SILVERLIGHT
            Mouse.OverrideCursor = null;
#endif

            if (!commit)
            {
                // Revert to the original scales
                _selectedAxis.Scale.Range = new DoubleRange(_axisMin, _axisMax);
            }
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            Point mousePosition = eventArgs.GetPosition(AssociatedObject);
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
            // -> Commit panning.
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
            bool buttonPressed = (eventArgs.ChangedButton == MouseButton);
#endif

            if (buttonPressed)
            {
                // Commit the panning.
                EndPanning(true);
            }
        }


        private void UpdatePanning(Point mousePosition)
        {
            bool panningCondition = PanningCondition();
            if (_isPanning && panningCondition)
                UpdateAxis(mousePosition);

            _lastMousePosition = mousePosition;
        }


        private void UpdateAxis(Point mousePosition)
        {
            Point translation = new Point(
              _lastMousePosition.X - mousePosition.X,
              _lastMousePosition.Y - mousePosition.Y);

            _selectedAxis.Pan(translation);
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
