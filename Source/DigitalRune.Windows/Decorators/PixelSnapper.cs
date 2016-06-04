// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Controls;
using System.Windows;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Arranges its child element at whole device pixels to minimize blurring.
    /// </summary>
    public class PixelSnapper : Decorator
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // The pixel offset that is required to align the child element to device pixels.
        private Point _pixelOffset;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="PixelSnapper"/> class.
        /// </summary>
        public PixelSnapper()
        {
            LayoutUpdated += OnLayoutUpdated;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnLayoutUpdated(object sender, EventArgs eventArgs)
        {
            // Layout has changed. Check whether we need to reposition the child element.
            Point newPixelOffset = GetPixelOffset();
            if (!WindowsHelper.AreEqual(newPixelOffset, _pixelOffset))
                InvalidateArrange();
        }


        private Point GetPixelOffset()
        {
            Point pixelOffset = new Point();

            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource != null)
            {
                if (presentationSource.CompositionTarget != null)
                {
                    // Get the root visual (usually the Window).
                    var rootVisual = presentationSource.RootVisual;

                    // Transform (0,0) from the local coordinate space to device pixels.
                    pixelOffset = TransformToAncestor(rootVisual).Transform(new Point(0, 0));
                    pixelOffset = presentationSource.CompositionTarget.TransformToDevice.Transform(pixelOffset);

                    // Round to device pixels.
                    pixelOffset.X = Math.Round(pixelOffset.X, MidpointRounding.AwayFromZero);
                    pixelOffset.Y = Math.Round(pixelOffset.Y, MidpointRounding.AwayFromZero);

                    // Transform the correct point back to local coordinate space.
                    pixelOffset = presentationSource.CompositionTarget.TransformFromDevice.Transform(pixelOffset);
                    pixelOffset = rootVisual.TransformToDescendant(this).Transform(pixelOffset);
                }
            }

            return pixelOffset;
        }


        /// <summary>
        /// Arranges the content of a <see cref="Decorator"/> element.
        /// </summary>
        /// <param name="arrangeSize">
        /// The <see cref="Size"/> this element uses to arrange its child content.
        /// </param>
        /// <returns>
        /// The <see cref="Size"/> that represents the arranged size of this <see cref="Decorator"/>
        /// element and its child.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (Child != null)
            {
                _pixelOffset = GetPixelOffset();
                Rect childBounds = new Rect(_pixelOffset, Child.DesiredSize);
                Child.Arrange(childBounds);
            }

            return arrangeSize;
        }
        #endregion
    }
}
