using System;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        /// <summary>
        /// Gets the string that begins a comment for the syntax highlighting.
        /// </summary>
        /// <returns>
        /// The begin comment string, for example "//" for C/C++, or <see langword="null"/> if no
        /// string is known.
        /// </returns>
        public string GetBeginCommentString()
        {
            if (SyntaxHighlighting == null)
                return null;

            switch (SyntaxHighlighting.Name)
            {
                case "Boo":
                case "C++":
                case "C#":
                case "Cg":
                case "Coco":
                case "HLSL":
                case "Java":
                case "JavaScript":
                    return "//";

                case "ASP/XHTML":
                case "HTML":
                case "XML":
                    return "<!--";

                case "TeX":
                    return "%";

                case "VBNET":
                    return "'";

                default:
                    return null;
            }
        }


        /// <summary>
        /// Gets the string that ends a comment for the syntax highlighting.
        /// </summary>
        /// <returns>
        /// The end comment string, for example "-->" for XML, or <see langword="null"/> if no
        /// string is known.
        /// </returns>
        public string GetEndCommentString()
        {
            if (SyntaxHighlighting == null)
                return null;

            switch (SyntaxHighlighting.Name)
            {
                //case "Boo":
                //case "C++":
                //case "C#":
                //case "Cg":
                //case "Coco":
                //case "HLSL":
                //case "Java":
                //case "JavaScript":
                //case "TeX":
                //case "VBNET":
                //    return null;

                case "ASP/XHTML":
                case "HTML":
                case "XML":
                    return "-->";

                default:
                    return null;
            }
        }


        /// <summary>
        /// Comments out the lines in the current selection.
        /// </summary>
        public void CommentSelection()
        {
            // Get the line comment string, e.g. "//" for C/C++.
            var beginCommentString = GetBeginCommentString();
            if (beginCommentString == null)
                return;

            // End comment strings, like "-->" are optional.
            var endCommentString = GetEndCommentString();

            if (SelectionLength == 0)
            {
                // Comment out line that contains caret.
                var lineNumber = TextArea.Caret.Line;

                // Don't change empty lines.
                if (TextUtilities.IsEmptyLine(Document, lineNumber))
                    return;

                // If we have to make 2 text changes, combine them in the undo buffer.
                if (endCommentString != null)
                    Document.UndoStack.StartUndoGroup();

                // Insert begin comment string, e.g. "//" for C# or "<!--" for XML.
                DocumentLine line = Document.GetLineByNumber(lineNumber);
                var firstCharOffset = TextUtilities.FindFirstNonWhitespace(Document, line.Offset);
                Document.Insert(firstCharOffset, beginCommentString);

                // Insert end comment string, e.g. "-->" for XML and nothing for C#.
                if (endCommentString != null)
                {
                    var lastCharOffset = TextUtilities.FindFirstNonWhitespace(
                        Document, 
                        line.EndOffset,
                        searchBackwards:true);
                    Document.Insert(lastCharOffset + 1, endCommentString);

                    Document.UndoStack.EndUndoGroup();
                }
            }
            else
            {
                // Comment out all lines in the selection.
                using (Document.RunUpdate())
                {
                    var startLocation = Document.GetLocation(SelectionStart);
                    var endLocation = Document.GetLocation(SelectionStart + SelectionLength);

                    int firstLineNumber = startLocation.Line;
                    int lastLineNumber = endLocation.Line;

                    // Exclude last line if caret is in first column.
                    if (endLocation.Column == 1)
                        --lastLineNumber;

                    // Find the relative line offset where the comment string should be placed (all
                    // should be vertically aligned.)
                    int commentOffset = int.MaxValue;
                    for (int i = firstLineNumber; i <= lastLineNumber; i++)
                    {
                        // Skip empty lines.
                        if (TextUtilities.IsEmptyLine(Document, i))
                            continue;

                        int lineStartOffset = Document.GetLineByNumber(i).Offset;
                        int firstCharOffset = TextUtilities.FindFirstNonWhitespace(Document, lineStartOffset);
                        commentOffset = Math.Min(commentOffset, firstCharOffset - lineStartOffset);

                        // No vertical alignment.
                        if (endCommentString != null)
                            break;
                    }

                    // Add line comment strings to all not empty lines.
                    for (int i = firstLineNumber; i <= lastLineNumber; i++)
                    {
                        // Skip empty lines.
                        if (TextUtilities.IsEmptyLine(Document, i))
                            continue;

                        DocumentLine line = Document.GetLineByNumber(i);
                        Document.Insert(line.Offset + commentOffset, beginCommentString);

                        // If we the comment strings form a bracket, (e.g. <!-- ... -->), we write
                        // the begin comment string only once.
                        if (endCommentString != null)
                            break;
                    }

                    if (endCommentString != null)
                    {
                        // Add line comment strings to all not empty lines.
                        for (int i = lastLineNumber; i >= firstLineNumber; i--)
                        {
                            // Skip empty lines.
                            if (TextUtilities.IsEmptyLine(Document, i))
                                continue;

                            DocumentLine line = Document.GetLineByNumber(i);
                            var lastCharOffset = TextUtilities.FindFirstNonWhitespace(
                                Document,
                                line.EndOffset,
                                searchBackwards: true);
                            Document.Insert(lastCharOffset + 1, endCommentString);

                            // We need to remove only one end comment string.
                            break;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Uncomments the lines in the current selection.
        /// </summary>
        public void UncommentSelection()
        {
            // Get the line comment string, e.g. "//" for C/C++.
            var beginCommentString = GetBeginCommentString();
            if (beginCommentString == null)
                return;
            
            // End comment strings, like "-->" are optional.
            var endCommentString = GetEndCommentString();

            if (SelectionLength == 0)
            {
                // Uncomment line that contains caret.
                int lineNumber = TextArea.Caret.Line;

                // Don't change empty lines.
                if (TextUtilities.IsEmptyLine(Document, lineNumber))
                    return;

                // If we have to make 2 text changes, combine them in the undo buffer.
                if (endCommentString != null)
                    Document.UndoStack.StartUndoGroup();

                // Find first character.
                DocumentLine line = Document.GetLineByNumber(lineNumber);
                var firstCharOffset = TextUtilities.FindFirstNonWhitespace(Document, line.Offset);

                // Compare beginning with line comment string and remove the line comment if found.
                if (firstCharOffset + beginCommentString.Length <= Document.TextLength
                    && Document.GetText(firstCharOffset, beginCommentString.Length) == beginCommentString)
                {
                    Document.Remove(firstCharOffset, beginCommentString.Length);
                }

                if (endCommentString != null)
                {
                    var lastCharOffset = TextUtilities.FindFirstNonWhitespace(
                            Document,
                            line.EndOffset,
                            searchBackwards: true);

                    // Compare with end comment string and remove the line comment if found.
                    var removeOffset = lastCharOffset - endCommentString.Length + 1;
                    if (removeOffset >= 0
                        && Document.GetText(removeOffset, endCommentString.Length) == endCommentString)
                    {
                        Document.Remove(removeOffset, endCommentString.Length);
                    }

                    Document.UndoStack.EndUndoGroup();
                }
            }
            else
            {
                // Uncomment all lines in the selection.
                using (Document.RunUpdate())
                {
                    var startLocation = Document.GetLocation(SelectionStart);
                    var endLocation = Document.GetLocation(SelectionStart + SelectionLength);

                    int firstLineNumber = startLocation.Line;
                    int lastLineNumber = endLocation.Line;

                    // Exclude last line if caret is in first column.
                    if (endLocation.Column == 1)
                        --lastLineNumber;

                    for (int i = firstLineNumber; i <= lastLineNumber; i++)
                    {
                        // Skip empty lines.
                        if (TextUtilities.IsEmptyLine(Document, i))
                            continue;

                        // Find first character.
                        DocumentLine line = Document.GetLineByNumber(i);
                        var firstCharOffset = TextUtilities.FindFirstNonWhitespace(Document, line.Offset);

                        // Compare beginning with line comment string and remove the line comment if found.
                        if (firstCharOffset + beginCommentString.Length <= Document.TextLength
                            && Document.GetText(firstCharOffset, beginCommentString.Length) == beginCommentString)
                        {
                            Document.Remove(firstCharOffset, beginCommentString.Length);
                        }

                        // If we the comment strings form a bracket, (e.g. <!-- ... -->), we write
                        // the begin comment string only once.
                        if (endCommentString != null)
                            break;
                    }

                    if (endCommentString != null)
                    {
                        // Add line comment strings to all not empty lines.
                        for (int i = lastLineNumber; i >= firstLineNumber; i--)
                        {
                            // Skip empty lines.
                            if (TextUtilities.IsEmptyLine(Document, i))
                                continue;

                            DocumentLine line = Document.GetLineByNumber(i);
                            var lastCharOffset = TextUtilities.FindFirstNonWhitespace(
                                Document,
                                line.EndOffset,
                                searchBackwards: true);

                            // Compare with end comment string and remove the line comment if found.
                            var removeOffset = lastCharOffset - endCommentString.Length + 1;
                            if (removeOffset >= 0
                                && Document.GetText(removeOffset, endCommentString.Length) == endCommentString)
                            {
                                Document.Remove(removeOffset, endCommentString.Length);
                            }

                            // We need to remove only one end comment string.
                            break;
                        }
                    }
                }
            }
        }


        private void CanCommentSelection(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            // We can execute fold commands only if we have set a folding strategy.
            eventArgs.CanExecute = !IsReadOnly && GetBeginCommentString() != null;

            // Checking TextEditor.IsReadOnly is a simplification. The exact solution would be to
            // get all insertion positions or deletion segments and check whether they are read-only
            // using TextArea.ReadOnlySectionProvider.
        }
    }
}
