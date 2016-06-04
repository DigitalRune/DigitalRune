// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using DigitalRune.Collections;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;


namespace DigitalRune.Editor.Shader
{
    internal abstract partial class ShaderIntelliSense
    {
        private TextEditor _textEditor;

        private CompletionWindow _completionWindow;
        private OverloadInsightWindow _overloadInsightWindow;
        private ToolTip _toolTip;

        // We have to close the completion window if the caret has moved back before this offset.
        private int _completionStartOffset;

        // This flag avoids unnecessary CompletionWindow updates in OnCaretPositionChanged().
        private bool _textIsUpdating;


        /// <summary>
        /// Adds IntelliSense to a text editor control.
        /// </summary>
        /// <param name="textEditor">
        /// The <see cref="ICSharpCode.AvalonEdit.TextEditor"/> control.
        /// </param>
        public void ConfigureTextEditor(TextEditor textEditor)
        {
            if (textEditor == null)
                throw new ArgumentNullException(nameof(textEditor));

            _textEditor = textEditor;

            // Event handlers that control IntelliSense.
            textEditor.PreviewKeyDown += OnPreviewKeyDown;
            textEditor.TextArea.TextEntering += OnTextEntering;
            textEditor.TextArea.TextEntered += OnTextEntered;
            textEditor.MouseHover += OnTextEditorMouseHover;
            textEditor.MouseHoverStopped += OnTextEditorMouseHoverStopped;
            textEditor.TextArea.Caret.PositionChanged += OnCaretPositionChanged;
        }


        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (_completionWindow == null && eventArgs.Key == Key.Tab)
            {
                // ------ Snippet Tab Completion
                // Tab pressed --> If word before tab is a snippet text, insert snippet.
                string word = GetWordBeforeCaret(_textEditor);

                //string word = TextUtilities.GetWordBeforeCaret(_textEditor.TextArea);
                if (!string.IsNullOrEmpty(word))
                {
                    SnippetCompletionData snippet;
                    if (Snippets.TryGetValue(word, out snippet))
                    {
                        int caretOffset = _textEditor.CaretOffset;
                        int wordStartOffset = caretOffset - word.Length;
                        snippet.Complete(_textEditor.TextArea, new TextSegment { StartOffset = wordStartOffset, Length = word.Length }, eventArgs);
                        eventArgs.Handled = true;
                    }
                }
            }
            else if (eventArgs.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // ----- Ctrl+Space --> Show completion data.
                // TODO: Create commands for IntelliSense.
                RequestCompletionWindow('\0', true);
                eventArgs.Handled = true;
            }
        }


        private static string GetWordBeforeCaret(TextEditor textEditor)
        {
            TextDocument document = textEditor.Document;
            int caretOffset = textEditor.CaretOffset;
            int startOfWord = TextUtilities.GetNextCaretPosition(document, caretOffset, LogicalDirection.Backward, CaretPositioningMode.WordStart);
            return (startOfWord >= 0) ? document.GetText(startOfWord, caretOffset - startOfWord) : string.Empty;
        }


        private void OnTextEntering(object sender, TextCompositionEventArgs eventArgs)
        {
            _textIsUpdating = true;

            string insertedText = eventArgs.Text;
            if (!string.IsNullOrEmpty(insertedText) && insertedText.Length == 1)
            {
                // The user has typed a single character. --> Update content of IntelliSense windows.
                RequestCompletionWindow(insertedText[0], false);
                RequestInsightWindow(insertedText[0]);
            }
        }


        private void OnTextEntered(object sender, TextCompositionEventArgs eventArgs)
        {
            _textIsUpdating = false;

            // In some cases a completion window should be shown after a character is already inserted in the 
            // document. For example: After the user types "myTexture." a completion window for all
            // member function shall be shown.
            // TODO: If we change the parsing in RequestCompletionWindow() we could remove this and only use OnTextEntering.

            if (_completionWindow == null)
            {
                // We want to show the completion window in following cases:
                //    identifier.|      (e.g. myTexture.)
                //    MinFilter = |     (e.g. in a sampler state).
                string insertedText = eventArgs.Text;
                if (!string.IsNullOrEmpty(insertedText))
                {
                    char lastChar = insertedText[insertedText.Length - 1];
                    switch (lastChar)
                    {
                        case '.':
                        case ' ':
                            RequestCompletionWindow('\0', false);
                            break;
                        default:
                            break;
                    }
                    return;
                }
            }
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


        //--------------------------------------------------------------
        #region Completion Data
        //--------------------------------------------------------------

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
            // ----- Determine ShaderRegion of current caret position.
            var document = _textEditor.Document;
            int caretOffset = _textEditor.CaretOffset;

            IList<NamedCompletionData> collectedIdentifiers;
            IList<NamedCompletionData> collectedFields;
            ShaderRegion region = _parser.IdentifyRegion(document, caretOffset, out collectedIdentifiers, out collectedFields);

            // ----- Abort if current region has no IntelliSense.
            switch (region)
            {
                case ShaderRegion.Unknown:
                case ShaderRegion.Assembler:
                case ShaderRegion.LineComment:
                case ShaderRegion.BlockComment:
                case ShaderRegion.String:
                case ShaderRegion.CharacterLiteral:
                    // No IntelliSense for these regions.
                    return;
            }

            // ----- # --> PreprocessorCompletionData
            if (key == '#')
            {
                char characterBeforeHash = (caretOffset > 0) ? document.GetCharAt(caretOffset - 1) : '\0';
                if (characterBeforeHash == '\0' || char.IsWhiteSpace(characterBeforeHash))
                    ShowCompletionWindow(PreprocessorCompletionData, null, '#');
                return;
            }

            // ----- Abort if a key is set and it is not part of an identifier.
            bool letterDigitOrUnderscorePressed = (TextUtilities.GetCharacterClass(key) == CharacterClass.IdentifierPart);
            if (key != '\0' && !letterDigitOrUnderscorePressed)
            {
                _completionWindow?.Close();
                return;
            }

            // ----- Find the symbol immediately before the caret. 
            string symbol;
            int offset = TextUtilities.FindStartOfIdentifier(document, caretOffset - 1);
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

            // ----- #xxxx --> PreprocessorCompletionData
            // Check for preprocessor directives
            if (previousCharacter == '#')
            {
                --offset;
                char characterBeforeHash = (0 <= offset) ? document.GetCharAt(offset) : '\0';
                if (characterBeforeHash == '\0' || char.IsWhiteSpace(characterBeforeHash))
                {
                    symbol = '#' + symbol;
                    ShowCompletionWindow(PreprocessorCompletionData, symbol, key);
                }
                return;
            }

            // ----- xxxxx. --> MemberCompletionData and guessed fields.
            // Check for object members such as "myTexture.|"
            if (previousCharacter == '.')
            {
                if (region == ShaderRegion.StructureOrInterface || region == ShaderRegion.Code)
                {
                    --offset;
                    bool showCompletionWindow = false;
                    char characterBeforeDot = (0 <= offset) ? document.GetCharAt(offset) : '\0';
                    if (characterBeforeDot == ']' || characterBeforeDot == ')')
                    {
                        // Looks like something similar to "myArray[n].|" or "GetTexture(n).|".
                        // We assume that a member is requested.
                        showCompletionWindow = true;
                    }
                    else
                    {
                        // Check if we have something like "myVariable.|"
                        string objectName = TextUtilities.GetIdentifierAt(document, offset);
                        if (!string.IsNullOrEmpty(objectName) && !Keywords.Contains(objectName) && !_parser.IsType(objectName))
                            showCompletionWindow = true;
                    }
                    if (showCompletionWindow)
                        ShowCompletionWindow(MergeCompletionData(MemberCompletionData, collectedFields), symbol, key);
                }
                return;
            }

            // ----- Get non-whitespace character before symbol. 
            char previousNonWhitespaceChar = previousCharacter;
            if (char.IsWhiteSpace(previousNonWhitespaceChar) && offset > 0)
            {
                offset = _parser.SkipWhiteSpaceBackwards(document, offset - 1);
                previousNonWhitespaceChar = document.GetCharAt(offset);
            }

            // ----- Check for effect state assignments
            if (previousNonWhitespaceChar == '=' && _parser.IsStateGroup(region))
            {
                // The line should look like this:
                //   "EffectState = |"

                --offset;
                offset = _parser.SkipWhiteSpaceBackwards(document, offset);

                // Skip index
                if (document.GetCharAt(offset) == ']')
                    offset = TextUtilities.FindOpeningBracket(document, offset - 1, '[', ']') - 1;

                string stateName = TextUtilities.GetIdentifierAt(document, offset);
                if (!string.IsNullOrEmpty(stateName))
                {
                    // Lookup the state and show the allowed values.
                    NamedObjectCollection<NamedCompletionData> lookupTable = null;
                    switch (region)
                    {
                        case ShaderRegion.BlendState10:
                            lookupTable = BlendStates;
                            break;
                        case ShaderRegion.DepthStencilState10:
                            lookupTable = DepthStencilStates;
                            break;
                        case ShaderRegion.RasterizerState10:
                            lookupTable = RasterizerStates;
                            break;
                        case ShaderRegion.SamplerState:
                            lookupTable = SamplerStates;
                            break;
                        case ShaderRegion.SamplerState10:
                            lookupTable = SamplerStates10;
                            break;
                        case ShaderRegion.TechniqueOrPass:
                        case ShaderRegion.TechniqueOrPass10:
                        case ShaderRegion.StateBlock:
                            lookupTable = EffectStates;
                            break;
                        default:
                            break;
                    }

                    NamedCompletionData state;
                    if (lookupTable != null && lookupTable.TryGet(stateName, out state))
                    {
                        List<NamedCompletionData> stateValueCompletionData;
                        string[] values = ((StateCompletionData)state).AllowedValues;
                        if (values.Length > 0)
                        {
                            // Add the allowed values for this state to completion data.
                            stateValueCompletionData = new List<NamedCompletionData>(values.Length);
                            foreach (string value in values)
                                stateValueCompletionData.Add(EffectStateValues[value]);
                        }
                        else
                        {
                            // This effect state has a generic parameter.
                            // Add types ("bool", "int", etc.) to the completion data.
                            stateValueCompletionData = new List<NamedCompletionData>(ScalarTypes.Count + Types.Count);
                            foreach (NamedCompletionData type in ScalarTypes)
                                stateValueCompletionData.Add(type);
                            foreach (NamedCompletionData type in Types)
                                stateValueCompletionData.Add(type);
                        }

                        // Add the collected identifiers
                        foreach (NamedCompletionData collectedIdentifier in collectedIdentifiers)
                            stateValueCompletionData.Add(collectedIdentifier);

                        ShowCompletionWindow(stateValueCompletionData.ToArray(), symbol, key);
                        return;
                    }
                }
            }

            // If we know nothing about the key, and we are after a blank, we don't want to open
            // a completion window. 
            // If we have a completion window, we want to continue.
            // If the user has explicitly request info (e.g. Ctrl+Space) we want to continue.
            if (key == '\0' && previousCharacter == ' ' && !explicitRequest && _completionWindow == null)
            {
                return;
            }

            // Show default completion data for each region.
            ICompletionData[] completionData;
            switch (region)
            {
                case ShaderRegion.Global:
                    completionData = GlobalCompletionData;
                    break;
                case ShaderRegion.StructureOrInterface:
                case ShaderRegion.Code:
                    completionData = CodeCompletionData;
                    break;
                case ShaderRegion.Annotation:
                    completionData = AnnotationCompletionData;
                    break;
                case ShaderRegion.TechniqueOrPass:
                case ShaderRegion.TechniqueOrPass10:
                    completionData = TechniqueCompletionData;
                    break;
                case ShaderRegion.BlendState10:
                    completionData = BlendStateCompletionData;
                    break;
                case ShaderRegion.DepthStencilState10:
                    completionData = DepthStencilStateCompletionData;
                    break;
                case ShaderRegion.RasterizerState10:
                    completionData = RasterizerStateCompletionData;
                    break;
                case ShaderRegion.SamplerState:
                    completionData = SamplerStateCompletionData;
                    break;
                case ShaderRegion.SamplerState10:
                    completionData = SamplerState10CompletionData;
                    break;
                case ShaderRegion.StateBlock:
                    completionData = StateBlockCompletionData;
                    break;
                default:
                    completionData = null;
                    break;
            }

            // No data --> close window.
            if (completionData == null)
            {
                _completionWindow?.Close();
                return;
            }

            // Combine static completion data with guessed identifiers
            List<ICompletionData> entireCompletionData = new List<ICompletionData>();
            foreach (ICompletionData completionEntry in completionData)
                entireCompletionData.Add(completionEntry);
            foreach (NamedCompletionData collectedIdentifier in collectedIdentifiers)
                entireCompletionData.Add(collectedIdentifier);

            // Show completion window
            ShowCompletionWindow(MergeCompletionData(completionData, collectedIdentifiers), symbol, key);
        }


        /// <summary>
        /// Shows the completion window.
        /// </summary>
        /// <param name="completionData">The completion data.</param>
        /// <param name="symbol">The symbol (pre-selection in the completion window).</param>
        /// <param name="key">The key typed.</param>
        private void ShowCompletionWindow(ICompletionData[] completionData, string symbol, char key)
        {
            // If there is no completion data in this context, we close existing windows.
            if (completionData == null || completionData.Length == 0)
            {
                _completionWindow?.Close();
                return;
            }

            // Get the text used to filter the list box.
            string selectionText;
            if (string.IsNullOrEmpty(symbol))
                selectionText = (key != '\0') ? key.ToString() : null;
            else if (key != '\0')
                selectionText = symbol + key;
            else
                selectionText = symbol;

            // Filter and sort the completion data.
            var filteredData = completionData.Where(d => string.IsNullOrEmpty(selectionText)
                                                          || d.Text.Length > selectionText.Length && d.Text.StartsWith(selectionText, true, null))
                                              .OrderBy(d => d.Text)
                                              .ToArray();

            // If there is no completion data in this context, we close existing windows.
            if (filteredData.Length == 0)
            {
                _completionWindow?.Close();
                return;
            }

            // Close insight windows.
            _overloadInsightWindow?.Close();

            // Create new window or we only update the existing one.
            if (_completionWindow == null)
            {
                _completionWindow = new CompletionWindow(_textEditor.TextArea) { MaxHeight = 300 };
            }

            // Set new completion items.
            _completionWindow.CompletionList.CompletionData.Clear();
            foreach (var data in filteredData)
            {
                if (!(data is GuessCompletionData) || data.Text != selectionText)
                    _completionWindow.CompletionList.CompletionData.Add(data);
            }

            // Select current item.
            if (selectionText != null)
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

            // Delete instance when window is closed.
            _completionWindow.Closed += delegate { _completionWindow = null; };
        }
        #endregion


        //--------------------------------------------------------------
        #region Insight
        //--------------------------------------------------------------

        /// <summary>
        /// Requests the insight window.
        /// </summary>
        /// <param name="key">
        /// The currently pressed key which is not yet inserted in the document.
        /// </param>
        /// <remarks>
        /// This method should be called whenever the user types a new character.
        /// </remarks>
        private void RequestInsightWindow(char key)
        {
            // The insight window is shown when '(' or a ',' of a function signature is typed.
            if (key != '(' && key != ',')
                return;

            // Identify the region at the cursor position
            var document = _textEditor.Document;
            int caretOffset = _textEditor.TextArea.Caret.Offset;
            ShaderRegion region = _parser.IdentifyRegion(document, caretOffset);

            // Fetch lookup tables for the given region.
            NamedObjectCollection<NamedCompletionData>[] lookupTables = GetInsightLookupTables(region);
            if (lookupTables == null)
                return;

            // Find the name of the function.
            int offset = caretOffset;
            if (offset == 0)
                return;

            string identifier = null;
            if (key == '(')
            {
                // Code should look like this:
                //   "Function|"
                int startOfIdentifier = TextUtilities.FindStartOfIdentifier(document, offset - 1);
                if (startOfIdentifier >= 0)
                    identifier = document.GetText(startOfIdentifier, offset - startOfIdentifier);
            }
            else if (key == ',')
            {
                // Check whether we are inside a parameter list.
                //   "Function(param1, param2|"
                offset = TextUtilities.FindOpeningBracket(document, offset - 1, '(', ')');
                identifier = TextUtilities.GetIdentifierAt(document, offset - 1);
            }

            // Fetch the function description.
            FunctionCompletionData function = null;
            if (!string.IsNullOrEmpty(identifier))
                foreach (NamedObjectCollection<NamedCompletionData> lookupTable in lookupTables)
                    if (lookupTable.TryGet(identifier, out function))
                        break;

            ShowOverloadInsightWindow(function);
        }


        private void ShowOverloadInsightWindow(FunctionCompletionData function)
        {
            // Close all existing IntelliSense windows.
            _overloadInsightWindow?.Close();
            _completionWindow?.Close();

            // Abort if we have no info to show.
            if (function == null || function.Signatures == null || function.Signatures.Length == 0)
                return;

            // Create new OverloadInsightWindow with data.
            _overloadInsightWindow = new OverloadInsightWindow(_textEditor.TextArea);
            OverloadProvider provider = new OverloadProvider();
            for (int i = 0; i < function.Signatures.Length; i++)
            {
                provider.Overloads.Add(new OverloadDescription(
                  string.Format(CultureInfo.InvariantCulture, "{0}/{1}", i + 1, function.Signatures.Length),
                  function.Signatures[i],
                  function.Description));
            }
            _overloadInsightWindow.Provider = provider;

            // Auto-delete instance when window is closed.
            _overloadInsightWindow.Closed += delegate { _overloadInsightWindow = null; };

            // Show window.
            _overloadInsightWindow.Show();
        }
        #endregion


        //--------------------------------------------------------------
        #region ToolTips
        //--------------------------------------------------------------

        private void OnTextEditorMouseHover(object sender, MouseEventArgs eventArgs)
        {
            // Find offset under mouse cursor.
            var mousePos = eventArgs.GetPosition(_textEditor.TextArea.TextView);
            double verticalOffset = _textEditor.VerticalOffset;
            VisualLine visualLine = _textEditor.TextArea.TextView.GetVisualLineFromVisualTop(mousePos.Y + verticalOffset);
            if (visualLine == null)
                return;

            double horizontalOffset = _textEditor.HorizontalOffset;
            int visualColumn = visualLine.GetVisualColumn(mousePos + new Vector(horizontalOffset, verticalOffset));
            int relativeOffset = visualLine.GetRelativeOffset(visualColumn);
            int offset = visualLine.FirstDocumentLine.Offset + relativeOffset;

            // Get word at mouse cursor.
            var text = TextUtilities.GetIdentifierAt(_textEditor.Document, offset);

            // Get region at mouse cursor.
            ShaderRegion region = _parser.IdentifyRegion(_textEditor.Document, offset);

            NamedObjectCollection<NamedCompletionData>[] lookupTables = GetToolTipLookupTables(region);
            if (lookupTables != null)
            {
                // Look up symbol in lookup tables.
                NamedCompletionData info = null;
                foreach (NamedObjectCollection<NamedCompletionData> lookupTable in lookupTables)
                    if (lookupTable.TryGet<NamedCompletionData>(text, out info))
                        break;

                // Show tooltip if lookup was successful and description for the symbol is available.
                if (info != null && !string.IsNullOrEmpty(info.Description as string))
                {
                    // TODO: Use s better tooltip style.
                    _toolTip = new ToolTip
                    {
                        Content = info.Description,
                        Placement = PlacementMode.Mouse,
                        IsOpen = true,
                    };
                }
            }
        }


        private void OnTextEditorMouseHoverStopped(object sender, MouseEventArgs eventArgs)
        {
            // Close any opened tool-tip popups.
            if (_toolTip != null)
                _toolTip.IsOpen = false;
        }
        #endregion
    }
}
