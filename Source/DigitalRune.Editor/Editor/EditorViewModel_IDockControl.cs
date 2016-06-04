// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor
{
    partial class EditorViewModel
    {
        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

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
        private IDockPane _rootPane = new DockAnchorPaneViewModel();


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
            set
            {
                if (SetProperty(ref _activeDockTabItem, value))
                    OnActiveDockTabItemChanged(EventArgs.Empty);
            }
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
        public DockControl DockControl
        {
            get { return _dockControl; }
            set
            {
                if (_dockControl == value)
                    return;

                if (_dockControl != null)
                {
                    ((INotifyCollectionChanged)_dockControl.FloatWindows).CollectionChanged -= OnFloatWindowsChanged;
                    ((INotifyCollectionChanged)_dockControl.AutoHideOverlays).CollectionChanged -= OnAutoHideOverlaysChanged;
                    RemoveInputBindings(_dockControl);
                    RemoveCommandBindings(_dockControl);
                }

                _dockControl = value;

                if (_dockControl != null)
                {
                    AddInputBindings(_dockControl);
                    AddCommandBindings(_dockControl);
                    ((INotifyCollectionChanged)_dockControl.FloatWindows).CollectionChanged += OnFloatWindowsChanged;
                    ((INotifyCollectionChanged)_dockControl.AutoHideOverlays).CollectionChanged += OnAutoHideOverlaysChanged;
                }
            }
        }
        private DockControl _dockControl;


        /// <inheritdoc/>
        public event EventHandler<EventArgs> LayoutChanged;


        /// <inheritdoc/>
        public event EventHandler<EventArgs> ActiveDockTabItemChanged;
        #endregion



        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnFloatWindowsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var window in eventArgs.NewItems.Cast<FloatWindow>())
                {
                    AddInputBindings(window);
                    AddCommandBindings(window);
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var window in DockControl.FloatWindows)
                {
                    // Clean up to avoid duplicates.
                    RemoveInputBindings(window);
                    RemoveCommandBindings(window);

                    AddInputBindings(window);
                    AddCommandBindings(window);
                }
            }
        }


        private void OnAutoHideOverlaysChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var window in eventArgs.NewItems.Cast<AutoHideOverlay>())
                {
                    AddInputBindings(window);
                    AddCommandBindings(window);
                }
            }
            else if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var window in DockControl.AutoHideOverlays)
                {
                    // Clean up to avoid duplicates.
                    RemoveInputBindings(window);
                    RemoveCommandBindings(window);

                    AddInputBindings(window);
                    AddCommandBindings(window);
                }
            }
        }


        /// <summary>
        /// Raises the <see cref="LayoutChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding <see cref="OnLayoutChanged"/>
        /// in a derived class, be sure to call the base class's <see cref="OnLayoutChanged"/>
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected internal virtual void OnLayoutChanged(EventArgs eventArgs)    // Called in EditorDockStrategy.
        {
            LayoutChanged?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="ActiveDockTabItemChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding <see cref="OnActiveDockTabItemChanged"/>
        /// in a derived class, be sure to call the base class's <see cref="OnActiveDockTabItemChanged"/>
        /// method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnActiveDockTabItemChanged(EventArgs eventArgs)
        {
            ActiveDockTabItemChanged?.Invoke(this, eventArgs);
        }


        #region ----- DockSerializer -----

        /// <summary>
        /// Loads the docking layout.
        /// </summary>
        /// <param name="storedLayout">The stored layout.</param>
        /// <inheritdoc cref="DockSerializer.Load"/>
        public void LoadLayout(XElement storedLayout)
        {
            try
            {
                DockStrategy.Begin();

                var oldItems = Items.ToList();
                DockSerializer.Load(this, storedLayout);
                var newItems = Items.ToList();

                // Screen conduction for items closed in Load().
                foreach (var dockTabItem in oldItems.Except(newItems))
                {
                    // IActivatable
                    (dockTabItem as IActivatable)?.OnDeactivate(true);

                    // IScreen
                    var screen = dockTabItem as IScreen;
                    if (screen != null)
                        screen.Conductor = null;
                }
            }
            finally
            {
                DockStrategy.End();
            }
        }


        /// <summary>
        /// Saves the docking layout.
        /// </summary>
        /// <param name="excludeNonPersistentItems">
        /// <see langword="true"/> to exclude non-persistent <see cref="IDockTabItem"/>s.
        /// <see langword="false"/> to store the layout of all (persistent and non-persistent)
        /// <see cref="IDockTabItem"/>s.
        /// </param>
        /// <returns>The <see cref="XElement"/> with the serialized layout.</returns>
        /// <inheritdoc cref="DockSerializer.Save(IDockControl,bool)"/>
        public XElement SaveLayout(bool excludeNonPersistentItems = false)
        {
            try
            {
                DockStrategy.Begin();
                return DockSerializer.Save(this, excludeNonPersistentItems);
            }
            finally
            {
                DockStrategy.End();
            }
        }
        #endregion

        #endregion
    }
}
