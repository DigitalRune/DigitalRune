using System.Windows;
using DigitalRune.Windows.Controls;


namespace WindowsControlsApp
{
    public partial class FontChooserTest
    {
        public FontChooserTest()
        {
            InitializeComponent();
        }


        private void OnButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            var fontDialog = new FontDialog();
            fontDialog.Chooser.SelectedFontFamily = FontChooser.SelectedFontFamily;
            fontDialog.Chooser.SelectedFontSize = FontChooser.SelectedFontSize;
            fontDialog.Chooser.SelectedFontStyle = FontChooser.SelectedFontStyle;
            fontDialog.Chooser.SelectedFontStretch = FontChooser.SelectedFontStretch;
            fontDialog.Chooser.SelectedFontWeight = FontChooser.SelectedFontWeight;

            bool? result = fontDialog.ShowDialog();
            if (result.GetValueOrDefault())
            {
                FontChooser.SelectedFontFamily = fontDialog.Chooser.SelectedFontFamily;
                FontChooser.SelectedFontSize = fontDialog.Chooser.SelectedFontSize;
                FontChooser.SelectedFontStyle = fontDialog.Chooser.SelectedFontStyle;
                FontChooser.SelectedFontStretch = fontDialog.Chooser.SelectedFontStretch;
                FontChooser.SelectedFontWeight = fontDialog.Chooser.SelectedFontWeight;
            }
        }
    }
}
