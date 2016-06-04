using System.Windows;


namespace WindowsControlsApp
{
    public partial class DropDownButtonTest
    {
        public DropDownButtonTest()
        {
            InitializeComponent();
        }


        private void OnDropDownButtonClick(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("Clicked");
        }


        private void OnSplitButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Clicked");
        }
    }
}
