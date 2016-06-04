#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample uses a RadialBlur to create a blur effect that can be used for fast moving 
cameras (for example for racing games).",
    "",
    42)]
  public class RadialBlurSample : PostProcessingSample
  {
    private readonly RadialBlur _radialBlur;


    public RadialBlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _radialBlur = new RadialBlur(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_radialBlur);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change number of samples.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        int delta = isShiftDown ? +1 : -1;
        _radialBlur.NumberOfSamples = Math.Max(_radialBlur.NumberOfSamples + delta, 1);
      }

      // <2> / <Shift> + <2> --> Change blur radius.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _radialBlur.MaxBlurRadius *= (float)Math.Pow(factor, time * 60);
      }

      // <3> / <Shift> + <3> --> Change blur amount.
      if (InputService.IsDown(Keys.D3))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _radialBlur.MaxBlurAmount *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the number of samples: " + _radialBlur.NumberOfSamples
        + "\nHold <2> or <Shift>+<2> to decrease or increase the blur radius: " + _radialBlur.MaxBlurRadius
        + "\nHold <3> or <Shift>+<3> to decrease or increase the blur amount: " + _radialBlur.MaxBlurAmount);
    }
  }
}
#endif