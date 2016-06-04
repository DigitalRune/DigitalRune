// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents an adorner that draws a selection rectangle.
    /// </summary>
    /// <remarks>
    /// The selection rectangle has a solid 1-pixel outline and a semitransparent fill. The color
    /// <see cref="SystemColors.HighlightColor"/> is used for outline and fill.
    /// </remarks>
    public class SelectionRectangle : Adorner
    {
        /// <summary>
        /// Gets or sets the start point of the rectangle.
        /// </summary>
        /// <value>The start point of the rectangle.</value>
        public Point Start { get; set; }


        /// <summary>
        /// Gets or sets the end point of the rectangle.
        /// </summary>
        /// <value>The end point of the rectangle.</value>
        public Point End { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionRectangle"/> class.
        /// </summary>
        /// <param name="adornedElement">The element to bind the adorner to.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="adornedElement"/> is <see langword="null"/>.
        /// </exception>
        public SelectionRectangle(UIElement adornedElement)
            : base(adornedElement)
        {
            Start = new Point(double.NaN, double.NaN);
            End = new Point(double.NaN, double.NaN);
        }


        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are
        /// directed by the layout system. The rendering instructions for this element are not used
        /// directly when this method is invoked, and are instead preserved for later asynchronous
        /// use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        /// The drawing instructions for a specific element. This context is provided to the layout
        /// system.
        /// </param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (drawingContext == null)
                throw new ArgumentNullException(nameof(drawingContext));

            if (!Numeric.IsNaN(Start.X)
                && !Numeric.IsNaN(Start.Y)
                && !Numeric.IsNaN(End.X)
                && !Numeric.IsNaN(End.Y))
            {
                var clip = new RectangleGeometry(new Rect(AdornedElement.RenderSize));
                drawingContext.PushClip(clip);

                // Define rectangle.
                var rect = new Rect
                {
                    X = Math.Min(Start.X, End.X),
                    Y = Math.Min(Start.Y, End.Y),
                    Width = Math.Abs(Start.X - End.X),
                    Height = Math.Abs(Start.Y - End.Y)
                };

                if (AdornedElement.SnapsToDevicePixels)
                {
                    var pixelSize = WindowsHelper.GetPixelSize(this);
                    rect = WindowsHelper.RoundToDevicePixelsCenter(rect, pixelSize);
                }

                // Draw a semi-transparent rectangle using the System's highlight color.
                var color = SystemColors.HighlightColor;
                var pen = new Pen(new SolidColorBrush(color), 1);
                var brush = new SolidColorBrush(Color.FromArgb(64, color.R, color.G, color.B));
                drawingContext.DrawRectangle(brush, pen, rect);

                drawingContext.Pop();
            }

            base.OnRender(drawingContext);
        }
    }
}
