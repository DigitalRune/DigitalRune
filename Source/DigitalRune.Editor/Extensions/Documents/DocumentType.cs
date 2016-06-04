// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using System.Windows.Media;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Describes a type of document and the <see cref="DocumentFactory"/> that is used to
    /// create/open these documents.
    /// </summary>
    /// <remarks>
    /// Note that multiple document handlers may handle the same type of documents (e.g. ".txt"
    /// files). Each document factory needs to define its own <see cref="DocumentType"/> instance.
    /// See <see cref="DocumentFactory.DocumentTypes"/>.
    /// </remarks>
    public class DocumentType
    {
        /// <inheritdoc/>
        public string Name { get; }


        /// <summary>
        /// Gets the document factory that handles this type of document.
        /// </summary>
        /// <value>The document factory that handles this type of document.</value>
        public DocumentFactory Factory { get; }


        /// <summary>
        /// Gets the icon ( <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) the document
        /// type.
        /// </summary>
        /// <value>The icon representing the document type.</value>
        /// <remarks>
        /// This icon is the default symbol for documents of this type. It can also be shown in
        /// menus (i.e. in the "New" menu).
        /// </remarks>
        public object Icon { get; }


        /// <summary>
        /// Gets the file extension(s).
        /// </summary>
        /// <value>
        /// The file extension(s). Example: ".txt".
        /// </value>
        /// <inheritdoc/>
        public IEnumerable<string> FileExtensions { get; }


        /// <summary>
        /// Gets a value indicating whether this document type should be listed in the "New" menu.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the document type should be listed in the "New" menu;
        /// otherwise, <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsCreatable { get; }


        /// <summary>
        /// Gets a value indicating whether existing documents of this type can be loaded.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if existing documents of this type can be loaded; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsLoadable { get; }


        /// <summary>
        /// Gets a value indicating whether documents of this type can be saved.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if documents of this type can be saved; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsSavable { get; }


        /// <summary>
        /// Gets the priority.
        /// </summary>
        /// <value>The priority.</value>
        /// <remarks>
        /// <para>
        /// The priority determines which document factory will be used to open a URI. Multiple
        /// document handlers can support the same document type. Unless explicitly specified, the
        /// document service chooses the document factory with the highest priority.
        /// </para>
        /// <para>
        /// The default priority is 0. Usually a general purpose editor, like text editor, uses the
        /// priority 0. A specialized editor, an FBX model editor, should use a higher priority for
        /// its file types. which can open all file types.
        /// </para>
        /// </remarks>
        public int Priority { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentType" /> class.
        /// </summary>
        /// <param name="name">The name of the document type.</param>
        /// <param name="factory">The document factory.</param>
        /// <param name="icon">
        /// The document icon (<see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>).
        /// </param>
        /// <param name="fileExtensions">The <see cref="FileExtensions"/>.</param>
        /// <param name="isCreatable">
        /// <see langword = "true" /> if the document type should be listed in the "New" menu;
        /// otherwise, <see langword="false"/>.
        /// </param>
        /// <param name="isLoadable">
        /// <see langword="true"/> if existing documents of this type can be loaded; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <param name="isSavable">
        /// <see langword="true"/> if existing documents of this type can be saved; otherwise,
        /// <see langword="false"/>.
        /// </param>
        /// <param name="priority">
        /// The <see cref="Priority"/>.
        /// </param>
        public DocumentType(
            string name, DocumentFactory factory, object icon = null,
            IEnumerable<string> fileExtensions = null, bool isCreatable = false,
            bool isLoadable = false, bool isSavable = false, int priority = 0)
        {
            Name = name;
            Factory = factory;
            Icon = icon;
            FileExtensions = fileExtensions;
            IsCreatable = isCreatable;
            IsLoadable = isLoadable;
            IsSavable = isSavable;
            Priority = priority;
        }
    }
}
