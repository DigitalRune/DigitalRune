// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Editor.Options;
using DigitalRune.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Commands
{
    /// <summary>
    /// Adds the basic menu/toolbar structure and adds functionality to customize commands.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed partial class CommandExtension : EditorExtension
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private MergeableNodeCollection<OptionsPageViewModel> _optionsNodes;
        private ToolBarsCommandItem _toolBarsCommandItem;
        private bool _toolBarStateLoaded;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandExtension"/> class.
        /// </summary>
        public CommandExtension()
        {
            // The core extension must be configured first!
            Priority = int.MaxValue;

            // Set input gestures for default routed commands.
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.OemPlus));
            NavigationCommands.IncreaseZoom.InputGestures.Add(new KeyGesture(Key.Add));      // + on number pad
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.OemMinus));
            NavigationCommands.DecreaseZoom.InputGestures.Add(new KeyGesture(Key.Subtract)); // - on number pad
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddToolBars();
            AddOptions();

            // The ToolBarsCommandItem controls EditorViewModel.ToolBarsContextMenu.
            // The submenu is built in CreateMenuItem.
            _toolBarsCommandItem.CreateMenuItem();

            // Add Quick Launch items and load toolbar states after menus and toolbars have
            // been created.
            Editor.UIInvalidated += OnUIInvalidated;
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            SaveToolBarStates();
            _toolBarStateLoaded = false;

            Editor.UIInvalidated -= OnUIInvalidated;

            RemoveQuickLaunchItems();

            // Remove toolbar context menu items. (Just be lazy and simply delete all not only ours!)
            Editor.ToolBarContextMenu.Clear();

            RemoveOptions();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Commands/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddCommands()
        {
            _toolBarsCommandItem = new ToolBarsCommandItem(this);

            CommandItems.Add(
                new DelegateCommandItem("Exit", new DelegateCommand(() => Editor.Exit()))
                {
                    Category = CommandCategories.File,
                    Icon = MultiColorGlyphs.Exit,
                    Text = "E_xit",
                    ToolTip = "Close this application."
                },
                new RoutedCommandItem(ApplicationCommands.Undo)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Undo,
                    ToolTip = "Undo the last operation.",
                },
                new RoutedCommandItem(ApplicationCommands.Redo)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Redo,
                    ToolTip = "Perform the last undone operation.",
                },
                new RoutedCommandItem(ApplicationCommands.Cut)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Cut,
                    ToolTip = "Remove selected item and copy it to the clipboard.",
                },
                new RoutedCommandItem(ApplicationCommands.Copy)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Copy,
                    ToolTip = "Copy selected item to the clipboard.",
                },
                new RoutedCommandItem(ApplicationCommands.Paste)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Paste,
                    ToolTip = "Paste the content of the clipboard into the active document.",
                },
                new RoutedCommandItem(ApplicationCommands.Delete)
                {
                    Category = CommandCategories.Edit,
                    Icon = MultiColorGlyphs.Delete,
                    ToolTip = "Delete the selected item.",
                },
                new RoutedCommandItem(ApplicationCommands.SelectAll)
                {
                    Category = CommandCategories.Edit,
                    Text = "Select all",
                    ToolTip = "Select all items of the current document.",
                },
                _toolBarsCommandItem,
                new DelegateCommandItem("ShowAllToolBars", _toolBarsCommandItem.ToggleAllToolBarsCommand)
                {
                    Category = CommandCategories.View,
                    CommandParameter = Boxed.BooleanTrue,   // true = make visible.
                    Text = "_Show all",
                    ToolTip = "Show all toolbars."
                },
                new DelegateCommandItem("HideAllToolBars", _toolBarsCommandItem.ToggleAllToolBarsCommand)
                {
                    Category = CommandCategories.View,
                    CommandParameter = Boxed.BooleanFalse,  // false = make invisible
                    Text = "_Hide all",
                    ToolTip = "Hide all toolbars."
                });
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();

            _toolBarsCommandItem.Dispose();
            _toolBarsCommandItem = null;
        }


        private void AddMenus()
        {
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                // File menu
                new MergeableNode<ICommandItem>(new CommandGroup("FileGroup", "_File"),
                    new MergeableNode<ICommandItem>(new CommandSeparator("OpenSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("CloseSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("SaveSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("PrintSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["Exit"])),

                // Edit menu
                new MergeableNode<ICommandItem>(new CommandGroup("EditGroup", "_Edit"),
                    new MergeableNode<ICommandItem>(CommandItems["Undo"]),
                    new MergeableNode<ICommandItem>(CommandItems["Redo"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("UndoSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["Cut"]),
                    new MergeableNode<ICommandItem>(CommandItems["Copy"]),
                    new MergeableNode<ICommandItem>(CommandItems["Paste"]),
                    new MergeableNode<ICommandItem>(CommandItems["Delete"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("ClipboardSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["SelectAll"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("SelectSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("SearchSeparator"))),

                // View menu
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup", "_View"),
                    new MergeableNode<ICommandItem>(_toolBarsCommandItem),
                    new MergeableNode<ICommandItem>(new CommandSeparator("GuiSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("WindowSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("DocumentSeparator"))),
        
                // Tools menu
                new MergeableNode<ICommandItem>(new CommandGroup("ToolsGroup", "_Tools"),
                    new MergeableNode<ICommandItem>(new CommandSeparator("ToolsSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("OptionsSeparator"))),

                // Window menu
                new MergeableNode<ICommandItem>(new CommandGroup("WindowGroup", "_Window"),
                    new MergeableNode<ICommandItem>(new CommandSeparator("WindowSpecificSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("CloseSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("DockSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("WindowManagementSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("WindowListSeparator"))),
    
                // Help menu
                new MergeableNode<ICommandItem>(new CommandGroup("HelpGroup", "_Help")),
            };

            Editor.MenuNodeCollections.Add(_menuNodes);
        }


        private void RemoveMenus()
        {
            Editor.MenuNodeCollections.Remove(_menuNodes);
            _menuNodes = null;
        }


        private void AddToolBars()
        {
            _toolBarNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("StandardGroup", "Standard"),
                    new MergeableNode<ICommandItem>(new CommandSeparator("FileSeparator")),
                    new MergeableNode<ICommandItem>(new CommandSeparator("PrintSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["Cut"]),
                    new MergeableNode<ICommandItem>(CommandItems["Copy"]),
                    new MergeableNode<ICommandItem>(CommandItems["Paste"]),
                    new MergeableNode<ICommandItem>(CommandItems["Delete"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("ClipboardSeparator")),
                    new MergeableNode<ICommandItem>(CommandItems["Undo"]),
                    new MergeableNode<ICommandItem>(CommandItems["Redo"]),
                    new MergeableNode<ICommandItem>(new CommandSeparator("UndoSeparator"))),
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup", "View")),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        private void AddOptions()
        {
            _optionsNodes = new MergeableNodeCollection<OptionsPageViewModel>
            {
                new MergeableNode<OptionsPageViewModel>(new OptionsGroupViewModel("General"), new MergePoint(MergeOperation.Prepend),
                    new MergeableNode<OptionsPageViewModel>(new ShortcutsOptionsPageViewModel(Editor))),
            };

            var optionsService = Editor.Services.GetInstance<IOptionsService>().WarnIfMissing();
            optionsService?.OptionsNodeCollections.Add(_optionsNodes);
        }


        private void RemoveOptions()
        {
            Editor.Services.GetInstance<IOptionsService>()?.OptionsNodeCollections.Remove(_optionsNodes);
            _optionsNodes = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void OnUIInvalidated(object sender, EventArgs eventArgs)
        {
            // We need to load toolbar states only once. Then the editor will automatically keep
            // the band index and visibility infos.
            if (!_toolBarStateLoaded)
            {
                _toolBarStateLoaded = true;
                LoadToolBarStates();
                _toolBarsCommandItem.UpdateMenuItems();
            }

            RemoveQuickLaunchItems();
            AddQuickLaunchItems();
        }
        #endregion
    }
}
