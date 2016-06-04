// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Hosts an <see cref="AutoHidePane"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AutoHidePane"/>s are animated panes that slide into the view when a tab in the
    /// <see cref="AutoHideBar"/> is clicked. They are rendered above other
    /// <see cref="DockTabItem"/>s. When mixing WPF and legacy controls (e.g. Windows Forms, native
    /// controls) within one window there are certain limitations (see "Airspaces" in MSDN
    /// documentation). To bypass these limitation each <see cref="AutoHidePane"/> is rendered in an
    /// <see cref="AutoHideOverlay"/> which is a intermediate window rendered on top of the
    /// <see cref="DockControl"/>.
    /// </para>
    /// </remarks>
    public class AutoHideOverlay : DockOverlay
    {
        /// <summary>
        /// Gets or sets the <see cref="DockControl"/>.
        /// </summary>
        /// <value>The <see cref="DockControl"/>.</value>
        [Browsable(false)]
        public DockControl DockControl { get; }


        /// <summary>
        /// Initializes static members of the <see cref="AutoHideOverlay"/> class.
        /// </summary>
        static AutoHideOverlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoHideOverlay), new FrameworkPropertyMetadata(typeof(AutoHideOverlay)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AutoHideOverlay"/> class.
        /// </summary>
        /// <param name="dockControl">
        /// The <see cref="DockControl"/>. Can be <see langword="null"/>.
        /// </param>
        /// <param name="target">
        /// The target element over which the overlay window should appear.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        public AutoHideOverlay(DockControl dockControl, FrameworkElement target)
          : base(target)
        {
            DockControl = dockControl;
            Owner = GetWindow(target);
            SourceInitialized += OnSourceInitialized;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DigitalRune.Windows.Interop.Win32.SetWindowLong(System.IntPtr,System.Int32,System.Int32)")]
        private void OnSourceInitialized(object sender, EventArgs eventArgs)
        {
            var helper = new WindowInteropHelper(this);

            // Make the overlay window a child of the main window. Otherwise the overlay window
            // steals the focus from the main window.
            int oldWindowStyle = Win32.GetWindowLong(helper.Handle, GetWindowLongIndex.GWL_STYLE);
            int newWindowStyle = oldWindowStyle | (int)WindowStyles.WS_CHILD;
            Win32.SetWindowLong(helper.Handle, GetWindowLongIndex.GWL_STYLE, newWindowStyle);

            // The flag WS_EX_NOACTIVATE is necessary, otherwise the child window cannot receive
            // keyboard focus.
            int oldWindowStyleEx = Win32.GetWindowLong(helper.Handle, GetWindowLongIndex.GWL_EXSTYLE);
            int newWindowStyleEx = oldWindowStyleEx | (int)WindowStylesEx.WS_EX_NOACTIVATE;
            Win32.SetWindowLong(helper.Handle, GetWindowLongIndex.GWL_EXSTYLE, newWindowStyleEx);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            DockControl?._autoHideOverlays.Add(this);
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            DockControl?._autoHideOverlays.Remove(this);
        }
    }
}
