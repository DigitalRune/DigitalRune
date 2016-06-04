using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using DigitalRune.Windows.Docking;
using DigitalRune.Windows.Docking.Tests;


namespace WindowsDockingApp
{
    public partial class FloatWindowTest
    {
        private readonly DockControlViewModel _dockControlViewModel;


        public FloatWindowTest()
        {
            InitializeComponent();

            _dockControlViewModel = new DockControlViewModel(new TestDockStrategy());
            DockControl1.DataContext = _dockControlViewModel;
        }


        private void OnOpenFloatingWindow(object sender, RoutedEventArgs eventArgs)
        {
            var dockTabItem = new DockTabItemViewModel
            {
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/TextDocument.png")),
                Title = "DockTabItem Title",
            };

            _dockControlViewModel.DockStrategy.Float(dockTabItem);
        }


        private void OnCloseFloatingWindow(object sender, RoutedEventArgs eventArgs)
        {
            var floatWindow = _dockControlViewModel.FloatWindows.FirstOrDefault();
            if (floatWindow != null)
                _dockControlViewModel.DockStrategy.Close(floatWindow);
        }
    }
}
