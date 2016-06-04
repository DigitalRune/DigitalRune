// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interactivity;


namespace DigitalRune.Windows.Framework
{
    /// <summary>
    /// Adds a watermark to a <see cref="TextBox"/> or <see cref="ComboBox"/>.
    /// </summary>
    [StyleTypedProperty(Property = nameof(WatermarkStyle), StyleTargetType = typeof(TextBlock))]
    public sealed class WatermarkBehavior : Behavior<Control>
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBox _textBox;
        private TextBlock _watermarkTextBlock;
        private SingleChildAdorner _textBoxAdorner;
        private bool _adornerAdded;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties & Routed Events
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register(
            "IsEnabled",
            typeof(bool),
            typeof(WatermarkBehavior),
            new PropertyMetadata(Boxed.BooleanTrue, OnPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the behavior is enabled.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the behavior is enabled; otherwise, <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the behavior is enabled.")]
        [Category(Categories.Common)]
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="WatermarkText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkTextProperty = DependencyProperty.Register(
            "WatermarkText",
            typeof(string),
            typeof(WatermarkBehavior),
            new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the watermark.
        /// This is a dependency property.
        /// </summary>
        /// <value>The watermark. The default value is <see langword="null"/>.</value>
        [Description("Gets or sets the watermark text.")]
        [Category(Categories.Appearance)]
        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="WatermarkStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WatermarkStyleProperty = DependencyProperty.Register(
            "WatermarkStyle",
            typeof(Style),
            typeof(WatermarkBehavior),
            new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Gets or sets the style of the watermark text.
        /// This is a dependency property.
        /// </summary>
        /// <value>The style of the watermark text.</value>
        [Description("Gets or sets the style of the watermark.")]
        [Category(Categories.Default)]
        public Style WatermarkStyle
        {
            get { return (Style)GetValue(WatermarkStyleProperty); }
            set { SetValue(WatermarkStyleProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void OnPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var behavior = (WatermarkBehavior)dependencyObject;
            behavior.UpdateWatermark();
        }


        /// <summary>
        /// Called after the behavior is attached to an <see cref="Behavior{T}.AssociatedObject"/>.
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            AssociatedObject.Loaded -= OnLoaded;

            _textBox = AssociatedObject.GetVisualSubtree().OfType<TextBox>().FirstOrDefault();
            if (_textBox != null)
            {
                _textBox.GotKeyboardFocus += OnGotKeyboardFocus;
                _textBox.LostKeyboardFocus += OnLostKeyboardFocus;
                _textBox.TextChanged += OnTextChanged;
                _textBox.PreviewDragEnter += OnDragEnter;
                _textBox.PreviewDragLeave += OnDragLeave;
            }

            UpdateWatermark();
        }


        /// <summary>
        /// Called when the behavior is being detached from its 
        /// <see cref="Behavior{T}.AssociatedObject"/>, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= OnLoaded;

            if (_textBox != null)
            {
                _textBox.GotKeyboardFocus -= OnGotKeyboardFocus;
                _textBox.LostKeyboardFocus -= OnLostKeyboardFocus;
                _textBox.TextChanged -= OnTextChanged;
                _textBox.PreviewDragEnter -= OnDragEnter;
                _textBox.PreviewDragLeave -= OnDragLeave;
                _textBox = null;
            }

            base.OnDetaching();
        }


        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            HideWatermark();
        }


        private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            UpdateWatermark();
        }


        private void OnDragEnter(object sender, DragEventArgs eventArgs)
        {
            HideWatermark();
        }


        private void OnDragLeave(object sender, DragEventArgs eventArgs)
        {
            UpdateWatermark();
        }


        private void OnTextChanged(object sender, TextChangedEventArgs eventArgs)
        {
            UpdateWatermark();
        }


        private void UpdateWatermark()
        {
            if (_textBox == null)
                return;

            if (IsEnabled
                && !string.IsNullOrWhiteSpace(WatermarkText)
                && string.IsNullOrEmpty(_textBox.Text)
                && !_textBox.IsKeyboardFocusWithin)
            {
                ShowWatermark();
            }
            else
            {
                HideWatermark();
            }

            if (_watermarkTextBlock != null)
            {
                _watermarkTextBlock.Style = WatermarkStyle;
                _watermarkTextBlock.Text = WatermarkText;
            }
        }


        private void ShowWatermark()
        {
            if (_textBox == null)
                return;

            if (_textBoxAdorner == null)
            {
                _watermarkTextBlock = new TextBlock();
                _textBoxAdorner = new SingleChildAdorner(_textBox, _watermarkTextBlock)
                {
                    IsHitTestVisible = false
                };
            }

            if (!_adornerAdded)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_textBox);
                if (adornerLayer != null)
                {
                    adornerLayer.Add(_textBoxAdorner);
                    _adornerAdded = true;
                }
            }
        }


        private void HideWatermark()
        {
            if (_adornerAdded)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(_textBox);
                adornerLayer?.Remove(_textBoxAdorner);
                _adornerAdded = false;
            }
        }
        #endregion
    }
}
