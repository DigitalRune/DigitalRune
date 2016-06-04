using System.Windows;
using System.Windows.Input;
using DigitalRune.Windows.Docking;


namespace WindowsDockingApp
{
    public partial class DockIndicatorOverlayTest
    {
        private DockIndicatorOverlay _borderIndicators;
        private DockIndicatorOverlay _paneIndicators;


        public DockIndicatorOverlayTest()
        {
            InitializeComponent();
        }


        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            bool isCaptured = CaptureMouse();
            if (isCaptured)
            {
                eventArgs.Handled = true;
            }
        }


        protected override void OnGotMouseCapture(MouseEventArgs eventArgs)
        {
            base.OnGotMouseCapture(eventArgs);

            if (Mouse.Captured != this)
                return;

            _borderIndicators = new BorderIndicators(this)
            {
                Owner = Window.GetWindow(this),
                Topmost = true,
                AllowDockLeft = AllowDockLeftBorderCheckBox.IsChecked ?? false,
                AllowDockRight = AllowDockRightBorderCheckBox.IsChecked ?? false,
                AllowDockTop = AllowDockTopBorderCheckBox.IsChecked ?? false,
                AllowDockBottom = AllowDockBottomBorderCheckBox.IsChecked ?? false,
                AllowDockInside = false,
            };

            _paneIndicators = new PaneIndicators(this)
            {
                Owner = Window.GetWindow(this),
                Topmost = true,
                AllowDockLeft = AllowDockLeftCheckBox.IsChecked ?? false,
                AllowDockRight = AllowDockRightCheckBox.IsChecked ?? false,
                AllowDockTop = AllowDockTopCheckBox.IsChecked ?? false,
                AllowDockBottom = AllowDockBottomCheckBox.IsChecked ?? false,
                AllowDockInside = AllowDockCenterCheckBox.IsChecked ?? false,
            };

            _borderIndicators.Show();
            _paneIndicators.Show();
        }


        protected override void OnLostMouseCapture(MouseEventArgs eventArgs)
        {
            base.OnLostMouseCapture(eventArgs);

            if (_borderIndicators != null)
            {
                _borderIndicators.Close();
                _borderIndicators = null;
            }

            if (_paneIndicators != null)
            {
                _paneIndicators.Close();
                _paneIndicators = null;
            }
        }


        protected override void OnMouseUp(MouseButtonEventArgs eventArgs)
        {
            base.OnMouseUp(eventArgs);
            ReleaseMouseCapture();
        }


        protected override void OnMouseMove(MouseEventArgs eventArgs)
        {
            base.OnMouseMove(eventArgs);

            if (_borderIndicators != null)
            {
                DockPosition dockPosition = _borderIndicators.HitTest();
                BorderResultTextBox.Text = dockPosition.ToString();
            }

            if (_paneIndicators != null)
            {
                DockPosition dockPosition = _paneIndicators.HitTest();
                PaneResultTextBox.Text = dockPosition.ToString();
            }
        }
    }
}
