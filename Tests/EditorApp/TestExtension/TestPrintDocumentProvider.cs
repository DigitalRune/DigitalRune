using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using DigitalRune.Editor.Printing;


namespace EditorApp
{
    internal class TestPrintDocumentProvider : IPrintDocumentProvider
    {
        public IDocumentPaginatorSource PrintDocument
        {
            get
            {
                if (_printDocument == null)
                {
                    PrintDialog printDialog = new PrintDialog();
                    Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    _printDocument = CreateDocument(pageSize);
                }

                return _printDocument;
            }
        }
        private IDocumentPaginatorSource _printDocument;


        private static FixedDocument CreateDocument(Size pageSize)
        {
            FixedDocument document = new FixedDocument();
            document.DocumentPaginator.PageSize = pageSize;

            FixedPage page1 = new FixedPage();
            page1.Width = pageSize.Width;
            page1.Height = pageSize.Height;

            // add some text to the page
            TextBlock page1Text = new TextBlock();
            page1Text.Text = "This is the first page";
            page1Text.FontSize = 40; // 30pt text
            page1Text.Margin = new Thickness(96); // 1 inch margin
            page1.Children.Add(page1Text);

            // add the page to the document
            PageContent page1Content = new PageContent();
            ((IAddChild)page1Content).AddChild(page1);
            document.Pages.Add(page1Content);

            // do the same for the second page
            FixedPage page2 = new FixedPage();
            page2.Width = pageSize.Width;
            page2.Height = pageSize.Height;
            page2.Children.Add(new Button
            {
                Content = "Hallo",
                Width = pageSize.Width,
                Height = 100,
            });
            PageContent page2Content = new PageContent();
            ((IAddChild)page2Content).AddChild(page2);
            document.Pages.Add(page2Content);

            return document;
        }


        public void Print()
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var document = CreateDocument(new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight));
                printDialog.PrintDocument(document.DocumentPaginator, "MyPrintJobName");
            }
        }
    }
}
