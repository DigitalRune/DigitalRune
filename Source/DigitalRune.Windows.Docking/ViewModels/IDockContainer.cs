// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a root container for the docking layout.
    /// </summary>
    /// <remarks>
    /// Root containers are <see cref="IDockControl"/> and <see cref="IFloatWindow"/>.
    /// </remarks>
    public interface IDockContainer
    {
        /// <summary>
        /// Gets or sets the root pane of the docking layout.
        /// </summary>
        /// <value>The root pane.</value>
        IDockPane RootPane { get; set; }
    }
}
