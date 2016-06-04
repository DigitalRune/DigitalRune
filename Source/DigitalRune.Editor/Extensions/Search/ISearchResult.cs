// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Describes a single result of a text search.
    /// </summary>
    public interface ISearchResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether this search result is selected in the associated
        /// document.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this search result is selected; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// This property can be set to <see langword="true"/> to select (highlight) the search
        /// result.
        /// </remarks>
        bool IsSelected { get; set; }


        /// <summary>
        /// Replaces the object that matches the search query with a new string.
        /// </summary>
        /// <param name="replacement">The replacement string.</param>
        void Replace(string replacement);
    }
}
