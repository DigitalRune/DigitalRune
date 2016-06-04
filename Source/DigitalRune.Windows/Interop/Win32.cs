// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Provides access to native Win32 functionality.
    /// </summary>
    [CLSCompliant(false)]
    public static partial class Win32
    {
        /// <summary>
        /// Gets a value indicating whether the application is idle.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the application is idle; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// An application is considered as idle, if its Win32 message queue is empty.
        /// </remarks>
        public static bool IsApplicationIdle
        {
            get
            {
                MSG message;
                return !PeekMessage(out message, IntPtr.Zero, 0, 0, 0);
            }
        }


        /// <summary>
        /// Returns a DWORD value by concatenation the specified values.
        /// </summary>
        /// <param name="low">The low-order word of the new value.</param>
        /// <param name="high">The high-order word of the new value.</param>
        /// <returns>An unsigned 32-bit value (DWORD).</returns>
        public static uint MAKELONG(int low, int high)
        {
            return (uint)((high << 16) + low);
        }


        /// <summary>
        /// Gets the signed x-coordinate from the given lParam value.
        /// </summary>
        /// <param name="lParam">The value to be converted.</param>
        /// <returns>The signed x-coordinate.</returns>
        public static int GET_X_LPARAM(IntPtr lParam)
        {
            return LOWORD(lParam.ToInt32());
        }


        /// <summary>
        /// Gets the signed y-coordinate from the given lParam value.
        /// </summary>
        /// <param name="lParam">The value to be converted.</param>
        /// <returns>The signed y-coordinate.</returns>
        public static int GET_Y_LPARAM(IntPtr lParam)
        {
            return HIWORD(lParam.ToInt32());
        }


        /// <summary>
        /// Gets the low-order word from the specified value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The low-order word.</returns>
        public static int LOWORD(int value)
        {
            return value & 0xFFFF;
        }


        /// <summary>
        /// Gets the high-order word from the specified value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The high-order word.</returns>
        public static int HIWORD(int value)
        {
            return value >> 16;
        }
    }

    // ReSharper restore InconsistentNaming
}
