using DigitalRune.Windows.Docking;


namespace WindowsThemesApp
{
    public class DockStrategy : DigitalRune.Windows.Docking.DockStrategy
    {
        protected override IDockTabItem OnCreateDockTabItem(string dockId)
        {
            return null;
        }
    }
}
