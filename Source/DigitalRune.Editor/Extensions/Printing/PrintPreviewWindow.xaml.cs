// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Documents;


namespace DigitalRune.Editor.Printing
{
    /// <summary>
    /// Shows a print preview.
    /// </summary>
    /// <remarks>
    /// This window contains a <see cref="System.Windows.Controls.DocumentViewer"/> to display a
    /// print document before printing. To specify the document that should be printed, you can
    /// either set the <see cref="FrameworkElement.DataContext"/> to a view model that implements
    /// <see cref="IPrintDocumentProvider"/>, or set the <see cref="PrintDocument"/> property (and
    /// leave <see cref="FrameworkElement.DataContext"/> unchanged).
    /// </remarks>
    partial class PrintPreviewWindow : IPrintDocumentProvider
    {
        /// <summary>
        /// Gets or sets the print document.
        /// </summary>
        /// <value>The print document.</value>
        public IDocumentPaginatorSource PrintDocument { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PrintPreviewWindow"/> class.
        /// </summary>
        public PrintPreviewWindow()
        {
            // The PrintPreviewWindow can be used stand-alone: Per default the DataContext is set to
            // the window itself.
            DataContext = this;

            InitializeComponent();
        }
    }
}
