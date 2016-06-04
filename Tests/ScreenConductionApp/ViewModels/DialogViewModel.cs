using DigitalRune.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class DialogViewModel : Dialog
    {
        internal static DialogViewModel DesignInstance => new DialogViewModel();


        public string Content
        {
            get { return _content; }
            set { SetProperty(ref _content, value); }
        }
        private string _content;


        public DelegateCommand OkCommand { get; private set; }
        public DelegateCommand CancelCommand { get; private set; }


        public DialogViewModel()
        {
            OkCommand = new DelegateCommand(Ok);
            CancelCommand = new DelegateCommand(Cancel);

            if (WindowsHelper.IsInDesignMode)
            {
                DisplayName = "Dialog title!";
                Content = "This is the dialog content.\n...";
            }
        }


        private void Ok()
        {
            DialogResult = true;
        }


        private void Cancel()
        {
            DialogResult = false;
        }
    }
}
