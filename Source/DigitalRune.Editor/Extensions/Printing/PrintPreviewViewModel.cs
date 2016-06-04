// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace DigitalRune.Editor.Printing
{
    /// <summary>
    /// Represents a "Print Preview" window.
    /// </summary>
    public class PrintPreviewViewModel : Screen, IPrintDocumentProvider
    {
        /// <summary>
        /// Gets or sets the print document.
        /// </summary>
        /// <value>The print document.</value>
        public IDocumentPaginatorSource PrintDocument { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="PrintPreviewViewModel"/> class.
        /// </summary>
        public PrintPreviewViewModel()
        {
            if (WindowsHelper.IsInDesignMode)
            {
                // At runtime use:
                //PrintDialog printDialog = new PrintDialog();
                //Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);

                Size pageSize = new Size(793.700787401575, 1122.51968503937);
                PrintDocument = CreateDesignTimeDocument(pageSize);
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
        private static IDocumentPaginatorSource CreateDesignTimeDocument(Size pageSize)
        {
            var document = new FixedDocument();
            document.DocumentPaginator.PageSize = pageSize;

            // ---- Page 1
            var page1 = new FixedPage { Width = pageSize.Width, Height = pageSize.Height };

            var page1Text = new TextBlock
            {
                FontSize = 16,
                Margin = new Thickness(96),
                Text = "Lorem ipsum dolor sit amet."
            };
            page1.Children.Add(page1Text);

            var page1Content = new PageContent();
            ((IAddChild)page1Content).AddChild(page1);
            document.Pages.Add(page1Content);

            // ---- Page 2
            // ...

            return document;
        }


        ///// <summary>
        ///// Prints the document.
        ///// </summary>
        //public void Print(string printJobDescription)
        //{
        //    if (PrintDocument == null)
        //        throw new EditorException("Cannot print document. The property PrintDocument is not set.");

        //    var printDialog = new PrintDialog();
        //    if (printDialog.ShowDialog() == true)
        //        printDialog.PrintDocument(PrintDocument.DocumentPaginator, "MyPrintJobName");
        //}
    }
}
