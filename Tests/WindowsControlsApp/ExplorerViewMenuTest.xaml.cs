using System.Windows;
using System.Windows.Controls;
using DigitalRune.Windows.Controls;


namespace WindowsControlsApp
{
    public partial class ExplorerViewMenuTest
    {
        public ExplorerViewMenuTest()
        {
            InitializeComponent();
        }


        private void OnButtonClick(object sender, RoutedEventArgs eventArgs)
        {
            var button = sender as Button;
            if (button != null)
            {
                var viewMenu = button.ContextMenu as ExplorerViewMenu;
                if (viewMenu != null)
                {
                    int viewMode = (int)viewMenu.Mode;
                    viewMode = (viewMode + 1) % ((int)ExplorerViewMode.ExtraLargeIcons + 1);
                    viewMenu.Mode = (ExplorerViewMode)viewMode;
                }
            }
        }
    }
}
