using System.Windows.Input;
using ICSharpCode.AvalonEdit.Formatting;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        /// <summary>
        /// Gets or sets the formatting strategy.
        /// </summary>
        /// <value>The formatting strategy.</value>
        public IFormattingStrategy FormattingStrategy { get; set; }


        /// <summary>
        /// Called when text is entered.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="TextCompositionEventArgs"/> instance containing the event data.
        /// </param>
        private void OnTextEntered(object sender, TextCompositionEventArgs eventArgs)
        {
            if (string.IsNullOrEmpty(eventArgs.Text) || eventArgs.Handled || FormattingStrategy == null)
                return;

            char ch = eventArgs.Text[0];

            // When entering a newline, AvalonEdit might use either "\r\n" or "\n", depending on
            // what was passed to TextArea.PerformTextInput. We'll normalize this to '\n'
            // so that formatting strategies don't have to handle both cases.
            if (ch == '\r')
                ch = '\n';

            // Format/indent line.
            using (Document.RunUpdate())
            {
                FormattingStrategy.FormatLine(textArea, ch);
            }
        }
    }
}
