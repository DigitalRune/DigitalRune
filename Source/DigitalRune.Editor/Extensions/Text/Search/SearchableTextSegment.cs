// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Editor.Search;


namespace DigitalRune.Editor.Text.Search
{
    /// <summary>
    /// Defines a segment of a text document that can be searched.
    /// </summary>
    internal class SearchableTextSegment : ISearchable
    {
        private readonly TextDocument _document;
        private readonly int _offset;
        private readonly int _length;


        /// <summary>
        /// Initializes a new instance of the <see cref="SearchableTextSegment"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public SearchableTextSegment(TextDocument document, int offset, int length)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            _document = document;
            _offset = offset;
            _length = length;
        }


        /// <inheritdoc/>
        public IEnumerable<ISearchResult> Search(SearchQuery query)
        {
            return _document.Search(query, _offset, _length);
        }


        /// <inheritdoc/>
        public void BeginReplaceAll()
        {
            _document.AvalonEditDocument.BeginUpdate();
        }


        /// <inheritdoc/>
        public void EndReplaceAll()
        {
            _document.AvalonEditDocument.EndUpdate();
        }
    }
}
