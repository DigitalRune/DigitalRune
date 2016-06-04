// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The extended window class styles. (See WS_EX_ constants in "winuser.h".)
    /// </summary>
    [CLSCompliant(false)]
    public static class WindowStylesEx
    {
        /// <summary>
        /// Creates a window that has a double border; the window can, optionally, be created with a
        /// title bar by specifying the <see cref="WindowStyles.WS_CAPTION"/> style in the dwStyle
        /// parameter.
        /// </summary>
        public const uint WS_EX_DLGMODALFRAME = 0x00000001;

        /// <summary>
        /// Specifies that a child window created with this style does not send the
        /// <see cref="WindowMessages.WM_PARENTNOTIFY"/> message to its parent window when it is
        /// created or destroyed.
        /// </summary>
        public const uint WS_EX_NOPARENTNOTIFY = 0x00000004;

        /// <summary>
        /// Specifies that a window created with this style should be placed above all non-topmost
        /// windows and should stay above them, even when the window is deactivated. To add or
        /// remove this style, use the <see cref="Win32.SetWindowPos"/> function.
        /// </summary>
        public const uint WS_EX_TOPMOST = 0x00000008;

        /// <summary>
        /// Specifies that a window created with this style accepts drag-drop files.
        /// </summary>
        public const uint WS_EX_ACCEPTFILES = 0x00000010;

        /// <summary>
        /// Specifies that a window created with this style should not be painted until siblings
        /// beneath the window (that were created by the same thread) have been painted. The window
        /// appears transparent because the bits of underlying sibling windows have already been
        /// painted. To achieve transparency without these restrictions, use the SetWindowRgn
        /// function.
        /// </summary>
        public const uint WS_EX_TRANSPARENT = 0x00000020;

        /// <summary>
        /// Creates a multiple-document interface (MDI) child window.
        /// </summary>
        public const uint WS_EX_MDICHILD = 0x00000040;

        /// <summary>
        /// Creates a tool window; that is, a window intended to be used as a floating toolbar. A
        /// tool window has a title bar that is shorter than a normal title bar, and the window
        /// title is drawn using a smaller font. A tool window does not appear in the taskbar or in
        /// the dialog that appears when the user presses ALT+TAB. If a tool window has a system
        /// menu, its icon is not displayed on the title bar. However, you can display the system
        /// menu by right-clicking or by typing ALT+SPACE.
        /// </summary>
        public const uint WS_EX_TOOLWINDOW = 0x00000080;

        /// <summary>
        /// Specifies that a window has a border with a raised edge.
        /// </summary>
        public const uint WS_EX_WINDOWEDGE = 0x00000100;

        /// <summary>
        /// Specifies that a window has a border with a sunken edge.
        /// </summary>
        public const uint WS_EX_CLIENTEDGE = 0x00000200;

        /// <summary>
        /// Includes a question mark in the title bar of the window. When the user clicks the
        /// question mark, the cursor changes to a question mark with a pointer. If the user then
        /// clicks a child window, the child receives a <see cref="WindowMessages.WM_HELP"/>
        /// message. <see cref="WS_EX_CONTEXTHELP"/> cannot be used with the
        /// <see cref="WindowStyles.WS_MAXIMIZEBOX"/> or <see cref="WindowStyles.WS_MINIMIZEBOX"/>
        /// styles.
        /// </summary>
        public const uint WS_EX_CONTEXTHELP = 0x00000400;

        /// <summary>
        /// The window has generic "right-aligned" properties. This depends on the window class.
        /// This style has an effect only if the shell language is Hebrew, Arabic, or another
        /// language that supports reading-order alignment; otherwise, the style is ignored.
        /// </summary>
        public const uint WS_EX_RIGHT = 0x00001000;

        /// <summary>
        /// Creates a window that has generic left-aligned properties. This is the default.
        /// </summary>
        public const uint WS_EX_LEFT = 0x00000000;

        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading-order
        /// alignment, the window text is displayed using right-to-left reading-order properties.
        /// For other languages, the style is ignored.
        /// </summary>
        public const uint WS_EX_RTLREADING = 0x00002000;

        /// <summary>
        /// The window text is displayed using left-to-right reading-order properties. This is the
        /// default.
        /// </summary>
        public const uint WS_EX_LTRREADING = 0x00000000;

        /// <summary>
        /// If the shell language is Hebrew, Arabic, or another language that supports reading order
        /// alignment, the vertical scroll bar (if present) is to the left of the client area. For
        /// other languages, the style is ignored.
        /// </summary>
        public const uint WS_EX_LEFTSCROLLBAR = 0x00004000;

        /// <summary>
        /// Vertical scroll bar (if present) is to the right of the client area. This is the
        /// default.
        /// </summary>
        public const uint WS_EX_RIGHTSCROLLBAR = 0x00000000;

        /// <summary>
        /// The window itself contains child windows that should take part in dialog box navigation.
        /// If this style is specified, the dialog manager recurses into children of this window
        /// when performing navigation operations such as handling the TAB key, an arrow key, or a
        /// keyboard mnemonic.
        /// </summary>
        public const uint WS_EX_CONTROLPARENT = 0x00010000;

        /// <summary>
        /// Creates a window with a three-dimensional border style intended to be used for items
        /// that do not accept user input.
        /// </summary>
        public const uint WS_EX_STATICEDGE = 0x00020000;

        /// <summary>
        /// Forces a top-level window onto the taskbar when the window is visible.
        /// </summary>
        public const uint WS_EX_APPWINDOW = 0x00040000;

        /// <summary>
        /// Combines the <see cref="WS_EX_CLIENTEDGE"/> and styles.
        /// </summary>
        public const uint WS_EX_OVERLAPPEDWINDOW = 0x00000300;

        /// <summary>
        /// Combines the <see cref="WS_EX_WINDOWEDGE"/>, <see cref="WS_EX_TOOLWINDOW"/>, and
        /// <see cref="WS_EX_TOPMOST"/> styles.
        /// </summary>
        public const uint WS_EX_PALETTEWINDOW = 0x00000188;

        /// <summary>
        /// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child
        /// windows.
        /// </summary>
        public const uint WS_EX_LAYERED = 0x00080000;

        /// <summary>
        /// Windows 2000/XP: A window created with this style does not pass its window layout to its
        /// child windows.
        /// </summary>
        public const uint WS_EX_NOINHERITLAYOUT = 0x00100000;

        /// <summary>
        /// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose
        /// horizontal origin is on the right edge. Increasing horizontal values advance to the
        /// left.
        /// </summary>
        public const uint WS_EX_LAYOUTRTL = 0x00400000;


        /// <summary>
        /// Windows XP: Paints all descendants of a window in bottom-to-top painting order using
        /// double-buffering.
        /// </summary>
        public const uint WS_EX_COMPOSITED = 0x02000000;

        /// <summary>
        /// Windows 2000/XP: A top-level window created with this style does not become the
        /// foreground window when the user clicks it. The system does not bring this window to the
        /// foreground when the user minimizes or closes the foreground window. To activate the
        /// window, use the SetActiveWindow() or <see cref="Win32.SetForegroundWindow"/> function.
        /// The window does not appear on the taskbar by default. To force the window to appear on
        /// the taskbar, use the <see cref="WS_EX_APPWINDOW"/> style.
        /// </summary>
        public const uint WS_EX_NOACTIVATE = 0x08000000;
    }

    // ReSharper restore InconsistentNaming
}
