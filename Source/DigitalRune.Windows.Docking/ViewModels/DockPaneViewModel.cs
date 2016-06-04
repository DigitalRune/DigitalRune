// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an pane in the docking layout.
    /// </summary>
    /// <inheritdoc cref="IDockPane"/>
    public abstract class DockPaneViewModel : DockElementViewModel, IDockPane
    {
        /// <inheritdoc/>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }
        private bool _isVisible;
    }
}
