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
    /// Allows the user to zoom the scale of an axis by using the mouse wheel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Zooming does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/> or
    /// when the <see cref="Axis.Scale"/> is read-only.
    /// </para>
    /// </remarks>
    /// <seealso cref="Axis.Zoom"/>
    public class AxisZoomBehavior : Behavior<UIElement>
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        private const double MinZoomFactor = -0.5;
        private const double MaxZoomFactor = 0.5;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties and Events
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
            typeof(AxisZoomBehavior),
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
            typeof(AxisZoomBehavior),
            new PropertyMetadata(ModifierKeys.None));

        /// <summary>
        /// Gets or sets the modifier keys that need to be pressed for zooming the scale of an axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The modifier keys. The default value is
        /// <see cref="System.Windows.Input.ModifierKeys.None"/>.
        /// </value>
        [Description("Gets or sets the modifier keys that need to be pressed for zooming.")]
        [Category(Categories.Default)]
        public ModifierKeys ModifierKeys
        {
            get { return (ModifierKeys)GetValue(ModifierKeysProperty); }
            set { SetValue(ModifierKeysProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ZoomFactor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(
            "ZoomFactor",
            typeof(double),
            typeof(AxisZoomBehavior),
            new PropertyMetadata(0.1));

        /// <summary>
        /// Gets or sets the zoom factor.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <para>
        /// The relative zoom factor in the range (-1, 1). For example: A value of -0.5 increases
        /// the range of the scale by 50% ("zoom out"). A value of 0.1 reduces the range of the
        /// scale by 10% ("zoom in"). The scale does not change if the zoom factor is 0.
        /// </para>
        /// <para>
        /// To invert the mouse wheel a negative value can be set as the zoom factor.
        /// </para>
        /// </value>
        [Description("Gets or sets the zoom factor.")]
        [Category(Categories.Default)]
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }
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


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void OnMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            if (!IsEnabled || Keyboard.Modifiers != ModifierKeys)
                return;

            var hitTestResult = ChartPanel.HitTest(AssociatedObject, eventArgs) as AxisHitTestResult;
            if (hitTestResult == null)
                return;

            var axis = hitTestResult.Axis;
            if (axis == null)
                return;

#if SILVERLIGHT
            const int mouseWheelDeltaForOneLine = 120;
#else
            const int mouseWheelDeltaForOneLine = Mouse.MouseWheelDeltaForOneLine;
#endif
            double zoomFactor = ZoomFactor * eventArgs.Delta / mouseWheelDeltaForOneLine;
            if (zoomFactor < MinZoomFactor)
                zoomFactor = MinZoomFactor;
            else if (zoomFactor > MaxZoomFactor)
                zoomFactor = MaxZoomFactor;

            Point mousePosition = eventArgs.GetPosition(axis);
            axis.Zoom(mousePosition, zoomFactor);
            eventArgs.Handled = true;
        }
        #endregion
    }
}
