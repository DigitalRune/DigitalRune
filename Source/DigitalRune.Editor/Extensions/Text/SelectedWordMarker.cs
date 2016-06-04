// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Special variant of the <see cref="BlockMarker"/> used to highlight the currently selected
    /// word.
    /// </summary>
    internal class SelectedWordMarker : BlockMarker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedWordMarker" /> class.
        /// </summary>
        /// <param name="brush">The background brush.</param>
        public SelectedWordMarker(Brush brush)
        {
            Brush = brush;
            Pen = null;
        }
    }
}
