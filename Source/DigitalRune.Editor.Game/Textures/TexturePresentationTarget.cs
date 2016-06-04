// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using DigitalRune.Editor.Game;
using DigitalRune.Graphics;
using DigitalRune.Graphics.Interop;
using DigitalRune.Mathematics;
using DigitalRune.Mathematics.Algebra;
using DigitalRune.Windows;
using Microsoft.Xna.Framework.Graphics;


namespace DigitalRune.Editor.Textures
{
    internal class TexturePresentationTarget : GamePresentationTarget, IScrollInfo
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private const double LineSize = 10.0;           // ScrollBar button scrolls 10 px.
        private const double MouseWheelZoom = 1.25;     // Zoom factor per mouse wheel increment.
        private static readonly double[] ZoomLevels =
        {
            0.01, 0.02, 0.03,
            1.0 / 25.0, 1.0 / 20.0, 1.0 / 16.0, 1.0 / 12.0,
            1.0 / 8.0, 1.0 / 6.0, 1.0 / 4.0, 1.0 / 3.0, 1.0 / 2.0, 2.0 / 3.0,
            1.0, 1.5, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0,
            10, 15, 20, 30, 40, 50, 60, 70, 80, 90, 100,
        };

        private TextureGraphicsScreen _textureGraphicsScreen;

        private ScrollViewer _scrollOwner;
        private bool _inUpdateScrollInfo;
        private Vector _viewport;           // Viewport size set in ArrangeOverride().
        private Vector _extent;             // Virtual extent.
        private Vector _margin;             // Virtual space around the texture.
        private Vector _offset;             // Scroll offset.
        private double _zoom = 1.0;         // Zoom factor.
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #region ----- Commands -----

        /// <summary>
        /// Gets the value that represents the <strong>Fit Screen</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Fit Screen".
        /// </value>
        public static RoutedUICommand FitScreen
        {
            get
            {
                if (_fitScreen == null)
                    _fitScreen = new RoutedUICommand("Fit Screen", "FitScreen", typeof(TexturePresentationTarget));

                return _fitScreen;
            }
        }
        private static RoutedUICommand _fitScreen;


        /// <summary>
        /// Gets the value that represents the <strong>Fill Screen</strong> command.
        /// </summary>
        /// <value>
        /// The command. There is no default key gesture. The default UI text is "Fill Screen".
        /// </value>
        public static RoutedUICommand FillScreen
        {
            get
            {
                if (_fillScreen == null)
                    _fillScreen = new RoutedUICommand("Fill Screen", "FillScreen", typeof(TexturePresentationTarget));

                return _fillScreen;
            }
        }
        private static RoutedUICommand _fillScreen;
        #endregion


        #region ----- IScrollInfo -----

        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the vertical axis is possible.
        /// </summary>
        bool IScrollInfo.CanVerticallyScroll { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.
        /// </summary>
        bool IScrollInfo.CanHorizontallyScroll { get; set; }


        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        double IScrollInfo.ExtentWidth
        {
            get { return _extent.X; }
        }


        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        double IScrollInfo.ExtentHeight
        {
            get { return _extent.Y; }
        }


        /// <summary>
        /// Gets the horizontal size of the viewport for this content.
        /// </summary>
        double IScrollInfo.ViewportWidth
        {
            get { return _viewport.X; }
        }


        /// <summary>
        /// Gets the vertical size of the viewport for this content.
        /// </summary>
        double IScrollInfo.ViewportHeight
        {
            get { return _viewport.Y; }
        }


        /// <summary>
        /// Gets the horizontal offset of the scrolled content.
        /// </summary>
        double IScrollInfo.HorizontalOffset
        {
            get { return _offset.X; }
        }


        /// <summary>
        /// Gets the vertical offset of the scrolled content.
        /// </summary>
        double IScrollInfo.VerticalOffset
        {
            get { return _offset.Y; }
        }


        /// <summary>
        /// Gets or sets a <see cref="ScrollViewer" /> element that controls scrolling behavior.
        /// </summary>
        ScrollViewer IScrollInfo.ScrollOwner
        {
            get { return _scrollOwner; }
            set { _scrollOwner = value; }
        }
        #endregion

        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Texture2D"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty Texture2DProperty = DependencyProperty.Register(
            "Texture2D",
            typeof(Texture2D),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(null, OnTexture2DChanged));

        /// <summary>
        /// Gets or sets the texture.
        /// This is a dependency property.
        /// </summary>
        /// <value>The texture.</value>
        [Description("Gets or sets the texture.")]
        [Category(Categories.Default)]
        public Texture2D Texture2D
        {
            get { return (Texture2D)GetValue(Texture2DProperty); }
            set { SetValue(Texture2DProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="HorizontalOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            "HorizontalOffset",
            typeof(double),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnHorizontalOffsetChanged));

        /// <summary>
        /// Gets or sets the horizontal offset.
        /// This is a dependency property.
        /// </summary>
        /// <value>The horizontal offset.</value>
        [Description("Gets or sets the horizontal offset.")]
        [Category(Categories.Layout)]
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            set { SetValue(HorizontalOffsetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="VerticalOffset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset",
            typeof(double),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(Boxed.DoubleZero, OnVerticalOffsetChanged));

        /// <summary>
        /// Gets or sets the vertical offset.
        /// This is a dependency property.
        /// </summary>
        /// <value>The vertical offset.</value>
        [Description("Gets or sets the vertical offset.")]
        [Category(Categories.Layout)]
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            set { SetValue(VerticalOffsetProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MaxZoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
            "MaxZoom",
            typeof(double),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(100.0, (s, e) => s.CoerceValue(ZoomProperty)));

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The maximum zoom level. The default value is 100.0.</value>
        [Description("Gets or sets the maximum zoom level.")]
        [Category(Categories.Layout)]
        public double MaxZoom
        {
            get { return (double)GetValue(MaxZoomProperty); }
            set { SetValue(MaxZoomProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="MinZoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
            "MinZoom",
            typeof(double),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(0.01, (s, e) => s.CoerceValue(ZoomProperty)));

        /// <summary>
        /// Gets or sets the minimal zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The minimal zoom level. The default value is 0.01.</value>
        [Description("Gets or sets the minimal zoom level.")]
        [Category(Categories.Layout)]
        public double MinZoom
        {
            get { return (double)GetValue(MinZoomProperty); }
            set { SetValue(MinZoomProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Zoom"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
            "Zoom",
            typeof(double),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(
                Boxed.DoubleOne,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnZoomChanged,
                CoerceZoom));


        /// <summary>
        /// Gets or sets the zoom level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The zoom level.</value>
        [Description("Gets or sets the zoom level.")]
        [Category(Categories.Layout)]
        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="EnableRedChannel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableRedChannelProperty = DependencyProperty.Register(
            "EnableRedChannel",
            typeof(bool),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the red channel is rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if red channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the red channel is rendered.")]
        [Category(Categories.Appearance)]
        public bool EnableRedChannel
        {
            get { return (bool)GetValue(EnableRedChannelProperty); }
            set { SetValue(EnableRedChannelProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="EnableGreenChannel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableGreenChannelProperty = DependencyProperty.Register(
            "EnableGreenChannel",
            typeof(bool),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the green channel is rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if green channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the green channel is rendered.")]
        [Category(Categories.Appearance)]
        public bool EnableGreenChannel
        {
            get { return (bool)GetValue(EnableGreenChannelProperty); }
            set { SetValue(EnableGreenChannelProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="EnableBlueChannel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableBlueChannelProperty = DependencyProperty.Register(
            "EnableBlueChannel",
            typeof(bool),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the blue channel is rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if blue channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the blue channel is rendered.")]
        [Category(Categories.Appearance)]
        public bool EnableBlueChannel
        {
            get { return (bool)GetValue(EnableBlueChannelProperty); }
            set { SetValue(EnableBlueChannelProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="EnableAlphaChannel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnableAlphaChannelProperty = DependencyProperty.Register(
          "EnableAlphaChannel",
          typeof(bool),
          typeof(TexturePresentationTarget),
          new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the alpha channel is rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if alpha channel is rendered; otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the alpha channel is rendered.")]
        [Category(Categories.Appearance)]
        public bool EnableAlphaChannel
        {
            get { return (bool)GetValue(EnableAlphaChannelProperty); }
            set { SetValue(EnableAlphaChannelProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="IsPremultiplied"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPremultipliedProperty = DependencyProperty.Register(
          "IsPremultiplied",
          typeof(bool),
          typeof(TexturePresentationTarget),
          new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the texture uses premultiplied alpha. This is a
        /// dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/>
        /// if the texture uses premultiplied alpha; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the texture uses premultiplied alpha.")]
        [Category(Categories.Appearance)]
        public bool IsPremultiplied
        {
            get { return (bool)GetValue(IsPremultipliedProperty); }
            set { SetValue(IsPremultipliedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="MipLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MipLevelProperty = DependencyProperty.Register(
            "MipLevel",
            typeof(float),
            typeof(TexturePresentationTarget),
            new FrameworkPropertyMetadata(0.0f, OnGraphicsPropertyChanged));

        /// <summary>
        /// Gets or sets the mip map level.
        /// This is a dependency property.
        /// </summary>
        /// <value>The mip map level.</value>
        [Description("Gets or sets the mip map level.")]
        [Category(Categories.Appearance)]
        public float MipLevel
        {
            get { return (float)GetValue(MipLevelProperty); }
            set { SetValue(MipLevelProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="TexturePresentationTarget"/> class.
        /// </summary>
        public TexturePresentationTarget()
        {
            if (!WindowsHelper.IsInDesignMode)
            {
                Loaded += OnLoaded;
                Unloaded += OnUnloaded;

                CommandBindings.Add(new CommandBinding(NavigationCommands.IncreaseZoom, IncreaseZoom, CanIncreaseZoom));
                CommandBindings.Add(new CommandBinding(NavigationCommands.DecreaseZoom, DecreaseZoom, CanDecreaseZoom));
                CommandBindings.Add(new CommandBinding(NavigationCommands.Zoom, SetZoom));
                CommandBindings.Add(new CommandBinding(FitScreen, FitScreenExecuted));
                CommandBindings.Add(new CommandBinding(FillScreen, FillScreenExecuted));
            }
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="Texture2D"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnTexture2DChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (TexturePresentationTarget)dependencyObject;
            Texture2D oldValue = (Texture2D)eventArgs.OldValue;
            Texture2D newValue = (Texture2D)eventArgs.NewValue;
            element.OnTexture2DChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="Texture2D"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnTexture2DChanged(Texture2D oldValue, Texture2D newValue)
        {
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, true);
        }


        /// <summary>
        /// Called when the <see cref="HorizontalOffset"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnHorizontalOffsetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (TexturePresentationTarget)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            element.OnHorizontalOffsetChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="HorizontalOffset"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnHorizontalOffsetChanged(double oldValue, double newValue)
        {
            _offset.X = newValue;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        /// <summary>
        /// Called when the <see cref="VerticalOffset"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnVerticalOffsetChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (TexturePresentationTarget)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            target.OnVerticalOffsetChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="VerticalOffset"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnVerticalOffsetChanged(double oldValue, double newValue)
        {
            _offset.Y = newValue;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        /// <summary>
        /// Called when the <see cref="Zoom"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnZoomChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (TexturePresentationTarget)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            element.OnZoomChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="Zoom"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnZoomChanged(double oldValue, double newValue)
        {
            UpdateScrollInfo(_viewport, newValue, _viewport / 2.0, false);
        }


        private static object CoerceZoom(DependencyObject dependencyObject, object baseValue)
        {
            var element = (TexturePresentationTarget)dependencyObject;
            return element.CoerceZoom((double)baseValue);
        }


        private double CoerceZoom(double zoom)
        {
            return MathHelper.Clamp(zoom, MinZoom, MaxZoom);
        }


        /// <summary>
        /// Called when a property is changed which influences the rendering.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnGraphicsPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (TexturePresentationTarget)dependencyObject;
            target.UpdateGraphicsScreen();
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            if (GraphicsService == null)
                return;

            _textureGraphicsScreen = new TextureGraphicsScreen(GraphicsService);
            GraphicsScreens = new GraphicsScreen[] { _textureGraphicsScreen };
            UpdateGraphicsScreen();
        }


        private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
        {
            _textureGraphicsScreen.SafeDispose();
            _textureGraphicsScreen = null;
            GraphicsScreens = null;
        }


        /// <summary>
        /// Arranges and sizes an image control.
        /// </summary>
        /// <param name="arrangeSize">The size used to arrange the control.</param>
        /// <returns>The size of the control.</returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            Vector newViewport = (Vector)arrangeSize;
            if (_viewport != newViewport)
                UpdateScrollInfo(newViewport, _zoom, _viewport / 2.0, false);

            return base.ArrangeOverride(arrangeSize);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="Mouse.MouseWheelEvent" /> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="MouseWheelEventArgs" /> that contains the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            const double mouseWheelDeltaForOneLine = Mouse.MouseWheelDeltaForOneLine;
            Point mousePosition = e.GetPosition(this);
            double increments = e.Delta / mouseWheelDeltaForOneLine;
            double zoom = _zoom * Math.Pow(MouseWheelZoom, increments);
            UpdateScrollInfo(_viewport, zoom, new Vector(mousePosition.X, mousePosition.Y), false);
            e.Handled = true;
            base.OnMouseWheel(e);
        }


        private void CanIncreaseZoom(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = _zoom < MaxZoom;
        }


        private void IncreaseZoom(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            double zoom = _zoom;
            for (int i = 0; i < ZoomLevels.Length; i++)
            {
                if (zoom < ZoomLevels[i])
                {
                    zoom = ZoomLevels[i];
                    break;
                }
            }

            UpdateScrollInfo(_viewport, zoom, _viewport / 2.0, false);
        }


        private void CanDecreaseZoom(object sender, CanExecuteRoutedEventArgs eventArgs)
        {
            eventArgs.CanExecute = Zoom > MinZoom;
        }


        private void DecreaseZoom(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            double zoom = _zoom;
            for (int i = ZoomLevels.Length - 1; i >= 0; i--)
            {
                if (zoom > ZoomLevels[i])
                {
                    zoom = ZoomLevels[i];
                    break;
                }
            }

            UpdateScrollInfo(_viewport, zoom, _viewport / 2.0, false);
        }


        private void FitScreenExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var texture2D = Texture2D;
            if (texture2D == null)
                return;

            double zx = _viewport.X / texture2D.Width;
            double zy = _viewport.Y / texture2D.Height;
            UpdateScrollInfo(_viewport, Math.Min(zx, zy), _viewport / 2.0, true);
        }


        private void FillScreenExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            var texture2D = Texture2D;
            if (texture2D == null)
                return;

            double zx = _viewport.X / texture2D.Width;
            double zy = _viewport.Y / texture2D.Height;
            UpdateScrollInfo(_viewport, Math.Max(zx, zy), _viewport / 2.0, true);
        }


        private void SetZoom(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            double zoom = ObjectHelper.ConvertTo<double>(eventArgs.Parameter, CultureInfo.InvariantCulture) / 100.0;
            UpdateScrollInfo(_viewport, zoom, _viewport / 2.0, false);
        }


        /// <summary>
        /// Updates the scroll information.
        /// </summary>
        /// <param name="newViewport">The new viewport size.</param>
        /// <param name="newZoom">The new zoom factor.</param>
        /// <param name="anchorViewport">The anchor point relative to the old viewport.</param>
        /// <param name="center">
        /// <see langword="true"/> to center the texture; otherwise, <see langword="false"/>.
        /// </param>
        private void UpdateScrollInfo(Vector newViewport, double newZoom, Vector anchorViewport, bool center)
        {
            if (_inUpdateScrollInfo)
                return;

            _inUpdateScrollInfo = true;

            try
            {
                var texture2D = Texture2D;
                if (texture2D != null)
                {
                    Vector oldViewport = _viewport;
                    _viewport = newViewport;

                    double oldZoom = _zoom;
                    newZoom = MathHelper.Clamp(newZoom, MinZoom, MaxZoom);
                    _zoom = newZoom;

                    // Get scaled texture size.
                    double textureWidth = texture2D.Width;
                    double textureHeight = texture2D.Height;
                    Vector textureSize = new Vector(Math.Round(textureWidth * newZoom, MidpointRounding.AwayFromZero), Math.Round(textureHeight * newZoom, MidpointRounding.AwayFromZero));

                    // Get anchor relative to unscaled texture.
                    Vector anchorTexture = (anchorViewport + _offset - _margin) / oldZoom;

                    // Add virtual scroll area, similar to Adobe Photoshop.
                    _margin.X = newViewport.X - Math.Min(textureSize.X, 100);
                    _margin.Y = newViewport.Y - Math.Min(textureSize.Y, 100);

                    // Get virtual extent.
                    _extent = _margin + textureSize + _margin;

                    if (center)
                    {
                        _offset = _extent / 2.0 - newViewport / 2.0;
                    }
                    else
                    {
                        if (newViewport != oldViewport || newZoom != oldZoom)
                        {
                            // Get anchor relative to scaled texture.
                            anchorTexture *= newZoom;

                            // Get the anchor in new viewport.
                            if (oldViewport.X > 0)
                                anchorViewport.X = anchorViewport.X / oldViewport.X * newViewport.X;
                            else
                                anchorViewport.X = newViewport.X / 2.0;

                            if (oldViewport.Y > 0)
                                anchorViewport.Y = anchorViewport.Y / oldViewport.Y * newViewport.Y;
                            else
                                anchorViewport.Y = newViewport.Y / 2.0;

                            // Get new offset such that anchor point is unchanged.
                            _offset = anchorTexture + _margin - anchorViewport;
                        }

                        _offset.X = MathHelper.Clamp(_offset.X, 0, _extent.X - newViewport.X);
                        _offset.Y = MathHelper.Clamp(_offset.Y, 0, _extent.Y - newViewport.Y);
                    }
                }
                else
                {
                    _viewport = newViewport;
                    _margin = newViewport / 2.0;
                    _extent = newViewport;
                    _offset = new Vector();
                    _zoom = newZoom;
                }

                HorizontalOffset = _offset.X;
                VerticalOffset = _offset.Y;
                Zoom = _zoom;

                _scrollOwner?.InvalidateScrollInfo();
                UpdateGraphicsScreen();
            }
            finally
            {
                _inUpdateScrollInfo = false;
            }
        }


        private void UpdateGraphicsScreen()
        {
            if (_textureGraphicsScreen == null)
                return;

            double x;
            if (_extent.X < _viewport.X)
                x = _viewport.X / 2.0 - _extent.X / 2.0;    // Center texture.
            else
                x = _margin.X -_offset.X;

            double y;
            if (_extent.Y < _viewport.Y)
                y = _viewport.Y / 2.0 - _extent.Y / 2.0;    // Center texture.
            else
                y = _margin.Y -_offset.Y;

            // Apply scaling for high-DPI displays.
            // Get DPI scale (as of .NET 4.6.1, this returns the DPI of the primary monitor, if you have several different DPIs).
            double dpiScale = 1.0; // Default value for 96 dpi.
            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource != null)
            {
                var compositionTarget = presentationSource.CompositionTarget as HwndTarget;
                if (compositionTarget != null)
                    dpiScale = compositionTarget.TransformToDevice.M11;
            }

            _textureGraphicsScreen.Texture2D = Texture2D;
            _textureGraphicsScreen.Offset = new Vector2F((float)(x * dpiScale), (float)(y * dpiScale));
            _textureGraphicsScreen.Scale = (float)(_zoom * dpiScale);

            _textureGraphicsScreen.InputGamma = 2.2f;
            _textureGraphicsScreen.OutputGamma = 2.2f;

            Matrix44F colorTransform;
            Vector4F colorOffset;
            if (EnableRedChannel && EnableGreenChannel && EnableBlueChannel && EnableAlphaChannel)
            {
                // RGBA
                colorTransform = Matrix44F.Identity;
                colorOffset = Vector4F.Zero;
            }
            else if (EnableRedChannel && EnableGreenChannel && EnableBlueChannel && !EnableAlphaChannel)
            {
                // RGB-
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (EnableRedChannel && !EnableGreenChannel && !EnableBlueChannel && !EnableAlphaChannel)
            {
                // R---
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               1, 0, 0, 0,
                                               1, 0, 0, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (!EnableRedChannel && EnableGreenChannel && !EnableBlueChannel && !EnableAlphaChannel)
            {
                // -G--
                colorTransform = new Matrix44F(0, 1, 0, 0,
                                               0, 1, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (!EnableRedChannel && !EnableGreenChannel && EnableBlueChannel && !EnableAlphaChannel)
            {
                // --B-
                colorTransform = new Matrix44F(0, 0, 1, 0,
                                               0, 0, 1, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (!EnableRedChannel && !EnableGreenChannel && !EnableBlueChannel && EnableAlphaChannel)
            {
                // ---A
                colorTransform = new Matrix44F(0, 0, 0, 1,
                                               0, 0, 0, 1,
                                               0, 0, 0, 1,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (EnableRedChannel && EnableGreenChannel && !EnableBlueChannel && !EnableAlphaChannel)
            {
                // RG--
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (EnableRedChannel && !EnableGreenChannel && EnableBlueChannel && !EnableAlphaChannel)
            {
                // R-B-
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (EnableRedChannel && !EnableGreenChannel && !EnableBlueChannel && EnableAlphaChannel)
            {
                // R--A
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else if (EnableRedChannel && EnableGreenChannel && !EnableBlueChannel && EnableAlphaChannel)
            {
                // RG-A
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else if (EnableRedChannel && !EnableGreenChannel && EnableBlueChannel && EnableAlphaChannel)
            {
                // R-BA
                colorTransform = new Matrix44F(1, 0, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else if (!EnableRedChannel && EnableGreenChannel && EnableBlueChannel && !EnableAlphaChannel)
            {
                // -GB-
                colorTransform = new Matrix44F(0, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 0);
                colorOffset = new Vector4F(0, 0, 0, 1);
            }
            else if (!EnableRedChannel && EnableGreenChannel && !EnableBlueChannel && EnableAlphaChannel)
            {
                // -G-A
                colorTransform = new Matrix44F(0, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else if (!EnableRedChannel && EnableGreenChannel && EnableBlueChannel && EnableAlphaChannel)
            {
                // -GBA
                colorTransform = new Matrix44F(0, 0, 0, 0,
                                               0, 1, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else if (!EnableRedChannel && !EnableGreenChannel && EnableBlueChannel && EnableAlphaChannel)
            {
                // --BA
                colorTransform = new Matrix44F(0, 0, 0, 0,
                                               0, 0, 0, 0,
                                               0, 0, 1, 0,
                                               0, 0, 0, 1);
                colorOffset = new Vector4F(0, 0, 0, 0);
            }
            else
            {
                // -
                colorTransform = Matrix44F.Zero;
                colorOffset = Vector4F.Zero;
            }

            _textureGraphicsScreen.ColorTransform = colorTransform;
            _textureGraphicsScreen.ColorOffset = colorOffset;
            _textureGraphicsScreen.IsPremultiplied = IsPremultiplied;
            _textureGraphicsScreen.MipLevel = MipLevel;
        }


        #region ----- IScrollInfo -----

        void IScrollInfo.LineUp()
        {
            _offset.Y -= LineSize;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.LineDown()
        {
            _offset.Y += LineSize;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.LineLeft()
        {
            _offset.X -= LineSize;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.LineRight()
        {
            _offset.X += LineSize;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.PageUp()
        {
            _offset.Y -= _viewport.X;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.PageDown()
        {
            _offset.Y += _viewport.Y;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.PageLeft()
        {
            _offset.X -= _viewport.X;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.PageRight()
        {
            _offset.X += _viewport.X;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.MouseWheelUp()
        {
        }


        void IScrollInfo.MouseWheelDown()
        {
        }


        void IScrollInfo.MouseWheelLeft()
        {
        }


        void IScrollInfo.MouseWheelRight()
        {
        }


        void IScrollInfo.SetHorizontalOffset(double offset)
        {
            _offset.X = offset;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        void IScrollInfo.SetVerticalOffset(double offset)
        {
            _offset.Y = offset;
            UpdateScrollInfo(_viewport, _zoom, _viewport / 2.0, false);
        }


        Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
        {
            return rectangle;
        }
        #endregion

        #endregion
    }
}
