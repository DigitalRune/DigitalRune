// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Provides helper functions for charts.
    /// </summary>
    internal static class ChartHelper
    {
        /// <summary>
        /// Gets the localization/globalization information that applies to the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>CultureInfo.</returns>
        public static CultureInfo GetCulture(FrameworkElement element)
        {
            CultureInfo culture;
            try
            {
#if SILVERLIGHT
                culture = new CultureInfo(element.Language.IetfLanguageTag);
#else
                culture = element.Language.GetSpecificCulture();
#endif
            }
            catch (InvalidOperationException)
            {
                culture = CultureInfo.InvariantCulture;
            }

            return culture;
        }


        /// <summary>
        /// Swaps the values.
        /// </summary>
        /// <typeparam name="T">The type of the objects to swap.</typeparam>
        /// <param name="a">First value to swap.</param>
        /// <param name="b">Second value to swap.</param>
        public static void Swap<T>(ref T a, ref T b)
        {
            var dummy = a;
            a = b;
            b = dummy;
        }


        /// <summary>
        /// Sets the stroke (outline) and the fill (background) of an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="strokeBrush">The stroke brush.</param>
        /// <param name="fillBrush">The fill brush.</param>
        /// <remarks>
        /// Supports only elements of type <see cref="Shape"/> or <see cref="Control"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        public static void SetStrokeAndFill(FrameworkElement element, Brush strokeBrush, Brush fillBrush)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            if (element is Control)
            {
                var control = (Control)element;
                if (fillBrush != null)
                    control.Background = fillBrush;
                if (strokeBrush != null)
                    control.BorderBrush = strokeBrush;
            }
            else if (element is Shape)
            {
                var shape = (Shape)element;
                if (fillBrush != null)
                    shape.Fill = fillBrush;
                if (strokeBrush != null)
                    shape.Stroke = strokeBrush;
            }
            else
            {
                throw new ArgumentException("Unsupported element. Only elements of type Shape or Controls are allowed.", "element");
            }
        }


#if !SILVERLIGHT
        /// <summary>
        /// Defers the specified action until the application is idle.
        /// </summary>
        /// <param name="dispatcher">The dispatcher.</param>
        /// <param name="action">The action.</param>
        public static void Defer(Dispatcher dispatcher, Action action)
        {
            var element = Mouse.Captured as DependencyObject;
            if (element != null)
            {
                // Mouse interaction in progress.
                // --> Run the specified action when the mouse interaction is complete.
                MouseEventHandler handler = null;
                handler = (sender, args) =>
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Mouse.RemoveLostMouseCaptureHandler(element, handler);
                    action();
                };
                Mouse.AddLostMouseCaptureHandler(element, handler);
            }
            else
            {
                // Run the specified action when the application is idle.
                dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, action);
            }
        }
#endif
    }
}
