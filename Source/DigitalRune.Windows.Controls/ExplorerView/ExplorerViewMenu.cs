// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using DigitalRune.Mathematics;
using Image = System.Windows.Controls.Image;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a context menu for selecting the mode and scale of an <see cref="ExplorerView"/>.
    /// </summary>
    [TemplatePart(Name = "PART_ViewSlider", Type = typeof(Slider))]
    public class ExplorerViewMenu : ContextMenu
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        // The scales that correspond to the individual view modes.
        internal const double ScaleExtraLarge = 16;
        internal const double ScaleLarge = 6;
        internal const double ScaleMedium = 3;
        internal const double ScaleSmall = 1;
        internal const double ScaleList = 1;
        internal const double ScaleDetails = 1;
        internal const double ScaleTiles = 1;

        // The slider positions that correspond to the view modes.
        private const double SliderPositionExtraLarge = 222;
        private const double SliderPositionLarge = 178;
        private const double SliderPositionMedium = 148;
        private const double SliderPositionSmall = 108;
        private const double SliderPositionList = 72;
        private const double SliderPositionDetails = 36;
        private const double SliderPositionTiles = 0;
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private bool _initialized;
        private bool _updating;
        private Slider _slider;
        private Thumb _sliderThumb;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        #region ----- Customization -----

        /// <summary>
        /// Identifies the <see cref="ImageSourceExtraLargeIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceExtraLargeIconsProperty = DependencyProperty.Register(
            "ImageSourceExtraLargeIcons",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets the <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is
        /// shown in the "Extra Large Icons" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Extra Large Icons' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceExtraLargeIcons
        {
            get { return GetValue(ImageSourceExtraLargeIconsProperty); }
            set { SetValue(ImageSourceExtraLargeIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceLargeIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceLargeIconsProperty = DependencyProperty.Register(
            "ImageSourceLargeIcons",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "Large Icons" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Large Icons' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceLargeIcons
        {
            get { return GetValue(ImageSourceLargeIconsProperty); }
            set { SetValue(ImageSourceLargeIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceMediumIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceMediumIconsProperty = DependencyProperty.Register(
            "ImageSourceMediumIcons",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "Medium Icons" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Medium Icons' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceMediumIcons
        {
            get { return GetValue(ImageSourceMediumIconsProperty); }
            set { SetValue(ImageSourceMediumIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceSmallIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceSmallIconsProperty = DependencyProperty.Register(
            "ImageSourceSmallIcons",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "Small Icons" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Small Icons' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceSmallIcons
        {
            get { return GetValue(ImageSourceSmallIconsProperty); }
            set { SetValue(ImageSourceSmallIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceList"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceListProperty = DependencyProperty.Register(
            "ImageSourceList",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "List" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'List' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceList
        {
            get { return GetValue(ImageSourceListProperty); }
            set { SetValue(ImageSourceListProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceDetails"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceDetailsProperty = DependencyProperty.Register(
            "ImageSourceDetails",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "Details" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Details' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceDetails
        {
            get { return GetValue(ImageSourceDetailsProperty); }
            set { SetValue(ImageSourceDetailsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ImageSourceTiles"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceTilesProperty = DependencyProperty.Register(
            "ImageSourceTiles",
            typeof(object),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="ImageSource"/> or <see cref="MultiColorGlyph"/> that is shown in
        /// the "Tiles" menu item. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The default display size is 16 x 16 device-independent pixels.
        /// </remarks>
        [Description("Gets or sets the image that represents the 'Tiles' mode.")]
        [Category(Categories.Appearance)]
        public object ImageSourceTiles
        {
            get { return GetValue(ImageSourceTilesProperty); }
            set { SetValue(ImageSourceTilesProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringExtraLargeIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringExtraLargeIconsProperty = DependencyProperty.Register(
            "StringExtraLargeIcons",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Extra Large Icons" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Extra Large Icons' mode.")]
        [Category(Categories.Default)]
        public string StringExtraLargeIcons
        {
            get { return (string)GetValue(StringExtraLargeIconsProperty); }
            set { SetValue(StringExtraLargeIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringLargeIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringLargeIconsProperty = DependencyProperty.Register(
            "StringLargeIcons",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Large Icons" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Large Icons' mode.")]
        [Category(Categories.Default)]
        public string StringLargeIcons
        {
            get { return (string)GetValue(StringLargeIconsProperty); }
            set { SetValue(StringLargeIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringMediumIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringMediumIconsProperty = DependencyProperty.Register(
            "StringMediumIcons",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Medium Icons" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Medium Icons' mode.")]
        [Category(Categories.Default)]
        public string StringMediumIcons
        {
            get { return (string)GetValue(StringMediumIconsProperty); }
            set { SetValue(StringMediumIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringSmallIcons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringSmallIconsProperty = DependencyProperty.Register(
            "StringSmallIcons",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Small Icons" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Small Icons' mode.")]
        [Category(Categories.Default)]
        public string StringSmallIcons
        {
            get { return (string)GetValue(StringSmallIconsProperty); }
            set { SetValue(StringSmallIconsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringDetails"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringDetailsProperty = DependencyProperty.Register(
            "StringDetails",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Details" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Details' mode.")]
        [Category(Categories.Default)]
        public string StringDetails
        {
            get { return (string)GetValue(StringDetailsProperty); }
            set { SetValue(StringDetailsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringList"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringListProperty = DependencyProperty.Register(
            "StringList",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "List" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'List' mode.")]
        [Category(Categories.Default)]
        public string StringList
        {
            get { return (string)GetValue(StringListProperty); }
            set { SetValue(StringListProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringTiles"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StringTilesProperty = DependencyProperty.Register(
            "StringTiles",
            typeof(string),
            typeof(ExplorerViewMenu));

        /// <summary>
        /// Gets or sets <see cref="string"/> that is shown in the "Tiles" menu item. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the name of the 'Tiles' mode.")]
        [Category(Categories.Default)]
        public string StringTiles
        {
            get { return (string)GetValue(StringTilesProperty); }
            set { SetValue(StringTilesProperty, value); }
        }
        #endregion


        private static readonly DependencyPropertyKey IconPropertyKey = DependencyProperty.RegisterReadOnly(
            "Icon",
            typeof(object),
            typeof(ExplorerViewMenu),
            new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty = IconPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the icon (<see cref="ImageSource"/> or <see cref="MultiColorGlyph"/>) representing
        /// the current view mode. This is a dependency property.
        /// </summary>
        [Description("Gets the image that represents the current view mode.")]
        [Category(Categories.Appearance)]
        public object Icon
        {
            get { return GetValue(IconProperty); }
            private set { SetValue(IconPropertyKey, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Mode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty = ExplorerView.ModeProperty.AddOwner(
            typeof(ExplorerViewMenu),
            new FrameworkPropertyMetadata(ExplorerViewMode.Details, FrameworkPropertyMetadataOptions.None, OnModeChanged));


        /// <summary>
        /// Gets or sets the mode of the <see cref="ExplorerView"/>. 
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The current <see cref="ExplorerViewMode"/>. The default value is 
        /// <see cref="ExplorerViewMode.Details"/>.
        /// </value>
        [Description("Gets or sets current view mode.")]
        [Category(Categories.Default)]
        public ExplorerViewMode Mode
        {
            get { return (ExplorerViewMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Scale"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScaleProperty = ExplorerView.ScaleProperty.AddOwner(
            typeof(ExplorerViewMenu),
            new FrameworkPropertyMetadata(ScaleDetails, FrameworkPropertyMetadataOptions.None, OnScaleChanged));


        /// <summary>
        /// Gets or sets the scale used to display the icons in the <see cref="ExplorerView"/>. 
        /// This is a dependency property.
        /// </summary>
        /// <value>A value in the range [1, 16]. The default value is 1.</value>
        /// <remarks>
        /// <para>
        /// This property is only relevant when the <see cref="Mode"/> is set to 
        /// <see cref="ExplorerViewMode.SmallIcons"/>, <see cref="ExplorerViewMode.MediumIcons"/>, 
        /// <see cref="ExplorerViewMode.LargeIcons"/>, or <see cref="ExplorerViewMode.ExtraLargeIcons"/>.
        /// </para>
        /// <para>
        /// When <see cref="Scale"/> is set to a value greater than 1 the property <see cref="Mode"/> is 
        /// automatically set to the appropriate mode (as listed in the previous paragraph).
        /// </para>
        /// </remarks>
        [Description("Gets or sets the scale of the current view [1, 16].")]
        [Category(Categories.Default)]
        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SliderPosition"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SliderPositionProperty = DependencyProperty.Register(
            "SliderPosition",
            typeof(double),
            typeof(ExplorerViewMenu),
            new FrameworkPropertyMetadata(SliderPositionMedium, FrameworkPropertyMetadataOptions.None, OnSliderPositionChanged));


        /// <summary>
        /// Gets or sets the position of the slider. 
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value in the range [0, 222]. The default value is 148. 
        /// (0 = Tiles, 36 = Details, 72 = List, 108 = Small Icons, 
        /// 148 = Medium Icons, 178 = Large Icons, 222 = Extra Large Icons)
        /// </value>
        [Description("Gets or sets the position of the slider.")]
        [Category(Categories.Default)]
        public double SliderPosition
        {
            get { return (double)GetValue(SliderPositionProperty); }
            set { SetValue(SliderPositionProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ExplorerViewMenu"/> class.
        /// </summary>
        static ExplorerViewMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExplorerViewMenu), new FrameworkPropertyMetadata(typeof(ExplorerViewMenu)));
            EventManager.RegisterClassHandler(typeof(ExplorerViewMenu), MenuItem.ClickEvent, new RoutedEventHandler(OnMenuItemClicked));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked whenever 
        /// <see cref="FrameworkElement.IsInitialized"/> is set to <see langword="true"/> internally.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var extraLargeIconsMenuItem = new MenuItem { Height = 24, Margin = new Thickness(0, 3, 0, 20), DataContext = ExplorerViewMode.ExtraLargeIcons };
            var largeIconsMenuItem = new MenuItem { Height = 24, Margin = new Thickness(0, 0, 0, 6), DataContext = ExplorerViewMode.LargeIcons };
            var mediumIconsMenuItem = new MenuItem { Height = 24, Margin = new Thickness(0, 0, 0, 16), DataContext = ExplorerViewMode.MediumIcons };
            var smallIconsMenuItem = new MenuItem { Height = 24, Margin = new Thickness(0, 0, 0, 0), DataContext = ExplorerViewMode.SmallIcons };
            var listMenuItem = new MenuItem { Height = 24, DataContext = ExplorerViewMode.List };
            var detailsMenuItem = new MenuItem { Height = 24, DataContext = ExplorerViewMode.Details };
            var tilesMenuItem = new MenuItem { Height = 24, Margin = new Thickness(0, 0, 0, 3), DataContext = ExplorerViewMode.Tiles };

            // Bind menu icons to the ImageSource provided by this control.
            BindMenuIcon(extraLargeIconsMenuItem, ImageSourceExtraLargeIconsProperty);
            BindMenuIcon(largeIconsMenuItem, ImageSourceLargeIconsProperty);
            BindMenuIcon(mediumIconsMenuItem, ImageSourceMediumIconsProperty);
            BindMenuIcon(smallIconsMenuItem, ImageSourceSmallIconsProperty);
            BindMenuIcon(listMenuItem, ImageSourceListProperty);
            BindMenuIcon(detailsMenuItem, ImageSourceDetailsProperty);
            BindMenuIcon(tilesMenuItem, ImageSourceTilesProperty);

            // Bind menu text to the strings provided by this control.
            BindMenuText(extraLargeIconsMenuItem, StringExtraLargeIconsProperty);
            BindMenuText(largeIconsMenuItem, StringLargeIconsProperty);
            BindMenuText(mediumIconsMenuItem, StringMediumIconsProperty);
            BindMenuText(smallIconsMenuItem, StringSmallIconsProperty);
            BindMenuText(listMenuItem, StringListProperty);
            BindMenuText(detailsMenuItem, StringDetailsProperty);
            BindMenuText(tilesMenuItem, StringTilesProperty);

            ItemsSource = new FrameworkElement[]
            {
                extraLargeIconsMenuItem,
                largeIconsMenuItem,
                mediumIconsMenuItem,
                smallIconsMenuItem,
                new Separator { Height = 1, Margin = new Thickness(-30, 6, 0, 5) },
                listMenuItem,
                new Separator { Height = 1, Margin = new Thickness(-30, 6, 0, 5) },
                detailsMenuItem,
                new Separator { Height = 1, Margin = new Thickness(-30, 6, 0, 5) },
                tilesMenuItem
            };
        }


        private void BindMenuIcon(MenuItem menuItem, DependencyProperty property)
        {
            var image = new Image { Width = 16, Height = 16 };
            var binding = new Binding { Source = this, Path = new PropertyPath(property) };
            image.SetBinding(Image.SourceProperty, binding);
            menuItem.Icon = image;
        }


        private void BindMenuText(MenuItem menuItem, DependencyProperty property)
        {
            var binding = new Binding { Source = this, Path = new PropertyPath(property) };
            menuItem.SetBinding(HeaderedItemsControl.HeaderProperty, binding);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            DetachFromVisualTree();
            AttachToVisualTree();
            base.OnApplyTemplate();
        }


        private void AttachToVisualTree()
        {
            _slider = GetTemplateChild("PART_ViewSlider") as Slider;
            if (_slider != null)
            {
                _sliderThumb = _slider.GetVisualDescendants(false).OfType<Thumb>().FirstOrDefault();
                if (_sliderThumb != null)
                {
                    _sliderThumb.MouseEnter += CaptureMouseToThumb;
                    _sliderThumb.LostMouseCapture += OnThumbLostMouseCapture;
                }
            }
        }


        private void DetachFromVisualTree()
        {
            if (_sliderThumb != null)
            {
                _sliderThumb.MouseEnter -= CaptureMouseToThumb;
                _sliderThumb.LostMouseCapture -= OnThumbLostMouseCapture;
            }
        }


        /// <summary>
        /// Captures the mouse to the thumb if the track is clicked somewhere other than the thumb.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        private static void CaptureMouseToThumb(object sender, MouseEventArgs eventArgs)
        {
            // When the left mouse button is pressed on the thumb, the thumb automatically captures
            // the mouse.
            // But when the left mouse button is pressed somewhere else on the slider, the mouse is
            // not captured by default. But we want the mouse to be captured. Therefore, we have set
            // up this event handler for Thumb.MouseEnter. The user presses the left mouse button,
            // the thumbs snaps to this position and Thumb.MouseEnter event is raised.
            var thumb = sender as Thumb;
            if (eventArgs.LeftButton == MouseButtonState.Pressed && thumb != null && eventArgs.MouseDevice.Captured != thumb)
            {
                // The left mouse button is pressed and the thumb does not have the mouse captured.
                // --> Generate a MouseLeftButtonDown event. The thumb will then capture the mouse.
                var args = new MouseButtonEventArgs(eventArgs.MouseDevice, eventArgs.Timestamp, MouseButton.Left);
                args.RoutedEvent = MouseLeftButtonDownEvent;
                thumb.RaiseEvent(args);
            }
        }


        /// <summary>
        /// Called when the thumb of the slider releases/loses the mouse capture.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        private void OnThumbLostMouseCapture(object sender, MouseEventArgs eventArgs)
        {
            // Close the context menu when the thumb loses the mouse capture.
            IsOpen = false;
        }


        /// <summary>
        /// Called when the property <see cref="ItemsControl.Items"/> has changed.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!_initialized)
            {
                UpdateFromMode(Mode);
                _initialized = true;
            }

            base.OnItemsChanged(e);
        }


        /// <summary>
        /// Called when the property <see cref="Scale"/> has changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnScaleChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var explorerViewMenu = (ExplorerViewMenu)dependencyObject;
            explorerViewMenu.UpdateFromScale((double)eventArgs.NewValue);
        }


        /// <summary>
        /// Called when property <see cref="SliderPosition"/> has changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnSliderPositionChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var explorerViewMenu = (ExplorerViewMenu)dependencyObject;
            explorerViewMenu.UpdateFromSlider((double)eventArgs.NewValue);
        }


        /// <summary>
        /// Called when the property <see cref="Mode"/> has changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var explorerViewMenu = (ExplorerViewMenu)dependencyObject;
            explorerViewMenu.UpdateFromMode((ExplorerViewMode)eventArgs.NewValue);
        }


        /// <summary>
        /// Raises the <see cref="ContextMenu.Opened"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnOpened(RoutedEventArgs e)
        {
            if (_sliderThumb != null)
            {
                Point centerOfThumb = new Point(_sliderThumb.DesiredSize.Width / 2, _sliderThumb.DesiredSize.Height / 2);
                Point offsetToThumb = _sliderThumb.TranslatePoint(centerOfThumb, this);
                Placement = PlacementMode.MousePoint;
                HorizontalOffset = -offsetToThumb.X;
                VerticalOffset = -offsetToThumb.Y;
            }

            base.OnOpened(e);
        }


        /// <summary>
        /// Called when a menu item is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMenuItemClicked(object sender, RoutedEventArgs eventArgs)
        {
            var explorerViewMenu = (ExplorerViewMenu)sender;
            if (explorerViewMenu != null)
            {
                var menuItem = eventArgs.OriginalSource as MenuItem;
                if (menuItem?.DataContext is ExplorerViewMode)
                {
                    explorerViewMenu.Mode = (ExplorerViewMode)menuItem.DataContext;
                    eventArgs.Handled = true;
                }
            }
        }


        private void UpdateFromScale(double scale)
        {
            // Avoid re-entrance.
            if (_updating)
                return;

            _updating = true;

            try
            {
                SetSliderPosition(scale);
                SetMode(SliderPosition);
                SetIcon(Mode);
            }
            finally
            {
                _updating = false;
            }
        }


        private void UpdateFromSlider(double sliderPosition)
        {
            // Avoid re-entrance.
            if (_updating)
                return;

            _updating = true;

            try
            {
                SetScale(sliderPosition);
                SetMode(sliderPosition);
                SetIcon(Mode);
            }
            finally
            {
                _updating = false;
            }
        }


        private void UpdateFromMode(ExplorerViewMode mode)
        {
            // Avoid re-entrance.
            if (_updating)
                return;

            _updating = true;

            try
            {
                SetScale(mode);
                SetSliderPosition(mode);
                SetIcon(mode);
            }
            finally
            {
                _updating = false;
            }
        }


        private void SetIcon(ExplorerViewMode mode)
        {
            switch (mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                    Icon = ImageSourceExtraLargeIcons;
                    break;
                case ExplorerViewMode.LargeIcons:
                    Icon = ImageSourceLargeIcons;
                    break;
                case ExplorerViewMode.MediumIcons:
                    Icon = ImageSourceMediumIcons;
                    break;
                case ExplorerViewMode.SmallIcons:
                    Icon = ImageSourceSmallIcons;
                    break;
                case ExplorerViewMode.List:
                    Icon = ImageSourceList;
                    break;
                case ExplorerViewMode.Details:
                    Icon = ImageSourceDetails;
                    break;
                default:
                    Icon = ImageSourceTiles;
                    break;
            }
        }


        private void SetScale(double sliderPosition)
        {
            if (Numeric.IsLessOrEqual(sliderPosition, SliderPositionSmall))
                Scale = 1.0;
            else if (Numeric.IsLess(sliderPosition, SliderPositionMedium))
                Scale = ((sliderPosition - 116) / 4) * 0.25 + 1.25;
            else if (Numeric.AreEqual(sliderPosition, SliderPositionMedium))
                Scale = 3;
            else if (Numeric.IsLess(sliderPosition, SliderPositionLarge))
                Scale = ((sliderPosition - 155) / 4) * 0.5 + 3.5;
            else if (Numeric.AreEqual(sliderPosition, SliderPositionLarge))
                Scale = 6;
            else if (Numeric.IsLess(sliderPosition, SliderPositionExtraLarge))
                Scale = ((sliderPosition - 184) / 4) + 7;
            else
                Scale = 16;
        }


        private void SetScale(ExplorerViewMode mode)
        {
            switch (mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                    Scale = ScaleExtraLarge;
                    break;
                case ExplorerViewMode.LargeIcons:
                    Scale = ScaleLarge;
                    break;
                case ExplorerViewMode.MediumIcons:
                    Scale = ScaleMedium;
                    break;
                case ExplorerViewMode.SmallIcons:
                    Scale = ScaleSmall;
                    break;
                default:
                    // Do not set scale in other cases, 
                    // because those modes do not affect the scale.
                    break;
            }
        }


        private void SetSliderPosition(double scale)
        {
            if (Numeric.IsGreater(scale, ScaleSmall))
            {
                // When scale > 1.0 then update the slider.
                if (Numeric.IsLessOrEqual(scale, ScaleSmall))
                    SliderPosition = SliderPositionSmall;
                else if (Numeric.IsLess(scale, ScaleMedium))
                    SliderPosition = (scale - 1.25) / 0.25 * 4 + 116;
                else if (Numeric.AreEqual(scale, ScaleMedium))
                    SliderPosition = SliderPositionMedium;
                else if (Numeric.IsLess(scale, ScaleLarge))
                    SliderPosition = (scale - 3.5) / 0.5 * 4 + 155;
                else if (Numeric.AreEqual(scale, ScaleLarge))
                    SliderPosition = SliderPositionLarge;
                else if (Numeric.IsLess(scale, ScaleExtraLarge))
                    SliderPosition = (scale - 7) * 4 + 184;
                else
                    SliderPosition = SliderPositionExtraLarge;
            }
        }


        private void SetSliderPosition(ExplorerViewMode mode)
        {
            switch (mode)
            {
                case ExplorerViewMode.ExtraLargeIcons:
                    SliderPosition = SliderPositionExtraLarge;
                    break;
                case ExplorerViewMode.LargeIcons:
                    SliderPosition = SliderPositionLarge;
                    break;
                case ExplorerViewMode.MediumIcons:
                    SliderPosition = SliderPositionMedium;
                    break;
                case ExplorerViewMode.SmallIcons:
                    SliderPosition = SliderPositionSmall;
                    break;
                case ExplorerViewMode.List:
                    SliderPosition = SliderPositionList;
                    break;
                case ExplorerViewMode.Details:
                    SliderPosition = SliderPositionDetails;
                    break;
                default:
                    SliderPosition = SliderPositionTiles;
                    break;
            }
        }


        private void SetMode(double sliderPosition)
        {
            if (Numeric.IsLess(sliderPosition, SliderPositionDetails))
                Mode = ExplorerViewMode.Tiles;
            else if (Numeric.IsLess(sliderPosition, SliderPositionList))
                Mode = ExplorerViewMode.Details;
            else if (Numeric.IsLess(sliderPosition, SliderPositionSmall))
                Mode = ExplorerViewMode.List;
            else if (Numeric.IsLess(sliderPosition, SliderPositionMedium))
                Mode = ExplorerViewMode.SmallIcons;
            else if (Numeric.IsLess(sliderPosition, SliderPositionLarge))
                Mode = ExplorerViewMode.MediumIcons;
            else if (Numeric.IsLess(sliderPosition, SliderPositionExtraLarge))
                Mode = ExplorerViewMode.LargeIcons;
            else
                Mode = ExplorerViewMode.ExtraLargeIcons;
        }
        #endregion
    }
}
