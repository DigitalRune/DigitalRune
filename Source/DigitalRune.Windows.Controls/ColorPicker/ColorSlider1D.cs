// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a slider-like control that displays a color gradient.
    /// </summary>
    /// <remarks>
    /// The color gradient must be set with the <see cref="Control.Foreground"/> property. The
    /// <see cref="Value"/> is in the range [0, 1]. The value can be changed with the mouse or with
    /// the arrow keys. When the arrow keys are pressed, the keys 'Shift' or 'Control' can be used
    /// to make change the value faster (Shift key) or slower (Control key).
    /// </remarks>
    [TemplatePart(Name = "PART_Thumb", Type = typeof(FrameworkElement))]
    public class ColorSlider1D : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private FrameworkElement _thumb;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(double),
            typeof(ColorSlider1D),
            new FrameworkPropertyMetadata(
                Boxed.DoubleZero,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                OnCoerceValue));

        /// <summary>
        /// Gets or sets the value.
        /// This is a dependency property.
        /// </summary>
        /// <value>A value in the range [0,1]. The default value is 0.</value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        [Description("Gets or sets the value.")]
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
            typeof(ColorSlider1D));

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
        /// Initializes static members of the <see cref="ColorSlider1D"/> class.
        /// </summary>
        static ColorSlider1D()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSlider1D), new FrameworkPropertyMetadata(typeof(ColorSlider1D)));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static double ClampValue(double value)
        {
            if (value < 0)
                value = 0;
            else if (value > 1)
                value = 1;

            return value;
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
            Value = ClampValue(Value + increment * multiplier);
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _thumb = null;

            base.OnApplyTemplate();

            _thumb = GetTemplateChild("PART_Thumb") as FrameworkElement;
            PositionThumb();
        }


        private static object OnCoerceValue(DependencyObject dependencyObject, object baseValue)
        {
            // Clamp value to [0,1].
            return ClampValue((double)baseValue);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseDown"/> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. This event data
        /// reports details about the mouse button that was pressed and the handled state.
        /// </param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseDown(e);

            // Set focus to this element.
            Focus();

            // Change value.
            Point mousePosition = e.MouseDevice.GetPosition(this);
            Debug.Assert(ActualWidth > 0, "MouseDown event should not be raised when the size of the control is 0.");
            Value = ClampValue(mousePosition.X / ActualWidth);

            // Capture mouse to receive mouse move events if the mouse is outside of the control's area.
            CaptureMouse();

            e.Handled = true;
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseMove"/> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseMove(e);

            if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                // Change value.
                Point mousePosition = e.MouseDevice.GetPosition(this);
                Debug.Assert(ActualWidth > 0, "MouseMove event should not be raised when the size of the control is 0.");
                Value = ClampValue(mousePosition.X / ActualWidth);
                e.Handled = true;
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.MouseUp"/> routed event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that the mouse button was released.
        /// </param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseUp(e);

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                e.Handled = true;
            }
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.PreviewKeyDown"/> attached event reaches
        /// an element in its route that is derived from this class. Implement this method to add
        /// class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Right || e.Key == Key.Up)
            {
                IncreaseWithModifiers(0.01);
                e.Handled = true;
            }
            else if (e.Key == Key.Left || e.Key == Key.Down)
            {
                IncreaseWithModifiers(-0.01);
                e.Handled = true;
            }
        }


        /// <summary>
        /// Raises the <see cref="FrameworkElement.SizeChanged"/> event, using the specified
        /// information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            PositionThumb();
        }


        private static void OnValueChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorSlider1D)dependencyObject;

            // Position thumb.
            control.PositionThumb();

            // Raise ValueChanged event.
            var e = new RoutedPropertyChangedEventArgs<double>((double)eventArgs.OldValue, control.Value, ValueChangedEvent);
            control.OnValueChanged(e);
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
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Sets the thumb to a position determined by the <see cref="Value"/>.
        /// </summary>
        private void PositionThumb()
        {
            if (_thumb != null)
            {
                double xOffset;
                switch (_thumb.HorizontalAlignment)
                {
                    case HorizontalAlignment.Left:
                        xOffset = ActualWidth * Value;
                        break;
                    case HorizontalAlignment.Right:
                        xOffset = ActualWidth * Value - _thumb.Width;
                        break;
                    default:
                        xOffset = ActualWidth * Value - _thumb.Width / 2;
                        break;
                }

                Canvas.SetLeft(_thumb, xOffset);
            }
        }
        #endregion
    }
}
