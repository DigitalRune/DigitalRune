using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit.Folding
{
    /// <summary>
    /// A simple folding strategy for C# files.
    /// </summary>
    public class CSharpFoldingStrategy : FoldingStrategy
    {
        /// <inheritdoc/>
        protected override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            // No error.
            firstErrorOffset = -1;

            var folds = new List<NewFolding>();
            MarkBlocks(document, folds);
            MarkRegions(document, folds);

            // Foldings must be sorted by start offset.
            folds.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));

            return folds;
        }


        /// <summary>
        /// Marks all code blocks (namespaces, classes, methods, etc.) in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="foldMarkers">The fold markers.</param>
        private static void MarkBlocks(TextDocument document, ICollection<NewFolding> foldMarkers)
        {
            int offset = 0;
            while (offset < document.TextLength)
            {
                switch (document.GetCharAt(offset))
                {
                    case '/':
                        offset = SkipComment(document, offset);
                        break;
                    case 'c':
                        offset = MarkBlock("class", document, offset, foldMarkers);
                        break;
                    case 'e':
                        offset = MarkBlock("enum", document, offset, foldMarkers);
                        break;
                    case 'i':
                        offset = MarkBlock("interface", document, offset, foldMarkers);
                        break;
                    case 'n':
                        offset = MarkBlock("namespace", document, offset, foldMarkers);
                        break;
                    case 's':
                        offset = MarkBlock("struct", document, offset, foldMarkers);
                        break;
                    case '{':
                        offset = MarkMethod(document, offset, foldMarkers);
                        break;
                    default:
                        int endOfIdentifier = TextUtilities.FindEndOfIdentifier(document, offset);
                        if (endOfIdentifier > 0)
                            offset = endOfIdentifier + 1;
                        else
                            ++offset;
                        break;
                }
            }
        }


        /// <summary>
        /// Skips any comments that start at the current offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The index of the next character after the comments.</returns>
        private static int SkipComment(TextDocument document, int offset)
        {
            if (offset >= document.TextLength - 1)
                return offset + 1;

            char current = document.GetCharAt(offset);
            char next = document.GetCharAt(offset + 1);

            if (current == '/' && next == '/')
            {
                // Skip line comment "//"
                var line = document.GetLineByOffset(offset);
                int offsetOfNextLine = line.Offset + line.TotalLength;
                return offsetOfNextLine;
            }

            if (current == '/' && next == '*')
            {
                // Skip block comment "/* ... */"
                offset += 2;
                while (offset + 1 < document.TextLength)
                {
                    if (document.GetCharAt(offset) == '*' && document.GetCharAt(offset + 1) == '/')
                    {
                        offset = offset + 2;
                        break;
                    }
                    offset++;
                }
                return offset;
            }

            return offset + 1;
        }


        /// <summary>
        /// Marks the block that starts at the current offset.
        /// </summary>
        /// <param name="name">The identifier of the block (e.g. "class", "struct").</param>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the identifier.</param>
        /// <param name="foldMarkers">The fold markers.</param>
        /// <returns>The index of the next character after the block.</returns>
        private static int MarkBlock(string name, TextDocument document, int offset, ICollection<NewFolding> foldMarkers)
        {
            if (offset >= document.TextLength)
                return offset;

            string word = TextUtilities.GetIdentifierAt(document, offset);
            if (word == name)
            {
                offset += word.Length;
                while (offset < document.TextLength)
                {
                    char c = document.GetCharAt(offset);
                    if (c == '}' || c == ';')
                    {
                        offset++;
                        break;
                    }
                    if (c == '{')
                    {
                        int startOffset = offset;
                        while (Char.IsWhiteSpace(document.GetCharAt(startOffset - 1)))
                            startOffset--;

                        int offsetOfClosingBracket = TextUtilities.FindClosingBracket(document, offset + 1, '{', '}');
                        if (offsetOfClosingBracket > 0)
                        {
                            AddFold(document, foldMarkers, startOffset, offsetOfClosingBracket + 1, "{...}");

                            // Skip to offset after '{'.
                            offset++;
                            break;
                        }
                    }
                    offset++;
                }
            }
            else
            {
                // Skip to next word
                offset += word.Length;
            }
            return offset;
        }


        /// <summary>
        /// Marks the method whose block starts at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the method body ('{').</param>
        /// <param name="folds">The fold markers.</param>
        /// <returns>The index of the next character after the method.</returns>
        private static int MarkMethod(TextDocument document, int offset, ICollection<NewFolding> folds)
        {
            if (offset >= document.TextLength)
                return offset;

            int startOffset = offset;
            while (startOffset - 1 > 0 && Char.IsWhiteSpace(document.GetCharAt(startOffset - 1)))
                startOffset--;

            int offsetOfClosingBracket = TextUtilities.FindClosingBracket(document, offset + 1, '{', '}');
            if (offsetOfClosingBracket > 0)
            {
                // Check whether next character is ';'
                int offsetOfNextCharacter = TextUtilities.FindFirstNonWhitespace(document, offsetOfClosingBracket + 1);
                if (offsetOfNextCharacter < document.TextLength && document.GetCharAt(offsetOfNextCharacter) == ';')
                    return offset + 1;

                AddFold(document, folds, startOffset, offsetOfClosingBracket + 1, "{...}");

                // Skip to offset after '}'. (Ignore nested blocks.)
                offset = offsetOfClosingBracket + 1;
                return offset;
            }

            return offset + 1;
        }


        /// <summary>
        /// Marks all regions ("#region") in the document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="folds">The fold markers.</param>
        private static void MarkRegions(TextDocument document, List<NewFolding> folds)
        {
            FindAndMarkRegions(document, 0, folds);
        }


        /// <summary>
        /// Finds and marks all regions.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset where the search starts.</param>
        /// <param name="folds">The fold markers.</param>
        /// <returns>The index of the next character after the all regions.</returns>
        /// <remarks>
        /// This method returns when it finds a "#endregion" string that does not have
        /// a "#region" statement after <paramref name="offset"/>. In this case it 
        /// returns the index of the next character after the "#endregion" statement.
        /// </remarks>
        private static int FindAndMarkRegions(TextDocument document, int offset, List<NewFolding> folds)
        {
            if (offset >= document.TextLength)
                return offset;

            while (offset < document.TextLength)
            {
                char c = document.GetCharAt(offset);
                switch (c)
                {
                    case '/':
                        // Skip comments
                        offset = SkipComment(document, offset);
                        break;
                    case '#':
                        string word = TextUtilities.GetIdentifierAt(document, offset + 1);
                        if (word == "region")
                        {
                            offset = MarkRegion(document, offset, folds);
                        }
                        else if (word == "endregion")
                        {
                            return offset + "endregion".Length + 1;
                        }
                        else
                        {
                            offset++;
                        }
                        break;
                    default:
                        // Skip to next word
                        int endOfIdentifier = TextUtilities.FindEndOfIdentifier(document, offset);
                        if (endOfIdentifier > 0)
                            offset = endOfIdentifier + 1;
                        else
                            ++offset;
                        break;
                }
            }
            return offset;
        }


        /// <summary>
        /// Marks the region that starts at the given offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="folds">The fold markers.</param>
        /// <returns>The index of the next character after the region.</returns>
        private static int MarkRegion(TextDocument document, int offset, List<NewFolding> folds)
        {
            if (offset >= document.TextLength)
                return offset;

            if (document.GetCharAt(offset) == '#')
            {
                int startOffset = offset;
                offset++;
                string word = TextUtilities.GetIdentifierAt(document, offset);
                if (word == "region")
                {
                    offset += "region".Length;

                    // Find label
                    var line = document.GetLineByOffset(offset);
                    int lineEnd = line.Offset + line.Length;
                    int labelLength = lineEnd - offset;
                    string label = document.GetText(offset, labelLength);
                    label = label.Trim();
                    if (label.Length == 0)
                        label = "#region";

                    // Find and mark subregions
                    offset = FindAndMarkRegions(document, lineEnd, folds);

                    if (offset <= document.TextLength)
                    {
                        AddFold(document, folds, startOffset, offset, label);
                        offset++;
                    }
                }
            }
            else
            {
                offset++;
            }
            return offset;
        }


        private static void AddFold(TextDocument document, ICollection<NewFolding> foldMarkers, int startOffset, int endOffset, string label)
        {
            // Do not add folding if start and end are on the same line.
            int startLine = document.GetLocation(startOffset).Line;
            int endLine = document.GetLocation(endOffset).Line;
            if (startLine >= endLine)
                return;

            foldMarkers.Add(new NewFolding(startOffset, endOffset) { Name = label });
        }
    }
}
