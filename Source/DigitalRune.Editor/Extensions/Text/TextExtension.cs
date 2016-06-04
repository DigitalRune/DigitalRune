// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using DigitalRune.Editor.Text.Search;
using DigitalRune.Editor.About;
using DigitalRune.Editor.Documents;
using DigitalRune.Editor.Search;
using DigitalRune.Linq;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using NLog;
using static System.FormattableString;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Provides functions for editing text documents.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed partial class TextExtension : EditorExtension, ITextService
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IDocumentService _documentService;
        private IWindowService _windowService;
        private EditorExtensionDescription _extensionDescription;
        private ResourceDictionary _resourceDictionary;
        private TextDocumentFactory _textDocumentFactory;
        private CurrentSelectionSearchScope _currentSelectionSearchScope;
        private HighlightingManager _highlightingManager;

        private TextEditorStatusViewModel _statusInfo;      // Shows line and column in status bar.
        private IDisposable _statusInfoUpdateSubscription;  // Updates the visibility of the _statusInfo.
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TextExtension"/> class.
        /// </summary>
        public TextExtension()
        {
            _menuManager = new MenuManager();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            // Override AvalonEdits highlighting manager.
            _highlightingManager = new HighlightingManager(Editor);
            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.SetHighlightingManager(_highlightingManager);

            // Register services.
            Editor.Services.Register(typeof(ITextService), null, this);
            Editor.Services.Register(typeof(IHighlightingService), null, _highlightingManager);

            // Register views.
            Editor.Services.RegisterView(typeof(GoToLineViewModel), typeof(GoToLineView));
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void OnStartup()
        {
            _documentService = Editor.Services.GetInstance<IDocumentService>().ThrowIfMissing();
            _windowService = Editor.Services.GetInstance<IWindowService>().ThrowIfMissing();

            Options = new TextEditorOptions();
            LoadOptions();

            AddExtensionDescription();
            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddContextMenu();
            AddOptions();
            AddStatusBarItems();
            AddDocumentFactories();
            AddSearchScopes();

            Editor.PropertyChanged += OnEditorPropertyChanged;
            Editor.UIInvalidated += OnEditorUIInvalidated;
            _documentService.ActiveDocumentChanged += OnActiveDocumentChanged;

            ShowCommands(false);
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            _documentService.ActiveDocumentChanged -= OnActiveDocumentChanged;
            Editor.UIInvalidated -= OnEditorUIInvalidated;
            Editor.PropertyChanged += OnEditorPropertyChanged;

            RemoveSearchScopes();
            RemoveDocumentFactories();
            RemoveStatusBarItems();
            RemoveOptions();
            RemoveContextMenu();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();
            RemoveExtensionDescription();

            SaveOptions();
            Options = null;

            _documentService = null;
            _windowService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            Editor.Services.UnregisterView(typeof(GoToLineViewModel));

            Editor.Services.Unregister(typeof(ITextService));
            Editor.Services.Unregister(typeof(IHighlightingService));

            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.SetHighlightingManager(ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            _highlightingManager = null;
        }


        private void AddExtensionDescription()
        {
            var aboutService = Editor.Services.GetInstance<IAboutService>();
            if (aboutService != null)
            {
                // Get version of current assembly.
                var version = Assembly.GetAssembly(typeof(TextExtension)).GetName().Version;
                _extensionDescription = new EditorExtensionDescription
                {
                    Name = "DigitalRune Text Extension",
                    Description = "The DigitalRune Text Extension provides functions for editing text files." + LegalInfo.Text,
                    Version = Invariant($"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}"),
                    //Icon = new BitmapImage(new Uri("pack://application:,,,/DigitalRune.Editor;component/Resources/Images/TextEditor.ico", UriKind.RelativeOrAbsolute)),
                };
                aboutService.ExtensionDescriptions.Add(_extensionDescription);
            }
        }


        private void RemoveExtensionDescription()
        {
            var aboutService = Editor.Services.GetInstance<IAboutService>();
            if (aboutService != null)
            {
                aboutService.ExtensionDescriptions.Remove(_extensionDescription);
                _extensionDescription = null;
            }
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Text/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            Application.Current.Resources.MergedDictionaries.Add(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            Application.Current.Resources.MergedDictionaries.Remove(_resourceDictionary);
            _resourceDictionary = null;
        }


        private void AddStatusBarItems()
        {
            _statusInfo = new TextEditorStatusViewModel { IsVisible = false };
            Editor.StatusBarItemsRight.Insert(0, _statusInfo);

            // Observe the IDocumentService.ActiveDocumentChanged event and show/hide our status info.
            var documentService = _documentService;
            _statusInfoUpdateSubscription =
                Observable.FromEventPattern<EventArgs>(
                              h => documentService.ActiveDocumentChanged += h, 
                              h => documentService.ActiveDocumentChanged -= h)
                          .Select(eventPattern => (IDocumentService)eventPattern.Sender)
                          .Select(ds => ds.ActiveDocument as TextDocument)
                          .Subscribe(textDocument => _statusInfo.IsVisible = (textDocument != null));
        }


        private void RemoveStatusBarItems()
        {
            _statusInfoUpdateSubscription.Dispose();
            _statusInfoUpdateSubscription = null;

            Editor.StatusBarItemsRight.Remove(_statusInfo);
            _statusInfo = null;
        }


        /// <summary>
        /// Sets the information in the status bar.
        /// </summary>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        /// <param name="character">The character number.</param>
        /// <param name="overstrike">
        /// <see langword="true"/> if overstrike is active; otherwise <see langword="false"/> if
        /// insert is active.
        /// </param>
        public void SetStatusInfo(int line, int column, int character, bool overstrike)
        {
            // This method is called by the TextDocumentViewModel to show the new caret position
            // in the status bar.
            _statusInfo.Line = line;
            _statusInfo.Column = column;
            _statusInfo.Character = character;
            _statusInfo.Insert = !overstrike;
            _statusInfo.Overstrike = overstrike;
        }


        private void AddDocumentFactories()
        {
            _textDocumentFactory = new TextDocumentFactory(Editor);
            _documentService.Factories.Add(_textDocumentFactory);
        }


        private void RemoveDocumentFactories()
        {
            _documentService.Factories.Remove(_textDocumentFactory);
            _textDocumentFactory = null;
        }


        private void AddSearchScopes()
        {
            var searchService = Editor.Services.GetInstance<ISearchService>();
            if (searchService != null)
            {
                _currentSelectionSearchScope = new CurrentSelectionSearchScope(_documentService);
                searchService.SearchScopes.Insert(0, _currentSelectionSearchScope);
            }
        }


        private void RemoveSearchScopes()
        {
            var searchService = Editor.Services.GetInstance<ISearchService>();
            if (searchService != null && _currentSelectionSearchScope != null)
                searchService.SearchScopes.Remove(_currentSelectionSearchScope);

            _currentSelectionSearchScope = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            return null;
        }


        private void OnEditorPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // Invalidate all DelegateCommands.
            CommandItems.OfType<DelegateCommandItem>()
                        .Select(item => item.Command)
                        .ForEach(command => command.RaiseCanExecuteChanged());

            UpdateSyntaxHighlightingItem();
        }


        private void OnEditorUIInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateContextMenu();
        }


        private void OnActiveDocumentChanged(object sender, EventArgs eventArgs)
        {
            ShowCommands(_documentService.ActiveDocument is TextDocument);
        }


        internal void UpdateSyntaxHighlightingItem()
        {
            _syntaxHighlightingItem?.Update();
        }
        #endregion
    }
}
