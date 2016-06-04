// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;
using NLog;


namespace DigitalRune.Editor
{
    /// <summary>
    /// Represents a dockable window in the editor layout.
    /// </summary>
    public abstract class EditorDockTabItemViewModel : Screen, IDockTabItem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #region ----- IDockElement -----

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
        #endregion


        #region ----- IDockTabItem -----

        /// <inheritdoc/>
        public DockState LastDockState
        {
            get { return _lastDockState; }
            set { SetProperty(ref _lastDockState, value); }
        }
        private DockState _lastDockState;


        /// <summary>
        /// Gets (or sets) a value indicating whether this item remains in the docking layout even
        /// when hidden.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this item remains in the docking layout; otherwise,
        /// <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        /// <inheritdoc/>
        public bool IsPersistent
        {
            get { return _isPersistent; }
            protected set { SetProperty(ref _isPersistent, value); }
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
        /// <value>The icon. The default value is <see langword="null"/>.</value>
        /// <inheritdoc/>
        public object Icon
        {
            get { return _icon; }
            set { SetProperty(ref _icon, value); }
        }
        private object _icon;


        /// <summary>
        /// Gets the title (same <see cref="Screen.DisplayName"/>).
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get { return DisplayName; }
        }


        /// <summary>
        /// Gets (or sets) the ID of this <see cref="IDockTabItem"/>.
        /// </summary>
        /// <inheritdoc/>
        public string DockId
        {
            get { return _dockId; }
            protected set { SetProperty(ref _dockId, value); }
        }
        private string _dockId;
        #endregion


        /// <summary>
        /// Gets or sets the context menu of the window tab.
        /// </summary>
        /// <value>The context menu of the window tab.</value>
        /// <remarks>
        /// Per default, the context menu of the editor (see
        /// <see cref="IEditorService.DockContextMenu"/>) is set. That means changing the content
        /// of the default menu item collection influences the default context menus of all other
        /// <see cref="EditorDockTabItemViewModel"/>s.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public MenuItemViewModelCollection DockContextMenu
        {
            get { return _dockContextMenu; }
            set { SetProperty(ref _dockContextMenu, value); }
        }
        private MenuItemViewModelCollection _dockContextMenu;


        /// <summary>
        /// Gets or sets the tooltip of the window tab.
        /// </summary>
        /// <value>The tooltip of the window tab. The default value is <see langword="null"/>.</value>
        public object DockToolTip
        {
            get { return _dockToolTip; }
            set { SetProperty(ref _dockToolTip, value); }
        }
        private object _dockToolTip;
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <inheritdoc/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            // Property Title is the same as DisplayName. --> Raise Title changed event when
            // DisplayName changes.
            if (string.IsNullOrEmpty(eventArgs.PropertyName) || nameof(DisplayName) == eventArgs.PropertyName)
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(IDockTabItem.Title));
            }

            base.OnPropertyChanged(eventArgs);
        }


        #region ----- IScreen -----

        /// <inheritdoc/>
        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            Logger.Debug("Activating {0} (\"{1}\").", GetType().Name, DisplayName);

            // If the user does not set a context menu, we use the default context menu defined
            // in the editor.
            if (DockContextMenu == null)
                DockContextMenu = (Conductor as EditorViewModel)?.DockContextMenu;

            base.OnActivated(eventArgs);
        }


        /// <inheritdoc/>
        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            Logger.Debug("Deactivating {0} (\"{1}\").", GetType().Name, DisplayName);

            base.OnDeactivated(eventArgs);
        }
        #endregion

        #endregion
    }
}
