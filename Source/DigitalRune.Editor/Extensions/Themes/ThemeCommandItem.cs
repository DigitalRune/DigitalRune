// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using static System.FormattableString;


namespace DigitalRune.Editor.Themes
{
    /// <summary>
    /// Creates a menu or toolbar item for switching the UI theme.
    /// </summary>
    internal class ThemeCommandItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly ThemeExtension _themeExtension;
        private MenuItemViewModel _menuItemViewModel;
        private ToolBarComboBoxViewModel _toolBarComboBoxViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name
        {
            get { return "Theme"; }
        }


        /// <inheritdoc/>
        public bool AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        public string Category
        {
            get { return CommandCategories.Tools; }
        }


        /// <inheritdoc/>
        public ICommand Command { get { return null; } }


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
        public string Text { get { return "Theme"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Change user interface theme."; }
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
        /// Initializes a new instance of the <see cref="ThemeCommandItem"/> class.
        /// </summary>
        /// <param name="themeExtension">The theme extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="themeExtension"/> is <see langword="null"/>.
        /// </exception>
        public ThemeCommandItem(ThemeExtension themeExtension)
        {
            if (themeExtension == null)
                throw new ArgumentNullException(nameof(themeExtension));

            _themeExtension = themeExtension;
            _themeExtension.ThemeChanged += OnThemeChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public MenuItemViewModel CreateMenuItem()
        {
            if (_menuItemViewModel == null)
            {
                _menuItemViewModel = new MenuItemViewModel(this) { Submenu = new MenuItemViewModelCollection() };

                var changeThemeCommand = new DelegateCommand<string>(ChangeTheme);

                foreach (var theme in _themeExtension.Themes)
                {
                    _menuItemViewModel.Submenu.Add(
                        new MenuItemViewModel(
                            new DelegateCommandItem("ChangeThemeTo" + theme, changeThemeCommand)
                            {
                                Category = Category,
                                CommandParameter = theme,
                                IsCheckable = true,
                                IsChecked = _themeExtension.Theme == theme,
                                Text = theme,
                                ToolTip = Invariant($"Change user interface theme to '{theme}'"),
                            }));
                }
            }

            return _menuItemViewModel;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            if (_toolBarComboBoxViewModel == null)
            {
                _toolBarComboBoxViewModel = new ToolBarComboBoxViewModel(this)
                {
                    Width = 75,
                    Items = _themeExtension.Themes,
                    SelectedItem = _themeExtension.Theme
                };

                _toolBarComboBoxViewModel.SelectedItemChanged += (s, e) => ChangeTheme((string)_toolBarComboBoxViewModel.SelectedItem);
            }

            return _toolBarComboBoxViewModel;
        }


        private void ChangeTheme(string theme)
        {
            _themeExtension.Theme = theme;
        }


        private void OnThemeChanged(object sender, EventArgs eventArgs)
        {
            UpdateMenuItem();
            UpdateToolBarItem();
        }


        private void UpdateMenuItem()
        {
            if (_menuItemViewModel != null)
            {
                foreach (var menuItem in _menuItemViewModel.Submenu)
                {
                    var commandItem = (DelegateCommandItem)menuItem.CommandItem;
                    commandItem.IsChecked = _themeExtension.Theme == (string)commandItem.CommandParameter;
                }
            }
        }


        private void UpdateToolBarItem()
        {
            if (_toolBarComboBoxViewModel != null)
                _toolBarComboBoxViewModel.SelectedItem = _themeExtension.Theme;
        }
        #endregion
    }
}
