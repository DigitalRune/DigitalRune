// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Describes a scope that can be searched by the Search service.
    /// </summary>
    /// <remarks>
    /// An <see cref="ISearchScope"/> is an <see cref="INamedObject"/>. The name will be shown in
    /// the "Find and Replace" dialog, for example "Current Document", "Current Project",
    /// "Selection", etc.
    /// </remarks>
    public interface ISearchScope : INamedObject
    {
        /// <summary>
        /// Gets the searchable objects within this search scope.
        /// </summary>
        /// <value>The searchable objects within this scope.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Searchables")]
        IEnumerable<ISearchable> Searchables { get; }
    }
}
