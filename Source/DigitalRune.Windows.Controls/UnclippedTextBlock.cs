// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a specialized <see cref="TextBlock"/> where the text is never clipped.
    /// </summary>
    /// <remarks>
    /// This is a <see cref="TextBlock"/> where <see cref="GetLayoutClip"/> returns
    /// <see langword="null"/> to avoid clipping at the bounds.
    /// </remarks>
    public class UnclippedTextBlock : TextBlock
    {
        /// <summary>
        /// Initializes static members of the <see cref="UnclippedTextBlock"/> class.
        /// </summary>
        static UnclippedTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UnclippedTextBlock), new FrameworkPropertyMetadata(typeof(UnclippedTextBlock)));
        }


        /// <summary>
        /// Returns a geometry for a clipping mask. The mask applies if the layout system attempts
        /// to arrange an element that is larger than the available display space.
        /// </summary>
        /// <param name="layoutSlotSize">
        /// The size of the part of the element that does visual presentation.
        /// </param>
        /// <returns>The clipping geometry.</returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return null;
        }
    }
}
