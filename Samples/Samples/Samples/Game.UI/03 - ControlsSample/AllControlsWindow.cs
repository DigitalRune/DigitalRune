using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Console = DigitalRune.Game.UI.Controls.Console;


namespace Samples.Game.UI
{
  // Displays the available UI controls.
  public class AllControlsWindow : Window
  {
    public AllControlsWindow(ContentManager content, IUIRenderer renderer)
    {
      Title = "All Controls (This window has a context menu!)";
      CanResize = false;

      // The window content is set to a horizontal stack panel that contains two vertical
      // stack panels. The controls are added to the vertical panels.

      // ----- Button with text content
      var button0 = new Button
      {
        Content = new TextBlock { Text = "Button" },
        Margin = new Vector4F(4),
      };

      // ----- Button with mixed content
      // The content of a button is usually a text block but can be any UI control.
      // Let's try a button with image + text.
      var buttonContentPanel = new StackPanel { Orientation = Orientation.Horizontal };
      buttonContentPanel.Children.Add(new Image
      {
        Width = 16,
        Height = 16,
        Texture = content.Load<Texture2D>("Icon")
      });

      buttonContentPanel.Children.Add(new TextBlock
      {
        Margin = new Vector4F(4, 0, 0, 0),
        Text = "Button with image",
        VerticalAlignment = VerticalAlignment.Center,
      });

      var button1 = new Button
      {
        Content = buttonContentPanel,
        Margin = new Vector4F(4),
      };

      // ----- Drop-down button
      var dropDown = new DropDownButton
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(4),
        MaxDropDownHeight = 250,
      };
      for (int i = 0; i < 20; i++)
      {
        dropDown.Items.Add("DropDownItem " + i);
      }
      dropDown.SelectedIndex = 0;

      // ----- Check box
      var checkBox = new CheckBox
      {
        Margin = new Vector4F(4),
        Content = new TextBlock { Text = "CheckBox" },
      };

      // ----- Group of radio buttons
      var radioButton0 = new RadioButton
      {
        Margin = new Vector4F(4, 8, 4, 4),
        Content = new TextBlock { Text = "RadioButton0" },
      };

      var radioButton1 = new RadioButton
      {
        Margin = new Vector4F(4, 2, 4, 2),
        Content = new TextBlock { Text = "RadioButton1" },
      };

      var radioButton2 = new RadioButton
      {
        Margin = new Vector4F(4, 2, 4, 4),
        Content = new TextBlock { Text = "RadioButton1" },
      };

      // ----- Progress bar with value
      var progressBar0 = new ProgressBar
      {
        IsIndeterminate = false,
        Value = 75,
        Margin = new Vector4F(4, 8, 4, 4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Cursor = renderer.GetCursor("Wait"),
      };

      // ----- Progress bar without value (indeterminate)
      var progressBar1 = new ProgressBar
      {
        IsIndeterminate = true,
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Cursor = renderer.GetCursor("Wait"),
      };

      // ----- Slider connected with a text box.
      var slider = new Slider
      {
        Value = 60,
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      var sliderValue = new TextBlock
      {
        Margin = new Vector4F(4, 0, 4, 4),
        Text = "(Value = 60)",
        HorizontalAlignment = HorizontalAlignment.Right
      };

      // To connect the slider with the text box, we need to get the "Value" property.
      var valueProperty = slider.Properties.Get<float>("Value");
      // This property is a GameObjectProperty<float>. We can attach an event handler to 
      // the Changed event of the property.
      valueProperty.Changed += (s, e) => sliderValue.Text = "(Value = " + (int)e.NewValue + ")";

      // ----- Scroll bar
      var scrollBar = new ScrollBar
      {
        Style = "ScrollBarHorizontal",
        SmallChange = 1,
        LargeChange = 10,
        Value = 45,
        ViewportSize = 0.3f,
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      // Add the controls to the first vertical stack panel.
      var stackPanel0 = new StackPanel
      {
        Margin = new Vector4F(4),
      };
      stackPanel0.Children.Add(button0);
      stackPanel0.Children.Add(button1);
      stackPanel0.Children.Add(dropDown);
      stackPanel0.Children.Add(checkBox);
      stackPanel0.Children.Add(radioButton0);
      stackPanel0.Children.Add(radioButton1);
      stackPanel0.Children.Add(radioButton2);
      stackPanel0.Children.Add(progressBar0);
      stackPanel0.Children.Add(progressBar1);
      stackPanel0.Children.Add(slider);
      stackPanel0.Children.Add(sliderValue);
      stackPanel0.Children.Add(scrollBar);

      // ----- Group box
      var groupBox = new GroupBox
      {
        Title = "GroupBox",
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(4),
        Content = new TextBlock
        {
          Margin = new Vector4F(4),
          Text = "GroupBox Content"
        }
      };

      // ----- Tab control with 3 tab items
      var tabItem0 = new TabItem
      {
        TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 0" },
        Content = new TextBlock { Text = "Item 0" },
      };
      var tabItem1 = new TabItem
      {
        TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 1" },
        Content = new TextBlock { Text = "Item 1" },
      };
      var tabItem2 = new TabItem
      {
        TabPage = new TextBlock { Margin = new Vector4F(4), Text = "Page 2" },
        Content = new TextBlock { Text = "Item 2" },
      };
      var tabControl = new TabControl
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(4),
      };
      tabControl.Items.Add(tabItem0);
      tabControl.Items.Add(tabItem1);
      tabControl.Items.Add(tabItem2);

      // ----- Scroll viewer showing an image
      var image = new Image
      {
        Texture = content.Load<Texture2D>("Sky"),
      };

      var scrollViewer = new ScrollViewer
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MinHeight = 100,
        HorizontalOffset = 200,
        VerticalOffset = 200,
        Height = 200,
      };
      scrollViewer.Content = image;

      // ----- Text box
      var textBox0 = new TextBox
      {
        Margin = new Vector4F(4),
        Text = "The quick brown fox jumps over the lazy dog.",
        MaxLines = 1,
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      // ----- Password box
      var textBox1 = new TextBox
      {
        Margin = new Vector4F(4),
        Text = "Secret password!",
        MaxLines = 1,
        IsPassword = true,
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      // ----- Multi-line text box
      var textBox2 = new TextBox
      {
        Margin = new Vector4F(4),
        Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed eleifend, nulla semper vestibulum congue, lectus lectus aliquam magna, lobortis luctus sem magna sit amet elit. Integer at neque nec mi dapibus tincidunt. Maecenas elit quam, varius luctus rutrum ut, congue quis libero. In hac habitasse platea dictumst. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Phasellus eu ante eros. Etiam odio lectus, sagittis non dictum eu, faucibus vitae magna. Maecenas mi lorem, semper vel condimentum sit amet, vehicula quis nulla. Cras suscipit scelerisque orci, ac ullamcorper lorem egestas sed. Nulla facilisi. Nam justo enim, mollis nec condimentum non, consectetur vel purus. Curabitur ac diam vitae justo ultricies auctor.\n"
             + "Pellentesque interdum vehicula nisi sed congue. Etiam eget magna nec metus suscipit tincidunt et in lectus. Praesent sapien tortor, congue et semper eu, mattis ac lorem. Duis id est et justo tempus consectetur. Aliquam rutrum ullamcorper augue non varius. Fusce ornare lectus et ipsum lobortis ut venenatis tellus sodales. Proin venenatis scelerisque dui eu viverra. Pellentesque vitae risus eget tellus vehicula molestie et quis ante. Nulla imperdiet rhoncus ante, eu tristique ante euismod id. Sed varius varius hendrerit. Praesent massa tortor, gravida suscipit lacinia non, suscipit a ipsum. Integer vulputate, felis vitae consectetur bibendum, ante velit tincidunt nunc, in convallis erat dolor eu purus.",
        MinLines = 5,
        MaxLines = 5,
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };

      // ----- Console (command prompt)
      var console = new Console
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MinHeight = 100,
      };

      // Add the controls to the second vertical stack panel.
      var stackPanel1 = new StackPanel
      {
        Width = 250,
        Margin = new Vector4F(4),
      };
      stackPanel1.Children.Add(groupBox);
      stackPanel1.Children.Add(tabControl);
      stackPanel1.Children.Add(scrollViewer);
      stackPanel1.Children.Add(textBox0);
      stackPanel1.Children.Add(textBox1);
      stackPanel1.Children.Add(textBox2);
      stackPanel1.Children.Add(console);

      // Add the two vertical stack panel into a horizontal stack panel.
      var stackPanelHorizontal = new StackPanel
      {
        Orientation = Orientation.Horizontal
      };
      stackPanelHorizontal.Children.Add(stackPanel0);
      stackPanelHorizontal.Children.Add(stackPanel1);

      Content = stackPanelHorizontal;

      // ----- Add a context menu to the window. (Each UI control can have its own context menu.)
      ContextMenu = new ContextMenu();

      // Add menu items.
      for (int i = 0; i < 10; i++)
      {
        var menuItem = new MenuItem
        {
          Content = new TextBlock { Text = "MenuItem" + i },
        };
        ContextMenu.Items.Add(menuItem);
      }
    }
  }
}
