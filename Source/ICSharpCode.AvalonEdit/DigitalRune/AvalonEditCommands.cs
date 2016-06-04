using System.Windows.Input;
using ICSharpCode.AvalonEdit.Highlighting;


namespace ICSharpCode.AvalonEdit
{
    static partial class AvalonEditCommands
    {
        /// <summary>
        /// Gets the value that represents the Paste Multiple command.
        /// </summary>
        /// <value>
        /// The command. The default key gesture is Ctrl+Shift+V. The default UI text is "Paste
        /// multiple".
        /// </value>
        /// <remarks>
        /// This command opens a completion window showing the most recent text entries in the
        /// clipboard.
        /// </remarks>
        public static RoutedUICommand PasteMultiple
        {
            get
            {
                if (_pasteMultiple == null)
                    _pasteMultiple = new RoutedUICommand("Paste multiple", "PasteMultiple", typeof(AvalonEditCommands), new InputGestureCollection(new[] { new KeyGesture(Key.V, ModifierKeys.Control | ModifierKeys.Shift) }));

                return _pasteMultiple;
            }
        }
        private static RoutedUICommand _pasteMultiple;


        /// <summary>
        /// Gets the value that represents the Comment command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Comment
        /// selection".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to comment out the selected lines.
        /// </remarks>
        public static RoutedUICommand Comment
        {
            get
            {
                if (_comment == null)
                    _comment = new RoutedUICommand("Comment selection", "Comment", typeof(AvalonEditCommands));

                return _comment;
            }
        }
        private static RoutedUICommand _comment;


        /// <summary>
        /// Gets the value that represents the Uncomment command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Uncomment
        /// selection".
        /// </value>
        /// <remarks>This command indicates the intention to uncomment the selected lines.</remarks>
        public static RoutedUICommand Uncomment
        {
            get
            {
                if (_uncomment == null)
                    _uncomment = new RoutedUICommand("Uncomment selection", "Uncomment", typeof(AvalonEditCommands));

                return _uncomment;
            }
        }
        private static RoutedUICommand _uncomment;


        /// <summary>
        /// Gets the value that represents the ToggleFold command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Fold/Unfold".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to toggle a fold (toggle outlining)
        /// </remarks>
        public static RoutedUICommand ToggleFold
        {
            get
            {
                if (_toggleFold == null)
                    _toggleFold = new RoutedUICommand("Fold/Unfold", "ToggleFold", typeof(AvalonEditCommands));

                return _toggleFold;
            }
        }
        private static RoutedUICommand _toggleFold;


        /// <summary>
        /// Gets the value that represents the ToggleAllFolds command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Fold/Unfold all".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to toggle all folds (toggle all outlining)
        /// </remarks>
        public static RoutedUICommand ToggleAllFolds
        {
            get
            {
                if (_toggleAllFolds == null)
                    _toggleAllFolds = new RoutedUICommand("Fold/Unfold all", "ToggleAllFolds", typeof(AvalonEditCommands));

                return _toggleAllFolds;
            }
        }
        private static RoutedUICommand _toggleAllFolds;


        /// <summary>
        /// Gets the value that represents the SyntaxHighlighting command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Set Syntax
        /// Highlighting".
        /// </value>
        /// <remarks>
        /// This command indicates the intention to set the syntax highlighting that is specified in
        /// the command parameter. The command parameter must be <see langword="null"/> or a
        /// <see cref="IHighlightingDefinition"/>.
        /// </remarks>
        public static RoutedUICommand SyntaxHighlighting
        {
            get
            {
                if (_syntaxHighlighting == null)
                    _syntaxHighlighting = new RoutedUICommand("Set syntax highlighting", "SyntaxHighlighting", typeof(AvalonEditCommands));

                return _syntaxHighlighting;
            }
        }
        private static RoutedUICommand _syntaxHighlighting;
    }
}
