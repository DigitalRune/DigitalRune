// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows.Markup;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents the control that contains and manages the docking layout.
    /// </summary>
    /// <inheritdoc cref="IDockControl"/>
    [ContentProperty(nameof(RootPane))]
    public class DockControlViewModel : ObservableObject, IDockControl
    {
        /// <inheritdoc/>
        public DockStrategy DockStrategy
        {
            get { return _dockStrategy; }
            set
            {
                if (_dockStrategy == value)
                    return;

                if (_dockStrategy != null)
                    _dockStrategy.DockControl = null;

                _dockStrategy = value;

                if (_dockStrategy != null)
                    _dockStrategy.DockControl = this;

                RaisePropertyChanged();
            }
        }
        private DockStrategy _dockStrategy;


        /// <summary>
        /// Gets or sets a value indicating whether the docking layout is locked.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the layout is locked to prevent dragging operations;
        /// otherwise, <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsLocked
        {
            get { return _isLocked; }
            set { SetProperty(ref _isLocked, value); }
        }
        private bool _isLocked;


        /// <inheritdoc/>
        public IDockPane RootPane
        {
            get { return _rootPane; }
            set { SetProperty(ref _rootPane, value); }
        }
        private IDockPane _rootPane;


        /// <inheritdoc/>
        public IDockTabPane ActiveDockTabPane
        {
            get { return _activeDockTabPane; }
            set { SetProperty(ref _activeDockTabPane, value); }
        }
        private IDockTabPane _activeDockTabPane;


        /// <inheritdoc/>
        public IDockTabItem ActiveDockTabItem
        {
            get { return _activeDockTabItem; }
            set { SetProperty(ref _activeDockTabItem, value); }
        }
        private IDockTabItem _activeDockTabItem;


        /// <inheritdoc/>
        public FloatWindowCollection FloatWindows { get; } = new FloatWindowCollection();


        /// <inheritdoc/>
        public DockTabPaneCollection AutoHideLeft { get; } = new DockTabPaneCollection();


        /// <inheritdoc/>
        public DockTabPaneCollection AutoHideRight { get; } = new DockTabPaneCollection();


        /// <inheritdoc/>
        public DockTabPaneCollection AutoHideTop { get; } = new DockTabPaneCollection();


        /// <inheritdoc/>
        public DockTabPaneCollection AutoHideBottom { get; } = new DockTabPaneCollection();


        /// <inheritdoc/>
        public DockControl DockControl { get; set; }


        /// <overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DockControlViewModel" /> class.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Initializes a new instance of the <see cref="DockControlViewModel" /> class without a
        /// <see cref="Docking.DockStrategy"/>. The <see cref="DockStrategy"/> needs to be set
        /// manually.
        /// </summary>
        public DockControlViewModel()
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockControlViewModel" /> class with the
        /// specified <see cref="Docking.DockStrategy"/>.
        /// </summary>
        /// <param name="dockStrategy">The<see cref="Docking.DockStrategy"/>.</param>
        public DockControlViewModel(DockStrategy dockStrategy)
        {
            DockStrategy = dockStrategy;
        }
    }
}
