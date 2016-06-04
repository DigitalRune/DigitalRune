#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SaturationFilter to convert the image to a grayscale image.",
    "",
    31)]
  public class SaturationSample : PostProcessingSample
  {
    private readonly SaturationFilter _saturationFilter;


    public SaturationSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create post-processor.
      _saturationFilter = new SaturationFilter(GraphicsService)
      {
        Saturation = 0.5f
      };

      // Register post-processor in the PostProcessManager of main graphics screen.
      GraphicsScreen.PostProcessors.Add(_saturationFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Decrease / Increase saturation.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 1.0f;
        _saturationFilter.Saturation = Math.Max(_saturationFilter.Saturation + delta, 0);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the saturation: "
        + _saturationFilter.Saturation);
    }
  }
}
#endif