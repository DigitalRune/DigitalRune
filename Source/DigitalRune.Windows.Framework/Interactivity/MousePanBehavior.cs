// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Allows to pan a <see cref="ScrollViewer"/> by pressing the a mouse button and dragging the
    /// mouse.
    /// </summary>
    /// <remarks>
    /// This behavior can be applied to any <see cref="UIElement"/> that contains a
    /// <see cref="ScrollViewer"/>. Panning with the mouse does not work if the
    /// <see cref="UIElement"/> already handles the mouse button and sets the corresponding mouse
    /// event to handled.
    /// </remarks>
    public class MousePanBehavior : Behavior<UIElement>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // TODO: Store all cursors in one place to save memory.
        private static readonly Cursor _grabCursor;
        private static readonly Cursor _grabbedCursor;
        private static IDisposable _cursorAnimation;

        private bool _isPanning;
        private Point _mouseDownPosition = new Point(double.NaN, double.NaN);
        private Point _originalScrollOffset = new Point(double.NaN, double.NaN);
        private ScrollViewer _scrollViewer;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(MousePanBehavior),
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
        [Category(Categories.Common)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="ModifierKeys"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register(
            "ModifierKeys",
            typeof(ModifierKeys),
            typeof(MousePanBehavior),
            new PropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// Gets or sets the modifier keys that need to be pressed for panning.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The modifier keys. The default value is
        /// <see cref="System.Windows.Input.ModifierKeys.None"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// In WPF: The mouse button specified in <strong>MouseButton</strong> needs to be pressed
        /// together with the keys specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// <para>
        /// In Silverlight: The left mouse button needs to be pressed together with the keys
        /// specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the modifier keys that need to be pressed for panning.")]
        [Category(Categories.Key)]
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
            typeof(MousePanBehavior),
            new PropertyMetadata(MouseButton.Middle));

        /// <summary>
        /// Gets or sets the mouse button that needs to be pressed for panning.
        /// This is a dependency property. (Not available in Silverlight.)
        /// </summary>
        /// <value>
        /// The mouse button that needs to be pressed for panning. The default value is
        /// <see cref="System.Windows.Input.MouseButton.Middle"/>.
        /// </value>
        /// <remarks>
        /// <para>This property is not available in Silverlight.</para>
        /// <para>
        /// In WPF the mouse button specified in <see cref="MouseButton"/> needs to be pressed
        /// together with the keys specified in <see cref="ModifierKeys"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the mouse button that needs to be pressed for panning.")]
        [Category(Categories.Mouse)]
        public MouseButton MouseButton
        {
            get { return (MouseButton)GetValue(MouseButtonProperty); }
            set { SetValue(MouseButtonProperty, value); }
        }
#endif
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="MousePanBehavior"/> class.
        /// </summary>
        static MousePanBehavior()
        {
            var streamResourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/DigitalRune.Windows.Framework;component/Resources/Hand.cur"));
            if (streamResourceInfo != null)
                _grabCursor = new Cursor(streamResourceInfo.Stream);

            streamResourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/DigitalRune.Windows.Framework;component/Resources/Grab.cur"));
            if (streamResourceInfo != null)
                _grabbedCursor = new Cursor(streamResourceInfo.Stream);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
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
        /// Called when the behavior is being detached from its
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            EndPanning(false);

#if SILVERLIGHT
            AssociatedObject.MouseLeftButtonDown -= OnMouseDown;
#else
            AssociatedObject.MouseDown -= OnMouseDown;
#endif

            base.OnDetaching();
        }


        private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
#if !SILVERLIGHT
            if (eventArgs.ChangedButton != MouseButton)
                return;
#endif

            if (IsEnabled && !eventArgs.Handled)
                eventArgs.Handled = BeginPanning(eventArgs);
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            Point mousePosition = eventArgs.GetPosition(AssociatedObject);
            UpdatePanning(mousePosition);
            eventArgs.Handled = true;
        }


        private void OnMouseUp(object sender, MouseButtonEventArgs eventArgs)
        {
#if !SILVERLIGHT
            if (eventArgs.ChangedButton != MouseButton)
                return;
#endif

            // Commit the panning.
            EndPanning(true);
            eventArgs.Handled = true;
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                // Cancel panning because user pressed <Escape>.
                EndPanning(false);
                eventArgs.Handled = true;
            }
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            // Cancel panning because the application lost the mouse capture.
            EndPanning(false);
        }


        private bool BeginPanning(MouseButtonEventArgs eventArgs)
        {
            // Look for the ScrollViewer, which will be manipulated.
            var source = eventArgs.OriginalSource as DependencyObject;
            if (source == null)
                source = eventArgs.Source as DependencyObject;

            if (source != null)
                _scrollViewer = source.GetVisualAncestors().OfType<ScrollViewer>().LastOrDefault();

            if (_scrollViewer == null)
                _scrollViewer = AssociatedObject.GetVisualSubtree().OfType<ScrollViewer>().LastOrDefault();

            if (_scrollViewer == null)
                return false;

#if !SILVERLIGHT
            // Abort if mouse is captured by other control.
            if (Mouse.Captured != null && Mouse.Captured != AssociatedObject)
                return false;
#endif

            // Try to capture the mouse.
            if (!AssociatedObject.CaptureMouse())
                return false;

            _isPanning = true;
            _mouseDownPosition = eventArgs.GetPosition(AssociatedObject);
            _originalScrollOffset = new Point(_scrollViewer.HorizontalOffset, _scrollViewer.VerticalOffset);

#if !SILVERLIGHT
            AssociatedObject.PreviewKeyDown += OnKeyDown;
#else
      AssociatedObject.KeyDown += OnKeyDown;
#endif
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
            AssociatedObject.MouseMove += OnMouseMove;
#if SILVERLIGHT
      AssociatedObject.MouseLeftButtonUp += OnMouseUp;
#else
            AssociatedObject.MouseUp += OnMouseUp;
#endif

#if !SILVERLIGHT
            if (!AssociatedObject.IsKeyboardFocusWithin)
#endif
            {
                // Focus control to receive keyboard events.
                AssociatedObject.Focus();
            }

            if (_grabCursor != null && _grabbedCursor != null)
            {
                if (_cursorAnimation != null)
                    _cursorAnimation.Dispose();

                Mouse.OverrideCursor = _grabCursor;
                _cursorAnimation = Observable.Interval(TimeSpan.FromMilliseconds(125))
                                             .Take(1)
                                             .ObserveOnDispatcher()
                                             .Subscribe(_ => Mouse.OverrideCursor = _grabbedCursor);
            }

            return true;
        }


        private void UpdatePanning(Point mousePosition)
        {
            bool panningCondition = PanningCondition();
            if (_isPanning && panningCondition)
            {
                Vector delta = mousePosition - _mouseDownPosition;
                _scrollViewer.ScrollToHorizontalOffset(_originalScrollOffset.X - delta.X);
                _scrollViewer.ScrollToVerticalOffset(_originalScrollOffset.Y - delta.Y);
            }
        }


        private void EndPanning(bool commit)
        {
            if (!_isPanning)
                return;

            if (!commit)
            {
                _scrollViewer.ScrollToHorizontalOffset(_originalScrollOffset.X);
                _scrollViewer.ScrollToVerticalOffset(_originalScrollOffset.Y);
            }

            _isPanning = false;
            _mouseDownPosition = new Point(double.NaN, double.NaN);
            _originalScrollOffset = new Point(double.NaN, double.NaN);
            _scrollViewer = null;

#if !SILVERLIGHT
            AssociatedObject.PreviewKeyDown -= OnKeyDown;
#else
      AssociatedObject.KeyDown -= OnKeyDown;
#endif
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;
            AssociatedObject.MouseMove -= OnMouseMove;
#if SILVERLIGHT
      AssociatedObject.MouseLeftButtonUp -= OnMouseUp;
#else
            AssociatedObject.MouseUp -= OnMouseUp;
#endif

            AssociatedObject.ReleaseMouseCapture();

            if (_grabCursor != null && _grabbedCursor != null)
            {
                if (_cursorAnimation != null)
                    _cursorAnimation.Dispose();

                Mouse.OverrideCursor = _grabCursor;
                _cursorAnimation = Observable.Interval(TimeSpan.FromMilliseconds(250))
                                             .Take(1)
                                             .ObserveOnDispatcher()
                                             .Subscribe(_ => Mouse.OverrideCursor = null);
            }
        }


        /// <summary>
        /// Checks whether the condition for panning is fulfilled (mouse buttons, keyboard
        /// modifiers).
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if <strong>MouseButton</strong> and <see cref="ModifierKeys"/>
        /// are pressed.
        /// </returns>
        private bool PanningCondition()
        {
            return MouseButtonPressed() && ModifierKeys == Keyboard.Modifiers;
        }


        /// <summary>
        /// Returns <see langword="true"/> when exactly the button specified in
        /// <strong>MouseButton</strong> is pressed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> when exactly the button specified in <strong>MouseButton</strong>
        /// is pressed.
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
