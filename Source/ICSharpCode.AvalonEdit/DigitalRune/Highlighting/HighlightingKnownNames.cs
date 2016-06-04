using System.Collections.Generic;
using System.Linq;


namespace ICSharpCode.AvalonEdit.Highlighting
{
    /// <summary>
    /// Provides the names of known spans in the syntax highlighting definitions.
    /// </summary>
    public static class HighlightingKnownNames
    {
        /// <summary>
        /// The name of the comment color/span in the highlighting definition.
        /// </summary>
        public const string Comment = "Comment";


        /// <summary>
        /// The name of the string color/span in the highlighting definition.
        /// </summary>
        public const string String = "String";


        /// <summary>
        /// The name of the character literal color/span in the highlighting definition.
        /// </summary>
        public const string Char = "Char";


        /// <summary>
        /// Determines whether current line is inside a comment.
        /// </summary>
        /// <param name="highlighter">The syntax highlighter.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>
        /// <see langword="true"/> if the specified line is inside a comment; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsLineStartInsideComment(this IHighlighter highlighter, int lineNumber)
        {
            return highlighter.GetSpanColorNamesFromLineStart(lineNumber).Contains(Comment);
        }


        /// <summary>
        /// Determines whether current line is inside a string.
        /// </summary>
        /// <param name="highlighter">The syntax highlighter.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>
        /// <see langword="true"/> if the specified line is inside a string; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        public static bool IsLineStartInsideString(this IHighlighter highlighter, int lineNumber)
        {
            return highlighter.GetSpanColorNamesFromLineStart(lineNumber).Contains(String);
        }


        /// <summary>
        /// Retrieves the names of the spans that are active at the start of the specified line.
        /// </summary>
        /// <param name="highlighter">The syntax highlighter.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <returns>
        /// The spans that are active at the start of the specified line. Nested spans are returned in 
        /// inside-out order (first element of result enumerable is the innermost span).
        /// </returns>
        public static IEnumerable<string> GetSpanColorNamesFromLineStart(this IHighlighter highlighter, int lineNumber)
        {
            if (highlighter != null)
            {
                return highlighter.GetColorStack(lineNumber - 1)
                                  .Select(highlightingColor => highlightingColor.Name)
                                  .Where(name => !string.IsNullOrWhiteSpace(name));
            }

            return Enumerable.Empty<string>();
        }
    }
}
