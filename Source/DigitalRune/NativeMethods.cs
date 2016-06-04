// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Text;


namespace DigitalRune
{
  /// <summary>
  /// Provides access to native methods using P/Invoke.
  /// </summary>
  internal static class NativeMethods
  {
    /// <summary>
    /// Loads an icon, cursor, animated cursor, or bitmap.
    /// </summary>
    /// <param name="instance">A handle to the module that contains the image to be loaded. </param>
    /// <param name="fileName">The name of the image to be loaded.</param>
    /// <param name="type">The type of image to be loaded: 0 = bitmap, 1 = icon, 2 = cursor.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="load">The load parameter.</param>
    /// <returns>The handle of the newly loaded image, or <see cref="IntPtr.Zero"/>.</returns>
    /// <remarks>
    /// See <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms648045.aspx"/>
    /// </remarks>
    [DllImport("User32.dll", CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadImage(IntPtr instance, string fileName, uint type, int width, int height, uint load);


    /// <summary>
    /// Destroys the specified cursor and frees the allocated memory.
    /// </summary>
    /// <param name="cursor">The handle of the cursor.</param>
    /// <returns><see langword="true"/> if successful; otherwise, <see langword="false"/>.</returns>
    [DllImport("User32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyCursor(IntPtr cursor);


    /// <summary>
    /// Translates the specified virtual-key code and keyboard state to the corresponding Unicode 
    /// character or characters.
    /// </summary>
    /// <param name="wVirtKey">The virtual-key code to be translated.</param>
    /// <param name="wScanCode">
    /// The hardware scan code of the key to be translated. The high-order bit of this value is set 
    /// if the key is up.
    /// </param>
    /// <param name="lpKeyState">
    /// A pointer to a 256-byte array that contains the current keyboard state. Each element (byte) 
    /// in the array contains the state of one key. If the high-order bit of a byte is set, the key 
    /// is down.
    /// </param>
    /// <param name="pwszBuff">
    /// The translated Unicode character. 
    /// </param>
    /// <param name="cchBuff">Must be 1.</param>
    /// <param name="wFlags">
    /// The behavior of the function. If bit 0 is set, a menu is active. Bits 1 through 31 are 
    /// reserved.
    /// </param>
    /// <returns>
    /// A value indicating the result of the function.
    /// </returns>
    /// <remarks>
    /// See <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646322.aspx"/>
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState, out char pwszBuff, int cchBuff, uint wFlags);


    /// <summary>
    /// Translates the specified virtual-key code and keyboard state to the corresponding Unicode 
    /// character or characters.
    /// </summary>
    /// <param name="wVirtKey">The virtual-key code to be translated.</param>
    /// <param name="wScanCode">
    /// The hardware scan code of the key to be translated. The high-order bit of this value is set 
    /// if the key is up.
    /// </param>
    /// <param name="lpKeyState">
    /// A pointer to a 256-byte array that contains the current keyboard state. Each element (byte) 
    /// in the array contains the state of one key. If the high-order bit of a byte is set, the key 
    /// is down.
    /// </param>
    /// <param name="pwszBuff">
    /// The translated Unicode character. 
    /// </param>
    /// <param name="cchBuff">Must be 1.</param>
    /// <param name="wFlags">
    /// The behavior of the function. If bit 0 is set, a menu is active. Bits 1 through 31 are 
    /// reserved.
    /// </param>
    /// <param name="layout">
    /// The input locale identifier used to translate the specified code. This parameter can be any
    /// input locale identifier previously returned by the LoadKeyboardLayout function.
    /// </param>
    /// <returns>
    /// A value indicating the result of the function.
    /// </returns>
    /// <remarks>
    /// See <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646322.aspx"/>
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, out char pwszBuff, int cchBuff, uint wFlags, IntPtr layout);


    /// <summary>
    /// Translates (maps) a virtual-key code into a scan code or character value, or translates a
    /// scan code into a virtual-key code.
    /// </summary>
    /// <param name="uCode">The virtual-key code or scan code for a key.</param>
    /// <param name="uMapType">The translation to perform.</param>
    /// <param name="layout">Input locale identifier to use for translating the specified code.</param>
    /// <returns>The scan code, a virtual-key code, or a character value.</returns>
    /// <remarks>
    /// See <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646307.aspx"/>
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr layout);


    /// <summary>
    /// Retrieves the active input locale identifier (formerly called the keyboard layout).
    /// </summary>
    /// <param name="thread">
    /// The identifier of the thread to query, or 0 for the current thread.
    /// </param>
    /// <returns>The return value is the input locale identifier for the thread.</returns>
    /// <remarks>
    /// See <see href="http://msdn.microsoft.com/en-us/library/windows/desktop/ms646296.aspx"/>
    /// </remarks>
    [DllImport("user32.dll")]
    public static extern IntPtr GetKeyboardLayout(uint thread);


    /// <summary>
    /// Retrieves the name of the active input locale identifier (formerly called the keyboard 
    /// layout) for the system.
    /// </summary>
    /// <param name="pwszKLID">
    /// The buffer (of at least KL_NAMELENGTH characters in length) that receives the name of the 
    /// input locale identifier, including the terminating null character. This will be a copy of 
    /// the string provided to the LoadKeyboardLayout function, unless layout substitution took 
    /// place.
    /// </param>
    /// <returns>
    /// If the function succeeds, the return value is nonzero. If the function fails, the return 
    /// value is zero.To get extended error information, call GetLastError.
    /// </returns>
    /// <remarks>
    /// See <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms646298.aspx"/>
    /// </remarks>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetKeyboardLayoutName([Out] StringBuilder pwszKLID);


    /// <summary>
    /// Fills in the specified buffer with information about the system.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <returns>
    /// On success, zero is returned.  On error, -1 is returned, and errno is
    /// set appropriately.
    /// </returns>
    /// <remarks>
    /// See also http://man7.org/linux/man-pages/man2/uname.2.html.
    /// </remarks>
    [DllImport("libc")]
    public static extern int uname(IntPtr buffer);
  }
}
#endif
