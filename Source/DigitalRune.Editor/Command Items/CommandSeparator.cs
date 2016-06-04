// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a separator between command items.
    /// </summary>
    public sealed class CommandSeparator : ObservableObject, ICommandItem
    {
        // Note: Properties are public to enable data binding. Properties that are NEVER used in
        // data bindings are implicit interface implementations.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public string Name { get; }


        /// <inheritdoc/>
        bool ICommandItem.AlwaysShowText { get { return false; } }


        /// <inheritdoc/>
        string ICommandItem.Category { get { return null; } }


        /// <inheritdoc/>
        ICommand ICommandItem.Command { get { return null; } }


        /// <inheritdoc/>
        object ICommandItem.CommandParameter { get { return null; } }


        /// <inheritdoc/>
        object ICommandItem.Icon { get { return null; } }


        /// <inheritdoc/>
        InputGestureCollection ICommandItem.InputGestures { get { return null; } }


        /// <inheritdoc/>
        bool ICommandItem.IsCheckable { get { return false; } }


        /// <inheritdoc/>
        bool ICommandItem.IsChecked { get { return false; } }


        /// <inheritdoc/>
        string ICommandItem.Text { get { return null; } }


        /// <inheritdoc/>
        string ICommandItem.ToolTip { get { return null; } }


        /// <inheritdoc/>
        bool ICommandItem.IsVisible
        {
            // Separators are always visible. However, the visibility of the linked menu item or
            // toolbar item is set automatically depending on the neighbor items.
            get { return true; }
            set { throw new NotSupportedException(); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandSeparator"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public CommandSeparator(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            Name = name;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public MenuItemViewModel CreateMenuItem()
        {
            return new MenuSeparatorViewModel(this);
        }


        /// <inheritdoc/>
        public ToolBarItemViewModel CreateToolBarItem()
        {
            return new ToolBarSeparatorViewModel(this);
        }
        #endregion
    }
}
