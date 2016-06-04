using DigitalRune.Windows.Framework;
using DigitalRune.Editor;
using Microsoft.Practices.ServiceLocation;


namespace EditorApp
{
    internal sealed class TestDockTabItemViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        public new const string DockId = "TestView";
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public DelegateCommand PrintPreviewCommand { get; private set; }

        public DelegateCommand PrintCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public TestDockTabItemViewModel()
        {
            DisplayName = "TestView";
            base.DockId = DockId;
            Icon = EditorHelper.GetPackedBitmap("pack://application:,,,/DigitalRune.Editor;component/Resources/Images/Icons.png", 32, 96, 32, 32);

            PrintPreviewCommand = new DelegateCommand(ShowPrintPreview);
            PrintCommand = new DelegateCommand(Print);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private static void ShowPrintPreview()
        {
            var windowService = ServiceLocator.Current.GetInstance<IWindowService>();
            windowService.ShowDialog(new TestPrintDocument());
        }


        private static void Print()
        {
            new TestPrintDocument().Print();
        }
        #endregion
    }
}
