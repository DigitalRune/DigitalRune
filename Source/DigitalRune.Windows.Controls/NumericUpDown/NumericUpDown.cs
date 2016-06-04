// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static System.FormattableString;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represent a control that displays a decimal value that can be increased or decreases using
    /// up/down-buttons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The decimal value (see <see cref="BaseUpDownControl{T}.Value"/>) can be entered directly in
    /// the text box, or it can be increased/decreased by clicking the up/down buttons or by
    /// pressing the up/down arrow keys on the keyboard.
    /// </para>
    /// <para>
    /// When increasing/decreasing the <see cref="BaseUpDownControl{T}.Value"/> the 'Shift' or
    /// 'Control' keys can be pressed to change the step size of the increment. Pressing the 'Shift'
    /// key increases the step size by a factor 10. Pressing the 'Control' key decreases the step
    /// size by a factor of 10.
    /// </para>
    /// </remarks>
    public class NumericUpDown : BaseUpDownControl<double>
    {
        // Notes:
        // - We do not use a two-way binding between the text box text and a double Value because 
        //   with UpdateSourceTrigger.PropertyChanged the text in the text box is immediately 
        //   changed by the binding. For example, you cannot enter "0." because the binding parses 
        //   this to "0" and immediately changes the text back to "0". 
        //   Instead, we could use a OneWayToTargetBinding and call bindingExpression.UpdateTarget()
        //   when needed but then we get a FormatException when the binding is set!?
        //   --> Bind to string property and parse text manually.


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBox _textBox;
        private Binding _textBoxBinding;
        private BindingExpression _textBoxBindingExpression;
        private string _stringFormat;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------   
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="DecimalPlaces"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            "DecimalPlaces",
            typeof(int),
            typeof(NumericUpDown),
            new FrameworkPropertyMetadata(-1, OnDecimalPlacesChanged),
            ValidateDecimalPlaces);

        /// <summary>
        /// Gets or sets the number of decimal places displayed in the text box.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The number of digits after decimal point which should be displayed in the text box. This
        /// value must be greater than or equal to 0 to set a fixed number of decimal places. Set
        /// the value to <c>-1</c> to allow any number of decimal places. The default is <c>-1</c>.
        /// </value>
        [Description("Gets or sets the number of decimal places displayed in the text box.")]
        [Category(Categories.Default)]
        public int DecimalPlaces
        {
            get { return (int)GetValue(DecimalPlacesProperty); }
            set { SetValue(DecimalPlacesProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Increment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
          "Increment",
          typeof(double),
          typeof(NumericUpDown),
          new FrameworkPropertyMetadata(Boxed.DoubleOne));

        /// <summary>
        /// Gets or sets the default increment.
        /// This is a dependency property.
        /// </summary>
        /// <value>A positive <see cref="double"/> value. The default value is 1.</value>
        /// <remarks>
        /// This value determines the step size when increasing or decreasing the
        /// <see cref="BaseUpDownControl{T}.Value"/>.
        /// </remarks>
        [Description("Gets or sets the increment.")]
        [Category(Categories.Default)]
        public double Increment
        {
            get { return (double)GetValue(IncrementProperty); }
            set { SetValue(IncrementProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="StringValue"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty StringValueProperty = DependencyProperty.Register(
            "StringValue",
            typeof(string),
            typeof(NumericUpDown),
            new PropertyMetadata(string.Empty, OnStringValueChanged));

        /// <summary>
        /// Gets or sets the string value which is bound to the text box.
        /// This is a dependency property.
        /// </summary>
        /// <value>The string value which is bound to the text box.</value>
        private string StringValue
        {
            get { return (string)GetValue(StringValueProperty); }
            set { SetValue(StringValueProperty, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="NumericUpDown"/> class.
        /// </summary>
        static NumericUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
            MinimumProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(double.NegativeInfinity));
            MaximumProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(double.PositiveInfinity));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        /// <summary>
        /// <see cref="PropertyChangedCallback"/> for <see cref="DecimalPlaces"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnDecimalPlacesChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (NumericUpDown)dependencyObject;
            if (control.DecimalPlaces >= 0)
                control._stringFormat = Invariant($"{{0:F{control.DecimalPlaces}}}");
            else
                control._stringFormat = null;

            control.UpdateStringValueFromDoubleValue();
        }


        /// <summary>
        /// Called when the <see cref="StringValue"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnStringValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var target = (NumericUpDown)dependencyObject;
            string oldValue = (string)eventArgs.OldValue;
            string newValue = (string)eventArgs.NewValue;
            target.OnStringValueChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="StringValue"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnStringValueChanged(string oldValue, string newValue)
        {
            double d;
            if (!string.IsNullOrEmpty(newValue) && double.TryParse(newValue, NumberStyles.Float, _textBox.Language.GetSpecificCulture(), out d))
            {
                Value = d;
            }
            else
            {
                var error = new ValidationError(new ExceptionValidationRule(), _textBoxBindingExpression);
                Validation.MarkInvalid(_textBoxBindingExpression, error);
            }
        }


        private void UpdateStringValueFromDoubleValue()
        {
            if (_textBox == null)
                return;

            var cultureInfo = _textBox.Language.GetSpecificCulture();
            if (_stringFormat == null)
                StringValue = string.Format(cultureInfo, "{0}", Value);
            else
                StringValue = string.Format(cultureInfo, _stringFormat, Value);
        }


        /// <summary>
        /// Sets the value binding.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        protected override void SetValueBinding(TextBox textBox)
        {
            if (textBox == null)
                throw new ArgumentNullException(nameof(textBox));

            _textBox = textBox;

            // Sets the binding for the text box.
            _textBoxBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(StringValueProperty),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay,
            };
            _textBox.SetBinding(TextBox.TextProperty, _textBoxBinding);

            _textBoxBindingExpression = _textBox.GetBindingExpression(TextBox.TextProperty);
            UpdateStringValueFromDoubleValue();

            // Set event handler for text box.
            textBox.PreviewKeyDown += OnTextBoxKeyDown;
            textBox.LostFocus += OnTextBoxLostFocus;
        }


        /// <summary>
        /// Removes the value binding.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        protected override void RemoveValueBinding(TextBox textBox)
        {
            if (textBox == null)
                throw new ArgumentNullException(nameof(textBox));

            BindingOperations.ClearBinding(textBox, TextBox.TextProperty);
            textBox.PreviewKeyDown += OnTextBoxKeyDown;
            textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox = null;
            _textBoxBinding = null;
            _textBoxBindingExpression = null;
        }


        /// <summary>
        /// Called when the value should be decreased.
        /// </summary>
        protected override void OnDecrease()
        {
            IncreaseWithModifiers(-Increment);
        }


        /// <summary>
        /// Called when the value should be increased.
        /// </summary>
        protected override void OnIncrease()
        {
            IncreaseWithModifiers(Increment);
        }


        /// <summary>
        /// Increases the value and handles modifier keys.
        /// </summary>
        /// <param name="increment">
        /// The step of the increment (can be negative to decrease the value).
        /// </param>
        private void IncreaseWithModifiers(double increment)
        {
            double multiplier = 1;

            // Speedup if Shift is pressed.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                multiplier *= 10;

            // Slow down if Control key is pressed.
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                multiplier *= 0.1;

            // Change value.
            Value += increment * multiplier;

            UpdateStringValueFromDoubleValue();
        }


        private void OnTextBoxKeyDown(object sender, KeyEventArgs eventArgs)
        {
            // Translate the numpad decimal key to the correct decimal separator.
            if (eventArgs.Key == Key.Decimal)
            {
                eventArgs.Handled = true;

                var cultureInfo = _textBox.Language.GetSpecificCulture();
                var textComposition = new TextComposition(InputManager.Current, _textBox, cultureInfo.NumberFormat.NumberDecimalSeparator);
                TextCompositionManager.StartComposition(textComposition);
            }
        }


        /// <inheritdoc/>
        protected override void OnValueChanged(double oldValue, double newValue)
        {
            // When the Value property changes, we have to update the text box text - but only
            // if the user is not currently using the text box.
            if (_textBox != null && !_textBox.IsFocused)
                UpdateStringValueFromDoubleValue();

            base.OnValueChanged(oldValue, newValue);
        }


        private void OnTextBoxLostFocus(object sender, RoutedEventArgs eventArgs)
        {
            // After leaving the text box, update Text to remove any invalid content.
            UpdateStringValueFromDoubleValue();
        }


        /// <summary>
        /// Validates a <see cref="DecimalPlaces"/> value.
        /// </summary>
        /// <param name="value">The integer value for <see cref="DecimalPlaces"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the value is valid for the property <see cref="DecimalPlaces"/>;
        /// otherwise <see langword="false"/>.
        /// </returns>
        private static bool ValidateDecimalPlaces(object value)
        {
            return (int)value >= -1;
        }
        #endregion
    }
}
