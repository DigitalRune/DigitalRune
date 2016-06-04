// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using DigitalRune.Editor.Search;
using DigitalRune.Editor.Status;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using Microsoft.Win32;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Manages documents and provides a user-interface for creating, opening, and saving documents.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Services:</strong><br/>
    /// The extension adds the following services to the service container:
    /// </para>
    /// <list type="bullet">
    /// <item><see cref="IDocumentService"/></item>
    /// </list>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public sealed partial class DocumentExtension : EditorExtension, IDocumentService, IGuardClose
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private IWindowService _windowService;
        private IStatusService _statusService;
        private ISearchService _searchService;
        private readonly List<Document> _documents;

        private ResourceDictionary _resourceDictionary;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public OpenFileDialog OpenFileDialog
        {
            get
            {
                if (_openFileDialog == null)
                {
                    _openFileDialog = new OpenFileDialog
                    {
                        Title = "Open File",
                        CheckFileExists = true,
                        CheckPathExists = true,
                        Multiselect = true,
                        RestoreDirectory = true,
                        ValidateNames = true
                    };
                }

                return _openFileDialog;
            }
        }
        private OpenFileDialog _openFileDialog;


        /// <inheritdoc/>
        public SaveFileDialog SaveFileDialog
        {
            get
            {
                if (_saveFileDialog == null)
                {
                    _saveFileDialog = new SaveFileDialog
                    {
                        Title = "Save File As",
                        CheckPathExists = true,
                        OverwritePrompt = true,
                        RestoreDirectory = true,
                        ValidateNames = true
                    };
                }

                return _saveFileDialog;
            }
        }
        private SaveFileDialog _saveFileDialog;


        /// <inheritdoc/>
        public Document ActiveDocument
        {
            get { return _activeDocument; }
            private set
            {
                if (_activeDocument == value)
                    return;

                if (_activeDocument != null)
                    _activeDocument.PropertyChanged -= OnActiveDocumentPropertyChanged;

                _activeDocument = value;

                if (_activeDocument != null)
                    _activeDocument.PropertyChanged += OnActiveDocumentPropertyChanged;

                OnActiveDocumentPropertyChanged(null, new PropertyChangedEventArgs(null));
                OnActiveDocumentChanged(EventArgs.Empty);
            }
        }
        private Document _activeDocument;


        /// <inheritdoc/>
        public IEnumerable<Document> Documents { get; }


        /// <inheritdoc/>
        public ICollection<DocumentFactory> Factories { get; }


        /// <inheritdoc/>
        public event EventHandler<EventArgs> ActiveDocumentChanged;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentExtension"/> class.
        /// </summary>
        public DocumentExtension()
        {
            _documents = new List<Document>();
            Documents = _documents.AsReadOnly();
            Factories = new List<DocumentFactory>();

            _menuManager = new MenuManager();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        protected override void OnInitialize()
        {
            AddCommandLineArguments();

            // Register services.
            Editor.Services.Register(typeof(IDocumentService), null, this);

            // Register views.
            // (The views are UserControls, so we could use data templates instead.)
            Editor.Services.RegisterView(typeof(ReloadFileViewModel), typeof(ReloadFileView));
            Editor.Services.RegisterView(typeof(SaveChangesViewModel), typeof(SaveChangesView));
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        protected override void OnStartup()
        {
            // Mandatory services.
            _windowService = Editor.Services.GetInstance<IWindowService>().ThrowIfMissing();
            _statusService = Editor.Services.GetInstance<IStatusService>().ThrowIfMissing();

            // Optional services.
            _searchService = Editor.Services.GetInstance<ISearchService>().WarnIfMissing();

            // Add the command items and register menus, toolbars, and context menus.
            AddDataTemplates();
            AddCommands();
            AddMenus();
            AddToolBars();
            AddContextMenu();
            AddSearchScopes();

            // Load the list of recently used files.
            LoadRecentFiles();

            // Monitor editor.
            Editor.Activated += OnEditorActivated;             // This calls AddCommandBindings.
            Editor.Deactivated += OnEditorDeactivated;
            Editor.UIInvalidated += OnEditorUIInvalidated;
            Editor.ActiveDockTabItemChanged += OnActiveDockTabItemChanged;
            Editor.WindowActivated += OnEditorWindowActivated;
        }


        /// <inheritdoc/>
        protected override void OnShutdown()
        {
            // Stop monitoring editor.
            Editor.Activated -= OnEditorActivated;
            Editor.Deactivated -= OnEditorDeactivated;
            Editor.UIInvalidated -= OnEditorUIInvalidated;
            Editor.ActiveDockTabItemChanged -= OnActiveDockTabItemChanged;
            Editor.WindowActivated -= OnEditorWindowActivated;

            RemoveSearchScopes();
            RemoveCommandBindings();
            RemoveContextMenu();
            RemoveToolBars();
            RemoveMenus();
            RemoveCommands();
            RemoveDataTemplates();

            // Store the list of recently used files.
            SaveRecentFiles();

            // Clear services.
            _windowService = null;
            _statusService = null;
            _searchService = null;
        }


        /// <inheritdoc/>
        protected override void OnUninitialize()
        {
            RemoveCommandLineArguments();

            Editor.Services.UnregisterView(typeof(ReloadFileViewModel));
            Editor.Services.UnregisterView(typeof(SaveChangesViewModel));

            Editor.Services.Unregister(typeof(IDocumentService));

            _openFileDialog = null;
            _saveFileDialog = null;
        }


        private void AddDataTemplates()
        {
            _resourceDictionary = new ResourceDictionary { Source = new Uri("pack://application:,,,/DigitalRune.Editor;component/Extensions/Documents/ResourceDictionary.xaml", UriKind.RelativeOrAbsolute) };
            EditorHelper.RegisterResources(_resourceDictionary);
        }


        private void RemoveDataTemplates()
        {
            EditorHelper.UnregisterResources(_resourceDictionary);
            _resourceDictionary = null;
        }


        /// <inheritdoc/>
        protected override IDockTabItem OnGetViewModel(string dockId)
        {
            // TODO: Add support for loading previously open files.
            return null;
        }


        private async void OnEditorActivated(object sender, ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                AddCommandBindings();

                // Enable drag-drop.
                Editor.Window.DragEnter += OnDragEnter;
                Editor.Window.Drop += OnDrop;
                Editor.Window.AllowDrop = true;

                // Load files specified at command-line.
                await OpenFromCommandLineAsync(Editor.CommandLineResult);
            }
        }


        private void OnEditorUIInvalidated(object sender, EventArgs eventArgs)
        {
            UpdateContextMenu();
        }


        private void OnEditorDeactivated(object sender, DeactivationEventArgs eventArgs)
        {
            if (eventArgs.Closed)
            {
                Editor.Window.DragEnter -= OnDragEnter;
                Editor.Window.Drop -= OnDrop;
            }
        }


        private void OnActiveDockTabItemChanged(object sender, EventArgs eventArgs)
        {
            var documentViewModel = Editor.ActiveDockTabItem as DocumentViewModel;
            if (documentViewModel != null)
                ActiveDocument = documentViewModel.Document;

            ApplyPendingFileChanges();
            UpdateCommands();
        }


        private void OnEditorWindowActivated(object sender, EventArgs eventArgs)
        {
            ApplyPendingFileChanges();
        }


        #region ----- IGuardClose -----

        /// <inheritdoc/>
        public async Task<bool> CanCloseAsync()
        {
            return await CloseAllAsync();
        }
        #endregion


        /// <summary>
        /// Raises the <see cref="ActiveDocumentChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        ///// <remarks>
        ///// <strong>Notes to Inheritors:</strong> When overriding
        ///// <see cref="OnActiveDocumentChanged"/> in a derived class, be sure to call the base
        ///// class's <see cref="OnActiveDocumentChanged"/> method so that registered delegates
        ///// receive the event.
        ///// </remarks>
        private void OnActiveDocumentChanged(EventArgs eventArgs)
        {
            ActiveDocumentChanged?.Invoke(this, eventArgs);
        }


        private void OnActiveDocumentPropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            Editor.Subtitle = ActiveDocument?.GetDisplayName();
        }


        internal void RegisterDocument(Document document)
        {
            Debug.Assert(document != null);
            Debug.Assert(!_documents.Contains(document), "Duplicate documents detected.");

            _documents.Add(document);

            Debug.Assert(ActiveDocument == null || _documents.Contains(ActiveDocument), "Active document is not registered in document service.");
        }


        internal void UnregisterDocument(Document document)
        {
            Debug.Assert(document != null);
            Debug.Assert(_documents.Contains(document), "Document not registered in document service.");

            if (ActiveDocument == document)
                ActiveDocument = null;

            _documents.Remove(document);

            Debug.Assert(ActiveDocument == null || _documents.Contains(ActiveDocument), "Active document is not registered in document service.");
        }


        /// <summary>
        /// Shows the document in the application. (Does not create a new view.)
        /// </summary>
        /// <param name="document">The document.</param>
        /// <remarks>
        /// If document has multiple views (dock windows), the first view is activated.
        /// </remarks>
        private static void ShowDocument(Document document)
        {
            foreach (var viewModel in document.ViewModels)
            {
                if (viewModel.Conductor != null)
                {
                    var task = viewModel.Conductor.ActivateItemAsync(viewModel);
                    Debug.Assert(task.IsCompleted, "ActivateItem expected to be synchronous operation.");
                    if (task.Result)
                        break;
                }
            }
        }
        #endregion
    }
}
