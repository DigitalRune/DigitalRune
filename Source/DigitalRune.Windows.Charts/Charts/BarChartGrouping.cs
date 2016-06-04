// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// The type of bar chart grouping.
    /// </summary>
    public enum BarChartGrouping
    {
        /// <summary>
        /// Bars are grouped in clusters.
        /// </summary>
        Clustered,

        /// <summary>
        /// Bars are stacked using absolute values. The values of the previous data series are added
        /// to the current data series.
        /// </summary>
        StackedAbsolute,

        /// <summary>
        /// Bars are stacked showing relative values. The scale is set to percent (the sum of bars
        /// in a group is 100%).
        /// </summary>
        StackedRelative
    }
}
