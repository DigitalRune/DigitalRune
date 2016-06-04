// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Editor.Options;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Commands
{
    /// <summary>
    /// Shows the available command items and shortcuts in the Options dialog.
    /// </summary>
    internal class ShortcutsOptionsPageViewModel : OptionsPageViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly IEditorService _editor;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        internal static ShortcutsOptionsPageViewModel DesignInstance
        {
            get { return new ShortcutsOptionsPageViewModel(null); }
        }


        /// <summary>
        /// Gets the command items grouped by category.
        /// </summary>
        /// <value>The command items grouped by category.</value>
        public IEnumerable<IGrouping<string, CommandItem>> Categories
        {
            get { return _categories; }
            private set { SetProperty(ref _categories, value); }
        }
        private IEnumerable<IGrouping<string, CommandItem>> _categories;


        /// <summary>
        /// Gets or sets the selected category.
        /// </summary>
        /// <value>The selected category.</value>
        public IGrouping<string, CommandItem> SelectedCategory
        {
            get { return _selectedCategory; }
            set { SetProperty(ref _selectedCategory, value); }
        }
        private IGrouping<string, CommandItem> _selectedCategory;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ShortcutsOptionsPageViewModel"/> class.
        /// </summary>
        /// <param name="editor">The editor.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="editor"/> is <see langword="null"/>.
        /// </exception>
        public ShortcutsOptionsPageViewModel(IEditorService editor)
            : base("Shortcuts")
        {
            if (editor == null && !WindowsHelper.IsInDesignMode)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
            {
                if (Categories == null)
                {
                    // Get all command items.
                    var items = _editor.Extensions
                                       .SelectMany(extension => extension.CommandItems)
                                       .OfType<CommandItem>()
                                       .ToList();

                    // Sort and group into "All" category.
                    var all = items.OrderBy(item => EditorHelper.FilterAccessKeys(item.Text))
                                   .GroupBy(item => "All");

                    // Sort and group by category.
                    var categories = items.OrderBy(item => item.Category)
                                          .ThenBy(item => EditorHelper.FilterAccessKeys(item.Text))
                                          .GroupBy(item => item.Category);

                    Categories = all.Concat(categories)
                                    .ToArray();

                    SelectedCategory = Categories.FirstOrDefault(group => group.Key == "File") 
                                       ?? Categories.FirstOrDefault();
                }
            }

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        protected override void OnApply()
        {
        }
        #endregion
    }
}
