#if !WP7 && !WP8
using DigitalRune.Graphics.PostProcessing;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a FxaaFilter to apply Fast Approximate Antialiasing (FXAA).",
    "",
    47)]
  public class FxaaSample : PostProcessingSample
  {
    public FxaaSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var fxaaFilter = new FxaaFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(fxaaFilter);
    }
  }
}
#endif