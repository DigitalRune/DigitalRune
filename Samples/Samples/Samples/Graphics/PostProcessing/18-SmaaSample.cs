#if !WP7 && !WP8
using DigitalRune.Graphics.PostProcessing;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses a SmaaFilter to apply Subpixel Morphological Antialiasing (SMAA).",
    "",
    48)]
  public class SmaaSample : PostProcessingSample
  {
    public SmaaSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      var smaaFilter = new SmaaFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(smaaFilter);
    }
  }
}
#endif