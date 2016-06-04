using System.Windows;


namespace WindowsDockingApp
{
    public partial class DockTabPaneTest
    {
        public DockTabPaneTest()
        {
            InitializeComponent();
        }


        private void SelectDockTabItem1(object sender, RoutedEventArgs eventArgs)
        {
            DockTabItem1.IsSelected = true;
        }


        private void SelectDockTabItem2(object sender, RoutedEventArgs eventArgs)
        {
            DockTabItem2.IsSelected = true;
        }


        private void SelectDockTabItem3(object sender, RoutedEventArgs eventArgs)
        {
            DockTabItem3.IsSelected = true;
        }
    }
}
