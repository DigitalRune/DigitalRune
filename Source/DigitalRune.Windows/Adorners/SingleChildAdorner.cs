// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using DigitalRune.Linq;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows
{
    /// <summary>
    /// Represents an adorner that has max one <see cref="UIElement"/> as its logical/visual child.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the child element is of type <see cref="FrameworkElement"/> the properties
    /// <see cref="FrameworkElement.HorizontalAlignment"/> and
    /// <see cref="FrameworkElement.VerticalAlignment"/> define the position of the child element
    /// relative to the adorned element. For example, when
    /// <see cref="FrameworkElement.HorizontalAlignment"/> is set to
    /// <see cref="HorizontalAlignment.Center"/> the child element is centered above the adorned
    /// element using its desired size.
    /// </para>
    /// </remarks>
    public class SingleChildAdorner : Adorner
    {
        // Note: The SingleChildAdorner is similar to a Popup in Silverlight.
        // (Silverlight Popups are more like adorners with a single child than WPF Popups.)

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the child element.
        /// </summary>
        /// <value>The child element.</value>
        protected UIElement Child
        {
            get { return _child; }
            set
            {
                // ReSharper disable once PossibleUnintendedReferenceComparison
                if (_child == value)
                    return;

                // Remove previous child from logical/visual tree.
                if (_child != null)
                {
                    RemoveLogicalChild(_child);
                    RemoveVisualChild(_child);
                }

                _child = value;

                // Add child to logical/visual tree.
                if (_child != null)
                {
                    AddLogicalChild(_child);
                    AddVisualChild(_child);
                }

                Update();
            }
        }
        private UIElement _child;


        /// <summary>
        /// Gets or sets the horizontal offset of the adorner.
        /// </summary>
        /// <value>The horizontal offset of the adorner. The default value is 0.</value>
        public double HorizontalOffset
        {
            get { return _horizontalOffset; }
            set
            {
                _horizontalOffset = value;
                Update();
            }
        }
        private double _horizontalOffset;


        /// <summary>
        /// Gets or sets the vertical offset of the adorner.
        /// </summary>
        /// <value>The vertical offset of the adorner. The default value is 0.</value>
        public double VerticalOffset
        {
            get { return _verticalOffset; }
            set
            {
                _verticalOffset = value;
                Update();
            }
        }
        private double _verticalOffset;


        /// <summary>
        /// Gets an enumerator for logical child elements of this element.
        /// </summary>
        /// <value>An enumerator for logical child elements of this element.</value>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                return (Child != null)
                       ? LinqHelper.Return(Child).GetEnumerator()
                       : Enumerable.Empty<UIElement>().GetEnumerator();
            }
        }


        /// <summary>
        /// Gets the number of visual child elements within this element.
        /// </summary>
        /// <value>The number of visual child elements for this element.</value>
        protected override int VisualChildrenCount
        {
            get { return (Child != null) ? 1 : 0; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChildAdorner"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChildAdorner"/> class.
        /// </summary>
        /// <param name="adornedElement">The element to which the adorner will be bound.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="adornedElement"/> is <see langword="null"/>.
        /// </exception>
        public SingleChildAdorner(UIElement adornedElement)
            : this(adornedElement, null)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="SingleChildAdorner"/> class with the given
        /// element.
        /// </summary>
        /// <param name="adornedElement">The element to which the adorner will be bound.</param>
        /// <param name="childElement">The element that will be added to the adorner.</param>
        public SingleChildAdorner(UIElement adornedElement, UIElement childElement)
            : base(adornedElement)
        {
            Child = childElement;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Returns a <see cref="Transform"/> for the adorner, based on the transform that is
        /// currently applied to the adorned element.
        /// </summary>
        /// <param name="transform">
        /// The transform that is currently applied to the adorned element.
        /// </param>
        /// <returns>A transform to apply to the adorner.</returns>
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var baseTransform = base.GetDesiredTransform(transform);
            var localTransform = new TranslateTransform(_horizontalOffset, _verticalOffset);

            if (baseTransform != null)
            {
                // Add translation to original transform.
                var result = new GeneralTransformGroup();
                result.Children.Add(baseTransform);
                result.Children.Add(localTransform);
                return result;
            }

            return localTransform;
        }


        /// <summary>
        /// Sets the horizontal and vertical offset of the adorner.
        /// </summary>
        /// <param name="horizontalOffset">The horizontal offset of the adorner.</param>
        /// <param name="verticalOffset">The vertical offset of the adorner.</param>
        public void SetOffsets(double horizontalOffset, double verticalOffset)
        {
            _horizontalOffset = horizontalOffset;
            _verticalOffset = verticalOffset;
            Update();
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
            if (Child == null)
                return new Size();

            var adornedElement = AdornedElement as FrameworkElement;
            if (adornedElement != null)
            {
                // The constraint size of the adorner layer is typically larger than the actual size
                // of the adorned element. To ensure that a child FrameworkElement is positioned
                // correctly we need to adjust the constraint size.
                double width = adornedElement.ActualWidth;
                if (Numeric.IsPositiveFinite(width) && width < constraint.Width)
                    constraint.Width = width;

                double height = adornedElement.ActualHeight;
                if (Numeric.IsPositiveFinite(height) && height < constraint.Height)
                    constraint.Height = height;
            }

            Child.Measure(constraint);
            return constraint;
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
            if (Child != null)
            {
                Child.Arrange(new Rect(finalSize));
                return finalSize;
            }

            return new Size();
        }


        /// <summary>
        /// Overrides <see cref="GetVisualChild(System.Int32)"/> and returns a child at the
        /// specified index from a collection of child elements.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the requested child element in the collection.
        /// </param>
        /// <returns>
        /// The requested child element. This should not return <see langword="null"/>; if the
        /// provided index is out of range, an exception is thrown.
        /// </returns>
        protected override Visual GetVisualChild(int index)
        {
            return Child;
        }


        /// <summary>
        /// Forces the parent adorner layer to redraw the adorner.
        /// </summary>
        private void Update()
        {
            (Parent as AdornerLayer)?.Update(AdornedElement);
        }
        #endregion
    }
}
