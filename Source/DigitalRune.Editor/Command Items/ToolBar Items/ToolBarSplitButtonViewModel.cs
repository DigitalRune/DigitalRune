// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents split button in a toolbar.
    /// </summary>
    public class ToolBarSplitButtonViewModel : ToolBarDropDownButtonViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolBarSplitButtonViewModel"/> class.
        /// </summary>
        /// <param name="commandItem">The command item. Must not be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="commandItem"/> is <see langword="null"/>.
        /// </exception>
        public ToolBarSplitButtonViewModel(ICommandItem commandItem) 
            : base(commandItem)
        {
        }
    }
}
