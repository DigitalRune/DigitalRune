using System;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to use a DelegateGraphicsScreen.",
    @"The graphics service manages a collection of graphics screens. The graphics screens of 
the graphics service are automatically updated and called to render their content. A 
DelegateGraphicsScreen is a simple graphics screen implementation that calls user-defined
callback methods.",
    1)]
  public class DelegateGraphicsScreenSample : Sample
  {
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteFont _spriteFont;


    public DelegateGraphicsScreenSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create the DelegateGraphicsService and add it to the graphics service.
      var delegateGraphicsScreen = new DelegateGraphicsScreen(GraphicsService)
      {
        UpdateCallback = Update,
        RenderCallback = Render,
      };

      // Graphics screens are rendered in the order in which they appear in the 
      // IGraphicsService.Screens collection. We insert our screen at the beginning
      // of the collection to render our screen before the other screens (e.g. menu,
      // help text, profiling).
      GraphicsService.Screens.Insert(0, delegateGraphicsScreen);

      // Create a sprite batch.
      _spriteBatch = new SpriteBatch(GraphicsService.GraphicsDevice);

      // Load a sprite font.
      _spriteFont = UIContentManager.Load<SpriteFont>("UI Themes/BlendBlue/Default");
    }


    private void Update(GraphicsScreen screen, TimeSpan deltaTime)
    {
      // If your graphics screen has any objects that need to be updated before 
      // rendering, you can do this here. This method is called once per frame if 
      // the graphics screen is visible.
    }


    private void Render(RenderContext context)
    {
      // Here we can render the content of the graphics screen. This method is only 
      // called if the screen is visible. Unlike the Update callback method, this 
      // callback method might be called several times per frame in complex 
      // applications, like game editors with multiple views. - But in most 
      // applications this method is called once per frame.

      var graphicsDevice = context.GraphicsService.GraphicsDevice;
      graphicsDevice.Clear(Color.CornflowerBlue);

      // Draw text.
      _spriteBatch.Begin();
      _spriteBatch.DrawString(_spriteFont, "Hello World!", new Vector2(200, 200), Color.White);
      _spriteBatch.End();
    }


    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        // Remove graphics screen.
        // Note: This operation is redundant because the Sample base class removes
        // the screen automatically.
        //GraphicsService.Screens.Remove(_delegateGraphicsScreen);

        _spriteBatch.Dispose();
      }

      base.Dispose(disposing);
    }
  }
}
