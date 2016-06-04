// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a control that changes the size of the elements in a <see cref="DockSplitPane"/>.
    /// </summary>
    /// <remarks>
    /// A <see cref="DockPaneSplitter"/> is automatically inserted between the elements of a
    /// <see cref="DockSplitPane"/>. Dragging the control resizes the adjacent elements.
    /// Double-clicking the control resets the position of the splitter.
    /// </remarks>
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    public class DockPaneSplitter : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly DockSplitPanel _panel;
        private readonly int _index;

        private bool _isDragging;

        // Mouse position (x or y) relative to DockSplitPanel.
        private double _startMousePosition;

        // Original length of the elements before and after the splitter.
        private double _actualLengthOfPrevious;
        private double _actualLengthOfNext;
        private GridLength _gridLengthOfPrevious;
        private GridLength _gridLengthOfNext;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockPaneSplitter"/> class.
        /// </summary>
        static DockPaneSplitter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPaneSplitter), new FrameworkPropertyMetadata(typeof(DockPaneSplitter)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockPaneSplitter"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="index">The index.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="owner"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is negative.
        /// </exception>
        public DockPaneSplitter(DockSplitPanel owner, int index)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must not be negative.");

            _panel = owner;
            _index = index;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UpdateVisualStates(false);
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Mouse.MouseEnter</strong> attached event is raised on this 
        /// element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Mouse.MouseLeave</strong> attached event is raised on this 
        /// element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseLeftButtonDown"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (!e.Handled)
                e.Handled = BeginDrag(e);
        }


        private bool BeginDrag(MouseEventArgs eventArgs)
        {
            if (_isDragging)
                return false;

            bool mouseCaptured = CaptureMouse();
            if (mouseCaptured)
            {
                _isDragging = true;

                // Get start mouse position relative to panel.
                bool isHorizontal = (_panel.Orientation == Orientation.Horizontal);
                var mousePosition = eventArgs.GetPosition(_panel);
                _startMousePosition = isHorizontal ? mousePosition.X : mousePosition.Y;

                // Register event handlers.
                MouseLeftButtonUp += OnMouseLeftButtonUp;
                MouseMove += OnMouseMove;
                KeyDown += OnKeyDown;
                LostMouseCapture += OnLostMouseCapture;

                // Focus element to receive keyboard input.
                Focus();

                // Store original lengths of the elements (for rollback).
                FrameworkElement previousElement, nextElement;
                GetAdjacentElements(out previousElement, out nextElement);

                _actualLengthOfPrevious = _panel.FinalSizes[_index];
                _actualLengthOfNext = _panel.FinalSizes[_index + 1];
                _gridLengthOfPrevious = GetDockSize(previousElement, isHorizontal);
                _gridLengthOfNext = GetDockSize(nextElement, isHorizontal);

                return true;
            }

            return false;
        }


        private void EndDrag(bool commit)
        {
            if (!_isDragging)
                return;

            _isDragging = false;

            // Unregister event handlers.
            MouseLeftButtonUp -= OnMouseLeftButtonUp;
            MouseMove -= OnMouseMove;
            KeyDown -= OnKeyDown;
            LostMouseCapture -= OnLostMouseCapture;

            ReleaseMouseCapture();

            if (!commit)
            {
                // Rollback: Restore original GridLengths.
                bool isHorizontal = (_panel.Orientation == Orientation.Horizontal);

                FrameworkElement previousElement, nextElement;
                GetAdjacentElements(out previousElement, out nextElement);
                SetDockSize(previousElement, _gridLengthOfPrevious, isHorizontal);
                SetDockSize(nextElement, _gridLengthOfNext, isHorizontal);
            }
        }


        private void OnKeyDown(object sender, KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Escape)
            {
                EndDrag(false);
                eventArgs.Handled = true;
            }
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            EndDrag(false);
            eventArgs.Handled = true;
        }


        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            EndDrag(true);
            eventArgs.Handled = true;
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            // Get orientation of pane.
            bool isHorizontal = (_panel.Orientation == Orientation.Horizontal);

            // Get mouse position relative to pane.
            Point mousePosition = eventArgs.GetPosition(_panel);

            // The absolute change of the mouse position.
            double change = isHorizontal
                            ? mousePosition.X - _startMousePosition
                            : mousePosition.Y - _startMousePosition;

            // Abort if mouse has not moved in the primary direction.
            if (Numeric.IsZero(change))
                return;

            // Get neighbors of the splitter in the pane.
            FrameworkElement previousElement, nextElement;
            GetAdjacentElements(out previousElement, out nextElement);

            // Get min sizes.
            double minLengthOfPrevious = isHorizontal ? previousElement.MinWidth : previousElement.MinHeight;
            double minLengthOfNext = isHorizontal ? nextElement.MinWidth : nextElement.MinHeight;

            // Limit change by min limits.
            if (_actualLengthOfPrevious + change < minLengthOfPrevious)
                change = minLengthOfPrevious - _actualLengthOfPrevious;

            if (_actualLengthOfNext - change < minLengthOfNext)
                change = _actualLengthOfNext - minLengthOfNext;

            // For absolute- or auto-sized elements, set the new absolute size.
            if (!_gridLengthOfPrevious.IsStar)
            {
                var gridLength = new GridLength(_actualLengthOfPrevious + change, GridUnitType.Pixel);
                SetDockSize(previousElement, gridLength, isHorizontal);
            }

            if (!_gridLengthOfNext.IsStar)
            {
                var gridLength = new GridLength(_actualLengthOfNext - change, GridUnitType.Pixel);
                SetDockSize(nextElement, gridLength, isHorizontal);
            }

            // If both neighbors are *-sized, compute correct coefficients for the *-values of both
            // neighbors.
            if (_gridLengthOfPrevious.IsStar && _gridLengthOfNext.IsStar)
            {
                // Compute the "size" of "1*". 
                // We compute "1*" of both neighbors. If one is limited or one is 0 length (-->NaN!),
                // we take the more useful value.
                double starSizeOfPrevious = _actualLengthOfPrevious / _gridLengthOfPrevious.Value;
                double starSizeOfNext = _actualLengthOfNext / _gridLengthOfNext.Value;
                double starSize = starSizeOfPrevious;
                if (Numeric.IsNaN(starSizeOfPrevious) || starSizeOfPrevious < starSizeOfNext)
                    starSize = starSizeOfNext;

                // Compute the new *-sizes.
                var newPreviousLength = new GridLength((_actualLengthOfPrevious + change) / starSize, GridUnitType.Star);
                var newNextLength = new GridLength((_actualLengthOfNext - change) / starSize, GridUnitType.Star);

                // Set the new sizes.
                SetDockSize(previousElement, newPreviousLength, isHorizontal);
                SetDockSize(nextElement, newNextLength, isHorizontal);
            }

            _panel.InvalidateMeasure();
        }


        /// <summary>
        /// Raises the <see cref="Control.MouseDoubleClick"/> routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (!e.Handled)
            {
                // If a panel has an absolute size value, set the size to "Auto".
                // If there are two *-sized panels, then set the size values to the average.

                // Get orientation of pane.
                bool isHorizontal = (_panel.Orientation == Orientation.Horizontal);

                // Get neighbors of the splitter in the pane.
                FrameworkElement previousElement, nextElement;
                GetAdjacentElements(out previousElement, out nextElement);

                // Get length info.
                GridLength previousLength = GetDockSize(previousElement, isHorizontal);
                GridLength nextLength = GetDockSize(nextElement, isHorizontal);

                // If an element has an absolute size, set the size to "Auto".
                if (!previousLength.IsAbsolute && nextLength.IsAbsolute)
                {
                    SetDockSize(nextElement, GridLength.Auto, isHorizontal);
                }
                else if (previousLength.IsAbsolute && !nextLength.IsAbsolute)
                {
                    SetDockSize(previousElement, GridLength.Auto, isHorizontal);
                }
                else if (previousLength.IsAbsolute && nextLength.IsAbsolute)
                {
                    // If both are absolute, set only the element to "Auto" which is further
                    // away from the *-sized elements. 

                    // Get index of the first *-sized element.
                    int starIndex;

                    for (starIndex = 0; starIndex < _panel.Children.Count; starIndex++)
                    {
                        UIElement element = _panel.Children[starIndex];
                        if (GetDockSize(element, isHorizontal).IsStar)
                            break;
                    }

                    var autoElement = (_index < starIndex) ? previousElement : nextElement;
                    SetDockSize(autoElement, GridLength.Auto, isHorizontal);
                }

                // If the two neighbors are *-sized, make them equal.
                if (previousLength.IsStar && nextLength.IsStar)
                {
                    // Compute average length.
                    var newLength = new GridLength((previousLength.Value + nextLength.Value) / 2, GridUnitType.Star);

                    SetDockSize(previousElement, newLength, isHorizontal);
                    SetDockSize(nextElement, newLength, isHorizontal);
                }

                // Update pane.
                _panel.InvalidateMeasure();

                e.Handled = true;
            }
        }


        /// <summary>
        /// Gets the elements adjacent to the splitter.
        /// </summary>
        /// <param name="previousElement">The element before the splitter.</param>
        /// <param name="nextElement">The element after the splitter.</param>
        private void GetAdjacentElements(out FrameworkElement previousElement, out FrameworkElement nextElement)
        {
            Debug.Assert(0 <= _index && _index <= _panel.Children.Count, "Invalid splitter index.");

            previousElement = (FrameworkElement)_panel.Children[_index];
            nextElement = (FrameworkElement)_panel.Children[_index + 1];
        }


        /// <summary>
        /// Gets the dock width or height.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="isHorizontal">
        /// If set to <see langword="true"/> the dock width is returned; otherwise the dock height
        /// is returned.
        /// </param>
        private static GridLength GetDockSize(DependencyObject element, bool isHorizontal)
        {
            Debug.Assert(element != null);
            Debug.Assert(element is DockAnchorPane || element is DockSplitPane || element is DockTabPane);

            return (GridLength)element.GetValue(isHorizontal ? DockControl.DockWidthProperty : DockControl.DockHeightProperty);
        }


        /// <summary>
        /// Sets the dock width or height.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="size">The size to set.</param>
        /// <param name="isHorizontal">
        /// If set to <see langword="true"/> the dock width is set; otherwise the dock height is
        /// set.
        /// </param>
        private static void SetDockSize(DependencyObject element, GridLength size, bool isHorizontal)
        {
            Debug.Assert(element != null);
            Debug.Assert(element is DockAnchorPane || element is DockSplitPane || element is DockTabPane);

            element.SetValue(isHorizontal ? DockControl.DockWidthProperty : DockControl.DockHeightProperty, size);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            if (IsEnabled)
            {
                if (IsMouseOver)
                    VisualStateManager.GoToState(this, "MouseOver", useTransitions);
                else
                    VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }
        }
        #endregion
    }
}
