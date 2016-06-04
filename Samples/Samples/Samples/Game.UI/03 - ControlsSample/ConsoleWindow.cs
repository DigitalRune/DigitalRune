using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Consoles;
using DigitalRune.Game.UI.Controls;


namespace Samples.Game.UI
{
  // Displays an interactive console.
  public class ConsoleWindow : Window
  {
    public ConsoleWindow()
    {
      Title = "Console";
      Width = 480;
      Height = 240;
      var console = new Console
      {
        HorizontalAlignment = HorizontalAlignment.Stretch,
        VerticalAlignment = VerticalAlignment.Stretch,
      };
      Content = console;

      // Print a message in the console.
      console.WriteLine("Enter 'help' to see all available commands.");

      // Register a new command 'close', which closes the ConsoleWindow.
      var closeCommand = new ConsoleCommand("close", "Close console.", _ => Close());
      console.Interpreter.Commands.Add(closeCommand);
    }
  }
}
