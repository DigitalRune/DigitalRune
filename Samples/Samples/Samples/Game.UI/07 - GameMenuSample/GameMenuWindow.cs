#if !WP7 && !WP8
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Game.UI
{
  // A window that represents the pause menu. The window contains three buttons:
  // - "Resume" closes the window when clicked.
  // - "Options" opens the OptionsWindow when clicked.
  // - "Back to main menu" sets the DialogResult to false when clicked.
  public class GameMenuWindow : Window
  {
    public GameMenuWindow()
    {
      // Center the window on the screen.
      HorizontalAlignment = HorizontalAlignment.Center;
      VerticalAlignment = VerticalAlignment.Center;

      Title = "Game Paused";

      // Optional: If we want to hide the window close button, we can simply set its style to null.
      CloseButtonStyle = null;

      var resumeButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Resume" },

        // The Resume button is the "Default button" as well as the "Cancel button". (If the
        // user presses A or START and no one else handles the button presses then the default
        // button is invoked. If the user presses B or BACK and no one else handles the button 
        // presses then the Cancel button is invoked.)
        IsDefault = true,
        IsCancel = true,
      };
      resumeButton.Click += (s, e) => Close();

      var optionsButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Options" },
      };
      optionsButton.Click += (s, e) => new OptionsWindow().Show(this);

      var endGameButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Back to main menu" },
      };
      endGameButton.Click += (s, e) => DialogResult = false;

      var stackPanel = new StackPanel
      {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      stackPanel.Children.Add(resumeButton);
      stackPanel.Children.Add(optionsButton);
      stackPanel.Children.Add(endGameButton);

      Content = stackPanel;
    }
  }
}
#endif