// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a content control that can be used as a replacement for Microsoft's
    /// <strong>Microsoft.Windows.Themes.ButtonChrome"</strong> class.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Microsoft's <strong>Microsoft.Windows.Themes.ButtonChrome"</strong> implementation has some
    /// limitations. The original <strong>Microsoft.Windows.Themes.ButtonChrome"</strong> class is
    /// derived from the <see cref="Decorator"/> and therefore does not support control templating.
    /// Another limitation of is that it does not support sharp corners: If you set the property
    /// <strong>Microsoft.Windows.Themes.ButtonChrome.RoundCorners"</strong> to
    /// <see langword="false"/> the right corners will still be rounded.
    /// </para>
    /// <para>To overcome these shortcomings we have implemented our own version.</para>
    /// </remarks>
    public class ButtonChrome : ContentControl
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="RenderBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderBackgroundProperty = DependencyProperty.Register(
            "RenderBackground",
            typeof(bool),
            typeof(ButtonChrome),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue));

        /// <summary>
        /// Gets or sets a value indicating whether the background is rendered.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to render the background; otherwise <see langword="false"/>.
        /// The default value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the background is rendered.")]
        [Category(Categories.Appearance)]
        public bool RenderBackground
        {
            get { return (bool)GetValue(RenderBackgroundProperty); }
            set { SetValue(RenderBackgroundProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="RenderDefaulted"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderDefaultedProperty = DependencyProperty.Register(
            "RenderDefaulted",
            typeof(bool),
            typeof(ButtonChrome),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Button"/> has the appearance of
        /// the default button on the form. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Button"/> has the appearance of the default
        /// button; otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the button has the appearance of the default button on the form.")]
        [Category(Categories.Appearance)]
        public bool RenderDefaulted
        {
            get { return (bool)GetValue(RenderDefaultedProperty); }
            set { SetValue(RenderDefaultedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="RenderMouseOver"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderMouseOverProperty = DependencyProperty.Register(
            "RenderMouseOver",
            typeof(bool),
            typeof(ButtonChrome),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Button"/> appears as if the mouse
        /// is over. This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Button"/> appears as if the mouse is over it;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [Description("Gets or sets Gets or sets a value indicating whether the Button appears as if the mouse is over.")]
        [Category(Categories.Appearance)]
        public bool RenderMouseOver
        {
            get { return (bool)GetValue(RenderMouseOverProperty); }
            set { SetValue(RenderMouseOverProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="RenderPressed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RenderPressedProperty = DependencyProperty.Register(
            "RenderPressed",
            typeof(bool),
            typeof(ButtonChrome),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Button"/> appears pressed.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="Button"/> appears pressed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the button appears pressed.")]
        [Category(Categories.Appearance)]
        public bool RenderPressed
        {
            get { return (bool)GetValue(RenderPressedProperty); }
            set { SetValue(RenderPressedProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="CornerRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ButtonChrome),
            new PropertyMetadata(new CornerRadius(2.75), OnCornerRadiusChanged));

        /// <summary>
        /// Gets or sets the corner radius.
        /// This is a dependency property.
        /// </summary>
        /// <value>The comment.</value>
        [Description("Gets or sets the corner radius.")]
        [Category(Categories.Appearance)]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }



        private static readonly DependencyPropertyKey InnerCornerRadiusPropertyKey = DependencyProperty.RegisterReadOnly(
            "InnerCornerRadius",
            typeof(CornerRadius),
            typeof(ButtonChrome),
            new FrameworkPropertyMetadata(new CornerRadius(1.75), FrameworkPropertyMetadataOptions.None));

        /// <summary>
        /// Identifies the <see cref="InnerCornerRadiusPropertyKey"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty InnerCornerRadiusProperty = InnerCornerRadiusPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the inner corner radius.
        /// This is a dependency property.
        /// </summary>
        /// <value>The inner corner radius.</value>
        [Browsable(false)]
        public CornerRadius InnerCornerRadius
        {
            get { return (CornerRadius)GetValue(InnerCornerRadiusProperty); }
            private set { SetValue(InnerCornerRadiusPropertyKey, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ButtonChrome"/> class.
        /// </summary>
        static ButtonChrome()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonChrome), new FrameworkPropertyMetadata(typeof(ButtonChrome)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// Called when the <see cref="CornerRadius"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnCornerRadiusChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (ButtonChrome)dependencyObject;
            CornerRadius oldValue = (CornerRadius)eventArgs.OldValue;
            CornerRadius newValue = (CornerRadius)eventArgs.NewValue;
            element.OnCornerRadiusChanged(oldValue, newValue);
        }

        /// <summary>
        /// Called when the <see cref="CornerRadius"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnCornerRadiusChanged(CornerRadius oldValue, CornerRadius newValue)
        {
            InnerCornerRadius = new CornerRadius(
                Math.Max(0, newValue.TopLeft - 1),
                Math.Max(0, newValue.TopRight - 1),
                Math.Max(0, newValue.BottomRight - 1),
                Math.Max(0, newValue.BottomLeft - 1));
        }
        #endregion
    }
}
