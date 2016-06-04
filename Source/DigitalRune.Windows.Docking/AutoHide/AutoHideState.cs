// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Defines the state of an <see cref="AutoHidePane"/>.
    /// </summary>
    public enum AutoHideState
    {
        /// <summary>
        /// The <see cref="AutoHidePane"/> is hidden. (Slid out of the visible area.)
        /// </summary>
        Hidden,

        /// <summary>
        /// The <see cref="AutoHidePane"/> is currently animated and moved in to the visible area.
        /// (It is partially visible.)
        /// </summary>
        SlidingIn,

        /// <summary>
        /// The <see cref="AutoHidePane"/> is fully visible.
        /// </summary>
        Shown,

        /// <summary>
        /// The <see cref="AutoHidePane"/> is currently animated and moved out of the visible area.
        /// (It is partially visible.)
        /// </summary>
        SlidingOut
    }
}
