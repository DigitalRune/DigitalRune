using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using DigitalRune.Windows.Docking;


namespace WindowsDockingApp
{
    public partial class DockControlTest
    {
        private int _nextId = 100;


        public DockControlTest()
        {
            InitializeComponent();
        }


        private void OnDebugBreak(object sender, RoutedEventArgs eventArgs)
        {
            Debugger.Break();
        }


        private void OnOpenFloatingWindow(object sender, RoutedEventArgs eventArgs)
        {
            var dockTabItem = new DockTabItemViewModel
            {
                Title = "DockTabItem" + _nextId,
                Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/TextDocument.png")),
            };

            DockControlViewModel.DockStrategy.Float(dockTabItem);
        }


        private void OnShowDockTabItem1(object sender, RoutedEventArgs eventArgs)
        {
            DockControlViewModel.DockStrategy.Show(DockTabItem1);
        }


        private void OnHideDockTabItem1(object sender, RoutedEventArgs eventArgs)
        {
            if (DockControlViewModel.DockStrategy.CanClose(DockTabItem1))
                DockControlViewModel.DockStrategy.Close(DockTabItem1);
        }


        private void OnLoadLayout(object sender, RoutedEventArgs eventArgs)
        {
            DockSerializer.Load(DockControlViewModel, XDocument.Load("DockControlTest.Layout").Root);
        }


        private void OnSaveLayout(object sender, RoutedEventArgs eventArgs)
        {
            new XDocument(DockSerializer.Save(DockControlViewModel)).Save("DockControlTest.Layout");
        }


        private void OnShowDockTabItem5(object sender, RoutedEventArgs eventArgs)
        {
            DockControlViewModel.DockStrategy.Show(DockTabItem5);
        }
    }
}
