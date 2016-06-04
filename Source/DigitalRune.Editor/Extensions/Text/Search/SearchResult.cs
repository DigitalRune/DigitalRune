// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Text.RegularExpressions;
using DigitalRune.Editor.Search;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Represents a result of a text search in a <see cref="TextDocument"/>.
    /// </summary>
    internal class SearchResult : TextSegment, ISearchResult
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly TextDocument _document;
        private readonly Match _match;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public bool IsSelected
        {
            get
            {
                var textEditor = _document.GetLastActiveTextEditor();
                if (textEditor == null)
                    return false;

                // Check whether current search result matches selection in text editor.
                int startOffset = StartOffset;
                int length = Length;
                if (startOffset != textEditor.SelectionStart || length != textEditor.SelectionLength)
                    return false;

                // Check whether caret is on current selection. (In case the text editor
                // supports multiple simultaneous selections.)
                int caretOffset = textEditor.CaretOffset;
                if (caretOffset < startOffset || caretOffset > startOffset + length)
                    return false;

                // Check whether content of text editor has changed.
                if (_match.Value != _document.AvalonEditDocument.GetText(startOffset, length))
                    return false;

                return true;
            }
            set
            {
                var view = _document.GetLastActiveViewModel();
                var textEditor = view?.TextEditor;
                if (textEditor == null)
                    return;

                if (value)
                {
                    // Select search result in TextEditor control.
                    textEditor.Select(StartOffset, Length);

                    // ----- Ensure that search result is visible.
                    // Unfold text segments.
                    var foldingManager = textEditor.TextArea.GetService(typeof(FoldingManager)) as FoldingManager;
                    if (foldingManager != null)
                    {
                        foreach (var folding in foldingManager.GetFoldingsContaining(StartOffset))
                            folding.IsFolded = false;
                    }

                    // Show document if hidden.
                    view.Conductor?.ActivateItemAsync(view);

                    // Position caret at start of selection.
                    textEditor.TextArea.Caret.Offset = StartOffset;
                    textEditor.TextArea.Caret.BringCaretToView();

                    // Show caret, even if text editor does not have focus.
                    textEditor.TextArea.Caret.Show();
                }
                else
                {
                    // Clear selection. 
                    textEditor.TextArea.ClearSelection();
                }
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResult"/> class.
        /// </summary>
        /// <param name="document">The text document.</param>
        /// <param name="match">The regular expression match.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="document"/> or <paramref name="match"/> is <see langword="null"/>.
        /// </exception>
        public SearchResult(TextDocument document, Match match)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            if (match == null)
                throw new ArgumentNullException(nameof(match));

            _document = document;
            StartOffset = match.Index;
            Length = match.Length;
            _match = match;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public void Replace(string replacement)
        {
            // Replace string.
            replacement = _match.Result(replacement ?? string.Empty);
            _document.AvalonEditDocument.Replace(StartOffset, Length, replacement);

            // Move caret to the end of the replacement.
            var textEditor = _document.GetLastActiveTextEditor();
            if (textEditor != null)
            {
                textEditor.TextArea.ClearSelection();
                textEditor.CaretOffset = StartOffset + replacement.Length;
            }
        }
        #endregion
    }
}
