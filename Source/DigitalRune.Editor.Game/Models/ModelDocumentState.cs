// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Editor.Models
{
    /// <summary>
    /// Defines the state of the <see cref="ModelDocument"/>.
    /// </summary>
    internal enum ModelDocumentState
    {
        Empty,
        Loading,
        Loaded,
        Error,
    }
}
