// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a separator in a menu.
    /// </summary>
    internal sealed class MenuSeparatorViewModel : MenuItemViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuSeparatorViewModel"/> class.
        /// </summary>
        /// <param name="commandSeparator">
        /// The command separator. Must not be <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandSeparator"/> is <see langword="null"/>.
        /// </exception>
        public MenuSeparatorViewModel(CommandSeparator commandSeparator)
            : base(commandSeparator)
        {
        }
    }
}
