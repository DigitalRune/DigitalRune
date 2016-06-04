// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Allows the user to zoom using the mouse wheel.
    /// </summary>
    /// <remarks>
    /// The behavior has a dependency property <see cref="Zoom"/> which changes when the mouse
    /// wheel is rotated. This property can be data bound to a property of the view model or another
    /// dependency property in the view.
    /// </remarks>
    public class MouseWheelZoomBehavior : Behavior<FrameworkElement>
    {
        // TODO: MouseWheelZoomBehavior is not tested yet.

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
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
          typeof(MouseWheelZoomBehavior),
          new FrameworkPropertyMetadata(Boxed.BooleanTrue));

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
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="ModifierKeys"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModifierKeysProperty = DependencyProperty.Register(
          "ModifierKeys",
          typeof(ModifierKeys),
          typeof(MouseWheelZoomBehavior),
          new FrameworkPropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// Gets or sets the modifier keys that need to be pressed for zooming.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The modifier keys. The default value is <see cref="System.Windows.Input.ModifierKeys.None"/>.
        /// </value>
        [Description("Gets or sets the modifier keys that need to be pressed for zooming.")]
        [Category(Categories.Default)]
        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MaxZoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
          "MaxZoom",
          typeof(double),
          typeof(MouseWheelZoomBehavior),
          new FrameworkPropertyMetadata(10.0, OnMaxZoomChanged));

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The maximum zoom level. The default value is 10.0.</value>
        [Description("Gets or sets the maximum zoom level.")]
        [Category(Categories.Default)]
        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinZoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
          "MinZoom",
          typeof(double),
          typeof(MouseWheelZoomBehavior),
          new FrameworkPropertyMetadata(0.01, OnMinZoomChanged));

        /// <summary>
        /// Gets or sets the minimal zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The minimal zoom level. The default value is 0.01.</value>
        [Description("Gets or sets the minimal zoom level.")]
        [Category(Categories.Default)]
        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Zoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
          "Zoom",
          typeof(double),
          typeof(MouseWheelZoomBehavior),
          new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomChanged, CoerceZoom));

        /// <summary>
        /// Gets or sets the zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The zoom level. The default value is 1.0.</value>
        [Description("Gets or sets the zoom factor.")]
        [Category(Categories.Default)]
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="MaxZoom"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMaxZoomChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (MouseWheelZoomBehavior)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            element.OnMaxZoomChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="MaxZoom"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMaxZoomChanged(double oldValue, double newValue)
        {
            CoerceValue(ZoomProperty);
        }


        /// <summary>
        /// Called when the <see cref="MinZoom"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMinZoomChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (MouseWheelZoomBehavior)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            element.OnMinZoomChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="MinZoom"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnMinZoomChanged(double oldValue, double newValue)
        {
            CoerceValue(ZoomProperty);
        }


        /// <summary>
        /// Called when the <see cref="Zoom"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnZoomChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (MouseWheelZoomBehavior)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            element.OnZoomChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="Zoom"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnZoomChanged(double oldValue, double newValue)
        {
        }


        private static object CoerceZoom(DependencyObject dependencyObject, object baseValue)
        {
            var element = (MouseWheelZoomBehavior)dependencyObject;
            return element.CoerceZoom((double)baseValue);
        }


        private double CoerceZoom(double zoom)
        {
            double minZoom = MinZoom;
            if (zoom < minZoom)
                zoom = minZoom;

            double maxZoom = MaxZoom;
            if (zoom > maxZoom)
                zoom = maxZoom;

            return zoom;
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
            AssociatedObject.MouseWheel += OnMouseWheel;
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
            AssociatedObject.MouseWheel -= OnMouseWheel;
            base.OnDetaching();
        }


        private void OnMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            if (!IsEnabled || Keyboard.Modifiers != ModifierKeys)
                return;

#if SILVERLIGHT
            const double mouseWheelDeltaForOneLine = 120;
#else
            const double mouseWheelDeltaForOneLine = Mouse.MouseWheelDeltaForOneLine;
#endif

            double increments = eventArgs.Delta / mouseWheelDeltaForOneLine;
            double zoom = Zoom * Math.Pow(1.1, increments);
            zoom = CoerceZoom(zoom);
            Zoom = zoom;
            eventArgs.Handled = true;
        }
        #endregion
    }
}
