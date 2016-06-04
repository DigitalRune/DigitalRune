using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"Graphics screens are usually drawn one after the other, from back to front. This sample 
shows that it is also possible that one graphics screen can use the result of the previous 
graphics screens.",
    "",
    2)]
  public class ScreenInScreenSample : Sample
  {
    private readonly DelegateGraphicsScreen _screen1;
    private readonly DelegateGraphicsScreen _screen2;
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;


    public ScreenInScreenSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // The first screen.
      _screen1 = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = RenderScreen1,
      };
      GraphicsService.Screens.Insert(0, _screen1);

      // The second screen. 
      _screen2 = new DelegateGraphicsScreen(GraphicsService)
      {
        RenderCallback = RenderScreen2,

        // A graphics screen should let the graphics service know if it renders 
        // to the whole screen, thereby hiding any graphics screens in the background.
        Coverage = GraphicsScreenCoverage.Full,

        // Tell the graphics service to render the previous screens into a render 
        // target with a custom format.
        RenderPreviousScreensToTexture = true,
        SourceTextureFormat = new RenderTargetFormat(800, 600, false, SurfaceFormat.Color, DepthFormat.Depth24),
      };
      GraphicsService.Screens.Insert(1, _screen2);



      // Create a sprite batch.
      _spriteBatch = new SpriteBatch(GraphicsService.GraphicsDevice);

      // Load a sprite font.
      _spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
    }


    // Renders the content of the first screen.
    private void RenderScreen1(RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.DarkBlue);

      _spriteBatch.Begin();
      _spriteBatch.DrawString(_spriteFont, "This is the first screen.", new Vector2(50, 80), Color.White);
      _spriteBatch.End();
    }


    // Renders the content of the second screen.
    private void RenderScreen2(RenderContext context)
    {
      var graphicsDevice = context.GraphicsService.GraphicsDevice;

      graphicsDevice.Clear(Color.CornflowerBlue);

      _spriteBatch.Begin();

      // Render the content of the first screen, which is provided in the render context. 
      _spriteBatch.Draw(context.SourceTexture, new Rectangle(200, 200, 600, 400), Color.White);

      _spriteBatch.DrawString(_spriteFont, "This is the second screen.", new Vector2(50, 80), Color.White);
      _spriteBatch.End();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _spriteBatch.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
