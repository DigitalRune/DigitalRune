// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Automatically scrolls any <see cref="ScrollViewer"/> in the visual tree when an object is
    /// dragged near its border.
    /// </summary>
    /// <remarks>
    /// This behavior affects all <see cref="ScrollViewer"/>s within the visual tree.
    /// <see cref="ScrollViewer"/>s should not be nested - this can lead to unexpected results.
    /// </remarks>
    public class AutoScrollBehavior : Behavior<FrameworkElement>
    {
        //--------------------------------------------------------------
        #region Nested Types
        //--------------------------------------------------------------

        [Flags]
        private enum Direction
        {
            None = 0,
            Left = 1 << 0,
            Right = 1 << 1,
            Up = 1 << 2,
            Down = 1 << 3,
        }
        #endregion

        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const double AutoScrollTolerance = 32;      // Area near border in device-independent pixels.
        private const double AutoScrollDistance = 10;       // Distance scrolled in device-independent pixels.
        private static readonly TimeSpan AutoScrollInterval = new TimeSpan(33333);
        private static readonly TimeSpan HoverInterval = TimeSpan.Zero; // SystemParameters.MouseHoverTime;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ScrollViewer _scrollViewer;
        private DispatcherTimer _hoverTimer;
        private DispatcherTimer _scrollTimer;
        private Direction _scrollDirection;
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
            typeof(AutoScrollBehavior),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnIsEnabledChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the behavior is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the behavior is enabled.")]
        [Category(Categories.Common)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
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

            AssociatedObject.PreviewDragEnter += OnDragEnterOrOver;
            AssociatedObject.PreviewDragOver += OnDragEnterOrOver;
            AssociatedObject.PreviewDragLeave += OnDragLeave;
            AssociatedObject.PreviewMouseMove += OnMouseMove;
            AssociatedObject.GotMouseCapture += OnGotMouseCapture;
            AssociatedObject.LostMouseCapture += OnLostMouseCapture;
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewDragEnter -= OnDragEnterOrOver;
            AssociatedObject.PreviewDragOver -= OnDragEnterOrOver;
            AssociatedObject.PreviewDragLeave -= OnDragLeave;
            AssociatedObject.PreviewMouseMove -= OnMouseMove;
            AssociatedObject.GotMouseCapture -= OnGotMouseCapture;
            AssociatedObject.LostMouseCapture -= OnLostMouseCapture;

            base.OnDetaching();
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (AutoScrollBehavior)dependencyObject;
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)eventArgs.NewValue;
            behavior.OnIsEnabledChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="IsEnabled"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnIsEnabledChanged(bool oldValue, bool newValue)
        {
            if (!IsEnabled)
                EndAutoScroll();
        }


        private void OnDragEnterOrOver(object sender, DragEventArgs eventArgs)
        {
            if (!IsEnabled)
                return;

            _scrollViewer = GetScrollViewer(eventArgs);
            if (_scrollViewer == null)
                return;

            Point mousePosition = eventArgs.GetPosition(_scrollViewer);
            BeginAutoScroll(mousePosition);
        }


        private void OnDragLeave(object sender, DragEventArgs eventArgs)
        {
            EndAutoScroll();
        }


        private void OnGotMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            if (!IsEnabled)
                return;

            _scrollViewer = GetScrollViewer(eventArgs);
            if (_scrollViewer == null)
                return;

            Point mousePosition = eventArgs.GetPosition(_scrollViewer);
            BeginAutoScroll(mousePosition);
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            EndAutoScroll();
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            if (AssociatedObject.IsMouseCaptureWithin)
            {
                if (!IsEnabled)
                    return;

                // An element inside the view has captured the mouse.
                _scrollViewer = GetScrollViewer(eventArgs);
                if (_scrollViewer == null)
                    return;

                Point mousePosition = eventArgs.GetPosition(_scrollViewer);
                BeginAutoScroll(mousePosition);
            }
            else
            {
                // A regular mouse move event means that drag-and-drop is over.
                EndAutoScroll();
            }
        }


        private void OnWindowDeactivated(object sender, EventArgs eventArgs)
        {
            // Stop scrolling if the parent window is deactivated.
            // (E.g. Stop scrolling if another window, such as a message box has focus.)
            EndAutoScroll();
        }


        private static ScrollViewer GetScrollViewer(RoutedEventArgs eventArgs)
        {
            var element = eventArgs.OriginalSource as DependencyObject
                          ?? eventArgs.Source as DependencyObject;

            while (element != null)
            {
                var scrollViewer = element as ScrollViewer;
                if (scrollViewer != null)
                    return scrollViewer;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }


        private void BeginAutoScroll(Point mousePosition)
        {
            Debug.Assert(IsEnabled, "BeginAutoScroll should not be called when the behavior is disabled.");
            Debug.Assert(_scrollViewer != null, "The ScrollViewer needs to be set before BeginAutoScroll is called.");

            Size size = _scrollViewer.RenderSize;
            _scrollDirection = Direction.None;
            if (size.Width >= 2 * AutoScrollTolerance && _scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                // Check if mouse cursor is near horizontal border.

                // Inside or outside. (Problematic if there are adjacent ScrollViewers.)
                //if (Math.Abs(mousePosition.X) < AutoScrollTolerance)
                //    _scrollDirection |= Direction.Left;
                //else if (Math.Abs(mousePosition.X - size.Width) < AutoScrollTolerance)
                //    _scrollDirection |= Direction.Right;

                // Inside only.
                if (0 <= mousePosition.X && mousePosition.X < AutoScrollTolerance)
                    _scrollDirection |= Direction.Left;
                else if (mousePosition.X < size.Width && size.Width - mousePosition.X < AutoScrollTolerance)
                    _scrollDirection |= Direction.Right;
            }

            if (size.Height >= 2 * AutoScrollTolerance && _scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                // Check if mouse cursor is near vertical border.

                // Inside or outside.
                //if (Math.Abs(mousePosition.Y) < AutoScrollTolerance)
                //    _scrollDirection |= Direction.Up;
                //else if (Math.Abs(mousePosition.Y - size.Height) < AutoScrollTolerance)
                //    _scrollDirection |= Direction.Down;

                // Inside only.
                if (0 <= mousePosition.Y && mousePosition.Y < AutoScrollTolerance)
                    _scrollDirection |= Direction.Up;
                else if (mousePosition.Y < size.Height && mousePosition.Y - size.Height < AutoScrollTolerance)
                    _scrollDirection |= Direction.Down;
            }

            if (_scrollDirection == Direction.None)
            {
                EndAutoScroll();
                return;
            }

            if (_hoverTimer == null && _scrollTimer == null)
            {
                _hoverTimer = new DispatcherTimer { Interval = HoverInterval };
                _hoverTimer.Tick += OnMouseHover;
                _hoverTimer.Start();
            }

            // Track status of parent window. (Note: The event handler must be set here and cannot be
            // set in OnAttached because the parent window is initially null.)
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
                window.Deactivated += OnWindowDeactivated;
        }


        private void EndAutoScroll()
        {
            // Stop hover timer.
            if (_hoverTimer != null)
            {
                _hoverTimer.Stop();
                _hoverTimer.Tick -= OnMouseHover;
                _hoverTimer = null;
            }

            // Stop scroll timer.
            if (_scrollTimer != null)
            {
                _scrollTimer.Stop();
                _scrollTimer.Tick -= OnAutoScroll;
                _scrollTimer = null;
            }

            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
                window.Deactivated -= OnWindowDeactivated;

            _scrollViewer = null;
            _scrollDirection = Direction.None;
        }


        private void OnMouseHover(object sender, EventArgs eventArgs)
        {
            // Stop hover timer.
            if (_hoverTimer != null)
            {
                _hoverTimer.Stop();
                _hoverTimer.Tick -= OnMouseHover;
                _hoverTimer = null;
            }

            // Start scroll timer.
            if (_scrollTimer == null)
            {
                _scrollTimer = new DispatcherTimer { Interval = AutoScrollInterval };
                _scrollTimer.Tick += OnAutoScroll;
                _scrollTimer.Start();
            }
        }


        private void OnAutoScroll(object sender, EventArgs eventArgs)
        {
            Debug.Assert(IsEnabled);
            Debug.Assert(_scrollViewer != null);
            Debug.Assert(_scrollDirection != Direction.None);

            // Convert physical size (device independent pixels) to logical size.
            Size renderSize = _scrollViewer.RenderSize;
            double deltaX = AutoScrollDistance / renderSize.Width * _scrollViewer.ViewportWidth;
            double deltaY = AutoScrollDistance / renderSize.Height * _scrollViewer.ViewportHeight;

            if ((_scrollDirection & Direction.Left) == Direction.Left)
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset - deltaX);
            if ((_scrollDirection & Direction.Right) == Direction.Right)
                _scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + deltaX);
            if ((_scrollDirection & Direction.Up) == Direction.Up)
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset - deltaY);
            if ((_scrollDirection & Direction.Down) == Direction.Down)
                _scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + deltaY);
        }
        #endregion
    }
}
