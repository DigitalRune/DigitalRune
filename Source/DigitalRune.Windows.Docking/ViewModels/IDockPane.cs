// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane in the docking layout.
    /// </summary>
    public interface IDockPane : IDockElement
    {
        /// <summary>
        /// Gets (or sets) a value indicating whether this pane is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this pane is visible; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property is set automatically by the <see cref="DockStrategy"/>.
        /// </remarks>
        bool IsVisible { get; set; }
    }
}
