// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Windows.Controls
{
    /// <summary>
    /// Allows the user to select a font using the <see cref="FontChooser"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="FontDialog"/> wraps the <see cref="Controls.FontChooser"/>. The property
    /// <see cref="Chooser"/> exposes the wrapped control.
    /// </remarks>
    public partial class FontDialog
    {
        /// <summary>
        /// Gets the <see cref="Controls.FontChooser"/>.
        /// </summary>
        /// <value>The <see cref="FontChooser"/>.</value>
        public FontChooser Chooser
        {
            get { return FontChooser; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FontDialog"/> class.
        /// </summary>
        public FontDialog()
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
        }
    }
}
