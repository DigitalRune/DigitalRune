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
    "This sample uses Blur with different blur kernels to blur the screen.",
    "",
    40)]
  public class BlurSample : PostProcessingSample
  {
    private readonly Blur _blur;
    private int _mode = -1;
    private string _modeName;
    private bool _useHardwareFiltering;


    public BlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _blur = new Blur(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_blur);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      var oldMode = _mode;
      var oldUseHardwareFiltering = _useHardwareFiltering;

      // <1> / <Shift> + <1> --> Change mode.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _mode++;
        else
          _mode--;
      }

      // <2> / <Shift> + <2> --> Change number of passes.
      if (InputService.IsPressed(Keys.D2, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        if (isShiftDown)
          _blur.NumberOfPasses++;
        else
          _blur.NumberOfPasses = Math.Max(1, _blur.NumberOfPasses - 1);
      }

      // <3> / <Shift> + <3> --> Change hardware filtering.
      if (InputService.IsPressed(Keys.D3, true))
      {
        _useHardwareFiltering = !_useHardwareFiltering;
      }

      // <4> / <Shift> + <4> --> Change scale.
      if (InputService.IsDown(Keys.D4))
      {
        // Increase or decrease value by a factor of 1.01 every frame (1/60 s).
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float factor = isShiftDown ? 1.01f : 1.0f / 1.01f;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _blur.Scale *= (float)Math.Pow(factor, time * 60);
      }

      // Update blur mode.
      _mode = MathHelper.Clamp(_mode, 0, 2);
      if (oldMode != _mode || oldUseHardwareFiltering != _useHardwareFiltering)
      {
        if (_mode == 0)
        {
          _modeName = "15-tap Box Blur";
          _blur.InitializeBoxBlur(15, _useHardwareFiltering);
        }
        else if (_mode == 1)
        {
          _modeName = "15-tap Gaussian Blur";
          _blur.InitializeGaussianBlur(15, 15.0f / 6, _useHardwareFiltering);
        }
        else if (_mode == 2)
        {
          _modeName = "13-tap Poisson Blur";
          _blur.InitializePoissonBlur();
        }
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to change the blur kernel: " + _modeName
        + "\nPress <2> or <Shift>+<2> to decrease or increase the number of passes: " + _blur.NumberOfPasses
        + "\nPress <3> toggle hardware filtering: " + _useHardwareFiltering
        + "\nPress <4> or <Shift>+<4> to decrease or increase the blur scale: " + _blur.Scale);
    }
  }
}
#endif