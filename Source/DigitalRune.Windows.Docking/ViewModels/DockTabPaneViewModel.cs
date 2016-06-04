// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Markup;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane that contains multiple items that share the same space on the screen.
    /// </summary>
    /// <inheritdoc cref="IDockTabPane"/>
    [ContentProperty(nameof(Items))]
    public class DockTabPaneViewModel : DockPaneViewModel, IDockTabPane
    {
        /// <inheritdoc/>
        public IDockTabItem SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }
        private IDockTabItem _selectedItem;


        /// <inheritdoc/>
        public DockTabItemCollection Items { get; } = new DockTabItemCollection();
    }
}
