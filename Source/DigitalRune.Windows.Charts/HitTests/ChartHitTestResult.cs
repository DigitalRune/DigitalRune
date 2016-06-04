// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents the result of a hit with a chart.
    /// </summary>
    public class ChartHitTestResult : ChartPanelHitTestResult
    {
        /// <summary>
        /// Gets or sets the <see cref="Charts.Chart"/> that was hit.
        /// </summary>
        /// <value>The chart hit.</value>
        public Chart Chart { get; set; }


        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        /// <value>
        /// The data if a data point was hit - usually a value of type <see cref="DataPoint"/> or
        /// the item from which the data point was created. Returns <see langword="null"/> if no
        /// data point was hit.
        /// </value>
        public object Data { get; set; }


        /// <summary>
        /// Gets or sets the x value of the hit (relative to the chart's x-axis).
        /// </summary>
        /// <value>The x value of the hit (relative to the chart's x-axis).</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        public double X { get; set; }


        /// <summary>
        /// Gets or sets the y value of the hit (relative to the chart's y-axis).
        /// </summary>
        /// <value>The y value of the hit (relative to the chart's y-axis).</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        public double Y { get; set; }


        /// <summary>
        /// Returns a <see cref="String"/> that represents the current
        /// <see cref="ChartHitTestResult"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current <see cref="ChartHitTestResult"/>.
        /// </returns>
        public override string ToString()
        {
            return String.Format(
              CultureInfo.InvariantCulture,
              "ChartHitTestResult{{Chart={0}, X={1}, Y={2}, Data={3}, Visual={4}}}",
              (Chart != null) ? Chart.Title : "null", X, Y, Data, Visual);
        }
    }
}
