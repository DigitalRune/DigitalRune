// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.FormattableString;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Controls the docking inside an <see cref="IDockControl"/> and its
    /// <see cref="IFloatWindow"/>s.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DockStrategy"/> handles all layout changes within a
    /// <see cref="IDockControl"/> and its <see cref="IFloatWindow"/>s.
    /// </para>
    /// <para>
    /// The <see cref="DockStrategy"/> is an abstract base class. The method 
    /// <see cref="OnCreateDockTabItem"/> need to be implemented by the user to provide the view 
    /// model that represents a <see cref="IDockTabItem"/>. Other <strong>OnCreate*</strong> methods
    /// can be overridden to use custom view models for other dock elements. The class also 
    /// includes other virtual methods that can be overridden to customize the docking behavior.
    /// </para>
    /// </remarks>
    public abstract class DockStrategy : ObservableObject
    {
        // Design notes:
        // - Minimize use of LINQ to reduce unnecessary memory allocations.


        // Incremented in Begin(). Decremented in End().
        private int _dockOperationCount;


        /// <summary>
        /// Gets the <see cref="IDockControl"/>.
        /// </summary>
        /// <value>The <see cref="IDockControl"/>.</value>
        public IDockControl DockControl
        {
            get { return _dockControl; }
            set
            {
                if (_dockControl == value)
                    return;

                if (value != null)
                {
                    // Basic validation of IDockControl.
                    if (value.FloatWindows == null)
                        throw new ArgumentException("Dock control is invalid: FloatWindows must not be null.", nameof(value));
                    if (value.AutoHideLeft == null)
                        throw new ArgumentException("Dock control is invalid: AutoHideLeft must not be null.", nameof(value));
                    if (value.AutoHideRight == null)
                        throw new ArgumentException("Dock control is invalid: AutoHideRight must not be null.", nameof(value));
                    if (value.AutoHideTop == null)
                        throw new ArgumentException("Dock control is invalid: AutoHideTop must not be null.", nameof(value));
                    if (value.AutoHideBottom == null)
                        throw new ArgumentException("Dock control is invalid: AutoHideBottom must not be null.", nameof(value));
                }

                if (_dockControl != null)
                {
                    // Weak event handler for IDockControl.ActiveDockTabItem.
                    PropertyChangedEventManager.RemoveHandler(DockControl, OnRootPaneChanged, nameof(DockControl.RootPane));
                    PropertyChangedEventManager.RemoveHandler(DockControl, OnActiveItemChanged, nameof(DockControl.ActiveDockTabItem));
                }

                _dockControl = value;

                if (value != null)
                {
                    // Weak event handler for IDockControl.ActiveDockTabItem.
                    PropertyChangedEventManager.AddHandler(DockControl, OnRootPaneChanged, nameof(DockControl.RootPane));
                    PropertyChangedEventManager.AddHandler(DockControl, OnActiveItemChanged, nameof(DockControl.ActiveDockTabItem));

                    Cleanup();
                }

                RaisePropertyChanged();
            }
        }
        private IDockControl _dockControl;


        /// <summary>
        /// Gets a value indicating whether a docking operation is currently in progress.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a docking operation is in progress; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsBusy
        {
            get { return _dockOperationCount > 0; }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private void ThrowIfDockControlIsNull()
        {
            if (DockControl == null)
                throw new DockException("DockControl is not set in DockStrategy.");
        }


        /// <summary>
        /// Indicates the start of a docking operation that involves multiple steps.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Complex layout changes may involve multiple steps. In order to prevent the view from
        /// interfering with the docking operations the methods <see cref="Begin"/> and
        /// <see cref="End"/> need to be called.
        /// </para>
        /// <para><see cref="Begin"/>- <see cref="End"/> section can be nested.</para>
        /// </remarks>
        public void Begin()
        {
            ThrowIfDockControlIsNull();
            Debug.Assert(_dockOperationCount >= 0);

            _dockOperationCount++;

            if (_dockOperationCount == 1)
            {
                RaisePropertyChanged(nameof(IsBusy));
                OnBegin();
            }
        }


        /// <summary>
        /// Ends a docking operation that involved multiple steps.
        /// </summary>
        /// <inheritdoc cref="Begin"/>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        public void End()
        {
            ThrowIfDockControlIsNull();

            if (_dockOperationCount == 0)
                throw new InvalidOperationException("DockStrategy.End() called without matching DockStrategy.Begin().");

            _dockOperationCount--;

            if (_dockOperationCount == 0)
            {
                Cleanup();
                RaisePropertyChanged(nameof(IsBusy));
                OnEnd();
            }
        }


        /// <summary>
        /// Called when a docking operation started.
        /// </summary>
        /// <remarks>
        /// Note that <see cref="Begin"/> and <see cref="End"/> may be called multiple times (nested
        /// docking operation). <see cref="OnBegin"/> and <see cref="OnEnd"/> are only called once.
        /// </remarks>
        protected virtual void OnBegin()
        {
        }


        /// <summary>
        /// Called when a docking operation completed.
        /// </summary>
        /// <inheritdoc cref="OnBegin"/>
        protected virtual void OnEnd()
        {
        }


        private void OnRootPaneChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // Abort if this is not the active dock strategy.
            if (DockControl.DockStrategy != this)
                return;

            // Ensure that panes are properly initialized.
            if (!IsBusy)
                Cleanup();
        }


        private void OnActiveItemChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            // Abort if this is not the active dock strategy.
            if (DockControl.DockStrategy != this)
                return;

            // Update IDockTabItem.LastActivation.
            if (DockControl.ActiveDockTabItem != null)
                DockControl.ActiveDockTabItem.LastActivation = DateTime.UtcNow;
        }


        #region ----- Clean-up -----

        /// <summary>
        /// Removes empty panes/windows and updates all properties to ensure that the
        /// <see cref="IDockControl"/> remains in valid state.
        /// </summary>
        /// <remarks>
        /// Docking operations may result in dock elements that are no longer used in the layout.
        /// This method removes any empty dock elements and ensures that the elements are set to a
        /// valid state.
        /// </remarks>
        internal void Cleanup()
        {
            CleanupDockControl();
            CleanupFloatWindows();
            CleanupAutoHidePanes();

            // Initialize/update properties.
            // (When a layout is loaded by the user, e.g. via XAML, some properties might not
            // be initialized.)
            UpdateProperties();

            if (!IsBusy)
            {
                // Reset ActiveDockTabPane if the IDockTabPane has been removed.
                if (DockControl.ActiveDockTabPane != null)
                {
                    bool isValid = false;
                    switch (DockControl.ActiveDockTabPane.DockState)
                    {
                        case DockState.Dock:
                            isValid = DockControl.Contains(DockControl.ActiveDockTabPane);
                            break;
                        case DockState.Float:
                            foreach (var floatWindow in DockControl.FloatWindows)
                            {
                                if (floatWindow.Contains(DockControl.ActiveDockTabPane))
                                {
                                    isValid = true;
                                    break;
                                }
                            }
                            break;
                        case DockState.AutoHide:
                            isValid = DockHelper.Contains(DockControl.AutoHideLeft, DockControl.ActiveDockTabPane)
                                      || DockHelper.Contains(DockControl.AutoHideRight, DockControl.ActiveDockTabPane)
                                      || DockHelper.Contains(DockControl.AutoHideTop, DockControl.ActiveDockTabPane)
                                      || DockHelper.Contains(DockControl.AutoHideBottom, DockControl.ActiveDockTabPane);
                            break;
                    }

                    if (!isValid)
                        DockControl.ActiveDockTabPane = null;
                }

                // When the ActiveDockTabItem is closed or hidden, try to activate the previously active
                // item in the ActiveDockTabPane.
                if (DockControl.ActiveDockTabItem == null || DockControl.ActiveDockTabItem.DockState == DockState.Hide)
                {
                    if (DockControl.ActiveDockTabPane != null && DockControl.ActiveDockTabPane.IsVisible)
                        Activate(DockControl.ActiveDockTabPane);
                }

                // When the ActiveDockTabItem is moved into an inactive AutoHide pane, switch to the
                // previously active item.
                if (DockControl.ActiveDockTabItem != null && DockControl.ActiveDockTabItem.DockState == DockState.AutoHide
                    && DockControl.ActiveDockTabPane != null && DockControl.ActiveDockTabPane.DockState != DockState.AutoHide)
                {
                    ActivateLastActive();
                }

                // When ActiveDockTabItem is re-docked, the ActiveDockTabPane needs to be updated.
                if (DockControl.ActiveDockTabItem == null || DockControl.ActiveDockTabItem.DockState != DockState.Hide)
                {
                    Activate(DockControl.ActiveDockTabItem);
                }

                // If the ActiveDockTabPane or ActiveDockTabItem was removed, switch to the previously active item.
                if (DockControl.ActiveDockTabPane == null || DockControl.ActiveDockTabPane.Items.Count == 0
                    || DockControl.ActiveDockTabItem == null || DockControl.ActiveDockTabItem.DockState == DockState.Hide)
                {
                    ActivateLastActive();
                }
            }

            Validate();
        }


        private void CleanupDockControl()
        {
            RemoveObsoleteDockPanes(DockControl);

            // Restore default IDockAnchorPane if IDockControl is empty.
            if (DockControl.RootPane == null)
            {
                var defaultPane = CreateDockAnchorPane(DockState.Dock, DockHelper.GridLengthOneStar, DockHelper.GridLengthOneStar);
                DockControl.RootPane = defaultPane;
            }
        }


        private void CleanupFloatWindows()
        {
            for (int i = DockControl.FloatWindows.Count - 1; i >= 0; i--)
            {
                var floatWindow = DockControl.FloatWindows[i];
                RemoveObsoleteDockPanes(floatWindow);

                if (floatWindow.RootPane == null)
                    DockControl.FloatWindows.RemoveAt(i);
            }
        }


        private void CleanupAutoHidePanes()
        {
            RemoveObsoleteDockPanes(DockControl.AutoHideLeft);
            RemoveObsoleteDockPanes(DockControl.AutoHideRight);
            RemoveObsoleteDockPanes(DockControl.AutoHideTop);
            RemoveObsoleteDockPanes(DockControl.AutoHideBottom);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static void RemoveObsoleteDockPanes(IDockContainer rootContainer)
        {
            Debug.Assert(rootContainer != null);

            var obsoleteDockPane = FindObsoleteDockPane(rootContainer.RootPane);
            while (obsoleteDockPane != null)
            {
                Debug.Assert(obsoleteDockPane is IDockSplitPane || obsoleteDockPane is IDockTabPane);

                // A IDockSplitPane is also considered obsolete if it contains an single pane.
                var dockSplitPane = obsoleteDockPane as IDockSplitPane;
                if (dockSplitPane != null && dockSplitPane.ChildPanes.Count == 1)
                {
                    // Remove DockSplitPanel, but keep child pane.
                    var childPane = dockSplitPane.ChildPanes[0];
                    if (rootContainer.RootPane == dockSplitPane)
                    {
                        rootContainer.RootPane = childPane;
                    }
                    else
                    {
                        var parent = DockHelper.GetParent(rootContainer.RootPane, dockSplitPane);
                        DockHelper.Replace(parent, dockSplitPane, childPane);
                    }
                }
                else
                {
                    // Remove obsolete pane.
                    DockHelper.Remove(rootContainer, obsoleteDockPane);
                }

                obsoleteDockPane = FindObsoleteDockPane(rootContainer.RootPane);
            }
        }


        private static void RemoveObsoleteDockPanes(DockTabPaneCollection dockTabPanes)
        {
            Debug.Assert(dockTabPanes != null);

            for (int i = dockTabPanes.Count - 1; i >= 0; i--)
                if (dockTabPanes[i].Items.Count == 0)
                    dockTabPanes.RemoveAt(i);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static IDockPane FindObsoleteDockPane(IDockPane dockPane)
        {
            if (dockPane == null)
                return null;

            if (dockPane is IDockAnchorPane)
            {
                // IDockAnchorPanes are never considered empty.

                // Visit children.
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                return FindObsoleteDockPane(dockAnchorPane.ChildPane);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;

                // A IDockSplitPane is also considered obsolete if it contains 0 or 1 panes.
                if (dockSplitPane.ChildPanes.Count <= 1)
                    return dockSplitPane;

                // Visit children.
                foreach (var childPane in dockSplitPane.ChildPanes)
                {
                    var obsoleteDockPane = FindObsoleteDockPane(childPane);
                    if (obsoleteDockPane != null)
                        return obsoleteDockPane;
                }
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;

                // Empty IDockTabPane?
                if (dockTabPane.Items.Count == 0)
                    return dockTabPane;
            }

            return null;
        }


        /// <summary>
        /// Updates the <see cref="IDockElement.DockState"/> and <see cref="IDockPane.IsVisible"/>
        /// properties.
        /// </summary>
        private void UpdateProperties()
        {
            UpdateProperties(DockControl.RootPane, DockState.Dock);

            foreach (var floatWindow in DockControl.FloatWindows)
            {
                UpdateProperties(floatWindow.RootPane, DockState.Float);
                floatWindow.IsVisible = IsVisible(floatWindow);
            }

            UpdateProperties(DockControl.AutoHideLeft, DockState.AutoHide);
            UpdateProperties(DockControl.AutoHideRight, DockState.AutoHide);
            UpdateProperties(DockControl.AutoHideTop, DockState.AutoHide);
            UpdateProperties(DockControl.AutoHideBottom, DockState.AutoHide);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void UpdateProperties(IDockPane dockPane, DockState dockState)
        {
            Debug.Assert(dockPane != null);
            Debug.Assert(dockState != DockState.Hide);

            dockPane.DockState = dockState;
            dockPane.IsVisible = IsVisible(dockPane, dockState);

            // Visit children.
            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                if (dockAnchorPane.ChildPane != null)
                    UpdateProperties(dockAnchorPane.ChildPane, dockState);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                UpdateProperties(dockSplitPane.ChildPanes, dockState);
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;

                Debug.Assert(dockTabPane.Items.Count > 0);

                // When the currently selected item was removed, select the previously selected item.
                if (dockTabPane.SelectedItem == null)
                    SelectLastActive(dockTabPane, dockState);
            }
        }


        private void UpdateProperties(IReadOnlyList<IDockPane> dockPanes, DockState dockState)
        {
            Debug.Assert(dockPanes != null);

            foreach (var dockPane in dockPanes)
                UpdateProperties(dockPane, dockState);
        }
        #endregion


        #region ----- Validation -----

        [Conditional("DEBUG")]
        private void Validate()
        {
            if (WindowsHelper.IsInDesignMode)
                return;

            Debug.Assert(DockControl.RootPane != null, "IDockControl must not be empty.");
            Debug.Assert(FindObsoleteDockPane(DockControl.RootPane) == null, "IDockControl must not contain empty panes.");
            Debug.Assert(DockControl.AutoHideLeft.All(dockTabPane => dockTabPane.Items.Count > 0), "AutoHideBars must not contain empty panes.");
            Debug.Assert(DockControl.AutoHideRight.All(dockTabPane => dockTabPane.Items.Count > 0), "AutoHideBars must not contain empty panes.");
            Debug.Assert(DockControl.AutoHideTop.All(dockTabPane => dockTabPane.Items.Count > 0), "AutoHideBars must not contain empty panes.");
            Debug.Assert(DockControl.AutoHideBottom.All(dockTabPane => dockTabPane.Items.Count > 0), "AutoHideBars must not contain empty panes.");
            Debug.Assert(DockControl.FloatWindows.All(floatWindow => floatWindow.RootPane != null), "IFloatWindows must not be empty.");
            Debug.Assert(DockControl.FloatWindows.All(floatWindow => FindObsoleteDockPane(floatWindow.RootPane) == null), "IFloatWindows must not contain empty panes.");

            // Check for duplicates.
            var dockElements = DockHelper.GetDockElements(DockControl.RootPane).ToArray();
            Debug.Assert(dockElements.Length == dockElements.Distinct().Count(), "DockControl must not contain duplicate elements.");

            var floatElements = new List<IDockElement>();
            foreach (var floatWindow in DockControl.FloatWindows)
                floatElements.AddRange(DockHelper.GetDockElements(floatWindow.RootPane));

            Debug.Assert(floatElements.Count == floatElements.Distinct().Count(), "FloatWindows must not contain duplicate elements.");

            var autoHideElements = new List<IDockElement>();
            autoHideElements.AddRange(DockHelper.GetDockElements(DockControl.AutoHideLeft));
            autoHideElements.AddRange(DockHelper.GetDockElements(DockControl.AutoHideRight));
            autoHideElements.AddRange(DockHelper.GetDockElements(DockControl.AutoHideTop));
            autoHideElements.AddRange(DockHelper.GetDockElements(DockControl.AutoHideBottom));
            Debug.Assert(autoHideElements.Count == autoHideElements.Distinct().Count(), "AutoHideBars must not contain duplicate elements.");

            if (!IsBusy)
            {
                // Validate ActiveDockTabPane/ActiveDockTabItem.
                if (DockControl.ActiveDockTabPane != null || DockControl.ActiveDockTabItem != null)
                {
                    Debug.Assert(DockControl.ActiveDockTabPane != null, "Both ActiveDockTabPane and ActiveDockTabItem need to be set.");
                    Debug.Assert(DockControl.ActiveDockTabItem != null, "Both ActiveDockTabPane and ActiveDockTabItem need to be set.");
                    Debug.Assert(DockControl.ActiveDockTabPane.Items.Contains(DockControl.ActiveDockTabItem), "The ActiveDockTabPane must contain the ActiveDockTabItem.");
                    Debug.Assert(DockControl.ActiveDockTabPane.SelectedItem == DockControl.ActiveDockTabItem, "The ActiveDockTabItem must be selected in the ActiveDockTabPane.");
                    Debug.Assert(DockControl.ActiveDockTabItem.DockState != DockState.Hide, "ActiveDockTabItem must not be hidden.");

                    switch (DockControl.ActiveDockTabItem.DockState)
                    {
                        case DockState.Dock:
                            Debug.Assert(DockControl.Contains(DockControl.ActiveDockTabPane), "ActiveDockTabPane not found.");
                            break;
                        case DockState.AutoHide:
                            Debug.Assert(
                                DockControl.AutoHideLeft.Contains(DockControl.ActiveDockTabPane)
                                || DockControl.AutoHideRight.Contains(DockControl.ActiveDockTabPane)
                                || DockControl.AutoHideTop.Contains(DockControl.ActiveDockTabPane)
                                || DockControl.AutoHideBottom.Contains(DockControl.ActiveDockTabPane),
                                "ActiveDockTabPane not found.");
                            break;
                        case DockState.Float:
                            Debug.Assert(
                                DockControl.FloatWindows.Any(floatWindow => floatWindow.Contains(DockControl.ActiveDockTabPane)),
                                "ActiveDockTabPane not found.");
                            break;
                    }
                }
            }
        }
        #endregion


        #region ----- Factory methods -----

        internal IFloatWindow CreateFloatWindow()
        {
            return OnCreateFloatWindow();
        }


        internal IDockAnchorPane CreateDockAnchorPane()
        {
            return OnCreateDockAnchorPane();
        }


        private IDockAnchorPane CreateDockAnchorPane(DockState dockState, GridLength dockWidth, GridLength dockHeight)
        {
            var dockAnchorPane = OnCreateDockAnchorPane();
            dockAnchorPane.DockState = dockState;
            dockAnchorPane.DockWidth = dockWidth;
            dockAnchorPane.DockHeight = dockHeight;
            return dockAnchorPane;
        }


        internal IDockSplitPane CreateDockSplitPane()
        {
            return OnCreateDockSplitPane();
        }


        private IDockSplitPane CreateDockSplitPane(DockState dockState, GridLength dockWidth, GridLength dockHeight, Orientation orientation)
        {
            var dockSplitPane = OnCreateDockSplitPane();
            dockSplitPane.DockState = dockState;
            dockSplitPane.DockWidth = dockWidth;
            dockSplitPane.DockHeight = dockHeight;
            dockSplitPane.Orientation = orientation;
            return dockSplitPane;
        }


        internal IDockTabPane CreateDockTabPane()
        {
            return OnCreateDockTabPane();
        }


        internal IDockTabPane CreateDockTabPane(IDockTabItem dockTabItem, DockState dockState)
        {
            Debug.Assert(dockTabItem != null);

            var dockTabPane = OnCreateDockTabPane();
            dockTabPane.DockState = dockState;
            dockTabPane.DockWidth = dockTabItem.DockWidth;
            dockTabPane.DockHeight = dockTabItem.DockHeight;
            dockTabPane.Items.Add(dockTabItem);
            dockTabPane.SelectedItem = dockTabItem;
            return dockTabPane;
        }


        internal IDockTabItem CreateDockTabItem(string dockId)
        {
            var item = OnCreateDockTabItem(dockId);
            if (item != null && item.DockId != dockId)
                throw new DockException(Invariant($"IDockTabItem has invalid DockId. Expected value: '{dockId}', actual value: '{item.DockId}'"));

            return item;
        }


        /// <summary>
        /// Creates a new <see cref="IFloatWindow"/> instance.
        /// </summary>
        /// <returns>The <see cref="IFloatWindow"/> instance.</returns>
        /// <remarks>
        /// The default implementation returns an new instance of type
        /// <see cref="FloatWindowViewModel"/>.
        /// </remarks>
        protected virtual IFloatWindow OnCreateFloatWindow()
        {
            return new FloatWindowViewModel();
        }


        /// <summary>
        /// Creates a new <see cref="IDockAnchorPane"/> instance.
        /// </summary>
        /// <returns>The <see cref="IDockAnchorPane"/> instance.</returns>
        /// <remarks>
        /// The default implementation returns an new instance of type
        /// <see cref="DockAnchorPaneViewModel"/>.
        /// </remarks>
        protected virtual IDockAnchorPane OnCreateDockAnchorPane()
        {
            return new DockAnchorPaneViewModel();
        }


        /// <summary>
        /// Creates a new <see cref="IDockSplitPane"/> instance.
        /// </summary>
        /// <returns>The <see cref="IDockSplitPane"/> instance.</returns>
        /// <remarks>
        /// The default implementation returns an new instance of type
        /// <see cref="DockSplitPaneViewModel"/>.
        /// </remarks>
        protected virtual IDockSplitPane OnCreateDockSplitPane()
        {
            return new DockSplitPaneViewModel();
        }


        /// <summary>
        /// Creates a new <see cref="IDockTabPane"/> instance.
        /// </summary>
        /// <returns>The <see cref="IDockTabPane"/> instance.</returns>
        /// <remarks>
        /// The default implementation returns an new instance of type
        /// <see cref="DockTabPaneViewModel"/>.
        /// </remarks>
        protected virtual IDockTabPane OnCreateDockTabPane()
        {
            return new DockTabPaneViewModel();
        }


        /// <summary>
        /// Creates a new <see cref="IDockTabItem" /> instance.
        /// </summary>
        /// <param name="dockId">The dock ID. See <see cref="IDockTabItem.DockId"/>.</param>
        /// <returns>The <see cref="IDockTabItem"/> instance.</returns>
        /// <remarks>
        /// <para>
        /// Usually, <see cref="IDockTabItem"/>s represent different documents or tool windows of
        /// the application. This method uses the <paramref name="dockId"/> to create the correct
        /// type of <see cref="IDockTabItem"/>.
        /// </para>
        /// <para>
        /// This method is usually only used when a docking layout is loaded from a serialized
        /// docking layout. See also <see cref="DockSerializer"/>.
        /// </para>
        /// </remarks>
        protected abstract IDockTabItem OnCreateDockTabItem(string dockId);
        #endregion


        #region ----- DockState -----

        private static void SetDockState(IDockTabItem dockTabItem, DockState state)
        {
            Debug.Assert(dockTabItem != null);

            if (dockTabItem.DockState != DockState.Hide)
                dockTabItem.LastDockState = dockTabItem.DockState;

            dockTabItem.DockState = state;
        }


        private DockState GetActualDockState(IDockPane dockPane)
        {
            if (DockControl.Contains(dockPane))
                return DockState.Dock;

            if (GetFloatWindow(dockPane) != null)
                return DockState.Float;

            if (DockControl.AutoHideLeft.Contains(dockPane)
                || DockControl.AutoHideRight.Contains(dockPane)
                || DockControl.AutoHideTop.Contains(dockPane)
                || DockControl.AutoHideBottom.Contains(dockPane))
            {
                return DockState.AutoHide;
            }

            return DockState.Hide;
        }
        #endregion


        #region ----- Active/Selected -----

        // Can be called with null.
        internal void Activate(IDockTabItem dockTabItem)
        {
            if (dockTabItem == null)
            {
                DockControl.ActiveDockTabPane = null;
                DockControl.ActiveDockTabItem = null;
                return;
            }

            // Get the parent IDockTabPane.
            var dockTabPane = (IDockTabPane)DockHelper.GetParent(DockControl, dockTabItem);

            Debug.Assert(dockTabPane != null);
            Debug.Assert(IsVisible(dockTabPane, dockTabItem.DockState));

            dockTabPane.SelectedItem = dockTabItem;
            Activate(dockTabPane);
        }


        internal void Activate(IDockTabPane dockTabPane)
        {
            if (dockTabPane == null)
            {
                DockControl.ActiveDockTabPane = null;
                DockControl.ActiveDockTabItem = null;
            }
            else
            {
                Debug.Assert(dockTabPane.Items.Count > 0);
                Debug.Assert(dockTabPane.SelectedItem != null);

                DockControl.ActiveDockTabPane = dockTabPane;
                DockControl.ActiveDockTabItem = dockTabPane.SelectedItem;
            }
        }


        private void ActivateLastActive()
        {
            // Find last active items based on LastActivation.
            IDockTabPane lastActivePane = null;
            IDockTabItem lastActiveItem = null;
            FindLastActive(DockControl.RootPane, DockState.Dock, ref lastActivePane, ref lastActiveItem);
            foreach (var floatWindow in DockControl.FloatWindows)
                FindLastActive(floatWindow.RootPane, DockState.Float, ref lastActivePane, ref lastActiveItem);

            // Ignore auto-hide panes.
            //FindLastActive(DockControl.AutoHideLeft, DockState.AutoHide, ref lastActivePane, ref lastActiveItem);
            //FindLastActive(DockControl.AutoHideRight, DockState.AutoHide, ref lastActivePane, ref lastActiveItem);
            //FindLastActive(DockControl.AutoHideTop, DockState.AutoHide, ref lastActivePane, ref lastActiveItem);
            //FindLastActive(DockControl.AutoHideBottom, DockState.AutoHide, ref lastActivePane, ref lastActiveItem);

            if (lastActivePane != null)
                lastActivePane.SelectedItem = lastActiveItem;

            DockControl.ActiveDockTabPane = lastActivePane;
            DockControl.ActiveDockTabItem = lastActiveItem;
        }


        private void SelectLastActive(IDockTabPane dockTabPane, DockState dockState)
        {
            IDockTabItem dockTabItem = null;
            FindLastActive(dockTabPane, dockState, ref dockTabPane, ref dockTabItem);
            dockTabPane.SelectedItem = dockTabItem;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void FindLastActive(IDockPane dockPane, DockState dockState, ref IDockTabPane lastActivePane, ref IDockTabItem lastActiveItem)
        {
            Debug.Assert(dockPane != null);
            Debug.Assert(dockState != DockState.Hide);

            if (!IsVisible(dockPane, dockState))
                return;

            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                if (dockAnchorPane.ChildPane != null)
                    FindLastActive(dockAnchorPane.ChildPane, dockState, ref lastActivePane, ref lastActiveItem);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                FindLastActive(dockSplitPane.ChildPanes, dockState, ref lastActivePane, ref lastActiveItem);
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;
                foreach (var dockTabItem in dockTabPane.Items)
                {
                    if (dockTabItem.DockState != dockState || dockTabItem is DockTabItemProxy)
                        continue;

                    if (lastActiveItem == null || lastActiveItem.LastActivation < dockTabItem.LastActivation)
                    {
                        lastActivePane = dockTabPane;
                        lastActiveItem = dockTabItem;
                    }
                }
            }
        }


        private void FindLastActive(IReadOnlyList<IDockPane> dockPanes, DockState dockState, ref IDockTabPane lastActivePane, ref IDockTabItem lastActiveItem)
        {
            Debug.Assert(dockPanes != null);

            for (int i = 0; i < dockPanes.Count; i++)
                FindLastActive(dockPanes[i], dockState, ref lastActivePane, ref lastActiveItem);
        }
        #endregion


        #region ----- Layout queries -----

        /// <summary>
        /// Gets the default target for docking the specified element.
        /// </summary>
        /// <param name="element">The element to dock.</param>
        /// <returns>The <see cref="IDockPane"/> designated as the default target.</returns>
        /// <remarks>
        /// <para>
        /// The default implementation checks these panes (in this order) and returns the first
        /// suitable pane:
        /// </para>
        /// <list type="ordered">
        /// <item>
        /// The currently focused <see cref="IDockTabPane"/> in the first 
        /// <see cref="IDockAnchorPane"/>.
        /// </item>
        /// <item>
        /// The first <see cref="IDockTabPane"/> in the first <see cref="IDockAnchorPane"/>.
        /// </item>
        /// <item>
        /// The first <see cref="IDockAnchorPane"/>.
        /// </item>
        /// <item>
        /// The first <see cref="IDockTabPane"/>.
        /// </item>
        /// <item>
        /// The <see cref="IDockContainer.RootPane"/>.
        /// </item>
        /// </list>
        /// <para>
        /// Panes in a <see cref="IFloatWindow"/> and in auto-hide bars are ignored.
        /// </para>
        /// </remarks>
        protected virtual IDockPane OnGetDefaultDockTarget(IDockElement element)
        {
            var dockAnchorPane = First<IDockAnchorPane>(DockControl.RootPane);
            if (dockAnchorPane != null)
            {
                // First IDockTabPane in IDockAnchorPane.
                var pane = First<IDockTabPane>(dockAnchorPane);
                if (pane != null)
                    return pane;

                // First IDockAnchorPane.
                return dockAnchorPane;
            }
            else
            {
                // First IDockTabPane.
                var pane = First<IDockTabPane>(DockControl.RootPane);
                if (pane != null)
                    return pane;

                return DockControl.RootPane;
            }
        }


        /// <summary>
        /// Gets the first visible instance of the specified type in the docking layout.
        /// </summary>
        /// <typeparam name="T">The type of the <see cref="IDockElement"/></typeparam>
        /// <param name="dockPane">The <see cref="IDockPane"/> to examine.</param>
        /// <returns>
        /// The first instance of <typeparamref name="T"/> found in <paramref name="dockPane"/>.
        /// </returns>
        private static T First<T>(IDockPane dockPane) where T : class, IDockElement
        {
            if (dockPane == null || !dockPane.IsVisible)
                return null;

            T t = dockPane as T;
            if (t != null)
                return t;

            var dockAnchorPane = dockPane as IDockAnchorPane;
            if (dockAnchorPane != null)
                return First<T>(dockAnchorPane.ChildPane);

            var dockSplitPane = dockPane as IDockSplitPane;
            if (dockSplitPane != null)
            {
                foreach (var childPane in dockSplitPane.ChildPanes)
                {
                    var result = First<T>(childPane);
                    if (result != null)
                        return result;
                }
            }

            var dockTabPane = dockPane as IDockTabPane;
            if (dockTabPane != null && dockTabPane.Items.Count > 0)
                return dockTabPane.Items[0] as T;

            return null;
        }


        /// <summary>
        /// Gets the <see cref="IFloatWindow"/> that contains the specified element.
        /// </summary>
        /// <param name="element">The element to find.</param>
        /// <returns>The parent <see cref="IFloatWindow"/>.</returns>
        internal IFloatWindow GetFloatWindow(IDockElement element)
        {
            foreach (var floatWindow in DockControl.FloatWindows)
                if (floatWindow.Contains(element))
                    return floatWindow;

            return null;
        }


        /// <summary>
        /// Gets the auto-hide bar that contains the specified element or <see langword="null" /> if
        /// the element is not in any auto-hide bar.
        /// </summary>
        /// <param name="element">The element to find.</param>
        /// <returns>The <see cref="DockTabItemCollection"/>.</returns>
        private DockTabPaneCollection GetAutoHideBar(IDockElement element)
        {
            // Is element already in an auto-hide bar?
            if (DockControl.AutoHideLeft.Contains(element))
                return DockControl.AutoHideLeft;
            if (DockControl.AutoHideRight.Contains(element))
                return DockControl.AutoHideRight;
            if (DockControl.AutoHideTop.Contains(element))
                return DockControl.AutoHideTop;
            if (DockControl.AutoHideBottom.Contains(element))
                return DockControl.AutoHideBottom;

            return null;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IFloatWindow"/> is visible.
        /// </summary>
        /// <param name="floatWindow">The <see cref="IFloatWindow"/> to check.</param>
        /// <returns>
        /// <see langword="true"/> if the window contains any visible <see cref="IDockTabItem"/>s;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsVisible(IFloatWindow floatWindow)
        {
            // A IFloatWindow is visible if the root pane is visible.
            return IsVisible(floatWindow?.RootPane, DockState.Float);
        }


        /// <summary>
        /// Determines whether the specified element is visible at the current position in the
        /// docking layout.
        /// </summary>
        /// <param name="dockElement">The <see cref="IDockElement"/> to check.</param>
        /// <param name="dockState">The current state.</param>
        /// <returns>
        /// <see langword="true"/> if element is/contain a visible <see cref="IDockTabItem"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        internal static bool IsVisible(IDockElement dockElement, DockState dockState)
        {
            if (dockElement == null || dockState == DockState.Hide)
                return false;

            // An IDockAnchorPane is visible if it is "docked" of "floating".
            var dockAnchorPane = dockElement as IDockAnchorPane;
            if (dockAnchorPane != null)
                return dockState == DockState.Dock || dockState == DockState.Float;

            // An IDockSplitPane is visible if any child is visible.
            var dockSplitPane = dockElement as IDockSplitPane;
            if (dockSplitPane != null)
                return HasVisible(dockSplitPane.ChildPanes, dockState);

            // An IDockTabPane is visible if any child is visible.
            var dockTabPane = dockElement as IDockTabPane;
            if (dockTabPane != null)
                return HasVisible(dockTabPane.Items, dockState);

            // An IDockTabItem is visible if its dock state matches the parent state.
            var dockTabItem = dockElement as IDockTabItem;
            if (dockTabItem != null)
                return dockTabItem.DockState == dockState;

            return false;
        }


        private static bool HasVisible(IReadOnlyList<IDockElement> dockElements, DockState dockState)
        {
            if (dockElements == null)
                return false;

            for (int i = 0; i < dockElements.Count; i++)
                if (IsVisible(dockElements[i], dockState))
                    return true;

            return false;
        }
        #endregion


        #region ----- Docking operations -----

        /// <overloads>
        /// <summary>
        /// Determines whether the specified element can be moved to an auto-hide bar.
        /// </summary>
        /// </overloads>
        ///
        /// <summary>
        /// Determines whether the specified <see cref="IDockTabPane"/> can be moved to an auto-hide
        /// bar.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to auto-hide.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabPane"/> can be auto-hidden; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Auto-Hide button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public bool CanAutoHide(IDockTabPane dockTabPane)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            // Check the IDockTabItems that are currently visible in the IDockTabPane.
            var dockState = GetActualDockState(dockTabPane);
            foreach (var dockTabItem in dockTabPane.Items)
                if (dockTabItem.DockState == dockState && !CanAutoHide(dockTabItem))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IDockTabItem"/> can be moved to an auto-hide
        /// bar.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to auto-hide.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabItem"/> can be auto-hidden; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Auto-Hide button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public virtual bool CanAutoHide(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            return dockTabItem.DockState != DockState.AutoHide // Item is in a different dock state.
                   || GetAutoHideBar(dockTabItem) == null; // Or, inconsistent state: Item is
                                                           // not found in any auto-hide-bars.
        }


        /// <overloads>
        /// <summary>
        /// Moves the specified element into an auto-hide bar.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Moves the specified <see cref="IDockTabPane"/> into an auto-hide bar.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to auto-hide.</param>
        /// <remarks>
        /// If <paramref name="dockTabPane"/> is already in an auto-hide bar, calling this method
        /// has no effect.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public void AutoHide(IDockTabPane dockTabPane)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            var dockState = GetActualDockState(dockTabPane);
            if (dockState == DockState.AutoHide)
                return;

            try
            {
                Begin();

                if (dockState != DockState.Dock)
                {
                    // IDockTabPane is AutoHide, Float, or Hide:
                    // --> Auto-hide all IDockTabItems that already have a designated auto-hide bar.
                    foreach (var dockTabItem in dockTabPane.Items)
                        if (dockTabItem.DockState == dockState && GetAutoHideBar(dockTabItem) != null)
                            AutoHide(dockTabItem);
                }
                else
                {
                    // IDockTabPane is docked:
                    // Some of the IDockTabItems may already have an auto-hide position.
                    // --> Remove all IDockTabItems from their previous auto-hide bar. The
                    // IDockTabItems should be auto-hidden as one group.
                    foreach (var dockTabItem in dockTabPane.Items)
                    {
                        if (dockTabItem.DockState == dockState)
                        {
                            DockHelper.Remove(DockControl.AutoHideLeft, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideRight, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideTop, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideBottom, dockTabItem);
                        }
                    }
                }

                var newAutoHideBar = OnGetAutoHideBar(dockTabPane);
                if (newAutoHideBar == null)
                {
                    // Auto-hide not allowed? (Should not happen unless user forgets to override
                    // CanAutoHide.)
                    return;
                }

                // Move all remaining IDockTabItems into a single auto-hide pane.
                IDockTabPane newDockTabPane = null;
                foreach (var dockTabItem in dockTabPane.Items)
                {
                    if (dockTabItem.DockState == dockState)
                    {
                        if (newDockTabPane == null)
                            newDockTabPane = CreateDockTabPane(dockTabItem, DockState.AutoHide);
                        else
                            newDockTabPane.Items.Add(dockTabItem);

                        SetDockState(dockTabItem, DockState.AutoHide);
                    }
                }

                if (newDockTabPane != null)
                {
                    // Select previously active item in the new IDockTabPane.
                    SelectLastActive(newDockTabPane, DockState.AutoHide);

                    // Add IDockTabPane to auto-hide bar.
                    newAutoHideBar.Add(newDockTabPane);
                }

                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Moves the specified <see cref="IDockTabItem"/> into an auto-hide bar.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to auto-hide.</param>
        /// <remarks>
        /// If <paramref name="dockTabItem"/> is already in an auto-hide bar, calling this method
        /// has no effect.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public void AutoHide(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            try
            {
                Begin();

                // Assign item to auto-hide bar.
                var newAutoHideBar = OnGetAutoHideBar(dockTabItem);
                if (newAutoHideBar == null)
                    return; // Auto-hide not allowed.

                // Compare with already assigned auto-hide bar.
                var oldAutoHideBar = GetAutoHideBar(dockTabItem);
                if (oldAutoHideBar != newAutoHideBar)
                {
                    // Move into new auto-hide bar.
                    if (oldAutoHideBar != null)
                        DockHelper.Remove(oldAutoHideBar, dockTabItem);

                    var dockTabPane = CreateDockTabPane(dockTabItem, DockState.AutoHide);
                    newAutoHideBar.Add(dockTabPane);
                }

                SetDockState(dockTabItem, DockState.AutoHide);
                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Determines the auto-hide bar for the specified element.
        /// </summary>
        /// <param name="element">The element to auto-hide.</param>
        /// <returns>
        /// The auto-hide bar into which <paramref name="element"/> should be moved when hidden.
        /// </returns>
        /// <remarks>
        /// This method is called to determine in which auto-hide bar
        /// (<see cref="IDockControl.AutoHideLeft"/>, <see cref="IDockControl.AutoHideRight"/>,
        /// <see cref="IDockControl.AutoHideTop"/>, or <see cref="IDockControl.AutoHideBottom"/>)
        /// should appear when auto-hidden.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="element"/> is <see langword="null"/>.
        /// </exception>
        protected virtual DockTabPaneCollection OnGetAutoHideBar(IDockElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // Is element already in an auto-hide bar?
            var oldAutoHideBar = GetAutoHideBar(element);
            if (oldAutoHideBar != null)
                return oldAutoHideBar;

            var defaultTargetPane = OnGetDefaultDockTarget(element);

            // The auto-hide place is relative to the default dock target.
            var defaultAutoHideBar = DockControl.AutoHideBottom;
            if (defaultTargetPane == null)
                return defaultAutoHideBar;

            // Determine the dock position of the element relative to the target.
            var position = GetRelativePosition(DockControl, defaultTargetPane, element);
            switch (position)
            {
                case DockPosition.Left:
                    return DockControl.AutoHideLeft;
                case DockPosition.Right:
                    return DockControl.AutoHideRight;
                case DockPosition.Top:
                    return DockControl.AutoHideTop;
                case DockPosition.Bottom:
                    return DockControl.AutoHideBottom;
                default:
                    return defaultAutoHideBar;
            }
        }


        /// <summary>
        /// Gets the position of the specified <see cref="IDockElement"/> relative to a target.
        /// </summary>
        /// <param name="rootContainer">
        /// The root container which must contain <paramref name="target"/> and
        /// <paramref name="element"/>.
        /// </param>
        /// <param name="target">The target dock pane.</param>
        /// <param name="element">The dock element.</param>
        /// <returns>
        /// The position of <paramref name="element"/> relative to <paramref name="target"/>.
        /// <see cref="DockPosition.None"/> is returned if there is no clear relation between
        /// the two elements.
        /// </returns>
        private static DockPosition GetRelativePosition(IDockContainer rootContainer, IDockPane target, IDockElement element)
        {
            if (target.Contains(element))
                return DockPosition.Inside;

            // Get the first parent which contains both.
            var parentPane = DockHelper.GetParent(rootContainer.RootPane, element);
            while (parentPane != null)
            {
                if (parentPane.Contains(target))
                    break;

                parentPane = DockHelper.GetParent(rootContainer.RootPane, parentPane);
            }

            if (parentPane is IDockSplitPane)
            {
                // Check child panes and see if we find element or target first.
                var dockSplitPane = (IDockSplitPane)parentPane;
                var isHorizontal = dockSplitPane.Orientation == Orientation.Horizontal;
                foreach (var pane in dockSplitPane.ChildPanes)
                {
                    if (pane.Contains(target))
                        return isHorizontal ? DockPosition.Right : DockPosition.Bottom;
                    if (pane.Contains(element))
                        return isHorizontal ? DockPosition.Left : DockPosition.Top;
                }
            }

            return DockPosition.None;
        }


        /// <overloads>
        /// <summary>
        /// Determines whether the specified element can be docked in the
        /// <see cref="IDockControl"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether the specified <see cref="IDockTabPane"/> can be docked in the
        /// <see cref="IDockControl"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to dock.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabPane"/> can be docked; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Dock button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public bool CanDock(IDockTabPane dockTabPane)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            // Check the IDockTabItems that are currently visible in the IDockTabPane.
            var dockState = GetActualDockState(dockTabPane);
            foreach (var dockTabItem in dockTabPane.Items)
                if (dockTabItem.DockState == dockState && !CanDock(dockTabItem))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IDockTabItem"/> can be docked in the
        /// <see cref="IDockControl"/>.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to dock.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabItem"/> can be docked; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Dock button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public virtual bool CanDock(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            return dockTabItem.DockState != DockState.Dock // Item is in a different dock state.
                   || !DockControl.Contains(dockTabItem); // Or, inconsistent state: Item is docked
                                                          // but not found in the IDockControl.
        }


        /// <overloads>
        /// <summary>
        /// Docks the specified element.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Docks the specified <see cref="IDockTabPane"/> in the <see cref="IDockControl"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to dock.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public void Dock(IDockTabPane dockTabPane)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            var dockState = GetActualDockState(dockTabPane);
            if (dockState == DockState.Dock)
                return;

            try
            {
                Begin();

                // Dock all IDockTabItems that already have a designated dock position.
                foreach (var dockTabItem in dockTabPane.Items)
                    if (dockTabItem.DockState == dockState && DockControl.Contains(dockTabItem))
                        Dock(dockTabItem);

                // Move all remaining IDockTabItems into a single dock pane.
                IDockTabPane newDockTabPane = null;
                foreach (var dockTabItem in dockTabPane.Items)
                {
                    if (dockTabItem.DockState == dockState)
                    {
                        if (newDockTabPane == null)
                            newDockTabPane = CreateDockTabPane(dockTabItem, DockState.Dock);
                        else
                            newDockTabPane.Items.Add(dockTabItem);

                        SetDockState(dockTabItem, DockState.Dock);
                    }
                }

                if (newDockTabPane != null)
                {
                    // Dock the new IDockTabPane in the default dock target.
                    var target = OnGetDefaultDockTarget(dockTabPane);
                    if (target != null)
                        Dock(newDockTabPane, target, DockPosition.Inside);
                }
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Docks the specified <see cref="IDockTabItem"/> in the <see cref="IDockControl"/>.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to dock.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public void Dock(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            try
            {
                Begin();
                if (DockControl.Contains(dockTabItem))
                {
                    // Item has a designated dock position.
                    SetDockState(dockTabItem, DockState.Dock);
                    Cleanup();
                }
                else
                {
                    // Dock item in the default dock target.
                    var target = OnGetDefaultDockTarget(dockTabItem);
                    if (target != null)
                        Dock(dockTabItem, target, DockPosition.Inside);
                }
            }
            finally
            {
                End();
            }
        }


        /// <overloads>
        /// <summary>
        /// Determines whether the specified element can be docked relative to a certain target.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether the specified <see cref="IDockTabPane"/> can be docked relative
        /// certain target.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to dock.</param>
        /// <param name="target">The target pane.</param>
        /// <param name="position">The position relative to the target pane.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabPane"/> can be docked relative to
        /// <paramref name="target"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> or <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dockTabPane"/> is identical to <paramref name="target"/>.
        /// </exception>
        public bool CanDock(IDockTabPane dockTabPane, IDockPane target, DockPosition position)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (dockTabPane == target)
                throw new ArgumentException("dockTabPane must not be same as target.");

            // Check the IDockTabItems that are currently visible in the IDockTabPane.
            var dockState = GetActualDockState(dockTabPane);
            foreach (var dockTabItem in dockTabPane.Items)
                if (dockTabItem.DockState == dockState && !CanDock(dockTabItem, target, position))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IDockTabItem"/> can be docked relative to a
        /// certain target.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to dock.</param>
        /// <param name="target">The target pane.</param>
        /// <param name="position">The position relative to the target pane.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabItem"/> can be docked relative to
        /// <paramref name="target"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> or <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="target"/> is not a valid dock target.<br/>
        /// Or, <paramref name="position"/> is not a valid dock position.
        /// </exception>
        public virtual bool CanDock(IDockTabItem dockTabItem, IDockPane target, DockPosition position)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (position == DockPosition.None)
                throw new ArgumentException("'None' is not a valid dock position.", nameof(position));

            var targetDockState = GetActualDockState(target);
            if (targetDockState == DockState.Hide || targetDockState == DockState.AutoHide)
                throw new ArgumentException("The specified target dock pane is not a valid dock target because it is hidden.", nameof(target));

            return true;
        }


        /// <summary>
        /// Docks the specified <see cref="IDockTabPane"/> relative to a certain target.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to dock.</param>
        /// <param name="target">The target pane.</param>
        /// <param name="position">The position relative to the target pane.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> or <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="target"/> is not a valid dock target.<br/>
        /// Or, <paramref name="position"/> is not a valid dock position.
        /// </exception>
        public void Dock(IDockTabPane dockTabPane, IDockPane target, DockPosition position)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (dockTabPane == target)
                throw new ArgumentException("The specified pane must not be same as the target.");

            var targetDockState = GetActualDockState(target);
            if (targetDockState == DockState.Hide || targetDockState == DockState.AutoHide)
                throw new ArgumentException("The specified target dock pane is not a valid dock target because it is hidden.", nameof(target));
            if (position == DockPosition.None)
                throw new ArgumentException("'None' is not a valid dock position.", nameof(position));

            try
            {
                Begin();

                // Remove dockTabPane from docking layout.
                var originalDockState = GetActualDockState(dockTabPane);
                switch (originalDockState)
                {
                    case DockState.Dock:
                        DockHelper.Remove(DockControl, dockTabPane);
                        break;
                    case DockState.Float:
                        foreach (var floatWindow in DockControl.FloatWindows)
                            DockHelper.Remove(floatWindow, dockTabPane);
                        break;
                    case DockState.AutoHide:
                        DockHelper.Remove(DockControl.AutoHideLeft, dockTabPane);
                        DockHelper.Remove(DockControl.AutoHideRight, dockTabPane);
                        DockHelper.Remove(DockControl.AutoHideTop, dockTabPane);
                        DockHelper.Remove(DockControl.AutoHideBottom, dockTabPane);
                        break;
                }

                // Get target in all allowed variants.
                var targetAnchorPane = target as IDockAnchorPane;
                var targetSplitPane = target as IDockSplitPane;
                var targetTabPane = target as IDockTabPane;

                // Find IDockContainer.
                var rootContainer = (targetDockState == DockState.Dock) ? (IDockContainer)DockControl : GetFloatWindow(target);

                Debug.Assert(rootContainer != null);
                Debug.Assert(rootContainer.RootPane != null);

                // Remove the IDockTabItems which should not be dragged together with the
                // IDockTabPane because they are currently shown at a different location.
                for (int i = dockTabPane.Items.Count - 1; i >= 0; i--)
                {
                    var dockTabItem = dockTabPane.Items[i];
                    if (dockTabItem.DockState == DockState.Dock && DockControl.Contains(dockTabItem)
                        || dockTabItem.DockState == DockState.Float && GetFloatWindow(dockTabItem) != null
                        || dockTabItem.DockState == DockState.AutoHide && GetAutoHideBar(dockTabItem) != null)
                    {
                        // The IDockTabItem is currently visible at a different location.
                        // --> Drop the IDockTabItem from the dragged IDockTabPane.
                        dockTabPane.Items.RemoveAt(i);
                    }
                }

                // Remove the dragged IDockTabItems from any previous dock position.
                switch (targetDockState)
                {
                    case DockState.Dock:
                        foreach (var dockTabItem in dockTabPane.Items)
                        {
                            DockHelper.Remove(DockControl, dockTabItem);

                            // Also remove from previous auto-hide bars. The items need to be
                            // assigned to new auto-hide bars.
                            DockHelper.Remove(DockControl.AutoHideLeft, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideRight, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideTop, dockTabItem);
                            DockHelper.Remove(DockControl.AutoHideBottom, dockTabItem);
                        }
                        break;
                    case DockState.Float:
                        foreach (var floatWindow in DockControl.FloatWindows)
                            foreach (var dockTabItem in dockTabPane.Items)
                                DockHelper.Remove(floatWindow, dockTabItem);
                        break;
                }

                if (position == DockPosition.Inside)
                {
                    // ----- Dock inside target pane.
                    if (targetAnchorPane != null)
                    {
                        // Dock in IDockAnchorPane.
                        if (targetAnchorPane.ChildPane == null)
                        {
                            // Dock in empty IDockAnchorPane.
                            targetAnchorPane.ChildPane = dockTabPane;
                        }
                        else
                        {
                            // Dock in non-empty IDockAnchorPane.
                            Dock(dockTabPane, targetAnchorPane.ChildPane, DockPosition.Inside);
                            return; // Skip clean-up.
                        }
                    }
                    else if (targetSplitPane != null)
                    {
                        // Dock in IDockSplitPane.
                        targetSplitPane.ChildPanes.Add(dockTabPane);
                    }
                    else if (targetTabPane != null)
                    {
                        // Dock in IDockTabPane.
                        foreach (var dockTabItem in dockTabPane.Items)
                            targetTabPane.Items.Add(dockTabItem);
                    }
                }
                else
                {
                    // ----- Dock left/right/top/bottom of target pane.

                    // For a "left" and "right" docking we need a horizontal IDockSplitPane, otherwise a vertical.
                    Orientation orientation = (position == DockPosition.Left || position == DockPosition.Right) ? Orientation.Horizontal : Orientation.Vertical;

                    // For "left" and "top" docking we add the window before the given dock pane.
                    bool dockBefore = (position == DockPosition.Left || position == DockPosition.Top);

                    // Since we insert relative to the target. We need the parent of the target and
                    // this should be IDockSplitPane with the correct orientation.
                    var parentPane = DockHelper.GetParent(rootContainer.RootPane, target);
                    var parentAnchorPane = parentPane as IDockAnchorPane;
                    var parentSplitPane = parentPane as IDockSplitPane;

                    // Get the child index where we want to insert.
                    int index = 0;
                    if (targetSplitPane != null && targetSplitPane.Orientation == orientation)
                    {
                        // The target itself is a IDockSplitPane with the correct orientation. We insert
                        // at the beginning or end of the panel.
                        parentSplitPane = targetSplitPane;
                        if (!dockBefore)
                            index = targetSplitPane.ChildPanes.Count - 1;
                    }
                    else if (parentSplitPane == null || parentSplitPane.Orientation != orientation)
                    {
                        // The parent of the target is not a IDockSplitPane with the correct orientation.
                        // Create a new IDockSplitPane to insert as the new parent of the target.
                        var newDockSplitPane = CreateDockSplitPane(targetDockState, target.DockWidth, target.DockHeight, orientation);
                        newDockSplitPane.ChildPanes.Add(target);

                        if (parentAnchorPane != null)
                        {
                            Debug.Assert(parentAnchorPane.ChildPane == target);
                            parentAnchorPane.ChildPane = newDockSplitPane;
                        }
                        else if (parentSplitPane != null)
                        {
                            int targetIndex = parentSplitPane.ChildPanes.IndexOf(target);
                            Debug.Assert(index >= -1);
                            parentSplitPane.ChildPanes.RemoveAt(targetIndex);
                            parentSplitPane.ChildPanes.Insert(targetIndex, newDockSplitPane);
                        }
                        else
                        {
                            Debug.Assert(rootContainer.RootPane == target);
                            rootContainer.RootPane = newDockSplitPane;
                        }

                        parentSplitPane = newDockSplitPane;
                    }
                    else
                    {
                        Debug.Assert(parentSplitPane != null && parentSplitPane.Orientation == orientation);

                        // The parent of the target is a DockSplitPanel with the correct orientation.
                        index = parentSplitPane.ChildPanes.IndexOf(target);
                    }

                    Debug.Assert(parentSplitPane != null);

                    // Depending on the dock position we insert at or after the found index.
                    if (!dockBefore)
                        index++;

                    parentSplitPane.ChildPanes.Insert(index, dockTabPane);
                }

                foreach (var dockTabItem in dockTabPane.Items)
                    if (dockTabItem.DockState == originalDockState)
                        SetDockState(dockTabItem, targetDockState);

                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Docks the specified <see cref="IDockTabItem"/> relative to a certain target.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to dock.</param>
        /// <param name="target">The target pane.</param>
        /// <param name="position">The position relative to the target pane.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> or <paramref name="target"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="target"/> is not a valid dock target.<br/>
        /// Or, <paramref name="position"/> is not a valid dock position.
        /// </exception>
        public void Dock(IDockTabItem dockTabItem, IDockPane target, DockPosition position)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var targetDockState = GetActualDockState(target);
            if (targetDockState == DockState.Hide || targetDockState == DockState.AutoHide)
                throw new ArgumentException("The specified target dock pane is not a valid dock target because it is hidden.", nameof(target));
            if (position == DockPosition.None)
                throw new ArgumentException("'None' is not a valid dock position.", nameof(position));

            try
            {
                Begin();

                // Get target in all allowed variants.
                var targetAnchorPane = target as IDockAnchorPane;
                var targetSplitPane = target as IDockSplitPane;
                var targetTabPane = target as IDockTabPane;

                // Remove from previous dock position.
                if (targetDockState == DockState.Dock)
                {
                    DockHelper.Remove(DockControl, dockTabItem);
                }
                else if (targetDockState == DockState.Float)
                {
                    foreach (var floatWindow in DockControl.FloatWindows)
                        DockHelper.Remove(floatWindow, dockTabItem);
                }

                if (position == DockPosition.Inside)
                {
                    // ----- Dock inside target pane.
                    if (targetAnchorPane != null)
                    {
                        // Dock in IDockAnchorPane.
                        if (targetAnchorPane.ChildPane == null)
                        {
                            // Dock in empty IDockAnchorPane.
                            var dockTabPane = CreateDockTabPane(dockTabItem, targetDockState);
                            targetAnchorPane.ChildPane = dockTabPane;
                        }
                        else
                        {
                            // Dock in non-empty IDockAnchorPane.
                            Dock(dockTabItem, targetAnchorPane.ChildPane, DockPosition.Inside);
                            return; // Skip clean-up.
                        }
                    }
                    else if (targetSplitPane != null)
                    {
                        // Dock in IDockSplitPane.
                        var dockTabPane = CreateDockTabPane(dockTabItem, targetDockState);
                        targetSplitPane.ChildPanes.Add(dockTabPane);
                    }
                    else if (targetTabPane != null)
                    {
                        // Dock in IDockTabPane.
                        targetTabPane.Items.Add(dockTabItem);
                    }
                }
                else
                {
                    // Create a new IDockTabPane and call overload.
                    var dockTabPane = CreateDockTabPane(dockTabItem, targetDockState);
                    Dock(dockTabPane, target, position);
                    return; // Skip clean-up.
                }

                SetDockState(dockTabItem, targetDockState);
                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <overloads>
        /// <summary>
        /// Determines whether the specified element can be shown in an <see cref="IFloatWindow"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether the specified <see cref="IDockTabPane"/> can be shown in an
        /// <see cref="IFloatWindow"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to float.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabPane"/> can be floated; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Float button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public bool CanFloat(IDockTabPane dockTabPane)
        {
            // Check the IDockTabItems that are currently visible in the IDockTabPane.
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            var dockState = GetActualDockState(dockTabPane);
            foreach (var dockTabItem in dockTabPane.Items)
                if (dockTabItem.DockState == dockState && !CanFloat(dockTabItem))
                    return false;

            return true;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IDockTabItem"/> can be shown in an
        /// <see cref="IFloatWindow"/>.
        /// </summary>
        /// <param name="dockTabItem">The item to float.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabItem"/> can be floated; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// The result of this method determines whether the Float button is enabled.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public virtual bool CanFloat(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            return dockTabItem.DockState != DockState.Float // Item is in a different dock state.
                   || GetFloatWindow(dockTabItem) == null;  // Or, inconsistent state: Item is
                                                            // not found in any IFloatWindow.
        }


        /// <overloads>
        /// <summary>
        /// Shows the specified element in an <see cref="IFloatWindow"/>.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Shows the specified <see cref="IDockTabPane"/> in an <see cref="IFloatWindow"/>.
        /// </summary>
        /// <param name="dockTabPane">The <see cref="IDockTabPane"/> to float.</param>
        /// <remarks>
        /// If <paramref name="dockTabPane"/> is already in an <see cref="IFloatWindow"/>, calling
        /// this method has no effect.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabPane"/> is <see langword="null"/>.
        /// </exception>
        public void Float(IDockTabPane dockTabPane)
        {
            ThrowIfDockControlIsNull();
            if (dockTabPane == null)
                throw new ArgumentNullException(nameof(dockTabPane));

            try
            {
                Begin();

                var dockState = GetActualDockState(dockTabPane);
                if (dockState == DockState.Float)
                    return;

                // Float all IDockTabItems that already have a designated IFloatWindow.
                foreach (var dockTabItem in dockTabPane.Items)
                    if (dockTabItem.DockState == dockState && GetFloatWindow(dockTabItem) != null)
                        Float(dockTabItem);

                // Move all remaining IDockTabItems into a single IFloatWindow.
                IDockTabPane newDockTabPane = null;
                foreach (var dockTabItem in dockTabPane.Items)
                {
                    if (dockTabItem.DockState == dockState)
                    {
                        if (newDockTabPane == null)
                            newDockTabPane = CreateDockTabPane(dockTabItem, DockState.Float);
                        else
                            newDockTabPane.Items.Add(dockTabItem);

                        SetDockState(dockTabItem, DockState.Float);
                    }
                }

                if (newDockTabPane != null)
                {
                    // Select previously active item in the new IDockTabPane.
                    SelectLastActive(newDockTabPane, DockState.Float);

                    // Add new IFloatWindow.
                    var floatWindow = CreateFloatWindow();
                    floatWindow.RootPane = newDockTabPane;
                    DockControl.FloatWindows.Add(floatWindow);
                }

                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Shows the specified <see cref="IDockTabItem"/> in an <see cref="IFloatWindow"/>.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> to float.</param>
        /// <remarks>
        /// If <paramref name="dockTabItem"/> is already in an <see cref="IFloatWindow"/>, calling
        /// this method has no effect.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public void Float(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            try
            {
                Begin();

                // Check if IDockTabItem already has a designated IFloatWindow.
                var floatWindow = GetFloatWindow(dockTabItem);
                if (floatWindow == null)
                {
                    // Create a new IFloatWindow.
                    floatWindow = CreateFloatWindow();
                    floatWindow.RootPane = CreateDockTabPane(dockTabItem, DockState.Float);
                    DockControl.FloatWindows.Add(floatWindow);
                }

                SetDockState(dockTabItem, DockState.Float);
                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Shows the specified <see cref="IDockTabItem"/> and sets it as the
        /// <see cref="IDockControl.ActiveDockTabItem"/>.
        /// </summary>
        /// <param name="dockTabItem">The item to show.</param>
        /// <remarks>
        /// If the specified item is not already in the dock layout, it is shown at the last known
        /// position or docked in the default dock target (see <see cref="OnGetDefaultDockTarget"/>).
        /// </remarks>
        public void Show(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            try
            {
                Begin();

                bool isVisible = false;
                switch (dockTabItem.DockState)
                {
                    case DockState.Dock:
                        if (DockControl.Contains(dockTabItem))
                            isVisible = true;
                        break;
                    case DockState.Float:
                        if (GetFloatWindow(dockTabItem) != null)
                            isVisible = true;
                        break;
                    case DockState.AutoHide:
                        if (GetAutoHideBar(dockTabItem) != null)
                            isVisible = true;
                        break;
                }

                if (!isVisible)
                {
                    var dockState = dockTabItem.DockState;
                    if (dockState == DockState.Hide)
                        dockState = dockTabItem.LastDockState;
                    if (dockState == DockState.Hide)
                        dockState = DockState.Dock;

                    switch (dockState)
                    {
                        case DockState.Dock:
                            Dock(dockTabItem);
                            break;

                        case DockState.Float:
                            Float(dockTabItem);
                            break;

                        case DockState.AutoHide:
                            AutoHide(dockTabItem);
                            break;
                    }
                }

                Activate(dockTabItem);
                Cleanup();
            }
            finally
            {
                End();
            }
        }


        /// <overloads>
        /// <summary>
        /// Determines whether the specified element can be closed.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Determines whether the specified <see cref="IFloatWindow"/> can be closed.
        /// </summary>
        /// <param name="floatWindow">The <see cref="IFloatWindow"/> to close.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="floatWindow"/> can be closed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="floatWindow"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public virtual bool CanClose(IFloatWindow floatWindow)
        {
            ThrowIfDockControlIsNull();
            if (floatWindow == null)
                throw new ArgumentNullException(nameof(floatWindow));

            return floatWindow.RootPane == null || CanClose(floatWindow.RootPane, DockState.Float);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private bool CanClose(IDockPane dockPane, DockState dockState)
        {
            Debug.Assert(dockPane != null);

            if (dockPane is IDockAnchorPane)
            {
                var dockAnchorPane = (IDockAnchorPane)dockPane;
                return CanClose(dockAnchorPane.ChildPane, dockState);
            }
            else if (dockPane is IDockSplitPane)
            {
                var dockSplitPane = (IDockSplitPane)dockPane;
                foreach (var childPane in dockSplitPane.ChildPanes)
                    if (!CanClose(childPane, dockState))
                        return false;
            }
            else if (dockPane is IDockTabPane)
            {
                var dockTabPane = (IDockTabPane)dockPane;
                foreach (var dockTabItem in dockTabPane.Items)
                    if (dockTabItem.DockState == dockState && !CanClose(dockTabItem))
                        return false;
            }

            return true;
        }


        /// <summary>
        /// Determines whether the specified <see cref="IDockTabItem"/> can be closed.
        /// </summary>
        /// <param name="dockTabItem">The item to close.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="dockTabItem"/> can be closed; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public virtual bool CanClose(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            return dockTabItem.DockState != DockState.Hide;
        }


        /// <overloads>
        /// <summary>
        /// Closes the specified element.
        /// </summary>
        /// </overloads>
        /// 
        /// <summary>
        /// Closes the specified <see cref="IFloatWindow"/>.
        /// </summary>
        /// <param name="floatWindow">The <see cref="IFloatWindow"/> to close.</param>
        /// <remarks>
        /// <strong>Important:</strong> The caller is responsible for checking
        /// <see cref="CanClose(IFloatWindow)"/> before calling <see cref="Close(IFloatWindow)"/>!
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="floatWindow"/> is <see langword="null"/>.
        /// </exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public void Close(IFloatWindow floatWindow)
        {
            ThrowIfDockControlIsNull();
            if (floatWindow == null)
                throw new ArgumentNullException(nameof(floatWindow));

            try
            {
                Begin();

                if (floatWindow.RootPane != null)
                    Close(floatWindow.RootPane, DockState.Float);
                else
                    Cleanup();
            }
            finally
            {
                End();
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private void Close(IDockPane dockPane, DockState dockState)
        {
            ThrowIfDockControlIsNull();
            Debug.Assert(dockPane != null);

            try
            {
                Begin();

                if (dockPane is IDockAnchorPane)
                {
                    var dockAnchorPane = (IDockAnchorPane)dockPane;
                    Close(dockAnchorPane.ChildPane, dockState);
                }
                else if (dockPane is IDockSplitPane)
                {
                    var dockSplitPane = (IDockSplitPane)dockPane;
                    for (int i = dockSplitPane.ChildPanes.Count - 1; i >= 0; i--)
                    {
                        var childPane = dockSplitPane.ChildPanes[i];
                        Close(childPane, dockState);
                    }
                }
                else if (dockPane is IDockTabPane)
                {
                    var dockTabPane = (IDockTabPane)dockPane;
                    for (int i = dockTabPane.Items.Count - 1; i >= 0; i--)
                    {
                        var dockTabItem = dockTabPane.Items[i];
                        if (dockTabItem.DockState == dockState)
                            Close(dockTabItem);
                    }
                }
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Closes the specified <see cref="IDockTabItem"/>.
        /// </summary>
        /// <param name="dockTabItem">The item to close.</param>
        /// <remarks>
        /// <para>
        /// When a <see cref="IDockTabItem"/> is closed, the <see cref="IDockElement.DockState"/> is
        /// set to <see cref="DockState.Hide"/>.
        /// </para>
        /// <para>
        /// If <see cref="IDockTabItem.IsPersistent"/> is <see langword="true"/>, the
        /// <see cref="IDockTabItem"/> remains in the dock layout; otherwise, it is removed from the
        /// dock layout.
        /// </para>
        /// <para>
        /// <strong>Important:</strong> The caller is responsible for checking
        /// <see cref="CanClose(IDockTabItem)"/> before calling <see cref="Close(IDockTabItem)"/>!
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dockTabItem"/> is <see langword="null"/>.
        /// </exception>
        public void Close(IDockTabItem dockTabItem)
        {
            ThrowIfDockControlIsNull();
            if (dockTabItem == null)
                throw new ArgumentNullException(nameof(dockTabItem));

            try
            {
                Begin();

                if (!dockTabItem.IsPersistent)
                {
                    DockHelper.Remove(DockControl, dockTabItem);
                    foreach (var floatWindow in DockControl.FloatWindows)
                        DockHelper.Remove(floatWindow, dockTabItem);

                    DockHelper.Remove(DockControl.AutoHideLeft, dockTabItem);
                    DockHelper.Remove(DockControl.AutoHideRight, dockTabItem);
                    DockHelper.Remove(DockControl.AutoHideTop, dockTabItem);
                    DockHelper.Remove(DockControl.AutoHideBottom, dockTabItem);
                }

                SetDockState(dockTabItem, DockState.Hide);
                Cleanup();
                OnClose(dockTabItem);
            }
            finally
            {
                End();
            }
        }


        /// <summary>
        /// Called in <see cref="Close(IDockTabItem)"/> when the <see cref="IDockTabItem"/> was
        /// closed.
        /// </summary>
        /// <param name="dockTabItem">The <see cref="IDockTabItem"/> that was closed.</param>
        protected virtual void OnClose(IDockTabItem dockTabItem)
        {
        }
        #endregion
    }
}
