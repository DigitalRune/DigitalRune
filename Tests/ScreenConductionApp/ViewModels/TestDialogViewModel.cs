using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;
using static System.FormattableString;


namespace ScreenConductionApp
{
    public class TestDialogViewModel : Screen
    {
        private readonly IWindowService _windowService;


        internal static TestDialogViewModel DesignInstance => new TestDialogViewModel(null);


        public ObservableCollection<string> Log { get; }
        public DelegateCommand ShowWindowCommand { get; }
        public DelegateCommand ShowUserControlCommand { get; }


        public TestDialogViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            _windowService = windowService;
            DisplayName = "Test dialog";
            Log = new ObservableCollection<string>();
            ShowWindowCommand = new DelegateCommand(() => ShowDialog("Window"));
            ShowUserControlCommand = new DelegateCommand(() => ShowDialog("UserControl"));

            if (WindowsHelper.IsInDesignMode)
            {
                Log.Add("Log entry 1...");
                Log.Add("Log entry 2...");
                Log.Add("Log entry 3...");
                Log.Add("Log entry 4...");
                Log.Add("Log entry 5...");
            }
        }


        private void ShowDialog(string context)
        {
            var dialogViewModel = new DialogViewModel();
            dialogViewModel.DisplayName = "Custom Dialog";
            dialogViewModel.Content = "This is the content of the dialog.";
            dialogViewModel.Activated += OnDialogActivated;
            dialogViewModel.Deactivated += OnDialogDeactivated;

            var result = _windowService.ShowDialog(dialogViewModel, context);
            Log.Add(Invariant($"DialogResult = {result}"));

            Debug.Assert(dialogViewModel.Conductor == null, "Conductor has not been reset.");
            Debug.Assert(dialogViewModel.DialogResult == result, "View model's DialogResult is wrong.");

            dialogViewModel.Activated -= OnDialogActivated;
            dialogViewModel.Deactivated -= OnDialogDeactivated;
        }


        private void OnDialogActivated(object sender, ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
                Log.Add("Dialog opened");

            Log.Add("Dialog activated");
        }


        private void OnDialogDeactivated(object sender, DeactivationEventArgs eventArgs)
        {
            Log.Add("Dialog deactivated");

            if (eventArgs.Closed)
                Log.Add("Dialog closed");
        }
    }
}
