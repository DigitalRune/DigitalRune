// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Windows.Themes;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a button that opens a drop-down when the button is clicked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The content of the drop-down (see property <see cref="DropDown"/>) is either a
    /// <see cref="ContextMenu"/>, a <see cref="Popup"/>, or a regular <see cref="UIElement"/>.
    /// </para>
    /// <para>
    /// The following key combinations can be used to open or close the drop-down.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Key</term>
    /// <description>Description</description>
    /// </listheader>
    /// <item>
    /// <term>Alt+Down</term>
    /// <description>Open drop-down.</description>
    /// </item>
    /// <item>
    /// <term>Alt+Up</term>
    /// <description>Close drop-down.</description>
    /// </item>
    /// <item>
    /// <term>Esc</term>
    /// <description>Close drop-down.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <example>
    /// The following XAML code shows how to use the <see cref="DropDownButton"/>.
    /// <code lang="xaml">
    /// <![CDATA[
    /// <dr:DropDownButton>
    ///     <dr:DropDownButton.DropDown>
    ///         <!-- TODO: Add ContextMenu, Popup or any other UIElement here. -->
    ///     </dr:DropDownButton.DropDown>
    ///     DropDown Menu
    /// </dr:DropDownButton>
    /// ]]>
    /// </code>
    /// </example>
    [TemplatePart(Name = PART_DropDownButton, Type = typeof(ToggleButton))]
    public class DropDownButton : ContentControl, ICommandSource
    {
        // Notes:
        // Popup.StaysOpen = false does not work with ToggleButton.ClickMode = Press.
        // Therefore, opening and closing of the popup is handled explicitly:
        // - When the drop-down is opened, the mouse is capture by the DropDownButton.
        // - Another element (e.g. Button) inside the drop-down may take over mouse capture.
        // - When an element inside the drop-down gives up mouse capture, the DropDownButton
        //   retakes the mouse capture.
        // - When the mouse capture is lost, or another element outside the drop-down captures
        //   the drop-down is closed.
        // - When the user clicks outside the drop-down, the drop-down is closed.
        // - When the focus moves outside of the DropDownButton or the drop-down, the drop-down
        //   is closed.
        //
        // Bugs:
        // - If we use a ContextMenu as DropDown, sometimes the DataContext is not properly 
        //   inherited. This can be fixed with:
        //   <dr:DropDownButton.DropDown>
        //       <ContextMenu DataContext = "{Binding}" ... />
        //   </ dr:DropDownButton.DropDown>
        //   Perhaps this is a WPF problem...


        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string PART_DropDownButton = nameof(PART_DropDownButton);
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ToggleButton _toggleButton;

        // Drop-down is either ContextMenu or Popup.
        private ContextMenu _contextMenu;
        private DependencyObject _contextMenuRoot;
        private Popup _popup;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the button that raises the <see cref="Command"/> and the
        /// <see cref="Click"/> event.
        /// </summary>
        /// <value>
        /// The button that raises the <see cref="Command"/> and the <see cref="Click"/> event.
        /// </value>
        protected ButtonBase Button
        {
            get { return _button; }
            set
            {
                if (_button != null)
                {
                    _button.Click -= OnButtonClicked;
                    _button = null;
                }

                _button = value;

                if (_button != null)
                    _button.Click += OnButtonClicked;
            }
        }
        private ButtonBase _button;


        /// <summary>
        /// Occurs when the drop-down opened.
        /// </summary>
        public event EventHandler<EventArgs> DropDownOpened;


        /// <summary>
        /// Occurs after the drop-down closed.
        /// </summary>
        public event EventHandler<EventArgs> DropDownClosed;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DropDown"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register(
            "DropDown",
            typeof(UIElement),
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(null, OnDropDownChanged));

        /// <summary>
        /// Gets or sets the <see cref="UIElement"/> that represents the drop-down.
        /// This is a dependency property.
        /// </summary>
        /// <value>The <see cref="UIElement"/> that represents the drop-down.</value>
        [Description("Gets or sets the UI element that represents the drop-down.")]
        [Category(Categories.Default)]
        public UIElement DropDown
        {
            get { return (UIElement)GetValue(DropDownProperty); }
            set { SetValue(DropDownProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Command"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets the command that will be executed when the command source is invoked.
        /// This is a dependency property.
        /// </summary>
        /// <value>The command that will be executed when the command source is invoked.</value>
        [Description("Gets or sets the command that will be executed when the command source is invoked.")]
        [Category(Categories.Action)]
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            "CommandParameter",
            typeof(object),
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the a user defined data value that can be passed to the command when it is executed.
        /// This is a dependency property.
        /// </summary>
        /// <value>The a user defined data value that can be passed to the command when it is executed.</value>
        [Description("Gets or sets the a user defined data value that can be passed to the command when it is executed.")]
        [Category(Categories.Action)]
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CommandTarget"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CommandTargetProperty = DependencyProperty.Register(
            "CommandTarget",
            typeof(IInputElement),
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the element that the command is executed on.
        /// This is a dependency property.
        /// </summary>
        /// <value>The element that the command is executed on.</value>
        [Description("Gets or sets the element that the command is executed on.")]
        [Category(Categories.Action)]
        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen),
            typeof(bool),
            typeof(DropDownButton),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

        /// <summary>
        /// Gets or sets the a value indicating whether the drop-down is currently visible on screen.
        /// This is a dependency property.
        /// </summary>
        /// <value>The a value indicating whether the drop-down is currently visible on screen.</value>
        [Description("Gets or sets the a value indicating whether the drop-down is currently visible on screen.")]
        [Category(Categories.Appearance)]
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="Click"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            "Click",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(DropDownButton));

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event RoutedEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="DropDownButton"/> class.
        /// </summary>
        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));

            EventManager.RegisterClassHandler(typeof(DropDownButton), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
            EventManager.RegisterClassHandler(typeof(DropDownButton), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnMouseDownOutsideCapturedElement));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="DropDownOpened"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/>
        /// When overriding <see cref="OnDropDownOpened"/> in a derived class, be sure to call the base class's
        /// <see cref="OnDropDownOpened"/> method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnDropDownOpened(EventArgs eventArgs)
        {
            DropDownOpened?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Raises the <see cref="DropDownClosed"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// <see cref="EventArgs"/> object that provides the arguments for the event.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/>
        /// When overriding <see cref="OnDropDownClosed"/> in a derived class, be sure to call the base class's
        /// <see cref="OnDropDownClosed"/> method so that registered delegates receive the event.
        /// </remarks>
        protected virtual void OnDropDownClosed(EventArgs eventArgs)
        {
            DropDownClosed?.Invoke(this, eventArgs);
        }


        /// <summary>
        /// Called when the <see cref="DropDown"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDropDownChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (DropDownButton)dependencyObject;
            var oldValue = (FrameworkElement)eventArgs.OldValue;
            var newValue = (FrameworkElement)eventArgs.NewValue;
            control.OnDropDownChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="DropDown"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnDropDownChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
            // Remove previous drop-down element.
            if (_contextMenu != null)
            {
                if (_contextMenuRoot != null)
                {
                    RemoveLogicalChild(_contextMenuRoot);
                    _contextMenuRoot = null;
                }

                BindingOperations.ClearBinding(_contextMenu, ContextMenu.IsOpenProperty);
                _contextMenu.ClearValue(ContextMenu.PlacementProperty);
                _contextMenu.ClearValue(ContextMenu.PlacementTargetProperty);
                _contextMenu = null;
            }
            else if (_popup != null)
            {
                RemoveLogicalChild(_popup);

                _popup.Opened -= OnPopupOpened;
                _popup.MouseDown -= OnPopupMouseDown;
                _popup.ContextMenuOpening -= OnPopupContextMenuOpening;
                BindingOperations.ClearBinding(_popup, Popup.IsOpenProperty);
                _popup.ClearValue(Popup.PlacementProperty);
                _popup.ClearValue(Popup.PlacementTargetProperty);
                _popup = null;
            }

            // Use new drop-down element.
            _contextMenu = newValue as ContextMenu;
            if (_contextMenu != null)
            {
                _contextMenu.Placement = PlacementMode.Bottom;
                _contextMenu.PlacementTarget = this;
                BindToIsDropDownOpen(_contextMenu, ContextMenu.IsOpenProperty);

                // Add as logical child for data binding and routed commands.
                _contextMenu.IsOpen = true;
                DependencyObject element = _contextMenu;
                do
                {
                    _contextMenuRoot = element;
                    element = LogicalTreeHelper.GetParent(element);
                } while (null != element);
                _contextMenu.IsOpen = false;
                AddLogicalChild(_contextMenuRoot);
            }
            else
            {
                _popup = newValue as Popup;
                if (_popup == null)
                {
                    _popup = new Popup
                    {
                        AllowsTransparency = true,
                        Child = new SystemDropShadowChrome
                        {
                            Color = SystemParameters.DropShadow ? Color.FromArgb(113, 0, 0, 0) : Colors.Transparent,
                            Margin = SystemParameters.DropShadow ? new Thickness(0, 0, 5, 5) : new Thickness(0),
                            SnapsToDevicePixels = true,
                            Child = newValue
                        }
                    };
                }
                _popup.Placement = PlacementMode.Bottom;
                _popup.PlacementTarget = this;
                BindToIsDropDownOpen(_popup, Popup.IsOpenProperty);
                _popup.Opened += OnPopupOpened;
                _popup.MouseDown += OnPopupMouseDown;

                // We need to stop the context menu of the drop down button or its ancestors from
                // opening in the popup.
                if (_popup.ContextMenu == null)
                    _popup.ContextMenuOpening += OnPopupContextMenuOpening;
                
                // Add as logical child for data binding and routed commands.
                AddLogicalChild(_popup);
            }
        }


        private void BindToIsDropDownOpen(DependencyObject dropDown, DependencyProperty isOpenProperty)
        {
            var binding = new Binding(nameof(IsDropDownOpen))
            {
                Source = this,
                Mode = BindingMode.TwoWay
            };
            BindingOperations.SetBinding(dropDown, isOpenProperty, binding);
        }


        /// <summary>
        /// Called when the <see cref="Command"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (DropDownButton)dependencyObject;
            var oldCommand = (ICommand)eventArgs.OldValue;
            var newCommand = (ICommand)eventArgs.NewValue;
            control.OnCommandChanged(oldCommand, newCommand);
        }

        /// <summary>
        /// Called when the <see cref="Command"/> property changed.
        /// </summary>
        /// <param name="oldCommand">The old value.</param>
        /// <param name="newCommand">The new value.</param>
        protected virtual void OnCommandChanged(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
                CanExecuteChangedEventManager.RemoveHandler(oldCommand, OnCanExecuteChanged);

            if (newCommand != null)
                CanExecuteChangedEventManager.AddHandler(newCommand, OnCanExecuteChanged);

            OnCanExecuteChanged();
        }


        private void OnCanExecuteChanged(object sender, EventArgs eventArgs)
        {
            OnCanExecuteChanged();
        }


        private void OnCanExecuteChanged()
        {
            var command = Command;
            if (command == null)
                return;

            var routedCommand = command as RoutedCommand;
            IsEnabled = (routedCommand != null)
                        ? routedCommand.CanExecute(CommandParameter, CommandTarget)
                        : command.CanExecute(CommandParameter);
        }


        /// <summary>
        /// Raises the <see cref="Click"/> routed event.
        /// </summary>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong> When overriding <see cref="OnClick"/> in a derived
        /// class, be sure to call the base class's <see cref="OnClick"/> method so that registered
        /// delegates receive the event.
        /// </remarks>
        protected virtual void OnClick(RoutedEventArgs eventArgs)
        {
            if (eventArgs == null || eventArgs.RoutedEvent != ClickEvent)
                throw new ArgumentException("Invalid arguments for DropDownButton.OnClick. eventArgs.RoutedEvent must be set to ClickEvent.", nameof(eventArgs));

            RaiseEvent(eventArgs);
            RaiseCommand();
        }


        private void RaiseCommand()
        {
            var command = Command;
            if (command == null)
                return;

            var routedCommand = command as RoutedCommand;
            if (routedCommand != null)
                routedCommand.Execute(CommandParameter, CommandTarget);
            else
                command.Execute(CommandParameter);
        }


        /// <summary>
        /// Called when the <see cref="IsDropDownOpen"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnIsOpenChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (DropDownButton)dependencyObject;
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)eventArgs.NewValue;
            control.OnIsOpenChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="IsDropDownOpen"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            if (newValue)
            {
                if (_popup != null)
                    Mouse.Capture(this, CaptureMode.SubTree);

                OnDropDownOpened(EventArgs.Empty);
            }
            else
            {
                ReleaseMouseCapture();

                OnDropDownClosed(EventArgs.Empty);
            }
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_toggleButton != null)
            {
                _toggleButton.PreviewMouseLeftButtonDown -= OnToggleButtonPreviewLeftButtonMouseDown;
                _toggleButton = null;
                Button = null;
            }

            base.OnApplyTemplate();

            _toggleButton = GetTemplateChild(PART_DropDownButton) as ToggleButton;

            if (_toggleButton != null)
            {
                _toggleButton.PreviewMouseLeftButtonDown += OnToggleButtonPreviewLeftButtonMouseDown;
                Button = _toggleButton;
            }
        }


        private void OnToggleButtonPreviewLeftButtonMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (_contextMenu != null && _contextMenu.IsLoaded)
            {
                // The click has already been handled by the ContextMenu and IsDropDown has toggled.
                // Do not let the event pass through to the ToggleButton. Otherwise, the ContextMenu
                // is shown again.
                eventArgs.Handled = true;
            }
        }


        private void OnButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            OnClick(new RoutedEventArgs(ClickEvent));
        }


        /// <summary>
        /// Handles the <see cref="IInputElement.KeyDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnKeyDown(e);

            if (e.Handled)
                return;

            if (!IsDropDownOpen)
            {
                // Alt-Down?
                if (((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) && e.SystemKey == Key.Down)
                {
                    IsDropDownOpen = true;
                    e.Handled = true;
                }
            }
            else
            {
                // Alt-Up?
                if (e.Key == Key.Escape || ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) && e.SystemKey == Key.Up)
                {
                    CloseDropDown(true);
                    e.Handled = true;
                }
            }
        }


        /// <summary>
        /// Handles the <see cref="UIElement.IsKeyboardFocusWithinChanged"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // This does not work if the DropDownButton is in a FocusScope!
            // (However, it works in a toolbar.)
            //if (IsDropDownOpen && !IsKeyboardFocusWithin && !IsLogicalDescendant(this, Keyboard.FocusedElement as DependencyObject))
            //    CloseDropDown(false);
        }


        private static void OnLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            var dropDownButton = (DropDownButton)sender;

            if (dropDownButton._popup == null)
            {
                // Do nothing. Drop-down is a context menu.
                return;
            }

            if (Mouse.Captured == dropDownButton)
            {
                // Do nothing. DropDownButton still has mouse capture.
                return;
            }

            if (eventArgs.OriginalSource == dropDownButton)
            {
                // Drop-down button lost the mouse capture.
                // If the new mouse capture is within the drop-down, we're good.
                // Otherwise, close the drop down.
                if (!IsLogicalDescendant(dropDownButton, Mouse.Captured as DependencyObject))
                    dropDownButton.CloseDropDown(false);
            }
            else
            {
                if (IsLogicalDescendant(dropDownButton, eventArgs.OriginalSource as DependencyObject))
                {
                    // Child element inside the drop-down gave up the mouse capture.
                    // --> Set mouse capture back to drop-down button.
                    if (dropDownButton.IsDropDownOpen && Mouse.Captured == null)
                    {
                        Mouse.Capture(dropDownButton, CaptureMode.SubTree);
                        eventArgs.Handled = true;
                    }
                }
                else
                {
                    // Event originated outside of the drop-down.
                    dropDownButton.CloseDropDown(false);
                }
            }
        }


        private static void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs eventArgs)
        {
            var dropDownButton = (DropDownButton)sender;

            if (Mouse.Captured == dropDownButton)
            {
                // Mouse button pressed outside of drop-down.
                dropDownButton.CloseDropDown(false);
            }
        }


        private void CloseDropDown(bool focusDropDownButton)
        {
            if (IsDropDownOpen)
                IsDropDownOpen = false;

            if (focusDropDownButton)
                Button?.Focus();
        }


        private static void OnPopupOpened(object sender, EventArgs eventArgs)
        {
            var popup = (Popup)sender;

            // Focus first element in popup.
            popup.Child?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
        }


        private static void OnPopupMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            // Popup swallows all mouse clicks. If we do not do this, the popup might close when
            // the user clicks in the empty popup space.
            eventArgs.Handled = true;
        }


        private static void OnPopupContextMenuOpening(object sender, ContextMenuEventArgs eventArgs)
        {
            // If we do not set Handled, then the context menu of the drop down button,
            // will open in the popup and possible close it.
            eventArgs.Handled = true;
        }


        #region ----- Helper methods -----

        private static bool IsLogicalDescendant(DependencyObject ancestor, DependencyObject node)
        {
            Debug.Assert(ancestor != null);

            while (node != null)
            {
                if (node == ancestor)
                    return true;

                node = node.GetLogicalParent();
            }

            return false;
        }
        #endregion

        #endregion
    }


    /// <summary>
    /// Represents a button that opens a drop-down when the button is clicked. (Same as
    /// <see cref="DropDownButton"/>, but different style.)
    /// </summary>
    /// <inheritdoc/>
    [TemplatePart(Name = PART_DropDownButton, Type = typeof(ToggleButton))]
    public class ToolBarDropDownButton : DropDownButton
    {
        /// <summary>
        /// Initializes static members of the <see cref="ToolBarDropDownButton"/> class.
        /// </summary>
        static ToolBarDropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolBarDropDownButton), new FrameworkPropertyMetadata(typeof(ToolBarDropDownButton)));
        }
    }
}
