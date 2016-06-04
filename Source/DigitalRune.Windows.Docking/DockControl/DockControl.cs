// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a control that contains and manages the docking layout.
    /// </summary>
    [TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
    public partial class DockControl : ContentControl
    {
        // Abbreviations:
        // - VM ... view-model


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------    

        private ContentPresenter _contentPresenter;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <inheritdoc cref="IDockControl.DockStrategy"/>
        internal DockStrategy DockStrategy { get; private set; }


        /// <summary>
        /// Gets or sets the <see cref="DragManager"/>.
        /// </summary>
        /// <value>The <see cref="DragManager"/>.</value>
        /// <remarks>
        /// The <see cref="DragManager"/> handles dragging of <see cref="DockTabItem"/>s, 
        /// <see cref="DockTabPane"/>s and <see cref="FloatWindow"/>s using the mouse.
        /// </remarks>
        internal DragManager DragManager { get; }


        /// <summary>
        /// Gets the <see cref="FrameworkElement"/> that represents the root pane.
        /// </summary>
        /// <value>The <see cref="FrameworkElement"/> that represents the root pane.</value>
        internal FrameworkElement RootPane
        {
            get { return _contentPresenter.GetContentContainer<FrameworkElement>(); }
        }


        /// <summary>
        /// Gets a (read-only) collection of all loaded <see cref="DockTabPane"/>s.
        /// </summary>
        /// <value>A collection of all loaded <see cref="DockTabPane"/>s.</value>
        public ReadOnlyObservableCollection<DockTabPane> DockTabPanes { get; }
        private readonly ObservableCollection<DockTabPane> _dockTabPanes;


        /// <summary>
        /// Gets a (read-only) collection of all loaded <see cref="DockTabItem"/>s.
        /// </summary>
        /// <value>A collection of all loaded <see cref="DockTabItem"/>s.</value>
        public ReadOnlyObservableCollection<DockTabItem> DockTabItems { get; }
        private readonly ObservableCollection<DockTabItem> _dockTabItems;


        /// <summary>
        /// Gets a (read-only) collection of all loaded <see cref="FloatWindow"/>s that are managed
        /// by the <see cref="DockControl"/>.
        /// </summary>
        /// <value>A collection of <see cref="FloatWindow"/>s.</value>
        public ReadOnlyObservableCollection<FloatWindow> FloatWindows { get; }
        private readonly ObservableCollection<FloatWindow> _floatWindows;


        /// <summary>
        /// Gets a (read-only) collection of all loaded <see cref="AutoHideOverlay"/> windows that
        /// are managed by the <see cref="DockControl"/>.
        /// </summary>
        /// <value>A collection of <see cref="AutoHideOverlay"/> windows.</value>
        public ReadOnlyObservableCollection<AutoHideOverlay> AutoHideOverlays { get; }
        // AutoHideOverlays add/remove themselves to/from this list.
        internal readonly ObservableCollection<AutoHideOverlay> _autoHideOverlays;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockControl"/> class.
        /// </summary>
        static DockControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockControl), new FrameworkPropertyMetadata(typeof(DockControl)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockControl"/> class.
        /// </summary>
        public DockControl()
        {
            // Create internal collections.
            _dockTabItems = new ObservableCollection<DockTabItem>();
            _dockTabPanes = new ObservableCollection<DockTabPane>();
            _floatWindows = new ObservableCollection<FloatWindow>();
            _autoHideOverlays = new ObservableCollection<AutoHideOverlay>();

            // Create read-only wrappers.
            DockTabItems = new ReadOnlyObservableCollection<DockTabItem>(_dockTabItems);
            DockTabPanes = new ReadOnlyObservableCollection<DockTabPane>(_dockTabPanes);
            FloatWindows = new ReadOnlyObservableCollection<FloatWindow>(_floatWindows);
            AutoHideOverlays = new ReadOnlyObservableCollection<AutoHideOverlay>(_autoHideOverlays);

            // Attach DragManager.
            DragManager = new DragManager(this);

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the view for a view model.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns>The <see cref="DockTabItem"/>.</returns>
        public DockTabItem GetView(IDockTabItem viewModel)
        {
            for (int i = 0; i < _dockTabItems.Count; i++)
                if (_dockTabItems[i].DataContext == viewModel)
                    return _dockTabItems[i];

            return null;
        }


        private FloatWindow GetView(IFloatWindow viewModel)
        {
            for (int i = 0; i < _floatWindows.Count; i++)
                if (_floatWindows[i].DataContext == viewModel)
                    return _floatWindows[i];

            return null;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!IsLoaded)
                return; // Wait until control is loaded.

            var oldContext = eventArgs.OldValue as IDockControl;
            if (oldContext != null)
                UnregisterEventHandlers(oldContext);

            var newContext = eventArgs.NewValue as IDockControl;
            if (newContext != null)
                RegisterEventHandlers(newContext);

            CommandManager.InvalidateRequerySuggested();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DockStrategy")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "IDockControl")]
        private void RegisterEventHandlers(IDockControl dataContext)
        {
            if (dataContext == null)
                return;

            // Attach to IDockControl.
            if (dataContext.DockStrategy == null)
                throw new DockException("IDockControl.DockStrategy must not be null.");

            DockStrategy = dataContext.DockStrategy;

            // Observe IDockControl properties.
            PropertyChangedEventManager.AddHandler(dataContext, OnDockStrategyChanged, nameof(IDockControl.DockStrategy));
            PropertyChangedEventManager.AddHandler(dataContext, OnActiveItemChanged, nameof(IDockControl.ActiveDockTabItem));
            //PropertyChangedEventManager.AddHandler(dataContext, OnActivePaneChanged, nameof(IDockControl.ActiveDockTabPane));

            // The ICollectionView is used to filter IFloatWindows.
            var collectionView = CollectionViewSource.GetDefaultView(dataContext.FloatWindows);
            var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
            if (collectionViewLiveShaping != null && collectionViewLiveShaping.CanChangeLiveFiltering)
            {
                collectionViewLiveShaping.LiveFilteringProperties.Clear();
                collectionViewLiveShaping.LiveFilteringProperties.Add(nameof(IFloatWindow.IsVisible));
                collectionViewLiveShaping.IsLiveFiltering = true;
                collectionView.Filter = floatWindow => ((IFloatWindow)floatWindow).IsVisible;
            }
            CollectionChangedEventManager.AddHandler(collectionView, OnFloatWindowsChanged);
        }


        private void UnregisterEventHandlers(IDockControl dataContext)
        {
            if (dataContext == null)
                return;

            // Detach from IDockControl.
            DockStrategy = null;
            PropertyChangedEventManager.RemoveHandler(dataContext, OnDockStrategyChanged, nameof(IDockControl.DockStrategy));
            PropertyChangedEventManager.RemoveHandler(dataContext, OnActiveItemChanged, nameof(IDockControl.ActiveDockTabItem));
            //PropertyChangedEventManager.RemoveHandler(dataContext, OnActivePaneChanged, nameof(IDockControl.ActiveDockTabPane));

            var collectionView = CollectionViewSource.GetDefaultView(dataContext.FloatWindows);
            CollectionChangedEventManager.RemoveHandler(collectionView, OnFloatWindowsChanged);
        }


        /// <summary>
        /// Called when <see cref="IDockControl.ActiveDockTabItem"/> changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnDockStrategyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            DockStrategy = this.GetViewModel()?.DockStrategy;

            // Update menu items and buttons.
            CommandManager.InvalidateRequerySuggested();
        }


        ///// <summary>
        ///// Called when <see cref="IDockControl.ActiveDockTabPane"/> changed.
        ///// </summary>
        ///// <param name="sender">The sender.</param>
        ///// <param name="eventArgs">
        ///// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
        ///// </param>
        //private void OnActivePaneChanged(object sender, PropertyChangedEventArgs eventArgs)
        //{
        //}


        /// <summary>
        /// Called when <see cref="IDockControl.ActiveDockTabItem"/> changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="PropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnActiveItemChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            Debug.Assert(DockStrategy != null);
            Debug.Assert(DockStrategy.DockControl.ActiveDockTabItem == null || DockStrategy.DockControl.ActiveDockTabPane != null,
                         "IDockControl.ActiveDockTabPane needs to be set before IDockControl.ActiveDockTabItem.");
            Debug.Assert(DockStrategy.DockControl.ActiveDockTabPane == null || DockStrategy.DockControl.ActiveDockTabPane.SelectedItem == DockStrategy.DockControl.ActiveDockTabItem,
                         "IDockControl.ActiveDockTabPane.SelectedItem needs to match IDockControl.ActiveDockTabItem.");

            var dockTabPaneVM = DockStrategy.DockControl.ActiveDockTabPane;
            var dockTabItemVM = DockStrategy.DockControl.ActiveDockTabItem;
            if (dockTabPaneVM == null || dockTabItemVM == null)
            {
                CloseAutoHidePanes();
                return;
            }

            if (dockTabItemVM.DockState == DockState.Dock)
            {
                CloseAutoHidePanes();
            }
            else if (dockTabItemVM.DockState == DockState.Float)
            {
                CloseAutoHidePanes();
                var floatWindowVM = DockStrategy?.GetFloatWindow(dockTabItemVM);
                GetView(floatWindowVM)?.Activate();
            }
            else if (dockTabItemVM.DockState == DockState.AutoHide)
            {
                ShowAutoHidePane(dockTabPaneVM, dockTabItemVM);
            }

            GetView(dockTabItemVM)?.Activate();
        }


        /// <summary>
        /// Registers the specified <see cref="DockTabPane"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="DockTabPane"/> to add.</param>
        internal void Register(DockTabPane dockTabPane)
        {
            Debug.Assert(dockTabPane != null);

            if (!_dockTabPanes.Contains(dockTabPane))
                _dockTabPanes.Add(dockTabPane);
        }


        /// <summary>
        /// Unregisters the specified <see cref="DockTabPane"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="DockTabPane"/> to remove.</param>
        internal void Unregister(DockTabPane dockTabPane)
        {
            _dockTabPanes.Remove(dockTabPane);
        }


        /// <summary>
        /// Registers the specified <see cref="DockTabItem"/>.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="DockTabItem"/> to add.</param>
        internal void Register(DockTabItem dockTabItem)
        {
            Debug.Assert(dockTabItem != null);

            if (!_dockTabItems.Contains(dockTabItem))
                _dockTabItems.Add(dockTabItem);

            // Activate the DockTabItem if it is the IDockControl.ActiveDockTabItem.
            if (DockStrategy != null)
            {
                var dockTabItemVM = dockTabItem.GetViewModel();
                if (dockTabItemVM != null && DockStrategy.DockControl.ActiveDockTabItem == dockTabItemVM)
                    dockTabItem.Activate();
            }
        }


        /// <summary>
        /// Unregisters the specified <see cref="DockTabItem"/>.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="DockTabItem"/> to remove.</param>
        internal void Unregister(DockTabItem dockTabItem)
        {
            Debug.Assert(dockTabItem != null);

            _dockTabItems.Remove(dockTabItem);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            var viewModel = DataContext as IDockControl;

            if (viewModel != null)
                viewModel.DockControl = this;

            RegisterEventHandlers(viewModel);
            LoadFloatWindows();
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            CloseAutoHidePanes();
            UnloadFloatWindows();

            var viewModel = DataContext as IDockControl;
            UnregisterEventHandlers(viewModel);

            if (viewModel != null)
                viewModel.DockControl = null;
        }
        #endregion
    }
}
