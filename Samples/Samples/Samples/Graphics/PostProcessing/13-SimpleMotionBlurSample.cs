#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SimpleMotionBlur to blur the scene when the camera or objects are moving.",
    "",
    43)]
  public class SimpleMotionBlurSample : PostProcessingSample
  {
    private readonly SimpleMotionBlur _simpleMotionBlur;


    public SimpleMotionBlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _simpleMotionBlur = new SimpleMotionBlur(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_simpleMotionBlur);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change strength.
      if (InputService.IsDown(Keys.D1))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _simpleMotionBlur.Strength *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the blur strength: "
        + _simpleMotionBlur.Strength);
    }
  }
}
#endif