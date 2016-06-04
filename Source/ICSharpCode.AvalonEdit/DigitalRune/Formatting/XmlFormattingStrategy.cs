using System;
using System.Diagnostics;
using System.Text;
using System.Xml;
using ICSharpCode.AvalonEdit.Editing;


namespace ICSharpCode.AvalonEdit.Formatting
{
    /// <summary>
    /// Formatting strategy for XML.
    /// </summary>
    public class XmlFormattingStrategy : IFormattingStrategy
    {
        /// <inheritdoc/>
        public void FormatLine(TextArea textArea, char charTyped)
        {
            // [DIGITALRUNE] The XmlFormattingStrategy in SharpDevelop 4.x contains several bugs,
            // which have been fixed. (The code was not updated properly for the new AvalonEdit.
            // SharpDevelop 3.x used 0-based line numbers, whereas SharpDevelop 4.x uses 1-based
            // line numbers!)
            textArea.Document.BeginUpdate();
            try
            {
                if (charTyped == '>')
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    int offset = Math.Min(textArea.Caret.Offset - 2, textArea.Document.TextLength - 1);
                    while (true)
                    {
                        if (offset < 0)
                        {
                            break;
                        }
                        char ch = textArea.Document.GetCharAt(offset);
                        if (ch == '<')
                        {
                            string reversedTag = stringBuilder.ToString().Trim();
                            if (!reversedTag.StartsWith("/", StringComparison.Ordinal) && !reversedTag.EndsWith("/", StringComparison.Ordinal))
                            {
                                bool validXml = true;
                                try
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(textArea.Document.Text);
                                }
                                catch (XmlException)
                                {
                                    validXml = false;
                                }
                                // only insert the tag, if something is missing
                                if (!validXml)
                                {
                                    StringBuilder tag = new StringBuilder();
                                    for (int i = reversedTag.Length - 1; i >= 0 && !Char.IsWhiteSpace(reversedTag[i]); --i)
                                    {
                                        tag.Append(reversedTag[i]);
                                    }
                                    string tagString = tag.ToString();
                                    if (tagString.Length > 0 && !tagString.StartsWith("!", StringComparison.Ordinal) && !tagString.StartsWith("?", StringComparison.Ordinal))
                                    {
                                        int caretOffset = textArea.Caret.Offset;
                                        textArea.Document.Insert(textArea.Caret.Offset, "</" + tagString + ">");
                                        textArea.Caret.Offset = caretOffset;
                                    }
                                }
                            }
                            break;
                        }
                        stringBuilder.Append(ch);
                        --offset;
                    }
                }
            }
            catch (Exception e)
            {
                // Sanity check.
                Debug.Assert(false, e.ToString());
            }

            // [DIGITALRUNE] Not necessary to call indentation strategy. The indentation
            // strategy is called automatically.
            //if (charTyped == '\n' && textArea.IndentationStrategy != null)
            //{
            //  textArea.IndentationStrategy.IndentLine(textArea, textArea.Document.GetLineByNumber(textArea.Caret.Line));
            //}

            textArea.Document.EndUpdate();
        }
    }
}
