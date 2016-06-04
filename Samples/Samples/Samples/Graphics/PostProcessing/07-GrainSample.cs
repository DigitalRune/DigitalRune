#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This sample uses a GrainFilter to add film grain to all parts or just the dark parts of 
the scene.",
    @"",
    37)]
  public class GrainSample : PostProcessingSample
  {
    private readonly GrainFilter _grainFilter;


    public GrainSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _grainFilter = new GrainFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_grainFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change strength.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.2f;
        _grainFilter.Strength = Math.Max(0, _grainFilter.Strength + delta);
      }

      // <2> / <Shift> + <2> --> Change grain scale.
      if (InputService.IsDown(Keys.D2))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.2f;
        _grainFilter.GrainScale = Math.Max(1, _grainFilter.GrainScale + delta);
      }

      // <3> / <Shift> + <3> --> Change luminance threshold.
      if (InputService.IsDown(Keys.D3))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.2f;
        _grainFilter.LuminanceThreshold = MathHelper.Clamp(_grainFilter.LuminanceThreshold + delta, 0, 1);
      }

      // <4> --> Toggle ScaleWithLuminance.
      if (InputService.IsPressed(Keys.D4, false))
        _grainFilter.ScaleWithLuminance = !_grainFilter.ScaleWithLuminance;

      // <5> --> Toggle IsAnimated.
      if (InputService.IsPressed(Keys.D5, false))
        _grainFilter.IsAnimated = !_grainFilter.IsAnimated;

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the grain strength: " + _grainFilter.Strength
        + "\nHold <2> or <Shift>+<2> to decrease or increase the grain scale: " + _grainFilter.GrainScale
        + "\nHold <3> or <Shift>+<3> to decrease or increase the luminance threshold: " + _grainFilter.LuminanceThreshold
        + "\nPress <4> to toggle 'scale with luminance': " + _grainFilter.ScaleWithLuminance
        + "\nPress <5> to toggle between animated and static noise: " + _grainFilter.IsAnimated);
    }
  }
}
#endif