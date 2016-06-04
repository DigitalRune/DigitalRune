// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.Runtime.InteropServices;


namespace DigitalRune.Graphics.Interop
{
  /// <summary>
  /// Provides access to native methods.
  /// </summary>
  internal static class NativeMethods
  {
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Places (posts) a message in the message queue associated with the thread that created the 
    /// specified window and returns without waiting for the thread to process the message. 
    /// </summary>
    /// <param name="hWnd">
    /// Handle to the window whose window procedure is to receive the message. Some values have
    /// special meanings - see MSDN documentation.</param>
    /// <param name="Msg">Specifies the message to be posted.</param>
    /// <param name="wParam">Specifies additional message-specific information.</param>
    /// <param name="lParam">Specifies additional message-specific information.</param>
    /// <returns>
    /// If the function succeeds, the return value is <see langword="true"/>. If the function fails, 
    /// the return value is <see langword="false"/>. To get extended error information, call 
    /// <see cref="Marshal.GetLastWin32Error"/>.
    /// </returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    // ReSharper restore InconsistentNaming
  }
}
#endif
