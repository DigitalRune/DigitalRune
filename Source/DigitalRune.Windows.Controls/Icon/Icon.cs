// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents an image or glyph based icon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Source"/> can be
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// A <see cref="string"/> or <see cref="Uri"/> identifying the image. Example:
    /// "pack://application:,,,/AssemblyName;component/Resources/Image.png"
    /// </item>
    /// <item>
    /// An <see cref="ImageSource"/>.
    /// </item>
    /// <item>
    /// A <see cref="MultiColorGlyph"/>.
    /// </item>
    /// </list>
    /// <para>
    /// The control automatically shows a grayed out, semi-transparent ("ghosted") image when
    /// disabled.
    /// </para>
    /// <para>
    /// If the icon is empty (<see cref="Source"/> is <see langword="null"/>), the element is
    /// <see cref="Visibility.Collapsed"/>.
    /// </para>
    /// </remarks>
    public class Icon : FrameworkElement, IUriContext // IUriContext is required to resolve ImageSource URIs.
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Dictionary<FontFamily, Typeface> TypefaceCache = new Dictionary<FontFamily, Typeface>();
        private MultiColorGlyph _multiColorGlyph;
        private ImageSource _imageSource;
        private ImageEffect _imageEffect;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DisabledOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisabledOpacityProperty = DependencyProperty.Register(
            "DisabledOpacity",
            typeof(double),
            typeof(Icon),
            new FrameworkPropertyMetadata(0.25, OnImageEffectChanged));

        /// <summary>
        /// Gets or sets the opacity of the image when disabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>The opacity of the image when disabled.</value>
        [Description("Gets or sets the opacity of the image when disabled.")]
        [Category(Categories.Appearance)]
        public double DisabledOpacity
        {
            get { return (double)GetValue(DisabledOpacityProperty); }
            set { SetValue(DisabledOpacityProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="EnabledOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EnabledOpacityProperty = DependencyProperty.Register(
            "EnabledOpacity",
            typeof(double),
            typeof(Icon),
            new FrameworkPropertyMetadata(Boxed.DoubleOne, OnImageEffectChanged));

        /// <summary>
        /// Gets or sets the opacity of the image when enabled.
        /// This is a dependency property.
        /// </summary>
        /// <value>The opacity of the image when enabled.</value>
        [Description("Gets or sets the opacity of the image when enabled.")]
        [Category(Categories.Appearance)]
        public double EnabledOpacity
        {
            get { return (double)GetValue(EnabledOpacityProperty); }
            set { SetValue(EnabledOpacityProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Background"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            "Background",
            typeof(Brush),
            typeof(Icon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        ///// <summary>
        ///// Identifies the <see cref="Background"/> dependency property.
        ///// </summary>
        //public static readonly DependencyProperty BackgroundProperty =
        //    TextElement.BackgroundProperty.AddOwner(typeof(Icon));

        /// <summary>
        /// Gets or sets the background brush.
        /// This is a dependency property.
        /// </summary>
        /// <value>The background brush.</value>
        [Description("Gets or sets the background brush.")]
        [Category(Categories.Appearance)]
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Foreground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            "Foreground",
            typeof(Brush),
            typeof(Icon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        ///// <summary>
        ///// Identifies the <see cref="Foreground"/> dependency property.
        ///// </summary>
        //public static readonly DependencyProperty ForegroundProperty =
        //    TextElement.ForegroundProperty.AddOwner(typeof(Icon));

        /// <summary>
        /// Gets or sets the foreground brush.
        /// This is a dependency property.
        /// </summary>
        /// <value>The foreground brush.</value>
        [Description("Gets or sets the foreground brush.")]
        [Category(Categories.Appearance)]
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source",
            typeof(object),
            typeof(Icon),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsRender,
                OnSourceChanged));

        /// <summary>
        /// Gets or sets the source of the icon.
        /// This is a dependency property.
        /// </summary>
        /// <value>The source of the icon.</value>
        /// <remarks>
        /// </remarks>
        [Description("Gets or sets the source of the icon.")]
        [Category(Categories.Common)]
        public object Source
        {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="CollapseIfEmpty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CollapseIfEmptyProperty = DependencyProperty.Register(
            "CollapseIfEmpty",
            typeof(bool),
            typeof(Icon),
            new FrameworkPropertyMetadata(
                Boxed.BooleanFalse, 
                FrameworkPropertyMetadataOptions.AffectsMeasure
                | FrameworkPropertyMetadataOptions.AffectsArrange
                | FrameworkPropertyMetadataOptions.AffectsRender,
                OnCollapseIfEmptyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the icon should collapse if no source is set.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether the icon should collapse if no source is set. The default
        /// value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the icon should collapse if no source is set.")]
        [Category(Categories.Layout)]
        public bool CollapseIfEmpty
        {
            get { return (bool)GetValue(CollapseIfEmptyProperty); }
            set { SetValue(CollapseIfEmptyProperty, Boxed.Get(value)); }
        }


        #region ----- IUriContext -----

        /// <summary>
        /// Gets or sets the base URI of the current application context.
        /// </summary>
        /// <value>The base URI.</value>
        Uri IUriContext.BaseUri
        {
            get { return BaseUri; }
            set { BaseUri = value; }
        }


        /// <summary>
        /// Gets or sets the base URI of the current application context.
        /// </summary>
        /// <value>The base URI.</value>
        private /* protected virtual */ Uri BaseUri
        {
            get { return (Uri)GetValue(BaseUriHelper.BaseUriProperty); }
            set { SetValue(BaseUriHelper.BaseUriProperty, value); }
        }
        #endregion

        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="Icon"/> class.
        /// </summary>
        static Icon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));

            // Add PropertyChangedCallbacks for IsEnabled and Source.
            IsEnabledProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnImageEffectChanged));
            WidthProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
            HeightProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static Typeface GetTypeface(FontFamily fontFamily)
        {
            Debug.Assert(fontFamily != null);

            Typeface typeface;
            if (!TypefaceCache.TryGetValue(fontFamily, out typeface))
            {
                typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                TypefaceCache.Add(fontFamily, typeface);
            }

            return typeface;
        }


        /// <summary>
        /// Called when the <see cref="DisabledOpacity"/> or <see cref="EnabledOpacity"/> property
        /// changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnImageEffectChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var image = (Icon)dependencyObject;
            image.OnImageEffectChanged();
            image.InvalidateVisual();
        }


        private void OnImageEffectChanged()
        {
            bool isEnabled = IsEnabled;
            if (_imageEffect != null)
            {
                _imageEffect.Opacity = isEnabled ? EnabledOpacity : DisabledOpacity;
                _imageEffect.Saturation = isEnabled ? 1.0 : 0.0;
                Opacity = 1;
            }
            else
            {
                Opacity = isEnabled ? EnabledOpacity : DisabledOpacity;
            }
        }


        /// <summary>
        /// Called when the <see cref="Source"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnSourceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var icon = (Icon)dependencyObject;
            icon.OnSourceChanged(eventArgs.OldValue, eventArgs.NewValue);
        }


        /// <summary>
        /// Called when the <see cref="Source"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSourceChanged(object oldValue, object newValue)
        {
            UpdateEffect(newValue);
            UpdateVisibility(newValue);
        }


        /// <summary>
        /// Called when the <see cref="CollapseIfEmpty"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCollapseIfEmptyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (Icon)dependencyObject;
            bool oldValue = (bool)eventArgs.OldValue;
            bool newValue = (bool)eventArgs.NewValue;
            target.OnCollapseIfEmptyChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="CollapseIfEmpty"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnCollapseIfEmptyChanged(bool oldValue, bool newValue)
        {
            UpdateVisibility(Source);
        }


        private void UpdateEffect(object source)
        {
            _multiColorGlyph = source as MultiColorGlyph;
            if (_multiColorGlyph != null)
            {
                Effect = null;
                return;
            }

            _imageSource = TryConvertToImageSource(source);
            if (_imageSource != null)
            {
                if (_imageEffect == null)
                {
                    _imageEffect = new ImageEffect();
                    OnImageEffectChanged();
                }

                // Set custom pixel shader.
                Effect = _imageEffect;
            }
        }


        private ImageSource TryConvertToImageSource(object value)
        {
            if (value is ImageSource)
            {
                var imageSource = (ImageSource)value;

                // Set BaseUri.
                var uriContext = imageSource as IUriContext;
                if (uriContext != null && !imageSource.IsFrozen && uriContext.BaseUri == null)
                    uriContext.BaseUri = BaseUriHelper.GetBaseUri(this);

                return imageSource;
            }

            if (value is string)
                value = new Uri((string)value, UriKind.RelativeOrAbsolute);

            if (value is Uri)
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.BaseUri = BaseUriHelper.GetBaseUri(this);
                bitmapImage.UriSource = (Uri)value;
                bitmapImage.EndInit();
                return bitmapImage;
            }

            return null;
        }


        private void UpdateVisibility(object source)
        {
            Visibility = (source == null && CollapseIfEmpty) ? Visibility.Collapsed : Visibility.Visible;
        }


        ///// <summary>
        ///// When overridden in a derived class, measures the size in layout required for child
        ///// elements and determines a size for the <see cref="FrameworkElement"/>-derived class.
        ///// </summary>
        ///// <param name="constraint">
        ///// The available size that this element can give to child elements. Infinity can be
        ///// specified as a value to indicate that the element will size to whatever content is
        ///// available.
        ///// </param>
        ///// <returns>
        ///// The size that this element determines it needs during layout, based on its calculations
        ///// of child element sizes.
        ///// </returns>
        //protected override Size MeasureOverride(Size constraint)
        //{
        //    return base.MeasureOverride(constraint);
        //}


        ///// <summary>
        ///// When overridden in a derived class, positions child elements and determines a size for a
        ///// <see cref="FrameworkElement"/> derived class.
        ///// </summary>
        ///// <param name="finalSize">
        ///// The final area within the parent that this element should use to arrange itself and its
        ///// children.
        ///// </param>
        ///// <returns>The actual size used.</returns>
        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return base.ArrangeOverride(finalSize);
        //}


        /// <summary>
        /// When overridden in a derived class, participates in rendering operations that are
        /// directed by the layout system. The rendering instructions for this element are not used
        /// directly when this method is invoked, and are instead preserved for later asynchronous
        /// use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        /// The drawing instructions for a specific element. This context is provided to the layout
        /// system.
        /// </param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_multiColorGlyph != null)
            {
                var typeface = GetTypeface(_multiColorGlyph.FontFamily);
                double size = RenderSize.Height;

                if (!string.IsNullOrEmpty(_multiColorGlyph.BackgroundGlyph))
                {
                    DrawGlyph(drawingContext, _multiColorGlyph.BackgroundGlyph, typeface, GetBackgroundBrush(), size);
                }

                if (!string.IsNullOrEmpty(_multiColorGlyph.ForegroundGlyph))
                {
                    DrawGlyph(drawingContext, _multiColorGlyph.ForegroundGlyph, typeface, GetForegroundBrush(), size);
                }

                if (!string.IsNullOrEmpty(_multiColorGlyph.OverlayBackgroundGlyph))
                {
                    DrawGlyph(drawingContext, _multiColorGlyph.OverlayBackgroundGlyph, typeface, GetOverlayBackgroundBrush(), size);
                }

                if (!string.IsNullOrEmpty(_multiColorGlyph.OverlayForegroundGlyph))
                {
                    DrawGlyph(drawingContext, _multiColorGlyph.OverlayForegroundGlyph, typeface, GetOverlayForegroundBrush(), size);
                }
            }
            else if (_imageSource != null)
            {
                drawingContext.DrawImage(_imageSource, new Rect(RenderSize));
            }
        }


        private static void DrawGlyph(DrawingContext drawingContext, string glyph, Typeface typeface, Brush brush, double size)
        {
            // Notes on glyph rendering:
            // - DrawingContext.DrawGlyphRun() is fastest.
            // - DrawingContext.DrawText() is slower and supports TextFormattingMode.Display.
            //   TextFormattingMode.Display is sharper, which may cause artifacts.
            //   TextFormattingMode.Ideal is smoother (blurry) without artifacts.

            Debug.Assert(drawingContext != null);
            Debug.Assert(typeface != null);
            Debug.Assert(!string.IsNullOrEmpty(glyph));
            if (brush == null)
                return;

            var formattedText = new FormattedText(
                glyph, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, brush, null,
                TextFormattingMode.Ideal);

            drawingContext.DrawText(formattedText, new Point(0, 0));
        }


        private Brush GetBackgroundBrush()
        {
            Brush brush = null;
            if (_multiColorGlyph.BackgroundBrushKey != null)
                brush = TryFindResource(_multiColorGlyph.BackgroundBrushKey) as Brush;

            return brush ?? Background;
        }


        private Brush GetForegroundBrush()
        {
            Brush brush = null;
            if (_multiColorGlyph.ForegroundBrushKey != null)
                brush = TryFindResource(_multiColorGlyph.ForegroundBrushKey) as Brush;

            return brush ?? Foreground;
        }


        private Brush GetOverlayBackgroundBrush()
        {
            Brush brush = null;
            if (_multiColorGlyph.OverlayBackgroundBrushKey != null && IsEnabled)
                brush = TryFindResource(_multiColorGlyph.OverlayBackgroundBrushKey) as Brush;

            return brush ?? GetBackgroundBrush();
        }


        private Brush GetOverlayForegroundBrush()
        {
            Brush brush = null;
            if (_multiColorGlyph.OverlayForegroundBrushKey != null && IsEnabled)
                brush = TryFindResource(_multiColorGlyph.OverlayForegroundBrushKey) as Brush;

            return brush ?? GetForegroundBrush();
        }
        #endregion
    }
}
