// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an element in the docking layout.
    /// </summary>
    /// <inheritdoc cref="IDockElement"/>
    public abstract class DockElementViewModel : ObservableObject, IDockElement
    {
        /// <inheritdoc/>
        public DockState DockState
        {
            get { return _dockState; }
            set { SetProperty(ref _dockState, value); }
        }
        private DockState _dockState;


        /// <inheritdoc/>
        public GridLength DockWidth
        {
            get { return _dockWidth; }
            set { SetProperty(ref _dockWidth, value); }
        }
        private GridLength _dockWidth = new GridLength(1, GridUnitType.Star);


        /// <inheritdoc/>
        public GridLength DockHeight
        {
            get { return _dockHeight; }
            set { SetProperty(ref _dockHeight, value); }
        }
        private GridLength _dockHeight = new GridLength(1, GridUnitType.Star);
    }
}
