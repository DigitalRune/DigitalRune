// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines how a <see cref="Palette"/> maps data values to colors.
    /// </summary>
    /// <remarks>
    /// A <see cref="Palette"/> associates data values with colors. The <see cref="PaletteMode"/>
    /// defines how the data values are mapped to the registered colors.
    /// </remarks>
    public enum PaletteMode
    {
        /// <summary>
        /// Returns the color where the data value exactly matches the parameter. (The query fails
        /// when the <see cref="Palette"/> does not contain an entry for the parameter.)
        /// </summary>
        Equal,

        /// <summary>
        /// Returns the closest color where the data value is less than (or equal to) the parameter.
        /// (The query fails if the parameter is less than all entries in the palette.)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LessOr")]
        LessOrEqual,

        /// <summary>
        /// Returns the closest color where the data value is greater than (or equal to) the
        /// parameter. (The query fails if the parameter is greater than all entries in the
        /// palette.)
        /// </summary>
        GreaterOrEqual,

        /// <summary>
        /// Returns the color where the data value is closest to the parameter. (If two entries have
        /// the same distance than the color with the higher data value is picked. The query fails
        /// only if the <see cref="Palette"/> is empty.)
        /// </summary>
        Closest,

        /// <summary>
        /// Returns a new color by interpolating the two colors closest to the parameter. (No
        /// extrapolation - when the parameter does not lie between to colors only the closest color
        /// is returned. The query fails if the <see cref="Palette"/> is empty.
        /// </summary>
        Interpolate
    }
}
