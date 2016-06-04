#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Consoles;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Console = DigitalRune.Game.UI.Controls.Console;


namespace Samples.Game.UI
{
  // Creates a UIScreen for debugging. The screen contains a console, but other controls
  // or text could be added as well.
  public class DebuggingComponent : GameComponent
  {
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;

    private DelegateGraphicsScreen _graphicsScreen;

    private UIScreen _uiScreen;
    private readonly Console _console;


    public DebuggingComponent(Microsoft.Xna.Framework.Game game, IServiceLocator services)
      : base(game)
    {
      _inputService = services.GetInstance<IInputService>();
      _graphicsService = services.GetInstance<IGraphicsService>();
      _uiService = services.GetInstance<IUIService>();

      // Get graphics service and add a DelegateGraphicsScreen as the first 
      // graphics screen. This lets us do the rendering in the Render method of
      // this class.
      
      _graphicsScreen = new DelegateGraphicsScreen(_graphicsService)
      {
        RenderCallback = Render,
      };
      _graphicsService.Screens.Insert(0, _graphicsScreen);

      // Load a UI theme and create a renderer. 
      // We could use the same renderer as the "Default" screen (see StartScreenComponent.cs).
      // But usually, the debug screen will use a more efficient theme (smaller fonts, no
      // fancy graphics). Here, we simply use the BlendBlue theme again.
      var contentManager = services.GetInstance<ContentManager>();
      var theme = contentManager.Load<Theme>("UI Themes/BlendBlue/Theme");
      UIRenderer renderer = new UIRenderer(Game, theme);

      // Create a UIScreen and add it to the UI service. 
      _uiScreen = new UIScreen("Debug", renderer)
      {
        // A transparent background.
        Background = new Color(0, 0, 0, 0),

        // The z-index is equal to the draw order. The z-index defines in which order the 
        // screens are updated. This screen with the debug console should be updated before
        // the actual game under this screen.
        ZIndex = 10,

        // Hide the screen. The user has to press a button to make the debug screen visible.
        IsVisible = false,
      };

      // Optional: 
      // The debug screen handles gamepad input first, then the other screens and game components
      // can handle input. We do not want that the game is controllable when the debug screen is
      // visible, therefore we set the IsHandled flags when the screen is finished with the input.
      _uiScreen.InputProcessed += (s, e) => _inputService.SetGamePadHandled(LogicalPlayerIndex.Any, true);

      // Add a console control on the left.
      _console = new Console
      {
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Stretch,
        Width = 500,
        Margin = new Vector4F(20),
      };
      _uiScreen.Children.Add(_console);

      // Print a few info messages in the console.
      _console.WriteLine("Press TAB or ChatPadGreen to display/hide console.");
      _console.WriteLine("Enter 'help' to view console commands.");

      // Add a custom command:
      _console.Interpreter.Commands.Add(new ConsoleCommand("greet", "greet [<name>] ... Prints a greeting message.", Greet));

      // Add the screen to the UI service. 
      _uiService.Screens.Add(_uiScreen);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_uiScreen != null)  // This check is necessary because Dispose() might be called more than once.
        {
          _uiService.Screens.Remove(_uiScreen);
          _uiScreen = null;

          _graphicsService.Screens.Remove(_graphicsScreen);
          _graphicsScreen = null;
        }
      }
      base.Dispose(disposing);
    }


    /// <summary>
    /// Handles the "greet" console command.
    /// </summary>
    /// <param name="args">
    /// The command arguments entered by the user. The first argument is the command name.
    /// Other arguments are optional.
    /// </param>
    private void Greet(string[] args)
    {
      if (args.Length > 1)
        _console.WriteLine("Hello " + args[1] + "!");
      else
        _console.WriteLine("Hello!");
    }


    public override void Update(GameTime gameTime)
    {
      if (!_inputService.IsKeyboardHandled
          && (_inputService.IsPressed(Keys.ChatPadGreen, false)
              || _inputService.IsPressed(Keys.Tab, false)))
      {
        _inputService.IsKeyboardHandled = true;

        // Toggle visibility of screen when TAB or ChatPadGreen is pressed.
        _uiScreen.IsVisible = !_uiScreen.IsVisible;

        // If the screen becomes visible, make sure that the console has the input focus.
        if (_uiScreen.IsVisible)
          _console.Focus();
      }

      base.Update(gameTime);
    }


    private void Render(RenderContext context)
    {
      // Draw the UI screen. This method does nothing if _uiScreen.IsVisible is false.
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
#endif