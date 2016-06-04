// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.Configuration;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Describes the search settings.
    /// </summary>
    /// <exclude/>
    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class SearchSettings   // Class is only public for serialization!
    {
        // This class is used to serialize the search settings.

        /// <summary>
        /// Gets or sets the recent find patterns.
        /// </summary>
        /// <value>The recent find patterns.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public StringCollection RecentFindPatterns { get; set; }


        /// <summary>
        /// Gets or sets the recent replacement strings.
        /// </summary>
        /// <value>The recent replacement strings.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public StringCollection RecentReplacePatterns { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the search is case-sensitive.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the search is case-sensitive; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        public bool MatchCase { get; set; }


        /// <summary>
        /// Gets or sets a value indicating whether the whole word must match when comparing the
        /// content with the search pattern.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the whole word must match; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        public bool MatchWholeWord { get; set; }


        /// <summary>
        /// Gets or sets the search mode.
        /// </summary>
        /// <value>The search mode.</value>
        public SearchMode Mode { get; set; }
    }
}
