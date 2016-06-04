using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class TestWindowViewModel : Screen
    {
        private readonly IWindowService _windowService;
        private WindowViewModel _windowViewModel;


        internal static TestWindowViewModel DesignInstance => new TestWindowViewModel(null);


        public ObservableCollection<string> Log { get; }
        public DelegateCommand ShowWindowCommand { get; }
        public DelegateCommand HideWindowCommand { get; }
        public DelegateCommand CloseWindowCommand { get; }


        public TestWindowViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            _windowService = windowService;
            DisplayName = "Test window";
            Log = new ObservableCollection<string>();
            ShowWindowCommand = new DelegateCommand(ShowWindow, CanShowWindow);
            HideWindowCommand = new DelegateCommand(HideWindow, CanHideWindow);
            CloseWindowCommand = new DelegateCommand(CloseWindow, CanCloseWindow);

            if (WindowsHelper.IsInDesignMode)
            {
                Log.Add("Log entry 1...");
                Log.Add("Log entry 2...");
                Log.Add("Log entry 3...");
                Log.Add("Log entry 4...");
                Log.Add("Log entry 5...");
            }
        }


        private void ShowWindow()
        {
            if (_windowViewModel != null)
            {
                _windowViewModel.Conductor.ActivateItemAsync(_windowViewModel);
            }
            else
            {
                _windowViewModel = new WindowViewModel();
                _windowViewModel.Activated += OnWindowActivated;
                _windowViewModel.Deactivated += OnWindowDeactivated;
                _windowService.ShowWindow(_windowViewModel);
            }
        }


        private bool CanShowWindow()
        {
            return _windowViewModel == null || !_windowViewModel.IsActive;
        }


        private void HideWindow()
        {
            _windowViewModel?.Conductor.DeactivateItemAsync(_windowViewModel, false);
        }


        private bool CanHideWindow()
        {
            return _windowViewModel != null && _windowViewModel.IsActive;
        }


        private void CloseWindow()
        {
            _windowViewModel?.Conductor.DeactivateItemAsync(_windowViewModel, true);
        }


        private bool CanCloseWindow()
        {
            return _windowViewModel != null;
        }


        private void RefreshButtons()
        {
            ShowWindowCommand.RaiseCanExecuteChanged();
            HideWindowCommand.RaiseCanExecuteChanged();
            CloseWindowCommand.RaiseCanExecuteChanged();
        }


        private void OnWindowActivated(object sender, ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
                Log.Add("Window opened");

            Log.Add("Window activated");

            RefreshButtons();
        }


        private void OnWindowDeactivated(object sender, DeactivationEventArgs eventArgs)
        {
            Log.Add("Window deactivated");

            if (eventArgs.Closed)
            {
                _windowViewModel.Activated -= OnWindowActivated;
                _windowViewModel.Deactivated -= OnWindowDeactivated;
                _windowViewModel = null;
                Log.Add("Window closed");
            }

            RefreshButtons();
        }


        public override Task<bool> CanCloseAsync()
        {
            if (_windowViewModel != null)
                return _windowViewModel.CanCloseAsync();

            return TaskHelper.FromResult(true);
        }


        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            // Override WindowConductor and immediately close window.
            // (We do not use the conductor because the conductor will check CanClose and it is too
            // late to cancel the deactivation now.)
            (_windowViewModel as IActivatable)?.OnDeactivate(true);

            base.OnDeactivated(eventArgs);
        }
    }
}
