using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using DigitalRune.Windows.Controls;


namespace WindowsThemesApp.Views
{
    public partial class PropertyGridView
    {
        // The history of objects that were shown in the first property grid.
        private readonly Stack<IPropertySource> _history = new Stack<IPropertySource>();

        public PropertyGridView()
        {
            InitializeComponent();

            MyPropertyGrid.PropertySource = PropertyGridHelper.CreatePropertySource(MyButton);

            MyPropertyGrid2.PropertySource = new PropertySource()
            {
                Name = "Custom Name",
                TypeName = "Custom Type",
                Properties =
                {
                    new CustomProperty("Name", "StringValue dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd")
                    {
                        Description = "Description of Property0",
                        Category = null,
                        DefaultValue = "Default",
                        CanReset = true,
                    },
                    new CustomProperty("Property 2", true)
                    {
                        Description = "Description of Property 2",
                        PropertyType = typeof(bool),
                        DefaultValue = false,
                        CanReset = true,
                    },
                    new CustomProperty("Float", 666.666f)
                    {
                        PropertyType = typeof(float),
                    },
                    new CustomProperty("Long", 1234567L)
                    {
                        Name = "Long",
                        PropertyType = typeof(long),
                        DefaultValue = 666L,
                        CanReset = true,
                    },
                    new CustomProperty("WpfColor", Colors.BlueViolet)
                    {
                        PropertyType = typeof(Color),
                        DefaultValue = Colors.Black,
                        CanReset = true,
                    },
                },
            };
        }


        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            // To test value-changed handler, change any value. Changes should be visible
            // in the grid.
            MyButton.IsDefault = !MyButton.IsDefault;
        }


        private void OnInspect(object sender, RoutedEventArgs e)
        {
            if (MyPropertyGrid.SelectedProperty == null)
                return;

            // Push old source to history stack.
            _history.Push(MyPropertyGrid.PropertySource);
            MyPropertyGrid.PropertySource =
                PropertyGridHelper.CreatePropertySource(MyPropertyGrid.SelectedProperty.Value);

            e.Handled = true;
        }


        private void OnBack(object sender, RoutedEventArgs e)
        {
            if (_history.Count == 0)
                return;

            // Pop properties source from history stack.
            MyPropertyGrid.PropertySource = _history.Pop();
        }
    }
}
