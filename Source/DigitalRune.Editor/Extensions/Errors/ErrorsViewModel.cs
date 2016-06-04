// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using DigitalRune.Editor.Documents;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Errors
{
    /// <summary>
    /// Represents the Errors window.
    /// </summary>
    internal class ErrorsViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "Errors";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IEditorService _editor;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets an instance of the <see cref="ErrorsViewModel"/> that can be used at
        /// design-time.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="ErrorsViewModel"/> that can be used at design-time.
        /// </value>
        internal static ErrorsViewModel DesignInstance
        {
            get
            {
                var vm = new ErrorsViewModel(null)
                {
                    Items =
                    {
                        new Error(ErrorType.Error, "Error description 01", "foo.txt", 100, 10),
                        new Error(ErrorType.Error, "Error description 02", "foo.txt", 200, 200),
                        new Error(ErrorType.Warning, "Warning description", "mymodel.fbx", 99),
                        new Error(ErrorType.Message, "Message Description")
                    }
                };
                return vm;
            }
        }


        /// <summary>
        /// Gets the errors.
        /// </summary>
        /// <value>The errors.</value>
        public ObservableCollection<Error> Items { get; } = new ObservableCollection<Error>();


        /// <summary>
        /// Gets or sets the a value indicating whether errors should be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show errors; otherwise, <see langword="false"/>.
        /// </value>
        public bool ShowErrors
        {
            get { return _showErrors; }
            set
            {
                if (SetProperty(ref _showErrors, value))
                    UpdateFilter();
            }
        }
        private bool _showErrors = true;


        /// <summary>
        /// Gets or sets the a value indicating whether warnings should be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show warnings; otherwise, <see langword="false"/>.
        /// </value>
        public bool ShowWarnings
        {
            get { return _showWarnings; }
            set
            {
                if (SetProperty(ref _showWarnings, value))
                    UpdateFilter();
            }
        }
        private bool _showWarnings = true;


        /// <summary>
        /// Gets or sets the a value indicating whether messages should be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show messages; otherwise, <see langword="false"/>.
        /// </value>
        public bool ShowMessages
        {
            get { return _showMessages; }
            set
            {
                if (SetProperty(ref _showMessages, value))
                    UpdateFilter();
            }
        }
        private bool _showMessages = true;


        /// <summary>
        /// Gets or sets the number of errors.
        /// </summary>
        /// <value>The number of errors.</value>
        public int NumberOfErrors
        {
            get { return _numberOfErrors; }
            set { SetProperty(ref _numberOfErrors, value); }
        }
        private int _numberOfErrors;


        /// <summary>
        /// Gets or sets the number of warnings.
        /// </summary>
        /// <value>The number of warnings.</value>
        public int NumberOfWarnings
        {
            get { return _numberOfWarnings; }
            set { SetProperty(ref _numberOfWarnings, value); }
        }
        private int _numberOfWarnings;


        /// <summary>
        /// Gets or sets the number of info messages.
        /// </summary>
        /// <value>The number of info messages.</value>
        public int NumberOfMessages
        {
            get { return _numberOfMessages; }
            set { SetProperty(ref _numberOfMessages, value); }
        }
        private int _numberOfMessages;


        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public Error SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        private Error _selectedItem;


        /// <summary>
        /// Gets the command that is invoked when the selected is double-clicked with mouse.
        /// </summary>
        /// <value>
        /// The command that is invoked when the selected is double-clicked with mouse.
        /// </value>
        public DelegateCommand<MouseButtonEventArgs> PreviewMouseDoubleClickCommand { get; }


        /// <summary>
        /// Gets the command that is invoked when a key is pressed.
        /// </summary>
        /// <value>
        /// The command that is invoked when a key is pressed.
        /// </value>
        public DelegateCommand<KeyEventArgs> PreviewKeyDownCommand { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorsViewModel" /> class.
        /// </summary>
        /// <param name="editor">The editor. Can be <see langword="null"/> at design-time.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public ErrorsViewModel(IEditorService editor)
        {
            DisplayName = "Errors";
            DockId = DockIdString;
            //Icon = MultiColorGlyphs.ErrorList;
            IsPersistent = true;

            if (!WindowsHelper.IsInDesignMode)
            {
                if (editor == null)
                    throw new ArgumentNullException(nameof(editor));

                _editor = editor;
            }

            Items.CollectionChanged += OnItemsChanged;

            var cvs = CollectionViewSource.GetDefaultView(Items);
            cvs.Filter = item =>
            {
                switch (((Error)item).ErrorType)
                {
                    case ErrorType.Error:
                        return ShowErrors;
                    case ErrorType.Warning:
                        return ShowWarnings;
                    case ErrorType.Message:
                        return ShowMessages;
                    default:
                        return true;
                }
            };

            PreviewMouseDoubleClickCommand = new DelegateCommand<MouseButtonEventArgs>(OnPreviewMouseDoubleClick);
            PreviewKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnPreviewKeyDown);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            int numberOfErrors = 0;
            int numberOfWarnings = 0;
            int numberOfMessages = 0;
            foreach (var item in Items)
            {
                switch (item.ErrorType)
                {
                    case ErrorType.Error:
                        numberOfErrors++;
                        break;
                    case ErrorType.Warning:
                        numberOfWarnings++;
                        break;
                    case ErrorType.Message:
                        numberOfMessages++;
                        break;
                }
            }

            NumberOfErrors = numberOfErrors;
            NumberOfWarnings = numberOfWarnings;
            NumberOfMessages = numberOfMessages;
        }


        private void UpdateFilter()
        {
            var cvs = CollectionViewSource.GetDefaultView(Items);
            cvs.Refresh();
        }


        private void OnPreviewMouseDoubleClick(MouseButtonEventArgs eventArgs)
        {
            if (SelectedItem != null)
            {
                GoToLocation();
                eventArgs.Handled = true;
            }
        }


        private void OnPreviewKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.Key == Key.Enter && SelectedItem != null)
            {
                GoToLocation();
                eventArgs.Handled = true;
            }
        }


        private void GoToLocation()
        {
            var selectedItem = SelectedItem;
            if (selectedItem == null)
                return;

            var command = selectedItem.GoToLocationCommand;
            if (command != null && command.CanExecute(SelectedItem))
            {
                command.Execute(SelectedItem);
                return;
            }

            // Find the document that contains the location string.
            var documentService = _editor.Services.GetInstance<IDocumentService>();
            if (documentService == null)
                return;

            var location = selectedItem.Location;
            foreach (var document in documentService.Documents)
            {
                if (document.IsUntitled && document.UntitledName.IndexOf(location, StringComparison.OrdinalIgnoreCase) >= 0
                    || !document.IsUntitled && document.Uri.LocalPath.IndexOf(location, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var vm = document.ViewModels.FirstOrDefault();
                    if (vm != null)
                    {
                        _editor.ActivateItem(vm);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
