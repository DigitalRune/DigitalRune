#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using DigitalRune.Graphics.SceneGraph;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a GodRayFilter to create light shafts.",
    "",
    49)]
  public class GodRaySample : PostProcessingSample
  {
    private readonly GodRayFilter _godRayFilter;


    public GodRaySample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _godRayFilter = new GodRayFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_godRayFilter);

      // The god ray filter light direction should match the direction of the sun light,
      // which was added by the StaticSkyObject.
      var lightNode = GraphicsScreen.Scene.GetSceneNode("Sunlight");
      _godRayFilter.LightDirection = lightNode.PoseWorld.ToWorldDirection(Vector3F.Forward);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

      // <1> / <Shift> + <1> --> Change the downsample factor.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _godRayFilter.DownsampleFactor++;
        else
          _godRayFilter.DownsampleFactor = Math.Max(1, _godRayFilter.DownsampleFactor - 1);
      }

      // <2> / <Shift> + <2> --> Change number of samples.
      if (InputService.IsPressed(Keys.D2, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _godRayFilter.NumberOfSamples++;
        else
          _godRayFilter.NumberOfSamples = Math.Max(1, _godRayFilter.NumberOfSamples - 1);
      }

      // <3> / <Shift> + <3> --> Change number of blur passes.
      if (InputService.IsPressed(Keys.D3, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _godRayFilter.NumberOfPasses++;
        else
          _godRayFilter.NumberOfPasses = Math.Max(1, _godRayFilter.NumberOfPasses - 1);
      }

      // <4> / <Shift> + <4> --> Change light radius.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        _godRayFilter.LightRadius *= (float)Math.Pow(factor, deltaTime * 60);
      }

      // <5> / <Shift> + <5> --> Change intensity.
      if (InputService.IsDown(Keys.D5))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        _godRayFilter.Intensity *= (float)Math.Pow(factor, deltaTime * 60);
      }

      // <6> / <Shift> + <6> --> Change scale.
      if (InputService.IsDown(Keys.D6))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        _godRayFilter.Scale *= (float)Math.Pow(factor, deltaTime * 60);
      }

      // <7> / <Shift> + <7> --> Change softness.
      if (InputService.IsDown(Keys.D7))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        _godRayFilter.Softness *= (float)Math.Pow(factor, deltaTime * 60);
        _godRayFilter.Softness = MathHelper.Clamp(_godRayFilter.Softness, 0, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the downsample factor: " + _godRayFilter.DownsampleFactor
        + "\nPress <2> or <Shift>+<2> to decrease or increase the number of samples: " + _godRayFilter.NumberOfSamples
        + "\nPress <3> or <Shift>+<3> to decrease or increase the number of passes: "
        + _godRayFilter.NumberOfPasses
        + "\nHold <4> or <Shift>+<4> to decrease or increase the light radius: " + _godRayFilter.LightRadius
        + "\nHold <5> or <Shift>+<5> to decrease or increase the intensity: " + _godRayFilter.Intensity
        + "\nHold <6> or <Shift>+<6> to decrease or increase the scale: " + _godRayFilter.Scale
        + "\nHold <7> or <Shift>+<7> to decrease or increase the softness: " + _godRayFilter.Softness);
    }
  }
}
#endif