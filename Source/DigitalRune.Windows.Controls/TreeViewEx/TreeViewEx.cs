// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that displays hierarchical data in a tree structure that has items that
    /// can expand and collapse. Supports multiple selected items.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Only visible items can be selected, i.e. all parent nodes need to be expanded. An item in a
    /// collapsed subtree cannot be selected. Adding an item to the <see cref="SelectedItems"/>
    /// collection does not guarantee that the item is selected. To properly select an item:
    /// </para>
    /// <list type="number">
    /// <item>
    /// Expand all parent nodes, by setting the <see cref="TreeViewItemEx.IsExpanded"/> properties.
    /// </item>
    /// <item>
    /// Then set the <see cref="TreeViewItemEx.IsSelected"/> property.
    /// </item>
    /// </list>
    /// <para>
    /// When the <see cref="SelectedItems"/> collection and the
    /// <see cref="TreeViewItemEx.IsSelected"/> properties are inconsistent, the
    /// <see cref="TreeViewItemEx.IsSelected"/> properties take precedence.
    /// </para>
    /// <para>
    /// When the <see cref="SelectedItems"/> collection is created by the view model, the view model
    /// will usually receive the <see cref="INotifyCollectionChanged.CollectionChanged"/> event
    /// before the <see cref="TreeViewEx"/>. As a result the <see cref="TreeViewItemEx.IsSelected"/>
    /// properties may not be up-to-date. Therefore, either handle the
    /// <see cref="TreeViewItemEx.IsSelected"/> property change events in the view model, or handle
    /// the <see cref="SelectedItems"/> collection change event and ignore the
    /// <see cref="TreeViewItemEx.IsSelected"/> properties.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
    public class TreeViewEx : ItemsControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const string PART_ScrollViewer = nameof(PART_ScrollViewer);

        private Window _window;
        private ScrollViewer _scrollViewer;
        private TreeViewItemEx _shiftSelectionAnchor;
        private TreeViewItemEx _lastFocusedContainer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private static bool IsControlDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
        }


        private static bool IsShiftDown
        {
            get { return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="SelectedItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(IList),
            typeof(TreeViewEx),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsChanged));


        /// <summary>
        /// Gets or sets the collection of currently selected items.
        /// This is a dependency property.
        /// </summary>
        /// <value>The collection of currently selected items.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets the collection of currently selected items.")]
        [Category(Categories.Appearance)]
        [Bindable(true)]
        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectionChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
            "SelectionChanged",
            RoutingStrategy.Bubble,
            typeof(SelectionChangedEventHandler),
            typeof(TreeViewEx));

        /// <summary>
        /// Occurs when selection changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged
        {
            add { AddHandler(SelectionChangedEvent, value); }
            remove { RemoveHandler(SelectionChangedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Attached Dependency Properties
        //--------------------------------------------------------------

        private static readonly DependencyPropertyKey IsSelectionActivePropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsSelectionActive",
            typeof(bool),
            typeof(TreeViewEx),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the <see cref="P:DigitalRune.Windows.Controls.TreeViewEx.IsSelectionActive"/>
        /// read-only attached dependency property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// 
        /// <summary>
        /// Gets or sets the a value indicating whether the keyboard focus is within the
        /// <see cref="TreeViewEx"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the keyboard focus is within the <see cref="TreeViewEx"/>;
        /// otherwise, <see langword="false"/>.
        /// </value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the value of the <see cref="P:DigitalRune.Windows.Controls.TreeViewEx.IsSelectionActive"/>
        /// attached property from a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object from which to read the property value.</param>
        /// <returns>
        /// The value of the <see cref="P:DigitalRune.Windows.Controls.TreeViewEx.IsSelectionActive"/>
        /// attached property.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        public static bool GetIsSelectionActive(DependencyObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return (bool)obj.GetValue(IsSelectionActiveProperty);
        }


        /// <summary>
        /// Sets the value of the <see cref="P:DigitalRune.Windows.Controls.TreeViewEx.IsSelectionActive"/>
        /// attached property to a given <see cref="DependencyObject"/> object.
        /// </summary>
        /// <param name="obj">The object on which to set the property value.</param>
        /// <param name="value">The property value to set.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="obj"/> is <see langword="null"/>.
        /// </exception>
        private static void SetIsSelectionActive(DependencyObject obj, bool value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            obj.SetValue(IsSelectionActivePropertyKey, Boxed.Get(value));
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="TreeViewEx"/> class.
        /// </summary>
        static TreeViewEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(typeof(TreeViewEx)));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TreeViewEx), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewEx"/> class.
        /// </summary>
        public TreeViewEx()
        {
            SelectedItems = new ObservableCollection<object>();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="SelectedItems"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnSelectedItemsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeView = (TreeViewEx)dependencyObject;

            var observableCollection = eventArgs.OldValue as INotifyCollectionChanged;
            if (observableCollection != null)
                observableCollection.CollectionChanged -= treeView.OnSelectedItemsChanged;

            var selectedItems = (IList)eventArgs.NewValue;
            if (selectedItems == null)
                return;

            // Synchronize IsSelected of existing containers with SelectedItems collection.
            // (The SelectedItems collection can be null when the item containers are created.)
            var realizedSelectedItems = GetAllContainers(treeView, true).Where(container => container.IsSelected)
                                                                        .Select(container => container.Item)
                                                                        .Where(item => item != null)
                                                                        .ToList();

            for (int i = selectedItems.Count - 1; i >= 0; i--)
                if (!realizedSelectedItems.Contains(selectedItems[i]))
                    selectedItems.RemoveAt(i);

            for (int i = 0; i < realizedSelectedItems.Count; i++)
                if (!selectedItems.Contains(realizedSelectedItems[i]))
                    selectedItems.Add(realizedSelectedItems[i]);

            observableCollection = selectedItems as INotifyCollectionChanged;
            if (observableCollection != null)
                observableCollection.CollectionChanged += treeView.OnSelectedItemsChanged;
        }


        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            var selectedItems = SelectedItems;
            if (eventArgs.Action == NotifyCollectionChangedAction.Add
                || eventArgs.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in eventArgs.NewItems)
                {
                    var container = GetContainer(item);
                    if (container != null)
                        container.IsSelected = true;
                }
            }

            if (eventArgs.Action == NotifyCollectionChangedAction.Remove
                || eventArgs.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in eventArgs.OldItems)
                {
                    var container = GetContainer(item);
                    if (container != null)
                        container.IsSelected = false;
                }
            }

            if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
            {
                if (selectedItems != null)
                {
                    var set = new HashSet<object>(selectedItems.Cast<object>());
                    foreach (var container in GetAllContainers(this, false))
                        container.IsSelected = set.Contains(container.Item);
                }
                else
                {
                    foreach (var container in GetAllContainers(this, false))
                        container.IsSelected = false;
                }
            }

            OnSelectionChanged(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                eventArgs.OldItems ?? Array.Empty<object>(),
                eventArgs.NewItems ?? Array.Empty<object>()));
        }


        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="SelectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnSelectionChanged"/> in a
        /// derived class, be sure to call the base class's <see cref="OnSelectionChanged"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnSelectionChanged(SelectionChangedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == SelectionChangedEvent, "Invalid arguments for TreeViewItemEx.OnSelectionChanged.");
            RaiseEvent(eventArgs);
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
            return item is TreeViewItemEx;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewItemEx();
        }


        /// <summary>
        /// Called when the <see cref="ItemsControl.ItemsSource"/> property changes.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            HandleGotFocus(null);
            base.OnItemsSourceChanged(oldValue, newValue);
        }


        /// <summary>
        /// Invoked when the <see cref="ItemsControl.Items"/> property changes.
        /// </summary>
        /// <param name="e">Information about the change.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            HandleItemsChanged(this, e);
            base.OnItemsChanged(e);
        }


        internal void HandleItemsChanged(ItemsControl itemsControl, NotifyCollectionChangedEventArgs eventArgs)
        {
            var selectedItems = SelectedItems;
            if (selectedItems == null || selectedItems.Count == 0)
                return;

            // Remove invalid items from SelectedItems collection.
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    foreach (var oldItem in eventArgs.OldItems)
                    {
                        var container = itemsControl.ItemContainerGenerator.ContainerFromItem(oldItem) as TreeViewItemEx;
                        if (container != null)
                        {
                            foreach (var item in GetAllItems(container, false))
                            {
                                selectedItems.Remove(item);
                                if (selectedItems.Count == 0)
                                    break;
                            }
                        }
                        else
                        {
                            selectedItems.Remove(oldItem);
                        }

                        if (selectedItems.Count == 0)
                            break;
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    // The item containers may not yet be generated. In this case there is no way to
                    // synchronize the SelectedItems collection with the tree.
                    // --> Do not handle Reset in order to enable MVVM (where the view model
                    //     determines the SelectedItems/IsSelected state.
                    // 
                    // This solution creates a potential memory leak in non-MVVM scenarios: An item
                    // in a subtree is selected. The subtree is replaced manually causing a Reset.
                    // --> The selected item remains in the SelectedItems collection until another
                    //     item is selected.
                    break;
            }
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _scrollViewer = null;
            base.OnApplyTemplate();
            _scrollViewer = GetTemplateChild(PART_ScrollViewer) as ScrollViewer;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            _window = Window.GetWindow(this);
            if (_window != null)
                _window.GotKeyboardFocus += OnWindowGotKeyboardFocus;
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            if (_window != null)
            {
                _window.GotKeyboardFocus -= OnWindowGotKeyboardFocus;
                _window = null;
            }
        }


        private void OnWindowGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            // IsSelectionActive is set if the keyboard focus is within another focus scope (e.g.
            // menu or toolbar). When the keyboard focus leaves this focus scope, we may need to
            // reset IsSelectionActive.
            if (!GetIsSelectionActive(this))
                return;

            var focusedElement = eventArgs.NewFocus as DependencyObject;
            if (focusedElement == null
                || (!IsKeyboardFocusWithin && FocusManager.GetFocusScope(focusedElement) == FocusManager.GetFocusScope(this)))
            {
                // Keyboard focus is on another control within the same focus scope.
                SetIsSelectionActive(this, false);
            }
        }


        /// <summary>
        /// Invoked whenever an unhandled <see cref="UIElement.GotFocus"/> event reaches this
        /// element in its route.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);

            // Focus last focused item or first selected item.
            if (_lastFocusedContainer == null)
            {
                var selectedItems = SelectedItems;
                if (selectedItems != null && selectedItems.Count > 0)
                    HandleGotFocus(GetContainer(selectedItems[0]));
            }

            if (_lastFocusedContainer != null)
                _lastFocusedContainer.FocusWithRetry();
            else
                GetFirstContainer(this, true)?.FocusWithRetry();
        }


        /// <summary>
        /// Invoked just before the <see cref="UIElement.IsKeyboardFocusWithinChanged"/> event is
        /// raised by this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // Update IsSelectionActive attached dependency property.
            bool isSelectionActive = false;
            bool isKeyboardFocusWithin = IsKeyboardFocusWithin;
            if (isKeyboardFocusWithin)
            {
                isSelectionActive = true;
            }
            else
            {
                var focusedElement = Keyboard.FocusedElement as DependencyObject;
                if (focusedElement != null)
                {
                    var root = this.GetVisualRoot() as UIElement;
                    if (root != null
                        && root.IsKeyboardFocusWithin
                        && FocusManager.GetFocusScope(focusedElement) != root)
                    {
                        // Keyboard focus is within menu or toolbar.
                        isSelectionActive = true;
                    }
                }
            }

            SetIsSelectionActive(this, isSelectionActive);
        }


        /// <summary>
        /// Raises the <see cref="UIElement.KeyDown"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnKeyDown(e);

            if (e.Handled)
                return;

            HandleKeyDown(null, e);
        }


        internal void HandleKeyDown(TreeViewItemEx container, KeyEventArgs e)
        {
            // TODO: Consider FlowDirection for Left/Right.
            switch (e.Key)
            {
                case Key.Home:
                    OnNavigationKey(GetFirstContainer(this, true));
                    e.Handled = true;
                    break;

                case Key.End:
                    OnNavigationKey(GetLastContainer(this, true));
                    e.Handled = true;
                    break;

                case Key.Up:
                    OnNavigationKey(GetPreviousContainer(container ?? GetFocusedContainer(), true));
                    e.Handled = true;
                    break;

                case Key.Down:
                    OnNavigationKey(GetNextContainer(container ?? GetFocusedContainer(), true));
                    e.Handled = true;
                    break;

                case Key.PageUp:
                    OnNavigationKey(GetPageUpDownContainer(container ?? GetFocusedContainer(), false));
                    e.Handled = true;
                    break;

                case Key.PageDown:
                    OnNavigationKey(GetPageUpDownContainer(container ?? GetFocusedContainer(), true));
                    e.Handled = true;
                    break;

                case Key.Left:
                    OnNavigationKey((container ?? GetFocusedContainer())?.ParentTreeViewItem);
                    e.Handled = true;
                    break;

                case Key.A:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                    {
                        SelectAll();
                        e.Handled = true;
                    }
                    break;

                case Key.Space:
                    MoveSelection(container ?? GetFocusedContainer());
                    e.Handled = true;
                    break;
            }
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseDown"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Mouse event was not handled by TreeViewItemEx.
            Focus();
        }


        /// <summary>
        /// Selects all visible items in the <see cref="TreeViewEx"/>.
        /// (Collapsed items are not selected.)
        /// </summary>
        public void SelectAll()
        {
            var selectedItems = SelectedItems;
            if (selectedItems != null)
            {
                var items = GetAllItems(this, true).ToArray();
                foreach (var item in items)
                {
                    if (!selectedItems.Contains(item))
                        selectedItems.Add(item);
                }
            }
        }


        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void UnselectAll()
        {
            SelectedItems?.Clear();
        }


        internal void MoveSelection(TreeViewItemEx container)
        {
            Debug.Assert(container != null);

            var selectedItems = SelectedItems;
            if (selectedItems != null)
            {
                var item = container.Item;
                if (selectedItems.Count == 1 && selectedItems[0] == item)
                {
                    // Focused moved back to current and only selected item.
                    _shiftSelectionAnchor = container;
                }
                else if (IsControlDown)
                {
                    // Ctrl-click item toggles selection.
                    if (selectedItems.Contains(item))
                        selectedItems.Remove(item);
                    else
                        selectedItems.Add(item);

                    _shiftSelectionAnchor = container;
                }
                else if (IsShiftDown && selectedItems.Count > 0)
                {
                    // Shift-click extends selection.
                    var anchorContainer = _shiftSelectionAnchor ?? GetContainer(selectedItems[0]);
                    var rangeItems = GetRangeItems(this, anchorContainer, container, true);

                    for (int i = selectedItems.Count - 1; i >= 0; i--)
                        if (!rangeItems.Contains(selectedItems[i]))
                            selectedItems.RemoveAt(i);

                    for (int i = 0; i < rangeItems.Count; i++)
                        if (!selectedItems.Contains(rangeItems[i]))
                            selectedItems.Add(rangeItems[i]);
                }
                else
                {
                    // Single selection.
                    for (int i = selectedItems.Count - 1; i >= 0; i--)
                        if (selectedItems[i] != item)
                            selectedItems.RemoveAt(i);

                    if (!selectedItems.Contains(item))
                        selectedItems.Add(item);

                    _shiftSelectionAnchor = container;
                }
            }

            container.FocusWithRetry();
            container.BringIntoView();
        }


        private void OnNavigationKey(TreeViewItemEx targetContainer)
        {
            if (targetContainer == null)
                return;

            if (IsControlDown)
            {
                targetContainer.FocusWithRetry();
                targetContainer.BringIntoView();
            }
            else
            {
                MoveSelection(targetContainer);
            }
        }


        internal void HandleCollapse(TreeViewItemEx container)
        {
            Debug.Assert(container != null);

            // Unselect all collapsed items.
            bool childFocused = false;
            bool childSelected = false;
            foreach (var childContainer in GetAllContainers(container, true))
            {
                if (childContainer.IsKeyboardFocusWithin)
                {
                    childFocused = true;
                }

                if (childContainer.IsSelected)
                {
                    childSelected = true;
                    childContainer.IsSelected = false;
                }
            }

            if (childSelected)
            {
                MoveSelection(container);
            }
            else if (childFocused)
            {
                container.FocusWithRetry();
            }
        }


        internal void HandleGotFocus(TreeViewItemEx container)
        {
            if (container != null)
            {
                // Initial selection.
                var selectedItems = SelectedItems;
                if (selectedItems != null && selectedItems.Count == 0)
                    MoveSelection(container);
            }

            if (_lastFocusedContainer == container)
                return;

            if (_lastFocusedContainer != null)
                _lastFocusedContainer.IsTabStop = false;

            _lastFocusedContainer = container;

            if (_lastFocusedContainer != null)
                _lastFocusedContainer.IsTabStop = true;

            IsTabStop = (_lastFocusedContainer == null);
        }


        #region ----- Helper methods -----

        private TreeViewItemEx GetContainer(object item)
        {
            if (item == null)
                return null;

            // Is item its own container?
            var treeViewItem = item as TreeViewItemEx;
            if (treeViewItem != null)
                return treeViewItem;

            // Try last focused container.
            if (_lastFocusedContainer?.Item == item)
                return _lastFocusedContainer;

            // Linear search.
            return GetAllContainers(this, true).FirstOrDefault(container => container.Item == item);
        }


        private TreeViewItemEx GetFocusedContainer()
        {
            if (_lastFocusedContainer != null)
            {
                Debug.Assert(_lastFocusedContainer.IsFocused, "Item container expected to have logical focus.");
                return _lastFocusedContainer;
            }

            return GetAllContainers(this, true).FirstOrDefault(container => container.IsFocused);
        }


        private static IEnumerable<TreeViewItemEx> GetAllContainers(ItemsControl itemsControl, bool onlyVisible)
        {
            Debug.Assert(itemsControl != null);

            var generator = itemsControl.ItemContainerGenerator;
            if (generator.Status != GeneratorStatus.ContainersGenerated)
                yield break;

            int numberOfItems = itemsControl.Items.Count;
            for (int i = 0; i < numberOfItems; i++)
            {
                var treeViewItem = generator.ContainerFromIndex(i) as TreeViewItemEx;
                if (treeViewItem != null)
                {
                    yield return treeViewItem;

                    if (treeViewItem.HasItems && (!onlyVisible || treeViewItem.IsExpanded))
                    {
                        foreach (var container in GetAllContainers(treeViewItem, onlyVisible))
                            yield return container;
                    }
                }
            }
        }


        private static IEnumerable<TreeViewItemEx> GetAllContainersReverse(ItemsControl itemsControl, bool onlyVisible)
        {
            Debug.Assert(itemsControl != null);

            var generator = itemsControl.ItemContainerGenerator;
            if (generator.Status != GeneratorStatus.ContainersGenerated)
                yield break;

            int numberOfItems = itemsControl.Items.Count;
            for (int i = numberOfItems - 1; i >= 0; i--)
            {
                var treeViewItem = generator.ContainerFromIndex(i) as TreeViewItemEx;
                if (treeViewItem != null)
                {
                    if (treeViewItem.HasItems && (!onlyVisible || treeViewItem.IsExpanded))
                    {
                        foreach (var container in GetAllContainersReverse(treeViewItem, onlyVisible))
                            yield return container;
                    }

                    yield return treeViewItem;
                }
            }
        }


        private static IEnumerable<object> GetAllItems(ItemsControl itemsControl, bool onlyVisible)
        {
            return GetAllContainers(itemsControl, onlyVisible).Select(container => container.Item);
        }


        private static TreeViewItemEx GetFirstContainer(ItemsControl itemsControl, bool onlyVisible)
        {
            return GetAllContainers(itemsControl, onlyVisible).FirstOrDefault();
        }


        private static TreeViewItemEx GetLastContainer(ItemsControl itemsControl, bool onlyVisible)
        {
            return GetAllContainersReverse(itemsControl, onlyVisible).FirstOrDefault();
        }


        private static TreeViewItemEx GetPreviousSibling(TreeViewItemEx current)
        {
            Debug.Assert(current != null);

            var itemsControl = current.ParentItemsControl;
            Debug.Assert(itemsControl != null, "Item container expected to be part of TreeViewEx.");

            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(current);
            Debug.Assert(0 <= index && index < itemsControl.Items.Count, "Invalid index.");

            int previousIndex = index - 1;
            return (previousIndex >= 0)
                   ? itemsControl.ItemContainerGenerator.ContainerFromIndex(previousIndex) as TreeViewItemEx
                   : null;
        }


        private static TreeViewItemEx GetNextSibling(TreeViewItemEx current)
        {
            Debug.Assert(current != null);

            var itemsControl = current.ParentItemsControl;
            Debug.Assert(itemsControl != null, "Item container expected to be part of TreeViewEx.");

            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(current);
            Debug.Assert(0 <= index && index < itemsControl.Items.Count, "Invalid index.");

            int nextIndex = index + 1;
            return (nextIndex < itemsControl.Items.Count)
                   ? itemsControl.ItemContainerGenerator.ContainerFromIndex(nextIndex) as TreeViewItemEx
                   : null;
        }


        private static TreeViewItemEx GetPreviousContainer(TreeViewItemEx current, bool onlyVisible)
        {
            if (current == null)
                return null;

            var sibling = GetPreviousSibling(current);
            if (sibling != null)
            {
                if (sibling.HasItems && (!onlyVisible || sibling.IsExpanded))
                    return GetLastContainer(sibling, onlyVisible);

                return sibling;
            }

            return current.ParentTreeViewItem;
        }


        private static TreeViewItemEx GetNextContainer(TreeViewItemEx current, bool onlyVisible)
        {
            if (current == null)
                return null;

            if (current.HasItems && (!onlyVisible || current.IsExpanded))
                return current.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItemEx;

            do
            {
                var sibling = GetNextSibling(current);
                if (sibling != null)
                    return sibling;

                current = current.ParentTreeViewItem;
            } while (current != null);

            return null;
        }


        private static List<TreeViewItemEx> GetRangeContainers(ItemsControl itemsControl, TreeViewItemEx first, TreeViewItemEx last, bool onlyVisible)
        {
            Debug.Assert(first != null);
            Debug.Assert(last != null);

            var list = new List<TreeViewItemEx>();
            if (first == last)
            {
                list.Add(first);
                return list;
            }

            using (var enumerator = GetAllContainers(itemsControl, onlyVisible).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current == first || enumerator.Current == last)
                    {
                        list.Add(enumerator.Current);
                        break;
                    }
                }

                while (enumerator.MoveNext())
                {
                    list.Add(enumerator.Current);
                    if (enumerator.Current == first || enumerator.Current == last)
                        break;
                }
            }

            Debug.Assert(list.Count > 0);

            if (list[0] != first)
                list.Reverse();

            Debug.Assert(list[0] == first);
            Debug.Assert(list[list.Count - 1] == last);

            return list;
        }


        private static List<object> GetRangeItems(ItemsControl itemsControl, TreeViewItemEx first, TreeViewItemEx last, bool onlyVisible)
        {
            var containers = GetRangeContainers(itemsControl, first, last, onlyVisible);
            return containers.Select(c => c.Item).ToList();
        }


        private TreeViewItemEx GetPageUpDownContainer(TreeViewItemEx container, bool down)
        {
            if (container == null)
                return down ? GetLastContainer(this, true) : GetFirstContainer(this, true);

            double yThreshold = container.TransformToAncestor(this).Transform(new Point()).Y;
            double itemHeight = container.HeaderHeight;
            double viewportHeight = _scrollViewer?.ViewportHeight ?? ActualHeight;
            double offset = viewportHeight - 2 * itemHeight;

            if (down)
            {
                yThreshold += offset;
                var nextContainer = GetNextContainer(container, true);
                while (nextContainer != null)
                {
                    container = nextContainer;
                    double y = container.TransformToAncestor(this).Transform(new Point()).Y;
                    if (y > yThreshold)
                        break;

                    nextContainer = GetNextContainer(nextContainer, true);
                }
            }
            else
            {
                yThreshold -= offset;
                var previousContainer = GetPreviousContainer(container, true);
                while (previousContainer != null)
                {
                    container = previousContainer;
                    double y = container.TransformToAncestor(this).Transform(new Point()).Y;
                    if (y < yThreshold)
                        break;

                    previousContainer = GetPreviousContainer(previousContainer, true);
                }
            }

            return container;
        }
        #endregion

        #endregion
    }
}
