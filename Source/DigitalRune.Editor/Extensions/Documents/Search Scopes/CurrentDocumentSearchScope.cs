// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Editor.Search;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Provides a search scope containing the currently active document.
    /// </summary>
    internal class CurrentDocumentSearchScope : ISearchScope
    {
        private readonly IDocumentService _documentService;


        /// <inheritdoc/>
        public string Name
        {
            get { return "Current document"; }
        }


        /// <inheritdoc/>
        public IEnumerable<ISearchable> Searchables
        {
            get
            {
                var searchable = _documentService.ActiveDocument as ISearchable;
                if (searchable != null)
                    yield return searchable;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentDocumentSearchScope"/> class.
        /// </summary>
        /// <param name="documentService">The document service.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentService"/> is <see langword="null"/>.
        /// </exception>
        public CurrentDocumentSearchScope(IDocumentService documentService)
        {
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));

            _documentService = documentService;
        }
    }
}
