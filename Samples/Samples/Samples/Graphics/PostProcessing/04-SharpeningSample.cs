#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SharpeningFilter to enhance the contrast in the image.",
    "",
    34)]
  public class SharpeningSample : PostProcessingSample
  {
    private readonly SharpeningFilter _sharpeningFilter;


    public SharpeningSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create and register filter.
      _sharpeningFilter = new SharpeningFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_sharpeningFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Decrease / Increase sharpness.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 1.0f;
        _sharpeningFilter.Sharpness = Math.Max(_sharpeningFilter.Sharpness + delta, 0);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the sharpness: "
        + _sharpeningFilter.Sharpness);
    }
  }
}
#endif