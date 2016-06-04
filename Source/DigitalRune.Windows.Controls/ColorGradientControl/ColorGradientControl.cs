// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Represents a control that lets the user edit a color gradient.
    /// </summary>
    /// <remarks>
    /// The color gradient that is shown by the <see cref="ColorGradientControl"/> needs to be set
    /// using the <see cref="GradientStops"/> property. The control is empty and read-only if the
    /// <see cref="GradientStops"/> property is <see langword="null"/>.
    /// </remarks>
    /// <example>
    /// In the following example a new <see cref="ColorGradientControl"/> is created by code that
    /// contains black-to-white color gradient.
    /// <code lang="csharp">
    /// <![CDATA[
    ///  GradientStopCollection gradientStops = new GradientStopCollection
    /// {
    ///   new GradientStop(Colors.Black, 0.0),
    ///   new GradientStop(Colors.White, 1.0),
    /// };
    /// 
    /// ColorGradientControl colorGradientControl = new ColorGradientControl();
    /// colorGradientControl.GradientStops = gradientStops;
    /// ]]>
    /// </code>
    /// </example>
    /// <example>
    /// In the following example the same <see cref="ColorGradientControl"/> control is created in
    /// XAML inside a <see cref="UserControl"/>. The <see cref="GradientStopCollection"/> be defined
    /// inline, as a resource or can be set via data binding. In this example the
    /// <see cref="GradientStopCollection"/> is defined as a local resource of the
    /// <see cref="UserControl"/>.
    /// <code lang="xaml">
    /// <![CDATA[
    /// <UserControl x:Class="MyApplication.MyUserControl"
    ///              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    ///              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    ///              xmlns:dr="http://schemas.digitalrune.com/windows">
    /// 
    ///   <UserControl.Resources>
    ///     <GradientStopCollection x:Key="GradientStops">
    ///       <GradientStop Color="Black" Offset="0"/>
    ///       <GradientStop Color="White" Offset="1"/>
    ///     </GradientStopCollection>
    ///   </UserControl.Resources>
    /// 
    ///   <Grid>
    ///     <dr:ColorGradientControl Height="48"
    ///                              GradientStops="{StaticResource GradientStops}"/>
    ///   </Grid>
    /// </UserControl>
    /// ]]>
    /// </code>
    /// </example>
    [TemplatePart(Name = "PART_ColorStopPanel", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_GradientArea", Type = typeof(FrameworkElement))]
    [ContentProperty("GradientStops")]
    public class ColorGradientControl : Control
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private int _selectedIndex = -1;
        private double _mouseOffsetToColorStop;
        private Canvas _colorStopPanel;
        private FrameworkElement _gradientArea;
        #endregion


        //--------------------------------------------------------------
        #region Properties
        //--------------------------------------------------------------
        #endregion


        //--------------------------------------------------------------
        #region Dependency Properties
        //--------------------------------------------------------------

        /// <summary>
        /// Identifies the <see cref="ColorStopSize"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorStopSizeProperty = DependencyProperty.Register(
            "ColorStopSize",
            typeof(Size),
            typeof(ColorGradientControl),
            new FrameworkPropertyMetadata(new Size { Width = 13, Height = 16 }));

        /// <summary>
        /// Gets or sets the size for the color stop controls in device-independent pixels.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// The width and height of the color stop indicator in device-independent pixels.
        /// The default size is 13 x 16 px.
        /// </value>
        [Description("Gets or sets the size for the color stop controls.")]
        [Category(Categories.Layout)]
        [TypeConverter(typeof(SizeConverter))]
        public Size ColorStopSize
        {
            get { return (Size)GetValue(ColorStopSizeProperty); }
            set { SetValue(ColorStopSizeProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="GradientStops"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GradientStopsProperty = DependencyProperty.Register(
            "GradientStops",
            typeof(GradientStopCollection),
            typeof(ColorGradientControl),
            new FrameworkPropertyMetadata(null, OnGradientStopsChanged));

        /// <summary>
        /// Gets or sets the gradient stops.
        /// This is a dependency property.
        /// </summary>
        /// <value>
        /// A collection of <see cref="GradientStop"/>s. The default value is
        /// <see langword="null"/>.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [Description("Gets or sets gradient stops.")]
        [Category(Categories.Default)]
        public GradientStopCollection GradientStops
        {
            get { return (GradientStopCollection)GetValue(GradientStopsProperty); }
            set { SetValue(GradientStopsProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="GradientStopsChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent GradientStopsChangedEvent = EventManager.RegisterRoutedEvent(
            "GradientStopsChanged",
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<GradientStopCollection>),
            typeof(ColorGradientControl));

        /// <summary>
        /// Occurs when the <see cref="GradientStops"/> property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<GradientStopCollection> GradientStopsChanged
        {
            add { AddHandler(GradientStopsChangedEvent, value); }
            remove { RemoveHandler(GradientStopsChangedEvent, value); }
        }
        #endregion


        //--------------------------------------------------------------
        #region Creation and Cleanup
        //--------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ColorGradientControl"/> class.
        /// </summary>
        static ColorGradientControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorGradientControl), new FrameworkPropertyMetadata(typeof(ColorGradientControl)));
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
            if (_colorStopPanel != null)
            {
                RemoveColorStopControls();
                _colorStopPanel = null;
            }
            _gradientArea = null;

            base.OnApplyTemplate();

            _colorStopPanel = GetTemplateChild("PART_ColorStopPanel") as Canvas;
            _gradientArea = GetTemplateChild("PART_GradientArea") as FrameworkElement;
            UpdateColorStops();
        }


        private static void OnGradientStopsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var control = (ColorGradientControl)dependencyObject;

            var newEventArgs = new RoutedPropertyChangedEventArgs<GradientStopCollection>(
                (GradientStopCollection)eventArgs.OldValue,
                control.GradientStops,
                GradientStopsChangedEvent);

            control.OnGradientStopsChanged(newEventArgs);
        }


        /// <summary>
        /// Raises the <see cref="GradientStopsChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">
        /// Arguments associated with the <see cref="GradientStopsChanged"/> event.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        protected void OnGradientStopsChanged(RoutedPropertyChangedEventArgs<GradientStopCollection> eventArgs)
        {
            UpdateColorStops();
            RaiseEvent(eventArgs);
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.PreviewMouseDown"/> attached routed event
        /// reaches an element in its route that is derived from this class. Implement this method
        /// to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="MouseButtonEventArgs"/> that contains the event data. The event data
        /// reports that one or more mouse buttons were pressed.
        /// </param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            Focus();
        }


        /// <summary>
        /// Invoked when an unhandled <see cref="UIElement.KeyDown"/> attached event reaches an
        /// element in its route that is derived from this class. Implement this method to add class
        /// handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (_selectedIndex >= 0 && GradientStops != null)
            {
                if (e.Key == Key.Delete)
                {
                    // When Delete is pressed, the currently selected color stop is removed.
                    GradientStops.RemoveAt(_selectedIndex);
                    _selectedIndex = -1;
                    e.Handled = true;
                }
                else if (e.Key == Key.Right || e.Key == Key.Up)
                {
                    IncreaseWithModifiers(0.01);
                    e.Handled = true;
                }
                else if (e.Key == Key.Left || e.Key == Key.Down)
                {
                    IncreaseWithModifiers(-0.01);
                    e.Handled = true;
                }
                else if (e.Key == Key.Return)
                {
                    GradientStop gradientStop = GradientStops[_selectedIndex];
                    ShowColorDialog(gradientStop);
                    e.Handled = true;
                }
            }

            base.OnKeyDown(e);
        }


        /// <summary>
        /// Increases the value and handles modifier keys.
        /// </summary>
        /// <param name="increment">
        /// The step of the increment (can be negative to decrease the value).
        /// </param>
        private void IncreaseWithModifiers(double increment)
        {
            if (_selectedIndex < 0 || GradientStops == null)
                return;

            GradientStop gradientStop = GradientStops[_selectedIndex];
            double multiplier = 1;

            // Speedup if Shift is pressed.
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                multiplier *= 10;

            // Slow down if Control key is pressed.
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                multiplier *= 0.1;

            // Change offset.
            double offset = gradientStop.Offset;
            offset += increment * multiplier;
            offset = Math.Min(1, Math.Max(0, offset));
            gradientStop.Offset = offset;
        }


        /// <summary>
        /// Raises the <see cref="Control.MouseDoubleClick"/> routed event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (_colorStopPanel == null || _gradientArea == null || GradientStops == null)
                return;

            // Check if a color stop was hit.
            // If we handle the ColorStop.MouseDoubleClick event, then this.MouseDoubleClick
            // will still be raised - even if the event is marked as handled.
            // Therefore we handle double clicks on color stops here.
            var originalSource = e.OriginalSource as DependencyObject;
            if (originalSource != null)
            {
                var colorStops = _colorStopPanel.Children.OfType<ColorStop>();
                foreach (var colorStop in colorStops)
                {
                    if (colorStop != null && colorStop.IsAncestorOf(originalSource))
                    {
                        // A color stop was hit. Show the color picker dialog and 
                        // update the color.
                        GradientStop gradientStop = (GradientStop)colorStop.Tag;
                        ShowColorDialog(gradientStop);
                        e.Handled = true;
                        return;
                    }
                }
            }

            // Assertion: No color stop was hit.

            // Get gradient offset of mouse position.
            Point mousePosition = Mouse.GetPosition(_gradientArea);
            double offset = mousePosition.X / _gradientArea.ActualWidth;
            offset = Math.Min(1, Math.Max(0, offset));

            // Check if there are color stops with the same offset.
            var stopsWithThisOffset = GradientStops.Where(stop => Math.Abs(stop.Offset - offset) < 0.00001);
            if (!stopsWithThisOffset.Any())
            {
                // There is no matching stop at this position. --> Create a new one.

                // Determine color at this offset.
                // Sort colors stops.
                var sortedStops = GradientStops.OrderBy(stop => stop.Offset);

                // Find stops left and right of the offset.
                GradientStop leftStop = null;
                GradientStop rightStop = null;
                int index = 0;
                foreach (GradientStop stop in sortedStops)
                {
                    if (rightStop != null)
                        break;
                    if (stop.Offset < offset)
                        leftStop = stop;
                    else
                        rightStop = stop;
                    index++;
                }

                // Get color. If leftStop and rightStop are valid and distinct
                // the color is interpolated in sRGB.
                Color color;
                if (leftStop == null && rightStop == null)
                    color = Colors.Black;
                else if (leftStop == null)
                    color = rightStop.Color;
                else if (rightStop == null)
                    color = leftStop.Color;
                else
                {
                    double delta = rightStop.Offset - leftStop.Offset;
                    double k = (offset - leftStop.Offset) / delta;
                    color = new Color
                    {
                        R = (byte)(rightStop.Color.R * k + leftStop.Color.R * (1 - k)),
                        G = (byte)(rightStop.Color.G * k + leftStop.Color.G * (1 - k)),
                        B = (byte)(rightStop.Color.B * k + leftStop.Color.B * (1 - k)),
                        A = (byte)(rightStop.Color.A * k + leftStop.Color.A * (1 - k))
                    };
                }

                // Insert a new gradient stop and select it.
                GradientStops.Insert(index, new GradientStop(color, offset));
                _selectedIndex = index;
                SetSelection();
            }

            base.OnMouseDoubleClick(e);
        }


        private void OnColorStopMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            // Set the focus.
            var colorStop = (ColorStop)sender;
            colorStop.Focus();

            // Save x offset from the center of ColorStop to the mouse position.
            _mouseOffsetToColorStop = eventArgs.GetPosition(colorStop).X - colorStop.ActualWidth / 2.0;

            // Capture mouse to receive mouse move events if the mouse is outside of the control's area.
            CaptureMouse();

            eventArgs.Handled = true;
        }


        private void OnColorStopGotFocus(object sender, RoutedEventArgs eventArgs)
        {
            // Select this color stop.
            var stop = (ColorStop)sender;
            var gradientStop = (GradientStop)stop.Tag;
            _selectedIndex = GradientStops.IndexOf(gradientStop);
            SetSelection();
            eventArgs.Handled = true;
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

            if (_gradientArea == null || GradientStops == null)
                return;

            if (IsMouseCaptured && e.LeftButton == MouseButtonState.Pressed)
            {
                // Move selected color stop.
                var gradientStop = GradientStops[_selectedIndex];
                Point mousePosition = Mouse.GetPosition(_gradientArea);
                double offset = (mousePosition.X - _mouseOffsetToColorStop) / _gradientArea.ActualWidth;
                offset = Math.Min(1, Math.Max(0, offset));
                gradientStop.Offset = offset;

                // Update index. ColorStops could have switched places.
                _selectedIndex = GradientStops.IndexOf(gradientStop);
                SetSelection();
                e.Handled = true;
            }

            base.OnMouseMove(e);
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

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
                e.Handled = true;
            }

            base.OnMouseUp(e);
        }


        /// <summary>
        /// Raises the <see cref="FrameworkElement.SizeChanged"/> event, using the specified
        /// information as part of the eventual event data.
        /// </summary>
        /// <param name="sizeInfo">Details of the old and new size involved in the change.</param>
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateColorStops();
        }


        /// <summary>
        /// Sets the <see cref="ColorStop.IsSelected"/> properties of the <see cref="ColorStop"/>
        /// controls.
        /// </summary>
        /// <remarks>The selection is determined by _selectedIndex.</remarks>
        private void SetSelection()
        {
            if (_colorStopPanel == null || GradientStops == null)
                return;

            foreach (var child in _colorStopPanel.Children)
            {
                ColorStop control = child as ColorStop;
                if (control != null)
                {
                    GradientStop gradientStop = (GradientStop)control.Tag;
                    control.IsSelected = (_selectedIndex == GradientStops.IndexOf(gradientStop));
                }
            }
        }


        /// <summary>
        /// Updates the color stop controls in the ColorStopPanel.
        /// </summary>
        private void UpdateColorStops()
        {
            if (_colorStopPanel == null)
                return;

            // Remember the focused color stop.
            var oldFocusedStopControl = _colorStopPanel.Children.OfType<ColorStop>().FirstOrDefault(cs => cs.IsFocused);
            GradientStop oldFocusedStop = null;
            if (oldFocusedStopControl != null)
                oldFocusedStop = oldFocusedStopControl.Tag as GradientStop;

            // Remove previous ColorStop controls.
            RemoveColorStopControls();

            if (GradientStops != null)
            {
                // Create new ColorStop controls.
                foreach (GradientStop stop in GradientStops)
                {
                    var colorStop = new ColorStop
                    {
                        Color = stop.Color,
                        Focusable = true,
                        Tag = stop
                    };
                    colorStop.SetBinding(
                      WidthProperty, new Binding("ColorStopSize.Width") { Source = this, Mode = BindingMode.OneWay });
                    colorStop.SetBinding(
                      HeightProperty, new Binding("ColorStopSize.Height") { Source = this, Mode = BindingMode.OneWay });
                    colorStop.SetBinding(
                      SnapsToDevicePixelsProperty, new Binding("SnapsToDevicePixels") { Source = this, Mode = BindingMode.OneWay });
                    colorStop.GotFocus += OnColorStopGotFocus;
                    colorStop.MouseDown += OnColorStopMouseDown;
                    _colorStopPanel.Children.Add(colorStop);

                    // Restore focus.
                    if (oldFocusedStop != null)
                    {
                        if (stop.Color == oldFocusedStop.Color && stop.Offset == oldFocusedStop.Offset)
                        {
                            colorStop.Focus();
                            oldFocusedStop = null;
                        }
                    }

                    // Position control.
                    double x;
                    switch (colorStop.HorizontalAlignment)
                    {
                        case HorizontalAlignment.Left:
                            x = stop.Offset * ActualWidth;
                            break;
                        case HorizontalAlignment.Right:
                            x = stop.Offset * ActualWidth - colorStop.Width;
                            break;
                        default:
                            x = stop.Offset * ActualWidth - colorStop.Width / 2;
                            break;
                    }

                    Canvas.SetLeft(colorStop, x);
                }

                SetSelection();
            }
        }


        private void RemoveColorStopControls()
        {
            var colorStops = _colorStopPanel.Children
                                            .OfType<ColorStop>()
                                            .ToArray();
            foreach (var colorStop in colorStops)
                _colorStopPanel.Children.Remove(colorStop);
        }


        private static void ShowColorDialog(GradientStop gradientStop)
        {
            var colorDialog = new ColorDialog
            {
                OldColor = gradientStop.Color,
                Color = gradientStop.Color,
            };

            colorDialog.ShowDialog();

            if (colorDialog.DialogResult.GetValueOrDefault())
                gradientStop.Color = colorDialog.Color;
        }
        #endregion
    }
}
