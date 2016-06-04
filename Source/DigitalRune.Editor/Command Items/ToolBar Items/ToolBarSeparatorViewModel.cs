// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a separator in a toolbar.
    /// </summary>
    public sealed class ToolBarSeparatorViewModel : ToolBarItemViewModel
    {
        /// <summary>
        /// Gets a value indicating whether this separator is visible.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this separator is visible; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsVisible
        {
            get { return _isVisible; }

            // Visibility is set automatically to hide unnecessary separators.
            internal set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible = true;


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarSeparatorViewModel"/> class.
        /// </summary>
        /// <param name="commandSeparator">
        /// The command separator. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandSeparator"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarSeparatorViewModel(CommandSeparator commandSeparator)
            : base(commandSeparator)
        {
        }
    }
}
