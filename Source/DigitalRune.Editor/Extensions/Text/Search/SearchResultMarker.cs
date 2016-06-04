// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Special variant of the <see cref="BlockMarker"/> used to highlight search results.
    /// </summary>
    internal class SearchResultMarker : BlockMarker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SearchResultMarker"/> class.
        /// </summary>
        /// <param name="brush">The background brush.</param>
        public SearchResultMarker(Brush brush)
        {
            Brush = brush;
            Pen = null;
        }
    }
}
