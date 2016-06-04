// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using DigitalRune.Windows.Docking;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Provides helper methods for documents.
    /// </summary>
    public static class DocumentHelper
    {
        /// <summary>
        /// Gets the documents ordered by appearance in the editor.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <param name="documentService">The document service.</param>
        /// <param name="activeDocument">The active document. Can be <see langword="null"/>.</param>
        /// <returns>The documents ordered by appearance.</returns>
        /// <remarks>
        /// When the users goes through the documents (e.g. searches all documents), the documents
        /// should be ordered by the appearance in the editor, starting with the
        /// <paramref name="activeDocument"/> if specified.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> or <paramref name="documentService"/> is
        /// <see langword="null"/>.
        /// </exception>
        internal static IEnumerable<Document> GetOrderedDocuments(IEditorService editor, IDocumentService documentService, Document activeDocument)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));

            var dockElements = editor.RootPane
                                     .GetDockElements()
                                     .Where(element => element.DockState == DockState.Dock);
            var floatElements = editor.FloatWindows
                                      .SelectMany(floatWindow => floatWindow.RootPane.GetDockElements())
                                      .Where(element => element.DockState == DockState.Float);
            var autoHideLeftElements = editor.AutoHideLeft
                                             .GetDockElements()
                                             .Where(element => element.DockState == DockState.AutoHide);
            var autoHideTopElements = editor.AutoHideTop
                                            .GetDockElements()
                                            .Where(element => element.DockState == DockState.AutoHide);
            var autoHideRightElements = editor.AutoHideRight
                                             .GetDockElements()
                                             .Where(element => element.DockState == DockState.AutoHide);
            var autoHideBottomElements = editor.AutoHideBottom
                                               .GetDockElements()
                                               .Where(element => element.DockState == DockState.AutoHide);
            var documents = dockElements.Concat(floatElements)
                                        .Concat(autoHideLeftElements)
                                        .Concat(autoHideTopElements)
                                        .Concat(autoHideRightElements)
                                        .Concat(autoHideBottomElements)
                                        .OfType<DocumentViewModel>()
                                        .Where(viewModel => viewModel.IsOpen)
                                        .Select(viewModel => viewModel.Document)
                                        .Concat(documentService.Documents)
                                        .Distinct();

            var queue = new Queue<Document>(documents);
            if (activeDocument == null || !queue.Contains(activeDocument))
                return queue;

            // Rotate documents until activeDocument is the first.
            while (queue.Peek() != activeDocument)
                queue.Rotate();

            return queue;
        }


        private static void Rotate<T>(this Queue<T> queue)
        {
            Debug.Assert(queue != null);
            var item = queue.Dequeue();
            queue.Enqueue(item);
        }


        /// <summary>
        /// Gets a collection of all document types that can be created.
        /// </summary>
        /// <returns>A collection of all document types that can be created.</returns>
        /// <remarks>
        /// If a document type is handled by multiple document handlers, only the document type of
        /// the document handlers with the highest priority is listed.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentService"/> is <see langword="null"/>.
        /// </exception>
        internal static IEnumerable<DocumentType> GetCreatableDocumentTypes(this IDocumentService documentService)
        {
            if (documentService == null)
                throw new ArgumentNullException(nameof(documentService));

            var documentTypes = new SortedList<string, DocumentType>();
            var allCreatableTypes = documentService.Factories
                                                   .SelectMany(documentHandler => documentHandler.DocumentTypes)
                                                   .Where(documentType => documentType.IsCreatable);

            foreach (var documentType in allCreatableTypes)
            {
                if (!documentTypes.ContainsKey(documentType.Name))
                {
                    // Add new document type
                    documentTypes.Add(documentType.Name, documentType);
                }
                else
                {
                    var existingDocumentType = documentTypes[documentType.Name];
                    if (documentType.Priority >= existingDocumentType.Priority)
                    {
                        // Replace existing document type
                        documentTypes[documentType.Name] = documentType;
                    }
                }
            }

            return documentTypes.Values;
        }


        /// <summary>
        /// Gets a name that represents the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>A name that represents the document.</returns>
        /// <remarks>
        /// If the document is untitled (see <see cref="Document.IsUntitled"/>, the
        /// <see cref="Document.UntitledName"/> is returned; otherwise the local path of the
        /// <see cref="Document.Uri"/> is returned.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        public static string GetName(this Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return document.IsUntitled ? document.UntitledName : document.Uri.LocalPath;
        }


        /// <summary>
        /// Gets the display name of the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The display name of the document that should be used in menus, window titles, etc.
        /// </returns>
        /// <remarks>
        /// If the document is untitled (see <see cref="Document.IsUntitled"/>, the 
        /// <see cref="Document.UntitledName"/> is returned; otherwise the local path of the 
        /// <see cref="Document.Uri"/> is returned. If the document is modified, a "*" is added
        /// to the string.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> is <see langword="null"/>.
        /// </exception>
        internal static string GetDisplayName(this Document document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            string title;
            if (document.IsUntitled)
                title = document.UntitledName;
            else if (document.Uri != null)
                title = Path.GetFileName(document.Uri.LocalPath);
            else
                title = string.Empty; // Title is not yet set.

            // Add "*" for documents that need to be saved.
            if (document.IsModified)
                title += "*";

            return title;
        }


        /// <summary>
        /// Creates the filter string that can be used in the "Save As" dialog.
        /// </summary>
        /// <param name="documentTypes">The document types.</param>
        /// <returns>The filter string.</returns>
        public static string GetFilterString(IEnumerable<DocumentType> documentTypes)
        {
            if (documentTypes == null)
                throw new ArgumentNullException(nameof(documentTypes));

            // Example: "DirectX Effect File (*.fx; *.fxh)|*.fx;*.fxh|NVIDIA Cg File (*.cg; *.cgh)|*.cg;*.cgh|All Files (*.*)|*.*";

            var stringBuilder = new StringBuilder();
            foreach (var documentType in documentTypes)
            {
                stringBuilder.Append(documentType.Name);

                var fileExtensions = documentType.FileExtensions.ToArray();
                stringBuilder.Append(" (");
                stringBuilder.AppendFileExtensions(fileExtensions, "; ");
                stringBuilder.Append(")|");
                stringBuilder.AppendFileExtensions(fileExtensions, ";");
                stringBuilder.Append("|");
            }

            stringBuilder.Append("All Files (*.*)|*.*");
            return stringBuilder.ToString();
        }


        private static void AppendFileExtensions(this StringBuilder builder, string[] fileExtensions, string separator)
        {
            for (int i = 0; i < fileExtensions.Length - 1; i++)
            {
                builder.Append('*');
                builder.Append(fileExtensions[i]);
                builder.Append(separator);
            }

            builder.Append('*');
            builder.Append(fileExtensions[fileExtensions.Length - 1]);
        }


        /// <summary>
        /// Determines whether the URI represents a document of the specified document types.
        /// </summary>
        /// <param name="documentTypes">The document types.</param>
        /// <param name="uri">
        /// The URI of the document (usually a local path, such as "C:\path\file.ext").
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the document type is one of the specified types; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method compares only the file extension of the <paramref name="uri"/> with the
        /// file extensions of the <paramref name="documentTypes"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTypes"/> or <paramref name="uri"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">The URI is a relative URI.</exception>
        internal static bool IsSupported(IEnumerable<DocumentType> documentTypes, Uri uri)
        {
            if (documentTypes == null)
                throw new ArgumentNullException(nameof(documentTypes));
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The URI must be an absolute URI.", nameof(uri));

            // Find document type with matching extension.
            var extension = Path.GetExtension(uri.LocalPath);
            foreach (var documentType in documentTypes)
            {
                if (documentType.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
                    || documentType.FileExtensions.Contains(".*", StringComparer.Ordinal))
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Gets the type of the document.
        /// </summary>
        /// <param name="documentTypes">The document types.</param>
        /// <param name="uri">The URI.</param>
        /// <returns>
        /// The document type or <see langword="null"/> if the document type is not in the specified
        /// list.
        /// </returns>
        /// <remarks>
        /// This method compares only the file extension of the <paramref name="uri"/> with the file
        /// extensions of the <paramref name="documentTypes"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentTypes"/> or <paramref name="uri"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">The URI is a relative URI.</exception>
        internal static DocumentType GetDocumentType(IEnumerable<DocumentType> documentTypes, Uri uri)
        {
            if (documentTypes == null)
                throw new ArgumentNullException(nameof(documentTypes));
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (!uri.IsAbsoluteUri)
                throw new ArgumentException("The URI must be an absolute URI.", nameof(uri));

            // Find document type with matching extension.
            var extension = Path.GetExtension(uri.LocalPath);
            DocumentType bestDocumentType = null;
            foreach (var documentType in documentTypes)
            {
                if (documentType.FileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
                    || documentType.FileExtensions.Contains(".*", StringComparer.Ordinal))
                {
                    if (bestDocumentType == null || bestDocumentType.Priority < documentType.Priority)
                        bestDocumentType = documentType;
                }
            }

            return bestDocumentType;
        }
    }
}
