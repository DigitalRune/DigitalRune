using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class SaveChangesViewModel : Dialog
    {
        // DialogResult = true .... Document can be closed. (Save or discard changes.)
        // DialogResult = false ... Document cannot be closed. (Cancel operation.)


        public DelegateCommand SaveCommand { get; }
        public DelegateCommand DiscardCommand { get; }
        public DelegateCommand CancelCommand { get; }


        public SaveChangesViewModel()
        {
            SaveCommand = new DelegateCommand(Save);
            DiscardCommand = new DelegateCommand(Discard);
            CancelCommand = new DelegateCommand(Cancel);
        }


        public void Save()
        {
            DialogResult = true;
        }


        public void Discard()
        {
            DialogResult = true;
        }


        public void Cancel()
        {
            DialogResult = false;
        }
    }
}
