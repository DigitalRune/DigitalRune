// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DigitalRune.Windows;
using DigitalRune.Windows.Controls;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Interop;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents the main window of the application.
    /// </summary>
    public partial class EditorWindow
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly BooleanToVisibilityConverter BooleanToVisibilityConverter = new BooleanToVisibilityConverter();

        private bool _menuOrToolBarClicked;
        private EditorViewModel _editorViewModel;
        private IDisposable _focusMessageSubscription;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorWindow"/> class.
        /// </summary>
        public EditorWindow()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            // Inject view into view model.

            var editorViewModel = DataContext as EditorViewModel;

            if (_editorViewModel != null)
            {
                _editorViewModel.Window = null;
                _editorViewModel.DockControl = null;
            }

            _editorViewModel = editorViewModel;

            if (editorViewModel != null)
            {
                _editorViewModel.Window = this;
                _editorViewModel.DockControl = DockControl;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            if (_editorViewModel == null)
                throw new EditorException("EditorWindow.DataContext must be an EditorViewModel.");

            _editorViewModel.UIInvalidated += OnUIInvalidated;
            OnUIInvalidated(null, null);

            var messageBus = _editorViewModel.Services.GetInstance<IMessageBus>();
            if (messageBus != null)
            {
                _focusMessageSubscription = messageBus.Listen<FocusMessage>()
                                                      .Subscribe(m => Focus(m.DataContext));
            }
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            _focusMessageSubscription?.Dispose();
            _focusMessageSubscription = null;

            _editorViewModel.UIInvalidated -= OnUIInvalidated;
            _editorViewModel.Window = null;
            
            ClearToolBars();
        }


        private void OnUIInvalidated(object sender, EventArgs eventArgs)
        {
            // Create toolbars. Unfortunately, WPF toolbars cannot be created from data templates. :-(
            ClearToolBars();
            AddToolBars();
        }


        private void ClearToolBars()
        {
            ToolBarTray.ToolBars.Clear();
        }


        private void AddToolBars()
        {
            foreach (var toolBarViewModel in _editorViewModel.ToolBars)
            {
                var toolBar = new ToolBarEx { DataContext = toolBarViewModel };
                toolBar.SetBinding(NameProperty, "CommandGroup.Name");
                if (toolBarViewModel.CommandGroup.AlwaysShowText)
                {
                    Binding binding = new Binding("CommandGroup.Text") { Converter = LabelToTextConverter.Instance };
                    toolBar.SetBinding(HeaderedItemsControl.HeaderProperty, binding);
                }

                toolBar.SetBinding(ToolBar.BandProperty, new Binding(nameof(ToolBarViewModel.Band)) { Mode = BindingMode.TwoWay });
                toolBar.SetBinding(ToolBar.BandIndexProperty, new Binding(nameof(ToolBarViewModel.BandIndex)) { Mode = BindingMode.TwoWay });
                toolBar.SetBinding(VisibilityProperty, new Binding(nameof(ToolBarViewModel.ActualIsVisible)) { Converter = BooleanToVisibilityConverter });

                toolBar.SetBinding(ItemsControl.ItemsSourceProperty, "Items");

                ToolBarTray.ToolBars.Add(toolBar);
            }
        }


        #region ----- Focus Management -----

        // The main window requires a special focus management. By default, a WPF window is
        // activated when the menu or a toolbar item is clicked. However, when documents are shown
        // in floating windows the menu items and toolbar items should be clickable, but must not
        // steel the focus from the floating window.

        /// <summary>
        /// Raises the <see cref="Window.SourceInitialized"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hook = new HwndSourceHook(FilterMessage);
            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            Debug.Assert(hwndSource != null, "Unable to retrieve HWND of main window.");
            hwndSource.AddHook(hook);
        }


        private IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Watch for WM_MOUSEACTIVATE events and return MA_NOACTIVATE to prevent the
            // main window from getting focus due to mouse clicks on the menu or toolbars.
            const int MA_NOACTIVATE = 3;

            switch (msg)
            {
                case WindowMessages.WM_MOUSEACTIVATE:
                    if (MenuOrToolBarClicked())
                    {
                        handled = true;
                        return new IntPtr(MA_NOACTIVATE);
                    }
                    break;
            }

            return IntPtr.Zero;
        }


        private bool MenuOrToolBarClicked()
        {
            _menuOrToolBarClicked = false;
            var mousePosition = Mouse.GetPosition(this);
            VisualTreeHelper.HitTest(this, null, HitTestCallback, new PointHitTestParameters(mousePosition));
            return _menuOrToolBarClicked;
        }


        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            var visual = result.VisualHit;
            while (visual != null)
            {
                if (visual == Menu || visual is ToolBar)
                {
                    _menuOrToolBarClicked = true;
                    return HitTestResultBehavior.Stop;
                }

                visual = VisualTreeHelper.GetParent(visual);
            }

            return HitTestResultBehavior.Continue;
        }


        /// <summary>
        /// Handles the <see cref="UIElement.PreviewGotKeyboardFocus"/> event of the menu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnPreviewMenuGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            if (!IsActive)
            {
                // Set the event to handled. This prevents WPF from activating the window due to
                // the mouse click on the menu.
                // The disadvantage is that now the menu cannot be controlled with the keyboard
                // (e.g. up/down arrow keys). Therefore, we do this only when a different window
                // is active. That means, if the editor window has focus everything is normal. If
                // a float window has focus, then we cannot use the window with the keyboard, but
                // commands are properly routed from the menu to the float window.
                // (A better solution could be this:
                // https://blogs.msdn.microsoft.com/visualstudio/2010/03/08/wpf-in-visual-studio-2010-part-3-focus-and-activation/
                // But I did not find enough information to make it work. Perhaps this concerns only
                // Win32 content in a float window.
                // A different solution would be: Do not set Handled. Instead, remember the focused
                // element in the float window. Route commands to this element. Then set focus back
                // to this element when a menu command is executed or a top menu item is closed
                // using mouse click or escape key.)
                eventArgs.Handled = true;
            }
        }


        /// <summary>
        /// Handles the <see cref="UIElement.PreviewGotKeyboardFocus"/> event of the toolbar tray 
        /// control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnPreviewToolBarTrayGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            var button = eventArgs.NewFocus as ButtonBase;
            var comboBox = eventArgs.NewFocus as ComboBox;
            if (button != null || (comboBox != null && !comboBox.IsEditable))
            {
                // Set the event to handled. This prevents WPF from activating the window due to
                // the mouse click on a toolbar button.
                eventArgs.Handled = true;
            }
        }


        /// <summary>
        /// Focuses the UI element with the specified data context.
        /// </summary>
        /// <param name="dataContext">
        /// The data context of the UI element that should receive the focus.
        /// </param>
        private void Focus(object dataContext)
        {
            if (dataContext == null)
                return;

            // Look for a focusable framework element with the given data context.
            var element = FindFocusableElement(this, dataContext);
            if (element == null)
            {
                // Check float windows.
                foreach (var floatWindow in DockControl.FloatWindows)
                {
                    element = FindFocusableElement(floatWindow, dataContext);
                    if (element != null)
                        break;
                }
            }

            element?.Focus();
        }


        private static FrameworkElement FindFocusableElement(FrameworkElement element, object dataContext)
        {
            return element.GetVisualSubtree(false)
                          .OfType<FrameworkElement>()
                          .FirstOrDefault(e => e.DataContext == dataContext)
                          ?.GetVisualSubtree(false)
                          .OfType<FrameworkElement>()
                          .FirstOrDefault(e => e.Focusable);
        }
        #endregion


        #region ----- Event Routing -----

        // Routed commands (such as Cut, Copy, Paste, ...) need to be routed from the 
        // main window to the floating window.

        private void OnPreviewCanExecute(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            // Forward all routed commands from the main window to the FloatWindows.
            var focusedElement = (DependencyObject)Keyboard.FocusedElement;
            if (focusedElement != null                        // Another element has focus...
                && GetWindow(focusedElement) is FloatWindow)  // ...which is in a FloatWindow.
            {
                var sourceElement = eventArgs.Source as DependencyObject;
                if (GetSelfOrVisualAncestor<DockControl>(sourceElement) == DockControl)
                {
                    // Routed command originated within the DockControl. These commands are
                    // local and must not be routed to FloatWindows.
                    return;
                }

                var routedCommand = eventArgs.Command as RoutedCommand;
                if (routedCommand != null)
                {
                    eventArgs.CanExecute = routedCommand.CanExecute(eventArgs.Parameter, Keyboard.FocusedElement);
                    eventArgs.Handled = true;
                }
            }
        }


        private void OnPreviewExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            // Forward all routed commands from the main window to the FloatWindows.
            var focusedElement = (DependencyObject)Keyboard.FocusedElement;
            if (focusedElement != null                 // Another element has focus...
                && GetWindow(focusedElement) != this)  // ...which is not in the main window.
            {
                var sourceElement = eventArgs.Source as DependencyObject;
                if (GetSelfOrVisualAncestor<DockControl>(sourceElement) == DockControl)
                {
                    // Routed command originated within the DockControl. These commands are
                    // local and must not be routed to FloatWindows.
                    return;
                }

                var routedCommand = eventArgs.Command as RoutedCommand;
                if (routedCommand != null)
                {
                    routedCommand.Execute(eventArgs.Parameter, Keyboard.FocusedElement);
                    eventArgs.Handled = true;
                }
            }
        }


        private static T GetSelfOrVisualAncestor<T>(DependencyObject element) where T : class
        {
            while (element != null)
            {
                T t = element as T;
                if (t != null)
                    return t;

                element = VisualTreeHelper.GetParent(element);
            }

            return null;
        }
        #endregion

        #endregion
    }
}
