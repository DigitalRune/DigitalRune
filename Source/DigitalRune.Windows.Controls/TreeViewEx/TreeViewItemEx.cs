// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Implements a selectable item in a <see cref="TreeViewEx"/> control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting <see cref="IsSelected"/> automatically adds the item to the
    /// <see cref="TreeViewEx.SelectedItems"/> of the parent <see cref="TreeViewEx"/>. Setting
    /// <see cref="IsSelected"/> does not affect the keyboard focus.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    [TemplatePart(Name = PART_Header, Type = typeof(FrameworkElement))]
    public class TreeViewItemEx : HeaderedItemsControl
    {
        // TODO: Implement IHierarchicalVirtualizationAndScrollInfo to support virtualization.
        // TODO: Add TreeViewItem.Value dependency property.
        // -> When pressing a key, jump to the first TreeViewItem with a Value starting with the key.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const string PART_Header = nameof(PART_Header);
        private FrameworkElement _header;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the parent <see cref="ItemsControl"/>.
        /// </summary>
        /// <value>The parent <see cref="ItemsControl"/>.</value>
        internal ItemsControl ParentItemsControl
        {
            get { return ItemsControlFromItemContainer(this); }
        }


        /// <summary>
        /// Gets the parent <see cref="TreeViewItemEx"/>.
        /// </summary>
        /// <value>The parent <see cref="TreeViewItemEx"/>.</value>
        internal TreeViewItemEx ParentTreeViewItem
        {
            get { return ParentItemsControl as TreeViewItemEx; }
        }


        /// <summary>
        /// Gets the parent <see cref="TreeViewEx"/>.
        /// </summary>
        /// <value>The parent <see cref="TreeViewEx"/>.</value>
        internal TreeViewEx ParentTreeView
        {
            get
            {
                var parent = ParentItemsControl;
                while (parent != null)
                {
                    var treeView = parent as TreeViewEx;
                    if (treeView != null)
                        return treeView;

                    parent = ItemsControlFromItemContainer(parent);
                }

                return null;
            }
        }


        /// <summary>
        /// Gets the height of the header host element.
        /// </summary>
        /// <value>The height of the header host element.</value>
        internal double HeaderHeight
        {
            get { return _header?.ActualHeight ?? 0; }
        }


        /// <summary>
        /// Gets the item held by this item container.
        /// </summary>
        /// <value>The item held by this item container.</value>
        internal object Item
        {
            get { return DataContext ?? this; }
        }


        /// <summary>
        /// Gets a value indicating whether this tree node can be expanded/collapsed.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this tree node can be expanded/collapsed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        private bool CanExpand
        {
            get { return HasItems; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        private static readonly DependencyPropertyKey IndentationLevelPropertyKey = DependencyProperty.RegisterReadOnly(
            "IndentationLevel",
            typeof(int),
            typeof(TreeViewItemEx),
            new FrameworkPropertyMetadata(Boxed.Int32Zero, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Identifies the <see cref="IndentationLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IndentationLevelProperty = IndentationLevelPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the indentation level of the current item.
        /// This is a dependency property.
        /// </summary>
        /// <value>The indentation level of the current item.</value>
        [Browsable(false)]
        public int IndentationLevel
        {
            get { return (int)GetValue(IndentationLevelProperty); }
            //private set { SetValue(IndentationLevelPropertyKey, value); }
        }


        #region ----- Expand/Collapse -----

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            "IsExpanded",
            typeof(bool),
            typeof(TreeViewItemEx),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExpandedChanged));

        /// <summary>
        /// Gets or sets a value indicating the <see cref="TreeViewItem"/> is expanded and showing
        /// it children. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the a value indicating the tree view item is expanded;
        /// otherwise, <see langword="false"/>. The default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets the a value indicating the item is expanded and showing it children.")]
        [Category(Categories.Layout)]
        [Bindable(true)]
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Expanded"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ExpandedEvent = EventManager.RegisterRoutedEvent(
            "Expanded",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TreeViewItemEx));

        /// <summary>
        /// Occurs when the <see cref="TreeViewItemEx"/> is expanded.
        /// </summary>
        public event RoutedEventHandler Expanded
        {
            add { AddHandler(ExpandedEvent, value); }
            remove { RemoveHandler(ExpandedEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Collapsed"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CollapsedEvent = EventManager.RegisterRoutedEvent(
            "Collapsed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TreeViewItemEx));

        /// <summary>
        /// Occurs when the item is collapsed.
        /// </summary>
        public event RoutedEventHandler Collapsed
        {
            add { AddHandler(CollapsedEvent, value); }
            remove { RemoveHandler(CollapsedEvent, value); }
        }
        #endregion


        #region ----- Selection -----

        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(TreeViewItemEx),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsSelectedChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="TreeViewItemEx"/> is
        /// selected in the <see cref="TreeViewEx"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="TreeViewItemEx"/> is selected in the current
        /// <see cref="TreeViewEx"/>; otherwise, <see langword="false"/>. The default value is
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the current TreeViewItemEx is selected in the TreeViewEx.")]
        [Category(Categories.Appearance)]
        [Bindable(true)]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Selected"/> routed event.
        /// </summary>
        public static readonly RoutedEvent SelectedEvent = EventManager.RegisterRoutedEvent(
            "Selected",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TreeViewItemEx));

        /// <summary>
        /// Occurs when when the <see cref="TreeViewItemEx"/> gets selected in the 
        /// <see cref="TreeViewEx"/>.
        /// </summary>
        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Unselected"/> routed event.
        /// </summary>
        public static readonly RoutedEvent UnselectedEvent = EventManager.RegisterRoutedEvent(
            "Unselected",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TreeViewItemEx));

        /// <summary>
        /// Occurs when the <see cref="TreeViewItemEx"/> gets unselected in the
        /// <see cref="TreeViewEx"/>.
        /// </summary>
        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value); }
            remove { RemoveHandler(UnselectedEvent, value); }
        }
        #endregion

        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="TreeViewItemEx"/> class.
        /// </summary>
        static TreeViewItemEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeViewItemEx), new FrameworkPropertyMetadata(typeof(TreeViewItemEx)));
            VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeof(TreeViewItemEx), new FrameworkPropertyMetadata(Boxed.BooleanFalse));
            IsTabStopProperty.OverrideMetadata(typeof(TreeViewItemEx), new FrameworkPropertyMetadata(Boxed.BooleanFalse));

            // Override BringIntoView behavior.
            EventManager.RegisterClassHandler(typeof(TreeViewItemEx), RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewItemEx"/> class.
        /// </summary>
        public TreeViewItemEx()
        {
            DataContextChanged += OnDataContextChanged;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="FrameworkElement.DataContext"/> property changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeViewItem = (TreeViewItemEx)sender;
            var selectedItems = treeViewItem.ParentTreeView?.SelectedItems;
            if (selectedItems == null || !treeViewItem.IsSelected)
                return;

            object oldItem = eventArgs.OldValue ?? treeViewItem;
            selectedItems.Remove(oldItem);

            object newItem = eventArgs.NewValue ?? treeViewItem;
            if (!selectedItems.Contains(newItem))
                selectedItems.Add(newItem);
        }


        /// <summary>
        /// Called when the <see cref="IsExpanded"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsExpandedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeViewItem = (TreeViewItemEx)dependencyObject;
            bool isExpanded = (bool)eventArgs.NewValue;

            if (!isExpanded)
                treeViewItem.ParentTreeView?.HandleCollapse(treeViewItem);

            if (isExpanded)
                treeViewItem.OnExpanded(new RoutedEventArgs(ExpandedEvent, treeViewItem));
            else
                treeViewItem.OnCollapsed(new RoutedEventArgs(CollapsedEvent, treeViewItem));
        }


        /// <summary>
        /// Called when the <see cref="IsSelected"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsSelectedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var treeViewItem = (TreeViewItemEx)dependencyObject;
            bool isSelected = (bool)eventArgs.NewValue;

            if (isSelected)
                treeViewItem.ExpandAncestors();

            // Update TreeViewEx.SelectedItems.
            var selectedItems = treeViewItem.ParentTreeView?.SelectedItems;
            if (selectedItems != null)
            {
                var item = treeViewItem.Item;
                if (isSelected)
                {
                    if (!selectedItems.Contains(item))
                        selectedItems.Add(item);
                }
                else
                {
                    selectedItems.Remove(item);
                }
            }

            if (isSelected)
                treeViewItem.OnSelected(new RoutedEventArgs(SelectedEvent, treeViewItem));
            else
                treeViewItem.OnUnselected(new RoutedEventArgs(UnselectedEvent, treeViewItem));
        }


        /// <summary>
        /// Raises the <see cref="Expanded"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnExpanded"/> in a
        /// derived class, be sure to call the base class's <see cref="OnExpanded"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnExpanded(RoutedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == ExpandedEvent, "Invalid arguments for TreeViewItemEx.OnExpanded.");
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="Collapsed"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnCollapsed"/> in a
        /// derived class, be sure to call the base class's <see cref="OnCollapsed"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnCollapsed(RoutedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == CollapsedEvent, "Invalid arguments for TreeViewItemEx.OnCollapsed.");
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="Selected"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnSelected"/> in a
        /// derived class, be sure to call the base class's <see cref="OnSelected"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        protected virtual void OnSelected(RoutedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == SelectedEvent, "Invalid arguments for TreeViewItemEx.OnSelected.");
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="Unselected"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnUnselected"/> in a
        /// derived class, be sure to call the base class's <see cref="OnUnselected"/> method so
        /// that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnUnselected(RoutedEventArgs eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == UnselectedEvent, "Invalid arguments for TreeViewItemEx.OnUnselected.");
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
        /// Sets the styles, templates, and bindings for a <see cref="TreeViewItemEx"/>.
        /// </summary>
        /// <param name="element">
        /// An object that is a <see cref="TreeViewItemEx"/> or that can be converted into one.
        /// </param>
        /// <param name="item">The object to use to create the <see cref="TreeViewItemEx"/>.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            base.PrepareContainerForItemOverride(element, item);
            element.SetValue(IndentationLevelPropertyKey, IndentationLevel + 1);
        }


        /// <summary>
        /// When overridden in a derived class, undoes the effects of the
        /// <see cref="PrepareContainerForItemOverride"/> method.
        /// </summary>
        /// <param name="element">The The container element.</param>
        /// <param name="item">The item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            base.ClearContainerForItemOverride(element, item);
            element.ClearValue(IndentationLevelPropertyKey);
        }


        /// <summary>
        /// Invoked when the <see cref="ItemsControl.Items"/> property changes.
        /// </summary>
        /// <param name="e">Information about the change.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            ParentTreeView?.HandleItemsChanged(this, e);
        }


        /// <summary>
        /// Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked
        /// whenever <see cref="FrameworkElement.IsInitialized"/> is set to <see langword="true"/>
        /// internally.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var selectedItems = ParentTreeView?.SelectedItems;
            if (selectedItems != null && selectedItems.Contains(Item))
                IsSelected = true;
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _header = null;
            base.OnApplyTemplate();
            _header = GetTemplateChild(PART_Header) as FrameworkElement;
        }


        private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (e.TargetObject == sender)
                ((TreeViewItemEx)sender).BringIntoViewOverride(e);
        }


        private void BringIntoViewOverride(RequestBringIntoViewEventArgs e)
        {
            ExpandAncestors();

            // Only bring header into view. Ignore children.
            if (e.TargetRect.IsEmpty)
            {
                if (_header != null)
                {
                    e.Handled = true;
                    _header.BringIntoView();
                }
                else
                {
                    // Retry when loaded.
                    Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => _header?.BringIntoView()));
                }
            }
        }


        private void ExpandAncestors()
        {
            var parent = ParentTreeViewItem;
            while (parent != null)
            {
                parent.IsExpanded = true;
                parent = parent.ParentTreeViewItem;
            }
        }


        internal void FocusWithRetry()
        {
            if (!Focus())
            {
                // Delayed retry:
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => Focus()));
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
            ParentTreeView?.HandleGotFocus(this);
            base.OnGotFocus(e);
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

            // TODO: Consider FlowDirection for Left/Right.
            switch (e.Key)
            {
                case Key.Left:
                case Key.Subtract:
                    if (CanExpand && IsEnabled && IsExpanded)
                    {
                        e.Handled = true;
                        IsExpanded = false;
                    }
                    break;

                case Key.Add:
                case Key.Right:
                    if (CanExpand && IsEnabled && !IsExpanded)
                    {
                        e.Handled = true;
                        IsExpanded = true;
                    }
                    break;
            }

            if (!e.Handled)
                ParentTreeView?.HandleKeyDown(this, e);
        }


        /// <summary>
        /// Raises the <see cref="Control.MouseDoubleClick"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseDoubleClick(e);

            if (e.Handled || e.ChangedButton != MouseButton.Left || _header == null || !_header.IsMouseOver)
                return;

            if (CanExpand)
            {
                e.Handled = true;
                IsExpanded = !IsExpanded;
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
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseDown(e);

            if (e.Handled || _header == null || !_header.IsMouseOver)
                return;

            if (e.ChangedButton == MouseButton.Left || (e.ChangedButton == MouseButton.Right && !IsSelected))
            {
                e.Handled = true;
                ParentTreeView?.MoveSelection(this);
            }
        }
        #endregion
    }
}
