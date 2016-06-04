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
    @"This samples shows how to create and use a custom post-processor to invert the scene colors.",
    @"The post-processing effect is the same as in CustomProcessorSample.cs, but here the class 
EffectPostProcessor is used. EffectPostProcessor automatically generates effect parameter 
bindings, so you do not have to derive a new PostProcessor class.",
    54)]
  public class CustomProcessorSample2 : PostProcessingSample
  {
    private readonly EffectPostProcessor _negativeFilter;
    private readonly ConstParameterBinding<float> _strengthParameterBinding;


    public CustomProcessorSample2(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Load effect.
      var effect = ContentManager.Load<Effect>("PostProcessing/NegativeFilter");

      // Create an EffectPostProcessor that uses the effect and controls the effect parameters
      // using automatically generated effect parameter bindings.
      _negativeFilter = new EffectPostProcessor(GraphicsService, effect);
      GraphicsScreen.PostProcessors.Add(_negativeFilter);

      // An EffectBinding wraps an Effect and automatically creates effect parameter bindings 
      // based on the names, semantics and/or annotations specified in the fx file. 
      // The effect parameters "ViewportSize" and "SourceTexture" have names which are known
      // by the graphics service (see IDs in class DefaultEffectParameterUsages), those effect 
      // parameters will automatically be set by the graphics engine. 
      // The effect parameter "Strength" is not a known name and does not have a semantic, 
      // therefore InitializeBindings() has created a ConstParameterBinding for this parameter 
      // with the default value that was specified in the fx file. We can get this parameter 
      // binding and change the value in Update().
      _strengthParameterBinding = (ConstParameterBinding<float>)_negativeFilter.EffectBinding.ParameterBindings["Strength"];
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
        float delta = sign * time * 0.5f;
        _strengthParameterBinding.Value = MathHelper.Clamp(_strengthParameterBinding.Value + delta, 0, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the strength: "
        + _strengthParameterBinding.Value);
    }
  }
}
#endif