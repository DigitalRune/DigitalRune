// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that can be used to display and edit a <see cref="TimeSpan"/> using
    /// up/down-buttons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="TimeSpan"/> value (see <see cref="BaseUpDownControl{T}.Value"/>) can be
    /// entered directly in the text box, or it can be increased/decreased by clicking the up/down
    /// buttons or by pressing the up/down arrow keys on the keyboard. Only the unit (hours,
    /// minutes, etc.) where the cursor is located is incremented/decremented.
    /// </para>
    /// <para>
    /// When increasing/decreasing the <see cref="BaseUpDownControl{T}.Value"/> the 'Shift' or
    /// 'Control' keys can be pressed to change the step size of the increment. Pressing the 'Shift'
    /// key increases the step size by a factor 10. Pressing the 'Control' key decreases the step
    /// size by a factor of 10.
    /// </para>
    /// </remarks>
    public class TimeSpanUpDown : BaseUpDownControl<TimeSpan>, IValueConverter
    {
        //--------------------------------------------------------------
        #region Types
        //--------------------------------------------------------------

        private enum TimeUnit
        {
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
        }
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBox _textBox;
        private Binding _textBoxBinding;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------   

        private bool IsBindingValid
        {
            get { return _textBoxBinding != null && _textBox != null; }
        }
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="AlwaysShowSign"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlwaysShowSignProperty = DependencyProperty.Register(
            "AlwaysShowSign",
            typeof(bool),
            typeof(TimeSpanUpDown),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnAlwaysShowXChanged));

        /// <summary>
        /// Gets or sets a value indicating whether the sign is always shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether the sign should be shown by default. The default value is
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether the sign is always shown.")]
        [Category(Categories.Appearance)]
        public bool AlwaysShowSign
        {
            get { return (bool)GetValue(AlwaysShowSignProperty); }
            set { SetValue(AlwaysShowSignProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AlwaysShowDays"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlwaysShowDaysProperty = DependencyProperty.Register(
            "AlwaysShowDays",
            typeof(bool),
            typeof(TimeSpanUpDown),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnAlwaysShowXChanged));

        /// <summary>
        /// Gets or sets a value indicating whether days are always shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether days should be shown by default. The default value is 
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether days are always shown.")]
        [Category(Categories.Appearance)]
        public bool AlwaysShowDays
        {
            get { return (bool)GetValue(AlwaysShowDaysProperty); }
            set { SetValue(AlwaysShowDaysProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AlwaysShowHoursAndMinutes"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlwaysShowHoursAndMinutesProperty = DependencyProperty.Register(
            "AlwaysShowHoursAndMinutes",
            typeof(bool),
            typeof(TimeSpanUpDown),
            new FrameworkPropertyMetadata(Boxed.BooleanTrue, OnAlwaysShowXChanged));

        /// <summary>
        /// Gets or sets a value indicating whether hours and minutes are always shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether hours and minutes should be shown by default. The default
        /// value is <see langword="true"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether hours and minutes are always shown.")]
        [Category(Categories.Appearance)]
        public bool AlwaysShowHoursAndMinutes
        {
            get { return (bool)GetValue(AlwaysShowHoursAndMinutesProperty); }
            set { SetValue(AlwaysShowHoursAndMinutesProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AlwaysShowSeconds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlwaysShowSecondsProperty = DependencyProperty.Register(
            "AlwaysShowSeconds",
            typeof(bool),
            typeof(TimeSpanUpDown),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnAlwaysShowXChanged));


        /// <summary>
        /// Gets or sets a value indicating whether seconds are always shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether seconds should be shown by default. The default value is
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether seconds are always shown.")]
        [Category(Categories.Appearance)]
        public bool AlwaysShowSeconds
        {
            get { return (bool)GetValue(AlwaysShowSecondsProperty); }
            set { SetValue(AlwaysShowSecondsProperty, Boxed.Get(value)); }
        }


        /// <summary>
        /// Identifies the <see cref="AlwaysShowMilliseconds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AlwaysShowMillisecondsProperty = DependencyProperty.Register(
            "AlwaysShowMilliseconds",
            typeof(bool),
            typeof(TimeSpanUpDown),
            new FrameworkPropertyMetadata(Boxed.BooleanFalse, OnAlwaysShowXChanged));

        /// <summary>
        /// Gets or sets a value indicating whether milliseconds are always shown.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value indicating whether milliseconds should be shown by default. The default value is
        /// <see langword="false"/>.
        /// </value>
        [Description("Gets or sets a value indicating whether milliseconds are always shown.")]
        [Category(Categories.Appearance)]
        public bool AlwaysShowMilliseconds
        {
            get { return (bool)GetValue(AlwaysShowMillisecondsProperty); }
            set { SetValue(AlwaysShowMillisecondsProperty, Boxed.Get(value)); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="TimeSpanUpDown"/> class.
        /// </summary>
        static TimeSpanUpDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSpanUpDown), new FrameworkPropertyMetadata(typeof(TimeSpanUpDown)));
            MinimumProperty.OverrideMetadata(typeof(TimeSpanUpDown), new FrameworkPropertyMetadata(TimeSpan.MinValue));
            MaximumProperty.OverrideMetadata(typeof(TimeSpanUpDown), new FrameworkPropertyMetadata(TimeSpan.MaxValue));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------    

        private static void OnAlwaysShowXChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            ((TimeSpanUpDown)dependencyObject).UpdateTextBox();
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
            UpdateValueBinding();

            // Set event handler for text box
            textBox.LostFocus += OnTextBoxLostFocus;
        }


        private void UpdateValueBinding()
        {
            Debug.Assert(_textBox != null, "UpdateValueBinding() should only be called after _textBox is set.");

            _textBoxBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath(ValueProperty),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay,
                Converter = this,
            };

            _textBox.SetBinding(TextBox.TextProperty, _textBoxBinding);
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
            textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox = null;
            _textBoxBinding = null;
        }


        /// <summary>
        /// Called when the value should be decreased.
        /// </summary>
        protected override void OnDecrease()
        {
            IncreaseWithModifiers(-1);
        }


        /// <summary>
        /// Called when the value should be increased.
        /// </summary>
        protected override void OnIncrease()
        {
            IncreaseWithModifiers(+1);
        }


        /// <summary>
        /// Increases the value and handles modifier keys.
        /// </summary>
        /// <param name="increment">
        /// The step of the increment (can be negative to decrease the value).
        /// </param>
        private void IncreaseWithModifiers(int increment)
        {
            // Speedup if Shift is pressed.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                increment *= 10;

            // First, lets find out where the caret is located in the TextBox.
            // The location in the TextBox determines the increment.
            TimeUnit unit = GetUnitAtCaret();
            TimeSpan timeIncrement = TimeSpan.Zero;
            switch (unit)
            {
                case TimeUnit.Days:
                    timeIncrement = new TimeSpan(increment, 0, 0, 0, 0);
                    break;
                case TimeUnit.Hours:
                    timeIncrement = new TimeSpan(0, increment, 0, 0, 0);
                    break;
                case TimeUnit.Minutes:
                    timeIncrement = new TimeSpan(0, 0, increment, 0, 0);
                    break;
                case TimeUnit.Seconds:
                    timeIncrement = new TimeSpan(0, 0, 0, increment, 0);
                    break;
                case TimeUnit.Milliseconds:
                    timeIncrement = new TimeSpan(0, 0, 0, 0, increment);
                    break;
            }

            // Change value.
            Value += timeIncrement;
            SetCaret(unit);
        }


        private TimeUnit GetUnitAtCaret()
        {
            TimeUnit unit = TimeUnit.Days;
            TimeSpan timeSpan = Value;
            bool isNegative = timeSpan < TimeSpan.Zero;
            bool showSign = AlwaysShowSign || isNegative;
            bool showMilliseconds = AlwaysShowMilliseconds || timeSpan.Milliseconds != 0;
            bool showSeconds = AlwaysShowSeconds || timeSpan.Seconds != 0 || showMilliseconds;
            bool showDays = AlwaysShowDays || timeSpan.Days != 0;
            bool showHoursAndMinutes = AlwaysShowHoursAndMinutes || timeSpan.Hours != 0 || timeSpan.Minutes != 0 || showDays || showSeconds;

            // Check whether the content of the TextBox is valid.
            TimeSpan dummy;
            if (!TimeSpan.TryParse(_textBox.Text, out dummy))
                UpdateTextBox();

            string text = _textBox.Text;
            int index = 0;
            int caretIndex = _textBox.CaretIndex;

            if (showSign)
            {
                // Skip sign.
                ++index;
                --caretIndex;
            }

            // Check whether caret at days.
            if (showDays)
            {
                while (index < text.Length && char.IsDigit(text[index]))
                {
                    ++index;
                    --caretIndex;
                }

                if (caretIndex <= 0)
                {
                    // Time increment is 1 day
                    unit = TimeUnit.Days;
                    return unit;
                }
            }

            // Check whether caret is at hours.
            if (showHoursAndMinutes)
            {
                if (index < text.Length && text[index] == '.')
                {
                    // Skip '.' between days and hours
                    ++index;
                    --caretIndex;
                }

                // Skip hours
                index += 2;
                caretIndex -= 2;

                if (caretIndex <= 0)
                {
                    // Time increment is 1 hour
                    unit = TimeUnit.Hours;
                    return unit;
                }
            }

            // Check whether caret is at minutes.
            if (showHoursAndMinutes)
            {
                // Skip ':' between hours and minutes
                ++index;
                --caretIndex;

                // Skip minutes
                index += 2;
                caretIndex -= 2;

                if (caretIndex <= 0)
                {
                    // Time increment is 1 minute
                    unit = TimeUnit.Minutes;
                    return unit;
                }
            }

            // Check whether caret is at seconds.
            if (showSeconds)
            {
                if (index < text.Length && text[index] == ':')
                {
                    // Skip ':' between minutes and seconds
                    ++index;
                    --caretIndex;
                }

                // Skip seconds
                // index += 2;
                caretIndex -= 2;

                if (caretIndex <= 0)
                {
                    // Time increment is 1 second
                    unit = TimeUnit.Seconds;
                    return unit;
                }
            }

            // Check whether caret is at milliseconds.
            if (caretIndex > 0 && showMilliseconds)
            {
                // Time increment is 1 second
                unit = TimeUnit.Milliseconds;
            }
            return unit;
        }


        private void SetCaret(TimeUnit unit)
        {
            TimeSpan timeSpan = Value;
            bool isNegative = timeSpan < TimeSpan.Zero;
            bool showSign = AlwaysShowSign || isNegative;
            bool showMilliseconds = AlwaysShowMilliseconds || timeSpan.Milliseconds != 0;
            bool showSeconds = AlwaysShowSeconds || timeSpan.Seconds != 0 || showMilliseconds;
            bool showDays = AlwaysShowDays || timeSpan.Days != 0;
            bool showHoursAndMinutes = AlwaysShowHoursAndMinutes || timeSpan.Hours != 0 || timeSpan.Minutes != 0 || showDays || showSeconds;

            // Check whether the content of the TextBox is valid.
            TimeSpan dummy;
            if (!TimeSpan.TryParse(_textBox.Text, out dummy))
                UpdateTextBox();

            string text = _textBox.Text;
            int index = 0;

            if (showSign)
            {
                // Skip sign.
                ++index;
            }

            // Check whether caret at days.
            if (showDays)
            {
                while (index < text.Length && char.IsDigit(text[index]))
                {
                    ++index;
                }

                if (unit == TimeUnit.Days)
                {
                    _textBox.CaretIndex = index;
                    return;
                }
            }

            // Check whether caret is at hours.
            if (showHoursAndMinutes)
            {
                if (index < text.Length && text[index] == '.')
                {
                    // Skip '.' between days and hours
                    ++index;
                }

                // Skip hours
                index += 2;

                if (unit == TimeUnit.Hours)
                {
                    _textBox.CaretIndex = index;
                    return;
                }
            }

            // Check whether caret is at minutes.
            if (showHoursAndMinutes)
            {
                // Skip ':' between hours and minutes
                ++index;

                // Skip minutes
                index += 2;

                if (unit == TimeUnit.Minutes)
                {
                    _textBox.CaretIndex = index;
                    return;
                }
            }

            // Check whether caret is at seconds.
            if (showSeconds)
            {
                if (index < text.Length && text[index] == ':')
                {
                    // Skip ':' between minutes and seconds
                    ++index;
                }

                // Skip seconds
                index += 2;

                if (unit == TimeUnit.Seconds)
                {
                    _textBox.CaretIndex = index;
                    return;
                }
            }

            if (showMilliseconds)
            {
                _textBox.CaretIndex = text.Length - 1;
            }
        }


        private void OnTextBoxLostFocus(object sender, RoutedEventArgs eventArgs)
        {
            UpdateTextBox();
        }


        private void UpdateTextBox()
        {
            if (IsBindingValid)
            {
                // After leaving the text box, update Text to remove any invalid content.
                BindingExpression bindingExpression = _textBox.GetBindingExpression(TextBox.TextProperty);
                Debug.Assert(bindingExpression != null, "bindingExpression should not be null when IsBindingValid is set!");
                bindingExpression.UpdateTarget();
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.PreviewKeyDown"/>
        /// attached event reaches an element in its route that is derived from this class.
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (e.Key == Key.Escape)
            {
                // Remove any invalid content.
                UpdateTextBox();
            }

            base.OnPreviewKeyDown(e);
        }


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
        /// used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value is TimeSpan, "Source type should be TimeSpan.");
            Debug.Assert(targetType == typeof(string), "Target type should be String.");

            TimeSpan timeSpan = (TimeSpan)value;

            bool isNegative = timeSpan < TimeSpan.Zero;
            bool showSign = AlwaysShowSign || isNegative;
            bool showMilliseconds = AlwaysShowMilliseconds || timeSpan.Milliseconds != 0;
            bool showSeconds = AlwaysShowSeconds || timeSpan.Seconds != 0 || showMilliseconds;
            bool showDays = AlwaysShowDays || timeSpan.Days != 0;
            bool showHoursAndMinutes = AlwaysShowHoursAndMinutes || timeSpan.Hours != 0 || timeSpan.Minutes != 0 || showDays || showSeconds;

            if (isNegative)
            {
                // We will handle the sign explicitly.
                timeSpan = timeSpan.Negate();
            }

            var s = new StringBuilder();
            if (showSign)
                s.Append(isNegative ? '-' : '+');

            if (showDays)
            {
                s.Append(timeSpan.Days);

                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                //   Currently when showDays is set showHoursAndMinutes is always true, so the following
                //   check is pointless.
                //   But we might change the condition in the future, therefore we keep the check.
                if (showHoursAndMinutes)
                    s.Append('.');
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            }

            if (showHoursAndMinutes)
            {
                s.Append(timeSpan.Hours.ToString("D2", culture));
                s.Append(':');
                s.Append(timeSpan.Minutes.ToString("D2", culture));
                if (showSeconds)
                    s.Append(':');
            }

            if (showSeconds)
            {
                s.Append(timeSpan.Seconds.ToString("D2", culture));
                if (showMilliseconds)
                    s.Append('.');
            }

            if (showMilliseconds)
                s.Append(timeSpan.Milliseconds.ToString("D3", culture));

            return s.ToString();
        }


        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns <see langword="null"/>, the valid null value is 
        /// used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            Debug.Assert(str != null, "Source type should be String.");
            Debug.Assert(targetType == typeof(TimeSpan), "Target type should be TimeSpan.");

            TimeSpan timeSpan;
            if (TimeSpan.TryParse(str, out timeSpan))
                return timeSpan;

            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}
