using System;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.Indentation
{
    /// <summary>
    /// Smart indentation for HLSL.
    /// </summary>
    public class HlslIntendationStrategy : DefaultIndentationStrategy
    {
        /// <summary>
        /// Performs indentation using the specified document accessor.
        /// </summary>
        /// <param name="document">Object used for accessing the document line-by-line</param>
        /// <param name="indentationString">The string used for indentation.</param>
        /// <param name="keepEmptyLines">Specifies whether empty lines should be kept</param>
        private void Indent(IDocumentAccessor document, string indentationString, bool keepEmptyLines)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            IndentationSettings settings = new IndentationSettings();
            settings.IndentString = indentationString;
            settings.LeaveEmptyLines = keepEmptyLines;

            HlslIndentationReformatter r = new HlslIndentationReformatter();
            r.Reformat(document, settings);
        }


        /// <inheritdoc cref="IIndentationStrategy.IndentLine"/>
        public override void IndentLine(TextArea textArea, DocumentLine line)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var document = textArea.Document;
            int lineNr = line.LineNumber;
            string indentationString = GetIndentationString(textArea);
            TextDocumentAccessor acc = new TextDocumentAccessor(document, lineNr, lineNr);
            Indent(acc, indentationString, false);

            string t = acc.Text;
            if (t.Length == 0)
            {
                // use AutoIndentation for new lines in comments / verbatim strings.
                base.IndentLine(textArea, line);
            }
        }


        /// <inheritdoc cref="IIndentationStrategy.IndentLines"/>
        public override void IndentLines(TextArea textArea, int beginLine, int endLine)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));

            var document = textArea.Document;
            string indentationString = GetIndentationString(textArea);
            Indent(new TextDocumentAccessor(document, beginLine, endLine), indentationString, false);
        }
    }
}
