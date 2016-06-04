// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Search;
using ICSharpCode.AvalonEdit;


namespace DigitalRune.Editor.Text.Search
{
    /// <summary>
    /// Provides a search scope for the current selection in the <see cref="TextEditor"/> control.
    /// </summary>
    internal class CurrentSelectionSearchScope : ISearchScope
    {
        private readonly IDocumentService _documentService;


        /// <inheritdoc/>
        public string Name
        {
            get { return "Current selection"; }
        }


        /// <inheritdoc/>
        public IEnumerable<ISearchable> Searchables
        {
            get
            {
                var document = _documentService.ActiveDocument;
                var textDocument = document as TextDocument;
                var textEditor = textDocument?.GetLastActiveTextEditor();
                if (textEditor != null)
                {
                    int offset = textEditor.SelectionStart;
                    int length = textEditor.SelectionLength;
                    if (offset >= 0 && length > 0)
                        yield return new SearchableTextSegment(textDocument, offset, length);
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSelectionSearchScope"/> class.
        /// </summary>
        /// <param name="documentService">The document service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentService"/> is <see langword="null"/>.
        /// </exception>
        public CurrentSelectionSearchScope(IDocumentService documentService)
        {
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));

            _documentService = documentService;
        }
    }
}
