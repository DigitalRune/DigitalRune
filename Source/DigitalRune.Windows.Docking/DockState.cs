// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Defines possible states in the docking layout.
    /// </summary>
    public enum DockState
    {
        /// <summary>
        /// The element is hidden.
        /// </summary>
        Hide,

        /// <summary>
        /// The element is docked in the <see cref="IDockControl"/>.
        /// </summary>
        Dock,

        /// <summary>
        /// The element is docked in one of the <see cref="IDockControl.FloatWindows"/>.
        /// </summary>
        Float,

        /// <summary>
        /// The element is in the left, right, top, or bottom auto-hide bar.
        /// </summary>
        AutoHide,
    }
}
