// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Input;
using DigitalRune.Collections;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Properties;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using DigitalRune.Windows.Themes;
using NLog;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Provides an user interface to find and replace text strings in documents.
    /// </summary>
    /// <remarks>
    /// Services provided by this extension: <see cref="ISearchService"/>.
    /// </remarks>
    public sealed partial class SearchExtension : EditorExtension, ISearchService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDocumentService _documentService;
        private ResourceDictionary _resourceDictionary;
        private MergeableNodeCollection<ICommandItem> _menuNodes;
        private MergeableNodeCollection<ICommandItem> _toolBarNodes;
        private CommandBinding _quickFindCommandBinding;
        private QuickFindCommandItem _quickFindCommandItem;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private FindAndReplaceViewModel FindAndReplaceViewModel
        {
            get
            {
                if (_findAndReplaceViewModel == null)
                    _findAndReplaceViewModel = new FindAndReplaceViewModel(this);

                return _findAndReplaceViewModel;
            }
        }
        private FindAndReplaceViewModel _findAndReplaceViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchExtension"/> class.
        /// </summary>
        public SearchExtension()
        {
            InitializeSearchService();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            Editor.Services.Register(typeof(ISearchService), null, this);
        }


        /// <inheritdoc/>
        protected override void OnStartup()
        {
            _documentService = Editor.Services.GetInstance<IDocumentService>();

            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddToolBars();

            // AddCommandBindings is called after window was loaded.
            Editor.Activated += OnEditorActivated;

            if (_documentService != null)
                _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;

            LoadSettings();
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            SaveSettings();                 // Save last used search patterns.

            Editor.ActiveDockTabItemChanged -= OnActiveDocumentChanged;

            if (_documentService != null)
                _documentService.ActiveDocumentChanged -= OnActiveDocumentChanged;

            RemoveCommandBindings();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();

            _documentService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.Unregister(typeof(ISearchService));
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Search/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddCommands()
        {
            _quickFindCommandItem = new QuickFindCommandItem(this);

            CommandItems.Add(
                new RoutedCommandItem(ApplicationCommands.Find)
                {
                    Category = CommandCategories.Search,
                    Text = "Quick _find",
                    ToolTip = "Go to Quick Find box.",
                },
                _quickFindCommandItem,
                new DelegateCommandItem("FindAndReplace", new DelegateCommand(ShowFindAndReplace))
                {
                    Category = CommandCategories.Search,
                    Icon = MultiColorGlyphs.Find,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F, ModifierKeys.Control | ModifierKeys.Shift) },
                    Text = "F_ind and replace...",
                    ToolTip = "Show the Find and Replace window.",
                },
                new DelegateCommandItem("FindPrevious", new DelegateCommand(FindPrevious, CanFind))
                {
                    Category = CommandCategories.Search,
                    Icon = MultiColorGlyphs.FindPrevious,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F3, ModifierKeys.Shift) },
                    Text = "Find previous",
                    ToolTip = "Jump to previous search result."
                },
                new DelegateCommandItem("FindNext", new DelegateCommand(FindNext, CanFind))
                {
                    Category = CommandCategories.Search,
                    Icon = MultiColorGlyphs.FindNext,
                    InputGestures = new InputGestureCollection { new KeyGesture(Key.F3, ModifierKeys.None) },
                    Text = "_Find next",
                    ToolTip = "Jump to next search result."
                },
                new DelegateCommandItem("Replace", new DelegateCommand(Replace, CanFind))
                {
                    Category = CommandCategories.Search,
                    Text = "_Replace",
                    ToolTip = "Replace current search result."
                },
                new DelegateCommandItem("ReplaceAll", new DelegateCommand(ReplaceAll, CanFind))
                {
                    Category = CommandCategories.Search,
                    Text = "Replace _all",
                    ToolTip = "Replace all search results."
                }
            );
        }


        private void RemoveCommands()
        {
            CommandItems.Clear();
            _quickFindCommandItem = null;
        }


        private void AddMenus()
        {
            var insertBeforeSearchSeparator = new MergePoint(MergeOperation.InsertBefore, "SearchSeparator");
            _menuNodes = new MergeableNodeCollection<ICommandItem>
            {
                new MergeableNode<ICommandItem>(new CommandGroup("EditGroup", "_Edit"), 
                    new MergeableNode<ICommandItem>(new CommandGroup("FindGroup", "_Find and replace"), insertBeforeSearchSeparator, MergePoint.Append,
                        new MergeableNode<ICommandItem>(CommandItems["Find"]),
                        new MergeableNode<ICommandItem>(CommandItems["FindAndReplace"]),
                        new MergeableNode<ICommandItem>(CommandItems["FindNext"]),
                        new MergeableNode<ICommandItem>(CommandItems["FindPrevious"]))),
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
                new MergeableNode<ICommandItem>(new CommandGroup("FindGroup", "Find"),
                    new MergeableNode<ICommandItem>(CommandItems["QuickFind"]),
                    new MergeableNode<ICommandItem>(CommandItems["FindNext"])),
                new MergeableNode<ICommandItem>(new CommandGroup("ViewGroup"),
                    new MergeableNode<ICommandItem>(CommandItems["FindAndReplace"])),
            };

            Editor.ToolBarNodeCollections.Add(_toolBarNodes);
        }


        private void RemoveToolBars()
        {
            Editor.ToolBarNodeCollections.Remove(_toolBarNodes);
            _toolBarNodes = null;
        }


        private void AddCommandBindings()
        {
            _quickFindCommandBinding = new CommandBinding(ApplicationCommands.Find, (s, e) => ShowQuickFind());
            Editor.Window.CommandBindings.Add(_quickFindCommandBinding);
        }


        private void RemoveCommandBindings()
        {
            Editor.Window?.CommandBindings.Remove(_quickFindCommandBinding);
            _quickFindCommandBinding = null;
        }


        private void LoadSettings()
        {
            _recentFindPatterns.Clear();
            _recentReplacePatterns.Clear();

            try
            {
                var settings = Settings.Default.SearchSettings;
                if (settings != null)
                {
                    _recentFindPatterns.AddRange(settings.RecentFindPatterns);
                    _recentReplacePatterns.AddRange(settings.RecentReplacePatterns);
                    Query.MatchCase = settings.MatchCase;
                    Query.MatchWholeWord = settings.MatchWholeWord;
                    Query.Mode = settings.Mode;
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception, "Could not load search settings.");
            }
        }


        private void SaveSettings()
        {
            Settings.Default.SearchSettings = new SearchSettings
            {
                RecentFindPatterns = _recentFindPatterns.AsStringCollection(),
                RecentReplacePatterns = _recentReplacePatterns.AsStringCollection(),
                MatchCase = Query.MatchCase,
                MatchWholeWord = Query.MatchWholeWord,
                Mode = Query.Mode,
            };
        }


        private void OnEditorActivated(object sender, ActivationEventArgs eventArgs)
        {
            AddCommandBindings();
            Editor.Activated -= OnEditorActivated;
        }


        private void OnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            InvalidateCommands();
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            if (dockId == FindAndReplaceViewModel.DockId)
                return FindAndReplaceViewModel;

            return null;
        }
        #endregion
    }
}
