using System.Diagnostics;
using System.Windows;


namespace WindowsThemesApp
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Break(object sender, RoutedEventArgs eventArgs)
        {
            Debugger.Break();
        }
    }
}
