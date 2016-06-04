// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    partial class ChartPanel
    {
        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <overrides>
        /// <summary>
        /// Performs a hit test.
        /// </summary>
        /// </overrides>
        /// 
        /// <summary>
        /// Tests if the given point hits a chart element.
        /// </summary>
        /// <param name="point">
        /// The point (relative to the <see cref="ChartPanel"/>) to hit test against.
        /// </param>
        /// <returns>The hit test result, or <see langword="null"/> if nothing was hit.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="UIElement"/>s where <see cref="UIElement.IsHitTestVisible"/> is
        /// <see langword="false"/> will be ignored in the hit test.
        /// </para>
        /// <para>
        /// If the point comes from a mouse event and the <see cref="MouseEventArgs"/> are
        /// available, then the overload <see cref="HitTest(MouseEventArgs)"/> should be used.
        /// </para>
        /// </remarks>
        public ChartPanelHitTestResult HitTest(Point point)
        {
            return HitTest(this, point);
        }


        /// <summary>
        /// Tests if the given point (including a tolerance radius) hits a chart element.
        /// </summary>
        /// <param name="point">
        /// The point (relative to <see cref="ChartPanel"/>) to hit test against.
        /// </param>
        /// <param name="radius">The tolerance radius for the hit test.</param>
        /// <returns>The hit test result, or <see langword="null"/> if nothing was hit.</returns>
        /// <remarks>
        /// <see cref="UIElement"/>s where <see cref="UIElement.IsHitTestVisible"/> is
        /// <see langword="false"/> will be ignored in the hit test.
        /// </remarks>
        public ChartPanelHitTestResult HitTest(Point point, double radius)
        {
            return HitTest(this, point, radius);
        }


        /// <summary>
        /// Converts the <see cref="MouseEventArgs"/> into a <see cref="ChartPanelHitTestResult"/>.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        /// <returns>
        /// The <see cref="ChartPanelHitTestResult"/> for the specified mouse event.
        /// </returns>
        /// <inheritdoc cref="HitTest(Point)"/>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        public ChartPanelHitTestResult HitTest(MouseEventArgs eventArgs)
        {
            return HitTest(this, eventArgs);
        }


        /// <summary>
        /// Converts the <see cref="DragEventArgs"/> into a <see cref="ChartPanelHitTestResult"/>.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="DragEventArgs"/> instance containing the event data.
        /// </param>
        /// <returns>
        /// The <see cref="ChartPanelHitTestResult"/> for the specified drag event.
        /// </returns>
        /// <inheritdoc cref="HitTest(Point)"/>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        public ChartPanelHitTestResult HitTest(DragEventArgs eventArgs)
        {
            return HitTest(this, eventArgs);
        }


        /// <summary>
        /// Tests if the given point hits a chart element.
        /// </summary>
        /// <param name="reference">
        /// The element (usually the <see cref="ChartPanel"/>) to hit test.
        /// </param>
        /// <param name="point">
        /// The point (relative to <paramref name="reference"/>) to hit test against.
        /// </param>
        /// <returns>The hit test result, or <see langword="null"/> if nothing was hit.</returns>
        /// <inheritdoc cref="HitTest(Point)"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "WPF could use Visual instead of UIElement but Visual is not supported in Silverlight.")]
        public static ChartPanelHitTestResult HitTest(UIElement reference, Point point)
        {
            var visuals = GetVisualsAt(reference, point, 0.5);
            return GetHitTestResult(visuals, point);
        }


        /// <summary>
        /// Tests if the given point (including a tolerance radius) hits a chart element.
        /// </summary>
        /// <param name="reference">
        /// The element (usually the <see cref="ChartPanel"/>) to hit test.
        /// </param>
        /// <param name="point">
        /// The point (relative to <paramref name="reference"/>) to hit test against.
        /// </param>
        /// <param name="radius">The tolerance radius for the hit test.</param>
        /// <returns>The hit test result, or <see langword="null"/> if nothing was hit.</returns>
        /// <remarks>
        /// <see cref="UIElement"/>s where <see cref="UIElement.IsHitTestVisible"/> is
        /// <see langword="false"/> will be ignored in the hit test.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "WPF could use Visual instead of UIElement but Visual is not supported in Silverlight.")]
        public static ChartPanelHitTestResult HitTest(UIElement reference, Point point, double radius)
        {
            var visuals = GetVisualsAt(reference, point, radius);
            return GetHitTestResult(visuals, point);
        }


        /// <summary>
        /// Converts the <see cref="MouseEventArgs"/> into a <see cref="ChartPanelHitTestResult"/>.
        /// </summary>
        /// <param name="reference">
        /// The element (usually the <see cref="ChartPanel"/>) to hit test.
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        /// <returns>
        /// The <see cref="ChartPanelHitTestResult"/> for the specified mouse event.
        /// </returns>
        /// <inheritdoc cref="HitTest(Point)"/>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "WPF could use Visual instead of UIElement but Visual is not supported in Silverlight.")]
        public static ChartPanelHitTestResult HitTest(UIElement reference, MouseEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException("eventArgs");

            var visual = eventArgs.OriginalSource as UIElement;
            var point = eventArgs.GetPosition(reference);
            return GetHitTestResult(visual, point);
        }


        /// <summary>
        /// Converts the <see cref="DragEventArgs"/> into a <see cref="ChartPanelHitTestResult"/>.
        /// </summary>
        /// <param name="reference">
        /// The element (usually the <see cref="ChartPanel"/>) to hit test.
        /// </param>
        /// <param name="eventArgs">
        /// The <see cref="DragEventArgs"/> instance containing the event data.
        /// </param>
        /// <returns>
        /// The <see cref="ChartPanelHitTestResult"/> for the specified drag event.
        /// </returns>
        /// <inheritdoc cref="HitTest(Point)"/>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="eventArgs"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "WPF could use Visual instead of UIElement but Visual is not supported in Silverlight.")]
        public static ChartPanelHitTestResult HitTest(UIElement reference, DragEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException("eventArgs");

            var visual = eventArgs.OriginalSource as UIElement;
            var point = eventArgs.GetPosition(reference);
            return GetHitTestResult(visual, point);
        }


        /// <summary>
        /// Tests if the given point hits a chart element.
        /// </summary>
        /// <param name="reference">
        /// The reference element (usually the <see cref="ChartPanel" />) to hit test.
        /// Can be <see langword="null"/>!
        /// </param>
        /// <param name="hitVisual">
        /// The hit visual (e.g. the <see cref="RoutedEventArgs.OriginalSource"/> of a mouse event).
        /// </param>
        /// <param name="point">
        /// The point (relative to <paramref name="reference"/>) to hit test against.
        /// </param>
        /// <returns>ChartPanelHitTestResult.</returns>
        /// <exception cref="System.ArgumentNullException">eventArgs</exception>
        /// <inheritdoc cref="HitTest(Point)" />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "reference")]
        public static ChartPanelHitTestResult HitTest(UIElement reference, UIElement hitVisual, Point point)
        {
            // Parameter "reference" is only used because the signature HitTest(UIElement, Point)
            // is already in use.

            return GetHitTestResult(hitVisual, point);
        }


        private static ChartPanelHitTestResult GetHitTestResult(IEnumerable<UIElement> visuals, Point position)
        {
            foreach (var visual in visuals)
            {
                var hitTestResult = GetHitTestResult(visual, position);
                if (hitTestResult != null)
                    return hitTestResult;
            }

            return null;
        }


        private static ChartPanelHitTestResult GetHitTestResult(UIElement visual, Point position)
        {
            if (visual == null)
                return null;

            // WPF3.5 Bug: IsHitTestVisible needs to be checked manually.
            if (!visual.IsHitTestVisible)
                return null;

            var parent = visual;
            while (parent != null)
            {
                var chart = parent as Chart;
                if (chart != null)
                    return HandleChartHit(chart, visual, position);

                var axis = parent as Axis;
                if (axis != null)
                    return HandleAxisHit(axis, visual, position);

                var chartPanel = parent as ChartPanel;
                if (chartPanel != null)
                    return HandleChartPanelHit(chartPanel, visual, position);

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }


        private static ChartPanel GetChartPanel(UIElement visual)
        {
            var parent = visual;
            while (parent != null)
            {
                var chartPanel = parent as ChartPanel;
                if (chartPanel != null)
                    return chartPanel;

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }


        private static ChartPanelHitTestResult HandleChartPanelHit(ChartPanel chartPanel, UIElement element, Point point)
        {
            return new ChartPanelHitTestResult
            {
                ChartPanel = chartPanel,
                Visual = element,
                Position = point,
            };
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static ChartHitTestResult HandleChartHit(Chart chart, UIElement element, Point point)
        {
            var xAxis = chart.XAxis;
            var yAxis = chart.YAxis;

            object dataContext;
            if (element is FrameworkElement && GetIsDataPoint(element))
            {
                dataContext = ((FrameworkElement)element).DataContext;
            }
            else
            {
                dataContext = element.GetVisualAncestors()
                                     .OfType<FrameworkElement>()
                                     .Where(GetIsDataPoint)
                                     .Select(e => e.DataContext)
                                     .FirstOrDefault();
            }

            return new ChartHitTestResult
            {
                ChartPanel = GetChartPanel(chart),
                Chart = chart,
                Visual = element,
                Data = dataContext,
                Position = point,
                X = (xAxis != null) ? xAxis.GetValue(point) : Double.NaN,
                Y = (yAxis != null) ? yAxis.GetValue(point) : Double.NaN,
            };
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static AxisHitTestResult HandleAxisHit(Axis axis, UIElement element, Point point)
        {
            double value;
            if (element is FrameworkElement && ((FrameworkElement)element).Tag is double)
                value = (double)((FrameworkElement)element).Tag;  // The value for tick marks and tick labels is in the tag.
            else
                value = axis.GetValue(point);

            return new AxisHitTestResult
            {
                ChartPanel = GetChartPanel(axis),
                Axis = axis,
                Value = value,
                Visual = element,
            };
        }


#if SILVERLIGHT
        private static IEnumerable<UIElement> GetVisualsAt(UIElement reference, Point point, double tolerance)
        {
            // Silverlight computes hit-tests in host coordinates.
            // --> Transform point from local coordinates to host coordinates.
            UIElement host = Application.Current.RootVisual;
            point = reference.TransformToVisual(host).Transform(point);

            // Use rectangle to model a tolerance.
            Rect rectangle = new Rect(point.X - tolerance, point.Y - tolerance, 2 * tolerance, 2 * tolerance);

            // Let Silverlight compute the hit-test.
            return VisualTreeHelper.FindElementsInHostCoordinates(rectangle, reference);
        }
#else
        private static IEnumerable<UIElement> GetVisualsAt(Visual reference, Point point, double radius)
        {
            var visualHits = new List<UIElement>();
            VisualTreeHelper.HitTest(
                reference,
                null,
                result =>
                {
                    var element = result.VisualHit as UIElement;
                    if (element != null)
                        visualHits.Add(element);

                    return HitTestResultBehavior.Continue;
                },
                new GeometryHitTestParameters(new EllipseGeometry(point, radius, radius)));

            return visualHits;
        }
#endif
        #endregion
    }
}
