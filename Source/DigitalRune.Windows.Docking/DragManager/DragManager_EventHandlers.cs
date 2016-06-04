// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace DigitalRune.Windows.Docking
{
    // Event handlers.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal partial class DragManager
    {
        internal void OnFloatWindowEnterMove(FloatWindow floatWindow)
        {
            Debug.Assert(floatWindow != null);

            if (!IsDragging)
                BeginDrag(floatWindow, null, null);
        }


        internal void OnFloatWindowMove(FloatWindow floatWindow)
        {
            if (IsDraggingFloatWindow)
            {
                Debug.Assert(_floatWindow == floatWindow);

                Drag();
            }
        }


        internal void OnFloatWindowExitMove(FloatWindow floatWindow)
        {
            if (IsDraggingFloatWindow)
            {
                Debug.Assert(_floatWindow == floatWindow);

                EndDrag(!Keyboard.IsKeyDown(Key.Escape));
            }
        }


        /// <summary>
        /// Called when <see cref="DockTabPane"/>s are added to or removed from to the
        /// <see cref="DockControl"/>.
        /// </summary>
        private void OnDockTabPanesChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.OldItems != null)
                foreach (var dockTabPane in eventArgs.OldItems.OfType<DockTabPane>())
                    dockTabPane.MouseLeftButtonDown -= OnDockTabPaneMouseDown;

            if (eventArgs.NewItems != null)
                foreach (var dockTabPane in eventArgs.NewItems.OfType<DockTabPane>())
                    dockTabPane.MouseLeftButtonDown += OnDockTabPaneMouseDown;
        }


        private void OnDockTabPaneMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.Handled || IsDragging)
                return;

            var dockTabPane = (DockTabPane)sender;

            // Make sure the DockTabPane is active. (Clicks on the tab are automatically handled in
            // DockTabItem. But clicks inside of the window need to be handled here.)
            var selectedDockTabItem = dockTabPane.ItemContainerGenerator.ContainerFromIndex(dockTabPane.SelectedIndex) as DockTabItem;
            selectedDockTabItem?.Activate();

            // Start dragging.
            var dockTabItem = GetDockTabItemAtMouse(dockTabPane);
            bool dockTabPaneClicked = (eventArgs.OriginalSource as FrameworkElement)?.Name == "PART_DragArea";
            bool dockTabItemClicked = dockTabItem != null;

            var floatWindow = Window.GetWindow(dockTabPane) as FloatWindow;
            var rootPane = floatWindow?.GetViewModel()?.RootPane as IDockTabPane;
            if (rootPane != null && (dockTabPaneClicked || dockTabItemClicked && dockTabPane.Items.Count == 1))
            {
                // The FloatWindow contains a single DockTabPane and the DockTabPane was clicked.
                // Or, the FloatWindow contains a single DockTabItem and the DockTabItem was clicked.
                // --> Drag entire FloatWindow.

                // If FloatWindow is maximized, then minimize it and move it at the mouse position.
                if (floatWindow.WindowState == WindowState.Maximized)
                {
                    _mouseOffset = eventArgs.GetPosition(floatWindow) - new Point();
                    floatWindow.WindowState = WindowState.Normal;
                    Point position = GetFloatWindowPosition();
                    floatWindow.Left = position.X;
                    floatWindow.Top = position.Y;
                }

                // Call Window.DragMove() to trigger the Win32 move window loop.
                floatWindow.DragMove();
            }
            else if (dockTabPaneClicked)
            {
                // --> Drag entire DockTabPane.
                eventArgs.Handled = BeginDrag(null, dockTabPane, null);
            }
            else if (dockTabItemClicked)
            {
                // --> Drag DockTabItem tab inside DockTabPanel.
                eventArgs.Handled = BeginDrag(null, dockTabPane, dockTabItem);
            }
        }


        private void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            Debug.Assert(IsDraggingDockTabItems);

            EndDrag(false);
        }


        private void OnMouseLeftButtonUp(object sender, MouseEventArgs eventArgs)
        {
            Debug.Assert(IsDraggingDockTabItems);

            EndDrag(true);
        }


        private void OnMouseMove(object sender, MouseEventArgs eventArgs)
        {
            Debug.Assert(IsDraggingDockTabItems);

            Drag();
        }


        private void OnPreviewKeyDown(object sender, KeyEventArgs eventArgs)
        {
            Debug.Assert(IsDraggingDockTabItems);

            if (eventArgs.Key == Key.Escape)
            {
                EndDrag(false);
                eventArgs.Handled = true;
            }
        }
    }
}
