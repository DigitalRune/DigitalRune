using System.Threading.Tasks;
using System.Windows;
using DigitalRune.Windows.Framework;


namespace ScreenConductionApp
{
    public class WindowViewModel : Screen
    {
        public WindowViewModel()
        {
            DisplayName = "Window";
        }


        public override Task<bool> CanCloseAsync()
        {
            var result = MessageBox.Show("Close window?", "Exit", MessageBoxButton.OKCancel);
            return TaskHelper.FromResult(result == MessageBoxResult.OK);
        }
    }
}
