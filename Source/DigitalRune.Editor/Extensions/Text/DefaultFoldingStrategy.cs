// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;
using ICSharpCode.AvalonEdit.Folding;
using AvalonEditDocument = ICSharpCode.AvalonEdit.Document.TextDocument;

namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Creates foldings based on indentation.
    /// </summary>
    public class DefaultFoldingStrategy : FoldingStrategy
    {
        /// <inheritdoc/>
        protected override IEnumerable<NewFolding> CreateNewFoldings(
            AvalonEditDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            List<NewFolding> foldings = new List<NewFolding>();

            Stack<NewFolding> activeFoldings = new Stack<NewFolding>();
            Stack<int> activeIndentations = new Stack<int>();

            NewFolding activeFolding = null;
            int activeIndentation = -1;
            int endOffsetOfLastNonEmptyLine = 0;
            for (int lineNumber = 1; lineNumber <= document.LineCount; lineNumber++)
            {
                var line = document.GetLineByNumber(lineNumber);

                // Determine indentation of line.
                int offset = line.Offset;
                int indentation = 0;
                bool isLineEmpty = true;
                for (int i = 0; i < line.Length; i++)
                {
                    if (char.IsWhiteSpace(document.GetCharAt(offset + i)))
                    {
                        indentation++;
                    }
                    else
                    {
                        // Found the first non-white space character.
                        isLineEmpty = false;
                        break;
                    }
                }

                // Skip empty lines.
                if (isLineEmpty)
                    continue;

                // If the indentation is less than the previous, then we close the last active
                // folding.
                if (indentation < activeIndentation)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    activeFolding.EndOffset = endOffsetOfLastNonEmptyLine;

                    // Keep foldings which span more than one line.
                    if (document.GetLineByOffset(activeFolding.StartOffset) != document.GetLineByOffset(activeFolding.EndOffset))
                        foldings.Add(activeFolding);

                    if (activeFoldings.Count > 0)
                    {
                        activeFolding = activeFoldings.Pop();
                        activeIndentation = activeIndentations.Pop();
                    }
                    else
                    {
                        activeFolding = null;
                        activeIndentation = -1;
                    }

                    // Test same line again. The previous folding could also end on this line.
                    lineNumber--;
                    continue;
                }

                endOffsetOfLastNonEmptyLine = line.EndOffset;

                // If the indentation is larger than the previous indentation, we start a new
                // folding.
                if (indentation > 0 && indentation > activeIndentation)
                {
                    // Store current folding on stack.
                    if (activeFolding != null)
                    {
                        activeFoldings.Push(activeFolding);
                        activeIndentations.Push(activeIndentation);
                    }

                    activeFolding = new NewFolding { StartOffset = offset + indentation, };
                    activeIndentation = indentation;
                }
            }

            // Close all open foldings.
            while (activeFoldings.Count > 0)
            {
                var folding = activeFoldings.Pop();
                folding.EndOffset = endOffsetOfLastNonEmptyLine;
                foldings.Add(folding);
            }
            
            foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldings;
        }
    }
}
