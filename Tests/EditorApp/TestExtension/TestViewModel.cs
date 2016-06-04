using System;
using DigitalRune.Windows.Framework;
using DigitalRune.Editor;
using DigitalRune.Windows.Themes;


namespace EditorApp
{
    internal sealed class TestViewModel : EditorDockTabItemViewModel
    {
        //--------------------------------------------------------------
        #region Constants
        //--------------------------------------------------------------

        internal const string DockIdString = "TestView";
        #endregion


        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static int NextId = 0;

        private readonly IEditorService _editor;
        private int _debugID = NextId++;
        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public DelegateCommand PrintPreviewCommand { get; private set; }

        public DelegateCommand PrintCommand { get; private set; }

        public DelegateCommand DoSomethingCommand { get; private set; }
        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public TestViewModel(IEditorService editor)
        {
            if (editor == null)
                throw new ArgumentNullException(nameof(editor));

            _editor = editor;
            DisplayName = "TestView";
            DockId = DockIdString;
            Icon = MultiColorGlyphs.Document;

            PrintPreviewCommand = new DelegateCommand(ShowPrintPreview);
            PrintCommand = new DelegateCommand(Print);
            DoSomethingCommand = new DelegateCommand(DoSomething);
        }
        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void ShowPrintPreview()
        {
            var windowService = _editor.Services.GetInstance<IWindowService>();
            windowService.ShowDialog(new TestPrintDocumentProvider());
        }


        private static void Print()
        {
            new TestPrintDocumentProvider().Print();
        }


        private static void DoSomething()
        {
            //CommandManager.InvalidateRequerySuggested();
        }
        #endregion
    }
}
