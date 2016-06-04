using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.Formatting
{
    /// <summary>
    /// Strategy how the text editor formats the document when the user inserts text.
    /// </summary>
    public interface IFormattingStrategy
    {
        /// <summary>
        /// Formats a specific line after character is entered.
        /// </summary>
        /// <param name="textArea">The active <see cref="TextArea"/>.</param>
        /// <param name="charTyped">The character that was typed.</param>
        /// <remarks>
        /// <strong>Important:</strong> The caller of the formatting strategy is responsible for
        /// wrapping the operation in an Undo group.
        /// </remarks>
        void FormatLine(TextArea textArea, char charTyped);
    }
}
