using System.Windows;
using DigitalRune.Windows.Controls;


namespace WindowsThemesApp.Views
{
    public partial class DigitalRuneView
    {
        public DigitalRuneView()
        {
            InitializeComponent();
        }


        private void ShowFontDialog(object sender, RoutedEventArgs eventArgs)
        {
            var fontDialog = new FontDialog();
            fontDialog.ShowDialog();
        }


        private void ShowColorDialog(object sender, RoutedEventArgs eventArgs)
        {
            var colorDialog = new ColorDialog();
            colorDialog.ShowDialog();
        }
    }
}
