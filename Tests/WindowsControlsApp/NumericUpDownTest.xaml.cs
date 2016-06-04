using System.Diagnostics;
using System.Windows.Input;


namespace WindowsControlsApp
{
    public partial class NumericUpDownTest
    {
        public NumericUpDownTest()
        {
            InitializeComponent();
        }


        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs eventArgs)
        {
            Trace.WriteLine(eventArgs.NewFocus);
        }
    }
}
