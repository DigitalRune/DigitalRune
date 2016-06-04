using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit.Folding
{
    /// <summary>
    /// A simple folding strategy for HLSL files.
    /// </summary>
    public class HlslFoldingStrategy : FoldingStrategy
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Dictionary<string, object> Blocks;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="HlslFoldingStrategy"/> class.
        /// </summary>
        static HlslFoldingStrategy()
        {
            Blocks = new Dictionary<string, object>
            {
                ["asm"] = null,
                ["BlendState"] = null,
                ["cbuffer"] = null,
                ["DepthStencilState"] = null,
                ["interface"] = null,
                ["pass"] = null,
                ["RasterizerState"] = null,
                ["struct"] = null,
                ["sampler_state"] = null,
                ["stateblock_state"] = null,
                ["SamplerState"] = null,
                ["SamplerComparisonState"] = null,
                ["tbuffer"] = null,
                ["technique"] = null,
                ["technique10"] = null
            };
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            // No error.
            firstErrorOffset = -1;

            var foldMarkers = new List<NewFolding>();
            MarkBlocks(document, foldMarkers);

            // Foldings must be sorted by start offset.
            foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));

            return foldMarkers;
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
                    case '{':
                        offset = MarkMethod(document, offset, foldMarkers);
                        break;
                    default:
                        offset = MarkBlock(document, offset, foldMarkers);
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
                return offset + 1;  // End of document, skip to end of document

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
                        break;  // End loop
                    }
                    offset++;
                }
            }
            else
            {
                offset++;
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
            }
            else
            {
                offset++;
            }
            return offset;
        }


        /// <summary>
        /// Marks the block that starts at the current offset.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="offset">The offset of the identifier.</param>
        /// <param name="foldMarkers">The fold markers.</param>
        /// <returns>The index of the next character after the block.</returns>
        private static int MarkBlock(TextDocument document, int offset, ICollection<NewFolding> foldMarkers)
        {
            if (offset >= document.TextLength)
                return offset;

            string word = TextUtilities.GetIdentifierAt(document, offset);
            if (Blocks.ContainsKey(word))
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
                if (word.Length > 0)
                {
                    // Skip to next word
                    offset += word.Length;
                }
                else
                {
                    // Skip to next character
                    offset++;
                }
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
        #endregion
    }
}
