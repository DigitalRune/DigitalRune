using System;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class TestOneActiveItemsConductorViewModel : OneActiveItemsConductor
    {
        private readonly IWindowService _windowService;


        internal static TestOneActiveItemsConductorViewModel DesignInstance => new TestOneActiveItemsConductorViewModel(null);
        

        public DelegateCommand NewCommand { get; }
        public DelegateCommand CloseCommand { get; }


        public TestOneActiveItemsConductorViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            _windowService = windowService;
            DisplayName = "Test OneActiveItemsConductor";
            NewCommand = new DelegateCommand(New);
            CloseCommand = new DelegateCommand(Close);

            if (WindowsHelper.IsInDesignMode)
            {
                ActivateItem(DocumentViewModel.DesignInstance);
                ActivateItem(DocumentViewModel.DesignInstance);
                ActivateItem(DocumentViewModel.DesignInstance);
                ActivateItem(DocumentViewModel.DesignInstance);
                ActivateItem(DocumentViewModel.DesignInstance);
            }
        }


        public void New()
        {
            ActivateItem(new DocumentViewModel(_windowService));
        }


        // Note: Methods are "async void Method()" for simplicity.
        // Recommendations:
        // - Avoid "async void Method()".
        // - Make methods "async Task<TResult> MethodAsync()".
        // - Await all asynchronous methods and catch all exceptions.
        public async void Close()
        {
            if (ActiveItem != null)
                await DeactivateItemAsync(ActiveItem, true);
        }


        protected override void OnActivated(ActivationEventArgs eventArgs)
        {
            if (eventArgs.Opened)
                New();

            base.OnActivated(eventArgs);
        }


        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            base.OnDeactivated(eventArgs);

            // Optional: Remove items. This is done automatically if RemoveItemsOnCanClose or
            // RemoveItemsOnClose is set.
            if (eventArgs.Closed)
                Items.Clear();
        }
    }
}
