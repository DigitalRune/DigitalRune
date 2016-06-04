using DigitalRune.Windows.Framework;


namespace WindowsFrameworkApp
{
    public partial class HelpTest
    {
        static HelpTest()
        {
            Help.HelpProvider = new FormsHelpProvider();

        }


        public HelpTest()
        {
            InitializeComponent();
        }
    }
}
