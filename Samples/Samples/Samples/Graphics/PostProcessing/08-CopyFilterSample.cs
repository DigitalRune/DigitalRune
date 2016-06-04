#if !WP7 && !WP8
using DigitalRune.Graphics;
using DigitalRune.Graphics.PostProcessing;
using Microsoft.Xna.Framework.Graphics;


namespace Samples.Graphics
{
  [Sample(SampleCategory.Graphics,
    "This sample uses two CopyFilters which simply copy an image.",
    "",
    38)]
  public class CopyFilterSample : PostProcessingSample
  {
    public CopyFilterSample(Microsoft.Xna.Framework.Game game)
      : base(game)
    {
      // A filter that copies the current image to a low resolution render target.
      var copyFilterA = new CopyFilter(GraphicsService)
      {
        DefaultTargetFormat = new RenderTargetFormat(320, 240, false, SurfaceFormat.Color, DepthFormat.None),
      };
      GraphicsScreen.PostProcessors.Add(copyFilterA);

      // A filter that copies the result of the first filter to the back buffer.
      var copyFilterB = new CopyFilter(GraphicsService);
      GraphicsScreen.PostProcessors.Add(copyFilterB);
    }
  }
}
#endif