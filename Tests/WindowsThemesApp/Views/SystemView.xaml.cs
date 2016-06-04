namespace WindowsThemesApp.Views
{
    public class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }


    public partial class SystemView
    {
        public SystemView()
        {
            InitializeComponent();

            ListView.Items.Add(new Person { FirstName = "Anton", LastName = "Berger" });
            ListView.Items.Add(new Person { FirstName = "Julius", LastName = "Cäsar" });
            ListView.Items.Add(new Person { FirstName = "Marco", LastName = "Polo" });
        }
    }
}
