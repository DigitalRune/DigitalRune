// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows.Input;
using System.Windows.Media;
using DigitalRune.Windows;
using DigitalRune.Windows.Controls;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Defines an item that invokes an <see cref="ICommand"/>.
    /// </summary>
    /// <inheritdoc cref="ICommandItem"/>
    public abstract class CommandItem : ObservableObject, ICommandItem
    {
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
        /// <inheritdoc/>
        public bool AlwaysShowText
        {
            get { return _alwaysShowText; }
            set { SetProperty(ref _alwaysShowText, value); }
        }
        private bool _alwaysShowText;


        /// <summary>
        /// Gets or sets the command category.
        /// </summary>
        /// <value>
        /// The command category. The default value is <see cref="CommandCategories.Default"/>.
        /// </value>
        /// <inheritdoc/>
        public string Category
        {
            get { return _category; }
            set { SetProperty(ref _category, value); }
        }
        private string _category = CommandCategories.Default;


        /// <inheritdoc/>
        public ICommand Command { get { return CommandAsICommand; } }


        /// <summary>
        /// Gets the command as <see cref="ICommand"/>.
        /// </summary>
        /// <value>The command as <see cref="ICommand"/>.</value>
        internal abstract ICommand CommandAsICommand { get; }


        /// <summary>
        /// Gets or sets the parameter passed to the command when it is executed.
        /// </summary>
        /// <value>
        /// The parameter passed to the command when it is executed. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <inheritdoc/>
        public object CommandParameter
        {
            get { return _commandParameter; }
            set { SetProperty(ref _commandParameter, value); }
        }
        private object _commandParameter;


        /// <summary>
        /// Gets or sets the icon (<see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) that
        /// represents this item.
        /// </summary>
        /// <value>The icon. The default value is <see langword="null"/>.</value>
        /// <inheritdoc/>
        public object Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
        private object _icon;


        /// <summary>
        /// Gets or sets the input gestures that trigger the <see cref="Command"/> of this item.
        /// </summary>
        /// <value>The input gestures. The default value is <see langword="null"/>.</value>
        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public InputGestureCollection InputGestures
        {
            get { return _inputGestures; }
            set { SetProperty(ref _inputGestures, value); }
        }
        private InputGestureCollection _inputGestures;


        /// <summary>
        /// Gets or sets a value indicating whether this item is checkable.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is checkable; otherwise, <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsCheckable
        {
            get { return _isCheckable; }
            set { SetProperty(ref _isCheckable, value); }
        }
        private bool _isCheckable;


        /// <summary>
        /// Gets or sets a value indicating whether this item is checked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is checked; otherwise, <see langword="false"/>. The
        /// default value is <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }
        private bool _isChecked;


        /// <summary>
        /// Gets or sets the UI text.
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
        /// Initializes a new instance of the <see cref="CommandItem"/> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> is <see langword="null"/>.
        /// </exception>
        protected CommandItem(string name)
            : this(name, name)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CommandItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="text">The UI text.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="name"/> or <paramref name="text"/> is <see langword="null"/>.
        /// </exception>
        protected CommandItem(string name, string text)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            Name = name;
            _text = name;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        public virtual MenuItemViewModel CreateMenuItem()
        {
            return new MenuItemViewModel(this);
        }


        /// <inheritdoc/>
        public virtual ToolBarItemViewModel CreateToolBarItem()
        {
            if (IsCheckable)
                return new ToolBarCheckBoxViewModel(this);

            return new ToolBarButtonViewModel(this);
        }
        #endregion
    }
}
