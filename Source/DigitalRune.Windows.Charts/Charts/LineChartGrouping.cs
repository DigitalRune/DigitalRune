// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Describes the type of grouping (stacking) for line charts.
    /// </summary>
    public enum LineChartGrouping
    {
        /// <summary>
        /// Line charts are stacked using absolute values. The y values of the previous data series
        /// are added to the current data series.
        /// </summary>
        StackedAbsolute,

        /// <summary>
        /// Line charts are stacked using relative values. The y-scale is set to percent (the sum of
        /// all data series in the current group is 100%).
        /// </summary>
        StackedRelative
    }
}
