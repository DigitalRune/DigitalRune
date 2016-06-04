// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that lets the user choose a color.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    [TemplatePart(Name = "PART_H", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_S", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_V", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_R", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_G", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_B", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_A", Type = typeof(ColorComponentControl))]
    [TemplatePart(Name = "PART_EyeDropperButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_OldColorNoAlpha", Type = typeof(Shape))]
    [TemplatePart(Name = "PART_OldColor", Type = typeof(Shape))]
    [TemplatePart(Name = "PART_NewColorNoAlpha", Type = typeof(Shape))]
    [TemplatePart(Name = "PART_NewColor", Type = typeof(Shape))]
    [TemplatePart(Name = "PART_HexValue", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Slider2D", Type = typeof(ColorSlider2D))]
    [TemplatePart(Name = "PART_Slider1D", Type = typeof(ColorSlider1D))]
    [TemplatePart(Name = "PART_ButtonH", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ButtonS", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ButtonV", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ButtonR", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ButtonG", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ButtonB", Type = typeof(RadioButton))]
    [TemplatePart(Name = "PART_ColorSpace", Type = typeof(RadioButton))]
    public class ColorPicker : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        // True if the eye dropper button was clicked.
        private bool _isInEyeDropperMode;

        // The current color before the eye dropper button was clicked.
        private Color _colorBeforeEyeDropperClicked;

        // An invisible window used for the eye dropper color picking.
        private Window _invisibleFullScreenWindow;

        // A screenshot used for eye dropper color picking.
        private System.Drawing.Bitmap _screenshot;

        // The image source for the 2D color area.
        private WriteableBitmap _detailAreaBitmap;

        // Pixel buffer which is used to update _detailAreaBitmap.
        private byte[] _pixelBuffer;

        // The components which were changed. This info is set in the value
        // changed events and used in the UpdateXxx() methods.
        // Set this field BEFORE the Color is changed. Then the ColorChanged
        // method knows that the color was updated via a control.
        private ColorComponents _changedComponents;

        // True if the values of the detail area were changed directly.
        private bool _detailAreaChanged;

        // False if the bitmap of the 2D area must be re-built.
        private bool _detailAreaBitmapIsValid;

        // An IValueConverter is used to convert between color spaces.
        private readonly ColorSpaceConverter _colorSpaceConverter;

        // ----- The template parts:
        private ColorComponentControl _colorHControl;
        private ColorComponentControl _colorSControl;
        private ColorComponentControl _colorVControl;
        private ColorComponentControl _colorRControl;
        private ColorComponentControl _colorGControl;
        private ColorComponentControl _colorBControl;
        private ColorComponentControl _colorAControl;
        private Shape _oldColorNoAlphaShape;
        private Shape _oldColorShape;
        private Shape _newColorNoAlphaShape;
        private Shape _newColorShape;
        private TextBox _hexValueTextBox;
        private ColorSlider1D _slider1D;
        private ColorSlider2D _slider2D;
        private RadioButton _buttonH;
        private RadioButton _buttonS;
        private RadioButton _buttonV;
        private RadioButton _buttonR;
        private RadioButton _buttonG;
        private RadioButton _buttonB;
        private Button _eyeDropperButton;
        private Selector _colorSpaceSelector;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ColorSpace"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorSpaceProperty = DependencyProperty.Register(
            "ColorSpace",
            typeof(ColorSpace),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(ColorSpace.SRgb, OnColorSpaceChanged));

        /// <summary>
        /// Gets or sets the color space (affects the R, G, B values displayed by the numeric
        /// controls). This is a dependency property.
        /// </summary>
        /// <value>
        /// The color space (affects the R, G, B values displayed by the numeric controls).
        /// </value>
        [Description("Gets or sets the color space (affects the R, G, B values displayed by the numeric controls).")]
        [Category(Categories.Default)]
        public ColorSpace ColorSpace
        {
            get { return (ColorSpace)GetValue(ColorSpaceProperty); }
            set { SetValue(ColorSpaceProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(
                Boxed.ColorBlack,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnColorChanged));

        /// <summary>
        /// Gets or sets the sRGB color selected by the user.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Media.Color"/>. The default color is
        /// <see cref="Colors.Black"/>.
        /// </value>
        /// <remarks>
        /// This is the new selected color.
        /// </remarks>
        [Description("Gets or sets the color.")]
        [Category(Categories.Default)]
        [TypeConverter(typeof(ColorConverter))]
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="OldColor"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OldColorProperty = DependencyProperty.Register(
            "OldColor",
            typeof(Color),
            typeof(ColorPicker),
            new FrameworkPropertyMetadata(Boxed.ColorBlack));

        /// <summary>
        /// Gets or sets the old sRGB color.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Media.Color"/>. The default color is
        /// <see cref="Colors.Black"/>.
        /// </value>
        /// <remarks>
        /// This color is displayed to compare with the new color.
        /// </remarks>
        [Description("Gets or sets the old color.")]
        [Category(Categories.Default)]
        [TypeConverter(typeof(ColorConverter))]
        public Color OldColor
        {
            get { return (Color)GetValue(OldColorProperty); }
            set { SetValue(OldColorProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="SelectedComponent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedComponentProperty = DependencyProperty.Register(
          "SelectedComponent",
          typeof(ColorComponents),
          typeof(ColorPicker),
          new FrameworkPropertyMetadata(ColorComponents.Hue, OnSelectedComponentChanged));

        /// <summary>
        /// Gets or sets the selected color component which determines the content of the 1D and 2D
        /// color slider. This is a dependency property.
        /// </summary>
        /// <value>
        /// The selected color component which determines the content of the 1D and 2D color slider.
        /// </value>
        /// <remarks>
        /// For example, if the selected color component is red, the 1D slider will show a red
        /// gradient to choose the intensity of red. The 2D slider will show the range of available
        /// colors for the given intensity of red.
        /// </remarks>
        [Description("Gets or sets the selected color component which determines the content of the 1D and 2D   color slider.")]
        [Category(Categories.Default)]
        public ColorComponents SelectedComponent
        {
            get { return (ColorComponents)GetValue(SelectedComponentProperty); }
            set { SetValue(SelectedComponentProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="ColorChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent ColorChangedEvent = EventManager.RegisterRoutedEvent(
          "ColorChanged",
          RoutingStrategy.Bubble,
          typeof(RoutedPropertyChangedEventHandler<Color>),
          typeof(ColorPicker));

        /// <summary>
        /// Occurs when the <see cref="Color"/> property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color> ColorChanged
        {
            add { AddHandler(ColorChangedEvent, value); }
            remove { RemoveHandler(ColorChangedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes the static members of the <see cref="ColorPicker"/> class.
        /// </summary>
        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPicker"/> class.
        /// </summary>
        public ColorPicker()
        {
            _colorSpaceConverter = new ColorSpaceConverter
            {
                SourceColorSpace = ColorSpace.SRgb,
                TargetColorSpace = ColorSpace.SRgb
            };

            Loaded += OnLoaded;

            CommandBindings.Add(new CommandBinding(ColorPickerCommands.ResetColor, OnResetColorExecuted));
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void OnResetColorExecuted(object sender, ExecutedRoutedEventArgs eventArgs)
        {
            Color = OldColor;
            eventArgs.Handled = true;
        }


        private Color GetColorFromControls()
        {
            Color color;
            if (_buttonH.IsChecked.GetValueOrDefault())
            {
                color = ColorHelper.FromHsv(
                  _slider1D.Value * 360,
                  _slider2D.Value.X * 100,
                  _slider2D.Value.Y * 100);
            }
            else if (_buttonS.IsChecked.GetValueOrDefault())
            {
                color = ColorHelper.FromHsv(
                  _slider2D.Value.X * 360,
                  _slider1D.Value * 100,
                  _slider2D.Value.Y * 100);
            }
            else if (_buttonV.IsChecked.GetValueOrDefault())
            {
                color = ColorHelper.FromHsv(
                  _slider2D.Value.X * 360,
                  _slider2D.Value.Y * 100,
                  _slider1D.Value * 100);
            }
            else if (_buttonR.IsChecked.GetValueOrDefault())
            {
                color = new Color
                {
                    R = (byte)(_slider1D.Value * 255),
                    G = (byte)(_slider2D.Value.X * 255),
                    B = (byte)(_slider2D.Value.Y * 255)
                };
            }
            else if (_buttonG.IsChecked.GetValueOrDefault())
            {
                color = new Color
                {
                    R = (byte)(_slider2D.Value.X * 255),
                    G = (byte)(_slider1D.Value * 255),
                    B = (byte)(_slider2D.Value.Y * 255)
                };
            }
            else
            {
                color = new Color
                {
                    R = (byte)(_slider2D.Value.X * 255),
                    G = (byte)(_slider2D.Value.Y * 255),
                    B = (byte)(_slider1D.Value * 255)
                };
            }

            color.A = (byte)(_colorAControl.Value * 255);
            return color;
        }


        /// <summary>
        /// Gets the color from hex value text box.
        /// </summary>
        /// <returns>The color, or <see langword="null"/> if the string is invalid.</returns>
        private Color? GetColorFromHexValue()
        {
            try
            {
                string hexText = _hexValueTextBox.Text;

                // Prepend '#'
                if (hexText.Length > 0 && hexText[0] != '#')
                    hexText = "#" + hexText;

                return (Color)ColorConverter.ConvertFromString(hexText);
            }
            catch (FormatException)
            {
                return null;
            }
        }


        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal 
        /// processes call <see cref="FrameworkElement.ApplyTemplate"/>.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Cleanup
            if (_eyeDropperButton != null)
            {
                _eyeDropperButton.Click += OnEyeDropperButtonClicked;
                _eyeDropperButton = null;
            }

            _oldColorNoAlphaShape = null;
            _oldColorShape = null;
            _newColorNoAlphaShape = null;
            _newColorShape = null;
            _hexValueTextBox = null;
            _slider1D = null;
            _slider2D = null;

            if (_buttonH != null)
            {
                _buttonH.Checked -= OnRadioButtonChecked;
                _buttonH = null;
            }

            if (_buttonS != null)
            {
                _buttonS.Checked -= OnRadioButtonChecked;
                _buttonS = null;
            }

            if (_buttonV != null)
            {
                _buttonV.Checked -= OnRadioButtonChecked;
                _buttonV = null;
            }

            if (_buttonR != null)
            {
                _buttonR.Checked -= OnRadioButtonChecked;
                _buttonR = null;
            }

            if (_buttonG != null)
            {
                _buttonG.Checked -= OnRadioButtonChecked;
                _buttonG = null;
            }

            if (_buttonB != null)
            {
                _buttonB.Checked -= OnRadioButtonChecked;
                _buttonB = null;
            }

            if (_colorSpaceSelector != null)
            {
                _colorSpaceSelector.SelectionChanged -= OnColorSpaceSelectionChanged;
                _colorSpaceSelector = null;
            }

            base.OnApplyTemplate();

            // Get TemplateParts.
            // Store color component enums in tags of color component controls.
            // If a template part is missing, create an empty dummy element to avoid many NullReference checks.
            // Where necessary, add an event handler.

            _colorHControl = GetTemplateChild("PART_H") as ColorComponentControl ?? new ColorComponentControl();
            _colorHControl.Tag = ColorComponents.Hue;

            _colorSControl = GetTemplateChild("PART_S") as ColorComponentControl ?? new ColorComponentControl();
            _colorSControl.Tag = ColorComponents.Saturation;

            _colorVControl = GetTemplateChild("PART_V") as ColorComponentControl ?? new ColorComponentControl();
            _colorVControl.Tag = ColorComponents.Value;

            _colorRControl = GetTemplateChild("PART_R") as ColorComponentControl ?? new ColorComponentControl();
            _colorRControl.DisplayValueConverter = _colorSpaceConverter;
            _colorRControl.Tag = ColorComponents.Red;

            _colorGControl = GetTemplateChild("PART_G") as ColorComponentControl ?? new ColorComponentControl();
            _colorGControl.DisplayValueConverter = _colorSpaceConverter;
            _colorGControl.Tag = ColorComponents.Green;

            _colorBControl = GetTemplateChild("PART_B") as ColorComponentControl ?? new ColorComponentControl();
            _colorBControl.DisplayValueConverter = _colorSpaceConverter;
            _colorBControl.Tag = ColorComponents.Blue;

            _colorAControl = GetTemplateChild("PART_A") as ColorComponentControl ?? new ColorComponentControl();
            _colorAControl.Tag = ColorComponents.Alpha;

            _eyeDropperButton = GetTemplateChild("PART_EyeDropperButton") as Button;
            if (_eyeDropperButton != null)
                _eyeDropperButton.Click += OnEyeDropperButtonClicked;

            _oldColorNoAlphaShape = GetTemplateChild("PART_OldColorNoAlpha") as Shape ?? new Rectangle();
            _oldColorShape = GetTemplateChild("PART_OldColor") as Shape ?? new Rectangle();
            _newColorNoAlphaShape = GetTemplateChild("PART_NewColorNoAlpha") as Shape ?? new Rectangle();
            _newColorShape = GetTemplateChild("PART_NewColor") as Shape ?? new Rectangle();

            _hexValueTextBox = GetTemplateChild("PART_HexValue") as TextBox ?? new TextBox();
            _hexValueTextBox.Tag = ColorComponents.Red | ColorComponents.Green | ColorComponents.Blue | ColorComponents.Alpha;

            _slider1D = GetTemplateChild("PART_Slider1D") as ColorSlider1D ?? new ColorSlider1D();
            _slider2D = GetTemplateChild("PART_Slider2D") as ColorSlider2D ?? new ColorSlider2D();
            _slider2D.SizeChanged += OnDetailAreaSizeChanged;

            _buttonH = GetTemplateChild("PART_ButtonH") as RadioButton ?? new RadioButton();
            _buttonH.Checked += OnRadioButtonChecked;
            _buttonS = GetTemplateChild("PART_ButtonS") as RadioButton ?? new RadioButton();
            _buttonS.Checked += OnRadioButtonChecked;
            _buttonV = GetTemplateChild("PART_ButtonV") as RadioButton ?? new RadioButton();
            _buttonV.Checked += OnRadioButtonChecked;
            _buttonR = GetTemplateChild("PART_ButtonR") as RadioButton ?? new RadioButton();
            _buttonR.Checked += OnRadioButtonChecked;
            _buttonG = GetTemplateChild("PART_ButtonG") as RadioButton ?? new RadioButton();
            _buttonG.Checked += OnRadioButtonChecked;
            _buttonB = GetTemplateChild("PART_ButtonB") as RadioButton ?? new RadioButton();
            _buttonB.Checked += OnRadioButtonChecked;

            _colorSpaceSelector = GetTemplateChild("PART_ColorSpace") as Selector;
            if (_colorSpaceSelector != null)
            {
                _colorSpaceSelector.SelectedIndex = (int)ColorSpace;
                _colorSpaceSelector.SelectionChanged += OnColorSpaceSelectionChanged;
            }

            OnSelectedComponentChanged(this, new DependencyPropertyChangedEventArgs());
        }


        private void OnDetailAreaSizeChanged(object sender, SizeChangedEventArgs eventArgs)
        {
            // If the control is loaded, create a new gradient bitmap.
            if (IsLoaded)
                UpdateDetailAreaBitmap();
        }


        /// <summary>
        /// Raises the <see cref="ColorChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// Arguments associated with the <see cref="ColorChanged"/> event.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected void OnColorChanged(RoutedPropertyChangedEventArgs<Color> eventArgs)
        {
            RaiseEvent(eventArgs);
        }


        private void OnDetailAreaChanged(object sender, RoutedEventArgs eventArgs)
        {
            // Set new color and update controls.
            _changedComponents |= (ColorComponents)((FrameworkElement)sender).Tag;
            _detailAreaChanged = true;
            Color = GetColorFromControls();
            UpdateControls();
        }


        private void OnHexValueChanged(object sender, TextChangedEventArgs eventArgs)
        {
            // Set the new color and update controls.
            _changedComponents = ColorComponents.All;
            Color? c = GetColorFromHexValue();
            if (c.HasValue)
            {
                Color = c.Value;
                UpdateControls();
            }
        }


        private void OnHexValueLostFocus(object sender, RoutedEventArgs eventArgs)
        {
            // Correct hex value when hex value text box loses focus.
            _hexValueTextBox.TextChanged -= OnHexValueChanged;
            _hexValueTextBox.Text = Color.ToString().Substring(1);
            _hexValueTextBox.TextChanged += OnHexValueChanged;
        }


        private void OnHsvChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            // Set new color and update controls.
            _changedComponents |= (ColorComponents)((FrameworkElement)sender).Tag;
            Color newColor = ColorHelper.FromHsv(
              _colorHControl.Value * 360,
              _colorSControl.Value * 100,
              _colorVControl.Value * 100);
            newColor.A = (byte)(_colorAControl.Value * 255);
            Color = newColor;
            UpdateControls();
        }


        private void OnRgbChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            // Set new color and update controls.
            _changedComponents |= (ColorComponents)((FrameworkElement)sender).Tag;
            Color = new Color
            {
                R = (byte)(_colorRControl.Value * 255),
                G = (byte)(_colorGControl.Value * 255),
                B = (byte)(_colorBControl.Value * 255),
                A = (byte)(_colorAControl.Value * 255),
            };
            UpdateControls();
        }


        private void OnAlphaChanged(object sender, RoutedPropertyChangedEventArgs<double> eventArgs)
        {
            // Set new color and update controls.
            _changedComponents |= ColorComponents.Alpha;
            Color newColor = Color;
            newColor.A = (byte)(eventArgs.NewValue * 255);
            Color = newColor;
            UpdateControls();
        }


        private static void OnColorChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorPicker)dependencyObject;

            // Call UpdateControls() if the color was set directly by the user.
            if (control._changedComponents == ColorComponents.None)
            {
                control._changedComponents = ColorComponents.All;
                control.UpdateControls();
            }

            var newEventArgs = new RoutedPropertyChangedEventArgs<Color>(
              (Color)eventArgs.OldValue,
              (Color)eventArgs.NewValue,
              ColorChangedEvent);
            control.OnColorChanged(newEventArgs);
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // Now all controls are loaded. Set the initial settings.
            _changedComponents = ColorComponents.All;
            UpdateControls();
        }
        

        private void OnRadioButtonChecked(object sender, RoutedEventArgs eventArgs)
        {
            // Change the SelectedComponent according to the new checked radio button.
            if (sender == _buttonH)
                SelectedComponent = ColorComponents.Hue;
            else if (sender == _buttonS)
                SelectedComponent = ColorComponents.Saturation;
            else if (sender == _buttonV)
                SelectedComponent = ColorComponents.Value;
            else if (sender == _buttonR)
                SelectedComponent = ColorComponents.Red;
            else if (sender == _buttonG)
                SelectedComponent = ColorComponents.Green;
            else if (sender == _buttonB)
                SelectedComponent = ColorComponents.Blue;

            // Another color component was selected: The detail area bitmap must be updated.
            _detailAreaBitmapIsValid = false;

            if (IsLoaded)
                UpdateControls();
        }


        private static void OnSelectedComponentChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var picker = (ColorPicker)dependencyObject;

            // Return if the radio buttons have not been loaded yet.
            if (picker._buttonH == null)
                return;

            // Check the correct radio button.
            if ((picker.SelectedComponent & ColorComponents.Hue) > 0)
                picker._buttonH.IsChecked = true;
            else if ((picker.SelectedComponent & ColorComponents.Saturation) > 0)
                picker._buttonS.IsChecked = true;
            else if ((picker.SelectedComponent & ColorComponents.Value) > 0)
                picker._buttonV.IsChecked = true;
            else if ((picker.SelectedComponent & ColorComponents.Red) > 0)
                picker._buttonR.IsChecked = true;
            else if ((picker.SelectedComponent & ColorComponents.Green) > 0)
                picker._buttonG.IsChecked = true;
            else if ((picker.SelectedComponent & ColorComponents.Blue) > 0)
                picker._buttonB.IsChecked = true;
            else
                picker._buttonH.IsChecked = true;   // H is the default.
        }


        /// <summary>
        /// Called when the <see cref="ColorSpace"/> property changed.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <param name="eventArgs">
        /// The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.
        /// </param>
        private static void OnColorSpaceChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorPicker)dependencyObject;
            ColorSpace oldValue = (ColorSpace)eventArgs.OldValue;
            ColorSpace newValue = (ColorSpace)eventArgs.NewValue;
            control.OnColorSpaceChanged(oldValue, newValue);
        }


        /// <summary>
        /// Called when the <see cref="ColorSpace"/> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnColorSpaceChanged(ColorSpace oldValue, ColorSpace newValue)
        {
            _colorSpaceConverter.TargetColorSpace = newValue;

            if (_colorSpaceSelector != null)
                _colorSpaceSelector.SelectedIndex = (int)newValue;

            _colorRControl.Update();
            _colorGControl.Update();
            _colorBControl.Update();
        }


        private void OnColorSpaceSelectionChanged(object sender, SelectionChangedEventArgs eventArgs)
        {
            ColorSpace = (ColorSpace)_colorSpaceSelector.SelectedIndex;
        }


        /// <summary>
        /// Registers the event handlers for the value changed events.
        /// </summary>
        private void RegisterEventHandlers()
        {
            _colorHControl.ValueChanged += OnHsvChanged;
            _colorSControl.ValueChanged += OnHsvChanged;
            _colorVControl.ValueChanged += OnHsvChanged;
            _colorRControl.ValueChanged += OnRgbChanged;
            _colorGControl.ValueChanged += OnRgbChanged;
            _colorBControl.ValueChanged += OnRgbChanged;
            _colorAControl.ValueChanged += OnAlphaChanged;

            _hexValueTextBox.TextChanged += OnHexValueChanged;
            _hexValueTextBox.LostFocus += OnHexValueLostFocus;
            _slider1D.ValueChanged += OnDetailAreaChanged;
            _slider2D.ValueChanged += OnDetailAreaChanged;
        }


        /// <summary>
        /// Remove the event handlers for the value changed events.
        /// </summary>
        private void UnregisterEventHandlers()
        {
            _colorHControl.ValueChanged -= OnHsvChanged;
            _colorSControl.ValueChanged -= OnHsvChanged;
            _colorVControl.ValueChanged -= OnHsvChanged;
            _colorRControl.ValueChanged -= OnRgbChanged;
            _colorGControl.ValueChanged -= OnRgbChanged;
            _colorBControl.ValueChanged -= OnRgbChanged;
            _colorAControl.ValueChanged -= OnAlphaChanged;

            _hexValueTextBox.TextChanged -= OnHexValueChanged;
            _hexValueTextBox.LostFocus -= OnHexValueLostFocus;
            _slider1D.ValueChanged -= OnDetailAreaChanged;
            _slider2D.ValueChanged -= OnDetailAreaChanged;
        }


        /// <summary>
        /// Updates the controls.
        /// </summary>
        private void UpdateControls()
        {
            // Abort if the controls have not been loaded yet.
            if (_colorHControl == null)
                return;

            // Remove event handlers to avoid update loops.
            UnregisterEventHandlers();

            if (_detailAreaChanged)
            {
                // Sync detail area with color component controls.
                switch ((ColorComponents)_slider1D.Tag)
                {
                    case ColorComponents.Hue:
                        _colorHControl.Value = _slider1D.Value;
                        _colorSControl.Value = _slider2D.Value.X;
                        _colorVControl.Value = _slider2D.Value.Y;
                        break;
                    case ColorComponents.Saturation:
                        _colorSControl.Value = _slider1D.Value;
                        _colorHControl.Value = _slider2D.Value.X;
                        _colorVControl.Value = _slider2D.Value.Y;
                        break;
                    case ColorComponents.Value:
                        _colorVControl.Value = _slider1D.Value;
                        _colorHControl.Value = _slider2D.Value.X;
                        _colorSControl.Value = _slider2D.Value.Y;
                        break;
                    case ColorComponents.Red:
                        _colorRControl.Value = _slider1D.Value;
                        _colorGControl.Value = _slider2D.Value.X;
                        _colorBControl.Value = _slider2D.Value.Y;
                        break;
                    case ColorComponents.Green:
                        _colorGControl.Value = _slider1D.Value;
                        _colorRControl.Value = _slider2D.Value.X;
                        _colorBControl.Value = _slider2D.Value.Y;
                        break;
                    case ColorComponents.Blue:
                        _colorBControl.Value = _slider1D.Value;
                        _colorRControl.Value = _slider2D.Value.X;
                        _colorGControl.Value = _slider2D.Value.Y;
                        break;
                    default:
                        Debug.Fail("Unhandled case in switch.");
                        break;
                }
            }

            // Update child controls - but only if necessary.
            // We must take care because the precision in Color is less than the precision of the
            // child control values.
            // _changedComponents tells us which component has changed. 

            // ----- Alpha
            if ((_changedComponents & ColorComponents.Alpha) > 0)
                _colorAControl.Value = Color.A / 255.0;

            // ----- HSV
            if ((_changedComponents & (ColorComponents.Red | ColorComponents.Green | ColorComponents.Blue)) > 0)
            {
                // RGB has changed:

                double h, s, v;
                Color.ToHsv(out h, out s, out v);

                if ((int)(_colorVControl.Value * 100) != (int)v)
                    _colorVControl.Value = v / 100.0;
                if ((int)(_colorSControl.Value * 100) != (int)s && v > 0)
                    _colorSControl.Value = s / 100.0;

                // We cannot simply compare _colorHControl.Value with h because different hue values
                // can lead to the same RGB color. It is better to compare the resulting colors.
                Color hsvColor = ColorHelper.FromHsv(_colorHControl.Value * 360, s, v);
                hsvColor.A = Color.A;
                if (hsvColor != Color && s > 0 && v > 0)
                    _colorHControl.Value = h / 360.0;
            }

            // ----- RGB
            if ((_changedComponents & (ColorComponents.Hue | ColorComponents.Saturation | ColorComponents.Value)) > 0)
            {
                // HSV has changed:

                if ((int)(_colorRControl.Value * 255) != Color.R)
                    _colorRControl.Value = Color.R / 255.0;
                if ((int)(_colorGControl.Value * 255) != Color.G)
                    _colorGControl.Value = Color.G / 255.0;
                if ((int)(_colorBControl.Value * 255) != Color.B)
                    _colorBControl.Value = Color.B / 255.0;
            }

            // Update gradients of color component controls.
            UpdateHsvGradients();
            UpdateRgbGradients();
            UpdateAlphaGradient();

            // Update detail area (gradients and values).
            UpdateDetailArea();

            // Update color fields.
            _oldColorNoAlphaShape.Fill = new SolidColorBrush(new Color { R = OldColor.R, G = OldColor.G, B = OldColor.B, A = 255 });
            _oldColorShape.Fill = new SolidColorBrush(OldColor);
            _newColorNoAlphaShape.Fill = new SolidColorBrush(new Color { R = Color.R, G = Color.G, B = Color.B, A = 255 });
            _newColorShape.Fill = new SolidColorBrush(Color);

            // Update hex text.
            Color? color = GetColorFromHexValue();
            if (color == null || color != Color)
                _hexValueTextBox.Text = Color.ToString().Substring(1);

            _changedComponents = ColorComponents.None;
            _detailAreaChanged = false;
            RegisterEventHandlers();
        }


        private void UpdateHsvGradients()
        {
            // Hue gradient
            var stops = new GradientStopCollection();
            for (int offset = 0; offset <= 36; offset++)
                stops.Add(new GradientStop(ColorHelper.FromHsv(offset * 10, _colorSControl.Value * 100.0, _colorVControl.Value * 100.0), offset / 36.0));
            _colorHControl.SliderBrush = new LinearGradientBrush(stops, 0);

            // Saturation gradient
            stops = new GradientStopCollection
            {
                new GradientStop(ColorHelper.FromHsv(_colorHControl.Value * 360.0, 0, _colorVControl.Value * 100.0), 0),
                new GradientStop(ColorHelper.FromHsv(_colorHControl.Value * 360.0, 100, _colorVControl.Value * 100.0), 1)
            };
            _colorSControl.SliderBrush = new LinearGradientBrush(stops, 0);

            // Value gradient
            stops = new GradientStopCollection
            {
                new GradientStop(ColorHelper.FromHsv(_colorHControl.Value * 360.0, _colorSControl.Value * 100.0, 0), 0),
                new GradientStop(ColorHelper.FromHsv(_colorHControl.Value * 360.0, _colorSControl.Value * 100.0, 100), 1)
            };
            _colorVControl.SliderBrush = new LinearGradientBrush(stops, 0);
        }


        private void UpdateRgbGradients()
        {
            // Red gradient
            var stops = new GradientStopCollection
            {
                new GradientStop(new Color { R = 0, G = Color.G, B = Color.B, A = 255 }, 0),
                new GradientStop(new Color { R = 255, G = Color.G, B = Color.B, A = 255 }, 1)
            };
            _colorRControl.SliderBrush = new LinearGradientBrush(stops, 0);

            // Green gradient
            stops = new GradientStopCollection
            {
                new GradientStop(new Color { R = Color.R, G = 0, B = Color.B, A = 255 }, 0),
                new GradientStop(new Color { R = Color.R, G = 255, B = Color.B, A = 255 }, 1)
            };
            _colorGControl.SliderBrush = new LinearGradientBrush(stops, 0);

            // Blue gradient
            stops = new GradientStopCollection
            {
                new GradientStop(new Color { R = Color.R, G = Color.G, B = 0, A = 255 }, 0),
                new GradientStop(new Color { R = Color.R, G = Color.G, B = 255, A = 255 }, 1)
            };
            _colorBControl.SliderBrush = new LinearGradientBrush(stops, 0);
        }


        private void UpdateAlphaGradient()
        {
            var stops = new GradientStopCollection
            {
                new GradientStop(new Color { R = Color.R, G = Color.G, B = Color.B, A = 0 }, 0),
                new GradientStop(new Color { R = Color.R, G = Color.G, B = Color.B, A = 255 }, 1)
            };
            _colorAControl.SliderBrush = new LinearGradientBrush(stops, 0);
        }


        private void UpdateDetailArea()
        {
            // Update detail 1D slider (brush and value) and 2D slider (image and value).
            // The content depends on the selection of the radio buttons.
            if (_buttonH.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection();
                for (int offset = 0; offset <= 36; offset++)
                    stops.Add(new GradientStop(ColorHelper.FromHsv(offset * 10, 100, 100), offset / 36.0));
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Hue;
                _slider2D.Tag = ColorComponents.Saturation | ColorComponents.Value;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorHControl.Value;
                    _slider2D.Value = new Point { X = _colorSControl.Value, Y = _colorVControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Hue | ColorComponents.Red | ColorComponents.Green | ColorComponents.Blue)) > 0
                  || _detailAreaBitmapIsValid == false)
                    UpdateDetailAreaBitmap();
            }
            else if (_buttonS.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection
                {
                    new GradientStop(ColorHelper.FromHsv(0, 0, 0), 0),
                    new GradientStop(ColorHelper.FromHsv(0, 0, 100), 1)
                };
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Saturation;
                _slider2D.Tag = ColorComponents.Hue | ColorComponents.Value;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorSControl.Value;
                    _slider2D.Value = new Point { X = _colorHControl.Value, Y = _colorVControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Saturation | ColorComponents.Red | ColorComponents.Green | ColorComponents.Blue)) > 0
                  || _detailAreaBitmapIsValid == false)
                    UpdateDetailAreaBitmap();
            }
            else if (_buttonV.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection
                {
                    new GradientStop(ColorHelper.FromHsv(0, 0, 0), 0),
                    new GradientStop(ColorHelper.FromHsv(0, 0, 100), 1)
                };
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Value;
                _slider2D.Tag = ColorComponents.Hue | ColorComponents.Saturation;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorVControl.Value;
                    _slider2D.Value = new Point { X = _colorHControl.Value, Y = _colorSControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Value | ColorComponents.Red | ColorComponents.Green | ColorComponents.Blue)) > 0
                  || _detailAreaBitmapIsValid == false)
                    UpdateDetailAreaBitmap();
            }
            else if (_buttonR.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection
                {
                    new GradientStop(new Color { R = 0, B = 0, G = 0, A = 255 }, 0),
                    new GradientStop(new Color { R = 255, B = 0, G = 0, A = 255 }, 1)
                };
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Red;
                _slider2D.Tag = ColorComponents.Green | ColorComponents.Blue;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorRControl.Value;
                    _slider2D.Value = new Point { X = _colorGControl.Value, Y = _colorBControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Hue | ColorComponents.Saturation | ColorComponents.Value | ColorComponents.Red)) > 0
                  || _detailAreaBitmapIsValid == false)
                    UpdateDetailAreaBitmap();
            }
            else if (_buttonG.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection
                {
                    new GradientStop(new Color { R = 0, B = 0, G = 0, A = 255 }, 0),
                    new GradientStop(new Color { R = 0, B = 0, G = 255, A = 255 }, 1)
                };
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Green;
                _slider2D.Tag = ColorComponents.Red | ColorComponents.Blue;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorGControl.Value;
                    _slider2D.Value = new Point { X = _colorRControl.Value, Y = _colorBControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Hue | ColorComponents.Saturation | ColorComponents.Value | ColorComponents.Green)) > 0
                    || _detailAreaBitmapIsValid == false)
                {
                    UpdateDetailAreaBitmap();
                }
            }
            else if (_buttonB.IsChecked.GetValueOrDefault())
            {
                var stops = new GradientStopCollection
                {
                    new GradientStop(new Color { R = 0, B = 0, G = 0, A = 255 }, 0),
                    new GradientStop(new Color { R = 0, B = 255, G = 0, A = 255 }, 1)
                };
                _slider1D.Foreground = new LinearGradientBrush(stops, 0);
                _slider1D.Tag = ColorComponents.Blue;
                _slider2D.Tag = ColorComponents.Red | ColorComponents.Green;
                if (!_detailAreaChanged)
                {
                    _slider1D.Value = _colorBControl.Value;
                    _slider2D.Value = new Point { X = _colorRControl.Value, Y = _colorGControl.Value };
                }

                if ((_changedComponents & (ColorComponents.Hue | ColorComponents.Saturation | ColorComponents.Value | ColorComponents.Blue)) > 0
                    || _detailAreaBitmapIsValid == false)
                {
                    UpdateDetailAreaBitmap();
                }
            }
        }


        private void UpdateDetailAreaBitmap()
        {
            // Update bitmap of 2d slider if necessary.

            if (_slider2D.ActualWidth > 0 && _slider2D.ActualHeight > 0)
            {
                int bitmapWidth = (int)_slider2D.ActualWidth;
                int bitmapHeight = (int)_slider2D.ActualHeight;
                if (_detailAreaBitmap == null || _detailAreaBitmap.Width != bitmapWidth || _detailAreaBitmap.Height != bitmapHeight)
                {
                    _detailAreaBitmap = new WriteableBitmap(
                      bitmapWidth,
                      bitmapHeight,
                      96,
                      96,
                      PixelFormats.Bgra32,
                      new BitmapPalette(new List<Color> { Colors.Black }));
                    _slider2D.ImageSource = _detailAreaBitmap;
                }

                int pixelBufferSize = bitmapWidth * bitmapHeight * 4;
                if (_pixelBuffer == null || _pixelBuffer.Length != pixelBufferSize)
                {
                    _pixelBuffer = new byte[pixelBufferSize];
                    for (int i = 0; i < pixelBufferSize; i++)
                        _pixelBuffer[i] = 255;
                }

                Fill2DArea(_pixelBuffer, bitmapWidth, bitmapHeight, _slider1D.Value, (ColorComponents)_slider1D.Tag);

                _detailAreaBitmap.WritePixels(new Int32Rect(0, 0, bitmapWidth, bitmapHeight), _pixelBuffer, bitmapWidth * 4, 0);
                _detailAreaBitmapIsValid = true;
            }
            else
            {
                _slider2D.ImageSource = null;
            }
        }


        /// <summary>
        /// Fills the pixel buffer.
        /// </summary>
        /// <param name="pixelBuffer">The pixel buffer.</param>
        /// <param name="width">The bitmap width.</param>
        /// <param name="height">The bitmap height.</param>
        /// <param name="value">The value given by the 1D color slider.</param>
        /// <param name="component">The component that is set by the 1D color slider.</param>
        private static void Fill2DArea(byte[] pixelBuffer, int width, int height, double value, ColorComponents component)
        {
            switch (component)
            {
                case ColorComponents.Hue:
                    {
                        int h = (int)(value * 360);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int offset = 4 * (y * width + x);
                                double s = x * 1.0 / width * 100;
                                double v = (height - y) * 1.0 / height * 100;
                                Color color = ColorHelper.FromHsv(h, s, v);
                                pixelBuffer[offset + 0] = color.B;
                                pixelBuffer[offset + 1] = color.G;
                                pixelBuffer[offset + 2] = color.R;
                            }
                        }
                    }
                    break;

                case ColorComponents.Saturation:
                    {
                        int s = (int)(value * 100);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int offset = 4 * (y * width + x);
                                double h = x * 1.0 / width * 360;
                                double v = (height - y) * 1.0 / height * 100;
                                Color color = ColorHelper.FromHsv(h, s, v);
                                pixelBuffer[offset + 0] = color.B;
                                pixelBuffer[offset + 1] = color.G;
                                pixelBuffer[offset + 2] = color.R;
                            }
                        }
                    }
                    break;

                case ColorComponents.Value:
                    {
                        int v = (int)(value * 100);
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                int offset = 4 * (y * width + x);
                                double h = x * 1.0 / width * 360;
                                double s = (height - y) * 1.0 / height * 100;
                                Color color = ColorHelper.FromHsv(h, s, v);
                                pixelBuffer[offset + 0] = color.B;
                                pixelBuffer[offset + 1] = color.G;
                                pixelBuffer[offset + 2] = color.R;
                            }
                        }
                    }
                    break;

                case ColorComponents.Red:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = 4 * (y * width + x);
                            pixelBuffer[offset + 0] = (byte)((height - y) * 1.0 / height * 255);
                            pixelBuffer[offset + 1] = (byte)(x * 1.0 / width * 255);
                            pixelBuffer[offset + 2] = (byte)(value * 255);
                        }
                    }
                    break;

                case ColorComponents.Green:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = 4 * (y * width + x);
                            pixelBuffer[offset + 0] = (byte)((height - y) * 1.0 / height * 255);
                            pixelBuffer[offset + 1] = (byte)(value * 255);
                            pixelBuffer[offset + 2] = (byte)(x * 1.0 / width * 255);
                        }
                    }
                    break;

                case ColorComponents.Blue:
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int offset = 4 * (y * width + x);
                            pixelBuffer[offset + 0] = (byte)(value * 255);
                            pixelBuffer[offset + 1] = (byte)((height - y) * 1.0 / height * 255);
                            pixelBuffer[offset + 2] = (byte)(x * 1.0 / width * 255);
                        }
                    }
                    break;

                default:
                    Debug.Fail("Unhandled case in switch.");
                    break;
            }
        }


        #region ----- EyeDropper Color Picking -----

        private void OnEyeDropperButtonClicked(object sender, RoutedEventArgs eventArgs)
        {
            // Enter eye dropper mode: Capture mouse.
            CaptureMouse();
            _isInEyeDropperMode = true;
            _colorBeforeEyeDropperClicked = Color;
            Cursor = Cursors.Cross;

            // Determine Screen coordinates in GDI+ space.
            int left = int.MaxValue, top = int.MaxValue;
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                left = Math.Min(left, screen.Bounds.Left);
                top = Math.Min(top, screen.Bounds.Top);
            }

            // Show an invisible window that covers the whole desktop - otherwise we would lose
            // mouse capture outside of our window.
            _invisibleFullScreenWindow = new Window
            {
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight,
                Topmost = true,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                AllowsTransparency = true,
                Background = new SolidColorBrush(new Color { R = 255, G = 255, B = 255, A = 1 }),
                WindowStyle = WindowStyle.None,
                IsHitTestVisible = true,
                ShowActivated = false,
                ShowInTaskbar = false
            };
            _invisibleFullScreenWindow.Show();

            // Make a screenshot (using GDI+ bitmap).
            Point nativeSize = ToDevice(new Point(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight));
            _screenshot = new System.Drawing.Bitmap((int)nativeSize.X, (int)nativeSize.Y);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(_screenshot);
            graphics.CopyFromScreen(left, top, 0, 0, _screenshot.Size);
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

            // Release mouse capture if it was 'Escape' - this will end the eye dropper mode (see
            // OnLostMouseCapture).

            if (!_isInEyeDropperMode)
                return;

            if (e.Key == Key.Escape)
            {
                ReleaseMouseCapture();
                Color = _colorBeforeEyeDropperClicked;
                e.Handled = true;
            }
        }


        /// <summary>
        /// Raises the <see cref="UIElement.MouseMove"/> event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> instance containing the event data.
        /// </param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            base.OnMouseMove(e);

            if (!_isInEyeDropperMode)
                return;

            // We are in eye dropper mode. Get the pixel of the screenshot at the mouse position.
            Point mousePosition = ToDevice(e.GetPosition(_invisibleFullScreenWindow));
            System.Drawing.Color c = _screenshot.GetPixel((int)mousePosition.X, (int)mousePosition.Y);
            Color = new Color { R = c.R, G = c.G, B = c.B, A = c.A };
            e.Handled = true;
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
            if (!_isInEyeDropperMode)
                return;

            // We are in eye dropper mode. Get the pixel of the screenshot at the mouse position.
            Point mousePosition = ToDevice(Mouse.GetPosition(_invisibleFullScreenWindow));
            System.Drawing.Color c = _screenshot.GetPixel((int)mousePosition.X, (int)mousePosition.Y);
            Color = new Color { R = c.R, G = c.G, B = c.B, A = c.A };
            ReleaseMouseCapture();
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.LostMouseCapture"/> attached event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseEventArgs"/> that contains event data.
        /// </param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            // End eye dropper mode.
            base.OnLostMouseCapture(e);
            
            // Dispose the screenshot.
            _screenshot?.Dispose();
            _screenshot = null;

            _isInEyeDropperMode = false;
            Cursor = null;

            // Close invisible window.
            _invisibleFullScreenWindow?.Close();
        }


        // Transforms position from device-independent pixels to device coordinates.
        private Point ToDevice(Point position)
        {
            Debug.Assert(_invisibleFullScreenWindow != null, "Invisible window is missing. (Method only works during color picking.)");

            var presentationSource = PresentationSource.FromVisual(_invisibleFullScreenWindow);
            if (presentationSource?.CompositionTarget != null)
                position = presentationSource.CompositionTarget.TransformToDevice.Transform(position);

            return position;
        }
        #endregion

        #endregion
    }
}
