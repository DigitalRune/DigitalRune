using System.Windows;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor.Options;
using NLog;


namespace EditorApp
{
    public class TestOptionsPageViewModel : OptionsPageViewModel
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


        internal static TestOptionsPageViewModel DesignInstance
        {
            get 
            {
                return new TestOptionsPageViewModel("Design instance");
            }
        }


        public DelegateCommand ClickCommand { get; private set; }


        public TestOptionsPageViewModel(string name)
            : base(name)
        {
            ClickCommand = new DelegateCommand(() => MessageBox.Show("Button clicked."));
        }


        protected override void OnApply()
        {
            Logger.Debug("Applying changes in test options page.");
        }
    }
}