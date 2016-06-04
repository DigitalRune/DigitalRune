// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// The different types of tick labels allowed in a <see cref="DateTimeScale"/>.
    /// </summary>
    public enum DateTimeLabel
    {
        /// <summary>
        /// Default - no tick labels.
        /// </summary>
        None,

        /// <summary>
        /// Tick labels should be years.
        /// </summary>
        Years,

        /// <summary>
        /// Tick labels should be months.
        /// </summary>
        Months,

        /// <summary>
        /// Tick labels should be days.
        /// </summary>
        Days,

        /// <summary>
        /// Tick labels should be hours.
        /// </summary>
        Hours,

        /// <summary>
        /// Tick labels should be hours / minutes.
        /// </summary>
        Minutes,

        /// <summary>
        /// Tick labels should be hours / minutes / seconds.
        /// </summary>
        Seconds,

        /// <summary>
        /// Tick labels should be hours / minutes / seconds / milliseconds.
        /// </summary>
        Milliseconds,

        /// <summary>
        /// The tick label format is specified in the <see cref="DateTimeScale.FormatString"/>.
        /// </summary>
        CustomFormat,
    }
}
