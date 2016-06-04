// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;


namespace DigitalRune.Editor.Text
{
    /// <summary>
    /// Displays a text document using the <see cref="ICSharpCode.AvalonEdit.TextEditor"/> control.
    /// </summary>
    partial class TextDocumentView
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextDocumentView"/> class.
        /// </summary>
        public TextDocumentView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }


        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs)
        {
            // Inject TextEditor control into view model.
            var oldViewModel = eventArgs.OldValue as TextDocumentViewModel;
            if (oldViewModel != null)
                oldViewModel.TextEditor = null;

            var newViewModel = eventArgs.NewValue as TextDocumentViewModel;
            if (newViewModel != null)
                newViewModel.TextEditor = TextEditor;
        }
    }
}
