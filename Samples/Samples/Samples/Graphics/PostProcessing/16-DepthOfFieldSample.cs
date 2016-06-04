#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample show how to use the depth-of-field effect.",
    "",
    46)]
  public class DepthOfFieldSample : PostProcessingSample
  {
    private readonly DepthOfFieldFilter _depthOfFieldFilter;


    public DepthOfFieldSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _depthOfFieldFilter = new DepthOfFieldFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_depthOfFieldFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change downsample factor.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _depthOfFieldFilter.DownsampleFactor++;
        else
          _depthOfFieldFilter.DownsampleFactor = Math.Max(1, _depthOfFieldFilter.DownsampleFactor - 1);
      }

      // <2> / <Shift> + <2> --> Change blur strength.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _depthOfFieldFilter.BlurStrength *= (float)Math.Pow(factor, time * 60);
      }

      // <3> / <Shift> + <3> --> Change near blur distance.
      if (InputService.IsDown(Keys.D3))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _depthOfFieldFilter.NearBlurDistance *= (float)Math.Pow(factor, time * 60);
      }

      // <4> / <Shift> + <4> --> Change near focus distance.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _depthOfFieldFilter.NearFocusDistance *= (float)Math.Pow(factor, time * 60);
      }

      // <5> / <Shift> + <5> --> Change far focus distance.
      if (InputService.IsDown(Keys.D5))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _depthOfFieldFilter.FarFocusDistance *= (float)Math.Pow(factor, time * 60);
      }

      // <6> / <Shift> + <6> --> Change far blur distance.
      if (InputService.IsDown(Keys.D6))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _depthOfFieldFilter.FarBlurDistance *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the downsample factor: " + _depthOfFieldFilter.DownsampleFactor
        + "\nHold <2> or <Shift>+<2> to decrease or increase the blur strength: " + _depthOfFieldFilter.BlurStrength
        + "\nHold <3> or <Shift>+<3> to decrease or increase the near blur distance: " + _depthOfFieldFilter.NearBlurDistance
        + "\nHold <4> or <Shift>+<4> to decrease or increase the near focus distance: " + _depthOfFieldFilter.NearFocusDistance
        + "\nHold <5> or <Shift>+<5> to decrease or increase the far focus distance: " + _depthOfFieldFilter.FarFocusDistance
        + "\nHold <6> or <Shift>+<6> to decrease or increase the far blur distance: " + _depthOfFieldFilter.FarBlurDistance);
    }
  }
}
#endif