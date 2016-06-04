// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace DigitalRune.Windows.Docking
{
    /// <summary>
    /// Represents an <see cref="IDockTabItem"/> in an <see cref="Docking.AutoHideBar"/>.
    /// </summary>
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Disabled")]
    [TemplateVisualState(GroupName = "LayoutStates", Name = "Upright")]
    [TemplateVisualState(GroupName = "LayoutStates", Name = "UpsideDown")]
    public class AutoHideTab : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly DispatcherTimer _hoverTimer;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets or sets the parent <see cref="AutoHideGroup"/>.
        /// </summary>
        internal AutoHideGroup AutoHideGroup
        {
            get { return ItemsControl.ItemsControlFromItemContainer(this) as AutoHideGroup; }
        }


        /// <summary>
        /// Gets the parent <see cref="AutoHideBar"/>.
        /// </summary>
        internal AutoHideBar AutoHideBar
        {
            get
            {
                var autoHideGroup = AutoHideGroup;
                if (autoHideGroup == null)
                    return null;

                return ItemsControl.ItemsControlFromItemContainer(autoHideGroup) as AutoHideBar;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this tab should be drawn upside down.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this tab should be drawn upside down; otherwise,
        /// <see langword="false"/>.
        /// </value>
        private bool IsUpsideDown
        {
            get
            {
                var dockPosition = AutoHideBar?.Dock;
                return dockPosition == Dock.Right || dockPosition == Dock.Top;
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="AutoHideTab"/> class.
        /// </summary>
        static AutoHideTab()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoHideTab), new FrameworkPropertyMetadata(typeof(AutoHideTab)));

            // When navigating with the keyboard focus auto-hide tab only once. Don't step inside.
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(AutoHideTab), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(AutoHideTab), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AutoHideTab"/> class.
        /// </summary>
        public AutoHideTab()
        {
            _hoverTimer = new DispatcherTimer { Interval = SystemParameters.MouseHoverTime };
            _hoverTimer.Tick += OnMouseHover;
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            UpdateVisualStates(false);
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
            AutoHideBar?.TabEnter(this);
            base.OnGotFocus(e);
        }


        /// <summary>
        /// Raises the <see cref="UIElement.LostFocus"/> routed event by using the event data that
        /// is provided.
        /// </summary>
        /// <param name="e">
        /// A <see cref="RoutedEventArgs"/> that contains event data. This event data must contain
        /// the identifier for the <see cref="UIElement.LostFocus"/> event.
        /// </param>
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            AutoHideBar?.TabLeave(this);
            base.OnLostFocus(e);
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Keyboard.KeyDown</strong> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled)
            {
                if ((e.Key == Key.Enter || e.Key == Key.Space))
                    AutoHideBar?.TabClicked(this);

                e.Handled = true;
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseLeftButtonDown"/> routed event is
        /// raised on this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that the left mouse button was pressed.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods")]
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (!e.Handled)
            {
                AutoHideBar?.TabClicked(this);
                e.Handled = true;
            }
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Mouse.MouseEnter</strong> attached event is raised on
        /// this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            // Start timer to measure hover-time.
            _hoverTimer.Start();

            base.OnMouseEnter(e);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Invoked when an unhandled <strong>Mouse.MouseLeave</strong> attached event is raised on
        /// this element. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _hoverTimer.Stop();
            AutoHideBar?.TabLeave(this);

            base.OnMouseLeave(e);
            UpdateVisualStates(true);
        }


        /// <summary>
        /// Called when the mouse cursor has hovered over the tab for a certain amount of time.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        private void OnMouseHover(object sender, EventArgs eventArgs)
        {
            _hoverTimer.Stop();
            AutoHideBar?.TabEnter(this);
        }


        private void UpdateVisualStates(bool useTransitions)
        {
            if (IsEnabled)
            {
                if (IsMouseOver)
                    VisualStateManager.GoToState(this, "MouseOver", useTransitions);
                else
                    VisualStateManager.GoToState(this, "Normal", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Disabled", useTransitions);
            }

            if (IsUpsideDown)
                VisualStateManager.GoToState(this, "UpsideDown", useTransitions);
            else
                VisualStateManager.GoToState(this, "Upright", useTransitions);
        }
        #endregion
    }
}
