using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;


namespace ICSharpCode.AvalonEdit.Formatting
{
    /// <summary>
    /// Formatting strategy for C#.
    /// </summary>
    public class CSharpFormattingStrategy : IFormattingStrategy
    {
        /// <inheritdoc/>
        public void FormatLine(TextArea textArea, char charTyped)
        {
            FormatLineInternal(textArea, textArea.Caret.Line, textArea.Caret.Offset, charTyped);
        }


        private void FormatLineInternal(TextArea textArea, int lineNr, int cursorOffset, char ch)
        {
            DocumentLine curLine = textArea.Document.GetLineByNumber(lineNr);
            DocumentLine lineAbove = lineNr > 1 ? textArea.Document.GetLineByNumber(lineNr - 1) : null;
            string terminator = TextUtilities.GetLineTerminator(textArea.Document, lineNr);

            string curLineText;
            // /// local string for curLine segment
            //if (ch == '/')
            //{
            //  curLineText = curLine.Text;
            //  string lineAboveText = lineAbove == null ? "" : lineAbove.Text;
            //  if (curLineText != null && curLineText.EndsWith("///") && (lineAboveText == null || !lineAboveText.Trim().StartsWith("///")))
            //  {
            //    string indentation = DocumentUtilitites.GetWhitespaceAfter(textArea.Document, curLine.Offset);
            //    object member = GetMemberAfter(textArea, lineNr);
            //    if (member != null)
            //    {
            //      StringBuilder sb = new StringBuilder();
            //      sb.Append(" <summary>");
            //      sb.Append(terminator);
            //      sb.Append(indentation);
            //      sb.Append("/// ");
            //      sb.Append(terminator);
            //      sb.Append(indentation);
            //      sb.Append("/// </summary>");

            //      if (member is IMethod)
            //      {
            //        IMethod method = (IMethod)member;
            //        if (method.Parameters != null && method.Parameters.Count > 0)
            //        {
            //          for (int i = 0; i < method.Parameters.Count; ++i)
            //          {
            //            sb.Append(terminator);
            //            sb.Append(indentation);
            //            sb.Append("/// <param name=\"");
            //            sb.Append(method.Parameters[i].Name);
            //            sb.Append("\"></param>");
            //          }
            //        }
            //        if (method.ReturnType != null && !method.IsConstructor && method.ReturnType.FullyQualifiedName != "System.Void")
            //        {
            //          sb.Append(terminator);
            //          sb.Append(indentation);
            //          sb.Append("/// <returns></returns>");
            //        }
            //      }
            //      textArea.Document.Insert(cursorOffset, sb.ToString());

            //      textArea.Caret.Offset = cursorOffset + indentation.Length + "/// ".Length + " <summary>".Length + terminator.Length;
            //    }
            //  }
            //  return;
            //}

            if (ch != '\n' && ch != '>')
            {
                if (IsInsideStringOrComment(textArea, curLine, cursorOffset))
                {
                    return;
                }
            }

            switch (ch)
            {
                case '>':
                    if (IsInsideDocumentationComment(textArea, curLine, cursorOffset))
                    {
                        curLineText = textArea.Document.GetText(curLine);
                        int column = cursorOffset - curLine.Offset;
                        int index = Math.Min(column - 1, curLineText.Length - 1);

                        while (index >= 0 && curLineText[index] != '<')
                        {
                            --index;
                            if (curLineText[index] == '/')
                                return; // the tag was an end tag or already
                        }

                        if (index > 0)
                        {
                            StringBuilder commentBuilder = new StringBuilder("");
                            for (int i = index; i < curLineText.Length && i < column && !Char.IsWhiteSpace(curLineText[i]); ++i)
                            {
                                commentBuilder.Append(curLineText[i]);
                            }
                            string tag = commentBuilder.ToString().Trim();
                            if (!tag.EndsWith(">"))
                            {
                                tag += ">";
                            }
                            if (!tag.StartsWith("/"))
                            {
                                textArea.Document.Insert(cursorOffset, "</" + tag.Substring(1));
                            }
                        }
                    }
                    break;
                case ':':
                case ')':
                case ']':
                case '}':
                case '{':
                    if (textArea.IndentationStrategy != null)
                        textArea.IndentationStrategy.IndentLine(textArea, curLine);
                    break;
                case '\n':
                    string lineAboveText = lineAbove == null ? "" : textArea.Document.GetText(lineAbove);
                    //// curLine might have some text which should be added to indentation
                    curLineText = textArea.Document.GetText(curLine);

                    if (lineAbove != null && textArea.Document.GetText(lineAbove).Trim().StartsWith("#region")
                        && NeedEndregion(textArea.Document))
                    {
                        textArea.Document.Insert(cursorOffset, "#endregion");
                        return;
                    }
                    IHighlighter highlighter = textArea.GetService(typeof(IHighlighter)) as IHighlighter;
                    bool isInMultilineComment = false;
                    bool isInMultilineString = false;
                    if (highlighter != null && lineAbove != null)
                    {
                        var spanStack = highlighter.GetSpanColorNamesFromLineStart(lineNr);
                        isInMultilineComment = spanStack.Contains(HighlightingKnownNames.Comment);
                        isInMultilineString = spanStack.Contains(HighlightingKnownNames.String);
                    }
                    bool isInNormalCode = !(isInMultilineComment || isInMultilineString);

                    if (lineAbove != null && isInMultilineComment)
                    {
                        string lineAboveTextTrimmed = lineAboveText.TrimStart();
                        if (lineAboveTextTrimmed.StartsWith("/*", StringComparison.Ordinal))
                        {
                            textArea.Document.Insert(cursorOffset, " * ");
                            return;
                        }

                        if (lineAboveTextTrimmed.StartsWith("*", StringComparison.Ordinal))
                        {
                            textArea.Document.Insert(cursorOffset, "* ");
                            return;
                        }
                    }

                    if (lineAbove != null && isInNormalCode)
                    {
                        DocumentLine nextLine = lineNr + 1 <= textArea.Document.LineCount ? textArea.Document.GetLineByNumber(lineNr + 1) : null;
                        string nextLineText = (nextLine != null) ? textArea.Document.GetText(nextLine) : "";

                        int indexAbove = lineAboveText.IndexOf("///");
                        int indexNext = nextLineText.IndexOf("///");
                        if (indexAbove > 0 && (indexNext != -1 || indexAbove + 4 < lineAbove.Length))
                        {
                            textArea.Document.Insert(cursorOffset, "/// ");
                            return;
                        }

                        if (IsInNonVerbatimString(lineAboveText, curLineText))
                        {
                            textArea.Document.Insert(cursorOffset, "\"");
                            textArea.Document.Insert(lineAbove.Offset + lineAbove.Length,
                                                     "\" +");
                        }
                    }
                    if (/*textArea.Options.AutoInsertBlockEnd &&*/ lineAbove != null && isInNormalCode)
                    {
                        string oldLineText = textArea.Document.GetText(lineAbove);
                        if (oldLineText.EndsWith("{"))
                        {
                            if (NeedCurlyBracket(textArea.Document.Text))
                            {
                                int insertionPoint = curLine.Offset + curLine.Length;
                                textArea.Document.Insert(insertionPoint, terminator + "}");
                                if (textArea.IndentationStrategy != null)
                                    textArea.IndentationStrategy.IndentLine(textArea, textArea.Document.GetLineByNumber(lineNr + 1));
                                textArea.Caret.Offset = insertionPoint;
                            }
                        }
                    }
                    return;
            }
        }


        private bool IsInsideStringOrComment(TextArea textArea, DocumentLine curLine, int cursorOffset)
        {
            // scan cur line if it is inside a string or single line comment (//)
            bool insideString = false;
            char stringstart = ' ';
            bool verbatim = false; // true if the current string is verbatim (@-string)
            char c = ' ';
            char lastchar;

            for (int i = curLine.Offset; i < cursorOffset; ++i)
            {
                lastchar = c;
                c = textArea.Document.GetCharAt(i);
                if (insideString)
                {
                    if (c == stringstart)
                    {
                        if (verbatim && i + 1 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '"')
                        {
                            ++i; // skip escaped character
                        }
                        else
                        {
                            insideString = false;
                        }
                    }
                    else if (c == '\\' && !verbatim)
                    {
                        ++i; // skip escaped character
                    }
                }
                else if (c == '/' && i + 1 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '/')
                {
                    return true;
                }
                else if (c == '"' || c == '\'')
                {
                    stringstart = c;
                    insideString = true;
                    verbatim = (c == '"') && (lastchar == '@');
                }
            }

            return insideString;
        }


        private bool IsInsideDocumentationComment(TextArea textArea, DocumentLine curLine, int cursorOffset)
        {
            for (int i = curLine.Offset; i < cursorOffset; ++i)
            {
                char ch = textArea.Document.GetCharAt(i);
                if (ch == '"')
                {
                    // parsing strings correctly is too complicated (see above),
                    // but I don't now any case where a doc comment is after a string...
                    return false;
                }
                if (ch == '/' && i + 2 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '/' && textArea.Document.GetCharAt(i + 2) == '/')
                {
                    return true;
                }
            }
            return false;
        }


        private bool NeedEndregion(TextDocument document)
        {
            int regions = 0;
            int endregions = 0;
            for (int i = 1; i <= document.LineCount; i++)
            {
                string text = document.GetLineAsString(i).Trim();
                if (text.StartsWith("#region"))
                {
                    ++regions;
                }
                else if (text.StartsWith("#endregion"))
                {
                    ++endregions;
                }
            }
            return regions > endregions;
        }


        /// <summary>
        /// Checks if the cursor is inside a non-verbatim string.
        /// This method is used to check if a line break was inserted in a string.
        /// The text editor has already broken the line for us, so we just need to check
        /// the two lines.
        /// </summary>
        /// <param name="start">The part before the line break</param>
        /// <param name="end">The part after the line break</param>
        /// <returns>
        /// True, when the line break was inside a non-verbatim-string, so when
        /// start does not contain a comment, but a non-even number of ", and
        /// end contains a non-even number of " before the first comment.
        /// </returns>
        private bool IsInNonVerbatimString(string start, string end)
        {
            bool inString = false;
            bool inChar = false;
            for (int i = 0; i < start.Length; ++i)
            {
                char c = start[i];
                if (c == '"' && !inChar)
                {
                    if (!inString && i > 0 && start[i - 1] == '@')
                        return false; // no string line break for verbatim strings
                    inString = !inString;
                }
                else if (c == '\'' && !inString)
                {
                    inChar = !inChar;
                }
                if (!inString && i > 0 && start[i - 1] == '/' && (c == '/' || c == '*'))
                    return false;
                if (inString && start[i] == '\\')
                    ++i;
            }
            if (!inString) return false;
            // we are possibly in a string, or a multiline string has just ended here
            // check if the closing double quote is in end
            for (int i = 0; i < end.Length; ++i)
            {
                char c = end[i];
                if (c == '"' && !inChar)
                {
                    if (!inString && i > 0 && end[i - 1] == '@')
                        break; // no string line break for verbatim strings
                    inString = !inString;
                }
                else if (c == '\'' && !inString)
                {
                    inChar = !inChar;
                }
                if (!inString && i > 0 && end[i - 1] == '/' && (c == '/' || c == '*'))
                    break;
                if (inString && end[i] == '\\')
                    ++i;
            }
            // return true if the string was closed properly
            return !inString;
        }


        private bool NeedCurlyBracket(string text)
        {
            int curlyCounter = 0;

            bool inString = false;
            bool inChar = false;
            bool verbatim = false;

            bool lineComment = false;
            bool blockComment = false;

            for (int i = 0; i < text.Length; ++i)
            {
                switch (text[i])
                {
                    case '\r':
                    case '\n':
                        lineComment = false;
                        inChar = false;
                        if (!verbatim)
                            inString = false;
                        break;
                    case '/':
                        if (blockComment)
                        {
                            Debug.Assert(i > 0);
                            if (text[i - 1] == '*')
                            {
                                blockComment = false;
                            }
                        }
                        if (!inString && !inChar && i + 1 < text.Length)
                        {
                            if (!blockComment && text[i + 1] == '/')
                            {
                                lineComment = true;
                            }
                            if (!lineComment && text[i + 1] == '*')
                            {
                                blockComment = true;
                            }
                        }
                        break;
                    case '"':
                        if (!(inChar || lineComment || blockComment))
                        {
                            if (inString && verbatim)
                            {
                                if (i + 1 < text.Length && text[i + 1] == '"')
                                {
                                    ++i; // skip escaped quote
                                    inString = false; // let the string go on
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && i > 0 && text[i - 1] == '@')
                            {
                                verbatim = true;
                            }
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!(inString || lineComment || blockComment))
                        {
                            inChar = !inChar;
                        }
                        break;
                    case '{':
                        if (!(inString || inChar || lineComment || blockComment))
                        {
                            ++curlyCounter;
                        }
                        break;
                    case '}':
                        if (!(inString || inChar || lineComment || blockComment))
                        {
                            --curlyCounter;
                        }
                        break;
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++i; // skip next character
                        break;
                }
            }

            return curlyCounter > 0;
        }
    }
}
