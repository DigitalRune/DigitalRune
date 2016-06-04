#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SepiaFilter to convert the image to a sepia image.",
    "",
    32)]
  public class SepiaSample : PostProcessingSample
  {
    private readonly SepiaFilter _sepiaFilter;


    public SepiaSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _sepiaFilter = new SepiaFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_sepiaFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Decrease / Increase saturation.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.5f;
        _sepiaFilter.Strength = MathHelper.Clamp(_sepiaFilter.Strength + delta, 0, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the effect: "
        + _sepiaFilter.Strength);
    }
  }
}
#endif