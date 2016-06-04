// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.Generic;


namespace DigitalRune.Editor.QuickLaunch
{
    /// <summary>
    /// Manages the items available in the Quick Launch box.
    /// </summary>
    public interface IQuickLaunchService
    {
        /// <summary>
        /// Gets the items available in the Quick Launch box.
        /// </summary>
        /// <value>The items available in the Quick Launch box.</value>
        IList<QuickLaunchItem> Items { get; }
    }
}
