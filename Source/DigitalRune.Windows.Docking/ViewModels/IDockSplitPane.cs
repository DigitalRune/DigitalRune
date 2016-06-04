// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Controls;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane in the docking layout that is split horizontally or vertically into two
    /// or more panes.
    /// </summary>
    public interface IDockSplitPane : IDockPane
    {
        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        /// <value>
        /// The orientation. <see cref="System.Windows.Controls.Orientation.Horizontal"/> means that
        /// the child panes are arranged horizontally next to each other.
        /// <see cref="System.Windows.Controls.Orientation.Vertical"/> means that the child panes
        /// are stacked vertically.
        /// </value>
        Orientation Orientation { get; set; }


        /// <summary>
        /// Gets the child panes.
        /// </summary>
        /// <value>The child panes. Must not be <see langword="null"/>.</value>
        DockPaneCollection ChildPanes { get; }
    }
}
