// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents the result of a hit with an <see cref="Axis"/>.
    /// </summary>
    public class AxisHitTestResult : ChartPanelHitTestResult
    {
        /// <summary>
        /// Gets or sets the <see cref="Axis"/> that was hit.
        /// </summary>
        /// <value>The axis hit.</value>
        public Axis Axis { get; set; }


        /// <summary>
        /// Gets or sets the value on the axis that was hit.
        /// </summary>
        /// <value>The value on the axis that was hit.</value>
        public double Value { get; set; }


        /// <summary>
        /// Returns a <see cref="String"/> that represents the current
        /// <see cref="AxisHitTestResult"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents the current <see cref="AxisHitTestResult"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
              CultureInfo.InvariantCulture,
              "AxisHitTestResult{{Axis={0}, Value={1}, Visual={2}}}",
              (Axis != null) ? Axis.Title : "null",
              Value,
              Visual);
        }
    }
}
