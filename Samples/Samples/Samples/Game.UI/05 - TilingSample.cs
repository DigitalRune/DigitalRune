using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;


namespace Samples.Game.UI
{
  [Sample(SampleCategory.GameUI,
    @"This sample shows how to create UI controls using image tiling.",
    @"Image tiling is new feature supported by DigitalRune Game UI: UI controls are usually 
rendered using images from a texture atlas. The images can be automatically repeated to 
fill up the required space.

This sample creates two resizable windows with custom styles:
- The first window consists of images which are stretched to fill the space of the window.
- The second windows consists of images which are tiled automatically depending on the size 
  of the window.

Open the file Content/UI Themes/TilingSample/Theme.xml to see how the styles are defined.",
    5)]
  public class TilingSample : Sample
  {
    private readonly UIScreen _uiScreen;


    public TilingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Add a DelegateGraphicsScreen as the first graphics screen to the graphics
      // service. This lets us do the rendering in the Render method of this class.
      var graphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = Render,
      };
      GraphicsService.Screens.Insert(0, graphicsScreen);

      // Load a UI theme, which defines the appearance and default values of UI controls.
      Theme theme = ContentManager.Load<Theme>("UI Themes/TilingSample/Theme");

      // Create a UI renderer, which uses the theme info to renderer UI controls.
      UIRenderer renderer = new UIRenderer(Game, theme);

      // Create a UIScreen and add it to the UI service. The screen is the root of the 
      // tree of UI controls. Each screen can have its own renderer.
      _uiScreen = new UIScreen("SampleUIScreen", renderer);
      UIService.Screens.Add(_uiScreen);

      // Create a window using the default style "Window".
      var stretchedWindow = new Window
      {
        X = 100,
        Y = 100,
        Width = 480,
        Height = 320,
        CanResize = true,
      };
      _uiScreen.Children.Add(stretchedWindow);

      // Create a window using the style "TiledWindow".
      var tiledWindow = new Window
      {
        X = 200,
        Y = 200,
        Width = 480,
        Height = 320,
        CanResize = true,
        Style = "TiledWindow",
      };
      _uiScreen.Children.Add(tiledWindow);

      // Check file TilingSampleContent/Theme.xml to see how the styles are defined.
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove UIScreen from UI service.
        UIService.Screens.Remove(_uiScreen);
      }

      base.Dispose(disposing);
    }


    private void Render(RenderContext context)
    {
      _uiScreen.Draw(context.DeltaTime);
    }
  }
}
