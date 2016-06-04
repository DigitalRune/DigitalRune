using System;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class TestItemConductorViewModel : ItemConductor
    {
        private readonly IWindowService _windowService;


        internal static TestItemConductorViewModel DesignInstance => new TestItemConductorViewModel(null);


        public DelegateCommand NewCommand { get; }
        public DelegateCommand CloseCommand { get; }


        public TestItemConductorViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            _windowService = windowService;
            DisplayName = "Test ItemConductor";
            NewCommand = new DelegateCommand(New);
            CloseCommand = new DelegateCommand(Close);

            if (WindowsHelper.IsInDesignMode)
            {
                ActivateItemAsync(DocumentViewModel.DesignInstance).Wait();
            }
        }


        // Note: Methods are "async void Method()" for simplicity.
        // Recommendations:
        // - Avoid "async void Method()".
        // - Make methods "async Task<TResult> MethodAsync()".
        // - Await all asynchronous methods and catch all exceptions.

        private async void New()
        {
            if (Item != null)
            {
                // Close previous item.
                bool success = await DeactivateItemAsync(Item, true);
                if (success)
                    await ActivateItemAsync(new DocumentViewModel(_windowService));
            }
            else
            {
                await ActivateItemAsync(new DocumentViewModel(_windowService));
            }
        }


        private async void Close()
        {
            if (Item != null)
                await DeactivateItemAsync(Item, true);
        }


        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (Item == null)
                New();

            base.OnActivated(eventArgs);
        }


        protected override async void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            base.OnDeactivated(eventArgs);

            // Optional: If we want to remove the strong reference to the current Item:
            if (eventArgs.Closed)
                await ActivateItemAsync(null);
        }
    }
}
