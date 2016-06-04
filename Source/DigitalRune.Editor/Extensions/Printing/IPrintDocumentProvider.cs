// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Documents;


namespace DigitalRune.Editor.Printing
{
    /// <summary>
    /// Represents an object which provides a print document.
    /// </summary>
    public interface IPrintDocumentProvider
    {
        /// <summary>
        /// Gets the print document.
        /// </summary>
        /// <value>The print document.</value>
        IDocumentPaginatorSource PrintDocument { get; }
    }
}
