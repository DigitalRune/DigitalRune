// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Groups other command items into a submenu or a toolbar.
    /// </summary>
    public sealed class CommandGroup : ObservableObject, ICommandItem
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


        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Text"/> should always be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Text"/> should always be shown; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <inheritdoc cref="CommandItem.AlwaysShowText"/>
        public bool AlwaysShowText
        {
            get { return _alwaysShowText; }
            set { SetProperty(ref _alwaysShowText, value); }
        }
        private bool _alwaysShowText;


        /// <inheritdoc/>
        public string Category { get { return null; } }


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


        /// <summary>
        /// Gets or sets the UI text that represents this item.
        /// </summary>
        /// <value>The UI text. By default the text is the same as <see cref="Name"/>.</value>
        /// <inheritdoc/>
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }
        private string _text;


        /// <summary>
        /// Gets or sets the tool tip text that explains the purpose of this item.
        /// </summary>
        /// <value>The tool tip text. The default value is <see langword="null"/>.</value>
        /// <inheritdoc/>
        public string ToolTip
        {
            get { return _toolTip; }
            set { SetProperty(ref _toolTip, value); }
        }
        private string _toolTip;


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

        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class with the given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        public CommandGroup(string name)
          : this(name, name)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandGroup"/> class with the given name
        /// and UI text.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="text">The UI text.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="text"/> is <see langword="null"/>.
        /// </exception>
        public CommandGroup(string name, string text)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            Name = name;
            _text = text;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public MenuItemViewModel CreateMenuItem()
        {
            return new MenuItemViewModel(this);
        }


        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public ToolBarItemViewModel CreateToolBarItem()
        {
            // Special: ToolBarViewModels do not derive from ToolBarItemViewModel. --> ToolBarVMs
            // must be created by the caller.
            return null;
        }
        #endregion
    }
}
