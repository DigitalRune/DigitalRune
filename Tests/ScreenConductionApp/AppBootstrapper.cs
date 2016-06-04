using System.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class AppBootstrapper : Bootstrapper
    {
        private ViewLocator _viewLocator;
        private ViewModelLocator _viewModelLocator;
        private WindowManager _windowManager;


        protected override void OnConfigure()
        {
            base.OnConfigure();

            _viewLocator = new ViewLocator();
            _windowManager = new WindowManager(_viewLocator);
        }


        protected override void OnStartup(object sender, StartupEventArgs eventArgs)
        {
            base.OnStartup(sender, eventArgs);

            // Build up ViewModelLocator. (Workaround required when not using an IoC container.)
            _viewModelLocator = Application.FindResource("Locator") as ViewModelLocator;
            if (_viewModelLocator != null)
                _viewModelLocator.WindowService = _windowManager;

            _windowManager.ShowWindow(new MainWindowViewModel(_windowManager));
        }
    }
}
