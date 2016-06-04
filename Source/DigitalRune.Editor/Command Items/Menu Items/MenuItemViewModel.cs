// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a command item in a menu.
    /// </summary>
    public class MenuItemViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the command item.
        /// </summary>
        /// <value>The command item.</value>
        public ICommandItem CommandItem { get; }


        /// <summary>
        /// Gets a value indicating whether this menu item is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this menu item is visible; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        public bool IsVisible
        {
            get { return _isVisible; }

            // The MenuManager hides empty sub-menus and unnecessary separators.
            internal set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible = true;


        /// <summary>
        /// Gets or sets the submenu.
        /// </summary>
        /// <value>The submenu. The default value is <see langword="null"/>.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public MenuItemViewModelCollection Submenu
        {
            get { return _submenu; }
            set { SetProperty(ref _submenu, value); }
        }
        private MenuItemViewModelCollection _submenu;


        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItemViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        public MenuItemViewModel(ICommandItem commandItem)
        {
            if (commandItem == null)
                throw new ArgumentNullException(nameof(commandItem));

            CommandItem = commandItem;
        }
    }
}
