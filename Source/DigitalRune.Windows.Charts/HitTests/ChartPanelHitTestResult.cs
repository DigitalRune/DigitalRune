// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents the result of a hit test on the <see cref="ChartPanel"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To perform a hit test call <see cref="Charts.ChartPanel.HitTest(Point)"/>. The hit test
    /// returns the chart element that was hit.
    /// </para>
    /// <para>
    /// Hit tests of charts work similar to WPF hit tests. However the class
    /// <see cref="ChartPanelHitTestResult"/> is not derived from <see cref="HitTestResult"/>.
    /// </para>
    /// </remarks>
    public class ChartPanelHitTestResult
    {
        /// <summary>
        /// Gets or sets the chart panel.
        /// </summary>
        /// <value>The chart panel.</value>
        public ChartPanel ChartPanel { get; set; }


        /// <summary>
        /// Gets or sets the visual object that was hit.
        /// </summary>
        /// <value>The visual hit.</value>
        public DependencyObject Visual { get; set; }


        /// <summary>
        /// Gets or sets the mouse position at which the hit occurred.
        /// </summary>
        /// <value>The mouse position of the hit relative to the <see cref="ChartPanel"/>.</value>
        public Point Position { get; set; }


        /// <summary>
        /// Returns a <see cref="String"/> that represents the current
        /// <see cref="ChartPanelHitTestResult"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current
        /// <see cref="ChartPanelHitTestResult"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
              CultureInfo.InvariantCulture,
              "ChartPanelHitTestResult{{ChartPanel={0}; Visual={1}; Position={2}}}",
              ChartPanel, Visual, Position);
        }
    }
}
