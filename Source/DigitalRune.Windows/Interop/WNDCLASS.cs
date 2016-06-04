// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Runtime.InteropServices;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The <see cref="WindowProc"/> is an application-defined function that processes messages sent
    /// to a window. (See header file "winuser.h".)
    /// </summary>
    /// <param name="hwnd">Handle to the window.</param>
    /// <param name="msg">Specifies the message.</param>
    /// <param name="wParam">
    /// Specifies additional message information. The contents of this parameter depend on the value
    /// of the <paramref name="msg"/> parameter.
    /// </param>
    /// <param name="lParam">
    /// Specifies additional message information. The contents of this parameter depend on the value
    /// of the <paramref name="msg"/> parameter.
    /// </param>
    /// <returns>
    /// The return value is the result of the message processing and depends on the message sent.
    /// </returns>
    [CLSCompliant(false)]
    public delegate IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);


    /// <summary>
    /// The <see cref="WNDCLASS"/> structure contains the window class attributes that are
    /// registered by the <see cref="Win32.RegisterClass"/> function. (See header file "winuser.h".)
    /// </summary>
    [CLSCompliant(false)]
    [StructLayout(LayoutKind.Sequential)]
    public struct WNDCLASS
    {
        /// <summary>
        /// Specifies the class style(s). This member can be any combination of
        /// <see cref="WindowStyles"/> flags.
        /// </summary>
        public uint style;


        /// <summary>
        /// The window procedure.
        /// </summary>
        public WindowProc lpfnWndProc;


        /// <summary>
        /// Specifies the number of extra bytes to allocate following the window-class structure.
        /// The system initializes the bytes to zero.
        /// </summary>
        public int cbClsExtra;


        /// <summary>
        /// Specifies the number of extra bytes to allocate following the window instance. The
        /// system initializes the bytes to zero.
        /// </summary>
        public int cbWndExtra;


        /// <summary>
        /// Handle to the instance that contains the window procedure for the class.
        /// </summary>
        public IntPtr hInstance;


        /// <summary>
        /// Handle to the class icon. This member must be a handle to an icon resource. If this
        /// member is <see langword="null"/>, the system provides a default icon.
        /// </summary>
        public IntPtr hIcon;


        /// <summary>
        /// Handle to the class cursor. This member must be a handle to a cursor resource. If this
        /// member is <see langword="null"/>, an application must explicitly set the cursor shape
        /// whenever the mouse moves into the application's window.
        /// </summary>
        public IntPtr hCursor;


        /// <summary>
        /// Handle to the class background brush. This member can be a handle to the physical brush
        /// to be used for painting the background, or it can be a color value.
        /// </summary>
        public IntPtr hbrBackground;


        /// <summary>
        /// The resource name of the class menu, as the name appears in the resource file.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszMenuName;


        /// <summary>
        /// The window class name.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClassName;
    }

    // ReSharper restore InconsistentNaming
}
