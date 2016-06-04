using System.Windows;
using System.Xml.Linq;
using DigitalRune.Windows.Docking;


namespace WindowsDockingApp
{
    public partial class SerializationTest
    {
        public SerializationTest()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OnSerialize(null, null);
        }


        private void OnSerialize(object sender, RoutedEventArgs e)
        {
            var xElement = DockSerializer.Save(DockControlViewModel);

            TextBox0.Text = xElement.ToString();
        }


        private void OnDeserialize(object sender, RoutedEventArgs e)
        {
            var xElement = XElement.Parse(TextBox0.Text);
            DockSerializer.Load(DockControlViewModel, xElement);
            xElement = DockSerializer.Save(DockControlViewModel);

            TextBox1.Text = xElement.ToString();
        }
    }
}
