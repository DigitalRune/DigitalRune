// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using DigitalRune.Collections;
using ICSharpCode.AvalonEdit;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Provides functions for editing text documents.
    /// </summary>
    public interface ITextService
    {
        /// <summary>
        /// Gets the collections of command item nodes that define the context menu of a text
        /// document.
        /// </summary>
        /// <value>
        /// The collections of command item nodes that define the context menu of a text document.
        /// </value>
        /// <remarks>
        /// <para>
        /// Editor extensions can insert menu items in the context menu of a text document by
        /// adding a collection of command item nodes. A command item node is a
        /// <see cref="MergeableNode{T}"/> that contains an <see cref="ICommandItem"/>. The command
        /// item node defines the point where the command item should be inserted in the context
        /// menu.
        /// </para>
        /// </remarks>
        /// <seealso cref="ContextMenu"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<ICommandItem>> ContextMenuNodeCollections { get; }


        /// <summary>
        /// Gets the context menu of the text editor.
        /// </summary>
        /// <value>The context menu of the text editor.</value>
        /// <remarks>
        /// <para>
        /// The context menu of a document window is built from the
        /// <see cref="ContextMenuNodeCollections"/>. To update the context menu call
        /// <see cref="IEditorService.InvalidateUI"/>.
        /// </para>
        /// <para>
        /// This collection is created once. <see cref="IEditorService.InvalidateUI"/> changes only
        /// the content.
        /// </para>
        /// </remarks>
        /// <seealso cref="ContextMenuNodeCollections"/>
        MenuItemViewModelCollection ContextMenu { get; }


        /// <summary>
        /// Gets the text editor options.
        /// </summary>
        /// <value>The text editor options.</value>
        TextEditorOptions Options { get; }


        /// <summary>
        /// Sets the information in the status bar.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        /// <param name="character">The character number.</param>
        /// <param name="overstrike">
        /// <see langword="true"/> if overstrike is active; otherwise <see langword="false"/> if
        /// insert is active.
        /// </param>
        void SetStatusInfo(int line, int column, int character, bool overstrike);
    }
}
