#if !WP7
using DigitalRune.Game.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to create and use a custom post-processor class (NegativeFilter) to 
invert the scene colors. (Note: CustomProcessorSample2 creates the same effect but without the 
need to implement a custom PostProcessor class.",
    @"",
    53)]
  public class CustomProcessorSample : PostProcessingSample
  {
    private readonly NegativeFilter _negativeFilter;


    public CustomProcessorSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      _negativeFilter = new NegativeFilter(GraphicsService, ContentManager);
      GraphicsScreen.PostProcessors.Add(_negativeFilter);
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Decrease / Increase strength.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.5f;
        _negativeFilter.Strength = MathHelper.Clamp(_negativeFilter.Strength + delta, 0, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the strength: "
        + _negativeFilter.Strength);
    }
  }
}
#endif