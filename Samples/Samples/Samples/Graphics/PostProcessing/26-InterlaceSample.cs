#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Graphics.Effects;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    @"This samples shows how to create an interlace effect.",
    @"",
    56)]
  public class InterlaceSample : PostProcessingSample
  {
    private readonly ConstParameterBinding<float> _strengthParameterBinding;


    public InterlaceSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var effect = ContentManager.Load<Effect>("PostProcessing/Interlace");

      var postProcessor = new EffectPostProcessor(GraphicsService, effect);
      GraphicsScreen.PostProcessors.Add(postProcessor);

      _strengthParameterBinding = (ConstParameterBinding<float>)postProcessor.EffectBinding.ParameterBindings["Strength"];
      _strengthParameterBinding.Value = 1;
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
        _strengthParameterBinding.Value *= (1 + sign * time);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the strength: "
        + _strengthParameterBinding.Value);
    }
  }
}
#endif