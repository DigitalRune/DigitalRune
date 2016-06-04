// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Handles dragging of docking objects with the mouse.
    /// </summary>
    internal partial class DragManager
    {
        // The DragManager has two modes:
        // - Dragging a FloatWindow.
        // - Dragging one or more DockTabItems in a DockTabPanel.
        // Depending on the current mouse position, the DragManager automatically switches between
        // these two modes.
        //
        // Dragging a FloatWindow:
        // FloatWindows are regular Windows. When they are dragged they enter the move window loop
        // (see WM_ENTERSIZEMOVE, WM_MOVING, WM_EXITSIZEMOVE). WPF cannot read mouse or keyboard
        // input during this loop, therefore it is necessary to hook into the Win32 message loop.
        // The mouse position can be read using Win32 interop (see WindowsHelper.GetMousePosition()).
        //
        // Dragging one or more DockTabItems in a DockTabPanel:
        // Mouse movement can be dragged normally by capturing the mouse and observing the WPF
        // mouse events.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Tolerance when dragging items in DockTabPanel.
        private const double VerticalTolerance = 24;

        private readonly DockControl _dockControl;
        private DockStrategy _dockStrategy;

        // The items that are dragged and the currently selected item.
        private readonly List<IDockTabItem> _draggedItems = new List<IDockTabItem>();
        private IDockTabItem _activeItem;

        // The FloatWindow that is currently being dragged.
        private FloatWindow _floatWindow;

        // The DockTabPane in which the items are currently being dragged.
        private DockTabPane _targetDockTabPane;

        // Flag to see if the docking layout has changed.
        private bool _layoutChanged;

        // The initial mouse position.
        private Point _initialMousePosition;
        private bool _dragDeltaExceeded;

        // The offset between the mouse cursor and the origin of the dragged element.
        private Vector _mouseOffset;

        // The initial size of the dragged elements.
        private Size _initialSize;

        // Is Float allowed as the final dock state?
        private bool _canFloat;

        // Rollback information.
        private DockState _originalDockState;
        private IFloatWindow _originalFloatWindow;
        private double _originalFloatLeft;
        private double _originalFloatTop;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether a dragging operation is in progress.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the user is dragging a <see cref="FloatWindow"/>; otherwise,
        /// <see langword="false"/>.
        /// </value>
        internal bool IsDragging
        {
            get { return _draggedItems.Count > 0; }
        }


        /// <summary>
        /// Gets a value indicating whether the user is dragging a <see cref="FloatWindow"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the user is dragging a <see cref="FloatWindow"/>; otherwise,
        /// <see langword="false"/>.
        /// </value>
        private bool IsDraggingFloatWindow
        {
            get { return _floatWindow != null; }
        }


        /// <summary>
        /// Gets a value indicating whether the user is dragging one or more
        /// <see cref="DockTabItem"/>s in a <see cref="DockTabPanel"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the user is dragging one or more <see cref="DockTabItem"/>s in
        /// a <see cref="DockTabPanel"/>; otherwise, <see langword="false"/>.
        /// </value>
        private bool IsDraggingDockTabItems
        {
            get { return IsDragging && !IsDraggingFloatWindow; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DragManager"/> class.
        /// </summary>
        /// <param name="dockControl">The <see cref="DockControl"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockControl"/> is <see langword="null"/>.
        /// </exception>
        internal DragManager(DockControl dockControl)
        {
            if (dockControl == null)
                throw new ArgumentNullException(nameof(dockControl));

            _dockControl = dockControl;

            Reset();

            // Register event handlers.
            ((INotifyCollectionChanged)_dockControl.DockTabPanes).CollectionChanged += OnDockTabPanesChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        /// <summary>
        /// Resets all fields.
        /// </summary>
        private void Reset()
        {
            Debug.Assert(Mouse.Captured == null);
            Debug.Assert(_borderDockIndicators == null);
            Debug.Assert(_paneDockIndicators == null);
            Debug.Assert(
                _dockStrategy == null || !_dockStrategy.DockControl.GetDockElements().OfType<DockTabItemProxy>().Any(),
                "All item proxies should have been removed.");

            _dockStrategy = null;
            _draggedItems.Clear();
            _activeItem = null;
            _floatWindow = null;
            _targetDockTabPane = null;
            _layoutChanged = false;
            _initialMousePosition = new Point(double.NaN, double.NaN);
            _dragDeltaExceeded = false;
            _mouseOffset = new Vector(double.NaN, double.NaN);
            _initialSize = new Size(double.NaN, double.NaN);
            _canFloat = true;
            _originalDockState = DockState.Hide;
            _originalFloatWindow = null;
            _originalFloatLeft = 0;
            _originalFloatTop = 0;
        }


        /// <summary>
        /// Starts a drag operation.
        /// </summary>
        /// <param name="floatWindow">The <see cref="FloatWindow"/> to be dragged.</param>
        /// <param name="dockTabPane">The <see cref="DockTabPane"/> to be dragged.</param>
        /// <param name="dockTabItem">The <see cref="DockTabItem"/> to be dragged.</param>
        /// <returns>
        /// <see langword="true" /> if the drag operation has been started; otherwise,
        /// <see langword="false" /> if the drag operation could not be started (e.g. because the
        /// mouse could not be captured).
        /// </returns>
        private bool BeginDrag(FloatWindow floatWindow, DockTabPane dockTabPane, DockTabItem dockTabItem)
        {
            _dockStrategy = _dockControl.GetViewModel()?.DockStrategy;
            if (_dockStrategy == null || _dockStrategy.DockControl.IsLocked)
            {
                Reset();
                return false;
            }

            FrameworkElement element = null;
            IDockTabPane draggedPane = null;
            IDockTabItem draggedItem = null;
            if (floatWindow != null)
            {
                // User is dragging a FloatWindow.
                // (Note: Dragging of FloatWindows with nested layouts is not supported.)
                draggedPane = floatWindow.GetViewModel()?.RootPane as IDockTabPane;
                element = floatWindow;
                _floatWindow = floatWindow;
                _initialSize = floatWindow.RenderSize;

                // Start dragging immediately.
                _dragDeltaExceeded = true;
            }
            else if (dockTabItem != null)
            {
                // User is dragging a DockTabItem in a DockTabPanel.
                draggedItem = dockTabItem.GetViewModel();
                element = dockTabItem;
                _targetDockTabPane = dockTabPane;
                _initialSize = dockTabPane.RenderSize;

                // Start dragging when threshold is exceeded.
                _initialMousePosition = WindowsHelper.GetMousePosition(_dockControl);
                _dragDeltaExceeded = false;
            }
            else if (dockTabPane != null)
            {
                // User is dragging a DockTabPane.
                draggedPane = dockTabPane.GetViewModel();
                element = dockTabPane;
                _initialSize = dockTabPane.RenderSize;
                _initialMousePosition = WindowsHelper.GetMousePosition(_dockControl);

                // Start dragging when threshold is exceeded.
                _initialMousePosition = WindowsHelper.GetMousePosition(_dockControl);
                _dragDeltaExceeded = false;
            }

            if (draggedPane == null && draggedItem == null)
            {
                Reset();
                return false;
            }

            // When the user is dragging the FloatWindow, the mouse is captured by Win32 move window
            // loop. When dragging a DockTabPane or DockTabItem, the mouse needs to be
            // captured to receive mouse events.
            if (_floatWindow == null)
            {
                if (!_dockControl.CaptureMouse())
                {
                    // Failed to capture the mouse.
                    Reset();
                    return false;
                }

                _dockControl.LostMouseCapture += OnLostMouseCapture;
                _dockControl.MouseLeftButtonUp += OnMouseLeftButtonUp;
                _dockControl.MouseMove += OnMouseMove;
                _dockControl.PreviewKeyDown += OnPreviewKeyDown;
                if (_targetDockTabPane != null)
                    _targetDockTabPane.PreviewKeyDown += OnPreviewKeyDown;
            }

            _dockStrategy.Begin();

            if (draggedPane != null)
            {
                _dockStrategy.Activate(draggedPane);
                _activeItem = draggedPane.SelectedItem;
                foreach (var item in draggedPane.Items)
                    if (item.DockState == draggedPane.DockState)
                        _draggedItems.Add(item);
            }
            else
            {
                Debug.Assert(draggedItem != null);

                _dockStrategy.Activate(draggedItem);
                _activeItem = draggedItem;
                _draggedItems.Add(draggedItem);
            }

            Debug.Assert(_draggedItems.Count > 0);

            // Determine whether dragged items may end in a FloatWindow.
            _canFloat = CanFloat();

            // Store the mouse offset relative to the dragged element.
            _mouseOffset = (Vector)WindowsHelper.GetMousePosition(element);

            // Remember information needed for a rollback.
            ReplaceItemsWithProxies(draggedPane ?? _targetDockTabPane.GetViewModel());
            _originalDockState = _draggedItems[0].DockState;
            BackupFloatWindowPosition();

            // Override mouse cursors. (Mouse cursor should not change to caret over text editor.)
            Mouse.OverrideCursor = Cursors.Arrow;

            return true;
        }


        /// <summary>
        /// Handles mouse move events when the drag operation is in progress.
        /// </summary>
        private void Drag()
        {
            Debug.Assert(IsDragging);

            if (!_dragDeltaExceeded)
            {
                Point mousePosition = WindowsHelper.GetMousePosition(_dockControl);
                Vector dragDelta = mousePosition - _initialMousePosition;
                if (Math.Abs(dragDelta.X) > SystemParameters.MinimumHorizontalDragDistance
                    || Math.Abs(dragDelta.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _dragDeltaExceeded = true;
                }
                else
                {
                    // Do nothing.
                    return;
                }
            }

            if (_targetDockTabPane != null && IsMouseOver(_targetDockTabPane.GetItemsPanel(), VerticalTolerance))
            {
                // Drag items inside a DockTabPanel.
                DragDockTabItems();
                return;
            }

            if (_floatWindow == null)
            {
                // Move the items into a FloatWindow.
                DragDockTabItemsIntoFloatWindow();
            }

            // Move the FloatWindow to the mouse position.
            DragFloatWindow();

            // Show DockIndicators and check possible dock positions.
            UpdateDockIndicators();

            if (!HasResult(_paneDockIndicators) && !HasResult(_borderDockIndicators))
            {
                // Check whether mouse cursor is over other DockTabItem tab and
                // move items into DockTabPane.
                DragFloatWindowIntoDockTabPanel();
            }
        }


        /// <summary>
        /// Drags the items to the mouse position within the <see cref="DockTabPanel"/>.
        /// </summary>
        private void DragDockTabItems()
        {
            Debug.Assert(_targetDockTabPane != null);

            // Switch item positions in DockTabPane.
            var hitItemVM = GetDockTabItemAtMouse(_targetDockTabPane, VerticalTolerance)?.GetViewModel();
            if (hitItemVM != null && !_draggedItems.Contains(hitItemVM))
            {
                var paneVM = _targetDockTabPane.GetViewModel();
                int index = paneVM.Items.IndexOf(hitItemVM);
                foreach (var item in _draggedItems)
                {
                    int oldIndex = paneVM.Items.IndexOf(item);
                    paneVM.Items.Move(oldIndex, index);

                    if (index < oldIndex)
                        index++;
                }

                _layoutChanged = true;
                _dockStrategy.Cleanup();
            }

            // Ensure that the visual tree is up-to-date.
            _targetDockTabPane.GetItemsPanel().UpdateLayout();

            // Apply a horizontal offset to move the dragged items with the mouse.
            SetTranslateTransform();
        }


        /// <summary>
        /// Called to move the <see cref="FloatWindow"/>.
        /// </summary>
        private void DragFloatWindow()
        {
            Debug.Assert(_floatWindow != null);

            // Do nothing. FloatWindow is moved in Win32 move window loop.
            _layoutChanged = true;
        }


        /// <summary>
        /// Move the dragged items from the current <see cref="DockTabPanel"/> into a
        /// <see cref="FloatWindow"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private void DragDockTabItemsIntoFloatWindow()
        {
            Debug.Assert(_floatWindow == null);

            if (_targetDockTabPane != null)
            {
                ClearTranslateTransform();

                _targetDockTabPane.PreviewKeyDown -= OnPreviewKeyDown;
                _targetDockTabPane = null;
            }

            _dockControl.LostMouseCapture -= OnLostMouseCapture;
            _dockControl.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            _dockControl.MouseMove -= OnMouseMove;
            _dockControl.PreviewKeyDown -= OnPreviewKeyDown;

            _dockControl.ReleaseMouseCapture();

            // Remove the dragged items from their current location.
            foreach (var item in _draggedItems)
            {
                DockHelper.Remove(_dockStrategy.DockControl.RootPane, item);

                foreach (var floatWindow in _dockStrategy.DockControl.FloatWindows)
                    DockHelper.Remove(floatWindow, item);

                DockHelper.Remove(_dockStrategy.DockControl.AutoHideLeft, item);
                DockHelper.Remove(_dockStrategy.DockControl.AutoHideRight, item);
                DockHelper.Remove(_dockStrategy.DockControl.AutoHideTop, item);
                DockHelper.Remove(_dockStrategy.DockControl.AutoHideBottom, item);
            }

            // Move items into a new FloatWindow.
            foreach (var item in _draggedItems)
                item.DockState = DockState.Float;

            var newPaneVM = _dockStrategy.CreateDockTabPane(_draggedItems[0], DockState.Float);
            for (int i = 1; i < _draggedItems.Count; i++)
                newPaneVM.Items.Add(_draggedItems[i]);

            var floatWindowVM = _dockStrategy.CreateFloatWindow();
            floatWindowVM.RootPane = newPaneVM;
            _dockStrategy.DockControl.FloatWindows.Add(floatWindowVM);
            _dockStrategy.Activate(_activeItem);
            _dockStrategy.Cleanup();

            // Get the newly created FloatWindow (view) from the DockControl.
            _floatWindow = GetFloatWindow(floatWindowVM);

            Debug.Assert(_floatWindow != null);

            LimitFloatWindowSize(_floatWindow, _initialSize);

            // Limit mouse offset to FloatWindow size.
            double actualWidth = _floatWindow.ActualWidth;
            if (actualWidth > 0)
                _mouseOffset.X = Math.Min(_mouseOffset.X, actualWidth / 2);

            Point position = GetFloatWindowPosition();
            _floatWindow.Left = position.X;
            _floatWindow.Top = position.Y;

            // Wait until FloatWindow is loaded, initiate the Win32 move window loop.
            _floatWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_floatWindow != null)
                {
                    LimitFloatWindowSize(_floatWindow, _initialSize);

                    // Limit mouse offset to FloatWindow size.
                    _mouseOffset.X = Math.Min(_mouseOffset.X, _floatWindow.ActualWidth / 2);
                    _mouseOffset.Y = Math.Max(_mouseOffset.Y, 8);

                    Point pos = GetFloatWindowPosition();
                    _floatWindow.Left = pos.X;
                    _floatWindow.Top = pos.Y;

                    if (Mouse.LeftButton == MouseButtonState.Pressed)
                        _floatWindow.DragMove();
                    else
                        EndDrag(true);
                }
            }), DispatcherPriority.Loaded); // Important: Action needs to be invoked before input.
        }


        /// <summary>
        /// Tries to move the dragged items from the <see cref="FloatWindow"/> into a
        /// <see cref="DockTabPanel"/>.
        /// </summary>
        private void DragFloatWindowIntoDockTabPanel()
        {
            Debug.Assert(_targetDockTabPane == null);
            Debug.Assert(_floatWindow != null);

            var dockTabPane = GetTargetPane() as DockTabPane;
            if (dockTabPane == null)
                return; // No DockTabPane (view).

            var dockTabPaneVM = dockTabPane.GetViewModel();
            if (dockTabPaneVM == null)
                return; // No IDockTabPane (view-model).

            if (GetDockTabItemAtMouse(dockTabPane) == null)
                return; // No DockTabItem hit.

            if (!CanDock(dockTabPaneVM, DockPosition.Inside))
                return; // Docking not allowed.

            // Remove currently dragged FloatWindow.
            var floatWindowVM = _floatWindow.GetViewModel();
            foreach (var item in _draggedItems)
                DockHelper.Remove(floatWindowVM, item);

            _floatWindow = null;
            Win32.ReleaseCapture(); // Exit Win32 move window loop.

            // Add items into target DockTabPane.
            _targetDockTabPane = dockTabPane;
            foreach (var item in _draggedItems)
            {
                item.DockState = dockTabPaneVM.DockState;
                dockTabPaneVM.Items.Add(item);
            }

            // Make sure the current item is selected in DockTabPane.
            _dockStrategy.Activate(_activeItem);

            // When the Win32 move window loop exits, the DockControl receives a LostMouseCapture
            // event. --> Defer dragging of the DockTabItems.
            _dockControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_dockControl.IsMouseCaptured)
                {
                    if (!_dockControl.CaptureMouse())
                    {
                        // Failed to capture the mouse.
                        EndDrag(false);
                        return;
                    }
                    _dockControl.LostMouseCapture += OnLostMouseCapture;
                    _dockControl.MouseMove += OnMouseMove;
                    _dockControl.MouseLeftButtonUp += OnMouseLeftButtonUp;
                    _dockControl.PreviewKeyDown += OnPreviewKeyDown;

                    _targetDockTabPane.PreviewKeyDown += OnPreviewKeyDown;

                    DragDockTabItems();
                }
            }));

            HideBorderDockIndicators();
            HidePaneIndicators();

            _dockControl.UpdateFloatWindows();

            _layoutChanged = true;
        }


        /// <summary>
        /// Ends the drag operation.
        /// </summary>
        /// <param name="commit">
        /// <see langword="true"/> to commit the changes; otherwise, <see langword="false"/> to
        /// revert the changes.
        /// </param>
        internal void EndDrag(bool commit)
        {
            Debug.Assert(IsDragging);

            if (!_dragDeltaExceeded)
                commit = false;

            if (_targetDockTabPane != null)
            {
                _targetDockTabPane.PreviewKeyDown -= OnPreviewKeyDown;

                if (_dragDeltaExceeded)
                    ClearTranslateTransform();
            }

            if (IsDraggingDockTabItems)
            {
                // Remove event handlers.
                _dockControl.LostMouseCapture -= OnLostMouseCapture;
                _dockControl.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                _dockControl.MouseMove -= OnMouseMove;
                _dockControl.PreviewKeyDown -= OnPreviewKeyDown;

                _dockControl.ReleaseMouseCapture();
            }

            if (commit)
            {
                Commit();
            }
            else
            {
                Win32.ReleaseCapture(); // Exit Win32 move window loop.
                Rollback();
            }

            // Get rid of DockIndicators.
            HideBorderDockIndicators();
            HidePaneIndicators();

            // Finalize docking layout.
            _dockStrategy.End();

            // Remove obsolete FloatWindows.
            _dockControl.UpdateFloatWindows();

            Mouse.OverrideCursor = null;

            Reset();
        }


        private void Commit()
        {
            if (_layoutChanged)
            {
                if (IsDraggingDockTabItems)
                {
                    // Dragging ended in DockTabPanel.
                    RestoreFloatWindowPosition();
                }
                else if (_floatWindow != null)
                {
                    // Dragging ended outside of a DockTabPanel. Check the dock indicators to find the
                    // desired target position.
                    IDockPane target = null;
                    DockPosition position = DockPosition.None;
                    if (HasResult(_borderDockIndicators))
                    {
                        target = _dockStrategy.DockControl.RootPane;
                        position = _borderDockIndicators.Result;
                    }
                    else if (HasResult(_paneDockIndicators))
                    {
                        target = DockHelper.GetViewModel<IDockPane>(_paneDockIndicators.Target);
                        position = _paneDockIndicators.Result;
                    }

                    if (position != DockPosition.None && target != null)
                    {
                        // User has dropped FloatWindow on a DockIndicator.
                        // --> Dock content.
                        var floatWindowVM = _floatWindow.GetViewModel();
                        foreach (var item in _draggedItems)
                        {
                            DockHelper.Remove(floatWindowVM, item);
                            item.DockState = DockState.Hide;
                        }

                        var dockTabPane = _dockStrategy.CreateDockTabPane(_draggedItems[0], DockState.Hide);
                        for (int i = 1; i < _draggedItems.Count; i++)
                            dockTabPane.Items.Add(_draggedItems[i]);

                        _dockStrategy.Dock(dockTabPane, target, position);
                        RestoreFloatWindowPosition();
                    }
                    else
                    {
                        // The final state is DockState.Float.
                        if (!_canFloat && _originalDockState != DockState.Float)
                        {
                            // DockState.Float is not allowed.
                            Rollback();
                            return;
                        }
                    }
                }
            }

            // Keep the items at their new position.
            // --> Remove the item proxies.
            var dockState = _draggedItems[0].DockState;
            RemoveItemProxies(dockState);
            if (_layoutChanged && dockState == DockState.Dock)
            {
                // The position within the DockControl may have changed. The assignment to the
                // auto-hide bar is no longer valid.
                // --> Also remove item proxies from auto-hide bars.
                RemoveItemProxies(DockState.AutoHide);
            }

            // Restore the original dock state of the dragged items.
            RestoreItemsFromProxies();
        }


        private void Rollback()
        {
            RestoreFloatWindowPosition();

            if (_layoutChanged)
            {
                // The docking layout has changed.
                // --> Remove the dragged items from their current location.
                switch (_draggedItems[0].DockState)
                {
                    case DockState.Dock:
                        foreach (var item in _draggedItems)
                            DockHelper.Remove(_dockStrategy.DockControl, item);
                        break;
                    case DockState.Float:
                        foreach (var item in _draggedItems)
                            foreach (var floatWindow in _dockStrategy.DockControl.FloatWindows)
                                DockHelper.Remove(floatWindow, item);
                        break;
                    case DockState.AutoHide:
                        foreach (var item in _draggedItems)
                        {
                            DockHelper.Remove(_dockStrategy.DockControl.AutoHideLeft, item);
                            DockHelper.Remove(_dockStrategy.DockControl.AutoHideRight, item);
                            DockHelper.Remove(_dockStrategy.DockControl.AutoHideTop, item);
                            DockHelper.Remove(_dockStrategy.DockControl.AutoHideBottom, item);
                        }
                        break;
                }
            }
            else
            {
                // The docking layout is unchanged.
                // --> Keep the dragged items and remove the item proxies.
                Debug.Assert(_originalDockState == _draggedItems[0].DockState);
                RemoveItemProxies(_draggedItems[0].DockState);
            }

            // Restore items and remove item proxies.
            RestoreItemsFromProxies();

            // Restore the original dock state of the dragged items.
            foreach (var item in _draggedItems)
                item.DockState = _originalDockState;
        }


        private void BackupFloatWindowPosition()
        {
            if (_floatWindow != null)
            {
                _originalFloatWindow = _floatWindow.GetViewModel();
                _originalFloatLeft = _originalFloatWindow.Left;
                _originalFloatTop = _originalFloatWindow.Top;
            }
        }


        private void RestoreFloatWindowPosition()
        {
            if (_originalFloatWindow != null)
            {
                _originalFloatWindow.Left = _originalFloatLeft;
                _originalFloatWindow.Top = _originalFloatTop;
            }
        }
        #endregion
    }
}
