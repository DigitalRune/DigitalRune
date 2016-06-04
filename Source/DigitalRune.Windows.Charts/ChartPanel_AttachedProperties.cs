// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    partial class ChartPanel
    {
        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.IsDataPoint"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the element represents a data point of a chart.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the element represents a data point of a chart; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// </AttachedPropertyComments>
        internal static readonly DependencyProperty IsDataPointProperty = DependencyProperty.RegisterAttached(
            "IsDataPoint",
            typeof(bool),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.IsDataPoint"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="element">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.IsDataPoint"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        internal static bool GetIsDataPoint(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (bool)element.GetValue(IsDataPointProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.IsDataPoint"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="element">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        internal static void SetIsDataPoint(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(IsDataPointProperty, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.ClipToChartArea"/>
        /// attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets a value indicating whether the chart should be clipped to the chart area.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to clip the chart to the chart area; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// The chart area of a <see cref="ChartPanel"/> is the rectangular region between the
        /// associated x-axis and the y-axis. If this property is set to <see langword="true"/>,
        /// visuals outside of this region are not visible. Set this property to
        /// <see langword="false"/> if drawing outside of the chart area should be allowed. (The
        /// default value is <see langword="false"/>.) When this property is set, the
        /// <see cref="ChartPanel"/> will automatically adjust the <see cref="UIElement.Clip"/>
        /// property of an element. If you want to set your own clipping geometry, then set this
        /// value to <see langword="false"/> and manually set <see cref="UIElement.Clip"/>.
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty ClipToChartAreaProperty = DependencyProperty.RegisterAttached(
            "ClipToChartArea",
            typeof(bool),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.ClipToChartArea"/> attached property
        /// from a given element.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.ClipToChartArea"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets a value indicating whether the element is clipped to the chart area.")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
#endif
        public static bool GetClipToChartArea(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (bool)element.GetValue(ClipToChartAreaProperty);
        }

        /// <summary>
        /// Sets the value of the
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.ClipToChartArea"/> attached property
        /// to a given element.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetClipToChartArea(DependencyObject element, bool value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(ClipToChartAreaProperty, value);
        }


        #region ----- XAxis, YAxis -----

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the x-axis assigned to the element.
        /// </summary>
        /// <value>The x-axis assigned to the element.</value>
        /// <remarks>
        /// <para>
        /// Each child of a <see cref="ChartPanel"/> needs to be assigned to an x-axis and a y-axis.
        /// The axes are set by using either the attached dependency properties
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/>, or by setting the local
        /// properties <see cref="ChartElement.XAxis"/> and <see cref="ChartElement.YAxis"/> if the
        /// element is derived from <see cref="ChartElement"/>. The attached dependency properties
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> can be set on any
        /// <see cref="UIElement"/>.
        /// </para>
        /// <para>
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> are mandatory. If these
        /// properties are not set, the child element of a <see cref="ChartPanel"/> won't be
        /// position properly.
        /// </para>
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty XAxisProperty = DependencyProperty.RegisterAttached(
            "XAxis",
            typeof(Axis),
            typeof(ChartPanel),
            new PropertyMetadata(null, OnXAxisChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/>
        /// attached property from a given element.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the associated x-axis.")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
#endif
        public static Axis GetXAxis(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (Axis)element.GetValue(XAxisProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/>
        /// attached property to a given element.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetXAxis(DependencyObject element, Axis value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(XAxisProperty, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the y-axis assigned to the element.
        /// </summary>
        /// <value>The y-axis assigned to the element.</value>
        /// <remarks>
        /// <para>
        /// Each child of a <see cref="ChartPanel"/> needs to be assigned to an x-axis and a y-axis.
        /// The axes are set by using either the attached dependency properties
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/>, or by setting the local
        /// properties <see cref="ChartElement.XAxis"/> and <see cref="ChartElement.YAxis"/> if the
        /// element is derived from <see cref="ChartElement"/>. The attached dependency properties
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> can be set on any
        /// <see cref="UIElement"/>.
        /// </para>
        /// <para>
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.XAxis"/> and
        /// <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> are mandatory. If these
        /// properties are not set, the child element of a <see cref="ChartPanel"/> won't be
        /// position properly.
        /// </para>
        /// </remarks>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty YAxisProperty = DependencyProperty.RegisterAttached(
            "YAxis",
            typeof(Axis),
            typeof(ChartPanel),
            new PropertyMetadata(null, OnYAxisChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/>
        /// attached property from a given element.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the associated y-axis.")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
#endif
        public static Axis GetYAxis(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (Axis)element.GetValue(YAxisProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.YAxis"/>
        /// attached property to a given element.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetYAxis(DependencyObject element, Axis value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(YAxisProperty, value);
        }
        #endregion


        #region ----- X1, Y1, X2, Y2 -----

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the x-coordinate of the element's left edge. The coordinate is a value on
        /// the assigned x-axis.
        /// </summary>
        /// <value>
        /// The x-coordinate of the element's left edge. The coordinate is a value on the assigned
        /// x-axis. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty X1Property = DependencyProperty.RegisterAttached(
            "X1",
            typeof(double),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.DoubleNaN, OnPositioningChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> attached 
        /// property from a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the left x-coordinate (data value).")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
        [TypeConverter(typeof(LengthConverter))]
#endif
        public static double GetX1(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (double)element.GetValue(X1Property);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X1"/> attached 
        /// property to a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetX1(DependencyObject element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(X1Property, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the y-coordinate of the element's top edge. The coordinate is a value on
        /// the assigned y-axis.
        /// </summary>
        /// <value>
        /// The y-coordinate of the element's top edge. The coordinate is a value on the assigned
        /// y-axis. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty Y1Property = DependencyProperty.RegisterAttached(
            "Y1",
            typeof(double),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.DoubleNaN, OnPositioningChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> attached
        /// property from a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the top y-coordinate (data value).")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
        [TypeConverter(typeof(LengthConverter))]
#endif
        public static double GetY1(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (double)element.GetValue(Y1Property);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y1"/> attached
        /// property to a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetY1(DependencyObject element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Y1Property, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the x-coordinate of the element's right edge. The coordinate is a value on
        /// the assigned x-axis.
        /// </summary>
        /// <value>
        /// The x-coordinate of the element's right edge. The coordinate is a value on the assigned
        /// x-axis. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty X2Property = DependencyProperty.RegisterAttached(
            "X2",
            typeof(double),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.DoubleNaN, OnPositioningChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> attached
        /// property from a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the right x-coordinate (data value).")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
        [TypeConverter(typeof(LengthConverter))]
#endif
        public static double GetX2(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (double)element.GetValue(X2Property);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.X2"/> attached
        /// property to a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetX2(DependencyObject element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(X2Property, value);
        }


        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> attached
        /// dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// Gets or sets the y-coordinate of the element's bottom edge. The coordinate is a value on
        /// the assigned y-axis.
        /// </summary>
        /// <value>
        /// The y-coordinate of the element's bottom edge. The coordinate is a value on the assigned
        /// y-axis. The default value is <see cref="double.NaN"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty Y2Property = DependencyProperty.RegisterAttached(
            "Y2",
            typeof(double),
            typeof(ChartPanel),
            new PropertyMetadata(Boxed.DoubleNaN, OnPositioningChanged));

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> attached
        /// property from a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [Description("Gets or sets the bottom y-coordinate (data value).")]
        [Category(ChartCategories.Default)]
#if !SILVERLIGHT
        [AttachedPropertyBrowsableForChildren]
        [TypeConverter(typeof(LengthConverter))]
#endif
        public static double GetY2(DependencyObject element)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            return (double)element.GetValue(Y2Property);
        }

        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Charts.ChartPanel.Y2"/> attached
        /// property to a given <see cref="UIElement"/> object.
        /// </summary>
        /// <param name="element">The element on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        public static void SetY2(DependencyObject element, double value)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            element.SetValue(Y2Property, value);
        }
        #endregion
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnPositioningChanged(DependencyObject element, DependencyPropertyChangedEventArgs eventArgs)
        {
            var chartPanel = element.GetVisualAncestors().OfType<ChartPanel>().FirstOrDefault();
            if (chartPanel != null)
                chartPanel.InvalidateArrange();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void OnXAxisChanged(DependencyObject element, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            var chartPanel = element.GetVisualAncestors()
                                    .OfType<ChartPanel>()
                                    .FirstOrDefault();

            if (element is IChartElement)
            {
                var chartElement = (IChartElement)element;
                var xAxis = (Axis)eventArgs.NewValue;

                // Synchronize the attached dependency property ChartPanel.XAxis with 
                // the dependency property ChartElement.XAxis.
                chartElement.XAxis = xAxis;
            }
            else if (element is UIElement)
            {
                // Size of the element might have changed.
                // --> Re-measure element.
                var uiElement = (UIElement)element;
                uiElement.InvalidateMeasure();

                // Reposition element in ChartPanel.
                if (chartPanel != null)
                    chartPanel.InvalidateArrange();
            }

            if (chartPanel != null)
                chartPanel.DetectAxes();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void OnYAxisChanged(DependencyObject element, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.OldValue == eventArgs.NewValue)
                return;

            var chartPanel = element.GetVisualAncestors()
                                    .OfType<ChartPanel>()
                                    .FirstOrDefault();

            if (element is IChartElement)
            {
                var chartElement = (IChartElement)element;
                var yAxis = (Axis)eventArgs.NewValue;

                // Synchronize the attached dependency property ChartPanel.XAxis with 
                // the dependency property ChartElement.XAxis.
                chartElement.YAxis = yAxis;
            }
            else if (element is UIElement)
            {
                // Size of the element might have changed.
                // --> Re-measure element.
                var uiElement = (UIElement)element;
                uiElement.InvalidateMeasure();

                // Reposition element in ChartPanel.
                if (chartPanel != null)
                    chartPanel.InvalidateArrange();
            }

            if (chartPanel != null)
                chartPanel.DetectAxes();
        }
        #endregion
    }
}
