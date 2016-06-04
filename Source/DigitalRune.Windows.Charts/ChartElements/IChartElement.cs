// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Represents an chart element.
    /// </summary>
    internal interface IChartElement
    {
        /// <summary>
        /// Gets a value indicating whether this chart element is valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is valid; otherwise, <see langword="false"/>.
        /// </value>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="Update"/>
        /// <seealso cref="Updated"/>
        bool IsValid { get; }


        /// <summary>
        /// Gets or sets the x-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated x-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.XAxis</strong> attached dependency property.
        /// </remarks>
        Axis XAxis { get; set; }


        /// <summary>
        /// Gets or sets the y-axis.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The associated y-axis of the <see cref="ChartPanel"/>. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// Settings these value has the same effect as setting the
        /// <strong>ChartPanel.YAxis</strong> attached dependency property.
        /// </remarks>
        Axis YAxis { get; set; }


        /// <summary>
        /// Occurs when the visual appearance of the chart element becomes invalid.
        /// </summary>
        /// <remarks>
        /// The event is raised automatically when x- or y-axis, the data, or an visual property
        /// changes. The chart element can be invalidated explicitly by calling
        /// <see cref="Invalidate"/>. The chart element is updated automatically when it is measured
        /// (see <see cref="UIElement.Measure"/>). It can be updated immediately by calling
        /// <see cref="Update"/>.
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Update"/>
        /// <seealso cref="Updated"/>
        event EventHandler<EventArgs> Invalidated;


        /// <summary>
        /// Occurs when the <see cref="ChartElement"/> is updated.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The event is raised when a invalid chart element is updated. The chart element can be
        /// invalidated explicitly by calling <see cref="Invalidate"/>. The chart element is updated
        /// automatically when it is measured (see <see cref="UIElement.Measure"/>). It can be
        /// updated immediately by calling <see cref="Update"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Update"/>
        event EventHandler<EventArgs> Updated;


        /// <summary>
        /// Invalidates the chart element and forces it to redraw.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method raises the <see cref="Invalidated"/> event. The chart element will be
        /// updated at the beginning of the next measure pass. To update the chart element
        /// immediately the method <see cref="Update"/> should be called.
        /// </para>
        /// <para>
        /// When <see cref="Invalidate"/> is called multiple times before a new measure pass, only
        /// the first call raises the <see cref="Invalidated"/> event.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Update"/>
        /// <seealso cref="Updated"/>
        void Invalidate();


        /// <summary>
        /// Updates (recalculates) the chart element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The chart element is updated immediately, no <see cref="Invalidated"/> event is raised.
        /// After the chart is updated the event <see cref="Updated"/> is raised.
        /// </para>
        /// <para>
        /// When possible, <see cref="Invalidate"/> should be called instead of
        /// <see cref="Update"/>. <see cref="Invalidate"/> performs a lazy update. The update occurs
        /// at the beginning of the next measure pass. Calling <see cref="Invalidate"/> multiple
        /// times is fast, whereas calling <see cref="Update"/> multiple times can be costly.
        /// </para>
        /// </remarks>
        /// <seealso cref="Invalidate"/>
        /// <seealso cref="Invalidated"/>
        /// <seealso cref="IsValid"/>
        /// <seealso cref="Updated"/>
        void Update();


        /// <summary>
        /// Suggests a scale for the x-axis.
        /// </summary>
        /// <returns>An appropriate scale for the x-axis. (Can be <see langword="null"/>.)</returns>
        /// <remarks>
        /// When the <see cref="XAxis"/> is invalidated and <see cref="Axis.AutoScale"/> is set to
        /// <see langword="true"/> the <see cref="XAxis"/> calls <see cref="SuggestXScale"/> to 
        /// determine the optimal scale.
        /// </remarks>
        AxisScale SuggestXScale();


        /// <summary>
        /// Suggests a scale for the y-axis.
        /// </summary>
        /// <returns>An appropriate scale for the y-axis. (Can be <see langword="null"/>.)</returns>
        /// <remarks>
        /// When the <see cref="YAxis"/> is invalidated and <see cref="Axis.AutoScale"/> is set to
        /// <see langword="true"/> the <see cref="YAxis"/> calls <see cref="SuggestYScale"/> to 
        /// determine the optimal scale.
        /// </remarks>
        AxisScale SuggestYScale();
    }
}
