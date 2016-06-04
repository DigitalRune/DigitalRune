using System.Collections.ObjectModel;


namespace WindowsControlsApp
{
    public partial class EditableTextBlockTest
    {
        public EditableTextBlockTest()
        {
            InitializeComponent();
        }
    }


    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Number { get; set; }
    }


    public class EmployeeList : ObservableCollection<Employee>
    { }
}
