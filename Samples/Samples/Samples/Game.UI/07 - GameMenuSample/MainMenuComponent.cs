#if !WP7 && !WP8
using System.Linq;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Graphics;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Xna.Framework;


namespace Samples.Game.UI
{
  // Displays the main menu of the game.
  public class MainMenuComponent : GameComponent
  {
    private readonly IServiceLocator _services;
    private readonly IInputService _inputService;
    private readonly IGraphicsService _graphicsService;
    private readonly IUIService _uiService;

    private DelegateGraphicsScreen _graphicsScreen;

    private MainMenuWindow _mainMenuWindow;
    private UIScreen _uiScreen;


    public MainMenuComponent(Microsoft.Xna.Framework.Game game, IServiceLocator services)
      : base(game)
    {
      _services = services;
      _inputService = services.GetInstance<IInputService>();
      _graphicsService = services.GetInstance<IGraphicsService>();
      _uiService = services.GetInstance<IUIService>();
    }


    public override void Initialize()
    {
      base.Initialize();

      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      _graphicsScreen = new DelegateGraphicsScreen(_graphicsService)
      {
        RenderCallback = Render,
      };
      _graphicsService.Screens.Insert(0, _graphicsScreen);

      // Get the "SampleUI" screen that was created by the StartScreenComponent.
      _uiScreen = _uiService.Screens["SampleUI"];

      // Show the main menu window.
      _mainMenuWindow = new MainMenuWindow();
      _mainMenuWindow.Show(_uiScreen);
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _graphicsService.Screens.Remove(_graphicsScreen);
        _graphicsScreen = null;
      }

      base.Dispose(disposing);
    }


    public override void Update(GameTime gameTime)
    {
      // This sample is written for the gamepad and the mouse cursor is hidden.
      // --> Ignore mouse input, otherwise it could conflict with the UI.
      _inputService.IsMouseOrTouchHandled = true;

      // If the main menu window sets its DialogResult to true, we start the game.
      // If the main menu window sets its DialogResult to false, we exit.

      if (_mainMenuWindow.DialogResult == true)
      {
        _mainMenuWindow.Close();
        Game.Components.Remove(this);
        Dispose();
        Game.Components.Add(new MyGameComponent(Game, _services));
      }
      else if (_mainMenuWindow.DialogResult == false)
      {
        // Here, we would exit the game.
        //Game.Exit();
        // In this project we switch to the next sample instead.
        ServiceLocator.Current.GetInstance<SampleFramework>().LoadNextSample();
      }
    }


    private void Render(RenderContext context)
    {
      _graphicsService.GraphicsDevice.Clear(new Color(64, 64, 64));

      // Draw the UI screen. 
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
#endif