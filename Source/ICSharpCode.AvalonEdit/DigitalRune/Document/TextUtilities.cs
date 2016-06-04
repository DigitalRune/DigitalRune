using System;
using System.Collections;
using System.Diagnostics;


namespace ICSharpCode.AvalonEdit.Document
{
    static partial class TextUtilities
    {
        /// <summary>
        /// Gets the line of the document as <see cref="string"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>The line as <see cref="string"/>.</returns>
        internal static string GetLineAsString(this TextDocument document, int lineNumber)
        {
            var line = document.GetLineByNumber(lineNumber);
            return document.GetText(line);
        }


        /// <summary>
        /// Gets the line terminator for the document around the specified line number.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>The line terminator.</returns>
        internal static string GetLineTerminator(TextDocument document, int lineNumber)
        {
            DocumentLine line = document.GetLineByNumber(lineNumber);
            if (line.DelimiterLength == 0)
            {
                // at the end of the document, there's no line delimiter, so use the delimiter
                // from the previous line
                if (lineNumber == 1)
                    return Environment.NewLine;

                line = document.GetLineByNumber(lineNumber - 1);
            }
            return document.GetText(line.Offset + line.Length, line.DelimiterLength);
        }


        /// <summary>
        /// Gets the type of code.
        /// </summary>
        internal enum CodeType
        {
            /// <summary>
            /// Code.
            /// </summary>
            Code,
            /// <summary>
            /// A comment.
            /// </summary>
            Comment,
            /// <summary>
            /// A string.
            /// </summary>
            String,
        }


        /// <summary>
        /// Gets the type of code at offset. (Block comments and multiline strings are not supported.)
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="linestart">The start offset of the line.</param>
        /// <param name="offset">The current offset.</param>
        /// <returns>The type of code.</returns>
        private static CodeType GetStartType(ITextSource document, int linestart, int offset)
        {
            bool inString = false;
            bool inChar = false;
            bool verbatim = false;
            for (int i = linestart; i < offset; i++)
            {
                switch (document.GetCharAt(i))
                {
                    case '/':
                        if (!inString && !inChar && i + 1 < document.TextLength)
                        {
                            if (document.GetCharAt(i + 1) == '/')
                            {
                                return CodeType.Comment;
                            }
                        }
                        break;
                    case '"':
                        if (!inChar)
                        {
                            if (inString && verbatim)
                            {
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i; // skip escaped quote
                                    inString = false; // let the string go on
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && i > 0 && document.GetCharAt(i - 1) == '@')
                            {
                                verbatim = true;
                            }
                            inString = !inString;
                        }
                        break;
                    case '\'':
                        if (!inString) inChar = !inChar;
                        break;
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++i; // skip next character
                        break;
                }
            }
            return (inString || inChar) ? CodeType.String : CodeType.Code;
        }


        /// <summary>
        /// Determines whether a line of a document is empty (no characters or whitespace).
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>
        /// <see langword="true"/> if line is empty of filled with whitespace; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        internal static bool IsEmptyLine(TextDocument document, int lineNumber)
        {
            return IsEmptyLine(document, document.GetLineByNumber(lineNumber));
        }


        /// <summary>
        /// Determines whether a line of a document is empty (no characters or whitespace).
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="line">The line.</param>
        /// <returns>
        /// <see langword="true"/> if line is empty of filled with whitespace; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        internal static bool IsEmptyLine(TextDocument document, DocumentLine line)
        {
            int lineOffset = line.Offset;
            int startOffset = lineOffset;
            int endOffset = lineOffset + line.Length;
            for (int i = startOffset; i < endOffset; ++i)
            {
                char ch = document.GetCharAt(i);
                if (!char.IsWhiteSpace(ch))
                    return false;
            }
            return true;
        }


        /// <summary>
        /// Gets the offset of the first non-whitespace character.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset where to start the search.</param>
        /// <returns>
        /// The offset of the first non-whitespace at or after <paramref name="offset"/>.
        /// <see cref="TextDocument.TextLength"/> is returned if no non-whitespace is found. (Line
        /// breaks do not count as whitespace.)
        /// </returns>
        internal static int FindFirstNonWhitespace(TextDocument document, int offset)
        {
            while (offset < document.TextLength && char.IsWhiteSpace(document.GetCharAt(offset)))
                ++offset;

            return offset;
        }


        /// <summary>
        /// Gets the offset of the first non-whitespace character.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset where to start the search.</param>
        /// <param name="searchBackwards">
        /// If set to <see langword="true"/>, the document is searched in backwards direction (=
        /// from <paramref name="offset"/> to start of text document.
        /// </param>
        /// <returns>
        /// <para>
        /// If <paramref name="searchBackwards"/> is <see langword="false"/>:
        /// The offset of the first non-whitespace at or after <paramref name="offset" />.
        /// <see cref="TextDocument.TextLength" /> is returned if no non-whitespace is found. (Line
        /// breaks do not count as whitespace.)
        /// </para>
        /// <para>
        /// If <paramref name="searchBackwards"/> is <see langword="true"/>:
        /// The offset of the first non-whitespace at or before <paramref name="offset" />.
        /// 0 is returned if no non-whitespace is found. (Line breaks do not count as whitespace.)
        /// </para>
        /// </returns>
        internal static int FindFirstNonWhitespace(TextDocument document, int offset, bool searchBackwards)
        {
            if (searchBackwards)
            {
                while (offset > 0 && char.IsWhiteSpace(document.GetCharAt(offset)))
                    --offset;
            }
            else
            {
                while (offset < document.TextLength && char.IsWhiteSpace(document.GetCharAt(offset)))
                    ++offset;
            }

            return offset;
        }


        /// <summary>
        /// Finds the offset of the opening bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (before the closing bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the opening bracket or -1 if no matching bracket was found.
        /// </returns>
        public static int FindOpeningBracket(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            return SearchBracketBackward(document, offset, openBracket, closingBracket);
        }


        /// <summary>
        /// Finds the offset of the closing bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (after the opening bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the closing bracket or -1 if no matching bracket was found.
        /// </returns>
        public static int FindClosingBracket(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            return SearchBracketForward(document, offset, openBracket, closingBracket);
        }


        private static bool IsIdentifierPart(char c)
        {
            return GetCharacterClass(c) == CharacterClass.IdentifierPart;
        }


        /// <summary>
        /// Gets the identifier at the given offset in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The identifier at <paramref name="offset"/>.</returns>
        /// <remarks>
        /// An identifier is a single word consisting of letters, digits, or underscores.
        /// An identifier must start with a letter or underscore.
        /// </remarks>
        public static string GetIdentifierAt(ITextSource document, int offset)
        {
            if (offset < 0 || offset >= document.TextLength || !IsIdentifierPart(document.GetCharAt(offset)))
                return string.Empty;

            int startOffset = FindStartOfIdentifier(document, offset);
            if (startOffset == -1)
                return string.Empty;

            int endOffset = FindEndOfIdentifier(document, offset);

            Debug.Assert(endOffset != -1);
            Debug.Assert(endOffset >= startOffset);

            return document.GetText(startOffset, endOffset - startOffset + 1);
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
        public static int FindStartOfIdentifier(ITextSource document, int offset)
        {
            if (offset < 0 || document.TextLength <= offset)
                return -1;

            if (!IsIdentifierPart(document.GetCharAt(offset)))
            {
                // Character at offset is does not belong to an identifier.
                return -1;
            }

            // Search backwards
            while (0 < offset && IsIdentifierPart(document.GetCharAt(offset - 1)))
                --offset;

            // Check if first character is the start of an identifier.
            // (We need to make sure that it is not a number.)
            char startCharacter = document.GetCharAt(offset);
            if (char.IsLetter(startCharacter) || startCharacter == '_')
                return offset;

            return -1;
        }


        /// <summary>
        /// Finds the end of the identifier at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>
        /// The offset of the last character of the identifier; or -1 if there is no identifier at
        /// the specified offset.
        /// </returns>
        /// <remarks>
        /// <para>
        /// An identifier is a single word consisting of letters, digits, or underscores. An
        /// identifier must start with a letter or underscore.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> This method does not guarantee that the word at
        /// <paramref name="offset"/> is an identifier - it could also be a number instead of an
        /// identifier. To make sure that the current word is really an identifier, you should
        /// search for the start of the identifier and check whether it starts with a letter or
        /// underscore.
        /// </para>
        /// </remarks>
        public static int FindEndOfIdentifier(ITextSource document, int offset)
        {
            if (offset < 0 || offset >= document.TextLength)
                return -1;

            if (!IsIdentifierPart(document.GetCharAt(offset)))
            {
                // Character at offset is does not belong to an identifier.
                return -1;
            }

            // Search forward
            while (offset + 1 < document.TextLength && IsIdentifierPart(document.GetCharAt(offset + 1)))
                ++offset;

            return offset;
        }


        /// <summary>
        /// Gets the expression before a given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="initialOffset">The initial offset.</param>
        /// <returns>The expression.</returns>
        /// <remarks>
        /// This method returns the expression before a specified offset.
        /// That method is used in code completion to determine the expression before
        /// the caret. The expression can be passed to a parser to resolve the type
        /// or similar.
        /// </remarks>
        internal static string GetExpressionBeforeOffset(TextDocument document, int initialOffset)
        {
            int offset = initialOffset;
            while (offset - 1 > 0)
            {
                switch (document.GetCharAt(offset - 1))
                {
                    case '\n':
                    case '\r':
                    case '}':
                        goto done;
                    //offset = FindOpeningBracket(document, offset - 2, '{','}');
                    //break;
                    case ']':
                        offset = FindOpeningBracket(document, offset - 2, '[', ']');
                        break;
                    case ')':
                        offset = FindOpeningBracket(document, offset - 2, '(', ')');
                        break;
                    case '.':
                        --offset;
                        break;
                    case '"':
                        if (offset < initialOffset - 1)
                        {
                            return null;
                        }
                        return "\"\"";
                    case '\'':
                        if (offset < initialOffset - 1)
                        {
                            return null;
                        }
                        return "'a'";
                    case '>':
                        if (document.GetCharAt(offset - 2) == '-')
                        {
                            offset -= 2;
                            break;
                        }
                        goto done;
                    default:
                        if (char.IsWhiteSpace(document.GetCharAt(offset - 1)))
                        {
                            --offset;
                            break;
                        }
                        int start = offset - 1;
                        if (!IsIdentifierPart(document.GetCharAt(start)))
                        {
                            goto done;
                        }

                        while (start > 0 && IsIdentifierPart(document.GetCharAt(start - 1)))
                        {
                            --start;
                        }
                        string word = document.GetText(start, offset - start).Trim();
                        switch (word)
                        {
                            case "ref":
                            case "out":
                            case "in":
                            case "return":
                            case "throw":
                            case "case":
                                goto done;
                        }

                        if (word.Length > 0 && !IsIdentifierPart(word[0]))
                        {
                            goto done;
                        }
                        offset = start;
                        break;
                }
            }
            done:
            // simple exit fails when : is inside comment line or any other character
            // we have to check if we got several ids in resulting line, which usually happens when
            // id. is typed on next line after comment one
            // Would be better if lexer would parse properly such expressions. However this will cause
            // modifications in this area too - to get full comment line and remove it afterwards
            if (offset < 0)
                return string.Empty;

            string resText = document.GetText(offset, initialOffset - offset).Trim();
            int pos = resText.LastIndexOf('\n');
            if (pos >= 0)
            {
                offset += pos + 1;
                // whitespaces and tabs, which might be inside, will be skipped by trim below
            }

            string expression = document.GetText(offset, initialOffset - offset).Trim();
            return expression;
        }


        /// <summary>
        /// Checks whether a region (offset + length) matches a given word.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="segment">
        /// The segment of the document to compare against <paramref name="word"/>.
        /// </param>
        /// <param name="word">The word.</param>
        /// <returns><see langword="true"/> if region matches word.</returns>
        /// <remarks>The comparison is case-sensitive.</remarks>
        internal static bool CompareSegment(TextDocument document, ISegment segment, string word)
        {
            return CompareSegment(document, segment.Offset, segment.Length, word);
        }


        /// <summary>
        /// Checks whether a region (offset + length) matches a given word.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="segment">The segment of the document to compare against <paramref name="word"/>.</param>
        /// <param name="word">The word.</param>
        /// <param name="caseSensitive">If set to <see langword="true"/> the comparison is case-sensitive.</param>
        /// <returns><see langword="true"/> if region matches word.</returns>
        internal static bool CompareSegment(TextDocument document, ISegment segment, string word, bool caseSensitive)
        {
            return CompareSegment(document, segment.Offset, segment.Length, word, caseSensitive);
        }


        /// <summary>
        /// Checks whether a region (offset + length) matches a given word.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="word">The word.</param>
        /// <returns><see langword="true"/> if region matches word.</returns>
        /// <remarks>
        /// The comparison is case-sensitive.
        /// </remarks>
        internal static bool CompareSegment(TextDocument document, int offset, int length, string word)
        {
            if (length != word.Length || document.TextLength < offset + length)
                return false;

            for (int i = 0; i < length; ++i)
                if (document.GetCharAt(offset + i) != word[i])
                    return false;

            return true;
        }


        /// <summary>
        /// Checks whether a region (offset + length) matches a given word.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="word">The word.</param>
        /// <param name="caseSensitive">
        /// If set to <see langword="true"/> the comparison is case-sensitive.
        /// </param>
        /// <returns><see langword="true"/> if region matches word.</returns>
        internal static bool CompareSegment(TextDocument document, int offset, int length, string word, bool caseSensitive)
        {
            if (caseSensitive)
                return CompareSegment(document, offset, length, word);

            if (length != word.Length || document.TextLength < offset + length)
                return false;

            for (int i = 0; i < length; ++i)
                if (char.ToUpper(document.GetCharAt(offset + i)) != char.ToUpper(word[i]))
                    return false;

            return true;
        }


        ///// <summary>
        ///// Converts leading whitespaces to tabs.
        ///// </summary>
        ///// <param name="line">The line.</param>
        ///// <param name="tabIndent">The indentation size.</param>
        ///// <returns>The converted line.</returns>
        ///// <remarks>
        ///// This function takes a string and converts the whitespace in front of
        ///// it to tabs. If the length of the whitespace at the start of the string
        ///// was not a whole number of tabs then there will still be some spaces just
        ///// before the text starts.
        ///// the output string will be of the form:
        ///// <list type="number">
        ///// <item><description>zero or more tabs</description></item>
        ///// <item><description>zero or more spaces (less than tabIndent)</description></item>
        ///// <item><description>the rest of the line</description></item>
        ///// </list>
        ///// </remarks>
        //public static string LeadingWhitespaceToTabs(string line, int tabIndent)
        //{
        //    StringBuilder sb = new StringBuilder(line.Length);
        //    int consecutiveSpaces = 0;
        //    int i;
        //    for (i = 0; i < line.Length; i++)
        //    {
        //        if (line[i] == ' ')
        //        {
        //            consecutiveSpaces++;
        //            if (consecutiveSpaces == tabIndent)
        //            {
        //                sb.Append('\t');
        //                consecutiveSpaces = 0;
        //            }
        //        }
        //        else if (line[i] == '\t')
        //        {
        //            sb.Append('\t');
        //            // if we had say 3 spaces then a tab and tabIndent was 4 then
        //            // we would want to simply replace all of that with 1 tab
        //            consecutiveSpaces = 0;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    if (i < line.Length)
        //        sb.Append(line.Substring(i - consecutiveSpaces));

        //    return sb.ToString();
        //}


        /// <summary>
        /// Searches for the start of the current line.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The current offset.</param>
        /// <returns>The start offset of the current line.</returns>
        private static int ScanLineStart(ITextSource document, int offset)
        {
            for (int i = offset - 1; i > 0; --i)
            {
                if (document.GetCharAt(i) == '\n')
                    return i + 1;
            }
            return 0;
        }


        /// <summary>
        /// Finds the offset of the opening bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (before the closing bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the opening bracket or -1 if no matching bracket was found.
        /// </returns>
        private static int SearchBracketBackwardQuick(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            int brackets = -1;

            // first try "quick find" - find the matching bracket if there is no string/comment in the way
            for (int i = offset; i >= 0; --i)
            {
                char ch = document.GetCharAt(i);
                if (ch == openBracket)
                {
                    ++brackets;
                    if (brackets == 0) return i;
                }
                else if (ch == closingBracket)
                {
                    --brackets;
                }
                else if (ch == '"')
                {
                    break;
                }
                else if (ch == '\'')
                {
                    break;
                }
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                    if (document.GetCharAt(i - 1) == '*')
                        break;
                }
            }
            return -1;
        }


        /// <summary>
        /// Finds the offset of the closing bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (after the opening bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the closing bracket or -1 if no matching bracket was found.
        /// </returns>
        private static int SearchBracketForwardQuick(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            int brackets = 1;
            // try "quick find" - find the matching bracket if there is no string/comment in the way
            for (int i = offset; i < document.TextLength; ++i)
            {
                char ch = document.GetCharAt(i);
                if (ch == openBracket)
                {
                    ++brackets;
                }
                else if (ch == closingBracket)
                {
                    --brackets;
                    if (brackets == 0)
                        return i;
                }
                else if (ch == '"')
                {
                    break;
                }
                else if (ch == '\'')
                {
                    break;
                }
                else if (ch == '/' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                }
                else if (ch == '*' && i > 0)
                {
                    if (document.GetCharAt(i - 1) == '/')
                        break;
                }
            }
            return -1;
        }

        /// <summary>
        /// Finds the offset of the opening bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (before the closing bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the opening bracket or -1 if no matching bracket was found.
        /// </returns>
        private static int SearchBracketBackward(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            if (offset >= document.TextLength)
                return -1;

            // first try "quick find" - find the matching bracket if there is no string/comment in the way
            int quickResult = SearchBracketBackwardQuick(document, offset, openBracket, closingBracket);
            if (quickResult >= 0)
                return quickResult;

            // we need to parse the line from the beginning, so get the line start position
            int linestart = ScanLineStart(document, offset + 1);

            // we need to know where offset is - in a string/comment or in normal code?
            // ignore cases where offset is in a block comment
            CodeType starttype = GetStartType(document, linestart, offset + 1);
            if (starttype != 0)
                return -1; // start position is in a comment/string

            // I don't see any possibility to parse a C# document backwards...
            // We have to do it forwards and push all bracket positions on a stack.
            Stack bracketStack = new Stack();
            bool blockComment = false;
            bool lineComment = false;
            bool inChar = false;
            bool inString = false;
            bool verbatim = false;

            for (int i = 0; i <= offset; ++i)
            {
                char ch = document.GetCharAt(i);
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        lineComment = false;
                        inChar = false;
                        if (!verbatim) inString = false;
                        break;
                    case '/':
                        if (blockComment)
                        {
                            Debug.Assert(i > 0);
                            if (document.GetCharAt(i - 1) == '*')
                            {
                                blockComment = false;
                            }
                        }
                        if (!inString && !inChar && i + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(i + 1) == '/')
                            {
                                lineComment = true;
                            }
                            if (!lineComment && document.GetCharAt(i + 1) == '*')
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
                                if (i + 1 < document.TextLength && document.GetCharAt(i + 1) == '"')
                                {
                                    ++i; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && offset > 0 && document.GetCharAt(i - 1) == '@')
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
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++i; // skip next character
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                bracketStack.Push(i);
                            }
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                if (bracketStack.Count > 0)
                                    bracketStack.Pop();
                            }
                        }
                        break;
                }
            }
            if (bracketStack.Count > 0) return (int)bracketStack.Pop();
            return -1;
        }


        /// <summary>
        /// Finds the offset of the closing bracket in the block defined by offset skipping
        /// brackets, strings and comments.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">
        /// The offset of an position in the block (after the opening bracket).
        /// </param>
        /// <param name="openBracket">The character for the opening bracket.</param>
        /// <param name="closingBracket">The character for the closing bracket.</param>
        /// <returns>
        /// Returns the offset of the closing bracket or -1 if no matching bracket was found.
        /// </returns>
        private static int SearchBracketForward(ITextSource document, int offset, char openBracket, char closingBracket)
        {
            bool inString = false;
            bool inChar = false;
            bool verbatim = false;

            bool lineComment = false;
            bool blockComment = false;

            if (offset < 0) return -1;

            // first try "quick find" - find the matching bracket if there is no string/comment in the way
            int quickResult = SearchBracketForwardQuick(document, offset, openBracket, closingBracket);
            if (quickResult >= 0) return quickResult;

            // we need to parse the line from the beginning, so get the line start position
            int linestart = ScanLineStart(document, offset);

            // we need to know where offset is - in a string/comment or in normal code?
            // ignore cases where offset is in a block comment
            CodeType starttype = GetStartType(document, linestart, offset);
            if (starttype != CodeType.Code)
                return -1; // start position is in a comment/string

            int brackets = 1;

            while (offset < document.TextLength)
            {
                char ch = document.GetCharAt(offset);
                switch (ch)
                {
                    case '\r':
                    case '\n':
                        lineComment = false;
                        inChar = false;
                        if (!verbatim) inString = false;
                        break;
                    case '/':
                        if (blockComment)
                        {
                            Debug.Assert(offset > 0);
                            if (document.GetCharAt(offset - 1) == '*')
                            {
                                blockComment = false;
                            }
                        }
                        if (!inString && !inChar && offset + 1 < document.TextLength)
                        {
                            if (!blockComment && document.GetCharAt(offset + 1) == '/')
                            {
                                lineComment = true;
                            }
                            if (!lineComment && document.GetCharAt(offset + 1) == '*')
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
                                if (offset + 1 < document.TextLength && document.GetCharAt(offset + 1) == '"')
                                {
                                    ++offset; // skip escaped quote
                                    inString = false; // let the string go
                                }
                                else
                                {
                                    verbatim = false;
                                }
                            }
                            else if (!inString && offset > 0 && document.GetCharAt(offset - 1) == '@')
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
                    case '\\':
                        if ((inString && !verbatim) || inChar)
                            ++offset; // skip next character
                        break;
                    default:
                        if (ch == openBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                ++brackets;
                            }
                        }
                        else if (ch == closingBracket)
                        {
                            if (!(inString || inChar || lineComment || blockComment))
                            {
                                --brackets;
                                if (brackets == 0)
                                {
                                    return offset;
                                }
                            }
                        }
                        break;
                }
                ++offset;
            }
            return -1;
        }
    }
}
