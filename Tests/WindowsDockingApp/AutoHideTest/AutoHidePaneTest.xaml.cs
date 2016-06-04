using System.Windows;
using System.Windows.Controls;
using DigitalRune.Windows.Docking;


namespace WindowsDockingApp
{
    public partial class AutoHidePaneTest
    {
        public AutoHidePaneTest()
        {
            InitializeComponent();
        }


        private void OnDockLeft(object sender, RoutedEventArgs eventArgs)
        {
            if (MyAutoHidePane != null)
                MyAutoHidePane.Dock = Dock.Left;
        }


        private void OnDockRight(object sender, RoutedEventArgs eventArgs)
        {
            if (MyAutoHidePane != null)
                MyAutoHidePane.Dock = Dock.Right;
        }


        private void OnDockTop(object sender, RoutedEventArgs eventArgs)
        {
            if (MyAutoHidePane != null)
                MyAutoHidePane.Dock = Dock.Top;
        }


        private void OnDockBottom(object sender, RoutedEventArgs eventArgs)
        {
            if (MyAutoHidePane != null)
                MyAutoHidePane.Dock = Dock.Bottom;
        }


        private void OnShowHideAutoHidePane(object sender, RoutedEventArgs eventArgs)
        {
            switch (MyAutoHidePane.State)
            {
                case AutoHideState.Hidden:
                case AutoHideState.SlidingOut:
                    MyAutoHidePane.Show();
                    break;
                case AutoHideState.SlidingIn:
                case AutoHideState.Shown:
                    MyAutoHidePane.Hide();
                    break;
            }
        }


        private void OnShowAndFocusAutoHidePane(object sender, RoutedEventArgs eventArgs)
        {
            MyAutoHidePane.Show();
            MyAutoHidePane.FocusContent();
        }
    }
}
