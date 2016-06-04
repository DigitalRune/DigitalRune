// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using ICSharpCode.AvalonEdit;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Provides helper method for text documents.
    /// </summary>
    public static class TextDocumentHelper
    {
        /// <summary>
        /// Gets the last active view model of the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The last active view model of the document, or <see langword="null"/> if the document 
        /// has no view models.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static TextDocumentViewModel GetLastActiveViewModel(this TextDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return document.ViewModels
                           .OrderByDescending(w => w.LastActivation)
                           .OfType<TextDocumentViewModel>()
                           .FirstOrDefault();
        }


        /// <summary>
        /// Gets the <see cref="TextEditor"/> control of any view of the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// A <see cref="TextEditor"/> control of a view, or <see langword="null"/> if no view is
        /// available.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static TextEditor GetAnyTextEditor(this TextDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return (document.ViewModels.LastOrDefault() as TextDocumentViewModel)?.TextEditor;
        }


        /// <summary>
        /// Gets the last active <see cref="TextEditor"/> control of of the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The last active <see cref="TextEditor"/> control of the document, or 
        /// <see langword="null"/> if no view is available.
        /// </returns>
        public static TextEditor GetLastActiveTextEditor(this TextDocument document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return document.GetLastActiveViewModel()?.TextEditor;
        }


        /// <summary>
        /// Checks whether the specified file contains an XML declaration.
        /// </summary>
        /// <param name="path">The file.</param>
        /// <returns>
        /// <see langword="true" /> if the file contains an XML declaration; otherwise, 
        /// <see langword="false" />.
        /// </returns>
        public static bool IsXmlFile(string path)
        {
            // ReSharper disable once EmptyGeneralCatchClause
            try
            {
                using (var reader = XmlReader.Create(path))
                {
                    Debug.Assert(reader.ReadState == ReadState.Initial);
                    Debug.Assert(reader.NodeType == XmlNodeType.None);

                    if (reader.Read())
                        return true;
                }
            }
            // ReSharper disable once UnusedVariable
            catch (Exception)
            {
                // Ignore exception.
            }

            return false;
        }
    }
}
