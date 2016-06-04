#if !WP7 && !WP8
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;


namespace Samples.Game.UI
{
  // A window that represents the main menu. The window contains three buttons:
  // - "Play" sets the DialogResult to true when clicked.
  // - "Options" opens the OptionsWindow when clicked.
  // - "Exit" sets the DialogResult to false when clicked.
  public class MainMenuWindow : Window
  {
    public MainMenuWindow()
    {
      // The window fill the whole screen.
      HorizontalAlignment = HorizontalAlignment.Stretch;
      VerticalAlignment = VerticalAlignment.Stretch;

      // The window itself (borders, title, background) should be invisible. Therefore, we have
      // added the "InvisibleWindow" style in the Theme.Xml.
      Style = "InvisibleWindow";

      var playButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Play" },
      };
      playButton.Click += (s, e) => DialogResult = true;

      var optionsButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Options" },
      };
      optionsButton.Click += (s, e) => new OptionsWindow().Show(this);

      var exitButton = new Button
      {
        Margin = new Vector4F(10),
        Width = 200,
        Height = 60,
        Content = new TextBlock { Text = "Exit" },
      };
      exitButton.Click += (s, e) => DialogResult = false;

      var stackPanel = new StackPanel
      {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
      };
      stackPanel.Children.Add(playButton);
      stackPanel.Children.Add(optionsButton);
      stackPanel.Children.Add(exitButton);

      Content = stackPanel;
    }
  }
}
#endif