// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Shows <see cref="IDockTabItem"/>s that are in auto-hide state as tabs.
    /// </summary>
    /// <remarks>
    /// The <see cref="ItemsControl.ItemsSource"/> a collection of <see cref="IDockTabPane"/>s.
    /// </remarks>
    [TemplateVisualState(GroupName = "DockStates", Name = "Left")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Right")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Top")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Bottom")]
    public class AutoHideBar : ItemsControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly List<AutoHideOverlay> _autoHideOverlays = new List<AutoHideOverlay>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Dock"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockProperty = DockPanel.DockProperty.AddOwner(
            typeof(AutoHideBar),
            new FrameworkPropertyMetadata(Dock.Right, OnDockPropertyChanged));

        /// <summary>
        /// Gets or sets dock position of the <see cref="AutoHideBar"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The dock position. The default value is <see cref="System.Windows.Controls.Dock.Right"/>.
        /// </value>
        [Description("Gets or sets dock position of the auto-hide bar.")]
        [Category(Categories.Layout)]
        public Dock Dock
        {
            get { return (Dock)GetValue(DockProperty); }
            set { SetValue(DockProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="TargetArea"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WindowPanelProperty = DependencyProperty.Register(
            "TargetArea",
            typeof(FrameworkElement),
            typeof(AutoHideBar),
            new FrameworkPropertyMetadata(null, OnTargetAreaChanged));

        /// <summary>
        /// Gets or sets an element that defines the area over which the <see cref="AutoHidePane"/>s
        /// are rendered. This is a dependency property.
        /// </summary>
        /// <value>
        /// The <see cref="FrameworkElement"/> over which the <see cref="AutoHidePane"/>s are
        /// rendered.
        /// </value>
        /// <remarks>
        /// <see cref="TargetArea"/> can be an arbitrary <see cref="FrameworkElement"/> inside the 
        /// <see cref="DockControl"/>. <see cref="AutoHidePane"/>s will be shown inside this area.
        /// </remarks>
        [Description("Gets or sets the element where that defines the area over which the auto-hide panes are shown.")]
        [Category(Categories.Default)]
        public FrameworkElement TargetArea
        {
            get { return (FrameworkElement)GetValue(WindowPanelProperty); }
            set { SetValue(WindowPanelProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="AutoHideBar"/> class.
        /// </summary>
        static AutoHideBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoHideBar), new FrameworkPropertyMetadata(typeof(AutoHideBar)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnDockPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var autoHideBar = (AutoHideBar)dependencyObject;

            autoHideBar.CloseAutoHideOverlays();
            autoHideBar.UpdateVisualStates(true);
        }


        private static void OnTargetAreaChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            // The target area over which the AutoHidePanes should be shown has changed.
            // --> Remove all AutoHideOverlays.
            var autoHideBar = (AutoHideBar)dependencyObject;
            autoHideBar.CloseAutoHideOverlays();
        }


        /// <summary>
        /// Called when the <see cref="ItemsControl.ItemsSource"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (newValue != null)
            {
                // Filter IDockTabPanes that are invisible.
                var collectionView = CollectionViewSource.GetDefaultView(newValue);
                var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
                if (collectionViewLiveShaping != null && collectionViewLiveShaping.CanChangeLiveFiltering)
                {
                    collectionViewLiveShaping.LiveFilteringProperties.Clear();
                    collectionViewLiveShaping.LiveFilteringProperties.Add(nameof(IDockPane.IsVisible));
                    collectionViewLiveShaping.IsLiveFiltering = true;
                    collectionView.Filter = Filter;
                }
            }
        }


        private static bool Filter(object item)
        {
            var dockTabPane = item as IDockTabPane;
            return dockTabPane == null || dockTabPane.IsVisible;
        }


        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// <see langword="true"/> if the item is (or is eligible to be) its own container;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is AutoHideGroup;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new AutoHideGroup();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateVisualStates(false);
        }


        /// <summary>
        /// Invoked when the <see cref="ItemsControl.Items"/> property changes.
        /// </summary>
        /// <param name="e">Information about the change.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            // Remove invalid AutoHideOverlays.
            for (int i = _autoHideOverlays.Count - 1; i >= 0; i--)
            {
                var autoHideOverlay = _autoHideOverlays[i];
                var autoHidePane = (AutoHidePane)autoHideOverlay.Content;
                var dockTabPane = autoHidePane.Content as IDockTabPane;
                if (dockTabPane != null && !Items.Contains(dockTabPane))
                    CloseAutoHideOverlay(autoHideOverlay);
            }
        }


        internal void TabClicked(AutoHideTab autoHideTab)
        {
            // Show AutoHidePane and set focus to content.
            var dockTabItem = autoHideTab.DataContext as IDockTabItem;
            var dockTabPane = autoHideTab.AutoHideGroup?.DataContext as IDockTabPane;
            var dockControl = DockHelper.GetDockControl(this);
            if (dockControl != null)
                dockControl.CloseAutoHidePanesExcept(dockTabPane);
            else
                CloseAutoHidePanesExcept(dockTabPane);

            ShowAutoHidePane(dockTabPane, dockTabItem, true);
        }


        internal void TabEnter(AutoHideTab autoHideTab)
        {
            // Show AutoHidePane, but do not set focus.
            var dockTabItem = autoHideTab.DataContext as IDockTabItem;
            var dockTabPane = autoHideTab.AutoHideGroup?.DataContext as IDockTabPane;
            var autoHidePane = ShowAutoHidePane(dockTabPane, dockTabItem, false);

            // Make sure that the AutoHidePane stays open, as long as the mouse cursor is on the tab.
            if (autoHidePane != null)
                autoHidePane.HideAutomatically = false;
        }


        internal void TabLeave(AutoHideTab autoHideTab)
        {
            // Mouse cursor has left the tab.
            // Re-enable automatic hiding of the AutoHidePane.
            var dockTabPane = autoHideTab.AutoHideGroup?.DataContext as IDockTabPane;
            var autoHidePane = GetAutoHidePane(dockTabPane);
            if (autoHidePane != null)
                autoHidePane.HideAutomatically = true;
        }


        private AutoHidePane GetAutoHidePane(IDockTabPane dockTabPane)
        {
            foreach (var autoHideOverlay in _autoHideOverlays)
            {
                var autoHidePane = (AutoHidePane)autoHideOverlay.Content;
                if (autoHidePane.Content == dockTabPane)
                    return autoHidePane;
            }

            return null;
        }


        internal AutoHidePane ShowAutoHidePane(IDockTabPane dockTabPane, IDockTabItem dockTabItem, bool focus)
        {
            if (dockTabPane == null || !Items.Contains(dockTabPane))
                return null;

            var dockControl = DockHelper.GetDockControl(this);

            // Ensure that correct item is selected.
            if (dockTabItem != null)
            {
                Debug.Assert(dockTabPane.Items.Contains(dockTabItem));
                dockTabPane.SelectedItem = dockTabItem;
            }

            // Get or create AutoHidePane that shows the DockTabItem.
            var autoHidePane = GetAutoHidePane(dockTabPane);
            if (autoHidePane == null)
            {
                // Create a new AutoHidePane if necessary.
                autoHidePane = new AutoHidePane
                {
                    Content = dockTabPane,
                    Dock = Dock,
                };
                var autoHideOverlay = new AutoHideOverlay(dockControl, TargetArea)
                {
                    Content = autoHidePane
                };
                autoHideOverlay.Show();
                _autoHideOverlays.Add(autoHideOverlay);
            }

            // Slide-in AutoHidePane.
            autoHidePane.Show();

            if (focus)
                dockControl?.DockStrategy?.Activate(dockTabPane);

            return autoHidePane;
        }


        internal void CloseAutoHidePanes()
        {
            for (int i = _autoHideOverlays.Count - 1; i >= 0; i--)
            {
                var autoHideOverlay = _autoHideOverlays[i];
                var autoHidePane = (AutoHidePane)autoHideOverlay.Content;
                CloseAutoHidePane(autoHidePane);
            }
        }


        internal void CloseAutoHidePanesExcept(IDockTabPane dockTabPane)
        {
            for (int i = _autoHideOverlays.Count - 1; i >= 0; i--)
            {
                var autoHideOverlay = _autoHideOverlays[i];
                var autoHidePane = (AutoHidePane)autoHideOverlay.Content;
                if (autoHidePane.Content != dockTabPane)
                    CloseAutoHidePane(autoHidePane);
            }
        }


        private void CloseAutoHidePane(AutoHidePane autoHidePane)
        {
            autoHidePane.Hide();
            var dockControl = DockHelper.GetDockControl(this);
            if (dockControl != null && autoHidePane.IsKeyboardFocusWithin)
            {
                // AutoHidePane still has focus, but it is moved out of the visible area.
                // --> Move focus back to DockControl.
                dockControl.Focus();
            }
        }


        private void CloseAutoHideOverlays()
        {
            foreach (var autoHideOverlay in _autoHideOverlays)
            {
                autoHideOverlay.Content = null;
                autoHideOverlay.Close();
            }

            _autoHideOverlays.Clear();
        }


        private void CloseAutoHideOverlay(AutoHideOverlay autoHideOverlay)
        {
            autoHideOverlay.Content = null;
            autoHideOverlay.Close();

            _autoHideOverlays.Remove(autoHideOverlay);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            switch (Dock)
            {
                case Dock.Left:
                    VisualStateManager.GoToState(this, "Left", useTransitions);
                    break;
                case Dock.Right:
                    VisualStateManager.GoToState(this, "Right", useTransitions);
                    break;
                case Dock.Top:
                    VisualStateManager.GoToState(this, "Top", useTransitions);
                    break;
                case Dock.Bottom:
                    VisualStateManager.GoToState(this, "Bottom", useTransitions);
                    break;
            }
        }
        #endregion
    }
}
