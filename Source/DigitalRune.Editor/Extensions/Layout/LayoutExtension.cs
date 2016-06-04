// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Editor.Properties;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Adds functions to persist and change the window layout.
    /// </summary>
    public sealed partial class LayoutExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IWindowService _windowService;
        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        //private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private MergeableNodeCollection<ICommandItem> _contextMenuNodes;
        private object _captionBarItem;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            AddCommandLineArguments();

            Editor.Services.RegisterView(typeof(SaveLayoutViewModel), typeof(SaveLayoutView));
            Editor.Services.RegisterView(typeof(ManageLayoutsViewModel), typeof(ManageLayoutsWindow));
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _windowService = Editor.Services.GetInstance<IWindowService>().ThrowIfMissing();

            AddDataTemplates();
            AddCommands();
            AddMenus();
            //AddToolBars();
            AddDockContextMenu();
            AddCaptionBarItems();

            InitializeLayouts();

            // Call LoadDockControlLayout when editor is activated. We cannot call it right now
            // because some extensions include their dock windows, which are not initialized yet.
            Editor.Activated += OnEditorActivated;
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            Editor.Activated -= OnEditorActivated;

            UninitializeLayouts();

            RemoveCaptionBarItems();
            RemoveDockContextMenu();
            RemoveMenus();
            //RemoveToolBars();
            RemoveCommands();
            RemoveDataTemplates();

            _windowService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.UnregisterView(typeof(SaveLayoutViewModel));
            Editor.Services.UnregisterView(typeof(ManageLayoutsViewModel));

            RemoveCommandLineArguments();
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Layout/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddCommands()
        {
            CommandItems.Add(
                new RoutedCommandItem(DockCommands.AutoHide)
                {
                    Category = CommandCategories.Window,
                    Text = "_Auto hide",
                    ToolTip = "Move the current window to the auto-hide pane.",
                },
                new RoutedCommandItem(DockCommands.Dock)
                {
                    Category = CommandCategories.Window,
                    Text = "_Dock",
                    ToolTip = "Dock the current window.",
                },
                new RoutedCommandItem(DockCommands.Float)
                {
                    Category = CommandCategories.Window,
                    Text = "_Float",
                    ToolTip = "Undock the current window and move it into a floating window.",
                },
                new RoutedCommandItem(ApplicationCommands.Close)
                {
                    Category = CommandCategories.Window,
                    Text = "_Close",
                    ToolTip = "Close the current window.",
                },
                new WindowLayoutItem(this),
                new DelegateCommandItem("SaveWindowLayout", new DelegateCommand(SavePreset, CanSavePreset))
                {
                    Category = CommandCategories.Window,
                    Text = "_Save layout",
                    ToolTip = "Save the current window layout."
                },
                new DelegateCommandItem("SaveWindowLayoutAs", new DelegateCommand(SavePresetAs))
                {
                    Category = CommandCategories.Window,
                    Text = "Save layout _as...",
                    ToolTip = "Save the current window layout as a new preset."
                },
                new DelegateCommandItem("ManageWindowLayouts", new DelegateCommand(ManageWindowLayouts))
                {
                    Category = CommandCategories.Window,
                    Text = "_Manage layouts...",
                    ToolTip = "Rename or delete the stored window layouts."
                },
                new DelegateCommandItem("ResetWindowLayout", new DelegateCommand(ResetWindowLayout, CanResetWindowLayout))
                {
                    Category = CommandCategories.Window,
                    Text = "_Reset layout",
                    ToolTip = "Reset the active window layout."
                });

            // Add input gestures to standard RoutedUICommands.
            ApplicationCommands.Close.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
        }


        private void AddMenus()
        {
            var insertBeforeCloseSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "CloseSeparator"), MergePoint.Append };
            var insertBeforeDockSeparator = new [] { new MergePoint(MergeOperation.InsertBefore, "DockSeparator"), MergePoint.Append };
            var insertBeforeWindowManagementSeparator = new[] { new MergePoint(MergeOperation.InsertBefore, "WindowManagementSeparator"), MergePoint.Append };

            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("WindowGroup", "_Window"),
                    new MergeableNode<ICommandItem>(CommandItems["Close"], insertBeforeCloseSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["AutoHide"], insertBeforeDockSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Dock"], insertBeforeDockSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["Float"], insertBeforeDockSeparator),
                    new MergeableNode<ICommandItem>(CommandItems["WindowLayout"], insertBeforeWindowManagementSeparator)),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        //private void AddToolBars()
        //{
        //    _toolBarNodes = new MergeableNodeCollection<ICommandItem>
        //    {
        //        new MergeableNode<ICommandItem>(new CommandGroup("LayoutGroup", "Layout"),
        //            new MergeableNode<ICommandItem>(CommandItems["WindowLayout"])),
        //    };

        //    Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        //}


        //private void RemoveToolBars()
        //{
        //    Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
        //    _toolBarNodes = null;
        //}


        private void AddDockContextMenu()
        {
            _contextMenuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(CommandItems["Close"]),
                new MergeableNode<ICommandItem>(new CommandSeparator("FileSeparator")),
                new MergeableNode<ICommandItem>(CommandItems["AutoHide"]),
                new MergeableNode<ICommandItem>(CommandItems["Dock"]),
                new MergeableNode<ICommandItem>(CommandItems["Float"]),
                new MergeableNode<ICommandItem>(new CommandSeparator("DockSeparator")),
            };

            Editor.DockContextMenuNodeCollections.Add(_contextMenuNodes);
        }


        private void RemoveDockContextMenu()
        {
            Editor.DockContextMenuNodeCollections.Remove(_contextMenuNodes);
            _contextMenuNodes = null;
        }


        private void AddCaptionBarItems()
        {
            // Note that we could use the ToolBarDropDownButtonViewModel. But when we use the
            // default data template the TextBlock.Foreground is invalid after theme changes.
            // The data template works inside a toolbar, but not in the caption bar. (WPF bug!)
            //_captionBarItem = ((WindowLayoutItem)CommandItems["WindowLayout"]).CreateToolBarItem();

            _captionBarItem = ((WindowLayoutItem)CommandItems["WindowLayout"]).CreateCaptionBarItem();
            Editor.CaptionBarItemsRight.Add(_captionBarItem);
        }


        private void RemoveCaptionBarItems()
        {
            Editor.CaptionBarItemsRight.Remove(_captionBarItem);
            _captionBarItem = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void OnEditorActivated(object sender, ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                Editor.Activated -= OnEditorActivated;

                // Load EditorWindow state.
                LoadEditorWindowState();

                // Check if a layout was requested using the --layout command line parameter.
                var layoutName = GetLayoutNameFromCommandLine();
                WindowLayout layout = null;
                if (layoutName != null)
                {
                    layout = Layouts.FirstOrDefault(l => l.Name == layoutName);
                    if (layout == null)
                        Logger.Error(Invariant($"The layout \"{layoutName}\" was not found. This layout was requested using a command line parameter."));
                }

                if (layout == null)
                {
                    // Load previous window layout.
                    layout = Layouts.FirstOrDefault(l => l.Name == Settings.Default.ActiveWindowLayout)
                             ?? Layouts.FirstOrDefault();
                }
                SwitchLayout(layout);

                if (Editor.Window != null)
                {
                    // Save EditorWindow state and window layout before window is closed.
                    Editor.Window.Closing += (s, e) =>
                                             {
                                                 SaveEditorWindowState();
                                                 SaveLayouts();
                                             };
                }
            }
        }


        /// <summary>
        /// Loads the state of the <see cref="EditorWindow"/>. (Needs to be called after the window
        /// was created!)
        /// </summary>
        private void LoadEditorWindowState()
        {
            Logger.Debug("Loading main window state.");

            // ----- Load position from settings. Make sure the window is on the screen.

            // A safety distance.
            const double safety = 30;

            // Screen limits.
            double screenLeft = SystemParameters.VirtualScreenLeft;
            double screenRight = screenLeft + SystemParameters.VirtualScreenWidth;
            double screenTop = SystemParameters.VirtualScreenTop;
            double screenBottom = screenTop + SystemParameters.VirtualScreenHeight;

            // Saved window location and size.
            double left = Settings.Default.WindowLeft;
            double top = Settings.Default.WindowTop;
            double width = Settings.Default.WindowWidth;
            double height = Settings.Default.WindowHeight;
            double right = left + width;
            double bottom = top + height;

            // Make sure the window is visible.
            if (width < safety)
                width = double.NaN;

            if (height < safety)
                height = double.NaN;

            if (width > 2 * SystemParameters.VirtualScreenWidth)
                width = double.NaN;

            if (height > 2 * SystemParameters.VirtualScreenHeight)
                height = double.NaN;

            if (left > screenRight - safety || (left < screenLeft && right < screenLeft + safety))
                left = double.NaN;

            if (top > screenBottom - safety || (top < screenTop && bottom < screenTop + safety))
                top = double.NaN;

            var window = Editor.Window;
            if (window != null)
            {
                window.Left = left;
                window.Top = top;
                window.Width = width;
                window.Height = height;
                window.WindowStartupLocation = WindowStartupLocation.Manual;

                // Restore window state - Minimized is not allowed.
                if (Settings.Default.WindowState != WindowState.Minimized)
                    window.WindowState = Settings.Default.WindowState;
            }
        }


        /// <summary>
        /// Saves the state of the <see cref="EditorWindow"/>. (Needs to be called before the window
        /// is closed!)
        /// </summary>
        private void SaveEditorWindowState()
        {
            Logger.Debug("Saving main window state.");

            var window = Editor.Window;
            if (window != null)
            {
                var bounds = window.RestoreBounds;
                if (!bounds.IsEmpty)
                {
                    Settings.Default.WindowLeft = bounds.Left;
                    Settings.Default.WindowTop = bounds.Top;
                    Settings.Default.WindowWidth = bounds.Width;
                    Settings.Default.WindowHeight = bounds.Height;
                }

                Settings.Default.WindowState = window.WindowState;
            }
        }
        #endregion
    }
}
