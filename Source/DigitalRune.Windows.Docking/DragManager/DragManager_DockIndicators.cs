// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows.Docking
{
    internal partial class DragManager
    {
        private DockIndicatorOverlay _borderDockIndicators;
        private DockIndicatorOverlay _paneDockIndicators;


        private void UpdateDockIndicators()
        {
            var targetPane = GetTargetPane();

            // Show/hide dock indicators.
            UpdatePaneIndicators(targetPane);
            UpdateBorderIndicators(targetPane); // Show BorderIndicator on top of PaneIndicators.

            // Hit-test dock indicators in z-order: BorderIndicators before PaneIndicators
            var result = DockPosition.None;
            if (_borderDockIndicators != null)
                result = _borderDockIndicators.HitTest();

            if (_paneDockIndicators != null)
            {
                if (result == DockPosition.None)
                    _paneDockIndicators.HitTest();
                else
                    _paneDockIndicators.ClearResult();
            }
        }


        private void UpdatePaneIndicators(FrameworkElement targetElement)
        {
            if (targetElement != null)
                ShowPaneIndicators(targetElement);
            else
                HidePaneIndicators();
        }


        private void ShowPaneIndicators(FrameworkElement targetElement)
        {
            Debug.Assert(targetElement != null);

            if (_paneDockIndicators != null && _paneDockIndicators.Target == targetElement)
                return;

            HidePaneIndicators();

            // The visible drop target buttons are determined by the DockStrategy.
            // For DockAnchorPanes only the inside button is visible.
            bool isAnchorPane = targetElement is DockAnchorPane;

            _paneDockIndicators = new PaneIndicators(targetElement)
            {
                AllowDockLeft = !isAnchorPane && CanDock(DockHelper.GetViewModel<IDockPane>(targetElement), DockPosition.Left),
                AllowDockTop = !isAnchorPane && CanDock(DockHelper.GetViewModel<IDockPane>(targetElement), DockPosition.Top),
                AllowDockRight = !isAnchorPane && CanDock(DockHelper.GetViewModel<IDockPane>(targetElement), DockPosition.Right),
                AllowDockBottom = !isAnchorPane && CanDock(DockHelper.GetViewModel<IDockPane>(targetElement), DockPosition.Bottom),
                AllowDockInside = CanDock(DockHelper.GetViewModel<IDockPane>(targetElement), DockPosition.Inside),
            };
            _paneDockIndicators.Show();
        }


        private void HidePaneIndicators()
        {
            if (_paneDockIndicators != null)
            {
                _paneDockIndicators.Close();
                _paneDockIndicators = null;
            }
        }


        private void UpdateBorderIndicators(FrameworkElement targetPane)
        {
            // Show BorderIndicators if the mouse is over the DockControl and the targetPane is
            // not in a FloatWindow.
            if (IsMouseOver(_dockControl) && (targetPane == null || !(Window.GetWindow(targetPane) is FloatWindow)))
                ShowBorderDockIndicators();
            else
                HideBorderDockIndicators();
        }


        private void ShowBorderDockIndicators()
        {
            Debug.Assert(_dockControl != null);

            if (_borderDockIndicators == null)
            {
                // The visible drop target buttons are determined by the DockStrategy.
                _borderDockIndicators = new BorderIndicators(_dockControl)
                {
                    AllowDockInside = false,
                    AllowDockLeft = CanDock(null, DockPosition.Left),
                    AllowDockTop = CanDock(null, DockPosition.Top),
                    AllowDockRight = CanDock(null, DockPosition.Right),
                    AllowDockBottom = CanDock(null, DockPosition.Bottom),
                };
                _borderDockIndicators.Show();
            }

            // Bring border DockIndicators to front, otherwise they might be behind the
            // previews (semi-transparent rectangles) of the pane DockIndicators.
            var hWnd = new WindowInteropHelper(_borderDockIndicators).Handle;
            Win32.SetWindowPos(hWnd, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOACTIVATE);

            // Alternative:
            //_borderDockIndicators.Topmost = true;
            //_borderDockIndicators.Topmost = false;
        }


        private void HideBorderDockIndicators()
        {
            if (_borderDockIndicators != null)
            {
                _borderDockIndicators.Close();
                _borderDockIndicators = null;
            }
        }


        private static bool HasResult(DockIndicatorOverlay dockIndicators)
        {
            return dockIndicators != null && dockIndicators.Result != DockPosition.None;
        }
    }
}
