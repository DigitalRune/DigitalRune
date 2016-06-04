// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines how a bar in a <see cref="ColoredBarChart"/> or <see cref="HeatChart"/> is filled.
    /// </summary>
    public enum BarFillMode
    {
        /// <summary>
        /// The bar is filled with a solid color.
        /// </summary>
        Solid,

        /// <summary>
        /// The bar is filled with a gradient.
        /// </summary>
        /// <remarks>
        /// The gradient is in the direction of the bar. For example, the gradient in horizontal bar
        /// charts is horizontal.
        /// </remarks>
        Gradient,
    }
}
