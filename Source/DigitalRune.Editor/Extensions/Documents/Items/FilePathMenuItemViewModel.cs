// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents a command item in a menu that represents a file path, e.g. "1 X:\...\foo.txt".
    /// </summary>
    /// <remarks>
    /// In the above example, the <see cref="Prefix"/> is "1 " (including the space character).
    /// The file path is the text of the command item. The file path is shortened intelligently to
    /// show as much useful information as possible in the available space.
    /// </remarks>
    public class FilePathMenuItemViewModel : MenuItemViewModel
    {
        /// <summary>
        /// Gets or sets the prefix text.
        /// </summary>
        /// <value>The prefix text.</value>
        public string Prefix
        {
            get { return _prefix; }
            set { SetProperty(ref _prefix, value); }
        }
        private string _prefix;


        /// <summary>
        /// Initializes a new instance of the <see cref="FilePathMenuItemViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        public FilePathMenuItemViewModel(ICommandItem commandItem)
            : base(commandItem)
        {
        }
    }
}
