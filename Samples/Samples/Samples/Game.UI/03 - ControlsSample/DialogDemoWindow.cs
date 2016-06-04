using System;
using System.Linq;
using System.Xml.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
#if MONOGAME
using DigitalRune.Storages;
using Microsoft.Practices.ServiceLocation;
#endif


namespace Samples.Game.UI
{
  // Shows how to create a window layout in code and how to load a window layout from an XML file.
  public class DialogDemoWindow : Window
  {
    public DialogDemoWindow()
    {
      Title = "Dialog Demo";
      ClipContent = true;

      var button0 = new Button
      {
        Content = new TextBlock { Text = "Open dialog with layout from code", WrapText = true },
        Margin = new Vector4F(4),
      };
      button0.Click += OpenDialogFromCode;

      var button1 = new Button
      {
        Content = new TextBlock { Text = "Open dialog with layout from XML", WrapText = true },
        Margin = new Vector4F(4),
      };
      button1.Click += OpenDialogFromXml;

      var stackPanel = new StackPanel { Margin = new Vector4F(4) };
      stackPanel.Children.Add(button0);
      stackPanel.Children.Add(button1);

      Content = stackPanel;
    }


    private void OpenDialogFromCode(object sender, EventArgs eventArgs)
    {
      // ----- Create dialog.
      var text = new TextBlock
      {
        Text = "This layout was defined in code.",
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Center,
      };

      var button = new Button
      {
        Content = new TextBlock { Text = "Ok" },
        IsCancel = true,       // Cancel buttons are clicked when the user presses ESC (or BACK or B on the gamepad).
        IsDefault = true,      // Default buttons are clicked when the user presses ENTER or SPACE (or START or A on the gamepad).
        Margin = new Vector4F(4),
        Width = 60,
        HorizontalAlignment = HorizontalAlignment.Center,
      };

      var stackPanel = new StackPanel { Margin = new Vector4F(4) };
      stackPanel.Children.Add(text);
      stackPanel.Children.Add(button);

      var window = new Window
      {
        CanResize = false,
        IsModal = true,             // Modal dialogs consume all input until the window is closed.
        Content = stackPanel,
        MinHeight = 0,
        Title = "A modal dialog from code",
      };

      button.Click += (s, e) => window.Close();

      // ----- Show the window in the center of the screen.
      // First, we need to open the window. 
      window.Show(this);

      // The window is now part of the visual tree of controls and can be measured. (The 
      // window does not have a fixed size. Window.Width and Window.Height are NaN. The 
      // size is calculated automatically depending on its content.)
      window.Measure(new Vector2F(float.PositiveInfinity));

      // Measure computes DesiredWidth and DesiredHeight. With this info we can center the 
      // window on the screen.
      window.X = Screen.ActualWidth / 2 - window.DesiredWidth / 2;
      window.Y = Screen.ActualHeight / 2 - window.DesiredHeight / 2;
    }


    private void OpenDialogFromXml(object sender, EventArgs eventArgs)
    {
      // ----- Load a dialog from an XML file.
      // Load the XML file that contains a layout that the LayoutSerializer can read.
#if MONOGAME
      var storage = ServiceLocator.Current.GetInstance<IStorage>();
      var xDocument = XDocument.Load(storage.OpenFile("Layout.xml"));
#else
      var xDocument = XDocument.Load("Content/Layout.xml");
#endif

      // Deserialize the objects in the XML document.
      var serializer = new LayoutSerializer();
      var objects = serializer.Load(xDocument.CreateReader());

      // Get the first window of the deserialized objects.
      var window = objects.OfType<Window>().First();

      // Get the button named "OkButton" and handle click event.
      var button = (Button)window.GetControl("OkButton");
      button.Click += (s, e) => window.Close();

      // ----- Show the window in the center of the screen.
      // First, we need to open the window. 
      window.Show(this);

      // The window is now part of the visual tree of controls and can be measured. (The 
      // window does not have a fixed size. Window.Width and Window.Height are NaN. The 
      // size is calculated automatically depending on its content.)
      window.Measure(new Vector2F(float.PositiveInfinity));

      // Measure computes DesiredWidth and DesiredHeight. With this info we can center the 
      // window on the screen.
      window.X = Screen.ActualWidth / 2 - window.DesiredWidth / 2;
      window.Y = Screen.ActualHeight / 2 - window.DesiredHeight / 2;
    }
  }
}
