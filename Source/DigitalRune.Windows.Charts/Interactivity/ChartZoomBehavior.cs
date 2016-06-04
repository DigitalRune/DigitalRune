// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts.Interactivity
{
    /// <summary>
    /// Allows the user to zoom the chart area by using the mouse wheel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Zooming does not work when <see cref="Axis.AutoScale"/> is set to <see langword="true"/> or
    /// when the <see cref="Axis.Scale"/> is read-only.
    /// </para>
    /// <para>
    /// By default, all axes are affected by this behavior. Optionally, a predicate can be specified
    /// that defines which axes are affected. See <see cref="Axes"/> for more information.
    /// </para>
    /// </remarks>
    /// <seealso cref="Axis.Zoom"/>
    public class ChartZoomBehavior : Behavior<UIElement>
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
            typeof(ChartZoomBehavior),
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
            typeof(ChartZoomBehavior),
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
            typeof(ChartZoomBehavior),
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
        /// Identifies the <see cref="ZoomFactor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomFactorProperty = DependencyProperty.Register(
            "ZoomFactor",
            typeof(double),
            typeof(ChartZoomBehavior),
            new FrameworkPropertyMetadata(0.1));

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
        /// Override this to hook up functionality to the
        /// <see cref="Behavior{T}.AssociatedObject"/>.
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


        private void OnMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            if (!IsEnabled || Keyboard.Modifiers != ModifierKeys)
                return;

            var chartPanel = GetChartPanel(eventArgs.OriginalSource as DependencyObject);
            if (chartPanel == null)
                return;

            Point mousePosition = eventArgs.GetPosition(chartPanel);
            var xAxes = chartPanel.Axes
                                  .Where(axis => axis.IsXAxis)
                                  .ToArray();
            var yAxes = chartPanel.Axes
                                  .Where(axis => axis.IsYAxis)
                                  .ToArray();

            var affectedAxes = new List<Axis>();
            foreach (var xAxis in xAxes)
            {
                foreach (var yAxis in yAxes)
                {
                    Rect chartAreaBounds = ChartPanel.GetChartAreaBounds(xAxis, yAxis);
                    if (chartAreaBounds.Contains(mousePosition))
                    {
                        // Mouse is on the chart area spanned by current axis pair.
                        // --> Axes will be affected by the zooming.
                        if (!affectedAxes.Contains(xAxis))
                            affectedAxes.Add(xAxis);
                        if (!affectedAxes.Contains(yAxis))
                            affectedAxes.Add(yAxis);
                    }
                }
            }

            if (affectedAxes.Count <= 0)
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

            Predicate<Axis> isAxisAffected = Axes;
            foreach (Axis axis in affectedAxes)
                if (isAxisAffected == null || isAxisAffected(axis))
                    axis.Zoom(mousePosition, zoomFactor);

            eventArgs.Handled = true;
        }
        #endregion
    }
}
