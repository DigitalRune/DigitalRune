using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  public class WpWindow : Window
  {
    // Create a window which shows all controls.
    public WpWindow()
    {
      CanDrag = false;
      CanResize = false;
      HorizontalAlignment = HorizontalAlignment.Stretch;
      VerticalAlignment = VerticalAlignment.Stretch;

      Title = "DigitalRune Game UI Sample";

      // We handle the Closed event.
      Closed += OnClosed;

      var applicationTitle = new TextBlock
      {
        Text = "DIGITALRUNE GUI SAMPLE",
        Margin = new Vector4F(12, 0, 0, 0),
      };

      var pageTitle = new TextBlock
      {
        Text = "controls",
        Style = "TextBlockTitle",
        Margin = new Vector4F(9, -7, 0, 0),
      };

      var titlePanel = new StackPanel
      {
        Margin = new Vector4F(12, 24, 8, 8),
      };
      titlePanel.Children.Add(applicationTitle);
      titlePanel.Children.Add(pageTitle);

      var textBlock = new TextBlock
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        WrapText = true,
        Text = "This is a window with a lot of controls.\n" +
               "All controls are within a scroll viewer.\n" +
               "Tap-and-hold in window area to display the context menu of the window.",
        Style = "TextBlockSubtle"
      };

      var buttonEnabled = new Button
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "Button" },
      };

      var buttonDisabled = new Button
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "Button (Disabled)" },
        IsEnabled = false,
      };

      var checkBoxEnabled = new CheckBox
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "CheckBox" },
      };

      var checkBoxEnabledChecked = new CheckBox
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "CheckBox" },
        IsChecked = true,
      };

      var checkBoxDisabled = new CheckBox
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "CheckBox (Disabled)" },
        IsEnabled = false,
      };

      var checkBoxDisabledChecked = new CheckBox
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "CheckBox (Disabled)" },
        IsEnabled = false,
        IsChecked = true,
      };

      var checkBoxLotsOfText = new CheckBox
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "CheckBox with a lot of text that does not fit into a single line." },
      };

      var radioButton0 = new RadioButton
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "RadioButton" },
      };

      var radioButton1 = new RadioButton
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "RadioButton with a lot of text that does not fit into a single line." },
        IsChecked = true
      };

      var radioButton2 = new RadioButton
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "RadioButton" },
        IsEnabled = false,
      };

      var radioButton3 = new RadioButton
      {
        Margin = new Vector4F(0, 10, 0, 10),
        Content = new TextBlock { Text = "RadioButton (Disabled)" },
        IsEnabled = false,
        IsChecked = true,
        GroupName = "OtherGroup",   // Put in another group, so that the IsChecked state is not removed when the user clicks another radio button.
      };

      var dropDownButton = new DropDownButton
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Title = "DROPDOWN ITEMS",
        SelectedIndex = 0,
      };
      // Add drop down items - each items is a string. Per default, the DropDownButton 
      // calls ToString() for each item and displays it in a text block.
      for (int i = 0; i < 30; i++)
        dropDownButton.Items.Add("DropDownItem " + i);

      var dropDownButtonDisabled = new DropDownButton
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Title = "DROPDOWN ITEMS",
        IsEnabled = false,
        SelectedIndex = 0,
      };
      // Add drop down items - each items is a string. Per default, the DropDownButton 
      // calls ToString() for each item and displays it in a text block.
      for (int i = 0; i < 30; i++)
        dropDownButtonDisabled.Items.Add("DropDownItem " + i + "(Disabled)");

      var textBoxEnabled = new TextBox
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        GuideTitle = "GUIDE TITLE",
        GuideDescription = "Guide description:",
        Text = "TextBox (Enabled)"
      };

      var textBoxDisabled = new TextBox
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Text = "TextBox (Disabled)",
        IsEnabled = false,
      };

      var slider = new Slider
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 24, 0, 24),
        Value = 33
      };

      var progressBar = new ProgressBar
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        Value = 66,
      };

      var progressBarIndeterminate = new ProgressBar
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = new Vector4F(0, 10, 0, 10),
        IsIndeterminate = true,
      };

      var stackPanel = new StackPanel
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Margin = new Vector4F(0, 0, 8, 0),
      };
      stackPanel.Children.Add(textBlock);
      stackPanel.Children.Add(buttonEnabled);
      stackPanel.Children.Add(buttonDisabled);
      stackPanel.Children.Add(checkBoxEnabled);
      stackPanel.Children.Add(checkBoxEnabledChecked);
      stackPanel.Children.Add(checkBoxDisabled);
      stackPanel.Children.Add(checkBoxDisabledChecked);
      stackPanel.Children.Add(checkBoxLotsOfText);
      stackPanel.Children.Add(radioButton0);
      stackPanel.Children.Add(radioButton1);
      stackPanel.Children.Add(radioButton2);
      stackPanel.Children.Add(radioButton3);
      stackPanel.Children.Add(dropDownButton);
      stackPanel.Children.Add(dropDownButtonDisabled);
      stackPanel.Children.Add(textBoxEnabled);
      stackPanel.Children.Add(textBoxDisabled);
      stackPanel.Children.Add(slider);
      stackPanel.Children.Add(progressBar);
      stackPanel.Children.Add(progressBarIndeterminate);

      var scrollViewer = new ScrollViewer
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Content = stackPanel,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
        Margin = new Vector4F(24, 0, 8, 0)
      };

      var layoutRoot = new StackPanel
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
        Margin = new Vector4F(0, 0, 0, 0),
      };
      layoutRoot.Children.Add(titlePanel);
      layoutRoot.Children.Add(scrollViewer);

      Content = layoutRoot;

      // Add a context menu
      MenuItem item0 = new MenuItem { Content = new TextBlock { Text = "Item 0" }, };
      MenuItem item1 = new MenuItem { Content = new TextBlock { Text = "Item 1" }, };
      MenuItem item2 = new MenuItem { Content = new TextBlock { Text = "Item 2" }, };
      MenuItem item3 = new MenuItem { Content = new TextBlock { Text = "Item 3" }, };
      MenuItem item4 = new MenuItem { Content = new TextBlock { Text = "Item 4" }, };

      ContextMenu = new ContextMenu();
      ContextMenu.Items.Add(item0);
      ContextMenu.Items.Add(item1);
      ContextMenu.Items.Add(item2);
      ContextMenu.Items.Add(item3);
      ContextMenu.Items.Add(item4);
    }


    // Called when the window should handle device input.
    protected override void OnHandleInput(InputContext context)
    {
      // Call base implementation to update window content.
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var inputService = InputService;

      // If gamepad was not handled (e.g. by a child control of the window) then we close
      // the window if the BACK button is pressed.
      if (!inputService.IsGamePadHandled(context.AllowedPlayer)
          && inputService.IsDown(Buttons.Back, context.AllowedPlayer))
      {
        inputService.SetGamePadHandled(context.AllowedPlayer, true);
        Close();
      }
    }


    // Called when the window was closed.
    private void OnClosed(object sender, EventArgs e)
    {
      // Here, we would exit the game.
      //_game.Exit();
      // In this project we switch to the next sample instead.
      ServiceLocator.Current.GetInstance<SampleFramework>().LoadNextSample();
    }
  }
}
