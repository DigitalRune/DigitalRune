#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SsaoFilter to add ambient occlusion.",
    "",
    50)]
  public class SsaoSample : PostProcessingSample
  {
    private readonly SsaoFilter _ssaoFilter;


    public SsaoSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _ssaoFilter = new SsaoFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_ssaoFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change quality.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _ssaoFilter.Quality = Math.Min(2, _ssaoFilter.Quality + 1);
        else
          _ssaoFilter.Quality = Math.Max(0, _ssaoFilter.Quality - 1);
      }

      // <2> / <Shift> + <2> --> Change downsample factor.
      if (InputService.IsPressed(Keys.D2, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _ssaoFilter.DownsampleFactor++;
        else
          _ssaoFilter.DownsampleFactor = Math.Max(1, _ssaoFilter.DownsampleFactor - 1);
      }

      // <3> / <Shift> + <3> --> Change number of blur passes.
      if (InputService.IsPressed(Keys.D3, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _ssaoFilter.NumberOfBlurPasses++;
        else
          _ssaoFilter.NumberOfBlurPasses = Math.Max(0, _ssaoFilter.NumberOfBlurPasses - 1);
      }

      // <4> --> Toggle edge-aware blur.
      if (InputService.IsPressed(Keys.D4, false))
      {
        _ssaoFilter.UseEdgeAwareBlur = !_ssaoFilter.UseEdgeAwareBlur;
      }

      // <5> / <Shift> + <5> --> Change strength.
      if (InputService.IsDown(Keys.D5))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ssaoFilter.Strength *= (float)Math.Pow(factor, time * 60);
      }

      // <6> / <Shift> + <6> --> Change inner radius.
      if (InputService.IsDown(Keys.D6))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2F radii = _ssaoFilter.Radii;
        radii.X *= (float)Math.Pow(factor, time * 60);
        _ssaoFilter.Radii = radii;
      }

      // <7> / <Shift> + <7> --> Change outer radius.
      if (InputService.IsDown(Keys.D7))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Vector2F radii = _ssaoFilter.Radii;
        radii.Y *= (float)Math.Pow(factor, time * 60);
        _ssaoFilter.Radii = radii;
      }

      // <8> / <Shift> + <8> --> Change max distances.
      if (InputService.IsDown(Keys.D8))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ssaoFilter.MaxDistances *= (float)Math.Pow(factor, time * 60);
      }

      // <9> --> Toggle edge-aware blur.
      if (InputService.IsPressed(Keys.D9, false))
        _ssaoFilter.CombineWithSource = !_ssaoFilter.CombineWithSource;

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the quality level: " + _ssaoFilter.Quality
        + "\nPress <2> or <Shift>+<2> to decrease or increase the downsample factor: " + _ssaoFilter.DownsampleFactor
        + "\nPress <3> or <Shift>+<3> to decrease or increase the number of blur passes: " + _ssaoFilter.NumberOfBlurPasses
        + "\nPress <4> to toggle the edge-aware blur: " + _ssaoFilter.UseEdgeAwareBlur
        + "\nHold <5> or <Shift>+<5> to decrease or increase the AO strength: " + _ssaoFilter.Strength
        + "\nHold <6> or <Shift>+<6> to decrease or increase the inner radius: " + _ssaoFilter.Radii.X
        + "\nHold <7> or <Shift>+<7> to decrease or increase the outer radius: " + _ssaoFilter.Radii.Y
        + "\nHold <8> or <Shift>+<8> to decrease or increase max distance: " + _ssaoFilter.MaxDistances
        + "\nPress <9> to toggle rendering of the AO buffer: " + _ssaoFilter.CombineWithSource);
    }
  }
}
#endif