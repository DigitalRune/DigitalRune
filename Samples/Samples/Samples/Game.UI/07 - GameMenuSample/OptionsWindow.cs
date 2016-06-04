#if !WP7 && !WP8
using System;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework.Input;


namespace Samples.Game.UI
{
  // A window that contains a tab control with two tabs. A "General" tab where the user can change
  // general settings and a "Graphics" tab where the user can change graphics settings. In this
  // sample, changing the settings has no effect.
  // The window also has an "OK" and a "Cancel" button. It can be closed by pressing the buttons
  // or with the "START" or "BACK" gamepad buttons.
  // The layout is created using nested StackPanels.
  public class OptionsWindow : Window
  {
    public OptionsWindow()
    {
      Title = "Options (Switch tabs with LB and RB)";
      CanResize = false;
      CanDrag = false;
      HorizontalAlignment = HorizontalAlignment.Center;
      VerticalAlignment = VerticalAlignment.Center;

      var nameTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Your name:",
      };

      var nameTextBox = new TextBox
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Text = "Player1",
      };

      var passwordTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Your password:",
      };

      var passwordTextBox = new TextBox
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        IsPassword = true,
      };

      var difficultyTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Difficulty:",
      };

      var easyRadioButton = new RadioButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new TextBlock { Text = "Easy" },
        GroupName = "Difficulty",
      };

      var normalRadioButton = new RadioButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new TextBlock { Text = "Normal" },
        GroupName = "Difficulty",
        IsChecked = true,
      };

      var hardRadioButton = new RadioButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new TextBlock { Text = "Hard" },
        GroupName = "Difficulty",
      };

      var modeTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Mode:",
      };

      var simulationRadioButton = new RadioButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new TextBlock { Text = "Simulation" },
        GroupName = "Mode",
      };

      var arcadeRadioButton = new RadioButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Content = new TextBlock { Text = "Arcade" },
        GroupName = "Mode",
        IsChecked = true,
      };

      var generalPanel = new StackPanel
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };
      generalPanel.Children.Add(nameTextBlock);
      generalPanel.Children.Add(nameTextBox);
      generalPanel.Children.Add(passwordTextBlock);
      generalPanel.Children.Add(passwordTextBox);
      generalPanel.Children.Add(difficultyTextBlock);
      generalPanel.Children.Add(easyRadioButton);
      generalPanel.Children.Add(normalRadioButton);
      generalPanel.Children.Add(hardRadioButton);
      generalPanel.Children.Add(modeTextBlock);
      generalPanel.Children.Add(simulationRadioButton);
      generalPanel.Children.Add(arcadeRadioButton);

      var generalTab = new TabItem
      {
        TabPage = generalPanel,
        Content = new TextBlock { Text = "General" },
      };

      var resolutionTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Resolution:",
      };

      var resolutionDropDownButton = new DropDownButton
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        MaxDropDownHeight = 400,
      };
      resolutionDropDownButton.Items.Add("640 x 400");
      resolutionDropDownButton.Items.Add("800 x 480");
      resolutionDropDownButton.Items.Add("1024 x 768");
      resolutionDropDownButton.Items.Add("1280 x 720");
      resolutionDropDownButton.Items.Add("1920 x 1080");
      resolutionDropDownButton.Items.Add("1920 x 1200");
      resolutionDropDownButton.SelectedIndex = 3;

      var qualityTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
        Text = "Quality:",
      };

      var qualityValueTextBlock = new TextBlock
      {
        Margin = new Vector4F(4),
      };

      var qualityTextPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal,
      };
      qualityTextPanel.Children.Add(qualityTextBlock);
      qualityTextPanel.Children.Add(qualityValueTextBlock);

      var qualitySlider = new Slider
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Minimum = 0,
        Maximum = 10,
        SmallChange = 1,        // This value is used when the user presses LEFT/RIGHT to move the slider.
        LargeChange = 1,        // This value is used when the user presses the slider (not the thumb).
      };

      // Each game object property (e.g. Slider.Minimum/Maximum/Margin/Value/...) has a Changing
      // and a Changed event. To use it, we have to get the property:
      GameProperty<float> sliderValueProperty = qualitySlider.Properties.Get<float>("Value");

      // We can use the Changing event to coerce the property value. For example: A normal slider
      // can have any values when dragged with the mouse. We can coerce the value to only allow
      // integer values!
      sliderValueProperty.Changing += (s, e) => e.CoercedValue = (float)Math.Round(e.CoercedValue);

      // We can use the Changed event to update dependent values. Here, we update the text block
      // text whenever the slider value changes.
      sliderValueProperty.Changed += (s, e) => qualityValueTextBlock.Text = qualitySlider.Value.ToString();

      // Initialize the slider value. This raises the Changing and Changed events, and the 
      // qualityValueTextBlock is updated too.
      qualitySlider.Value = 4;

      var graphicsPanel = new StackPanel
      {
        Margin = new Vector4F(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
      };
      graphicsPanel.Children.Add(resolutionTextBlock);
      graphicsPanel.Children.Add(resolutionDropDownButton);
      graphicsPanel.Children.Add(qualityTextPanel);
      graphicsPanel.Children.Add(qualitySlider);

      var graphicsTab = new TabItem
      {
        TabPage = graphicsPanel,
        Content = new TextBlock { Text = "Graphics" },
      };

      var tabControl = new TabControl
      {
        Width = 300,
        Height = 300,
        Margin = new Vector4F(4)
      };
      tabControl.Items.Add(generalTab);
      tabControl.Items.Add(graphicsTab);

      var okButton = new Button
      {
        Width = 100,
        Margin = new Vector4F(4),
        Content = new TextBlock { Text = "OK (START)" },
        IsDefault = true,
      };
      okButton.Click += OnOK;

      var cancelButton = new Button
      {
        Width = 100,
        Margin = new Vector4F(4),
        Content = new TextBlock { Text = "Cancel (BACK)" },
        IsCancel = true,
      };
      cancelButton.Click += OnCancel;

      var buttonPanel = new StackPanel
      {
        Orientation = Orientation.Horizontal,
        HorizontalAlignment = HorizontalAlignment.Center,
      };
      buttonPanel.Children.Add(okButton);
      buttonPanel.Children.Add(cancelButton);

      var mainPanel = new StackPanel
      {
        Margin = new Vector4F(4)
      };
      mainPanel.Children.Add(tabControl);
      mainPanel.Children.Add(buttonPanel);

      Content = mainPanel;
    }


    // Called when this UI control should process input.
    protected override void OnHandleInput(InputContext context)
    {
      // Call base method to update the contained controls first.
      base.OnHandleInput(context);

      if (!IsLoaded)
        return;

      var inputService = InputService;

      // We call OnOk() when START is pressed and OnCancel() when BACK is pressed. Before that
      // we must check if the gamepad input hasn't already been handled by a control inside
      // this window. (If the drop down button is opened, the BACK button is handled by the
      // drop down button, and must not close the window!).
      if (!inputService.IsGamePadHandled(context.AllowedPlayer))
      {
        if (inputService.IsPressed(Buttons.Start, false, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);
          OnOK(null, null);
        }
        else if (inputService.IsPressed(Buttons.Back, false, context.AllowedPlayer))
        {
          inputService.SetGamePadHandled(context.AllowedPlayer, true);
          OnCancel(null, null);
        }
      }
    }


    // Called when the OK button is pressed.
    private void OnOK(object sender, EventArgs eventArgs)
    {
      // TODO: Save changes.

      Close();
    }


    // Called when the Cancel button is pressed.
    private void OnCancel(object sender, EventArgs eventArgs)
    {
      // TODO: Undo changes.

      Close();
    }
  }
}
#endif