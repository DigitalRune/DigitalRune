// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The window class styles. (See WS_ constants in "winuser.h".)
    /// </summary>
    [CLSCompliant(false)]
    public static class WindowStyles
    {
        /// <summary>
        /// Creates an overlapped window. An overlapped window has a title bar and a border. Same as
        /// the <see cref="WS_TILED"/> style.
        /// </summary>
        public const uint WS_OVERLAPPED = 0x00000000;

        /// <summary>
        /// Creates a pop-up window. This style cannot be used with the <see cref="WS_CHILD"/>
        /// style.
        /// </summary>
        public const uint WS_POPUP = 0x80000000;

        /// <summary>
        /// Creates a child window. A window with this style cannot have a menu bar. This style
        /// cannot be used with the <see cref="WS_POPUP"/> style.
        /// </summary>
        public const uint WS_CHILD = 0x40000000;

        /// <summary>
        /// Creates a window that is initially minimized. Same as the <see cref="WS_ICONIC"/> style.
        /// </summary>
        public const uint WS_MINIMIZE = 0x20000000;

        /// <summary>
        /// Creates a window that is initially visible. This style can be turned on and off by using
        /// <see cref="Win32.ShowWindow"/> or <see cref="Win32.SetWindowPos"/>.
        /// </summary>
        public const uint WS_VISIBLE = 0x10000000;

        /// <summary>
        /// Creates a window that is initially disabled. A disabled window cannot receive input from
        /// the user. To change this after a window has been created, use EnableWindow().
        /// </summary>
        public const uint WS_DISABLED = 0x08000000;

        /// <summary>
        /// Clips child windows relative to each other; that is, when a particular child window
        /// receives a <see cref="WindowMessages.WM_PAINT"/> message, the
        /// <see cref="WS_CLIPSIBLINGS"/> style clips all other overlapping child windows out of the
        /// region of the child window to be updated. If <see cref="WS_CLIPSIBLINGS"/> is not
        /// specified and child windows overlap, it is possible, when drawing within the client area
        /// of a child window, to draw within the client area of a neighboring child window.
        /// </summary>
        public const uint WS_CLIPSIBLINGS = 0x04000000;

        /// <summary>
        /// Excludes the area occupied by child windows when drawing occurs within the parent
        /// window. This style is used when creating the parent window.
        /// </summary>
        public const uint WS_CLIPCHILDREN = 0x02000000;

        /// <summary>
        /// Creates a window that is initially maximized.
        /// </summary>
        public const uint WS_MAXIMIZE = 0x01000000;

        /// <summary>
        /// Creates a window that has a title bar (includes the <see cref="WS_BORDER"/> style).
        /// </summary>
        public const uint WS_CAPTION = 0x00C00000;

        /// <summary>
        /// Creates a window that has a thin-line border.
        /// </summary>
        public const uint WS_BORDER = 0x00800000;

        /// <summary>
        /// Creates a window that has a border of a style typically used with dialog boxes. A window
        /// with this style cannot have a title bar.
        /// </summary>
        public const uint WS_DLGFRAME = 0x00400000;

        /// <summary>
        /// Creates a window that has a vertical scroll bar.
        /// </summary>
        public const uint WS_VSCROLL = 0x00200000;

        /// <summary>
        /// Creates a window that has a horizontal scroll bar.
        /// </summary>
        public const uint WS_HSCROLL = 0x00100000;

        /// <summary>
        /// Creates a window that has a window menu on its title bar. The <see cref="WS_CAPTION"/>
        /// style must also be specified.
        /// </summary>
        public const uint WS_SYSMENU = 0x00080000;

        /// <summary>
        /// Creates a window that has a sizing border. Same as the <see cref="WS_SIZEBOX"/> style.
        /// </summary>
        public const uint WS_THICKFRAME = 0x00040000;

        /// <summary>
        /// Specifies the first control of a group of controls. The group consists of this first
        /// control and all controls defined after it, up to the next control with the
        /// <see cref="WS_GROUP"/> style. The first control in each group usually has the
        /// <see cref="WS_TABSTOP"/> style so that the user can move from group to group. The user
        /// can subsequently change the keyboard focus from one control in the group to the next
        /// control in the group by using the direction keys. You can turn this style on and off to
        /// change dialog box navigation. To change this style after a window has been created, use
        /// <see cref="Win32.SetWindowLong"/>.
        /// </summary>
        public const uint WS_GROUP = 0x00020000;

        /// <summary>
        /// Specifies a control that can receive the keyboard focus when the user presses the TAB
        /// key. Pressing the TAB key changes the keyboard focus to the next control with the
        /// <see cref="WS_TABSTOP"/> style. You can turn this style on and off to change dialog box
        /// navigation. To change this style after a window has been created, use
        /// <see cref="Win32.SetWindowLong"/>.
        /// </summary>
        public const uint WS_TABSTOP = 0x00010000;

        /// <summary>
        /// Creates a window that has a minimize button. Cannot be combined with the
        /// <see cref="WindowStylesEx.WS_EX_CONTEXTHELP"/> style. The <see cref="WS_SYSMENU"/> style
        /// must also be specified.
        /// </summary>
        public const uint WS_MINIMIZEBOX = 0x00020000;

        /// <summary>
        /// Creates a window that has a maximize button. Cannot be combined with the
        /// <see cref="WindowStylesEx.WS_EX_CONTEXTHELP"/> style. The <see cref="WS_SYSMENU"/> style
        /// must also be specified.
        /// </summary>
        public const uint WS_MAXIMIZEBOX = 0x00010000;

        /// <summary>
        /// Creates an overlapped window. An overlapped window has a title bar and a border. Same as
        /// the <see cref="WS_OVERLAPPED"/> style.
        /// </summary>
        public const uint WS_TILED = 0x00000000;

        /// <summary>
        /// Creates a window that is initially minimized. Same as the <see cref="WS_MINIMIZE"/>
        /// style.
        /// </summary>
        public const uint WS_ICONIC = 0x20000000;

        /// <summary>
        /// Creates a window that has a sizing border. Same as the <see cref="WS_THICKFRAME"/>
        /// style.
        /// </summary>
        public const uint WS_SIZEBOX = 0x00040000;

        /// <summary>
        /// Creates a pop-up window with <see cref="WS_BORDER"/>, <see cref="WS_POPUP"/>, and
        /// <see cref="WS_SYSMENU"/> styles. The <see cref="WS_CAPTION"/> and
        /// <see cref="WS_POPUPWINDOW"/> styles must be combined to make the window menu visible.
        /// </summary>
        public const uint WS_POPUPWINDOW = 0x80880000;

        /// <summary>
        /// Creates an overlapped window with the <see cref="WS_OVERLAPPED"/>,
        /// <see cref="WS_CAPTION"/>, <see cref="WS_SYSMENU"/>, <see cref="WS_THICKFRAME"/>,
        /// <see cref="WS_MINIMIZEBOX"/>, and <see cref="WS_MAXIMIZEBOX"/> styles. Same as the
        /// <see cref="WS_TILEDWINDOW"/> style.
        /// </summary>
        public const uint WS_OVERLAPPEDWINDOW = 0x00CF0000;

        /// <summary>
        /// Creates an overlapped window with the <see cref="WS_OVERLAPPED"/>,
        /// <see cref="WS_CAPTION"/>, <see cref="WS_SYSMENU"/>, <see cref="WS_THICKFRAME"/>,
        /// <see cref="WS_MINIMIZEBOX"/>, and <see cref="WS_MAXIMIZEBOX"/> styles. Same as the
        /// <see cref="WS_OVERLAPPEDWINDOW"/> style.
        /// </summary>
        public const uint WS_TILEDWINDOW = 0x00CF0000;

        /// <summary>
        /// Same as the <see cref="WS_CHILD"/> style.
        /// </summary>
        public const uint WS_CHILDWINDOW = 0x40000000;
    }

    // ReSharper restore InconsistentNaming
}
