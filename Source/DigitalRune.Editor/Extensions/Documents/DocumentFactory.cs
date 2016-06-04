// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Creates specific types of documents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IDocumentService"/> has a list of document factories (property
    /// <see cref="IDocumentService.Factories"/>. It uses these factories to create documents, e.g.
    /// when the user selects the "New" or the "Open" menu.
    /// </para>
    /// <para>
    /// <see cref="EditorExtension"/>s have to provide the document factories, for example: A text
    /// editor extension will add a factory which creates text document to handle .txt files. An
    /// image viewer extension will add a factory which creates image documents to handle images.
    /// </para>
    /// <para>
    /// Each document factory can handle several document types; e.g. a text editor could also
    /// handle XML (.xml) files. The property <see cref="DocumentTypes"/> defines which documents
    /// are supported by the factory. A text editor could basically handle any file, but it should
    /// have a lower priority (see <see cref="DocumentType.Priority"/>) than a dedicated document
    /// factory. For example, a text editor should handle FBX files with a lower priority than a 3D
    /// model editor.
    /// </para>
    /// <para>
    /// The <see cref="DocumentFactory"/> base class cannot create any documents. Derived classes
    /// have to implement the <see cref="OnCreate"/> method.
    /// </para>
    /// </remarks>
    public abstract class DocumentFactory : INamedObject
    {
        /// <summary>
        /// Gets the name of the document factory.
        /// </summary>
        /// <value>The name of the document factory, e.g. "Text Editor", "Model Viewer".</value>
        public string Name { get; }


        /// <summary>
        /// Gets the supported document types.
        /// </summary>
        /// <value>The supported document types.</value>
        public IEnumerable<DocumentType> DocumentTypes { get; protected set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentFactory"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="name"/> is empty.
        /// </exception>
        protected DocumentFactory(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("Name must not be empty.", nameof(name));

            Name = name;
        }


        /// <summary>
        /// Gets the document type for the given document URI.
        /// </summary>
        /// <param name="uri">
        /// The URI of the document (usually a local path, such as "C:\path\file.ext").
        /// </param>
        /// <returns>
        /// The <see cref="DocumentType"/> if the document is supported; otherwise,
        /// <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// The default implementation in <see cref="DocumentFactory"/> compares the file extension
        /// of the <paramref name="uri"/> with the file extensions of the
        /// <see cref="DocumentTypes"/>. Derived classes can override this method if they want to
        /// make more elaborate checks, e.g. checking the file content.
        /// </remarks>
        public virtual DocumentType GetDocumentType(Uri uri)
        {
            return DocumentHelper.GetDocumentType(DocumentTypes, uri);
        }


        /// <summary>
        /// Creates a new document of the specified type.
        /// </summary>
        /// <param name="documentType">The type of the document.</param>
        /// <returns>
        /// The newly created document if successful; otherwise, <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The document type belongs to a different factory.
        /// </exception>
        internal Document Create(DocumentType documentType)
        {
            if (documentType == null)
                throw new ArgumentNullException(nameof(documentType));
            if (documentType.Factory != this)
                throw new ArgumentException("The document type belongs to a different factory.");

            return OnCreate(documentType);
        }


        /// <summary>
        /// Creates a new document of the specified type.
        /// </summary>
        /// <param name="documentType">Type of the document.</param>
        /// <returns>
        /// The newly created document if successful; otherwise, <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// When this method is called, it is guaranteed that <paramref name="documentType"/> is
        /// not <see langword="null"/> and that it is a document type of this factory.
        /// </remarks>
        protected abstract Document OnCreate(DocumentType documentType);
    }
}
