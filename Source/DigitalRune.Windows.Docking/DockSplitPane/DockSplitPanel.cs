// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a <see cref="Panel"/> that arranges its children horizontally or vertically.
    /// </summary>
    [StyleTypedProperty(Property = "SplitterStyle", StyleTargetType = typeof(DockPaneSplitter))]
    public class DockSplitPanel : Panel
    {
        // We call the dimension where the children are distributed the "primary size/dimension".
        // The other size is the "secondary size/dimension".
        // Example: Orientation = Horizontal --> Width = Primary Size, Height = Secondary Size


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        double[] _minSizes;
        private double[] _finalSizes;
        private double _splitterSize;
        private Orientation _splitterOrientation;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the final arrange sizes of the child elements.
        /// </summary>
        internal double[] FinalSizes
        {
            get { return _finalSizes; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <inheritdoc cref="DockSplitPane.Orientation"/>
        public static readonly DependencyProperty OrientationProperty = DockSplitPane.OrientationProperty.AddOwner(
            typeof(DockSplitPanel),
            new FrameworkPropertyMetadata(
                Orientation.Horizontal,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <inheritdoc cref="DockSplitPane.Orientation"/>
        [Description("Gets or sets the split orientation.")]
        [Category(Categories.Layout)]
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SplitterPanel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SplitterPanelProperty = DependencyProperty.Register(
            "SplitterPanel",
            typeof(Panel),
            typeof(DockSplitPanel),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the panel above the DockSplitPanel where the DockPaneSplitters are added.
        /// This is a dependency property.
        /// </summary>
        /// <value>The panel above the DockSplitPanel where the DockPaneSplitters are added.</value>
        [Description("Gets or sets the panel above the DockSplitPanel where the DockPaneSplitters are added.")]
        [Category(Categories.Default)]
        public Panel SplitterPanel
        {
            get { return (Panel)GetValue(SplitterPanelProperty); }
            set { SetValue(SplitterPanelProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SplitterSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SplitterSizeProperty = DependencyProperty.Register(
            "SplitterSize",
            typeof(double),
            typeof(DockSplitPanel),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the size of the splitter between panes.
        /// This is a dependency property.
        /// </summary>
        /// <value>The size of the splitter between panes.</value>
        [Description("Gets or sets the size of the splitter between panes.")]
        [Category(Categories.Layout)]
        public double SplitterSize
        {
            get { return (double)GetValue(SplitterSizeProperty); }
            set { SetValue(SplitterSizeProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="DockSplitPanel"/> class.
        /// </summary>
        static DockSplitPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockSplitPanel), new FrameworkPropertyMetadata(typeof(DockSplitPanel)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, measures the size in layout required for child elements 
        /// and determines a size for the <see cref="FrameworkElement"/>-derived class.
        /// </summary>
        /// <param name="availableSize">
        /// The available size that this element can give to child elements. Infinity can be specified 
        /// as a value to indicate that the element will size to whatever content is available.
        /// </param>
        /// <returns>
        /// The size that this element determines it needs during layout, based on its calculations of 
        /// child element sizes.
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // Re-create splitter if necessary.
            bool splittersChanged = CreateSplitters();

            // Check whether layout is invalid.
            if (!splittersChanged && availableSize == DesiredSize)
            {
                bool remeasure = false;
                for (int i = 0; i < Children.Count; i++)
                {
                    if (!Children[i].IsMeasureValid)
                    {
                        remeasure = true;
                        break;
                    }
                }

                if (!remeasure)
                {
                    // Layout still valid.
                    return availableSize;
                }
            }

            ValidateDockSizes();

            // Measure children with the full available size to get the desired sizes.
            for (int i = 0; i < Children.Count; i++)
                Children[i].Measure(availableSize);

            var orientation = Orientation;
            double primaryAvailableSize = GetPrimarySize(availableSize, orientation);
            double secondaryAvailableSize = GetSecondarySize(availableSize, orientation);
            bool primaryAvailableSizeIsFinite = Numeric.IsFinite(primaryAvailableSize);
            bool secondaryAvailableSizeIsFinite = Numeric.IsFinite(secondaryAvailableSize);

            // Compute secondary size:
            // We use the largest desired size.
            double secondarySize = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                double childSize = GetSecondarySize(Children[i].DesiredSize, orientation);
                if (childSize > secondarySize)
                    secondarySize = childSize;
            }

            // Compute primary size:
            // We use the sum of the desired sizes.
            // If we have a *-sized child, we use all available space (if it is finite).
            double primarySize = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                double childDesiredSize = GetPrimarySize(child.DesiredSize, orientation);
                GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                if (childDockSize.IsAbsolute)
                {
                    primarySize += childDockSize.Value;
                }
                else if (childDockSize.IsStar && primaryAvailableSizeIsFinite)
                {
                    primarySize = primaryAvailableSize;
                    break; // Abort loop!
                }
                else
                {
                    primarySize += childDesiredSize;
                }
            }

            ComputeFinalSizes(availableSize);

            // Add size of splitters.
            if (Children.Count > 1)
                primarySize += _splitterSize * Children.Count - 1;

            // Re-measure children. If we do not call measure with the smaller size, then they are
            // arranged to the larger size - no matter with what parameters we call Arrange()!
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                if (Numeric.IsNaN(_finalSizes[i]))
                    continue;

                if (orientation == Orientation.Horizontal)
                    child.Measure(new Size(_finalSizes[i], availableSize.Height));
                else
                    child.Measure(new Size(availableSize.Width, _finalSizes[i]));
            }

            Debug.Assert(!Numeric.IsNaN(primarySize + secondarySize), "primarySize and secondarySize must not be NaN.");

            // Ensure that we do not use more space than is available.
            if (primaryAvailableSizeIsFinite)
                primarySize = Math.Min(primarySize, primaryAvailableSize);
            if (secondaryAvailableSizeIsFinite)
                secondarySize = Math.Min(secondarySize, secondaryAvailableSize);

            return (orientation == Orientation.Horizontal)
                   ? new Size(primarySize, secondarySize)
                   : new Size(secondarySize, primarySize);
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
            ComputeFinalSizes(finalSize);

            // The splitters are positioned in a panel above the DockSplitPanel.
            var splitterPanel = SplitterPanel;
            var splitters = splitterPanel?.Children;
            Debug.Assert(splitters == null || Children.Count <= 1 || splitters.Count == Children.Count - 1, "Invalid number of splitters.");

            // Arrange children and position splitters.
            double offset = 0;
            if (Orientation == Orientation.Horizontal)
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var child = Children[i] as FrameworkElement;
                    child?.Arrange(new Rect(offset, 0, _finalSizes[i], finalSize.Height));

                    offset += _finalSizes[i];

                    if (splitters != null && i < splitters.Count)
                    {
                        var splitter = splitters[i] as FrameworkElement;
                        if (splitter != null)
                            splitter.Margin = new Thickness(offset, 0, 0, 0);
                    }

                    offset += _splitterSize;
                }
            }
            else
            {
                for (int i = 0; i < Children.Count; i++)
                {
                    var child = Children[i] as FrameworkElement;
                    child?.Arrange(new Rect(0, offset, finalSize.Width, _finalSizes[i]));

                    offset += _finalSizes[i];

                    if (splitters != null && i < splitters.Count)
                    {
                        var splitter = splitters[i] as FrameworkElement;
                        if (splitter != null)
                            splitter.Margin = new Thickness(0, offset, 0, 0);
                    }

                    offset += _splitterSize;
                }
            }

            // We use all available space: actualSize = finalSize.
            return finalSize;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
        private void ComputeFinalSizes(Size fullSize)
        {
            var orientation = Orientation;
            int numberOfChildren = Children.Count;
            int numberOfSplitters = numberOfChildren - 1;

            if (numberOfChildren == 0)
            {
                // Abort if there are no children.
                _finalSizes = null;
                return;
            }

            // Reset splitter size.
            _splitterSize = SplitterSize;

            // Clear old size info.
            if (_finalSizes == null || _finalSizes.Length != numberOfChildren)
            {
                _minSizes = new double[numberOfChildren];
                _finalSizes = new double[numberOfChildren];
            }
            else
            {
                Array.Clear(_minSizes, 0, _minSizes.Length);
                Array.Clear(_finalSizes, 0, _finalSizes.Length);
            }

            // Compute min sizes.
            for (int i = 0; i < numberOfChildren; i++)
            {
                var child = Children[i] as FrameworkElement;
                if (child != null)
                    _minSizes[i] = (orientation == Orientation.Horizontal) ? child.MinWidth : child.MinHeight;
            }

            double minSizeSum = Sum(_minSizes);               // Children
            minSizeSum += numberOfSplitters * _splitterSize;  // Splitters

            if (minSizeSum > GetPrimarySize(fullSize, orientation))
            {
                // ----- Total min limit reached!
                // The total min size is larger than the available area.
                // --> Each child gets a weighted part.
                for (int i = 0; i < numberOfChildren; i++)
                {
                    var child = Children[i] as FrameworkElement;
                    if (child != null)
                        _finalSizes[i] = _minSizes[i] / minSizeSum * GetPrimarySize(fullSize, orientation);
                }

                _splitterSize = _splitterSize / minSizeSum * GetPrimarySize(fullSize, orientation);
            }
            else
            {
                // ---- We have at least enough space for the min sizes.

                // Store the desired sizes in FinalSizes.
                double totalNonStarSize = 0;        // Sum up the total size of non-*-elements.
                double totalMinStarSize = 0;        // Sum up the min size of *-elements. 
                for (int i = 0; i < numberOfChildren; i++)
                {
                    var child = Children[i] as FrameworkElement;
                    if (child != null)
                    {
                        GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                        if (childDockSize.IsAbsolute)
                        {
                            // Absolute pixel size: Use the DockWidth/DockHeight size.
                            _finalSizes[i] = GetPrimaryDockSize(child, orientation).Value;
                            totalNonStarSize += _finalSizes[i];
                        }
                        else if (childDockSize.IsStar)
                        {
                            // * size: Use the MinWidth/MinHeight and compute a better result later.
                            _finalSizes[i] = _minSizes[i];
                            totalMinStarSize += _finalSizes[i];
                        }
                        else
                        {
                            // Auto size: Use the DesiredSize of the child.
                            _finalSizes[i] = GetPrimarySize(child.DesiredSize, orientation);
                            totalNonStarSize += _finalSizes[i];
                        }
                    }
                }

                // Add size for splitters.
                totalNonStarSize += numberOfSplitters * _splitterSize;

                if (totalMinStarSize + totalNonStarSize <= GetPrimarySize(fullSize, orientation))
                {
                    #region ----- There is enough room for the * children. -----
                    // The non-* children use their optimal size.

                    // The non-* children use this total size:
                    double limitedChildrenSize = totalNonStarSize;

                    // Compute the sum of all star-values.
                    double starValueSum = 0;
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        FrameworkElement child = Children[i] as FrameworkElement;
                        GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                        if (child != null && childDockSize.IsStar)
                        {
                            GridLength starValue = GetPrimaryDockSize(child, orientation);
                            starValueSum += starValue.Value;
                        }
                    }

                    // Compute width of *-children.
                    // This size is available to distribute under the *-elements:
                    double availableSize = GetPrimarySize(fullSize, orientation) - limitedChildrenSize;
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        FrameworkElement child = Children[i] as FrameworkElement;
                        GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                        if (child != null && childDockSize.IsStar)
                        {
                            _finalSizes[i] = childDockSize.Value / starValueSum * availableSize;

                            // Do not use a size below the MinWidth/MinHeight.
                            if (_finalSizes[i] <= _minSizes[i])
                                _finalSizes[i] = _minSizes[i];
                        }
                    }

                    // Now recompute the starValueSum for all non-limited *-elements.
                    // Add the sizes of the limited elements to limitedChildrenSize.
                    starValueSum = 0;
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        FrameworkElement child = Children[i] as FrameworkElement;
                        GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                        if (child != null && childDockSize.IsStar)
                        {
                            if (childDockSize.IsStar && _finalSizes[i] > _minSizes[i])
                                starValueSum += childDockSize.Value;
                            else
                                limitedChildrenSize += _finalSizes[i];
                        }
                    }

                    // If min size limits were reached, the computed results are not correct. Keep
                    // computing until no min limits are reached anymore.
                    availableSize = GetPrimarySize(fullSize, orientation) - limitedChildrenSize;

                    // This loop finalizes all elements that are at their min limit.
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        UIElement child = Children[i];
                        GridLength childDockSize = GetPrimaryDockSize(child, orientation);
                        if (child != null && childDockSize.IsStar && _finalSizes[i] > _minSizes[i])
                        {
                            double weight = childDockSize.Value / starValueSum;
                            _finalSizes[i] = weight * availableSize;

                            if (_finalSizes[i] <= _minSizes[i])
                            {
                                _finalSizes[i] = _minSizes[i];

                                starValueSum -= childDockSize.Value;
                                limitedChildrenSize += _minSizes[i];
                                i = 0;
                                availableSize = GetPrimarySize(fullSize, orientation) - limitedChildrenSize;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    #region ----- The * children are at their minimum and the other sizes must shrink too. -----

                    // The *-sized elements use their min size.
                    // Of the others a few - but no all - must shrink too.
                    // Nothing is below its min size limit.

                    // Compute the sum size of all elements which are at their min limit:
                    double limitedChildrenSize = 0;
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        var child = Children[i] as FrameworkElement;
                        if (child != null)
                        {
                            if (_finalSizes[i] <= _minSizes[i])
                            {
                                _finalSizes[i] = _minSizes[i];  // Set to exact min limit to avoid numerical errors.
                                limitedChildrenSize += _minSizes[i];
                            }
                        }
                    }

                    // The shrinkable children desire this size:
                    double unlimitedChildrenDesiredSize = Sum(_finalSizes) - limitedChildrenSize;

                    // Shrink everything proportionally but not below the min size.
                    double availableSize = GetPrimarySize(fullSize, orientation) - limitedChildrenSize - numberOfSplitters * _splitterSize;

                    // This loop finalizes all elements that are at their min limit.
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        var child = Children[i] as FrameworkElement;
                        if (child != null && _finalSizes[i] > _minSizes[i])
                        {
                            double weight = _finalSizes[i] / unlimitedChildrenDesiredSize;
                            double size = weight * availableSize;

                            if (size <= _minSizes[i])
                            {
                                _finalSizes[i] = _minSizes[i];
                                unlimitedChildrenDesiredSize -= _minSizes[i];
                                limitedChildrenSize += _minSizes[i];

                                // Restart shrinking process!
                                i = 0;
                                availableSize = GetPrimarySize(fullSize, orientation) - limitedChildrenSize - numberOfSplitters * _splitterSize;
                            }
                        }
                    }
                    // This loop finalizes all the other elements.
                    for (int i = 0; i < numberOfChildren; i++)
                    {
                        var child = Children[i] as FrameworkElement;
                        if (child != null && _finalSizes[i] > _minSizes[i])
                        {
                            double weight = _finalSizes[i] / unlimitedChildrenDesiredSize;
                            _finalSizes[i] = weight * availableSize;
                            Debug.Assert(_finalSizes[i] >= _minSizes[i]);
                        }
                    }
                    #endregion
                }
            }

            bool roundSizes = SnapsToDevicePixels || UseLayoutRounding;
            if (roundSizes)
            {
                for (int i = 0; i < numberOfChildren; i++)
                    _finalSizes[i] = Math.Round(_finalSizes[i], MidpointRounding.AwayFromZero);
            }

            // If we have no *-sized children, we do not use all available space.
            // Therefore, we always give all the available space to the last child.
            _finalSizes[numberOfChildren - 1] = 0;

            // Lower limit is 0. Due to numerical errors a negative size like "-0.00000012" could be the result.
            double remainingSize = GetPrimarySize(fullSize, orientation) - Sum(_finalSizes) - numberOfSplitters * _splitterSize;

            _finalSizes[numberOfChildren - 1] = Math.Max(0, remainingSize);
        }


        /// <summary>
        /// (Re-)Creates the splitters.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if splitters have changed; otherwise, <see langword="false"/>.
        /// </returns>
        private bool CreateSplitters()
        {
            var splitterPanel = SplitterPanel;
            if (splitterPanel == null)
                return false;

            var orientation = Orientation;
            int numberOfSplitters = Math.Max(0, Children.Count - 1);
            if (orientation == _splitterOrientation && numberOfSplitters == splitterPanel.Children.Count)
                return false;

            // Remove existing splitters.
            splitterPanel.Children.Clear();

            // Create new splitters.
            _splitterOrientation = orientation;
            for (int i = 0; i < numberOfSplitters; i++)
            {
                var splitter = new DockPaneSplitter(this, i);
                if (orientation == Orientation.Horizontal)
                {
                    splitter.HorizontalAlignment = HorizontalAlignment.Left;
                    splitter.Width = SplitterSize;
                    splitter.Height = double.NaN;
                    splitter.Cursor = Cursors.SizeWE;
                }
                else
                {
                    splitter.VerticalAlignment = VerticalAlignment.Top;
                    splitter.Width = double.NaN;
                    splitter.Height = SplitterSize;
                    splitter.Cursor = Cursors.SizeNS;
                }

                splitterPanel.Children.Add(splitter);
            }

            return true;
        }


        /// <summary>
        /// Gets the primary DockWidth/DockHeight value for the given element.
        /// </summary>
        private static GridLength GetPrimaryDockSize(DependencyObject element, Orientation orientation)
        {
            if (element == null)
                return GridLength.Auto;

            return (orientation == Orientation.Vertical) ? DockControl.GetDockHeight(element) : DockControl.GetDockWidth(element);
        }


        /// <summary>
        /// Gets the primary length of the given <see cref="Size"/>.
        /// </summary>
        private static double GetPrimarySize(Size size, Orientation orientation)
        {
            return (orientation == Orientation.Vertical) ? size.Height : size.Width;
        }


        /// <summary>
        /// Gets the secondary length of the given <see cref="Size"/>.
        /// </summary>
        private static double GetSecondarySize(Size size, Orientation orientation)
        {
            return (orientation == Orientation.Horizontal) ? size.Height : size.Width;
        }


        /// <summary>
        /// Validates and corrects the DockWidth/DockHeight properties of the children.
        /// </summary>
        private void ValidateDockSizes()
        {
            // Make all sizes positive.
            // Absolute sizes must not be less than the MinWidth/MinHeight values.
            foreach (var child in Children)
            {
                var element = child as FrameworkElement;
                if (element == null)
                    continue;

                var width = DockControl.GetDockWidth(element);
                if (width.IsAbsolute && width.Value < element.MinWidth)
                    DockControl.SetDockWidth(element, new GridLength(element.MinWidth, width.GridUnitType));
                else if (width.Value < 0)
                    DockControl.SetDockWidth(element, new GridLength(0, width.GridUnitType));

                var height = DockControl.GetDockHeight(element);
                if (height.IsAbsolute && height.Value < element.MinHeight)
                    DockControl.SetDockHeight(element,new GridLength(element.MinHeight, height.GridUnitType));
                else if (height.Value < 0)
                    DockControl.SetDockHeight(element, new GridLength(0, height.GridUnitType));
            }
        }


        private static double Sum(double[] values)
        {
            double sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += values[i];

            return sum;
        }
        #endregion
    }
}
