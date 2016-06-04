using System.Windows;
using System.Windows.Media;
using DigitalRune.Windows;


namespace CustomWindowChromeApp
{
    public partial class MainWindow
    {
        private bool _isAeroGlassEnabled;
        private Brush _background;


        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs eventArgs)
        {
            SetAeroGlass(AeroGlassCheckBox.IsChecked ?? false);
        }


        private void OnEnableAeroGlass(object sender, RoutedEventArgs eventArgs)
        {
            SetAeroGlass(true);
        }


        private void OnDisableAeroGlass(object sender, RoutedEventArgs eventArgs)
        {
            SetAeroGlass(false);
        }


        private void SetAeroGlass(bool enable)
        {
            if (!IsLoaded)
                return;

            if (enable == _isAeroGlassEnabled)
                return;

            _isAeroGlassEnabled = enable && WindowsHelper.SetAeroGlass(this, enable);
            if (_isAeroGlassEnabled)
            {
                // Set background transparent.
                _background = Background;
                Background = Brushes.Transparent;
            }
            else
            {
                // Restore background.
                Background = _background;
            }
        }
    }
}
