using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Utils;


namespace ICSharpCode.AvalonEdit
{
    partial class TextEditor
    {
        /// <summary>
        /// Creates a <see cref="FixedDocument"/> containing the text.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="documentTitle">The document title (can be <see langword="null"/>).</param>
        /// <returns>A <see cref="FixedDocument"/>.</returns>
        /// <remarks>This document can be used for printing.</remarks>
        public FixedDocument CreateFixedDocument(Size pageSize, string documentTitle)
        {
            // Create fixed document with the specified page size.
            var fixedDocument = new FixedDocument();
            fixedDocument.DocumentPaginator.PageSize = pageSize;

            // We add a hardcoded border.
            var borderPadding = new Thickness(96, 90, 96, 80);

            // CreateFlowDocument() creates the highlighted, word-wrapped pages for us.
            var flowDocument = CreateFlowDocument(pageSize, borderPadding);
            var paginator = ((IDocumentPaginatorSource)flowDocument).DocumentPaginator;
            paginator.ComputePageCount();

            // Loop through all pages and add a FixedPage for each page in the FlowDocument.
            var numberOfPages = paginator.PageCount;
            for (int pageNumber = 0; pageNumber < numberOfPages; pageNumber++)
            {
                var fixedPage = new FixedPage
                {
                    Width = fixedDocument.DocumentPaginator.PageSize.Width,
                    Height = fixedDocument.DocumentPaginator.PageSize.Height
                };

                // The first child of the FixedPage is a DocumentPageView control that displays
                // a page of the FlowDocument.
                var documentPageView = new DocumentPageView
                {
                    DocumentPaginator = paginator,
                    PageNumber = pageNumber,
                };
                fixedPage.Children.Add(documentPageView);

                // The second child of the FixedPage is a header with document title and page number.
                var header = new Grid
                {
                    Margin = new Thickness(borderPadding.Left, 60, 0, 0),
                    Width = pageSize.Width - borderPadding.Left - borderPadding.Right,
                };
                header.Children.Add(new Border
                {
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(0, 0, 0, 0.5),
                });
                header.Children.Add(new TextBlock(new Run($"{pageNumber + 1} / {numberOfPages}"))
                {
                    FontFamily = new FontFamily("Times New Roman"),
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 0, 3),
                });
                header.Children.Add(new TextBlock(new Run(documentTitle))
                {
                    FontFamily = new FontFamily("Times New Roman"),
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Margin = new Thickness(0, 0, 0, 3),
                });
                fixedPage.Children.Add(header);

                // Add the FixedPage to the FixedDocument.
                PageContent pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDocument.Pages.Add(pageContent);
            }

            return fixedDocument;
        }


        /// <summary>
        /// Creates a <see cref="FlowDocument"/> containing the text.
        /// </summary>
        /// <param name="pageSize">Size of the page.</param>
        /// <param name="pagePadding">The page padding.</param>
        /// <returns>A <see cref="FlowDocument"/>.</returns>
        /// <remarks>
        /// </remarks>
        public FlowDocument CreateFlowDocument(Size pageSize, Thickness pagePadding)
        {
            var doc = DocumentPrinter.CreateFlowDocumentForEditor(this);
            doc.ColumnWidth = pageSize.Width;
            doc.FontStretch = FontStretch;
            doc.FontStyle = FontStyle;
            doc.FontWeight = FontWeight;
            doc.PageWidth = pageSize.Width;
            doc.PageHeight = pageSize.Height;
            doc.PagePadding = pagePadding;
            return doc;
        }
    }
}
