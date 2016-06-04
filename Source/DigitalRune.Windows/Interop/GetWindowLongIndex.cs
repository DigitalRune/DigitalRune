// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The <see cref="Win32.GetWindowLong"/> index. (See GWL_ constants in "winuser.h".)
    /// </summary>
    public static class GetWindowLongIndex
    {
        /// <summary>
        /// Retrieves the address of the window procedure, or a handle representing the address of
        /// the window procedure. You must use the CallWindowProc function to call the window
        /// procedure.
        /// </summary>
        public const int GWL_WNDPROC = -4;

        /// <summary>
        /// Retrieves a handle to the application instance.
        /// </summary>
        public const int GWL_HINSTANCE = -6;

        /// <summary>
        /// Retrieves a handle to the parent window, if any.
        /// </summary>
        public const int GWL_HWNDPARENT = -8;

        /// <summary>
        /// Retrieves the window styles. See <see cref="WindowStyles"/>.
        /// </summary>
        public const int GWL_STYLE = -16;

        /// <summary>
        /// Retrieves the extended window styles. For more information, see 
        /// <see cref="Win32.CreateWindowEx"/>. 
        /// </summary>
        public const int GWL_EXSTYLE = -20;

        /// <summary>
        /// Retrieves the user data associated with the window. This data is intended for use by the
        /// application that created the window. Its value is initially zero.
        /// </summary>
        public const int GWL_USERDATA = -21;

        /// <summary>
        /// Retrieves the identifier of the window.
        /// </summary>
        public const int GWL_ID = -12;
    }

    // ReSharper restore InconsistentNaming
}
