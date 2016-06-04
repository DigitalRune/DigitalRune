// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor.Search
{
    /// <summary>
    /// Represents a Quick Find combo box in a toolbar.
    /// </summary>
    public class ToolBarQuickFindViewModel : ToolBarComboBoxViewModel
    {
        /// <summary>
        /// Gets the search query.
        /// </summary>
        /// <value>The search query.</value>
        public SearchQuery Query { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarQuickFindViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarQuickFindViewModel(ICommandItem commandItem)
            : base(commandItem)
        {
            Width = 150;
            Query = (commandItem as QuickFindCommandItem)?.SearchExtension?.Query;
        }
    }
}
