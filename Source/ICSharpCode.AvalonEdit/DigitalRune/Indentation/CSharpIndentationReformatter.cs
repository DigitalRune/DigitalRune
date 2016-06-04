// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;


namespace ICSharpCode.AvalonEdit.Indentation
{
    internal sealed class IndentationSettings
    {
        public string IndentString = "\t";
        /// <summary>Leave empty lines empty.</summary>
        public bool LeaveEmptyLines = true;
    }


    internal sealed class CSharpIndentationReformatter
    {
        /// <summary>
        /// An indentation block. Tracks the state of the indentation.
        /// </summary>
        private struct Block
        {
            /// <summary>
            /// The indentation outside of the block.
            /// </summary>
            public string OuterIndent;

            /// <summary>
            /// The indentation inside the block.
            /// </summary>
            public string InnerIndent;

            /// <summary>
            /// The last word that was seen inside this block.
            /// Because parenthesis open a sub-block and thus don't change their parent's LastWord,
            /// this property can be used to identify the type of block statement (if, while, switch)
            /// at the position of the '{'.
            /// </summary>
            public string LastWord;

            /// <summary>
            /// The type of bracket that opened this block (, [ or {
            /// </summary>
            public char Bracket;

            /// <summary>
            /// Gets whether there's currently a line continuation going on inside this block.
            /// </summary>
            public bool Continuation;

            /// <summary>
            /// Gets whether there's currently a 'one-line-block' going on. 'one-line-blocks' occur
            /// with if statements that don't use '{}'. They are not represented by a Block instance on
            /// the stack, but are instead handled similar to line continuations.
            /// This property is an integer because there might be multiple nested one-line-blocks.
            /// As soon as there is a finished statement, OneLineBlock is reset to 0.
            /// </summary>
            public int OneLineBlock;

            /// <summary>
            /// The previous value of one-line-block before it was reset.
            /// Used to restore the indentation of 'else' to the correct level.
            /// </summary>
            public int PreviousOneLineBlock;

            public void ResetOneLineBlock()
            {
                PreviousOneLineBlock = OneLineBlock;
                OneLineBlock = 0;
            }

            /// <summary>
            /// Gets the line number where this block started.
            /// </summary>
            public int StartLine;

            public void Indent(string indentationString)
            {
                OuterIndent = InnerIndent;
                InnerIndent += indentationString;
                Continuation = false;
                ResetOneLineBlock();
                LastWord = "";
            }

            public override string ToString()
            {
                return string.Format(
                  CultureInfo.InvariantCulture,
                  "[Block StartLine={0}, LastWord='{1}', Continuation={2}, OneLineBlock={3}, PreviousOneLineBlock={4}]",
                  StartLine, LastWord, Continuation, OneLineBlock, PreviousOneLineBlock);
            }
        }


        // StringBuilders used locally in Step().
        private StringBuilder _indent;
        private StringBuilder _wordBuilder;

        // Block descriptions.
        private Stack<Block> _blocks;     // All blocks outside of the current block.
        private Block _block;             // The current block.

        // Flags indicating whether current line is part of a multi-line construct.
        private bool _inBlockComment;     // Multi-line block comment           /* comment */
        private bool _inVerbatimString;   // Verbatim string                    @"text"

        private int _lineCommentStart;


        public void Reformat(IDocumentAccessor doc, IndentationSettings settings)
        {
            Init();

            while (doc.MoveNext())
                Step(doc, settings);
        }


        private void Init()
        {
            _indent = new StringBuilder();
            _wordBuilder = new StringBuilder();

            _blocks = new Stack<Block>();
            _block = new Block();
            _block.InnerIndent = "";
            _block.OuterIndent = "";
            _block.Bracket = '{';
            _block.Continuation = false;
            _block.LastWord = "";
            _block.OneLineBlock = 0;
            _block.PreviousOneLineBlock = 0;
            _block.StartLine = 0;

            _inBlockComment = false;
            _inVerbatimString = false;

            _lineCommentStart = 0;
        }


        private void Step(IDocumentAccessor doc, IndentationSettings settings)
        {
            string line = doc.Text;
            if (settings.LeaveEmptyLines && line.Length == 0)
            {
                // Leave empty lines empty.
                _lineCommentStart = 0;
                return;
            }

            line = line.TrimStart();
            if (line.Length == 0)
            {
                _lineCommentStart = 0;
                if (_inBlockComment || _inVerbatimString)
                {
                    // Examples:
                    //
                    //  /* comment
                    //                      <-- HERE
                    //     comment
                    //  */
                    //
                    //  string s = @"text
                    //                      <-- HERE
                    //  text";
                    return;
                }

                _indent.Clear();
                _indent.Append(_block.InnerIndent);
                _indent.Append(Repeat(settings.IndentString, _block.OneLineBlock));
                if (_block.Continuation)
                {
                    // Example:
                    //
                    //   method(
                    //                      <-- HERE
                    //
                    _indent.Append(settings.IndentString);
                }

                // Apply indentation to current line.
                if (doc.Text.Length != _indent.Length) // Check length first to avoid unnecessary ToString().
                {
                    string text = _indent.ToString();
                    // ReSharper disable once RedundantCheckBeforeAssignment
                    if (doc.Text != text)
                        doc.Text = text;
                }

                return;
            }

            if (TrimEnd(doc))
                line = doc.Text.TrimStart();

            // oldBlock is the block at the start of the line.
            // _block is the block at the current character.
            Block oldBlock = _block;

            bool startInComment = _inBlockComment;
            bool startInString = _inVerbatimString;
            bool inLineComment = false;
            int lastLineCommentStart = _lineCommentStart;
            _lineCommentStart = 0;
            bool inString = _inVerbatimString;
            bool inChar = false;
            bool isEscapeChar = false;

            char lastRealChar = '\n'; // The last non-comment character.

            #region ----- Parse line character by character. -----

            char previousChar;
            char currentChar = ' ';
            char nextChar = line[0];
            for (int i = 0; i < line.Length; i++)
            {
                if (inLineComment)
                {
                    // Cancel parsing current line.
                    break;
                }

                previousChar = currentChar;
                currentChar = nextChar;
                if (i + 1 < line.Length)
                    nextChar = line[i + 1];
                else
                    nextChar = '\n';

                // Skip escape characters.
                if (isEscapeChar)
                {
                    // Example:
                    //
                    //   char c = '\t';
                    //              ^
                    //             HERE
                    //
                    isEscapeChar = false;
                    continue;
                }

                // ----- Check for comment, preprocessor directive, string, character.
                switch (currentChar)
                {
                    // Check for preprocessor directive.
                    case '#':
                        if (!(_inBlockComment || inString || inChar))
                        {
                            inLineComment = true;
                        }
                        break;

                    // Check for comment.
                    case '/':
                        if (_inBlockComment && previousChar == '*')
                            _inBlockComment = false;

                        if (!inString && !inChar)
                        {
                            if (!_inBlockComment && nextChar == '/')
                            {
                                inLineComment = true;
                                _lineCommentStart = i;
                            }
                            if (!inLineComment && nextChar == '*')
                            {
                                _inBlockComment = true;
                            }
                        }
                        break;

                    // Check for string.
                    case '"':
                        if (!(_inBlockComment || inLineComment || inChar))
                        {
                            inString = !inString;
                            if (!inString && _inVerbatimString)
                            {
                                if (nextChar == '"')
                                {
                                    // Example:
                                    //
                                    //   string s = @"Printing ""double quotation"" ...";
                                    //                         ^
                                    //                        HERE
                                    //
                                    isEscapeChar = true; // Skip escaped quote.
                                    inString = true;
                                }
                                else
                                {
                                    // Example:
                                    //
                                    //   string s = @"Printing ""double quotation"" ...";
                                    //                                                 ^
                                    //                                                HERE
                                    //
                                    _inVerbatimString = false;
                                }
                            }
                            else if (inString)
                            {
                                // Example:
                                //
                                //   string s = "Text";
                                //              ^
                                //             HERE
                                //
                                //   string s = @"Printing ""double quotation"" ...";
                                //               ^
                                //              HERE
                                //
                                _inVerbatimString = (previousChar == '@');
                            }
                            else
                            {
                                // Example:
                                //
                                //   string s = "Text";
                                //                   ^
                                //                  HERE
                                //
                                _inVerbatimString = false;
                            }
                        }
                        break;

                    // Check for character.
                    case '\'':
                        if (!(_inBlockComment || inLineComment || inString))
                        {
                            inChar = !inChar;
                        }
                        break;

                    // Check for escape character.
                    case '\\':
                        if ((inString && !_inVerbatimString) || inChar)
                            isEscapeChar = true; // Skip next character at start of loop.
                        break;
                }

                Debug.Assert(!_inVerbatimString || _inVerbatimString && inString, "When _inVerbatimString is set, inString needs to be set.");

                // At this point the following variables are set:
                //   _inDirective, _inBlockComment,
                //   inLineComment, lineCommentStart,
                //   _inVerbatimString, _inString,
                //   inChar, _isEscapeChar

                if (_inBlockComment || inLineComment || inString || inChar)
                {
                    // Store last word before directive/comment/string/char and continue
                    // with next character.
                    if (_wordBuilder.Length > 0)
                    {
                        _block.LastWord = _wordBuilder.ToString();
                        _wordBuilder.Clear();
                    }

                    continue;
                }

                if (!Char.IsWhiteSpace(currentChar) && currentChar != '[' && currentChar != '/')
                {
                    if (_block.Bracket == '{')
                    {
                        // The current line potentially contains a statement. If the statement
                        // is not completed in this line, it the next line is a continuation.
                        _block.Continuation = true;
                    }
                }

                if (Char.IsLetterOrDigit(currentChar))
                {
                    _wordBuilder.Append(currentChar);
                }
                else
                {
                    if (_wordBuilder.Length > 0)
                    {
                        _block.LastWord = _wordBuilder.ToString();
                        _wordBuilder.Clear();
                    }
                }

                // ----- Push/pop the blocks.
                switch (currentChar)
                {
                    case '{':
                        _block.ResetOneLineBlock();
                        _blocks.Push(_block);
                        _block.StartLine = doc.LineNumber;
                        if (_block.LastWord == "switch")
                        {
                            _block.Indent(settings.IndentString + settings.IndentString);
                            /* oldBlock refers to the previous line, not the previous block
                             * The block we want is not available anymore because it was never pushed.
                             * } else if (oldBlock.OneLineBlock) {
                            // Inside a one-line-block is another statement
                            // with a full block: indent the inner full block
                            // by one additional level
                            block.Indent(settings, settings.IndentString + settings.IndentString);
                            block.OuterIndent += settings.IndentString;
                            // Indent current line if it starts with the '{' character
                            if (i == 0) {
                              oldBlock.InnerIndent += settings.IndentString;
                            }*/
                        }
                        else
                        {
                            if (i == line.Length - 1)
                            {
                                // Example:
                                //
                                //   if (condition) {
                                //                  ^
                                //                 HERE
                                //
                                _block.Indent(settings.IndentString);
                            }
                            else
                            {
                                // Example:
                                //
                                //   char[] array = { value, value,
                                //                  ^
                                //                 HERE
                                //
                                // Align subsequent lines with first value.
                                _block.Indent(new string(' ', i + 2));
                            }
                        }
                        _block.Bracket = '{';
                        break;

                    case '}':
                        while (_block.Bracket != '{')
                        {
                            if (_blocks.Count == 0)
                                break;
                            _block = _blocks.Pop();
                        }
                        if (_blocks.Count == 0)
                            break;
                        _block = _blocks.Pop();
                        _block.Continuation = false;
                        _block.ResetOneLineBlock();
                        break;

                    case '(':
                    case '[':
                        _blocks.Push(_block);
                        if (_block.StartLine == doc.LineNumber)
                        {
                            // Example:
                            //
                            //   ???
                            //
                            _block.InnerIndent = _block.OuterIndent;
                        }
                        else
                        {
                            _block.StartLine = doc.LineNumber;
                        }

                        _indent.Clear();

                        // New block may be part of line continuation.
                        if (oldBlock.Continuation)
                            _indent.Append(settings.IndentString);

                        // New block may be part to one-line-block.
                        _indent.Append(Repeat(settings.IndentString, oldBlock.OneLineBlock));

                        if (i == line.Length - 1)
                        {
                            // Example:
                            //
                            //   method(
                            //         ^
                            //        HERE
                            //
                            _indent.Append(settings.IndentString);
                        }
                        else
                        {
                            // Example:
                            //
                            //   method(param,
                            //         ^
                            //        HERE
                            //
                            // Align subsequent lines with first parameter.
                            _indent.Append(' ', i + 1);
                        }

                        _block.Indent(_indent.ToString());
                        _block.Bracket = currentChar;
                        break;

                    case ')':
                        if (_blocks.Count == 0)
                            break;
                        if (_block.Bracket == '(')
                        {
                            // Example:
                            //
                            //   method(param);
                            //               ^
                            //              HERE
                            //
                            _block = _blocks.Pop();

                            if (IsSingleStatementKeyword(_block.LastWord))
                            {
                                // Example:
                                //
                                //   if (condition)
                                //                ^
                                //               HERE
                                _block.Continuation = false;
                            }
                        }
                        break;

                    case ']':
                        if (_blocks.Count == 0)
                            break;
                        if (_block.Bracket == '[')
                        {
                            // Example:
                            //
                            //   array[index]
                            //              ^
                            //             HERE
                            _block = _blocks.Pop();
                        }
                        break;

                    case ';':
                    case ',':
                        // Example:
                        //
                        //   statement;
                        //            ^
                        //           HERE
                        _block.Continuation = false;
                        _block.ResetOneLineBlock();
                        break;
                    case ':':
                        if (_block.LastWord == "case"
                            || line.StartsWith("case ", StringComparison.Ordinal)
                            || line.StartsWith(_block.LastWord + ":", StringComparison.Ordinal))
                        {
                            // Examples:
                            //
                            //   case 1:
                            //         ^
                            //        HERE
                            //
                            //   label:
                            //        ^
                            //       HERE
                            _block.Continuation = false;
                            _block.ResetOneLineBlock();
                        }
                        break;
                }

                if (!Char.IsWhiteSpace(currentChar))
                {
                    // Register this character as last non-comment characater.
                    lastRealChar = currentChar;
                }
            }
            #endregion

            // At this point the line is parsed.

            if (_wordBuilder.Length > 0)
            {
                _block.LastWord = _wordBuilder.ToString();
                _wordBuilder.Clear();
            }

            if (startInComment && line[0] != '*')
                return;
            if (startInString)
                return;
            if (doc.Text.StartsWith("//\t", StringComparison.Ordinal) || doc.Text == "//")
                return;

            #region ----- Build indentation string. -----

            // Note: Line continuations, one-line-blocks, and multiline block comments
            // are not handled here. They are handled explicitly when the indentation
            // is applied.

            _indent.Clear();
            if (line[0] == '}')
            {
                // Example:
                //
                //   {
                //     statement;
                //     statement;
                //   }                    <-- HERE
                //
                _indent.Append(oldBlock.OuterIndent);
                oldBlock.ResetOneLineBlock();
                oldBlock.Continuation = false;
            }
            else
            {
                // Example:
                //
                //   {
                //     statement;
                //     statement;         <-- HERE
                //   }
                //
                _indent.Append(oldBlock.InnerIndent);
            }

            if (_indent.Length > 0 && oldBlock.Bracket == '(' && line[0] == ')')
            {
                // Example:
                //
                //   Method(param,
                //          param
                //         );             <-- HERE
                //
                _indent.Remove(_indent.Length - 1, 1);
            }
            else if (_indent.Length > 0 && oldBlock.Bracket == '[' && line[0] == ']')
            {
                // Example:
                //
                //   array[index0,
                //         index1
                //        ];             <-- HERE
                //
                _indent.Remove(_indent.Length - 1, 1);
            }

            if (line[0] == ':')
            {
                // Example:
                //
                //   ???
                //
                oldBlock.Continuation = true;
            }
            else if (lastRealChar == ':' && _indent.Length >= settings.IndentString.Length)
            {
                if (_block.LastWord == "case"
                    || line.StartsWith("case ", StringComparison.Ordinal)
                    || line.StartsWith(_block.LastWord + ":", StringComparison.Ordinal))
                {
                    // Examples:
                    //
                    //   switch (variable)
                    //   {
                    //     case 1:          <-- HERE
                    //       statement;
                    //   }
                    //
                    //   label:             <-- HERE
                    //     statement;
                    _indent.Remove(_indent.Length - settings.IndentString.Length, settings.IndentString.Length);
                }
            }
            else if (lastRealChar == ')')
            {
                if (IsSingleStatementKeyword(_block.LastWord))
                {
                    // Example:
                    //
                    //   if (condition)     <--- HERE
                    // 
                    _block.OneLineBlock++;
                }
            }
            else if (lastRealChar == 'e' && _block.LastWord == "else")
            {
                // Example:
                //
                //   if (condition)
                //     statement;
                //   else                 <-- HERE
                //     statement;
                //
                // PreviousOneLineBlock stores the indentation level used by the previous
                // if-branch. Use the same indentation on the following else-branch.
                _block.OneLineBlock = Math.Max(1, _block.PreviousOneLineBlock);
                _block.Continuation = false;
                oldBlock.OneLineBlock = _block.OneLineBlock - 1;
            }
            #endregion


            #region ----- Apply indentation. -----

            if (doc.IsReadOnly)
            {
                // ----- Read-only line. (Not in selected text region.)
                // We can't change the current line, but we should accept the existing
                // indentation if possible (=if the current statement is not a multiline
                // statement).
                if (!oldBlock.Continuation
                    && oldBlock.OneLineBlock == 0
                    && oldBlock.StartLine == _block.StartLine
                    && _block.StartLine < doc.LineNumber
                    && lastRealChar != ':')
                {
                    // Use indent StringBuilder to get the indentation of the current line.
                    _indent.Clear();
                    line = doc.Text; // get untrimmed line
                    for (int i = 0; i < line.Length; ++i)
                    {
                        if (!Char.IsWhiteSpace(line[i]))
                            break;
                        _indent.Append(line[i]);
                    }

                    // /* */ multiline block comments have an extra space - do not count it
                    // for the block's indentation. (The extra space is applied explicitly below.)
                    if (startInComment && _indent.Length > 0 && _indent[_indent.Length - 1] == ' ')
                    {
                        _indent.Length -= 1;
                    }

                    _block.InnerIndent = _indent.ToString();
                }
            }
            else
            {
                // ----- Reformat current line.
                if (line[0] == '#')
                {
                    // Do not indent preprocessor directives.
                    _indent.Clear();
                }
                else if (lastLineCommentStart > 0
                         && line.StartsWith("//", StringComparison.Ordinal)
                         && (line.Length <= 2 || char.IsWhiteSpace(line[2]))) // Ignore commented code such as "//statement;".
                {
                    // Special treatment to align dangling comments.
                    // Example:
                    //
                    //   statement;  // comment
                    //               // comment            <-- HERE
                    //               // comment
                    _indent.Append(' ', lastLineCommentStart);
                    _lineCommentStart = lastLineCommentStart;
                }
                else if (line[0] != '{')
                {
                    // Handle line continuation.
                    if (line[0] != ')' && oldBlock.Continuation && oldBlock.Bracket == '{')
                    {
                        // Variant #1: Reformat line. (Overrides user-defined indentation.)
                        //_indent.Append(settings.IndentString);

                        // Variant #2: Ignore line. (Keep any user-defined indentation.)
                        return;
                    }

                    // Handle one-line-blocks.
                    _indent.Append(Repeat(settings.IndentString, oldBlock.OneLineBlock));
                }

                // Handle multiline block comments.
                if (startInComment)
                {
                    Debug.Assert(line[0] == '*', "Other cases should have been handled above.");

                    // This is a multiline block comment.
                    // Example:
                    //
                    //   /* comment
                    //    * comment         <-- HERE
                    //
                    // Add ' ' to align the '*' characters.
                    _indent.Append(' ');
                }

                // Check whether line already has correct indentation to avoid unnecessary change.
                if (_indent.Length != (doc.Text.Length - line.Length)
                    || Char.IsWhiteSpace(doc.Text[_indent.Length])
                    || !doc.Text.StartsWith(_indent.ToString(), StringComparison.Ordinal))
                {
                    doc.Text = _indent.Append(line).ToString();
                }
            }
            #endregion
        }


        private static string Repeat(string text, int count)
        {
            if (count == 0)
                return string.Empty;
            if (count == 1)
                return text;
            var b = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
                b.Append(text);
            return b.ToString();
        }


        private static bool IsSingleStatementKeyword(string keyword)
        {
            switch (keyword)
            {
                case "if":
                case "for":
                case "while":
                case "do":
                case "foreach":
                case "using":
                case "lock":
                    return true;
                default:
                    return false;
            }
        }


        private static bool TrimEnd(IDocumentAccessor doc)
        {
            string line = doc.Text;
            if (!char.IsWhiteSpace(line[line.Length - 1]))
                return false;

            // one space after an empty comment is allowed
            if (line.EndsWith("// ", StringComparison.Ordinal) || line.EndsWith("* ", StringComparison.Ordinal))
                return false;

            doc.Text = line.TrimEnd();
            return true;
        }
    }
}
