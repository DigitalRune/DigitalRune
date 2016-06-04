// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.Indentation
{
    /// <summary>
    /// Smart indentation for C#.
    /// </summary>
    public class CSharpIndentationStrategy : DefaultIndentationStrategy
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

            CSharpIndentationReformatter r = new CSharpIndentationReformatter();
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
