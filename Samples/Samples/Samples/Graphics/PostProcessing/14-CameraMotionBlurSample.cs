#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses the CameraMotionBlur to blur the scene when the camera is moving.",
    "",
    44)]
  public class CameraMotionBlurSample : PostProcessingSample
  {
    private readonly CameraMotionBlur _cameraMotionBlur;


    public CameraMotionBlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _cameraMotionBlur = new CameraMotionBlur(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_cameraMotionBlur);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change number of samples.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _cameraMotionBlur.NumberOfSamples++;
        else
          _cameraMotionBlur.NumberOfSamples = Math.Max(1, _cameraMotionBlur.NumberOfSamples - 1);
      }

      // <2> / <Shift> + <2> --> Change blur strength.
      if (InputService.IsDown(Keys.D2))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _cameraMotionBlur.Strength *= (float)Math.Pow(factor, time * 60);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the number of samples: " + _cameraMotionBlur.NumberOfSamples
        + "\nHold <2> or <Shift>+<2> to decrease or increase the blur strength: " + _cameraMotionBlur.Strength);
    }
  }
}
#endif