// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an anchored pane in the docking layout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An <see cref="IDockAnchorPane"/> may contain 0 or 1 child pane.
    /// </para>
    /// <para>
    /// An <see cref="IDockControl"/> usually contains one <see cref="IDockAnchorPane"/> that
    /// represents the main pane (similar to the "Documents" pane in Microsoft Visual Studio®).
    /// </para>
    /// </remarks>
    public interface IDockAnchorPane : IDockPane
    {
        /// <summary>
        /// Gets or sets the child pane.
        /// </summary>
        /// <value>The child pane.</value>
        IDockPane ChildPane { get; set; }
    }
}
