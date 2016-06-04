// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Docking
{
    // Helper methods
    internal partial class DragManager
    {
        /// <summary>
        /// Determines whether this dragged items may be docked at the specified position.
        /// </summary>
        /// <param name="target">The target pane.</param>
        /// <param name="position">The position relative to <paramref name="target"/>.</param>
        /// <returns>
        /// <see langword="true"/> the specified dock position in allowed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        private bool CanDock(IDockPane target, DockPosition position)
        {
            Debug.Assert(_dockStrategy != null);
            Debug.Assert(_draggedItems.Count > 0);

            target = target ?? _dockStrategy.DockControl.RootPane;

            foreach (var item in _draggedItems)
                if (!_dockStrategy.CanDock(item, target, position))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether a <see cref="FloatWindow"/> is valid as the final dock state.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> the dragged items may be shown in a <see cref="FloatWindow"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private bool CanFloat()
        {
            Debug.Assert(_dockStrategy != null);
            Debug.Assert(_draggedItems.Count > 0);

            foreach (var item in _draggedItems)
                if (item.DockState != DockState.Float && !_dockStrategy.CanFloat(item))
                    return false;

            return true;
        }


        /// <summary>
        /// Gets the <see cref="FloatWindow"/> (view) for the <see cref="IFloatWindow"/>
        /// (view-model).
        /// </summary>
        /// <param name="viewModel">The <see cref="IFloatWindow"/>.</param>
        /// <returns>The <see cref="FloatWindow"/>.</returns>
        private FloatWindow GetFloatWindow(IFloatWindow viewModel)
        {
            Debug.Assert(_dockControl != null);
            Debug.Assert(viewModel != null);

            _dockControl.UpdateFloatWindows();
            for (int i = 0; i < _dockControl.FloatWindows.Count; i++)
            {
                var floatWindow = _dockControl.FloatWindows[i];
                if (floatWindow.GetViewModel() == viewModel)
                    return floatWindow;
            }

            return null;
        }


        /// <summary>
        /// Gets the position of a <see cref="FloatWindow"/> if one would be shown at the current
        /// mouse position.
        /// </summary>
        /// <returns>The <see cref="FloatWindow"/> position.</returns>
        private Point GetFloatWindowPosition()
        {
            Debug.Assert(_dockControl != null);

            // Mouse position relative to DockControl in device-independent pixels.
            Point position = WindowsHelper.GetMousePosition(_dockControl);

            // Absolute mouse position in native screen coordinates.
            position = _dockControl.PointToScreen(position);

            // Absolute mouse position in device-independent pixels.
            position = DockHelper.ScreenToLogical(_dockControl, position);

            // Offset between window origin and mouse cursor.
            position -= _mouseOffset;

            return position;
        }


        /// <summary>
        /// Limits the extent of the <see cref="FloatWindow"/> to a reasonable size.
        /// </summary>
        /// <param name="floatWindow">The <see cref="FloatWindow"/>.</param>
        /// <param name="fallbackSize">The fallback size. Can be (NaN, NaN).</param>
        private static void LimitFloatWindowSize(FloatWindow floatWindow, Size fallbackSize)
        {
            Debug.Assert(floatWindow != null);
            Debug.Assert(Numeric.IsNaN(fallbackSize.Width) || fallbackSize.Width > 0);
            Debug.Assert(Numeric.IsNaN(fallbackSize.Height) || fallbackSize.Height > 0);

            double maxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            double maxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            Size size = floatWindow.RenderSize;
            if (size.Width > maxWidth)
                floatWindow.Width = Numeric.IsNaN(fallbackSize.Width) ? maxWidth : fallbackSize.Width;
            if (size.Height > maxHeight)
                floatWindow.Height = Numeric.IsNaN(fallbackSize.Height) ? maxHeight : fallbackSize.Height;
        }


        /// <summary>
        /// Gets the <see cref="DockTabItem"/> at the mouse position by testing against the
        /// <see cref="DockTabItem"/> tabs in the specified pane.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="DockTabPane"/>.</param>
        /// <param name="verticalTolerance">
        /// The tolerance (margin at top and bottom) in pixels.
        /// </param>
        /// <returns>The <see cref="DockTabItem"/> under the mouse cursor.</returns>
        private static DockTabItem GetDockTabItemAtMouse(DockTabPane dockTabPane, double verticalTolerance = 0)
        {
            Debug.Assert(dockTabPane != null);

            if (dockTabPane.Items.Count == 0)
                return null;    // Empty DockTabPane.

            var itemsPanel = dockTabPane.GetItemsPanel();
            if (itemsPanel == null)
                return null;    // ItemsPanel missing.

            Point mousePosition = WindowsHelper.GetMousePosition(itemsPanel);

            Rect bounds = new Rect(itemsPanel.RenderSize);
            bounds.Inflate(0, verticalTolerance);
            if (!bounds.Contains(mousePosition))
                return null;    // Mouse position outside ItemsPanel.

            // Test mouse position against DockTabItems bounds.
            double height = itemsPanel.RenderSize.Height;
            double x = 0;
            for (int i = 0; i < dockTabPane.Items.Count; i++)
            {
                var dockTabItem = dockTabPane.ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;
                if (dockTabItem == null)
                    break;

                bounds = new Rect(new Point(x, 0), new Size(dockTabItem.RenderSize.Width, height));
                bounds.Inflate(0, verticalTolerance);
                if (bounds.Contains(mousePosition))
                    return dockTabItem;

                x += bounds.Width;
            }

            return null;
        }


        /// <summary>
        /// Applies a horizontal offset to the dragged <see cref="DockTabItem"/>s.
        /// </summary>
        private void SetTranslateTransform()
        {
            Debug.Assert(_targetDockTabPane != null);

            double? offset = null;
            for (int i = 0; i < _targetDockTabPane.Items.Count; i++)
            {
                var dockTabItem = _targetDockTabPane.ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;

                if (dockTabItem == null)
                    continue;   // Item container not yet generated.

                if (!_draggedItems.Contains(dockTabItem.GetViewModel()))
                    continue;   // Item is not dragged.

                if (offset == null)
                {
                    // The mouse cursor should be on the first dragged DockTabItem.
                    if (_mouseOffset.X < 0 || _mouseOffset.X > dockTabItem.RenderSize.Width)
                    {
                        if (dockTabItem.RenderSize.Width > 0)
                            _mouseOffset.X = dockTabItem.RenderSize.Width / 4;
                        else
                            _mouseOffset.X = 32;    // Item not yet measured. Use default value.
                    }

                    Point mousePosition = WindowsHelper.GetMousePosition(dockTabItem);
                    offset = mousePosition.X - _mouseOffset.X;
                }

                // Set DockTabPanel.IsDragged flag. Items which are dragged are not animated
                // by the DockTabPanel.
                DockTabPanel.SetIsDragged(dockTabItem, true);

                // Apply offset as RenderTransform.
                var translateTransform = dockTabItem.RenderTransform as TranslateTransform;
                if (translateTransform == null)
                {
                    translateTransform = new TranslateTransform();
                    dockTabItem.RenderTransform = translateTransform;
                }

                translateTransform.X += offset.Value;
            }
        }


        /// <summary>
        /// Clears the horizontal offset from the dragged <see cref="DockTabItem"/>s.
        /// </summary>
        private void ClearTranslateTransform()
        {
            Debug.Assert(_targetDockTabPane != null);

            for (int i = 0; i < _targetDockTabPane.Items.Count; i++)
            {
                var dockTabItem = _targetDockTabPane.ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;
                if (dockTabItem != null && DockTabPanel.GetIsDragged(dockTabItem))
                {
                    DockTabPanel.SetIsDragged(dockTabItem, false);
                    var translateTransform = dockTabItem.RenderTransform as TranslateTransform;
                    if (translateTransform != null)
                        translateTransform.X = 0;
                }
            }
        }


        /// <summary>
        /// Determines whether the mouse cursor is over the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="verticalTolerance">
        /// The tolerance (margin at top and bottom) in pixels.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if the mouse is over <paramref name="element"/>; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        private static bool IsMouseOver(UIElement element, double verticalTolerance = 0)
        {
            Debug.Assert(element != null);

            Rect bounds = new Rect(element.RenderSize);
            bounds.Inflate(0, verticalTolerance);
            Point mousePosition = WindowsHelper.GetMousePosition(element);
            return bounds.Contains(mousePosition);
        }


        /// <summary>
        /// Gets the dock pane under the mouse cursor.
        /// </summary>
        /// <returns>The dock pane under the mouse cursor.</returns>
        /// <remarks>This method ignores the currently dragged floating window.</remarks>
        private FrameworkElement GetTargetPane()
        {
            Debug.Assert(_dockControl != null);

            FrameworkElement targetPane = null;
            bool floatWindowHit = false;

            // Check whether mouse is above one of the other FloatWindows.
            // (Iterate from top to bottom window.)
            var floatWindows = _dockControl.FloatWindows.OrderByDescending(window => window.LastActivation);
            foreach (var floatWindow in floatWindows)
            {
                Debug.Assert(floatWindow != null, "DockControl.FloatWindows must not contain null.");

                if (floatWindow == _floatWindow || !floatWindow.IsVisible)
                    continue;

                floatWindowHit = IsMouseOver(floatWindow);
                if (floatWindowHit)
                {
                    targetPane = GetTargetPane(floatWindow);
                    if (targetPane != null)
                        break;
                }
            }

            // Reject targetPane if _canFloat is false because we are not allowed to drag the items
            // into a FloatWindow.
            if (targetPane != null && !_canFloat)
                return null;

            if (!floatWindowHit)
                targetPane = GetTargetPane(_dockControl);

            return targetPane;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static FrameworkElement GetTargetPane(FrameworkElement element)
        {
            Debug.Assert(element != null);

            if (element is DockControl)
            {
                var dockControl = (DockControl)element;
                var rootPane = dockControl.RootPane;
                if (rootPane != null)
                    return GetTargetPane(rootPane);
            }
            else if (element is FloatWindow)
            {
                var floatWindow = (FloatWindow)element;
                var rootPane = floatWindow.RootPane;
                if (rootPane != null)
                    return GetTargetPane(rootPane);
            }
            else if (element is DockAnchorPane)
            {
                var dockAnchorPane = (DockAnchorPane)element;
                var childPane = dockAnchorPane.ChildPane;
                if (childPane != null)
                    return GetTargetPane(childPane);
                if (IsMouseOver(dockAnchorPane))
                    return dockAnchorPane;
            }
            else if (element is DockSplitPane)
            {
                var dockSplitPane = (DockSplitPane)element;
                for (int i = 0; i < dockSplitPane.Items.Count; i++)
                {
                    var childPane = dockSplitPane.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
                    if (childPane == null)
                        continue;

                    var targetPane = GetTargetPane(childPane);
                    if (targetPane != null)
                        return targetPane;
                }
            }
            else if (element is DockTabPane)
            {
                var dockTabPane = (DockTabPane)element;
                if (IsMouseOver(dockTabPane))
                    return dockTabPane;
            }

            return null;
        }
    }
}
