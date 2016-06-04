// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Controls;
using System.Windows.Markup;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane in the docking layout that is split horizontally or vertically into two
    /// or more panes.
    /// </summary>
    /// <inheritdoc cref="IDockSplitPane"/>
    [ContentProperty(nameof(ChildPanes))]
    public class DockSplitPaneViewModel : DockPaneViewModel, IDockSplitPane
    {
        /// <inheritdoc/>
        public Orientation Orientation
        {
            get { return _orientation; }
            set { SetProperty(ref _orientation, value); }
        }
        private Orientation _orientation;


        /// <inheritdoc/>
        public DockPaneCollection ChildPanes { get; } = new DockPaneCollection();
    }
}
