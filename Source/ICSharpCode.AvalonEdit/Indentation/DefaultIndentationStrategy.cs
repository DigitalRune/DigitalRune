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
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory.Editor;
using ICSharpCode.AvalonEdit.Document;


namespace ICSharpCode.AvalonEdit.Indentation
{
	/// <summary>
	/// Handles indentation by copying the indentation from the previous line.
	/// Does not support indenting multiple lines.
	/// </summary>
	public class DefaultIndentationStrategy : IIndentationStrategy
	{
		/// <inheritdoc/>
		public virtual void IndentLine(TextArea textArea, DocumentLine line)
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");
			if (line == null)
				throw new ArgumentNullException("line");

			var document = textArea.Document;
			DocumentLine previousLine = line.PreviousLine;
			if (previousLine != null) {
				ISegment indentationSegment = TextUtilities.GetWhitespaceAfter(document, previousLine.Offset);
				string indentation = document.GetText(indentationSegment);
				// copy indentation to line
				indentationSegment = TextUtilities.GetWhitespaceAfter(document, line.Offset);
				document.Replace(indentationSegment, indentation);
			}
		}
		
		/// <summary>
		/// Does nothing: indenting multiple lines is useless without a smart indentation strategy.
		/// </summary>
		public virtual void IndentLines(TextArea textArea, int beginLine, int endLine)
		{
		}

		/// <summary>
		/// Gets the text used for indentation.
		/// </summary>
		/// <param name="textArea">The active <see cref="TextArea"/>.</param>
		/// <returns>The text used for indentation.</returns>
		public static string GetIndentationString(TextArea textArea)    // [DIGITALRUNE]
		{
			if (textArea == null)
				throw new ArgumentNullException("textArea");

			var options = textArea.Options;
			return (options != null) ? options.IndentationString : "\t";
		}
	}
}
