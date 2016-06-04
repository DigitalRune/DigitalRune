using System;
using System.Threading.Tasks;
using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class DocumentViewModel : Screen
    {
        private static int _count;
        private readonly IWindowService _windowService;


        internal static DocumentViewModel DesignInstance => new DocumentViewModel(null);


        public bool IsDirty
        {
            get { return _isDirty; }
            set { SetProperty(ref _isDirty, value); }
        }
        private bool _isDirty;


        public string Text
        {
            get { return _text; }
            set
            {
                if (SetProperty(ref _text, value))
                    IsDirty = true;
            }
        }
        private string _text;


        public DocumentViewModel(IWindowService windowService)
        {
            if (!WindowsHelper.IsInDesignMode)
                if (windowService == null)
                    throw new ArgumentNullException(nameof(windowService));

            _windowService = windowService;

            // Use DisplayName to store document title.
            DisplayName = "Document" + _count;
            _text = "This is " + DisplayName + ".";
            _count++;
        }


        public override Task<bool> CanCloseAsync()
        {
            if (IsDirty)
            {
                var closeDialog = new SaveChangesViewModel { DisplayName = DisplayName };
                bool? result = _windowService.ShowDialog(closeDialog);
                // DialogResult = true .... Document can be closed. (Save or discard changes.)
                // DialogResult = false ... Document cannot be closed. (Cancel operation.)

                return TaskHelper.FromResult(result ?? false);
            }

            return TaskHelper.FromResult(true);
        }


        protected override void OnDeactivated(DeactivationEventArgs eventArgs)
        {
            base.OnDeactivated(eventArgs);
            if (eventArgs.Closed)
            {
                _isDirty = false;
                _text = null;
            }
        }
    }
}
