// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using DigitalRune.Windows;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Creates a combo box for "Quick Find" in the toolbar.
    /// </summary>
    internal class QuickFindCommandItem : ObservableObject, ICommandItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ToolBarQuickFindViewModel _toolBarQuickFindViewModel;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name
        {
            get { return "QuickFind"; }
        }


        /// <inheritdoc/>
        public bool AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        public string Category
        {
            get { return CommandCategories.Search; }
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
        public string Text { get { return "Quick find"; } }


        /// <inheritdoc/>
        public string ToolTip
        {
            get { return "Enter search pattern and press RETURN to find next occurrence."; }
        }


        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible = true;


        /// <summary>
        /// Gets the <see cref="SearchExtension"/>.
        /// </summary>
        /// <value>The <see cref="SearchExtension"/>.</value>
        public SearchExtension SearchExtension { get; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="QuickFindCommandItem"/> class.
        /// </summary>
        /// <param name="searchExtension">The search extension.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="searchExtension"/> is <see langword="null"/>.
        /// </exception>
        public QuickFindCommandItem(SearchExtension searchExtension)
        {
            if (searchExtension == null)
                throw new ArgumentNullException(nameof(searchExtension));

            SearchExtension = searchExtension;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public MenuItemViewModel CreateMenuItem()
        {
            return null;
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            if (_toolBarQuickFindViewModel == null)
            {
                _toolBarQuickFindViewModel = new ToolBarQuickFindViewModel(this)
                {
                    Items = SearchExtension.RecentFindPatterns,

                    // Text is used in DataTemplate instead of SelectedItem
                    //SelectedItem = SearchService.Query.FindPattern,
                };
            }

            return _toolBarQuickFindViewModel;
        }
        #endregion
    }
}
