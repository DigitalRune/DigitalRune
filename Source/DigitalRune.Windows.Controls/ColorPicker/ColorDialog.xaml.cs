// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.ComponentModel;
using System.Windows;
using System.Windows.Media;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Allows the user to select a color from a <see cref="Controls.ColorPicker"/>.
    /// </summary>
    public partial class ColorDialog
    {
        // Notes:
        // This dialog is not styleable. If you need a different style, just create your own
        // and use this code as a starting point.


        /// <summary>
        /// Gets or sets the old color.
        /// </summary>
        /// <value>The old color.</value>
        /// <remarks>
        /// This color will be displayed in the dialog to compare with the new color.
        /// </remarks>
        [TypeConverter(typeof(ColorConverter))]
        public Color OldColor
        {
            get { return ColorPicker.OldColor; }
            set { ColorPicker.OldColor = value; }
        }


        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// <remarks>
        /// The color selected with the <see cref="Controls.ColorPicker"/>.
        /// </remarks>
        [TypeConverter(typeof(ColorConverter))]
        public Color Color
        {
            get { return ColorPicker.Color; }
            set { ColorPicker.Color = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialog"/> class.
        /// </summary>
        public ColorDialog()
        {
            InitializeComponent();
        }


        private void OnOkClick(object sender, RoutedEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            DialogResult = true;
            Hide();
        }


        private void OnCancelClick(object sender, RoutedEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            DialogResult = false;
            Hide();
            Color = OldColor;
        }
    }
}
