// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Defines the search mode.
    /// </summary>
    public enum SearchMode
    {
        /// <summary>
        /// Normal search: Match the <see cref="SearchQuery.FindPattern"/> directly.
        /// </summary>
        Normal,


        /// <summary>
        /// The <see cref="SearchQuery.FindPattern"/> is a regular expression.
        /// </summary>
        Regex,


        /// <summary>
        /// The <see cref="SearchQuery.FindPattern"/> contains wildcard characters.
        /// </summary>
        Wildcards,
    }
}
