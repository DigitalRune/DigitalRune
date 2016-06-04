#if !WP7 && !WP8
using DigitalRune.Game.Input;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MathHelper = DigitalRune.Mathematics.MathHelper;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample demonstrates color-grading using the ColorCorrectionFilter.",
    "",
    33)]
  public class ColorCorrectionSample : PostProcessingSample
  {
    private readonly ColorCorrectionFilter _filter;


    public ColorCorrectionSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // Create and register filter.
      _filter = new ColorCorrectionFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(_filter);
      
      var lookupTexture2D = ContentManager.Load<Texture2D>("PostProcessing/ColorLookupModified");
      var lookupTexture3D = ColorCorrectionFilter.ConvertLookupTexture(lookupTexture2D);
      _filter.LookupTextureA = lookupTexture3D;
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
        _filter.Strength = MathHelper.Clamp(_filter.Strength + delta, 0, 1);
      }

      GraphicsScreen.DebugRenderer.DrawText(
        "\n\nHold <1> or <Shift>+<1> to decrease or increase the effect: "
        + _filter.Strength);
    }
  }
}
#endif