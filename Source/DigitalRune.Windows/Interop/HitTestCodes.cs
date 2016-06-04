// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The HT constants for <see cref="WindowMessages.WM_NCHITTEST"/> and MOUSEHOOKSTRUCT Mouse
    /// Position Codes. (See header file "winuser.h".)
    /// </summary>
    public static class HitTestCodes
    {
        /// <summary>
        /// On the screen background or on a dividing line between windows (same as
        /// <see cref="HTNOWHERE"/>, except that the DefWindowProc function produces a system beep
        /// to indicate an error).
        /// </summary>
        public const int HTERROR = -2;

        /// <summary>
        /// In a window currently covered by another window in the same thread (the message will be
        /// sent to underlying windows in the same thread until one of them returns a code that is
        /// not <see cref="HTTRANSPARENT"/>).
        /// </summary>
        public const int HTTRANSPARENT = -1;

        /// <summary>
        /// On the screen background or on a dividing line between windows.
        /// </summary>
        public const int HTNOWHERE = 0;

        /// <summary>
        /// In a client area.
        /// </summary>
        public const int HTCLIENT = 1;

        /// <summary>
        /// In a title bar.
        /// </summary>
        public const int HTCAPTION = 2;

        /// <summary>
        /// In a window menu or in a Close button in a child window.
        /// </summary>
        public const int HTSYSMENU = 3;

        /// <summary>
        /// In a size box (same as <see cref="HTGROWBOX"/>).
        /// </summary>
        public const int HTSIZE = 4;

        /// <summary>
        /// In a size box (same as <see cref="HTSIZE"/>).
        /// </summary>
        public const int HTGROWBOX = 4;

        /// <summary>
        /// In a menu.
        /// </summary>
        public const int HTMENU = 5;

        /// <summary>
        /// In a horizontal scroll bar.
        /// </summary>
        public const int HTHSCROLL = 6;

        /// <summary>
        /// In the vertical scroll bar.
        /// </summary>
        public const int HTVSCROLL = 7;

        /// <summary>
        /// In a Minimize button (same as <see cref="HTREDUCE"/>).
        /// </summary>
        public const int HTMINBUTTON = 8;

        /// <summary>
        /// In a Minimize button (same as <see cref="HTMINBUTTON"/>).
        /// </summary>
        public const int HTREDUCE = 8;

        /// <summary>
        /// In a Maximize button (same as <see cref="HTZOOM"/>).
        /// </summary>
        public const int HTMAXBUTTON = 9;

        /// <summary>
        /// In a Maximize button (same as <see cref="HTMAXBUTTON"/>).
        /// </summary>
        public const int HTZOOM = 9;

        /// <summary>
        /// In the left border of a resizable window (the user can click the mouse to resize the
        /// window horizontally). (Same as <see cref="HTSIZEFIRST"/>.)
        /// </summary>
        public const int HTLEFT = 10;

        /// <summary>
        /// In the left border of a resizable window (the user can click the mouse to resize the
        /// window horizontally). (Same as <see cref="HTLEFT"/>.)
        /// </summary>
        public const int HTSIZEFIRST = 10;

        /// <summary>
        /// In the right border of a resizable window (the user can click the mouse to resize the
        /// window horizontally).
        /// </summary>
        public const int HTRIGHT = 11;

        /// <summary>
        /// In the upper-horizontal border of a window.
        /// </summary>
        public const int HTTOP = 12;

        /// <summary>
        /// In the upper-left corner of a window border.
        /// </summary>
        public const int HTTOPLEFT = 13;

        /// <summary>
        /// In the upper-right corner of a window border.
        /// </summary>
        public const int HTTOPRIGHT = 14;

        /// <summary>
        /// In the lower-horizontal border of a resizable window (the user can click the mouse to
        /// resize the window vertically).
        /// </summary>
        public const int HTBOTTOM = 15;

        /// <summary>
        /// In the lower-left corner of a border of a resizable window (the user can click the mouse
        /// to resize the window diagonally).
        /// </summary>
        public const int HTBOTTOMLEFT = 16;

        /// <summary>
        /// In the lower-right corner of a border of a resizable window (the user can click the
        /// mouse to resize the window diagonally). (Same as <see cref="HTSIZELAST"/>.)
        /// </summary>
        public const int HTBOTTOMRIGHT = 17;

        /// <summary>
        /// In the lower-right corner of a border of a resizable window (the user can click the
        /// mouse to resize the window diagonally). (Same as <see cref="HTBOTTOMRIGHT"/>.)
        /// </summary>
        public const int HTSIZELAST = 17;

        /// <summary>
        /// In the border of a window that does not have a sizing border.
        /// </summary>
        public const int HTBORDER = 18;

        /// <summary>
        /// -
        /// </summary>
        public const int HTOBJECT = 19;

        /// <summary>
        /// In a Close button.
        /// </summary>
        public const int HTCLOSE = 20;

        /// <summary>
        /// In a Help button.
        /// </summary>
        public const int HTHELP = 21;
    }

    // ReSharper restore InconsistentNaming
}
