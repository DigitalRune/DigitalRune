using System.Windows;
using System.Windows.Controls;


namespace WindowsDockingApp
{
    public partial class DockSplitPaneTest
    {
        public DockSplitPaneTest()
        {
            InitializeComponent();
        }


        private void OnOrientationCheckBoxClicked(object sender, RoutedEventArgs eventArgs)
        {
            var checkBox = (CheckBox)sender;

            // Set Orientation of the DockSplitPanel.
            if (checkBox.IsChecked.HasValue && checkBox.IsChecked.Value)
                TestSplitPanel.Orientation = Orientation.Horizontal;
            else
                TestSplitPanel.Orientation = Orientation.Vertical;
        }
    }
}
