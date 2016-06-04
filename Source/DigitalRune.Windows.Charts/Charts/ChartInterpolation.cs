// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// The interpolation used for line charts.
    /// </summary>
    public enum ChartInterpolation
    {
        /// <summary>
        /// Linear interpolation is performed between two points. That is, all points are connected
        /// by straight lines.
        /// </summary>
        Linear,

        /// <summary>
        /// A piecewise constant interpolation is performed between two points. Points are connected
        /// with steps where the horizontal part of a step is centered at the data point.
        /// </summary>
        CenteredSteps,

        /// <summary>
        /// A piecewise constant interpolation is performed between two points. Points are connected
        /// with steps where the horizontal part of a step ends at the data point.
        /// </summary>
        LeftSteps,

        /// <summary>
        /// A piecewise constant interpolation is performed between two points. That is, all points
        /// are connected with steps where the horizontal part of a step begins at the data point.
        /// </summary>
        RightSteps,

        // In the future we can add additional interpolation types (see
        // DigitalRune.Mathematics.Interpolation).
    }
}
