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
    @"This samples shows how to create a chromatic distortion effect.",
    @"",
    57)]
  public class LensDistortionSample : PostProcessingSample
  {
    private readonly ConstParameterBinding<Vector3> _distortionParameterBinding;
    private float _distortion = -0.02f;
    private float _chromaticDistortion = -0.01f;



    public LensDistortionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var effect = ContentManager.Load<Effect>("PostProcessing/LensDistortion");

      var postProcessor = new EffectPostProcessor(GraphicsService, effect);
      GraphicsScreen.PostProcessors.Add(postProcessor);

      var powerParameterBinding = (ConstParameterBinding<float>)postProcessor.EffectBinding.ParameterBindings["Power"];
      powerParameterBinding.Value = 1f;

      _distortionParameterBinding = (ConstParameterBinding<Vector3>)postProcessor.EffectBinding.ParameterBindings["Distortion"];
      _distortionParameterBinding.Value = new Vector3(
        _distortion,
        _distortion + _chromaticDistortion,
        _distortion + 2 * _chromaticDistortion);
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
        float delta = sign * time * 0.01f;
        _distortion = _distortion + delta;

        _distortionParameterBinding.Value =
          new Vector3(_distortion, _distortion + _chromaticDistortion, _distortion + 2 * _chromaticDistortion);
      }

      // <2> / <Shift> + <2> --> Decrease / Increase chromatic distortion.
      if (InputService.IsDown(Keys.D2))
      {
        bool isShiftDown = (InputService.ModifierKeys & ModifierKeys.Shift) != 0;
        float sign = isShiftDown ? +1 : -1;
        float time = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float delta = sign * time * 0.01f;
        _chromaticDistortion = _chromaticDistortion + delta;

        _distortionParameterBinding.Value =
          new Vector3(_distortion, _distortion + _chromaticDistortion, _distortion + 2 * _chromaticDistortion);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the distortion: "
        + _distortion
        + "\nHold <2> or <Shift>+<2> to decrease or increase the chromatic distortion: "
        + _chromaticDistortion);
    }
  }
}
#endif