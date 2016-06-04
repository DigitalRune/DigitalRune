using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.Indentation
{
    /// <summary>
    /// Smart indentation for XML.
    /// </summary>
    public class XmlIndentationStrategy : DefaultIndentationStrategy
    {
        /// <inheritdoc/>
        public override void IndentLine(TextArea textArea, DocumentLine line)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            try
            {
                TryIndent(textArea, line.LineNumber, line.LineNumber);
            }
            catch (XmlException)
            {
                // Indentation failed.
            }
        }


        /// <inheritdoc/>
        public override void IndentLines(TextArea textArea, int beginLine, int endLine)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));

            try
            {
                TryIndent(textArea, beginLine, endLine);
            }
            catch (XmlException)
            {
                // Indentation failed.
            }
        }


        private static void TryIndent(TextArea textArea, int begin, int end)
        {
            string currentIndentation = "";
            var tagStack = new Stack<string>();

            var document = textArea.Document;
            string tab = GetIndentationString(textArea);
            int nextLine = begin; // in #dev coordinates
            bool wasEmptyElement = false;
            var lastType = XmlNodeType.XmlDeclaration;
            using (var stringReader = new StringReader(document.Text))
            {
                var r = new XmlTextReader(stringReader);
                r.XmlResolver = null; // prevent XmlTextReader from loading external DTDs
                while (r.Read())
                {
                    if (wasEmptyElement)
                    {
                        wasEmptyElement = false;
                        if (tagStack.Count == 0)
                            currentIndentation = "";
                        else
                            currentIndentation = tagStack.Pop();
                    }
                    if (r.NodeType == XmlNodeType.EndElement)
                    {
                        // Indent lines before closing tag.
                        while (nextLine < r.LineNumber)
                        {
                            // Set indentation of 'nextLine'
                            DocumentLine line = document.GetLineByNumber(nextLine);
                            string lineText = document.GetText(line);

                            string newText = currentIndentation + lineText.Trim();

                            if (newText != lineText)
                                document.Replace(line.Offset, line.Length, newText);

                            nextLine += 1;
                        }

                        if (tagStack.Count == 0)
                            currentIndentation = "";
                        else
                            currentIndentation = tagStack.Pop();
                    }

                    while (r.LineNumber >= nextLine)
                    {
                        if (nextLine > end)
                            break;
                        if (lastType == XmlNodeType.CDATA || lastType == XmlNodeType.Comment)
                        {
                            nextLine++;
                            continue;
                        }
                        // set indentation of 'nextLine'
                        DocumentLine line = document.GetLineByNumber(nextLine);
                        string lineText = document.GetText(line);

                        string newText;
                        // special case: opening tag has closing bracket on extra line: remove one indentation level
                        if (lineText.Trim() == ">")
                            newText = tagStack.Peek() + lineText.Trim();
                        else
                            newText = currentIndentation + lineText.Trim();

                        document.SmartReplaceLine(line, newText);
                        nextLine++;
                    }

                    if (r.LineNumber >= end)
                        break;

                    wasEmptyElement = r.NodeType == XmlNodeType.Element && r.IsEmptyElement;
                    string attribIndent = null;
                    if (r.NodeType == XmlNodeType.Element)
                    {
                        tagStack.Push(currentIndentation);
                        if (r.LineNumber <= begin)
                        {
                            var whitespace = TextUtilities.GetWhitespaceAfter(document, document.GetOffset(r.LineNumber, 1));
                            currentIndentation = document.GetText(whitespace);
                        }
                        if (r.Name.Length < 16)
                            attribIndent = currentIndentation + new string(' ', 2 + r.Name.Length);
                        else
                            attribIndent = currentIndentation + tab;
                        currentIndentation += tab;
                    }

                    lastType = r.NodeType;
                    if (r.NodeType == XmlNodeType.Element && r.HasAttributes)
                    {
                        int startLine = r.LineNumber;
                        r.MoveToAttribute(0); // move to first attribute
                        if (r.LineNumber != startLine)
                            attribIndent = currentIndentation; // change to tab-indentation
                        r.MoveToAttribute(r.AttributeCount - 1);
                        while (r.LineNumber >= nextLine)
                        {
                            if (nextLine > end)
                                break;
                            // set indentation of 'nextLine'
                            DocumentLine line = document.GetLineByNumber(nextLine);
                            string newText = attribIndent + document.GetText(line).Trim();
                            document.SmartReplaceLine(line, newText);
                            nextLine++;
                        }
                    }
                }

                r.Close();
            }
        }
    }
}
