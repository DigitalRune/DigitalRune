// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Charts
{
    /// <summary>
    /// Defines the render mode for a chart element.
    /// </summary>
    public enum ChartRenderMode
    {
        /// <summary>
        /// Always renders the element in best quality. (Slow)
        /// </summary>
        Quality,

        /// <summary>
        /// Renders the element in reduced quality when CPU is busy. When the CPU is idle again the
        /// element is automatically updated and rendered in best quality.
        /// </summary>
        Performance,

        /// <summary>
        /// Do not render the element.
        /// </summary>
        DoNotRender
    }
}
