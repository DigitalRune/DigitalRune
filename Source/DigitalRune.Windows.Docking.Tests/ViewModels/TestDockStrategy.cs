namespace DigitalRune.Windows.Docking.Tests
{
    public class TestDockStrategy : DockStrategy
    {
        protected override IDockTabItem OnCreateDockTabItem(string dockId)
        {
            return new DockTabItemViewModel
            {
                DockId = dockId,
                Title = dockId,
            };
        }
    }
}
