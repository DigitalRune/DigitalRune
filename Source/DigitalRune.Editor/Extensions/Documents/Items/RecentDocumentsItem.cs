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
using static System.FormattableString;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Creates the "Recent Files" menu.
    /// </summary>
    internal class RecentDocumentsItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly DocumentExtension _documentExtension;
        private MenuItemViewModel _menuItem;
        private ToolBarDropDownButtonViewModel _toolBarDropDownButton;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name { get { return "RecentFiles"; } }


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
        public string Text { get { return "Recent _files"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Open a recently used file"; }
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
        /// Initializes a new instance of the <see cref="RecentDocumentsItem"/> class.
        /// </summary>
        /// <param name="documentExtension">The <see cref="DocumentExtension"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="documentExtension"/> is <see langword="null"/>.
        /// </exception>
        public RecentDocumentsItem(DocumentExtension documentExtension)
        {
            if (documentExtension == null)
                throw new ArgumentNullException(nameof(documentExtension));

            _documentExtension = documentExtension;
            Command = new DelegateCommand<string>(OnOpenRecentDocument);
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
                Update();
            }

            return _menuItem;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            if (_toolBarDropDownButton == null)
            {
                // Reuse sub-menu from menu item.
                var menuItem = CreateMenuItem();

                _toolBarDropDownButton = new ToolBarDropDownButtonViewModel(this)
                {
                    Width = 150,
                    Items = menuItem.Submenu,
                    SelectedItem = this,
                };
            }

            return _toolBarDropDownButton;
        }


        private void OnOpenRecentDocument(string recentFile)
        {
            Logger.Info(CultureInfo.InvariantCulture, "Opening recently used file \"{0}\".", recentFile);

            _documentExtension.Open(new Uri(recentFile));
        }


        public void Update()
        {
            if (_menuItem == null)
                return;

            _menuItem.Submenu.Clear();

            // Add a sub-menu item for each recently used file.
            int i = 0;
            int maxNumberOfRecentFiles = _documentExtension.NumberOfRecentFiles;
            foreach (string recentFile in _documentExtension.RecentFiles.Take(maxNumberOfRecentFiles))
            {
                _menuItem.Submenu.Add(
                    new FilePathMenuItemViewModel(
                        new DelegateCommandItem($"Open {recentFile}", (IDelegateCommand)Command)
                        {
                            Category = Category,
                            CommandParameter = recentFile,
                            Text = recentFile,
                            ToolTip = ToolTip
                        })
                    {
                        Prefix = Invariant($"{i + 1} "),
                    });
                i++;
            }

            // Add dummy sub-menu item for unused slots.
            var disabledCommand = new DelegateCommand(() => { }, () => false);
            for (; i < maxNumberOfRecentFiles; i++)
            {
                _menuItem.Submenu.Add(
                    new FilePathMenuItemViewModel(
                        new DelegateCommandItem(Invariant($"OpenUnused{i}"), disabledCommand)
                        {
                            Text = null,
                        })
                    {
                        Prefix = Invariant($"{i + 1} "),
                    });
            }
        }
        #endregion
    }
}
