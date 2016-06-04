// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Editor.Documents
{
    /// <summary>
    /// Represents the Save Changes dialog that prompts the user to save modified files.
    /// </summary>
    internal partial class SaveChangesView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveChangesView"/> class.
        /// </summary>
        public SaveChangesView()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            // Set initial focus.
            YesButton.Focus();
        }
    }
}
