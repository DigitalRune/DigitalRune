// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;


namespace DigitalRune.Windows.Interop
{
    /// <remarks>
    /// <para>
    /// <see cref="Win32"/> contains only a selected list of native Win32 functions. The list may be
    /// extended in the future.
    /// </para>
    /// </remarks>
    partial class Win32
    {
        // ReSharper disable InconsistentNaming

        /// <summary>
        /// Dispatches incoming sent Window messages, checks the thread message queue for a posted
        /// message, and retrieves the message (if any exist).
        /// </summary>
        /// <param name="message">The received message.</param>
        /// <param name="hWnd">The handle of the window whose messages are to be examined.</param>
        /// <param name="messageFilterMin">
        /// Specifies the value of the first message in the range of messages to be examined.
        /// </param>
        /// <param name="messageFilterMax">
        /// Specifies the value of the last message in the range of messages to be examined.
        /// </param>
        /// <param name="flags">
        /// Specifies how messages are handled. (Default: 0. Look up other constants in MSDN
        /// library.)
        /// </param>
        /// <returns>
        /// <see langword="true"/> if message is available; otherwise <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// Set both <paramref name="messageFilterMin"/> and <paramref name="messageFilterMax"/> to
        /// 0 to return all available message (that is, no range filtering is performed).
        /// </remarks>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out MSG message, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);


        /// <summary>
        /// Places (posts) a message in the message queue associated with the thread that created
        /// the specified window and returns without waiting for the thread to process the message.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window whose window procedure is to receive the message. Some values have
        /// special meanings - see MSDN documentation.
        /// </param>
        /// <param name="Msg">Specifies the message to be posted.</param>
        /// <param name="wParam">Specifies additional message-specific information.</param>
        /// <param name="lParam">Specifies additional message-specific information.</param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// Sends the specified message to a window or windows. It calls the window procedure for
        /// the specified window and does not return until the window procedure has processed the
        /// message.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window whose window procedure is to receive the message. Some values have
        /// special meanings - see MSDN documentation.
        /// </param>
        /// <param name="msg">Specifies the message to be sent.</param>
        /// <param name="wParam">Specifies additional message-specific information.</param>
        /// <param name="lParam">Specifies additional message-specific information.</param>
        /// <returns>
        /// The return value specifies the result of the message processing; it depends on the
        /// message sent.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        // NOTE: Additional overloads for SendMessage might be useful. See
        // http://pinvoke.net/default.aspx/user32/SendMessage.html.


        /// <summary>
        /// Gets the handle to the window that has the keyboard focus, if the window is attached to
        /// the calling thread's message queue.
        /// </summary>
        /// <returns>
        /// The handle to the window with the keyboard focus. If the calling thread's message queue
        /// does not have an associated window with the keyboard focus, the return value is
        /// <see langword="null"/>.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetFocus();


        /// <summary>
        /// Sets the keyboard focus to the specified window. The window must be attached to the
        /// calling thread's message queue.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window that will receive the keyboard input. If this parameter is
        /// <see langword="null"/>, keystrokes are ignored.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to the window that previously
        /// had the keyboard focus. If the hWnd parameter is invalid or the window is not attached
        /// to the calling thread's message queue, the return value is <see langword="null"/>.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);


        /// <summary>
        /// Gets the current double-click time for the mouse. A double-click is a series of two
        /// clicks of the mouse button, the second occurring within a specified time after the
        /// first.
        /// </summary>
        /// <returns>
        /// The return value specifies the current double-click time, in milliseconds. (The
        /// double-click time is the maximum number of milliseconds that may occur between the first
        /// and second click of a double-click.)
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();


        /// <summary>
        /// Gets the mouse cursor's position, in screen coordinates.
        /// </summary>
        /// <param name="lpPoint">The screen coordinates of the cursor.</param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(ref POINT lpPoint);


        /// <summary>
        /// Converts the screen coordinates of a specified point on the screen to client-area
        /// coordinates.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window whose client area will be used for the conversion.
        /// </param>
        /// <param name="lpPoint">the screen coordinates to be converted.</param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);


        /// <summary>
        /// Sets the specified window's show state.
        /// </summary>
        /// <param name="hWnd">Handle to the window.</param>
        /// <param name="nCmdShow">Specifies how the window is to be shown. See</param>
        /// <returns>
        /// If the window was previously visible, the return value is <see langword="true"/>. If the
        /// window was previously hidden, the return value is <see langword="false"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowStyles nCmdShow);


        /// <summary>
        /// Changes the size, position, and Z order of a child, pop-up, or top-level window. These
        /// windows are ordered according to their appearance on the screen. The topmost window
        /// receives the highest rank and is the first window in the Z order.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="hWndAfter">
        /// A handle to the window to precede the positioned window in the Z order. This parameter
        /// must be a window handle or one of the following values:
        /// <see cref="HwndConstants.HWND_BOTTOM"/>, <see cref="HwndConstants.HWND_NOTOPMOST"/>,
        /// <see cref="HwndConstants.HWND_TOP"/>, or <see cref="HwndConstants.HWND_TOPMOST"/>.
        /// </param>
        /// <param name="X">
        /// The new position of the left side of the window, in client coordinates.
        /// </param>
        /// <param name="Y">
        /// The new position of the top of the window, in client coordinates.
        /// </param>
        /// <param name="cx">The new width of the window, in pixels.</param>
        /// <param name="cy">The new height of the window, in pixels.</param>
        /// <param name="flags">The window sizing and positioning flags.</param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndAfter, int X, int Y, int cx, int cy, SetWindowPosFlags flags);


        /// <summary>
        /// Gets information about the specified window. The function also retrieves the 32-bit
        /// (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window and, indirectly, the class to which the window belongs.
        /// </param>
        /// <param name="nIndex">
        /// Specifies the zero-based offset to the value to be set. Valid values are in the range
        /// zero through the number of bytes of extra window memory, minus the size of an integer.
        /// To set any other value, specify one of the <see cref="GetWindowLongIndex"/> constants.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the requested 32-bit value. If the
        /// function fails, the return value is zero. To get extended error information, call
        /// <see cref="Marshal.GetLastWin32Error"/>. If <see cref="SetWindowLong"/> has not been
        /// called previously, <see cref="GetWindowLong"/> returns zero for values in the extra
        /// window or class memory.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);


        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long)
        /// value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window and, indirectly, the class to which the window belongs.
        /// </param>
        /// <param name="nIndex">
        /// Specifies the zero-based offset to the value to be set. Valid values are in the range
        /// zero through the number of bytes of extra window memory, minus the size of an integer.
        /// To set any other value, specify one of the <see cref="GetWindowLongIndex"/> constants.
        /// </param>
        /// <param name="dwNewLong">Specifies the replacement value.</param>
        /// <returns>
        /// If the function succeeds, the return value is the previous value of the specified 32-bit
        /// integer. If the function fails, the return value is zero. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);


        /// <summary>
        /// Gets a handle to the window that contains the specified point.
        /// </summary>
        /// <param name="point">The point to be checked.</param>
        /// <returns>
        /// A handle to the window that contains the point. If no window exists at the given point,
        /// the return value is <see langword="null"/>. If the point is over a static text control,
        /// the return value is a handle to the window under the static text control.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr WindowFromPoint(POINT point);


        /// <summary>
        /// Brings the specified window to the top of the Z order. If the window is a top-level
        /// window, it is activated. If the window is a child window, the top-level parent window
        /// associated with the child window is activated.
        /// </summary>
        /// <param name="hWnd">A handle to the window to bring to the top of the Z order.</param>
        /// <returns>
        /// <see langword="true"/> if function succeeds; otherwise, <see langword="false"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);


        /// <summary>
        /// Gets the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The return value is the thread identifier of the calling thread.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetCurrentThreadId();


        /// <summary>
        /// The hook procedure. See <see cref="Win32.SetWindowsHookEx"/>.
        /// </summary>
        public delegate IntPtr HookProc(int code, IntPtr wParam, IntPtr lParam);


        /// <summary>
        /// Installs an application-defined hook procedure into a hook chain. You would install a
        /// hook procedure to monitor the system for certain types of events. These events are
        /// associated either with a specific thread or with all threads in the same desktop as the
        /// calling thread.
        /// </summary>
        /// <param name="hookType">Specifies the type of hook procedure to be installed.</param>
        /// <param name="callback">the hook procedure.</param>
        /// <param name="hInstance">
        /// Handle to the DLL containing the hook procedure pointed to by the
        /// <paramref name="callback"/> parameter. The <paramref name="hInstance"/> parameter must
        /// be set to <see langword="null"/> if the <paramref name="threadID"/> parameter specifies
        /// a thread created by the current process and if the hook procedure is within the code
        /// associated with the current process.
        /// </param>
        /// <param name="threadID">
        /// The identifier of the thread with which the hook procedure is to be associated. If this
        /// parameter is zero, the hook procedure is associated with all existing threads running in
        /// the same desktop as the calling thread.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the handle to the hook procedure. If the
        /// function fails, the return value is <see langword="null"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(HookType hookType, HookProc callback, IntPtr hInstance, int threadID);


        /// <summary>
        /// Removes a hook procedure installed in a hook chain by the <see cref="SetWindowsHookEx"/>
        /// function.
        /// </summary>
        /// <param name="hhk">
        /// Handle to the hook to be removed. This parameter is a hook handle obtained by a previous
        /// call to <see cref="SetWindowsHookEx"/>.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int UnhookWindowsHookEx(IntPtr hhk);


        /// <summary>
        /// Passes the hook information to the next hook procedure in the current hook chain. A hook
        /// procedure can call this function either before or after processing the hook information.
        /// </summary>
        /// <param name="hhk">
        /// <para>
        /// Windows 95/98/ME: Handle to the current hook. An application receives this handle as a
        /// result of a previous call to the <see cref="SetWindowsHookEx"/> function.
        /// </para>
        /// <para>Windows NT/XP/2003: Ignored.</para>
        /// </param>
        /// <param name="nCode">
        /// Specifies the hook code passed to the current hook procedure. The next hook procedure
        /// uses this code to determine how to process the hook information.
        /// </param>
        /// <param name="wParam">
        /// Specifies the wParam value passed to the current hook procedure. The meaning of this
        /// parameter depends on the type of hook associated with the current hook chain.
        /// </param>
        /// <param name="lParam">
        /// Specifies the lParam value passed to the current hook procedure. The meaning of this
        /// parameter depends on the type of hook associated with the current hook chain.
        /// </param>
        /// <returns>
        /// This value is returned by the next hook procedure in the chain. The current hook
        /// procedure must also return this value. The meaning of the return value depends on the
        /// hook type. For more information, see the descriptions of the individual hook procedures
        /// (MSDN documentation).
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);


        ///<summary>
        /// Constant used by <see cref="CreateWindowEx"/>.
        ///</summary>
        public const uint CW_USEDEFAULT = 0x80000000;


        /// <summary>
        /// Creates the window ex.
        /// </summary>
        /// <param name="dwExStyle">
        /// The extended window style of the window being created. This parameter can be one or more
        /// of the <see cref="WindowStylesEx"/> values.
        /// </param>
        /// <param name="lpClassName">
        /// A class atom created by a previous call to the <see cref="RegisterClass"/> function.
        /// </param>
        /// <param name="lpWindowName">The window name.</param>
        /// <param name="dwStyle">
        /// The style of the window being created. This parameter can be a combination of
        /// <see cref="WindowStyles"/> flags, plus additional control styles (see MSDN
        /// documentation).
        /// </param>
        /// <param name="x">
        /// The initial horizontal position of the window. For an overlapped or pop-up window, the x
        /// parameter is the initial x-coordinate of the window's upper-left corner, in screen
        /// coordinates. For a child window, x is the x-coordinate of the upper-left corner of the
        /// window relative to the upper-left corner of the parent window's client area. If x is set
        /// to <see cref="CW_USEDEFAULT"/>, the system selects the default position for the window's
        /// upper-left corner and ignores the y parameter. <see cref="CW_USEDEFAULT"/> is valid only
        /// for overlapped windows; if it is specified for a pop-up or child window, the x and y
        /// parameters are set to zero.
        /// </param>
        /// <param name="y">
        /// The initial vertical position of the window. For an overlapped or pop-up window, the y
        /// parameter is the initial y-coordinate of the window's upper-left corner, in screen
        /// coordinates. For a child window, y is the initial y-coordinate of the upper-left corner
        /// of the child window relative to the upper-left corner of the parent window's client
        /// area. For a list box y is the initial y-coordinate of the upper-left corner of the list
        /// box's client area relative to the upper-left corner of the parent window's client area.
        /// If an overlapped window is created with the <see cref="WindowStyles.WS_VISIBLE"/> style
        /// bit set and the x parameter is set to <see cref="CW_USEDEFAULT"/>, then the y parameter
        /// determines how the window is shown. If the y parameter is <see cref="CW_USEDEFAULT"/>,
        /// then the window manager calls <see cref="ShowWindow"/> with the
        /// <see cref="ShowWindowStyles.SW_SHOW"/> flag after the window has been created. If the y
        /// parameter is some other value, then the window manager calls <see cref="ShowWindow"/>
        /// with that value as the nCmdShow parameter.
        /// </param>
        /// <param name="nWidth">
        /// The width, in device units, of the window. For overlapped windows,
        /// <paramref name="nWidth"/> is the window's width, in screen coordinates, or
        /// <see cref="CW_USEDEFAULT"/>. If <paramref name="nWidth"/> is
        /// <see cref="CW_USEDEFAULT"/>, the system selects a default width and height for the
        /// window; the default width extends from the initial x-coordinates to the right edge of
        /// the screen; the default height extends from the initial y-coordinate to the top of the
        /// icon area. <see cref="CW_USEDEFAULT"/> is valid only for overlapped windows; if
        /// <see cref="CW_USEDEFAULT"/> is specified for a pop-up or child window, the
        /// <paramref name="nWidth"/> and <paramref name="nHeight"/> parameter are set to zero.
        /// </param>
        /// <param name="nHeight">
        /// Specifies the height, in device units, of the window. For overlapped windows,
        /// <paramref name="nHeight"/> is the window's height, in screen coordinates. If the
        /// <paramref name="nWidth"/> parameter is set to <see cref="CW_USEDEFAULT"/>, the system
        /// ignores <paramref name="nHeight"/>.
        /// </param>
        /// <param name="hWndParent">
        /// <para>
        /// Handle to the parent or owner window of the window being created. To create a child
        /// window or an owned window, supply a valid window handle. This parameter is optional for
        /// pop-up
        /// </para>
        /// <para>
        /// windows.Windows 2000/XP: To create a message-only window, supply
        /// <see cref="HwndConstants.HWND_MESSAGE"/> or a handle to an existing message-only window.
        /// </para>
        /// </param>
        /// <param name="hMenu">
        /// Handle to a menu, or specifies a child-window identifier, depending on the window style.
        /// For an overlapped or pop-up window, <paramref name="hMenu"/> identifies the menu to be
        /// used with the window; it can be <see langword="null"/> if the class menu is to be used.
        /// For a child window, <paramref name="hMenu"/> specifies the child-window identifier, an
        /// integer value used by a dialog box control to notify its parent about events. The
        /// application determines the child-window identifier; it must be unique for all child
        /// windows with the same parent window.
        /// </param>
        /// <param name="hInstance">
        /// Handle to the instance of the module to be associated with the window.
        /// </param>
        /// <param name="lpParam">
        /// A value to be passed to the window through the CREATESTRUCT structure (lpCreateParams
        /// member) pointed to by the lParam param of the <see cref="WindowMessages.WM_CREATE"/>
        ///         message. This message is sent to the created window by this function before it
        /// returns. If an application calls CreateWindow to create a MDI client window,
        /// <paramref name="lpParam"/> should point to a CLIENTCREATESTRUCT structure. If an MDI
        /// client window calls CreateWindow to create an MDI child window,
        /// <paramref name="lpParam"/> should point to a MDICREATESTRUCT structure.
        /// <paramref name="lpParam"/> may be <see langword="null"/> if no additional data is
        /// needed.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the new window. If the
        /// function fails, the return value is <see langword="null"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateWindowEx(
          uint dwExStyle,
          string lpClassName,
          string lpWindowName,
          uint dwStyle,
          int x,
          int y,
          int nWidth,
          int nHeight,
          IntPtr hWndParent,
          IntPtr hMenu,
          IntPtr hInstance,
          IntPtr lpParam);


        /// <summary>
        /// Processes a default windows procedure.
        /// </summary>
        /// <param name="hWnd">Handle to the window procedure that received the message.</param>
        /// <param name="msg">The message.</param>
        /// <param name="wParam">
        /// Specifies additional message information. The content of this parameter depends on the
        /// value of the <paramref name="msg"/> parameter.
        /// </param>
        /// <param name="lparam">
        /// Specifies additional message information. The content of this parameter depends on the
        /// value of the <paramref name="msg"/> parameter.
        /// </param>
        /// <returns>
        /// The return value is the result of the message processing and depends on the message. If
        /// <paramref name="msg"/> is <see cref="WindowMessages.WM_SETTEXT"/>, zero is returned.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lparam);


        /// <summary>
        /// Registers a window class for subsequent use in calls to the <see cref="CreateWindowEx"/>
        /// function.
        /// </summary>
        /// <param name="lpWndClass">The <see cref="WNDCLASS"/> structure.</param>
        /// <returns>
        /// If the function succeeds, the return value is a class atom that uniquely identifies the
        /// class being registered. If the function fails, the return value is zero. To get extended
        /// error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern short RegisterClass(ref WNDCLASS lpWndClass);


        /// <summary>
        /// Defines a new window message that is guaranteed to be unique throughout the system. The
        /// message value can be used when sending or posting messages.
        /// </summary>
        /// <param name="lpString">Specifies the message to be registered.</param>
        /// <returns>
        /// If the message is successfully registered, the return value is a message identifier in
        /// the range 0xC000 through 0xFFFF. If the function fails, the return value is zero. To get
        /// extended error information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);


        /// <summary>
        /// Destroys the specified window. The function sends
        /// <see cref="WindowMessages.WM_DESTROY"/> and <see cref="WindowMessages.WM_NCDESTROY"/>
        /// messages to the window to deactivate it and remove the keyboard focus from it. The
        /// function also destroys the window's menu, flushes the thread message queue, destroys
        /// timers, removes clipboard ownership, and breaks the clipboard viewer chain (if the
        /// window is at the top of the viewer chain).
        /// </summary>
        /// <param name="hWnd">Handle to the window to be destroyed.</param>
        /// <returns>
        /// If the function succeeds, the return value is <see langword="true"/>. If the function
        /// fails, the return value is <see langword="false"/>. To get extended error information,
        /// call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyWindow(IntPtr hWnd);


        /// <summary>
        /// Puts the thread that created the specified window into the foreground and activates the
        /// window. Keyboard input is directed to the window, and various visual cues are changed
        /// for the user. The system assigns a slightly higher priority to the thread that created
        /// the foreground window than it does to other threads.
        /// </summary>
        /// <param name="hWnd">
        /// Handle to the window that should be activated and brought to the foreground.
        /// </param>
        /// <returns>
        /// If the window was brought to the foreground, the return value is <see langword="true"/>.
        /// If the window was not brought to the foreground, the return value is
        /// <see langword="false"/>.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        /// <summary>
        /// Enables, disables, or grays the specified menu item.
        /// </summary>
        /// <param name="hMenu">Handle to the menu.</param>
        /// <param name="uIDEnableItem">
        /// Specifies the menu item to be enabled, disabled, or grayed, as determined by the
        /// <paramref name="uEnable"/> parameter. This parameter specifies an item in a menu bar,
        /// menu, or submenu.
        /// </param>
        /// <param name="uEnable">
        /// Controls the interpretation of the <paramref name="uIDEnableItem"/> parameter and
        /// indicate whether the menu item is enabled, disabled, or grayed.
        /// </param>
        /// <returns>
        /// The return value specifies the previous state of the menu item (it is either
        /// MF_DISABLED, MF_ENABLED, or MF_GRAYED). If the menu item does not exist, the return
        /// value is -1.
        /// </returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableMenuItem(IntPtr hMenu, SystemCommands uIDEnableItem, MenuItemFlags uEnable);


        /// <summary>
        /// Allows the application to access the window menu (also known as the system menu or the
        /// control menu) for copying and modifying.
        /// </summary>
        /// <param name="hWnd">Handle to the window that will own a copy of the window menu.</param>
        /// <param name="bRevert">
        /// Specifies the action to be taken. If this parameter is <see langword="false"/>,
        /// <see cref="GetSystemMenu"/> returns a handle to the copy of the window menu currently in
        /// use. The copy is initially identical to the window menu, but it can be modified. If this
        /// parameter is <see langword="true"/>, <see cref="GetSystemMenu"/> resets the window menu
        /// back to the default state. The previous window menu, if any, is destroyed.
        /// </param>
        /// <returns>
        /// If the <paramref name="bRevert"/> parameter is <see langword="false"/>, the return value
        /// is a handle to a copy of the window menu. If the <paramref name="bRevert"/> parameter is
        /// <see langword="true"/>, the return value is <see cref="IntPtr.Zero"/>.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, [MarshalAs(UnmanagedType.Bool)] bool bRevert);


        /// <summary>
        /// Parses a Unicode command line string and returns an array of pointers to the command
        /// line arguments, along with a count of such arguments, in a way that is similar to the
        /// standard C run-time argv and argc values.
        /// </summary>
        /// <param name="cmdLine">
        /// A Unicode string that contains the full command line. If this parameter is an empty
        /// string the function returns the path to the current executable file.
        /// </param>
        /// <param name="numArgs">The number of array elements returned, similar to argc.</param>
        /// <returns>
        /// A pointer to an array of LPWSTR values, similar to argv. If the function fails, the
        /// return value is NULL. To get extended error information, call GetLastError.
        /// </returns>
        [DllImport("shell32.dll", EntryPoint = "CommandLineToArgvW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);


        /// <summary>
        /// Frees the specified local memory object and invalidates its handle.
        /// </summary>
        /// <param name="hMem">
        /// A handle to the local memory object. This handle is returned by either the LocalAlloc or
        /// LocalReAlloc function. It is not safe to free memory allocated with GlobalAlloc.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is NULL. If the function fails, the return
        /// value is equal to a handle to the local memory object. To get extended error
        /// information, call GetLastError.
        /// </returns>
        [DllImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
        private static extern IntPtr LocalFree(IntPtr hMem);


        /// <summary>
        /// Parses a Unicode command line string and returns the arguments.
        /// </summary>
        /// <param name="cmdLine">
        /// A Unicode string that contains the full command line. If this parameter is an empty
        /// string the function returns the path to the current executable file.
        /// </param>
        /// <returns>An array of command line arguments.</returns>
        public static string[] CommandLineToArgvW(string cmdLine)
        {
            IntPtr argv = IntPtr.Zero;

            try
            {
                int numArgs;
                argv = CommandLineToArgvW(cmdLine, out numArgs);
                if (argv == IntPtr.Zero)
                    throw new Win32Exception();

                var result = new string[numArgs];
                for (int i = 0; i < numArgs; i++)
                {
                    IntPtr currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(typeof(IntPtr)));
                    result[i] = Marshal.PtrToStringUni(currArg);
                }

                return result;
            }
            finally
            {
                LocalFree(argv);
            }
        }


        /// <summary>
        /// Loads a string resource from the executable file associated with a specified module,
        /// copies the string into a buffer, and appends a terminating null character.
        /// </summary>
        /// <param name="hInstance">
        /// A handle to an instance of the module whose executable file contains the string
        /// resource. To get the handle to the application itself, call the GetModuleHandle function
        /// with NULL.
        /// </param>
        /// <param name="uID">The identifier of the string to be loaded.</param>
        /// <param name="lpBuffer">
        /// The buffer is to receive the string. Must be of sufficient length to hold a pointer (8
        /// bytes).
        /// </param>
        /// <param name="nBufferMax">
        /// The size of the buffer, in characters. The string is truncated and null-terminated if it
        /// is longer than the number of characters specified. If this parameter is 0, then lpBuffer
        /// receives a read-only pointer to the resource itself.
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is the number of characters copied into the
        /// buffer, not including the terminating null character, or zero if the string resource
        /// does not exist. To get extended error information, call GetLastError.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ID")]
        [DllImport("user32", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadStringW", SetLastError = true)]
        public static extern int LoadString(SafeLibraryHandle hInstance, uint uID, StringBuilder lpBuffer, int nBufferMax);


        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified
        /// module may cause other modules to be loaded.
        /// </summary>
        /// <param name="lpFileName">The name of the module.</param>
        /// <returns>
        /// The handle to the module, or <see langword="null"/> if the function fails. To get
        /// extended error information, call GetLastError.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [DllImport("kernel32", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "LoadLibraryW", SetLastError = true)]
        public static extern SafeLibraryHandle LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpFileName);


        /// <summary>
        /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its
        /// reference count. When the reference count reaches zero, the module is unloaded from the
        /// address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="hModule">A handle to the loaded library module.</param>
        /// <returns>
        /// <see langword="true"/> if succeeds; otherwise, <see langword="false"/>. To get extended
        /// error information, call the GetLastError function.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);


        /// <summary>
        /// Releases the mouse capture from a window in the current thread and restores normal mouse
        /// input processing.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the function succeeded; otherwise, <see langword="false"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1401:PInvokesShouldNotBeVisible")]
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ReleaseCapture();

        // ReSharper restore InconsistentNaming
    }
}
