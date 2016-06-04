// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Hosts tabs inside a <see cref="DockTabPane"/>.
    /// </summary>
    public class DockTabPanel : Panel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private double _elementHeight;
        private readonly List<double> _elementWidths = new List<double>();
        private readonly List<double> _elementWidthsBackup = new List<double>();

        // Current and previous offsets for animation.
        private readonly List<double> _offsets = new List<double>();
        private readonly List<double> _previousOffsets = new List<double>();
        private readonly List<UIElement> _previousChildren = new List<UIElement>();

        private double _scrollOffset;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="InvertZOrder"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InvertZOrderProperty = DependencyProperty.Register(
            "InvertZOrder",
            typeof(bool),
            typeof(DockTabPanel),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets a value indicating whether the Z-order of the tabs should be reversed.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the Z-order should be reversed; otherwise, 
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets the a value indicating whether the Z-order of the tabs should be reversed.")]
        [Category(Categories.Layout)]
        public bool InvertZOrder
        {
            get { return (bool)GetValue(InvertZOrderProperty); }
            set { SetValue(InvertZOrderProperty, Boxed.Get(value)); }
        }


        /// <summary>
            /// Identifies the <see cref="P:DigitalRune.Windows.Docking.DockTabPanel.IsDraggedProperty"/> attached dependency
            /// property.
            /// </summary>
            /// <AttachedPropertyComments>
            /// <summary>
            /// Gets or sets the a value indicating whether the element is currently being dragged by the user.
            /// </summary>
            /// <value>The a value indicating whether the element is currently being dragged by the user.</value>
            /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsDraggedProperty = DependencyProperty.RegisterAttached(
            "IsDragged",
            typeof(bool),
            typeof(DockTabPanel),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsDraggedPropertyChanged));


        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Docking.DockTabPanel.IsDraggedProperty"/> attached
        /// property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Docking.DockTabPanel.IsDraggedProperty"/> attached
        /// property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetIsDragged(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(IsDraggedProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Docking.DockTabPanel.IsDraggedProperty"/> attached
        /// property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static void SetIsDragged(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(IsDraggedProperty, Boxed.Get(value));
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockTabPanel"/> class.
        /// </summary>
        static DockTabPanel()
        {
            // When navigating arrow keys: Cycle through the tabs.
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DockTabPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));

            // When navigating with Tab: The tabs receive focus only once. On the next press of Tab we 
            // move the focus to the content of the DockTabItem.
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DockTabPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="P:DigitalRune.Windows.Docking.DockTabPanel.IsDraggedProperty"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsDraggedPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!(bool)eventArgs.NewValue)
            {
                // An element dragged with the mouse and was released. Dragging is done by applying
                // a TranslateTransform (see DragManager).
                // --> Animate translation from current value to 0.
                var element = dependencyObject as UIElement;
                if (element != null)
                    AnimateOffset(element, null, 0, 125);
            }
        }


        /// <summary>
        /// Returns a geometry for a clipping mask. The mask applies if the layout system attempts
        /// to arrange an element that is larger than the available display space. (Always returns
        /// <see langword="null"/>.)
        /// </summary>
        /// <param name="layoutSlotSize">
        /// The size of the part of the element that does visual presentation.
        /// </param>
        /// <returns>
        /// The clipping geometry. <see cref="DockTabPanel"/> always returns
        /// <see langword="null"/>.
        /// </returns>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            // DockTabItem may have negative margin to draw outside the panel.
            return null;
        }


        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child
        /// elements and determines a size for the <see cref="FrameworkElement"/>-derived class.
        /// </summary>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements. Infinity can be
        /// specified as a value to indicate that the element will size to whatever content is
        /// available.
        /// </param>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations
        /// of child element sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            int numberOfElements = Children.Count;

            // ----- Update z-index.
            for (int i = 0; i < numberOfElements; i++)
            {
                // Set z-index of element.
                var element = Children[i];
                bool isSelected = (bool)element.GetValue(DockTabItem.IsSelectedProperty);
                if (isSelected)
                {
                    SetZIndex(element, numberOfElements);
                }
                else
                {
                    if (InvertZOrder)
                        SetZIndex(element, numberOfElements - 1 - i);
                    else
                        SetZIndex(element, i);
                }
            }

            // ----- Measure elements granting them the max available size.
            for (int i = 0; i < numberOfElements; i++)
            {
                var element = Children[i];
                if (element.Visibility == Visibility.Collapsed)
                    continue;

                var dockTabItem = element as DockTabItem;
                if (dockTabItem != null && dockTabItem.IsTabWidthFixed)
                {
                    // DockTabItem.IsTabWidthFixed is set, which prevents shrinking.
                    element.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                }
                else
                {
                    element.Measure(availableSize);
                }
            }

            bool remeasure = ComputeElementSizes(Children, availableSize);
            if (remeasure)
            {
                // Re-measure elements.
                for (int i = 0; i < numberOfElements; i++)
                    Children[i]?.Measure(new Size(_elementWidths[i], availableSize.Height));
            }

            double width = Math.Min(_elementWidths.Sum(), availableSize.Width);
            return new Size(width, _elementHeight);
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
            int numberOfElements = Children.Count;

            if (numberOfElements > 0)
            {
                // Get index of last selected item.
                int indexOfSelected = -1;
                for (int i = 0; i < numberOfElements; i++)
                {
                    var element = Children[i];
                    bool isSelected = (bool)element.GetValue(DockTabItem.IsSelectedProperty);
                    if (isSelected)
                    {
                        indexOfSelected = i;
                    }
                }

                // ----- Calculate the x-offsets of all elements.
                double x = 0.0;
                _offsets.Clear();
                for (int i = 0; i < numberOfElements; i++)
                {
                    _offsets.Add(x);
                    x += _elementWidths[i];
                }

                // ----- Scroll offset
                // Keep scroll offset within allowed range.
                double maxScrollOffset = _offsets[numberOfElements - 1] + _elementWidths[numberOfElements - 1]
                                         - finalSize.Width;
                if (_scrollOffset > maxScrollOffset)
                    _scrollOffset = maxScrollOffset;

                // Ensure that the selected element is always visible.
                if (indexOfSelected >= 0)
                {
                    double width = _elementWidths[indexOfSelected];
                    double left = _offsets[indexOfSelected];
                    double right = left + width;
                    if (width >= finalSize.Width)
                        _scrollOffset = left;
                    else if (left < _scrollOffset)
                        _scrollOffset = left;
                    else if (right - _scrollOffset > finalSize.Width)
                        _scrollOffset = right - finalSize.Width;
                }

                // Apply scroll offset.
                if (_scrollOffset >= 0)
                    for (int i = 0; i < numberOfElements; i++)
                        _offsets[i] -= _scrollOffset;

                // ----- Arrange elements.
                for (int i = 0; i < numberOfElements; i++)
                {
                    var element = Children[i];
                    if (element.Visibility == Visibility.Collapsed)
                        continue;

                    Rect bounds = new Rect(_offsets[i], 0, _elementWidths[i], _elementHeight);
                    element.Arrange(bounds);
                }

                // ----- Animate elements.
                for (int i = 0; i < numberOfElements; i++)
                {
                    var element = Children[i];
                    if (GetIsDragged(element))
                        continue;

                    int previousIndex = _previousChildren.IndexOf(element);
                    if (previousIndex >= 0 && previousIndex != i)
                        AnimateOffset(element, _previousOffsets[previousIndex] - _offsets[i], 0, 250);
                }

                _previousChildren.Clear();
                _previousOffsets.Clear();
                for (int i = 0; i < numberOfElements; i++)
                {
                    var element = Children[i];
                    _previousChildren.Add(element);
                    _previousOffsets.Add(_offsets[i]);
                }

                Clip = new RectangleGeometry { Rect = new Rect(0, 0, finalSize.Width + 12, finalSize.Height) };
            }
            else
            {
                Clip = null;
            }

            return finalSize;
        }


        /// <summary>
        /// Calculates the sizes of all elements in the panel.
        /// </summary>
        /// <param name="elements">The elements.</param>
        /// <param name="availableSize">The available size.</param>
        /// <returns>
        /// <see langword="true"/> if some elements need to be shrunken or cut-off; otherwise,
        /// <see langword="false"/> if all elements can be shown with their desired size.
        /// </returns>
        private bool ComputeElementSizes(UIElementCollection elements, Size availableSize)
        {
            int numberOfElements = elements.Count;
            bool[] isFixed = new bool[numberOfElements]; // DockTabItem have a flag IsTabWidthFixed, which prevents shrinking.
            double totalDesiredWidth = 0.0;              // The total size requested by the elements.
            double totalMinWidth = 0.0;                  // The min required size.

            _elementHeight = 0.0;
            for (int i = 0; i < numberOfElements; i++)
                _elementWidths.Add(0);

            // ----- Determine totalDesiredWidth and totalMinWidth.
            for (int i = 0; i < numberOfElements; i++)
            {
                var element = elements[i];
                if (element.Visibility == Visibility.Collapsed)
                    continue;

                Size desiredSize = element.DesiredSize;
                _elementWidths[i] = desiredSize.Width;
                if (_elementHeight < desiredSize.Height)
                    _elementHeight = desiredSize.Height;

                totalDesiredWidth += desiredSize.Width;

                var frameworkElement = element as FrameworkElement;
                var dockTabItem = element as DockTabItem;
                if (dockTabItem != null && dockTabItem.IsTabWidthFixed)
                {
                    // DockTabItem.IsTabWidthFixed is set, which prevents shrinking: MinWidth = DesiredSize.Width
                    isFixed[i] = true;
                    totalMinWidth += desiredSize.Width;
                }
                else if (frameworkElement != null)
                {
                    totalMinWidth += frameworkElement.MinWidth;
                }
            }

            // ----- Shrink items if necessary.
            bool remeasure;
            if (totalDesiredWidth <= availableSize.Width)
            {
                // ----- No shrinking:
                // Each items gets its desired size. 
                remeasure = false;
            }
            else if (totalMinWidth <= availableSize.Width)
            {
                // ----- Shrink items until they fit into the available space.        

                // The total desired size of all tabs is: totalDesiredWidth.
                // After the following loop totalDesiredWith will only contain the sum of 
                // desired sizes of elements which are NOT at their min-limit.

                // Store the sum of the limited children with in limitedWidth.
                double limitedWidth = 0;
                for (int i = 0; i < numberOfElements; i++)
                {
                    if (isFixed[i])
                    {
                        totalDesiredWidth -= _elementWidths[i];
                        limitedWidth += _elementWidths[i];
                        continue;
                    }

                    var element = elements[i];
                    if (element.Visibility == Visibility.Collapsed)
                        continue;

                    var frameworkElement = element as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        var minWidth = frameworkElement.MinWidth;
                        if (_elementWidths[i] <= minWidth)
                        {
                            totalDesiredWidth -= minWidth;
                            limitedWidth += minWidth;
                        }
                    }
                }

                // The available size for the unlimited children is:
                double availableWidth = availableSize.Width - limitedWidth;

                // Backup the original widths because we may need to backtrack.
                _elementWidthsBackup.Clear();
                _elementWidthsBackup.AddRange(_elementWidths);

                // Shrink everything proportionally but not below the min size.
                for (int i = 0; i < numberOfElements; i++)
                {
                    if (isFixed[i])
                        continue;

                    var element = elements[i];
                    if (element.Visibility == Visibility.Collapsed)
                        continue;

                    var frameworkElement = element as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        var minWidth = frameworkElement.MinWidth;
                        if (_elementWidths[i] > minWidth)
                        {
                            // Shrink width proportionally.
                            _elementWidths[i] *= availableWidth / totalDesiredWidth;
                            if (_elementWidths[i] <= minWidth)
                            {
                                // Set tab to minimum and restart the shrinking process.
                                isFixed[i] = true;
                                totalDesiredWidth -= minWidth;
                                limitedWidth += minWidth;
                                availableWidth = availableSize.Width - limitedWidth;
                                _elementWidthsBackup[i] = minWidth;
                                _elementWidths.Clear();
                                _elementWidths.AddRange(_elementWidthsBackup);
                                i = -1; // Restart!
                            }
                        }
                    }
                }

                remeasure = true;
            }
            else // (totalMinWidth > availableSize.Width)
            {
                // Shrink items to minimum.
                for (int i = 0; i < numberOfElements; i++)
                {
                    if (isFixed[i])
                        continue;

                    var element = elements[i];
                    if (element.Visibility == Visibility.Collapsed)
                        continue;

                    var frameworkElement = element as FrameworkElement;
                    if (frameworkElement != null)
                        _elementWidths[i] = frameworkElement.MinWidth;
                }

                remeasure = true;
            }

            return remeasure;
        }


        private static void AnimateOffset(UIElement element, double? from, double? to, double milliseconds)
        {
            var translateTransform = element.RenderTransform as TranslateTransform;
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform();
                element.RenderTransform = translateTransform;
            }

            var doubleAnimation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(milliseconds)),
                FillBehavior = FillBehavior.Stop,
                EasingFunction = new CubicEase()
            };
            translateTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation, HandoffBehavior.SnapshotAndReplace);
        }
        #endregion
    }
}
