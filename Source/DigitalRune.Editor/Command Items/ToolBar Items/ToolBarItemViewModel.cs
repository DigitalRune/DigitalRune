// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using DigitalRune.Windows;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a command item in a toolbar.
    /// </summary>
    public abstract class ToolBarItemViewModel : ObservableObject
    {
        /// <summary>
        /// Gets or sets the command item.
        /// </summary>
        /// <value>The command item.</value>
        public ICommandItem CommandItem { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarItemViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item.  Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        protected ToolBarItemViewModel(ICommandItem commandItem)
        {
            if (commandItem == null)
                throw new ArgumentNullException(nameof(commandItem));

            CommandItem = commandItem;
        }
    }
}
