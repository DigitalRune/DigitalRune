// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;
using System.Windows;


namespace DigitalRune.Editor.Output
{
    /// <summary>
    /// Represents the Output window.
    /// </summary>
    partial class OutputView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputView"/> class.
        /// </summary>
        public OutputView()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            // When document is assigned for the first time, scroll to end.
            EventHandler handler = null;
            handler = (s, e) =>
            {
                TextEditor.TextArea.Caret.Offset = TextEditor.Document.TextLength;
                TextEditor.ScrollToEnd();
                TextEditor.DocumentChanged -= handler;
            };
            TextEditor.DocumentChanged += handler;
        }


        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            // Inject TextEditor control into view model.
            var viewModel = (OutputViewModel)DataContext;
            viewModel.TextEditor = TextEditor;
        }
    }
}
