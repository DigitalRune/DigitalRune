// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a text box for a value with buttons to increase or decrease the value.
    /// </summary>
    public abstract class BaseUpDownControl : Control
    {
    }


    /// <summary>
    /// Represents a text box for a value of a certain type with buttons to increase or decrease the
    /// value.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <remarks>
    /// This controls provides the default functionality for all up/down-controls, such as
    /// <see cref="NumericUpDown"/> or <see cref="TimeSpanUpDown"/>.
    /// </remarks>
    [TemplatePart(Name = "PART_Value", Type = typeof(TextBox))]   // Value text box.
    [TemplatePart(Name = "PART_Up", Type = typeof(ButtonBase))]   // Up button.
    [TemplatePart(Name = "PART_Down", Type = typeof(ButtonBase))] // Down button.
    [ContentProperty(nameof(Value))]
    public abstract class BaseUpDownControl<T> : BaseUpDownControl where T : IComparable
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private TextBox _textBox;
        private ButtonBase _buttonUp;
        private ButtonBase _buttonDown;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------   
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum",
            typeof(T),
            typeof(BaseUpDownControl<T>),
            new FrameworkPropertyMetadata(OnMaximumChanged));

        /// <summary>
        /// Gets or sets the maximum value.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value greater than or equal to <see cref="Minimum"/>).
        /// </value>
        [Description("Gets or sets the maximum value.")]
        [Category(Categories.Default)]
        public T Maximum
        {
            get { return (T)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum",
            typeof(T),
            typeof(BaseUpDownControl<T>),
            new FrameworkPropertyMetadata(OnMinimumChanged));

        /// <summary>
        /// Gets or sets the minimum value.
        /// This is a dependency property.
        /// </summary>
        /// A value less than or equal to <see cref="Maximum"/>).
        [Description("Gets or sets the minimum value.")]
        [Category(Categories.Default)]
        public T Minimum
        {
            get { return (T)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(T),
            typeof(BaseUpDownControl<T>),
            new FrameworkPropertyMetadata(
                default(T),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged));

        /// <summary>
        /// Gets or sets the value. 
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A value that lies in the range [<see cref="Minimum"/>, <see cref="Maximum"/>].
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [Description("Gets or sets the value.")]
        [Category(Categories.Default)]
        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ValueChanged"/> routed event.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            "ValueChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<T>),
            typeof(BaseUpDownControl<T>));

        /// <summary>
        /// Occurs when the <see cref="Value"/> property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<T> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// <see cref="PropertyChangedCallback"/> for <see cref="Maximum"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMaximumChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            BaseUpDownControl<T> control = (BaseUpDownControl<T>)dependencyObject;
            control.CoerceValue();
        }


        /// <summary>
        /// <see cref="PropertyChangedCallback"/> for <see cref="Minimum"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnMinimumChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            BaseUpDownControl<T> control = (BaseUpDownControl<T>)dependencyObject;
            control.CoerceValue();
        }


        /// <summary>
        /// Coerces the <see cref="Value"/> to be within <see cref="Minimum"/> and <see cref="Maximum"/>.
        /// </summary>
        private void CoerceValue()
        {
            // Special case: NaN should not be set to Minimum or Maximum.
            if (Value is float && Value.CompareTo(float.NaN) == 0
                || Value is double && Value.CompareTo(double.NaN) == 0)
                return;

            // The value is not NaN. We can clamp it.
            if (Value.CompareTo(Minimum) < 0)
                Value = Minimum;
            if (Value.CompareTo(Maximum) > 0)
                Value = Maximum;
        }


        /// <summary>
        /// <see cref="PropertyChangedCallback"/> for <see cref="Value"/>.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.NewValue == eventArgs.OldValue)
                return;

            BaseUpDownControl<T> control = (BaseUpDownControl<T>)dependencyObject;
            control.CoerceValue();

            if (control.Value.CompareTo((T)eventArgs.OldValue) != 0)
                control.OnValueChanged((T)eventArgs.OldValue, control.Value);
        }


        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        /// <remarks>
        /// <strong>Notes to Inheritors:</strong><br/> When overriding
        /// <see cref="OnValueChanged(T,T)"/> in a derived class, be sure to call the base class's
        /// <see cref="OnValueChanged(T,T)"/> method to raise the event.
        /// </remarks>
        protected virtual void OnValueChanged(T oldValue, T newValue)
        {
            RoutedPropertyChangedEventArgs<T> eventArgs = new RoutedPropertyChangedEventArgs<T>(oldValue, newValue, ValueChangedEvent);
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Unregister old bindings/event handlers.
            if (_textBox != null)
            {
                RemoveValueBinding(_textBox);
                _textBox.PreviewTextInput -= OnPreviewTextBoxInput;
                _textBox = null;
            }

            if (_buttonUp != null)
            {
                _buttonUp.Click -= OnUpClicked;
                _buttonUp = null;
            }

            if (_buttonDown != null)
            {
                _buttonDown.Click -= OnDownClicked;
                _buttonDown = null;
            }

            base.OnApplyTemplate();

            // Get template parts.
            _textBox = GetTemplateChild("PART_Value") as TextBox;
            _buttonUp = GetTemplateChild("PART_Up") as ButtonBase;
            _buttonDown = GetTemplateChild("PART_Down") as ButtonBase;

            // Register bindings/event handlers.
            if (_textBox != null)
            {
                SetValueBinding(_textBox);
                _textBox.PreviewTextInput += OnPreviewTextBoxInput;
            }

            if (_buttonUp != null)
                _buttonUp.Click += OnUpClicked;

            if (_buttonDown != null)
                _buttonDown.Click += OnDownClicked;
        }


        /// <summary>
        /// Removes the value binding.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        protected abstract void RemoveValueBinding(TextBox textBox);


        /// <summary>
        /// Sets the value binding.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        protected abstract void SetValueBinding(TextBox textBox);


        /// <summary>
        /// Called when the down-button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnDownClicked(object sender, RoutedEventArgs eventArgs)
        {
            OnDecrease();
        }


        /// <summary>
        /// Called when up-button is clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        private void OnUpClicked(object sender, RoutedEventArgs eventArgs)
        {
            OnIncrease();
        }


        /// <summary>
        /// Called when the value should be decreased.
        /// </summary>
        protected abstract void OnDecrease();


        /// <summary>
        /// Called when the value should be increased.
        /// </summary>
        protected abstract void OnIncrease();


        /// <summary>
        /// Invoked whenever an unhandled <see cref="UIElement.GotFocus"/> event reaches this
        /// element in its route.
        /// </summary>
        /// <param name="e">
        /// The <see cref="RoutedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnGotFocus(e);

            if (!e.Handled)
                SelectAll();
        }


        /// <summary>
        /// Raises the <see cref="UIElement.GotKeyboardFocus"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyboardFocusChangedEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnGotKeyboardFocus(e);

            if (e.OldFocus != _textBox)
            {
                // Forward focus to TextBox
                _textBox.Focus();
            }
            else
            {
                // Forward focus to previous element.
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
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

            if (e.Key == Key.Up)
            {
                OnIncrease();
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                OnDecrease();
                e.Handled = true;
            }
        }


        private void OnPreviewTextBoxInput(object sender, TextCompositionEventArgs eventArgs)
        {
            if (eventArgs.Handled)
                return;

            // When the caret is in front of a non-digit ('+', '.', ':', ...) and this character is
            // entered, skip ahead.
            if (eventArgs.Text != null && eventArgs.Text.Length == 1)
            {
                char c = eventArgs.Text[0];
                if (!char.IsDigit(c))
                {
                    int caretIndex = _textBox.CaretIndex;
                    string text = _textBox.Text;
                    if (caretIndex < text.Length - 1 && text[caretIndex] == c)
                    {
                        _textBox.CaretIndex = caretIndex + 1;
                        eventArgs.Handled = true;
                    }
                }
            }
        }


        /// <summary>
        /// Selects the entire contents of the text editing control.
        /// </summary>
        public void SelectAll()
        {
            _textBox?.SelectAll();
        }
        #endregion
    }
}
