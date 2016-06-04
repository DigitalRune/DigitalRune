// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that marks the position of a <see cref="GradientStop"/> in a
    /// <see cref="ColorGradientControl"/>.
    /// </summary>
    [TemplateVisualState(GroupName = "SelectionStates", Name = "Unselected")]
    [TemplateVisualState(GroupName = "SelectionStates", Name = "Selected")]
    public class ColorStop : Control
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
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ColorStop),
            new FrameworkPropertyMetadata(Boxed.ColorBlack, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Gets or sets the color of this color stop.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value. The default value is <see cref="Colors.Black"/>.
        /// </value>
        [Description("Gets or sets the color of this color stop.")]
        [Category(Categories.Default)]
        [TypeConverter(typeof(ColorConverter))]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="IsSelected"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
            "IsSelected",
            typeof(bool),
            typeof(ColorStop),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnIsSelectedChanged));

        /// <summary>
        /// Gets or sets a value indicating whether this control is selected.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this control is selected; otherwise <see langword="false"/>.
        /// The default value is <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether this value is selected.")]
        [Category(Categories.Default)]
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, Boxed.Get(value)); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ColorStop"/> class.
        /// </summary>
        static ColorStop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorStop), new FrameworkPropertyMetadata(typeof(ColorStop)));
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
            base.OnApplyTemplate();
            UpdateStates(false);
        }


        private static void OnIsSelectedChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var colorStop = (ColorStop)dependencyObject;
            colorStop.UpdateStates(true);
        }


        private void UpdateStates(bool useTransitions)
        {
            string state = IsSelected ? "Selected" : "Unselected";
            VisualStateManager.GoToState(this, state, useTransitions);
        }
        #endregion
    }
}
