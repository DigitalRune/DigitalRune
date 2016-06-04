// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a selectable, draggable item in the docking layout.
    /// </summary>
    /// <inheritdoc cref="IDockTabItem"/>
    public class DockTabItemViewModel : DockElementViewModel, IDockTabItem
    {
        /// <inheritdoc/>
        public DockState LastDockState
        {
            get { return _lastDockState; }
            set { SetProperty(ref _lastDockState, value); }
        }
        private DockState _lastDockState;


        /// <summary>
        /// Gets or sets a value indicating whether this item remains in the docking layout even
        /// when hidden.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item remains in the docking layout; otherwise,
        /// <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsPersistent
        {
            get { return _isPersistent; }
            set { SetProperty(ref _isPersistent, value); }
        }
        private bool _isPersistent;

        
        /// <inheritdoc/>
        public DateTime LastActivation
        {
            get { return _lastActivation; }
            set { SetProperty(ref _lastActivation, value); }
        }
        private DateTime _lastActivation;


        /// <inheritdoc/>
        public double AutoHideWidth
        {
            get { return _autoHideWidth; }
            set { SetProperty(ref _autoHideWidth, value); }
        }
        private double _autoHideWidth = double.NaN;


        /// <inheritdoc/>
        public double AutoHideHeight
        {
            get { return _autoHideHeight; }
            set { SetProperty(ref _autoHideHeight, value); }
        }
        private double _autoHideHeight = double.NaN;


        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        /// <inheritdoc/>
        public object Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
        private object _icon;


        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        /// <inheritdoc/>
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }
        private string _title;


        /// <inheritdoc/>
        public string DockId
        {
            get { return _dockId; }
            set { SetProperty(ref _dockId, value); }
        }
        private string _dockId;
    }
}
