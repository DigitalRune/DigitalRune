#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  // This post-processor adds a simple distortion effect. See Water/Underwater.fx.
  // Underwater.fx uses only effect parameters for which the DigitalRune Engine already
  // provides effect parameter bindings:
  //   DefaultEffectParameterSemantics.ViewportSize
  //   DefaultEffectParameterSemantics.Time
  //   DefaultEffectParameterSemantics.SourceTexture
  // Therefore, we can simply derive from the EffectPostProcessor and all parameters will
  // be set automatically.
  public class UnderwaterPostProcessor : EffectPostProcessor
  {
    public UnderwaterPostProcessor(IGraphicsService graphicsService, ContentManager content)
      : base(graphicsService, content.Load<Effect>("Water/Underwater"))
    {
    }
  }
}
#endif