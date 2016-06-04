// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The HWND_ constants. (See header file "winuser.h".)
    /// </summary>
    [CLSCompliant(false)]
    public static class HwndConstants
    {
        /// <summary>
        /// Places the window at the top of the Z order.
        /// </summary>
        public static readonly IntPtr HWND_TOP = new IntPtr(0);

        /// <summary>
        /// Places the window at the bottom of the Z order. If the hWnd parameter identifies a
        /// topmost window, the window loses its topmost status and is placed at the bottom of all
        /// other windows.
        /// </summary>
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        /// <summary>
        /// Places the window above all non-topmost windows. The window maintains its topmost
        /// position even when it is deactivated.
        /// </summary>
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        /// <summary>
        /// Places the window above all non-topmost windows (that is, behind all topmost windows).
        /// This flag has no effect if the window is already a non-topmost window.
        /// </summary>
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        /// <summary>
        /// Used by <see cref="Win32.CreateWindowEx"/> to create a message only window.
        /// </summary>
        public static readonly IntPtr HWND_MESSAGE = new IntPtr(-3);

        /* Currently not used.
        public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);
        public static readonly IntPtr HWND_DESKTOP = new IntPtr(0);
        */
    }

    // ReSharper restore InconsistentNaming
}
