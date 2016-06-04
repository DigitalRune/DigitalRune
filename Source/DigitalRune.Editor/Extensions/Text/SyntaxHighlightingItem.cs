// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Linq;
using System.Windows.Input;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using ICSharpCode.AvalonEdit.Highlighting;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Creates a menu that controls syntax highlighting of the currently active text editor control.
    /// </summary>
    internal sealed class SyntaxHighlightingItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly TextExtension _textExtension;
        private readonly IHighlightingService _highlightingService;
        private MenuItemViewModel _menuItemViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name
        {
            get { return "SyntaxHighlighting"; }
        }


        /// <inheritdoc/>
        public bool AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        public string Category
        {
            get { return CommandCategories.View; }
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
        public string Text { get { return "Syntax highlighting"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Change the syntax highlighting."; }
        }


        /// <summary>
        /// Gets or sets a value indicating whether this command item is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this command item is visible; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
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
        /// Initializes a new instance of the <see cref="SyntaxHighlightingItem"/> class.
        /// </summary>
        /// <param name="textExtension">The text editor extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="textExtension"/> is <see langword="null"/>.
        /// </exception>
        public SyntaxHighlightingItem(TextExtension textExtension)
        {
            if (textExtension == null)
                throw new ArgumentNullException(nameof(textExtension));

            _textExtension = textExtension;
            _highlightingService = textExtension.Editor.Services.GetInstance<IHighlightingService>().ThrowIfMissing();
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

                var changeHighlightingCommand = new DelegateCommand<IHighlightingDefinition>(ChangeHighlighting, CanChangeHighlighting);

                _menuItemViewModel.Submenu.Add(
                    new MenuItemViewModel(
                        new DelegateCommandItem("None", changeHighlightingCommand)
                        {
                            Category = Category,
                            CommandParameter = null,
                            IsCheckable = true,
                            IsChecked = false,
                            Text = "None"
                        }));

                var highlightingDefinitions = _highlightingService.HighlightingDefinitions.OrderBy(def => def.Name);
                foreach (var syntaxHighlighting in highlightingDefinitions)
                {
                    _menuItemViewModel.Submenu.Add(
                        new MenuItemViewModel(
                            new DelegateCommandItem(syntaxHighlighting.Name, changeHighlightingCommand)
                            {
                                Category = Category,
                                CommandParameter = syntaxHighlighting,
                                IsCheckable = true,
                                IsChecked = false,
                                Text = syntaxHighlighting.Name,
                            }));
                }

                Update();
            }

            return _menuItemViewModel;
        }


        private bool CanChangeHighlighting(IHighlightingDefinition highlighting)
        {
            return _textExtension.Editor?.ActiveDockTabItem is TextDocumentViewModel;
        }


        private void ChangeHighlighting(IHighlightingDefinition highlighting)
        {
            var textDocumentViewModel = _textExtension.Editor?.ActiveDockTabItem as TextDocumentViewModel;
            if (textDocumentViewModel != null)
                textDocumentViewModel.TextEditor.SyntaxHighlighting = highlighting;
        }


        public void Update()
        {
            if (_menuItemViewModel == null)
                return;

            var textDocumentViewModel = _textExtension.Editor?.ActiveDockTabItem as TextDocumentViewModel;
            var syntaxHighlighting = textDocumentViewModel?.SyntaxHighlighting;
            foreach (var menuItem in _menuItemViewModel.Submenu)
            {
                var commandItem = (DelegateCommandItem)menuItem.CommandItem;
                commandItem.IsChecked = (commandItem.CommandParameter == syntaxHighlighting);

                commandItem.Command.RaiseCanExecuteChanged();
            }
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            return null;
        }
        #endregion
    }
}
