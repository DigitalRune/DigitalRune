// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Editor.Search;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Provides a search scope containing all open documents.
    /// </summary>
    internal class OpenDocumentsSearchScope : ISearchScope
    {
        private readonly IEditorService _editor;
        private readonly IDocumentService _documentService;



        /// <inheritdoc/>
        public string Name
        {
            get { return "All open documents"; }
        }


        /// <inheritdoc/>
        public IEnumerable<ISearchable> Searchables
        {
            get
            {
                return DocumentHelper.GetOrderedDocuments(_editor, _documentService, _documentService.ActiveDocument)
                                     .OfType<ISearchable>();
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="OpenDocumentsSearchScope"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentService">The document service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentService"/> is
        /// <see langword="null"/>.
        /// </exception>
        public OpenDocumentsSearchScope(IEditorService editor, IDocumentService documentService)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));

            _editor = editor;
            _documentService = documentService;
        }
    }
}
