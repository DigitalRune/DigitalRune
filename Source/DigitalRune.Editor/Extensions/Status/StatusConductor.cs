// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Linq;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Status
{
    /// <summary>
    /// Manages the messages shown in the status bar.
    /// </summary>
    internal class StatusConductor : OneActiveItemsConductor
    {
        /// <inheritdoc/>
        protected override object GetNextItemToActivate(IList items, object activeItem)
        {
            // Pick previous status message.
            return Items.Reverse().FirstOrDefault(item => !item.Equals(activeItem));
        }
    }
}
