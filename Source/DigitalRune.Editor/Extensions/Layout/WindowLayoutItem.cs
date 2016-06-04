// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Layout
{
    /// <summary>
    /// Represents the menu and toolbar item for managing the window layouts.
    /// </summary>
    internal class WindowLayoutItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly LayoutExtension _layoutExtensions;
        private MenuItemViewModel _menuItem;
        private ToolBarDropDownButtonViewModel _toolBarDropDownButton;
        private WindowLayoutCaptionBarViewModel _captionBarViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name { get { return "WindowLayout"; } }


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
        public string Text { get { return "Layout"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Change the window layout."; }
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
        /// Initializes a new instance of the <see cref="WindowLayoutItem"/> class.
        /// </summary>
        /// <param name="layoutExtensions">The layout extensions.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="layoutExtensions"/> is <see langword="null"/>.
        /// </exception>
        public WindowLayoutItem(LayoutExtension layoutExtensions)
        {
            if (layoutExtensions == null)
                throw new ArgumentNullException(nameof(layoutExtensions));

            _layoutExtensions = layoutExtensions;
            Command = new DelegateCommand<WindowLayout>(_layoutExtensions.SwitchLayout);
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
                UpdateMenu();
            }

            return _menuItem;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            if (_toolBarDropDownButton == null)
            {
                _toolBarDropDownButton = new ToolBarDropDownButtonViewModel(this)
                {
                    Width = double.NaN,
                    // Reuse sub-menu from menu time.
                    Items = CreateMenuItem().Submenu
                };
                UpdateToolBar();
            }

            return _toolBarDropDownButton;
        }


        public WindowLayoutCaptionBarViewModel CreateCaptionBarItem()
        {
            if (_captionBarViewModel == null)
            {
                _captionBarViewModel = new WindowLayoutCaptionBarViewModel
                {
                    // Reuse sub-menu from menu time.
                    Items = CreateMenuItem().Submenu
                };
                UpdateCaptionBar();
            }

            return _captionBarViewModel;
        }


        public void Update()
        {
            UpdateMenu();
            UpdateToolBar();
            UpdateCaptionBar();
        }


        private void UpdateMenu()
        {
            if (_menuItem == null)
                return;

            _menuItem.Submenu.Clear();
            var activeLayout = _layoutExtensions.ActiveLayout;
            foreach (var layout in _layoutExtensions.Layouts)
            {
                _menuItem.Submenu.Add(
                    new MenuItemViewModel(
                        new DelegateCommandItem($"Load {layout.Name}", (IDelegateCommand)Command)
                        {
                            Category = Category,
                            CommandParameter = layout,
                            IsCheckable = true,
                            IsChecked = activeLayout == layout,
                            Icon = null,
                            Text = layout.Name,
                            ToolTip = ToolTip
                        }));
            }

            _menuItem.Submenu.Add(new MenuSeparatorViewModel(new CommandSeparator("LayoutSeparator")));
            _menuItem.Submenu.Add(new MenuItemViewModel(_layoutExtensions.CommandItems["SaveWindowLayout"]));
            _menuItem.Submenu.Add(new MenuItemViewModel(_layoutExtensions.CommandItems["SaveWindowLayoutAs"]));
            _menuItem.Submenu.Add(new MenuItemViewModel(_layoutExtensions.CommandItems["ManageWindowLayouts"]));

            // The "Reset window layout" always affects the active window layout.
            // --> Update the UI text.
            var commandItem = (CommandItem)_layoutExtensions.CommandItems["ResetWindowLayout"];
            commandItem.Text = activeLayout != null ? $"_Reset \"{activeLayout.Name}\""  : "_Reset layout";

            _menuItem.Submenu.Add(new MenuItemViewModel(commandItem));
        }


        private void UpdateToolBar()
        {
            if (_menuItem == null || _toolBarDropDownButton == null)
                return;

            _toolBarDropDownButton.SelectedItem = _menuItem.Submenu
                                                           .Select(m => m.CommandItem)
                                                           .FirstOrDefault(commandItem => commandItem.IsChecked);

            if (_toolBarDropDownButton.SelectedItem == null)
                _toolBarDropDownButton.SelectedItem = this;
        }


        private void UpdateCaptionBar()
        {
            if (_menuItem == null || _captionBarViewModel == null)
                return;

            _captionBarViewModel.SelectedItem = _menuItem.Submenu
                                                         .Select(m => m.CommandItem)
                                                         .FirstOrDefault(commandItem => commandItem.IsChecked);

            if (_captionBarViewModel.SelectedItem == null)
                _captionBarViewModel.SelectedItem = this;
        }
        #endregion
    }
}
