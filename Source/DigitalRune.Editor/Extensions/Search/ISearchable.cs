// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Represents an object that can be searched.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All documents and objects that should be searched by the search service need to implement
    /// this interface.
    /// </para>
    /// <para>
    /// <see cref="Search"/> is called by the Search Service to find a certain text described by a
    /// <see cref="SearchQuery"/>. The method <see cref="Search"/> can return all results at once,
    /// or result by result using the <c>yield</c> statement in C#.
    /// </para>
    /// </remarks>
    public interface ISearchable
    {
        /// <summary>
        /// Searches the object for the text specified by the <see cref="SearchQuery"/>.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <returns>The search results.</returns>
        /// <remarks>
        /// The method needs to return the search results. All results can be returned at once by
        /// returning a collection of <see cref="ISearchResult"/> objects. Or the results can be
        /// return separately by using the <c>yield</c> statement in C#.
        /// </remarks>
        IEnumerable<ISearchResult> Search(SearchQuery query);


        /// <summary>
        /// Called when the Search service begins a replace operation.
        /// </summary>
        /// <remarks>
        /// The <see cref="ISearchable"/> can create a snapshot of the data when
        /// <see cref="BeginReplaceAll"/> is called. This is necessary to create a single undo
        /// operation which reverts all changes between <see cref="BeginReplaceAll"/> and
        /// <see cref="EndReplaceAll"/>.
        /// </remarks>
        void BeginReplaceAll();


        /// <summary>
        /// Called when the Search service ends the replace operation.
        /// </summary>
        /// <inheritdoc cref="BeginReplaceAll"/>
        void EndReplaceAll();
    }
}
