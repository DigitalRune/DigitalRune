// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using DigitalRune.Mathematics;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows.Docking
{
    partial class DockControl
    {
        /// <summary>
        /// Called when the <see cref="IDockControl.FloatWindows"/> collection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnFloatWindowsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            UpdateFloatWindows();
        }


        internal void UpdateFloatWindows()
        {
            if (DockStrategy == null)
                return;

            var owner = Window.GetWindow(this);

            // Close obsolete FloatWindows.
            for (int i = _floatWindows.Count - 1; i >= 0; i--)
            {
                var floatWindow = _floatWindows[i];
                var floatWindowVM = floatWindow.GetViewModel();
                if (floatWindowVM == null
                    || !DockStrategy.DockControl.FloatWindows.Contains(floatWindowVM)
                    || !DockStrategy.IsVisible(floatWindowVM))
                {
                    // ----- Close FloatWindow.
                    floatWindow.Close();
                    _floatWindows.RemoveAt(i);
                }
            }

            // Open new FloatWindows.
            for (int i = 0; i < DockStrategy.DockControl.FloatWindows.Count; i++)
            {
                var floatWindowVM = DockStrategy.DockControl.FloatWindows[i];
                if (DockStrategy.IsVisible(floatWindowVM) && GetView(floatWindowVM) == null)
                {
                    // ----- Open FloatWindow.

                    // Make sure that the floating window stays on the screen.
                    // At least 30 pixels must be visible.
                    const double safety = 30;
                    double screenLeft = SystemParameters.VirtualScreenLeft;
                    double screenTop = SystemParameters.VirtualScreenTop;
                    double screenRight = screenLeft + SystemParameters.VirtualScreenWidth;
                    double screenBottom = screenTop + SystemParameters.VirtualScreenHeight;
                    floatWindowVM.Left = Math.Min(Math.Max(floatWindowVM.Left, screenLeft), screenRight - safety);
                    floatWindowVM.Top = Math.Min(Math.Max(floatWindowVM.Top, screenTop), screenBottom - safety);

                    var floatWindow = new FloatWindow(this)
                    {
                        DataContext = floatWindowVM,
                        Owner = owner,
                    };

                    bool autoWidth = Numeric.IsNaN(floatWindowVM.Width);
                    bool autoHeight = Numeric.IsNaN(floatWindowVM.Height);
                    if (autoWidth && autoHeight)
                        floatWindow.SizeToContent = SizeToContent.WidthAndHeight;
                    else if (autoWidth)
                        floatWindow.SizeToContent = SizeToContent.Width;
                    else if (autoHeight)
                        floatWindow.SizeToContent = SizeToContent.Height;
                    else
                        floatWindow.SizeToContent = SizeToContent.Manual;

                    floatWindow.SetBinding(ContentProperty, new Binding(nameof(IFloatWindow.RootPane)));
                    floatWindow.SetBinding(WidthProperty, new Binding(nameof(IFloatWindow.Width)) { Mode = BindingMode.TwoWay });
                    floatWindow.SetBinding(HeightProperty, new Binding(nameof(IFloatWindow.Height)) { Mode = BindingMode.TwoWay });
                    floatWindow.SetBinding(Window.LeftProperty, new Binding(nameof(IFloatWindow.Left)) { Mode = BindingMode.TwoWay });
                    floatWindow.SetBinding(Window.TopProperty, new Binding(nameof(IFloatWindow.Top)) { Mode = BindingMode.TwoWay });

                    floatWindow.Show();

                    // Bind WindowState after showing the window. Otherwise, it could be maximized
                    // on the wrong screen.
                    floatWindow.SetBinding(Window.WindowStateProperty, new Binding(nameof(IFloatWindow.WindowState)) { Mode = BindingMode.TwoWay });

                    _floatWindows.Add(floatWindow);
                }
            }
        }


        private void LoadFloatWindows()
        {
            // Show temporarily hidden FloatWindows.
            for (int i = 0; i < _floatWindows.Count; i++)
                _floatWindows[i].Show();

            // Show new FloatWindows.
            UpdateFloatWindows();

            // We need to activate the main window. If it is not activated and a FloatWindow
            // was opened in UpdateDockState() and after the start Focus() is called on the 
            // FloatWindow Content then the main window will not be able to get the focus back. 
            //--> WPF Bug?
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.Activate();
                window.StateChanged += OnWindowStateChanged;
            }
        }


        private void UnloadFloatWindows()
        {
            // When the DockControl is unloaded (e.g. because it is in a tab control), the
            // DragManager does not work.
            // --> Hide all floating windows together with the DockControl.
            for (int i = _floatWindows.Count - 1; i >= 0; i--)
            {
                var floatWindow = _floatWindows[i];
                floatWindow.Hide();
            }

            var window = Window.GetWindow(this);
            if (window != null)
                window.StateChanged -= OnWindowStateChanged;
        }


        private void OnWindowStateChanged(object sender, EventArgs eventArgs)
        {
            // WPF does not restore child windows correctly when the main window is minimized and
            // restored. This needs to be handled explicitly.

            // Note: Do not use SystemCommands.MinimizeWindow(floatWindow) and
            // SystemCommands.RestoreWindow(floatWindow)! These methods work correctly when
            // used the first time, but screw up the window state when used multiple times.
            var window = (Window)sender;
            if (window.WindowState == WindowState.Minimized)
            {
                for (int i = 0; i < _floatWindows.Count; i++)
                {
                    var floatWindow = _floatWindows[i];
                    Win32.ShowWindow(new WindowInteropHelper(floatWindow).Handle, ShowWindowStyles.SW_MINIMIZE);
                }
            }
            else
            {
                for (int i = 0; i < _floatWindows.Count; i++)
                {
                    var floatWindow = _floatWindows[i];
                    if (floatWindow.WindowState == WindowState.Minimized)
                        Win32.ShowWindow(new WindowInteropHelper(floatWindow).Handle, ShowWindowStyles.SW_RESTORE);
                }
            }
        }
    }
}
