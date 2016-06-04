// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Defines the user choices provided by a Save Changes dialog.
    /// </summary>
    internal enum SaveChangesDialogResult
    {
        /// <summary>
        /// The user has chosen to save and close the modified files.
        /// </summary>
        SaveAndClose,

        /// <summary>
        /// The user has chosen to close the modified files without saving.
        /// </summary>
        CloseWithoutSaving,

        /// <summary>
        /// The user has chosen to do nothing and leave the files opened.
        /// </summary>
        Cancel,
    }
}
