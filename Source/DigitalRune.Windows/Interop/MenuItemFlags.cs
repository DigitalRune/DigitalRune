// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The <see cref="Win32.EnableMenuItem"/> flags. (See MF_ constants in "winuser.h".)
    /// </summary>
    [CLSCompliant(false)]
    [Flags]
    public enum MenuItemFlags : uint
    {
        /// <summary>
        /// Indicates that uIDEnableItem gives the identifier of the menu item. If neither the
        /// <see cref="MF_BYCOMMAND"/> nor <see cref="MF_BYPOSITION"/> flag is specified, the
        /// <see cref="MF_BYCOMMAND"/> flag is the default flag.
        /// </summary>
        MF_BYCOMMAND = 0x00000000,

        /// <summary>
        /// Indicates that uIDEnableItem gives the zero-based relative position of the menu item.
        /// </summary>
        MF_BYPOSITION = 0x00000400,

        /// <summary>
        /// Indicates that the menu item is disabled, but not grayed, so it cannot be selected.
        /// </summary>
        MF_DISABLED = 0x00000002,

        /// <summary>
        /// Indicates that the menu item is enabled and restored from a grayed state so that it can
        /// be selected.
        /// </summary>
        MF_ENABLED = 0x00000000,

        /// <summary>
        /// Indicates that the menu item is disabled and grayed so that it cannot be selected.
        /// </summary>
        MF_GRAYED = 0x00000001,
    }

    // ReSharper restore InconsistentNaming
}
