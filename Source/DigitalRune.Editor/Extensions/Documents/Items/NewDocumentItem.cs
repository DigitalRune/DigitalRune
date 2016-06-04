// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Creates the "New" menu and toolbar item that contains a list of all document types that can
    /// be created.
    /// </summary>
    internal class NewDocumentItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DocumentExtension _documentExtension;
        private MenuItemViewModel _menuItem;
        private ToolBarSplitButtonViewModel _toolBarSplitButton;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name { get { return "New"; } }


        /// <inheritdoc/>
        public bool AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        public string Category { get { return CommandCategories.File; } }


        /// <inheritdoc/>
        public ICommand Command { get; }


        /// <inheritdoc/>
        public object CommandParameter { get { return null; } }


        /// <inheritdoc/>
        public object Icon { get { return null; } }


        /// <inheritdoc/>
        public InputGestureCollection InputGestures { get { return null; } }


        /// <inheritdoc/>
        public bool IsCheckable { get { return false; } }


        /// <inheritdoc/>
        public bool IsChecked { get { return false; } }


        /// <inheritdoc/>
        public string Text { get { return "New"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Create new document."; }
        }


        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible = true;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="NewDocumentItem"/> class.
        /// </summary>
        /// <param name="documentExtension">The <see cref="DocumentExtension"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentExtension"/> is <see langword="null"/>.
        /// </exception>
        public NewDocumentItem(DocumentExtension documentExtension)
        {
            if (documentExtension == null)
                throw new ArgumentNullException(nameof(documentExtension));

            _documentExtension = documentExtension;
            Command = new DelegateCommand<DocumentType>(OnNewDocument);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public MenuItemViewModel CreateMenuItem()
        {
            if (_menuItem == null)
            {
                _menuItem = new MenuItemViewModel(this) { Submenu = new MenuItemViewModelCollection() };

                // Add a sub-menu item for each document type.
                var documentTypes = _documentExtension.GetCreatableDocumentTypes()
                                                      .OrderBy(documentType => documentType.Name);

                foreach (var documentType in documentTypes)
                {
                    _menuItem.Submenu.Add(
                        new MenuItemViewModel(
                            new DelegateCommandItem($"New {documentType.Name}", (IDelegateCommand)Command)
                            {
                                Category = Category,
                                CommandParameter = documentType,
                                Icon = documentType.Icon,
                                Text = documentType.Name,
                                ToolTip = ToolTip
                            }));
                }
            }

            return _menuItem;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            if (_toolBarSplitButton == null)
            {
                // Reuse sub-menu from menu time.
                var menuItemViewModel = CreateMenuItem();

                _toolBarSplitButton = new ToolBarSplitButtonViewModel(this)
                {
                    Items = menuItemViewModel.Submenu,
                    SelectedItem = menuItemViewModel.Submenu.FirstOrDefault()?.CommandItem
                };
            }

            return _toolBarSplitButton;
        }


        private void OnNewDocument(DocumentType documentType)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Creating new document of type \"{0}\".", documentType.Name);

            var document = _documentExtension.New(documentType);

            if (_toolBarSplitButton != null && document != null)
            {
                // Select last recently used document type in toolbar.
                _toolBarSplitButton.SelectedItem = _toolBarSplitButton.Items
                                                                      .Select(menuItem => menuItem.CommandItem)
                                                                      .FirstOrDefault(commandItem => commandItem.CommandParameter == documentType);
            }
        }
        #endregion
    }
}
