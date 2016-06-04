// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines which value is used as the index in a <see cref="Palette"/>.
    /// </summary>
    public enum PaletteIndex
    {
        /// <summary>
        /// The index of a data point is used to index the <see cref="Palette"/>.
        /// </summary>
        Index,

        /// <summary>
        /// The x value of a data point is used to index the <see cref="Palette"/>.
        /// </summary>
        XValue,

        /// <summary>
        /// The y value of a data point is used to index the <see cref="Palette"/>.
        /// </summary>
        YValue,
    }
}
