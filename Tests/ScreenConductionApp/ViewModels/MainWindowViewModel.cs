using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class MainWindowViewModel : ItemConductor
    {
        public List<object> TestCases { get; }


        // If you use this, you do not need the ViewModelLocator at design time!
        //internal static MainWindowViewModel DesignInstance => new MainWindowViewModel(null);


        public object SelectedTestCase
        {
            get { return _selectedTestCase; }
            set
            {
                if (SetProperty(ref _selectedTestCase, value))
                    TryActivateItemAsync(value).Forget();
            }
        }
        private object _selectedTestCase;


        public DelegateCommand CloseCommand { get; }


        public MainWindowViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            TestCases = new List<object>
            {
                new TestWindowViewModel(windowService),
                new TestDialogViewModel(windowService),
                new TestItemConductorViewModel(windowService),
                new TestOneActiveItemsConductorViewModel(windowService),
            };

            CloseCommand = new DelegateCommand(Close);

            if (WindowsHelper.IsInDesignMode)
            {
                DisplayName = "Main window title!";
            }
        }


        private async Task<bool> TryActivateItemAsync(object item)
        {
            var oldItem = Item;
            bool success = await ActivateItemAsync(item);
            if (success)
                return true;

            // Revert selection because the screen could not be activated. (Wait before 
            // changing the properties, otherwise data bindings won't be updated.)
            await Task.Yield();
            SelectedTestCase = oldItem;
            return false;
        }


        private void Close()
        {
            // Close this screen. Do not await because we are in the Close event handler.
            Conductor.DeactivateItemAsync(this, true);
        }
    }
}
