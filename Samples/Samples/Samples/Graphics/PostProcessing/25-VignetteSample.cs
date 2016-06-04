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
    @"This samples shows how to create a Vignette effect.",
    @"",
    55)]
  public class VignetteSample : PostProcessingSample
  {
    private readonly ConstParameterBinding<Vector2> _scaleParameterBinding;
    private readonly ConstParameterBinding<float> _powerParameterBinding;


    public VignetteSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var effect = ContentManager.Load<Effect>("PostProcessing/Vignette");

      var postProcessor = new EffectPostProcessor(GraphicsService, effect);
      GraphicsScreen.PostProcessors.Add(postProcessor);

      // "Scale" defines the vignette size and shape.
      _scaleParameterBinding = (ConstParameterBinding<Vector2>)postProcessor.EffectBinding.ParameterBindings["Scale"];
      // Elliptic vignette.
      _scaleParameterBinding.Value = new Vector2(2.0f, 2.0f);
      // Circular vignette.
      //_scaleParameterBinding.Value = new Vector2(2.0f, 2.0f * 1280 / 720);

      // "Power" defines the vignette curve. 
      // 1 .... linear brightness falloff
      // >1 ... non-linear brightness falloff
      _powerParameterBinding = (ConstParameterBinding<float>)postProcessor.EffectBinding.ParameterBindings["Power"];
      _powerParameterBinding.Value = 2;
    }


    public override void Update(GameTime gameTime)
    {
      base.Update(gameTime);

      // <1> / <Shift> + <1> --> Decrease / Increase scale.
      if (InputService.IsDown(Keys.D1))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _scaleParameterBinding.Value *= (1 + sign * time);
      }

      // <2> / <Shift> + <2> --> Decrease / Increase power.
      if (InputService.IsDown(Keys.D2))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.5f;
        _powerParameterBinding.Value = MathHelper.Max(_powerParameterBinding.Value + delta, 0);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the scale: "
        + _scaleParameterBinding.Value
        + "\nHold <2> or <Shift>+<2> to decrease or increase the power: "
        + _powerParameterBinding.Value);
    }
  }
}
#endif