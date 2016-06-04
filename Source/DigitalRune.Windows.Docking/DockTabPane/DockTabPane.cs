// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a pane that contains multiple items that share the same space on the screen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DockTabPane"/> is similar to a <see cref="TabControl"/>. The item containers
    /// are of type <see cref="DockTabItem"/>.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TemplatePart(Name = "PART_DragArea", Type = typeof(UIElement))]
    [TemplatePart(Name = "PART_SelectedContentHost", Type = typeof(ContentPresenter))]
    [TemplatePart(Name = "PART_AutoHideButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_DockButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_WindowListButton", Type = typeof(ToggleButton))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "DockStates", Name = "AutoHide")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Dock")]
    [TemplateVisualState(GroupName = "DockStates", Name = "Float")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Active")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Inactive")]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(DockTabItem))]
    public partial class DockTabPane : Selector
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Workaround for creating item containers.
        private object _currentItem;

        private DockControl _dockControl;
        private DockState _dockState;
        private bool _isActive;

        // Template parts
        private Button _autoHideButton;
        private Button _dockButton;
        private Button _closeButton;
        private ToggleButton _windowListButton;
        private UIElement _dragArea;                // Marks the area where the user can drag the DockTabPane.
        private ContentPresenter _contentPresenter; // Contains the currently selected DockTabItem content.

        // Visual states
        private bool _isMouseOverDragArea;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DockWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockWidthProperty = DockControl.DockWidthProperty.AddOwner(typeof(DockTabPane));

        /// <inheritdoc cref="IDockElement.DockWidth"/>
        [Description("Gets or sets the desired width in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockWidth
        {
            get { return (GridLength)GetValue(DockWidthProperty); }
            set { SetValue(DockWidthProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DockHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockHeightProperty = DockControl.DockHeightProperty.AddOwner(typeof(DockTabPane));

        /// <inheritdoc cref="IDockElement.DockHeight"/>
        [Description("Gets or sets the desired height in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockHeight
        {
            get { return (GridLength)GetValue(DockHeightProperty); }
            set { SetValue(DockHeightProperty, value); }
        }


        private static readonly DependencyPropertyKey HasSingleItemPropertyKey = DependencyProperty.RegisterReadOnly(
            "HasSingleItem",
            typeof(bool),
            typeof(DockTabPane),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Identifies the <see cref="HasSingleItem"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasSingleItemProperty = HasSingleItemPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value indicating whether this <see cref="DockTabPane"/> has exactly one item.
        /// This is a dependency property.
        /// </summary>
        [Browsable(false)]
        public bool HasSingleItem
        {
            get { return (bool)GetValue(HasSingleItemProperty); }
            private set { SetValue(HasSingleItemPropertyKey, Boxed.Get(value)); }
        }


        private static readonly DependencyPropertyKey SelectedContentPropertyKey = DependencyProperty.RegisterReadOnly(
            "SelectedContent",
            typeof(object),
            typeof(DockTabPane),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SelectedContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedContentProperty = SelectedContentPropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the content of the currently selected <see cref="DockTabItem"/>.
        /// This is a dependency property.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public object SelectedContent
        {
            get { return GetValue(SelectedContentProperty); }
            internal set { SetValue(SelectedContentPropertyKey, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockTabPane"/> class.
        /// </summary>
        static DockTabPane()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockTabPane), new FrameworkPropertyMetadata(typeof(DockTabPane)));

            // DockTabPane is not a tab stop, only the DockTabItems
            IsTabStopProperty.OverrideMetadata(typeof(DockTabPane), new FrameworkPropertyMetadata(Boxed.BooleanFalse));

            // We handle the access key events to make sure that it behaves properly.
            EventManager.RegisterClassHandler(typeof(DockTabPane), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));

            // Keyboard navigation with arrow keys cycles through DockTabItems
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DockTabPane), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DockTabPane"/> class.
        /// </summary>
        public DockTabPane()
        {
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsEnabledChanged += OnIsEnabledChanged;

            RegisterCommandBindings();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs eventArgs)
        {
            if (eventArgs.Handled || eventArgs.Scope != null)
                return;

            var focusedElement = Keyboard.FocusedElement as FrameworkElement;
            if (focusedElement == null)
                return;

            // Do not accept access keys that originated outside of the DockTabPane.
            var dockTabPane = (DockTabPane)sender;
            if (!dockTabPane.IsVisualAncestorOf(focusedElement))
            {
                // Do not set eventArgs.Handled because this would break the chain.
                // --> Setting the scope skips the current target.
                eventArgs.Scope = dockTabPane;

                // See DigitalRune.Windows.AccessKeyScope for additional information!
            }
        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            var oldDataContext = eventArgs.OldValue as INotifyPropertyChanged;
            if (oldDataContext != null)
                PropertyChangedEventManager.RemoveHandler(oldDataContext, OnDockStateChanged, nameof(IDockElement.DockState));

            var newDataContext = eventArgs.NewValue as INotifyPropertyChanged;
            if (newDataContext != null)
                PropertyChangedEventManager.AddHandler(newDataContext, OnDockStateChanged, nameof(IDockElement.DockState));

            var dockTabPane = eventArgs.NewValue as IDockTabPane;
            _dockState = dockTabPane?.DockState ?? DockState.Dock;
        }


        private void OnDockStateChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            var dockTabPane = sender as IDockTabPane;
            _dockState = dockTabPane?.DockState ?? DockState.Dock;

            // Filter criterion has changed. --> Refresh collection view.
            var itemsSource = ItemsSource;
            if (itemsSource != null)
            {
                var collectionView = CollectionViewSource.GetDefaultView(itemsSource);
                collectionView.Refresh();
            }
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
                // Filter IDockPanes that are invisible.
                var collectionView = CollectionViewSource.GetDefaultView(newValue);
                var collectionViewLiveShaping = collectionView as ICollectionViewLiveShaping;
                if (collectionViewLiveShaping != null && collectionViewLiveShaping.CanChangeLiveFiltering)
                {
                    collectionViewLiveShaping.LiveFilteringProperties.Clear();
                    collectionViewLiveShaping.LiveFilteringProperties.Add(nameof(IDockTabItem.DockState));
                    collectionViewLiveShaping.IsLiveFiltering = true;
                    collectionView.Filter = Filter;
                }
            }
        }


        private bool Filter(object item)
        {
            var dockTabItem = item as IDockTabItem;
            return dockTabItem == null || dockTabItem.DockState == _dockState;
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
            if (item is DockTabItem)
            {
                _currentItem = null;
                return true;
            }

            // Store the current item for use in GetContainerForItemOverride.
            // (The base ItemsControl will call IsItemItsOwnContainerOverride followed by
            // GetContainerForItemOverride if the result was false.)
            _currentItem = item;
            return false;
        }


        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>The element that is used to display the given item.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            if (_currentItem != null)
            {
                var type = _currentItem.GetType();
                _currentItem = null;

                // Manually load implicit data template. (Otherwise the ItemsControl will create a
                // ContentPresenter.)
                var dataTemplateKey = new DataTemplateKey(type);
                var dataTemplate = TryFindResource(dataTemplateKey) as DataTemplate;
                var container = dataTemplate?.LoadContent();
                if (container is DockTabItem)
                    return container;
            }

            return new DockTabItem();
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
            ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
        }


        /// <summary>
        /// Called when status of the <see cref="ItemsControl.ItemContainerGenerator"/> has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void OnGeneratorStatusChanged(object sender, EventArgs eventArgs)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                UpdateSelectedContent();
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // ----- Cleanup
            if (_dragArea != null)
            {
                _dragArea.MouseEnter -= OnDragAreaMouseEnter;
                _dragArea.MouseLeave -= OnDragAreaMouseLeave;
                _dragArea.MouseLeftButtonDown -= OnDragAreaMouseLeftButtonDown;
                _dragArea = null;
            }

            _autoHideButton = null;
            _dockButton = null;
            _closeButton = null;
            _windowListButton = null;

            if (_contentPresenter != null)
            {
                _contentPresenter.GotKeyboardFocus -= OnContentGotKeyboardFocus;
                _contentPresenter = null;
            }

            base.OnApplyTemplate();

            // ----- Get template parts.
            _dragArea = GetTemplateChild("PART_DragArea") as UIElement;
            if (_dragArea != null)
            {
                _dragArea.MouseEnter += OnDragAreaMouseEnter;
                _dragArea.MouseLeave += OnDragAreaMouseLeave;
                _dragArea.MouseLeftButtonDown += OnDragAreaMouseLeftButtonDown;
            }

            _autoHideButton = GetTemplateChild("PART_AutoHideButton") as Button;
            _dockButton = GetTemplateChild("PART_DockButton") as Button;
            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _windowListButton = GetTemplateChild("PART_WindowListButton") as ToggleButton;

            _contentPresenter = GetTemplateChild("PART_SelectedContentHost") as ContentPresenter;
            if (_contentPresenter != null)
            {
                _contentPresenter.GotKeyboardFocus += OnContentGotKeyboardFocus;
            }

            InitializeButtons();
            UpdateSelectedContent();
            UpdateVisualStates(false);
        }


        /// <summary>
        /// Updates the current selection when an item in the <see cref="Selector"/> has changed.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (e.Action == NotifyCollectionChangedAction.Remove && SelectedIndex == -1)
            {
                // Selected one of the remaining neighbor DockTabItems. Try to take the right
                // DockTabItem. If the right DockTabItem was removed take the left DockTabItem.

                // Tab i was removed. Go to index i + 1 and search backwards.
                int startIndex = e.OldStartingIndex + 1;
                if (startIndex > Items.Count)
                {
                    // Tab i + 1 does not exist. Start search at tab 0.
                    startIndex = 0;
                }

                // Go one step back.
                // This way we find either
                // - the tab that has moved to the index of the removed tab,
                // - or, the previous tab if the removed tab was the last in the DockTabPane.
                int nextIndex = GetNextIndex(startIndex, -1);
                if (nextIndex >= 0)
                {
                    SelectedIndex = nextIndex;
                    if (_isActive)
                        FocusSelectedContent();
                }
            }

            // Ensure that there is always something selected - unless the DockTabPane is empty.
            if (SelectedIndex == -1 && Items.Count > 0)
                SelectedIndex = 0;

            // Update property HasSingleItem.
            HasSingleItem = (Items.Count == 1);

            UpdateVisualStates(true);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            _dockControl = DockHelper.GetDockControl(this);
            if (_dockControl != null)
            {
                _dockControl.Register(this);
                CommandManager.InvalidateRequerySuggested();
            }

            UpdateVisualStates(true);
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            if (_dockControl != null)
            {
                _dockControl.Unregister(this);
                _dockControl = null;
            }

            UpdateVisualStates(true);
        }


        /// <summary>
        /// Raises the <see cref="UIElement.GotFocus"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Raises the <see cref="UIElement.LostFocus"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            UpdateVisualStates(true);
        }


        private void OnContentGotKeyboardFocus(object sender, RoutedEventArgs eventArgs)
        {
            var dockTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DockTabItem;
            if (dockTabItem != null && !dockTabItem.IsActive)
                dockTabItem.OnActivated(new RoutedEventArgs(DockTabItem.ActivatedEvent, dockTabItem));
        }


        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        private void OnDragAreaMouseEnter(object sender, MouseEventArgs eventArgs)
        {
            _isMouseOverDragArea = true;
            UpdateVisualStates(true);
        }


        private void OnDragAreaMouseLeave(object sender, MouseEventArgs eventArgs)
        {
            _isMouseOverDragArea = false;
            UpdateVisualStates(true);
        }


        private void OnDragAreaMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.ClickCount == 2)
            {
                // Execute Dock/Float command.
                if (DockCommands.Dock.CanExecute(null, this))
                {
                    DockCommands.Dock.Execute(null, this);
                    eventArgs.Handled = true;
                }
                else if (DockCommands.Float.CanExecute(null, this))
                {
                    DockCommands.Float.Execute(null, this);
                    eventArgs.Handled = true;
                }
            }
        }


        /// <summary>
        /// Gets the index the next item searching in a given direction (cyclic).
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <param name="direction">
        /// <para>
        /// The search direction. -1 to search backwards, 1 to search forwards.
        /// </para>
        /// <para>
        /// This value is usually set to 1 to search forwards or -1 to search backwards.
        /// </para>
        /// </param>
        /// <returns>
        /// The index of the next item found in the given search direction. -1 if no item was found.
        /// </returns>
        private int GetNextIndex(int startIndex, int direction)
        {
            Debug.Assert(Math.Abs(direction) == 1, "The search direction needs to be either -1 or 1.");

            int index = startIndex + direction;
            if (index >= Items.Count)
                index = 0;
            else if (index < 0)
                index = Items.Count - 1;

            return index;
        }


        /// <summary>
        /// Invoked when the <see cref="UIElement.KeyDown"/> event is received.
        /// </summary>
        /// <param name="e">Information about the event.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
                return;

            if (_contentPresenter?.IsKeyboardFocusWithin == true)
                return;

            // Translate key if necessary.
            var key = e.Key;
            if (FlowDirection == FlowDirection.RightToLeft)
            {
                switch (key)
                {
                    case Key.Left:
                        key = Key.Right;
                        break;
                    case Key.Right:
                        key = Key.Left;
                        break;
                    case Key.Home:
                        key = Key.End;
                        break;
                    case Key.End:
                        key = Key.Home;
                        break;
                }
            }

            int direction = 0;
            int startIndex = SelectedIndex;
            switch (key)
            {
                case Key.Left:
                    direction = -1;
                    break;

                case Key.Right:
                    direction = 1;
                    break;

                case Key.End:
                    // Select last DockTabItem
                    direction = -1;
                    startIndex = Items.Count;
                    break;

                case Key.Home:
                    // Select first DockTabItem
                    direction = 1;
                    startIndex = -1;
                    break;

                case Key.Tab:
                    bool controlPressed = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
                    if (controlPressed)
                    {
                        // Cycle through tabs.
                        // Ctrl+Tab ... cycle forwards
                        // Shift+Ctrl+Tab ... cycle backwards
                        startIndex = SelectedIndex;
                        bool shiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
                        direction = shiftPressed ? -1 : 1;
                    }
                    break;
            }

            if (direction != 0)
            {
                int index = GetNextIndex(startIndex, direction);
                if (index >= 0 && index != SelectedIndex)
                {
                    var dockTabItem = ItemContainerGenerator.ContainerFromIndex(index) as DockTabItem;
                    if (dockTabItem != null)
                    {
                        // Select DockTabItem and set keyboard focus to content.
                        dockTabItem.Activate();
                        e.Handled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Called when the selection changes.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (IsKeyboardFocusWithin)
            {
                // Make sure DockTabItem has focus. (Necessary to ensure proper Tab navigation order.)
                var dockTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DockTabItem;
                dockTabItem?.Focus();
            }

            UpdateSelectedContent();
        }


        /// <summary>
        /// Sets the content of the <see cref="_contentPresenter"/> to the selected 
        /// <see cref="DockTabItem"/>.
        /// </summary>
        internal void UpdateSelectedContent()
        {
            if (SelectedIndex < 0)
            {
                SelectedContent = null;
            }
            else
            {
                var dockTabItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as DockTabItem;
                if (dockTabItem != null)
                {
                    var content = dockTabItem.Content;
                    SelectedContent = content;
                    if (_contentPresenter != null)
                    {
                        _contentPresenter.HorizontalAlignment = dockTabItem.HorizontalContentAlignment;
                        _contentPresenter.VerticalAlignment = dockTabItem.VerticalContentAlignment;
                        _contentPresenter.ContentTemplate = dockTabItem.ContentTemplate;
                        _contentPresenter.ContentTemplateSelector = dockTabItem.ContentTemplateSelector;
                        _contentPresenter.ContentStringFormat = dockTabItem.ContentStringFormat;
                    }
                }
            }
        }


        /// <summary>
        /// Moves the keyboard focus to the content of the currently selected
        /// <see cref="DockTabItem"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the focus was set to the selected content; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        internal bool FocusSelectedContent()
        {
            // IMPORTANT: Calling UpdateLayout() when the window is in the process of being closed,
            // may cause a FatalExecutionEngineError. Always check the IsVisible flag!
            if (_contentPresenter == null || !IsVisible)
                return false;

            // Make sure content is set.
            UpdateLayout();

            // Try to move focus to content. (Does not always work.)
            bool focusMoved = false;
            if (!_contentPresenter.IsKeyboardFocusWithin)
                focusMoved = _contentPresenter.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            return focusMoved;
        }


        private void ShowWindowList()
        {
            var contextMenu = new DockPaneMenu
            {
                DataContext = this,
                IsOpen = true,
            };

            if (_windowListButton != null)
            {
                _windowListButton.IsChecked = true;
                contextMenu.Closed += (s, e) => _windowListButton.IsChecked = false;
                contextMenu.Placement = PlacementMode.Bottom;
                contextMenu.PlacementTarget = _windowListButton;
            }
        }


        internal void UpdateVisualStates(bool useTransitions)
        {
            // The DockTabPane is considered active if a DockTabItem is active.
            _isActive = false;
            for (int i = 0; i < Items.Count; i++)
            {
                var dockTabItem = ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;
                if (dockTabItem != null && dockTabItem.IsActive)
                {
                    _isActive = true;
                    break;
                }
            }

            if (_isActive)
            {
                VisualStateManager.GoToState(this, "Active", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Inactive", useTransitions);
            }

            if (IsEnabled)
            {
                bool isLocked = _dockControl?.DockStrategy?.DockControl.IsLocked ?? false;
                if (_isMouseOverDragArea && !isLocked)
                {
                    VisualStateManager.GoToState(this, "MouseOver", useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Normal", useTransitions);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }

            switch (_dockState)
            {
                case DockState.AutoHide:
                    VisualStateManager.GoToState(this, "AutoHide", useTransitions);
                    break;
                case DockState.Dock:
                    VisualStateManager.GoToState(this, "Dock", useTransitions);
                    break;
                case DockState.Float:
                    VisualStateManager.GoToState(this, "Float", useTransitions);
                    break;
            }
        }
        #endregion
    }
}
