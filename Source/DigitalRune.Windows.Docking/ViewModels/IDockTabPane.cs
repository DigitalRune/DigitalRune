// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane that contains multiple items that share the same space on the screen.
    /// </summary>
    /// <remarks>
    /// A <see cref="IDockTabPane"/> may contain one or more <see cref="IDockTabItem"/>s. The 
    /// <see cref="IDockTabItem"/> share the same space - similar to 
    /// <see cref="System.Windows.Controls.TabItem"/>s in a
    /// <see cref="System.Windows.Controls.TabControl"/>.
    /// </remarks>
    public interface IDockTabPane : IDockPane
    {
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        /// <remarks>
        /// This property is usually set automatically by the <see cref="DockStrategy"/>.
        /// </remarks>
        IDockTabItem SelectedItem { get; set; }


        /// <summary>
        /// Gets the items docked in this pane.
        /// </summary>
        /// <value>The items docked in this pane. Must not be <see langword="null"/>.</value>
        DockTabItemCollection Items { get; }
    }
}
