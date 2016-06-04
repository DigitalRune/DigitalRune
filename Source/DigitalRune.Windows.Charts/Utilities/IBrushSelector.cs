// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Media;


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Provides brushes for items. (Selects brush for pie chart elements.)
    /// </summary>
    public interface IBrushSelector
    {
        /// <summary>
        /// Selects the brush for an item at a certain index.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The brush. Or <see langword="null"/> if the selector is not able to provide a brush.
        /// </returns>
        Brush SelectBrush(object item, int index);
    }
}
