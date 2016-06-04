#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses the BloomFilter to add a bloom/glare effect to an LDR (low dynamic range) scene.",
    "",
    51)]
  public class BloomSample : PostProcessingSample
  {
    private readonly BloomFilter _filter;


    public BloomSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _filter = new BloomFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_filter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change downsample factor.
      if (InputService.IsPressed(Keys.D1, false))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _filter.DownsampleFactor++;
        else
          _filter.DownsampleFactor = Math.Max(1, _filter.DownsampleFactor - 1);
      }

      // <2> / <Shift> + <2> --> Change brightness threshold.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _filter.Threshold = Math.Min(_filter.Threshold * (float)Math.Pow(factor, time * 60), 1);
      }

      // <3> / <Shift> + <3> --> Change bloom brightness threshold.
      if (InputService.IsDown(Keys.D3))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _filter.Intensity *= (float)Math.Pow(factor, time * 60);
      }

      // <4> / <Shift> + <4> --> Change bloom saturation.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _filter.Saturation *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the downsample factor: " + _filter.DownsampleFactor
        + "\nHold <2> or <Shift>+<2> to decrease or increase the brightness threshold: " + _filter.Threshold
        + "\nHold <3> or <Shift>+<3> to decrease or increase the intensity: " + _filter.Intensity
        + "\nHold <4> or <Shift>+<4> to decrease or increase the bloom saturation: " + _filter.Saturation);
    }
  }
}
#endif