// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;


namespace DigitalRune.Editor.Options
{
    /// <summary>
    /// Shows and controls the content of the Options dialog.
    /// </summary>
    public interface IOptionsService
    {
        /// <summary>
        /// Gets the collections of options nodes.
        /// </summary>
        /// <value>The collections of options nodes.</value>
        /// <remarks>
        /// <para>
        /// Editor extensions can add options pages to the Options dialog, by adding a collection of
        /// options nodes. An options node is a <see cref="MergeableNode{T}"/> that contains an
        /// <see cref="OptionsPageViewModel"/>. The <see cref="OptionsPageViewModel"/> defines the
        /// options pages that should be displayed. The options node defines the point where the
        /// options page should be inserted in the Options dialog.
        /// </para>
        /// <para>
        /// <see cref="OptionsGroupViewModel"/> is a special <see cref="OptionsPageViewModel"/>
        /// which can be used to group other options pages: Create a <see cref="MergeableNode{T}"/>
        /// containing a <see cref="OptionsGroupViewModel"/> and add the other options pages as
        /// as the children of the node.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<OptionsPageViewModel>> OptionsNodeCollections { get; }


        /// <summary>
        /// Shows the Options dialog.
        /// </summary>
        void Show();
    }
}
