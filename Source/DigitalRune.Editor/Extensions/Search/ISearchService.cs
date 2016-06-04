// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Provides a user interface to find and replace text strings in documents.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Gets the search query.
        /// </summary>
        /// <value>The search query.</value>
        SearchQuery Query { get; }


        /// <summary>
        /// Gets the search scopes.
        /// </summary>
        /// <value>The search scopes.</value>
        IList<ISearchScope> SearchScopes { get; }


        /// <summary>
        /// Shows the Find and Replace window.
        /// </summary>
        void ShowFindAndReplace();


        /// <summary>
        /// Focuses the Quick Find control.
        /// </summary>
        void ShowQuickFind();
    }
}
