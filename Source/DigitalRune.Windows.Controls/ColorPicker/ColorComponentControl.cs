// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DigitalRune.Mathematics;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a single component of a color.
    /// </summary>
    [TemplatePart(Name = "PART_Slider", Type = typeof(ColorSlider1D))]
    [TemplatePart(Name = "PART_DoubleValue", Type = typeof(NumericUpDown))]
    [TemplatePart(Name = "PART_Int32Value", Type = typeof(NumericUpDown))]
    public class ColorComponentControl : Control
    {
        // Notes:
        // - We could use simply two-way data binding without pre-defined template parts.
        //   Template parts are used because this implementation is significantly faster.
        // - The DisplayValueConverter converts the actual value to a display value, which
        //   is used by the numeric up/down controls.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // Flag to avoid update loops.
        private bool _isUpdating;

        // Template parts:
        private ColorSlider1D _slider;
        private NumericUpDown _int32ValueControl;
        private NumericUpDown _doubleValueControl;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="SliderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SliderBrushProperty = DependencyProperty.Register(
            "SliderBrush",
            typeof(Brush),
            typeof(ColorComponentControl),
            new FrameworkPropertyMetadata(Brushes.Blue));

        /// <summary>
        /// Gets or sets the brush for the slider gradient.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="Brush"/> that is used to fill the slider control, typically a
        /// <see cref="LinearGradientBrush"/>. The default value is <see cref="Brushes.Blue"/>.
        /// </value>
        [Description("Gets or sets the brush for the slider gradient.")]
        [Category(Categories.Brushes)]
        [TypeConverter(typeof(BrushConverter))]
        public Brush SliderBrush
        {
            get { return (Brush)GetValue(SliderBrushProperty); }
            set { SetValue(SliderBrushProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label",
            typeof(string),
            typeof(ColorComponentControl),
            new FrameworkPropertyMetadata("_R:"));

        /// <summary>
        /// Gets or sets the label text. 
        /// This is a dependency property.
        /// </summary>
        [Description("Gets or sets the label text.")]
        [Category(Categories.Appearance)]
        [TypeConverter(typeof(StringConverter))]
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum",
            typeof(int),
            typeof(ColorComponentControl),
            new FrameworkPropertyMetadata(255, OnMaximumChanged, OnCoerceMaximum));

        /// <summary>
        /// Gets or sets the maximum value for the integral component.
        /// This is a dependency property.
        /// </summary>
        /// <value>The maximum value for the integral component.</value>
        [Description("Gets or sets the maximum value for the integral component.")]
        [Category(Categories.Default)]
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="DisplayValueConverter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayValueConverterProperty = DependencyProperty.Register(
            "DisplayValueConverter",
            typeof(IValueConverter),
            typeof(ColorComponentControl),
            new FrameworkPropertyMetadata(null, OnDisplayValueConverterChanged));

        /// <summary>
        /// Gets or sets the value converter used for the numeric values that are shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>The value converter used for the numeric values that are shown.</value>
        [Description("Gets or sets the value converter used for the numeric values that are shown.")]
        [Category(Categories.Default)]
        public IValueConverter DisplayValueConverter
        {
            get { return (IValueConverter)GetValue(DisplayValueConverterProperty); }
            set { SetValue(DisplayValueConverterProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(ColorComponentControl),
            new FrameworkPropertyMetadata(
                Boxed.DoubleZero,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

        /// <summary>
        /// Gets or sets the value of the color component [0, 1].
        /// This is a dependency property.
        /// </summary>
        /// <value>The value of the color component [0 1].</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [Description("Gets or sets the value of the color component [0, 1].")]
        [Category(Categories.Default)]
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ValueChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>),
            typeof(ColorComponentControl));


        /// <summary>
        /// Occurs when the <see cref="Value"/> property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ColorComponentControl"/> class.
        /// </summary>
        static ColorComponentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorComponentControl), new FrameworkPropertyMetadata(typeof(ColorComponentControl)));
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
            if (_slider != null)
            {
                _slider.ValueChanged -= OnSliderChanged;
                _slider = null;
            }

            if (_int32ValueControl != null)
            {
                _int32ValueControl.ValueChanged -= OnInt32ValueChanged;
                _int32ValueControl = null;
            }

            if (_doubleValueControl != null)
            {
                _doubleValueControl.ValueChanged -= OnDoubleValueChanged;
                _doubleValueControl = null;
            }

            base.OnApplyTemplate();

            _slider = GetTemplateChild("PART_Slider") as ColorSlider1D;
            if (_slider != null)
                _slider.ValueChanged += OnSliderChanged;

            _int32ValueControl = GetTemplateChild("PART_Int32Value") as NumericUpDown;
            if (_int32ValueControl != null)
                _int32ValueControl.ValueChanged += OnInt32ValueChanged;

            _doubleValueControl = GetTemplateChild("PART_DoubleValue") as NumericUpDown;
            if (_doubleValueControl != null)
                _doubleValueControl.ValueChanged += OnDoubleValueChanged;
        }


        private static object OnCoerceMaximum(DependencyObject dependencyObject, object baseValue)
        {
            int maximum = (int)baseValue;
            if (Numeric.IsZero(maximum))
            {
                // Avoid division by zero.
                maximum = 255;
            }

            return maximum;
        }


        private static void OnMaximumChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorComponentControl)dependencyObject;
            control.CoerceValue(ValueProperty);
        }


        private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            if (_isUpdating)
                return;

            var slider = (ColorSlider1D)sender;
            double value = slider.Value;
            if (!Numeric.AreEqual(Value, value))
                Value = value;
        }


        private void OnInt32ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            if (_isUpdating)
                return;

            var numericUpDown = (NumericUpDown)sender;
            double displayValue = numericUpDown.Value / Maximum;
            double value = FromDisplayValue(displayValue);
            if (!Numeric.AreEqual(Value, value))
                Value = value;
        }


        private void OnDoubleValueChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            if (_isUpdating)
                return;

            var numericUpDown = (NumericUpDown)sender;
            double displayValue = numericUpDown.Value;
            double value = FromDisplayValue(displayValue);
            if (!Numeric.AreEqual(Value, value))
                Value = value;
        }


        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorComponentControl)dependencyObject;
            double oldValue = (double)eventArgs.OldValue;
            double newValue = (double)eventArgs.NewValue;
            control.OnValueChanged(oldValue, newValue);
        }


        private void OnValueChanged(double oldValue, double newValue)
        {
            UpdateControls(newValue);

            // Raise event.
            var routedEventArgs = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);
            OnValueChanged(routedEventArgs);
        }


        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// Arguments associated with the <see cref="ValueChanged"/> event.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected void OnValueChanged(RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            if (eventArgs == null)
                throw new ArgumentNullException(nameof(eventArgs));

            Debug.Assert(eventArgs.RoutedEvent == ValueChangedEvent, "Invalid RoutedEvent specified.");
            RaiseEvent(eventArgs);
        }


        private void UpdateControls(double value)
        {
            Debug.Assert(!_isUpdating, "Unexpected update loop.");

            try
            {
                _isUpdating = true;

                double displayValue = ToDisplayValue(value);

                // ColorSlider1D is bound to actual value.
                if (_slider != null)
                {
                    if (!Numeric.AreEqual(value, _slider.Value))
                        _slider.Value = value;
                }

                // Integer NumericUpDown is bound to the display value.
                if (_int32ValueControl != null)
                {
                    double intValue = displayValue * Maximum;
                    if (!Numeric.AreEqual(intValue, _int32ValueControl.Value))
                        _int32ValueControl.Value = intValue;
                }

                // Double NumericUpDown is bound to the display value.
                if (_doubleValueControl != null)
                {
                    if (!Numeric.AreEqual(displayValue, _doubleValueControl.Value))
                        _doubleValueControl.Value = displayValue;
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }


        internal void Update()
        {
            UpdateControls(Value);
        }


        /// <summary>
        /// Called when the <see cref="DisplayValueConverter"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDisplayValueConverterChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorComponentControl)dependencyObject;
            control.UpdateControls(control.Value);
        }


        private double FromDisplayValue(double displayValue)
        {
            var converter = DisplayValueConverter;
            if (converter != null)
                return (double)converter.ConvertBack(displayValue, typeof(double), null, null);

            return displayValue;
        }


        private double ToDisplayValue(double value)
        {
            var converter = DisplayValueConverter;
            if (converter != null)
                return (double)converter.Convert(value, typeof(double), null, null);

            return value;
        }
        #endregion
    }
}
