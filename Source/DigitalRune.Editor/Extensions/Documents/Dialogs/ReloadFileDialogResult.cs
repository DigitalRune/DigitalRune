// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// The user choices provided by a Reload File dialog.
    /// </summary>
    internal enum ReloadFileDialogResult
    {
        /// <summary>
        /// The user has chosen to reload the file.
        /// </summary>
        Yes,

        /// <summary>
        /// The user has chosen to reload all modified files.
        /// </summary>
        YesToAll,

        /// <summary>
        /// The user has chosen to ignore the file changes.
        /// </summary>
        No,

        /// <summary>
        /// The user has chosen to ignore the changes of all modified files.
        /// </summary>
        NoToAll
    }
}
