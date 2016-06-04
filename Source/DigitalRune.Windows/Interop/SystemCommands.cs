// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Interop
{
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// The system commands. See <see cref="WindowMessages.WM_SYSCOMMAND"/>.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1717:OnlyFlagsEnumsShouldHavePluralNames")]
    [CLSCompliant(false)]
    public enum SystemCommands : uint
    {
        /// <summary>
        /// Sizes the window.
        /// </summary>
        SC_SIZE = 0xF000,

        /// <summary>
        /// Moves the window.
        /// </summary>
        SC_MOVE = 0xF010,

        /// <summary>
        /// Minimizes the window.
        /// </summary>
        SC_MINIMIZE = 0xF020,

        /// <summary>
        /// Maximizes the window.
        /// </summary>
        SC_MAXIMIZE = 0xF030,

        /// <summary>
        /// Moves to the next window.
        /// </summary>
        SC_NEXTWINDOW = 0xF040,

        /// <summary>
        /// Moves to the previous window.
        /// </summary>
        SC_PREVWINDOW = 0xF050,

        /// <summary>
        /// Closes the window.
        /// </summary>
        SC_CLOSE = 0xF060,

        /// <summary>
        /// Scrolls vertically.
        /// </summary>
        SC_VSCROLL = 0xF070,

        /// <summary>
        /// Scrolls horizontally.
        /// </summary>
        SC_HSCROLL = 0xF080,

        /// <summary>
        /// Retrieves the window menu as a result of a mouse click.
        /// </summary>
        SC_MOUSEMENU = 0xF090,

        /// <summary>
        /// Retrieves the window menu as a result of a keystroke.
        /// </summary>
        SC_KEYMENU = 0xF100,

        /// <summary>
        /// </summary>
        SC_ARRANGE = 0xF110,

        /// <summary>
        /// Restores the window to its normal position and size
        /// </summary>
        SC_RESTORE = 0xF120,

        /// <summary>
        /// Activates the Start menu
        /// </summary>
        SC_TASKLIST = 0xF130,

        /// <summary>
        /// Executes the screen saver application specified in the [boot] section of the System.ini
        /// file.
        /// </summary>
        SC_SCREENSAVE = 0xF140,

        /// <summary>
        /// Activates the window associated with the application-specified hot key. The lParam
        /// parameter identifies the window to activate.
        /// </summary>
        SC_HOTKEY = 0xF150,

        /// <summary>
        /// Selects the default item; the user double-clicked the window menu.
        /// </summary>
        SC_DEFAULT = 0xF160,

        /// <summary>
        /// Sets the state of the display. This command supports devices that have power-saving
        /// features, such as a battery-powered personal computer. The lParam parameter can have the
        /// following values: 
        /// -1 ... the display is powering on,
        /// 1 ... the display is going to low power,
        /// 2 ... the display is being shut off.
        /// </summary>
        SC_MONITORPOWER = 0xF170,

        /// <summary>
        /// Changes the cursor to a question mark with a pointer. If the user then clicks a control
        /// in the dialog box, the control receives a <see cref="WindowMessages.WM_HELP"/> message.
        /// </summary>
        SC_CONTEXTHELP = 0xF180,

        /// <summary>
        /// Separator.
        /// </summary>
        SC_SEPARATOR = 0xF00F,
    }

    // ReSharper restore InconsistentNaming
}
