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

using System.Collections.Generic;


namespace ICSharpCode.AvalonEdit.Highlighting
{
	/// <summary>
	/// Interface for resolvers that can solve cross-definition references.
	/// </summary>
	public interface IHighlightingDefinitionReferenceResolver
	{
		/// <summary>
		/// Gets a highlighting definition by name.
		/// </summary>
		/// <param name="name">The name of the highlighting definition.</param>
		/// <returns>
		/// The definition with the given name. Returns <see langword="null"/> if no matching definition 
		/// was found.
		/// </returns>
		IHighlightingDefinition GetDefinition(string name);
	}


	// [DIGITALRUNE] Interface IHightlightingService added for IoC.

	/// <summary>
	/// Provides a list of syntax highlighting definitions.
	/// </summary>
	public interface IHighlightingService : IHighlightingDefinitionReferenceResolver
	{
		/// <summary>
		/// Gets a read-only copy of all registered highlighting definitions.
		/// </summary>
		/// <value>A read-only copy of all registered highlighting definitions.</value>
		IReadOnlyCollection<IHighlightingDefinition> HighlightingDefinitions { get; }

		/// <summary>
		/// Gets the definition by file extension.
		/// </summary>
		/// <param name="extension">The file extension.</param>
		/// <returns>
		/// The definition for the given file extension. Returns <see langword="null"/> if no matching
		/// definition was found.
		/// </returns>
		IHighlightingDefinition GetDefinitionByExtension(string extension);
	}
}
