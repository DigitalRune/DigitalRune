// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents a group of line charts where the group overrides the drawing behavior of the
    /// individual line charts.
    /// </summary>
    public interface ILineChartGroup
    {
        /// <summary>
        /// Gets the type of data interpolation between data points.
        /// </summary>
        /// <value>
        /// The interpolation that is used to connect individual data points. The default is
        /// <see cref="ChartInterpolation.Linear"/>.
        /// </value>
        ChartInterpolation Interpolation { get; }


        /// <summary>
        /// Gets the data values in the specified range, which should be rendered.
        /// </summary>
        /// <param name="chart">The line chart.</param>
        /// <param name="startIndex">
        /// The start index in the <see cref="Chart.Data"/> collection.
        /// </param>
        /// <param name="endIndexExclusive">
        /// The end index (exclusive) in the <see cref="Chart.Data"/> collection.
        /// </param>
        /// <param name="xValues">The x values of the data points.</param>
        /// <param name="baseValues">The base y values of the data points.</param>
        /// <param name="yValues">The y values of the data points.</param>
        /// <remarks>
        /// <para>
        /// This method returns the data points that should be rendered. The input parameters
        /// <paramref name="startIndex"/> and <paramref name="endIndexExclusive"/> define the range
        /// of the data points that are visible in the chart. The method needs to read the data
        /// points from the line chart's <see cref="Chart.Data"/> collection and store the adjusted
        /// values in the arrays <paramref name="xValues"/>, <paramref name="baseValues"/>, and
        /// <paramref name="yValues"/> at the corresponding indices. That means the values for
        /// <c>chart.Data[i]</c> needs to be stored at <c>xValues[i]</c>, <c>baseValues[i]</c> and
        /// <c>yValues[i]</c>.
        /// </para>
        /// <para>
        /// The <paramref name="xValues"/> and <paramref name="yValues"/> define the data points.
        /// The line chart will draw a line connecting these data points. <paramref name="yValues"/>
        /// can be <see cref="double.NaN"/> to add gaps.
        /// </para>
        /// <para>
        /// <paramref name="baseValues"/> and are the base y values. The area between
        /// <paramref name="baseValues"/> and <paramref name="yValues"/> will be filled if
        /// <see cref="LineChart.Filled"/> is set.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        void GetValues(LineChart chart, int startIndex, int endIndexExclusive, double[] xValues, double[] baseValues, double[] yValues);
    }
}
