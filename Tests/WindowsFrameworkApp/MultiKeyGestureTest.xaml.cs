using System.Windows;
using System.Windows.Input;
using DigitalRune.Windows.Framework;


namespace WindowsFrameworkApp
{
    public partial class MultiKeyGestureTest
    {
        // Some static routed commands
        public static readonly ICommand Exit;
        public static readonly ICommand FormatSelection;
        public static readonly ICommand FormatDocument;


        // A non-static DelegateCommand
        public ICommand Extra
        {
            get
            {
                if (_extra == null)
                    _extra = new DelegateCommand(() => MessageBox.Show("Extra Extra Extra"));

                return _extra;
            }
        }
        private ICommand _extra;


        static MultiKeyGestureTest()
        {
            var gestures = new InputGestureCollection
            {
                new MultiKeyGesture(new[] { Key.K, Key.X }, ModifierKeys.Control)
            };
            Exit = new RoutedUICommand(
              "E_xit",
              "Exit",
              typeof(MultiKeyGestureTest),
              gestures);

            gestures = new InputGestureCollection
            {
                new MultiKeyGesture(new[] { Key.K, Key.F }, ModifierKeys.Control)
            };
            FormatSelection = new RoutedUICommand(
              "Format Selection",
              "FormatSelection",
              typeof(MultiKeyGestureTest),
              gestures);

            gestures = new InputGestureCollection
            {
                new MultiKeyGesture(new[] { Key.K, Key.D }, ModifierKeys.Control | ModifierKeys.Shift)
            };
            FormatDocument = new RoutedUICommand(
              "Format Document",
              "FormatDocument",
              typeof(MultiKeyGestureTest),
              gestures);
        }


        public MultiKeyGestureTest()
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(FormatSelection, delegate { MessageBox.Show("Format Selection"); }));
            CommandBindings.Add(new CommandBinding(FormatDocument, delegate { MessageBox.Show("Format Document"); }));
            CommandBindings.Add(new CommandBinding(Exit, delegate { MessageBox.Show("Exit"); }));

            DataContext = this;
        }
    }
}
