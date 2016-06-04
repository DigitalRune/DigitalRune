using System;
using DigitalRune.Graphics;
using Microsoft.Xna.Framework;


namespace InteropSample
{
  // A simple GraphicsScreen that only clears the background of the back buffer.
  public class BackgroundGraphicsScreen : GraphicsScreen
  {
    public BackgroundGraphicsScreen(IGraphicsService graphicsService)
      : base(graphicsService)
    {
    }


    protected override void OnUpdate(TimeSpan deltaTime)
    {
    }


    protected override void OnRender(RenderContext context)
    {
      GraphicsService.GraphicsDevice.Clear(Color.CornflowerBlue);
    }
  }
}
