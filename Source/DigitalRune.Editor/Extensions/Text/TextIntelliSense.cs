// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Provides code completion for a text document.
    /// </summary>
    /// <remarks>
    /// This class simply parses the whole text and adds all words to the completion data. The
    /// completion window opens when CTRL+Space is pressed or when key matching an existing word
    /// is pressed.
    /// </remarks>
    internal class TextIntelliSense
    {
        // TODO: Optimize.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly TextEditor _textEditor;
        private CompletionWindow _completionWindow;

        // We have to close the completion window if the caret has moved back before this offset.
        private int _completionStartOffset;

        // This flag avoids unnecessary CompletionWindow updates in OnCaretPositionChanged().
        private bool _textIsUpdating;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        private TextIntelliSense(TextEditor textEditor)
        {
            //if (textEditor == null)
            //    throw new ArgumentNullException(nameof(textEditor));

            _textEditor = textEditor;

            // Event handlers that control IntelliSense.
            textEditor.PreviewKeyDown += OnPreviewKeyDown;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        #region ----- Initialization -----

        public static void EnableIntelliSense(TextDocument document)
        {
            // Register event handler that enables IntelliSense on every view which is
            // added to the document.
            ((INotifyCollectionChanged)document.ViewModels).CollectionChanged += OnViewModelsChanged;
        }


        private static void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var viewModel in eventArgs.NewItems.OfType<TextDocumentViewModel>())
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var viewModel in eventArgs.OldItems.OfType<TextDocumentViewModel>())
                    viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                var document = (TextDocument)sender;
                foreach (var viewModel in document.ViewModels)
                {
                    viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                    viewModel.PropertyChanged += OnViewModelPropertyChanged;
                }
            }
        }


        private static void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // Note: When the view is re-docked or the theme changes, a new TextEditor is created.
            if (eventArgs.PropertyName == nameof(TextDocumentViewModel.TextEditor))
            {
                var textEditor = ((TextDocumentViewModel)sender).TextEditor;
                if (textEditor != null)
                    new TextIntelliSense(textEditor);
            }
        }
        #endregion


        #region ----- Event handling -----

        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // ----- Ctrl+Space --> Show completion data.
                // TODO: Create commands for IntelliSense.
                RequestCompletionWindow('\0', true);
                eventArgs.Handled = true;
            }
        }


        private void OnTextEntering(object sender, TextCompositionEventArgs eventArgs)
        {
            _textIsUpdating = true;

            string insertedText = eventArgs.Text;
            if (!string.IsNullOrEmpty(insertedText) && insertedText.Length == 1)
            {
                // The user has typed a single character. --> Update content of IntelliSense windows.
                RequestCompletionWindow(insertedText[0], false);
            }
        }


        private void OnTextEntered(object sender, TextCompositionEventArgs eventArgs)
        {
            _textIsUpdating = false;
        }


        private void OnCaretPositionChanged(object sender, EventArgs eventArgs)
        {
            // When text is entered, there is nothing todo.
            if (_textIsUpdating)
                return;

            // The caret could have changed because of a Backspace or similar operations.
            // If the caret has moved back to far we close the completion window.
            if (_completionWindow != null && _textEditor.CaretOffset < _completionStartOffset)
                _completionWindow.Close();

            // When a backspace is pressed, we get no TextEntering events but we still want to
            // update the filter of the completion window.
            if (_completionWindow != null)
                RequestCompletionWindow('\0', false);
        }
        #endregion


        /// <summary>
        /// Requests the completion window.
        /// </summary>
        /// <param name="key">The last typed character that was not yet inserted in the document or '\0'.</param>
        /// <param name="explicitRequest">
        /// If set to <see langword="true"/> this is an explicit request to show the window; if <see langword="false"/>
        /// the user is typing.
        /// </param>
        private void RequestCompletionWindow(char key, bool explicitRequest)
        {
            var document = _textEditor.Document;
            int caretOffset = _textEditor.CaretOffset;

            // ----- Abort if a key is set and it is not part of an identifier.
            bool letterDigitOrUnderscorePressed = IsIdentifierCharacter(key);
            if (key != '\0' && !letterDigitOrUnderscorePressed)
            {
                _completionWindow?.Close();
                return;
            }

            // ----- Find the symbol immediately before the caret. 
            string symbol;
            int offset = FindStartOfIdentifier(document, caretOffset - 1);
            if (offset >= 0)
            {
                symbol = document.GetText(offset, caretOffset - offset);
            }
            else
            {
                symbol = string.Empty;
                offset = caretOffset;
            }

            // ----- Find the character before the symbol.
            --offset;
            char previousCharacter = (offset >= 0) ? document.GetCharAt(offset) : '\0';

            // If we know nothing about the key, and we are after a blank, we don't want to open
            // a completion window. 
            // If we have a completion window, we want to continue.
            // If the user has explicitly request info (e.g. Ctrl+Space) we want to continue.
            if (key == '\0' && previousCharacter == ' ' && !explicitRequest && _completionWindow == null)
            {
                return;
            }

            // Get the text used to filter the list box.
            string selectionText;
            if (string.IsNullOrEmpty(symbol))
                selectionText = (key != '\0') ? key.ToString() : string.Empty;
            else if (key != '\0')
                selectionText = symbol + key;
            else
                selectionText = symbol;

            var completionData = GetCompletionData(_textEditor.Text, selectionText);
            
            // If there is no completion data in this context, we close existing windows.
            if (completionData == null || completionData.Length == 0)
            {
                _completionWindow?.Close();
                return;
            }

            // Sort the completion data.
            var filteredData = completionData.OrderBy(d => d.Text);

            // Create new window or we only update the existing one.
            if (_completionWindow == null)
            {
                _completionWindow = new CompletionWindow(_textEditor.TextArea) { MaxHeight = 300 };
                
                // Delete instance when window is closed.
                _completionWindow.Closed += delegate { _completionWindow = null; };
            }

            // Set new completion items.
            _completionWindow.CompletionList.CompletionData.Clear();
            string previousEntry = null;
            foreach (var data in filteredData)
            {
                if (data.Text != selectionText && data.Text != previousEntry)
                    _completionWindow.CompletionList.CompletionData.Add(data);

                previousEntry = data.Text;
            }

            // If there is no completion data in this context, we close existing windows.
            if (_completionWindow.CompletionList.CompletionData.Count == 0)
            {
                _completionWindow?.Close();
                return;
            }
            
            // Select current item.
            if (!string.IsNullOrEmpty(selectionText))
                _completionWindow.CompletionList.SelectItem(selectionText);

            // Remember start offset of current identifier, to close window when cursor
            // moves back too far.
            if (symbol != null)
            {
                _completionStartOffset = _textEditor.CaretOffset - symbol.Length;
                _completionWindow.StartOffset = _completionStartOffset;
            }
            else
            {
                _completionStartOffset = _textEditor.CaretOffset;
            }

            // Show window.
            _completionWindow.Show();
        }


        /// <summary>
        /// Skips the white space backwards.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the first character to check.</param>
        /// <returns>The offset of the first non-whitespace character before <paramref name="offset"/>.</returns>
        public int SkipWhiteSpaceBackwards(ITextSource document, int offset)
        {
            while (offset >= 1 && char.IsWhiteSpace(document.GetCharAt(offset)))
                --offset;

            return offset;
        }


        private static CompletionData[] GetCompletionData(string text, string selectionText)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            var textLength = text.Length;
            var stringBuilder = new StringBuilder();
            var completionData = new List<CompletionData>();
            for (int i = 0; i < textLength; i++)
            {
                var c = text[i];
                if (IsIdentifierCharacter(c))
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    if (stringBuilder.Length > 3)    // Skip short words.
                    {
                        var word = stringBuilder.ToString();
                        if (word.StartsWith(selectionText, StringComparison.Ordinal))
                            completionData.Add(new CompletionData(word, null, null));
                    }

                    stringBuilder.Clear();
                }
            }

            return completionData.ToArray();
        }


        private static bool IsIdentifierCharacter(char c)
        {
            return (TextUtilities.GetCharacterClass(c) == CharacterClass.IdentifierPart);
        }


        /// <summary>
        /// Finds the start of the identifier at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The offset of the first character of the identifier; or -1 if there is no identifier at
        /// the specified offset.
        /// </returns>
        /// <remarks>
        /// <para>
        /// An identifier is a single word consisting of letters, digits, or underscores. An
        /// identifier must start with a letter or underscore.
        /// </para>
        /// </remarks>
        private static int FindStartOfIdentifier(ITextSource document, int offset)
        {
            if (offset < 0 || document.TextLength <= offset)
                return -1;

            if (!IsIdentifierCharacter(document.GetCharAt(offset)))
            {
                // Character at offset is does not belong to an identifier.
                return -1;
            }

            // Search backwards
            while (0 < offset && IsIdentifierCharacter(document.GetCharAt(offset - 1)))
                --offset;

            return offset;
        }
        #endregion
    }
}
