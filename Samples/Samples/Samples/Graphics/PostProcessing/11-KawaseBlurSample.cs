#if !WP7 && !WP8
using System;
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a KawaseBlur to blur the screen.",
    "",
    41)]
  public class KawaseBlurSample : PostProcessingSample
  {
    private readonly KawaseBlur _blur;


    public KawaseBlurSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _blur = new KawaseBlur(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_blur);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change number of passes.
      if (InputService.IsPressed(Keys.D1, true))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        int delta = isShiftDown ? +1 : -1;
        _blur.NumberOfPasses = Math.Max(_blur.NumberOfPasses + delta, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nPress <1> or <Shift>+<1> to decrease or increase the number of passes: "
        + _blur.NumberOfPasses);
    }
  }
}
#endif