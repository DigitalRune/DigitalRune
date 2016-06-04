// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DigitalRune.Windows.Interop;
using SystemCommands = System.Windows.SystemCommands;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represent a control that renders the Minimize/Maximize/Close buttons of a window.
    /// </summary>
    [StyleTypedProperty(Property = "CloseButtonStyle", StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = "MaximizeButtonStyle", StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = "MinimizeButtonStyle", StyleTargetType = typeof(Button))]
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MaximizeButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_MinimizeButton", Type = typeof(Button))]
    public class WindowCaptionButtons : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private SafeLibraryHandle _user32;
        private Button _closeButton;
        private Button _maximizeButton;
        private Button _minimizeButton;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// Gets the caption (= tooltip text) of the Close button.
        /// </summary>
        /// <value>The caption (= tooltip text) of the Close button.</value>
        public string CloseCaption
        {
            get
            {
                if (string.IsNullOrEmpty(_closeCaption))
                    _closeCaption = GetCaption(905);

                return _closeCaption;
            }
        }
        private static string _closeCaption;


        /// <summary>
        /// Gets the caption (= tooltip text) of the Maximize button.
        /// </summary>
        /// <value>The caption (= tooltip text) of the Maximize button.</value>
        public string MaximizeCaption
        {
            get
            {
                if (string.IsNullOrEmpty(_maximizeCaption))
                    _maximizeCaption = GetCaption(901);

                return _maximizeCaption;
            }
        }
        private static string _maximizeCaption;


        /// <summary>
        /// Gets the caption (= tooltip text) of the Minimize button.
        /// </summary>
        /// <value>The caption (= tooltip text) of the Minimize button.</value>
        public string MinimizeCaption
        {
            get
            {
                if (string.IsNullOrEmpty(_minimizeCaption))
                    _minimizeCaption = GetCaption(900);

                return _minimizeCaption;
            }
        }
        private static string _minimizeCaption;



        /// <summary>
        /// Gets the caption (= tooltip text) of the Restore button.
        /// </summary>
        /// <value>The caption (= tooltip text) of the Restore button.</value>
        public string RestoreCaption
        {
            get
            {
                if (string.IsNullOrEmpty(_restoreCaption))
                    _restoreCaption = GetCaption(903);

                return _restoreCaption;
            }
        }
        private static string _restoreCaption;
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ShowCloseButton"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register(
            "ShowCloseButton",
            typeof(bool),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether the Close button is shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the Close button; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the Close button is shown.")]
        [Category(Categories.Appearance)]
        public bool ShowCloseButton
        {
            get { return (bool)GetValue(ShowCloseButtonProperty); }
            set { SetValue(ShowCloseButtonProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="ShowMaximizeButton"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowMaximizeButtonProperty = DependencyProperty.Register(
            "ShowMaximizeButton",
            typeof(bool),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether the Maximize button is shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the Maximize button; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the Maximize button is shown.")]
        [Category(Categories.Appearance)]
        public bool ShowMaximizeButton
        {
            get { return (bool)GetValue(ShowMaximizeButtonProperty); }
            set { SetValue(ShowMaximizeButtonProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="ShowMinimizeButton"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowMinimizeButtonProperty = DependencyProperty.Register(
            "ShowMinimizeButton",
            typeof(bool),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether the Minimize button is shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to show the Minimize button; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the Minimize button is shown.")]
        [Category(Categories.Appearance)]
        public bool ShowMinimizeButton
        {
            get { return (bool)GetValue(ShowMinimizeButtonProperty); }
            set { SetValue(ShowMinimizeButtonProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="CloseButtonStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseButtonStyleProperty = DependencyProperty.Register(
            "CloseButtonStyle",
            typeof(Style),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style of the Close button.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style of the Close button.</value>
        [Description("Gets or sets the style of the Close button.")]
        [Category(Categories.Default)]
        public Style CloseButtonStyle
        {
            get { return (Style)GetValue(CloseButtonStyleProperty); }
            set { SetValue(CloseButtonStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MaximizeButtonStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximizeButtonStyleProperty = DependencyProperty.Register(
            "MaximizeButtonStyle",
            typeof(Style),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style of the Maximize button.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style of the Maximize button.</value>
        [Description("Gets or sets the style of the Maximize button.")]
        [Category(Categories.Default)]
        public Style MaximizeButtonStyle
        {
            get { return (Style)GetValue(MaximizeButtonStyleProperty); }
            set { SetValue(MaximizeButtonStyleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinimizeButtonStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimizeButtonStyleProperty = DependencyProperty.Register(
            "MinimizeButtonStyle",
            typeof(Style),
            typeof(WindowCaptionButtons),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the style of the Minimize button.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style of the Minimize button.</value>
        [Description("Gets or sets the style of the Minimize button.")]
        [Category(Categories.Default)]
        public Style MinimizeButtonStyle
        {
            get { return (Style)GetValue(MinimizeButtonStyleProperty); }
            set { SetValue(MinimizeButtonStyleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="WindowCaptionButtons"/> class.
        /// </summary>
        static WindowCaptionButtons()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowCaptionButtons), new FrameworkPropertyMetadata(typeof(WindowCaptionButtons)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private string GetCaption(uint id)
        {
            if (_user32 == null)
                _user32 = Win32.LoadLibrary(Environment.SystemDirectory + "\\User32.dll");

            var sb = new StringBuilder(256);
            if (Win32.LoadString(_user32, id, sb, sb.Capacity) >= 0)
                return sb.ToString().Replace("&", "");

            return string.Empty;
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _closeButton = GetTemplateChild("PART_CloseButton") as Button;
            _maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
            _minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;

            if (_closeButton != null)
                _closeButton.Click += OnCloseButtonClicked;
            if (_maximizeButton != null)
                _maximizeButton.Click += OnMaximizeButtonClicked;
            if (_minimizeButton != null)
                _minimizeButton.Click += OnMinimizeButtonClicked;
        }


        private void OnCloseButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            SystemCommands.CloseWindow(window);
        }


        private void OnMaximizeButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            if (window.WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(window);
            else
                SystemCommands.MaximizeWindow(window);
        }


        private void OnMinimizeButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            var window = Window.GetWindow(this);
            if (window == null)
                return;

            SystemCommands.MinimizeWindow(window);
        }
        #endregion
    }
}
