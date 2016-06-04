#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses and UnsharpMaskingFilter to increase the contrast.",
    "",
    35)]
  public class UnsharpMaskingSample : PostProcessingSample
  {
    private readonly UnsharpMaskingFilter _unsharpMaskingFilter;


    public UnsharpMaskingSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _unsharpMaskingFilter = new UnsharpMaskingFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_unsharpMaskingFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Change sharpness.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 1.0f;
        _unsharpMaskingFilter.Sharpness = MathHelper.Max(0, _unsharpMaskingFilter.Sharpness + delta);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the sharpness: "
        + _unsharpMaskingFilter.Sharpness);
    }
  }
}
#endif