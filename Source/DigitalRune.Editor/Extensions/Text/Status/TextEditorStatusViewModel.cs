// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using DigitalRune.Windows;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Shows status information for the text editor (line number, column number) in the status bar.
    /// </summary>
    internal class TextEditorStatusViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets a value indicating whether insert is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if insert is active; otherwise <see langword="false"/> if
        /// overstrike is active.
        /// </value>
        public bool Insert
        {
            get { return _insert; }
            set { SetProperty(ref _insert, value); }
        }
        private bool _insert;



        /// <summary>
        /// Gets or sets a value indicating whether overstrike is active.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if overstrike is active; otherwise <see langword="false"/> if
        /// insert is active.
        /// </value>
        public bool Overstrike
        {
            get { return _overstrike; }
            set { SetProperty(ref _overstrike, value); }
        }
        private bool _overstrike;


        /// <summary>
        /// Gets or sets the character number.
        /// </summary>
        /// <value>The character number.</value>
        public int Character
        {
            get { return _character; }
            set { SetProperty(ref _character, value); }
        }
        private int _character;


        /// <summary>
        /// Gets or sets the column number.
        /// </summary>
        /// <value>The column number.</value>
        public int Column
        {
            get { return _column; }
            set { SetProperty(ref _column, value); }
        }
        private int _column;


        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>The line number.</value>
        public int Line
        {
            get { return _line; }
            set { SetProperty(ref _line, value); }
        }
        private int _line;


        /// <summary>
        /// Gets or sets a value indicating whether this item is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item is visible; otherwise,
        /// <see langword="false"/>. The default value is <see langword="true"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditorStatusViewModel"/> class.
        /// </summary>
        public TextEditorStatusViewModel()
        {
            if (WindowsHelper.IsInDesignMode)
            {
                _character = 9999;
                _column = 9999;
                _line = 9999;
                _insert = true;
                _overstrike = false;
                _isVisible = true;
            }
        }
    }
}
