// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represent an adorner that renders a <see cref="System.Windows.Controls.TextBox"/> above a
    /// <see cref="UIElement"/>.
    /// </summary>
    internal class TextBoxAdorner : Adorner
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        /// <summary>
        /// Extra space that is shown in the <see cref="System.Windows.Controls.TextBox"/>
        /// </summary>
        private const double ExtraWidth = 12;

        /// <summary>
        /// The <see cref="System.Windows.Controls.TextBox"/> is larger than the
        /// <see cref="TextBlock"/>. This value indicates the additional size that needs to be
        /// considered at each side.
        /// </summary>
        private const double TextBoxBorder = 2;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly VisualCollection _visualChildren;
        private readonly TextBox _textBox;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="System.Windows.Controls.TextBox"/>.
        /// </summary>
        public TextBox TextBox
        {
            get { return _textBox; }
        }


        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <returns>
        /// The number of visual child elements for this element.
        /// </returns>
        protected override int VisualChildrenCount
        {
            get { return _visualChildren.Count; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The element to bind the adorner to.</param>
        /// <exception cref="ArgumentNullException">
        /// Raised when adornedElement is <see langword="null"/>.
        /// </exception>
        public TextBoxAdorner(UIElement adornedElement) : base(adornedElement)
        {
            _visualChildren = new VisualCollection(this);

            _textBox = new TextBox();
            _textBox.LayoutUpdated += OnTextBoxLayoutUpdated;
            _textBox.TextChanged += OnTextBoxTextChanged;
            _visualChildren.Add(_textBox);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnTextBoxLayoutUpdated(object sender, EventArgs eventArgs)
        {
            _textBox.Focus();
        }


        private void OnTextBoxTextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            InvalidateMeasure();
        }


        /// <summary>
        /// Overrides <see cref="Visual.GetVisualChild(int)"/>, and returns a child at the 
        /// specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the requested child element in the collection.
        /// </param>
        /// <returns>
        /// The requested child element. This should not return null; if the provided index is out of
        /// range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return _visualChildren[index];
        }


        /// <summary>
        /// Implements any custom measuring behavior for the adorner.
        /// </summary>
        /// <param name="constraint">A size to constrain the adorner to.</param>
        /// <returns>
        /// A <see cref="Size"/> object representing the amount of layout space needed by the
        /// adorner.
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
            AdornedElement.Measure(constraint);
            _textBox.Measure(constraint);

            // Determine width of TextBox.
            // Minimal width is defined by the adorned element.
            // Maximal width is defined by 'constraint'.
            double width = Math.Max(AdornedElement.DesiredSize.Width + ExtraWidth, _textBox.DesiredSize.Width + ExtraWidth);
            width = Math.Min(width, constraint.Width);

            // Determine height of TextBox.
            // Minimal height is defined by the adorned element.
            // Maximal width is defined by 'constraint'.
            double height = Math.Max(AdornedElement.DesiredSize.Height + 2 * TextBoxBorder, _textBox.DesiredSize.Height);
            height = Math.Min(height, constraint.Height);
            return new Size(width, height);
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
            // Initialize left, top with a default offset.
            double left = -TextBoxBorder;
            double top = -TextBoxBorder;

            if (_textBox.DesiredSize.Height > AdornedElement.DesiredSize.Height)
            {
                // Calculate offset from size difference.
                double oversize = _textBox.DesiredSize.Height - AdornedElement.DesiredSize.Height;
                double offset = Math.Max(0, oversize / 2);
                left = 0 - offset;
                top = 0 - offset;
            }

            _textBox.Arrange(new Rect(left, top, finalSize.Width, finalSize.Height));
            return finalSize;
        }
        #endregion
    }
}
