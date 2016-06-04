// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Arranges its child element to the max available space without demanding extra space during
    /// the measure pass.
    /// </summary>
    /// <remarks>
    /// This <see cref="Decorator"/> always returns the minimum size
    /// (<see cref="FrameworkElement.MinWidth"/>, <see cref="FrameworkElement.MinHeight"/>) in the
    /// measure pass. The size required by the child element is ignored. In the arrange pass the
    /// child element is stretched to the available space.
    /// </remarks>
    public class MinimumStretch : Decorator
    {
        /// <summary>
        /// Measures the child element of a <see cref="MinimumStretch"/> to prepare for arranging it
        /// during the <see cref="ArrangeOverride(Size)"/> pass.
        /// </summary>
        /// <param name="constraint">
        /// An upper limit <see cref="Size"/> that should not be exceeded.
        /// </param>
        /// <returns>The target <see cref="Size"/> of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Child?.Measure(constraint);
            return new Size(MinWidth, MinHeight);
        }


        /// <summary>
        /// When overridden in a derived class, positions child elements and determines a size for a
        /// <see cref="FrameworkElement"/> derived class.
        /// </summary>
        /// <param name="finalSize">
        /// The final area within the parent that this element should use to arrange itself and its
        /// children.
        /// </param>
        /// <returns>The actual size used.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            Child?.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return finalSize;
        }
    }
}
