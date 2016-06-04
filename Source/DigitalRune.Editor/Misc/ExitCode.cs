// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Editor
{
    /// <summary>
    /// Defines the Win32 system error codes.
    /// </summary>
    /// <remarks>
    /// Reference:
    /// <seealso href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms681382.aspx"/>
    /// </remarks>
    public enum ExitCode
    {
        // TODO: Add system error codes as needed.

        /// <summary>The operation completed successfully.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        ERROR_SUCCESS = 0,

        /// <summary>One or more arguments are not correct.</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        ERROR_BAD_ARGUMENTS = 160,  // 0x0A0
    }
}
