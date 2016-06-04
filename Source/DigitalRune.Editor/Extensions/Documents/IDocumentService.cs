// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using DigitalRune.Collections;
using Microsoft.Win32;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Manages documents and provides a user-interface for creating, opening, and saving documents.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Gets the collections of command item nodes that define the context menu of a document
        /// window.
        /// </summary>
        /// <value>
        /// The collections of command item nodes that define the context menu of a document window.
        /// </value>
        /// <remarks>
        /// <para>
        /// Editor extensions can insert menu items in the context menu of a document window by
        /// adding a collection of command item nodes. A command item node is a
        /// <see cref="MergeableNode{T}"/> that contains an <see cref="ICommandItem"/>. The command
        /// item node defines the point where the command item should be inserted in the context
        /// menu.
        /// </para>
        /// <para>
        /// The context menu of a document window is built from the 
        /// <see cref="IEditorService.DockContextMenuNodeCollections"/> of the 
        /// <see cref="IEditorService"/> and the <see cref="DockContextMenuNodeCollections"/> of the
        /// <see cref="IDocumentService"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="DockContextMenu"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        ICollection<MergeableNodeCollection<ICommandItem>> DockContextMenuNodeCollections { get; }


        /// <summary>
        /// Gets the context menu of a document window.
        /// </summary>
        /// <value>The context menu of a document window.</value>
        /// <remarks>
        /// <para>
        /// The context menu of a document window is built from the
        /// <see cref="IEditorService.DockContextMenuNodeCollections"/> of the
        /// <see cref="IEditorService"/> and the <see cref="DockContextMenuNodeCollections"/> of the
        /// <see cref="IDocumentService"/>. To update the context menu call
        /// <see cref="IEditorService.InvalidateUI"/>.
        /// </para>
        /// <para>
        /// This collection is created once. <see cref="IEditorService.InvalidateUI"/> changes only
        /// the content.
        /// </para>
        /// </remarks>
        /// <seealso cref="DockContextMenuNodeCollections"/>
        MenuItemViewModelCollection DockContextMenu { get; }


        /// <summary>
        /// Gets the default <see cref="OpenFileDialog"/>.
        /// </summary>
        /// <value>The default <see cref="OpenFileDialog"/>.</value>
        OpenFileDialog OpenFileDialog { get; }


        /// <summary>
        /// Gets the default <see cref="SaveFileDialog"/>.
        /// </summary>
        /// <value>The default <see cref="SaveFileDialog"/>.</value>
        SaveFileDialog SaveFileDialog { get; }


        /// <summary>
        /// Gets the document which is currently selected in the editor.
        /// </summary>
        /// <value>
        /// The currently selected document; <see langword="null"/> if no document is selected.
        /// </value>
        /// <remarks>
        /// The <see cref="ActiveDocument"/> is the document that currently has the focus inside the
        /// editor. If a tool window has focus, then the <see cref="ActiveDocument"/> is the last
        /// document that had the focus in the editor.
        /// </remarks>
        Document ActiveDocument { get; }


        /// <summary>
        /// Gets all documents.
        /// </summary>
        /// <value>All documents.</value>
        /// <remarks>
        /// Note that the order of the documents can be random. It is usually the order in which the
        /// documents were created inside the editor. (The order does not reflect the order of the
        /// windows in the editor!)
        /// </remarks>
        IEnumerable<Document> Documents { get; }


        /// <summary>
        /// Gets the document factories.
        /// </summary>
        /// <value>The document factories.</value>
        /// <remarks>
        /// <para>
        /// If extensions want to handle documents, they add their document factory to this
        /// collection.
        /// </para>
        /// <para>
        /// When a new document factory is added or removed after startup, the method
        /// <see cref="IEditorService.InvalidateUI"/> must be called to update the menu and toolbar
        /// items.
        /// </para>
        /// </remarks>
        ICollection<DocumentFactory> Factories { get; }


        /// <summary>
        /// Occurs when the <see cref="ActiveDocument"/> changed.
        /// </summary>
        event EventHandler<EventArgs> ActiveDocumentChanged;


        /// <summary>
        /// Creates a new document of the specified type and shows it in the editor.
        /// </summary>
        /// <param name="documentType">The document type description.</param>
        /// <returns>
        /// The newly created document, or <see langword="null"/> if the creation has failed.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Creating a new document of <paramref name="documentType"/> is not supported.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "New")]
        Document New(DocumentType documentType);


        /// <summary>
        /// Opens the document identified by the specified URI.
        /// </summary>
        /// <param name="uri">The URI of the document.</param>
        /// <returns>
        /// The newly opened document, or <see langword="null"/> if the document could not be
        /// loaded.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="uri"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The type of document is not supported.
        /// </exception>
        Document Open(Uri uri);


        /// <summary>
        /// Closes the specified document.
        /// </summary>
        /// <param name="document">The document to close.</param>
        /// <param name="force">
        /// <see langword="true"/> to close the document immediately; <see langword="false"/> to
        /// check for any unsaved changes and prompt the user to save the document.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the documents was closed; otherwise, <see langword="false"/>
        /// if the operation was canceled by the user.
        /// </returns>
        /// <remarks>
        /// If a savable document is modified and if <paramref name="force"/> is
        /// <see langword="false"/>, a dialog is shown to let the user save the document before it
        /// is closed. If the user cancels the save operation, the document is not closed and
        /// this method returns <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        bool Close(Document document, bool force);


        /// <summary>
        /// Saves the specified document.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if all changes were saved successfully or can be discarded;
        /// otherwise <see langword="false"/> if there are still changes that need to be saved.
        /// </returns>
        /// <remarks>
        /// If the document is untitled, a "Save as" dialog is shown to let the user choose a file
        /// name. I the user cancels the dialog, the document is not saved and this method returns
        /// <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        bool Save(Document document);


        /// <summary>
        /// Saves all documents.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if all documents were saved successfully; otherwise
        /// <see langword="false"/> if there are still documents with unsaved modifications.
        /// </returns>
        /// <remarks>This method calls <see cref="Save"/> for all documents.</remarks>
        bool SaveAll();
    }
}
