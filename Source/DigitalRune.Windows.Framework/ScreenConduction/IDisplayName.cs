// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Represents an item which has a display name (title).
    /// </summary>
    /// <remarks>
    /// The display name is usually read by the object which creates the corresponding WPF elements,
    /// e.g. the <see cref="WindowManager"/> uses this info to set the window title.
    /// </remarks>
    public interface IDisplayName
    {
        /// <summary>
        /// Gets the display name (title).
        /// </summary>
        /// <value>The display name (title) of the item.</value>
        string DisplayName { get; }
    }
}
