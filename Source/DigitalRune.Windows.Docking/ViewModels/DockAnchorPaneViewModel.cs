// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Markup;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an anchored pane in the docking layout.
    /// </summary>
    /// <inheritdoc cref="IDockAnchorPane"/>
    [ContentProperty(nameof(ChildPane))]
    public class DockAnchorPaneViewModel : DockPaneViewModel, IDockAnchorPane
    {
        /// <inheritdoc/>
        public IDockPane ChildPane
        {
            get { return _childPane; }
            set { SetProperty(ref _childPane, value); }
        }
        private IDockPane _childPane;
    }
}
