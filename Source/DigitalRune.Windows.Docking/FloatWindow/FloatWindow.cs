// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a window that contains elements when they are dragged from the docking layout.
    /// </summary>
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public class FloatWindow : Window
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private HwndSource _hwndSource;
        private ContentPresenter _contentPresenter;
        private bool _ignoreSizingMessage;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="DockControl"/>.
        /// </summary>
        /// <value>The <see cref="DockControl"/>.</value>
        [Browsable(false)]
        public DockControl DockControl { get; }


        /// <summary>
        /// Gets the time of the last activation.
        /// </summary>
        /// <value>The time of the last activation.</value>
        internal DateTime LastActivation { get; private set; }


        /// <summary>
        /// Gets the <see cref="FrameworkElement"/> that represents the
        /// <see cref="IDockContainer.RootPane"/> of the window.
        /// </summary>
        /// <value>
        /// The <see cref="FrameworkElement"/> that represents the
        /// <see cref="IDockContainer.RootPane"/>.
        /// </value>
        internal FrameworkElement RootPane
        {
            get { return _contentPresenter.GetContentContainer<FrameworkElement>(); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="FloatWindow"/> class.
        /// </summary>
        static FloatWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatWindow), new FrameworkPropertyMetadata(typeof(FloatWindow)));
            ShowInTaskbarProperty.OverrideMetadata(typeof(FloatWindow), new FrameworkPropertyMetadata(Boxed.BooleanFalse));
            WindowStyleProperty.OverrideMetadata(typeof(FloatWindow), new FrameworkPropertyMetadata(WindowStyle.ToolWindow));
            SizeToContentProperty.OverrideMetadata(typeof(FloatWindow), new FrameworkPropertyMetadata(SizeToContent.WidthAndHeight));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FloatWindow"/> class.
        /// </summary>
        /// <param name="dockControl">The <see cref="DockControl"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockControl"/> is <see langword="null"/>.
        /// </exception>
        internal FloatWindow(DockControl dockControl)
        {
            if (dockControl == null)
                throw new ArgumentNullException(nameof(dockControl));

            DockControl = dockControl;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="Window.SourceInitialized"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            // Add hook to intercept window messages.
            _hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            _hwndSource?.AddHook(WindowMessageHandler);

            base.OnSourceInitialized(e);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _contentPresenter = null;

            base.OnApplyTemplate();

            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            Width = ActualWidth;
            Height = ActualHeight;
            SizeToContent = SizeToContent.Manual;
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            // Remove hook.
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WindowMessageHandler);
                _hwndSource = null;
            }
        }


        /// <summary>
        /// Raises the <see cref="Window.Activated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            LastActivation = DateTime.UtcNow;
            base.OnActivated(e);
        }


        /// <summary>
        /// Raises the <see cref="Window.Closing"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="CancelEventArgs"/> instance containing the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnClosing(CancelEventArgs e)
        {
            // Raise Closing event.
            base.OnClosing(e);

            if (e.Cancel)
                return;

            var floatWindowVM = DataContext as IFloatWindow;
            if (floatWindowVM != null && floatWindowVM.IsVisible)
            {
                var dockStrategy = DockControl?.DockStrategy;
                if (dockStrategy != null && !dockStrategy.IsBusy)
                    e.Cancel = !dockStrategy.CanClose(floatWindowVM);
            }
        }


        /// <summary>
        /// Raises the <see cref="Window.Closed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            // Activating the owner is necessary, otherwise the owner window flickers or is
            // minimized sometimes.
            var ownerWindow = Owner;
            if (ownerWindow != null && !ownerWindow.IsActive)
                ownerWindow.Activate();

            // Raise Closed event.
            base.OnClosed(e);

            var floatWindowVM = DataContext as IFloatWindow;
            if (floatWindowVM != null && floatWindowVM.IsVisible)
            {
                var dockStrategy = DockControl?.DockStrategy;
                if (dockStrategy != null && !dockStrategy.IsBusy)
                    dockStrategy.Close(floatWindowVM);
            }
        }


        /// <summary>
        /// Handles Win32 window messages.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The message ID. </param>
        /// <param name="wParam">The message's wParam value.</param>
        /// <param name="lParam">The message's lParam value.</param>
        /// <param name="handled">
        /// A value that indicates whether the message was handled. Set the value to 
        /// <see langword="true"/> if the message was handled; otherwise, <see langword="false"/>.
        /// </param>
        /// <returns>
        /// The appropriate return value depends on the particular message. See the message 
        /// documentation details for the Win32 message being handled. 
        /// </returns>
        private IntPtr WindowMessageHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;

            Debug.Assert(DockControl != null);
            Debug.Assert(DockControl.DragManager != null);

            var dragManager = DockControl.DragManager;
            switch (msg)
            {
                case WindowMessages.WM_ENTERSIZEMOVE:
                    // Window enters sizing or moving modal loop.

                    // When the window is maximized (aero snap), WM_ENTERSIZEMOVE is followed by
                    // WM_SIZING. --> Do not exit modal loop on WM_SIZING.
                    _ignoreSizingMessage = WindowState == WindowState.Maximized;
                    dragManager.OnFloatWindowEnterMove(this);
                    break;

                case WindowMessages.WM_MOVING:
                    // In moving modal loop.
                    dragManager.OnFloatWindowMove(this);
                    break;

                case WindowMessages.WM_SIZING:
                    if (!_ignoreSizingMessage)
                        dragManager.OnFloatWindowExitMove(this);
                    break;

                case WindowMessages.WM_EXITSIZEMOVE:
                    // Window exits size/move modal loop.
                    dragManager.OnFloatWindowExitMove(this);
                    break;
            }

            return IntPtr.Zero;
        }
        #endregion
    }
}
