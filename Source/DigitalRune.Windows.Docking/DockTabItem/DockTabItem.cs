// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DigitalRune.Linq;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents a selectable, draggable item in the docking layout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="DockTabItem"/> is usually displayed inside a <see cref="Docking.DockTabPane"/>.
    /// When multiple <see cref="DockTabItem"/> are docked inside a
    /// <see cref="Docking.DockTabPane"/> they are displayed as tabs (similar to a
    /// <see cref="TabControl"/>). The <see cref="IsSelected"/> property indicates which of the
    /// <see cref="DockTabItem"/>s is currently visible in the <see cref="Docking.DockTabPane"/>.
    /// </para>
    /// <para>
    /// <strong>Control Template:</strong> Please note that the <see cref="DockTabItem"/> is a
    /// <see cref="ContentControl"/>. The visual that represents the <see cref="DockTabItem"/> is
    /// actually just a tab shown in a <see cref="DockTabPane"/> containing the <see cref="Icon"/>
    /// and <see cref="Title"/>. The control template of a <see cref="DockTabItem"/> defines only
    /// the appearance of the tab. The <see cref="ContentControl.Content"/> is not a visual child of
    /// the <see cref="DockTabItem"/>. Instead the <see cref="ContentControl.Content"/> is hosted in
    /// the <see cref="DockTabPane"/>. The <see cref="DockTabPane"/> automatically displays the
    /// content of the selected <see cref="DockTabItem"/>. (A <see cref="DockTabPane"/> and its
    /// <see cref="DockTabItem"/>s is similar to a normal <see cref="TabControl"/> and its
    /// <see cref="TabItem"/>s.)
    /// </para>
    /// <para>
    /// <strong>Focus:</strong> When a <see cref="DockTabItem"/> is activated the focus is set to
    /// the first focusable element of the <see cref="ContentControl.Content"/> in tab order.
    /// </para>
    /// </remarks>
    [DefaultEvent("Activated")]
    [TemplatePart(Name = "PART_Tab", Type = typeof(UIElement))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Selected")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Active")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Unfocused")]
    [TemplateVisualState(GroupName = "FocusStates", Name = "Focused")]
    public partial class DockTabItem : ContentControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private DockControl _dockControl;
        private bool _activating;
        private UIElement _tab;
        private IEnumerable _logicalChildren = Enumerable.Empty<object>();
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the parent <see cref="Docking.DockTabPane"/>.
        /// </summary>
        /// <value>The parent <see cref="Docking.DockTabPane"/>.</value>
        private DockTabPane DockTabPane
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as DockTabPane; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------    

        /// <summary>
        /// Identifies the <see cref="DockWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DockWidthProperty = DockControl.DockWidthProperty.AddOwner(typeof(DockTabItem));

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
        public static readonly DependencyProperty DockHeightProperty = DockControl.DockHeightProperty.AddOwner(typeof(DockTabItem));

        /// <inheritdoc cref="IDockElement.DockHeight"/>
        [Description("Gets or sets the desired height in the docking layout.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(GridLengthConverter))]
        public GridLength DockHeight
        {
            get { return (GridLength)GetValue(DockHeightProperty); }
            set { SetValue(DockHeightProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon",
            typeof(object),
            typeof(DockTabItem),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a icon of the <see cref="DockTabItem"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The icon of the <see cref="DockTabItem"/>. The default value is <see langword="null"/>.
        /// </value>
        [Description("Gets or sets a icon of the dock window.")]
        [Category(Categories.Common)]
        public object Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }


        private static readonly DependencyPropertyKey IsActivePropertyKey = DependencyProperty.RegisterReadOnly(
            "IsActive",
            typeof(bool),
            typeof(DockTabItem),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Identifies the <see cref="IsActive"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="DockTabItem"/> is active.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="DockTabItem"/> is active; otherwise, 
        /// <see langword="false"/>. The default is <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            private set { SetValue(IsActivePropertyKey, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="IsTabWidthFixed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTabWidthFixedProperty = DependencyProperty.Register(
            "IsTabWidthFixed",
            typeof(bool),
            typeof(DockTabItem),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse));

        /// <summary>
        /// Gets or sets the a value indicating whether the width of the <see cref="DockTabItem"/>
        /// tab is fixed. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the width of the <see cref="DockTabItem"/> tab is fixed;
        /// otherwise, <see langword="false"/> if the width is flexible. The default is
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// The width of the <see cref="DockTabItem"/> tab is determined by the properties
        /// <see cref="FrameworkElement.MinWidth"/>, <see cref="FrameworkElement.MaxWidth"/>, and
        /// <see cref="FrameworkElement.Width"/>. If the <see cref="FrameworkElement.Width"/> is
        /// <see cref="double.NaN"/>, then the extent is determined by the content (usually
        /// <see cref="Icon"/> + <see cref="Title"/>) of the tab.
        /// </para>
        /// <para>
        /// If <see cref="IsTabWidthFixed"/> is <see langword="false"/>, the width of the tab may be
        /// reduced (down to <see cref="FrameworkElement.MinWidth"/>) if there is not enough space
        /// to show the full tab. But if <see cref="IsTabWidthFixed"/> is set, the tab is always
        /// rendered with its full size - even if there is not enough space.
        /// </para>
        /// </remarks>
        [Description("Gets or sets the a value indicating whether the width of the dock window tab is fixed.")]
        [Category(Categories.Layout)]
        public bool IsTabWidthFixed
        {
            get { return (bool)GetValue(IsTabWidthFixedProperty); }
            set { SetValue(IsTabWidthFixedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(DockTabItem),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets title of the <see cref="DockTabItem"/>.
        /// This is a dependency property.
        /// </summary>
        /// <value>The title of the <see cref="DockTabItem"/>.</value>
        [Description("Gets or sets title of the dock window.")]
        [Category(Categories.Common)]
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(
            typeof(DockTabItem),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse,
                FrameworkPropertyMetadataOptions.Journal
                | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault
                | FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                OnIsSelectedChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the current <see cref="DockTabItem"/> is
        /// selected in the <see cref="Docking.DockTabPane"/>. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="DockTabItem"/> is selected in the current
        /// <see cref="Docking.DockTabPane"/>; otherwise, <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Setting <see cref="IsSelected"/> to <see langword="true"/> automatically activates and
        /// focuses the <see cref="DockTabItem"/> when it is unselected. Setting the value to
        /// <see langword="false"/> automatically deactivates this <see cref="DockTabItem"/> and
        /// selects another <see cref="DockTabItem"/> inside the parent
        /// <see cref="Docking.DockTabPane"/>.
        /// </para>
        /// <para>
        /// A <see cref="DockTabItem"/> cannot be deselected if it is the only item inside a
        /// <see cref="Docking.DockTabPane"/>.
        /// </para>
        /// </remarks>
        [Description("Gets or sets a value indicating whether the current DockTabItem is selected in the DockTabPane.")]
        [Category(Categories.Default)]
        [Bindable(true)]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Selected"/> routed event.
        /// </summary>
        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(DockTabItem));

        /// <summary>
        /// Occurs when when the <see cref="DockTabItem"/> gets selected in the 
        /// <see cref="Docking.DockTabPane"/>.
        /// </summary>
        public event RoutedEventHandler Selected
        {
            add { AddHandler(SelectedEvent, value); }
            remove { RemoveHandler(SelectedEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Unselected"/> routed event.
        /// </summary>
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(DockTabItem));

        /// <summary>
        /// Occurs when the <see cref="DockTabItem"/> gets unselected in the 
        /// <see cref="Docking.DockTabPane"/>.
        /// </summary>
        public event RoutedEventHandler Unselected
        {
            add { AddHandler(UnselectedEvent, value); }
            remove { RemoveHandler(UnselectedEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Activated"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ActivatedEvent = EventManager.RegisterRoutedEvent(
            "Activated",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DockTabItem));

        /// <summary>
        /// Occurs when <see cref="DockTabItem"/> gets activated.
        /// </summary>
        /// <remarks>
        /// A <see cref="DockTabItem"/> is active if it is selected inside the current
        /// <see cref="Docking.DockTabPane"/> and has keyboard focus. This can happen in various
        /// ways:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// By calling <see cref="Activate"/> from code.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// By clicking on the tab or inside the <see cref="DockTabItem"/>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// By navigating to the <see cref="DockTabItem"/> by using keyboard navigation (Tab key,
        /// arrow keys, etc.).
        /// </description>
        /// </item>
        /// </list>
        /// <item>
        /// <description>
        /// ...
        /// </description>
        /// </item>
        /// </remarks>
        public event RoutedEventHandler Activated
        {
            add { AddHandler(ActivatedEvent, value); }
            remove { RemoveHandler(ActivatedEvent, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Deactivated"/> routed event.
        /// </summary>
        public static readonly RoutedEvent DeactivatedEvent = EventManager.RegisterRoutedEvent(
            "Deactivated",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DockTabItem));

        /// <summary>
        /// Occurs when <see cref="DockTabItem"/> gets deactivated.
        /// </summary>
        /// <remarks>
        /// A <see cref="DockTabItem"/> gets deactivated when it loses the keyboard focus.
        /// </remarks>
        public event RoutedEventHandler Deactivated
        {
            add { AddHandler(DeactivatedEvent, value); }
            remove { RemoveHandler(DeactivatedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DockTabItem"/> class.
        /// </summary>
        static DockTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DockTabItem), new FrameworkPropertyMetadata(typeof(DockTabItem)));

            // We handle the access key events the same way as the WPF TabControl to make sure that it
            // behaves properly.
            EventManager.RegisterClassHandler(typeof(DockTabItem), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));

            // When navigating with Tab and arrow keys: Keep focus inside DockTabItem.
            // User needs to press Ctrl+Tab to move focus away.
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DockTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DockTabItem), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="DockTabItem"/> class.
        /// </summary>
        public DockTabItem()
        {
            IsEnabledChanged += OnIsEnabledChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;

            RegisterCommandBindings();
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs eventArgs)
        {
            // This code is borrowed from the WPF TabItem.
            if (!eventArgs.Handled && eventArgs.Scope == null)
            {
                var dockTabItem = (DockTabItem)sender;
                if (eventArgs.Target == null)
                {
                    eventArgs.Target = dockTabItem;
                }
                else if (!dockTabItem.IsSelected)    // If DockTabItem is not active it is a scope for its content elements.
                {
                    eventArgs.Scope = dockTabItem;
                    eventArgs.Handled = true;
                }
            }
        }


        /// <summary>
        /// Provides class handling for when an access key that is meaningful for this element is
        /// invoked.
        /// </summary>
        /// <param name="e">
        /// The event data to the access key event. The event data reports which key was invoked,
        /// and indicate whether the <see cref="AccessKeyManager"/> object that controls the sending
        /// of these events also sent this access key invocation to other elements.
        /// </param>
        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            Activate();
        }


        /// <summary>
        /// Gets an enumerator to the content control's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                // [FIX] Labels in DockTabItems do not show access key when ALT is pressed.
                // Cause: ContentControl does not return the Content, even though it is the logical
                //        child. Probably because ContentControl contains some secret sauce for use
                //        within an ItemsControl.
                // Solution: Override LogicalChildren enumerator.
                return _logicalChildren.GetEnumerator();
            }
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_tab != null)
            {
                _tab.MouseDown -= OnTabMouseButtonDown;
                _tab = null;
            }

            base.OnApplyTemplate();

            _tab = GetTemplateChild("PART_Tab") as UIElement;
            if (_tab != null)
                _tab.MouseDown += OnTabMouseButtonDown;

            UpdateVisualStates(false);
        }


        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            _dockControl = DockHelper.GetDockControl(this);
            _dockControl?.Register(this);

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


        private void OnMouseEnter(object sender, MouseEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        private void OnMouseLeave(object sender, MouseEventArgs eventArgs)
        {
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseLeftButtonDown"/> routed event is
        /// raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that the left mouse button was pressed.
        /// </param>
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // Select the current DockTabItem. (Don't set eventArgs.Handled, the event needs to pass
            // through to detect dragging, etc.)
            Activate();

            base.OnMouseLeftButtonDown(e);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseRightButtonDown"/> routed event is
        /// raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that the right mouse button was pressed.
        /// </param>
        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            // The right-clicked DockTabItem needs to be activated to receive keyboard
            // focus. Otherwise, routed commands are routed to a different window.
            Activate();

            base.OnMouseRightButtonDown(e);
        }


        private void OnTabMouseButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (eventArgs.Handled)
                return;

            if (eventArgs.ChangedButton == MouseButton.Left)
            {
                if (eventArgs.ClickCount == 2)
                {
                    // Left mouse button double click executes the Dock/Float command.
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
            else if (eventArgs.ChangedButton == MouseButton.Middle)
            {
                // Middle mouse button executes the Close command.
                if (ApplicationCommands.Close.CanExecute(null, this))
                {
                    ApplicationCommands.Close.Execute(null, this);
                    eventArgs.Handled = true;
                }
            }
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.PreviewGotKeyboardFocus</strong> attached
        /// event reaches an element in its route that is derived from this class.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnPreviewGotKeyboardFocus(e);

            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
            {
                if (!e.Handled && e.NewFocus == this && !IsSelected)
                {
                    IsSelected = true;
                    if (e.OldFocus != Keyboard.FocusedElement)
                    {
                        // Focus has already moved.
                        // --> Do nothing.
                        e.Handled = true;
                    }
                    else
                    {
                        // Focus has not moved.
                        // --> Move focus to content.
                        if (dockTabPane.FocusSelectedContent())
                            e.Handled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.GotKeyboardFocus</strong> attached event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            // Make sure that the DockTabItem is selected in the parent DockTabPane.
            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
            {
                if (!e.Handled && e.NewFocus == this && !IsSelected)
                    dockTabPane.SelectedItem = dockTabPane.ItemContainerGenerator.ItemFromContainer(this);
            }

            if (!IsActive && IsLoaded && IsSelected && IsKeyboardFocusWithin)
            {
                // Raise the Activated event.
                OnActivated(new RoutedEventArgs(ActivatedEvent, this));
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
            // Make sure that the DockTabItem is selected in the parent DockTabPane.
            var dockTabPane = DockTabPane;
            if (dockTabPane != null && !IsSelected)
                 dockTabPane.SelectedItem = dockTabPane.ItemContainerGenerator.ItemFromContainer(this);

            if (!IsActive && IsLoaded && IsSelected && IsKeyboardFocusWithin)
            {
                // Raise the Activated event.
                OnActivated(new RoutedEventArgs(ActivatedEvent, this));
            }

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


        /// <summary>
        /// Called when the <see cref="ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">
        /// The old value of the <see cref="ContentControl.Content"/> property.
        /// </param>
        /// <param name="newContent">
        /// The new value of the <see cref="ContentControl.Content"/> property.
        /// </param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            _logicalChildren = (newContent != null) ? LinqHelper.Return(newContent) : Enumerable.Empty<object>();
            UpdateDockPaneContent();
        }


        /// <summary>
        /// Occurs when the <see cref="ContentControl.ContentStringFormat"/> property changes.
        /// </summary>
        /// <param name="oldContentStringFormat">
        /// The old value of <see cref="ContentControl.ContentStringFormat"/>.
        /// </param>
        /// <param name="newContentStringFormat">
        /// The new value of <see cref="ContentControl.ContentStringFormat"/>.
        /// </param>
        protected override void OnContentStringFormatChanged(string oldContentStringFormat, string newContentStringFormat)
        {
            base.OnContentStringFormatChanged(oldContentStringFormat, newContentStringFormat);
            UpdateDockPaneContent();
        }


        /// <summary>
        /// Called when the <see cref="ContentControl.ContentTemplate"/> property changes.
        /// </summary>
        /// <param name="oldContentTemplate">
        /// The old value of the <see cref="ContentControl.ContentTemplate"/> property.
        /// </param>
        /// <param name="newContentTemplate">
        /// The new value of the <see cref="ContentControl.ContentTemplate"/> property.
        /// </param>
        protected override void OnContentTemplateChanged(DataTemplate oldContentTemplate, DataTemplate newContentTemplate)
        {
            base.OnContentTemplateChanged(oldContentTemplate, newContentTemplate);
            UpdateDockPaneContent();
        }


        /// <summary>
        /// Called when the <see cref="ContentControl.ContentTemplateSelector"/> property changes.
        /// </summary>
        /// <param name="oldContentTemplateSelector">
        /// The old value of the <see cref="ContentControl.ContentTemplateSelector"/> property.
        /// </param>
        /// <param name="newContentTemplateSelector">
        /// The new value of the <see cref="ContentControl.ContentTemplateSelector"/> property.
        /// </param>
        protected override void OnContentTemplateSelectorChanged(DataTemplateSelector oldContentTemplateSelector, DataTemplateSelector newContentTemplateSelector)
        {
            base.OnContentTemplateSelectorChanged(oldContentTemplateSelector, newContentTemplateSelector);
            UpdateDockPaneContent();
        }


        private void UpdateDockPaneContent()
        {
            if (IsSelected)
            {
                // DockTabItem is visible: Show new content in parent DockTabPane.
                DockTabPane?.UpdateSelectedContent();
            }
        }


        private static void OnIsSelectedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var dockTabItem = (DockTabItem)dependencyObject;
            bool isSelected = (bool)eventArgs.NewValue;
            if (isSelected)
                dockTabItem.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, dockTabItem));
            else
                dockTabItem.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, dockTabItem));

            dockTabItem.IsTabStop = isSelected;
            dockTabItem.UpdateVisualStates(true);
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void OnSelected(RoutedEventArgs eventArgs)
        {
            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
                dockTabPane.SelectedItem = dockTabPane.ItemContainerGenerator.ItemFromContainer(this);

            Debug.Assert(eventArgs.RoutedEvent == SelectedEvent, "Invalid arguments for DockTabItem.OnSelected.");
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected virtual void OnUnselected(RoutedEventArgs eventArgs)
        {
            Deactivate();

            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
            {
                var selectedItem = dockTabPane.SelectedItem;
                if (selectedItem == dockTabPane.ItemContainerGenerator.ItemFromContainer(this))
                    dockTabPane.SelectedIndex = -1;
            }

            Debug.Assert(eventArgs.RoutedEvent == UnselectedEvent, "Invalid arguments for DockTabItem.OnUnselected.");
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Selects the <see cref="DockTabItem"/> and gives it keyboard focus.
        /// </summary>
        /// <remarks>
        /// When a <see cref="DockTabItem"/> is activated the focus is set to the first focusable
        /// element in <see cref="ContentControl.Content"/> - or the element with tab index 0 if the
        /// <strong>KeyboardNavigation.TabIndex</strong> attached dependency property is set.
        /// </remarks>
        public void Activate()
        {
            if (IsActive)
                return;

            // Avoid re-entrance.
            if (_activating)
                return;

            _activating = true;

            try
            {
                FocusContent();

                if (!IsActive && IsSelected && IsKeyboardFocusWithin)
                {
                    // Raise the Activated event.
                    OnActivated(new RoutedEventArgs(ActivatedEvent, this));
                }
            }
            finally
            {
                _activating = false;
            }
        }


        private void FocusContent()
        {
            // Try to set focus to content.
            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
            {
                // Make sure that this DockTabItem is selected and that its Content is visible in
                // the DockTabPane.
                var item = dockTabPane.ItemContainerGenerator.ItemFromContainer(this);
                dockTabPane.SelectedItem = item;

                // Now try to move focus to content.
                dockTabPane.FocusSelectedContent();

                // When the above DockTabPane.FocusSelectedContent() fails, it can happen that
                // another DockTabItem is selected. Therefore, we need to check once more, that
                // this DockTabItem is selected.
                dockTabPane.SelectedItem = item;
            }
            else
            {
                Focus();
            }
        }


        /// <summary>
        /// Deactivates the <see cref="DockTabItem"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Deactivate"/> is private and is only called internally.
        /// </remarks>
        private void Deactivate()
        {
            if (!IsActive)
                return;

            OnDeactivated(new RoutedEventArgs(DeactivatedEvent, this));
        }


        /// <summary>
        /// Raises the <see cref="Activated"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnActivated"/> in a
        /// derived class, be sure to call the base class's <see cref="OnActivated"/> method so that
        /// registered delegates receive the event.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected internal virtual void OnActivated(RoutedEventArgs eventArgs)
        {
            Debug.Assert(!IsActive, "DockTabItem.OnActivated should not be called when item is active.");

            // Set DockControl.ActiveDockTabPane/ActiveDockTabItem.
            SetActiveItem();

            IsActive = true;

            // Deactivate other DockTabItems and raise Deactivated event.
            DeactivateOtherItems();

            // Raise Activated event.
            Debug.Assert(eventArgs.RoutedEvent == ActivatedEvent, "Invalid arguments for DockTabItem.OnActivated.");
            RaiseEvent(eventArgs);

            // Update visual states.
            UpdateVisualStates(true);
            DockTabPane?.UpdateVisualStates(true);
        }


        /// <summary>
        /// Raises the <see cref="Deactivated"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnDeactivated"/> in a
        /// derived class, be sure to call the base class's <see cref="OnDeactivated"/> method so
        /// that registered delegates receive the event.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected internal virtual void OnDeactivated(RoutedEventArgs eventArgs)
        {
            Debug.Assert(IsActive, "DockTabItem.OnDeactivated should only be called when item is active.");

            IsActive = false;

            // Raise Deactivated event.
            Debug.Assert(eventArgs.RoutedEvent == DeactivatedEvent, "Invalid arguments for DockTabItem.OnDeactivated.");
            RaiseEvent(eventArgs);

            // Update visual states.
            UpdateVisualStates(true);
            DockTabPane?.UpdateVisualStates(true);
        }


        private void DeactivateOtherItems()
        {
            var dockControl = _dockControl;
            if (dockControl != null)
            {
                for (int i = 0; i < dockControl.DockTabItems.Count; i++)
                {
                    var dockTabItem = dockControl.DockTabItems[i];
                    if (dockTabItem != null && dockTabItem != this)
                        dockTabItem.Deactivate();
                }

                return;
            }

            var dockTabPane = DockTabPane;
            if (dockTabPane != null)
            {
                for (int i = 0; i < dockTabPane.Items.Count; i++)
                {
                    var dockTabItem = dockTabPane.ItemContainerGenerator.ContainerFromIndex(i) as DockTabItem;
                    if (dockTabItem != null && dockTabItem != this)
                        dockTabItem.Deactivate();
                }
            }
        }


        private void SetActiveItem()
        {
            var dockStrategy = _dockControl?.DockStrategy;
            var dockTabItem = this.GetViewModel();
            if (dockStrategy != null && !dockStrategy.IsBusy && dockTabItem != null)
                dockStrategy.Activate(dockTabItem);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            if (IsEnabled)
            {
                if (IsActive)
                    VisualStateManager.GoToState(this, "Active", useTransitions);
                else if (IsSelected)
                    VisualStateManager.GoToState(this, "Selected", useTransitions);
                else if (IsMouseOver)
                    VisualStateManager.GoToState(this, "MouseOver", useTransitions);
                else
                    VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }

            if (IsFocused)
                VisualStateManager.GoToState(this, "Focused", useTransitions);
            else
                VisualStateManager.GoToState(this, "Unfocused", useTransitions);
        }
        #endregion
    }
}
